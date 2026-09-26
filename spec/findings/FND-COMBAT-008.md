---
id: FND-COMBAT-008
title: An overlay 182 routine marks the four party records and preloads the status panel images while the word at 57E0:0DAB is 2
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:0DAB..57E0:0DAD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:0F3B..57E0:0F43
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:0000
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1; Capstone 5.0.7 through Python 3.14.7
environment: null
---

## Observation

Both routines are in overlay 182, whose code starts at `DSUN.EXE+0x00068850` and whose resident
header segment is `56BD` (the location above, since the routines have no resident address).

The routine at `DSUN.EXE+0x00069090` (overlay offset `0x0840`) loads `RESOURCE.GFF#BMP/19000` into
the far pointer at `57E0:0F3B` when that pointer is 0, and then `RESOURCE.GFF#BMP/19003` into the
far pointer at `57E0:0F3F` when that one is 0 and the first load succeeded. The routine at overlay
offset `0x088B` frees both. `57E0:0F3F` is the pointer the resident panel routine draws from
(FND-COMBAT-004).

The routine at `DSUN.EXE+0x0006A248` (offset `0x19F8`) clears the bytes `57E0:0DA4` and
`57E0:0DA3`. When the word at `57E0:0DAB` is 2, it visits records 0 to 3 of the 49-byte records at
the far pointer `57E0:19C9` (FMT-COMBAT-001) and sets byte `0x14` to 1 in each whose word `0x06` is
not 0 and whose byte `0x14` is 0. It then calls the routine at offset `0x0840`, a routine at
`362C:06B9`, two routines with the word at `57E0:140C`, and a routine of overlay 200 with the
argument `0x92E0`. Afterwards it goes on only while `57E0:0DAB` is 2 or 3.

The routine at offset `0x19F8` has one direct caller: the case for selector `0x7FC` of the routine
at offset `0x103D` of overlay 183 (FND-COMBAT-010).

## Interpretation

When the state at `57E0:0DAB` is 2, the game marks every present party member with byte `0x14`
set to 1 and loads the two panel images. The panel then draws the condition line only for a
character whose byte `0x14` is 1 (FND-COMBAT-022), so this reads as the start of a combat.

## Alternatives

The legacy record gave the routines at their overlay-mapped addresses `7393:0560` and `74BB:0498`
and read the second as iterating four records "conditionally changing byte `0x14`" without saying
how. That `57E0:0DAB` value 2 means combat is not shown; FND-COMBAT-011 lists the other values
the word takes.

## How to reproduce

Disassemble overlay 182 from its first byte at file offset `0x68850` for 9,400 bytes and read the
code at offsets `0x0840`, `0x088B` and `0x19F8`. Overlay segment words in far calls resolve through
the segment table at file offset `0x4B080`, record `word >> 3`, whose first word plus `0x1000` is
the segment (`0x0640` is record 200, `5773`). The overlay map reporter converts the legacy mapped
addresses.
