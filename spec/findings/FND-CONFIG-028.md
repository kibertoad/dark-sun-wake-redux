---
id: FND-CONFIG-028
title: Start Game button branch delegates setup without direct settings writes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 8BF3:0128..8BF3:0325
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 8BF3:0504..8BF3:0598
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV overlay 194; ReportFbovOverlayMap.ps1
environment: null
---

## Observation

The overlay 194 callback at `DSUN.EXE+0x00081130` dispatches one event value
through four consecutive button IDs starting at 19300. Its jump table sends
19300 (`RESOURCE.GFF#WIND/19500`'s Start Game button, FND-UI-024) to
`DSUN.EXE+0x0008126E`. That branch repeats two resident calls three times,
then invokes a local setup helper once with argument 1, updates a few
separate state fields and calls other routines before returning through the
callback's common exit.

The helper at `DSUN.EXE+0x00081634` takes an argument of 1 from this branch.
Its argument-1 path calls resident entries `0160:0B84`, `0058:4FEB`,
`01D0:0008`, `0108:0514`, `01D0:0092` and `0160:0942`, followed by the
common calls `00B8:0182` and `05B0:0043`. The same helper also has an
argument-0 path. Neither the Start Game branch nor this helper directly
addresses the saved settings globals named in FMT-CONFIG-003, the runtime
voice gate `DS:14E4`, or the message-delay word `DS:26B7`.

## Interpretation

The button callback does not itself establish new-game settings values. Its
named callees, later game-start paths, and indirect writes remain possible
initialization points for Q-CONFIG-002 and Q-CONFIG-007.

## Alternatives

The callback's event parameter and the setup helper's full effects were not
established here. The button ID identifies a branch, but this reading alone
does not show that an ordinary click always reaches it. An unexamined callee
may initialize settings or copy a larger block of state.

## How to reproduce

Use `ReportFbovOverlayMap.ps1` on the approved `DSUN.EXE` to locate overlay
194 at `0x00081130`. Disassemble its callback at `0x00081130..0x00081456`,
including the jump table at `0x00081457`, and the local setup helper at
`0x00081634..0x000816C8`. Follow the button-ID comparison at `0x00081258`
and the first jump-table target. Compare the button ID with FND-UI-024.
