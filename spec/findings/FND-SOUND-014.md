---
id: FND-SOUND-014
title: The six PLYL resources are lists of byte pairs ending in 0 or 100, the shape an uncalled playlist routine reads
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x189B3..0x189CF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2660:0004..2660:0048
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2660:0061..2660:01AF
tool: hex inspection with Python 3.14.7, reading the GFF directory as FMT-GFF-001 describes; Capstone 5.0.7 16-bit disassembly, MZ relocations applied for a load image at segment 0x1000; Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1 (ReportBytePattern) and Python byte searches of the whole DSUN.EXE
environment: null
---

## Observation

`RESOURCE.GFF` holds six `PLYL` resources, stored one after another from file offset `0x189B3`:

| Resource | Bytes |
|---|---|
| `PLYL/0` | `01 FF 00` |
| `PLYL/10` | `02 FF 00` |
| `PLYL/50` | `07 FF 64` |
| `PLYL/51` | `05 FF 08 FF 07 FF 64` |
| `PLYL/52` | `06 FF 04 FF 09 FF 64` |
| `PLYL/53` | `04 FF 03 FF 64` |

No other GFF file of the installation holds a `PLYL` resource. A search of the whole of `DSUN.EXE`
finds neither the four bytes `PLYL` nor `CSEQ`; an earlier Ghidra search of the load image alone
found neither.

The far routine at `2660:0004` takes a far pointer and copies bytes from it into a buffer at
`DS:A354`, up to 80 bytes, stopping after it copies a 0 or `0x64` (100). It sets the word at
`DS:3480` to 0, calls `4A32:0011` (FND-SOUND-013) with the first byte, and sets the byte at
`DS:347D` to 1.

The far routine at `2660:0061` does nothing unless the byte at `4E71:0C3F` and `DS:347D` are not 0.
Keeping a state in the byte at `DS:347F`, it passes the byte after the current one to `4654:04FE`
with 6,000 and 6,001, the routine that plays a `BVOC` resource (FND-SOUND-007), calls
`56EF:00F7`, which stops the music (FND-SOUND-008), and, when the byte at `4E71:0C4A` equals
the word at `4E71:0C3B` plus 1, adds 2 to `DS:3480`. When the byte at the new index is 0 it stops
the music and clears `DS:347D`; when it is `0x64` it steps back 2 and calls the `BVOC` player with
the byte after the index; otherwise it calls `2660:0250` (FND-SOUND-013) with the word at
`4E71:0C3B` and the `BVOC` player with the byte before the index.

A search of the load image and the overlays finds no far call to `2660:0004`, `2660:0048` or
`2660:0061`.

## Interpretation

A `PLYL` resource is a playlist: pairs of a song number and a sound-effect number, where 255 is
no effect, ended by 0 to stop or by 100 to repeat the last pair. `2660:0004` starts such a list
and `2660:0061` steps through it as tracks end. The six lists name songs 1 to 9 only.

## Alternatives

Nothing shown ties the `PLYL` resources to `2660:0004`: no caller of the routine and no reference
to the tag were found, so the playlist code may be unused in this build or reached through a
pointer. The resources may be read some other way, and FND-PARTY-019 found that they do not
list characters. How `2660:0061` decides that a track has ended, from the words at `4E71:0C19`,
`4E71:0C3B` and `4E71:0C4A`, was not read.

## How to reproduce

List the `PLYL` resources of `RESOURCE.GFF` through its directory and read their bytes;
disassemble `2660:0004` to `2660:01AF`; search the load image for far calls to the three entries
and the overlays for `9A 04 00 68 00`, `9A 48 00 68 00` and `9A 61 00 68 00`.
