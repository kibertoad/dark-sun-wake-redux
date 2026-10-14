---
id: FND-PARTY-196
title: In the 151 records of chunk types 2 and 4 in the shipped CHAR resources, the word at 0x04 is 9999 exactly in the records no type-4 chunk follows, the word at 0x08 is 9999 in all but three, and the byte at 0x15 is always 0
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1C..0x44F
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x517..0xCA4
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0xD6E..0xE83
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0xEB7..0xFED
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1052..0x1125
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1128..0x11FB
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1395..0x1468
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1496..0x1527
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x16AB..0x17E1
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1964..0x1A9A
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1B48..0x1EF1
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x21B3..0x28C3
tool: Python 3.14.7, tools/research/exec-census/char_item_records.py
environment: null
---

## Observation

`char_item_records.py` walks the chunks of every `CHAR` resource of the installed `CHARSAVE.GFF`
as FND-PARTY-052 does and takes the 23 data bytes of each chunk of type 2 or 4. There are 151 such
records in 18 resources: 42 of type 2, whose field word is 16 (14 records), 17 (18) or 4 (10),
and 109 of type 4, all with field word 2.

**Chain ends.** Each record whose chunk is the `target_chunk` of a type-4 chunk (109 records)
holds a word at `0x04` other than 9999, and each record that no type-4 chunk names as its target
(42 records) holds 9999 there. The records of type 2 are 10 of the 42 and 32 of the 109.

**Fields.** Reading `0x00` to `0x11` as signed words and `0x12` to `0x16` as bytes:

| Offset | Distinct values | Range | Most common |
| --- | --- | --- | --- |
| `0x00` word | 86 | -31990 to -603 | -900 (12), -633 (8), -842 (5), -831 (4) |
| `0x02` word | 52 | 0 to 332 | 0 (87), 153, 37, 71 (3 each) |
| `0x04` word | 92 | 0 to 9999 | 9999 (42), 11 (4), 4 (4), 14 (3) |
| `0x06` word | 51 | -30536 to 30000 | 30000 (13), 9999 (12), 1 (9), 20 (8) |
| `0x08` word | 4 | 484 to 9999 | 9999 (148), 518, 508, 484 (1 each) |
| `0x0A` word | 42 | 2 to 97 | 9 (27), 97 (12), 6 (8), 77 (8) |
| `0x0C` word | 19 | 0 to 2910 | 0 (124), 2028 (5), 2159 (2), 2314 (2) |
| `0x0E` word | 12 | 0 to 70 | 0 (123), 1 (17), 12 (2), 65 (1) |
| `0x10` word | 39 | 0 to 327 | 0 (105), 36, 326, 327 (3 each) |
| `0x12` byte | 3 | 4 to 6 | 5 (73), 6 (43), 4 (35) |
| `0x13` byte | 33 | 0 to 255 | 3 (16), 9 (12), 10 (12), 2 (11) |
| `0x14` byte | 50 | 3 to 247 | 8 (12), 69 (12), 244 (10), 26 (9) |
| `0x15` byte | 1 | 0 | 0 (151) |
| `0x16` byte | 5 | 0 to 4 | 0 (91), 1 (30), 3 (16), 2 (10) |

The three records whose word at `0x08` is not 9999 are type-4 chunks of resources 30 and 33 with
object numbers 2212, 2211 and 2212 (words -2212, -2211, -2212 at `0x00`); each holds 19 at `0x0A`
and 102 at `0x14`. The negated words at `0x00` name 86 different objects, from 603 to 31990, and
`OBJEX.GFF` has an `RDFF` resource for each.

## Interpretation

The game stores 9999 in the link word at `0x04` of the last record of each chain, and a record
number in the others, so 9999 is the end marker of a chain that type-4 chunks link through
`0x04` (FND-PARTY-051). The kind-1 load of overlay 187 `+04F8`, which sets the words at `0x04`
and `0x08` of a new record to 9999 (FND-PARTY-051), fits a second link or reference at `0x08`
that is empty in most records. The other fields vary from item to item and are not named by
this reading.

## Alternatives

- The record numbers in the words at `0x04` are those of the game that saved the characters; the
  load replaces the links with its own (FND-PARTY-051), so their values say nothing beyond
  "not 9999".
- That `0x08` is a link is a guess from its 9999 and the three records' shared values; no code
  that reads it is cited here.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, run
`tools/research/exec-census/char_item_records.py "<install>"` and read its summary lines.
