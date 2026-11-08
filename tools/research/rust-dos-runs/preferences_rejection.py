"""Fixed native forwarded-P turn rejection; no guest writes or forced state."""
import struct

from preferences_history import ValueHistory
from preferences_key import KeyTrace
from regenerate_checkpoints import checked_read, ready


class RejectionHistory(ValueHistory):
    def __init__(self, delta):
        super().__init__(delta)
        self.report["positive_mutation_control"] = False
        self.report["limits"] = "fixed rejected-P no-observed-change interval; no positive mutation delivery, unchanged writes, intra-instruction effects or other paths"

    def changed(self, client, stop):
        super().changed(client, stop)
        raise ValueError("rejected route changed a watched value")

    def finish(self, client):
        self.check(client)
        if self.report["changes"]:
            raise ValueError("rejected route history is not unchanged")
        self.report["final"] = dict(self.previous)


class RejectionTrace(KeyTrace):
    EXPECT_REJECTION = True

    def __init__(self, delta):
        super().__init__(delta)
        self.report["baseline"], self.report["endpoint"] = None, None
        self.report["limits"] = "fixed completed P turn-rejection samples only; no positive callback/getter/store, other roots or continuous frame/state history"

    def state(self, client):
        ds = 0x57E0 + self.delta
        return {"mode": int.from_bytes(checked_read(client, ds, 0x0DAB, 2), "little"),
                "turn": int.from_bytes(checked_read(client, ds, 0x426D, 2), "little", signed=True)}

    def pointers(self, client):
        return [f"{seg:04X}:{off:04X}" for off, seg in (
            struct.unpack("<HH", checked_read(client, s, o, 4))
            for s, o in ((0x57E0 + self.delta, 0x1431), (0x4E46 + self.delta, 0)))]

    def baseline(self, client, stop):
        if self.phase != "unarmed" or self.report["baseline"] is not None or stop.get("reason") != "load":
            raise ValueError("rejection baseline needs the strict restored entry")
        if stop.get("registers", {}).get("system", {}).get("mode") != "real":
            raise ValueError("rejection baseline requires real mode")
        self.report["baseline"] = {**self.state(client), **{k: stop[k] for k in ("cs_ip", "seq", "emu_ns")}}

    def arm(self, client):
        if self.report["baseline"] is None:
            raise ValueError("rejection lacks restored baseline")
        super().arm(client)
        comparison = bytes((0x83, 0x3E)) + struct.pack("<HB", 0x426D, 4)
        if checked_read(client, self.segment, 0x1A8E, 5) != comparison:
            raise ValueError("rejection comparison operand differs")
        extras = {f"{self.segment:04X}:1EC9": "reject-epilogue",
                  f"{self.menu:04X}:0039": "menu-return",
                  self.address(0x56B9, 0x002A): "forbidden-close",
                  f"{self.menu:04X}:0612": "forbidden-close"}
        self.points.update(extras)
        for site in extras: client.set_breakpoint(site, once=True)

    def boundary(self, client, stop):
        phase = self.points[stop["cs_ip"]]
        if phase not in ("turn", "reject-epilogue", "menu-return"):
            super().boundary(client, stop)
            return
        if phase != self.phase or stop.get("reason") != "breakpoint":
            raise ValueError("rejection phase/reason differs")
        if phase == "menu-return":
            self.returned(client, stop)
            return
        item, _ = self.sample(client, stop, phase)
        item["accepted"] = False
        self.report["events"].append(item)
        if item["bp"] != self.initial["bp"] or item["sp"] != self.initial["sp"] or item["menu_pointers"] != self.initial["menu_pointers"]:
            raise ValueError("rejection keyboard frame/menu differs")
        saved = int.from_bytes(checked_read(client, item["ss"], item["bp"], 2), "little")
        item["saved_menu_bp"] = saved
        if phase == "turn":
            if item["turn"] < 4:
                raise ValueError("rejection requires a rejecting signed turn")
            self.rejected = dict(item)
            self.phase = "reject-epilogue"
        else:
            if any(item[k] != self.rejected[k] for k in ("mode", "turn", "saved_menu_bp")):
                raise ValueError("rejection epilogue state/frame changed")
            self.phase = "menu-return"
        item["accepted"] = True

    def returned(self, client, stop):
        r = stop.get("registers", {})
        if (r.get("system", {}).get("mode") != "real"
                or type(stop.get("seq")) is not int or stop["seq"] <= self.sequence
                or type(stop.get("emu_ns")) is not int or stop["emu_ns"] < self.time):
            raise ValueError("rejection return needs ordered real mode")
        try: ds, ss, sp, bp = (int(r[k], 16) for k in ("ds", "ss", "sp", "bp"))
        except (KeyError, ValueError, TypeError) as error: raise ValueError("rejection return registers missing") from error
        if (ds != 0x57E0 + self.delta or ss != self.initial["ss"]
                or sp != self.initial["bp"] + 6 or bp != self.rejected["saved_menu_bp"]):
            raise ValueError("rejection menu return frame differs")
        # The keyboard frame has expired. Read only live registers and named globals.
        state = self.state(client)
        pointers = self.pointers(client)
        if pointers != self.initial["menu_pointers"] or any(state[k] != self.rejected[k] for k in ("mode", "turn")):
            raise ValueError("rejection returned menu/state differs")
        self.report["events"].append({"phase": "menu-return", "accepted": True,
            **{k: stop[k] for k in ("cs_ip", "reason", "seq", "emu_ns")},
            "ss": ss, "sp": sp, "bp": bp, **state, "menu_pointers": pointers})
        self.sequence, self.time, self.phase = stop["seq"], stop["emu_ns"], "delivery"

    def endpoint(self, client, stop, current, delivered):
        if self.phase != "delivery": raise ValueError("rejection input poll before menu return")
        if (current.get("window") or {}).get("number") != 10500:
            raise ValueError("rejection input poll lost Game Menu")
        if not delivered or not ready(current, 10500): return False
        state = self.state(client)
        if self.pointers(client) != self.initial["menu_pointers"]:
            raise ValueError("rejection endpoint menu changed")
        self.report["endpoint"] = {**{k: stop[k] for k in ("cs_ip", "seq", "emu_ns")},
                                   **state, "ui_state": current, "input_delivery": "done"}
        self.phase = "done"
        return True

    def complete(self):
        if self.phase != "done" or len(self.report["events"]) != 5 or self.report["endpoint"] is None:
            raise ValueError("rejection endpoint incomplete")
        self.report["complete"] = True
