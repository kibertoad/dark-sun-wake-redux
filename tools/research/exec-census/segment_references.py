"""Lists the references to a resident code segment in DSUN.EXE that are not direct far calls or jumps.

Usage: python -I segment_references.py PATH_TO_DSUN.EXE SEGMENT [SEGMENT ...]

A SEGMENT is a resident segment as the load image at segment 0x1000 names it (for example 3D72),
taken from the segment table (FND-EXE-571). Checks the installed DSUN.EXE's size and XXH3-128 and
finds every segment word that names it: an MZ relocation in the load image whose word is the
segment minus 0x1000, or an FBOV fixup in an overlay's code whose word is the descriptor * 8.
A word three bytes after a 9A or EA byte is the segment of a far call or far jump and is
dropped, since direct_callers.py covers those. For each
remaining word it prints the location and the instructions that decode without a gap from up to
16 bytes before it through the instruction holding it and the next two, so the offset that goes
with the segment can be read. Each hit is a byte pattern; check it against the instructions
around it. Nothing is written or executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages, relocs = struct.unpack_from("<HHH", body, 2)
reloc_at = struct.unpack_from("<H", body, 0x18)[0]
image_end = (pages - 1) * 512 + (cblp or 512)
relocated = []
for k in range(relocs):
    off, seg = struct.unpack_from("<HH", body, reloc_at + 4 * k)
    relocated.append(header + seg * 16 + off)
table_offset, count = struct.unpack_from("<IH", body, image_end + 8)
rows = [struct.unpack_from("<HHHH", body, table_offset + 8 * i) for i in range(count)]
fixups = []
for i, (seg, _, flags, _) in enumerate(rows):
    if flags == 3:
        payload, code_size, fixup_size = struct.unpack_from("<IHH", body, header + seg * 16 + 4)
        code = image_end + 16 + payload
        fixups += [(code + struct.unpack_from("<H", body, code + code_size + 2 * k)[0], i, code)
                   for k in range(fixup_size // 2)]
md = Cs(CS_ARCH_X86, CS_MODE_16)


def show(at, low):
    for back in range(16, 0, -1):
        start = max(low, at - back)
        run = list(md.disasm(body[start:at + 24], start))
        if run and run[0].address == start and any(i.address <= at < i.address + i.size for i in run):
            last = next(k for k, i in enumerate(run) if i.address <= at < i.address + i.size)
            for i in run[:last + 3]:
                print(f"    0x{i.address:08X}  {i.mnemonic} {i.op_str}")
            return


for item in sys.argv[2:]:
    seg = int(item, 16) - 0x1000
    descriptor = next((i for i, r in enumerate(rows) if r[2] in (0, 1) and r[0] == seg), None)
    print(f"-- {item}" + (f" (descriptor {descriptor})" if descriptor is not None else " (no descriptor)"))
    for at in relocated:
        if struct.unpack_from("<H", body, at)[0] == seg and body[at - 3] not in (0x9A, 0xEA):
            s = (at - header) // 16
            print(f"load image 0x{at:08X} ({s + 0x1000:04X}:{at - header - s * 16:04X} as paragraph)")
            show(at, header)
    if descriptor is None:
        continue
    for at, i, code in fixups:
        if struct.unpack_from("<H", body, at)[0] == descriptor * 8 and body[at - 3] not in (0x9A, 0xEA):
            print(f"overlay {i} 0x{at:08X} (+{at - code:04X})")
            show(at, code)
