"""Synthetic direct window-dispatch admission; no original or emulator."""
import contextlib
import io
from pathlib import Path
import struct
import sys
import unittest
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parent))
from check_preferences_parent import prepared, stopped, close, Session
from check_preferences_readback import window
from preferences_parent import DispatchTrace
from preferences_history import ValueHistory
from preferences_readback import observe_readback


def fixture():
    client = prepared()
    client.put("39D1:0802", bytes((0x26, 0xFF, 0x9F, 0xF5, 0)))
    client.put("9000:01F5", struct.pack("<HH", 0x0034, 0x56B9))
    client.put("57E0:2002", struct.pack("<HH", 0x0807, 0x39D1))
    return client


def dispatch(seq=3):
    stop = stopped("39D1:0802", seq, sp=0x2006, bp=0x3000)
    stop["registers"].update(es="9000", bx="0100")
    return stop


class Controls(unittest.TestCase):
    def test_actual_copied_event_target_window_and_handler_frame_join(self):
        client, trace = fixture(), DispatchTrace(0); trace.arm(client)
        trace.boundary(client, dispatch())
        trace.boundary(client, stopped("6200:025E", 4)); close(client)
        trace.boundary(client, stopped("6200:0281", 5))
        trace.boundary(client, stopped("5782:0048", 6, sp=0x1FF8))
        trace.nested(client, stopped("4A6E:011F", 7, sp=0x1FF2, bp=0x1FF6), "getter")
        trace.nested(client, stopped("7000:0041", 9, sp=0x1FF6, bp=0x1FF6), "store")
        trace.complete()
        self.assertTrue(trace.report["complete"])
        self.assertEqual(trace.report["events"][0]["handler"], "56B9:0034")
        self.assertEqual(trace.report["events"][1]["handler_return"], "39D1:0807")

    def test_opcode_event_target_window_identity_and_flags_refuse(self):
        for address, body in (("57E0:2006", bytes(2)), ("57E0:2008", struct.pack("<H", 10304)),
                              ("9000:01F5", bytes(4)), ("9000:0100", b"FAIL"),
                              ("9000:019E", struct.pack("<H", 4)), ("4E46:0000", bytes(4))):
            client, trace = fixture(), DispatchTrace(0); trace.arm(client); client.put(address, body)
            with self.assertRaises(ValueError): trace.boundary(client, dispatch())
        client = fixture(); client.put("39D1:0802", bytes(5))
        with self.assertRaises(ValueError): DispatchTrace(0).arm(client)

    def test_real_mode_register_bounds_short_reads_and_phase_refuse(self):
        for key, value in (("ds", "57E1"), ("ss", "57E1"), ("sp", "FFFE"),
                           ("bx", "FFFF"), ("system", {"mode": "protected"})):
            client, trace = fixture(), DispatchTrace(0); trace.arm(client)
            event = dispatch(); event["registers"][key] = value
            with self.assertRaises(ValueError): trace.boundary(client, event)
        client, trace = fixture(), DispatchTrace(0); trace.arm(client); client.short = True
        with self.assertRaises(ValueError): trace.boundary(client, dispatch())
        client, trace = fixture(), DispatchTrace(0); trace.arm(client)
        event = dispatch(); event["reason"] = "watchpoint"
        with self.assertRaises(ValueError): trace.boundary(client, event)
        with self.assertRaises(ValueError): trace.boundary(client, stopped("6200:025E", 4))

    def test_menu_frame_must_join_actual_pre_call_sp_and_return(self):
        for sp, bp in ((0x1FFA, 0x2000), (0x1FFC, 0x2002)):
            client, trace = fixture(), DispatchTrace(0); trace.arm(client); trace.boundary(client, dispatch())
            with self.assertRaises(ValueError): trace.boundary(client, stopped("6200:025E", 4, sp=sp, bp=bp))
        client, trace = fixture(), DispatchTrace(0); trace.arm(client); trace.boundary(client, dispatch())
        client.put("57E0:2002", struct.pack("<HH", 0x0808, 0x39D1))
        with self.assertRaises(ValueError): trace.boundary(client, stopped("6200:025E", 4))
        with self.assertRaises(ValueError): trace.complete()

    def test_shared_observer_requires_all_six_points_before_completion(self):
        client, trace = fixture(), DispatchTrace(0)
        history = ValueHistory(0); initial = stopped("2000:0000", 1); initial["reason"] = "load"
        history.arm(client, initial)
        watched = stopped("7000:0041", 10, sp=0x1FF6, bp=0x1FF6)
        watched["reason"], watched["watch"] = "watchpoint", {"addr": f'{0x57E0 * 16 + 0x26B4:X}', "len": 1, "old": "25", "new": "FF"}
        session = Session(client, [(stopped("39D1:097D", 2), None), (stopped("39D1:097D", 3), None),
            (dispatch(4), None), (stopped("6200:025E", 5), None), (stopped("6200:0281", 6), close),
            (stopped("5782:0048", 7, sp=0x1FF8), None),
            (stopped("4A6E:011F", 8, sp=0x1FF2, bp=0x1FF6), None),
            (stopped("7000:003E", 9, sp=0x1FF6, bp=0x1FF6), None),
            (watched, lambda c: c.put("57E0:26B4", bytes([255])))])
        report = {"inputs": [], "draws": [], "readback": []}
        with patch("preferences_readback.snapshot", return_value=window(10500)), contextlib.redirect_stdout(io.StringIO()):
            observe_readback(session, 0, report, lambda: None, history, parent=trace)
        self.assertTrue(report["passed"])
        self.assertTrue(trace.report["complete"])
        self.assertEqual(len(trace.report["events"]), 6)


if __name__ == "__main__": unittest.main()
