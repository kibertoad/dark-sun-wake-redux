---
id: FND-SCRIPT-037
title: The zero-height second-entry geometry gate tests only interior grid-cell bit seven and skips both endpoints
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1B0B:0088..1B0B:00A4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1B0B:00A4..1B0B:0142
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1B0B:0142..1B0B:017E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2778:0006..2778:0035
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:207E..2D40:2196
tool: tools/research/exec-census/resident_listing.py at a4d5abe3; Python 3.12.10, Capstone module 5.0.7; composition with FND-PARTY-098, FND-EXPLORE-001 and FND-COMBAT-060
environment: null
---

## Observation

FND-PARTY-098 leaves 1B0B:00A4's return meaning open in the second-entry
script gate. The complete local bodies resolve its zero-height map-cell
case. Source DSUN.EXE is length 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, at analysis load segment 1000.
No original function was executed.

### Admitted domain and caller

Use two endpoint cells with x in 0..127 and y in 0..63, both height
arguments zero. The pointer at 57E0:0538 names at least 8192 readable
grid bytes with valid nonwrapping offsets. Scratch, grid, frames and
argument slots are valid and disjoint. All numbers below describe this
domain; arbitrary wrapped/negative coordinates and nonzero heights are
not admitted by this result.

The second-entry runner 2D40:207E passes both heights as zero and derives
each coordinate by logical right shift four of a position word. The shift
alone does not establish the map-cell bounds above. Slot/position/grid
producers and preservation at that caller remain independent obligations.

### Axis setup and interior sampling

Wrapper 00A4 saves incoming DS, SI and DI, then sets DS to 4C49. It
compares endpoint x words unsigned. When the first is not greater, it
swaps all three coordinate words of the endpoints in its incoming stack
arguments. Thus the local starting x is the larger one; equality also
takes the swap. It does not modify the caller's source position records.

Axis leaf 0088 stores the starting coordinate, forms the word difference
to the destination, stores its absolute magnitude and direction +1 or -1,
and ends with comparison against 255. The wrapper prepares height first.
The zero height difference leaves that test accepted; the intervening
MOV instructions do not overwrite its flags before the conditional branch.
It then prepares x and y and chooses C=max(abs(dx),abs(dy)). In this
domain C is 0..127 and all differences are representable without wrap.

If C is zero the wrapper returns AX zero immediately. Otherwise it sets
remaining count to C, both x/y error accumulators to floor(C/2), and
the height error to zero. It repeatedly invokes near step helper 0142:

- Add abs(dx) to the x error; at error >= C, advance x by its direction
  and subtract C. The comparison is signed in the original.
- Do the same for y using abs(dy).
- Advance height while its accumulated magnitude reaches C. With zero
  height difference and zero initial height error this loop does not run.

After each step, the wrapper decrements remaining count. Zero returns AX
zero before the sampled-cell call. Otherwise it calls 2778:0006. A result
of FFFF returns immediately; zero repeats the next step. This order skips
the initial endpoint before the first step and the final endpoint after
the C-th step. There are at most C-1, hence at most 126, grid samples.

Predicate 2778:0006 reads the far grid pointer from 57E0:0538 and computes
offset base + low16(128*y) + x from current DS words +19 and +17.
It returns FFFF exactly when that cell byte's bit seven is set, otherwise
zero. It makes no child call, performs no grid write and preserves the
loop's DX, ES and DI; BX, CX and SI are not modified. There is no bounds
check in the predicate itself. FND-EXPLORE-001 already records this leaf's
bit test; this reading identifies the coordinates and sampling order its
sole known caller supplies.

### Return and state footprint

For the admitted domain the gate returns only zero or FFFF: zero for no
sampled interior cell with bit seven set, FFFF at the first such cell.
For C=0 or C=1 it returns zero without any grid sample, regardless of
endpoint bits. For a horizontal two-cell separation it samples the one
middle cell; a set endpoint alone cannot make that call return FFFF.
These are static branch deductions, not native captures.

The wrapper restores saved DS, SI and DI. Apart from its stack argument
swaps and stack traffic, its writes and the axis/step writes are to scratch
words at 4C49 offsets 0B,0D,0F,11,13,15,17,19,1B,1D,21 (hex).
The last word is the remaining count. No child writes to grid or script
operand storage occur in this domain. These scratch addresses physically
overlap the 4C13-based state starting at offset 036B, as FND-COMBAT-060
already warns; distinct segment names do not imply separate memory.

## Interpretation

The second-entry condition-one gate's zero helper result means no set
bit seven among this raster's interior cells under the admitted domain.
Condition two requires the FFFF case as well as its signed distance test.
It is not a test of either endpoint, all geometrically intersected cells,
movement bit six, or occupancy bit five. The record's distance threshold
is a separate calculation through 1B0B:000C.

This removes the helper's zero-height local return and cell-sampling
meaning from Q-SCRIPT-017. Valid use-time coordinates/grid storage,
record identity, other callers/heights and state lifetimes remain open.
No general visibility rule, native phase admission, complete-reading or
parity promotion follows.

## Alternatives

A whole-segment test including endpoints would reject a blocked endpoint
even for adjacent cells. The decrement-before-sample branch instead returns
zero there. A movement-blocking interpretation would require bit six;
the leaf tests bit seven only. Nonzero-height or out-of-domain arguments
must be read separately rather than extending this conditional contract.

## How to reproduce

Run resident_listing.py with the verified source over the five half-open
ranges in Locations. Retained files under
GAME_DIR/analysis/walk-script-20261104/ are geometry-gate.txt (00A4 and
0142), geometry-children.txt (0088 and 2778:0006), and entry-runners.txt
(the second-entry caller). Follow the actual final comparison flags across
the MOV instructions, argument swaps, initialized error/count values,
decrement-before-call and the predicate's complete register/write footprint.
Check C=0, C=1 and a horizontal C=2 with a set middle or endpoint bit
independently of the separate distance helper. No original code or data
fixtures are committed.
