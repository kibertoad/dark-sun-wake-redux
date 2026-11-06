"""Fixed slot-call and stream-use observations for the Quit runner."""
import struct
from regenerate_checkpoints import checked_read


def admitted(stop, delta, segment, offsets):
    if (stop.get("reason") != "breakpoint"
            or stop.get("cs_ip", "").upper() not in {f"{segment + delta:04X}:{o:04X}" for o in offsets}):
        raise ValueError("wrong shutdown use boundary")
    registers = stop["registers"]
    if registers.get("system", {}).get("mode") != "real":
        raise ValueError("shutdown use requires real mode")
    ds, ss = (int(registers[name], 16) for name in ("ds", "ss"))
    if ds != 0x57E0 + delta or ss != ds:
        raise ValueError("shutdown use DS/SS binding differs")
    return registers, ds, ss


def slot_call(client, stop, delta):
    registers, ds, ss = admitted(stop, delta, 0x1425, (0x16AB,))
    index, stride, sp = (int(registers[name], 16) for name in ("si", "bx", "sp"))
    if not 0 <= index < 16 or stride != 14 * index:
        raise ValueError("shutdown slot index/stride outside fixed contract")
    passed = struct.unpack("<H", checked_read(client, ss, sp, 2))[0]
    current = struct.unpack("<H", checked_read(client, ds, 0x3F4E + stride, 2))[0]
    off, seg = struct.unpack("<HH", checked_read(client, ds, 0x3F4A + stride, 4))
    return {"index": index, "passed_argument": passed, "table_argument": current,
            "argument_equal": passed == current, "target": f"{seg:04X}:{off:04X}",
            "before": {name: registers[name] for name in ("ds", "ss", "si", "sp")}}


def slot_entry(client, stop, delta, before):
    registers, _, ss = admitted(stop, delta, 0x1425, (0x0454,))
    if before["target"] != f"{0x1425 + delta:04X}:0454":
        raise ValueError("observed target differs from fixed type-three entry")
    sp, prior_sp = int(registers["sp"], 16), int(before["before"]["sp"], 16)
    if prior_sp < 4 or sp != prior_sp - 4:
        raise ValueError("cleanup entry stack differs from far-call frame")
    off, seg, argument = struct.unpack("<3H", checked_read(client, ss, sp, 6))
    if off != 0x16AF or seg != 0x1425 + delta:
        raise ValueError("cleanup entry return pair differs from dispatcher")
    return {"entry": stop["cs_ip"], "return": f"{seg:04X}:{off:04X}",
            "argument": argument, "argument_equal": argument == before["passed_argument"],
            "sp": registers["sp"]}


def slot_return(stop, delta, before):
    if (stop.get("reason") != "breakpoint"
            or stop.get("cs_ip", "").upper() != f"{0x1425 + delta:04X}:16AF"):
        raise ValueError("wrong shutdown slot return boundary")
    registers = stop["registers"]
    return {"ax": registers["ax"],
            "real_mode": registers.get("system", {}).get("mode") == "real",
            "preserved": {name: int(registers[name], 16) == int(value, 16)
                          for name, value in before["before"].items()},
            "after": {name: registers[name] for name in ("ds", "ss", "si", "sp")}}


def stream_use(client, stop, delta):
    registers, ds, _ = admitted(stop, delta, 0x1000, (0x3F1E, 0x2F30))
    selector = stop["cs_ip"].upper().endswith(":3F1E")
    off = int(registers["si" if selector else "di"], 16)
    if off not in (0x3682, 0x3692, 0x36A2, 0x36B2):
        raise ValueError("shutdown stream outside four fixed records")
    def word(relative):
        return struct.unpack("<H", checked_read(client, ds, off + relative, 2))[0]
    count = word(0)
    return {"boundary": "selector" if selector else "count-test", "offset": off,
            "count": count - 0x10000 if count & 0x8000 else count,
            "flags": word(2), "handle_byte": checked_read(client, ds, off + 4, 1)[0],
            "capacity": word(6), "buffer": word(8), "cursor": word(10), "self": word(14)}
