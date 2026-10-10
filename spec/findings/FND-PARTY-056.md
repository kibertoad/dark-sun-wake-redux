---
id: FND-PARTY-056
title: In the 19 stored CHAR records, bytes 0x57, 0x58 and 0x59 hold origins 1 to 8, genders 1 and 2 and alignments 1 to 8, which name the four captured characters' origin, gender and alignment, and bytes 0x60 to 0x62 hold class codes from 1 to 17
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x73..0x82
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x188..0x197
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x2BE..0x2CD
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x391..0x3A0
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x56E..0x57D
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x76A..0x779
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x85E..0x86D
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0xB20..0xB2F
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0xDC5..0xDD4
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0xF0E..0xF1D
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x10A9..0x10B8
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x117F..0x118E
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x13EC..0x13FB
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x14ED..0x14FC
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1702..0x1711
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x19BB..0x19CA
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1B9F..0x1BAE
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x220A..0x2219
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x250E..0x251D
tool: a Python 3.14.7 reading of CHARSAVE.GFF through its directory (FMT-GFF-001, FMT-GFF-002)
environment: null
---

## Observation

In each of the 19 `CHAR` resources of the installed `CHARSAVE.GFF`, the bytes at `0x57`, `0x58`,
`0x59`, `0x60` to `0x62` and `0x63` to `0x65` are bytes `0x12`, `0x13`, `0x14`, `0x1B` to `0x1D` and
`0x1E` to `0x20` of the type-3 chunk's data (FND-PARTY-052). The names in brackets are the entries
of the tables at `DS:1144`, `DS:113C` and `DS:119C` that FND-PARTY-055 counts from 1.

| Resource | `0x57` | `0x58` | `0x59` | `0x60` to `0x62` | `0x63` to `0x65` |
| --- | --- | --- | --- | --- | --- |
| 29 | 6 (`HALFLING`) | 2 (`FEMALE`) | 5 (`TRUE NEUTRAL`) | 9, 0, 0 | 7, 0, 0 |
| 30 | 4 (`HALF-ELF`) | 2 (`FEMALE`) | 7 (`CHAOTIC GOOD`) | 12, 17, 16 | 8, 9, 7 |
| 31 | 1 (`HUMAN`) | 1 (`MALE`) | 4 (`NEUTRAL GOOD`) | 11, 10, 0 | 9, 3, 0 |
| 32 | 8 (`THRI-KREEN`) | 2 (`FEMALE`) | 5 (`TRUE NEUTRAL`) | 9, 3, 0 | 13, 14, 0 |
| 33 | 5 (`HALF-GIANT`) | 1 (`MALE`) | 7 (`CHAOTIC GOOD`) | 10, 0, 0 | 9, 0, 0 |
| 34 | 8 (`THRI-KREEN`) | 2 (`FEMALE`) | 5 (`TRUE NEUTRAL`) | 6, 12, 0 | 6, 6, 0 |
| 35 | 8 (`THRI-KREEN`) | 2 (`FEMALE`) | 5 (`TRUE NEUTRAL`) | 12, 6, 0 | 6, 6, 0 |
| 36 | 8 (`THRI-KREEN`) | 2 (`FEMALE`) | 5 (`TRUE NEUTRAL`) | 7, 12, 0 | 6, 6, 0 |
| 37 | 3 (`ELF`) | 2 (`FEMALE`) | 4 (`NEUTRAL GOOD`) | 2, 11, 0 | 6, 6, 0 |
| 38 | 6 (`HALFLING`) | 2 (`FEMALE`) | 7 (`CHAOTIC GOOD`) | 16, 17, 0 | 6, 6, 0 |
| 39 | 7 (`MUL`) | 1 (`MALE`) | 1 (`LAWFUL GOOD`) | 10, 0, 0 | 7, 0, 0 |
| 40 | 3 (`ELF`) | 2 (`FEMALE`) | 8 (`CHAOTIC NEUTRAL`) | 11, 12, 17 | 6, 6, 7 |
| 41 | 1 (`HUMAN`) | 1 (`MALE`) | 1 (`LAWFUL GOOD`) | 2, 0, 0 | 7, 0, 0 |
| 42 | 8 (`THRI-KREEN`) | 2 (`FEMALE`) | 5 (`TRUE NEUTRAL`) | 9, 7, 0 | 6, 6, 0 |
| 43 | 5 (`HALF-GIANT`) | 1 (`MALE`) | 7 (`CHAOTIC GOOD`) | 10, 0, 0 | 7, 0, 0 |
| 50 | 4 (`HALF-ELF`) | 2 (`FEMALE`) | 4 (`NEUTRAL GOOD`) | 11, 12, 17 | 15, 14, 15 |
| 51 | 8 (`THRI-KREEN`) | 2 (`FEMALE`) | 4 (`NEUTRAL GOOD`) | 15, 7, 0 | 12, 14, 0 |
| 52 | 7 (`MUL`) | 1 (`MALE`) | 5 (`TRUE NEUTRAL`) | 9, 7, 0 | 6, 7, 0 |
| 53 | 1 (`HUMAN`) | 1 (`MALE`) | 1 (`LAWFUL GOOD`) | 1, 0, 0 | 15, 0, 0 |

FND-PARTY-020 gives, for the four members of the supplied party matched to records 40 to 43: a
female elf, chaotic neutral, Preserver/Psionic/Thief; a male human, lawful good, Cleric; a female
thri-kreen, true neutral, Fighter/Druid; and a male half-giant, chaotic good, Gladiator. In every
record the class bytes of 0 come after the nonzero ones, and a level byte is 0 exactly where its
class byte is 0.

## Interpretation

The origin, gender and alignment the four captured screens show are the names these bytes select
through the tables FND-PARTY-055 reads, so FND-PARTY-055's offsets agree with the screens. The class
codes of those four members put Cleric at 2, Druid at 7, Fighter at 9, Gladiator at 10, Preserver
at 11, Psionicist at 12 and Thief at 17, in the order of the class name table but not at its
positions; codes 1, 3, 6, 15 and 16 also occur. That is a value pattern only: the mapping is in
the routine behind trampoline `571F:0089`, which was not read.

## Alternatives

- The bytes were read from a different record than the captured screens drew: FND-PARTY-020 matches
  the four members to records 40 to 43 by their names and scores.
- The levels at `0x63` to `0x65` are those the captured screens showed: FND-PARTY-020 says play
  before the captures changed the levels, so they cannot be compared.

## How to reproduce

The locations are bytes `0x57` to `0x65` of each record. From the commit that adds this finding,
read each `CHAR` resource of the installed `CHARSAVE.GFF` through the archive's directory
(FMT-GFF-002) and print its bytes at `0x57`, `0x58`, `0x59` and `0x60` to `0x65`.
