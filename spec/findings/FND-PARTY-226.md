---
id: FND-PARTY-226
title: Inventory image requests use signed dimensions to choose centered placement or a temporary view
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007663D..0x00076728
tool: tools/research/exec-census/overlay_listing.py; Python 3.12.10, Capstone wheel 5.0.9 (module version 5.0.7), xxhash 4.0.1
environment: null
---

## Observation

FND-PARTY-224 follows the selected record's BMP number into this
shared tail of overlay 189's inventory drawing function. The local
far image pointer is cleared, offered to 38FF:04AB with that number
and tag BMP, and tested after the request. AX is ignored. A null
pointer returns immediately; a nonnull pointer continues without a
local resource-size or frame-storage check.

The caller asks 1BF3:76C2 and 1BF3:771C for frame-zero width and
height. FND-CONFIG-190 records their complete local bodies, raw word
returns, FFFF index-failure result and normal register restoration.
In particular, the height reader preserves the width retained in SI.
The following comparisons are signed word comparisons. If either
dimension exceeds 16, the temporary-view path is taken; otherwise
the centered path is taken. There is no own positive-dimension test.

Let x and y denote the two local coordinate words already supplied
by the slot lookup, not necessarily the shipped source origins.
All coordinate additions and subtractions below retain word width.

- The temporary-view path calls 1BF3:282D with words
  (1, x+1, y+1, x+16, y+16). It retains returned AX in SI. FFFF
  returns immediately, before this body's later marker write and
  release request. Other returned words are passed to 2D40:3BEC
  with x+1, y+1, fourth word zero, the image far pointer and frame
  zero. The draw result is ignored. It next passes the current SI
  word to 2D40:3BD1, also ignoring its result. This describes the
  local operand; preservation through the draw backend remains a
  condition for identifying it with the original returned handle.
- The centered path calls 2D40:3BEC with first word 1, fourth word
  zero, that image pointer and frame zero. Its x argument is
  x+1+trunc0(int16(16-width)/2); its y argument uses height in the
  same expression. Here int16 interprets the wrapped subtraction
  as signed, and trunc0 divides toward zero. The sign adjustment
  and arithmetic shift operate on words. The draw result is ignored.

FND-COMBAT-038 supplies the draw wrapper's argument order and
separates its backend from the request. FND-CONFIG-191 supplies the
temporary-view constructor's own reference traversal and coordinate
checks, including writes retained on a failed request. Thus the
FFFF branch here is not proof of an unchanged graphics state.
FND-CONFIG-184 records the release wrappers' local behavior and
the unresolved runtime and VGA effects.

After either returning draw path, this body writes byte one at the
current DS:2FB8, reloads the local far image pointer and passes it
to 444C:0092. The result is ignored and the function returns. There
is no own matching release call on the view-constructor FFFF exit.
This is an observed local branch difference, not proof of a global
allocation leak or of successful release on the other paths.

For static arithmetic controls with ordinary nonwrapping origins,
width 16 places x at x+1, width 14 at x+2, and width zero at x+9.
Raw FFFF is signed -1 and also produces x+9 in the centered path
when the other dimension does not select the temporary view. Width
17 selects the temporary view. These controls were not game runs;
no claim that these dimensions occur in an admitted native resource
is made. A dimension helper's FFFF result is not a local early exit.

## Interpretation

The caller's image-placement requests and its two explicit early
returns are mapped. This removes the local dimension/placement and
cleanup branch reading from Q-PARTY-085. It does not establish actual
loaded coordinates, admitted image storage, backend register/global
preservation, clipping, pixels or complete record/root provenance.

## Alternatives

- Centering every image omits the signed greater-than-16 branch.
- Treating dimensions as unsigned, or rejecting zero and FFFF here,
  introduces tests this caller does not make.
- Treating the temporary-view request as successful clipping ignores
  both the constructor's failure path and the unresolved backend.
- Treating every nonnull image as reaching the release request omits
  the FFFF view-constructor exit.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and locked evidence Python.
Run overlay_listing.py for descriptor 189 from 76487 to 76728,
hexadecimal and half-open. Follow the linear decode from the declared
function entry across the inventory fragment anomaly described in
FND-PARTY-224, then inspect 7663D onward. The retained report is
GAME_DIR/analysis/party-record-heads/drawing-20261018/draw-linear-context.txt.

Check operands and operand size directly at the two sign-extension
instructions in the centered path: unprefixed opcode 99 in this
16-bit body extends AX into DX:AX. The pinned decoder prints cdq
there incorrectly; a synthetic reproduction is reported in toolkit
issue https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/538.
Track wrapped subtraction, signed division, current registers after
children, and the local release branch separately. Reuse the cited
child findings rather than interpreting a request as a pixel result.
