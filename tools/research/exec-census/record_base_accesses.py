"""Lists the ES record accesses at given displacements in DSUN.EXE with the pointer their base came from.

Usage: python -I record_base_accesses.py PATH_TO_DSUN.EXE INVENTORY_TSV [--source WORD] DISP [DISP ...]

Checks the installed DSUN.EXE's size and XXH3-128. Decodes each range of each row of
INVENTORY_TSV (resident rows as SEGMENT:OFFSET with the load image at segment 0x1000, overlay
rows as file offsets) from its start to its end, one instruction after another, as
field_reads.py does, and keeps every instruction with an ES memory operand whose base register is
set and whose displacement is one of the hexadecimal DISPs (compared as a word), whether it reads
or writes it (lea excluded).

For each it walks back through the same decoding, at most 40 instructions and never past a call,
ret, retf or jmp, to the last instruction whose first operand is the base register, skipping
`add REG, r16`. It prints the pointer word when that instruction is `les REG, ptr [0xWORD]` or
`mov REG, word ptr [0xWORD]`, with the multiplier of the last `imul r, r, IMM` among the six
instructions before that load; otherwise it prints that instruction, or "undecided" when the walk stops first. The walk is
textual and does not follow branches into the range, so a source is a lead to check in the
listing, and accesses outside the inventory or inside other instructions are not listed (use
field_reads.py and field_stores.py for those). With --source WORD (repeatable), only accesses
whose pointer word is one of the hexadecimal WORDs are printed. Nothing is written or executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs
from capstone.x86 import X86_OP_MEM, X86_OP_REG, X86_REG_ES

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
STOPS = ("call", "lcall", "ret", "retf", "jmp", "ljmp")

argv = sys.argv[1:]
sources = {int(argv[i + 1], 16) for i, a in enumerate(argv) if a == "--source"}
args = [a for i, a in enumerate(argv) if a != "--source" and (i == 0 or argv[i - 1] != "--source")]
body = open(args[0], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
disps = {int(w, 16) for w in args[2:]}
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages = struct.unpack_from("<HH", body, 2)
image_end = (pages - 1) * 512 + (cblp or 512)
table_offset, count = struct.unpack_from("<IH", body, image_end + 8)
rows = [struct.unpack_from("<HHHH", body, table_offset + 8 * i) for i in range(count)]
spans = []
for i, (seg, _, flags, _) in enumerate(rows):
    if flags == 3:
        payload, code_size = struct.unpack_from("<IH", body, header + seg * 16 + 4)
        spans.append((image_end + 16 + payload, image_end + 16 + payload + code_size, i))
md = Cs(CS_ARCH_X86, CS_MODE_16)
md.detail = True


def site(text):
    if ":" in text:
        seg, off = (int(p, 16) for p in text.split(":"))
        return header + (seg - 0x1000) * 16 + off, seg
    return int(text, 16), None


def where(offset, seg):
    if seg is None:
        found = next(((o, low) for low, high, o in spans if low <= offset < high), None)
        if found is None:
            return f"file 0x{offset:08X}"
        return f"overlay {found[0]} +{offset - found[1]:04X} (file 0x{offset:08X})"
    return f"{seg:04X}:{offset - header - (seg - 0x1000) * 16:04X} (file 0x{offset:08X})"


def access(ins):
    if ins.mnemonic == "lea":
        return None
    for k, op in enumerate(ins.operands):
        if op.type == X86_OP_MEM and op.mem.segment == X86_REG_ES and op.mem.base and (op.mem.disp & 0xFFFF) in disps:
            return op.mem.base, k
    return None


def scale(before):
    for ins in reversed(before[-6:]):
        if ins.mnemonic in STOPS:
            return None
        if ins.mnemonic == "imul" and len(ins.operands) == 3:
            return ins.operands[2].imm
    return None


def source(run, base):
    window = run[-40:]
    for back, ins in enumerate(reversed(window)):
        if ins.mnemonic in STOPS:
            return "undecided", None
        multiplier = scale(window[:len(window) - back])
        if not ins.operands or ins.operands[0].type != X86_OP_REG or ins.operands[0].reg != base:
            continue
        if ins.mnemonic == "add" and ins.operands[1].type == X86_OP_REG:
            continue
        second = ins.operands[1] if len(ins.operands) > 1 else None
        if ins.mnemonic in ("les", "mov") and second is not None and second.type == X86_OP_MEM \
                and not second.mem.base and not second.mem.index:
            return f"[{second.mem.disp & 0xFFFF:04x}]", multiplier
        return f"{ins.mnemonic} {ins.op_str}", multiplier
    return "undecided", None


for line in open(args[1], encoding="utf-8").read().splitlines()[1:]:
    start, _, ranges = line.split("\t")
    for item in ranges.split():
        low, high = item.split("..")
        lo, seg = site(low)
        hi, _ = site(high)
        run = []
        for ins in md.disasm(body[lo:hi], lo):
            found = access(ins)
            if found:
                origin, multiplier = source(run, found[0])
                word = int(origin[1:5], 16) if origin.startswith("[") else None
                if not sources or word in sources:
                    kind = "write" if found[1] == 0 and ins.mnemonic not in ("cmp", "test", "push") else "read"
                    times = f" x{multiplier}" if multiplier is not None else ""
                    print(f"{where(ins.address, seg)} in {start}: {kind} {ins.mnemonic} {ins.op_str} <- {origin}{times}")
            run.append(ins)
