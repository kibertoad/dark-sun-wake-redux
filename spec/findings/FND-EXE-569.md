---
id: FND-EXE-569
title: Only the overlay manager's startup looks for the FBOV pack header, in both DSUN.EXE editions
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:00B2..4AE5:00C3
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 4AD6:00B1..4AD6:00C2
tool: Capstone 5.0.7 16-bit decoding at every byte with Python 3.14.7 and xxhash 4.0.1 (tools/research/exec-census/pack_header_readers.py)
environment: null
---

## Observation

In each edition, every byte of the MZ load image and of each of the 49 overlays' code was decoded
as the start of a 16-bit instruction, and the instructions with an encoded immediate or
displacement equal to one of these words were listed: `0x4246` (`FB`), `0x564F` (`OV`), the low
word of the pack header's file offset (`0x7570` installed, `0x7430` disc), and that word plus 8
(`segment_table_offset`) and plus `0x0C` (`segment_count`).

In the installed `DSUN.EXE` there are two: `cmp word ptr [bp-0x14], 0x4246` at `4AE5:00B2` and
`cmp word ptr [bp-0x12], 0x564F` at `4AE5:00BE`. On the disc there are the same two at `4AD6:00B1`
and `4AD6:00BD`. They are the overlay manager startup's signature checks (FND-EXE-560 and, for
the disc's manager, FND-EXE-565), and are the search's positive control.

## Interpretation

No code other than the overlay manager's startup checks the pack header's signature or names its
file offset, so nothing else locates the header in the file to read `segment_table_offset` or
`segment_count`. The startup reads the header without those two fields (FND-EXE-560), and the
manager walks the table at its address in memory (FND-EXE-561, FND-EXE-567). The two fields are
not used at run time.

## Alternatives

The search finds constants encoded in instructions. A position kept as data, built by arithmetic
from other values, or reached by seeking back from the end of the file, where the pack ends
(FMT-EXE-001), is not found. Neither is a read through a program other than `DSUN.EXE`.

## How to reproduce

Run `python -I tools/research/exec-census/pack_header_readers.py <install dir>/DSUN.EXE <local
copy of the disc's DSUN.EXE>` from the commit that adds this finding, with the locked evidence
Python. It checks both files, finds each pack header at the end of the load image, and lists the
matching instructions in the load image and the overlays.
