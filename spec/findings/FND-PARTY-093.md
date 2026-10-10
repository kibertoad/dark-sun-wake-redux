---
id: FND-PARTY-093
title: Every read through ES of the displacements 0x31 to 0x35 outside overlay 210's save setter goes through a pointer other than the details table, and no read without ES follows the details pointers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000955F8..0x000955FC
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/field_reads.py)
environment: null
---

## Observation

File offsets are those of the installed `DSUN.EXE`. FND-PARTY-100 lists 54 reads through ES of the
displacements `0x31` to `0x35` in the whole file, of which overlay 210 `+0698`, at file
`0x955F8`, is the only one in overlay code. Of the resident ones, the instructions before each
load ES and BX from these far pointers: `DS:9D9F` for the six dword reads and updates from
`37FC:020A` to `37FC:074C`; the routine's argument at `bp+6` for the four dword reads from
`37FC:09D7` to `37FC:0D21`, and for `39A9:0071` after a comparison of that record with the four
bytes `GFFI`; and `DS:3411` for the word comparisons in segments `4654`, `4734`, `47B9`, `47E5` and
`4A6E`. None reads a byte. The decodings listed as outside the inventory at those
displacements start inside the instructions above.

`field_reads.py <dsun> <inventory> --table 19c5 --table 1429 31 32 33 34 35`, without `--es`, lists
overlay 210 `+0698` and eight decodings outside the inventory, each starting one byte or more
inside an instruction of its routine (overlay 210 `+0699` inside `+0698`, and seven `add`, `or` and
`fsub` forms). The same search for `76` to `7A`, the saves' offsets in the FMT-PARTY-001 record,
lists three such decodings and nothing else.

## Interpretation

No instruction of a form these searches cover reads the five saving throw bytes of a details
record, or of the CHAR record they are loaded from, except overlay 210's own comparison while
setting them. A reader would have to form the address in a register first, read through a
pointer kept somewhere other than `DS:19C5` or `DS:1429` without an ES override, or read a copy.

## Alternatives

- A reader through a pointer held in a local or another global and accessed through ES: the
  search without `--table` covers it for ES and finds none outside the setter.
- A reader through a computed address, or one in a decoding the per-byte pass does not separate
  from its neighbours: not covered.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `field_reads.py <dsun>
../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv --es 31 32 33 34 35`, reading the six instructions
printed before each hit, `field_reads.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv
--table 19c5 --table 1429 31 32 33 34 35` and the same for `76 77 78 79 7a`.
