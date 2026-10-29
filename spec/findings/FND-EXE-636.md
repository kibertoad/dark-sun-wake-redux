---
id: FND-EXE-636
title: Overlay tail census distinguishes shared-frame continuations from interior decode gaps
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066A12..0x00066B11
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006B581..0x0006B7A1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000705F6..0x0007060A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C647..0x0008C7CB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C8E6..0x0008C90E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C9AB..0x0008C9AF
tool: inventory_tails.py at 106df5d0; overlay_listing.py at fc630230; Ghidra 12.1.3 exports described by FND-EXE-635
environment: null
---

## Observation

The bounded instruction-level final-range census checks all 877 committed
overlay rows, skipping 2356 resident rows. It reports 863 returns, eight
unconditional jumps, three fall-throughs into adjacent entries and three
fall-throughs into unowned bytes. No final range has an incomplete decode.
The three unowned cases are exactly the truncated tails in FND-EXE-633.
This is not the older last-byte heuristic: it decodes instructions and does
not mistake an immediate operand for a terminal opcode.

### Adjacent entries continue existing frames

| Parent start | Adjacent start | Exclusive return end | Direct boundary evidence |
|---|---|---|---|
| 0x00065653 | 0x00066A2C | 0x00066B11 | Internal jumps at 0x00066A12 and 0x00066A1F enter the adjacent block; a conditional at 0x00066A2A also falls into it. |
| 0x0006B581 | 0x0006B749 | 0x0006B7A1 | The parent's default branch at 0x0006B593 enters this block after establishing its frame. |
| 0x0007054D | 0x00070607 | 0x0007060A | The boundary splits the saved-register restoration: one restore is before it and the remaining restore, frame teardown and far return follow it. |

These blocks use or restore their parent's existing frame, rather than
establishing independent call frames. The first reads a parameter through
that frame; the second reaches the parent's frame teardown. They belong
to the respective parent bodies for inventory ownership. This does not
assert that no stored pointer or indirect transfer can target an internal
label, nor does it establish the complete behavior of any parent.

### Terminal returns do not detect interior omissions

The full-model control of FND-EXE-635 has 878 overlay rows: 866 returns,
ten unconditional jumps and two adjacent fall-throughs. Its owner at
0x0006B581 already incorporates the block at 0x0006B749. The isolated
three-tail control also has 878 rows, but 866 returns, eleven jumps and
one adjacent fall-through. Different body ownership explains why these
counts cannot be used interchangeably.

Comparing actual ranges, rather than subtracting sizes from an assumed
contiguous end, exposes these omissions in the full-model control:

| Owner | Omitted half-open range | Bytes |
|---|---|---:|
| 0x0008C647 | 0x0008C686..0x0008C690 | 10 |
| 0x0008C647 | 0x0008C6FF..0x0008C7C7 | 200 |
| 0x0008C834 | 0x0008C8FE..0x0008C90E | 16 |

The first gap is a conditional path between already decoded blocks. The
second continues the preceding arithmetic and conditional paths into the
existing final restore sequence. Its end is still 0x0008C7CB. The third
continues argument cleanup into arithmetic and a direct jump to an existing
internal block. Its owner's final restores and far return remain at
0x0008C9AB..0x0008C9AF. These 226 bytes are interior omissions, not truncated
routine ends. The older five-byte gap at 0x0008C97D..0x0008C982 is present in
both views and is not included in this proposed repair.

## Interpretation

Q-EXE-032 remains open. The declarations and accepted joined inventory have
not yet been regenerated with these ownership and interior-range repairs.
The two additional helper starts and all remaining replacement differences
still need reconciliation. Neither this census nor this bounded reading is
an accepted complete function reading or an established gameplay claim.

## Alternatives

- Treating every adjacent inventoried start as a separate callable routine
  fails at these three boundaries: the paths retain the parent's frame and
  reach its cleanup. Alternate internal entry labels remain possible.
- Treating a terminal return as proof of a complete body fails for the two
  owners with the interior omissions above. The census deliberately checks
  only the final range and cannot supply a full control-flow proof.
- Treating a body-size decrease as a shortened end fails for these sparse
  bodies: their final return ranges remain present.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3
e296af55ba2ecde7e77f555c90f33d0b, and the mapped-image identities and frozen
controls in FND-EXE-635. No original-game execution is involved.

Run tools/research/exec-census/inventory_tails.py with the committed mixed
inventory in file view, or the isolated/full-model exports in mapped view,
using its documented CLI in docs/EVIDENCE-TOOLS.md. The retained reports are
GAME_DIR/analysis/inventory-reconcile-20261029/runs/tail-census-03/
committed.json, isolated.json and full-model.json. Reuse them when their
input and tool identities match. Python is 3.12.10; the Capstone distribution
is 5.0.9, its binding version string is 5.0.7 and its native version tuple is
(5, 0, 1280). These distinct identities are recorded by the checker.

Compare the frozen exports' complete range columns. Confirm the listed
boundaries with overlay_listing.py SOURCE DESCRIPTOR START END, using
descriptor 179 for 0x00066A12..0x00066B11, 183 for
0x0006B581..0x0006B7A1, 187 for 0x000705F6..0x0007060A, and 204 for the
interior ranges and their preceding decoded instructions. Begin each
inspection at a validated instruction boundary; do not decode embedded
switch data as code. The listing is local licensed evidence, not a Git asset.

