"""Lists which driver functions the sound utility can ask an installed driver for (FND-EXE-497).

Usage: python -I sound_ds_driver_function_census.py PATH_TO_SOUND_DS.EXE

Checks FND-EXE-350's identity, then, in the load image (file 0x1400..0x1E5E0; segment 1C08 starts
at load offset 0xC080):
- finds every stub `mov ax,imm16; jmp 1C08:03F6` and counts far calls (any seg:off resolving to
  the stub), near calls, and stored dwords resolving to it;
- lists every other near call or jump to the dispatcher 1C08:03F6 and to the lookup 1C08:03BE,
  with the instructions before it;
- for functions 00BD and 00BE, lists every instruction whose last operand is that immediate.
"""
import struct
import sys
from pathlib import Path

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

data = Path(sys.argv[1]).read_bytes()
assert (len(data), xxhash.xxh3_128_hexdigest(data)) == (204593, "236c2dc23c071eca421eb5b427caee57")
img = data[0x1400:0x1E5E0]
md = Cs(CS_ARCH_X86, CS_MODE_16)
SEG, DISPATCH, LOOKUP = 0xC080, 0xC476, 0xC43E

far, near, dwords = {}, {}, {}
for p in range(len(img) - 4):
    off, seg = struct.unpack_from("<HH", img, p)
    dwords.setdefault(seg * 16 + off, []).append(p)
    if img[p] == 0x9A:
        off, seg = struct.unpack_from("<HH", img, p + 1)
        far.setdefault(seg * 16 + off, []).append(p)
    if p >= SEG and img[p] in (0xE8, 0xE9):
        near.setdefault(p + 3 + struct.unpack_from("<h", img, p + 1)[0], []).append(p)

stubs = {}
for p in range(SEG, len(img) - 6):
    if img[p] == 0xB8 and img[p + 3] == 0xE9 and p + 6 + struct.unpack_from("<h", img, p + 4)[0] == DISPATCH:
        stubs[p + 3] = struct.unpack_from("<H", img, p + 1)[0]
        n = struct.unpack_from("<H", img, p + 1)[0]
        callers = [f"far {c:05X}" for c in far.get(p, [])] + [f"near {c:05X}" for c in near.get(p, [])]
        stored = [f"{c:05X}" for c in dwords.get(p, [])]
        print(f"function {n:04X} stub 1C08:{p - SEG:04X}: callers {callers or 'none'}; stored dwords {stored or 'none'}")


def before(p):
    for s in range(p - 12, p):
        seq = list(md.disasm(img[s:p + 3], s))
        if seq and seq[-1].address == p:
            return "; ".join(f"{i.mnemonic} {i.op_str}" for i in seq[-4:-1])
    return "?"


for name, target in (("dispatcher 1C08:03F6", DISPATCH), ("lookup 1C08:03BE", LOOKUP)):
    for c in near.get(target, []):
        if c not in stubs:
            print(f"{name}: {'call' if img[c] == 0xE8 else 'jmp'} at 1C08:{c - SEG:04X} after {before(c)}")
    for c in far.get(target, []):
        print(f"{name}: far call at load {c:05X}")
    print(f"{name}: stored dwords {[f'{c:05X}' for c in dwords.get(target, [])] or 'none'}")

for n in (0x00BD, 0x00BE):
    pat, sites = struct.pack("<H", n), []
    p = img.find(pat)
    while p >= 0:
        for s in range(max(0, p - 5), p):
            i = next(md.disasm(img[s:s + 8], s), None)
            if i and s + i.size == p + 2 and i.op_str.split(", ")[-1] == hex(n):
                sites.append(f"{s:05X} {i.mnemonic} {i.op_str}")
        p = img.find(pat, p + 1)
    print(f"immediate {n:04X}: {'; '.join(sites)}")
