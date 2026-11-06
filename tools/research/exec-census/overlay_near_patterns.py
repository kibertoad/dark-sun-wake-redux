"""Bounded near-transfer patterns for Q-PARTY-136's alternate overlay entries.

Usage: python -I overlay_near_patterns.py DSUN_EXE INVENTORY_TSV
       DESCRIPTOR+OFFSET [...] --control DESCRIPTOR+OFFSET@FILE_SITE [...]

Targets and sites are hexadecimal, descriptors decimal. Each queried overlay
requires a positive control at a separately established instruction start.
Scans every byte of the named flags-3 overlay code, not its fixups. Inventoried
linear instruction starts are labels, not reachability evidence. Reports only
unprefixed rel8/rel16 call/jump/Jcc/loop patterns with 16-bit IP arithmetic.
Far, indirect, table, dynamically changed and operand-size-32 transfers remain
outside this model. Nothing is executed or written.
"""
import argparse
import hashlib
from importlib.metadata import version
from pathlib import Path
import re
import struct

import capstone
import xxhash


def relative_patterns(data, start, end, origin, target):
    """Return complete unprefixed rel8/16 patterns wholly within [start,end)."""
    if not 0 <= start <= end <= len(data) or not 0 <= target <= 0xFFFF:
        raise ValueError("invalid scan bounds or target")
    found = []
    for site in range(start, end):
        op = data[site]
        size, displacement, kind = 0, 0, None
        if op in (0xE8, 0xE9) and site + 3 <= end:
            size = 3
            displacement = struct.unpack_from("<h", data, site + 1)[0]
            kind = "call16" if op == 0xE8 else "jump16"
        elif (op == 0xEB or 0x70 <= op <= 0x7F or 0xE0 <= op <= 0xE3) and site + 2 <= end:
            size = 2
            displacement = struct.unpack_from("<b", data, site + 1)[0]
            kind = "jump8" if op == 0xEB else "condition8" if op < 0x80 else "loop8"
        elif op == 0x0F and site + 4 <= end and 0x80 <= data[site + 1] <= 0x8F:
            size = 4
            displacement = struct.unpack_from("<h", data, site + 2)[0]
            kind = "condition16"
        if size and (site - origin + size + displacement) & 0xFFFF == target:
            found.append({"site": site, "size": size, "kind": kind})
    return found


def check_control(patterns, starts, expected_site):
    if expected_site not in starts or not any(row["site"] == expected_site for row in patterns):
        raise ValueError(f"positive control failed at file {expected_site:08X}")


def target_text(value):
    match = re.fullmatch(r"([0-9]+)\+([0-9A-Fa-f]{1,4})", value)
    if not match:
        raise ValueError("expected decimal DESCRIPTOR+hexadecimal OFFSET")
    return int(match[1]), int(match[2], 16)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source")
    parser.add_argument("inventory")
    parser.add_argument("targets", nargs="+")
    parser.add_argument("--control", action="append", required=True)
    args = parser.parse_args()
    if Path(args.source).stat().st_size != 634416:
        raise ValueError("not the approved installed DSUN.EXE size")
    body = Path(args.source).read_bytes()
    digest = xxhash.xxh3_128_hexdigest(body)
    if (len(body), digest) != (634416, "e296af55ba2ecde7e77f555c90f33d0b"):
        raise ValueError("not the approved installed DSUN.EXE")
    header = struct.unpack_from("<H", body, 8)[0] * 16
    last, pages = struct.unpack_from("<HH", body, 2)
    image_end = (pages - 1) * 512 + (last or 512)
    table, count = struct.unpack_from("<IH", body, image_end + 8)
    overlays = {}
    for descriptor in range(count):
        segment, _, flags, _ = struct.unpack_from("<HHHH", body, table + 8 * descriptor)
        if flags != 3:
            continue
        payload, size = struct.unpack_from("<IH", body, header + segment * 16 + 4)
        start = image_end + 16 + payload
        if not 0 < size <= 0x10000 or not 0 <= start < start + size <= len(body):
            raise ValueError("overlay code outside source bounds")
        overlays[descriptor] = (start, start + size)
    targets = [target_text(value) for value in args.targets]
    controls = []
    for value in args.control:
        name, site = value.split("@")
        descriptor, offset = target_text(name)
        controls.append((descriptor, offset, int(site, 16)))
    for descriptor, offset in targets + [(d, o) for d, o, _ in controls]:
        if descriptor not in overlays or offset >= overlays[descriptor][1] - overlays[descriptor][0]:
            raise ValueError("target or control outside declared overlay code")
    if {d for d, _ in targets} - {d for d, _, _ in controls}:
        raise ValueError("every queried overlay requires its own positive control")
    if Path(args.inventory).stat().st_size > 32 * 1024 * 1024:
        raise ValueError("inventory exceeds 32 MiB")
    inventory = Path(args.inventory).read_text(encoding="utf-8")
    decoder = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_16)
    starts, covered = set(), set()
    for line in inventory.splitlines()[1:]:
        columns = line.split("\t")
        for span in columns[2].split():
            low, high = span.split("..")
            if ":" in low:
                continue
            low, high = int(low, 16), int(high, 16)
            if not any(a <= low < high <= b for a, b in overlays.values()):
                raise ValueError("inventory overlay range outside declared code")
            for ins in decoder.disasm(body[low:high], low):
                starts.add(ins.address)
                covered.update(range(ins.address, ins.address + ins.size))
    # Validate all controls before printing results that could support a negative claim.
    for descriptor, offset, site in controls:
        low, high = overlays[descriptor]
        check_control(relative_patterns(body, low, high, low, offset), starts, site)
    normalized = inventory.replace("\r\n", "\n").encode("utf-8")
    print(f"source size={len(body)} xxh3={digest} decoder=Capstone module {capstone.__version__} distribution {version('capstone')}")
    print(f"inventory LF-sha256={hashlib.sha256(normalized).hexdigest()}")
    print("model: raw unprefixed rel8/rel16 patterns; instruction ownership does not prove reachability")
    print("excluded: far/indirect/table/dynamic/operand-size-32 transfers and unqueried code")
    for descriptor, offset, site in controls:
        print(f"control PASS {descriptor}+{offset:04X} at file {site:08X} inventoried-start")
    for descriptor, offset in targets:
        low, high = overlays[descriptor]
        hits = relative_patterns(body, low, high, low, offset)
        print(f"target {descriptor}+{offset:04X} code={low:08X}..{high:08X} patterns={len(hits)}")
        for row in hits:
            site = row["site"]
            ownership = "inventoried-start" if site in starts else "inside-instruction" if site in covered else "outside-decoded-ranges"
            print(f"  file {site:08X} +{site-low:04X} {row['kind']} {ownership}")


if __name__ == "__main__":
    main()
