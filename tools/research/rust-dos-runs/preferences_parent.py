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


class DispatchTrace(ParentTrace):
    """Only the known window-specific dispatch call, plus the parent samples."""
    def arm(self, client):
        site = self.address(0x39D1, 0x0802)
        if checked_read(client, 0x39D1 + self.delta, 0x0802, 5) != bytes((0x26, 0xFF, 0x9F, 0xF5, 0)):
            raise ValueError("dispatcher call instruction differs")
        super().arm(client)
        self.points[site] = "dispatch"
        client.set_breakpoint(site, once=True)
        self.phase, self.dispatch = "dispatch", None
        self.report["limits"] = "fixed direct dispatcher and sampled menu/outer-return bindings; no other roots, continuous frame writes, keyboard path or later menu return"

    def boundary(self, client, stop):
        if self.points[stop["cs_ip"]] != "dispatch":
            if self.points[stop["cs_ip"]] == "arm":
                if self.dispatch is None:
                    raise ValueError("menu arm lacks observed dispatcher call")
                r = stop["registers"]
                if (int(r["ss"], 16) != self.dispatch["ss"]
                        or int(r["sp"], 16) != self.dispatch["sp"] - 10
                        or int(r["bp"], 16) != self.dispatch["sp"] - 6):
                    raise ValueError("menu arm does not join dispatcher frame")
            super().boundary(client, stop)
            if self.report["events"][-1]["phase"] == "arm":
                item = self.report["events"][-1]
                if (item["handler_return"] != self.address(0x39D1, 0x0807)
                        or item["menu_pointers"] != [self.dispatch["window"]] * 2):
                    raise ValueError("menu arm dispatcher return/window differs")
            return
        if self.phase != "dispatch" or stop.get("reason") != "breakpoint":
            raise ValueError("dispatcher phase/reason differs")
        r = stop.get("registers", {})
        if (r.get("system", {}).get("mode") != "real"
                or type(stop.get("seq")) is not int or stop["seq"] <= self.sequence
                or type(stop.get("emu_ns")) is not int or stop["emu_ns"] < self.time):
            raise ValueError("dispatcher requires ordered real-mode stop")
        try:
            ds, ss, sp, es, bx = (int(r[key], 16) for key in ("ds", "ss", "sp", "es", "bx"))
        except (KeyError, TypeError, ValueError) as error:
            raise ValueError("dispatcher registers missing") from error
        if ds != 0x57E0 + self.delta or ss != ds or sp < 10:
            raise ValueError("dispatcher data/stack binding differs")
        kind, control = struct.unpack("<HH", checked_read(client, ss, sp, 4))
        if (kind, control) != (2, 10303):
            raise ValueError("dispatcher copied event differs")
        pointers = [struct.unpack("<HH", checked_read(client, s, o, 4))
                    for s, o in ((ds, 0x1431), (0x4E46 + self.delta, 0))]
        if pointers != [(bx, es), (bx, es)] or (bx, es) == (0, 0):
            raise ValueError("dispatcher window differs from live menu")
        window = checked_read(client, es, bx, 12)
        flags = int.from_bytes(checked_read(client, es, bx + 0x9E, 2), "little")
        if window[:4] != b"WIND" or int.from_bytes(window[8:12], "little") != 10500 or flags & 0x1F != 8:
            raise ValueError("dispatcher window is not ready Game Menu")
        off, seg = struct.unpack("<HH", checked_read(client, es, bx + 0xF5, 4))
        if (off, seg) != (0x0034, 0x56B9 + self.delta):
            raise ValueError("dispatcher handler target differs")
        item = {"phase": "dispatch", "reason": stop["reason"], "cs_ip": stop["cs_ip"],
                "seq": stop["seq"], "emu_ns": stop["emu_ns"], "ss": ss, "sp": sp,
                "event_kind": kind, "control": control, "window": f"{es:04X}:{bx:04X}",
                "window_flags": flags, "handler": f"{seg:04X}:{off:04X}",
                "loaded_menu": f"{self.menu:04X}:0000"}
        self.report["events"].append(item)
        self.dispatch, self.phase = item, "arm"
        self.sequence, self.time = stop["seq"], stop["emu_ns"]

    def complete(self):
        if self.phase != "done" or len(self.report["events"]) != 6:
            raise ValueError("dispatcher endpoint is incomplete")
        self.report["complete"] = True
