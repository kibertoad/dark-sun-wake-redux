---
id: FND-CONFIG-113
title: A resident event-five value-64 branch forwards the event record to mode dispatch
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:1261
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:05CF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0699
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; MZ relocation and segment-table mapping
environment: null
---

## Observation

Resident entry 28C9:1261 begins at file offset `0x0001F0F1`
and returns at `0x0001F3A7`. It requires its first word argument
at BP+06 to equal five. A five-entry discriminator table compares
its third word argument at BP+0A against 8, 32, 64, 128 and 256.
The value-64 target is `0x0001F19A`.

The value-64 branch exits when DS:0DA4 is nonzero and the far
pointer at `42A1:0004` is zero. Otherwise it requires DS:0DAB
nonzero. Values two and three enter a separate helper-heavy path;
other nonzero values jump to `0x0001F31C`.

For the bounded DS:0DAB-equals-one path, that block continues only
when word `3C10:0019` is zero or signed DS:426D is below four.
It checks DS:0DAB still equal to one, sets byte DS:0DA4 to one,
and calls resident helper 1000:0699 with source SS:BP+06 and
count 24. It then calls local 28C9:05CF at
`0x0001F34C`, and cleans up 24 argument bytes on return.
No return separates this call from the entry.

Helper 1000:0699, at file offset `0x00005899`, reserves the
requested byte count beneath its far-return address and copies from
the supplied source pointer to that stack argument area. It copies
whole words followed by the remaining byte, then returns with the
copied area retained. This call therefore forwards the twelve input
words starting at BP+06 in order, including the words at BP+12
and BP+14 used by 28C9:05CF (FND-CONFIG-112).

A search of the complete declared resident segment range
`0x0001DE90..0x000217F0` yields one 16-bit relative near-call
candidate to `0x0001E45F`: the verified call above. The exact
MZ relocated offset/segment inventory finds no pointer to raw
18C9:05CF, and declared FBOV fixups find none to descriptor 22,
offset 05CF. These negatives are limited to those encodings;
local near calls use neither relocated far-pointer representation.

## Interpretation

One explicit event-five/value-64 route preserves the incoming record
through resident mode dispatch. When the later stored mode is five,
its two words at event-record offsets 12 and 14 reach overlay 208
entry 006B (FND-CONFIG-112, FND-CONFIG-110). The handler does
not construct those input words on this bounded state-one path.
The early event and state checks remain necessary in addition to
mode five and the later selector gates.

## Alternatives

FND-CONFIG-114 records bounded incoming-reference inventories
without identifying registration or a producer. FND-CONFIG-115 reads
the value-64 state-two/three block and its later state-one equality.
Registration, event producers, meanings of the two forwarded words,
allowed state values and helper state changes remain unread
(Q-CONFIG-008). A physical action and visible feedback are not
established. Other pointer forms, call encodings and computed routes
remain possible despite the qualified inventory negatives.

## How to reproduce

Read the entry gate and five-value dispatch at
`0x0001F0F1..0x0001F122`. Decode exactly five discriminator
words and five target words at `0x0001F3A8..0x0001F3BC`,
using resident file base `0x0001DE90`; select value 64. Read
`0x0001F19A..0x0001F1CD` and the direct state-one block
`0x0001F31C..0x0001F34F`, checking the cleanup at
`0x0001F38C..0x0001F391` and return at
`0x0001F3A4..0x0001F3A8`. Inspect the argument-copy helper
only through its return, `0x00005899..0x000058BA`.
Compare the preserved word slots with FND-CONFIG-112. Search
relative-call candidates across the complete segment-table range,
not merely the target routine's prefix or ending offset. Check
relocated far-pointer and FBOV descriptor representations separately.
