"""Declared overlay 187 returns only; FND-EXE-562 and FND-COMBAT-142."""
import struct

from regenerate_checkpoints import checked_read


def region_target(client, registers, delta, expected=0xF26):
    if expected not in (0xF26, 0xF2F):
        raise ValueError("undeclared overlay return offset")
    off, seg = struct.unpack("<HH", checked_read(client, int(registers["ss"],16), int(registers["sp"],16), 4))
    header = 0x56EF + delta
    saved = int.from_bytes(checked_read(client, header, 2, 2), "little")
    loaded = int.from_bytes(checked_read(client, header, 0x10, 2), "little")
    if not seg or (seg != header and seg != loaded):
        raise ValueError("return segment is not the declared header or current load")
    if off != expected and not (off == 0 and saved == expected and seg in (header, loaded)):
        raise ValueError("unsupported encoded overlay return")
    return f"{seg:04X}:{expected:04X}", seg == header


def region_iret(client, registers, original, delta, expected=0xF26):
    if expected not in (0xF26, 0xF2F):
        raise ValueError("undeclared overlay IRET offset")
    if (registers["ss"] != original["ss"] or
            int(registers["sp"],16) != int(original["sp"],16)-2):
        return None
    off, seg, _ = struct.unpack("<HHH", checked_read(client, int(registers["ss"],16), int(registers["sp"],16), 6))
    loaded = int.from_bytes(checked_read(client, 0x56EF+delta, 0x10, 2), "little")
    if off != expected or not loaded or seg != loaded:
        raise ValueError("overlay IRET does not identify the reloaded caller")
    return f"{seg:04X}:{off:04X}"


class ReaderReturn:
    def __init__(self, client, registers, delta):
        self.client, self.original, self.delta = client, registers, delta
        off, seg = struct.unpack("<HH", checked_read(client, int(registers["ss"],16), int(registers["sp"],16), 4))
        if off != 0xF2F:
            raise ValueError("reader return differs from declared caller")
        self.target = f"{seg:04X}:{off:04X}"
        self.watch = f"{registers['ss']}:{int(registers['sp'],16)+2:04X}"
        self.trap = f"{0x56EF+delta:04X}:0000"
        self.iret = f"{0x4AE5+delta:04X}:0559"
        self.breakpoint = self.target
        self.awaiting_trap = self.awaiting_iret = False
        self.events = []
        client.set_watchpoint(self.watch, 2)
        client.set_breakpoint(self.target, once=True)

    def handle(self, stop):
        r, client = stop["registers"], self.client
        if stop["reason"] == "watchpoint" and self.watch is not None:
            target, trapped = region_target(client, self.original, self.delta, 0xF2F)
            self.events.append({"kind": "relocation", "target": target, "stop": stop})
            if self.breakpoint is not None:
                client.remove_breakpoints(self.breakpoint)
                self.breakpoint = None
            self.target = target
            if trapped:
                client.set_breakpoint(self.trap, once=True)
                self.awaiting_trap = True
            else:
                client.set_breakpoint(target, once=True)
                self.breakpoint = target
            return "continued"
        if stop["reason"] != "breakpoint":
            return None
        if self.awaiting_trap and stop["cs_ip"] == self.trap:
            self._returned_frame(r)
            header = 0x56EF + self.delta
            if (checked_read(client, header, 0, 2) != bytes([0xCD,0x3F]) or
                    int.from_bytes(checked_read(client, header, 2, 2), "little") != 0xF2F):
                raise ValueError("reader trap metadata differs from declared return")
            client.remove_watchpoints(self.watch)
            self.watch = None
            if checked_read(client, 0x4AE5+self.delta, 0x559, 1) != bytes([0xCF]):
                raise ValueError("overlay handler lacks declared IRET")
            client.set_breakpoint(self.iret)
            self.awaiting_trap, self.awaiting_iret = False, True
            self.events.append({"kind": "trap", "stop": stop})
            return "continued"
        if self.awaiting_iret and stop["cs_ip"] == self.iret:
            target = region_iret(client, r, self.original, self.delta, 0xF2F)
            if target is None:
                self.events.append({"kind": "nested-iret", "stop": stop})
                return "continued"
            client.remove_breakpoints(self.iret)
            self.awaiting_iret = False
            self.target = self.breakpoint = target
            client.set_breakpoint(target, once=True)
            self.events.append({"kind": "reload", "target": target, "stop": stop})
            return "continued"
        if self.breakpoint is not None and stop["cs_ip"] == self.target:
            self._returned_frame(r)
            seg, off = (int(x,16) for x in self.target.split(":"))
            if checked_read(client, seg, off, 4) != bytes([0x59,0x88,0x46,0xFE]):
                raise ValueError("reader caller lacks declared POP and saved-AL store")
            if self.watch is not None:
                client.remove_watchpoints(self.watch)
                self.watch = None
            self.breakpoint = None
            return "returned"
        return None

    def _returned_frame(self, registers):
        if (registers["ss"] != self.original["ss"] or
                int(registers["sp"],16) != int(self.original["sp"],16)+4):
            raise ValueError("reader return belongs to another frame")
