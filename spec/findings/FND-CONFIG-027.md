---
id: FND-CONFIG-027
title: Literal speech-gate writes occur in launcher, load and Preferences paths
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:0024..277B:023F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5736:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5782:0000
tool: bounded physical-byte search and Capstone 5.0.7 16-bit disassembly with Python 3.14.7; ReportFbovOverlayMap.ps1; Ghidra 12.1.3 ReportReferences.java
environment: null
---

## Observation

A physical search of the approved `DSUN.EXE` for the two-byte displacements
`39 14` (`DS:1439`, saved speech gate) and `E4 14` (`DS:14E4`, runtime voice gate)
finds eight and thirteen candidates, respectively. The direct writes in
bounded instruction contexts are:

| Gate | File offset | Write path |
|---|---|---|
| `DS:1439` | `0x0007D9AC` | Load Game copies byte eight of the `PREF/100` resource (FND-SAVE-005). |
| `DS:14E4` | `0x0001CAD0`, `0x0001CB12`, `0x0001CC47` | Launcher option branches set or clear the runtime gate (FND-SOUND-010). |
| `DS:14E4` | `0x0008B403` | Preferences voice-button branch stores the toggled value (FND-CONFIG-010). |

The `39 14` matches at `0x00072CA3` and `0x0007495E`, and the `E4 14` match
at `0x00082F46`, lie in the fixup payloads of overlays 187, 188 and 195,
respectively, outside those overlays' code ranges. Other candidates include
reads and bytes outside the named direct-write paths. Mapped Ghidra reference
queries recognize reads of both gates and the load write to `DS:1439`, but
do not provide a complete inventory of the launcher and Preferences writes.

## Interpretation

No literal-displacement instruction in this inventory copies either gate to
the other or separately initializes the saved speech gate for a new game.
The loaded image has `DS:1439` at one and `DS:14E4` at zero; the GOG launch
option changes the latter (FND-CONFIG-014).

## Alternatives

The byte search cannot exclude an indirect or computed write, a block copy,
or a new-game path that leaves the loaded-image value in place. Fixup payload
matches are not executable direct references, but may still participate in
overlay relocation; this search does not establish their runtime effect.

## How to reproduce

Search the approved `DSUN.EXE` for physical bytes `39 14` and `E4 14`.
Inspect each candidate's bounded 16-bit instruction context before labeling
it a read or write. Use `ReportFbovOverlayMap.ps1` on the approved source to
compare the three named table-like hits with overlay code and fixup boundaries.
Cross-check the load, launcher and Preferences branches against FND-SAVE-005,
FND-SOUND-010 and FND-CONFIG-010; a mapped `ReportReferences.java` query for
`5000:9239` and `5000:92E4` is supplementary, not an absence proof.
