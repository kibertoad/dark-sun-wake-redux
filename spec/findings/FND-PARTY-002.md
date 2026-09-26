---
id: FND-PARTY-002
title: Each CHAR record has a one-byte PSIN resource of the same number, holding 1, 2, 4, 5, 6 or 7
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x513..0x517
  - build: BLD-GOG-EN-1.1
    file: CD:CHARSAVE.GFF
    offset: 0x513..0x517
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

The installed `CHARSAVE.GFF` holds 19 `PSIN` resources, with the same 19 numbers as its `CHAR`
resources (29 to 43 and 50 to 53), and no `PSIN` without a `CHAR` or `CHAR` without a `PSIN`.
Every `PSIN` resource is one byte. The values are:

| Value | Resources |
|---|---|
| 1 | 29, 52, 53 |
| 2 | 37, 38, 39, 42, 43 |
| 4 | 32, 41, 51 |
| 5 | 33 |
| 6 | 31 |
| 7 | 30, 34, 35, 36, 40, 50 |

Each value is a nonzero combination of bits 0 to 2; 3 does not occur. `PSIN/40` to `PSIN/43` are
the bytes at `0x513..0x517`, in the installed file and in the disc's `CD:CHARSAVE.GFF`, which
holds the same eight `PSIN` resources 40 to 43 and 50 to 53 with the same values.

## Interpretation

Each character has one `PSIN` byte, found by the character's resource number. The three bits and
the manual's three psionic disciplines (SRC-MANUAL-1994, pages 8 and 9: a psionicist has all
three, anyone else picks one) suggest one bit per discipline.

## Alternatives

The values 5 and 6 have two bits set, which a created non-psionicist would not have under the
manual's description, so the bits may mean something else, or these records were not made on the
creation screen. Which bit stands for which discipline, if any, is not shown. Record 40, whose
value is 7, matches the party member the captures show with the psionicist class, which fits
all three bits for a psionicist (FND-PARTY-020). The fourth member's Use screen shows the
caption Metabolic under PSIONIC (FND-MAGIC-001), and record 43, which the party loader puts in
the fourth slot (FND-PARTY-013), holds 2, so bit 1 may be psychometabolism. The game's overlay
186 reads and writes one `PSIN` byte per character (FND-PARTY-012), but what it does with the
value has not been read.

## How to reproduce

List the `PSIN` and `CHAR` resources of both copies of `CHARSAVE.GFF` through their directories
(FMT-GFF-001), compare the numbers, and read each `PSIN` byte.
