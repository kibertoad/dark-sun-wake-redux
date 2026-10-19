"""Bounded gameplay draw recorder from FND-RNG-011 and FND-SCRIPT-011.

Only the sound-rule random_mod caller and script instruction 0x52 are
admitted. Unknown callers stop; addresses remain in local research output.
"""
import struct
from pathlib import Path
from startup_window import wait_for_stop


def linear(address):
    segment, offset = (int(x, 16) for x in address.split(":"))
    return segment * 16 + offset


def remainder(draw, bound):
    if bound == 0:
        raise ValueError("zero random_mod bound")
    quotient = abs(draw) // abs(bound)
    if (draw < 0) != (bound < 0):
        quotient = -quotient
    return draw - quotient * bound


class Recorder:
    def __init__(self, delta):
        self.delta = delta
        self.ranges = []
        inventory = Path(__file__).resolve().parents[3] / "coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv"
        for row in inventory.read_text(encoding="utf-8-sig").splitlines()[1:]:
            start, _, spans = row.split("\t")
            if ":" in start:
                for span in spans.split():
                    low, high = span.split("..")
                    self.ranges.append((linear(low), linear(high), start))

    def address(self, segment, offset):
        return f"{segment + self.delta:04X}:{offset:04X}"

    def routine(self, segment, offset):
        at = (segment - self.delta) * 16 + offset
        found = [name for low, high, name in self.ranges if low <= at < high]
        return found[0] if len(found) == 1 else None

    def record(self, s, stop):
        r = stop["registers"]
        def read(segment, offset, size):
            if offset < 0 or offset + size > 0x10000:
                raise ValueError("RNG frame crosses stack bound")
            return s.client.read_memory(f"{segment:04X}:{offset:04X}", size).data
        def reach(segment, offset):
            target = self.address(segment, offset)
            ob, exhausted = wait_for_stop(s, s.continue_(until=target))
            if exhausted or ob.stop["reason"] != "breakpoint" or ob.stop["cs_ip"] != target:
                raise RuntimeError("RNG arithmetic continuation differs")
            return ob.stop["registers"]
        ss, sp, bp = (int(r[x], 16) for x in ("ss", "sp", "bp"))
        offset, segment = struct.unpack("<HH", read(ss, sp, 4))
        seed_address = self.address(0x57E0, 0x384A)
        seed = struct.unpack("<I", s.client.read_memory(seed_address, 4).data)[0]
        after = (seed * 0x015A4E35 + 1) & 0xFFFFFFFF
        draw = (after >> 16) & 0x7FFF
        caller = (segment - self.delta) * 16 + offset
        if caller == 0x2834 * 16 + 0x0633:
            bound = struct.unpack("<h", read(ss, bp + 6, 2))[0]
            off, seg = struct.unpack("<HH", read(ss, bp + 2, 4))
            if self.routine(seg, off) == "2834:0519":
                cbp = struct.unpack("<H", read(ss, bp, 2))[0]
                off, seg = struct.unpack("<HH", read(ss, cbp + 2, 4))
            if self.routine(seg, off) != "2834:000C":
                raise RuntimeError(f"unrecorded RNG caller {self.routine(seg, off)}")
            result = remainder(draw, bound)
            registers = reach(0x2834, 0x0636)
            if int(registers["dx"], 16) != result & 0xFFFF:
                raise RuntimeError("RNG remainder differs")
            rule = "RULE-SOUND-003"
        elif caller == 0x172C * 16 + 0x1710:
            registers = reach(0x172C, 0x1719)
            bound = (int(registers["dx"], 16) << 16) | int(registers["ax"], 16)
            bound = bound - 0x100000000 if bound & 0x80000000 else bound
            product = draw * (bound + 1)
            result = abs(product) // 0x8000 * (-1 if product < 0 else 1)
            reach(0x172C, 0x1741)
            stored = struct.unpack("<i", s.client.read_memory(self.address(0x4C13, 0x031B), 4).data)[0]
            if stored != result:
                raise RuntimeError("script RNG accumulator differs")
            rule = "RULE-SCRIPT-006"
        else:
            raise RuntimeError("unrecorded generator return site")
        actual = struct.unpack("<I", s.client.read_memory(seed_address, 4).data)[0]
        if actual != after:
            raise RuntimeError("RNG next state differs")
        return {"rule": rule, "bound": bound, "result": result,
                "seed_before": seed, "seed_after": after}
