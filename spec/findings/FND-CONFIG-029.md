---
id: FND-CONFIG-029
title: Literal music-level request writes occur in Load Game and Preferences entry
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5736:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5782:0000
tool: bounded physical-byte search and Capstone 5.0.7 16-bit disassembly with Python 3.14.7; ReportFbovOverlayMap.ps1
environment: null
---

## Observation

A physical search of the approved `DSUN.EXE` for the two-byte displacement
`B4 26` of the saved music-level request at `DS:26B4` finds 12 candidates.
Two instructions with this displacement write the byte:

| File offset | Write path |
|---|---|
| `0x0007D987` | Load Game copies `PREF/100` offset `0x02` from its local resource buffer (FND-SAVE-005). |
| `0x0008B27E` | Preferences entry stores the sound-library getter's return when the master and music-enable gates are nonzero (FND-CONFIG-012). |

The file-header match at `0x0000168E` is outside loaded code, and the
resident match at `0x0000D485` is the low word of a near-call displacement.
The remaining eight matches are reads in overlays 192 and 203, including
the save copy, sound-library setter arguments and bar drawing. No other
candidate with this literal displacement writes `DS:26B4`.

## Interpretation

This inventory identifies no separate literal write from a music-volume
arrow or a new-game initialization branch. It agrees with the bounded
Preferences dispatcher reading in FND-CONFIG-010 and the getter path in
FND-CONFIG-012.

## Alternatives

An indirect or computed write, block copy, or sound-library side effect
could still change a value used for audible music level. The search does
not show what values the getter returns in ordinary play or whether another
control exists outside the named direct-write paths. It does not establish
new-game defaults.

## How to reproduce

Search the approved `DSUN.EXE` for physical bytes `B4 26`. Compare the 12
hits with the MZ header and the overlay code ranges reported by
`ReportFbovOverlayMap.ps1`. Inspect bounded 16-bit instruction contexts at
the ten loaded-code candidates, distinguishing the two `A2` writes at
`0x0007D987` and `0x0008B27E` from reads and instruction-interior bytes.
Compare the paths with FND-SAVE-005 and FND-CONFIG-012.
