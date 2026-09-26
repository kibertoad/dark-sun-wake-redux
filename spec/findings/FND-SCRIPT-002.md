---
id: FND-SCRIPT-002
title: GPLDATA.GFF#GPLI/1 is 1,316 records of three words that pair entry numbers with script offsets
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x1F8F7F..0x1FAE57
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

`GPLDATA.GFF#GPLI/1`, the only `GPLI` resource, is 7,896 bytes at `0x1F8F7F`, found through the
`GFFI` table of the installed file (FMT-GFF-001). Read as 1,316 records of three `UINT16LE` words
`a`, `b` and `c` with no header:

- the `a` words are the numbers 0 to 1,315, each once;
- record 0 is `(0, 0, 0)`, and it is the only record whose `c` is not the number of a `GPL `
  resource of the same file (FND-SCRIPT-001);
- the `c` words take 331 distinct values, which include all 330 `GPL ` numbers;
- in every other record, `b` is less than the size of the `GPL ` resource `c` names;
- with the records sorted by `a`, the `c` words never decrease.

The first six records in file order are `(0, 0, 0)`, `(1, 1, 1)`, `(3, 712, 1)`, `(10, 2911, 1)`,
`(23, 1, 4)` and `(41, 767, 5)`.

## Interpretation

Each record names a place in a script, offset `b` in `GPL ` resource `c`, and gives it the
number `a`. Numbered in order of script, the places form one list of 1,316 entry points, with
entry 0 standing for none. Overlay 187 uses the records to convert between the two forms
(FND-SCRIPT-017).

## Alternatives

An earlier reading split the resource into 329 records of four 6-byte lanes, found the value 135
twice at unrelated positions, and found that the third word of each lane is almost always a `GPL `
number. Those are the same bytes read with a 24-byte stride; the 6-byte records above account for
every word, and SRC-OPENDS-5C6CBD7 reads the resource the same way. Whether every `b` falls on an
instruction boundary was not checked.

## How to reproduce

Find `GPLI/1` through the `GFFI` table of `GPLDATA.GFF`, read it as three-word records, and
compare the `c` words with the `GPL ` resource numbers and the `b` words with their sizes.
