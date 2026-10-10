---
id: FND-PARTY-061
title: No byte or aligned word in the first 79 bytes of CHAR records 40 and 42 holds the label positions of their gender, origin, alignment or class counted from 0
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
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

The first 79 bytes of each record (`CHAR/40` at `0x1C..0x6B` and `CHAR/42` at `0x267..0x2B6`)
are its type-1 chunk at `0x00..0x3B` and the first 20 bytes of its type-3 chunk, the 10-byte
header at `0x3B..0x45` and data bytes `0x45..0x4F` (FND-PARTY-052). Leaving out bytes 0 and 1,
the six scores at `0x23..0x29` and the name at `0x2B..0x3B`, no offset below `0x4F` holds one of
those four pairs of values as a byte in both records, and no even offset below `0x4E` holds one
as a 16-bit little-endian word in both records.

## Interpretation

The two chunks' bytes before `0x4F` do not hold gender, origin, alignment or class as the
label's position in the executable's runs counted from 0. The search did not reach the type-3
chunk's data from `0x4F`, where FND-PARTY-056 finds the first three counted from 1 at `0x57`,
`0x58` and `0x59` and class codes at `0x60..0x63`.

## Alternatives

FND-PARTY-018 recorded the same comparison and called these 79 bytes the records' headers,
reading the sizes as FND-PARTY-004 does. FND-PARTY-052 shows the bytes are the first chunk and
the start of the second, and FND-PARTY-004 is superseded, so FND-PARTY-018's interpretation (no
header byte or word holds the four values) and its alternative (33-byte records after a header)
no longer describe the record. This finding keeps its comparison with the offsets placed in the
chunks.

## How to reproduce

From the commit that adds this finding, take bytes `0x00..0x4F` of `CHAR/40` and `CHAR/42`
(file `0x1C..0x6B` and `0x267..0x2B6` of the installed `CHARSAVE.GFF`). For every byte offset
below `0x4F` and every even offset below `0x4E`, skipping offsets 0, 1, `0x23..0x29` and
`0x2B..0x3B` (a word is skipped if either of its bytes is), compare the pair of values the two
records hold with each of the four pairs above. No pair matches.
