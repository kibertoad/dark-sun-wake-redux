"""Lists the copy sites and the calls inside given routines of DSUN.EXE.

Usage: python -I callee_copies.py PATH_TO_DSUN.EXE INVENTORY_TSV COPIES_TXT TARGET [TARGET ...]

COPIES_TXT is the output of block_copies.py. A TARGET is SEG:OFF or DESCRIPTOR+OFF; a SEG of
5600 or above is read as an overlay stub segment and replaced by the routine its trampoline
enters. The routine's extent is the first range of the inventory row starting at it, or, with no
such row, the bytes up to the next row start in the same code (marked "no row").
Calls are printed with their resolved targets. Nothing is written or executed.
"""
import re
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

body = open(sys.argv[1], "rb").read()
assert xxhash.xxh3_128_hexdigest(body) == "e296af55ba2ecde7e77f555c90f33d0b"
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages, relocs = struct.unpack_from("<HHH", body, 2)
reloc_at = struct.unpack_from("<H", body, 0x18)[0]
image_end = (pages - 1) * 512 + (cblp or 512)
relocated = {header + struct.unpack_from("<H", body, reloc_at + 4 * k + 2)[0] * 16 + struct.unpack_from("<H", body, reloc_at + 4 * k)[0] for k in range(relocs)}
table_offset, count = struct.unpack_from("<IH", body, image_end + 8)
rows = [struct.unpack_from("<HHHH", body, table_offset + 8 * i) for i in range(count)]
overlays = {}
for i, (seg, _, flags, _) in enumerate(rows):
    if flags == 3:
        payload, code_size, fixup_size = struct.unpack_from("<IHH", body, header + seg * 16 + 4)
        code = image_end + 16 + payload
        fixups = {code + struct.unpack_from("<H", body, code + code_size + 2 * k)[0] for k in range(fixup_size // 2)}
        overlays[i] = (code, code + code_size, fixups, seg)
md = Cs(CS_ARCH_X86, CS_MODE_16)


def file_of(t):
    if "+" in t:
        d, o = t.split("+")
        return overlays[int(d)][0] + int(o, 16), None
    s, o = (int(p, 16) for p in t.split(":"))
    return header + (s - 0x1000) * 16 + o, s


def stub_target(seg, off):
    """An overlay routine reached through the stub segment seg's trampoline at off, or None."""
    t = header + (seg - 0x1000) * 16 + off
    if body[t:t + 2] != b"\xcd\x3f":
        return None
    d = next((i for i, o in overlays.items() if o[3] == seg - 0x1000), None)
    return f"{d}+{struct.unpack_from('<H', body, t + 2)[0]:04X}" if d is not None else None


starts = []
for line in open(sys.argv[2], encoding="utf-8").read().splitlines()[1:]:
    start, _, ranges = line.split("\t")
    first = ranges.split()[0].split("..")
    if ":" in first[0]:
        s, o = (int(p, 16) for p in first[0].split(":"))
        lo = header + (s - 0x1000) * 16 + o
        hi = header + (s - 0x1000) * 16 + int(first[1].split(":")[1], 16)
    else:
        lo, hi = int(first[0], 16), int(first[1], 16)
    starts.append((lo, hi, ranges))
starts.sort()
copies = [l.rstrip() for l in open(sys.argv[3], encoding="utf-8") if l.startswith(("call ", "movs"))]

for t in sys.argv[4:]:
    seg_off = re.fullmatch(r"([0-9A-F]{4}):([0-9A-F]{4})", t)
    if seg_off and int(seg_off[1], 16) >= 0x5600:
        st = stub_target(int(seg_off[1], 16), int(seg_off[2], 16))
        print(f"== {t} is a trampoline to {st}")
        t = st
    at, seg = file_of(t)
    row = next((r for r in starts if r[0] == at), None)
    if row:
        lo, hi, label = at, row[1], row[2]
    else:
        nxt = next((r[0] for r in starts if r[0] > at), at + 0x400)
        lo, hi, label = at, min(nxt, at + 0x2000), "no row"
    print(f"== {t} file 0x{lo:X}..0x{hi:X} ({label})")
    for c in copies:
        m = re.search(r"file 0x([0-9A-F]+)", c)
        if m and lo <= int(m[1], 16) < hi:
            print("   COPY", c)
    for ins in md.disasm(body[lo:hi], lo):
        if ins.mnemonic in ("lcall", "call"):
            raw = ins.bytes
            if raw[0] == 0x9A:
                off, word = struct.unpack_from("<HH", raw, 1)
                if seg is not None:
                    dest = f"{word + 0x1000:04X}:{off:04X}"
                    st = stub_target(word + 0x1000, off) if word + 0x1000 >= 0x5600 else None
                    dest += f" ({st})" if st else ""
                else:
                    d = word // 8
                    if d in overlays:
                        tt = header + overlays[d][3] * 16 + off
                        dest = f"{d}+{struct.unpack_from('<H', body, tt + 2)[0]:04X}" if body[tt:tt + 2] == b"\xcd\x3f" else f"{d}:{off:04X}"
                    else:
                        dest = f"{rows[d][0] + 0x1000:04X}:{off:04X}" if d < len(rows) else f"?{word:04X}:{off:04X}"
            elif raw[0] == 0xE8:
                tgt = ins.address + 3 + struct.unpack_from("<h", raw, 1)[0]
                if seg is None:
                    o = next(i for i, v in overlays.items() if v[0] <= ins.address < v[1])
                    dest = f"{o}+{tgt - overlays[o][0]:04X}"
                else:
                    dest = f"{seg:04X}:{(tgt - header - (seg - 0x1000) * 16) & 0xFFFF:04X}"
            else:
                dest = ins.op_str
            print(f"   call {dest}")
