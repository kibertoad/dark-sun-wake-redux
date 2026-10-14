"""Reads the far pointer a caller passes as one argument of a routine in the installed DSUN.EXE (Q-EXE-007).

Usage: python -I callback_args.py PATH_TO_DSUN.EXE LABEL LO SITE [SITE ...]
LABEL is printed on each line. SITE is the file offset of a call instruction, in hexadecimal.
For each site, finds the pushes that fill the callee's argument bytes [bp+LO, bp+LO+4) (far frame;
a near call preceded by push cs counts as far), and prints the pushed offset and segment, whether
each pushed immediate segment word is relocated (MZ relocation for resident code, the overlay's fixup
list for overlay code), and the resolved target: an overlay trampoline's code offset (D+XXXX) or a
resident address. Non-immediate pushes are printed as they are. A far call to 1000:0699 (segment word 0000 before relocation) after
mov cx, N counts as N pushed bytes (the runtime routine that copies a structure argument onto the
stack). Checks the installed DSUN.EXE's size and XXH3-128. Nothing is written or executed.
"""
import struct
import sys
from capstone import CS_ARCH_X86, CS_MODE_16, Cs
from capstone.x86 import X86_OP_IMM
import xxhash

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"

b = open(sys.argv[1], "rb").read()
assert (len(b), xxhash.xxh3_128_hexdigest(b)) == (SIZE, XXH3), "not the installed DSUN.EXE"
setter, lo = sys.argv[2], int(sys.argv[3], 16)
md = Cs(CS_ARCH_X86, CS_MODE_16); md.detail = True
cblp, pages = struct.unpack_from("<HH", b, 2)
end = (pages - 1) * 512 + (cblp or 512)
hdr = struct.unpack_from("<H", b, 8)[0] * 16
to, n = struct.unpack_from("<IH", b, end + 8)
rows = [struct.unpack_from("<HHHH", b, to + 8 * i) for i in range(n)]
nr = struct.unpack_from("<H", b, 6)[0]; ra = struct.unpack_from("<H", b, 0x18)[0]
mz = {0x5200 + struct.unpack_from("<H", b, ra + 4 * k + 2)[0] * 16 + struct.unpack_from("<H", b, ra + 4 * k)[0]
      for k in range(nr)}
fix, spans = set(), []
for d, r in enumerate(rows):
    if r[2] != 3:
        continue
    base = hdr + r[0] * 16
    po, cs, fs, tr = struct.unpack_from("<IHHH", b, base + 4)
    code = end + 16 + po
    spans.append((code, code + cs, d))
    fix |= {code + f for f in struct.unpack_from("<%dH" % (fs // 2), b, code + cs)}


def where(at):
    for s, e, d in spans:
        if s <= at < e:
            return "%d+%04X" % (d, at - s)
    return "resident 0x%X" % at


def relocated(at):
    return at in mz or at in fix


def resolve(seg, off):
    if seg == 0 and off == 0:
        return "null"
    if seg % 8 or seg // 8 >= n:
        return "?"
    r = rows[seg // 8]
    if r[2] == 3:
        base = hdr + r[0] * 16
        tr = struct.unpack_from("<H", b, base + 12)[0]
        k, rest = divmod(off - 0x20, 5)
        if rest or not 0 <= k < tr or b[base + off:base + off + 2] != b"\xcd\x3f":
            return "? not a trampoline"
        return "%d+%04X" % (seg // 8, struct.unpack_from("<H", b, base + off + 2)[0])
    return "%04X:%04X" % (r[0] + 0x1000, off)


def back(site):
    for m in range(96, 0, -1):
        ins = list(md.disasm(b[site - m:site + 5], site - m))
        addrs = [i.address for i in ins]
        if site in addrs:
            k = addrs.index(site)
            if all(ins[j].address + ins[j].size == ins[j + 1].address for j in range(k)):
                return ins[:k]
    return []


for a in sys.argv[4:]:
    site = int(a, 16)
    ins = back(site)
    if b[site] == 0xE8 and ins and ins[-1].op_str == "cs":
        ins = ins[:-1]
    pos, slot = 6, {}
    rev = list(reversed(ins))
    for x, i in enumerate(rev):
        if pos >= lo + 4:
            break
        if i.mnemonic == "lcall" and i.op_str == "0, 0x699":
            # runtime helper that copies CX bytes onto the stack (a structure argument)
            cx = next((int(j.op_str.split(", ")[1], 16) for j in rev[x + 1:x + 5]
                       if j.mnemonic == "mov" and j.op_str.startswith("cx, 0x")), None)
            if cx is None:
                break
            for byte in range(pos, pos + cx):
                slot[byte] = None
            pos += cx
            continue
        if i.mnemonic != "push":
            if i.mnemonic in ("call", "lcall", "pop", "ret", "retf") or i.mnemonic.startswith("j") \
                    or i.op_str.startswith("sp"):
                break
            continue
        size = 4 if 0x66 in i.prefix else 2
        for byte in range(pos, pos + size):
            slot[byte] = (i, pos)
        pos += size
    if pos < lo + 4:
        print(setter, where(site), "UNREAD"); continue
    # The argument's four bytes, each from the push that wrote it; an immediate push gives its bytes.
    parts = [slot[lo + k] for k in range(4)]
    if any(p is None or p[0].operands[0].type != X86_OP_IMM for p in parts):
        whole = parts[0] is not None and all(p == parts[0] for p in parts) and parts[0][1] == lo
        print(setter, where(site), "dword" if whole else "pushes",
              " / ".join(dict.fromkeys(p[0].op_str for p in parts if p is not None)))
        continue
    value = bytes(((p[0].operands[0].imm & 0xFFFFFFFF) >> (8 * (lo + k - p[1]))) & 0xFF
                  for k, p in enumerate(parts))
    off, seg = struct.unpack("<HH", value)
    if seg == 0 and off == 0:
        print(setter, where(site), "null"); continue
    push = parts[2][0]
    # A segment word an MZ relocation or an overlay fixup can patch is a 16-bit immediate push of its own.
    reloc = push.size == 3 and parts[2][1] == lo + 2 and relocated(push.address + 1)
    print(setter, where(site), "%04X:%04X" % (seg, off), "reloc" if reloc else "NOT RELOCATED", resolve(seg, off))
