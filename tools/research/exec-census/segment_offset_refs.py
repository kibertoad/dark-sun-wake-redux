"""Lists DSUN.EXE sites that load a segment word into a register and address one offset of it.

Usage: python -I segment_offset_refs.py PATH_TO_DSUN.EXE SEG OFF

Checks the installed DSUN.EXE's size and XXH3-128. SEG is the segment at load segment 1000 (for
example 4E71), OFF the offset, both hexadecimal. For every `mov r16, imm16` in the load image whose
immediate is SEG - 0x1000, and in every overlay's code whose immediate is the overlay segment word
of SEG's descriptor (index times 8), it decodes up to six instructions from the move and prints the
site when one of them has a memory operand with displacement OFF and no base or index register.
Moves are found at every byte, so each hit is to be checked against the instructions around it.
Sites that reach the segment through `les`, a pushed far pointer, a computed offset or more than six
instructions are not listed. Nothing is written or executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs
from capstone.x86 import X86_OP_MEM

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
seg = int(sys.argv[2], 16); off = int(sys.argv[3], 16)
md = Cs(CS_ARCH_X86, CS_MODE_16); md.detail = True
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages = struct.unpack_from("<HH", body, 2)
image_end = (pages - 1) * 512 + (cblp or 512)
table_offset, count = struct.unpack_from("<IH", body, image_end + 8)
words = {seg - 0x1000: "load"}
for i in range(count):
    s, _, flags, _ = struct.unpack_from("<HHHH", body, table_offset + 8 * i)
    if s == seg - 0x1000:
        words[i * 8] = "overlay"
spans = [("load", header, image_end, seg - 0x1000)]
for i in range(count):
    s, _, flags, _ = struct.unpack_from("<HHHH", body, table_offset + 8 * i)
    if flags == 3:
        po, cs = struct.unpack_from("<IH", body, header + s * 16 + 4)
        st = image_end + 16 + po
        spans.append((f"overlay {i}", st, st + cs, [w for w, k in words.items() if k == "overlay"][0]))
for name, start, end, w in spans:
    pat = struct.pack("<H", w)
    at = start
    while True:
        at = body.find(pat, at, end)
        if at < 0:
            break
        op = body[at - 1]
        at += 1
        if not (0xB8 <= op <= 0xBF):
            continue
        p = at - 2
        out = []
        hit = None
        for ins in md.disasm(body[p:min(p + 40, end)], p):
            out.append(f"{ins.mnemonic} {ins.op_str}")
            for o in ins.operands:
                if o.type == X86_OP_MEM and o.mem.disp == off and o.mem.base == 0 and o.mem.index == 0:
                    hit = ins
            if len(out) >= 6 or hit:
                break
        if hit:
            if name == "load":
                lin = p - header
                loc = f"{(lin >> 4) + 0x1000:04X}:{lin & 0xF:04X} (file 0x{p:X})"
            else:
                loc = f"{name} file 0x{p:X}"
            print(loc, "|", " ; ".join(out))
