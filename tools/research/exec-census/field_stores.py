"""Lists the instructions in DSUN.EXE that write a record field: a displacement through a register.

Usage: python -I field_stores.py PATH_TO_DSUN.EXE INVENTORY_TSV [--es] DISP [DISP ...]

Checks the installed DSUN.EXE's size and XXH3-128. Keeps the instructions whose first operand is
a memory operand with a base or index register and one of the hexadecimal DISPs as its
displacement (one byte or two, compared as a word), and that write it (mov, pop, xchg, add, sub,
and, or, xor, inc, dec, adc, sbb, not, neg; les/lds and cmp excluded since they only read). With
--es, only operands with an ES segment override are kept, the form the game uses for far records.

Two passes. The first decodes each range of each row of INVENTORY_TSV (resident rows as
SEGMENT:OFFSET with the load image at segment 0x1000, overlay rows as file offsets) from its
start to its end, one instruction after another, and prints each hit with its row, address and
up to six instructions before it in that range. The second decodes at every byte of the load
image and of every overlay's code (flags-3 descriptors, FMT-EXE-002/FMT-EXE-003) and prints, as
"outside", each hit the first pass did not print: an instruction outside every inventoried
range, or one that starts inside another instruction. Nothing is written or executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs
from capstone.x86 import X86_OP_MEM, X86_REG_ES

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
WRITES = {"mov", "pop", "xchg", "add", "sub", "and", "or", "xor", "inc", "dec", "adc", "sbb", "not", "neg"}

args = [a for a in sys.argv[1:] if a != "--es"]
es_only = "--es" in sys.argv
body = open(args[0], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
inventory = args[1]
disps = {int(w, 16) for w in args[2:]}
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


def site(text):
    """File offset and display segment (None for an overlay) of an inventory site."""
    if ":" in text:
        seg, off = (int(p, 16) for p in text.split(":"))
        return header + (seg - 0x1000) * 16 + off, seg
    return int(text, 16), None


def where(offset, seg):
    if seg is None:
        overlay = next((o for low, high, o in spans if o is not None and low <= offset < high), None)
        code = next(low for low, high, o in spans if o == overlay)
        return f"overlay {overlay} +{offset - code:04X} (file 0x{offset:08X})"
    return f"{seg:04X}:{offset - header - (seg - 0x1000) * 16:04X} (file 0x{offset:08X})"


def hit(ins):
    if ins.mnemonic not in WRITES or not ins.operands:
        return False
    op = ins.operands[0]
    if op.type != X86_OP_MEM or not (op.mem.base or op.mem.index):
        return False
    if es_only and op.mem.segment != X86_REG_ES:
        return False
    return (op.mem.disp & 0xFFFF) in disps


printed = set()
for line in open(inventory, encoding="utf-8").read().splitlines()[1:]:
    start, _, ranges = line.split("\t")
    for item in ranges.split():
        low, high = item.split("..")
        lo, seg = site(low)
        hi, _ = site(high)
        run = []
        for ins in md.disasm(body[lo:hi], lo):
            if hit(ins):
                printed.add(ins.address)
                print(f"{where(ins.address, seg)} in {start}: {ins.mnemonic} {ins.op_str}")
                for c in run[-6:]:
                    print(f"    {c.mnemonic} {c.op_str}")
            run.append(ins)
for low, high, overlay in spans:
    for at in range(low, high):
        ins = next(md.disasm(body[at:min(at + 8, high)], at), None)
        if ins is None or at in printed or not hit(ins):
            continue
        seg = None if overlay is not None else 0x1000 + (at - header) // 16
        print(f"outside: {where(at, seg)}: {ins.mnemonic} {ins.op_str}")
