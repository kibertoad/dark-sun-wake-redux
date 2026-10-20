"""Lists the instructions in DSUN.EXE's known routines that write SS or DS.

Usage: python -I segment_register_writes.py PATH_TO_DSUN.EXE PATH_TO_COVERAGE_TSV [ROUTINE ...]

Checks the installed DSUN.EXE's size and XXH3-128. Reads the routine table of the coverage TSV
(a resident routine's ranges as SEGMENT:OFFSET in the load image at segment 0x1000, an overlay
routine's as file offsets), decodes each range with Capstone in 16-bit mode from its start, and
prints every instruction that writes SS (mov ss, pop ss, lss) with its routine, then, for DS
(mov ds, pop ds, lds), the number of such instructions per routine. Each ROUTINE given (the
routine's start as the TSV writes it) also has its DS writes printed in full. Code outside the
TSV's routines is not read. Nothing is written or executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16


def file_offset(text):
    if text.startswith("0x"):
        return int(text, 16)
    seg, off = (int(p, 16) for p in text.split(":"))
    return header + (seg - 0x1000) * 16 + off


md = Cs(CS_ARCH_X86, CS_MODE_16)
wanted = set(sys.argv[3:])
ss_writes, ds_counts, ds_listed = [], {}, []
for line in open(sys.argv[2], encoding="utf-8"):
    parts = line.rstrip("\n").split("\t")
    if parts[0] == "start" or len(parts) < 3:
        continue
    routine = parts[0]
    for item in parts[2].split():
        low, high = item.split("..")
        start, end = file_offset(low), file_offset(high)
        for ins in md.disasm(body[start:end], start):
            ops = ins.op_str.split(",")[0].strip()
            name = ins.mnemonic
            writes_ss = (name in ("mov", "pop") and ops == "ss") or name == "lss"
            writes_ds = (name in ("mov", "pop") and ops == "ds") or name == "lds"
            text = f"0x{ins.address:08X} {name} {ins.op_str}"
            if writes_ss:
                ss_writes.append((routine, text))
            if writes_ds:
                ds_counts[routine] = ds_counts.get(routine, 0) + 1
                if routine in wanted:
                    ds_listed.append((routine, text))
print("SS writes:")
for routine, text in ss_writes:
    print(f"  {routine}: {text}")
print(f"DS writes: {sum(ds_counts.values())} in {len(ds_counts)} routines")
for routine in sorted(ds_counts):
    print(f"  {routine}: {ds_counts[routine]}")
print("DS writes in the routines asked for:")
for routine in sorted(wanted):
    rows = [text for r, text in ds_listed if r == routine]
    print(f"  {routine}: " + ("; ".join(rows) if rows else "none"))
