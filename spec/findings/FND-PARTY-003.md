---
id: FND-PARTY-003
title: Bytes 0x23 to 0x28 of every CHAR record hold six values from 12 to 24
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x3F..0x45
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

In each of the 19 `CHAR` resources of the installed `CHARSAVE.GFF`, the six bytes at offsets
`0x23..0x29` of the resource hold values from 12 to 24, the lowest being 12 and the highest 24.
Records 33 and 43, and 41 and 53, carry the same name (FND-PARTY-001) and the same six values.
Records 40 and 50 share a name and differ only in the first of the six (18 and 19). Records 34 and
35, and 42 and 51, hold the same six values under names that differ. For `CHAR/40`, which starts
at file offset `0x1C`, the six bytes are at `0x3F..0x45`.

The four View Character captures show, next to the labels STR, DEX, CON, INT, WIS and CHR in that
order, the same six values as records 40, 41, 42 and 33 hold at `0x23..0x29`, in the same order
(FND-PARTY-020).

## Interpretation

`0x23..0x29` holds the six ability scores as unsigned bytes: Strength, Dexterity, Constitution,
Intelligence, Wisdom and Charisma, in the order the View Character screen lists them and the
manual describes them (SRC-MANUAL-1994, page 16).

## Alternatives

The captures were taken after some play, so a score the game changed during play would differ
from the file; none did. No code that reads these bytes has been located. Whether the bytes are
the scores after origin modifiers or before has not been checked.

## How to reproduce

List the `CHAR` resources of the installed `CHARSAVE.GFF` through its directory and read bytes
`0x23..0x29` of each; compare them with the values the four captures of FND-PARTY-020 show.
