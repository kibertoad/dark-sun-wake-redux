---
id: FND-PARTY-050
title: In the 19 stored CHAR records, the words and bytes the load copies to the combatant record's known fields hold hit points from 21 to 165, identifiers from 32,769 to 33,536, image offsets from 0 to 13 and marks of 0 or 1
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x26..0x3F
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x13B..0x154
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x271..0x28A
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x344..0x35D
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x521..0x53A
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x71D..0x736
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x811..0x82A
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0xAD3..0xAEC
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0xD78..0xD91
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0xEC1..0xEDA
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x105C..0x1075
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1132..0x114B
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x139F..0x13B8
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x14A0..0x14B9
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x16B5..0x16CE
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x196E..0x1987
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1B52..0x1B6B
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x21BD..0x21D6
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x24C1..0x24DA
tool: a Python 3.14.7 reading of CHARSAVE.GFF through its directory (FMT-GFF-001, FMT-GFF-002)
environment: null
---

## Observation

FND-PARTY-049 shows a `CHAR` load copying a record's bytes `0x0A` to `0x3A` to bytes `0x00` to
`0x30` of the slot's FMT-COMBAT-001 record. These are the `CHAR` offsets that land on that
record's named fields, and what the 19 `CHAR` resources of the installed `CHARSAVE.GFF` (29 to 43
and 50 to 53) hold there:

| `CHAR` offset | Lands on | Values in the 19 records |
| --- | --- | --- |
| `0x0A`, `INT16LE` | `hit_points` (`0x00`) | 21 to 165: 29 21, 30 81, 31 76, 32 111, 33 152, 34 54, 35 54, 36 54, 37 48, 38 78, 39 112, 40 48, 41 64, 42 72, 43 165, 50 34, 51 98, 52 61, 53 88 |
| `0x0E`, `UINT16LE` | `details_index` (`0x04`) | 0 to 3 |
| `0x10`, `UINT16LE` | `character_id` (`0x06`) | 32,769 to 33,536 (`0x8001` to `0x8300`), none equal to its resource number; 35 and 40 both hold 32,793 |
| `0x1A`, `UINT16LE` | the word at `0x10` that 300 is added to (FND-PARTY-048) | 0 to 13 (FND-PARTY-049) |
| `0x1E`, `UINT8` | `combat_mark` (`0x14`) | 0 or 1 |
| `0x22`, `UINT8` | the byte at `0x18` holding `computer_control` and `control_locked` | 0 in every record |
| `0x23` to `0x28` | bytes `0x19` to `0x1E` of `unk_19` | the six ability scores (FMT-PARTY-001) |
| `0x2B`, `char[16]` | `name` (`0x21`) | the name (FMT-PARTY-001) |

After the copy, overlay 187 `+045E` stores 9,999 to the combatant record's words at `0x04`,
`0x08`, `0x0A` and `0x0C` and 1 to its `combat_mark` when that is 0 (FND-PARTY-049), so the
`CHAR` words at `0x0E`, `0x12`, `0x14` and `0x16` and a `combat_mark` of 0 do not survive that
load.

## Interpretation

The `CHAR` word at `0x0A` is the character's current hit points: the load puts it where the
combat status panel reads them (FND-COMBAT-018, FND-COMBAT-022). The word at `0x10` is what the
combatant record keeps as `character_id`, and its values show it is not the resource number of
the `CHAR` record; what it identifies is open. The six scores and the name sit where
FND-COMBAT-022 reads the combatant's scores and name, which agrees with FMT-PARTY-001's offsets.

## Alternatives

- The word at `0x0A` is maximum rather than current hit points: FMT-COMBAT-001's `hit_points`
  is the first number of the panel's line (FND-COMBAT-022), and this finding does not read the
  second.

## How to reproduce

The locations are bytes `0x0A` to `0x22` of each record. From the commit that adds this finding, read each `CHAR` resource of the installed
`CHARSAVE.GFF` through the archive's directory (FMT-GFF-002) and print, per resource, the signed
word at `0x0A`, the words at `0x0E`, `0x10` and `0x1A`, and the bytes at `0x1E` and `0x22`.
