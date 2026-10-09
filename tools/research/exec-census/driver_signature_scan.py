"""Lists build files that the sound utility's driver install routine would accept (FND-EXE-492).

Usage: python -I driver_signature_scan.py INSTALL_DIR

1C08:0A25 in SOUND_DS.EXE installs an image whose bytes 3..8 are `DIGPAK` or whose bytes 2..5
are `Copy`. This walks every file of the install directory except the disc image, and every file
of the ISO 9660 volume on the MODE2/2352 data track of game.gog, and prints those that pass
either test, with their sizes. File bytes are read in memory only.
"""
import os
import sys

SECTOR, DATA = 2352, 24
root = sys.argv[1]


def accepts(head):
    return head[3:9] == b"DIGPAK" or head[2:6] == b"Copy"


hits, checked = [], 0
for dirpath, dirnames, filenames in os.walk(root):
    for name in filenames:
        full = os.path.join(dirpath, name)
        rel = os.path.relpath(full, root).replace("\\", "/")
        if rel == "game.gog":
            continue
        with open(full, "rb") as f:
            head = f.read(9)
        checked += 1
        if accepts(head):
            hits.append((rel, os.path.getsize(full)))

disc_checked = 0
with open(os.path.join(root, "game.gog"), "rb") as f:
    def sectors(lba, count):
        out = bytearray()
        for i in range(count):
            f.seek((lba + i) * SECTOR + DATA)
            out += f.read(2048)
        return bytes(out)

    pvd = sectors(16, 1)
    assert pvd[0] == 1 and pvd[1:6] == b"CD001"

    def walk(lba, length, prefix):
        global disc_checked
        data = sectors(lba, (length + 2047) // 2048)
        pos = 0
        while pos < length:
            n = data[pos]
            if n == 0:
                pos = (pos // 2048 + 1) * 2048
                continue
            ext = int.from_bytes(data[pos + 2:pos + 6], "little")
            size = int.from_bytes(data[pos + 10:pos + 14], "little")
            flags, idlen = data[pos + 25], data[pos + 32]
            ident = data[pos + 33:pos + 33 + idlen]
            pos += n
            if idlen == 1 and ident[0] in (0, 1):
                continue
            name = ident.decode("latin1").split(";")[0].rstrip(".")
            if flags & 2:
                walk(ext, size, f"{prefix}{name}/")
            else:
                disc_checked += 1
                if size >= 9 and accepts(sectors(ext, 1)[:9]):
                    hits.append((f"CD:{prefix}{name}", size))

    walk(int.from_bytes(pvd[158:162], "little"), int.from_bytes(pvd[166:170], "little"), "")

print(f"install files checked {checked}, disc files checked {disc_checked}, accepted {len(hits)}")
for path, size in sorted(hits):
    print(f"  {path} {size}")
