---
id: FND-CONFIG-055
title: Overlay 187 sends guarded save-space and cinematic-copy messages through the shared entry
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56EF:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:002A
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV windows; FBOV fixup inspection
environment: null
---

## Observation

Four direct far calls in overlay 187 target overlay 172's `566A:002A`
message entry (FND-CONFIG-035). Two are in the save-file update and copy
wrapper at `0x00070BFD` (FND-SAVE-008). The other two are in the cinematic
copy size check at `0x00072632` (FND-VIDEO-004).

| Call file offset | Local condition and passed text |
|---|---|
| `0x00070C5A` | After `01F8:04A1` fills three local words, the routine multiplies the first two zero-extended words into a 32-bit `product`, and computes `required = floor(0x144B50 / third word) + 1`. If `2 * required` is above `product` in an unsigned comparison, it passes `DS:1702`, a low disk-space warning. |
| `0x00070C70` | Independently, if `product` is below `required` in an unsigned comparison, it passes `DS:1715`, a stronger disk-space warning. Both calls precede the archive update and file copy; neither branches around them. |
| `0x000726AC` | The helper gets the candidate file length through `01F8:0127`, then calls `01F8:04A1` and multiplies the first two zero-extended local words. Unless a signed 32-bit comparison puts the length below that product, it formats `DS:1902`, a copy-failure message, into `DS:43FB` and passes the buffer. |
| `0x000726D2` | The same failure branch subtracts the product from the length, formats `DS:191C` with that difference into `DS:43FB`, and passes the additional-byte message. The helper then returns zero; the branch that skips both messages returns one. |

At both routines, a `0xFFFF` result from `01F8:04A1` first sends
`DS:16E1` to `05A0:0034`, but the local path then continues to the
comparisons if that call returns. Each listed message call passes one far
text pointer and removes four argument bytes. The two conditions in the
save wrapper are separate, so the same invocation can reach both calls.

## Interpretation

These four sites complete the bounded local conditions for overlay 187's
direct calls in FND-CONFIG-035. FND-CONFIG-076 traces their direct incoming
routes through Save Game, region staging and cinematic playback.
Entry into the later message-delay wait
still depends on overlay 172's `WIND/10501` acquisition and setup gate
(FND-CONFIG-018).

## Alternatives

The complete behavior of `01F8:04A1`, its error path, and actual drive
capacity are not established by these call windows. The arithmetic and
message calls alone do not establish that either operation succeeds, that
the message window opens, or that the wait executes in a live state.

## How to reproduce

Disassemble the approved `DSUN.EXE` in bounded windows at
`0x00070BFD..0x00070CED` and `0x00072632..0x000726E2`. Resolve the
`0x0560` FBOV fixups at `0x00070C5A`, `0x00070C70`, `0x000726AC` and
`0x000726D2` to overlay 172's `566A:002A` entry. Read only the short
strings at `DS:1702`, `DS:1715`, `DS:1902` and `DS:191C`.
