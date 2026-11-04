"""Fixed GMAP/50 reconstruction observation; FND-COMBAT-147, FMT-REGION-003."""
import hashlib
import struct

from regenerate_checkpoints import checked_read


def request(client, registers, delta):
    ss, sp = (int(registers[key],16) for key in ("ss","sp"))
    off, seg, tag, number, start, count, slot = struct.unpack("<HHIIIII", checked_read(client,ss,sp,24))
    if tag != int.from_bytes(b"GMAP","little") or number != 50:
        return None
    caller_off, caller_seg = struct.unpack("<HH",checked_read(client,ss,sp+26,4))
    loaded = int.from_bytes(checked_read(client,0x56EF+delta,0x10,2),"little")
    if (off != 0x4D0 or seg != 0x38FF+delta or start or count != 0xFFFFFFFF
            or slot != ((0x57E0+delta)<<16 | 0x538) or caller_off != 0x1D30 or caller_seg != loaded
            or checked_read(client,caller_seg,0x1D2B,5) != struct.pack("<BHH",0x9A,0x4AB,0x38FF+delta)):
        raise ValueError("GMAP request differs from the declared map-child frame")
    return {"number":number,"ss":ss,"bp":sp-2,"slot":slot,"kind":"map"}


def state(client, delta):
    result = {}
    for name, offset in (("map",0x538),("first_plane",0x55B8),("second_plane",0x55B4)):
        off, seg = struct.unpack("<HH",checked_read(client,0x57E0+delta,offset,4))
        result[name] = f"{seg:04X}:{off:04X}"
    return result


def bounds(address, size):
    if size not in (12544,1568,1920,2):
        raise ValueError("undeclared map/effect span size")
    seg, off = (int(x,16) for x in address.split(":"))
    low = seg*16+off
    if not (seg or off) or not 0 <= seg <= 0xFFFF or not 0 <= off <= 0xFFFF or off+size > 0x10000 or low+size > 0x100000:
        raise ValueError("map/effect span exceeds declared real-mode bounds")
    return low, low+size


def read(client, address, size):
    if size not in (12544,1568):
        raise ValueError("undeclared map input size")
    bounds(address,size)
    seg, off = (int(x,16) for x in address.split(":"))
    return b"".join(checked_read(client,seg,off+at,min(261,size-at)) for at in range(0,size,261))


def expected_flags(base, first, second):
    if len(base) != 12544 or len(first) != 1568 or len(second) != 1568:
        raise ValueError("map/plane lengths differ from the fixed grid")
    result = bytearray(base)
    for x in range(128):
        for y in range(98):
            bit = 98*x+y
            value = (0x40 if first[bit//8] & (1<<(bit%8)) else 0) | (0x80 if second[bit//8] & (1<<(bit%8)) else 0)
            result[128*y+x] = (result[128*y+x] & 0x3F) | value
    return bytes(result)


def comparison(actual, expected):
    if len(actual) != 12544 or len(expected) != 12544:
        raise ValueError("map comparison length differs from the fixed grid")
    return {"equal":actual==expected,"different_bytes":sum(a!=b for a,b in zip(actual,expected)),
            "actual_sha256":hashlib.sha256(actual).hexdigest(),"expected_sha256":hashlib.sha256(expected).hexdigest()}


def overlaps(spans):
    ranges = {name:bounds(address,size) for name,(address,size) in spans.items()}
    names = sorted(ranges)
    return [[a,b] for i,a in enumerate(names) for b in names[i+1:]
            if max(ranges[a][0],ranges[b][0]) < min(ranges[a][1],ranges[b][1])]
