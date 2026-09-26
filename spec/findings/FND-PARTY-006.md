---
id: FND-PARTY-006
title: Each CHAR record has a 34-byte PSST and a 9- or 15-byte SPST resource of the same number
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x44F..0x45E
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x48B..0x4AD
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1468..0x1471
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

The installed `CHARSAVE.GFF` holds one `PSST` and one `SPST` resource for each of its 19 `CHAR`
numbers (29 to 43 and 50 to 53), and none under another number; the disc copy holds the eight for
40 to 43 and 50 to 53 (FND-PARTY-005).

Every `PSST` resource is 34 bytes. Of the 646 bytes across the 19 resources, 475 are 0, 158 are
2, 5 are 4, 4 are 3 and 4 are 6. `PSST/40` is at `0x48B..0x4AD`.

`SPST` resources are 15 bytes, except `SPST/30`, `SPST/31` and `SPST/33`, which are 9 bytes.
`SPST/40` is at `0x44F..0x45E` and `SPST/30` at `0x1468..0x1471`. The 15-byte resources of records
29 and 32 and the 9-byte resource of record 33 are all zero; the others hold nonzero bytes from 1 to
132 among zero bytes, many of them a single bit such as 1, 2, 8, 32, 64 or 128.

## Interpretation

`PSST` and `SPST` are per-character records kept beside `CHAR` and `PSIN`, found by the same
resource number. The two `SPST` sizes match the two writers: the transfer utility writes a 9-byte
`SPST` (FND-PARTY-011) and the game's overlay 186 a 15-byte one (FND-PARTY-012), so records 30,
31 and 33 were last written by the transfer utility.

## Alternatives

The tag names and the manual (psionic powers, and spells per level) suggest that `PSST` holds
psionic powers and `SPST` spells, and the bit patterns in `SPST` would fit a set of known
spells, but nothing here shows it. No field of either resource is identified.

## How to reproduce

List the `PSST`, `SPST` and `CHAR` resources of both copies of `CHARSAVE.GFF` through their
directories, compare the numbers and sizes, and count the byte values of the `PSST` resources.
