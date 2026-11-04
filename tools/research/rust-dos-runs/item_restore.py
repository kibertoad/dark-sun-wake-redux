"""Fixed numbered-restore item boundaries; FND-PARTY-236, FND-CONFIG-143/145."""
import hashlib
import struct

from regenerate_checkpoints import checked_read
from request_provider import frame
from object_restore import brief


def bounds(address, size):
    if size not in (2,296,792,1920,9200):
        raise ValueError("undeclared item observation span")
    seg, off = (int(x,16) for x in address.split(":"))
    low = seg*16+off
    if not (seg or off) or not 0<=seg<=0xFFFF or not 0<=off<=0xFFFF or off+size>0x10000 or low+size>0x100000:
        raise ValueError("item observation span crosses bounds")
    return low,low+size


def read(client, address, size):
    bounds(address,size)
    seg, off = (int(x,16) for x in address.split(":"))
    return b"".join(checked_read(client,seg,off+at,min(261,size-at)) for at in range(0,size,261))


def prediction(table):
    if len(table)!=9200:
        raise ValueError("item table differs from 400 fixed records")
    result = bytearray(table)
    for at in range(12,9200,23):
        result[at:at+2] = bytes(2)
    return bytes(result)


def comparison(actual, expected):
    if len(actual)!=len(expected) or len(actual) not in (792,9200):
        raise ValueError("item comparison span differs")
    return {"equal":actual==expected,"different_bytes":sum(a!=b for a,b in zip(actual,expected)),
            "actual_sha256":hashlib.sha256(actual).hexdigest(),"expected_sha256":hashlib.sha256(expected).hexdigest()}


class ItemTrace:
    def __init__(self,client,delta,r):
        self.client,self.delta = client,delta
        self.reader = {"ss":int(r["ss"],16),"bp":int(r["sp"],16)-2}
        self.segment = int(r["cs"],16)
        self.clear = f"{self.segment:04X}:0CC1"
        self.entry,self.end = (self.address(0x2D40,off) for off in (0x2196,0x240D))
        self.helpers = {self.address(0x1AA0,off):f"{off:04X}" for off in (0x566,0x51C)}
        self.report = {"helpers":{"0566":0,"051C":0}}
        self.active = False
        self.enum = self.table = self.expected = self.output = None
        client.set_breakpoint(self.clear,once=True)

    def address(self,seg,off):
        return f"{seg+self.delta:04X}:{off:04X}"

    def pointer(self,off):
        offset,seg = struct.unpack("<HH",checked_read(self.client,0x57E0+self.delta,off,4))
        return f"{seg:04X}:{offset:04X}"

    def handle(self,stop):
        if stop["reason"]!="breakpoint":
            return False
        at,r = stop["cs_ip"],stop["registers"]
        if at==self.clear:
            frame(r,self.reader)
            if int(r["ds"],16)!=0x57E0+self.delta:
                raise ValueError("item clear lacks resident DS")
            self.table = self.pointer(0x19C1)
            before = read(self.client,self.table,9200)
            self.expected = prediction(before)
            self.report["clear_entry"] = {"stop":brief(stop),"table":self.table,
                "nonzero_words":sum(before[at:at+2]!=bytes(2) for at in range(12,9200,23)),
                "changed_bytes":sum(a!=b for a,b in zip(before,self.expected))}
            self.client.set_breakpoint(self.entry,once=True)
            return True
        if at==self.entry:
            ss,sp = (int(r[key],16) for key in ("ss","sp"))
            ret,seg,index,off,outseg = struct.unpack("<5H",checked_read(self.client,ss,sp,10))
            selected = int.from_bytes(checked_read(self.client,0x57E0+self.delta,0x1A32,2),"little")
            if (ret!=0xCEA or seg!=self.segment or index!=selected or off!=0x40 or outseg!=0x5072+self.delta
                    or int(r["ds"],16)!=0x57E0+self.delta or self.expected is None):
                raise ValueError("item enumeration arguments/caller differ")
            self.enum = {"ss":ss,"bp":sp-2}
            self.output = f"{outseg:04X}:{off:04X}"
            spans = {"items":(self.table,9200),"output":(self.output,792),
                     "locals":(f"{ss:04X}:{sp-2-296:04X}",296),
                     "effects":(self.pointer(0x157E+4*23),1920),"count":(self.pointer(0x157E+4*25),2)}
            ranges = {key:bounds(*value) for key,value in spans.items()}
            names = sorted(ranges)
            overlaps = [[a,b] for i,a in enumerate(names) for b in names[i+1:]
                        if max(ranges[a][0],ranges[b][0])<min(ranges[a][1],ranges[b][1])]
            self.report["enumeration_entry"] = {"stop":brief(stop),"index":index,"output":self.output,
                "same_table_binding":self.pointer(0x19C1)==self.table,"ds_equals_ss":r["ds"]==r["ss"],
                "clear_comparison":comparison(read(self.client,self.table,9200),self.expected),
                "spans":spans,"overlaps":overlaps}
            self.client.set_breakpoint(self.end,once=True)
            for address in self.helpers:
                self.client.set_breakpoint(address)
            self.active = True
            return True
        if self.active and at in self.helpers:
            self.report["helpers"][self.helpers[at]] += 1
            return True
        if at==self.end:
            frame(r,self.enum)
            if not self.active:
                raise ValueError("item enumeration epilogue lacks active frame")
            count = int(r["ax"],16)
            if not 0<=count<=79:
                raise ValueError("enumeration count leaves the bounded output")
            self.cached_output = read(self.client,self.output,792)
            self.report["enumeration_epilogue"] = {"stop":brief(stop),"count":count,
                "sentinel":self.cached_output[10*count],"count_byte":self.cached_output[1],
                "table_comparison":comparison(read(self.client,self.table,9200),self.expected)}
            for address in self.helpers:
                self.client.remove_breakpoints(address)
            self.active = False
            return True
        return False

    def finish(self, boundary="reader_return"):
        if boundary not in ("reader_return","caller_epilogue"):
            raise ValueError("undeclared item comparison boundary")
        if self.active or "enumeration_epilogue" not in self.report:
            raise ValueError("reader returned without admitted item enumeration")
        self.report[boundary] = {"same_table_binding":self.pointer(0x19C1)==self.table,
            "table_comparison":comparison(read(self.client,self.table,9200),self.expected),
            "output_comparison":comparison(read(self.client,self.output,792),self.cached_output)}
        return self.report
