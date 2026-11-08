"""Synthetic fixed disc observer controls: no game, original bytes or emulator.

FND-SOUND-085/092/093/094/095 and FND-RNG-001/010 supply named locations;
all values and memory below are invented controls, not extracted content.
"""
import contextlib
import copy
import io
from pathlib import Path
import struct
import sys
from types import SimpleNamespace
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parent))
from disc_startup import SITES, address, snapshot, observe


def point(phase):
    return next(f"{s:04X}:{o:04X}" for (s, o), name in SITES.items() if name == phase)


def stopped(phase):
    return {"reason": "breakpoint", "cs_ip": point(phase), "registers": {
        "system": {"mode": "real"}, "ds": "57E0", "ss": "57E0", "sp": "1000",
        "ax": "4E00", "dx": "1234", "si": "0004", "es": "7000", "bx": "0215",
    }}


class Memory:
    def __init__(self):
        self.data = bytearray(0x100000)
        self.reads = []
        self.short = False
        self.put("57E0:3411", struct.pack("<HH", 0x100, 0x6000))
        self.put("6000:0114", struct.pack("<H", 11))
        self.put("57E0:3482", struct.pack("<HH", 0x200, 0x7000))
        self.put("57E0:3444", struct.pack("<H", 1))
        self.put("57E0:343E", struct.pack("<H", 0xFFFF))
        self.put("4E71:0C42", bytes([1, 2]))
        self.put("4E71:0C44", bytes([10, 20, 30, 40]))
        self.put("7000:0217", bytes([9, 8, 7, 6]))
        self.put("57E0:1000", struct.pack("<HH", 0x019D, 0x4734))

    def put(self, address, value):
        s, o = (int(x, 16) for x in address.split(":"))
        self.data[s * 16 + o:s * 16 + o + len(value)] = value

    def read_memory(self, address, size):
        self.reads.append((address, size))
        s, o = (int(x, 16) for x in address.split(":"))
        assert 0 < size <= 59 and o + size <= 0x10000 and s * 16 + o + size <= len(self.data)
        body = bytes(self.data[s * 16 + o:s * 16 + o + size])
        return SimpleNamespace(data=body[:-1] if self.short else body)


class Session:
    def __init__(self, stops, memory=None):
        self.stops = iter(stops)
        self.client = memory or Memory()
        self.breakpoints = []
        self.operations = []
        self.client.set_breakpoint = self.breakpoints.append

    def continue_(self):
        operation = object()
        self.operations.append(operation)
        return operation

    def observe(self, operation, timeout):
        assert operation is self.operations[-1] and timeout == 30
        return SimpleNamespace(pending=False, stop=next(self.stops))


class Controls(unittest.TestCase):
    def test_all_fixed_points_have_bounded_named_reads(self):
        for phase in SITES.values():
            with self.subTest(phase=phase):
                memory = Memory()
                result = snapshot(memory, stopped(phase), 0)
                self.assertEqual(result["phase"], phase)
                self.assertEqual(result["configuration_flags"], 11)
                self.assertEqual(result["music_slot"], 0xFFFF)
                self.assertEqual(result["lead_out"], "0a141e28")
                self.assertTrue(memory.reads)
                self.assertLessEqual(max(n for _, n in memory.reads), 59)

    def test_invalid_boundaries_reject_before_memory(self):
        for path, value in ((["reason"], "pause"), (["cs_ip"], "1000:0000"),
                            (["registers", "system", "mode"], "protected"),
                            (["registers", "ds"], "57E1"), (["registers", "ss"], "57E1"),
                            (["registers", "sp"], "unknown")):
            memory = Memory()
            stop = stopped("initializer-entry")
            target = stop
            for key in path[:-1]:
                target = target[key]
            target[path[-1]] = value
            with self.subTest(path=path), self.assertRaises(ValueError):
                snapshot(memory, stop, 0)
            self.assertEqual(memory.reads, [])

    def test_configuration_and_short_read_refusals(self):
        for segment, offset in ((0, 0), (0x6000, 0xFFF0), (0xFFFF, 0x10)):
            memory = Memory()
            memory.put("57E0:3411", struct.pack("<HH", offset, segment))
            with self.subTest(segment=segment, offset=offset), self.assertRaises(ValueError):
                snapshot(memory, stopped("initializer-entry"), 0)
        memory = Memory()
        memory.short = True
        with self.assertRaisesRegex(ValueError, "short"):
            snapshot(memory, stopped("initializer-entry"), 0)

    def test_callback_requires_exact_return_pair_and_frame_extent(self):
        for body, sp in ((struct.pack("<HH", 0x019E, 0x4734), "1000"),
                         (struct.pack("<HH", 0x019D, 0x4735), "1000"),
                         (b"", "FFFF")):
            memory = Memory()
            memory.put("57E0:1000", body)
            stop = stopped("callback-entry")
            stop["registers"]["sp"] = sp
            with self.assertRaises(ValueError):
                snapshot(memory, stop, 0)

    def test_allocation_reads_full_dword_and_final_target_is_exact(self):
        memory = Memory()
        memory.put("57E0:1000", struct.pack("<I", 0x12345678))
        self.assertEqual(snapshot(memory, stopped("allocation-call"), 0)["allocation_size"], 0x12345678)
        for phase in ("table-write-before", "table-write-after"):
            result = snapshot(memory, stopped(phase), 0)
            self.assertEqual(result["table_write_target"], "7000:0217")
            self.assertEqual(result["table_write_bytes"], "09080706")
        for key in ("es", "bx"):
            stop = stopped("table-write-before")
            stop["registers"][key] = "0000"
            with self.assertRaisesRegex(ValueError, "destination"):
                snapshot(memory, stop, 0)
        for segment, offset in ((0, 0), (0x7000, 0xFFF0), (0xFFFF, 0)):
            memory = Memory()
            memory.put("57E0:3482", struct.pack("<HH", offset, segment))
            with self.assertRaises(ValueError):
                snapshot(memory, stopped("table-write-after"), 0)

    def execute(self, phases, memory=None):
        session = Session([stopped(phase) for phase in phases], memory)
        report = {"events": [], "complete": False}
        with contextlib.redirect_stdout(io.StringIO()):
            observe(session, 0, report, lambda: None)
        return session, report

    def test_bypass_and_callback_failure_paths_finish_at_gate(self):
        for middle in ([], ["callback-call", "callback-entry", "presence-return", "callback-return"]):
            session, report = self.execute(["initializer-entry", "callback-gate", *middle,
                                            "callback-join", "music-gate"])
            self.assertTrue(report["complete"])
            self.assertEqual(len(session.breakpoints), len(SITES) + 2)
            self.assertEqual(len(session.operations), len(report["events"]))

    def test_completed_final_write_pair_and_iterations(self):
        _, report = self.execute(["initializer-entry", "callback-gate", "callback-call", "callback-entry",
            "presence-return", "metadata-return", "allocation-call", "allocation-return",
            "track-return", "track-return", "table-write-before", "table-write-after",
            "callback-return", "callback-join", "music-gate"])
        self.assertTrue(report["complete"])
        self.assertEqual(sum(e["phase"] == "track-return" for e in report["events"]), 2)

    def test_missing_caller_entry_join_and_pending_write_fail(self):
        for phases in (["callback-gate"], ["initializer-entry", "callback-entry"],
                       ["initializer-entry", "callback-return"], ["initializer-entry", "music-gate"],
                       ["initializer-entry", "initializer-entry"],
                       ["initializer-entry", "table-write-after"],
                       ["initializer-entry", "table-write-before", "callback-gate", "callback-join", "music-gate"]):
            with self.subTest(phases=phases), self.assertRaises(ValueError):
                self.execute(phases)

    def test_changed_final_write_storage_is_rejected(self):
        class ChangedMemory(Memory):
            def read_memory(self, address, size):
                result = super().read_memory(address, size)
                if address == "7000:0217":
                    self.put("4E71:0C43", bytes([3]))
                return result
        with self.assertRaisesRegex(ValueError, "storage pair changed"):
            self.execute(["initializer-entry", "table-write-before", "table-write-after"], ChangedMemory())

    def test_unknown_and_rng_boundaries_do_not_capture(self):
        for site in ("1000:0822", "1000:0811", "1000:0000"):
            memory = Memory()
            stop = stopped("initializer-entry")
            stop["cs_ip"] = site
            session = Session([stop], memory)
            with self.assertRaises((RuntimeError, ValueError)):
                observe(session, 0, {"events": []}, lambda: None)
            self.assertEqual(memory.reads, [])

    def test_stop_cap_and_relocation_rejections(self):
        with self.assertRaisesRegex(RuntimeError, "exceeds600"), contextlib.redirect_stdout(io.StringIO()):
            self.execute(["initializer-entry"] + ["track-return"] * 600)
        for delta in (None, True, -0x1001, 0x10000):
            with self.assertRaises(ValueError):
                address(0x1000, 0, delta)


if __name__ == "__main__":
    unittest.main()
