"""Q-CONFIG-014: bounded literal table/count census for installed ADV 11.

Usage: python -I adv_literal_operands.py RESOURCE.GFF
       python -I adv_literal_operands.py --self-test

Validates the whole archive and selected resource. Walks direct near transfers
from first-match function-table entries; retains unresolved transfers and every
raw-offset candidate separately. This is structural decoding, not execution or
proof of native reachability. Computed addresses and implicit operands are not
covered. JSON goes to stdout; retain it under GAME_DIR, never in Git.
"""
import json
import struct
import sys
from pathlib import Path

import xxhash
import capstone
from capstone import Cs, CS_ARCH_X86, CS_MODE_16
from capstone.x86 import X86_OP_IMM, X86_OP_MEM

ARCHIVE = (5724669, "92fe031616f36062f99484b0f728210e")
RESOURCE = (0x81DDF, 16357, "0faa31676d7d993a393da11bde9e7885")
LOW, HIGH = 0x293B, 0x295D


def table(data):
    if len(data) < 2:
        raise ValueError("truncated table pointer")
    cursor = struct.unpack_from("<H", data)[0]
    first, rows = {}, []
    while cursor + 4 <= len(data):
        number, entry = struct.unpack_from("<HH", data, cursor)
        if number == 0xFFFF:
            return first, rows
        if entry >= len(data):
            raise ValueError("table entry outside resource")
        rows.append({"number": number, "entry": entry,
                     "shadowed": number in first})
        first.setdefault(number, entry)
        cursor += 4
    raise ValueError("unterminated function table")


def decoder():
    result = Cs(CS_ARCH_X86, CS_MODE_16)
    result.detail = True
    return result


def hits(ins, low=LOW, high=HIGH):
    result = []
    for operand in ins.operands:
        if operand.type == X86_OP_MEM and ins.disp_size:
            value = operand.mem.disp & 0xFFFF
            # Include encoded access extents beginning before the window.
            # A base/index makes the actual runtime extent unknown.
            if value < high and value + operand.size > low:
                result.append({"kind": "memory", "displacement": value,
                               "width": operand.size, "access": operand.access,
                               "segment": ins.reg_name(operand.mem.segment) or
                               ("ss" if ins.reg_name(operand.mem.base) == "bp" else "ds"),
                               "base": ins.reg_name(operand.mem.base),
                               "index": ins.reg_name(operand.mem.index)})
        elif operand.type == X86_OP_IMM and ins.imm_size:
            if not ins.mnemonic.startswith(("j", "call", "lcall", "loop")):
                value = operand.imm & 0xFFFF
                if low <= value < high:
                    result.append({"kind": "immediate", "value": value})
    return result


def census(data, roots):
    md = decoder()
    decoded, unresolved, work = {}, [], list(roots)
    while work:
        at = work.pop()
        while at not in decoded:
            if not 0 <= at < len(data):
                unresolved.append({"site": at, "reason": "outside resource"})
                break
            ins = next(md.disasm(data[at:at + 15], at), None)
            if ins is None:
                unresolved.append({"site": at, "reason": "undecodable"})
                break
            decoded[at] = ins
            m = ins.mnemonic
            if m in ("ret", "retf", "iret"):
                break
            if m in ("lcall", "ljmp", "int", "int3", "into"):
                unresolved.append({"site": at, "reason": m})
                if m == "ljmp":
                    break
            elif m.startswith(("j", "loop")) or m == "call":
                target = ins.operands[0] if ins.operands else None
                if target is not None and target.type == X86_OP_IMM:
                    work.append(target.imm & 0xFFFF)
                else:
                    unresolved.append({"site": at, "reason": "indirect " + m})
                if m == "jmp":
                    break
            at += ins.size
    covered = set()
    for at, ins in decoded.items():
        covered.update(range(at, at + ins.size))
    owned, candidates = [], []
    for at in range(len(data)):
        ins = decoded.get(at) or next(md.disasm(data[at:at + 15], at), None)
        if ins is None:
            continue
        operands = hits(ins)
        if not operands:
            continue
        row = {"site": at, "size": ins.size, "mnemonic": ins.mnemonic,
               "operands": operands}
        if at in decoded:
            owned.append(row)
        else:
            row["classification"] = "overlap" if at in covered else "unowned"
            candidates.append(row)
    return {"structuralInstructionCount": len(decoded),
            "structuralByteCount": len(covered), "operands": owned,
            "rawCandidates": candidates, "unresolvedTransfers": unresolved,
            "negativeUsable": False,
            "exclusions": ["computed addresses without matching encoded displacement",
                           "implicit operands and block-copy destinations",
                           "physical aliases and changed runtime segments/code",
                           "unresolved transfers, external and asynchronous effects",
                           "instruction ownership is not native reachability"]}


def controls(report):
    expected = {0x38A7: ("mov", 0x293D, 2),
                0x3973: ("inc", 0x295B, 3),
                0x39C4: ("mov", 0x293D, 2),
                0x39CB: ("dec", 0x295B, 3),
                0x33AB: ("mov", 0x295B, 1)}
    rows = {row["site"]: row for row in report["operands"]}
    for site, (mnemonic, displacement, access) in expected.items():
        row = rows.get(site)
        if row is None or row["mnemonic"] != mnemonic or not any(
                op.get("displacement") == displacement and op.get("access") == access
                and op.get("segment") == "cs" for op in row["operands"]):
            raise ValueError(f"independent operand control failed at {site:04X}")
    return len(expected)


def self_test():
    # A near call enters an IRET offcut inside another linear instruction.
    report = census(bytes.fromhex("e8010080cf00cb"), [0])
    assert report["structuralInstructionCount"] == 4
    # Typed indexed CS write, immediate address-taking, and an overlapping extent.
    md = decoder()
    a = next(md.disasm(bytes.fromhex("2e89473d"), 0))
    assert hits(a, 0x3D, 0x3F)[0]["base"] == "bx"
    b = next(md.disasm(bytes.fromhex("b85b29"), 0))
    assert hits(b)[0]["kind"] == "immediate"
    c = next(md.disasm(bytes.fromhex("2e66a35a29"), 0))
    assert hits(c)[0]["width"] == 4
    assert hits(c, 0x295D, 0x295E)
    # A literal in the interior of another instruction remains a raw candidate.
    overlap = census(bytes.fromhex("b866a15b29cb"), [0])
    assert any(r["classification"] == "overlap" for r in overlap["rawCandidates"])
    indirect = census(bytes.fromhex("ff26aa00"), [0])
    assert indirect["unresolvedTransfers"][0]["reason"] == "indirect jmp"
    assert not indirect["negativeUsable"]
    data = struct.pack("<HHHHHHHH", 2, 0x97, 14, 0x97, 15, 0xFFFF, 0, 0)
    first, rows = table(data)
    assert first[0x97] == 14 and rows[1]["shadowed"]
    try:
        table(b"\x00")
        raise AssertionError("truncated table accepted")
    except ValueError:
        pass
    try:
        table(struct.pack("<HHH", 2, 0x97, 0xFFFF))
        raise AssertionError("out-of-resource entry accepted")
    except ValueError:
        pass
    assert census(b"\xe9\x20\x00", [0])["unresolvedTransfers"][0]["reason"] == "outside resource"
    try:
        controls({"operands": []})
        raise AssertionError("missing control accepted")
    except ValueError:
        pass
    print("ADV literal census synthetic controls passed")


def main():
    if sys.argv[1:] == ["--self-test"]:
        self_test()
        return
    if len(sys.argv) != 2:
        raise SystemExit(__doc__)
    source = Path(sys.argv[1])
    archive = source.read_bytes()
    if (len(archive), xxhash.xxh3_128_hexdigest(archive)) != ARCHIVE:
        raise ValueError("not the approved installed RESOURCE.GFF")
    offset, size, digest = RESOURCE
    data = archive[offset:offset + size]
    if len(data) != size or xxhash.xxh3_128_hexdigest(data) != digest:
        raise ValueError("ADV 11 resource identity mismatch")
    first, rows = table(data)
    if any(first.get(n) != a for n, a in {0x97: 0x3852, 0x98: 0x3998, 0x67: 0x336C}.items()):
        raise ValueError("independent table controls failed")
    report = census(data, first.values())
    report.update({"source": str(source), "archiveXxh3": ARCHIVE[1],
                   "decoder": {"name": "Capstone", "version": capstone.__version__, "bits": 16},
                   "resource": {"number": 11, "offset": offset, "size": size, "xxh3": digest},
                   "window": [LOW, HIGH], "tableRows": rows,
                   "positiveOperandControls": controls(report),
                   "positiveTableControls": 3})
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
