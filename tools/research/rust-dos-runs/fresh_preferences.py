"""One fixed fresh two-character map/Preferences route for Q-CONFIG-014.

Reuses the admitted regeneration gestures and readback contract. No restored
state, guest writes, arbitrary input targets or generic runner interface.
"""
from checkpoint_rng import Recorder
from load_entry import fields, input_map_ready
from preferences_history import StartupHistory, ValueHistory
from preferences_readback import observe_readback, input_poll
from regenerate_checkpoints import checked_read, snapshot, ready
from restore_opener import opener_mapping
from startup_window import wait_for_stop


class FreshHistory(StartupHistory):
    def __init__(self, delta):
        super().__init__(delta)
        self.report["limits"] = "fixed fresh program-entry-to-Preferences-store changed values; no unchanged writes, intra-instruction effects, continuous segment identity or other paths"

    def finish(self, client):
        StartupHistory.finish(self, client)
        ValueHistory.finish(self, client)  # Positive byte plus startup word/pointer controls.


class FixedMapRoute:
    CLICKS = {(160, 93, "left"), (121, 47, "right"), (158, 69, "left"),
              (252, 47, "left"), (121, 107, "right"), (158, 129, "left"),
              (125, 56, "left"), (157, 76, "left")}

    def __init__(self, session, delta, report, save, history):
        self.session, self.delta, self.report, self.save, self.history = session, delta, report, save, history
        self.rng = Recorder(delta)
        self.seed_expected = report["opener_events"][-1]["seed"]
        self.report["menu_stops"], self.report["menu_states"] = [], []
        self.targets = {"opener-return": self.rng.address(0x277B, 0x02E9),
                        "input-builder": input_poll(session.client, delta)}
        self.generator, self.setter = (self.rng.address(0x1000, off) for off in (0x0822, 0x0811))

    def seed(self):
        return int.from_bytes(checked_read(self.session.client, 0x57E0 + self.delta, 0x384A, 4), "little")

    def accept(self, stop):
        self.history._stop(stop)
        self.history.check(self.session.client)
        if self.seed() != self.seed_expected:
            raise ValueError("fresh route seed differs from recorded draw chain")
        self.history.sequence, self.history.time = stop["seq"], stop["emu_ns"]
        self.report["menu_stops"].append({key: stop[key] for key in ("reason", "cs_ip", "seq", "emu_ns")})
        if len(self.report["menu_stops"]) > 4096:
            raise ValueError("fresh route total stop budget exhausted")
        self.report["last_stop"] = stop
        self.save()

    def advance(self, target=None):
        if target is not None and target not in self.targets:
            raise ValueError("unsupported fresh route boundary")
        deadline = self.session.stop["emu_ns"] // 1000000 + 100
        for _ in range(200):
            operation = (self.session.continue_(until=self.targets[target]) if target else
                         self.session.continue_(until_ms=deadline))
            ob, exhausted = wait_for_stop(self.session, operation, attempts=10)
            stop = self.report["last_stop"] = ob.stop
            self.save()
            if exhausted:
                raise ValueError("fresh route observation exhausted")
            site, reason = stop.get("cs_ip"), stop.get("reason")
            if site == self.setter or site == self.generator and reason != "breakpoint":
                raise ValueError("fresh route reached an unadmitted RNG/seed boundary")
            if reason == "watchpoint":
                if target and site == self.targets[target]:
                    raise ValueError("watch collides with a required fresh route boundary")
                self.history.changed(self.session.client, stop)
                if self.seed() != self.seed_expected:
                    raise ValueError("fresh route seed changed outside a recorded draw")
                self.save()
                if not target and stop["emu_ns"] // 1000000 >= deadline:
                    return stop
                continue
            self.accept(stop)
            if reason == "breakpoint" and site == self.generator:
                draw = self.rng.record(self.session, stop)
                if draw["seed_before"] != self.seed_expected:
                    raise ValueError("fresh draw seed does not join previous state")
                self.seed_expected = draw["seed_after"]
                self.report["draws"].append(draw)
                self.accept(self.session.stop)
                if not target and self.session.stop["emu_ns"] // 1000000 >= deadline:
                    return self.session.stop
                continue
            if (target and reason == "breakpoint" and site == self.targets[target]
                    or target is None and reason == "time"):
                return stop
            raise ValueError("fresh route reached an undeclared stop")
        raise ValueError("fresh route continuation exceeds200 stops")

    def state(self):
        current = snapshot(self.session.client, self.delta)
        if not self.report["menu_states"] or current != self.report["menu_states"][-1]:
            self.report["menu_states"].append(current)
            self.save()
        return current

    def reach(self, number):
        if number not in (19500, 11500, 14000, 18501):
            raise ValueError("unsupported fresh route window")
        previous = None
        for _ in range(100):
            self.advance()
            current = self.state()
            if ready(current, number) and current == previous:
                return current
            previous = current
        raise ValueError("fresh route window readiness exhausted")

    def deliver(self, kind, payload, budget, motion=False):
        # Only click()/key() construct inputs; preserve the request even on failure.
        item = {"kind": kind, "request": payload, "delivery": None}
        self.report["inputs"].append(item)
        self.save()
        operation = self.session.send_input(kind, payload)
        for _ in range(budget):
            self.advance()
            delivered = self.session.observe(operation, timeout=0)
            item["delivery"] = {"state": delivered.state, "detail": delivered.detail}
            self.save()
            if delivered.state == "done" and (not motion or self.state()["mouse_enabled"] == 1):
                return
            if not delivered.pending and delivered.state != "done":
                raise ValueError("fresh route input delivery failed")
        raise ValueError("fresh route input delivery exhausted")

    def click(self, x, y, button="left"):
        if (x, y, button) not in self.CLICKS:
            raise ValueError("unsupported fresh route click")
        self.deliver("mouse", {"action": "move", "x": x, "y": y, "coords": "virtual"}, 100, motion=True)
        self.deliver("mouse", {"action": "click", "button": button, "hold_ms": 500}, 10)

    def key(self, key):
        if key not in ("esc", "2"):
            raise ValueError("unsupported fresh route key")
        self.deliver("key", {"key": key, "hold_ms": 100}, 10)

    def play(self):
        call = checked_read(self.session.client, 0x277B + self.delta, 0x02E4, 5)
        if opener_mapping(self.session.stop["cs_ip"], call) != 0x1000 + self.delta:
            raise ValueError("fresh opener call mapping differs")
        self.session.client.remove_breakpoints(self.rng.address(0x277B, 0x02E4))
        self.advance("opener-return")
        self.reach(19500)
        self.click(160, 93)
        self.reach(11500)
        for index, (x, y, mx, my) in enumerate(((121, 47, 158, 69), (121, 107, 158, 129))):
            self.click(x, y, "right")
            self.reach(14000)
            self.click(mx, my)
            self.reach(18501)
            if index == 1:
                self.click(125, 56)
            self.click(252, 47)
            self.reach(11500)
        self.key("esc")
        self.reach(19500)
        self.click(157, 76)
        previous, skipped = None, False
        for _ in range(1000):
            self.advance()
            current = self.state()
            if current["mode"] == 1:
                self.advance("input-builder")
                current = self.state()
                if ready(current, 19501) and current["mode"] == 1 and current["slot_kinds"] == [0, 2, 0, 2]:
                    break
            if not skipped and ready(current, 19501) and current["mode"] == 0 and current == previous:
                self.key("esc")
                skipped = True
            previous = current
        else:
            raise ValueError("fresh map readiness exhausted")
        self.key("2")
        for _ in range(64):
            self.advance("input-builder")
            current = self.state()
            if (ready(current, 19501) and current["mode"] == 1 and current["leader"] == 1
                    and current["slot_kinds"] == [0, 2, 0, 2]
                    and input_map_ready(self.session.client, self.delta, current, fields(self.session.client, self.delta))):
                self.report["fresh_map_ready"] = current
                self.save()
                return
        raise ValueError("fresh leader/input readiness exhausted")


def observe_fresh_preferences(session, delta, report, save, history):
    route = FixedMapRoute(session, delta, report, save, history)
    route.play()
    report["readback"] = []
    observe_readback(session, delta, report, save, history, seed_expected=route.seed_expected)
    if not report.get("passed") or not history.report["complete"]:
        raise ValueError("fresh Preferences endpoint is incomplete")
