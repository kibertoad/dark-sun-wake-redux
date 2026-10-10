"""Lists the instructions in DSUN.EXE that add a record offset to an address: an immediate or a lea displacement.

Usage: python -I field_offsets.py PATH_TO_DSUN.EXE INVENTORY_TSV [--table WORD] VALUE [VALUE ...]

Checks the installed DSUN.EXE's size and XXH3-128. Keeps the add, mov, lea, sub and or
instructions with an immediate operand that is one of the hexadecimal VALUEs (compared as a
word), and the lea instructions whose memory operand has one as its displacement: the forms that
point a register at a field before an access without a displacement. With --table WORD
(repeatable), a hit is kept only when one of the six instructions before it names the hexadecimal
WORD. Decodes the inventory and then every byte of the load image and the overlays as
field_reads.py does. Nothing is written or executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs
from capstone.x86 import X86_OP_MEM, X86_REG_ES

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"

argv = sys.argv[1:]
tables = [argv[i + 1].lower() for i, a in enumerate(argv) if a == "--table"]
args = [a for i, a in enumerate(argv) if a != "--table" and (i == 0 or argv[i - 1] != "--table")]
body = open(args[0], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
inventory = args[1]
disps = {int(w, 16) for w in args[2:]}
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages = struct.unpack_from("<HH", body, 2)
image_end = (pages - 1) * 512 + (cblp or 512)
table_offset, count = struct.unpack_from("<IH", body, image_end + 8)
rows = [struct.unpack_from("<HHHH", body, table_offset + 8 * i) for i in range(count)]
spans = [(header, image_end, None)]
for i, (seg, _, flags, _) in enumerate(rows):
    if flags == 3:
        payload, code_size = struct.unpack_from("<IH", body, header + seg * 16 + 4)
        spans.append((image_end + 16 + payload, image_end + 16 + payload + code_size, i))
md = Cs(CS_ARCH_X86, CS_MODE_16)
md.detail = True


def site(text):
    """File offset and display segment (None for an overlay) of an inventory site."""
    if ":" in text:
        seg, off = (int(p, 16) for p in text.split(":"))
        return header + (seg - 0x1000) * 16 + off, seg
    return int(text, 16), None


def where(offset, seg):
    if seg is None:
        overlay = next((o for low, high, o in spans if o is not None and low <= offset < high), None)
        code = next(low for low, high, o in spans if o == overlay)
        return f"overlay {overlay} +{offset - code:04X} (file 0x{offset:08X})"
    return f"{seg:04X}:{offset - header - (seg - 0x1000) * 16:04X} (file 0x{offset:08X})"


from capstone.x86 import X86_OP_IMM
def hit(ins):
    if ins.mnemonic not in ("add", "mov", "lea", "sub", "or"):
        return False
    for op in ins.operands:
        if op.type == X86_OP_IMM and (op.imm & 0xFFFF) in disps:
            return True
        if ins.mnemonic == "lea" and op.type == X86_OP_MEM and (op.mem.disp & 0xFFFF) in disps:
            return True
    return False


def in_context(run):
    return not tables or any(t in f"{c.mnemonic} {c.op_str}" for c in run[-6:] for t in tables)


printed = set()
for line in open(inventory, encoding="utf-8").read().splitlines()[1:]:
    start, _, ranges = line.split("\t")
    for item in ranges.split():
        low, high = item.split("..")
        lo, seg = site(low)
        hi, _ = site(high)
        run = []
        for ins in md.disasm(body[lo:hi], lo):
            if hit(ins):
                printed.add(ins.address)
                if not in_context(run):
                    run.append(ins)
                    continue
                print(f"{where(ins.address, seg)} in {start}: {ins.mnemonic} {ins.op_str}")
                for c in run[-6:]:
                    print(f"    {c.mnemonic} {c.op_str}")
            run.append(ins)
for low, high, overlay in spans:
    for at in range(low, high):
        ins = next(md.disasm(body[at:min(at + 8, high)], at), None)
        if ins is None or at in printed or not hit(ins):
            continue
        before = list(md.disasm(body[max(low, at - 24):at], max(low, at - 24)))
        if not in_context(before):
            continue
        seg = None if overlay is not None else 0x1000 + (at - header) // 16
        print(f"outside: {where(at, seg)}: {ins.mnemonic} {ins.op_str}")
