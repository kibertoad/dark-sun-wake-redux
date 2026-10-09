"""Resolves DSUN.EXE overlay trampolines to their entry offsets and file offsets.

Usage: python -I trampoline_target.py PATH_TO_DSUN.EXE SEGMENT:OFFSET [SEGMENT:OFFSET ...]

Checks the installed DSUN.EXE's size and XXH3-128. For each address, finds the flags-3 descriptor
whose header segment (descriptor segment + 0x1000) is SEGMENT, checks that OFFSET starts one of
its FMT-EXE-004 trampolines (0x20 + 5 * index), and prints the descriptor, the trampoline index,
its target offset and the target's file offset in the overlay's code. Nothing is written or
executed.
"""
import struct
import sys

import xxhash

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages = struct.unpack_from("<HH", body, 2)
image_end = (pages - 1) * 512 + (cblp or 512)
table_offset, count = struct.unpack_from("<IH", body, image_end + 8)
rows = [struct.unpack_from("<HHHH", body, table_offset + 8 * i) for i in range(count)]
for address in sys.argv[2:]:
    segment, offset = (int(p, 16) for p in address.split(":"))
    descriptor = next(i for i, r in enumerate(rows) if r[2] == 3 and r[0] + 0x1000 == segment)
    base = header + rows[descriptor][0] * 16
    payload_offset, code_size, _, trampolines = struct.unpack_from("<IHHH", body, base + 4)
    index, rest = divmod(offset - 0x20, 5)
    assert rest == 0 and 0 <= index < trampolines, f"{address} does not start a trampoline"
    assert body[base + offset:base + offset + 2] == b"\xcd\x3f"
    target = struct.unpack_from("<H", body, base + offset + 2)[0]
    print(f"{address}: descriptor {descriptor}, trampoline {index}, target +{target:04X},"
          f" file 0x{image_end + 16 + payload_offset + target:08X}")
