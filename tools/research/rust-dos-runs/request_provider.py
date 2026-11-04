"""Bounded SAVE/24 and SAVE/26 observations; FND-CONFIG-151/FND-COMBAT-120."""
import struct

from regenerate_checkpoints import checked_read


def request(client, registers, segment):
    ss, sp = (int(registers[key], 16) for key in ("ss", "sp"))
    off, seg, tag, number, start, count, slot = struct.unpack(
        "<HHIIIII", checked_read(client, ss, sp, 24))
    if tag != int.from_bytes(b"SAVE", "little") or number not in (24, 26):
        return None
    if off != 0x4D0 or seg != segment or start != 0 or count != 0xFFFFFFFF or not slot:
        raise ValueError("SAVE target lacks the declared complete-transfer wrapper frame")
    return {"number": number, "start": start, "count": count, "slot": slot,
            "ss": ss, "bp": sp - 2}


def frame(registers, original):
    if (int(registers["ss"], 16) != original["ss"] or
            int(registers["bp"], 16) != original["bp"]):
        raise ValueError("resource stop belongs to another core frame")


def selected(client, registers, original):
    frame(registers, original)
    ss, bp = original["ss"], original["bp"]
    def local(displacement):
        return int.from_bytes(checked_read(client, ss, bp + displacement, 4), "little")
    length, offset, count = local(-0x1C), local(-0x20), local(-0x18)
    provider, destination = local(-0xC), local(-0x14)
    if not 0 < length < 0x80000000 or offset >= 0x80000000 or count != length or not provider or not destination:
        raise ValueError("selected resource does not satisfy the declared full-transfer path")
    off, seg = provider & 0xFFFF, provider >> 16
    return {"length": length, "offset": offset, "count": count,
            "provider": f"{seg:04X}:{off:04X}",
            "provider_handle": int.from_bytes(checked_read(client, seg, off + 0x28, 4), "little"),
            "io_handle": int.from_bytes(checked_read(client, seg, off + 4, 2), "little"),
            "destination": f"{destination >> 16:04X}:{destination & 0xFFFF:04X}"}
