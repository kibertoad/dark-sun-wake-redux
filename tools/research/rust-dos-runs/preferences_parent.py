"""Fixed CONFIG-249 menu-event/outer-return samples for Q-CONFIG-014."""
import struct

from regenerate_checkpoints import checked_read


class ParentTrace:
    def __init__(self, delta):
        if type(delta) is not int or not -0x1000 <= delta <= 0:
            raise ValueError("unsupported parent relocation")
        self.delta, self.menu, self.root_sp = delta, None, None
        self.phase, self.initial = "unarmed", None
        self.sequence, self.time = -1, -1
        self.report = {"events": [], "complete": False,
                       "limits": "fixed sampled menu/event/outer-return bindings only; no continuous frame writes, other roots, keyboard path or later menu return"}

    def address(self, segment, offset):
        return f"{segment + self.delta:04X}:{offset:04X}"

    def arm(self, client):
        if self.phase != "unarmed":
            raise ValueError("parent observer must arm once")
        jump = checked_read(client, 0x56B9 + self.delta, 0x0034, 5)
        opcode, off, self.menu = struct.unpack("<BHH", jump)
        if opcode != 0xEA or off != 0 or self.menu == 0:
            raise ValueError("parent menu trampoline differs")
        call = checked_read(client, self.menu, 0x0281, 5)
        if call != struct.pack("<BHH", 0x9A, 0x0048, 0x5782 + self.delta):
            raise ValueError("parent menu call differs")
        self.points = {f"{self.menu:04X}:025E": "arm", f"{self.menu:04X}:0281": "call",
                       self.address(0x5782, 0x0048): "trampoline"}
        for point in self.points:
            client.set_breakpoint(point, once=True)
        self.phase = "arm"

    def is_site(self, stop):
        return self.menu is not None and stop.get("cs_ip") in self.points

    def sample(self, client, stop, phase):
        registers = stop.get("registers", {})
        if (registers.get("system", {}).get("mode") != "real"
                or type(stop.get("seq")) is not int or stop["seq"] <= self.sequence
                or type(stop.get("emu_ns")) is not int or stop["emu_ns"] < self.time):
            raise ValueError("parent requires ordered real-mode stops")
        try:
            ss, ds, sp, bp = (int(registers[key], 16) for key in ("ss", "ds", "sp", "bp"))
        except (KeyError, ValueError, TypeError) as error:
            raise ValueError("parent frame registers missing") from error
        if ss != 0x57E0 + self.delta or ds != ss or not 0 <= sp <= 0xFFFF or not 0 <= bp <= 0xFFFF:
            raise ValueError("parent data/stack binding differs")
        parent_bp = bp if self.initial is None else self.initial["bp"]
        kind, control = struct.unpack("<HH", checked_read(client, ss, parent_bp + 6, 4))
        off, seg = struct.unpack("<HH", checked_read(client, ss, parent_bp + 2, 4))
        pointers = [struct.unpack("<HH", checked_read(client, segment, offset, 4))
                    for segment, offset in ((ds, 0x1431), (0x4E46 + self.delta, 0))]
        item = {"phase": phase, "cs_ip": stop["cs_ip"], "reason": stop["reason"],
                "seq": stop["seq"], "emu_ns": stop["emu_ns"], "ss": ss, "sp": sp, "bp": bp,
                "event_kind": kind, "control": control, "handler_return": f"{seg:04X}:{off:04X}",
                "menu_pointers": [f"{s:04X}:{o:04X}" for o, s in pointers]}
        if kind != 2 or control != 10303:
            raise ValueError("parent event/control differs")
        if self.initial and item["handler_return"] != self.initial["handler_return"]:
            raise ValueError("parent handler return changed")
        self.sequence, self.time = stop["seq"], stop["emu_ns"]
        return item, pointers

    def boundary(self, client, stop):
        phase = self.points[stop["cs_ip"]]
        if stop.get("reason") != "breakpoint" or phase != self.phase:
            raise ValueError("parent boundary phase/reason differs")
        item, pointers = self.sample(client, stop, phase)
        if phase == "arm":
            if item["bp"] < 4 or item["sp"] != item["bp"] - 4 or pointers[0] == (0, 0) or pointers[0] != pointers[1]:
                raise ValueError("parent arm frame/menu pointers differ")
            off, seg = pointers[0]
            window = checked_read(client, seg, off, 12)
            if window[:4] != b"WIND" or int.from_bytes(window[8:12], "little") != 10500:
                raise ValueError("parent arm window is not Game Menu")
            item["window_flags"] = int.from_bytes(checked_read(client, seg, off + 0x9E, 2), "little")
            self.initial, self.phase = dict(item), "call"
        else:
            if pointers != [(0, 0), (0, 0)] or item["ss"] != self.initial["ss"] or item["bp"] != self.initial["bp"]:
                raise ValueError("parent post-close binding differs")
            if phase == "call":
                if item["sp"] != self.initial["sp"]:
                    raise ValueError("parent post-child SP differs")
                self.phase = "trampoline"
            else:
                if self.initial["sp"] < 6 or item["sp"] != self.initial["sp"] - 4:
                    raise ValueError("parent outer call SP differs")
                self.root_sp = item["sp"]
                self.outer(client, item)
                self.phase = "getter"
        self.report["events"].append(item)

    def outer(self, client, item):
        off, seg = struct.unpack("<HH", checked_read(client, item["ss"], self.root_sp, 4))
        if (off, seg) != (0x0286, self.menu):
            raise ValueError("parent outer Preferences return differs")
        item["preferences_return"] = f"{seg:04X}:{off:04X}"

    def nested(self, client, stop, phase):
        if self.phase != phase or stop.get("reason") not in (("breakpoint", "watchpoint") if phase == "store" else ("breakpoint",)):
            raise ValueError("parent nested phase/reason differs")
        item, pointers = self.sample(client, stop, phase)
        expected_sp = self.root_sp - (6 if phase == "getter" else 2)
        if item["sp"] != expected_sp or item["bp"] != self.root_sp - 2 or pointers != [(0, 0), (0, 0)]:
            raise ValueError("parent nested stack/menu binding differs")
        saved = int.from_bytes(checked_read(client, item["ss"], self.root_sp - 2, 2), "little")
        if saved != self.initial["bp"]:
            raise ValueError("parent saved BP differs")
        item["saved_parent_bp"] = saved
        self.outer(client, item)
        self.report["events"].append(item)
        self.phase = "store" if phase == "getter" else "done"

    def complete(self):
        if self.phase != "done" or len(self.report["events"]) != 5:
            raise ValueError("parent endpoint is incomplete")
        self.report["complete"] = True
