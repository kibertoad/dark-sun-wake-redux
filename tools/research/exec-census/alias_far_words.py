"""Lists the relocated or fixed-up segment words of DSUN.EXE that can form a routine's address (Q-RNG-008).

Usage: python -I alias_far_words.py PATH_TO_DSUN.EXE SEGMENT:OFFSET [SEGMENT:OFFSET ...]

A far pointer or far call names code by a segment word and an offset word. In the load image the
segment word carries an MZ relocation (it names the word plus 0x1000); in FBOV overlay code it is
in the overlay's fixup list and is a descriptor * 8 (it names that descriptor's segment plus
0x1000). For each target, given as the load-time address at segment 0x1000, this takes every such
segment word whose segment S can form the target's linear address S * 16 + O with an offset O from
0 to 0xFFFF, and looks for the word O in the 12 bytes either side of the segment word (the offset
of a stored pair, of a far call or jump, or of an immediate pushed or moved next to the segment).
It prints each hit with where it lies, whether S is the target's own segment ("canonical") or
another one ("alias"), and whether the segment word ends a far call (9A) or far jump (EA) whose
offset is O. Segments formed at run time (segment arithmetic, a segment read from memory that no
relocation or fixup marks) are not covered. Checks the size and XXH3-128. Nothing is executed.
"""
import struct
import sys

import xxhash

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages, relocs = struct.unpack_from("<HHH", body, 2)
reloc_at = struct.unpack_from("<H", body, 0x18)[0]
image_end = (pages - 1) * 512 + (cblp or 512)
words = []  # (file offset of the segment word, segment it names, where)
for k in range(relocs):
    off, seg = struct.unpack_from("<HH", body, reloc_at + 4 * k)
    at = header + seg * 16 + off
    words.append((at, struct.unpack_from("<H", body, at)[0] + 0x1000, "load image"))
table_offset, count = struct.unpack_from("<IH", body, image_end + 8)
rows = [struct.unpack_from("<HHHH", body, table_offset + 8 * i) for i in range(count)]
for i, (seg, _, flags, _) in enumerate(rows):
    if flags == 3:
        payload, code_size, fixup_size = struct.unpack_from("<IHH", body, header + seg * 16 + 4)
        code = image_end + 16 + payload
        for k in range(fixup_size // 2):
            rel = struct.unpack_from("<H", body, code + code_size + 2 * k)[0]
            word = struct.unpack_from("<H", body, code + rel)[0]
            named = rows[word // 8][0] + 0x1000 if word % 8 == 0 and word // 8 < len(rows) else None
            words.append((code + rel, named, f"overlay {i} code +{rel:04X}"))
print(f"segment words: {len(words)}, unreadable: {sum(1 for w in words if w[1] is None)}")
for item in sys.argv[2:]:
    seg, off = (int(p, 16) for p in item.split(":"))
    linear = seg * 16 + off
    print(f"-- {item}")
    for at, named, where in words:
        if named is None or not 0 <= linear - named * 16 <= 0xFFFF:
            continue
        want = linear - named * 16
        form = "canonical" if named == seg else "alias"
        for near in range(at - 12, at + 13):
            if near in (at - 1, at, at + 1) or near < 0:
                continue
            if struct.unpack_from("<H", body, near)[0] != want:
                continue
            kind = "pair"
            if near == at - 2 and body[at - 3] in (0x9A, 0xEA):
                kind = "far call" if body[at - 3] == 0x9A else "far jump"
            print(f"0x{at:X} {where} {form} {named:04X}:{want:04X} {kind}, offset word at {near - at:+d}")
