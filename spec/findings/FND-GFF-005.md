---
id: FND-GFF-005
title: The 26 installed GFF files hold 16,168 resources under 42 tags, with no shared bytes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x573CE9..0x574455
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x2141E5..0x214371
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x675506..0x67748A
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x28C3..0x2DD6
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

Read as FND-GFF-001 to FND-GFF-004 describe, the directories of the 26 installed `.GFF` files list
16,168 resources: 1,858 in `RESOURCE.GFF`, 535 in `GPLDATA.GFF`, 10,532 in `OBJEX.GFF`, 98 in
`CHARSAVE.GFF`, one each in `DARKRUN.GFF` and `DARKSAVE.GFF`, and 3,143 in the 20 region files.
The 25 files on the disc list 16,097. The `GFFI` resources count among them.

- The installed files use 42 tags, the disc's 39. Every tag byte is a printable ASCII character.
  Seven tags end in one space, which pads a three-letter name: `ADV `, `ALL `, `BMP `, `GPL `,
  `MAP `, `MAS ` and `PAL `. No tag has a space anywhere else.
- Resource numbers run from 0 to 32,003. Within one tag table, no number occurs twice.
- No resource has a size of 0, and no two resources in a file share a byte.

## Interpretation

A resource is identified within its file by its tag and its number, and each has bytes of its own.

## Alternatives

Two tags in different files, or the same tag in two files, name separate resources; nothing here
shows how the game decides which file to look in for a tag.

## How to reproduce

Read every directory of the installation and of the disc's ISO 9660 volume, expand the indexed
tables, and count resources per file and per tag. Collect the tags and check each byte. Sort all
resources of a file by offset and check that no range overlaps the next.
