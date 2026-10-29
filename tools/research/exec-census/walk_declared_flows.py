"""Writes the flow declarations that give a DSUN.EXE inventory the code a reach walk decodes outside it.

Usage: python -I walk_declared_flows.py PATH_TO_DSUN.EXE INVENTORY_TSV REACH_REPORT_JSON OUTPUT_JSON
       [--also INVENTORY_TSV ...]

Checks the installed DSUN.EXE's size and XXH3-128. Decodes each of the report's `decodedRanges`
linearly with Capstone in 16-bit mode, as walk_inventory_coverage.py does, and finds in each the
first instruction whose first byte lies in no range of the inventory, or of an inventory --also
names. It writes, in the layout tools/ghidra/ApplyDeclaredFlows.java reads:

- `jumps`: every table jump of the report's `indirectJumpDeclarations` that has rows, with its site,
  its table's start, count and stride, the evidence the walk gives for its bound, and the distinct
  targets of its rows;
- `decode`: for each decoded range with such an instruction, the half-open range from it to the
  end of the decoded range (ranges of one routine that meet are joined), with the walk routine
  that decoded it as `owner`, except ranges that start at one of the `notRows` starts of
  tools/ghidra/dsun-declared-entries.json.

Resident addresses are written segment:offset in the segment the report's `declaredRegions` give
them (load image at segment 0x1000), overlay addresses as eight-digit file offsets. The `returns`
and `fallThroughs` lists and the `about` and `evidence` fields are copied from OUTPUT_JSON when it exists, since they
are written by hand, and so are the `jumps` entries there that carry an `evidence` field (table
jumps the walk does not reach, declared from a finding) and whose site the walk does not declare.
Decode rows with an explicit nonempty evidence string are also preserved. They replace an exact
walk-generated range/owner match, so later regeneration cannot lose their evidence. A conflicting
owner or duplicate manual range is rejected. Unevidenced old decode rows are regenerated.
Nothing else is written and nothing is executed.
"""
import json
import os
import struct
import sys

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"


def merge_decode(generated, previous):
    """Keep evidenced repairs independent of whether the current walk discovers them."""
    manual = {}
    for row in previous:
        if "evidence" not in row:
            continue
        if not isinstance(row["evidence"], str) or not row["evidence"].strip():
            raise ValueError("Manual decode requires a nonempty evidence string")
        if any(not isinstance(row.get(key), str) or not row[key].strip() for key in ("range", "owner")):
            raise ValueError("Manual decode requires a range and owner")
        if row["range"] in manual:
            raise ValueError("Duplicate manual decode range: " + row["range"])
        manual[row["range"]] = row
    result, retained = [], set()
    for row in generated:
        repair = manual.get(row["range"])
        if repair is None:
            result.append(row)
            continue
        if repair["owner"] != row["owner"]:
            raise ValueError("Conflicting decode owner for " + row["range"])
        if row["range"] not in retained:
            result.append(repair)
            retained.add(row["range"])
    result.extend(row for key, row in manual.items() if key not in retained)
    return result


def main():
    import xxhash
    from capstone import CS_ARCH_X86, CS_MODE_16, Cs

    dsun, inventory, report, out = sys.argv[1:5]
    body = open(dsun, "rb").read()
    assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
    header = struct.unpack_from("<H", body, 8)[0] * 16
    cblp, pages = struct.unpack_from("<HH", body, 2)
    image_end = (pages - 1) * 512 + (cblp or 512)
    walk = json.load(open(report, encoding="utf-8"))
    segments = [(r["start"], r["end"], r["segment"]) for r in walk["declaredRegions"] if r["segment"] < 0x9000]


    def name(at, segment=None):
        if header <= at < image_end:
            for low, high, s in segments:
                if low <= at < high:
                    return f"{s:04X}:{at - header - (s - 0x1000) * 16:04X}"
            raise ValueError(f"resident byte 0x{at:X} in no declared region")
        return f"0x{at:08X}"


    def span(a, b):
        first = name(a)
        if ":" not in first:
            return f"{first}..0x{b:08X}"
        s = int(first.split(":")[0], 16)
        return f"{first}..{s:04X}:{b - header - (s - 0x1000) * 16:04X}"


    def file_offset(text):
        if ":" not in text:
            return int(text, 16)
        s, o = (int(p, 16) for p in text.split(":"))
        return (s - 0x1000) * 16 + o + header


    # A byte counts as covered only when every inventory given covers it.
    inventories = [inventory] + [sys.argv[k + 1] for k, v in enumerate(sys.argv) if v == "--also"]
    covered = bytearray(b"\x01" * len(body))
    for path in inventories:
        mine = bytearray(len(body))
        for line in open(path, encoding="utf-8").read().splitlines()[1:]:
            for r in line.split("\t")[2].split():
                a, b = (file_offset(x) for x in r.split(".."))
                mine[a:b] = b"\x01" * (b - a)
        covered = bytearray(x & y for x, y in zip(covered, mine))

    entries = json.load(open(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "ghidra",
                                          "dsun-declared-entries.json"), encoding="utf-8"))
    not_rows = {file_offset(s) for s in entries["notRows"]["starts"]}

    md = Cs(CS_ARCH_X86, CS_MODE_16)
    runs = []
    for r in walk["decodedRanges"]:
        # From the first instruction outside to the end of the decoded range, so that the views decode
        # the rest of the range in step with the walk as well.
        for i in md.disasm(body[r["start"]:r["end"]], r["start"]):
            if not covered[i.address]:
                runs.append([i.address, r["end"], r["routine"]])
                break
    # Join ranges that continue across the end of one decoded range into the next of the same routine.
    runs.sort()
    joined = []
    for run in runs:
        if joined and joined[-1][1] == run[0] and joined[-1][2] == run[2]:
            joined[-1][1] = run[1]
        else:
            joined.append(run)

    jumps = []
    for d in walk["indirectJumpDeclarations"]:
        rows = d.get("rows", [])
        if not rows:
            continue
        targets = sorted({row["target"] for row in rows})
        jumps.append({
            "site": name(d["site"]),
            "table": {"start": name(d["table"]["start"]) if d["table"]["start"] >= header else f"DS:{d['table']['start']:04X}",
                      "count": d["table"]["count"], "stride": d["table"]["stride"]},
            "bound": d["table"].get("evidence", d.get("evidence", "")),
            "targets": [name(t) for t in targets],
        })

    previous = json.load(open(out, encoding="utf-8")) if os.path.exists(out) else {}
    walk_sites = {j["site"] for j in jumps}
    jumps += [j for j in previous.get("jumps", []) if "evidence" in j and j["site"] not in walk_sites]
    result = {
        "build": "BLD-GOG-EN-1.1",
        "file": "DSUN.EXE",
        "about": previous.get("about", ""),
        "evidence": previous.get("evidence", []),
        "returns": previous.get("returns", []),
        "fallThroughs": previous.get("fallThroughs", []),
        "jumps": jumps,
        "decode": merge_decode(
            [{"range": span(a, b), "owner": name(o)} for a, b, o in joined if a not in not_rows],
            previous.get("decode", [])),
    }
    with open(out, "w", encoding="utf-8", newline="\n") as f:
        json.dump(result, f, indent=1)
        f.write("\n")
    print("jumps", len(jumps), "targets", sum(len(j["targets"]) for j in jumps), "decode", len(result["decode"]),
          "skipped notRows", sum(1 for a, _, _ in joined if a in not_rows))


if __name__ == "__main__":
    main()
