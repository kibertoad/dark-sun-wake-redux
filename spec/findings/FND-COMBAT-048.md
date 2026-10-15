---
id: FND-COMBAT-048
title: The end-of-move position predicate tests an inclusive anchor rectangle with wrapped unsigned differences
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:000A..28C9:0061
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006A472..0x0006A501
tool: tools/research/exec-census/resident_listing.py, overlay_listing.py and trampoline_target.py; Python 3.12.10, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

The complete local body at 28C9:000A..0061 has one slot argument at
SS:BP+6, no child calls and no stores outside its ordinary stack frame.
It reads words through DS, with object record base DS:67BB and stride
37, and uses the words at record offsets 0A and 0C. FMT-ACTOR-004 names
these position_x and position_y, the object's anchor coordinates. They
differ from the figure's x and y at record offsets three and five.

The predicate returns word one only when both conditions hold:

- The low 16 bits of position_x minus DS:1408 are between 64 and 136,
  inclusive, when interpreted unsigned.
- The low 16 bits of position_y minus DS:140A are between 64 and 136,
  inclusive, when interpreted unsigned.

Each rejected comparison returns word zero. The bounds use JB and JA,
not signed branches. Each upper-bound test reloads the relevant record
word and view offset instead of reusing the earlier difference. With
stable ordinary memory, 63 and 137 fail, 64 and 136 pass that axis,
and a difference wrapping to FFFF fails. This is a branch reading,
not an executed synthetic or native experiment. There is no local
slot-count check, figure-dimension test or rendered-pixel test.

FND-COMBAT-026 identifies the menu root as descriptor 182 +1C22;
trampoline_target.py independently resolves 56BD:00CA to it at file
6A472. Its reached +1C75 continuation pushes the current DS:426D slot
and calls this predicate. On return it removes the slot word, clears AH
and tests AX, so it consumes only the predicate's low byte.

With nonzero result it bypasses the adjustment call and retains the
incoming figure coordinates saved in SI and DI. With zero it calls
2C5F:018C with the current slot and word one, then rereads the current
DS:426D slot and figure x/y, subtracting the current view offsets.
Those rereads occur after the adjustment call; the local body does not
establish that slot, view or object state remained unchanged across it.

## Interpretation

This settles the local test performed by fn_28C9_000A in Q-COMBAT-006.
It is an anchor-in-central-rectangle predicate, not proof that the whole
figure is visible or that the adjustment successfully changes the view.
The adjustment body, native view effects, admitted slot/DS storage,
all producers and complete callers remain outside this finding.

## Alternatives

- A whole-screen or figure-bounds visibility test does not match the
  fixed 64..136 interval or the anchor-coordinate fields read.
- Signed comparisons would admit different wrapped differences; the
  instruction conditions here are unsigned on each axis.
- The menu does not always recompute its coordinates: the nonzero
  predicate branch retains the coordinates supplied to the menu.

## How to reproduce

Use approved installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, after the documented identity gate.
Run resident_listing.py with 28C9:000A..28C9:0061, matching the committed
function inventory's body end. Resolve 56BD:00CA with
trampoline_target.py and read overlay_listing.py descriptor 182,
6A472..6A501 hexadecimal, half-open. Follow the predicate call, AH
clear, zero/nonzero branch and post-adjustment reloads from the root.
Retain current-DS/storage admission and prior formatting calls as
separate dependencies. Reports remain in GAME_DIR/analysis/combat-actions/;
no original code, bytes or screenshots are committed or executed.
