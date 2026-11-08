"""Synthetic fixed forwarded-P gate/callback controls; no native execution."""
import contextlib
import io
from pathlib import Path
import struct
import sys
from types import SimpleNamespace
import unittest
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parent))
from check_preferences_parent import prepared, stopped, close, Session
from check_preferences_readback import window
from preferences_key import KeyTrace
from preferences_history import ValueHistory
from preferences_readback import observe_readback


def fixture():
    c = prepared()
    c.put("571F:0020", struct.pack("<BHH", 0xEA, 0x139B, 0x6300))
    c.put("56B9:002A", struct.pack("<BHH", 0xEA, 0x0612, 0x6200))
    c.put("6300:1EC4", struct.pack("<BHH", 0x9A, 0x0048, 0x5782))
    c.put("57E0:3002", struct.pack("<HHH", 0x0039, 0x6200, 6))
    c.put("57E0:3012", struct.pack("<H", 0x1970))
    c.put("57E0:301E", struct.pack("<HH", 0x002A, 0x56B9))
    c.put("57E0:0DAB", struct.pack("<H", 1))
    c.put("57E0:426D", bytes(2))
    c.put("57E0:2FEC", struct.pack("<HH", 0x1EC9, 0x6300))
    c.put("57E0:2FEA", struct.pack("<H", 0x3000))
    c.put("57E0:2FE6", struct.pack("<HH", 0x003E, 0x7000))
    return c


def point(site, seq, sp=0x2FF0, bp=0x3000):
    result = stopped(site, seq, sp=sp, bp=bp)
    result["registers"]["ax"] = "0001"
    return result


def prefix(c, trace):
    trace.arm(c)
    for index, (offset, phase) in enumerate(KeyTrace.OFFSETS):
        if phase == "reload": close(c)
        trace.boundary(c, point(f"6300:{offset:04X}", index + 4))
    trace.boundary(c, point("5782:0048", 11, sp=0x2FEC))


class Controls(unittest.TestCase):
    def test_ten_samples_admit_callback_reload_and_nested_return(self):
        c, trace = fixture(), KeyTrace(0); prefix(c, trace)
        trace.nested(c, point("4A6E:011F", 12, sp=0x2FE6, bp=0x2FEA), "getter")
        trace.nested(c, point("7000:0041", 14, sp=0x2FEA, bp=0x2FEA), "store")
        trace.complete()
        self.assertEqual(len(trace.report["events"]), 10)
        self.assertTrue(trace.report["complete"])
        self.assertEqual(trace.report["events"][-1]["preferences_return"], "6300:1EC9")

    def test_wrong_trampoline_callback_target_and_opener_call_refuse(self):
        for address in ("571F:0020", "56B9:0034", "56B9:002A", "6300:1EC4"):
            c = fixture(); c.put(address, bytes(5))
            with self.assertRaises(ValueError): KeyTrace(0).arm(c)

    def test_wrong_forwarded_event_key_callback_and_handler_frame_refuse(self):
        for address, body in (("57E0:3006", bytes(2)), ("57E0:3012", struct.pack("<H", 0x196F)),
                              ("57E0:301E", bytes(4)), ("57E0:3002", bytes(4))):
            c, trace = fixture(), KeyTrace(0); trace.arm(c); c.put(address, body)
            with self.assertRaises(ValueError): trace.boundary(c, point("6300:1A82", 4))

    def test_full_state_return_and_signed_turn_gate_are_checked(self):
        for ax in (0, 2):
            c, trace = fixture(), KeyTrace(0); trace.arm(c)
            trace.boundary(c, point("6300:1A82", 4))
            event = point("6300:1A87", 5); event["registers"]["ax"] = f"{ax:04X}"
            with self.assertRaises(ValueError): trace.boundary(c, event)
        for turn, accepted in ((-1, True), (0, True), (3, True), (4, False), (32767, False)):
            c, trace = fixture(), KeyTrace(0); trace.arm(c)
            trace.boundary(c, point("6300:1A82", 4)); trace.boundary(c, point("6300:1A87", 5))
            c.put("57E0:426D", struct.pack("<h", turn))
            if accepted: trace.boundary(c, point("6300:1A8E", 6))
            else:
                with self.assertRaises(ValueError): trace.boundary(c, point("6300:1A8E", 6))

    def test_callback_key_frame_and_pointer_changes_refuse(self):
        for change in ("key", "sp", "pointer", "callback"):
            c, trace = fixture(), KeyTrace(0); trace.arm(c)
            for index, (off, _) in enumerate(KeyTrace.OFFSETS[:5]): trace.boundary(c, point(f"6300:{off:04X}", index + 4))
            close(c)
            event = point("6300:1CC8", 9)
            if change == "key": c.put("57E0:3012", struct.pack("<H", 0x1950))
            if change == "sp": event["registers"]["sp"] = "2FEE"
            if change == "pointer": c.put("57E0:1431", struct.pack("<HH", 0x100, 0x9000))
            if change == "callback": c.put("57E0:301E", bytes(4))
            with self.assertRaises(ValueError): trace.boundary(c, event)

    def test_wrong_mode_segments_wrap_order_and_incomplete_phase_refuse(self):
        for key, value in (("ds", "57E1"), ("ss", "57E1"), ("bp", "FFF0"), ("system", {"mode": "protected"})):
            c, trace = fixture(), KeyTrace(0); trace.arm(c)
            event = point("6300:1A82", 4); event["registers"][key] = value
            with self.assertRaises(ValueError): trace.boundary(c, event)
        c, trace = fixture(), KeyTrace(0); trace.arm(c)
        with self.assertRaises(ValueError): trace.boundary(c, point("6300:1A87", 4))
        with self.assertRaises(ValueError): trace.complete()

    def test_wrong_saved_bp_outer_return_or_nested_sp_refuse(self):
        for address in ("57E0:2FEA", "57E0:2FEC"):
            c, trace = fixture(), KeyTrace(0); prefix(c, trace); c.put(address, bytes(2))
            with self.assertRaises(ValueError): trace.nested(c, point("4A6E:011F", 12, sp=0x2FE6, bp=0x2FEA), "getter")

    def test_shared_loop_uses_only_tab_then_p_and_rejects_pending_endpoint(self):
        for pending in (False, True):
            c, trace = fixture(), KeyTrace(0)
            history = ValueHistory(0); initial = point("2000:0000", 1); initial["reason"] = "load"; history.arm(c, initial)
            actions = [(point("39D1:097D", 2), None)]
            for index, (off, phase) in enumerate(KeyTrace.OFFSETS):
                actions.append((point(f"6300:{off:04X}", index + 4), close if phase == "reload" else None))
            actions += [(point("5782:0048", 11, sp=0x2FEC), None),
                        (point("4A6E:011F", 12, sp=0x2FE6, bp=0x2FEA), None),
                        (point("7000:003E", 13, sp=0x2FEA, bp=0x2FEA), None)]
            # The shared Readback fixture observes returned AX255.
            for event, _ in actions:
                if event["cs_ip"] in ("4A6E:011F", "7000:003E"): event["registers"]["ax"] = "00FF"
            watched = point("7000:0041", 14, sp=0x2FEA, bp=0x2FEA)
            watched["registers"]["ax"] = "00FF"
            watched["reason"], watched["watch"] = "watchpoint", {"addr": f'{0x57E0 * 16 + 0x26B4:X}', "len": 1, "old": "25", "new": "FF"}
            actions.append((watched, lambda client: client.put("57E0:26B4", bytes([255]))))
            session = Session(c, actions)
            original_observe = session.observe
            def observe(operation, timeout):
                if pending and operation == ("input", 2): return SimpleNamespace(pending=True, state="pending", detail={})
                return original_observe(operation, timeout)
            session.observe = observe
            report = {"inputs": [], "draws": [], "readback": []}
            with patch("preferences_readback.snapshot", return_value=window(10500)), contextlib.redirect_stdout(io.StringIO()):
                if pending:
                    with self.assertRaisesRegex(ValueError, "delivery incomplete"): observe_readback(session, 0, report, lambda: None, history, keyboard=trace)
                else: observe_readback(session, 0, report, lambda: None, history, keyboard=trace)
            self.assertEqual(session.inputs, [("key", {"key": "tab", "hold_ms": 100}), ("key", {"key": "p", "hold_ms": 1})])
            self.assertEqual(trace.report["complete"], not pending)


if __name__ == "__main__": unittest.main()
