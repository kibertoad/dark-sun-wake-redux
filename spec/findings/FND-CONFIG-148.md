---
id: FND-CONFIG-148
title: A validated overlay-range query finds selector-table reads but no verified literal producer
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1AA0:0566
tool: Python 3.14.7 and Capstone 5.0.7 bounded operand-candidate search and entry-based instruction checks
environment: null
---

## Observation

This replaces FND-CONFIG-146's query-method description. Its
scratch range selector also treated resident descriptors with flag
bit zero set as overlays. The validated repeat selects overlay
flag bit one, checks resident headers and payload/fixup bounds,
and obtains exactly 49 overlays and 8262 fixups. It confirms
the same two reads and three rejected decodes described below.

FND-CONFIG-145 reads a selector word using DS:5ECB and
a starting-position word using DS:5EEB. A literal query searched
both little-endian words in the resident MZ load image at
`0x00005200..0x00057570` and all 49 declared FBOV
code-and-data ranges. For each hit it decoded candidate starts
one through five bytes earlier and selected instructions whose
printed operand named the literal value. These are candidates,
not verified uses until checked from a containing entry.

The verified uses are reads at `0x000101A8`, selector word
DS:5ECB indexed by BX, and `0x0001018F`, starting-position
word DS:5EEB indexed by BX. Both are in FND-CONFIG-145's
complete entry 1AA0:0566. The query also produced three
locally decodable mov candidates that fail the boundary check:

| Rejected candidate | Actual containing instruction | Entry used for the check |
|---|---|---|
| 0x000101A9, immediate 5ECB | Word read beginning at 0x000101A8 | 0x00010166 |
| 0x00025D46, apparent immediate store of 5EEB | Addition beginning at 0x00025D45, followed by jump at 0x00025D48 | 0x00025CE4 |
| 0x000328AA, apparent word read of DS:5EEB | Push beginning at 0x000328A7, followed by jump at 0x000328AB | 0x000327BF |

The apparent store joins bytes inside one real instruction with
bytes from its following jump. It is not a table-address producer.
No selected producer was verified. Other raw matches without
these selected decodes were not classified as behavior evidence.
This query does not cover computed bases, other field offsets,
indirect or block writes, aliases or every instruction encoding.

## Interpretation

The two named reads and the initial image words in
FND-CONFIG-145 do not establish a runtime producer or an
immutable selector table. The rejected apparent address store
cannot supply that missing evidence. This bounded query adds
no new setup-time range invariant or termination claim.

## Alternatives

FND-CONFIG-150 subsequently identifies a producer using separate
per-type addresses, one of the forms this literal query did not cover.
Its concrete stores replace the unchanged-table reading on that path.

Q-CONFIG-008 retains indirect and block producers, parent-base
arithmetic, other field-offset encodings and later state timing.
An unchanged initial table and runtime population by unread
writers remain competing readings. A new producer reading
must reach one of these uncovered forms; do not repeat this
literal query without new coverage, a new tool or a new reading.

## How to reproduce

Bound the resident and overlay ranges with FMT-EXE-001
through FMT-EXE-005. Select descriptors whose flags contain
bit one, verify header trap and trampoline bounds, even fixup
size, bounded code-and-fixup payload and each fixup operand.
Require 49 overlay ranges and 8262 fixups for this build.
Search little-endian 5ECB and 5EEB,
locally decode starts one through five preceding bytes, and keep
matching operands as candidates. Confirm the two reads from
1AA0:0566 and reject overlaps by decoding the named entries
from their inventory starts, not from the candidate bytes.
Keep unclassified raw hits and absent selected producers
separate from an exhaustive negative claim. Compare
FND-CONFIG-145 and FND-CONFIG-142's coverage limits.
