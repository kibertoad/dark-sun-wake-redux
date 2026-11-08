"""Synthetic fixed post-Save source and natural turn admission controls."""
from pathlib import Path
import struct
import sys
import unittest
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parent))
from check_preferences_key import fixture, point, prefix
from preferences_saved import source_control, verify_save, SavedKeyTrace


def metadata():
    return {"passed": True, "writes": [], "checkpoint": {"name": "saved-map", "checked": True},
            "inputs": [{"key": key, "delivery": {"state": "done"}} for key in ("f1", "enter")],
            "save_after": {"exists": True, "length": 12, "sha256": "synthetic"},
            "saved_state": {"synthetic": True}, "frame_sha256": "synthetic"}


class Controls(unittest.TestCase):
    def test_source(self):
        m = metadata()
        self.assertEqual(source_control(m), {"state": m["saved_state"], "frame_sha256": "synthetic"})

    def test_source_refuses_incomplete_or_written(self):
        for field, value in (("passed", False), ("writes", [{}]), ("inputs", []),
                             ("checkpoint", {"name": "saved-map", "checked": False})):
            m = metadata(); m[field] = value
            with self.assertRaises(ValueError): source_control(m)
        for key in ("state", "key"):
            m = metadata()
            if key == "state": m["inputs"][1]["delivery"][key] = "pending"
            else: m["inputs"][0][key] = "p"
            with self.assertRaises(ValueError): source_control(m)

    def test_file_digest(self):
        m = metadata()
        with patch("preferences_saved.named_file", return_value=m["save_after"]):
            self.assertEqual(verify_save(Path("synthetic"), m), m["save_after"])
        with patch("preferences_saved.named_file", return_value={"exists": False}):
            with self.assertRaises(ValueError): verify_save(Path("synthetic"), m)

    def baseline(self, turn=1, leader=1, combat=0):
        c = fixture()
        for address, value in (("57E0:426D", turn), ("4C13:0369", leader), ("4C10:0019", combat)):
            c.put(address, struct.pack("<H", value & 0xFFFF))
        stop = point("1000:0000", 1); stop["reason"] = "load"
        return c, stop

    def test_natural_turn(self):
        c, stop = self.baseline()
        trace = SavedKeyTrace(0); trace.baseline(c, stop)
        self.assertEqual(trace.report["baseline"]["turn"], 1)
        trace.arm(c)
        with self.assertRaises(ValueError): trace.baseline(c, stop)

    def test_turn_refusal_retains_actual(self):
        for turn, leader, combat in ((9999, 1, 0), (-1, 1, 0), (1, 2, 0), (1, 1, 1)):
            c, stop = self.baseline(turn, leader, combat)
            trace = SavedKeyTrace(0)
            with self.assertRaises(ValueError): trace.baseline(c, stop)
            self.assertEqual(trace.report["baseline"]["turn"], turn)
        with self.assertRaises(ValueError): SavedKeyTrace(0).arm(fixture())

    def test_rejected_outer_pair_retained(self):
        c, stop = self.baseline()
        trace = SavedKeyTrace(0); trace.baseline(c, stop)
        prefix(c, trace)
        c.put("57E0:2FEC", struct.pack("<HH", 0x1EC9, 0x6400))
        with self.assertRaises(ValueError):
            trace.nested(c, point("4A6E:011F", 12, sp=0x2FE6, bp=0x2FEA), "getter")
        self.assertEqual(trace.report["events"][-1]["preferences_return"], "6400:1EC9")
        self.assertFalse(trace.report["events"][-1]["accepted"])
        self.assertEqual(trace.phase, "getter")
        self.assertFalse(trace.report["complete"])

    def test_declared_header_return(self):
        c, stop = self.baseline()
        trace = SavedKeyTrace(0); trace.baseline(c, stop); prefix(c, trace)
        c.put("57E0:2FEC", struct.pack("<HH", 0, 0x571F))
        c.put("571F:0000", bytes((0xCD, 0x3F, 0xC9, 0x1E)))
        c.put("571F:0020", bytes((0xCD, 0x3F, 0x9B, 0x13, 0)))
        trace.nested(c, point("4A6E:011F", 12, sp=0x2FE6, bp=0x2FEA), "getter")
        self.assertEqual(trace.report["events"][-1]["return_representation"], "unloaded-header")
        for addr, value in (("571F:0000", bytes(4)), ("571F:0020", bytes(5))):
            original = bytes((0xCD, 0x3F, 0xC9, 0x1E)) if addr.endswith("0000") else bytes((0xCD, 0x3F, 0x9B, 0x13, 0))
            c.put(addr, value)
            with self.assertRaises(ValueError): trace.outer(c, {"ss": 0x57E0})
            c.put(addr, original)


if __name__ == "__main__": unittest.main()
