"""Synthetic payload, private lock and failed-checkpoint controls; no game."""
import copy
import os
from pathlib import Path
import struct
import sys
import tempfile
import unittest

sys.path.insert(0,str(Path(__file__).resolve().parent))
from check_shutdown_snapshot import Memory
from private_payload_lock import PrivatePayloadLock, LIMIT
from save_failure_fields import payload, pending_watch, pending_change
from shutdown_run import controlled_stage, failed_source
from failed_save import arm_save_return


class FailureControls(unittest.TestCase):
    def setUp(self):
        self.memory=Memory()
        m=self.memory
        m.put(0x57E0,0xFE00,struct.pack("<3H",0xFF00,0x4E8,0x37FC))
        m.put(0x57E0,0xFF00,struct.pack("<3H",0xFF80,0x9D8,0x7000))
        m.put(0x57E0,0xFF06,struct.pack("<4I",0x72000100,14,int.from_bytes(b"SAVE","little"),1))
        m.put(0x57E0,0xFEEC,struct.pack("<I",512))
        m.put(0x7000,0x9D3,struct.pack("<BHH",0x9A,0xE8,0x37FC))
        m.put(0x7100,0x104,struct.pack("<HH",7,2))
        m.put(0x7100,0x128,struct.pack("<I",12))
        self.published={"provider":"7100:0100","provider_handle":12,"io_handle":7}
        self.archive={"active":{"pointer":"7100:0100"},"working_handle":12}
        self.stop={"reason":"breakpoint","cs_ip":"44DE:0285","registers":{
            "ss":"57E0","bp":"FE00","ax":"4000","bx":"0007","cx":"000E",
            "ds":"7200","dx":"0100","system":{"mode":"real"}}}

    def capture(self):
        return payload(self.memory,self.stop,0,self.published,self.archive)

    def test_named_payload_parent_and_span(self):
        r=self.capture()
        self.assertEqual((r["position"],r["length"],r["number"]),(512,14,1))
        self.assertEqual((r["resource_bp"],r["numbered_bp"]),(0xFF00,0xFF80))
        self.assertEqual(r["record"]["io_handle"],7)

    def test_initial_return_is_armed_before_relocation(self):
        class Client(Memory):
            def __init__(self):
                super().__init__()
                self.breakpoints={}
                self.watches={}
            def set_breakpoint(self,address,once=False): self.breakpoints[address]=once
            def set_watchpoint(self,address,length): self.watches[address]=length
        client=Client()
        client.put(0x57E0,0xFF90,struct.pack("<3H",0xBF2,0x6506,1))
        destination,watch=arm_save_return(client,{"ss":"57E0","sp":"FF90"})
        self.assertEqual(destination,"6506:0BF2")
        self.assertEqual(client.breakpoints,{destination:True})
        self.assertEqual(client.watches,{watch:2})

    def test_pending_watch_rejects_return_segment_and_wrong_initial_value(self):
        class Client(Memory):
            def set_watchpoint(self,address,length): self.watch=(address,length)
        client=Client()
        client.put(0x7100,0x12C,b"\x01")
        physical=pending_watch(client,"7100:0100",1)
        self.assertEqual(client.watch,("7100:012C",1))
        stop={"reason":"watchpoint","cs_ip":"37FC:09BC","seq":10,"emu_ns":200,
              "watch":{"addr":f"{physical:05X}","len":1,"old":"01","new":"00"}}
        self.assertEqual(pending_change(stop,physical)["new"],0)
        bad=copy.deepcopy(stop);bad["watch"]["len"]=2
        with self.assertRaises(ValueError): pending_change(bad,physical)
        bad=copy.deepcopy(stop);bad["watch"]["addr"]="50000"
        with self.assertRaises(ValueError): pending_change(bad,physical)
        with self.assertRaises(ValueError): pending_watch(client,"7100:0100",0)

    def test_header_write_other_record_and_wrapping_span_rejected(self):
        for case in ("header","readonly","through","span","number","handle","active"):
            self.setUp()
            if case=="header": self.memory.put(0x57E0,0xFE02,struct.pack("<H",0x955))
            elif case=="readonly": self.memory.put(0x7100,0x106,b"\x00\x00")
            elif case=="through": self.memory.put(0x7100,0x124,struct.pack("<I",8))
            elif case=="span": self.memory.put(0x57E0,0xFEEC,struct.pack("<I",LIMIT-1))
            elif case=="number": self.memory.put(0x57E0,0xFF12,struct.pack("<I",2))
            elif case=="handle": self.stop["registers"]["bx"]="0008"
            else: self.archive["active"]["pointer"]="7200:0100"
            with self.assertRaises(ValueError,msg=case): self.capture()

    @unittest.skipUnless(os.name=="nt","Windows mandatory byte-range lock")
    def test_real_lock_denies_write_and_releases_on_exception(self):
        with tempfile.TemporaryDirectory() as directory:
            run=Path(directory)/"rust-dos-save-failure"/"synthetic"
            (run/"drive-c").mkdir(parents=True)
            target=run/"drive-c/DARKRUN.GFF"
            target.write_bytes(b"synthetic")
            lock=PrivatePayloadLock(run)
            with self.assertRaisesRegex(RuntimeError,"synthetic exit"):
                with lock:
                    with target.open("r+b",buffering=0) as other:
                        with self.assertRaises(OSError): other.write(b"x")
                    raise RuntimeError("synthetic exit")
            self.assertFalse(lock.locked)
            self.assertEqual(target.read_bytes(),b"synthetic")
            with target.open("r+b",buffering=0) as other:
                self.assertEqual(other.write(b"x"),1)

    @unittest.skipUnless(os.name=="nt","Windows lock path guard")
    def test_missing_wrong_root_and_nonfile_fail_before_lock(self):
        with tempfile.TemporaryDirectory() as directory:
            run=Path(directory)/"wrong-root"/"synthetic"
            with self.assertRaisesRegex(ValueError,"root"): PrivatePayloadLock(run).acquire()
            run=Path(directory)/"rust-dos-save-failure"/"synthetic"
            (run/"drive-c").mkdir(parents=True)
            with self.assertRaises(ValueError): PrivatePayloadLock(run).acquire()
            (run/"drive-c/DARKRUN.GFF").mkdir()
            with self.assertRaises(ValueError): PrivatePayloadLock(run).acquire()

    def test_failed_source_requires_real_result_and_same_pending_record(self):
        record={"pointer":"7100:0100","io_handle":7,"options":2,"flags":0,"handle":12,"dirty":1}
        meta={"passed":True,"writes":[],"fault":{"record":copy.deepcopy(record),"lock":{
            "file":"drive-c/DARKRUN.GFF","start":0,"length":LIMIT,"acquired":True,"released":True},
            "write_result":{"ax":"0005","carry":True}},"resource_result":{"ax":"FFFF","record":record},
            "numbered_result":{"success_byte":0,"record":record},"post_failed_map":{"record":record},
            "files_after":{"SAVE02.SAV":{"exists":False}}}
        self.assertEqual(failed_source(meta),record)
        self.assertFalse(controlled_stage(["source","run","failed-save"]))
        for case in ("locked","success","dirty","changed","copied"):
            bad=copy.deepcopy(meta)
            if case=="locked": bad["fault"]["lock"]["released"]=False
            elif case=="success": bad["fault"]["write_result"]["carry"]=False
            elif case=="dirty": bad["post_failed_map"]["record"]["dirty"]=0
            elif case=="changed": bad["post_failed_map"]["record"]["handle"]=13
            else: bad["files_after"]["SAVE02.SAV"]["exists"]=True
            with self.assertRaises(ValueError,msg=case): failed_source(bad)


if __name__=="__main__":
    unittest.main()
