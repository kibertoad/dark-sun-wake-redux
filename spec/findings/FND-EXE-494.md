---
id: FND-EXE-494
title: Sound utility null timer registration and CS:0E1E calls run only through its DIGPAK function table
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1C08:0AF3..1C08:0AFF
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0000E200..0x0000E238
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1C08:13DC..1C08:1502
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

Segment 1C08 holds a table of four-byte pairs at CS:0D80 (file
0xE200..0xE238): a function number, then an offset in 1C08, ending at
number FFFF. Its pairs are 0064->13DC, 0065->145C, 0066->146A,
0068->14CF, 007B->169C, 007D->186D, 007E->18B5, 007C->18F9, 0078->1502,
0079->1628, 007A->1687, 0085->1742 and 0086->1830.

The only word in the load image equal to 0D80 is the immediate of
`mov word cs:[si+0128],0D80` at 1C08:0AF3, which FND-EXE-491's install
routine executes after an image passes the `DIGPAK` test and before it
stores CS into the same slot's segment word. The only words equal to
146A and 14CF are this table's entries for 0066 and 0068, and neither
offset has a near or far caller. The only word equal to 13DC is the entry
for 0064, and its only caller is the near call at 1C08:14A1 inside
1C08:146A.

FND-EXE-493's null timer registration is at 1C08:1485, inside 1C08:146A.
The two far calls through CS:0E1E are at 1C08:13FD, inside 1C08:13DC,
and at 1C08:14EC, inside 1C08:14CF; CS:0E1E is loaded from the DIGPAK
entry pointer just before each.

## Interpretation

1C08:146A and 1C08:14CF are entered only through FND-EXE-491's function
lookup on a driver slot that holds the CS:0D80 table, and 1C08:13DC only
through that lookup or from 1C08:146A, and that
slot is filled only for an image with `DIGPAK` at byte 3. FND-EXE-492
finds no such file in the installation or on the disc. In this build the
null timer registration and the CS:0E1E calls therefore never run, and the
null slot is never registered, so it is never dispatched. This settles
Q-EXE-016 without the call-order reading FND-EXE-493 left open. Code the
other table entries reach is not read here.

## Alternatives

Reading 1C08:146A as a direct entry ignores that no instruction calls it
and that its only reference is a table entry. Reading the table as always
installed ignores that its offset appears in one store, on the DIGPAK
branch. A stale CS:01B2 handle, the path FND-EXE-493 leaves open, could
name the null slot only after that slot was registered, which needs the
DIGPAK branch.

## How to reproduce

Require FND-EXE-350's SOUND_DS.EXE identity: length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57. Read the four-byte pairs at file
0xE200 until number FFFF. Search the load image (file 0x1400..0x1E5E0)
for words equal to 0D80, 13DC, 146A and 14CF, decoding the instruction that
holds each with locked Capstone 5.0.7 in sixteen-bit mode, and for near
and far calls to load offsets 0xD45C, 0xD4EA and 0xD54F (segment 1C08 starts at
load offset 0xC080). Decode load offsets 0xCADB..0xCB86 and
0xD45C..0xD582. Original bytes stay outside Git; no original process,
DOSBox or emulated call runs.
