"""Disassembles a range of one DSUN.EXE overlay's code, resolving its far calls through the fixups.

Usage: python -I overlay_listing.py PATH_TO_DSUN.EXE DESCRIPTOR START END

Checks the installed DSUN.EXE's size and XXH3-128. Finds the overlay of segment-table descriptor
DESCRIPTOR (FMT-EXE-002/FMT-EXE-003), and disassembles file offsets START..END (hexadecimal, inside
its code) as 16-bit x86 from START. For each instruction whose segment word is in the overlay's
fixup list (FMT-EXE-005), it prints the descriptor the word names (word / 8) and the segment that
gives (descriptor segment + 0x1000). Prints the strings at the near offsets pushed or moved as
immediates when they fall in the data segment 57E0 and hold printable text. Nothing is written or
executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
DS = 0x57E0

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
want, start, end = int(sys.argv[2]), int(sys.argv[3], 16), int(sys.argv[4], 16)
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages = struct.unpack_from("<HH", body, 2)
image_end = (pages - 1) * 512 + (cblp or 512)
table_offset, count = struct.unpack_from("<IH", body, image_end + 8)
rows = [struct.unpack_from("<HHHH", body, table_offset + 8 * i) for i in range(count)]
seg = rows[want][0]
payload_offset, code_size, fixup_size = struct.unpack_from("<IHH", body, header + seg * 16 + 4)
code = image_end + 16 + payload_offset
assert code <= start < end <= code + code_size, "range outside the overlay's code"
fixups = {struct.unpack_from("<H", body, code + code_size + 2 * k)[0] for k in range(fixup_size // 2)}
md = Cs(CS_ARCH_X86, CS_MODE_16)
for ins in md.disasm(body[start:end], start):
    note = ""
    for k in range(ins.size - 1):
        if ins.address + k - code in fixups:
            word = struct.unpack_from("<H", body, ins.address + k)[0]
            note = f"  ; segment word {word:#06x}: descriptor {word >> 3}, segment {rows[word >> 3][0] + 0x1000:04X}"
    if ins.mnemonic in ("push", "mov") and ins.op_str.split(", ")[-1].startswith("0x"):
        value = int(ins.op_str.split(", ")[-1], 16)
        text = body[header + (DS - 0x1000) * 16 + value:][:40].split(b"\0")[0]
        if len(text) >= 4 and all(32 <= c < 127 for c in text):
            note += f"  ; DS:{value:04X} {text.decode()!r}"
    print(f"0x{ins.address:08X} +{ins.address - code:04X}  {ins.bytes.hex():<12} {ins.mnemonic} {ins.op_str}{note}")
