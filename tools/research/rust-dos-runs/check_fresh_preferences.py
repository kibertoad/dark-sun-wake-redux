"""Synthetic fresh-route and shared readback controls; no original execution."""
import contextlib
import io
from pathlib import Path
import struct
import sys
from types import SimpleNamespace
import unittest
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parent))
from check_opener_history import setup, stop
from check_startup_history import mutation, Client, entry
from check_preferences_readback import Memory, stop as getter_stop, window
from fresh_preferences import FixedMapRoute, FreshHistory
from preferences_history import ValueHistory
from preferences_readback import observe_readback


class Session:
    def __init__(self, client, actions):
        self.client, self.actions = client, iter(actions)
        self.stop = stop("277B:02E4", 4)
        self.continuations, self.inputs = [], []
        self.client.set_breakpoint = lambda *args, **kwargs: None
        self.client.remove_breakpoints = lambda _: None

    def continue_(self, **kwargs):
        self.continuations.append(kwargs)
        return ("continue", len(self.continuations))

    def observe(self, operation, timeout):
        if operation[0] == "input":
            return SimpleNamespace(state="done", pending=False, detail={"delivered": True})
        self.stop, action = next(self.actions)
        if action:
            self.client.put(*action)
        return SimpleNamespace(stop=self.stop, pending=False)

    def send_input(self, kind, payload):
        self.inputs.append((kind, payload))
        return ("input", len(self.inputs))


def route(actions=()):
    client, history, report = setup()
    report["opener_events"], report["draws"], report["inputs"] = [{"seed": 1}], [], []
    client.put("277B:0541", struct.pack("<BHH", 0x9A, 0x097D, 0x39D1))
    client.put("277B:02E4", struct.pack("<BHH", 0x9A, 0x0020, 0x574B))
    session = Session(client, actions)
    return FixedMapRoute(session, 0, report, lambda: None, history)


def ui(mode=1, leader=1, slots=None):
    return {**window(19501), "mode": mode, "leader": leader,
            "slot_kinds": [0, 2, 0, 2] if slots is None else slots}


class Controls(unittest.TestCase):
    def test_interleaved_watch_keeps_the_same_poll_deadline(self):
        r = route()
        event = mutation(r.history, "music_level", 0, 55, 5)
        timed = stop("2000:0000", 6); timed["reason"] = "time"; timed["emu_ns"] = 100000000
        r.session.actions = iter([(event, ("57E0:26B4", bytes([55]))), (timed, None)])
        r.advance()
        self.assertEqual(r.session.continuations, [{"until_ms": 100}, {"until_ms": 100}])
        self.assertEqual(r.history.report["changes"][-1]["primary"], "music_level")

    def test_draw_chain_is_recorded_and_followed(self):
        r = route([(stop("1000:0822", 5), None)])
        after = stop("2834:0636", 6)
        timed = stop("2000:0000", 7); timed["reason"] = "time"
        r.session.actions = iter([(stop("1000:0822", 5), None), (timed, None)])
        def draw(session, _):
            session.client.put("57E0:384A", struct.pack("<I", 7))
            session.stop = after
            return {"rule": "RULE-SOUND-003", "bound": 3, "result": 1, "seed_before": 1, "seed_after": 7}
        r.rng.record = draw
        r.advance()
        self.assertEqual(r.seed_expected, 7)
        self.assertEqual(r.report["draws"][0]["rule"], "RULE-SOUND-003")

    def test_seed_mismatch_reseed_unknown_stop_and_watch_guard_collisions_fail(self):
        for site, reason in (("1000:0811", "breakpoint"), ("1000:0822", "watchpoint"),
                             ("1234:0000", "breakpoint")):
            event = stop(site, 5); event["reason"] = reason
            r = route([(event, None)])
            with self.assertRaises(ValueError):
                r.advance()
        r = route([(stop("277B:02E9", 5), ("57E0:384A", struct.pack("<I", 2)))])
        with self.assertRaisesRegex(ValueError, "seed"):
            r.advance("opener-return")
        r = route()
        with self.assertRaises(ValueError):
            r.advance("unrestricted-target")

    def test_exhaustion_and_unknown_rng_caller_cannot_continue(self):
        r = route()
        with patch("fresh_preferences.wait_for_stop", return_value=(SimpleNamespace(stop=stop("2000:0000", 5)), True)):
            with self.assertRaisesRegex(ValueError, "exhausted"):
                r.advance()
        r = route([(stop("1000:0822", 5), None)])
        r.rng.record = lambda *_: (_ for _ in ()).throw(RuntimeError("unknown caller"))
        with self.assertRaisesRegex(RuntimeError, "unknown caller"):
            r.advance()
        self.assertEqual(r.report["draws"], [])

    def test_only_declared_gestures_are_admitted_and_requests_survive_failure(self):
        r = route(); delivered = []
        r.deliver = lambda *args, **kwargs: delivered.append((args, kwargs))
        r.click(121, 47, "right"); r.key("2")
        self.assertEqual(delivered[0][0][1], {"action": "move", "x": 121, "y": 47, "coords": "virtual"})
        self.assertEqual(delivered[1][0][1], {"action": "click", "button": "right", "hold_ms": 500})
        for args in ((160, 93, "right"), (0, 0, "left")):
            with self.assertRaises(ValueError): r.click(*args)
        with self.assertRaises(ValueError): r.key("enter")
        r = route(); r.advance = lambda *args: None
        r.session.observe = lambda *_args, **_kwargs: SimpleNamespace(state="pending", pending=True, detail={})
        with self.assertRaisesRegex(ValueError, "delivery exhausted"):
            r.key("esc")
        self.assertEqual(len(r.report["inputs"]), 1)
        self.assertEqual(len(r.session.inputs), 1)

    def test_window_readiness_requires_two_equal_ready_states_and_is_bounded(self):
        r = route(); calls = []; r.advance = lambda *_: calls.append(1)
        states = iter([window(11500, mouse=0), window(11500), window(11500)])
        r.state = lambda: next(states)
        r.reach(11500)
        self.assertEqual(len(calls), 3)
        r.state = lambda: window(19500)
        with self.assertRaisesRegex(ValueError, "readiness exhausted"):
            r.reach(11500)

    def test_fixed_play_route_reaches_only_the_admitted_slot_and_leader_state(self):
        r = route(); gestures = []; reached = []
        r.advance = lambda target=None: None
        r.reach = reached.append
        r.click = lambda *args: gestures.append(("click", args))
        r.key = lambda key: gestures.append(("key", key))
        states = iter([ui(), ui(), ui()])
        r.state = lambda: next(states)
        with patch("fresh_preferences.fields", return_value={}), patch("fresh_preferences.input_map_ready", return_value=True):
            r.play()
        self.assertEqual(reached, [19500, 11500, 14000, 18501, 11500, 14000, 18501, 11500, 19500])
        self.assertIn(("click", (125, 56)), gestures)
        self.assertEqual(gestures[-1], ("key", "2"))
        self.assertEqual(r.report["fresh_map_ready"]["leader"], 1)

    def test_intro_escape_requires_stable_mode_zero_and_is_sent_only_once(self):
        r = route(); gestures = []
        r.advance = lambda *_: None; r.reach = lambda *_: None
        r.click = lambda *_: None; r.key = gestures.append
        states = iter([ui(mode=0), ui(mode=0), ui(mode=0), ui(), ui(), ui()])
        r.state = lambda: next(states)
        with patch("fresh_preferences.fields", return_value={}), patch("fresh_preferences.input_map_ready", return_value=True):
            r.play()
        self.assertEqual(gestures, ["esc", "esc", "2"])

    def test_total_stop_bound_and_unexplained_watch_change_refuse(self):
        r = route(); r.report["menu_stops"] = [{}] * 4096
        with self.assertRaisesRegex(ValueError, "total stop"):
            r.accept(stop("2000:0000", 5))
        r = route(); r.session.client.put("57E0:26B4", bytes([55]))
        with self.assertRaisesRegex(ValueError, "without a recorded watch"):
            r.accept(stop("2000:0000", 5))

    def test_wrong_slots_leader_or_input_binding_never_admit_map(self):
        for bad in (ui(leader=0), ui(slots=[0, 2, 0, 0])):
            r = route(); r.advance = lambda *_: None; r.reach = lambda *_: None
            r.click = lambda *_: None; r.key = lambda *_: None
            initial = iter([ui(), ui()])
            r.state = lambda: next(initial, bad)
            with patch("fresh_preferences.fields", return_value={}), patch("fresh_preferences.input_map_ready", return_value=True):
                with self.assertRaisesRegex(ValueError, "leader/input readiness exhausted"):
                    r.play()
        r = route(); r.advance = lambda *_: None; r.reach = lambda *_: None
        r.click = lambda *_: None; r.key = lambda *_: None; r.state = ui
        with patch("fresh_preferences.fields", return_value={}), patch("fresh_preferences.input_map_ready", return_value=False):
            with self.assertRaisesRegex(ValueError, "leader/input readiness exhausted"): r.play()

    def test_full_history_requires_all_three_positive_widths(self):
        client, history = Client(), FreshHistory(0)
        history.arm(client, entry())
        for seq, name, old, new, address, body in (
            (2, "configuration_pointer", 0, 0x60000100, "57E0:3411", struct.pack("<I", 0x60000100)),
            (3, "driver_enable", 1, 0, "57E0:3444", bytes(2)),
        ):
            client.put(address, body); history.changed(client, mutation(history, name, old, new, seq))
        with self.assertRaisesRegex(ValueError, "music-byte"):
            history.finish(client)
        client.put("57E0:26B4", bytes([55]))
        history.changed(client, mutation(history, "music_level", 0, 55, 4))
        history.finish(client); history.close(client); history.complete()
        self.assertTrue(history.report["complete"])

    def test_shared_preferences_loop_pairs_actual_frame_store_and_cleanup(self):
        client = Memory(); client.set_watchpoint = lambda *_: None; client.remove_watchpoints = lambda *_: None
        history = ValueHistory(0)
        history.arm(client, {**stop("2000:0000", 1), "reason": "load"})
        def boundary(site, seq, sp=0x1000):
            return {**getter_stop(site, sp=sp), "seq": seq, "emu_ns": seq * 10}
        poll = "39D1:097D"
        watched = boundary("7000:0041", 6, sp=0x1004)
        watched["reason"], watched["watch"] = "watchpoint", {"addr": f'{0x57E0 * 16 + 0x26B4:X}', "len": 1, "old": "25", "new": "FF"}
        session = Session(client, [(boundary(poll, 2), None), (boundary(poll, 3), None),
            (boundary("4A6E:011F", 4), None), (boundary("7000:003E", 5, sp=0x1004), None),
            (watched, ("57E0:26B4", bytes([255])))])
        report = {"inputs": [], "draws": [], "readback": []}
        with patch("preferences_readback.snapshot", return_value=window(10500)), contextlib.redirect_stdout(io.StringIO()):
            observe_readback(session, 0, report, lambda: None, history, seed_expected=0)
        self.assertTrue(report["passed"])
        self.assertTrue(history.report["complete"])
        self.assertEqual([item["phase"] for item in report["readback"]], ["entry", "return", "stored"])

    def test_shared_loop_refuses_reseed_and_generator_watch_before_dispatch(self):
        for site in ("1000:0811", "1000:0822"):
            client = Memory(); event = getter_stop(site); event["reason"] = "watchpoint"
            session = Session(client, [(event, None)])
            with self.assertRaisesRegex(ValueError, "RNG/seed"):
                observe_readback(session, 0, {"inputs": [], "draws": []}, lambda: None, object())


if __name__ == "__main__":
    unittest.main()
