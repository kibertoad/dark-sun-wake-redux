---
id: FND-UI-042
title: The literal window-picture displacement census yields only interior-decode candidates
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:386D..1000:38B0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3986..1000:39C0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0D66..4842:0D7E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:02B3..3A8E:0370
tool: tools/research/exec-census/immediate_search.py and resident_listing.py; tools/evidence/report.mjs table; Python 3.12.10, Capstone wheel 5.0.9 (module version 5.0.7), xxhash 4.0.1
environment: null
---

## Observation

FMT-UI-001 records the shipped WIND width, height and picture
reference at offsets BE, C0 and C2 hexadecimal. Q-UI-001 asks,
among other fields, whether and how that picture reference is drawn.
FND-UI-008's earlier generic-window reading concerns offset 3A;
it is not a census of literal C2 memory operands.

The hash-checked immediate_search.py query for BE, C0 and C2
decodes at every byte of the resident MZ load image and every
flags-three overlay code span. Its 285 matching raw decode sites
include immediates and displacements, false starts and prefixless
interior starts. Only three printed instructions have an explicit
memory displacement C2. None is an instruction start when checked
against its declared entry path:

| Raw candidate, normalized address | Entry-based classification |
|---|---|
| 1000:3883 | Inside the absolute-address operand of the word load beginning at 3880; the next instruction begins at 3884 |
| 1000:399C | Inside the corresponding absolute-address operand of the word load beginning at 3999; the next instruction begins at 399D |
| 4842:0D77 | Inside the near-jump displacement beginning at 0D75; its apparent operand also consumes bytes of the following driver thunk at 0D78 |

The first two declared functions begin at 1000:386D and 3986.
The last candidate is printed as paragraph address 4919:0007;
descriptor 75 maps it to 4842:0D77. Its segment-table range is
0002..1906 with flags one. It is not excluded merely for lying
in an unowned inventory gap: decoding the six-byte thunk from
4842:0D72 establishes its actual instruction boundaries. The
adjacent thunk starts at 0D78. FND-EXE-585 and FND-EXE-590
describe this module's driver-call mechanism; no driver or game
code was executed for this check.

Positive controls are the four reached word accesses in the
registration entry 3A8E:02B3:

- 02E9 adds width at supplied window +BE to a coordinate.
- 02F8 adds height at supplied window +C0 to a coordinate.
- 0352 reads that width word.
- 035C reads that height word.

The census finds each correct prefixed instruction start, along
with a prefixless raw duplicate one byte later. The entry-based
decode qualifies the prefixed starts. FND-CONFIG-030 independently
records this registration path's position/dimension checks. Thus
the neighboring-field controls are real window accesses, while
the three apparent picture-field hits are interior decodes.

## Interpretation

This closes the literal positive-C2 memory-displacement candidate
classification for the stated decoder and source spans: it supplies
no qualified direct memory instruction reading or writing window
+C2. It does not prove that the picture word is unused. A pointer
can be rebased before a later small-displacement access; a loop,
bulk copy, table or callback can propagate the field without a
literal C2 memory operand. Unrecognized instruction forms and
runtime-loaded code are outside this decoder-defined census.

Q-UI-001 retains those computed/copied paths and actual background
production, as well as its other fields and masks. Source picture
numbers and matching BMP dimensions remain source observations,
not evidence of a native draw operation.

## Alternatives

- An unused source field is consistent with this negative literal
  census, but the census does not establish it.
- A copied, rebased or computed consumer is also consistent with it;
  a concrete producer-to-consumer trace is still needed.
- Treating all-byte matches as three real picture-field instructions
  conflicts with the entry-based boundaries above.
- Treating the neighboring width/height controls as picture evidence
  assigns a different field to otherwise valid instructions.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and locked evidence Python.
Run immediate_search.py with arguments be c0 c2 and retain the
result under GAME_DIR/analysis/ui-window-image-20261018/.
Filter explicit memory operands ending in 0xc2, including absolute
and based forms, rather than every printed immediate 0xc2.

Read resident_listing.py from 1000:386D to 38B0, from
1000:3986 to 39C0 and from 4842:0D66 to 0D7E; qualify
the three raw candidates against those starts. For the address
normalization, query the 229 eight-byte FMT-EXE-002 records at
file 4B080 with the same source hash and select the span containing
load-image-relative 39197. Descriptor 75 gives segment 3842 before
the documented 1000 load relocation and candidate offset 0D77.
Read 3A8E:02B3..0370 for the four dimension controls.

Retained reports are window-header-candidates.txt,
picture-candidate-contexts.txt, driver-thunk-context.txt and dimension-positive-controls.txt;
the segment query and result are also in that local directory.
Keep the literal census's limits when using its negative result.
