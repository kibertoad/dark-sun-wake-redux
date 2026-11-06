"""Synthetic admission and bounded-read controls; no emulator or licensed input."""
import copy
from pathlib import Path
import struct
import sys
import unittest
from types import SimpleNamespace

sys.path.insert(0, str(Path(__file__).resolve().parent))
from shutdown_snapshot import capture


class Memory:
    def __init__(self):
        self.bytes = {}
        self.reads = []
        self.short = False

    def put(self, segment, offset, body):
        for i, value in enumerate(body):
            self.bytes[segment * 16 + offset + i] = value

    def word(self, offset, value):
        self.put(0x57E0, offset, struct.pack("<H", value))

    def pointer(self, segment, offset, target_segment, target_offset):
        self.put(segment, offset, struct.pack("<HH", target_offset, target_segment))

    def read_memory(self, address, size):
        segment, offset = (int(x, 16) for x in address.split(":"))
        self.reads.append((segment, offset, size))
        body = bytes(self.bytes.get(segment * 16 + offset + i, 0) for i in range(size))
        return SimpleNamespace(data=body[:-1] if self.short else body)


class SnapshotControls(unittest.TestCase):
    def setUp(self):
        self.memory = Memory()
        self.stop = {"reason": "breakpoint", "cs_ip": "1000:0388",
                     "registers": {"ds": "57E0", "ss": "57E0", "sp": "FF00",
                                   "system": {"mode": "real"}}}
        self.memory.put(0x57E0, 0xFF00, struct.pack("<4H", 0x03EC, 7, 0, 0))

    def test_named_positive(self):
        m = self.memory
        m.word(0x3572, 1)
        m.pointer(0x57E0, 0xA446, 0x1234, 0x5678)
        m.pointer(0x57E0, 0x9DA3, 0x7000, 0x100)
        m.pointer(0x57E0, 0x9D9F, 0x7000, 0x100)
        m.put(0x7000, 0x104, struct.pack("<HH", 9, 2))
        m.put(0x7000, 0x128, struct.pack("<IB", 42, 1))
        m.word(0x3682, 0xFFFD)
        m.word(0x3F4E, 9)
        m.pointer(0x57E0, 0x3F4A, 0x1425, 0x007F)
        result = capture(m, self.stop, 0)
        self.assertEqual(result["callbacks"], ["1234:5678"])
        self.assertEqual(result["archives"][0]["dirty"], 1)
        self.assertEqual(result["active_record"]["handle"], 42)
        self.assertEqual(result["streams"][0]["count"], -3)
        self.assertEqual(result["slots"][0], {"index": 0, "argument": 9, "target": "1425:007F"})
        self.assertTrue(result["active_in_head"])
        self.assertTrue(all(n <= 128 and off + n <= 0x10000 for _, off, n in m.reads))

    def test_stop_admission_before_reads(self):
        for field, value in [("reason", "load"), ("cs_ip", "1000:0389")]:
            stop = copy.deepcopy(self.stop)
            stop[field] = value
            with self.assertRaises(ValueError):
                capture(self.memory, stop, 0)
        for field, value in [("ds", "57E1"), ("ss", "57E1")]:
            stop = copy.deepcopy(self.stop)
            stop["registers"][field] = value
            with self.assertRaises(ValueError):
                capture(self.memory, stop, 0)
        stop = copy.deepcopy(self.stop)
        stop["registers"]["system"]["mode"] = "protected"
        with self.assertRaises(ValueError):
            capture(self.memory, stop, 0)
        self.assertEqual(self.memory.reads, [])

    def test_frame_count_and_short_read(self):
        self.memory.put(0x57E0, 0xFF00, struct.pack("<4H", 0x03EC, 0, 1, 0))
        with self.assertRaisesRegex(ValueError, "frame"):
            capture(self.memory, self.stop, 0)
        self.setUp()
        self.memory.word(0x3572, 33)
        with self.assertRaisesRegex(ValueError, "capacity32"):
            capture(self.memory, self.stop, 0)
        self.setUp()
        self.memory.short = True
        with self.assertRaisesRegex(ValueError, "short"):
            capture(self.memory, self.stop, 0)

    def test_cycles_and_named_field_wrap(self):
        m = self.memory
        m.pointer(0x57E0, 0x9DA3, 0x7000, 0x100)
        m.pointer(0x7000, 0x10C, 0x7000, 0x100)
        with self.assertRaisesRegex(ValueError, "cycle"):
            capture(m, self.stop, 0)
        m.pointer(0x57E0, 0x9DA3, 0x7000, 0xFFF0)
        with self.assertRaisesRegex(ValueError, "bounds"):
            capture(m, self.stop, 0)

    def test_archive_tooling_limit(self):
        m = self.memory
        m.pointer(0x57E0, 0x9DA3, 0x7000, 0)
        for i in range(65):
            m.pointer(0x7000, i * 0x40 + 0x0C, 0x7000, (i + 1) * 0x40)
        with self.assertRaisesRegex(ValueError, "limit64"):
            capture(m, self.stop, 0)
        m.pointer(0x7000, 63 * 0x40 + 0x0C, 0, 0)
        self.assertEqual(len(capture(m, self.stop, 0)["archives"]), 64)


if __name__ == "__main__":
    unittest.main()
