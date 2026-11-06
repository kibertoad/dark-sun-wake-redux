"""Controlled pending metadata and fixed preparation pairing; synthetic only."""
import copy
import hashlib
from pathlib import Path
import struct
import sys
from types import SimpleNamespace
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parent))
from check_shutdown_snapshot import Memory
from shutdown_dirty import seed_active, Preparation
from shutdown_run import controlled_stage


class DirtyControls(unittest.TestCase):
    def test_fixed_optional_stage(self):
        self.assertFalse(controlled_stage(["source", "run"]))
        self.assertTrue(controlled_stage(["source", "run", "dirty-active"]))
        for args in ([], ["source"], ["../source", "run"],
                     ["source", "run", "7000:012C"], ["source", "run", "dirty-active", "1"]):
            with self.assertRaises(SystemExit):
                controlled_stage(args)

    def setUp(self):
        self.memory = Memory()
        self.stop = {"reason": "breakpoint", "cs_ip": "1000:0388",
                     "registers": {"ds": "57E0", "ss": "57E0", "sp": "FF00",
                                   "system": {"mode": "real"}}}
        self.memory.put(0x57E0, 0xFF00, struct.pack("<4H", 0x03EC, 0, 0, 0))
        for field in (0x9DA3, 0x9D9F):
            self.memory.pointer(0x57E0, field, 0x7000, 0x100)
        self.memory.put(0x7000, 0x104, struct.pack("<HH", 9, 2))
        self.writes = []

    def write(self, contract, name, body, expected):
        self.writes.append((contract, name, body, expected))
        return SimpleNamespace(to_json=lambda: {"status": "verified"})

    def seed(self):
        return seed_active(SimpleNamespace(client=self.memory, write=self.write), self.stop, 0)

    def test_single_guarded_named_byte(self):
        result = self.seed()
        contract, name, body, expected = self.writes[0]
        self.assertEqual(len(self.writes), 1)
        self.assertEqual(len(contract.fields), 1)
        self.assertEqual((name, contract.fields[0].address, contract.fields[0].length, body),
                         ("archive_pending_metadata", "7000:012C", 1, b"\x01"))
        self.assertEqual(expected, hashlib.sha256(b"\x00").hexdigest())
        self.assertEqual(result["kind"], "controlled-setup")
        self.assertEqual(result["verified"]["status"], "verified")

    def test_bad_setup_has_no_write(self):
        for case in ("dirty", "option", "head", "frame"):
            self.setUp()
            if case == "dirty":
                self.memory.put(0x7000, 0x12C, b"\x01")
            elif case == "option":
                self.memory.put(0x7000, 0x106, b"\x00\x00")
            elif case == "head":
                self.memory.pointer(0x57E0, 0x9D9F, 0x7100, 0x100)
            else:
                self.stop["registers"]["ss"] = "57E1"
            with self.assertRaises(ValueError):
                self.seed()
            self.assertEqual(self.writes, [])

    def test_session_write_failure_is_not_reported_verified(self):
        def fail(*args):
            raise RuntimeError("old-byte guard failed")
        with self.assertRaisesRegex(RuntimeError, "old-byte"):
            seed_active(SimpleNamespace(client=self.memory, write=fail), self.stop, 0)

    def pair(self):
        pair = Preparation(0)
        item = {"return": "38FF:0324", "argument": 0x70000100}
        pair.entry(item, self.stop)
        returned = copy.deepcopy(self.stop)
        returned["cs_ip"] = "38FF:0324"
        returned["registers"].update(sp="FF04", ax="0000")
        return pair, item, returned

    def test_preparation_result_and_rejected_pairing(self):
        pair, item, returned = self.pair()
        pair.returned(self.memory, returned)
        self.assertEqual(item["result"], {"ax": "0000", "dirty": 0, "io_handle": 9})
        with self.assertRaises(ValueError):
            pair.returned(self.memory, returned)
        pair, item, returned = self.pair()
        with self.assertRaisesRegex(ValueError, "nested"):
            pair.entry(item, self.stop)
        returned["registers"]["sp"] = "FF06"
        with self.assertRaisesRegex(ValueError, "frame"):
            pair.returned(self.memory, returned)
        with self.assertRaisesRegex(ValueError, "caller"):
            Preparation(0).entry({"return": "38FF:0325"}, self.stop)


if __name__ == "__main__":
    unittest.main()
