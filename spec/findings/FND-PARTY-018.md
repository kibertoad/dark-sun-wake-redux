---
id: FND-PARTY-018
title: No CHAR header byte or word of records 40 and 42 holds the label positions of their gender, origin, alignment or class
status: superseded
builds: [BLD-GOG-EN-1.1]
superseded_by: [FND-PARTY-061]
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1C..0x6B
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x267..0x2B6
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

Records 40 and 42 of the installed `CHARSAVE.GFF` are the first and third members of the
owner's View Character captures (FND-PARTY-020), which show the first as female, elf, chaotic
neutral and preserver among other classes, and the third as female, thri-kreen, true neutral and
fighter among other classes. The positions of those labels in the runs of FND-PARTY-016 and
FND-PARTY-017, counted from 0, are: female 1 and 1; elf 2 and thri-kreen 7; chaotic neutral 7 and
true neutral 4; preserver 4 and fighter 2.

In the 79-byte headers of the two records (FND-PARTY-004; `CHAR/40` at `0x1C..0x6B` and
`CHAR/42` at `0x267..0x2B6`), leaving out bytes 0 and 1, the six scores at `0x23..0x29` and the
name at `0x2B..0x3B`, no offset holds one of those four pairs of values as a byte in both
records, and no even offset holds one as a 16-bit little-endian word in both records.

## Interpretation

The records do not keep gender, origin, alignment or class in a header byte or aligned word as
the label's position in the executable's runs.

## Alternatives

Any of the four may still be in the header in another form: counted from 1, packed into bits,
in an odd-aligned word, or as a code in an order other than the labels'. They may also be in the
33-byte records after the header, or not stored in `CHAR` at all.

## How to reproduce

Take the headers of `CHAR/40` and `CHAR/42`, and for every remaining byte offset and every
remaining even offset, compare the pair of values the two records hold with each of the four
pairs above.
