"""Synthetic frame graph controls; no original bytes or emulator."""
from pathlib import Path
import struct
import sys
import unittest
sys.path.insert(0, str(Path(__file__).resolve().parent))
from frame_links import observe


class Controls(unittest.TestCase):
    def reader(self, words):
        calls = []
        def read(ss, off, size):
            calls.append((ss, off, size))
            return b"".join(struct.pack("<H", words.get(x, 0))
                            for x in range(off, off + size, 2))
        return read, calls

    def test_sentinels(self):
        for link in (0, 1):
            read, calls = self.reader({0x100: link})
            result = observe(read, 0x2000, 0x100)
            self.assertEqual(result["end"], "sentinel")
            self.assertEqual(calls, [(0x2000, 0x100, 2)])

    def test_even_and_odd_links(self):
        read, calls = self.reader({0x100: 0x201, 0x200: 0x300,
                                   0x202: 0x1234, 0x204: 0x4567, 0x300: 1})
        result = observe(read, 0x2000, 0x100)
        self.assertEqual(result["end"], "sentinel")
        self.assertFalse(result["nodes"][0]["compares_return_segment"])
        self.assertEqual(result["nodes"][1]["return_segment"], 0x4567)
        self.assertEqual(result["nodes"][1]["return_offset"], 0x1234)
        self.assertNotIn((0x2000, 0x102, 4), calls)

    def test_cycle(self):
        read, _ = self.reader({0x100: 0x200, 0x200: 0x100})
        result = observe(read, 0x2000, 0x100)
        self.assertEqual((result["end"], len(result["nodes"])), ("cycle", 2))

    def test_limit(self):
        read, _ = self.reader({x: x + 8 for x in range(0x100, 0x400, 8)})
        result = observe(read, 0x2000, 0x100)
        self.assertEqual((result["end"], len(result["nodes"])), ("node-limit", 64))

    def test_crossings(self):
        for ss, bp in ((0x2000, 0xFFFF), (0xFFFF, 0x10)):
            read, calls = self.reader({})
            self.assertEqual(observe(read, ss, bp)["end"], "link-crossing")
            self.assertEqual(calls, [])
        read, calls = self.reader({0xFFFC: 0x100})
        self.assertEqual(observe(read, 0x2000, 0xFFFC)["end"], "return-crossing")
        self.assertEqual(calls, [(0x2000, 0xFFFC, 2)])

    def test_short_read(self):
        with self.assertRaises(struct.error):
            observe(lambda *_: b"", 0x2000, 0x100)


if __name__ == "__main__":
    unittest.main()
