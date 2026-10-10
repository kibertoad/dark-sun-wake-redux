---
id: FND-PARTY-142
title: The six bytes at 0x5A of a CHAR record are the character's base ability scores, which overlay 197 +1E7B adjusts to set the current scores at 0x23
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008527B..0x00085769
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x53A..0x540
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x571..0x577
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/field_reads.py, overlay_listing.py, direct_callers.py; a scratch script reading the CHAR resources through the GFF directory as details_chunks.py does)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. A load copies a CHAR record's bytes `0x23` to `0x28` to `+0x19` of the character's
FMT-COMBAT-001 record and bytes `0x5A` to `0x5F` to `+0x15` of its FMT-COMBAT-002 record
(FND-PARTY-049, FND-PARTY-051). The generation finish copies the six score bytes at `+0x19` of
the combatant record to `+0x15` of the details record (FND-PARTY-070), and the scores it copies
are the rolled ones with the origin modifiers added (FND-PARTY-071).

**The shipped records.** In 18 of the 19 `CHAR` resources of the installed `CHARSAVE.GFF` the
bytes at `0x5A` to `0x5F` equal those at `0x23` to `0x28`. In resource 50 (`Ar'Anda`) the
strength at `0x23` (file `0x53A`) is 19 and the byte at `0x5A` (file `0x571`) is 18; the other five
agree. Resource 40, also
`Ar'Anda`, holds 18 in both.

**`field_reads.py <dsun> <inventory> --es --table 19c5 15 16 17 18 19 1a`** finds reads of those
details bytes in overlay 176 `+01FD`, overlay 197 `+1ED2`, `+1F9A` and `+1FB1`, overlay 204
`+1226` and `+1262`, overlay 209 `+17FB` and overlay 210 `+0D86` and `+0DA8`.

**Overlay 197 `+1E7B`** (trampoline `575A:0039`) copies the six bytes at `+0x15` of a character's
details record into two arrays of six words at `bp-0x33C` and `bp-0x348` (`+1EC2..+1EF4`). It then
goes through the 10-byte entries at `4E71:0251`, dispatching on the byte at `+2` of each through a
table at `cs:2386`; the cases set or raise words of the arrays, such as raising the first word of
`bp-0x33C` to a value capped at 24 (`+1FC8..+1FF4`) and adding the entry's byte at `+5` to the
first word of `bp-0x348`, kept within 3 and 24 (`+1FFD..+2035`). For each of the six positions it
stores a byte from one of the two arrays, or 3 for positions 3 and 4 under a flag, at `+0x19` of
the character's combatant record (`+22B0..+233D`), and then tests the stored byte against 0x19
(`+234F`). `direct_callers.py <dsun> 197+1E7B` finds it called from overlays 179, 189, 193 and
195.

## Interpretation

`unk_5a` holds the character's base scores: the six scores as generated, origin modifiers
included. The scores at `0x23` are the current ones, which overlay 197 `+1E7B` rebuilds from the
base scores whenever the entries at `4E71:0251` that it applies change; resource 50's strength
of 19 over a base of 18 is such a raised score. A save stores both, so a score raised by one of
those entries keeps its base beside it.

## Alternatives

- What the entries at `4E71:0251` are, and which of their codes raise or set which score, was not
  read case by case; their word at `+0` is compared with 0xEB, the first psionic power number
  (FND-PARTY-108), which suggests active spells and powers.
- The reads in overlays 176, 204, 209 and 210 were not read for what they do with the base scores.
- Resource 50 may differ for another reason, such as an edit of the shipped save.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `<dsun>` the installed
`DSUN.EXE` and `<inventory>` `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`: in
`tools/research/exec-census/`, run `field_reads.py <dsun> <inventory> --es --table 19c5 15 16 17
18 19 1a`, `overlay_listing.py <dsun> 197 8527B 85770` and `direct_callers.py <dsun> 197+1E7B`.
Read the directory of the installed `CHARSAVE.GFF` as `details_chunks.py` does and compare bytes
`0x23` to `0x28` with `0x5A` to `0x5F` of each `CHAR` resource.
