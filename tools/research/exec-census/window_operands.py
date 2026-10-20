"""Lists DSUN.EXE instructions whose memory displacement or immediate falls in given data windows.

Usage: python -I window_operands.py PATH_TO_DSUN.EXE PATH_TO_COVERAGE_TSV|REACH_REPORT_JSON
       [--extra LOW..HIGH ...] WINDOW [WINDOW ...]

A WINDOW is LOW..HIGH in hexadecimal, half-open, a range of data-segment offsets. Checks the
installed DSUN.EXE's size and XXH3-128. Reads the routine table of the coverage TSV as
segment_register_writes.py does, or, for a file ending in `.json`, the `decodedRanges` of a
scientific-method-engine `reach` report (file offsets of the runs of instructions its walk
decoded, each with its routine), adds each `--extra` range (SEGMENT:OFFSET..SEGMENT:OFFSET or file
offsets 0xHEX..0xHEX, a routine whose entry a finding establishes but the TSV lacks), and decodes
each range linearly from its start with Capstone in 16-bit mode. For every decoded instruction it
prints each operand whose value lies in a window:

- `mem`: a memory operand whose encoded displacement (taken as a word) is in the window, with its
  base and index registers, the segment it goes through (an override prefix, or SS for a BP base
  and DS otherwise), and whether the instruction reads it, writes it or both, from Capstone's
  operand access flags (`address` for `lea`);
- `imm`: an immediate operand whose word value is in the window, other than the target of a
  call, jump or loop, and other than a word that an MZ relocation or an overlay fixup adjusts (a
  segment); the rest are pointers formed as literals or constants that happen to fall there, and
  the listing does not tell them apart.

Lines are grouped by window, each with its routine (the range start) and its place: a resident
address in its routine's segment (load image at segment 0x1000) or `overlay N +OFFSET` for overlay
code (flags-3 descriptors, FMT-EXE-002/FMT-EXE-003). String instructions (`stos`, `movs`, `rep`
forms), whose destination is in registers, are counted per routine for the routines with a hit.
A second pass decodes at every byte of the load image and of every overlay's code and prints, as
candidates, the hits that are not instruction starts of the first pass, marked `inside` when the
byte lies within an instruction the first pass decoded and `outside` when no listed range covers
it; neither kind is an established instruction. Nothing is written or executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs
from capstone.x86 import X86_OP_IMM, X86_OP_MEM

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
BRANCH = {"call", "lcall", "jmp", "ljmp", "loop", "loope", "loopne", "jcxz"}

args = sys.argv[3:]
extras, windows = [], []
while args:
    text = args.pop(0)
    if text == "--extra":
        extras.append(args.pop(0))
    else:
        low, high = (int(p, 16) for p in text.split(".."))
        windows.append((low, high))
body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages = struct.unpack_from("<HH", body, 2)
image_end = (pages - 1) * 512 + (cblp or 512)
table_offset, count = struct.unpack_from("<IH", body, image_end + 8)
overlays = []
relocated = set()
relocs, reloc_at = struct.unpack_from("<H", body, 6)[0], struct.unpack_from("<H", body, 0x18)[0]
for k in range(relocs):
    off, seg = struct.unpack_from("<HH", body, reloc_at + 4 * k)
    relocated.add(header + seg * 16 + off)
for i in range(count):
    seg, _, flags, _ = struct.unpack_from("<HHHH", body, table_offset + 8 * i)
    if flags == 3:
        payload_offset, code_size, fixup_size = struct.unpack_from("<IHH", body, header + seg * 16 + 4)
        start = image_end + 16 + payload_offset
        overlays.append((i, start, start + code_size))
        for k in range(fixup_size // 2):
            relocated.add(start + struct.unpack_from("<H", body, start + code_size + 2 * k)[0])


def file_offset(text):
    if text.startswith("0x"):
        return int(text, 16)
    seg, off = (int(p, 16) for p in text.split(":"))
    return header + (seg - 0x1000) * 16 + off


SEGMENTS = []
if sys.argv[2].endswith(".json"):
    import json
    SEGMENTS = [(r["start"], r["end"], r["segment"])
                for r in json.load(open(sys.argv[2], encoding="utf-8"))["declaredRegions"] if r["segment"] < 0x9000]


def place(at, routine=None):
    if header <= at < image_end:
        seg = int(routine.split(":")[0], 16) if routine and ":" in routine else 0x1000 + (at - header) // 16
        for low, high, segment in SEGMENTS:
            if low <= at < high:
                seg = segment
        return f"{seg:04X}:{at - header - (seg - 0x1000) * 16:04X}"
    for i, start, end in overlays:
        if start <= at < end:
            return f"overlay {i} +{at - start:04X}"
    return f"file 0x{at:08X}"


def window_of(value):
    for w in windows:
        if w[0] <= value < w[1]:
            return w
    return None


md = Cs(CS_ARCH_X86, CS_MODE_16)
md.detail = True


def hits(ins):
    out = []
    for o in ins.operands:
        if o.type == X86_OP_MEM and ins.disp_size:
            value = o.mem.disp & 0xFFFF
            w = window_of(value)
            if w is None:
                continue
            base = ins.reg_name(o.mem.base) if o.mem.base else ""
            index = ins.reg_name(o.mem.index) if o.mem.index else ""
            seg = ins.reg_name(o.mem.segment) if o.mem.segment else ("ss" if base == "bp" else "ds")
            access = {1: "read", 2: "write", 3: "read+write"}.get(o.access, "address")
            if ins.mnemonic == "lea":
                access = "address"
            regs = "+".join(r for r in (base, index) if r) or "none"
            out.append((w, f"mem {access} {seg}:{value:04X} regs={regs}"))
        elif o.type == X86_OP_IMM and ins.imm_size and ins.mnemonic not in BRANCH \
                and not ins.mnemonic.startswith("j"):
            value = o.imm & 0xFFFF
            w = window_of(value)
            if w is not None and ins.address + ins.size - ins.imm_size not in relocated:
                out.append((w, f"imm {value:04X}"))
    return out


ranges = []
if sys.argv[2].endswith(".json"):
    import json
    for r in json.load(open(sys.argv[2], encoding="utf-8"))["decodedRanges"]:
        ranges.append((place(r["routine"]), r["start"], r["end"]))
else:
    for line in open(sys.argv[2], encoding="utf-8"):
        parts = line.rstrip("\n").split("\t")
        if parts[0] == "start" or len(parts) < 3:
            continue
        for item in parts[2].split():
            low, high = item.split("..")
            ranges.append((parts[0], file_offset(low), file_offset(high)))
for item in extras:
    low, high = item.split("..")
    ranges.append((low, file_offset(low), file_offset(high)))

found = {w: [] for w in windows}
strings = {}
starts, covered, listed = set(), set(), set()
for routine, start, end in ranges:
    listed.update(range(start, end))
    for ins in md.disasm(body[start:end], start):
        starts.add(ins.address)
        covered.update(range(ins.address, ins.address + ins.size))
        name = ins.mnemonic.split()[-1]
        if name.startswith(("stos", "movs")):
            strings[routine] = strings.get(routine, 0) + 1
        for w, text in hits(ins):
            found[w].append((routine, f"  {place(ins.address, routine)} [{routine}] {ins.mnemonic} {ins.op_str}  -- {text}"))
candidates = {w: [] for w in windows}
for start, end in [(header, image_end)] + [(s, e) for _, s, e in overlays]:
    for at in range(start, end):
        if at in starts:
            continue
        ins = next(md.disasm(body[at:min(at + 15, end)], at), None)
        if ins is None:
            continue
        kind = "inside" if at in covered else ("listed" if at in listed else "outside")
        for w, text in hits(ins):
            candidates[w].append(f"  {kind} {place(at)} {ins.mnemonic} {ins.op_str}  -- {text}")
for w in windows:
    print(f"Window {w[0]:04X}..{w[1]:04X}: {len(found[w])} operands in listed routines")
    for _, row in found[w]:
        print(row)
    routines = sorted({r for r, _ in found[w]})
    print("  string stores in those routines: " + ", ".join(f"{r} {strings.get(r, 0)}" for r in routines))
    print(f"Window {w[0]:04X}..{w[1]:04X}: {len(candidates[w])} candidates outside instruction starts")
    for row in candidates[w]:
        print(row)
