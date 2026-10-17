---
id: FND-PARTY-223
title: The fourteen other main inventory codes select frame origins around the portrait
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00077A92..0x00077AE9
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0004B3EF..0x0004BF62
tool: tools/research/exec-census/overlay_listing.py; tools/evidence/report.mjs table; Python 3.12.10, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

FND-PARTY-217 reads the signed position byte and its addition of four
to obtain the logical inventory position. FND-PARTY-222 reads the
rectangle helper's identifier calculation: logical position plus
11212, using the current inventory window. For stored codes zero
through thirteen, this gives logical positions four through seventeen
and APFM identifiers 11216 through 11229. They are inside the main
redraw's four-through-29 interval.

The fingerprint-matching WIND 13500 child records place those
identifiers at these window-relative origins:

| Stored position code | X | Y |
|---|---|---|
| 0 | 57 | 62 |
| 1 | 64 | 18 |
| 2 | 57 | 42 |
| 3 | 57 | 83 |
| 4 | 57 | 101 |
| 5 | 57 | 121 |
| 6 | 165 | 121 |
| 7 | 111 | 18 |
| 8 | 159 | 18 |
| 9 | 165 | 62 |
| 10 | 165 | 83 |
| 11 | 165 | 101 |
| 12 | 165 | 42 |
| 13 | 165 | 141 |

For every row the requested identifier is 11216 plus the stored code.
The child records' stored order is not this numeric order: 11217 and
11218 precede 11216, and 11228 precedes 11225. Selecting a coordinate
by a child's ordinal instead of its identifier would give a different
mapping. FND-UI-030's independent BUTN 11300 control remains at
(12,5) in the same bounded child query.

The manual's fourteen body slots are a source description of the screen
(SRC-MANUAL-1994, page 11); this reading does not assign equipment or
body-part names to individual numeric codes. The table establishes
source origins and identifier correspondence, not those gameplay roles.

## Interpretation

Together with FND-PARTY-222, these are the shipped origins for every
logical position visited by the main four-through-29 draw loop. This
answers that source-coordinate part of Q-PARTY-085. It does not
establish native loaded rectangles, the window's current origin,
generic frame treatment, all image consumers or pixel parity.

Other logical-position paths and code meanings, item legality and
state/pointer lifetime remain open. No original function was executed
and no screenshot was inspected. No complete reading is claimed.

## Alternatives

- Child-array order and numeric position order differ; their equality
  is ruled out by the first three selected identifiers.
- The source table does not by itself label a position as a hand,
  ring, head, armour or another equipment category.
- Rectangle lookup uses APFM identifiers, not bitmap identifiers;
  source geometry does not establish an image's content or size.

## How to reproduce

Use installed RESOURCE.GFF, size 5724669, XXH3-128
92fe031616f36062f99484b0f728210e. Run report.mjs table with start
4B4F4 hexadecimal, count/limit 89 and stride thirty. Select width-four
fields at offsets four and eight for tag and identifier, and width-two
fields at offsets twelve and fourteen for x/y. Filter APFM 11216..11229,
sort by identifier and subtract 11216 to obtain the code column.
All selected coordinate words are nonnegative, so the unsigned report
values equal their signed format interpretation. Count/location evidence
is FND-UI-030 and FMT-UI-001/002. Check BUTN 11300 independently.

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and overlay_listing.py descriptor
189 at 77A92..77AE9, hexadecimal and half-open, to verify the
identifier addition. Follow the caller and output/storage qualifications
in FND-PARTY-217 and FND-PARTY-222. Reports/configurations remain in
GAME_DIR/analysis/party-record-heads/drawing-20261018/.
