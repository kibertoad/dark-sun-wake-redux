"""Synthetic caller and near-buffer controls; no original or emulator starts."""
from pathlib import Path
import struct
import sys
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parent))
from clock_source import buffer_offsets, source_caller, environment_words, read_environment
from clock_source import constructor_caller, timezone_caller, constructor_sizes, constructor_array


class ClockChecks(unittest.TestCase):
    def test_initialization_records_require_distinct_near_and_far_frames(self):
        near_frame = struct.pack("<HH", 0x0260, 0x3000)
        near_record = struct.pack("<BBHH", 0xFF, 16, 0x29AA, 0)
        far_frame = struct.pack("<HHH", 0x0259, 0x2000, 0x3000)
        far_record = struct.pack("<BBHH", 0xFF, 30, 0x3B56, 0x2000)
        constructor_caller(near_frame, near_record, 0x3000)
        timezone_caller(far_frame, far_record, 0x2000, 0x3000)
        for frame, record in ((near_frame[:2], near_record),
                              (struct.pack("<HH", 0x0259, 0x3000), near_record),
                              (near_frame, struct.pack("<BBHH", 0, 16, 0x29AA, 0))):
            with self.assertRaises(ValueError):
                constructor_caller(frame, record, 0x3000)
        with self.assertRaises(ValueError):
            timezone_caller(far_frame, far_record, 0x4000, 0x3000)

    def test_constructor_sizes_keep_source_opaque_and_enforce_scan_bounds(self):
        self.assertEqual(constructor_sizes(struct.pack("<HHH", 2, 0, 8)),
                         {"payload_bytes": 2, "source_segment": 0, "table_bytes": 8})
        for raw in (b"", struct.pack("<HHH", 1, 0, 8),
                    struct.pack("<HHH", 32768, 0, 8), struct.pack("<HHH", 2, 0, 10)):
            with self.assertRaises(ValueError):
                constructor_sizes(raw)

    def test_constructor_array_reads_only_terminated_pointer_metadata(self):
        calls = []
        words = {0x0200: 0x0100, 0x0202: 0x0103, 0x0204: 0}
        def read(segment, offset, size):
            calls.append((segment, offset, size))
            return struct.pack("<H", words[offset])
        self.assertEqual(constructor_array(read, "3000", "3000", 0x0100, 8, 0x0200, 8),
                         {"member_count": 2, "terminated": True, "members_inside_payload": True})
        self.assertEqual([offset for _, offset, _ in calls], [0x0200, 0x0202, 0x0204])
        words[0x0200] = 0
        self.assertEqual(constructor_array(read, "3000", "3000", 0x0100, 2, 0x0200, 8)["member_count"], 0)

    def test_constructor_array_refuses_invalid_spans_before_memory_access(self):
        calls = []
        def read(*args):
            calls.append(True)
            return b"\0\0"
        for actual, payload, table in (("4000", 0x0100, 0x0200),
                                       ("3000", 0, 0x0200),
                                       ("3000", 0xFFFF, 0x0200),
                                       ("3000", 0x0100, 0x0100),
                                       ("3000", 0x0100, 0xFFFE)):
            with self.assertRaises(ValueError):
                constructor_array(read, actual, "3000", payload, 2, table, 8)
        self.assertEqual(calls, [])

    def test_constructor_array_rejects_outside_repeated_and_unterminated_members(self):
        for words in ([0x0108], [0x0100, 0x0100], [0x0100, 0x0102, 0x0104, 0x0106]):
            def read(segment, offset, size):
                return struct.pack("<H", words[(offset - 0x0200) // 2])
            with self.assertRaises(ValueError):
                constructor_array(read, "3000", "3000", 0x0100, 8, 0x0200, 8)

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
