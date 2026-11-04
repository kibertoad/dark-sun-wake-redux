"""Fixed native GPLI reverse observation; FND-COMBAT-151 and FND-PARTY-119."""
import hashlib
import struct

from regenerate_checkpoints import checked_read
from request_provider import frame, selected
from object_restore import brief


def address_range(address, size):
    if size not in (1,2,8,14,2600,7896):
        raise ValueError("undeclared conversion span")
    seg, off = (int(x,16) for x in address.split(":"))
    low = seg*16+off
    if not (seg or off) or not 0<=seg<=0xFFFF or not 0<=off<=0xFFFF or off+size>0x10000 or low+size>0x100000:
        raise ValueError("conversion span crosses declared bounds")
    return low,low+size


def blob(client, address, size):
    if size not in (2600,7896):
        raise ValueError("undeclared conversion block")
    address_range(address,size)
    seg, off = (int(x,16) for x in address.split(":"))
    return b"".join(checked_read(client,seg,off+at,min(261,size-at)) for at in range(0,size,261))


def snapshot(client, delta):
    ds = 0x57E0+delta
    head = int.from_bytes(checked_read(client,ds,0x5AF5,2),"little")
    off, seg = struct.unpack("<HH",checked_read(client,ds,0x40C0,4))
    trigger = f"{seg:04X}:{off:04X}"
    nodes, seen, index = [],set(),head
    while index!=0xFFFF:
        if not 0<=index<48 or index in seen:
            raise ValueError("conversion chain leaves the admitted pool or cycles")
        seen.add(index)
        pairs = list(struct.unpack("<4H",checked_read(client,0x4F49+delta,0x8AB+19*index,8)))
        link = int.from_bytes(checked_read(client,0x4F49+delta,0x8B5+19*index,1),"little",signed=True)
        nodes.append({"slot":index,"pairs":pairs,"next":link})
        index = link&0xFFFF
    return {"head":head,"trigger_pointer":trigger,"nodes":nodes},blob(client,trigger,2600)


def prediction(table, nodes, trigger):
    if len(table)!=7896 or len(trigger)!=2600:
        raise ValueError("conversion inputs differ from the admitted fixed sizes")
    lookup = {}
    for at in range(0,len(table),6):
        entry,offset,script = struct.unpack_from("<3H",table,at)
        lookup.setdefault(entry,(offset,script))
    expected_nodes, result = [],bytearray(trigger)
    stats = {"pairs_matched":0,"pairs_changed":0,"pairs_unmatched":0,
             "triggers_matched":0,"triggers_changed":0,"triggers_unmatched":0}
    for node in nodes:
        pairs = node["pairs"].copy()
        for j in range(2):
            if pairs[j] in lookup:
                values = lookup[pairs[j]]
                stats["pairs_matched"]+=1
                stats["pairs_changed"]+=int((pairs[j],pairs[j+2])!=values)
                pairs[j],pairs[j+2] = values
            else:
                stats["pairs_unmatched"]+=1
        expected_nodes.append({**node,"pairs":pairs})
    for i in range(200):
        entry,script = struct.unpack_from("<2H",trigger,13*i)
        if script:
            if entry in lookup:
                values = lookup[entry]
                struct.pack_into("<2H",result,13*i,*values)
                stats["triggers_matched"]+=1
                stats["triggers_changed"]+=int((entry,script)!=values)
            else:
                stats["triggers_unmatched"]+=1
    return expected_nodes,bytes(result),stats


def comparison(state, trigger, before, nodes, expected):
    if len(trigger)!=2600 or len(expected)!=2600:
        raise ValueError("conversion comparison has a short trigger block")
    return {"same_bindings":state["head"]==before["head"] and state["trigger_pointer"]==before["trigger_pointer"],
            "chain_equal":state["nodes"]==nodes,"trigger_equal":trigger==expected,
            "different_trigger_bytes":sum(a!=b for a,b in zip(trigger,expected)),
            "trigger_sha256":hashlib.sha256(trigger).hexdigest(),"expected_sha256":hashlib.sha256(expected).hexdigest()}


class ScriptTrace:
    def __init__(self, client, delta):
        self.client,self.delta = client,delta
        self.report = {}
        self.converter = self.query = self.core = None
        self.before = self.expected_nodes = self.expected_trigger = None
        self.size_entry,self.size_selection,self.size_end = (self.address(0x38FF,off) for off in (0x5B5,0x548,0x613))
        self.core_entry,self.selection,self.core_end = (self.address(0x38FF,off) for off in (0x7B1,0x94A,0x9D4))
        self.release = self.address(0x444C,0x92)
        client.set_breakpoint(self.size_entry)

    def address(self, seg, off):
        return f"{seg+self.delta:04X}:{off:04X}"

    def local(self, displacement, size):
        return checked_read(self.client,self.converter["ss"],self.converter["bp"]+displacement,size)

    def check_converter(self, registers):
        if int(registers["ss"],16)!=0x57E0+self.delta or registers["ds"]!=registers["ss"]:
            raise ValueError("conversion lacks the admitted DS/SS relationship")

    def handle(self, stop):
        if stop["reason"]!="breakpoint":
            return False
        at,r,client = stop["cs_ip"],stop["registers"],self.client
        if at==self.size_entry:
            ss,sp,bp = (int(r[key],16) for key in ("ss","sp","bp"))
            off,seg,tag,number,output = struct.unpack("<HHIII",checked_read(client,ss,sp,16))
            if tag!=int.from_bytes(b"GPLI","little") or number!=1:
                return True
            self.check_converter(r)
            loaded = int.from_bytes(checked_read(client,0x56EF+self.delta,0x10,2),"little")
            parent = struct.unpack("<HH",checked_read(client,ss,bp+2,4))
            flag = checked_read(client,ss,bp+6,1)[0]
            if (self.converter is not None or off!=0x184C or seg!=loaded or parent!=(0xCC0,seg) or flag!=1
                    or output!=(ss<<16|bp-8) or checked_read(client,seg,0x1847,5)!=struct.pack("<BHH",0x9A,0x5B5,0x38FF+self.delta)):
                raise ValueError("GPLI size request differs from the declared reverse caller")
            self.converter = {"ss":ss,"bp":bp,"segment":seg}
            if self.local(-4,4)!=bytes(4):
                raise ValueError("reverse table slot is not initially null")
            self.query = {"ss":ss,"bp":sp-2}
            self.report["size_entry"] = brief(stop)
            client.remove_breakpoints(self.size_entry)
            client.set_breakpoint(self.size_selection,once=True)
            client.set_breakpoint(self.size_end,once=True)
            return True
        if self.converter is None:
            return False
        if at==self.size_selection:
            expected = {"ss":self.query["ss"],"bp":self.query["bp"]-0x16}
            frame(r,expected)
            bp,ss = expected["bp"],expected["ss"]
            off,seg = struct.unpack("<HH",checked_read(client,ss,bp-6,4))
            type_off,type_seg = struct.unpack("<HH",checked_read(client,ss,bp-0xA,4))
            tag,flags = struct.unpack("<4sI",checked_read(client,type_seg,type_off,8))
            if tag!=b"GPLI" or int(r["eax"],16)!=7896 or int(r["es"],16)!=self.converter["ss"] or int(r["bx"],16)!=self.converter["bp"]-8:
                raise ValueError("GPLI size selection differs from the admitted output/length")
            self.report["size_selected"] = {"provider":f"{seg:04X}:{off:04X}","length":7896,"indexed":bool(flags&0x80000000),
                "provider_handle":int.from_bytes(checked_read(client,seg,off+0x28,4),"little"),
                "io_handle":int.from_bytes(checked_read(client,seg,off+4,2),"little")}
            return True
        if at==self.size_end:
            frame(r,self.query)
            length = int.from_bytes(self.local(-8,4),"little")
            self.report["size_result"] = {"stop":brief(stop),"length":length}
            if int(r["ax"],16) or length!=7896 or "size_selected" not in self.report:
                raise ValueError("GPLI size query did not complete the admitted length")
            client.set_breakpoint(self.core_entry,once=True)
            return True
        if at==self.core_entry:
            ss,sp = (int(r[key],16) for key in ("ss","sp"))
            ret,seg,tag,num,start,count,slot = struct.unpack("<HHIIIII",checked_read(client,ss,sp,24))
            outer = struct.unpack("<HH",checked_read(client,ss,sp+26,4))
            if (ret!=0x4D0 or seg!=0x38FF+self.delta or tag!=int.from_bytes(b"GPLI","little") or num!=1 or start or count!=0xFFFFFFFF
                    or slot!=(self.converter["ss"]<<16|self.converter["bp"]-4) or outer!=(0x1879,self.converter["segment"]) or self.local(-4,4)!=bytes(4)):
                raise ValueError("GPLI data request differs from the admitted allocation slot")
            self.core = {"ss":ss,"bp":sp-2}
            self.report["data_entry"] = brief(stop)
            client.set_breakpoint(self.selection,once=True)
            client.set_breakpoint(self.core_end,once=True)
            return True
        if at==self.selection:
            result = selected(client,r,self.core)
            self.report["selected"] = result
            if result["count"]!=7896:
                raise ValueError("GPLI data selection differs from the admitted table span")
            self.report["same_provider"] = all(result[key]==self.report["size_selected"][key] for key in ("provider","provider_handle","io_handle"))
            return True
        if at==self.core_end:
            frame(r,self.core)
            self.check_converter(r)
            count = int.from_bytes(checked_read(client,0x57E0+self.delta,0x9D91,4),"little")
            self.report["data_result"] = {"stop":brief(stop),"count":count}
            pointer = struct.unpack("<HH",self.local(-4,4))
            destination = f"{pointer[1]:04X}:{pointer[0]:04X}"
            if int(r["ax"],16) or count!=7896 or destination!=self.report["selected"]["destination"]:
                raise ValueError("GPLI data result lacks the admitted successful span")
            table = blob(client,destination,7896)
            self.before,trigger = snapshot(client,self.delta)
            self.expected_nodes,self.expected_trigger,stats = prediction(table,self.before["nodes"],trigger)
            spans = {"table":(destination,7896),"triggers":(self.before["trigger_pointer"],2600),
                     "locals":(f"{self.converter['ss']:04X}:{self.converter['bp']-14:04X}",14)}
            for node in self.before["nodes"]:
                spans[f"pairs-{node['slot']}"] = (self.address(0x4F49,0x8AB+19*node["slot"]),8)
                spans[f"link-{node['slot']}"] = (self.address(0x4F49,0x8B5+19*node["slot"]),1)
            ranges = {key:address_range(*value) for key,value in spans.items()}
            names = sorted(ranges)
            overlap = [[a,b] for i,a in enumerate(names) for b in names[i+1:] if max(ranges[a][0],ranges[b][0])<min(ranges[a][1],ranges[b][1])]
            self.report.update({"before":self.before,"prediction":stats,"overlaps":overlap,"table_sha256":hashlib.sha256(table).hexdigest()})
            if overlap:
                raise ValueError("conversion prediction inputs overlap")
            client.set_breakpoint(self.release,once=True)
            return True
        if at==self.release:
            frame(r,self.converter)
            self.check_converter(r)
            off,seg,pointer = struct.unpack("<HHI",checked_read(client,int(r["ss"],16),int(r["sp"],16),8))
            if (off!=0x1A27 or seg!=self.converter["segment"] or pointer!=int.from_bytes(self.local(-4,4),"little")
                    or f"{pointer>>16:04X}:{pointer&0xFFFF:04X}"!=self.report["selected"]["destination"]):
                raise ValueError("GPLI release request differs from the matched converter")
            current,trigger = snapshot(client,self.delta)
            self.report["before_release"] = comparison(current,trigger,self.before,self.expected_nodes,self.expected_trigger)
            self.report["release_entry"] = brief(stop)
            return True
        return False

    def finish(self):
        if "before_release" not in self.report:
            raise ValueError("reader returned without the admitted conversion boundary")
        current,trigger = snapshot(self.client,self.delta)
        self.report["reader_return"] = comparison(current,trigger,self.before,self.expected_nodes,self.expected_trigger)
        return self.report
