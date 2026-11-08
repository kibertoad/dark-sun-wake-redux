"""Synthetic fresh-prefix watch controls; no original or emulator execution."""
import contextlib
import io
from pathlib import Path
import struct
import sys
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parent))
from check_disc_startup import Memory, Session, stopped
from disc_startup import observe
from preferences_history import StartupHistory


class Client(Memory):
    def __init__(self):
        super().__init__()
        self.put("57E0:3411", bytes(4))
        self.watches = {}

    def set_watchpoint(self, address, length):
        self.watches[address] = length

    def remove_watchpoints(self, address):
        self.watches.pop(address, None)


def entry():
    return {**stopped("initializer-entry"), "reason": "program_start", "cs_ip": "1000:0000",
            "seq": 1, "emu_ns": 10}


def mutation(history, name, old, new, seq):
    field = history.fields[name]
    return {**stopped("initializer-entry"), "reason": "watchpoint", "cs_ip": "4734:0131",
            "seq": seq, "emu_ns": seq * 10,
            "watch": {"addr": f'{field["physical"]:05X}', "len": field["length"],
                      "old": f"{old:X}", "new": f"{new:X}"}}


class Scripted(Session):
    def __init__(self, actions, client):
        super().__init__([stop for stop, _ in actions], client)
        self.actions = iter(action for _, action in actions)

    def observe(self, operation, timeout):
        result = super().observe(operation, timeout)
        action = next(self.actions)
        if action:
            self.client.put(*action)
        return result


class Controls(unittest.TestCase):
    def test_only_fixed_fields_are_armed_at_actual_program_start(self):
        client, history = Client(), StartupHistory(0)
        history.arm(client, entry())
        self.assertEqual(len(client.watches), 9)
        self.assertEqual(client.watches["57E0:3444"], 2)
        self.assertEqual(client.watches["57E0:3411"], 4)
        self.assertNotIn("card_id", history.fields)
        self.assertFalse(any(address.startswith("6000:") for address, _ in client.reads))
        for reason in ("load", "breakpoint"):
            bad = entry(); bad["reason"] = reason
            with self.assertRaises(ValueError):
                StartupHistory(0).arm(Client(), bad)
        for pointer, enabled in ((0x60000100, 1), (0, 0), (0, 2)):
            other = Client()
            other.put("57E0:3411", struct.pack("<I", pointer))
            other.put("57E0:3444", struct.pack("<H", enabled))
            with self.assertRaises(ValueError):
                StartupHistory(0).arm(other, entry())
            self.assertEqual(other.watches, {})

    def test_publication_does_not_follow_pointer_or_supply_both_controls(self):
        client, history = Client(), StartupHistory(0)
        history.arm(client, entry())
        before = dict(client.watches)
        client.put("57E0:3411", struct.pack("<I", 0x60000100))
        history.changed(client, mutation(history, "configuration_pointer", 0, 0x60000100, 2))
        self.assertEqual(client.watches, before)
        self.assertFalse(any(address.startswith("6000:") for address, _ in client.reads))
        with self.assertRaisesRegex(ValueError, "mutation controls"):
            history.finish(client)
        client.put("57E0:3444", bytes(2))
        history.changed(client, mutation(history, "driver_enable", 1, 0, 3))
        history.finish(client)
        with self.assertRaises(ValueError):
            history.complete()
        history.close(client)
        history.complete()
        self.assertEqual(client.watches, {})
        self.assertTrue(history.report["complete"])

    def test_enable_control_alone_or_wrong_direction_does_not_complete(self):
        for publish, value in ((False, 0), (True, 2)):
            client, history = Client(), StartupHistory(0)
            history.arm(client, entry())
            if publish:
                client.put("57E0:3411", struct.pack("<I", 0x60000100))
                history.changed(client, mutation(history, "configuration_pointer", 0, 0x60000100, 2))
            client.put("57E0:3444", struct.pack("<H", value))
            history.changed(client, mutation(history, "driver_enable", 1, value, 3))
            with self.assertRaises(ValueError):
                history.finish(client)

    def test_watches_and_declared_points_complete_only_at_known_gate(self):
        client, history = Client(), StartupHistory(0)
        history.arm(client, entry())
        actions = [
            (mutation(history, "configuration_pointer", 0, 0x60000100, 2),
             ("57E0:3411", struct.pack("<I", 0x60000100))),
            (stopped("initializer-entry"), None), (stopped("callback-gate"), None),
            (mutation(history, "driver_enable", 1, 0, 3), ("57E0:3444", bytes(2))),
            (stopped("callback-join"), None), (stopped("music-gate"), None),
        ]
        report = {"events": [], "complete": False, "value_history": history.report}
        with contextlib.redirect_stdout(io.StringIO()):
            observe(Scripted(actions, client), 0, report, lambda: None, history)
        self.assertTrue(report["complete"])
        self.assertTrue(history.report["complete"])
        self.assertEqual(len(report["events"]), 4)
        self.assertEqual(len(history.report["changes"]), 2)
        self.assertEqual(client.watches, {})

    def test_watch_at_rng_or_seed_guard_cannot_resume_past_guard(self):
        for target in ("1000:0822", "1000:0811"):
            client, history = Client(), StartupHistory(0)
            history.arm(client, entry())
            event = mutation(history, "configuration_pointer", 0, 0x60000100, 2)
            event["cs_ip"] = target
            with self.assertRaisesRegex(RuntimeError, "RNG/seed"):
                observe(Session([event], client), 0, {"events": []}, lambda: None, history)
            self.assertEqual(history.report["changes"], [])
            self.assertFalse(history.report["complete"])
            history.close(client)

    def test_unknown_watch_or_missing_gate_never_marks_history_complete(self):
        client, history = Client(), StartupHistory(0)
        history.arm(client, entry())
        bad = mutation(history, "configuration_pointer", 0, 0x60000100, 2)
        bad["watch"]["addr"] = "12345"
        with self.assertRaises(ValueError):
            observe(Session([bad], client), 0, {"events": []}, lambda: None, history)
        self.assertFalse(history.report["complete"])
        history.close(client)
        client, history = Client(), StartupHistory(0)
        history.arm(client, entry())
        actions = [
            (mutation(history, "configuration_pointer", 0, 0x60000100, 2),
             ("57E0:3411", struct.pack("<I", 0x60000100))),
            (stopped("initializer-entry"), None), (stopped("callback-gate"), None),
            (mutation(history, "driver_enable", 1, 0, 3), ("57E0:3444", bytes(2))),
            (stopped("callback-join"), None),
        ]
        report = {"events": [], "complete": False}
        with self.assertRaises(StopIteration), contextlib.redirect_stdout(io.StringIO()):
            observe(Scripted(actions, client), 0, report, lambda: None, history)
        self.assertFalse(report["complete"])
        self.assertFalse(history.report["complete"])
        self.assertIsNone(history.report["final"])
        history.close(client)
        self.assertEqual(client.watches, {})


if __name__ == "__main__":
    unittest.main()
