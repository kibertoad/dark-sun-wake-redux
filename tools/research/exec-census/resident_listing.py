"""Disassembles ranges of an approved resident MZ image with its relocations applied.

Usage: python -I resident_listing.py PATH_TO_DSUN.EXE SEGMENT:START..SEGMENT:END [...]

Checks installed DSUN.EXE or SOUND_DS.EXE size and XXH3-128, applies MZ relocations for a load image
at segment 0x1000, and disassembles each half-open range as 16-bit x86, addressed in the
range's segment. Nothing is written or executed.
"""
import struct
import sys
import re

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

SOURCES = {(634416, "e296af55ba2ecde7e77f555c90f33d0b"),
           (204593, "236c2dc23c071eca421eb5b427caee57")}

def span(item, image_size):
    if not isinstance(item, str) or not re.fullmatch(r"[0-9A-Fa-f]{4}:[0-9A-Fa-f]{4}\.\.[0-9A-Fa-f]{4}:[0-9A-Fa-f]{4,5}", item):
        raise ValueError("resident listing: expected SEG:START..SEG:END")
    low, high = item.split("..")
    seg, start = (int(p, 16) for p in low.split(":"))
    end_seg, end = (int(p, 16) for p in high.split(":"))
    base = (seg - 0x1000) * 16
    if seg != end_seg or not 0 <= start < end <= 0x10000 or base < 0 or base + end > image_size:
        raise ValueError("resident listing: range outside the declared segment/load image")
    return seg, start, end, base


def main():
    if len(sys.argv) < 3:
        raise SystemExit("usage: resident_listing.py APPROVED_EXE SEG:START..SEG:END [...]")
    with open(sys.argv[1], "rb") as stream:
        body = stream.read(634417)
    if (len(body), xxhash.xxh3_128_hexdigest(body)) not in SOURCES:
        raise ValueError("resident listing: not the approved DSUN.EXE or SOUND_DS.EXE")
    header = struct.unpack_from("<H", body, 8)[0] * 16
    cblp, pages, count = struct.unpack_from("<HHH", body, 2)
    reloc_at = struct.unpack_from("<H", body, 0x18)[0]
    image = bytearray(body[header:(pages - 1) * 512 + (cblp or 512)])
    ranges = [span(item, len(image)) for item in sys.argv[2:]]
    for k in range(count):
        off, seg = struct.unpack_from("<HH", body, reloc_at + 4 * k)
        struct.pack_into("<H", image, seg * 16 + off, (struct.unpack_from("<H", image, seg * 16 + off)[0] + 0x1000) & 0xFFFF)
    md = Cs(CS_ARCH_X86, CS_MODE_16)
    for item, (seg, start, end, base) in zip(sys.argv[2:], ranges):
        print(f"-- {item}")
        for ins in md.disasm(bytes(image[base + start:base + end]), start):
            print(f"{seg:04X}:{ins.address:04X}  {ins.bytes.hex():<14} {ins.mnemonic} {ins.op_str}")


if __name__ == "__main__":
    main()
