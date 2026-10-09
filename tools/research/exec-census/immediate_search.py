"""Lists DSUN.EXE instructions whose encoded immediate or displacement equals given words.

Usage: python -I immediate_search.py PATH_TO_DSUN.EXE WORD [WORD ...]

Checks the installed DSUN.EXE's size and XXH3-128. Decodes 16-bit x86 at every byte of the MZ
load image and of every overlay's code (flags-3 descriptors, FMT-EXE-002/FMT-EXE-003), and prints
each instruction with an encoded immediate or displacement equal to one of the hexadecimal WORDs,
with its resident address or overlay file offset and the 24 bytes of instructions before it.
Decoding at every byte also lists instructions that start inside other instructions; each hit is
to be checked against the instructions around it. Nothing is written or executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs
from capstone.x86 import X86_OP_IMM, X86_OP_MEM

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
wanted = {int(w, 16) for w in sys.argv[2:]}
md = Cs(CS_ARCH_X86, CS_MODE_16)
md.detail = True
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages = struct.unpack_from("<HH", body, 2)
image_end = (pages - 1) * 512 + (cblp or 512)
table_offset, count = struct.unpack_from("<IH", body, image_end + 8)
spans = [("load image", header, image_end)]
for i in range(count):
    seg, _, flags, _ = struct.unpack_from("<HHHH", body, table_offset + 8 * i)
    if flags == 3:
        payload_offset, code_size = struct.unpack_from("<IH", body, header + seg * 16 + 4)
        start = image_end + 16 + payload_offset
        spans.append((f"overlay {i}", start, start + code_size))
for name, start, end in spans:
    for at in range(start, end):
        ins = next(md.disasm(body[at:min(at + 15, end)], 0), None)
        if ins is None:
            continue
        for o in ins.operands:
            value = (o.imm & 0xFFFF if o.type == X86_OP_IMM and ins.imm_size else
                     o.mem.disp & 0xFFFF if o.type == X86_OP_MEM and ins.disp_size else None)
            if value in wanted:
                where = (f"{0x1000 + (at - header) // 16:04X}:{(at - header) % 16:04X}" if name == "load image"
                         else f"{name} file 0x{at:08X}")
                print(f"{where}  {ins.mnemonic} {ins.op_str}")
                break
