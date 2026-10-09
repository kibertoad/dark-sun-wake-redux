"""Static census of DOS service selection in SOUND_DS.EXE (FND-EXE-490).

Usage: python -I sound_ds_exec_census.py PATH_TO_SOUND_DS.EXE

Reads the shipped file only. Prints the MZ layout, every interrupt 21 instruction in the
load image with its nearest preceding immediate AX/AH writer, every far call to the
generic interrupt wrapper with its selector and input word zero, the immediate-0xCD and
vector-displacement controls, and the branch-into-gap control. Nothing is executed.
"""
import re
import struct
import sys
from collections import Counter
from pathlib import Path

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

LENGTH = 204593
XXH3_128 = "236c2dc23c071eca421eb5b427caee57"
WRAPPER = 0x20DF  # linear offset of 1000:20DF in the load image
INNER = 0x2110

data = Path(sys.argv[1]).read_bytes()
digest = xxhash.xxh3_128_hexdigest(data)
if len(data) != LENGTH or digest != XXH3_128:
    sys.exit(f"identity mismatch: length {len(data)} xxh3-128 {digest}")
md = Cs(CS_ARCH_X86, CS_MODE_16)

last, pages, nrel, hdrpar = struct.unpack_from("<4H", data, 2)
relo = struct.unpack_from("<H", data, 0x18)[0]
HDR = hdrpar * 16
END = pages * 512 - ((512 - last) if last else 0)
print(f"header 0x{HDR:X}, load image file 0x{HDR:X}..0x{END:X}, appended {len(data) - END} bytes "
      f"starting {data[END:END + 2].hex()}, relocations {nrel}")
relocs = set()
for k in range(nrel):
    off, seg = struct.unpack_from("<HH", data, relo + 4 * k)
    relocs.add(HDR + seg * 16 + off)


def aligned_chain(end_at, window=48):
    """Most common decode that ends exactly on the instruction at end_at."""
    first = next(md.disasm(data[end_at:end_at + 16], end_at))
    votes = Counter()
    for back in range(1, window + 1):
        s = end_at - back
        chain = list(md.disasm(data[s:end_at + first.size], s))
        if chain and chain[-1].address == end_at:
            votes[tuple((x.address, x.size, x.mnemonic, x.op_str) for x in chain)] += 1
    if not votes:
        return []
    return max(votes, key=lambda t: (votes[t], len(t)))


def pattern(pat):
    return [HDR + m.start() for m in re.finditer(pat, data[HDR:END], re.S)]


print("interrupt 21 sites:")
gaps = []
for h in pattern(re.escape(b"\xCD\x21")):
    chain = aligned_chain(h)
    writer = next(((a, sz, o) for a, sz, m, o in reversed(chain[:-1])
                   if m == "mov" and re.match(r"a[hx], (0x)?[0-9a-f]+$", o)), None)
    if writer is None:
        print(f"  file 0x{h:05X}: no immediate AX/AH writer")
        continue
    value = int(writer[2].split(", ")[1], 0)
    ah = value if writer[2].startswith("ah") else value >> 8
    between = [f"{m} {o}" for a, sz, m, o in chain if writer[0] < a < h]
    print(f"  file 0x{h:05X}: AH=0x{ah:02X} from file 0x{writer[0]:05X}; between: {between}")
    gaps.append((h, writer[0] + writer[1]))
print("interrupt 2E / 2F immediates in image:", len(pattern(re.escape(b"\xCD\x2E"))),
      len(pattern(re.escape(b"\xCD\x2F"))))
print("mov ah,4B in image:", len(pattern(re.escape(b"\xB4\x4B"))),
      "; mov ax,4Bxx candidates:", [hex(p) for p in pattern(b"\xB8.\x4B")])

print("far calls/jumps to the generic interrupt wrapper and its inner routine:")
for p in range(HDR, END - 4):
    if data[p] in (0x9A, 0xEA):
        off, seg = struct.unpack_from("<HH", data, p + 1)
        if seg * 16 + off in (WRAPPER, INNER):
            chain = aligned_chain(p)
            text = [f"{m} {o}" for a, sz, m, o in chain[-12:-1]]
            print(f"  file 0x{p:05X} -> {seg:04X}:{off:04X} relocated={p + 3 in relocs}: {text}")
for p in range(HDR, END - 2):
    if data[p] in (0xE8, 0xE9) and p + 3 + struct.unpack_from("<h", data, p + 1)[0] - HDR in (WRAPPER, INNER):
        print(f"  near rel16 at file 0x{p:05X}")
print("raw words equal to wrapper/inner offsets:",
      sum(struct.unpack_from("<H", data, p)[0] == WRAPPER for p in range(HDR, END - 1)),
      sum(struct.unpack_from("<H", data, p)[0] == INNER for p in range(HDR, END - 1)))

print("superset decode controls (every byte offset of the image as an instruction start):")
for p in range(HDR, END):
    i = next(md.disasm(data[p:p + 16], p), None)
    if i is None:
        continue
    last_op = i.op_str.split(",")[-1].strip()
    cd = i.mnemonic in ("mov", "push", "or", "xor", "add") and re.fullmatch(
        r"0x(cd|cd[0-9a-f]{2}|[0-9a-f]{1,2}cd)", last_op)
    vec = re.search(r"\[0x8[4-7]\]", i.op_str)
    if cd or vec:
        print(f"  {'imm-CD' if cd else 'vector'} file 0x{p:05X} aligned-starts={len(aligned_chain(p)) > 0} "
              f"{i.mnemonic} {i.op_str}")

print("direct branches or far immediates landing between an AH writer and its interrupt:")
for p in range(HDR, END - 1):
    b, targets = data[p], []
    if 0x70 <= b <= 0x7F or b in (0xEB, 0xE0, 0xE1, 0xE2, 0xE3):
        targets.append(p + 2 + struct.unpack_from("<b", data, p + 1)[0])
    elif b in (0xE8, 0xE9):
        targets.append(p + 3 + struct.unpack_from("<h", data, p + 1)[0])
    elif b in (0x9A, 0xEA) and p + 5 <= END:
        off, seg = struct.unpack_from("<HH", data, p + 1)
        targets.append(HDR + seg * 16 + off)
    for t in targets:
        for h, lo in gaps:
            if lo <= t <= h:
                host = next(i for i in md.disasm(data[p - 40:p + 8], p - 40) if i.address <= p < i.address + i.size)
                print(f"  candidate file 0x{p:05X} -> 0x{t:05X} (site 0x{h:05X}); aligned decode covering it: "
                      f"0x{host.address:05X} {host.mnemonic} {host.op_str}")
