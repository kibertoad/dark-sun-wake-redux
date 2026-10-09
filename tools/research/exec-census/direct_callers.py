"""Lists the direct call sites of routines in DSUN.EXE, resident or in an overlay.

Usage: python -I direct_callers.py PATH_TO_DSUN.EXE TARGET [TARGET ...]

A TARGET is a resident address SEGMENT:OFFSET (load image at segment 0x1000; it is first written
in the segment of the segment-table descriptor whose span holds it, FND-EXE-571) or an overlay
routine DESCRIPTOR+OFFSET (the offset in that overlay's code, hexadecimal). Checks the installed
DSUN.EXE's size and XXH3-128 and searches the raw bytes for:

- far calls (9A, offset, segment word) in the load image whose segment word has an MZ
  relocation and names the target's segment, for a resident target, or the target's trampoline
  header segment, for an overlay target;
- far calls in overlay code whose segment word is in that overlay's fixup list and equals the
  descriptor * 8 of the target's segment, or of the overlay whose trampoline leads to the target;
- near calls (E8, rel16) in the same code as the target: the same load-image segment for a
  resident target, the same overlay's code for an overlay target;
- for a resident target, push cs (0E) followed by a near call is reported as a near call.

An overlay target is reached by far calls only through its FMT-EXE-004 trampolines, so each
trampoline whose target is the routine is searched too. Each hit is printed with its file offset
and, for load-image hits, its address. A hit is a byte pattern; check it against the instructions
around it. Indirect calls and jumps are not searched. Nothing is written or executed.
"""
import struct
import sys

import xxhash

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages, relocs = struct.unpack_from("<HHH", body, 2)
reloc_at = struct.unpack_from("<H", body, 0x18)[0]
image_end = (pages - 1) * 512 + (cblp or 512)
relocated = set()
for k in range(relocs):
    off, seg = struct.unpack_from("<HH", body, reloc_at + 4 * k)
    relocated.add(header + seg * 16 + off)
table_offset, count = struct.unpack_from("<IH", body, image_end + 8)
rows = [struct.unpack_from("<HHHH", body, table_offset + 8 * i) for i in range(count)]
overlays = {}
for i, (seg, _, flags, _) in enumerate(rows):
    if flags != 3:
        continue
    payload, code_size, fixup_size, trampolines = struct.unpack_from("<IHHH", body, header + seg * 16 + 4)
    code = image_end + 16 + payload
    fixups = {code + struct.unpack_from("<H", body, code + code_size + 2 * k)[0] for k in range(fixup_size // 2)}
    overlays[i] = (code, code_size, fixups, seg, trampolines)


def far_sites(offset, raw_segment, descriptor):
    """Far calls to raw_segment:offset from the load image and to descriptor*8:offset from overlays."""
    found = []
    pattern = b"\x9a" + struct.pack("<HH", offset, raw_segment)
    start = body.find(pattern, header, image_end)
    while start != -1:
        if start + 3 in relocated:
            seg = (start - header) // 16
            found.append(f"far, load image 0x{start:08X} ({seg + 0x1000:04X}:{start - header - seg * 16:04X} as paragraph)")
        start = body.find(pattern, start + 1, image_end)
    pattern = b"\x9a" + struct.pack("<HH", offset, descriptor * 8)
    for i, (code, code_size, fixups, _, _) in overlays.items():
        start = body.find(pattern, code, code + code_size)
        while start != -1:
            if start + 3 in fixups:
                found.append(f"far, overlay {i} 0x{start:08X} (+{start - code:04X})")
            start = body.find(pattern, start + 1, code + code_size)
    return found


def near_sites(target, low, high):
    found = []
    for at in range(low, high - 2):
        if body[at] == 0xE8 and (at + 3 + struct.unpack_from("<h", body, at + 1)[0] - low) & 0xFFFF == target - low:
            found.append(f"near 0x{at:08X} (+{at - low:04X})" + (" after push cs" if body[at - 1] == 0x0E else ""))
    return found


for item in sys.argv[2:]:
    print(f"-- {item}")
    if ":" in item:
        seg, off = (int(p, 16) for p in item.split(":"))
        linear = (seg - 0x1000) * 16 + off
        # The segment-table descriptor whose span (FND-EXE-571) holds the address gives its code segment.
        descriptor = next((i for i, r in enumerate(rows) if r[2] in (0, 1) and r[0] * 16 + r[3] <= linear < r[0] * 16 + r[1]), None)
        raw = rows[descriptor][0] if descriptor is not None else seg - 0x1000
        off = linear - raw * 16
        if (raw + 0x1000, off) != (seg, int(item.split(":")[1], 16)):
            print(f"as {raw + 0x1000:04X}:{off:04X}")
        sites = far_sites(off, raw, descriptor) if descriptor is not None else []
        base = header + raw * 16
        sites += near_sites(base + off, base, min(base + 0x10000, image_end))
    else:
        descriptor, off = item.split("+")
        descriptor, off = int(descriptor), int(off, 16)
        code, code_size, _, seg, trampolines = overlays[descriptor]
        sites = near_sites(code + off, code, code + code_size)
        for t in range(trampolines):
            at = header + seg * 16 + 0x20 + 5 * t
            if body[at:at + 2] == b"\xcd\x3f" and struct.unpack_from("<H", body, at + 2)[0] == off:
                sites.append(f"trampoline {seg + 0x1000:04X}:{0x20 + 5 * t:04X}")
                sites += far_sites(0x20 + 5 * t, seg, descriptor)
    for site in sites:
        print(site)
