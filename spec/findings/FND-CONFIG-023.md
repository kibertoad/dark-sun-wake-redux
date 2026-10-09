---
id: FND-CONFIG-023
title: SOUND.CFG tail words select ADV resource requests and conditional paths
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 47F0:000A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 46BC:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 45F9:0002
tool: Ghidra 12.1.3 ReportReferences.java mapped-image query and Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded resident-image ranges
environment: null
---

## Observation

The sound library keeps the loaded `SOUND.CFG` buffer pointer at `DS:3411`
(FND-CONFIG-020). Bounded reads after that pointer load show four uses of
the tail beyond the `0x32` word already described:

| Field | File offset | Direct use |
|---|---|---|
| `0x38` | `0x0003D10E` | When a local selector is zero, widens this word to 32 bits. |
| `0x36` | `0x0003D11F` | When that selector is one, widens this word instead. |
| `0x34` | `0x0003BDC0`, `0x0003C011` | Compares the word with a resident runtime word using a signed greater-than branch; the branch skips two local calls. |
| `0x3A` | `0x0003B196`, `0x0003B1C7`, `0x0003BF16`, `0x0003BF47`, `0x0003D33E` | Tests the byte for 1 or 0 before distinct later branches. |

The selected `0x38` or `0x36` word is passed as the number with the four-byte
tag `ADV ` (`0x20564441`) and a local output address to `38FF:05B5` at
`DSUN.EXE+0x0003D132`. Prior bounded readings identify that entry as a
resource-size query (FND-SCRIPT-019, FND-SOUND-007). The installed values
are 11 and 8 respectively (FND-CONFIG-003), and setup copies them from
separate input-record fields (FND-CONFIG-214).

## Interpretation

The two tail words at `0x38` and `0x36` act as alternative `ADV ` resource
numbers for selector values zero and one. The word at `0x34` and byte at
`0x3A` affect control flow; their player-visible or device effects are not
established by these windows.

## Alternatives

FND-CONFIG-024 shows that the selector pairs each `ADV ` number with one
ten-byte settings block. Whether the resource request succeeds and what
the branches for `0x34` and `0x3A` do remain unread. The
installed values match music and digital chunk numbers in `SOUND.INI`
(FND-CONFIG-003), but that match alone does not assign those roles to the
selector values. Ghidra's recognized references and a raw search for nearby
pointer loads are not an exhaustive consumer inventory.

## How to reproduce

In the mapped image of the approved `DSUN.EXE`, query references to the
buffer pointer at `5000:B211` (`DS:3411`). In physical 16-bit code, inspect
only `0x0003D106..0x0003D154`, `0x0003BDA7..0x0003BDDC`,
`0x0003BFF3..0x0003C02D`, `0x0003B192..0x0003B1CE`,
`0x0003BF12..0x0003BF4E` and `0x0003D32F..0x0003D369`.
Follow each loaded pointer to its immediate field access and branch.
