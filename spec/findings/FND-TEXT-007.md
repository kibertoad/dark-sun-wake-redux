---
id: FND-TEXT-007
title: Each of the five TEXT tag occurrences in DSUN.EXE is the tag operand of a resource request, two in resident code and three in overlays 186 and 188
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:05D5..277B:06AF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006FCDD..0x0006FDAD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00072EA0..0x0007325A
tool: tools/research/exec-census/resident_listing.py, overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

The input is the installed `DSUN.EXE` (634,416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`). FND-TEXT-004 found the four bytes `TEXT` at load-image
addresses `2000:7DB5` and `2000:7E14` and at files `0x0006FD40`, `0x00073103` and `0x00073173`,
and no others in the file. Read as instructions from the routines that hold them:

- `2000:7DB5` is `277B:0605` and `2000:7E14` is `277B:0664`, inside the routine `277B:05D5`
  (`2000:7D85`). Each is the immediate of a 32-bit push at `277B:0603` and `277B:0662`, which
  follows the push of the resource number 98 and 99 and of a far pointer to the routine's
  destination pointer, and precedes a far call of the resource request `38FF:04AB` at
  `277B:0609` and `277B:0668`.
- `0x0006FD40` is the immediate of a 32-bit push at overlay 186 `+063E`, inside the routine
  `+05DD`, followed by a far call of `38FF:04AB` at `+0644`.
- `0x00073103` and `0x00073173` are the immediates of 32-bit pushes at overlay 188 `+0261` and
  `+02D1`, inside the routine `+0000`, each after a push of the number 1,000; they are followed by
  far calls of the size request `38FF:05B5` at `+0267` and of `38FF:04AB` at `+02D7`.

`direct_callers.py` finds `277B:05D5` called far from overlay 190 `+1567` and overlay 203
`+0990`, and overlay 186 `+05DD` called far, through trampoline `56E9:002F`, from overlay 183
`+020F` and overlay 184 `+0B9F`.

## Interpretation

Every literal `TEXT` tag in the executable is passed to the resource request or the size request,
and the numbers passed with them are 98, 99, 1,000 and the number overlay 186 `+05DD` computes.
The two resident occurrences are code that Ghidra's analysis did not decode, not data.

## Alternatives

- This replaces FND-TEXT-004, which took the two resident occurrences for unreferenced data
  because Ghidra had decoded no instruction around them; the instructions of `277B:05D5` hold
  them as push operands. FND-TEXT-004's other observations stand here.
- A request whose tag is not a literal, such as one a script or a table supplies, is outside
  this census.

## How to reproduce

In `tools/research/exec-census/`, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: `resident_listing.py <dsun> 2000:7D40..2000:7E80` (the same bytes as
`277B:05C0..277B:06B0`); `overlay_listing.py <dsun> 186 6FCC0 6FDAD`, `188 72EA0 73260`;
`direct_callers.py <dsun> 2000:7D85 186+05DD`. The outputs are retained under
`GAME_DIR/analysis/text-resource-20261018/`. No original function or game process was run.
