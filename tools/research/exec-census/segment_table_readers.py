"""Lists the relocated segment words that name DSUN.EXE's segment table or the overlay manager's
data segment (FND-EXE-567).

Usage: python -I segment_table_readers.py PATH_TO_DSUN.EXE

Checks the installed DSUN.EXE's size and XXH3-128 and applies its MZ relocations for a load
image at segment 0x1000. For every relocation whose word becomes 0x55E8 (the segment table,
FMT-EXE-002) or 0x55CE (the manager's data segment, whose offsets 0x01A0 to 0x08C8 hold the same
table), it prints where the word is and decodes the bytes before it until an instruction ends
exactly after the word, printing that instruction and the next six. It then searches the FBOV
overlays' code for fixup words that name descriptors whose segment is 0x45E8 or 0x45CE before
relocation. Last, it sorts every relocated word whose value is an overlay header's segment into
the segment word of a far call or jump (9A or EA three bytes before it), a word inside the segment
table, or anything else, and decodes the instructions from one byte before each of the others.
It sorts the overlays' fixup words that name an overlay header's descriptor the same way, and for
each that is not a far-transfer operand checks for `push` of the segment followed by `push` of a
trampoline start in that header. Finally it lists every instruction of a linear disassembly of
4AE5:0010..4AE5:1258 with a memory operand based on SI, DI or BX, at displacement 0, 2, 4 or 6,
through a segment other than ES. Nothing is written or executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs
from capstone.x86 import X86_OP_MEM

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
TARGETS = {0x55E8: "segment table", 0x55CE: "manager data"}
TABLE, PAYLOAD = 0x4B080, 0x57580

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages, count = struct.unpack_from("<HHH", body, 2)
reloc_at = struct.unpack_from("<H", body, 0x18)[0]
image = bytearray(body[header:(pages - 1) * 512 + (cblp or 512)])
relocated = []
for k in range(count):
    off, seg = struct.unpack_from("<HH", body, reloc_at + 4 * k)
    at = seg * 16 + off
    struct.pack_into("<H", image, at, (struct.unpack_from("<H", image, at)[0] + 0x1000) & 0xFFFF)
    relocated.append((seg, off, at))

md = Cs(CS_ARCH_X86, CS_MODE_16)
for seg, off, at in sorted(relocated, key=lambda r: r[2]):
    value = struct.unpack_from("<H", image, at)[0]
    if value not in TARGETS:
        continue
    print(f"-- word at {0x1000 + seg:04X}:{off:04X} = {value:04X} ({TARGETS[value]})")
    for back in range(1, 6):
        first = next(md.disasm(bytes(image[at - back:at + 8]), 0), None)
        if first is not None and first.size == back + 2:
            for i in list(md.disasm(bytes(image[at - back:at + 40]), off - back))[:7]:
                print(f"{0x1000 + seg:04X}:{i.address & 0xFFFF:04X}  {i.mnemonic} {i.op_str}")
            break
    else:
        print("   no instruction ends after the word: data")

segs = [struct.unpack_from("<H", body, TABLE + 8 * i)[0] for i in range(229)]
named = [i for i, s in enumerate(segs) if s + 0x1000 in TARGETS]
print("-- descriptors whose segment is a target:", named)
for i, s in enumerate(segs):
    if struct.unpack_from("<H", body, TABLE + 8 * i + 4)[0] != 3:
        continue
    payload_offset, code_size, fixup_size = struct.unpack_from("<IHH", body, header + s * 16 + 4)
    code = PAYLOAD + payload_offset
    for (f,) in struct.iter_unpack("<H", body[code + code_size:code + code_size + fixup_size]):
        if struct.unpack_from("<H", body, code + f)[0] // 8 in named:
            print(f"overlay {i}: fixup at code offset {f:#06x} names descriptor {struct.unpack_from('<H', body, code + f)[0] // 8}")

heads = {s + 0x1000 for i, s in enumerate(segs) if struct.unpack_from("<H", body, TABLE + 8 * i + 4)[0] == 3}
table_lin = (0x55E8 - 0x1000) * 16
far, in_table, others = 0, [], []
for seg, off, at in sorted(relocated, key=lambda r: r[2]):
    if struct.unpack_from("<H", image, at)[0] not in heads:
        continue
    if image[at - 3] in (0x9A, 0xEA):
        far += 1
    elif table_lin <= at < table_lin + 229 * 8:
        in_table.append((at - table_lin) % 8)
    else:
        others.append((seg, off, at))
print(f"-- header segments: {far} far call or jump segment words, {len(in_table)} words in the segment table"
      f" (record offsets {sorted(set(in_table))}), {len(others)} others")
for seg, off, at in others:
    print(f"word at {0x1000 + seg:04X}:{off:04X}:",
          "; ".join(f"{i.mnemonic} {i.op_str}" for i in list(md.disasm(bytes(image[at - 1:at + 16]), 0))[:4]))

far, pointers, odd = 0, 0, []
for i, s in enumerate(segs):
    if struct.unpack_from("<H", body, TABLE + 8 * i + 4)[0] != 3:
        continue
    payload_offset, code_size, fixup_size = struct.unpack_from("<IHH", body, header + s * 16 + 4)
    code = PAYLOAD + payload_offset
    for (f,) in struct.iter_unpack("<H", body[code + code_size:code + code_size + fixup_size]):
        target = segs[struct.unpack_from("<H", body, code + f)[0] // 8]
        if target + 0x1000 not in heads:
            continue
        if f >= 3 and body[code + f - 3] in (0x9A, 0xEA):
            far += 1
            continue
        ins = list(md.disasm(body[code + f - 1:code + f + 8], 0))
        count = struct.unpack_from("<H", body, header + target * 16 + 0x0C)[0]
        if (body[code + f - 1] == 0x68 and len(ins) > 1 and ins[1].mnemonic == "push"
                and ins[1].op_str.startswith("0x")):
            t = int(ins[1].op_str, 16)
            if t >= 0x20 and (t - 0x20) % 5 == 0 and t < 0x20 + 5 * count:
                pointers += 1
                continue
        odd.append((i, f))
print(f"-- overlay fixups naming a header: {far} far-transfer operands, {pointers} far pointers to a trampoline,"
      f" others {odd}")

detail = Cs(CS_ARCH_X86, CS_MODE_16)
detail.detail = True
base = (0x4AE5 - 0x1000) * 16
print("-- manager operands [si|di|bx + 0|2|4|6] outside ES")
for i in detail.disasm(bytes(image[base + 0x10:base + 0x1258]), 0x10):
    for o in i.operands:
        if (o.type == X86_OP_MEM and i.reg_name(o.mem.base) in ("si", "di", "bx")
                and i.reg_name(o.mem.segment) != "es" and o.mem.disp in (0, 2, 4, 6)):
            print(f"4AE5:{i.address:04X}  {i.mnemonic} {i.op_str}")
            break
