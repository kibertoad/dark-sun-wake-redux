---
id: FND-UI-038
title: Save and Load callback navigates ten rows and tests available records in Load mode
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5736:002F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5664:0000
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV overlays 192 and 171; FBOV descriptor inspection
environment: null
---

## Observation

Overlay 192's Save/Load callback has an event-6 table with input words
`0x011B`, `0x1C0D`, `0x4800` and `0x5000` (FND-UI-037). The last two
dispatch to `DSUN.EXE+0x0007DBE2` and `0x0007DAC6`, respectively.

The `0x5000` branch increments the row-selection word at `4C4C:0000` if
it is below 9, otherwise sets it to 0. The `0x4800` branch decrements
that word if it is above 0, otherwise sets it to 9. Both branches redraw
the old and new row controls and call the row-to-name-box helper with the
selected row. In Load mode, the branches test the first byte of the
125-byte record indexed by the word at `4C4C:0002` plus the selected row;
an empty marker
causes another move or exits a bounded retry path. Save mode does not
take that marker guard in these navigation branches. FND-SAVE-007 traces
how the list builder clears this marker after a failed archive open or
`STXT/1` request.

The callback reads `4C4C:0002` as the base index when locating records.
Overlay 192 has no direct write to that word in its code range. Overlay
171, which also uses the descriptor-95 segment, writes it at
`DSUN.EXE+0x0005830A`, `0x00058718`, `0x00058755`, `0x00058858`,
`0x000588C2` and `0x0005890D` in its stored-character list paths
(FND-PARTY-012).

## Interpretation

The two event-6 words navigate the ten visible rows in opposite
directions, wrapping at the ends. Load-mode navigation attempts to avoid
rows whose saved-game record was not read successfully. The base index
is shared state, so its value on entering the Save/Load screen needs a
caller or lifecycle trace before the numbered slot can be assumed to
start at zero.

## Alternatives

This finding does not identify which physical keys generate `0x4800` and
`0x5000`. The row-to-name-box helper and the final effect of its control
calls were not fully read. An indirect callee could reset `4C4C:0002`
before the Save/Load callback; the cross-overlay writes alone do not
establish that a character-list value carries into this screen. The
record-marker retry behavior on a list with no readable saves needs a
complete path reading before specifying its final selection.

## How to reproduce

Resolve overlay 192 and its `5736:002F` callback trampoline with
`tools/ghidra/ReportFbovOverlayMap.ps1`. Read the event-6 word and target
tables at `0x0007E048..0x0007E057`, then the two bounded branches at
`0x0007DAC6..0x0007DD34`. Compare their mode and record-marker tests
with FND-SAVE-007. Search overlay 192 for direct writes to the
descriptor-95 word at offset 2; inspect the named writes in overlay 171
after resolving its descriptor.
