"""Writes the overlay plan AddFbovOverlayBlocks.java reads, for a single import of an FBOV executable.

Usage: python -I fbov_overlay_plan.py SHIPPED_EXE MAPPED_EXE MAPPING_JSON OUT_JSON

MAPPING_JSON is the ReportFbovOverlayMap.ps1 output and MAPPED_EXE the New-FbovMappedImage.ps1 copy.
For each overlay it applies the fixup list to the shipped code (each fixup word is a descriptor
index times 8 and becomes that descriptor's segment plus 0x1000) and checks the result against
the mapped image's bytes after MZ relocation, printing any overlay that differs. The plan names
each overlay's file offset, length, mapped start, fixup words and trampoline targets (FMT-EXE-003,
FMT-EXE-004). Addresses and counts only; nothing is executed.
"""
import json
import struct
import sys


def mz(path):
    body = open(path, "rb").read()
    header = struct.unpack_from("<H", body, 8)[0] * 16
    cblp, pages, count = struct.unpack_from("<HHH", body, 2)
    reloc_at = struct.unpack_from("<H", body, 0x18)[0]
    image = bytearray(body[header:(pages - 1) * 512 + (cblp or 512)])
    for k in range(count):
        off, seg = struct.unpack_from("<HH", body, reloc_at + 4 * k)
        at = seg * 16 + off
        struct.pack_into("<H", image, at, (struct.unpack_from("<H", image, at)[0] + 0x1000) & 0xFFFF)
    return body, header, image

body, header, _ = mz(sys.argv[1])
_, _, mapped = mz(sys.argv[2])
mapping = json.load(open(sys.argv[3], encoding="utf-8-sig"))
fbov = int(mapping["FbovFileOffset"], 16)
table_off, count = struct.unpack_from("<IH", body, fbov + 8)
segs = [struct.unpack_from("<H", body, table_off + 8 * i)[0] for i in range(count)]
plan, mismatches = [], 0
for o in mapping["Overlays"]:
    code_at, size, fix = int(o["CodeFileOffset"], 16), o["CodeBytes"], o["FixupBytes"]
    code = bytearray(body[code_at:code_at + size])
    writes = []
    for (off,) in struct.iter_unpack("<H", body[code_at + size:code_at + size + fix]):
        word = struct.unpack_from("<H", code, off)[0]
        value = (segs[word // 8] + 0x1000) & 0xFFFF
        struct.pack_into("<H", code, off, value)
        writes.append([off, value])
    seg, off = (int(x, 16) for x in o["MappedCodeStart"].split(":"))
    lin = (seg - 0x1000) * 16 + off
    if bytes(mapped[lin:lin + size]) != bytes(code):
        mismatches += 1
        diff = [i for i in range(size) if mapped[lin + i] != code[i]]
        print("mismatch", o["Descriptor"], len(diff), diff[:8])
    hdr = header + segs[o["Descriptor"]] * 16
    targets = sorted({struct.unpack_from("<H", body, hdr + 0x20 + 5 * t + 2)[0] for t in range(o["TrampolineCount"])})
    plan.append({"descriptor": o["Descriptor"], "fileOffset": code_at, "length": size,
                 "start": o["MappedCodeStart"], "writes": writes, "targets": targets})
json.dump(plan, open(sys.argv[4], "x"), indent=0)
print("overlays", len(plan), "mismatches", mismatches, "writes", sum(len(p["writes"]) for p in plan),
      "targets", sum(len(p["targets"]) for p in plan))
