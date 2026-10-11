"""Write a scientific-method-engine `reach` config for SOUND_DS.EXE (Q-EXE-025).

Usage: python -I sound_ds_reach_config.py SOUND_DS_EXE OUT_JSON [STARTS_JSON]

The regions are the load image's code segments: each paragraph a relocated far call or far jump
names as its segment, up to the next such paragraph, and the last up to the data segment the
startup code loads into DS. Code addresses are written as file offsets. The relocations are the
image's relocated far calls and far jumps. The jump tables are those of the `jmp cs:[bx+table]`
shapes reach_config.py reads, to their checked bounds.

The starts are the MZ entry point and the routines the startup records name, read from the
record walker's bounds at 0000:012F (six-byte records: a type byte, 0 for a near routine in the
startup segment, a priority byte and a pointer). STARTS_JSON, when given, is a list of
{"site", "evidence"} with further established entries as file offsets. Each region's
entries are the starts and far-call targets in it.

The targets are every `CD 21` pair in the regions. The controls are FND-EXE-353's far call to
1000:20DF at 1BD4:0331, and the instruction controls FND-EXE-370's AH=43 and AH=3D interrupt
sites at 1000:27A8 and 1000:3316. The report lists the ranges the walk decoded. Nothing is
executed.
"""
import json
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

SIZE, XXH3 = 204593, "236c2dc23c071eca421eb5b427caee57"
DGROUP = 0xE36  # paragraph the startup code loads into DS (mov dx, 0x0E36 at 0000:0003)
RECORDS = (0xDF1C, 0xDF34)  # DGROUP bounds of the startup records (0000:012F, 0000:0132)

source, out = sys.argv[1:3]
extra = json.load(open(sys.argv[3])) if len(sys.argv) > 3 else []
body = open(source, "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed SOUND_DS.EXE"
cblp, pages, reloc_count, header_paragraphs = struct.unpack_from("<4H", body, 2)
header = header_paragraphs * 16
reloc_at = struct.unpack_from("<H", body, 0x18)[0]
entry_ip, entry_cs = struct.unpack_from("<HH", body, 0x14)
assert body[header + 1:header + 3] == struct.pack("<H", DGROUP), "startup does not load the expected DS"

far = []
for k in range(reloc_count):
    off, seg = struct.unpack_from("<HH", body, reloc_at + 4 * k)
    word_at = header + seg * 16 + off
    if body[word_at - 3] in (0x9A, 0xEA):
        far.append((word_at, struct.unpack_from("<H", body, word_at)[0], struct.unpack_from("<H", body, word_at - 2)[0]))
# A module starts within the first 16 bytes of the frame its far calls name; the far call with the
# smallest offset into a frame bounds that start from above.
first = {entry_cs: entry_ip}
# Near calls the walk decoded inside a module whose targets lie below its first far-called routine.
NEAR_STARTS = {0x429: (6, "near call at file 0x594F"), 0x4A2: (9, "near call at file 0x5FC3"),
               0x64C: (8, "near call at file 0x8A08"), 0x783: (7, "near call at file 0x8F36"),
               0xDB2: (8, "near call at file 0xF0EE")}
for frame, (ip, _) in NEAR_STARTS.items():
    first[frame] = ip
for _, raw, ip in far:
    if raw < DGROUP:
        first[raw] = min(first.get(raw, 0x10000), ip)
frames = sorted(first)
starts_at = [(s, min(first[s], 15)) for s in frames] + [(DGROUP, 0)]
regions = [{"name": f"segment-{s:04X}", "start": header + s * 16 + o, "end": header + e * 16 + eo,
            "segment": 0x1000 + s, "ip": o,
            "evidence": "module start in the frame a relocated far transfer names, up to the next one"}
           for (s, o), (e, eo) in zip(starts_at, starts_at[1:])]


def inside(at):
    return any(r["start"] <= at < r["end"] for r in regions)


relocations = [{"site": w, "segment": 0x1000 + raw, "target": header + raw * 16 + ip, "evidence": "MZ relocation"}
               for w, raw, ip in far if raw < DGROUP]

dgroup = header + DGROUP * 16
starts = [header + entry_cs * 16 + entry_ip]
start_notes = [{"site": starts[0], "evidence": "MZ entry point"}]
for at in range(dgroup + RECORDS[0], dgroup + RECORDS[1], 6):
    kind, _, ip, seg = struct.unpack_from("<BBHH", body, at)
    site = header + (0 if kind == 0 else seg) * 16 + ip
    starts.append(site)
    start_notes.append({"site": site, "evidence": f"startup record at DGROUP:{at - dgroup:04X}, type {kind}"})
for row in extra:
    site = int(row["site"], 16)
    starts.append(site)
    start_notes.append({"site": site, "evidence": row["evidence"]})
entries = set(starts) | {t["target"] for t in relocations}
for r in regions:
    r["entries"] = sorted(e for e in entries if r["start"] <= e < r["end"])
targets = [at for r in regions for at in range(r["start"], r["end"] - 1) if body[at:at + 2] == b"\xcd\x21"]
controls = [header + 0xBD4 * 16 + 0x331]
instruction_controls = [header + 0x27A8, header + 0x3316]
for at in starts + targets + controls + instruction_controls:
    assert inside(at), f"0x{at:X} is outside the regions"
assert body[controls[0]:controls[0] + 5] == bytes.fromhex("9adf200000"), "control is not the far call to 1000:20DF"

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
            text = [(i.mnemonic, i.op_str) for i in before(at, r)]
            table = None
            cmp = next((i for i in range(len(text) - 1, -1, -1) if text[i][0] == "cmp" and text[i][1].startswith("bx, ")), None)
            if (cmp is not None and cmp + 1 < len(text) and text[cmp + 1][0] in ("ja", "jbe")
                    and any(t in (("shl", "bx, 1"), ("add", "bx, bx")) for t in text[cmp + 2:])):
                bound = int(text[cmp][1].split(", ")[1], 0)
                table = {"start": r["start"] - r["ip"] + disp, "count": bound + 1, "stride": 2,
                         "evidence": f"index bounded by cmp bx, {bound:#x} and {text[cmp + 1][0]} before the jump"}
            movcx = next((t for t in text if t[0] == "mov" and t[1].startswith("cx, ")), None)
            movbx = next((t for t in text if t[0] == "mov" and t[1].startswith("bx, 0x")), None)
            if table is None and movcx and movbx and ("add", "bx, 2") in text:
                n, values = int(movcx[1].split(", ")[1], 0), int(movbx[1].split(", ")[1], 0)
                if disp == 2 * n:
                    table = {"start": r["start"] - r["ip"] + values + disp, "count": n, "stride": 2,
                             "evidence": f"value scan of {n} words at CS:{values:#x}, targets after them"}
            if table and 1 <= table["count"] <= 256:
                jumps.append({"site": at, "exhaustive": True, "table": table,
                              "evidence": "bounded jmp cs:[bx+table] in a declared code region"})
        at = body.find(b"\x2e\xff", at + 1, r["end"])

json.dump({"source": source, "xxh3": XXH3, "sourceKind": "mz", "regions": regions, "relocations": relocations,
           "indirectJumps": jumps, "starts": starts, "targets": targets, "controls": controls,
           "instructionControls": instruction_controls, "decodedRanges": True,
           "instructionLimit": max(100000, sum(r["end"] - r["start"] for r in regions)), "limit": 10000},
          open(out, "w"), separators=(",", ":"))
print(f"{len(regions)} regions, {len(relocations)} far transfers, {len(jumps)} jump tables, {len(targets)} targets")
for note in start_notes:
    print(f"start 0x{note['site']:X}: {note['evidence']}")
