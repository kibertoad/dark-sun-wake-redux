"""Synthetic fixed-field and refusal controls, no original or emulator."""
import copy
from pathlib import Path
import struct
import sys
from types import SimpleNamespace
import unittest
sys.path.insert(0, str(Path(__file__).resolve().parent))
from overlay_backing import capture


class Memory:
    def __init__(self, delta=0):
        self.delta, self.calls, self.short = delta, [], False
    def read_memory(self, address, size):
        seg, off = (int(x, 16) for x in address.split(":"))
        self.calls.append((seg, off, size))
        values = {0x10: b"\x03", 0x124: struct.pack("<H", 0x7000),
                  0x126: struct.pack("<H", 0x7800), 0x30: struct.pack("<H", 0xE000)}
        body = values.get(off, bytes(size)) if seg == 0x55CE + self.delta else bytes(size)
        if seg == 0x57E0 + self.delta and off == 0x915:
            body = b"\xff"
        return SimpleNamespace(data=body[:-1] if self.short else body)


class Controls(unittest.TestCase):
    def stop(self, delta=0):
        return {"reason":"load", "registers":{"system":{"mode":"real"},
                "ds":f"{0x57E0 + delta:04X}", "ss":f"{0x57E0 + delta:04X}",
                "bp":"FF00", "sp":"FEF0"}}
    def test_named_and_roundtrip(self):
        m=Memory(); r=capture(m,self.stop(),0)
        self.assertEqual(r["selected_cache"],"extended")
        self.assertEqual(r["ems_window_segment"],0xE000)
        self.assertEqual(r["queue_index"],-1)
        self.assertEqual(r["queue_kinds"],[0]*6)
        self.assertTrue(r["buffer_disjoint"] and r["ss_window_disjoint"])
        self.assertEqual(r,capture(m,self.stop(),0))
        self.assertTrue(all(0<n<=12 and off+n<=0x10000 for _,off,n in m.calls))
    def test_relocation(self):
        r=capture(Memory(-0xA46),self.stop(-0xA46),-0xA46)
        self.assertEqual(r["queue_linear"],[0x435B0,0x43610])
    def test_refuses_before_reads(self):
        for field,value in [("ds","0000"),("sp","10000")]:
            s=self.stop();s["registers"][field]=value;m=Memory()
            with self.assertRaises(ValueError):capture(m,s,0)
            self.assertEqual(m.calls,[])
        for change in ["mode","reason"]:
            s=self.stop();m=Memory()
            if change=="mode":s["registers"]["system"]["mode"]="protected"
            else:s["reason"]="breakpoint"
            with self.assertRaises(ValueError):capture(m,s,0)
            self.assertEqual(m.calls,[])
    def test_short_read(self):
        m=Memory();m.short=True
        with self.assertRaisesRegex(ValueError,"short"):capture(m,self.stop(),0)
    def test_invalid_mapping(self):
        for delta in [False,-0x57E1,0xB260]:
            m=Memory()
            with self.assertRaises(ValueError):capture(m,self.stop(),delta)
            self.assertEqual(m.calls,[])


if __name__=="__main__":unittest.main()
