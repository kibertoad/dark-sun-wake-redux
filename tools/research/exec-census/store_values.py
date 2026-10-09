"""Lists the instructions in DSUN.EXE that store to given words, with the instructions before each.

Usage: python -I store_values.py PATH_TO_DSUN.EXE [--indexed] WORD [WORD ...]

Checks the installed DSUN.EXE's size and XXH3-128. Decodes 16-bit x86 at every byte of the MZ
load image and of every overlay's code, as immediate_search.py does, and keeps the instructions
whose first operand is a memory operand with the hexadecimal WORD as its displacement and that
write it (mov, pop, xchg, add, sub, and, or, xor, inc, dec, les/lds excluded since they read).
For each it prints the location and up to six instructions that decode without a gap before it,
so the stored value can be read. With --indexed, stores through a base or index register
with that displacement (a table entry) are kept too. Every hit is a byte pattern; check that it lies on the
instruction path around it. Nothing is written or executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs
from capstone.x86 import X86_OP_MEM

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
WRITES = {"mov", "pop", "xchg", "add", "sub", "and", "or", "xor", "inc", "dec", "adc", "sbb", "not", "neg"}

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
indexed = "--indexed" in sys.argv
words = {int(w, 16) for w in sys.argv[2:] if w != "--indexed"}
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages = struct.unpack_from("<HH", body, 2)
image_end = (pages - 1) * 512 + (cblp or 512)
table_offset, count = struct.unpack_from("<IH", body, image_end + 8)
rows = [struct.unpack_from("<HHHH", body, table_offset + 8 * i) for i in range(count)]
spans = [(header, image_end, None)]
for i, (seg, _, flags, _) in enumerate(rows):
    if flags == 3:
        payload, code_size = struct.unpack_from("<IH", body, header + seg * 16 + 4)
        spans.append((image_end + 16 + payload, image_end + 16 + payload + code_size, i))
md = Cs(CS_ARCH_X86, CS_MODE_16)
md.detail = True
needles = [struct.pack("<H", w) for w in words]
for low, high, overlay in spans:
    candidates = set()
    for needle in needles:
        at = body.find(needle, low, high)
        while at != -1:
            candidates.update(range(max(low, at - 5), at))
            at = body.find(needle, at + 1, high)
    for start in sorted(candidates):
        ins = next(md.disasm(body[start:start + 8], start), None)
        if ins is None or ins.mnemonic not in WRITES or not ins.operands:
            continue
        op = ins.operands[0]
        if op.type != X86_OP_MEM or (not indexed and (op.mem.base or op.mem.index)) or (op.mem.disp & 0xFFFF) not in words:
            continue
        where = (f"overlay {overlay} 0x{start:08X}" if overlay is not None
                 else f"{(start - header) // 16 + 0x1000:04X}:{(start - header) % 16:04X} (file 0x{start:08X})")
        context = []
        for back in range(24, 0, -1):
            run = list(md.disasm(body[start - back:start], start - back))
            if run and run[0].address == start - back and run[-1].address + run[-1].size == start:
                context = run[-6:]
                break
        print(f"{where}: {ins.mnemonic} {ins.op_str}")
        for c in context:
            print(f"    {c.mnemonic} {c.op_str}")
