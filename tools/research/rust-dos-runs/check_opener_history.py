"""Synthetic fixed seed/opener history controls; no native game execution."""
import contextlib
import copy
import io
from pathlib import Path
import struct
import sys
import unittest
from unittest.mock import patch
from types import SimpleNamespace

sys.path.insert(0, str(Path(__file__).resolve().parent))
from check_startup_history import Client, Scripted, entry, mutation
from check_disc_startup import stopped
from disc_startup import observe
from opener_history import observe_opener
from preferences_history import StartupHistory


def stop(site, seq):
    return {**stopped("initializer-entry"), "cs_ip": site, "seq": seq, "emu_ns": seq * 10}


def setup():
    client, history = Client(), StartupHistory(0)
    history.arm(client, entry())
    for seq, name, old, new, address, body in (
        (2, "configuration_pointer", 0, 0x60000100, "57E0:3411", struct.pack("<I", 0x60000100)),
        (3, "driver_enable", 1, 0, "57E0:3444", bytes(2)),
    ):
        client.put(address, body)
        history.changed(client, mutation(history, name, old, new, seq))
    client.put("57E0:384A", struct.pack("<I", 1))
    client.put("57E0:1000", struct.pack("<HHH", 0x0663, 0x6200, 1234))
    client.put("56B2:0020", struct.pack("<BHH", 0xEA, 0x0141, 0x6200))
    client.put("6200:065E", struct.pack("<BHH", 0x9A, 0x0811, 0x1000))
    return client, history, {"initial_rng_state": "01000000", "events": [], "complete": False}


def actions():
    return [(stop("1000:0811", 10), None),
            (stop("6200:0663", 11), ("57E0:384A", struct.pack("<I", 1234))),
            (stop("277B:02E4", 12), ("57E0:1000", struct.pack("<H", 1)))]


class Controls(unittest.TestCase):
    def test_exhausted_same_operation_cannot_become_an_opener_result(self):
        client, history, report = setup()
        session = Scripted([], client)
        with patch("opener_history.wait_for_stop", return_value=(SimpleNamespace(stop=stop("1234:0000", 10)), True)) as waiter:
            with self.assertRaisesRegex(RuntimeError, "budget exhausted"):
                observe_opener(session, 0, report, lambda: None, history)
        waiter.assert_called_once_with(session, session.operations[0], attempts=10)
        self.assertEqual(report["opener_events"], [])
        self.assertFalse(history.report["complete"])

    def test_native_sequence_records_caller_seed_and_opener_without_writes(self):
        client, history, report = setup()
        session = Scripted(actions(), client)
        session.client.remove_breakpoints = lambda _: None
        observe_opener(session, 0, report, lambda: None, history)
        self.assertEqual([event["phase"] for event in report["opener_events"]],
                         ["seed-entry", "seed-return", "opener-call"])
        self.assertEqual(report["opener_events"][0]["caller"]["argument"], 1234)
        self.assertEqual(report["opener_events"][-1]["opener_argument_word"], 1)
        self.assertEqual(history.sequence, 12)
        self.assertEqual(len(client.watches), 9)  # Common caller owns cleanup.

    def test_watch_event_between_native_boundaries_remains_in_same_history(self):
        client, history, report = setup()
        script = actions()
        script[-1][0]["seq"] = 14; script[-1][0]["emu_ns"] = 140
        script.insert(2, (mutation(history, "music_level", 0, 55, 12), ("57E0:26B4", bytes([55]))))
        session = Scripted(script, client)
        session.client.remove_breakpoints = lambda _: None
        observe_opener(session, 0, report, lambda: None, history)
        self.assertEqual(history.report["changes"][-1]["primary"], "music_level")
        self.assertEqual(report["opener_events"][-1]["values"]["music_level"], 55)

    def test_wrong_frame_trampoline_call_or_short_read_refuses_native_seed(self):
        for address, body in (("57E0:1000", struct.pack("<HHH", 0x0662, 0x6200, 1234)),
                              ("56B2:0020", bytes(5)), ("6200:065E", bytes(5))):
            client, history, report = setup()
            client.put(address, body)
            with self.assertRaises(ValueError):
                observe_opener(Scripted(actions(), client), 0, report, lambda: None, history)
        client, history, report = setup()
        client.short = True
        with self.assertRaises(ValueError):
            observe_opener(Scripted(actions(), client), 0, report, lambda: None, history)

    def test_wrong_binding_mode_order_or_wrapping_frame_refuses(self):
        for key, value in (("ds", "57E1"), ("ss", "57E1"), ("sp", "FFFC"),
                           ("system", {"mode": "protected"})):
            client, history, report = setup()
            script = actions(); script[0][0]["registers"] = copy.deepcopy(script[0][0]["registers"])
            script[0][0]["registers"][key] = value
            with self.assertRaises(ValueError):
                observe_opener(Scripted(script, client), 0, report, lambda: None, history)
        client, history, report = setup()
        script = actions(); script[1][0]["seq"] = 9
        with self.assertRaises(ValueError):
            observe_opener(Scripted(script, client), 0, report, lambda: None, history)

    def test_wrong_seed_before_during_or_after_call_or_wrong_opener_argument(self):
        for index, address, body in ((0, "57E0:384A", struct.pack("<I", 2)),
                                     (1, "57E0:384A", struct.pack("<I", 7)),
                                     (2, "57E0:384A", struct.pack("<I", 7)),
                                     (2, "57E0:1000", bytes(2))):
            client, history, report = setup()
            script = actions(); script[index] = (script[index][0], (address, body))
            session = Scripted(script, client); session.client.remove_breakpoints = lambda _: None
            with self.assertRaises(ValueError):
                observe_opener(session, 0, report, lambda: None, history)

    def test_generator_second_seed_unknown_stop_and_guard_watch_collisions_refuse(self):
        for index, site, reason in ((0, "1000:0822", "breakpoint"),
                                    (1, "1000:0811", "breakpoint"),
                                    (0, "1234:0000", "breakpoint"),
                                    (0, "1000:0811", "watchpoint"),
                                    (0, "1000:0822", "watchpoint"),
                                    (1, "6200:0663", "watchpoint"),
                                    (2, "277B:02E4", "watchpoint")):
            client, history, report = setup()
            script = actions(); script[index][0]["cs_ip"] = site; script[index][0]["reason"] = reason
            session = Scripted(script, client); session.client.remove_breakpoints = lambda _: None
            with self.assertRaises(ValueError):
                observe_opener(session, 0, report, lambda: None, history)
            self.assertFalse(history.report["complete"])

    def test_missing_endpoint_never_completes(self):
        client, history, report = setup()
        session = Scripted(actions()[:-1], client); session.client.remove_breakpoints = lambda _: None
        with self.assertRaises(StopIteration):
            observe_opener(session, 0, report, lambda: None, history)
        self.assertFalse(history.report["complete"])

    def test_connected_observer_removes_old_stops_and_finishes_only_at_opener(self):
        client, history, report = setup()
        prefix = [(stopped(phase), None) for phase in (
            "initializer-entry", "callback-gate", "callback-join", "music-gate")]
        session = Scripted(prefix + actions(), client)
        removed = []; session.client.remove_breakpoints = removed.append
        with contextlib.redirect_stdout(io.StringIO()):
            observe(session, 0, report, lambda: None, history, through_opener=True)
        self.assertTrue(report["complete"])
        self.assertTrue(history.report["complete"])
        self.assertEqual(len(removed), 15)
        self.assertEqual(client.watches, {})
        self.assertEqual(report["opener_events"][-1]["phase"], "opener-call")


if __name__ == "__main__":
    unittest.main()
