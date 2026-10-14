"""Lists the instructions of DSUN.EXE that write through ES at displacement 0x14 of a base register.

Usage: python -I offset14_stores.py PATH_TO_DSUN.EXE

Searches the whole file for an ES override followed by a write opcode (C6, 88, FE, 80, 00, 28,
08, 20, 30, 86, C7, 89, FF, 83, 81), a ModRM byte with mod 01 (0x40 to 0x7F) and the displacement
byte 0x14, decodes the instruction at each hit with Capstone and keeps those whose first operand
is ES memory at `+ 0x14]` and whose mnemonic is not cmp, test or push. For each it prints the file
offset, the instruction, whether the 80 bytes before it hold the bytes C9 19 (the address DS:19C9
of the combatant table), and the last far-pointer load (`les`) among the instructions decoded from
up to 80 bytes before it. A hit is a candidate: the search does not show that an established entry
reaches it, and it misses writes with a 16-bit displacement, string stores and block copies.
Nothing is written or executed.
"""
import re
import sys

import capstone

data = open(sys.argv[1], "rb").read()
md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_16)
pattern = re.compile(rb"\x26[\xC6\x88\xFE\x80\x00\x28\x08\x20\x30\x86\xC7\x89\xFF\x83\x81][\x40-\x7F]\x14")
count = 0
for match in pattern.finditer(data):
    start = match.start()
    ins = next(md.disasm(data[start:start + 8], start, 1), None)
    if ins is None or ins.mnemonic in ("cmp", "test", "push"):
        continue
    first = ins.op_str.split(",")[0]
    if "es:[" not in first or "+ 0x14]" not in first:
        continue
    near = b"\xc9\x19" in data[max(0, start - 80):start]
    load = "none"
    for back in range(80, 0, -1):
        run = list(md.disasm(data[start - back:start + ins.size], start - back))
        if run and run[-1].address == start:
            loads = [f"{i.mnemonic} {i.op_str}" for i in run[:-1] if i.mnemonic == "les"]
            load = loads[-1] if loads else "none"
            break
    count += 1
    print(f"0x{start:05X} {ins.mnemonic} {ins.op_str} | DS:19C9 before: {'yes' if near else 'no'} | last les: {load}")
print(count, "candidates")
