---
id: FND-CONFIG-073
title: Overlay 172 calls its own shared message entry from two other routines
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:002A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:0034
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:0043
tool: Python 3.14.7 bounded near-call target search; Capstone 5.0.7 16-bit disassembly of caller windows; FBOV trampoline inspection
environment: null
---

## Observation

Overlay 172's `566A:002A` entry targets code offset `0x05DB` at physical
file offset `0x00059A0B` (FND-CONFIG-011). Five `E8` near-call instructions
in the same overlay target that offset. Each is immediately preceded by
`push cs`, supplying the segment word required by the callee's far return,
and follows a pushed far text pointer:

| Call file offset | Containing exported entry | Text pointer and local route |
|---|---|---|
| `0x0005A047` | `566A:0034` | `DS:04AD` reaches the shared call when an indexed record's byte at `+0x14` is not one; another path passes `DS:04C7` after the indexed word at `DS:445E` is zero. The two pointers concern casting availability and an empty spell queue. |
| `0x0005A10A` | `566A:0043` | `DS:04DA` when a preceding far call returns zero; text concerns psionic start failure. |
| `0x0005A131` | `566A:0043` | `DS:04F3` when local byte `[BP-2]` is not two and a preceding far call returns zero; text concerns rest. |
| `0x0005A15D` | `566A:0043` | `DS:0504` when the word at `02E0:0019` and an indexed byte at `02E0:[BX+0xB2]` are nonzero; text concerns an interruption. |
| `0x0005A187` | `566A:0043` | `DS:0511` when `[BP-2]` is not two and a preceding far call returns zero; text concerns inability to cast. |

The `566A:0034` entry is itself a direct resident call target at
`0x0001DF65` (FND-CONFIG-072). Within `0034`, the address of entry
`566A:0043` is pushed at `0x00059D37` and `0x00059D99` as an argument to
other far calls. FND-CONFIG-074 traces the latter call's installation of
that pointer on seven application frames and the frame dispatch path.
The other resident call target
`566A:002F` spans `0x00059B76..0x00059C45` and contains none of these
five near calls.

## Interpretation

The shared message routine has five additional syntactic call sites
inside its own overlay, beyond the 56 inter-overlay calls of
FND-CONFIG-035 and four resident calls of FND-CONFIG-017. The `0034`
entry has a direct resident route; the `0043` routine is supplied as a
far pointer by `0034`. Each call still faces the later `WIND/10501`
acquisition gate (FND-CONFIG-018).

## Alternatives

The complete state and event sequence into entry `0043`, and the results
of its preceding far calls, remain unread. The text-pointer and local-branch
reading does not prove that any of these messages appears or waits in a
particular live state. A computed call elsewhere may add callers.

## How to reproduce

Map overlay 172 from its header at `0x0004B8A0` and code start
`0x00059430` using FMT-EXE-003 and FMT-EXE-004. Its `002A`, `0034` and
`0043` trampolines target code offsets `0x05DB`, `0x0877` and `0x0C21`.
Search its declared 4,814-byte code range for `E8` instructions whose
signed relative displacement resolves to `0x05DB`, then check instruction
alignment and arguments in bounded windows around the five physical
offsets above. Read `0x00059CA7..0x00059D18`,
`0x00059D20..0x00059DA9`, `0x0005A019..0x0005A051`, and
`0x0005A0F0..0x0005A190` for the branch and pointer paths. The resident
call to `0034` is in FND-CONFIG-072.
