"""Read fixed startup-layout words of the fingerprinted installed DSUN.EXE.

usage: python -I startup_layout.py PATH_TO_DSUN.EXE
No execution or writes. Output includes shipped offsets for the stated fields.
"""
import json
from pathlib import Path
import struct
import sys
import xxhash

if len(sys.argv) != 2:
    raise SystemExit("usage: startup_layout.py PATH_TO_DSUN.EXE")
with Path(sys.argv[1]).open("rb") as source:
    body = source.read(634417)
if len(body) != 634416 or xxhash.xxh3_128_hexdigest(body) != "e296af55ba2ecde7e77f555c90f33d0b":
    raise SystemExit("not the approved installed DSUN.EXE")
header = struct.unpack_from("<H", body, 8)[0] * 16
fields = [
    ("MZ.initial_ss_relative", 14),
    ("MZ.initial_sp", 16),
    ("startup.word_384e", header + (0x57E0 - 0x1000) * 16 + 0x384E),
    ("startup.word_37ec", header + (0x57E0 - 0x1000) * 16 + 0x37EC),
    ("startup.word_37c2", header + (0x57E0 - 0x1000) * 16 + 0x37C2),
]
print(json.dumps({"header_bytes": header, "fields": [
    {"name": name, "file_offset": f"0x{offset:08X}",
     "value": struct.unpack_from("<H", body, offset)[0]}
    for name, offset in fields
]}, indent=2))
