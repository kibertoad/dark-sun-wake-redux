"""Synthetic CONFIG-249 parent/event/frame controls; no original execution."""
import contextlib
import io
from pathlib import Path
import struct
import sys
from types import SimpleNamespace
import unittest
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parent))
from check_preferences_readback import Memory, stop as base_stop, window
from preferences_parent import ParentTrace
from preferences_history import ValueHistory
from preferences_readback import observe_readback


def stopped(site, seq, sp=0x1FFC, bp=0x2000):
    item = {**base_stop(site, sp=sp), "seq": seq, "emu_ns": seq * 10}
    item["registers"]["bp"] = f"{bp:04X}"
    return item


def prepared():
    client = Memory()
    client.set_breakpoint = lambda *_args, **_kwargs: None
    client.set_watchpoint = lambda *_: None
    client.remove_watchpoints = lambda *_: None
    client.remove_breakpoints = lambda *_: None
    client.put("56B9:0034", struct.pack("<BHH", 0xEA, 0, 0x6200))
    client.put("6200:0281", struct.pack("<BHH", 0x9A, 0x0048, 0x5782))
    client.put("57E0:1431", struct.pack("<HH", 0x100, 0x9000))
    client.put("4E46:0000", struct.pack("<HH", 0x100, 0x9000))
    body = bytearray(160); body[:4] = b"WIND"
    struct.pack_into("<I", body, 8, 10500); struct.pack_into("<H", body, 0x9E, 8)
    client.put("9000:0100", body)
    client.put("57E0:2002", struct.pack("<HHHH", 0x100, 0x3000, 2, 10303))
    client.put("57E0:1FF8", struct.pack("<HH", 0x0286, 0x6200))
    client.put("57E0:1FF6", struct.pack("<H", 0x2000))
    client.put("57E0:1FF2", struct.pack("<HH", 0x003E, 0x7000))
    return client


def close(client):
    client.put("57E0:1431", bytes(4)); client.put("4E46:0000", bytes(4))


def through_trampoline(client, parent):
    parent.arm(client)
    parent.boundary(client, stopped("6200:025E", 4))
    close(client)
    parent.boundary(client, stopped("6200:0281", 5))
    parent.boundary(client, stopped("5782:0048", 6, sp=0x1FF8))


class Session:
    def __init__(self, client, actions):
        self.client, self.actions = client, iter(actions)
        self.stop = None
        self.inputs = []

    def continue_(self): return ("continue", 0)

    def send_input(self, kind, payload):
        self.inputs.append((kind, payload))
        return ("input", len(self.inputs))

    def observe(self, operation, timeout):
        if operation[0] == "input":
            return SimpleNamespace(pending=False, state="done", detail={})
        self.stop, action = next(self.actions)
        if action: action(self.client)
        return SimpleNamespace(pending=False, stop=self.stop)


class Controls(unittest.TestCase):
    def test_all_five_boundaries_and_no_dereference_after_close(self):
        client, parent = prepared(), ParentTrace(0)
        through_trampoline(client, parent)
        after = len(client.reads)
        parent.nested(client, stopped("4A6E:011F", 7, sp=0x1FF2, bp=0x1FF6), "getter")
        parent.nested(client, stopped("7000:0041", 9, sp=0x1FF6, bp=0x1FF6), "store")
        parent.complete()
        self.assertTrue(parent.report["complete"])
        self.assertEqual([e["phase"] for e in parent.report["events"]], ["arm", "call", "trampoline", "getter", "store"])
        self.assertFalse(any(address.startswith("9000:") for address, _ in client.reads[after:]))
        self.assertEqual(parent.report["events"][-1]["preferences_return"], "6200:0286")

    def test_only_known_handler_jump_and_call_are_admitted(self):
        for address in ("56B9:0034", "6200:0281"):
            client = prepared(); client.put(address, bytes(5))
            with self.assertRaises(ValueError): ParentTrace(0).arm(client)

    def test_wrong_event_control_window_pointers_and_arm_frame_refuse(self):
        for address, body in (("57E0:2006", bytes(2)), ("57E0:2008", struct.pack("<H", 10304)),
                              ("57E0:1431", bytes(4)), ("9000:0100", b"FAIL")):
            client, parent = prepared(), ParentTrace(0); parent.arm(client)
            client.put(address, body)
            with self.assertRaises(ValueError): parent.boundary(client, stopped("6200:025E", 4))
        client, parent = prepared(), ParentTrace(0); parent.arm(client)
        with self.assertRaises(ValueError): parent.boundary(client, stopped("6200:025E", 4, sp=0x1FFA))

    def test_post_child_event_frame_return_and_uncleared_pointer_changes_refuse(self):
        for address, body in (("57E0:2006", bytes(2)), ("57E0:2002", bytes(4)),
                              ("4E46:0000", struct.pack("<HH", 0x100, 0x9000))):
            client, parent = prepared(), ParentTrace(0); parent.arm(client)
            parent.boundary(client, stopped("6200:025E", 4)); close(client); client.put(address, body)
            with self.assertRaises(ValueError): parent.boundary(client, stopped("6200:0281", 5))
        for sp, bp in ((0x1FFA, 0x2000), (0x1FFC, 0x2002)):
            client, parent = prepared(), ParentTrace(0); parent.arm(client)
            parent.boundary(client, stopped("6200:025E", 4)); close(client)
            with self.assertRaises(ValueError): parent.boundary(client, stopped("6200:0281", 5, sp=sp, bp=bp))

    def test_outer_return_saved_parent_and_nested_stack_mismatches_refuse(self):
        for address, body in (("57E0:1FF8", bytes(4)), ("57E0:1FF6", bytes(2))):
            client, parent = prepared(), ParentTrace(0); through_trampoline(client, parent)
            client.put(address, body)
            with self.assertRaises(ValueError): parent.nested(client, stopped("4A6E:011F", 7, sp=0x1FF2, bp=0x1FF6), "getter")
        client, parent = prepared(), ParentTrace(0); through_trampoline(client, parent)
        with self.assertRaises(ValueError): parent.nested(client, stopped("4A6E:011F", 7, sp=0x1FF4, bp=0x1FF6), "getter")

    def test_wrong_mode_segments_order_short_reads_and_wrapping_fields_refuse(self):
        for key, value in (("ds", "57E1"), ("ss", "57E1"), ("bp", "FFFC"), ("system", {"mode": "protected"})):
            client, parent = prepared(), ParentTrace(0); parent.arm(client)
            event = stopped("6200:025E", 4); event["registers"][key] = value
            with self.assertRaises(ValueError): parent.boundary(client, event)
        client, parent = prepared(), ParentTrace(0); parent.arm(client); client.short = True
        with self.assertRaises(ValueError): parent.boundary(client, stopped("6200:025E", 4))
        client, parent = prepared(), ParentTrace(0); parent.arm(client)
        parent.boundary(client, stopped("6200:025E", 4)); close(client)
        with self.assertRaises(ValueError): parent.boundary(client, stopped("6200:0281", 4))

    def test_phase_watch_collision_or_missing_endpoint_cannot_complete(self):
        client, parent = prepared(), ParentTrace(0); parent.arm(client)
        event = stopped("6200:025E", 4); event["reason"] = "watchpoint"
        with self.assertRaises(ValueError): parent.boundary(client, event)
        with self.assertRaises(ValueError): parent.complete()
        with self.assertRaises(ValueError): parent.boundary(client, stopped("6200:0281", 4))
        self.assertFalse(parent.report["complete"])

    def test_shared_observer_retains_parent_route_through_real_paired_store(self):
        client, parent = prepared(), ParentTrace(0)
        history = ValueHistory(0); initial = stopped("2000:0000", 1); initial["reason"] = "load"
        history.arm(client, initial)
        watched = stopped("7000:0041", 9, sp=0x1FF6, bp=0x1FF6)
        watched["reason"], watched["watch"] = "watchpoint", {"addr": f'{0x57E0 * 16 + 0x26B4:X}', "len": 1, "old": "25", "new": "FF"}
        session = Session(client, [(stopped("39D1:097D", 2), None), (stopped("39D1:097D", 3), None),
            (stopped("6200:025E", 4), None), (stopped("6200:0281", 5), close),
            (stopped("5782:0048", 6, sp=0x1FF8), None),
            (stopped("4A6E:011F", 7, sp=0x1FF2, bp=0x1FF6), None),
            (stopped("7000:003E", 8, sp=0x1FF6, bp=0x1FF6), None),
            (watched, lambda c: c.put("57E0:26B4", bytes([255])))])
        report = {"inputs": [], "draws": [], "readback": []}
        with patch("preferences_readback.snapshot", return_value=window(10500)), contextlib.redirect_stdout(io.StringIO()):
            observe_readback(session, 0, report, lambda: None, history, parent=parent)
        self.assertTrue(report["passed"])
        self.assertTrue(parent.report["complete"])
        self.assertTrue(history.report["complete"])


if __name__ == "__main__": unittest.main()
