---
id: FND-CONFIG-052
title: Overlays 175 and 176 send conditional item and psionic feedback
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5689:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5691:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:002A
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV windows; FBOV fixup and overlay-map inspection
environment: null
---

## Observation

Four direct calls in overlays 175 and 176 target overlay 172's
`566A:002A` message entry (FND-CONFIG-035):

| Overlay and call file offset | Local condition and text argument |
|---|---|
| 175, `0x00060959` | A preceding branch reaches local UI and state calls after a bit-`0x20` test, then calls `0028:0304` with three words. Only when that call returns zero does it pass `DS:05E3`, a no-effect message. A nonzero return jumps past the message. |
| 175, `0x00060EF3` | A branch adds the selected record's value, or a value derived from it and a second record word, to the double word at `4C13:0357`. It formats `DS:05F3`, a money-received line with the selected record's word at `+6`, calls `00C8:2410`, clears `DS:1A34` to `0xFFFF`, and passes the stack buffer. |
| 176, `0x000617BE` | After a local eligibility helper returns nonzero, the routine computes a threshold and compares the selected actor word at `+2` with it. When that word is lower and `SI < 4`, it passes `DS:0600`, an insufficient-psionic-points message. The branch then skips the later action helper. |
| 176, `0x0006182A` | After the action helper at file offset `0x00061D2E` returns a nonpositive byte, a result of `0xFF` selects `DS:0614`, a disruption message. Other nonpositive results select `DS:0633`, a failed-power-check message, only when `SI < 4`. Both paths share this call instruction. |

Each site passes a far text pointer, including a stack buffer at the
money-received site. The two overlay 176 strings are selected by local
guards; this reading does not establish the full psionic action rule.

## Interpretation

These conditional paths enter the shared message routine, but its later
`WIND/10501` acquisition and setup still decide whether the message-delay
wait runs (FND-CONFIG-018).

## Alternatives

FND-CONFIG-092 traces guarded overlay 178 calls to setup, attempted frame
registration and the separate value-32 handler into the overlay 175 helper.
FND-CONFIG-100 traces overlay 176's two sites to a range- and byte-gated
overlay 193 selector; FND-CONFIG-101 inventories its eleven declared
upstream calls. The complete effects of the item, eligibility and action
helpers and the callers' runtime inputs remain unread. The observed text paths do not prove
an action outcome or a successful message-window setup in a live state.

## How to reproduce

Disassemble the approved `DSUN.EXE` in bounded windows at
`0x00060900..0x00060961`, `0x00060E84..0x00060EFD`,
`0x000616D6..0x000617DD`, and `0x00061814..0x00061832`. Resolve
the four `0x0560` FBOV fixups to overlay 172's `566A:002A` entry.
Read only the short text resources at `DS:05E3`, `DS:05F3`, `DS:0600`,
`DS:0614` and `DS:0633`.
