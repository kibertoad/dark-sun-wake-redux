"""Lists the DSUN.EXE instructions in inventory rows whose immediate or displacement lies in a range.

Usage: python -I operand_range.py PATH_TO_DSUN.EXE INVENTORY_TSV LOW HIGH

Checks the installed DSUN.EXE's size and XXH3-128 and decodes each range of each row of
INVENTORY_TSV linearly from its start, restarting one byte on after an undecodable byte, as
block_copies.py does (resident rows as SEGMENT:OFFSET with the load image at segment 0x1000,
overlay rows as file offsets). It prints each instruction with an immediate or a memory
displacement whose 16-bit value is at least LOW and below HIGH (hexadecimal), with its row,
address, the operand's kind ("imm", or "disp" with the segment override and base register, if
any) and its value. Nothing is written or executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs
from capstone.x86 import X86_OP_IMM, X86_OP_MEM

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
inventory = sys.argv[2]
low_value, high_value = int(sys.argv[3], 16), int(sys.argv[4], 16)
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages = struct.unpack_from("<HH", body, 2)
image_end = (pages - 1) * 512 + (cblp or 512)
table_offset, count = struct.unpack_from("<IH", body, image_end + 8)
rows = [struct.unpack_from("<HHHH", body, table_offset + 8 * i) for i in range(count)]
overlays = []
for i, (seg, _, flags, _) in enumerate(rows):
    if flags == 3:
        payload, code_size = struct.unpack_from("<IH", body, header + seg * 16 + 4)
        code = image_end + 16 + payload
        overlays.append((i, code, code + code_size))
md = Cs(CS_ARCH_X86, CS_MODE_16)
md.detail = True


def site(text):
    if ":" in text:
        seg, off = (int(p, 16) for p in text.split(":"))
        return header + (seg - 0x1000) * 16 + off, seg
    return int(text, 16), None


def where(offset, seg):
    if seg is None:
        o = next((o for o in overlays if o[1] <= offset < o[2]), None)
        return f"overlay {o[0]} +{offset - o[1]:04X} (file 0x{offset:08X})" if o else f"file 0x{offset:08X}"
    return f"{seg:04X}:{offset - header - (seg - 0x1000) * 16:04X}"


for line in open(inventory, encoding="utf-8").read().splitlines()[1:]:
    start, _, ranges = line.split("\t")
    for item in ranges.split():
        lo_text, hi_text = item.split("..")
        lo, seg = site(lo_text)
        hi, _ = site(hi_text)
        at = lo
        while at < hi:
            part = list(md.disasm(body[at:hi], at))
            for ins in part:
                for op in ins.operands:
                    if op.type == X86_OP_IMM:
                        value, kind = op.imm & 0xFFFF, "imm"
                    elif op.type == X86_OP_MEM:
                        value = op.mem.disp & 0xFFFF
                        base = ins.reg_name(op.mem.base) if op.mem.base else ""
                        override = ins.reg_name(op.mem.segment) + ":" if op.mem.segment else ""
                        kind = f"disp {override}{base}".rstrip()
                    else:
                        continue
                    if low_value <= value < high_value:
                        print(f"{where(ins.address, seg)} in {start}: {kind} {value:04X}: {ins.mnemonic} {ins.op_str}")
            end = part[-1].address + part[-1].size if part else at
            at = end + 1 if end < hi else hi
