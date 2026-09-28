---
id: FND-CONFIG-048
title: Overlay 197 message calls report a guarded dissipation and two failures
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 575A:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:002A
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV windows; FBOV fixup inspection
environment: null
---

## Observation

Three direct far calls in overlay 197 target overlay 172's `566A:002A`
message entry (FND-CONFIG-035):

| Call file offset | Local condition and text argument |
|---|---|
| `0x0008601E` | The path requires `SI == 4` and byte `0388:[SI*3+0C33] == 2`. It calls `00F0:05BB` with four arguments, then passes `DS:2514`, a monster-dissipation line. If either guard fails, this call is skipped. |
| `0x00086734` | The entry tests byte `0388:0C3F`. When it is nonzero, it passes `DS:252D`, a summoning failure line, and jumps past the local dispatch table. The zero branch instead compares its input against fifteen table words. |
| `0x00086F56` | The entry first calls `05E0:0070` with five words and requires its return to be zero. It then requests tag `OJFF` with the second word argument as resource number. Only if that request returns nonzero does it pass `DS:254D`, a missing-object-resource diagnostic; it then returns zero. When the first helper returns nonzero, this message is skipped and the entry returns one. |

Each site passes a far text pointer. The call at `0x0008601E` reaches a shared
continuation that cleans up the four argument bytes; the other two clean up
immediately. FND-ACTOR-006 identifies the `OJFF` tag occurrence at
`0x00086F42`.

## Interpretation

These local branches add conditional summoning and object-resource feedback
to the shared message-call inventory. The call sites do not decide whether
overlay 172 subsequently acquires `WIND/10501` and enters its
message-delay wait (FND-CONFIG-018).

## Alternatives

The meaning of `0388:0C3F`, the complete effects of the preceding helpers,
and the caller inputs are not established by these windows. The resource
request's nonzero result is not, by itself, proof of an observed live error
or a successful message-window setup.

## How to reproduce

Disassemble the approved `DSUN.EXE` at physical offsets
`0x00085FEC..0x0008604C`, `0x00086715..0x00086762`, and
`0x00086F09..0x00086F67`. Resolve the three `0x0560` FBOV fixups to
overlay 172's `566A:002A` entry. Read only the short strings at `DS:2514`,
`DS:252D` and `DS:254D`.
