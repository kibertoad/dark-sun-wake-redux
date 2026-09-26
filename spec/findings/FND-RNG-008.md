---
id: FND-RNG-008
title: The random table selection and a panel initializer meet in one overlay routine
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2834:000C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 27C0:011F
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1, with the FBOV overlays mapped into the image
environment: null
---

## Observation

Direct calls lead from `2834:0519` through `2834:000C` (FND-RNG-007), the shared input loop at
`28C9:2322` and `27C0:011F`, to code in overlay 182. In that overlay, the routine at
`DSUN.EXE+0x00069E27` (offset `0x15D7` in the overlay's code) calls the code at
`DSUN.EXE+0x00068BB5` (offset `0x0365`), which reaches that chain, and later calls
`DSUN.EXE+0x00069090` (offset `0x0840`), which initializes the status panel that the combat
captures show. In one branch, taken when a state byte is 4, the routine dispatches through a table
of seven indirect targets and may loop over four resident records at the strides 49 and 37 bytes.
Ghidra records no direct reference to the routine. A string that reads as an inventory caption
sits near it and is passed to a helper as an argument.

`27C0:011F` and `277B:056F` are the same linear address, `0x27D1F`.

## Interpretation

The random table selection and the status panel are set up from the same routine, which the game
reaches only through an indirect call.

## Alternatives

Nothing here shows that the selection is a combat roll, that the four records are combatants, or
that the panel is updated by a combat turn. The caption string need not name the routine's screen.

## How to reproduce

Build the mapped image with `tools/ghidra/New-FbovMappedImage.ps1` and import it with the MZ loader
at segment `0x1000`. In that image the overlay routine is at `74BB:0077`, the chain entry at
`7393:0085` and the panel initializer at `7393:0560`;
`tools/ghidra/ReportFbovOverlayMap.ps1 -MappedAddress 74BB:0077` converts each to its file offset.
Follow the direct references from `2834:0519` upward, and decompile `74BB:0077`.
