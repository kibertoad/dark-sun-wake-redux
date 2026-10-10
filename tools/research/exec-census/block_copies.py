"""Lists the block copies in DSUN.EXE: calls of the copy routines and inline string moves.

Usage: python -I block_copies.py PATH_TO_DSUN.EXE INVENTORY_TSV [CONTEXT]

Checks the installed DSUN.EXE's size and XXH3-128 and decodes each range of each row of
INVENTORY_TSV (resident rows as SEGMENT:OFFSET with the load image at segment 0x1000, overlay rows
as file offsets) from its start to its end, as field_stores.py does. It prints three kinds of site:

- "call 0452": a far call of 1000:0452, which copies CX bytes from the far pointer pushed last to
  the far pointer pushed before it (its own frame reads the source at [bp+6], the destination at
  [bp+0Ah], and it returns with retf 8);
- "call 3F5A": a far call of 1000:3F5A, which copies the count pushed first from the far pointer
  pushed second to the far pointer pushed last ([bp+6] destination, [bp+0Ah] source, [bp+0Eh]
  count; the caller removes the arguments);
- "movs": a movsb or movsw, with or without a rep prefix, outside those two routines.

A far call is matched on its offset word and its segment word: in the load image the segment word
must be 0 and carry an MZ relocation; in overlay code it must be in the overlay's fixup list and
equal 8 times the index of the segment-table descriptor of the resident segment at paragraph 0
(FMT-EXE-003). Near calls into segment 1000 are matched by their target. Each site is printed with
its row, address and the CONTEXT instructions before it in the row (default 16), and "19C9" when
the row reads the word at displacement 0x19C9 or 0x19CB without a base register. A second pass
decodes each stretch of the load image and of overlay code that no row covers, linearly from its
start and restarting one byte on after an undecodable byte, and prints its sites as "outside".
Nothing is written or executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs
from capstone.x86 import X86_OP_MEM

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
COPIES = {0x0452: "call 0452", 0x3F5A: "call 3F5A"}

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
inventory = sys.argv[2]
context = int(sys.argv[3]) if len(sys.argv) > 3 else 16
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages, relocs = struct.unpack_from("<HHH", body, 2)
reloc_at = struct.unpack_from("<H", body, 0x18)[0]
image_end = (pages - 1) * 512 + (cblp or 512)
relocated = set()
for k in range(relocs):
    off, seg = struct.unpack_from("<HH", body, reloc_at + 4 * k)
    relocated.add(header + seg * 16 + off)
table_offset, count = struct.unpack_from("<IH", body, image_end + 8)
rows = [struct.unpack_from("<HHHH", body, table_offset + 8 * i) for i in range(count)]
resident0 = next(i for i, r in enumerate(rows) if r[2] in (0, 1) and r[0] == 0)
overlays = []
for i, (seg, _, flags, _) in enumerate(rows):
    if flags == 3:
        payload, code_size, fixup_size = struct.unpack_from("<IHH", body, header + seg * 16 + 4)
        code = image_end + 16 + payload
        fixups = {code + struct.unpack_from("<H", body, code + code_size + 2 * k)[0] for k in range(fixup_size // 2)}
        overlays.append((i, code, code + code_size, fixups))
md = Cs(CS_ARCH_X86, CS_MODE_16)
md.detail = True
# The two copy routines themselves move strings; their own movs are not sites.
own = {header + 0x0452 + k for k in range(0x1C)} | {header + 0x3F5A + k for k in range(0x24)}


def site(text):
    if ":" in text:
        seg, off = (int(p, 16) for p in text.split(":"))
        return header + (seg - 0x1000) * 16 + off, seg
    return int(text, 16), None


def where(offset, seg):
    if seg is None:
        o = next((o for o in overlays if o[1] <= offset < o[2]), None)
        return f"overlay {o[0]} +{offset - o[1]:04X} (file 0x{offset:08X})" if o else f"file 0x{offset:08X}"
    return f"{seg:04X}:{offset - header - (seg - 0x1000) * 16:04X} (file 0x{offset:08X})"


def copy_call(ins, seg):
    """The copy routine a call instruction enters, or None."""
    raw = ins.bytes
    if raw[0] == 0x9A and len(raw) == 5:
        off, word = struct.unpack_from("<HH", raw, 1)
        if off not in COPIES:
            return None
        if seg is not None:
            return COPIES[off] if word == 0 and ins.address + 3 in relocated else None
        o = next((o for o in overlays if o[1] <= ins.address < o[2]), None)
        return COPIES[off] if o and word == resident0 * 8 and ins.address + 3 in o[3] else None
    if raw[0] == 0xE8 and seg == 0x1000:
        target = (ins.address + 3 + struct.unpack_from("<h", raw, 1)[0] - header) & 0xFFFF
        return COPIES.get(target)
    return None


def reads_19c9(ins):
    for op in ins.operands:
        if op.type == X86_OP_MEM and not op.mem.base and not op.mem.index and (op.mem.disp & 0xFFFF) in (0x19C9, 0x19CB):
            return True
    return False


totals = {}


def scan(lo, hi, seg, label):
    """Decodes lo..hi linearly and prints each copy site with its context."""
    decoded = []
    at = lo
    while at < hi:
        # Restart one byte on after an undecodable byte, so a gap's data does not end the decode.
        part = list(md.disasm(body[at:hi], at))
        decoded += part
        end = part[-1].address + part[-1].size if part else at
        at = end + 1 if end < hi else hi
    marker = " 19C9" if any(reads_19c9(i) for i in decoded) else ""
    for n, ins in enumerate(decoded):
        kind = copy_call(ins, seg)
        if kind is None and ins.mnemonic.split()[-1] in ("movsb", "movsw") and ins.address not in own:
            kind = "movs"
        if kind is None:
            continue
        totals[kind] = totals.get(kind, 0) + 1
        print(f"{kind}: {where(ins.address, seg)} {label}{marker}: {ins.mnemonic} {ins.op_str}")
        for c in decoded[max(0, n - context):n]:
            print(f"    {c.address - (header + (seg - 0x1000) * 16) if seg else c.address:05X}  {c.mnemonic} {c.op_str}")


covered = []
for line in open(inventory, encoding="utf-8").read().splitlines()[1:]:
    start, _, ranges = line.split("\t")
    for item in ranges.split():
        low, high = item.split("..")
        lo, seg = site(low)
        hi, _ = site(high)
        covered.append((lo, hi))
        scan(lo, hi, seg, f"in {start}")
# Second pass: the bytes of the load image and of each overlay's code that no row covers, decoded
# linearly from the start of each gap. A linear decode can misread data or start out of alignment,
# so each "outside" site is to be checked against the instructions around it.
covered.sort()
spans = [(header, image_end, 0x1000)] + [(o[1], o[2], None) for o in overlays]
for low, high, seg in spans:
    at = low
    for lo, hi in covered + [(high, high)]:
        if hi <= low or lo >= high or hi <= at:
            continue
        if lo > at:
            gap_seg = None if seg is None else 0x1000 + (at - header) // 16
            scan(at, min(lo, high), gap_seg, "outside")
        at = max(at, hi)
print(totals)
