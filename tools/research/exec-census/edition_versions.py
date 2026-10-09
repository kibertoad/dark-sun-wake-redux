"""Compares the installed and disc DSUN.EXE versions and reads PATCH.RTP's size records (FND-EXE-570).

Usage: python -I edition_versions.py INSTALL_DIR DISC_DSUN.EXE

Checks the size and XXH3-128 of the installed DSUN.EXE, the disc's DSUN.EXE and the installed
PATCH.RTP against the build manifest. Prints every run of printable bytes in each DSUN.EXE that
starts with "VERSION " or "Mel Real Mode Version", with its file offset. In PATCH.RTP, finds each
of the eight files that differ between the disc and the installation, and prints the 32-bit
little-endian words 16 and 50 bytes after the first occurrence of its name, with the disc and
installed sizes the manifest gives. Nothing is written or executed.
"""
import re
import struct
import sys
from pathlib import Path

import xxhash

MANIFEST = Path(__file__).resolve().parents[3] / "spec/builds/BLD-GOG-EN-1.1.files.yaml"
NAMES = ["DSUN.EXE", "RESOURCE.GFF", "GPLDATA.GFF", "OBJEX.GFF", "CHARSAVE.GFF", "SOUND.INI", "SOUND.BAT",
         "STDPATCH.AD"]

records, path = {}, None
for line in MANIFEST.read_text(encoding="utf-8").splitlines():
    m = re.match(r"\s*- path: (.+)$", line)
    if m:
        path = m.group(1)
        records[path] = {}
    m = re.match(r"\s+(size|xxh3): (\S+)$", line)
    if m and path:
        records[path][m.group(1)] = m.group(2)


def load(file, name):
    body = Path(file).read_bytes()
    want = records[name]
    assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (int(want["size"]), want["xxh3"]), f"{file} is not {name}"
    return body


install = Path(sys.argv[1])
for name, body in (("DSUN.EXE", load(install / "DSUN.EXE", "DSUN.EXE")),
                   ("CD:DSUN.EXE", load(sys.argv[2], "CD:DSUN.EXE"))):
    for m in re.finditer(rb"[ -~]{6,}", body):
        if m.group().startswith((b"VERSION ", b"Mel Real Mode Version")):
            print(f"{name} 0x{m.start():08X}..0x{m.end():08X} {m.group().decode()!r}")
patch = load(install / "PATCH.RTP", "PATCH.RTP")
for name in NAMES:
    at = patch.find(name.encode() + b"\0")
    if at < 0:
        print(f"PATCH.RTP: {name} not named")
        continue
    old, new = (struct.unpack_from("<I", patch, at + k)[0] for k in (16, 50))
    print(f"PATCH.RTP 0x{at:08X} {name}: +16 {old}, +50 {new}; disc {records['CD:' + name]['size']},"
          f" installed {records[name]['size']}")
