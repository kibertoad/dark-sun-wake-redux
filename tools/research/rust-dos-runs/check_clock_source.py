"""Synthetic caller and near-buffer controls; no original or emulator starts."""
from pathlib import Path
import struct
import sys
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parent))
from clock_source import buffer_offsets, source_caller


class ClockChecks(unittest.TestCase):
    def test_near_buffers_require_matching_segments_and_bounds(self):
        self.assertEqual(buffer_offsets({"bp": "0110", "ds": "2000", "ss": "2000"}), (0x010C, 0x0108))
        for registers in ({"bp": "0007", "ds": "2000", "ss": "2000"},
                          {"bp": "0110", "ds": "2000", "ss": "3000"}):
            with self.assertRaises(ValueError):
                buffer_offsets(registers)

    def test_caller_requires_startup_zero_output_argument(self):
        frame = struct.pack("<HHH", 0x065C, 0x3000, 0)
        trampoline = struct.pack("<BHH", 0xEA, 0x0141, 0x3000)
        call = struct.pack("<BHH", 0x9A, 0x0963, 0x2000)
        source_caller(frame, trampoline, call, 0x2000)
        for bad in (frame[:4], struct.pack("<HHH", 0x065C, 0x3000, 1),
                    struct.pack("<HHH", 0x0663, 0x3000, 0)):
            with self.assertRaises(ValueError):
                source_caller(bad, trampoline, call, 0x2000)


if __name__ == "__main__":
    unittest.main()
