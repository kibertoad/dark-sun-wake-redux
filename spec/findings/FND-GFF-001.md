---
id: FND-GFF-001
title: Every GFF file opens with a 28-byte header that gives the directory's offset and size
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00..0x1C
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x00..0x1C
  - build: BLD-GOG-EN-1.1
    file: RGN001.GFF
    offset: 0x00..0x1C
  - build: BLD-GOG-EN-1.1
    file: CD:RESOURCE.GFF
    offset: 0x00..0x1C
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

All 26 installed `.GFF` files and all 25 `.GFF` files on the disc start with the same seven
little-endian fields:

- `0x00`: the ASCII bytes `GFFI`.
- `0x04`: `0x00030000` in every file.
- `0x08`: 28 in every file.
- `0x0C`: a file offset. In every file, every resource the directory lists (FND-GFF-002,
  FND-GFF-003) lies between offset 28 and this offset, and the directory starts here:
  `0x573CE9` in the installed `RESOURCE.GFF`, `0x28C3` in the installed `CHARSAVE.GFF`.
- `0x10`: the byte count from that offset to the end of the directory's gap list (FND-GFF-004):
  1,900 in `RESOURCE.GFF`, 1,300 in `CHARSAVE.GFF`. In 23 of the installed files and 22 of the
  disc's, the two fields add up to the file size. In `GPLDATA.GFF`, `OBJEX.GFF` and
  `RESOURCE.GFF`, in both copies, bytes follow (FND-GFF-004).
- `0x14`: 8 in the 20 region files `RGN*.GFF` and in the installed `CHARSAVE.GFF`; 0 in every
  other file, the disc's `CHARSAVE.GFF` included.
- `0x18`: 1 in every file but the region files and the installed `CHARSAVE.GFF`. The installed
  `CHARSAVE.GFF` has 117. The region files have 3 (`RGN001.GFF`) to 22 (`RGN0FF.GFF`), each file
  one more than the one before it in file-name order, the same in both copies.

## Interpretation

The header identifies the file as a GFF container of version `0x00030000` and gives where its
directory starts and how long it is. The field at `0x08` is the header's own size, which is also
where the stored data starts.

## Alternatives

The values at `0x14` and `0x18` follow the file's kind: 8 and a running number in the region
files, and 8 and 117 in the installed `CHARSAVE.GFF`, which differs from the disc's. They may be
a flag and a counter a writer updates, but nothing here shows what reads them. The field at
`0x08` could equally be the offset of the first stored byte; the two readings give the same value
in every shipped file.

## How to reproduce

Read the first 28 bytes of every `.GFF` file in the installation, and of every `.GFF` file in the
root of the ISO 9660 volume in track 1 of `game.gog` (2,352-byte sectors whose user data starts at
byte 24). Compare `0x0C + 0x10` with the file size, and compare the resource ranges from the
directory with `0x0C`.
