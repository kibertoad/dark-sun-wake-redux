---
id: FND-COMBAT-005
title: DSUN.EXE holds no string Moves; the status panel's caption is the format Move : %d
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:0F60..57E0:0F6A
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1; byte search with Python 3.14.7
environment: null
---

## Observation

The bytes `Moves`, with or without a NUL after them, do not occur in `DSUN.EXE`, in the load image
or in the overlay pack. The string `Move : %d` does, once, at `57E0:0F60` (file offset `0x4DF60`),
and `MOVES` does not occur.

## Interpretation

No executable string reads `Moves`. The fourth line of the status panel is built from `Move : %d`
(FND-COMBAT-022).

## Alternatives

The legacy record searched for `Moves` because it read the panel's caption in the captures as
`Moves 15` and `Moves 20`. The captures show `Move : 15` and `Move : 9` (FND-COMBAT-018,
FND-COMBAT-019), so the negative result follows from the misreading.

## How to reproduce

Search the whole file for `Moves`, `MOVES` and `Move :`. In the overlay-mapped copy described in
`docs/GHIDRA.md`, "FBOV mapped image", `ReportBytePattern` for `4D 6F 76 65 73 00` finds nothing.
