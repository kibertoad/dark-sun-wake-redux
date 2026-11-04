"""Fixed working-open and selected plain-row guards; FND-COMBAT-110/127, FND-CONFIG-038."""
import struct

from regenerate_checkpoints import checked_read
from request_provider import frame


def pointer(client, segment, offset):
    off, seg = struct.unpack("<HH", checked_read(client, segment, offset, 4))
    if not (off or seg):
        raise ValueError("null archive-origin pointer")
    return seg, off


def opening(client, registers, delta, read_path):
    ss, sp = (int(registers[key], 16) for key in ("ss", "sp"))
    ret, caller, path, reserve, options, output = struct.unpack(
        "<HHIIHI", checked_read(client, ss, sp, 18))
    name = read_path(client, path >> 16, path & 0xFFFF, 82)
    if bytes.fromhex(name["bytes_hex"]).lower() != b"c:\\darkrun.gff":
        return None
    if (ret != 0x42 or reserve != 0x4D4 or options != 2 or
            output != ((0x57E0 + delta) << 16 | 0x144E) or
            checked_read(client, caller, 0x3D, 5) != struct.pack("<BHH", 0x9A, 0x66, 0x38FF + delta)):
        raise ValueError("working open differs from its declared wrapper contract")
    return {"ss": ss, "bp": sp - 2, "path": name, "output": output,
            "reserve": reserve, "options": options}


def publication(client, registers, original, archive):
    frame(registers, original)
    if int(registers["ax"], 16) != 0:
        raise ValueError("working archive open did not succeed")
    seg, off = pointer(client, original["ss"], original["bp"] - 0xC)
    record = f"{seg:04X}:{off:04X}"
    io = int.from_bytes(checked_read(client, seg, off + 4, 2), "little")
    handle = int.from_bytes(checked_read(client, seg, off + 0x28, 4), "little")
    if (io != original["io_handle"] or handle != archive["working_handle"] or
            any(archive[key]["pointer"] != record or archive[key]["handle_field"] != handle for key in ("active", "head"))):
        raise ValueError("working record publication differs from its observed open")
    return {"provider": record, "provider_handle": handle, "io_handle": io}


def directory(client, registers, original, selection, published):
    frame(registers, original)
    if any(selection[key] != published[key] for key in ("provider", "provider_handle", "io_handle")):
        raise ValueError("selected provider differs from the observed working publication")
    seg, off = (int(x, 16) for x in selection["provider"].split(":"))
    position, length = struct.unpack("<II", checked_read(client, seg, off + 0x1C, 8))
    # FND-COMBAT-127: the native opener loads the directory at record+31.
    if not length or off + 0x31 + length > 0x10000 or seg * 16 + off + 0x31 + length > 0x100000:
        raise ValueError("provider directory exceeds the admitted contiguous bounds")
    base, end = seg * 16 + off + 0x31, seg * 16 + off + 0x31 + length
    type_seg, type_off = pointer(client, original["ss"], original["bp"] - 8)
    row_seg, row_off = pointer(client, original["ss"], original["bp"] - 4)
    type_at, row_at = type_seg * 16 + type_off, row_seg * 16 + row_off
    if not base <= type_at <= end - 8:
        raise ValueError("selected type lies outside the provider directory")
    tag, count = struct.unpack("<4sI", checked_read(client, type_seg, type_off, 8))
    if tag != b"SAVE" or count & 0x80000000 or type_at + 8 + count * 12 > end:
        raise ValueError("selected type is not a bounded plain SAVE table")
    displacement = row_at - (type_at + 8)
    if displacement < 0 or displacement % 12 or displacement // 12 >= count:
        raise ValueError("selected row is outside or unaligned in the plain table")
    number, offset, size = struct.unpack("<III", checked_read(client, row_seg, row_off, 12))
    if (number, offset, size) != (original["number"], selection["offset"], selection["length"]):
        raise ValueError("selected plain row differs from the resource locals")
    return {"directory_position": position, "directory_length": length,
            "table_count": count, "row_index": displacement // 12,
            "file_row_offset": position + row_at - base,
            "number": number, "offset": offset, "length": size}
