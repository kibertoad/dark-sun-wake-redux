"""Reads DSUN.EXE's overlay cache in EMS and extended memory (FND-EXE-563, FND-EXE-564).

Usage: python -I overlay_cache.py PATH_TO_DSUN.EXE

Checks the installed DSUN.EXE's size and XXH3-128 and applies its MZ relocations for a load
image at segment 0x1000. Prints the device name at 4AE5:0D82 and disassembles the manager
ranges FND-EXE-563 cites: the EMS setup (4AE5:08EB..4AE5:09CB), the copy to EMS
(4AE5:09CB..4AE5:0A4B), the load from EMS (4AE5:0A4B..4AE5:0AB5), the extended-memory setup
(4AE5:0AB5..4AE5:0BCF), the copy to and load from extended memory (4AE5:0BCF..4AE5:0C2E), the
cache allocation (4AE5:0C2E..4AE5:0D11) and the unload hook (4AE5:0D11..4AE5:0D27). It then
finds the descriptor of segment 4AE5 in the segment table, searches the FBOV payload for far
calls to 4AE5:08EB and 4AE5:0AB5 through that descriptor, checks that each call's segment word
is in its overlay's fixup list, and disassembles the overlay code around the calls
(FND-EXE-564). Nothing is written or executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
RANGES = [(0x08EB, 0x09CB), (0x09CB, 0x0A4B), (0x0A4B, 0x0AB5), (0x0AB5, 0x0BCF), (0x0BCF, 0x0C2E),
          (0x0C2E, 0x0D11), (0x0D11, 0x0D27)]
TABLE, PAYLOAD = 0x4B080, 0x57580

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages, count = struct.unpack_from("<HHH", body, 2)
reloc_at = struct.unpack_from("<H", body, 0x18)[0]
image = bytearray(body[header:(pages - 1) * 512 + (cblp or 512)])
for k in range(count):
    off, seg = struct.unpack_from("<HH", body, reloc_at + 4 * k)
    struct.pack_into("<H", image, seg * 16 + off, (struct.unpack_from("<H", image, seg * 16 + off)[0] + 0x1000) & 0xFFFF)

md = Cs(CS_ARCH_X86, CS_MODE_16)
base = (0x4AE5 - 0x1000) * 16
print("4AE5:0D82", bytes(image[base + 0xD82:base + 0xD8B]))
for start, end in RANGES:
    print(f"-- 4AE5:{start:04X}..4AE5:{end:04X}")
    for i in md.disasm(bytes(image[base + start:base + end]), start):
        print(f"4AE5:{i.address:04X}  {i.bytes.hex():<14} {i.mnemonic} {i.op_str}")

index = [i for i in range(229) if struct.unpack_from("<H", body, TABLE + 8 * i)[0] == 0x4AE5 - 0x1000]
print("descriptor of 4AE5:", index)
for i in range(229):
    seg, _, flags, _ = struct.unpack_from("<HHHH", body, TABLE + 8 * i)
    if flags != 3:
        continue
    stub = header + seg * 16
    payload_offset, code_size, fixup_size = struct.unpack_from("<IHH", body, stub + 4)
    code = PAYLOAD + payload_offset
    fixups = set(struct.unpack_from(f"<{fixup_size // 2}H", body, code + code_size))
    for at in range(code, code + code_size - 4):
        if body[at] == 0x9A:
            off, word = struct.unpack_from("<HH", body, at + 1)
            if off in (0x08EB, 0x0AB5) and word in [x * 8 for x in index]:
                rel = at - code
                print(f"-- overlay {i}: far call at code offset {rel:#06x} (file {at:#010x}) to 4AE5:{off:04X},"
                      f" segment word in fixups: {rel + 3 in fixups}")
relocated = set()
for k in range(count):
    off, seg = struct.unpack_from("<HH", body, reloc_at + 4 * k)
    relocated.add(seg * 16 + off)
print("-- relocated offset:segment pairs in the load image naming manager routines")
for at in range(len(image) - 4):
    off, seg = struct.unpack_from("<HH", image, at)
    if seg == 0x4AE5 and at + 2 in relocated and off in (0x08EB, 0x0AB5, 0x0D27, 0x0193):
        print(f"{0x1000 + at // 16:04X}:{at % 16:04X} -> 4AE5:{off:04X}, byte before {image[at - 1]:02X}")
for i in range(229):
    seg, _, flags, _ = struct.unpack_from("<HHHH", body, TABLE + 8 * i)
    if i == 180:
        payload_offset, code_size = struct.unpack_from("<IH", body, header + seg * 16 + 4)
        code = PAYLOAD + payload_offset
        print("-- overlay 180 code offsets 0x0261..0x02BD")
        for ins in md.disasm(body[code + 0x261:code + 0x2BD], 0x261):
            print(f"+{ins.address:04X}  {ins.bytes.hex():<18} {ins.mnemonic} {ins.op_str}")
