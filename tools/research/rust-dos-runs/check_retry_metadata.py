"""Synthetic directory-bound and same-record controls for Save retry."""
import copy
import struct
import tempfile
import unittest
from pathlib import Path

from retry_metadata import disk, memory, summary, source_record
from check_failed_save import Memory


class RetryMetadataControls(unittest.TestCase):
    def setUp(self):
        self.header=struct.pack("<4s6I",b"GFFI",0x30000,28,128,20,0,1)
        self.directory=b"synthetic-directory!"
        self.directory=self.directory.ljust(20,b".")

    def test_retry_checkpoint_requires_unseeded_success_and_same_record(self):
        r={"pointer":"7100:0100","io_handle":7,"options":2,"flags":0,"handle":1,"dirty":1}
        meta={"retry":True,"passed":True,"writes":[],"fault":{"record":r,"write_result":{"carry":False}},
            "resource_result":{"ax":"0000"},"numbered_result":{"success_byte":1},
            "copy_entry":{"ownership":0},"checkpoint":{"name":"retry-map","checked":True},
            "post_failed_map":{"record":r},"files_after":{"SAVE02.SAV":{"exists":True}}}
        self.assertEqual(source_record(meta),r)
        for case in ("seed","failure","lock","ownership","checkpoint","changed","clean"):
            bad=copy.deepcopy(meta)
            if case=="seed": bad["writes"]=[{"field":"synthetic"}]
            elif case=="failure": bad["fault"]["write_result"]["carry"]=True
            elif case=="lock": bad["fault"]["lock"]={}
            elif case=="ownership": bad["copy_entry"]["ownership"]=1
            elif case=="checkpoint": bad["checkpoint"]["checked"]=False
            elif case=="changed": bad["post_failed_map"]["record"]=dict(r,io_handle=8)
            else: bad["post_failed_map"]["record"]=dict(r,dirty=0)
            with self.assertRaises(ValueError,msg=case): source_record(bad)

    def test_disk_matches_metadata_without_equating_payloads(self):
        with tempfile.TemporaryDirectory() as name:
            run=Path(name)/"run"
            (run/"drive-c").mkdir(parents=True)
            target=run/"drive-c/DARKRUN.GFF"
            target.write_bytes(self.header+b"a"*100+self.directory)
            first=disk(run,"DARKRUN.GFF")
            target.write_bytes(self.header+b"b"*100+self.directory)
            self.assertEqual(first,disk(run,"DARKRUN.GFF"))
            self.assertEqual(first,summary(self.header,self.directory))
            target.write_bytes(self.header+b"b"*100+b"x"*20)
            self.assertNotEqual(first["directory_sha256"],disk(run,"DARKRUN.GFF")["directory_sha256"])

    def test_file_name_nonfile_and_outside_directory_rejected(self):
        with tempfile.TemporaryDirectory() as name:
            run=Path(name)/"run"
            (run/"drive-c").mkdir(parents=True)
            for file in ("../DARKRUN.GFF","OTHER.GFF","DARKRUN.GFF"):
                with self.assertRaises(ValueError): disk(run,file)
            target=run/"drive-c/DARKRUN.GFF"
            target.write_bytes(self.header)
            with self.assertRaisesRegex(ValueError,"bound"): disk(run,"DARKRUN.GFF")

    def test_summary_bad_signature_length_and_extent_rejected(self):
        for header,body in ((b"x"*28,self.directory),(self.header,b"short"),
                (self.header[:12]+struct.pack("<I",0)+self.header[16:],self.directory)):
            with self.assertRaises(ValueError): summary(header,body)

    def test_memory_identity_and_segment_bound(self):
        client=Memory()
        client.put(0x7100,0x104,struct.pack("<HH",7,2))
        client.put(0x7100,0x110,self.header)
        client.put(0x7100,0x12C,b"\x01")
        client.put(0x7100,0x131,self.directory)
        expected={"pointer":"7100:0100","io_handle":7,"options":2,"flags":0,"handle":1,"dirty":1}
        self.assertEqual(memory(client,expected["pointer"],expected)["metadata"],summary(self.header,self.directory))
        with self.assertRaisesRegex(ValueError,"identity"):
            memory(client,expected["pointer"],dict(expected,io_handle=8))
        client.put(0x7100,0x120,struct.pack("<I",65535))
        with self.assertRaisesRegex(ValueError,"bound"):
            memory(client,expected["pointer"],expected)

    def test_directory_larger_than_one_named_read_is_chunked(self):
        class Limited(Memory):
            def read_memory(self,address,size):
                if size>261:
                    raise AssertionError("single read exceeds the existing named-field limit")
                return super().read_memory(address,size)
        client=Limited()
        header=struct.pack("<4s6I",b"GFFI",0x30000,28,1024,600,0,1)
        directory=b"synthetic"*66+b"......"
        self.assertEqual(len(directory),600)
        client.put(0x7100,0x104,struct.pack("<HH",7,2))
        client.put(0x7100,0x110,header)
        client.put(0x7100,0x12C,b"\x01")
        client.put(0x7100,0x131,directory)
        expected={"pointer":"7100:0100","io_handle":7,"options":2,"flags":0,"handle":1,"dirty":1}
        self.assertEqual(memory(client,expected["pointer"],expected)["metadata"],summary(header,directory))


if __name__=="__main__":
    unittest.main()
