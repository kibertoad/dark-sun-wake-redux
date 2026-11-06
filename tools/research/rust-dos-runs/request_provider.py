"""Bounded SAVE/24 and SAVE/26 observations; FND-CONFIG-151/FND-COMBAT-120."""
import struct
import hashlib

from regenerate_checkpoints import checked_read


def request(client, registers, segment, include_trigger=False):
    ss, sp = (int(registers[key], 16) for key in ("ss", "sp"))
    off, seg, tag, number, start, count, slot = struct.unpack(
        "<HHIIIII", checked_read(client, ss, sp, 24))
    if tag != int.from_bytes(b"SAVE", "little") or number not in ((24, 26, 38) if include_trigger else (24, 26)):
        return None
    if off != 0x4D0 or seg != segment or start != 0 or count != 0xFFFFFFFF or not slot:
        raise ValueError("SAVE target lacks the declared complete-transfer wrapper frame")
    return {"number": number, "start": start, "count": count, "slot": slot,
            "ss": ss, "bp": sp - 2}


def trigger_bindings(client, original, delta):
    if original["number"] != 38 or original["slot"] >> 16 != original["ss"]:
        raise ValueError("trigger request lacks the declared reader-local slot")
    values = {}
    for name, segment, offset in (("destination_slot", original["ss"], original["slot"] & 0xFFFF),
                                  ("prepared_slot", 0x57E0 + delta, 0x1612),
                                  ("live_pointer", 0x57E0 + delta, 0x40C0)):
        off, seg = struct.unpack("<HH", checked_read(client, segment, offset, 4))
        values[name] = f"{seg:04X}:{off:04X}"
    values["equal"] = len(set(values.values())) == 1 and values["live_pointer"] != "0000:0000"
    return values


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


def payload(client, selection, number, expected):
    count = selection["count"]
    if number not in (24, 26, 38) or count != {24: 1920, 26: 2, 38: 2600}[number] or len(expected) != count:
        raise ValueError("payload comparison is outside the admitted target spans")
    seg, off = (int(x, 16) for x in selection["destination"].split(":"))
    if off + count > 0x10000 or seg * 16 + off + count > 0x100000:
        raise ValueError("target payload crosses the declared destination bounds")
    actual = b"".join(checked_read(client, seg, off + at, min(261, count - at))
                      for at in range(0, count, 261))
    return {"equal": actual == expected, "count": count,
            "destination_sha256": hashlib.sha256(actual).hexdigest(),
            "private_span_sha256": hashlib.sha256(expected).hexdigest()}
