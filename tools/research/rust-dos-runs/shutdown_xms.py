"""Fixed type-three installation query and XMS free call linkage; no runner."""
import struct
from regenerate_checkpoints import checked_read

OFFSETS = (0xCC, 0xCE, 0xD8, 0xDA, 0x629, 0x62E)
NATIVE_ENTRY = "0160:1F00"  # rust-dos v1.8.1 dos_data::XMS_ENTRY.


class XmsCleanup:
    def __init__(self, delta, records):
        self.delta, self.records, self.current, self.phase = delta, records, None, None

    def observe(self, client, stop, outer):
        code, data = 0x15F3 + self.delta, 0x57E0 + self.delta
        r = stop["registers"]
        off = int(stop["cs_ip"].split(":")[1], 16)
        native = stop["cs_ip"] == NATIVE_ENTRY
        if (stop.get("reason") != "breakpoint"
                or not native and (stop["cs_ip"] != f"{code:04X}:{off:04X}" or off not in OFFSETS)
                or r.get("system", {}).get("mode") != "real"
                or r["ss"] != f"{data:04X}"):
            raise ValueError("wrong fixed XMS boundary or segment")
        bp, sp = (int(r[k], 16) for k in ("bp", "sp"))
        ax = int(r["ax"], 16)
        if native:
            self.require("native-entry")
            if (bp != self.current["bp"] or sp != bp - 8 or ax != 0x0A01
                    or int(r["ds"], 16) != data):
                raise ValueError("XMS native entry lacks fixed request/frame")
            ret, seg = struct.unpack("<HH", checked_read(client, data, sp, 4))
            if ret != 0x62E or seg != code:
                raise ValueError("XMS native return pair differs from fixed caller")
            self.current["native_entry"] = {"target": NATIVE_ENTRY, "ax": r["ax"], "dx": r["dx"],
                "argument_equal": int(r["dx"], 16) == self.current["argument"], "sp": r["sp"]}
            self.phase = "free-result"
            return
        if off == 0xCC:
            if self.current is not None or ax != 0x4300:
                raise ValueError("XMS installation request out of order or wrong selector")
            if bp != int(outer["sp"], 16) - 10:
                raise ValueError("XMS helper frame differs from observed outer callee")
            ret, seg, argument = struct.unpack("<3H", checked_read(client, data, bp + 2, 6))
            if ret != 0x45F or seg != 0x1425 + self.delta:
                raise ValueError("XMS helper caller differs from fixed type-three wrapper")
            self.current = {"bp": bp, "argument": argument,
                            "outer_argument_equal": argument == outer["argument"]}
            self.records.append(self.current)
            self.phase = "installation-result"
        elif self.current is None or bp != self.current["bp"]:
            raise ValueError("XMS observation lacks paired preserved helper frame")
        expected_sp = bp - (12 if off < 0x100 else 4)
        expected_ds = code if off < 0x100 else data
        if sp != expected_sp or int(r["ds"], 16) != expected_ds:
            raise ValueError("XMS observation frame or DS differs")
        if off == 0xCE:
            self.require("installation-result")
            self.current["installation_al"] = ax & 0xFF
            self.phase = "entry-request" if ax & 0xFF == 0x80 else "skip-return"
        elif off == 0xD8:
            self.require("entry-request")
            if ax != 0x4310:
                raise ValueError("XMS entry request has wrong selector")
            self.phase = "entry-result"
        elif off == 0xDA:
            self.require("entry-result")
            self.current["queried_target"] = f'{int(r["es"], 16):04X}:{int(r["bx"], 16):04X}'
            self.phase = "free-call"
        elif off == 0x629:
            self.require("free-call")
            if ax != 0x0A01:
                raise ValueError("XMS free call has wrong selector")
            target_off, target_seg = struct.unpack("<HH", checked_read(client, code, 0xB8, 4))
            target = f"{target_seg:04X}:{target_off:04X}"
            self.current["free"] = {"ax": r["ax"], "dx": r["dx"], "target": target,
                "target_equal": target == self.current["queried_target"],
                "argument_equal": int(r["dx"], 16) == self.current["argument"]}
            if target != NATIVE_ENTRY or target != self.current["queried_target"]:
                raise ValueError("XMS free pointer differs from observed fixed entry query")
            self.phase = "native-entry"
        elif off == 0x62E:
            if self.phase not in ("skip-return", "free-result"):
                raise ValueError("XMS return lacks free call or absent installation result")
            self.current["result"] = {"ax": r["ax"], "bx": r["bx"], "dx": r["dx"],
                                       "branch": self.phase}
            self.current, self.phase = None, None

    def require(self, phase):
        if self.phase != phase:
            raise ValueError("XMS service observation out of order")
