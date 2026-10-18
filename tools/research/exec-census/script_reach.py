"""Follows script control flow in a `script_decode.py --all` listing and reports which instructions
of given opcodes each entry point can reach.

Usage: python -I script_reach.py LISTING OPCODE[,OPCODE...] (--registrations OP[,OP...] | ENTRY ...)

LISTING is the output of `script_decode.py GPLDATA.GFF OBJEX.GFF --all`. OPCODE values are
hexadecimal. ENTRY is TAG/NUMBER@OFFSET with a decimal offset (`GPL/142@95`). With
`--registrations 1B,1C` the entries are the code offsets (operand 0) in the `GPL ` resources
(operand 1) that every instruction of those opcodes names, as the trigger registrations of opcodes
0x1B and 0x1C take them (FND-PARTY-240); an instruction whose two operands are not both literal
is reported and not followed.

From each entry the walk takes every instruction it can reach under RULE-SCRIPT-003: the next
instruction after any instruction other than 0x12 and 0x64 (jumps), 0x15 (local return), 0x19
(return to the calling script), 0x31 and the fifteen stop opcodes of FND-SCRIPT-005; the literal
target of 0x12, 0x13, 0x29, 0x3E, 0x3F, 0x63 and 0x64 and the second operand of 0x27; every row
target of a 0x48 menu; and, for 0x14, the literal offset (operand 0) in the `GPL ` resource
(operand 1) it calls, as a new entry. A 0x13 local call also continues at the next instruction,
as if its routine returned. Nested instructions inside expressions are read for the opcodes only.
Every transfer whose target is not a literal, or lands on no instruction start, is reported, and
each hit is printed with one route to it from each entry that reaches it, latest instruction first. The
walk over-approximates what runs: it takes both ways at every condition. Nothing is executed.
"""
import re
import sys
from collections import deque

STOPS = {0x15, 0x19, 0x31, 0x26, 0x4A, 0x4C, 0x4D, 0x4E, 0x53, 0x55, 0x56, 0x57, 0x60, 0x71, 0x72,
         0x73, 0x74, 0x75}
JUMPS = {0x12: 0, 0x13: 0, 0x29: 0, 0x3E: 0, 0x3F: 0, 0x63: 0, 0x64: 0, 0x27: 1}
LINE = re.compile(r"^(\w+/\d+) ([0-9A-F]{4}): ([0-9A-F]{2})(?: (.*))?$")


def operands(text):
    """Top-level comma-separated operands of an instruction's text."""
    out, depth, current = [], 0, ""
    for ch in text or "":
        if ch in "({":
            depth += 1
        elif ch in ")}":
            depth -= 1
        if ch == "," and depth == 0:
            out.append(current.strip())
            current = ""
        else:
            current += ch
    if current.strip():
        out.append(current.strip())
    return out


def literal(text):
    return int(text) if re.fullmatch(r"-?\d+", text or "") else None


scripts = {}
for line in open(sys.argv[1], encoding="utf-8", errors="replace"):
    m = LINE.match(line.rstrip("\n"))
    if m:
        scripts.setdefault(m.group(1), []).append((int(m.group(2), 16), int(m.group(3), 16), m.group(4) or ""))
index = {name: {off: i for i, (off, _, _) in enumerate(code)} for name, code in scripts.items()}
wanted = {int(v, 16) for v in sys.argv[2].split(",")}

entries, problems = [], []
if sys.argv[3] == "--registrations":
    kinds = {int(v, 16) for v in sys.argv[4].split(",")}
    for name, code in scripts.items():
        for off, op, text in code:
            if op in kinds:
                ops = operands(text)
                target, number = literal(ops[0]), literal(ops[1])
                if target is None or number is None:
                    problems.append(f"registration {name} {off:04X} {op:02X} {text}: operands not literal")
                else:
                    entries.append((f"GPL/{number}", target, f"{name} {off:04X} {op:02X} {text}"))
else:
    for spec in sys.argv[3:]:
        name, off = spec.split("@")
        entries.append((name, int(off), spec))

hits = {}


def follow(queue, parent, node, source):
    if node not in parent:
        parent[node] = source
        queue.append(node)


for name, start, label in entries:
    parent, queue = {(name, start): None}, deque([(name, start)])
    while queue:
        script, off = queue.popleft()
        if script not in index or off not in index[script]:
            problems.append(f"{label}: {script}@{off} is no instruction start")
            continue
        i = index[script][off]
        at, op, text = scripts[script][i]
        nested = [int(x, 16) for x in re.findall(r"\{[0-9A-F]{4}: ([0-9A-F]{2})", text)]
        for o in [op] + nested:
            if o in wanted:
                if (script, at, o, text) not in hits or label not in hits[(script, at, o, text)]:
                    route, node = [], (script, off)
                    while node is not None:
                        route.append(f"{node[0]}@{node[1]:04X}")
                        node = parent[node]
                    hits.setdefault((script, at, o, text), {})[label] = " < ".join(route)
        ops = operands(text)
        if op in JUMPS:
            target = literal(ops[JUMPS[op]]) if len(ops) > JUMPS[op] else None
            if target is None:
                problems.append(f"{label}: {script} {at:04X} {op:02X} {text}: target not literal")
            else:
                follow(queue, parent, (script, target), (script, off))
        if op == 0x48:
            for row in re.findall(r"row\((.*?)\)(?:;|$)", text):
                parts = operands(row)
                target = literal(parts[1]) if len(parts) > 1 else None
                if target is None:
                    problems.append(f"{label}: {script} {at:04X} menu row {row}: target not literal")
                else:
                    follow(queue, parent, (script, target), (script, off))
        if op == 0x14:
            target, number = (literal(ops[0]), literal(ops[1])) if len(ops) == 2 else (None, None)
            if target is None or number is None:
                problems.append(f"{label}: {script} {at:04X} 14 {text}: call not literal")
            else:
                follow(queue, parent, (f"GPL/{number}", target), (script, off))
        if op not in STOPS and op not in (0x12, 0x64) and i + 1 < len(scripts[script]):
            follow(queue, parent, (script, scripts[script][i + 1][0]), (script, off))

print(f"{len(entries)} entries")
for (script, at, op, text), labels in sorted(hits.items()):
    print(f"HIT {script} {at:04X} {op:02X} {text}  from {len(labels)} entries:")
    for label in sorted(labels):
        print(f"    {label}")
        print(f"      route (latest first): {labels[label]}")
for problem in sorted(set(problems)):
    print("PROBLEM", problem)
