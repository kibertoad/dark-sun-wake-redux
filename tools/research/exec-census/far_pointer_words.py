"""Classifies every 16-bit word of the installed DSUN.EXE that equals a routine's offset (Q-EXE-007).

Usage: python -I far_pointer_words.py DSUN_EXE SEGMENT:OFFSET [SEGMENT:OFFSET ...]

A far routine can be called through a pointer only if its offset is stored with its segment,
which in the load image carries an MZ relocation and in overlay code is in the overlay's fixup
list. For each word equal to the routine's offset it prints the file offset, where it lies (the
load image, an overlay's code, an overlay's fixup list or elsewhere in the pack) and:

- "far call" or "far jump": 9A or EA before it and a relocated or fixed-up segment word after it
  equal to the routine's segment (load image) or its descriptor * 8 (overlay code);
- "relocated pointer": the word after it is relocated or fixed up;
- "immediate": an instruction decoded from one of the 6 bytes before the word that ends after it
  and has the word as an immediate operand (mov, push), with the relocated or fixed-up segment
  words within 12 bytes either side;
- otherwise "no segment": the word has no relocated or fixed-up segment word after it and is
  not a far call, with the instruction that holds it when one decoded from the 6 bytes before it
  covers it.
Checks the size and XXH3-128. Nothing is executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages, relocs = struct.unpack_from("<HHH", body, 2)
reloc_at = struct.unpack_from("<H", body, 0x18)[0]
image_end = (pages - 1) * 512 + (cblp or 512)
fixed = set()
for k in range(relocs):
    off, seg = struct.unpack_from("<HH", body, reloc_at + 4 * k)
    fixed.add(header + seg * 16 + off)
table_offset, count = struct.unpack_from("<IH", body, image_end + 8)
rows = [struct.unpack_from("<HHHH", body, table_offset + 8 * i) for i in range(count)]
spans = []
for i, (seg, _, flags, _) in enumerate(rows):
    if flags == 3:
        payload, code_size, fixup_size = struct.unpack_from("<IHH", body, header + seg * 16 + 4)
        code = image_end + 16 + payload
        spans.append((code, code + code_size, code + code_size + fixup_size, i))
        fixed |= {code + struct.unpack_from("<H", body, code + code_size + 2 * k)[0] for k in range(fixup_size // 2)}
md = Cs(CS_ARCH_X86, CS_MODE_16)


def place(at):
    if header <= at < image_end:
        return "load image"
    for code, end, fix_end, i in spans:
        if code <= at < end:
            return f"overlay {i} code +{at - code:04X}"
        if end <= at < fix_end:
            return f"overlay {i} fixups"
    return "header" if at < header else "pack outside overlay code"


def holder(at):
    for back in range(6, 0, -1):
        for i in md.disasm(body[at - back:at + 8], at - back):
            if i.address <= at and i.address + i.size >= at + 2:
                return i
            break
    return None


for item in sys.argv[2:]:
    seg, off = (int(p, 16) for p in item.split(":"))
    print(f"-- {item}")
    word = struct.pack("<H", off)
    at = body.find(word)
    while at != -1:
        kind = "no segment"
        if at + 2 in fixed:
            kind = "relocated pointer"
            if body[at - 1] in (0x9A, 0xEA):
                kind = "far call" if body[at - 1] == 0x9A else "far jump"
        i = holder(at)
        text = f"{i.mnemonic} {i.op_str}" if i else "no instruction"
        if kind == "no segment" and i and i.mnemonic in ("mov", "push") and f"{off:#x}" in i.op_str and "[" not in i.op_str.split(",")[-1]:
            near = [f for f in fixed if at - 12 <= f <= at + 12]
            kind = "immediate" + (" near a segment word" if near else " with no segment word near")
        print(f"0x{at:X} {place(at)} {kind}: {text}")
        at = body.find(word, at + 1)
