"""Bounded raw direct-transfer candidates for the fixed setup error-boundary question.

No execution, source text or configurable target. Offsets are in the MZ load image.
Every-byte candidates require separate instruction-boundary and reachability review.
"""
import struct

FRAME = 0xC080
TARGETS = (0xCBF5, 0xC476, 0xCD15, 0xCD16)


def candidates(image):
    if not isinstance(image, bytes) or not FRAME + 0x10000 <= len(image) <= 0x20000:
        raise ValueError("setup transfer census: bounded complete 1C08 frame required")
    result = {target: {"far": [], "relative": []} for target in TARGETS}
    for at in range(len(image) - 4):
        if image[at] in (0x9A, 0xEA):
            offset, segment = struct.unpack_from("<HH", image, at + 1)
            target = segment * 16 + offset
            if target in result:
                result[target]["far"].append({"site": at, "kind": "call" if image[at] == 0x9A else "jump"})
    for ip in range(0x10000):
        # Instruction fetch and the relative destination wrap within this frame.
        b = bytes(image[FRAME + ((ip + k) & 0xFFFF)] for k in range(4))
        kind, width, length = None, None, None
        if b[0] in (0xE8, 0xE9):
            kind, width, length = ("call" if b[0] == 0xE8 else "jump"), 2, 3
        elif b[0] == 0xEB or 0x70 <= b[0] <= 0x7F or 0xE0 <= b[0] <= 0xE3:
            kind, width, length = "jump" if b[0] == 0xEB else "conditional", 1, 2
        elif b[0] == 0x0F and 0x80 <= b[1] <= 0x8F:
            kind, width, length = "conditional", 2, 4
        if kind is None:
            continue
        displacement = int.from_bytes(b[length - width:length], "little", signed=True)
        target = FRAME + ((ip + length + displacement) & 0xFFFF)
        if target in result:
            result[target]["relative"].append({"site": FRAME + ip, "kind": kind})
    return result
