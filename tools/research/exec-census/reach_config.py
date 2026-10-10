"""Writes a scientific-method-engine `reach` configuration for the installed DSUN.EXE.

Usage: python -I reach_config.py PATH_TO_DSUN.EXE INVENTORY_TSV OUT_JSON QUERY_JSON

QUERY_JSON holds `starts`, `targets` and `controls` (lists of sites) and `leaves` (a list of
{site, reason}), and may hold `instructionControls` (a list of sites) and `indirectCalls` (the
engine's declarations of computed call targets, from scientific-method-engine 18.4.0: each with
`site`, `evidence`, `exhaustive` and either `targets`, a list of sites, or `table`, whose `start`
is a site), and `noReturn` (the engine's list of {routine, reason} and {interrupt, reason}
entries, from 18.0.0, whose routine or interrupt is a site). A site is a file offset 0xHEX, a resident address SEGMENT:OFFSET (load image at
segment 0x1000) or an overlay routine DESCRIPTOR+OFFSET (hexadecimal offset in that overlay's
code); each becomes a file offset. Checks the installed DSUN.EXE's size and XXH3-128.

Regions: one per overlay, its whole code (FMT-EXE-003) at an analysis segment 0x9000 + descriptor
and IP 0, the IP its code runs at; and one per resident segment-table descriptor of flags 0 or 1
whose span is not empty, from segment * 16 + start_offset to segment * 16 + end_offset (FND-EXE-571)
at its own segment and IP start_offset. Every such region is declared, also one that holds no
entry (scientific-method-engine 15.2.0 and later accept a region with no entries as long as some
region lists one; a walk reaches its code only through a transfer into it). The entries of each
region are the inventory starts in it, every query start and leaf in it, since `reach` takes only
established entries as starts, and, for an overlay, the code offset each of its trampolines names
(FMT-EXE-004).

Relocations: every far call or jump (9A or EA, offset, segment word) whose segment word has an MZ
relocation or is in an overlay's fixup list, with the file offset it transfers to: for a resident
segment, segment * 16 + offset in the load image; for an overlay's trampoline header, the overlay
code offset the trampoline at that offset names (FMT-EXE-004). A fixup word is descriptor * 8.

Indirect jumps: every `jmp cs:[bx + table]` (2E FF A7 or 2E FF 67) inside a region whose index is
bounded in one of three ways, read from the bytes before it:
- `cmp bx, imm` then `ja` or `jbe`, with `shl bx, 1` or `add bx, bx` before the jump: entries 0
  to imm of the word table at CS:table;
- a word value scan: `mov cx, n`, `mov bx, values`, a loop comparing `cs:[bx]` and adding 2 to
  bx, then the jump with table = 2 * n: the n words at CS:values + table;
- a double-word value scan: the same with `add bx, 4`, then `add cx, cx` and `add bx, cx` before
  the jump: the n words at CS:values + 2 * n + table.
Each is declared exhaustive. A site whose bound is not found is left undeclared, so `reach`
lists it unresolved. The instruction limit is the total size of the declared regions, or 100,000
when that is larger: since engine 15.2.0 that is the largest limit the engine takes, and every
instruction a walk can decode starts in a declared region, so a walk never stops at the limit.
Nothing is executed.
"""
import json
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"

source, inventory, out, query_path = sys.argv[1:5]
body = open(source, "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages = struct.unpack_from("<HH", body, 2)
image_end = (pages - 1) * 512 + (cblp or 512)
table_offset, count = struct.unpack_from("<IH", body, image_end + 8)
rows = [struct.unpack_from("<HHHH", body, table_offset + 8 * i) for i in range(count)]
overlay_code = {}
for i, (seg, _, flags, _) in enumerate(rows):
    if flags == 3:
        payload, code_size = struct.unpack_from("<IH", body, header + seg * 16 + 4)
        overlay_code[i] = (image_end + 16 + payload, code_size)


def resident(text):
    seg, off = (int(p, 16) for p in text.split(":"))
    return header + (seg - 0x1000) * 16 + off


def site(text):
    if text.startswith("0x"):
        return int(text, 16)
    if ":" in text:
        return resident(text)
    descriptor, off = text.split("+")
    return overlay_code[int(descriptor)][0] + int(off, 16)


query = json.load(open(query_path))
starts = [site(s) for s in query["starts"]]
targets = [site(s) for s in query["targets"]]
controls = [site(s) for s in query.get("controls", [])]
leaves = [{"routine": site(leaf["site"]), "reason": leaf["reason"]} for leaf in query.get("leaves", [])]
instruction_controls = [site(s) for s in query.get("instructionControls", [])]
no_return = [{key: (site(value) if key in ("routine", "interrupt") else value) for key, value in entry.items()}
             for entry in query.get("noReturn", [])]
calls = []
for claim in query.get("indirectCalls", []):
    call = {"site": site(claim["site"]), "evidence": claim["evidence"], "exhaustive": claim["exhaustive"]}
    if "table" in claim:
        call["table"] = dict(claim["table"], start=site(claim["table"]["start"]))
    else:
        call["targets"] = [site(t) for t in claim["targets"]]
    calls.append(call)

inventory_starts = [line.split("\t")[0] for line in open(inventory).read().splitlines()[1:]]
entries = {resident(s) if ":" in s else int(s, 16) for s in inventory_starts}
entries |= set(starts) | {leaf["routine"] for leaf in leaves}
regions = []
for i, (seg, end, flags, begin) in enumerate(rows):
    if flags == 3:
        a, n = overlay_code[i]
        region = {"name": f"overlay-{i}", "start": a, "end": a + n, "segment": 0x9000 + i, "ip": 0,
                  "evidence": "FBOV overlay code (FMT-EXE-003)"}
        trampolines = struct.unpack_from("<H", body, header + seg * 16 + 12)[0]
        for t in range(trampolines):
            at = header + seg * 16 + 0x20 + 5 * t
            if body[at:at + 2] == b"\xcd\x3f" and struct.unpack_from("<H", body, at + 2)[0] < n:
                entries.add(a + struct.unpack_from("<H", body, at + 2)[0])
    elif flags in (0, 1) and end > begin:
        a = header + seg * 16 + begin
        region = {"name": f"segment-{i}", "start": a, "end": header + seg * 16 + end, "segment": 0x1000 + seg,
                  "ip": begin, "evidence": "segment-table span (FND-EXE-571)"}
    else:
        continue
    region["entries"] = sorted(e for e in entries if region["start"] <= e < region["end"])
    regions.append(region)
for at in starts + targets + controls + [leaf["routine"] for leaf in leaves]:
    assert any(r["start"] <= at < r["end"] for r in regions), f"0x{at:X} is outside the regions"


def canonical(row, ip):
    """File offset a far transfer to descriptor ``row`` at ``ip`` runs, through a trampoline for an overlay."""
    seg, _, flags, _ = rows[row]
    at = header + seg * 16 + ip
    if flags != 3:
        return at
    if body[at:at + 2] != b"\xcd\x3f":
        return None
    return overlay_code[row][0] + struct.unpack_from("<H", body, at + 2)[0]


relocations = []
reloc_count, reloc_at = struct.unpack_from("<H", body, 6)[0], struct.unpack_from("<H", body, 0x18)[0]
by_segment = {}
for i, (seg, _, flags, _) in enumerate(rows):
    by_segment.setdefault(seg, i)
for k in range(reloc_count):
    off, seg = struct.unpack_from("<HH", body, reloc_at + 4 * k)
    word_at = header + seg * 16 + off
    if word_at < header + 3 or body[word_at - 3] not in (0x9A, 0xEA):
        continue
    raw, ip = struct.unpack_from("<H", body, word_at)[0], struct.unpack_from("<H", body, word_at - 2)[0]
    target = canonical(by_segment[raw], ip) if raw in by_segment else None
    if target is not None:
        relocations.append({"site": word_at, "segment": 0x1000 + raw, "target": target, "evidence": "MZ relocation"})
for descriptor, (code, code_size) in overlay_code.items():
    fixup_size = struct.unpack_from("<H", body, header + rows[descriptor][0] * 16 + 10)[0]
    for k in range(fixup_size // 2):
        word_at = code + struct.unpack_from("<H", body, code + code_size + 2 * k)[0]
        if body[word_at - 3] not in (0x9A, 0xEA):
            continue
        word, ip = struct.unpack_from("<H", body, word_at)[0], struct.unpack_from("<H", body, word_at - 2)[0]
        if word % 8 or word // 8 >= len(rows):
            continue
        target = canonical(word // 8, ip)
        if target is not None:
            relocations.append({"site": word_at, "segment": 0x1000 + rows[word // 8][0], "target": target,
                                "evidence": "FBOV fixup"})

md = Cs(CS_ARCH_X86, CS_MODE_16)


def before(at, region, limit=40):
    """The instructions that decode, without a gap, from some start up to ``at``; the longest such run."""
    for back in range(limit, 0, -1):
        start = at - back
        if start < region["start"]:
            continue
        run = list(md.disasm(body[start:at], start))
        if run and run[0].address == start and run[-1].address + run[-1].size == at:
            return run
    return []


jumps = []
for r in regions:
    at = body.find(b"\x2e\xff", r["start"], r["end"])
    while at != -1:
        modrm = body[at + 2] if at + 2 < r["end"] else None
        if modrm in (0xA7, 0x67):
            disp = struct.unpack_from("<H", body, at + 3)[0] if modrm == 0xA7 else body[at + 3]
            run = before(at, r)
            text = [(i.mnemonic, i.op_str) for i in run]
            cs_base = r["start"] - r["ip"]
            table = None
            cmp = next((i for i in range(len(text) - 1, -1, -1) if text[i][0] == "cmp" and text[i][1].startswith("bx, ")), None)
            if (cmp is not None and cmp + 1 < len(text) and text[cmp + 1][0] in ("ja", "jbe")
                    and any(t in (("shl", "bx, 1"), ("add", "bx, bx")) for t in text[cmp + 2:])):
                bound = int(text[cmp][1].split(", ")[1], 0)
                table = {"start": cs_base + disp, "count": bound + 1, "stride": 2,
                         "evidence": f"index bounded by cmp bx, {bound:#x} and {text[cmp + 1][0]} before the jump"}
            movcx = next((t for t in text if t[0] == "mov" and t[1].startswith("cx, ")), None)
            movbx = next((t for t in text if t[0] == "mov" and t[1].startswith("bx, 0x")), None)
            if table is None and movcx and movbx and ("add", "bx, 2") in text:
                n, values = int(movcx[1].split(", ")[1], 0), int(movbx[1].split(", ")[1], 0)
                if disp == 2 * n:
                    table = {"start": cs_base + values + disp, "count": n, "stride": 2,
                             "evidence": f"value scan of {n} words at CS:{values:#x}, targets after them"}
            if (table is None and movcx and movbx and ("add", "bx, 4") in text
                    and text[-2:] == [("add", "cx, cx"), ("add", "bx, cx")]):
                # At a match on value k, cx is n - k and bx is values + 4k, so the target word is
                # at values + 2n + table + 2k.
                n, values = int(movcx[1].split(", ")[1], 0), int(movbx[1].split(", ")[1], 0)
                table = {"start": cs_base + values + 2 * n + disp, "count": n, "stride": 2,
                         "evidence": f"value scan of {n} double words at CS:{values:#x}, targets after them"}
            if table and 1 <= table["count"] <= 256:
                jumps.append({"site": at, "exhaustive": True, "table": table,
                              "evidence": "bounded jmp cs:[bx+table] in a declared code region"})
        at = body.find(b"\x2e\xff", at + 1, r["end"])

json.dump({"source": source, "xxh3": XXH3, "sourceKind": "mz", "regions": regions, "relocations": relocations,
           "indirectJumps": jumps, "starts": starts, "targets": targets, "leaves": leaves, "controls": controls,
           **({"instructionControls": instruction_controls} if instruction_controls else {}),
           **({"indirectCalls": calls} if calls else {}),
           **({"noReturn": no_return} if no_return else {}),
           "instructionLimit": max(100000, sum(r["end"] - r["start"] for r in regions)), "limit": 10000}, open(out, "w"), separators=(",", ":"))
print(f"{len(regions)} regions, {len(relocations)} far transfers, {len(jumps)} jump tables, {len(starts)} starts,"
      f" {len(targets)} targets, {len(leaves)} leaves, {len(controls)} controls")
