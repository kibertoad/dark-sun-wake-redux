---
id: FND-ITEM-077
title: Removal redraw clips and aligns its rectangle but returns success after later capture failures
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:128C..31E0:1478
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 371D:071E..371D:0770
tool: tools/research/exec-census/resident_listing.py at ac2485e3; Python 3.12.10, Capstone 5.0.7; composition with FND-ITEM-073, FND-PARTY-033, FND-RNG-097 and FND-REGION-006
environment: null
---

## Observation

Q-PARTY-116 retains redraw after local removal, queue and cell contracts.
FND-ITEM-073 supplies its actual argument producers. This reads the full
31E0:128C body and caller continuation, reusing the established sub-entry
and terrain-request descriptions without claiming native pixels or full
callee preservation. Addresses are hexadecimal; dimensions are decimal.

The eight arguments at BP+06 through BP+14 are origin x/y, page, left,
top, right, bottom and a final flag byte. The removal caller supplies
current DS:1408/140A as origin, page one, its four accumulated bounds and
final byte one. The routine saves SI/DI and initializes its second
temporary handle to FFFF.

### Clipping and initial sub-entry

All four bounds are compared signed. Negative left/top become zero;
left above 319 or top above 199 returns AL zero. Right at least 320
becomes 319, while negative right returns zero. Bottom at least 200
becomes 199, while negative bottom returns zero. There is no own later
test requiring left<=right or top<=bottom.

It aligns left downward to a multiple of four. It adds four to the
clipped inclusive right and clears its low two bits, producing an
exclusive right edge at the next multiple of four, at most 320. It
requests 1BF3:282D(page, aligned_left, top, exclusive_right-1, bottom).
An exact FFFF handle returns AL zero. Other word results continue;
this local test does not validate page/header/storage identity.

After an accepted initial handle it requests 2C5F:0791 with one and a
zero double word, terrain drawing through 362C:04BF with page, aligned
rectangle and origin, and 31BA:000E with origin, that handle and literal
three. It then requests release through 2D40:3BD1. FND-REGION-006
describes the terrain request's separate map/TILE behavior; FND-PARTY-033
and FND-RNG-097 describe sub-entry aliasing and release prerequisites.
The current routine does not test these children's results.

### Optional continuation and cleanup

Final byte zero skips directly to AL one after the first handle release.
The removal caller's byte one instead takes the following path:

1. Read two words through two 1000:060E far-pointer results, formed from
   DS:A14D/A14F/A151 and table fields +6/+8. Their native table/index and
   returned-pointer admission are not established by these dereferences.
2. Pass the saved pair to 28C9:2C50, then request 2C5F:070E(page),
   28C9:2AD0 with DS:A10D, one and near output pointers to that pair,
   2C5F:06CB(1,1), 3D72:0DE5(1) and 1BF3:5814(FFFF), in that order.
   These children can affect presentation state; none is modeled as a
   harmless return.
3. Request a new 1BF3:282D handle on page zero for the aligned rectangle,
   passing the exclusive right value directly this time. If it is not
   FFFF, request the corresponding page-one handle. Only if both pass
   their FFFF tests call 2D40:3B30 with the two handles and 28C9:2B3B
   with one, DS:A10D and the saved pair.
4. Independently release each of these two handles whose saved word is
   not FFFF, then call 28C9:2C80. The second handle remains FFFF when
   the first capture fails. After this cleanup return AL one, including
   either capture-failure branch.

The initial handle is released before this optional path and its local
slot is overwritten by the page-zero request. No local branch releases
an FFFF optional handle or reuses the old initial handle for that cleanup.
Native child effects, successful releases and preservation of handles,
arguments and frames remain explicit conditions.

### Removal does not gate scheduling on the result

371D:073B removes the redraw arguments from the stack and immediately
prepares the slot/mode payload described by FND-ITEM-073. It does not
test AL before computing time and calling the queue insertion. Thus an
early zero result does not itself suppress the deferred request. Conversely
AL one does not prove every later capture or copy succeeded. This is
local caller ordering, not a native scheduling or visible redraw result.

## Interpretation

This removes 128C's own clipping/alignment, call sequencing, handle tests,
cleanup and return-value ambiguity from Q-PARTY-116. The remaining redraw
work is the admitted page/table/rectangle/source state and named graphics
children, their effects and use-time preservation. Native row/slot/map
lifetime and unload effects remain separate. The routine's own stores are
in its frame/stack arguments; this does not constrain children's writes.

## Alternatives

A blanket success interpretation of AL one ignores the optional capture
failure branches. Treating zero as a cancellation of later removal
scheduling adds a caller gate that is absent. Clipping does not reorder
inverted bounds or validate graphics storage. No original was run,
complete reading declared or parity status promoted.

## How to reproduce

Use approved DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run resident_listing.py with -I and
the two location ranges above in order. Under
GAME_DIR/analysis/item-removal-20261104/, effect-removal-redraw-contract.txt
and effect-removal-redraw-contract-repeat.txt both have SHA-256
ae11f1411c21e83b180f3e581e7d7d2ec9e6b1cd7b323171451ca6e64699e7c0.
Check the signed bounds, two different right-edge arguments, first-handle
release before optional calls, both FFFF cleanup gates and the caller's
missing result test. Keep original listings and captures only locally.
