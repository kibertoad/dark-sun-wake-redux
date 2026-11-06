"""Synthetic per-call observation controls; no game execution."""
import copy
from pathlib import Path
import struct
import sys
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parent))
from check_shutdown_snapshot import Memory
from shutdown_calls import slot_call, slot_return, stream_use


class CallControls(unittest.TestCase):
    def setUp(self):
        self.memory = Memory()
        self.stop = {"reason": "breakpoint", "cs_ip": "1425:16AB",
                     "registers": {"ds": "57E0", "ss": "57E0", "si": "0000",
                                   "bx": "0000", "sp": "FF00", "ax": "1234",
                                   "system": {"mode": "real"}}}
        self.memory.word(0xFF00, 7)
        self.memory.word(0x3F4E, 7)
        self.memory.pointer(0x57E0, 0x3F4A, 0x1425, 0x0454)

    def test_actual_argument_and_changed_table(self):
        first = slot_call(self.memory, self.stop, 0)
        self.assertEqual((first["index"], first["passed_argument"], first["target"]), (0, 7, "1425:0454"))
        self.assertTrue(first["argument_equal"])
        self.memory.word(0x3F4E, 8)
        second = slot_call(self.memory, self.stop, 0)
        self.assertEqual((second["passed_argument"], second["table_argument"]), (7, 8))
        self.assertFalse(second["argument_equal"])

    def test_return_records_preservation_failure(self):
        before = slot_call(self.memory, self.stop, 0)
        after = copy.deepcopy(self.stop)
        after["cs_ip"] = "1425:16AF"
        self.assertTrue(all(slot_return(after, 0, before)["preserved"].values()))
        after["registers"]["ds"] = "6000"
        self.assertFalse(slot_return(after, 0, before)["preserved"]["ds"])
        after["registers"]["system"]["mode"] = "protected"
        self.assertFalse(slot_return(after, 0, before)["real_mode"])
        after["cs_ip"] = "1425:16B0"
        with self.assertRaisesRegex(ValueError, "return boundary"):
            slot_return(after, 0, before)

    def test_invalid_index_stride_and_binding(self):
        for field, value in (("si", "0010"), ("bx", "000E"), ("ds", "6000"), ("sp", "FFFF")):
            stop = copy.deepcopy(self.stop)
            stop["registers"][field] = value
            with self.assertRaises(ValueError):
                slot_call(self.memory, stop, 0)
        stop = copy.deepcopy(self.stop)
        stop["cs_ip"] = "1425:16AA"
        with self.assertRaisesRegex(ValueError, "boundary"):
            slot_call(self.memory, stop, 0)
        stop = copy.deepcopy(self.stop)
        stop["registers"]["system"]["mode"] = "protected"
        with self.assertRaisesRegex(ValueError, "real mode"):
            slot_call(self.memory, stop, 0)

    def test_signed_stream_and_fixed_scope(self):
        stop = copy.deepcopy(self.stop)
        stop["cs_ip"] = "1000:2F30"
        stop["registers"]["di"] = "3682"
        self.memory.word(0x3682, 0xFFFE)
        self.memory.word(0x3684, 3)
        result = stream_use(self.memory, stop, 0)
        self.assertEqual((result["count"], result["flags"], result["boundary"]), (-2, 3, "count-test"))
        stop["cs_ip"] = "1000:3F1E"
        stop["registers"]["si"] = "3692"
        self.assertEqual(stream_use(self.memory, stop, 0)["offset"], 0x3692)
        stop["registers"]["si"] = "36C2"
        with self.assertRaisesRegex(ValueError, "four fixed"):
            stream_use(self.memory, stop, 0)


if __name__ == "__main__":
    unittest.main()
