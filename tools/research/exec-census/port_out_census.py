"""Lists every OUT instruction in the inventoried code of DSUN.EXE with the port and value it is given.

Usage: python -I port_out_census.py PATH_TO_DSUN.EXE INVENTORY_TSV

Checks the installed DSUN.EXE's size and XXH3-128 and applies the MZ relocations for a load image
at segment 0x1000. For each row of the function inventory (coverage/<build>/DSUN.EXE.tsv) it
decodes every range of the row from its start as 16-bit x86: a resident range SSSS:OOOO..SSSS:OOOO
in the load image, an eight-digit range in the file (overlay code). For every OUT and OUTS
instruction (opcodes E6, E7, EE, EF, 6E and 6F, with any prefix) it prints the site and, from
the instructions decoded before it in the same range, the last one that writes DX and the last one that writes AL, AH or AX. An
immediate load is printed as its value; any other writer is printed as its text, and "none" means
no writer earlier in the range. The search is linear: a jump into the range between that writer
and the OUT, or a callee that changes the register, is not followed, so each value is to be
checked against the instructions around the site. Code outside the inventory is not searched.
After the sites it prints how many bytes of the load image and of overlay code the inventory
covers, and the number of those opcode bytes outside those ranges. Nothing is written or
executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs
from capstone.x86 import X86_OP_IMM, X86_OP_REG, X86_REG_AH, X86_REG_AL, X86_REG_AX, X86_REG_DX, X86_REG_DL, X86_REG_DH, X86_REG_EAX, X86_REG_EDX

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
OUT_OPCODES = {0x6E, 0x6F, 0xE6, 0xE7, 0xEE, 0xEF}
DX_REGS = {X86_REG_DX, X86_REG_DL, X86_REG_DH, X86_REG_EDX}
AX_REGS = {X86_REG_AX, X86_REG_AL, X86_REG_AH, X86_REG_EAX}

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages, count = struct.unpack_from("<HHH", body, 2)
reloc_at = struct.unpack_from("<H", body, 0x18)[0]
image_end = (pages - 1) * 512 + (cblp or 512)
image = bytearray(body[header:image_end])
for k in range(count):
    off, seg = struct.unpack_from("<HH", body, reloc_at + 4 * k)
    struct.pack_into("<H", image, seg * 16 + off, (struct.unpack_from("<H", image, seg * 16 + off)[0] + 0x1000) & 0xFFFF)
table_offset, segs = struct.unpack_from("<IH", body, image_end + 8)
overlay_spans = []
for i in range(segs):
    seg, _, flags, _ = struct.unpack_from("<HHHH", body, table_offset + 8 * i)
    if flags == 3:
        payload_offset, code_size = struct.unpack_from("<IH", body, header + seg * 16 + 4)
        start = image_end + 16 + payload_offset
        overlay_spans.append((i, start, start + code_size))

md = Cs(CS_ARCH_X86, CS_MODE_16)
md.detail = True


def writes(ins, regs):
    _, written = ins.regs_access()
    return any(r in regs for r in written)


def describe(ins):
    ops = ins.operands
    if ins.mnemonic == "mov" and len(ops) == 2 and ops[0].type == X86_OP_REG and ops[1].type == X86_OP_IMM:
        return f"{ins.reg_name(ops[0].reg)}={ops[1].imm & 0xFFFF:#06x}"
    if ins.mnemonic == "xor" and len(ops) == 2 and ops[0].type == X86_OP_REG and ops[1].type == X86_OP_REG and ops[0].reg == ops[1].reg:
        return f"{ins.reg_name(ops[0].reg)}=0"
    return f"[{ins.mnemonic} {ins.op_str}]"


covered_image = bytearray(len(image))
covered_file = set()
sites = 0
for line in open(sys.argv[2], encoding="utf-8").read().splitlines()[1:]:
    cols = line.split("\t")
    for part in cols[2].split():
        low, high = part.split("..")
        if ":" in low:
            seg, start = (int(p, 16) for p in low.split(":"))
            end = int(high.split(":")[1], 16)
            base = (seg - 0x1000) * 16
            code, origin = bytes(image[base + start:base + end]), start
            for b in range(base + start, base + end):
                covered_image[b] = 1
            name = lambda a, seg=seg: f"{seg:04X}:{a:04X}"
        else:
            start, end = int(low, 16), int(high, 16)
            code, origin = body[start:end], start
            covered_file.update(range(start, end))
            name = lambda a: f"0x{a:08X}"
        last_dx = last_ax = "none"
        for ins in md.disasm(code, origin):
            if ins.mnemonic.split()[-1].startswith("out"):
                sites += 1
                print(f"{name(ins.address)}  {ins.mnemonic} {ins.op_str:<10} dx: {last_dx:<28} ax: {last_ax}")
            if writes(ins, DX_REGS):
                last_dx = describe(ins)
            if writes(ins, AX_REGS):
                last_ax = describe(ins)
print(f"sites: {sites}")
print(f"load image bytes covered: {sum(covered_image)} of {len(image)}")
outside = sum(1 for b in range(len(image)) if not covered_image[b] and image[b] in OUT_OPCODES)
print(f"OUT opcode bytes in the load image outside the inventory: {outside}")
total = sum(e - s for _, s, e in overlay_spans)
inside = sum(1 for _, s, e in overlay_spans for b in range(s, e) if b in covered_file)
print(f"overlay code bytes covered: {inside} of {total}")
outside = sum(1 for _, s, e in overlay_spans for b in range(s, e) if b not in covered_file and body[b] in OUT_OPCODES)
print(f"OUT opcode bytes in overlay code outside the inventory: {outside}")
# Outside the inventory, an OUT byte with a load of DX with 0x3B0..0x3DF (BA xx 03) or a read of the
# BIOS CRT base word 0040:0063 (16 63 00) in the 32 bytes before it is printed for checking by hand.
def near_video_port(data, at):
    window = data[max(0, at - 32):at]
    if any(window[k] == 0xBA and window[k + 2] == 0x03 and 0xB0 <= window[k + 1] <= 0xDF for k in range(len(window) - 2)):
        return True
    return any(window[k:k + 3] == b"\x16\x63\x00" for k in range(len(window) - 2))
for b in range(len(image)):
    if not covered_image[b] and image[b] in OUT_OPCODES and near_video_port(image, b):
        print(f"outside the inventory, load image byte {b:#07x} ({0x1000 + b // 16:04X}:{b % 16:04X} as paragraph): {image[max(0, b - 32):b + 1].hex()}")
for _, s, e in overlay_spans:
    for b in range(s, e):
        if b not in covered_file and body[b] in OUT_OPCODES and near_video_port(body, b):
            print(f"outside the inventory, file byte 0x{b:08X}: {body[max(s, b - 32):b + 1].hex()}")
