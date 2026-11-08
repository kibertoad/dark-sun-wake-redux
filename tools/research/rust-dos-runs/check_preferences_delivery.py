"""Synthetic completed positive P delivery and ready Preferences controls."""
import contextlib
import io
from pathlib import Path
import sys
from types import SimpleNamespace
import unittest
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parent))
from check_preferences_key import fixture, point
from check_preferences_parent import close, Session
from check_preferences_readback import window
from preferences_key import KeyTrace
from preferences_saved import SavedKeyTrace
from preferences_history import ValueHistory
from preferences_readback import observe_readback


def ui(**changes):
    return {**window(16500), "mode": 2, "handler": "5782:0043", **changes}


def timed(seq=15):
    return {**point("2000:0100", seq), "reason": "time", "emu_ns": (seq - 14) * 1000000 + 16}


def prepared():
    c, trace, history = fixture(), SavedKeyTrace(0), ValueHistory(0)
    c.put("4C13:0369", bytes(2)); c.put("4C10:0019", bytes(2))
    start = point("2000:0000", 1); start["reason"] = "load"
    trace.baseline(c, start); history.arm(c, start)
    actions = [(point("39D1:097D", 2), None)]
    for index, (off, phase) in enumerate(KeyTrace.OFFSETS):
        actions.append((point(f"6300:{off:04X}", index + 4), close if phase == "reload" else None))
    actions += [(point("5782:0048", 11, sp=0x2FEC), None),
                (point("4A6E:011F", 12, sp=0x2FE6, bp=0x2FEA), None),
                (point("7000:003E", 13, sp=0x2FEA, bp=0x2FEA), None)]
    for event, _ in actions:
        if event["cs_ip"] in ("4A6E:011F", "7000:003E"): event["registers"]["ax"] = "00FF"
    watched = point("7000:0041", 14, sp=0x2FEA, bp=0x2FEA)
    watched["registers"]["ax"] = "00FF"
    watched["reason"], watched["watch"] = "watchpoint", {"addr": f'{0x57E0 * 16 + 0x26B4:X}', "len": 1, "old": "25", "new": "FF"}
    actions.append((watched, lambda client: client.put("57E0:26B4", bytes([255]))))
    return c, trace, history, actions


class Controls(unittest.TestCase):
    def test_shared_loop_requires_later_delivery_and_ready_window(self):
        c, trace, history, actions = prepared()
        session = Session(c, actions + [(timed(), None), (timed(16), None)])
        original = session.observe
        def observe(operation, timeout):
            if operation == ("input", 2) and session.stop["seq"] < 16:
                return SimpleNamespace(pending=True, state="pending", detail={})
            return original(operation, timeout)
        session.observe = observe
        continuations = []
        def resume(**kwargs): continuations.append(kwargs); return ("continue", 0)
        session.continue_ = resume
        report = {"inputs": [], "draws": [], "readback": []}
        with patch("preferences_readback.snapshot", side_effect=[window(10500), ui(), ui()]), contextlib.redirect_stdout(io.StringIO()):
            observe_readback(session, 0, report, lambda: None, history, keyboard=trace)
        self.assertTrue(report["passed"])
        self.assertEqual(report["result"], "keyboard-preferences-ready")
        self.assertEqual([e["phase"] for e in report["readback"]], ["entry", "return", "stored"])
        self.assertTrue(trace.report["complete"] and history.report["complete"])
        self.assertEqual(len(report["inputs"]), 2)
        self.assertEqual(session.inputs[-1], ("key", {"key": "p", "hold_ms": 1}))
        self.assertEqual(continuations[-2:], [{"until_ms": 1}, {"until_ms": 2}])
        self.assertTrue(all(not x for x in continuations[:-2]))
        self.assertFalse(history.armed)

    def test_endpoint_rejects_incomplete_ui_hook_and_pending(self):
        for current, delivered in ((ui(), False), (ui(mouse_enabled=0), True),
                                   (ui(window={"number": 10500, "flags": 8}), True),
                                   (ui(handler="56B9:0034"), True), (ui(mode=1), True)):
            trace = SavedKeyTrace(0); trace.phase = "delivery"
            self.assertFalse(trace.endpoint(timed(), current, delivered))
            with self.assertRaises(ValueError): trace.complete()

    def test_endpoint_never_reads_expired_frame_and_orders_stops(self):
        trace = SavedKeyTrace(0); trace.phase = "delivery"
        self.assertFalse(trace.endpoint(timed(), ui(), False))
        with self.assertRaises(ValueError): trace.endpoint(timed(), ui(), True)
        self.assertTrue(trace.endpoint(timed(16), ui(), True))

    def test_store_is_required_before_sampling(self):
        trace = SavedKeyTrace(0)
        with self.assertRaises(ValueError): trace.after_store(point("7000:0041", 14))
        with self.assertRaises(ValueError): trace.endpoint(timed(), ui(), True)

    def test_time_before_store_or_repeated_getter_cannot_be_endpoint(self):
        for phase, site, message in (("unarmed", "2000:0100", "declared instruction"),
                                     ("delivery", "4A6E:011F", "repeated getter"),
                                     ("delivery", "1000:0811", "RNG/seed")):
            c, trace, history, _ = prepared(); trace.phase = phase
            event = timed(); event["cs_ip"] = site
            session = Session(c, [(event, None)])
            session.stop = point("2000:0000", 1)
            session.continue_ = lambda **kwargs: ("continue", 0)
            with self.assertRaisesRegex(ValueError, message):
                observe_readback(session, 0, {"inputs": [], "draws": [], "readback": []}, lambda: None, history, keyboard=trace)


if __name__ == "__main__": unittest.main()
