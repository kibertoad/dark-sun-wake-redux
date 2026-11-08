"""Synthetic complete P turn rejection; no original or emulator execution."""
import contextlib
import io
from pathlib import Path
import struct
import sys
from types import SimpleNamespace
import unittest
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parent))
from check_preferences_key import fixture, point
from check_preferences_parent import Session
from check_preferences_readback import window
from preferences_rejection import RejectionTrace, RejectionHistory
from preferences_readback import observe_readback


def prepared():
    c = fixture()
    c.put("6300:1A8E", bytes((0x83, 0x3E)) + struct.pack("<HB", 0x426D, 4))
    c.put("57E0:426D", struct.pack("<h", 9999))
    c.put("57E0:3000", struct.pack("<H", 0x3100))
    return c


def baseline():
    result = point("2000:0000", 1); result["reason"] = "load"
    return result


def prefix(c, trace):
    trace.baseline(c, baseline()); trace.arm(c)
    for seq, off in ((4, 0x1A82), (5, 0x1A87), (6, 0x1A8E), (7, 0x1EC9)):
        trace.boundary(c, point(f"6300:{off:04X}", seq))


class Controls(unittest.TestCase):
    def test_rejecting_branch_epilogue_and_native_menu_return_complete(self):
        c, trace = prepared(), RejectionTrace(0); prefix(c, trace)
        after = len(c.reads)
        trace.boundary(c, point("6200:0039", 8, sp=0x3006, bp=0x3100))
        self.assertFalse(any(address.startswith("57E0:30") for address, _ in c.reads[after:]))
        self.assertTrue(trace.endpoint(c, point("39D1:097D", 9), window(10500), True))
        trace.complete()
        self.assertEqual([e["phase"] for e in trace.report["events"]], ["state-call", "state-return", "turn", "reject-epilogue", "menu-return"])
        self.assertTrue(trace.report["complete"])
        self.assertEqual(trace.report["baseline"]["turn"], 9999)

    def test_baseline_and_known_comparison_required_before_arming(self):
        c, trace = prepared(), RejectionTrace(0)
        with self.assertRaises(ValueError): trace.arm(c)
        trace.baseline(c, baseline()); c.put("6300:1A8E", bytes(5))
        with self.assertRaises(ValueError): trace.arm(c)

    def test_turn_below_four_and_any_positive_path_guard_refuse(self):
        c, trace = prepared(), RejectionTrace(0); trace.baseline(c, baseline()); trace.arm(c)
        trace.boundary(c, point("6300:1A82", 4)); trace.boundary(c, point("6300:1A87", 5))
        c.put("57E0:426D", struct.pack("<h", 3))
        with self.assertRaises(ValueError): trace.boundary(c, point("6300:1A8E", 6))
        for site in ("6300:1CBE", "6300:1CC5", "6300:1EC4", "5782:0048", "56B9:002A", "6200:0612"):
            c, trace = prepared(), RejectionTrace(0); trace.baseline(c, baseline()); trace.arm(c)
            with self.assertRaises(ValueError): trace.boundary(c, point(site, 4))

    def test_epilogue_and_return_frame_state_changes_refuse(self):
        for address, body in (("57E0:3000", bytes(2)), ("57E0:426D", struct.pack("<h", 4)), ("57E0:1431", bytes(4))):
            c, trace = prepared(), RejectionTrace(0); trace.baseline(c, baseline()); trace.arm(c)
            for seq, off in ((4, 0x1A82), (5, 0x1A87), (6, 0x1A8E)): trace.boundary(c, point(f"6300:{off:04X}", seq))
            c.put(address, body)
            with self.assertRaises(ValueError): trace.boundary(c, point("6300:1EC9", 7))
        for sp, bp in ((0x3004, 0x3100), (0x3006, 0x3102)):
            c, trace = prepared(), RejectionTrace(0); prefix(c, trace)
            with self.assertRaises(ValueError): trace.boundary(c, point("6200:0039", 8, sp=sp, bp=bp))

    def test_delivery_and_same_ready_menu_are_required_for_endpoint(self):
        c, trace = prepared(), RejectionTrace(0); prefix(c, trace)
        trace.boundary(c, point("6200:0039", 8, sp=0x3006, bp=0x3100))
        self.assertFalse(trace.endpoint(c, point("39D1:097D", 9), window(10500), False))
        self.assertFalse(trace.endpoint(c, point("39D1:097D", 9), window(10500, mouse=0), True))
        with self.assertRaises(ValueError): trace.endpoint(c, point("39D1:097D", 9), window(19501), True)
        with self.assertRaises(ValueError): trace.complete()

    def test_no_change_history_has_no_positive_mutation_claim_and_needs_cleanup(self):
        c, history = prepared(), RejectionHistory(0); history.arm(c, baseline())
        history.finish(c)
        with self.assertRaises(ValueError): history.complete()
        history.close(c); history.complete()
        self.assertTrue(history.report["complete"])
        self.assertFalse(history.report["positive_mutation_control"])
        self.assertEqual(history.report["initial"], history.report["final"])
        c, history = prepared(), RejectionHistory(0); history.arm(c, baseline())
        field = history.fields["music_level"]; c.put("57E0:26B4", bytes([55]))
        stop = point("7000:0041", 4)
        stop["reason"], stop["watch"] = "watchpoint", {"addr": f'{field["physical"]:X}', "len": 1, "old": "25", "new": "37"}
        with self.assertRaises(ValueError): history.changed(c, stop)

    def test_shared_observer_finishes_rejection_after_menu_return_and_p_delivery(self):
        c, trace, history = prepared(), RejectionTrace(0), RejectionHistory(0)
        history.arm(c, baseline()); trace.baseline(c, baseline())
        actions = [(point("39D1:097D", 2), None), (point("6300:1A82", 4), None),
            (point("6300:1A87", 5), None), (point("6300:1A8E", 6), None),
            (point("6300:1EC9", 7), None), (point("6200:0039", 8, sp=0x3006, bp=0x3100), None),
            ({**point("2000:0100", 9, sp=0x4000, bp=0x4100), "reason": "time", "emu_ns": 1000016}, None)]
        session = Session(c, actions)
        continuations = []
        def resume(**kwargs):
            continuations.append(kwargs)
            return ("continue", 0)
        session.continue_ = resume
        report = {"inputs": [], "draws": [], "readback": []}
        with patch("preferences_readback.snapshot", return_value=window(10500)), contextlib.redirect_stdout(io.StringIO()):
            observe_readback(session, 0, report, lambda: None, history, keyboard=trace)
        self.assertTrue(report["passed"])
        self.assertEqual(report["result"], "keyboard-turn-rejected")
        self.assertEqual(report["readback"], [])
        self.assertEqual(len(report["inputs"]), 2)
        self.assertTrue(trace.report["complete"])
        self.assertTrue(history.report["complete"])
        self.assertEqual(continuations[-1], {"until_ms": 1})
        self.assertTrue(all(not item for item in continuations[:-1]))

    def test_time_stops_cannot_hide_getter_or_precede_paired_return(self):
        for phase, site, message in (("delivery", "4A6E:011F", "forbidden getter"),
                                      ("unarmed", "2000:0100", "declared instruction")):
            c, trace, history = prepared(), RejectionTrace(0), RejectionHistory(0)
            history.arm(c, baseline()); trace.phase = phase
            timed = {**point(site, 2), "reason": "time", "emu_ns": 1000016}
            session = Session(c, [(timed, None)])
            session.stop = baseline()
            session.continue_ = lambda **kwargs: ("continue", 0)
            with self.assertRaisesRegex(ValueError, message):
                observe_readback(session, 0, {"inputs": [], "draws": [], "readback": []}, lambda: None, history, keyboard=trace)


if __name__ == "__main__": unittest.main()
