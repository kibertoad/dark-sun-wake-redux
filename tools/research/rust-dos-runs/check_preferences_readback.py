"""Synthetic menu and getter/store controls; no original content or emulator.

FND-CONFIG-235/236, FND-PARTY-031 and FND-UI-010/054 supply named
addresses; data values below are invented.
"""
from pathlib import Path
import struct
import sys
from types import SimpleNamespace
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parent))
from preferences_readback import MenuFlow, Readback, input_poll


def window(number, flags=8, mouse=1):
    return {"window": {"number": number, "flags": flags} if number else None, "mouse_enabled": mouse}


def stop(at="4A6E:011F", sp=0x1000, ax=0x00FF):
    return {"reason": "breakpoint", "cs_ip": at, "registers": {
        "system": {"mode": "real"}, "ds": "57E0", "ss": "57E0",
        "sp": f"{sp:04X}", "ax": f"{ax:04X}",
    }}


class Memory:
    def __init__(self):
        self.data = bytearray(0x100000)
        self.reads, self.short = [], False
        self.put("57E0:3411", struct.pack("<HH", 0x100, 0x6000))
        self.put("6000:0108", struct.pack("<H", 122))
        self.put("57E0:13F7", bytes([1]))
        self.put("57E0:1436", bytes([1]))
        self.put("57E0:26B4", bytes([37]))
        self.put("57E0:343E", struct.pack("<H", 0xFFFF))
        self.put("57E0:3464", struct.pack("<H", 0xFFFF))
        self.put("4E71:0B51", struct.pack("<H", 259))
        self.put("57E0:1000", struct.pack("<HH", 0x003E, 0x7000))
        self.put("5782:0000", struct.pack("<BHH", 0xEA, 0, 0x7000))
        self.put("7000:0039", struct.pack("<BHH", 0x9A, 0x011F, 0x4A6E))
        self.put("7000:003E", struct.pack("<BH", 0xA2, 0x26B4))
        self.put("277B:0541", struct.pack("<BHH", 0x9A, 0x097D, 0x39D1))

    def put(self, address, body):
        s, o = (int(x, 16) for x in address.split(":"))
        self.data[s * 16 + o:s * 16 + o + len(body)] = body

    def read_memory(self, address, size):
        s, o = (int(x, 16) for x in address.split(":"))
        assert 0 < size <= 261 and o + size <= 0x10000 and s * 16 + o + size <= len(self.data)
        self.reads.append((address, size))
        body = bytes(self.data[s * 16 + o:s * 16 + o + size])
        return SimpleNamespace(data=body[:-1] if self.short else body)


class Controls(unittest.TestCase):
    def test_poll_admission_requires_main_call_not_the_choice_loop(self):
        memory = Memory()
        self.assertEqual(input_poll(memory, 0), "39D1:097D")
        for opcode, offset, segment in ((0x9A, 0x0854, 0x39D1), (0xEA, 0x097D, 0x39D1),
                                         (0x9A, 0x097D, 0x39D2)):
            memory.put("277B:0541", struct.pack("<BHH", opcode, offset, segment))
            with self.assertRaises(ValueError):
                input_poll(memory, 0)

    def test_menu_route_requires_ready_menu_and_separate_motion_delivery(self):
        flow = MenuFlow()
        self.assertIsNone(flow.poll(window(19501), True))
        self.assertIsNone(flow.poll(window(10500), False))
        self.assertIsNone(flow.poll(window(10500, flags=0), True))
        self.assertIsNone(flow.poll(window(10500, mouse=0), True))
        self.assertEqual(flow.poll(window(10500), True), "move")
        self.assertIsNone(flow.poll(window(10500), False))
        self.assertEqual(flow.poll(window(10500), True), "click")
        self.assertEqual(flow.phase, "getter")
        self.assertIsNone(flow.poll(window(None), True))
        with self.assertRaisesRegex(ValueError, "window"):
            flow.poll(window(16500), True)

    def test_unexpected_menu_or_lost_menu_after_motion_rejects(self):
        for number in (14000, 18500, 16500):
            with self.assertRaises(ValueError):
                MenuFlow().poll(window(number), True)
        flow = MenuFlow()
        flow.poll(window(10500), True)
        with self.assertRaises(ValueError):
            flow.poll(window(None), True)

    def test_entry_records_full_width_mode_and_exact_caller(self):
        memory, trace = Memory(), Readback(0)
        result = trace.entry(memory, stop())
        self.assertEqual(result["library_mode"], 259)
        self.assertIn(("4E71:0B51", 2), memory.reads)
        self.assertEqual(result["card_id"], 122)
        self.assertEqual(result["music_level"], 37)
        self.assertEqual(result["getter_handle"], 0xFFFF)
        self.assertEqual(trace.caller, 0x7000)
        self.assertEqual(trace.stack, (0x57E0, 0x1004))

    def test_actual_word_result_and_low_byte_store_are_distinct(self):
        memory, trace = Memory(), Readback(0)
        trace.entry(memory, stop())
        returned = trace.returned(memory, stop("7000:003E", 0x1004, 0x12FF))
        self.assertEqual(returned["ax"], 0x12FF)
        self.assertEqual(returned["music_level"], 37)
        memory.put("57E0:26B4", bytes([255]))
        stored = trace.stored(memory, stop("7000:0041", 0x1004, 0x12FF))
        self.assertEqual(stored["music_level"], 255)
        self.assertEqual(trace.phase, "done")

    def test_wrong_boundary_mode_or_segment_rejects_before_reads(self):
        controls = [("reason", "pause"), ("cs_ip", "4A6E:0120")]
        for key, value in controls:
            memory, sample = Memory(), stop()
            sample[key] = value
            with self.assertRaises(ValueError):
                Readback(0).entry(memory, sample)
            self.assertEqual(memory.reads, [])
        for key, value in (("ds", "57E1"), ("ss", "57E1"), ("sp", "unknown"), ("ax", "10000")):
            memory, sample = Memory(), stop()
            sample["registers"][key] = value
            with self.assertRaises(ValueError):
                Readback(0).entry(memory, sample)
            self.assertEqual(memory.reads, [])
        memory, sample = Memory(), stop()
        sample["registers"]["system"]["mode"] = "protected"
        with self.assertRaises(ValueError):
            Readback(0).entry(memory, sample)
        self.assertEqual(memory.reads, [])

    def test_trampoline_call_store_and_return_pair_controls(self):
        for address, body in (
            ("5782:0000", struct.pack("<BHH", 0xEA, 1, 0x7000)),
            ("5782:0000", struct.pack("<BHH", 0xEA, 0, 0x7001)),
            ("7000:0039", struct.pack("<BHH", 0x9A, 0x0120, 0x4A6E)),
            ("7000:0039", struct.pack("<BHH", 0x9A, 0x011F, 0x4A6F)),
            ("7000:003E", struct.pack("<BH", 0xA2, 0x26B5)),
            ("57E0:1000", struct.pack("<HH", 0x003F, 0x7000)),
            ("57E0:13F7", b"\0"), ("57E0:1436", b"\0"),
        ):
            memory = Memory()
            memory.put(address, body)
            with self.subTest(address=address), self.assertRaises(ValueError):
                Readback(0).entry(memory, stop())

    def test_null_short_wrapping_inputs_fail(self):
        for segment, offset in ((0, 0), (0x6000, 0xFFF0), (0xFFFF, 0x10)):
            memory = Memory()
            memory.put("57E0:3411", struct.pack("<HH", offset, segment))
            with self.assertRaises(ValueError):
                Readback(0).entry(memory, stop())
        memory = Memory()
        memory.short = True
        with self.assertRaisesRegex(ValueError, "short"):
            Readback(0).entry(memory, stop())
        with self.assertRaises(ValueError):
            Readback(0).entry(Memory(), stop(sp=0xFFFD))

    def test_unpaired_reentry_wrong_frame_and_changed_overlay_fail(self):
        memory, trace = Memory(), Readback(0)
        with self.assertRaises(ValueError):
            trace.returned(memory, stop("7000:003E", 0x1004))
        with self.assertRaises(ValueError):
            trace.stored(memory, stop("7000:0041", 0x1004))
        trace.entry(memory, stop())
        with self.assertRaises(ValueError):
            trace.entry(memory, stop())
        with self.assertRaises(ValueError):
            trace.returned(memory, stop("7000:003E", 0x1006))
        memory.put("5782:0000", struct.pack("<BHH", 0xEA, 0, 0x7001))
        with self.assertRaises(ValueError):
            trace.returned(memory, stop("7000:003E", 0x1004))

    def test_changed_result_or_wrong_stored_byte_fails(self):
        for ax, level in ((0x00FE, 255), (0x00FF, 37)):
            memory, trace = Memory(), Readback(0)
            trace.entry(memory, stop())
            trace.returned(memory, stop("7000:003E", 0x1004))
            memory.put("57E0:26B4", bytes([level]))
            with self.assertRaises(ValueError):
                trace.stored(memory, stop("7000:0041", 0x1004, ax))

    def test_unsupported_relocations_fail(self):
        for delta in (True, None, 1, -0x1001):
            with self.assertRaises(ValueError):
                Readback(delta)


if __name__ == "__main__":
    unittest.main()
