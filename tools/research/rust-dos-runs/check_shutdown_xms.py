"""Synthetic fixed XMS service ordering and frame controls; no native execution."""
from pathlib import Path
import struct
import sys
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parent))
from check_shutdown_snapshot import Memory
from shutdown_xms import XmsCleanup


class XmsControls(unittest.TestCase):
    def setUp(self):
        self.memory = Memory()
        self.memory.put(0x57E0, 0xFF02, struct.pack("<3H", 0x45F, 0x1425, 7))
        self.memory.pointer(0x15F3, 0xB8, 0x160, 0x1F00)
        self.memory.put(0x57E0, 0xFEF8, struct.pack("<HH", 0x62E, 0x15F3))
        self.records = []
        self.observer = XmsCleanup(0, self.records)
        self.outer = {"sp": "FF0A", "argument": 7}

    def stop(self, off, ax, **changed):
        r = {"ss": "57E0", "ds": "15F3" if off < 0x100 else "57E0",
             "bp": "FF00", "sp": "FEF4" if off < 0x100 else "FEFC",
             "ax": f"{ax:04X}", "bx": "1F00", "es": "0160", "dx": "0007",
             "system": {"mode": "real"}}
        r.update(changed)
        return {"reason": "breakpoint", "cs_ip": f"15F3:{off:04X}", "registers": r}

    def observe(self, off, ax, **changed):
        self.observer.observe(self.memory, self.stop(off, ax, **changed), self.outer)

    def query(self):
        for off, ax in ((0xCC, 0x4300), (0xCE, 0x4380), (0xD8, 0x4310), (0xDA, 0x4310)):
            self.observe(off, ax)

    def native(self, **changed):
        stop = self.stop(0x629, 0x0A01, sp="FEF8", **changed)
        stop["cs_ip"] = "0160:1F00"
        self.observer.observe(self.memory, stop, self.outer)

    def test_linked_free_request_and_result(self):
        self.query()
        self.observe(0x629, 0x0A01)
        self.native()
        self.observe(0x62E, 1)
        self.assertEqual(len(self.records), 1)
        item = self.records[0]
        self.assertTrue(item["outer_argument_equal"])
        self.assertEqual(item["queried_target"], "0160:1F00")
        self.assertTrue(item["native_entry"]["argument_equal"])
        self.assertTrue(item["free"]["target_equal"])
        self.assertTrue(item["free"]["argument_equal"])
        self.assertEqual(item["result"]["ax"], "0001")
        self.assertIsNone(self.observer.current)

    def test_absent_installation_skips_free(self):
        self.observe(0xCC, 0x4300)
        self.observe(0xCE, 0x4300)
        self.observe(0x62E, 0)
        self.assertNotIn("free", self.records[0])
        self.assertEqual(self.records[0]["result"]["branch"], "skip-return")

    def test_wrong_selector_order_and_unpaired_return(self):
        for off, ax in ((0xCC, 0x4310), (0xDA, 0x4310), (0x62E, 1)):
            self.setUp()
            with self.assertRaises(ValueError):
                self.observe(off, ax)
        self.setUp()
        self.query()
        with self.assertRaisesRegex(ValueError, "selector"):
            self.observe(0x629, 0x0B01)

    def test_wrong_frame_and_segment(self):
        for changed in ({"ds": "57E0"}, {"sp": "FEF6"}, {"bp": "FF02"},
                        {"ss": "57E1"}, {"system": {"mode": "protected"}}):
            self.setUp()
            with self.assertRaises(ValueError):
                self.observe(0xCC, 0x4300, **changed)
        self.setUp()
        self.memory.put(0x57E0, 0xFF02, struct.pack("<H", 0x460))
        with self.assertRaisesRegex(ValueError, "caller"):
            self.observe(0xCC, 0x4300)

    def test_changed_target_is_retained_before_stop(self):
        self.query()
        self.memory.pointer(0x15F3, 0xB8, 0xF000, 0x1235)
        with self.assertRaisesRegex(ValueError, "pointer"):
            self.observe(0x629, 0x0A01)
        self.assertFalse(self.records[0]["free"]["target_equal"])

    def test_argument_mismatch_and_native_failure_are_observations(self):
        self.query()
        self.observe(0x629, 0x0A01, dx="0008")
        self.native(dx="0008")
        self.observe(0x62E, 0, bx="00A2")
        self.assertFalse(self.records[0]["free"]["argument_equal"])
        self.assertEqual(self.records[0]["result"]["ax"], "0000")
        self.assertEqual(self.records[0]["result"]["bx"], "00A2")

    def test_native_entry_and_return_frame_required(self):
        self.query()
        self.observe(0x629, 0x0A01)
        with self.assertRaises(ValueError):
            self.observe(0x62E, 1)
        self.memory.put(0x57E0, 0xFEF8, struct.pack("<HH", 0x62F, 0x15F3))
        with self.assertRaisesRegex(ValueError, "return pair"):
            self.native()


if __name__ == "__main__":
    unittest.main()
