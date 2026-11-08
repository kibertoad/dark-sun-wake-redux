"""Synthetic fixed-history controls; no game execution or original bytes."""
import copy
from pathlib import Path
import struct
import sys
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parent))
from check_preferences_readback import Memory, stop
from preferences_history import ValueHistory
from preferences_readback import Readback


class Client(Memory):
    def __init__(self):
        super().__init__()
        self.watches, self.removed, self.fail_arm = {}, [], None
        self.put("57E0:3440", struct.pack("<H", 0xFFFF))

    def set_watchpoint(self, address, length):
        self.watches[address] = length
        if len(self.watches) == self.fail_arm:
            raise RuntimeError("invented arm failure")

    def remove_watchpoints(self, address):
        self.removed.append(address)
        self.watches.pop(address, None)


def restored():
    return {**stop(), "reason": "load", "seq": 1, "emu_ns": 100}


def changed(history, name="music_level", old=37, new=255, seq=2):
    field = history.fields[name]
    return {**stop("7000:0041", 0x1004), "reason": "watchpoint", "seq": seq, "emu_ns": 101,
            "watch": {"addr": f'{field["physical"]:05X}', "len": field["length"],
                      "old": f"{old:X}", "new": f"{new:X}"}}


class Controls(unittest.TestCase):
    def test_fixed_spans_and_completion_need_positive_change_and_cleanup(self):
        client, history = Client(), ValueHistory(0)
        history.arm(client, restored())
        self.assertEqual(len(client.watches), 9)
        self.assertEqual(client.watches["57E0:3411"], 4)
        self.assertEqual(client.watches["57E0:3440"], 2)
        self.assertEqual(client.watches["6000:0108"], 2)
        self.assertEqual(history.report["initial"]["getter_handle"], 0xFFFF)
        with self.assertRaises(ValueError):
            history.finish(client)
        client.put("57E0:26B4", b"\xff")
        history.changed(client, changed(history))
        history.finish(client)
        with self.assertRaises(ValueError):
            history.complete()
        history.close(client)
        history.complete()
        self.assertTrue(history.report["complete"])
        self.assertEqual(client.watches, {})
        self.assertEqual(len(client.removed), 9)
        history.close(client)
        self.assertEqual(len(client.removed), 9)

    def test_missing_change_and_bad_watch_metadata_reject(self):
        for alteration in ("unknown", "width", "old", "new", "same", "malformed", "reason", "mode", "sequence", "time"):
            client, history = Client(), ValueHistory(0)
            history.arm(client, restored())
            client.put("57E0:26B4", b"\xff")
            event = changed(history)
            if alteration == "unknown": event["watch"]["addr"] = "12345"
            if alteration == "width": event["watch"]["len"] = 2
            if alteration == "old": event["watch"]["old"] = "24"
            if alteration == "new": event["watch"]["new"] = "FE"
            if alteration == "same": event["watch"]["old"] = "FF"
            if alteration == "malformed": event["watch"]["new"] = "unknown"
            if alteration == "reason": event["reason"] = "breakpoint"
            if alteration == "mode": event["registers"]["system"]["mode"] = "protected"
            if alteration == "sequence": event["seq"] = 1
            if alteration == "time": event["emu_ns"] = 99
            with self.subTest(alteration=alteration), self.assertRaises(ValueError):
                history.changed(client, event)
            self.assertFalse(history.report["complete"])
        with self.assertRaisesRegex(ValueError, "without a recorded"):
            history.check(client)

    def test_all_changed_fields_at_one_stop_are_retained(self):
        client, history = Client(), ValueHistory(0)
        history.arm(client, restored())
        client.put("57E0:26B4", b"\xff")
        client.put("57E0:3440", struct.pack("<H", 7))
        event = changed(history)
        event["registers"]["ds"] = "0000"  # A different actor can write the fixed globals.
        history.changed(client, event)
        self.assertEqual(set(history.report["changes"][0]["values"]), {"music_level", "source_handle"})
        self.assertEqual(history.previous["source_handle"], 7)

    def test_word_changes_that_return_to_initial_value_remain_in_history(self):
        client, history = Client(), ValueHistory(0)
        history.arm(client, restored())
        client.put("57E0:3440", struct.pack("<H", 0xABCD))
        history.changed(client, changed(history, "source_handle", 0xFFFF, 0xABCD))
        client.put("57E0:3440", struct.pack("<H", 0xFFFF))
        history.changed(client, changed(history, "source_handle", 0xABCD, 0xFFFF, seq=3))
        self.assertEqual(history.previous["source_handle"], history.report["initial"]["source_handle"])
        self.assertEqual([e["values"]["source_handle"]["new"] for e in history.report["changes"]], [0xABCD, 0xFFFF])
        with self.assertRaisesRegex(ValueError, "ordered"):
            history.changed(client, changed(history, "source_handle", 0xFFFF, 0xABCD, seq=3))

    def test_relocated_fixed_fields_and_cleanup_do_not_select_other_watches(self):
        client, delta = Client(), -0xA46
        for _, segment, offset, length in ValueHistory.FIELDS:
            value = client.read_memory(f"{segment:04X}:{offset:04X}", length).data
            client.put(f"{segment + delta:04X}:{offset:04X}", value)
        client.watches["7000:0020"] = 4
        history = ValueHistory(delta)
        history.arm(client, restored())
        self.assertEqual(history.fields["getter_handle"]["physical"], (0x57E0 + delta) * 16 + 0x3464)
        self.assertEqual(history.fields["card_id"]["segment"], 0x6000)
        with self.assertRaises(ValueError):
            history.arm(client, restored())
        history.close(client)
        self.assertEqual(client.watches, {"7000:0020": 4})

    def test_pointer_mutation_is_recorded_but_never_followed(self):
        client, history = Client(), ValueHistory(0)
        history.arm(client, restored())
        old = history.previous["configuration_pointer"]
        client.put("57E0:3411", struct.pack("<I", 0x61000200))
        event = changed(history, "configuration_pointer", old, 0x61000200)
        with self.assertRaisesRegex(ValueError, "no target following"):
            history.changed(client, event)
        self.assertEqual(history.fields["card_id"]["segment"], 0x6000)
        self.assertEqual(len(history.report["changes"]), 1)
        self.assertFalse(history.report["complete"])
        self.assertNotIn("6100:0208", client.watches)

    def test_partial_arm_failure_still_has_bounded_cleanup(self):
        client, history = Client(), ValueHistory(0)
        client.fail_arm = 3
        with self.assertRaises(RuntimeError):
            history.arm(client, restored())
        history.close(client)
        self.assertEqual(client.watches, {})
        self.assertEqual(len(client.removed), 3)
        self.assertFalse(history.report["complete"])

    def test_unsupported_initial_bindings_fail_before_arming(self):
        for pointer in ((0, 0), (0xFFF0, 0x6000), (0x10, 0xFFFF), (0x13EF, 0x57E0)):
            client = Client()
            client.put("57E0:3411", struct.pack("<HH", *pointer))
            with self.subTest(pointer=pointer), self.assertRaises(ValueError):
                ValueHistory(0).arm(client, restored())
            self.assertEqual(client.watches, {})
        for delta in (True, None, 1, -0x1001):
            with self.assertRaises(ValueError):
                ValueHistory(delta)

    def test_only_exact_paired_music_store_watch_is_an_endpoint(self):
        client, trace = Client(), Readback(0)
        history = ValueHistory(0)
        history.arm(client, restored())
        trace.entry(client, stop())
        trace.returned(client, stop("7000:003E", 0x1004))
        client.put("57E0:26B4", b"\xff")
        event = changed(history)
        for case in ("address", "width", "boundary", "frame", "new"):
            bad = copy.deepcopy(event)
            if case == "address": bad["watch"]["addr"] = "12345"
            if case == "width": bad["watch"]["len"] = 2
            if case == "boundary": bad["cs_ip"] = "7000:0042"
            if case == "frame": bad["registers"]["sp"] = "1006"
            if case == "new": bad["watch"]["new"] = "FE"
            with self.subTest(case=case), self.assertRaises(ValueError):
                trace.stored(client, bad)
        history.changed(client, event)
        self.assertEqual(trace.stored(client, event)["music_level"], 255)
        self.assertEqual(event["reason"], "watchpoint")
        self.assertEqual(trace.phase, "done")
        history.finish(client)
        history.close(client)
        history.complete()


if __name__ == "__main__":
    unittest.main()
