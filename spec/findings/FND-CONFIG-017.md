---
id: FND-CONFIG-017
title: The message-delay routine has resident text-message callers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2B00:000A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2B10:006E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 297F:015E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2910:0083
tool: Ghidra 12.1.3 ReportReferences.java and ReportInstructionContext.java, corroborated by Capstone 5.0.7 16-bit disassembly with Python 3.14.7
environment: null
---

## Observation

Overlay 172's resident trampoline `566A:002A` leads to the routine at
`DSUN.EXE+0x00059A0B` that conditionally waits using `DS:26B7`
(FND-CONFIG-011). A mapped-image reference query found four direct resident
calls. Physical disassembly corroborates the call instructions and arguments:

| Call file offset | Argument supplied before the call | Local condition |
|---|---|---|
| `0x0002020A` | `DS:0E29` | preceding far call returns zero |
| `0x0002036E` | `DS:0E29` | preceding far call returns zero |
| `0x0001EB4E` | one of `DS:0DE5`, `DS:0DD4`, `DS:0DF2`, `DS:0DFE`, `DS:0E0D` | converging branches choose the argument |
| `0x0001E383` | `DS:0DB5` | byte at the indexed record field equals `0xFF` |

Each call removes four argument bytes afterward. The loaded-image text at
`DS:0E29` describes a link problem; the text at `DS:0DB5` describes a path
failure. Earlier bounded readings also identify calls through the same
trampoline for combat messages (FND-COMBAT-023, FND-COMBAT-025). A video
finding infers another message use (FND-VIDEO-004).

## Interpretation

The wait routine is reachable from several text-message paths outside
Preferences. The message-delay value is therefore connected to a message
display entry, rather than only to the Preferences bar.

## Alternatives

These call sites do not establish which calls reach the later nonzero
`0300:0007` test and hence actually wait. The pointer's role and the meaning
of the five messages converging at `0x0001EB4E` remain unread. Ghidra's four
recognized resident references are not an exhaustive caller inventory:
previous overlay readings identify additional calls, and indirect calls may
also exist. No player-visible event timing is established by this finding.

## How to reproduce

Use `tools/ghidra/ReportFbovOverlayMap.ps1` on the approved `DSUN.EXE` to
place overlay 172's trampoline and convert the four resident mapped addresses
above to file offsets. Query `566A:002A` with `ReportReferences.java` in the
mapped image. Inspect the four call sites with `ReportInstructionContext.java`
and disassemble bounded physical windows around file offsets
`0x0001E330..0x0001E390`, `0x0001EB30..0x0001EB58`,
`0x000201F0..0x00020215`, and `0x0002035F..0x00020379` in 16-bit mode.
