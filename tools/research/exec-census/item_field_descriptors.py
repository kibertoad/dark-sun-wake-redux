"""Read selected shipped FNFO descriptors relevant to attached-record research.

usage: python -I item_field_descriptors.py INSTALL_DIR
Fixed fingerprinted inputs only; no execution or writes. FND-PARTY-052 gives
the FNFO source spans, and 2D40:06C4 indexes 196-byte kind rows.
"""
import json
from pathlib import Path
import struct
import sys
import xxhash

if len(sys.argv) != 2:
    raise SystemExit("usage: item_field_descriptors.py INSTALL_DIR")
game = Path(sys.argv[1])
sources = {}
for name, size, digest in (
    ("DSUN.EXE", 634416, "e296af55ba2ecde7e77f555c90f33d0b"),
    ("OBJEX.GFF", 6816516, "e08d3cac153c547f6804ca27b155915d"),
):
    with (game / name).open("rb") as stream:
        body = stream.read(size + 1)
    if len(body) != size or xxhash.xxh3_128_hexdigest(body) != digest:
        raise SystemExit(f"{name} differs from the approved installed source")
    sources[name] = body
rows = []
for kind, field in ((1, 0), (1, 1), (1, 2), (1, 3), (1, 4),
                    (2, 4), (2, 15), (2, 16), (2, 17)):
    descriptor = 0x1FF48 + (kind - 1) * 196 + field * 2
    type_at = 0x2031C + field
    record_offset = struct.unpack_from("<h", sources["OBJEX.GFF"], descriptor)[0]
    field_type = sources["OBJEX.GFF"][type_at]
    width_at = 0x4D40C + field_type
    rows.append({"kind": kind, "field": field, "record_offset": record_offset,
                 "type": field_type, "width": sources["DSUN.EXE"][width_at],
                 "descriptor_file_offset": f"0x{descriptor:08X}",
                 "type_file_offset": f"0x{type_at:08X}",
                 "width_file_offset": f"0x{width_at:08X}"})
print(json.dumps(rows, indent=2))
