---
id: FND-PARTY-181
title: In CHARSAVE.GFF every 23-byte record of a type-2 or type-4 chunk has a negative word 0, whose negation is always the number of an RDFF resource of OBJEX.GFF
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x00..0x2DD7
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x00..0x680304
tool: Python 3.14.7 reading the archives' directories (FMT-GFF-002)
environment: null
---

## Observation

Every `CHAR` resource of the installed `CHARSAVE.GFF` was walked as FMT-PARTY-006 describes, and
for each chunk whose `chunk_type` is 2 or 4 the signed 16-bit word at offset 0 of its data
(offset `0x0A` of the chunk) was taken. `OBJEX.GFF`'s directory lists the tags `GFFI`, `CBMP`,
`RDFF`, `SCMD`, `BMP `, `FNFO`, `ALL ` and `OJFF`, with 1,643 `RDFF` resources.

There are 151 such records, with 86 distinct values of the word. All 151 are negative. For each,
the negated value is the number of an `RDFF` resource of `OBJEX.GFF`; none of the words is itself
an `RDFF` number. The smallest 40 values are -31990, -31981, -31980, -31972, -31961, -31951,
-31941, -31940, -31901, -31411, -31407, -31333, -31323, -31314, -31211, -31209, -31208,
-31207, -31200, -31033, -31031, -31022, -31003, -30042, -30037, -30034, -30033, -30027,
-30021, -30016, -30014, -30011, -30010, -30004, -30003, -30002, -30001, -2212, -2211 and -1610.

## Interpretation

The word at offset 0 of a shipped 23-byte record holds an `OBJEX.GFF` resource number, negated.
This is a value pattern; FND-PARTY-182 gives the code that uses the word that way.

## Alternatives

A pattern this complete is unlikely by chance, but other numbering that happens to coincide with
`RDFF` numbers is not ruled out by the data alone.

## How to reproduce

From the commit that adds this finding, read each `CHAR` resource of the installed
`CHARSAVE.GFF` through the archive's directory and walk its chunks: from offset 0, read byte 0
and the word at `0x08`, step on by 10 plus that word until byte 0 is 0xFF, and for byte 0 equal
to 2 or 4 read the signed word at `0x0A`. Collect the `RDFF` resource numbers from `OBJEX.GFF`'s
directory, following its `GFFI` index for an indexed tag, and test each negated word against them.
