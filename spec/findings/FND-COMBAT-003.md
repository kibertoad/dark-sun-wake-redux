---
id: FND-COMBAT-003
title: The strings COMBAT and GUARD occur only inside messages and labels of the data segment, and ATTACK does not occur as a string
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:1D38..57E0:1D56
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:1052..57E0:1058
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:66EB
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1; byte search with Python 3.14.7
environment: null
---

## Observation

`COMBAT` occurs four times in `DSUN.EXE`, at file offsets `0x4ED4F`, `0x4EDB3`, `0x4EED1` and
`0x4F8EC`, all in the resident data segment `57E0` (`57E0:1D4F`, `57E0:1DB3`, `57E0:1ED1` and
`57E0:28EC`). Each is the end of a longer NUL-terminated message: `CAN'T CHANGE LEADER IN COMBAT`
at `57E0:1D38`, `CAN'T ADD CHARS IN COMBAT` at `57E0:1DA0`, `CAN'T SAVE DURING COMBAT` at
`57E0:1EBF` and `NO RESTING DURING COMBAT` at `57E0:28DA`. Ghidra records no reference to any of
the four `COMBAT` addresses.

`GUARD` occurs twice: as the whole string at `57E0:1052`, between `END MOVE` and `WAIT`, and inside
`%Fs GUARDS` at `57E0:1EAA`. Ghidra records one read of `57E0:1052`, from `1BF3:66EB` in the
function it starts at `28C9:19A1`; the eight instructions on each side compare `AL` with `0x20`,
load a far pointer, change a word through it, and loop.

`ATTACK` followed by a NUL does not occur in the file.

## Interpretation

The `COMBAT` matches are messages the game shows when it refuses something during combat. A
reference, if any, points at the start of each message, which is where FND-COMBAT-024 finds them
pushed. The context of the one `GUARD` read is string handling, not command handling.

## Alternatives

The legacy record gave these addresses in the overlay-mapped copy (`5000:9B4F` and so on) and
read the missing references as showing that the strings identify no combat code; the references
were looked for at the wrong addresses. Ghidra's read from `1BF3:66EB` does not match the one push
of `57E0:1052` that a byte search finds, in overlay 182 (FND-COMBAT-024); which is right has not
been checked.

## How to reproduce

Search the file for the ASCII bytes of `COMBAT`, `GUARD` and `ATTACK` with a NUL, convert each
file offset `f` to `57E0:(f - 0x5200 + 0x10000 - 0x57E00)`, and list the NUL-terminated string
that contains it. In the overlay-mapped copy described in `docs/GHIDRA.md`, "FBOV mapped image",
run `ReportBytePattern` and `ReportInstructionContext` on the matches (mapped `5000:x + 0x7E00` is
`57E0:x`).
