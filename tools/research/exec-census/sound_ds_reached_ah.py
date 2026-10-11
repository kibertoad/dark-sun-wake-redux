"""AH at each interrupt 21 site a reach report reaches, read along its decoded ranges (FND-EXE-572).

Usage: python -I sound_ds_reached_ah.py SOUND_DS_EXE REACH_REPORT_JSON

For each reached target it decodes the report's decoded range that holds the site, from the
range's start up to the site, and keeps the last immediate load of AH or AX; an instruction
in between that writes AH or AX some other way leaves the value "not immediate". Prints each
site with its AH and the counts. Nothing is executed.
"""
import json
import sys
from collections import Counter

from capstone import CS_ARCH_X86, CS_MODE_16, Cs

body = open(sys.argv[1], "rb").read()
report = json.load(open(sys.argv[2]))
md = Cs(CS_ARCH_X86, CS_MODE_16)
ranges = sorted((r["start"], r["end"]) for r in report["decodedRanges"])
values = Counter()
for t in report["targets"]:
    if not t.get("reached"):
        continue
    site = t["target"]
    start = max(a for a, e in ranges if a <= site < e)
    ah = None
    note = ""
    for i in md.disasm(body[start:site], start):
        op = i.op_str
        if i.mnemonic == "mov" and op.startswith("ah, 0x"):
            ah, note = int(op.split(", ")[1], 16), f"mov ah at {i.address:X}"
        elif i.mnemonic == "mov" and op.startswith("ax, 0x"):
            ah, note = int(op.split(", ")[1], 16) >> 8, f"mov ax at {i.address:X}"
        elif op.startswith("ah,") or op.startswith("ax,") or op in ("ax", "ah") or i.mnemonic in ("lodsw", "cwd", "mul", "div", "cbw", "pop") and op in ("", "ax"):
            ah, note = None, f"{i.mnemonic} {op} at {i.address:X}"
    key = f"{ah:02X}" if ah is not None else "not immediate"
    values[key] += 1
    print(f"{site:X} range {start:X} AH {key} ({note})")
print(sorted(values.items()))
