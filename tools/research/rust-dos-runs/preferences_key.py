"""Fixed forwarded P-key gate/callback samples for Q-CONFIG-014."""
import struct

from regenerate_checkpoints import checked_read


class KeyTrace:
    OFFSETS = ((0x1A82, "state-call"), (0x1A87, "state-return"),
               (0x1A8E, "turn"), (0x1CBE, "close-test"),
               (0x1CC5, "close-call"), (0x1CC8, "reload"), (0x1EC4, "opener-call"))

    def __init__(self, delta):
        if type(delta) is not int or not -0x1000 <= delta <= 0:
            raise ValueError("unsupported keyboard relocation")
        self.delta, self.phase, self.initial = delta, "unarmed", None
        self.sequence, self.time, self.root_sp = -1, -1, None
        self.points = {}
        self.report = {"events": [], "complete": False,
                       "limits": "fixed forwarded P-key samples only; no other callers/keys, continuous frame or target histories, later Preferences or reentrant-entry proof"}

    def address(self, segment, offset): return f"{segment + self.delta:04X}:{offset:04X}"

    def arm(self, client):
        if self.phase != "unarmed": raise ValueError("keyboard observer must arm once")
        opcode, off, self.segment = struct.unpack("<BHH", checked_read(client, 0x571F + self.delta, 0x0020, 5))
        if opcode != 0xEA or off != 0x139B or self.segment == 0:
            raise ValueError("keyboard trampoline differs")
        opcode, off, self.menu = struct.unpack("<BHH", checked_read(client, 0x56B9 + self.delta, 0x0034, 5))
        close = struct.unpack("<BHH", checked_read(client, 0x56B9 + self.delta, 0x002A, 5))
        if opcode != 0xEA or off != 0 or self.menu == 0 or close != (0xEA, 0x0612, self.menu):
            raise ValueError("keyboard menu close binding differs")
        if checked_read(client, self.segment, 0x1EC4, 5) != struct.pack("<BHH", 0x9A, 0x0048, 0x5782 + self.delta):
            raise ValueError("keyboard P opener call differs")
        self.points = {f"{self.segment:04X}:{o:04X}": phase for o, phase in self.OFFSETS}
        self.points[self.address(0x5782, 0x0048)] = "trampoline"
        for site in self.points: client.set_breakpoint(site, once=True)
        self.phase = "state-call"

    def is_site(self, stop): return stop.get("cs_ip") in self.points

    def sample(self, client, stop, phase):
        r = stop.get("registers", {})
        if (r.get("system", {}).get("mode") != "real"
                or type(stop.get("seq")) is not int or stop["seq"] <= self.sequence
                or type(stop.get("emu_ns")) is not int or stop["emu_ns"] < self.time):
            raise ValueError("keyboard requires ordered real-mode stop")
        try: ds, ss, sp, bp, ax = (int(r[k], 16) for k in ("ds", "ss", "sp", "bp", "ax"))
        except (KeyError, TypeError, ValueError) as error: raise ValueError("keyboard registers missing") from error
        if ds != 0x57E0 + self.delta or ss != ds or not 0 <= sp <= 0xFFFF or not 0 <= bp <= 0xFFFF:
            raise ValueError("keyboard data/stack binding differs")
        parent_bp = self.initial["bp"] if self.initial else bp
        kind = int.from_bytes(checked_read(client, ss, parent_bp + 6, 2), "little")
        key = int.from_bytes(checked_read(client, ss, parent_bp + 0x12, 2), "little")
        cb_off, cb_seg = struct.unpack("<HH", checked_read(client, ss, parent_bp + 0x1E, 4))
        ret_off, ret_seg = struct.unpack("<HH", checked_read(client, ss, parent_bp + 2, 4))
        mode = int.from_bytes(checked_read(client, ds, 0x0DAB, 2), "little")
        turn = int.from_bytes(checked_read(client, ds, 0x426D, 2), "little", signed=True)
        pointers = [struct.unpack("<HH", checked_read(client, s, o, 4))
                    for s, o in ((ds, 0x1431), (0x4E46 + self.delta, 0))]
        item = {"phase": phase, "cs_ip": stop["cs_ip"], "reason": stop["reason"],
                "seq": stop["seq"], "emu_ns": stop["emu_ns"], "ss": ss, "sp": sp, "bp": bp, "ax": ax,
                "event_kind": kind, "key_word": key, "mode": mode, "turn": turn,
                "callback": f"{cb_seg:04X}:{cb_off:04X}", "handler_return": f"{ret_seg:04X}:{ret_off:04X}",
                "menu_pointers": [f"{s:04X}:{o:04X}" for o, s in pointers]}
        if kind != 6 or (key | 0x20) & 0xFF != ord("p") or (cb_off, cb_seg) != (0x002A, 0x56B9 + self.delta):
            raise ValueError("keyboard event/key/close callback differs")
        if self.initial and any(item[k] != self.initial[k] for k in ("ss", "key_word", "callback", "handler_return")):
            raise ValueError("keyboard parent slots changed")
        self.sequence, self.time = stop["seq"], stop["emu_ns"]
        return item, pointers

    def boundary(self, client, stop):
        phase = self.points[stop["cs_ip"]]
        if phase != self.phase or stop.get("reason") != "breakpoint":
            raise ValueError("keyboard boundary phase/reason differs")
        item, pointers = self.sample(client, stop, phase)
        item["accepted"] = False
        self.report["events"].append(item)
        if phase == "state-call":
            if pointers[0] == (0, 0) or pointers[0] != pointers[1] or item["handler_return"] != f"{self.menu:04X}:0039":
                raise ValueError("keyboard forwarded menu/frame differs")
            self.initial = dict(item)
        elif phase != "trampoline" and item["bp"] != self.initial["bp"]:
            raise ValueError("keyboard parent BP differs")
        if phase == "state-return" and (item["ax"] == 0 or item["ax"] != item["mode"] or item["sp"] != self.initial["sp"]):
            raise ValueError("keyboard state gate/return differs")
        if phase == "turn" and item["turn"] >= 4:
            raise ValueError("keyboard turn gate rejected")
        if phase == "close-call": self.close_sp = item["sp"]
        if phase in ("reload", "opener-call", "trampoline") and pointers != [(0, 0), (0, 0)]:
            raise ValueError("keyboard menu pointers not cleared")
        if phase == "reload" and item["sp"] != self.close_sp:
            raise ValueError("keyboard callback SP differs")
        if phase == "opener-call": self.opener_sp = item["sp"]
        if phase == "trampoline":
            if self.opener_sp < 10 or item["sp"] != self.opener_sp - 4 or item["bp"] != self.initial["bp"]:
                raise ValueError("keyboard outer opener frame differs")
            self.root_sp = item["sp"]
            self.outer(client, item)
        item["accepted"] = True
        order = [p for _, p in self.OFFSETS] + ["trampoline", "getter"]
        self.phase = order[order.index(phase) + 1]

    def outer(self, client, item):
        off, seg = struct.unpack("<HH", checked_read(client, item["ss"], self.root_sp, 4))
        if (off, seg) != (0x1EC9, self.segment): raise ValueError("keyboard outer return differs")
        item["preferences_return"] = f"{seg:04X}:{off:04X}"

    def nested(self, client, stop, phase):
        if self.phase != phase or stop.get("reason") not in (("breakpoint", "watchpoint") if phase == "store" else ("breakpoint",)):
            raise ValueError("keyboard nested phase/reason differs")
        item, pointers = self.sample(client, stop, phase)
        if item["bp"] != self.root_sp - 2 or item["sp"] != self.root_sp - (6 if phase == "getter" else 2) or pointers != [(0, 0), (0, 0)]:
            raise ValueError("keyboard nested stack/pointer binding differs")
        saved = int.from_bytes(checked_read(client, item["ss"], self.root_sp - 2, 2), "little")
        if saved != self.initial["bp"]: raise ValueError("keyboard saved parent BP differs")
        item["saved_parent_bp"] = saved
        self.outer(client, item)
        self.report["events"].append(item)
        self.phase = "store" if phase == "getter" else "done"

    def complete(self):
        if self.phase != "done" or len(self.report["events"]) != 10:
            raise ValueError("keyboard endpoint incomplete")
        self.report["complete"] = True
