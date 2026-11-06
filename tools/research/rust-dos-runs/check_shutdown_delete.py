"""Synthetic fixed registered-delete request/result and frame controls."""
import copy
import struct
import unittest

from check_failed_save import Memory
from shutdown_delete import RegisteredDelete


class DeleteControls(unittest.TestCase):
    def setUp(self):
        self.client=Memory()
        self.client.put(0x57E0,0xFFB0,struct.pack("<HH",0x3A5,0x1000))
        self.client.put(0x57E0,0xFFA6,struct.pack("<4H",0x3A,0x167B,0x4022,0x57E0))
        self.client.put(0x57E0,0x4022,b"C:\\SYNTH.TMP\0"+b"\0"*70)
        self.client.put(0x57E0,0x33BE,struct.pack("<H",7))
        self.calls=[]
        self.observer=RegisteredDelete(0,self.calls)
        self.callbacks=["167B:0029"]

    def stop(self,index,**changes):
        sp=(0xFFB0,0xFFA6,0xFFA0,0xFFA0,0xFFAA)[index]
        bp=(0xFFC0,0xFFAE,0xFFA4,0xFFA4,0xFFAE)[index]
        r={"ss":"57E0","sp":f"{sp:04X}","bp":f"{bp:04X}","ds":"57E0",
            "dx":"4022" if index in (2,3) else "1234","ax":"4100","flags":"0202","system":{"mode":"real"}}
        r.update(changes)
        return {"reason":"breakpoint","cs_ip":self.observer.stops[index],"registers":r}

    def observe(self,index,**changes):
        self.observer.observe(self.client,self.stop(index,**changes),self.callbacks)

    def prefix(self):
        for i in (0,1,2): self.observe(i)

    def test_native_success_and_normalized_zero(self):
        self.prefix(); self.observe(3,ax="4100")
        self.observe(4,ax="0000")
        self.assertIsNone(self.observer.current)
        self.assertEqual(self.calls[0]["result"],{"ax":"0000","carry":False,"error_word":7})

    def test_native_error_becomes_ffff_with_carry_cleared(self):
        self.prefix(); self.observe(3,ax="0002",flags="0203")
        self.client.put(0x57E0,0x33BE,struct.pack("<H",2))
        self.observe(4,ax="FFFF")
        self.assertEqual(self.calls[0]["native"],{"ax":"0002","carry":True})
        self.assertFalse(self.calls[0]["result"]["carry"])

    def test_unregistered_callback_wrong_argument_and_frame_rejected(self):
        with self.assertRaises(ValueError):
            self.observer.observe(self.client,self.stop(0),[])
        self.observe(0)
        with self.assertRaises(ValueError): self.observe(1,sp="FFA8")
        self.client.put(0x57E0,0xFFAA,struct.pack("<H",0x4024))
        with self.assertRaises(ValueError): self.observe(1)

    def test_native_selector_path_and_unpaired_result_rejected(self):
        with self.assertRaises(ValueError): self.observe(3)
        self.observe(0); self.observe(1)
        with self.assertRaises(ValueError): self.observe(2,ax="4000")
        with self.assertRaises(ValueError): self.observe(2,dx="4023")
        self.client.put(0x57E0,0x4022,b"D")
        with self.assertRaises(ValueError): self.observe(2)

    def test_wrong_normalization_or_error_store_rejected(self):
        self.prefix(); self.observe(3,ax="0002",flags="0203")
        with self.assertRaises(ValueError): self.observe(4,ax="0000")
        with self.assertRaises(ValueError): self.observe(4,ax="FFFF")
        self.client.put(0x57E0,0x33BE,struct.pack("<H",2))
        with self.assertRaises(ValueError): self.observe(4,ax="FFFF",flags="0203")

    def test_registered_then_exact_game_cleanup_caller_and_no_third(self):
        self.prefix(); self.observe(3); self.observe(4,ax="0000")
        self.client.put(0x57E0,0xFFA6,struct.pack("<HH",0x7B4,0x5F31))
        self.client.put(0x57E0,0xFF9C,struct.pack("<4H",0x3A,0x167B,0x4022,0x57E0))
        self.client.put(0x5F31,0x7AF,struct.pack("<BHH",0x9A,0x29,0x167B))
        for i in range(5):
            stop=self.stop(i)
            for key in ("sp","bp"):
                stop["registers"][key]=f'{int(stop["registers"][key],16)-10:04X}'
            if i==4: stop["registers"]["ax"]="0000"
            self.observer.observe(self.client,stop,self.callbacks)
        self.assertEqual([x["callback_source"] for x in self.calls],["registered","game-cleanup"])
        with self.assertRaises(ValueError): self.observe(0)


if __name__=="__main__":
    unittest.main()
