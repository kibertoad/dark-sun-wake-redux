"""Lists transfer kinds in the disc's Miles .ADV drivers that a direct interrupt census misses.

Usage: python -I disc_adv_transfer_scan.py PATH_TO_game.gog PATH_TO_BLD-GOG-EN-1.1.files.yaml

For each CD:*.ADV file (identity checked against the manifest), reports every instruction start
that decodes the same way from at least 20 of the 48 preceding start offsets and is: a far call or
jump through memory (FF /3, FF /5), a far call or jump with an immediate target (9A, EA), an
`iret`, or an instruction with an immediate whose bytes include CD (a possible interrupt opcode
store). Importing disc_adv_exec_census prints its census first. Reads the disc image in memory
only.
"""
import re
import sys
from collections import Counter

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

sys.path.insert(0, __file__.rsplit("\\", 1)[0].rsplit("/", 1)[0])
from disc_adv_exec_census import expected, files  # noqa: E402  (reads the disc on import)

md = Cs(CS_ARCH_X86, CS_MODE_16)


def aligned(body, p, size):
    votes = 0
    for back in range(1, 49):
        s = p - back
        if s < 0:
            break
        chain = list(md.disasm(body[s:p + size], s))
        if chain and chain[-1].address == p:
            votes += 1
    return votes >= 20


for path in sorted(files):
    body = files[path]
    if (len(body), xxhash.xxh3_128_hexdigest(body)) != expected.get(path):
        print(f"{path}: identity mismatch")
        continue
    kinds = Counter()
    notes = []
    for p in range(len(body)):
        i = next(md.disasm(body[p:p + 16], p), None)
        if i is None:
            continue
        b = i.bytes
        kind = None
        if b[0] == 0xFF and len(b) > 1 and ((b[1] >> 3) & 7) in (3, 5) and (b[1] >> 6) != 3:
            kind = "far-indirect"
        elif b[0] in (0x2E, 0x26, 0x36, 0x3E) and len(b) > 2 and b[1] == 0xFF and ((b[2] >> 3) & 7) in (3, 5):
            kind = "far-indirect"
        elif b[0] in (0x9A, 0xEA):
            kind = "far-immediate"
        elif i.mnemonic == "iret":
            kind = "iret"
        elif i.mnemonic in ("mov", "push", "or", "xor", "add") and re.search(
                r"0x(cd|cd[0-9a-f]{2}|[0-9a-f]{1,2}cd)$", i.op_str.split(",")[-1].strip()):
            kind = "imm-CD"
        if kind and aligned(body, p, i.size):
            kinds[kind] += 1
            notes.append(f"0x{p:04X} {kind}: {i.mnemonic} {i.op_str}")
    print(f"{path}: {dict(kinds)}")
    for n in notes:
        print("   ", n)
