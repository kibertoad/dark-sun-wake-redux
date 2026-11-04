"""Fixed normal restore caller epilogue; FND-COMBAT-043."""
import struct

from regenerate_checkpoints import checked_read
from request_provider import frame
from object_restore import brief


class CallerTrace:
    def __init__(self,client,delta,r):
        self.client,self.delta = client,delta
        self.parent = {"ss":int(r["ss"],16),"bp":int(r["bp"],16)}
        self.segment = int(r["cs"],16)
        self.post,self.end = (f"{self.segment:04X}:{off:04X}" for off in (0xFA1,0xFB5))
        self.close_entry,self.close_end = (f"{0x38FF+delta:04X}:{off:04X}" for off in (0x2B5,0x427))
        self.report = {"reader_al":int(r["ax"],16)&255}
        self.complete = False
        self.close = None
        for address in (self.post,self.end):
            client.set_breakpoint(address,once=True)

    def state(self):
        ds = 0x57E0+self.delta
        return {"reuse":checked_read(self.client,ds,0x167A,1)[0],
                "working_handle":int.from_bytes(checked_read(self.client,ds,0x144E,4),"little"),
                "saved_open":checked_read(self.client,self.parent["ss"],self.parent["bp"]-1,1)[0],
                "saved_result":checked_read(self.client,self.parent["ss"],self.parent["bp"]-2,1)[0]}

    def close_frame(self,r):
        ss,sp = (int(r[key],16) for key in ("ss","sp"))
        ret,seg,handle = struct.unpack("<HHI",checked_read(self.client,ss,sp,8))
        helper = int.from_bytes(checked_read(self.client,ss,sp+8,2),"little")
        expected = self.state()
        if (ret!=0x1C3A or seg!=self.segment or handle!=expected["working_handle"] or handle==0xFFFFFFFF
                or helper!=self.parent["bp"] or int(r["ds"],16)!=0x57E0+self.delta):
            raise ValueError("cleanup close differs from the declared parent/handle")
        return {"ss":ss,"bp":sp-2},handle

    def handle(self,stop):
        if stop["reason"]!="breakpoint":
            return False
        at,r = stop["cs_ip"],stop["registers"]
        if at==self.post:
            frame(r,self.parent)
            self.report["post_setup"] = {"stop":brief(stop),"state":self.state()}
            self.client.set_breakpoint(self.close_entry,once=True)
            return True
        if at==self.close_entry:
            if "post_setup" not in self.report:
                raise ValueError("cleanup close preceded post-setup boundary")
            self.close,handle = self.close_frame(r)
            self.report["close_entry"] = {"stop":brief(stop),"handle":handle,"state":self.state()}
            self.client.set_breakpoint(self.close_end,once=True)
            return True
        if at==self.close_end:
            frame(r,self.close)
            self.report["close_epilogue"] = {"stop":brief(stop),"ax":int(r["ax"],16),"state":self.state()}
            return True
        if at==self.end:
            frame(r,self.parent)
            if "post_setup" not in self.report or self.close is not None and "close_epilogue" not in self.report:
                raise ValueError("parent epilogue lacks matched preparation/close")
            if self.close is None:
                self.client.remove_breakpoints(self.close_entry)
            self.report["epilogue"] = {"stop":brief(stop),"al":int(r["ax"],16)&255,"state":self.state()}
            self.complete = True
            return True
        return False
