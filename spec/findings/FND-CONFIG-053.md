---
id: FND-CONFIG-053
title: Overlay 179 sends guarded combat and item feedback through shared message calls
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56A7:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:002A
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV windows; FBOV fixup inspection
environment: null
---

## Observation

Four direct far calls in overlay 179 target overlay 172's `566A:002A`
message entry (FND-CONFIG-035):

| Call file offset | Local path and passed text |
|---|---|
| `0x00065A7D` | The far record at `[BP+0A]` has bit `0x04` or `0x20` in its byte at `+8`, and a subsequent `0608:0020` call with `0x18` and `[BP+8]` returns zero. The path passes `DS:0916`, a message that a flayer begins consuming a brain, before additional local calls. |
| `0x00065BFE` | The routine formats `DS:0934` with a selected far string and another word into `DS:43FB`. A length call selects that buffer when its result is at most `0x1D`; otherwise it selects the shorter `DS:0946`. Both branches reach the same call. |
| `0x00066882` | A preceding local helper returns nonzero, and the result of `00B0:391D` compares at least as high as a threshold computed from a byte at `[BP-0A]+0x1B`. The branch passes `DS:09A4`, a permanent-death message, then writes `8` to the far record byte at `[BP-0A]+0x14`. |
| `0x000669D1` | This shared sink receives `DS:098D` from a weapon-in-hand failure branch (FND-ITEM-009), `DS:09C5` from another branch, or `DS:09D2` when a local helper returned zero. The latter two texts report that raising or reincarnation cannot proceed. Other incoming branches were not exhaustively traced here. |

The `0x00065BFE` and `0x000669D1` instructions each have multiple incoming
text paths. Every listed call passes a far pointer to the shared message
routine and removes four argument bytes at the site or a continuation.

## Interpretation

These paths add conditional combat and item feedback to the direct-call
inventory. Their later entry into the message-delay wait still depends on
the `WIND/10501` acquisition and setup gate inside overlay 172
(FND-CONFIG-018).

## Alternatives

The local helpers' complete effects, all paths into the shared sinks and
the broader combat rules remain unread. The call sites do not establish
that a particular outcome occurs in live play or that the message window
successfully opens.

## How to reproduce

Disassemble the approved `DSUN.EXE` in bounded windows at
`0x00065A51..0x00065A9B`, `0x00065BCC..0x00065C0E`,
`0x000667AD..0x00066895`, and `0x000669C6..0x000669DB`.
Resolve the four `0x0560` FBOV fixups to overlay 172's `566A:002A`
entry. Read only the short strings at the data-segment offsets named
above.
