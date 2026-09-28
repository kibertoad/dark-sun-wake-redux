---
id: FND-CONFIG-057
title: Overlay 189 sends four guarded inventory and item-interaction messages
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5713:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:002A
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV windows; FBOV fixup inspection
environment: null
---

## Observation

Four direct far calls in overlay 189 target overlay 172's `566A:002A`
message entry (FND-CONFIG-035):

| Call file offset | Local condition and passed text |
|---|---|
| `0x000751B1` | A selection branch requires `05F8:0025` to return nonzero, the input word minus `0x2C24` to differ from `0370:0B44`, the selected 0x42-byte record's word at `+0x0E` to be nonzero, and `[BP+18]` to be at least 8. When `05F8:003E` then returns `0xFFFF`, it passes `DS:1AC5`, the backpack-full message; otherwise it calls `05F8:0048` without this message. |
| `0x00076B14` | A branch requires `DS:1A30` to equal `0x270F` and bit `0x20` in the return of `0028:0834` for either `DS:1A32` or the word at `0388:000B`. If the local `DI` is at least `0x1E`, it formats `DS:1AEC` with `DS:1AD7` when `[BP-7]` is nonzero, or `DS:1AD3` otherwise, then passes the buffer. The text says an item cannot be used with an item in a bag or box. |
| `0x000777DB` | After several earlier record and helper guards, `DI` must be below `0x12`, differ from the local comparison value, and equal `0x0B`. The code applies `CDQ`, XOR and subtraction to a selected record word and compares the result with `0x7CD9`. When `[BP+0A]` is nonzero, it passes `DS:1B60`, a short cannot-hear message. |
| `0x000777E7` | The same branch immediately passes `DS:1B71`, another short ear-related message, then continues into a helper call. There is no intervening condition between these two message calls. |

Each call passes a far text pointer and removes four argument bytes.
The two calls at `0x000777DB` and `0x000777E7` are consecutive on their
local path. Other item-placement messages in this overlay use a separate
helper and are outside this direct-call inventory (FND-ITEM-009).

## Interpretation

These four sites add guarded inventory feedback and an item-specific
two-message path to FND-CONFIG-035. Their later entry into the delay wait
still depends on overlay 172's `WIND/10501` acquisition and setup
(FND-CONFIG-018).

## Alternatives

The selected records' complete formats, the helper returns, and the
larger inventory and item rules remain unread here. The call sites do not
establish that a particular item is reached in live play or that the
message window opens.

## How to reproduce

Disassemble the approved `DSUN.EXE` in bounded windows at
`0x000750D5..0x000751CB`, `0x00076A4C..0x00076B1F`, and
`0x00077620..0x00077800`. Resolve the four `0x0560` FBOV fixups at
`0x000751B1`, `0x00076B14`, `0x000777DB` and `0x000777E7` to overlay
172's `566A:002A` entry. Read only the short strings at `DS:1AC5`,
`DS:1AD3`, `DS:1AD7`, `DS:1AEC`, `DS:1B60` and `DS:1B71`.
