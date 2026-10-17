---
id: FND-PARTY-222
title: Backpack position codes select twelve inventory frame origins in two columns of six
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00075DE2..0x00075E1B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00076487..0x000764AC
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

FND-PARTY-217 identifies the inventory redraw at descriptor 189 +1432,
which projects the graph and visits logical positions four through 29.
For each nonempty map word it calls +1AD7. FND-PARTY-220 identifies
the recipient backpack code interval 14..25; the projection adds four,
making those logical indices 18..29.

### The drawing request uses a frame identifier derived from the slot

At +1AD7 the drawing entry passes its logical-position word to local
+30E2, file 77A92, with four far output pointers. The first two point
to separate local x/y words. The final two intentionally point to the
same local word. The helper's later third and fourth output stores
therefore overwrite that shared word in order; they are not four
independent destination words.

+30E2 returns without output stores when its signed position is below
four. Otherwise it adds 2BCC hexadecimal to that word, retains the
low word, sign-extends it to a doubleword and requests a rectangle
through 3F96:01A6, using the window pointer at 5072:003C and an
eight-byte SS-local result buffer. There is no own upper-position
check. It copies the four local words to the supplied far outputs in
order and ignores the rectangle helper's AX.

FND-COMBAT-038 independently reads 3F96:01A6: on its normal local
lookup path it clears eight output bytes, looks for APFM with the
low identifier word, returns FFFF for a missing control, or copies
eight bytes from the found record's +10 hexadecimal and returns zero.
Guards, loader state, lookup and copy preservation remain conditions.
A zero coordinate pair alone is not proof that the requested frame
was successfully found.

For the backpack logical indices 18..29, the addition does not wrap
and the identifiers are 11230..11241. For a stored position code p
in 14..25, the same identifier is 11216 + p. This is a request for a
frame's rectangle, not a request for an image numbered after the slot.

### The shipped inventory placements

FND-UI-030 locates WIND 13500 at RESOURCE.GFF offset 4B3EF,
length 2931, with 89 child records after its 261-byte prefix.
Reading those records under FMT-UI-002 gives APFM children with
identifiers 11230..11241 at these window-relative origins:

| Stored position code | X | Y |
|---|---|---|
| 14 | 186 | 18 |
| 15 | 186 | 36 |
| 16 | 186 | 54 |
| 17 | 186 | 72 |
| 18 | 186 | 90 |
| 19 | 186 | 108 |
| 20 | 204 | 18 |
| 21 | 204 | 36 |
| 22 | 204 | 54 |
| 23 | 204 | 72 |
| 24 | 204 | 90 |
| 25 | 204 | 108 |

The identifiers and child coordinates, rather than a visual guess,
establish this column-first order. In formula form, for column c in
zero..one and row r in zero..five, code is 14 + 6*c + r, logical
index is 18 + 6*c + r, frame identifier is 11230 + 6*c + r,
and child origin is (186 + 18*c, 18 + 18*r).

As an independent control, the same child query finds BUTN 11300
at (12,5), agreeing with the previously recorded character-box
placement in FND-UI-030. The query does not infer a rectangle's
width or height from the spacing between these origins.

## Interpretation

This answers the twelve backpack source-placement origins and their
slot/control bindings in Q-PARTY-085. Source positions are coordinates
relative to the window under FMT-UI-002. They do not by themselves
prove the current loaded rectangle, window origin, native frame
treatment, image placement or pixel parity.

Other slot codes, all coordinate and image consumers, pointer/graph
admission, later control mutations and native presentation remain
open. No original function was executed and no original screenshot
was inspected. No complete reading of the drawing helper is claimed.

## Alternatives

- Row-first numbering would put code 15 in the second column; the
  shipped child record for its requested identifier instead has x=186.
- Treating the identifier as an ICON number conflicts with the
  rectangle helper's APFM lookup.
- Treating zero outputs as successful placement omits the ignored
  lookup status and the rectangle helper's missing-control route.
- Treating the last two caller outputs as independent misses their
  identical far addresses. Their alias does not merge the first two.
- Spacing of eighteen pixels does not establish a frame size, bevel,
  clipping treatment or native rendered interior.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and RESOURCE.GFF, size 5724669,
XXH3-128 92fe031616f36062f99484b0f728210e.
With the locked evidence Python read overlay_listing.py descriptor
189 at 75DE2..75E1B, 76487..764AC and 77A92..77AE9,
hexadecimal, half-open. Follow the existing root/caller qualification
in FND-PARTY-217 and the rectangle helper in FND-COMBAT-038.

Run report.mjs table on RESOURCE.GFF with that explicit hash and
start 4B4F4, count/limit 89, stride thirty. Use width-four fields at
offset four for the tag and eight for the identifier, and width-two
fields at offsets twelve and fourteen for x/y. The count and starting
offset follow the WIND layout and FND-UI-030, not a scan for nearby
numbers. Filter APFM identifiers 11230..11241; all selected coordinate
words are nonnegative, so their unsigned table values equal the
format's signed coordinates. Retain BUTN 11300 at (12,5) as the
independent positive control.

Inspect each output pointer and the ignored lookup status. Do not
infer current control lifetime or image pixels from this source
table. Configurations/reports remain under
GAME_DIR/analysis/party-record-heads/drawing-20261018/.
