"""Lists DSUN.EXE inventory rows whose bodies run through FBOV trampolines, and near transfers to given addresses.

Usage: python -I inventory_boundaries.py PATH_TO_DSUN.EXE INVENTORY_TSV [--walk WALK_TSV] [SEGMENT:OFFSET ...]

Checks the installed DSUN.EXE's size and XXH3-128 and reads the function inventory
(coverage/<build>/DSUN.EXE.tsv). Part 1 finds every FMT-EXE-004 trampoline: for each flags-3
descriptor of the FBOV segment table, the header at the descriptor's segment plus 0x1000 holds
`trampolines` five-byte stubs from offset 0x20. It then prints every inventory row whose resident
ranges cover a trampoline byte, as "exact" when the row is one trampoline's five bytes and nothing
else, "short" when the row starts a trampoline and covers only some of its five bytes, "long" when
the row starts a trampoline and its ranges cover more, and "inside" when the row
starts at a byte that is not a trampoline's first, followed by the count of each kind, the
trampolines no row starts and those no row covers. Addresses are compared as linear load-image addresses, so 5664:0060 and
566A:0000 are the same byte.

Part 2, for each SEGMENT:OFFSET given, scans every byte of the inventory's resident rows whose
segment is SEGMENT for a near call or jump (E8 or E9 with a 16-bit displacement, EB, 70..7F or
E0..E3 with an 8-bit one, 0F 80..8F with a 16-bit one) whose target is OFFSET in that segment, and
prints each hit with "start" when the linear decode of the row that holds it from the row's start
begins an instruction there, or "inside" when it does not. A hit is a byte pattern; a "start" is an
instruction of a linear decode, not proof that execution reaches it. Far calls and stored pointers
are covered by direct_callers.py and far_pointer_words.py.

Part 3, with --walk and a routine table walk_routine_table.py wrote, prints each walk routine whose
start is not the start of an inventory row, as "inside" when an inventory range holds its first
byte and "none" when none does, with its size, and then the number of walk routines of each kind
and of those that start a row. Resident and overlay starts are compared as file offsets. Nothing
is written or executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages = struct.unpack_from("<HH", body, 2)
image_end = (pages - 1) * 512 + (cblp or 512)
table_offset, count = struct.unpack_from("<IH", body, image_end + 8)
descriptors = [struct.unpack_from("<HHHH", body, table_offset + 8 * i) for i in range(count)]


def linear(text):
    segment, offset = (int(p, 16) for p in text.split(":"))
    return segment * 16 + offset


args = sys.argv[3:]
walk = None
if "--walk" in args:
    k = args.index("--walk")
    walk = args[k + 1]
    del args[k:k + 2]


def file_offset(text):
    return int(text, 16) if ":" not in text else linear(text) - 0x10000 + header


all_starts, all_bytes = set(), set()
for line in open(sys.argv[2], encoding="utf-8").read().splitlines()[1:]:
    start, size, ranges = line.split("	")
    all_starts.add(file_offset(start))
    for r in ranges.split():
        a, b = (file_offset(x) for x in r.split(".."))
        all_bytes.update(range(a, b))

rows = []
for line in open(sys.argv[2], encoding="utf-8").read().splitlines()[1:]:
    start, size, ranges = line.split("\t")
    if ":" not in start:
        continue
    spans = []
    for r in ranges.split():
        a, b = r.split("..")
        spans.append((linear(a), linear(b)))
    rows.append((start, int(size), spans))

stubs = {}
for index, (segment, _, flags, _) in enumerate(descriptors):
    if flags != 3:
        continue
    base = header + segment * 16
    trampolines = struct.unpack_from("<H", body, base + 12)[0]
    for i in range(trampolines):
        at = (segment + 0x1000) * 16 + 0x20 + 5 * i
        assert body[at - 0x10000 + header:at - 0x10000 + header + 2] == b"\xcd\x3f"
        stubs[at] = (index, i)
stub_bytes = {at + k: at for at in stubs for k in range(5)}

kinds = {"exact": 0, "short": 0, "long": 0, "inside": 0}
covered_any = set()
started = set()
for start, size, spans in rows:
    covered = sorted({stub_bytes[b] for a, e in spans for b in range(a, e) if b in stub_bytes})
    if not covered:
        continue
    covered_any.update(covered)
    first = linear(start)
    if first in stubs:
        started.add(first)
        if spans == [(first, first + 5)]:
            kind = "exact"
        elif all(first <= a and e <= first + 5 for a, e in spans):
            kind = "short"
        else:
            kind = "long"
    else:
        kind = "inside"
    kinds[kind] += 1
    if kind != "exact":
        print(f"{kind} {start} size {size}: {len(covered)} trampolines covered, ranges "
              + " ".join(f"{a:05X}..{e:05X}" for a, e in spans))
print("trampolines", len(stubs), "rows", kinds, "not started by a row", len(set(stubs) - started),
      "in no row", len(set(stubs) - covered_any))

md = Cs(CS_ARCH_X86, CS_MODE_16)
for target in args:
    segment, offset = (int(p, 16) for p in target.split(":"))
    hits = 0
    for start, _, spans in rows:
        if int(start.split(":")[0], 16) != segment:
            continue
        starts = set()
        for a, e in spans:
            code = body[a - 0x10000 + header:e - 0x10000 + header]
            starts.update(a + i.address for i in md.disasm(code, 0))
        for a, e in spans:
            for at in range(a, e):
                f = at - 0x10000 + header
                op = body[f]
                ip = at - segment * 16
                if op in (0xE8, 0xE9):
                    dest = (ip + 3 + struct.unpack_from("<h", body, f + 1)[0]) & 0xFFFF
                elif op == 0xEB or 0x70 <= op <= 0x7F or 0xE0 <= op <= 0xE3:
                    dest = (ip + 2 + struct.unpack_from("<b", body, f + 1)[0]) & 0xFFFF
                elif op == 0x0F and 0x80 <= body[f + 1] <= 0x8F:
                    dest = (ip + 4 + struct.unpack_from("<h", body, f + 2)[0]) & 0xFFFF
                else:
                    continue
                if dest == offset:
                    hits += 1
                    print(f"{target}: {op:02X} at {segment:04X}:{ip:04X} (row {start},"
                          f" file 0x{f:08X}) {'start' if at in starts else 'inside'}")
    print(f"{target}: {hits} near transfer patterns")

if walk:
    counts = {"row": 0, "inside": 0, "none": 0}
    for line in open(walk, encoding="utf-8").read().splitlines()[1:]:
        start, size, _ = line.split("	")
        at = file_offset(start)
        kind = "row" if at in all_starts else "inside" if at in all_bytes else "none"
        counts[kind] += 1
        if kind != "row":
            print(f"walk {kind} {start} size {size}")
    print("walk routines", counts)
