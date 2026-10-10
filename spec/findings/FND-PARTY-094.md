---
id: FND-PARTY-094
title: 353 RDFF resources of OBJEX.GFF start with the chunk headers of a CHAR record, and two of them with the combatant kind 7 hold 107,000 and 33,000 where a details record holds kill_experience
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x00528D5A..0x00528DE1
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x00529C24..0x00529CAB
tool: Python 3.14.7 (tools/research/exec-census/details_chunks.py, gff_tag_numbers.py)
environment: null
---

## Observation

`gff_tag_numbers.py <install> CHAR` finds `CHAR` resources only in `CHARSAVE.GFF`, 19 of them.
`details_chunks.py` finds the 10-byte header FMT-PARTY-006 gives a CHAR record's details chunk
(`chunk_type` 3, `kind` 3, `unk_03` 0, `field` 15, `len_data` 66) 22 times in `CHARSAVE.GFF` and
375 times in `OBJEX.GFF`: 353 inside `RDFF` resources, all at offset `0x3B`, and 22 outside the
resources the directory lists. In each of the eight `RDFF` resources whose dword at `0x04` of the
66 bytes after that header is 32,768 or more, the resource starts with the header of a type-1
chunk as a CHAR record does (`01 00 02 00`, a word, `00 00 31 00`). Reading each as FMT-PARTY-001
reads a CHAR record, the byte at `0x1F` (the combatant record's byte at `+0x15`) and the dword at
`0x49` are:

| `RDFF` | Byte at `0x1F` | Dword at `0x49` | Word at `0x0A` |
| --- | --- | --- | --- |
| 32000 | 0 | 110,000 | 64 |
| 32001 | 0 | 60,000 | 72 |
| 32002 | 0 | 125,000 | 165 |
| 32003 | 0 | 55,000 | 59 |
| 415 | 5 | 34,000 | 170 |
| 430 | 7 | 107,000 | 330 |
| 541 | 7 | 33,000 | 175 |
| 561 | 1 | 34,000 | 170 |

The other 367 dwords, inside `RDFF` resources and outside them, are 0 to 30,000.

## Interpretation

If an `RDFF` resource loads into the combatant and details records as a CHAR record does, its
byte at `0x1F` is the combatant kind and its dword at `0x49` is `kill_experience`. Then a kill of
`RDFF` 430, of kind 7, gives each party member 107,000 divided by the filled party slots: 26,750
with four, but 35,666 with three, 53,500 with two and 107,000 with one, each of which reaches
`+1604` as a negative 16-bit share (RULE-PARTY-015, FND-PARTY-086) and lowers the experience. A
kill of `RDFF` 541 does so with one filled slot. The kind-0 records 32000 to 32003 give nothing,
as kind 0 is outside the mask `0xF80`.

## Alternatives

- `RDFF` resources load some other way: FMT-ACTOR-002 claims nothing of their layout and the
  routine that receives the request was not read (Q-ACTOR-003), so the reading rests on the
  matching headers, which are circumstantial.
- A loaded combatant's kind byte changed before the kill, by a script or the fight: not read.
- The 22 headers outside the listed resources lie in data the directory reader here does not
  attribute; their dwords are below 32,768 or were not attributed to a resource.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, run in
`tools/research/exec-census/`: `gff_tag_numbers.py <install> CHAR`, `details_chunks.py
<install>/OBJEX.GFF 32768`, `details_chunks.py <install>/OBJEX.GFF` and `details_chunks.py
<install>/CHARSAVE.GFF`. Read the bytes at `0x1F`, the word at `0x0A` and the dword at `0x49` of
each `RDFF` resource listed.
