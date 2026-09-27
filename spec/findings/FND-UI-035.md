---
id: FND-UI-035
title: Start window opener and callback dispatch its four button IDs
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 574B:0020..574B:002A
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of FBOV overlay 194 and bounded callees; ReportFbovOverlayMap.ps1; FBOV descriptor and trampoline inspection
environment: null
---

## Observation

Overlay 194's resident trampolines `574B:0020`, `574B:0025` and
`574B:002A` target code at file offsets `0x000814AB`, `0x00081634` and
`0x00081130`, respectively. The first routine passes 19500 to a window
acquisition call, stores its returned far pointer, and makes a subsequent
window call. `RESOURCE.GFF#WIND/19500` contains four buttons numbered
19300 through 19303 (FND-UI-024).

For callback event value 2, the routine at `0x00081130` subtracts 19300
from the passed control ID and dispatches values 0 through 3 through a
four-entry jump table:

| Button | Branch file offset | Bounded branch action |
|---|---|---|
| 19300, Start Game | `0x0008126E` | Calls the local setup helper with argument 1, then overlay 187 and 182 entries (FND-CONFIG-028). |
| 19301, Create Characters | `0x00081324` | Calls the same helper with argument 0, then overlay 212's `57C9:0025` trampoline, targeting `0x0009881C`. |
| 19302, Load Saved Game | `0x00081391` | Calls the helper with argument 0, then overlay 192's `5736:0020` trampoline, targeting `0x0007D300`. |
| 19303, Exit to DOS | `0x000813EE` | Reaches the common path that calls the helper with argument 1 and clears `DS:1462`. |

The overlay 192 target sets a byte in a separate state segment to one and
calls its local shared routine. The overlay 212 target stores passed
arguments in globals before continuing. Their full effects are outside this
bounded reading.

## Interpretation

The start window has an executable control-ID dispatcher for all four
button numbers. The Load Saved Game and Create Characters branches enter
different overlays. The button-ID mapping corroborates the resource order;
the downstream screen transitions and final exit behavior still require
their callee paths or native observation.

## Alternatives

This reading does not establish which input gesture produces callback event
value 2, which icon frame represents each visual state, or whether a
separate path bypasses this callback. FND-UI-012 searched the resident image
only, so its negative result does not cover this overlay code.

## How to reproduce

Use `ReportFbovOverlayMap.ps1` on the approved `DSUN.EXE` to locate overlay
194 at `0x00081130`. Read the three resident trampolines in its header,
disassemble the opener at `0x000814AB..0x00081633` and the callback at
`0x00081130..0x00081456`, and read the jump table at `0x00081457`.
Resolve the two named far calls through their FBOV descriptors and
trampolines. Compare the button IDs with FND-UI-024.
