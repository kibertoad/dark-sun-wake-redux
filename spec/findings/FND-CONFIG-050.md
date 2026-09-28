---
id: FND-CONFIG-050
title: Five late overlays share the message entry for item and character feedback
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57A0:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57B0:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57B9:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57C1:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57CE:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:002A
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV windows; FBOV fixup and overlay-map inspection
environment: null
---

## Observation

Each of overlays 207, 209, 210, 211 and 213 has one direct far call to
overlay 172's `566A:002A` message entry (FND-CONFIG-035):

| Overlay and call file offset | Local path and passed text |
|---|---|
| 207, `0x00090F28` | The selected record's word at `+6` equals `0x270F`. This path passes `DS:2967`, a failed-sale message, and jumps past the other sale path. |
| 209, `0x000943FA` | A loop tests values 1 through 17 with a local helper and records whether any returned nonzero. Only when none did does this path pass `DS:2B1B`, a no-other-classes message. |
| 210, `0x00095A6B` | Inside an actor loop, a selected actor has passed the local record tests and a field at `+0x1E` has been incremented. The routine formats `DS:2B68` with a name, level and class text into a stack buffer; when that buffer is longer than `0x1D`, it instead formats the shorter `DS:2B87`. The shared call receives the resulting buffer before further local updates. |
| 211, `0x00096790` | The word at `4C10:0019` is nonzero. This path passes `DS:2C3B`, a refusal to cast from a character other than the active one, and jumps past its noncombat branch. |
| 213, `0x00099B72` | A shared call sink receives either a stack buffer or a literal pointer. Visible incoming paths use `DS:2E87` for no effect, `DS:2E97` for learning a spell, `DS:2EAB` for one already known, a buffer formatted from `DS:2EC7` or `DS:2EDA` for enhancement or learning, and `DS:2EE6` when the item cannot teach. The entry's earlier guards and helpers select among these paths; this window does not constitute a complete reading of that item rule. |

The first four calls each pass one far text pointer. Overlay 213 funnels
multiple text branches into one call instruction at `0x00099B72`. Each call
cleans up four argument bytes, either at the site or its continuation.

## Interpretation

These direct sites add item and character feedback to the message-call
inventory. Each call can enter overlay 172, but only successful subsequent
`WIND/10501` acquisition and setup permits the message-delay wait
(FND-CONFIG-018).

## Alternatives

The caller inputs, complete class and level rules, and every path into
overlay 213's shared sink were not established by these bounded readings.
The text arguments do not prove that any particular result occurs in a live
state, nor that the window setup succeeds.

## How to reproduce

Disassemble the approved `DSUN.EXE` in bounded windows at
`0x00090EF4..0x00090F33`, `0x000943A1..0x00094405`,
`0x0009581C..0x00095A73`, `0x0009677F..0x00096798`, and
`0x0009997D..0x00099B7A`. Resolve each listed call through its `0x0560`
FBOV fixup to overlay 172's `566A:002A` entry. Read only the short
data-segment strings named above and use the overlay map to check the
caller descriptors.
