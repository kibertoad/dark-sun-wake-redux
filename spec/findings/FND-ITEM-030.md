---
id: FND-ITEM-030
title: Six explicit effect-block dereferences read caller bytes rather than writing the adjacent hit flags
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065A51..0x00065A65
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066078..0x0006609F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066106..0x0006613B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006616D..0x00066179
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000661AB..0x000661B9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000661FF..0x00066209
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066A84..0x00066A8E
tool: tools/research/exec-census/operand_range.py and overlay_listing.py at 74ce65f6; locked evidence Python and Capstone
environment: null
---

## Observation

Q-ITEM-013 needs actual write destinations rather than treating every
pointer argument as an output buffer. FND-ITEM-017 records the caller's
explicit far pointer SS:BP-20 and its separate near flags offset BP-7.
Here BP refers to the hit caller's frame; offsets are hexadecimal.

The effect switch's inventory row at file 65653 contains six explicit
far loads of its argument at child BP+0A. The existing bounded operand
reader locates them at overlay 179 +1771, +1DA6, +1E42, +1ECB, +1F1F
and +27A4. Searching displacements 0A through 0D inclusive finds those
same six BP-relative uses, not separate pointer-word copies in that
declared-range decoding. This is a candidate locator, not a proof that
arbitrary aliases or all native instructions have been found.

The bounded instruction readings classify these uses:

| Pointer load | Immediate block use |
|---|---|
| +1771 | Tests masks 04 and 20 of byte +8 |
| +1DA6 | Reads byte at the index formed by shifting the local counter right three |
| +1E42 | Reads byte at the same counter-derived index |
| +1ECB | Reads byte +8 and tests mask 80 |
| +1F1F | Tests mask 08 of byte +3 |
| +27A4 | Tests mask 01 of byte +7 |

All six are reads, not stores through that pointer. The two indexed loops
initialize their local counter to zero and compare it unsigned with 113
before admitting an iteration. For a stable counter between that check
and its right shift, their byte index is zero through fourteen. This is
consistent with the effect-bit block in FND-MAGIC-021, but does not prove
the child's whole memory contract or absence of writes by its callees.

For the caller's preserved far pointer and nonwrapping frame, the maximum
read span is fifteen bytes starting at caller BP-20, ending before BP-11.
The separately reloaded flags byte at caller BP-7 lies outside that span.
No equality of DS and SS is needed to interpret these explicit far reads;
it is needed separately for the near flags store described by FND-ITEM-017.

## Interpretation

These explicit block accesses do not supply an output write that could
overwrite the adjacent flags byte. Q-ITEM-013 can stop treating those six
dereferences as unread candidate output stores. It still needs other
direct writes, copied or computed aliases, child destinations, frame
admission and lifetime. No whole-switch read-only claim, complete reading,
native observation or confidence promotion follows.

## Alternatives

- An argument's far-pointer type does not make it an output destination.
- Nonoverlapping read spans do not prove that another child cannot write
  the frame through a different alias.
- An inventory-range operand search is not complete path coverage.
- A preserved far pointer does not establish the separate near-pointer
  segment identity retained by Q-ITEM-011.

## How to reproduce

Use installed DSUN.EXE resolved through tools/game-dir.mjs, size 634416,
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b, and locked evidence Python.
Copy only the header and row starting 0x00065653 from the inventory at
74ce65f6 to a local metadata TSV. Run operand_range.py with that TSV
and bounds A E (hexadecimal). It linearly decodes declared ranges and
restarts after undecodable bytes; its results require bounded review.
Run overlay_listing.py with descriptor 179 and the file bounds listed
in Locations. Reuse FND-ITEM-017's verified caller-pointer report.

Retain local files only in GAME_DIR/analysis/hit-incoming-20261104/:
effect-switch-row.tsv, effect-block-pointer-words.txt,
block-use-65A51.txt, block-use-66086.txt, block-use-66122.txt,
block-use-661AB.txt, block-use-661FF.txt, effect-block-output-tail.txt,
block-index-one.txt, block-index-two.txt and block-index-two-ending.txt.
Some retained windows extend beyond the immediate reads; use the stated
paths and ranges. Original listings and bytes are not committed.
