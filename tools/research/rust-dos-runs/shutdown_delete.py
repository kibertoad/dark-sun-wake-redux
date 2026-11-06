"""Fixed registered callback -> AH41 -> normalized return; no guest writes."""
import struct

from regenerate_checkpoints import checked_read
from load_entry import terminated_path


class RegisteredDelete:
    def __init__(self,delta,records):
        self.delta,self.records=delta,records
        self.current=None

    def address(self,segment,offset):
        return f"{segment+self.delta:04X}:{offset:04X}"

    @property
    def stops(self):
        return tuple(self.address(s,o) for s,o in
            ((0x167B,0x29),(0x44DE,0x2A9),(0x44DE,0x2B3),(0x44DE,0x2B5),(0x167B,0x3A)))

    def observe(self,client,stop,callbacks):
        address,r=stop["cs_ip"],stop["registers"]
        data=0x57E0+self.delta
        if (stop.get("reason")!="breakpoint" or r.get("system",{}).get("mode")!="real"
                or int(r["ss"],16)!=data):
            raise ValueError("delete observer requires its fixed real-mode frame")
        ss,sp,bp=(int(r[k],16) for k in ("ss","sp","bp"))
        if address==self.stops[0]:
            ret,caller=struct.unpack("<HH",checked_read(client,ss,sp,4))
            if (self.current is not None or self.records or address not in callbacks
                    or caller!=0x1000+self.delta or not 0x388<=ret<0x3CC):
                raise ValueError("delete callback lacks its registered ordinary-exit frame")
            self.current={"callback":stop,"callback_sp":sp,"phase":"wrapper"}
            self.records.append(self.current)
            return
        item=self.current
        if item is None:
            raise ValueError("delete stop has no admitted callback")
        if address==self.stops[1]:
            ret,caller,off,seg=struct.unpack("<4H",checked_read(client,ss,sp,8))
            if (item["phase"]!="wrapper" or (ret,caller)!=(0x3A,0x167B+self.delta)
                    or (seg,off)!=(data,0x4022) or int(r["ds"],16)!=data
                    or sp!=item["callback_sp"]-10 or bp!=item["callback_sp"]-2):
                raise ValueError("delete wrapper differs from its fixed callback argument/frame")
            item.update(wrapper=stop,wrapper_sp=sp,wrapper_bp=bp,
                saved_dx=r["dx"],path=terminated_path(client,seg,off,82),
                error_before=int.from_bytes(checked_read(client,data,0x33BE,2),"little"),phase="native")
        elif address==self.stops[2]:
            if (item["phase"]!="native" or int(r["ax"],16)>>8!=0x41
                    or (int(r["ds"],16),int(r["dx"],16))!=(data,0x4022)
                    or bp!=item["wrapper_sp"]-2 or sp!=item["wrapper_sp"]-6
                    or terminated_path(client,data,0x4022,82)!=item["path"]):
                raise ValueError("delete native selector/path/frame differs")
            item.update(request=stop,phase="result")
        elif address==self.stops[3]:
            if item["phase"]!="result" or bp!=item["wrapper_sp"]-2 or sp!=item["wrapper_sp"]-6:
                raise ValueError("delete native result lacks its request/frame")
            item.update(native={"ax":r["ax"],"carry":bool(int(r["flags"],16)&1)},phase="return")
        elif address==self.stops[4]:
            if (item["phase"]!="return" or sp!=item["wrapper_sp"]+4 or bp!=item["wrapper_bp"]
                    or int(r["ds"],16)!=data or r["dx"]!=item["saved_dx"]):
                raise ValueError("delete wrapper return frame/registers differ")
            error=int.from_bytes(checked_read(client,data,0x33BE,2),"little")
            failed=item["native"]["carry"]
            if (int(r["ax"],16)!=(0xFFFF if failed else 0) or int(r["flags"],16)&1
                    or error!=(int(item["native"]["ax"],16) if failed else item["error_before"])):
                raise ValueError("delete wrapper result/error mapping differs")
            item.update(result={"ax":r["ax"],"carry":False,"error_word":error},returned=stop,phase="complete")
            self.current=None
        else:
            raise ValueError("unadmitted delete observation point")
