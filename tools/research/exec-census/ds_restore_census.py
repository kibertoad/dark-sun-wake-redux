"""Finds which of DSUN.EXE's known routines can return, or call out, with DS not as they found it.

Usage: python -I ds_restore_census.py PATH_TO_DSUN.EXE PATH_TO_COVERAGE_TSV

Checks the installed DSUN.EXE's size and XXH3-128. Reads the routine table of the coverage TSV as
segment_register_writes.py does, applies the MZ relocations for a load image at segment 0x1000,
and decodes each routine's ranges with Capstone in 16-bit mode. From the routine's start it walks
every path through direct jumps, conditional jumps and loops, keeping an abstract value of DS:
`entry` (what the caller left), `data` (the data segment 0x57E0) or `other`, the values on the
stack and in the general registers and ES. The rules are:

- `push` of DS, SS, a register or anything else pushes `entry`/`data`/`other` as the source
  holds; `pop ds` and `pop REG` take the top value; `pushf`, `pusha`, `sub sp, N` and `enter`
  push `other` words, `popf`, `popa` and `add sp, N` drop them, and `mov sp, bp` and `leave`
  drop back to the depth at `mov bp, sp` or `enter`;
- `mov REG, ds` copies the DS value; `mov REG, ss` and `mov REG, 0x57E0` (a relocated load-image
  word, or an overlay segment word whose fixup names descriptor segment 0x57E0) give `data`;
  `mov word ptr cs:[...], ds` keeps the value under that operand, and `mov ds, X` takes the
  value of X; `lds` and any other write give `other`;
- `push cs` before a near `call` is dropped (a far call made near), and so is one more word when
  the call lands on an `iret` byte (a `popf` made with `iret`); a call drops the argument words
  its target's `ret N` or `retf N` removes (the largest N in the target); after a call AX, BX, CX, DX and
  ES are `other`, and DS is `other` when the direct target is a routine found to change DS.

A routine changes DS when some path reaches `ret`, `retf` or `iret` with DS `other`. The walk is
repeated over the routines that write DS or call a routine that changes it until no new routine
is found. It prints, for each routine walked, `restores` or the returns, calls and `int`s reached
with DS `other`, the indirect and far jumps (which end a path) and the paths that leave the
routine's ranges. Far calls into overlays go through trampolines and are taken to keep DS. Code
outside the TSV's routines is not read. Nothing is written or executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs
from capstone.x86 import X86_OP_IMM, X86_OP_REG

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
DATA = 0x57E0
LIMIT = 200000
REGS = {"ax", "bx", "cx", "dx", "si", "di", "bp", "es"}

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages, relocs = struct.unpack_from("<HHH", body, 2)
reloc_at = struct.unpack_from("<H", body, 0x18)[0]
image_end = (pages - 1) * 512 + (cblp or 512)
image = bytearray(body)
for k in range(relocs):
    off, seg = struct.unpack_from("<HH", body, reloc_at + 4 * k)
    at = header + seg * 16 + off
    struct.pack_into("<H", image, at, (struct.unpack_from("<H", image, at)[0] + 0x1000) & 0xFFFF)
table_offset, count = struct.unpack_from("<IH", body, image_end + 8)
rows = [struct.unpack_from("<HHHH", body, table_offset + 8 * i) for i in range(count)]
overlays = []
for i, row in enumerate(rows):
    if row[2] == 3:
        payload_offset, code_size, fixup_size = struct.unpack_from("<IHH", body, header + row[0] * 16 + 4)
        code = image_end + 16 + payload_offset
        words = set()
        for k in range(fixup_size // 2):
            at = code + struct.unpack_from("<H", body, code + code_size + 2 * k)[0]
            words.add(at)
        overlays.append((code, code + code_size, words))


def file_offset(text):
    if text.startswith("0x"):
        return int(text, 16)
    seg, off = (int(p, 16) for p in text.split(":"))
    return header + (seg - 0x1000) * 16 + off


def loads_data(ins, start):
    """True when ins is mov REG, imm whose immediate names the data segment."""
    if ins.mnemonic != "mov" or len(ins.operands) != 2 or ins.operands[1].type != X86_OP_IMM:
        return False
    at = ins.address + ins.size - 2
    if start < image_end:
        return (ins.operands[1].imm & 0xFFFF) == DATA
    for code, end, words in overlays:
        if code <= at < end:
            word = struct.unpack_from("<H", body, at)[0]
            return at in words and word >> 3 < len(rows) and rows[word >> 3][0] + 0x1000 == DATA
    return False


def call_target(ins, start):
    """The file offset a direct near call, or a direct far call into the load image, enters."""
    if ins.mnemonic == "call" and ins.operands and ins.operands[0].type == X86_OP_IMM:
        return ins.operands[0].imm
    if ins.mnemonic == "lcall" and start < image_end and len(ins.operands) == 2:
        seg, off = ins.operands[0].imm & 0xFFFF, ins.operands[1].imm & 0xFFFF
        return header + (seg - 0x1000) * 16 + off
    return None


md = Cs(CS_ARCH_X86, CS_MODE_16)
md.detail = True
routines = []
for line in open(sys.argv[2], encoding="utf-8"):
    parts = line.rstrip("\n").split("\t")
    if parts[0] == "start" or len(parts) < 3:
        continue
    spans = []
    for item in parts[2].split():
        low, high = item.split("..")
        spans.append((file_offset(low), file_offset(high)))
    code, writes, calls = {}, 0, set()
    for start, end in sorted(spans):
        for ins in md.disasm(bytes(image[start:end]), start):
            code[ins.address] = (ins, start)
            first = ins.op_str.split(",")[0].strip()
            if (ins.mnemonic in ("mov", "pop") and first == "ds") or ins.mnemonic == "lds":
                writes += 1
            target = call_target(ins, start)
            if target is not None:
                calls.add(target)
    routines.append((parts[0], file_offset(parts[0]), code, writes, calls))
cleanup = {}
for _, entry, code, _, _ in routines:
    for ins, _ in code.values():
        if ins.mnemonic in ("ret", "retf") and ins.operands and ins.operands[0].type == X86_OP_IMM:
            cleanup[entry] = max(cleanup.get(entry, 0), (ins.operands[0].imm & 0xFFFF) // 2)


def walk(code, entry, changers):
    flagged, seen, work = set(), set(), [(entry, "entry", (), ())]
    while work and len(seen) < LIMIT:
        at, state, stack, regs = work.pop()
        if (at, state, stack, regs) in seen:
            continue
        seen.add((at, state, stack, regs))
        if at not in code:
            flagged.add(f"0x{at:08X} path leaves the routine's ranges with DS {state}")
            continue
        ins, span = code[at]
        name, ops = ins.mnemonic, ins.op_str
        first = ops.split(",")[0].strip()
        second = ops.split(",")[-1].strip() if "," in ops else ""
        reg = dict(regs)
        stack = list(stack)
        nexts = [at + ins.size]
        if name in ("pushf", "pusha"):
            stack.extend(["other"] * (1 if name == "pushf" else 8))
        elif name in ("popf", "popa"):
            del stack[len(stack) - min(len(stack), 1 if name == "popf" else 8):]
        elif name in ("add", "sub") and first == "sp" and ins.operands[1].type == X86_OP_IMM:
            words = (ins.operands[1].imm & 0xFFFF) // 2
            if name == "add":
                del stack[len(stack) - min(len(stack), words):]
            else:
                stack.extend(["other"] * min(words, 512))
        elif name == "mov" and ops == "bp, sp":
            reg["frame"] = len(stack)
        elif name == "enter":
            stack.append(reg.get("bp", "other"))
            reg["frame"] = len(stack)
            stack.extend(["other"] * min((ins.operands[0].imm & 0xFFFF) // 2, 512))
        elif (name == "mov" and ops == "sp, bp") or name == "leave":
            del stack[reg.get("frame", len(stack)):]
            if name == "leave":
                reg["bp"] = stack.pop() if stack else "other"
        elif name == "push":
            stack.append(state if first == "ds" else "data" if first == "ss"
                         else reg.get(first, "other"))
        elif name == "pop" and first == "ds":
            state = stack.pop() if stack else "other"
        elif name == "pop":
            reg[first] = stack.pop() if stack else "other"
        elif name == "mov" and second == "ds" and first.startswith("word ptr cs:["):
            reg[first] = state
        elif (name == "mov" and first == "ds") or name == "lds":
            state = reg.get(second, "other") if name == "mov" else "other"
        elif name == "mov" and first in REGS:
            reg[first] = (state if second == "ds" else "data" if second == "ss"
                          else "data" if loads_data(ins, span)
                          else reg.get(second, "other") if second in REGS else "other")
        elif name in ("ret", "retf", "iret"):
            if state == "other":
                flagged.add(f"0x{at:08X} {name} with DS other")
            nexts = []
        elif name in ("call", "lcall", "int"):
            if state == "other":
                flagged.add(f"0x{at:08X} {name} {ops} with DS other")
            if name == "call" and at - 1 in code and image[at - 1] == 0x0E and stack:
                stack.pop()
                target = call_target(ins, span)
                if target is not None and image[target] == 0xCF and stack:
                    stack.pop()
            for r in ("ax", "bx", "cx", "dx", "es"):
                reg[r] = "other"
            words = cleanup.get(call_target(ins, span), 0)
            del stack[len(stack) - min(len(stack), words):]
            if call_target(ins, span) in changers:
                state = "other"
        elif name == "jmp":
            if ins.operands[0].type == X86_OP_IMM:
                nexts = [ins.operands[0].imm]
            else:
                flagged.add(f"0x{at:08X} indirect jmp {ops} with DS {state}")
                nexts = []
        elif name == "ljmp":
            flagged.add(f"0x{at:08X} far jmp {ops} with DS {state}")
            nexts = []
        elif (name.startswith("j") or name.startswith("loop")) and ins.operands[0].type == X86_OP_IMM:
            nexts.append(ins.operands[0].imm)
        elif first in REGS and name not in ("cmp", "test"):
            reg[first] = "other"
        if name in ("xchg",) and "ds" in ops:
            state = "other"
        for n in nexts:
            work.append((n, state, tuple(stack), tuple(sorted(reg.items()))))
    if work:
        flagged.add(f"walk stopped at the limit of {LIMIT} states")
    return sorted(flagged)


changers, results, rounds = set(), {}, 0
while True:
    rounds += 1
    grown = False
    for name, entry, code, writes, calls in routines:
        if not writes and not calls & changers:
            continue
        flagged = walk(code, entry, changers)
        results[name] = (writes, flagged)
        if any(" ret" in f or "iret" in f for f in flagged) and entry not in changers:
            changers.add(entry)
            grown = True
    if not grown:
        break
written = [n for n in results if results[n][0]]
print(f"{len(written)} routines write DS, {sum(results[n][0] for n in written)} writes; "
      f"{len(results)} walked in {rounds} rounds; {len(changers)} may return with DS changed")
for n, (writes, flagged) in results.items():
    if not flagged:
        print(f"{n}: {writes} writes, restores")
for n, (writes, flagged) in results.items():
    if flagged:
        print(f"{n}: {writes} writes, flagged:")
        for text in flagged:
            print(f"    {text}")
