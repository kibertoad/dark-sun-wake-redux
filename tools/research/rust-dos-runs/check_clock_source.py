"""Synthetic caller and near-buffer controls; no original or emulator starts."""
from pathlib import Path
import struct
import sys
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parent))
from clock_source import buffer_offsets, source_caller, environment_words, read_environment


class ClockChecks(unittest.TestCase):
    def test_wrong_environment_segment_is_refused_before_memory_access(self):
        calls = []
        def read(*args):
            calls.append(True)
            return struct.pack("<HHH", 5, 2, 7)
        with self.assertRaises(ValueError):
            read_environment(read, "3000", "2000")
        self.assertEqual(calls, [])
        self.assertEqual(read_environment(read, "2000", "2000")["offset"], 131077)
        self.assertEqual(len(calls), 1)

    def test_environment_record_requires_declared_segment_and_complete_words(self):
        raw = struct.pack("<HHH", 5, 2, 7)
        self.assertEqual(environment_words(raw, "2000", "2000"),
                         {"offset": 131077, "daylight_enabled": 7})
        for bad, segment in ((raw[:4], "2000"), (raw, "3000")):
            with self.assertRaises(ValueError):
                environment_words(bad, segment, "2000")

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
