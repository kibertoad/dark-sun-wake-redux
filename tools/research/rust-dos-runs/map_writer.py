"""Watch only FND-COMBAT-157's named cell after the admitted reader return."""
import struct

from regenerate_checkpoints import checked_read
from map_restore import bounds
from object_restore import brief


class MapWriter:
    def __init__(self,client,delta,address):
        self.client,self.delta,self.address = client,delta,address
        bounds(address,12544)
        seg,off = (int(x,16) for x in address.split(":"))
        self.watch = f"{seg:04X}:{off+4715:04X}"
        self.report = {"cell":{"x":107,"y":36,"offset":4715},"map":address,"changes":[]}
        if self.binding()!=address or self.value()!=0:
            raise ValueError("named map writer requires the admitted zero cell/binding")
        client.set_watchpoint(self.watch,1)

    def binding(self):
        off,seg = struct.unpack("<HH",checked_read(self.client,0x57E0+self.delta,0x538,4))
        return f"{seg:04X}:{off:04X}"

    def value(self):
        seg,off = (int(x,16) for x in self.watch.split(":"))
        return checked_read(self.client,seg,off,1)[0]

    def handle(self,stop):
        if stop["reason"]!="watchpoint":
            return False
        r=stop["registers"]
        if self.binding()!=self.address:
            raise ValueError("map binding changed before named-cell attribution")
        ss,bp = (int(r[key],16) for key in ("ss","bp"))
        ret,seg,x,y = struct.unpack("<4H",checked_read(self.client,ss,bp+2,8))
        value=self.value()
        target=f"{int(r['es'],16):04X}:{int(r['bx'],16):04X}"
        self.report["changes"].append({"stop":brief(stop),"value":value,"target":target,
            "x":x,"y":y,"caller":f"{seg:04X}:{ret:04X}"})
        if (stop["cs_ip"]!=f"{0x25AF+self.delta:04X}:05F9" or int(r["ds"],16)!=0x57E0+self.delta
                or (int(r["dx"],16),int(r["cx"],16))!=(107,36) or (x,y)!=(107,36)
                or target!=self.watch or value!=0x60):
            raise ValueError("named cell changed outside the admitted occupancy store")
        return True

    def finish(self):
        if self.binding()!=self.address or not self.report["changes"]:
            raise ValueError("caller epilogue lacks admitted named-cell writer")
        self.report["final_value"] = self.value()
        self.client.remove_watchpoints(self.watch)
        return self.report
