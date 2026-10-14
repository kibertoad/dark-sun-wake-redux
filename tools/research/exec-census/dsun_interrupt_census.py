"""AH at every interrupt 21 and interrupt 2F site of the installed DSUN.EXE (Q-EXE-007).

Usage: python -I dsun_interrupt_census.py DSUN_EXE INVENTORY_TSV REACH_REPORT_JSON [REACH_CONFIG_JSON]

The report is `reach` run on a reach_config.py configuration with `decodedRanges` whose targets
are every `CD 21` and `CD 2F` pair in the load image; a target that is not such a pair, such as a
call into code built on the stack, is printed with the interrupt "call". For a reached site it decodes the report's
decoded range that holds the site, from the range's start up to the site, as
sound_ds_reached_ah.py does. For a site the walk did not reach (a candidate) it decodes from the
start of the inventory range that holds it when that decode lands on the site, and otherwise the
longest gapless run of at most 40 bytes that ends at it. Either way it keeps the last immediate
load of AH or AX; an instruction in between that writes AH or AX some other way leaves the value
"not immediate". With the configuration the report was run on, it also lists for each site the
transfers that land after that last writer and up to the interrupt, from outside the span from
the writer to the interrupt: a relative jump, call, loop or jcxz decoded at any byte offset of
the site's region, a far transfer the configuration declares, or an entry of a declared jump
table. A transfer listed there can reach the interrupt with another AH. Prints each site with its resident address, interrupt, reach, AH and the
instruction it was read from, then the counts and the report's unresolved transfers, gaps and
contested starts. Addresses are written as resident SEGMENT:OFFSET in the segment of the
segment-table descriptor whose span holds them, or overlay DESCRIPTOR+OFFSET. Nothing is executed.
"""
import json
import struct
import sys
from collections import Counter

from capstone import CS_ARCH_X86, CS_MODE_16, Cs

body = open(sys.argv[1], "rb").read()
header = struct.unpack_from("<H", body, 8)[0] * 16
report = json.load(open(sys.argv[3]))
md = Cs(CS_ARCH_X86, CS_MODE_16)
decoded = sorted((r["start"], r["end"]) for r in report["decodedRanges"])


def resident(text):
    if text.startswith("0x"):
        return int(text, 16)
    seg, off = (int(p, 16) for p in text.split(":"))
    return header + (seg - 0x1000) * 16 + off


inventory = []
for line in open(sys.argv[2]).read().splitlines()[1:]:
    for part in line.split("\t")[2].split():
        a, e = part.split("..")
        inventory.append((resident(a), resident(e)))


cblp, pages = struct.unpack_from("<HH", body, 2)
image_end = (pages - 1) * 512 + (cblp or 512)
table_offset, count = struct.unpack_from("<IH", body, image_end + 8)
rows = [struct.unpack_from("<HHHH", body, table_offset + 8 * i) for i in range(count)]
overlays = {}
for i, (seg, _, flags, _) in enumerate(rows):
    if flags == 3:
        payload, code_size = struct.unpack_from("<IH", body, header + seg * 16 + 4)
        overlays[i] = (image_end + 16 + payload, code_size)


def address(at):
    """A resident address in the segment of the descriptor whose span holds it (FND-EXE-571), or
    an overlay address DESCRIPTOR+OFFSET."""
    for i, (code, size) in overlays.items():
        if code <= at < code + size:
            return f"{i}+{at - code:04X}"
    linear = at - header
    raw = next((r[0] for r in rows if r[2] in (0, 1) and r[0] * 16 + r[3] <= linear < r[0] * 16 + r[1]), linear // 16)
    return f"{0x1000 + raw:04X}:{linear - raw * 16:04X}"


def run_to(start, site):
    run = list(md.disasm(body[start:site], start))
    return run if run and run[0].address == start and run[-1].address + run[-1].size == site else None


def ah_along(run):
    """AH after ``run``, the note naming its last writer, and that writer's end (None for no write)."""
    ah, note, end = None, "no write", None
    for i in run:
        op = i.op_str
        if i.mnemonic == "mov" and op.startswith(("ah, ", "ax, ")) and not op.split(", ")[1].startswith(("word", "byte", "[")):
            end = i.address + i.size
            try:
                value = int(op.split(", ")[1], 0)
            except ValueError:
                ah, note = None, f"{i.mnemonic} {op} at {address(i.address)}"
                continue
            ah, note = (value if op.startswith("ah") else value >> 8), f"{i.mnemonic} {op} at {address(i.address)}"
        elif (op.startswith(("ah,", "ax,", "al,")) and i.mnemonic not in ("cmp", "test") and not op.startswith("al,")
              or op in ("ax", "ah") and i.mnemonic not in ("push",)
              or i.mnemonic in ("lodsw", "cwd", "cbw", "mul", "div", "imul", "idiv", "xchg", "lahf", "in")):
            if op.startswith("al,") or i.mnemonic == "xchg" and "ax" not in op:
                continue
            ah, note, end = None, f"{i.mnemonic} {op} at {address(i.address)}", i.address + i.size
    return ah, note, end


config = json.load(open(sys.argv[4])) if len(sys.argv) > 4 else None
REL8 = {0xEB, 0xE0, 0xE1, 0xE2, 0xE3} | set(range(0x70, 0x80))
branch_targets = {}


def branches_into(region):
    """Every relative jump, call, loop or jcxz that decodes at some byte offset of ``region``, as
    {target file offset: [source file offsets]}, read once per region."""
    key = (region["start"], region["end"])
    if key not in branch_targets:
        found = {}
        base = region["start"] - region["ip"]
        for at in range(region["start"], region["end"]):
            op = body[at]
            if op in REL8 and at + 2 <= region["end"]:
                size, rel = 2, struct.unpack_from("<b", body, at + 1)[0]
            elif op in (0xE8, 0xE9) and at + 3 <= region["end"]:
                size, rel = 3, struct.unpack_from("<h", body, at + 1)[0]
            elif op == 0x0F and at + 4 <= region["end"] and 0x80 <= body[at + 1] <= 0x8F:
                size, rel = 4, struct.unpack_from("<h", body, at + 2)[0]
            else:
                continue
            found.setdefault(base + ((at + size - base + rel) & 0xFFFF), []).append(at)
        branch_targets[key] = found
    return branch_targets[key]


def entries_into(window_start, site, writer_start):
    """Sources outside [writer_start, site) of transfers that land in [window_start, site]."""
    if config is None:
        return None
    region = next(r for r in config["regions"] if r["start"] <= site < r["end"])
    hits = []
    for target, sources in branches_into(region).items():
        if window_start <= target <= site:
            hits += [f"branch at {address(s)}" for s in sources if not writer_start <= s < site]
    hits += [f"far transfer at {address(r['site'] - 3)}" for r in config["relocations"] if window_start <= r["target"] <= site]
    for jump in config["indirectJumps"]:
        table = jump["table"]
        own = next(r for r in config["regions"] if r["start"] <= jump["site"] < r["end"])
        for k in range(table["count"]):
            word = struct.unpack_from("<H", body, table["start"] + table["stride"] * k)[0]
            if window_start <= own["start"] - own["ip"] + word <= site:
                hits.append(f"jump table entry of {address(jump['site'])}")
    return hits


counts = Counter()
windows = Counter()
for t in report["targets"]:
    site = t["target"]
    kind = f"{body[site + 1]:02X}" if body[site] == 0xCD else "call"
    if t.get("reached"):
        start = max(a for a, e in decoded if a <= site < e)
        run, how = run_to(start, site), "decoded range"
    else:
        start = next((a for a, e in inventory if a <= site < e), None)
        run = run_to(start, site) if start is not None else None
        how = "inventory range"
        if run is None:
            run = next((r for back in range(40, 0, -1) if site - back >= header
                        for r in [run_to(site - back, site)] if r), [])
            how = "gapless run"
            start = run[0].address if run else site
    ah, note, end = ah_along(run or [])
    value = f"{ah:02X}" if ah is not None else "not immediate"
    reach = "reached" if t.get("reached") else "candidate"
    counts[(kind, reach, value)] += 1
    window = ""
    if end is not None and kind != "call":
        writer = next(i.address for i in run if i.address + i.size == end)
        hits = entries_into(end, site, writer)
        if hits is not None:
            windows["entered" if hits else "not entered"] += 1
            window = f"; window {address(end)}..{address(site)} entered by {', '.join(hits)}" if hits else ""
    print(f"{address(site)} file {site:X} int {kind} {reach} from {how} {address(start)} AH {value} ({note}){window}")
for key, n in sorted(counts.items()):
    print(*key, n)
if config is not None:
    print("writer-to-interrupt windows:", dict(windows))
for u in report["unresolved"]:
    print(f"unresolved {address(u['site'])} {u['instruction']} in {address(u['routine'])}")
for g in report["gaps"]:
    print(f"gap {address(g['site'])} {g['reason']}")
for c in report["contested"]:
    print(f"contested {address(c)}")
