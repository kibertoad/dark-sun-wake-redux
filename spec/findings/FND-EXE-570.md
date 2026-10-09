---
id: FND-EXE-570
title: The disc's DSUN.EXE is version 1.0 and the installed one version 1.1, and PATCH.RTP records the disc's sizes as the old and the installed sizes as the new for seven differing files
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D77B..0x0004D786
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00050534..0x0005055A
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x0004D6DC..0x0004D6E7
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x0005041A..0x0005043F
  - build: BLD-GOG-EN-1.1
    file: PATCH.RTP
    offset: 0x00000032..0x0001629F
tool: Python 3.14.7 with xxhash 4.0.1 (tools/research/exec-census/edition_versions.py)
environment: null
---

## Observation

The installed `DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`) holds the
text `VERSION 1.1` at `0x0004D77B` and `Mel Real Mode Version 2.2.7, 10/14/94` at `0x00050534`.
The disc's `DSUN.EXE` (634,704 bytes, `318cd5ec0559901add3780097162a919`) holds `VERSION 1.0` at
`0x0004D6DC` and `Mel Real Mode Version 2.2.5, 4/28/94` at `0x0005041A`. Neither file holds any
other run of printable bytes starting with `VERSION ` or `Mel Real Mode Version`.

The installed `PATCH.RTP` (112,568 bytes, `50bb466a0b03d74f97810d6f591daf46`) names seven of the
eight files that differ between the disc and the installation. For each, the 32-bit
little-endian word 16 bytes after the first occurrence of the name, followed by a NUL, equals the
disc copy's size, and the word 50 bytes after it equals the installed copy's size:

| Name at | File | +16 | +50 |
|---|---|---|---|
| `0x00000032` | `CHARSAVE.GFF` | 3,864 | 11,735 |
| `0x00000FB8` | `DSUN.EXE` | 634,704 | 634,416 |
| `0x0000EBD6` | `GPLDATA.GFF` | 2,191,945 | 2,191,945 |
| `0x00012691` | `OBJEX.GFF` | 6,816,516 | 6,816,516 |
| `0x00012FA0` | `RESOURCE.GFF` | 5,782,746 | 5,724,669 |
| `0x0001531D` | `SOUND.INI` | 34,542 | 54,669 |
| `0x00016269` | `STDPATCH.AD` | 5,162 | 4,662 |

The name occurs again 34 bytes after its first occurrence in each record. `SOUND.BAT`, the eighth
differing file, is not named.

## Interpretation

The two copies of `DSUN.EXE` are two versions of one program: the disc's is 1.0 and the
installed one 1.1, which links a later release of the sound library. FND-EXE-565 finds the
disc's overlay manager to be the installed one's code shifted by one byte, and FMT-EXE-002 finds
229 segment descriptors in each, with the same number of each `flags` value and the same overlay
indexes. `PATCH.RTP` is laid out as one record per patched
file, giving the old size and then the new: it describes an update from the disc's files to
the installed ones, which is the 1.1 update the installation's `README.TXT` (SRC-README-1.1)
names. The installed files are the newer version.

## Alternatives

The record layout of `PATCH.RTP` is read from where the sizes fall, not from the program that
applies it. `PATCH.EXE` was not read, so whether applying `PATCH.RTP` to the disc's files gives
the installed files byte for byte is not shown; only the sizes match. A `VERSION` string is text
the program may print; where it is read was not traced.

## How to reproduce

Run `python -I tools/research/exec-census/edition_versions.py <install dir> <local copy of the
disc's DSUN.EXE>` from the commit that adds this finding, with the locked evidence Python. It
checks the three files against the build manifest, prints the version strings with their
offsets, and prints the two words after each name in `PATCH.RTP` beside the manifest's sizes.
