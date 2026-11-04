"""Fixed reader object-child observation; FND-COMBAT-077/149, FMT-ACTOR-001."""
import struct

from regenerate_checkpoints import checked_read
from request_provider import selected, frame


def brief(stop):
    return {"at":stop["cs_ip"],"emu_ns":stop["emu_ns"],
            "registers":{key:stop["registers"][key] for key in ("ss","sp","bp","ax")}}


def loader_frame(client, registers, delta):
    ss, sp = (int(registers[key],16) for key in ("ss","sp"))
    ret, caller, number, entry, slot, category, flags = struct.unpack("<7H",checked_read(client,ss,sp,14))
    loaded = int.from_bytes(checked_read(client,0x56EF+delta,0x10,2),"little")
    if (ret!=0xC3F or caller!=loaded or slot>=320 or flags!=0 or category&0xFF>7
            or int(registers["ds"],16)!=0x57E0+delta
            or checked_read(client,caller,0xC3A,5)!=struct.pack("<BHH",0x9A,0xEFF,0x31E0+delta)):
        raise ValueError("object loader lacks the declared flags-zero reader call")
    return {"ss":ss,"bp":sp-2,"number":number if number<0x8000 else number-0x10000,
            "entry_index":entry,"slot":slot,"category":category&0xFF,"flags":flags}


def ojff_frame(client, registers, delta, loader):
    ss, sp = (int(registers[key],16) for key in ("ss","sp"))
    ret, caller, tag, number, start, count, slot = struct.unpack("<HHIIIII",checked_read(client,ss,sp,24))
    outer_off, outer_seg = struct.unpack("<HH",checked_read(client,ss,sp+26,4))
    expected_slot = loader["ss"]<<16 | loader["bp"]-0xC
    destination = struct.unpack("<HH",checked_read(client,loader["ss"],loader["bp"]-0xC,4))
    if (ret!=0x4D0 or caller!=0x38FF+delta or tag!=int.from_bytes(b"OJFF","little")
            or number!=loader["number"]&0xFFFFFFFF or start or count!=0xFFFFFFFF or slot!=expected_slot
            or (outer_off,outer_seg)!=(0xF40,0x31E0+delta)
            or destination!=(loader["bp"]-0x24,loader["ss"])):
        raise ValueError("OJFF request differs from its declared loader local slot")
    return {"ss":ss,"bp":sp-2,"number":number}


class ObjectTrace:
    def __init__(self, client, delta):
        self.client, self.delta = client, delta
        self.active = self.core = None
        self.armed = set()
        self.report = {"objects":[],"child_counts":{},"outside_child_counts":{}}
        self.root, self.end, self.after_ojff = (self.address(0x31E0,off) for off in (0xEFF,0x1288,0xF40))
        self.core_entry, self.selection, self.core_end = (self.address(0x38FF,off) for off in (0x7B1,0x94A,0x9D4))
        self.children = {self.address(seg,off):(name,words) for seg,off,name,words in (
            (0x31E0,0xE1B,"initializer",5),(0x2D40,0x461,"allocator",1),
            (0x31E0,0xD09,"eviction",3),(0x2D40,0x3C1,"release",1),
            (0x2D40,0xB54,"script-chain",1),(0x31E0,0x2670,"image-selector",4),
            (0x31E0,0x271E,"animation",2))}
        for address in (self.root,self.end,self.after_ojff,*self.children):
            client.set_breakpoint(address)

    def address(self, segment, offset):
        return f"{segment+self.delta:04X}:{offset:04X}"

    def census(self):
        markers = [int.from_bytes(checked_read(self.client,0x57E0+self.delta,0x67BC+37*slot,2),"little")
                   for slot in range(320)]
        return {"active_slots":[slot for slot,value in enumerate(markers) if value!=0xFFFF],
                "slot_319_entry":markers[319]}

    def capture_before(self):
        if self.active is not None or self.report["objects"] or "before" in self.report:
            raise ValueError("object census is not before the reconstruction loop")
        self.report["before"] = self.census()

    def once(self, address):
        self.client.set_breakpoint(address,once=True)
        self.armed.add(address)

    def handle(self, stop):
        if stop["reason"]!="breakpoint":
            return False
        at, r, client = stop["cs_ip"],stop["registers"],self.client
        if at==self.root:
            if self.active is not None or "before" not in self.report or len(self.report["objects"])>=320:
                raise ValueError("nested, premature or excessive object-loader entry")
            original = loader_frame(client,r,self.delta)
            self.active = {"arguments":original,"entry":brief(stop),"children":[]}
            self.report["objects"].append(self.active)
            self.once(self.core_entry)
            return True
        if at in self.children:
            name, words = self.children[at]
            counts = self.report["child_counts" if self.active is not None else "outside_child_counts"]
            counts[name] = counts.get(name,0)+1
            if self.active is not None:
                arguments = list(struct.unpack(f"<{words}H",checked_read(client,int(r["ss"],16),int(r["sp"],16)+4,2*words)))
                self.active["children"].append({"name":name,"stop":brief(stop),"arguments":arguments})
            return True
        if self.active is None:
            return False
        if at in (self.selection,self.core_end) and self.core is None:
            raise ValueError("unmatched OJFF core stop")
        if at==self.core_entry:
            self.armed.discard(at)
            if self.core is not None:
                raise ValueError("nested OJFF core request")
            self.core = ojff_frame(client,r,self.delta,self.active["arguments"])
            self.active["request"] = self.core
            self.once(self.selection)
            self.once(self.core_end)
            return True
        if at==self.selection and self.core is not None:
            self.armed.discard(at)
            result = selected(client,r,self.core)
            self.active["selected"] = result
            original = self.active["arguments"]
            if result["count"]!=16 or result["destination"]!=f"{original['ss']:04X}:{original['bp']-0x24:04X}":
                raise ValueError("OJFF selection exceeds the admitted sixteen-byte local destination")
            return True
        if at==self.core_end and self.core is not None:
            self.armed.discard(at)
            frame(r,self.core)
            result = {"stop":brief(stop),"ax":int(r["ax"],16),
                      "count":int.from_bytes(checked_read(client,0x57E0+self.delta,0x9D91,4),"little")}
            self.active["core_return"] = result
            if result["ax"]==0 and ("selected" not in self.active or result["count"]!=16):
                raise ValueError("successful OJFF return lacks the admitted selection/count")
            self.core = None
            return True
        if at==self.after_ojff:
            frame(r,self.active["arguments"])
            self.active["ojff_result"] = int(r["ax"],16)
            return True
        if at==self.end:
            frame(r,self.active["arguments"])
            self.active["epilogue"] = brief(stop)
            self.active["result_slot"] = int(r["ax"],16)
            for address in sorted(self.armed):
                client.remove_breakpoints(address)
            self.armed.clear()
            self.active = self.core = None
            return True
        return False

    def finish(self):
        if self.active is not None or "before" not in self.report:
            raise ValueError("reader returned with incomplete object observation")
        self.report["observed_slots"] = [item["arguments"]["slot"] for item in self.report["objects"]]
        self.report["census_matches_calls"] = self.report["observed_slots"]==self.report["before"]["active_slots"]
        self.report["slot_319_entry_at_return"] = int.from_bytes(checked_read(self.client,0x57E0+self.delta,0x95D7,2),"little")
        return self.report
