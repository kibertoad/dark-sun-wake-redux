"""Lists the memory writes reached from given functions of the ADV sound drivers in RESOURCE.GFF.

Usage: python -I adv_function_stores.py PATH_TO_RESOURCE.GFF FUNCTION [FUNCTION ...]

For each ADV resource of the installed RESOURCE.GFF (offsets and sizes below), reads the
driver's function table (the word at offset 0 points at pairs of words, function number and
entry offset, ending at function 0xFFFF; a resource whose pairs run past its end is reported as
having no table), and for each FUNCTION (hexadecimal) the driver lists,
walks its code by recursive descent from the entry: every near call, jump and conditional jump
with an immediate target is followed, and a walk stops at ret, retf, iret, an indirect jump and
undecodable bytes. Indirect calls are listed and not followed. It prints, per driver and
function, each instruction that writes memory (a memory first operand of any instruction other
than cmp, test, push and the transfers; string stores and moves), grouped as:

- frame: an operand based on BP with no segment prefix (the stack);
- cs: an operand with a CS prefix (the driver's own variables);
- other: anything else, printed with the instruction;

and every load of DS, ES or SS, and every far call or interrupt. Nothing is written or executed.
"""
import struct
import sys

from capstone import Cs, CS_ARCH_X86, CS_MODE_16
from capstone.x86 import X86_OP_IMM, X86_OP_MEM, X86_REG_BP, X86_REG_CS, X86_REG_INVALID

ADV = {1: (0x6A737, 14775), 2: (0x6E0EE, 10707), 3: (0x70AC1, 4361), 4: (0x71BCA, 15242),
       5: (0x75754, 8434), 6: (0x77846, 4667), 7: (0x78A81, 14825), 8: (0x7C46A, 4946),
       9: (0x7D7BC, 15235), 10: (0x8133F, 2720), 11: (0x81DDF, 16357), 12: (0x85DC4, 16257),
       13: (0x89D45, 5770), 17: (0x8B3CF, 10816), 18: (0x8DE0F, 5232), 19: (0x8F27F, 8792),
       20: (0x914D7, 5926)}
NOWRITE = ("cmp", "test", "push", "jmp", "call", "ljmp", "lcall")


def table(d):
    p = struct.unpack_from("<H", d, 0)[0]
    t = {}
    while True:
        if p + 4 > len(d):
            return None
        fn, o = struct.unpack_from("<HH", d, p)
        if fn == 0xFFFF:
            return t
        t[fn] = o
        p += 4


def walk(d, entry, md):
    seen, work, groups = set(), [entry], {"frame": 0, "cs": 0, "other": [], "seg": [], "far": []}
    while work:
        a = work.pop()
        while a not in seen and 0 <= a < len(d):
            seen.add(a)
            insn = next(md.disasm(d[a:a + 16], a), None)
            if insn is None:
                groups["other"].append(f"{a:04X} undecodable")
                break
            m, ops = insn.mnemonic, insn.operands
            text = f"{a:04X} {m} {insn.op_str}"
            if m.startswith(("rep", "stos", "movs")) and not m.startswith(("rep cmps", "repe cmps", "repne cmps", "repe scas", "repne scas")):
                groups["other"].append(text)
            elif ops and ops[0].type == X86_OP_MEM and m not in NOWRITE:
                mem = ops[0].mem
                if mem.segment == X86_REG_CS:
                    groups["cs"] += 1
                elif mem.segment == X86_REG_INVALID and mem.base == X86_REG_BP:
                    groups["frame"] += 1
                else:
                    groups["other"].append(text)
            elif m in ("mov", "pop", "les", "lds", "lss") and ops and (
                    m in ("les", "lds", "lss") or (ops[0].type != X86_OP_MEM and insn.reg_name(ops[0].reg) in ("ds", "es", "ss"))):
                groups["seg"].append(text)
            elif m in ("lcall", "ljmp", "int") or (m == "call" and ops and ops[0].type != X86_OP_IMM):
                groups["far"].append(text)
            if m in ("ret", "retf", "iret"):
                break
            if m.startswith("j") or m in ("call", "loop", "loope", "loopne", "jcxz"):
                op = ops[0] if ops else None
                if op is not None and op.type == X86_OP_IMM:
                    work.append(op.imm)
                elif m != "call":
                    groups["far"].append(text)
                    break
                if m == "jmp":
                    break
            a += insn.size
    return len(seen), groups


def main():
    gff, fns = sys.argv[1], [int(x, 16) for x in sys.argv[2:]]
    r = open(gff, "rb").read()
    md = Cs(CS_ARCH_X86, CS_MODE_16)
    md.detail = True
    for num, (off, size) in sorted(ADV.items()):
        d = r[off:off + size]
        t = table(d)
        if t is None:
            print(f"ADV {num}: no function table ending inside the resource")
            continue
        for fn in fns:
            if fn not in t:
                print(f"ADV {num} function {fn:02X}: absent")
                continue
            n, g = walk(d, t[fn], md)
            print(f"ADV {num} function {fn:02X} at {t[fn]:04X}: {n} instructions, {g['frame']} frame "
                  f"and {g['cs']} cs writes")
            for k in ("other", "seg", "far"):
                for line in sorted(set(g[k])):
                    print(f"  {k}: {line}")


main()
