"""Bounded linear tail census for the installed DSUN.EXE overlay inventory.

Usage: python -I inventory_tails.py DSUN.EXE INVENTORY.tsv [--view file|mapped]
Reports metadata only. A terminal instruction is not a complete function reading.
"""
import argparse
import csv
import hashlib
import importlib.metadata
import io
import json
from pathlib import Path
import re
import struct
import sys
from collections import Counter

SOURCE_SIZE = 634416
SOURCE_XXH3 = "e296af55ba2ecde7e77f555c90f33d0b"
MAX_METADATA = 2 * 1024 * 1024
MAX_ROWS = 16384
MAX_RANGES = 65536


def read_limited(path, limit):
    if not isinstance(limit, int) or isinstance(limit, bool) or not 0 < limit <= 16 * 1024 * 1024:
        raise ValueError("Invalid input byte limit")
    if path.stat().st_size > limit:
        raise ValueError("Input exceeds its byte limit")
    with path.open("rb") as stream:
        data = stream.read(limit + 1)
    if len(data) > limit:
        raise ValueError("Input exceeds its byte limit")
    return data


def overlay_regions(body):
    """Read only the existing MZ/FBOV code-bound contracts, with explicit bounds."""
    def span(at, size, limit=None):
        if limit is None:
            limit = len(body)
        if at < 0 or size < 0 or at > limit - size:
            raise ValueError("MZ/FBOV field or payload outside its boundary")

    def unpack(fmt, at, limit=None):
        span(at, struct.calcsize(fmt), limit)
        return struct.unpack_from(fmt, body, at)

    if len(body) > 16 * 1024 * 1024 or body[:2] != b"MZ":
        raise ValueError("Expected a bounded MZ source")
    last, pages = unpack("<HH", 2)
    header = unpack("<H", 8)[0] * 16
    if not pages or last > 512:
        raise ValueError("Invalid MZ page counts")
    image_end = (pages - 1) * 512 + (last or 512)
    if not 28 <= header <= image_end <= len(body):
        raise ValueError("Invalid MZ initialized-image boundary")
    fbov = (image_end + 15) // 16 * 16
    span(fbov, 16)
    if body[fbov:fbov + 4] != b"FBOV":
        raise ValueError("Missing aligned FBOV envelope")
    payload, table, count = unpack("<III", fbov + 4)
    span(fbov, 16 + payload)
    if not 1 <= count <= 4096 or table < header:
        raise ValueError("Invalid FBOV descriptor count or table")
    span(table, count * 8, image_end)
    regions = []
    for index in range(count):
        segment, end, flags, start = unpack("<HHHH", table + index * 8, image_end)
        if flags != 3:
            continue
        at = header + segment * 16
        span(at, 32, image_end)
        if start or body[at:at + 2] != b"\xCD\x3F":
            raise ValueError("Invalid overlay header")
        offset, code, fixups, stubs = unpack("<IHHH", at + 4, image_end)
        if not code or fixups % 2 or end < 32 + stubs * 5:
            raise ValueError("Invalid overlay code, fixup or trampoline length")
        span(at, end, image_end)
        low = fbov + 16 + offset
        span(low, code + fixups, fbov + 16 + payload)
        regions.append({"descriptor": index, "start": low, "end": low + code})
    regions.sort(key=lambda r: r["start"])
    if not regions or any(a["end"] > b["start"] for a, b in zip(regions, regions[1:])):
        raise ValueError("Empty or overlapping overlay code regions")
    return header, image_end, regions


def analyze(body, text, view="file"):
    """Classify the final linear range, preserving every undecoded boundary."""
    from capstone import Cs, CS_ARCH_X86, CS_MODE_16, CS_GRP_RET, CS_GRP_IRET
    from capstone.x86_const import X86_INS_JMP, X86_INS_LJMP

    if (view not in ("file", "mapped") or len(text) > MAX_METADATA
            or len(text.encode("utf-8")) > MAX_METADATA):
        raise ValueError("Invalid view or oversized inventory metadata")
    header, image_end, regions = overlay_regions(body)

    def address(value, mapped):
        if mapped and re.fullmatch(r"[0-9a-fA-F]{4}:[0-9a-fA-F]{4}", value):
            segment, offset = (int(p, 16) for p in value.split(":"))
            return header + (segment - 0x1000) * 16 + offset
        if not mapped and re.fullmatch(r"0x[0-9a-fA-F]{8}", value):
            return int(value, 16)
        raise ValueError("Inventory address does not match the explicit view")

    reader = csv.DictReader(io.StringIO(text), delimiter="\t")
    if reader.fieldnames != ["start", "size", "ranges"]:
        raise ValueError("Expected start, size and ranges TSV columns")
    rows, skipped, total_ranges, row_count = [], 0, 0, 0
    for raw in reader:
        row_count += 1
        if row_count > MAX_ROWS or set(raw) != set(reader.fieldnames) or any(v is None for v in raw.values()):
            raise ValueError("Inventory row count or TSV width is invalid")
        if not re.fullmatch(r"[1-9][0-9]*", raw["size"]) or int(raw["size"]) > len(body):
            raise ValueError("Invalid inventory body size")
        if view == "file" and ":" in raw["start"]:
            resident = address(raw["start"], True)
            if not header <= resident < image_end:
                raise ValueError("Segmented overlay rows require --view mapped")
            skipped += 1
            continue
        start = address(raw["start"], view == "mapped")
        region = next((r for r in regions if r["start"] <= start < r["end"]), None)
        if region is None:
            raise ValueError("Overlay entry outside source code regions")
        spans = []
        for part in raw["ranges"].split():
            pair = part.split("..")
            if len(pair) != 2:
                raise ValueError("Malformed inventory body range")
            low, high = (address(p, view == "mapped") for p in pair)
            if not region["start"] <= low < high <= region["end"]:
                raise ValueError("Body range outside its entry's overlay code")
            spans.append((low, high))
            total_ranges += 1
            if total_ranges > MAX_RANGES:
                raise ValueError("Inventory range limit exceeded")
        spans.sort()
        if (not spans or any(a[1] > b[0] for a, b in zip(spans, spans[1:]))
                or sum(b - a for a, b in spans) != int(raw["size"])
                or not any(a <= start < b for a, b in spans)):
            raise ValueError("Invalid body ranges, size or entry inclusion")
        rows.append({"start": start, "spans": spans, "region": region})
    if not rows:
        raise ValueError("No overlay rows selected; no census was made")
    owners = [-1] * len(body)
    entries = set()
    for index, row in enumerate(rows):
        if row["start"] in entries:
            raise ValueError("Duplicate overlay entry")
        entries.add(row["start"])
        for low, high in row["spans"]:
            if any(owner != -1 for owner in owners[low:high]):
                raise ValueError("Overlapping inventory bodies")
            owners[low:high] = [index] * (high - low)
    decoder = Cs(CS_ARCH_X86, CS_MODE_16)
    decoder.detail = True
    results = []
    for row in rows:
        low, end = row["spans"][-1]
        cursor, last = low, None
        for instruction in decoder.disasm(body[low:end], low):
            if instruction.address != cursor:
                break
            cursor += instruction.size
            last = instruction
        next_owner = None
        if cursor != end or last is None:
            status = "incomplete_decode"
        elif last.group(CS_GRP_RET) or last.group(CS_GRP_IRET):
            status = "terminal_return"
        elif last.id in (X86_INS_JMP, X86_INS_LJMP):
            status = "terminal_jump"
        elif end == row["region"]["end"]:
            status = "fallthrough_code_end"
        elif end in entries:
            status = "fallthrough_adjacent_entry"
            next_owner = end
        elif owners[end] != -1:
            status = "fallthrough_other_body"
            next_owner = rows[owners[end]]["start"]
        else:
            status = "fallthrough_unowned"
        results.append({"start": f"0x{row['start']:08X}", "end": f"0x{end:08X}",
                        "lastRangeStart": f"0x{low:08X}", "decodedEnd": f"0x{cursor:08X}",
                        "lastInstruction": f"0x{last.address:08X}" if last else None,
                        "nextOwner": f"0x{next_owner:08X}" if next_owner is not None else None,
                        "descriptor": row["region"]["descriptor"], "status": status})
    return {"version": 1, "view": view, "rowsChecked": len(rows), "residentRowsSkipped": skipped,
            "overlayRegions": len(regions), "counts": dict(Counter(r["status"] for r in results)),
            "scope": "Linear decode of each overlay body's final range; not reachability or a complete reading.",
            "rows": results}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source")
    parser.add_argument("inventory")
    parser.add_argument("--view", choices=("file", "mapped"), default="file")
    args = parser.parse_args()
    try:
        import capstone
        import xxhash
        if importlib.metadata.version("capstone") != "5.0.9":
            raise ValueError("Restore the locked Capstone distribution before running this census")
        source, inventory = Path(args.source), Path(args.inventory)
        body = read_limited(source, SOURCE_SIZE)
        if len(body) != SOURCE_SIZE:
            raise ValueError("Source size is not the supported installed DSUN.EXE")
        if xxhash.xxh3_128_hexdigest(body) != SOURCE_XXH3:
            raise ValueError("Source hash is not the supported installed DSUN.EXE")
        text = read_limited(inventory, MAX_METADATA).decode("utf-8-sig")
        report = analyze(body, text, args.view)
        report.update(sourceXxh3=SOURCE_XXH3,
                      sourceByteLength=len(body), pythonVersion=sys.version.split()[0],
                      toolSha256=hashlib.sha256(Path(__file__).read_bytes()).hexdigest(),
                      inventorySha256=hashlib.sha256(text.replace("\r\n", "\n").encode()).hexdigest(),
                      capstoneDistribution=importlib.metadata.version("capstone"),
                      capstoneBinding=capstone.__version__, capstoneNative=list(capstone.cs_version()))
        json.dump(report, sys.stdout, indent=2)
        print()
        return 0
    except (ValueError, OSError, ImportError, csv.Error) as error:
        print(f"inventory tail census: {error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    sys.exit(main())
