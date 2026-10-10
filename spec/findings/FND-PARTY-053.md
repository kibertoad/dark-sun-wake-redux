---
id: FND-PARTY-053
title: In the 19 stored CHAR records, the word the type-3 chunk puts at the details record's max_hit_points holds 21 to 165, equal to the hit points at 0x0A in 16 records and above them in three
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x69..0x71
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x17E..0x186
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x2B4..0x2BC
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x387..0x38F
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x564..0x56C
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x760..0x768
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x854..0x85C
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0xB16..0xB1E
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0xDBB..0xDC3
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0xF04..0xF0C
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x109F..0x10A7
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1175..0x117D
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x13E2..0x13EA
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x14E3..0x14EB
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x16F8..0x1700
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x19B1..0x19B9
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1B95..0x1B9D
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x2200..0x2208
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x2504..0x250C
tool: a Python 3.14.7 reading of CHARSAVE.GFF through its directory (FMT-GFF-001, FMT-GFF-002)
environment: null
---

## Observation

In each of the 19 `CHAR` resources of the installed `CHARSAVE.GFF`, the type-3 chunk's data
starts at `0x45` (FND-PARTY-052). Its bytes `0x08` and `0x0E`, at the offsets of
FMT-COMBAT-002's `max_hit_points` and `unk_0E`, are the record's words at `0x4D` and `0x53`:

| Resource | Word at `0x0A` | Word at `0x4D` | Word at `0x53` |
| --- | --- | --- | --- |
| 29 | 21 | 21 | 32,795 |
| 30 | 81 | 81 | 32,770 |
| 31 | 76 | 76 | 65,515 |
| 32 | 111 | 111 | 32,769 |
| 33 | 152 | 152 | 65,522 |
| 34 | 54 | 54 | 32,794 |
| 35 | 54 | 54 | 32,793 |
| 36 | 54 | 54 | 32,792 |
| 37 | 48 | 48 | 32,791 |
| 38 | 78 | 78 | 32,790 |
| 39 | 112 | 112 | 32,789 |
| 40 | 48 | 48 | 32,793 |
| 41 | 64 | 64 | 32,792 |
| 42 | 72 | 72 | 32,791 |
| 43 | 165 | 165 | 32,790 |
| 50 | 34 | 57 | 58,497 |
| 51 | 98 | 101 | 58,500 |
| 52 | 61 | 61 | 32,789 |
| 53 | 88 | 113 | 58,498 |

The word at `0x53` equals the type-1 chunk's word at `0x10` (FND-PARTY-050) in 14 records and
differs in 31, 33, 50, 51 and 53, where the word at `0x10` is 33,534, 33,536, 33,533, 33,535 and
33,536.

## Interpretation

For a party slot the load copies the type-3 chunk unchanged into the slot's combatant details
record (FND-PARTY-051), so the word at `0x4D` is the character's greatest hit points, which the
status panel shows second (FND-COMBAT-022). The word at `0x0A` that lands on the combatant
record's `hit_points` is therefore the current hit points: it never exceeds the word at `0x4D`
and is below it in records 50, 51 and 53.

## Alternatives

- The word at `0x0A` is the greatest hit points rather than the current (FND-PARTY-050): ruled
  against by this finding, which places the greatest hit points at `0x4D`.
- The word at `0x4D` is changed before it is shown: for a slot above 4 the load scales it by the
  difficulty (FND-PARTY-051), but not for the party slots 0 to 3; later writes were not searched
  for.

## How to reproduce

The locations are bytes `0x4D` to `0x55` of each record. From the commit that adds this finding,
read each `CHAR` resource of the installed `CHARSAVE.GFF` through the archive's directory
(FMT-GFF-002) and print its signed words at `0x0A` and `0x4D` and its unsigned words at `0x53`
and `0x10`.
