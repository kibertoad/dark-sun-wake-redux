"""Lists DSUN.EXE instructions that write to one word or byte of a segment through ES.

Usage: python -I segment_displacement_writes.py PATH_TO_DSUN.EXE SEGMENT DISPLACEMENT

Checks the installed DSUN.EXE's size and XXH3-128. SEGMENT is a resident segment as the load
image at segment 0x1000 names it (hexadecimal, such as 4C0E) and DISPLACEMENT a hexadecimal
offset in it. Decodes 16-bit x86 at every byte of the MZ load image and of every overlay's code
(flags-3 descriptors, FMT-EXE-002/FMT-EXE-003), and keeps each instruction with an ES segment
override whose first operand is memory with that displacement and no base or index register
(mov, the and/or/xor/add/sub/adc/sbb group, inc, dec, not, neg, xchg and the shifts), at any
operand width, so a word or double-word store that covers the byte is listed too. It prints the
instruction when one of the 12 bytes before it loads a register with the segment's word: the
file word SEGMENT - 0x1000 in the load image, or in overlay code the segment word descriptor * 8
of the descriptor whose segment is SEGMENT (FMT-EXE-005). Hits whose ES load is further away,
writes through a base or index register, string instructions and block copies are not listed.
Decoding at every byte also lists instructions that start inside other instructions; each hit
is to be checked against the instructions around it. Nothing is written or executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs
from capstone.x86 import X86_OP_MEM, X86_PREFIX_ES, X86_REG_ES

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
WRITES = {"mov", "and", "or", "xor", "add", "sub", "adc", "sbb", "inc", "dec", "not", "neg",
          "xchg", "shl", "shr", "sar", "sal", "rol", "ror", "rcl", "rcr"}

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
segment, disp = int(sys.argv[2], 16), int(sys.argv[3], 16)
md = Cs(CS_ARCH_X86, CS_MODE_16)
md.detail = True
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages = struct.unpack_from("<HH", body, 2)
image_end = (pages - 1) * 512 + (cblp or 512)
table_offset, count = struct.unpack_from("<IH", body, image_end + 8)
rows = [struct.unpack_from("<HHHH", body, table_offset + 8 * i) for i in range(count)]
overlay_word = next((8 * i for i, r in enumerate(rows) if r[0] + 0x1000 == segment), None)
spans = [("load image", header, image_end, segment - 0x1000)]
for i, (seg, _, flags, _) in enumerate(rows):
    if flags == 3 and overlay_word is not None:
        payload_offset, code_size = struct.unpack_from("<IH", body, header + seg * 16 + 4)
        start = image_end + 16 + payload_offset
        spans.append((f"overlay {i}", start, start + code_size, overlay_word))
print(f"segment {segment:04X}, displacement 0x{disp:X}, overlay segment word "
      f"{'none' if overlay_word is None else hex(overlay_word)}")
for name, start, end, word in spans:
    loads = {bytes([op]) + struct.pack("<H", word) for op in (0xB8, 0xB9, 0xBA, 0xBB, 0xBE, 0xBF)}
    for at in range(start, end):
        ins = next(md.disasm(body[at:min(at + 15, end)], 0), None)
        if ins is None or ins.mnemonic not in WRITES or not ins.operands:
            continue
        o = ins.operands[0]
        if (o.type != X86_OP_MEM or o.mem.segment != X86_REG_ES or o.mem.base or o.mem.index
                or (o.mem.disp & 0xFFFF) != disp or X86_PREFIX_ES not in ins.prefix):
            continue
        before = body[max(start, at - 12):at]
        if not any(load in before for load in loads):
            continue
        where = (f"{0x1000 + (at - header) // 16:04X}:{(at - header) % 16:04X} (file 0x{at:08X})"
                 if name == "load image" else f"{name} file 0x{at:08X}")
        print(f"{where}  {ins.mnemonic} {ins.op_str}")
