---
id: FND-CONFIG-058
title: Overlay 190 sends guarded combat-state and party-action messages through the shared entry
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 571F:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:002A
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV windows; FBOV fixup inspection
environment: null
---

## Observation

Four direct far calls in overlay 190 target overlay 172's `566A:002A`
message entry (FND-CONFIG-035). Two lie in the selector routine in the
code block beginning at `0x00078340`; two lie in the 33-target dispatch routine that
starts at `0x000796DB` (FND-CONFIG-044).

| Call file offset | Local condition and passed text |
|---|---|
| `0x00078C48` | If the word at `02E0:0019` is nonzero and the input word minus `0x2C2D` differs from `DS:426D`, it passes `DS:1D38`, a message that the leader cannot change during combat. The path then jumps to a shared four-byte argument cleanup and exit. |
| `0x00079194` | After two preceding `05B0` calls, the selected 0x42-byte record's word at `+0x0E` must be zero and `02E0:0019` nonzero. The branch passes `DS:1DA0`, a message that characters cannot be added during combat. If `02E0:0019` is zero, it instead follows another routine without this message. |
| `0x00079A4B` | A dispatch target requires `02E0:0019` nonzero and the signed word at `DS:426D` below 4. It formats `DS:1EAA` with a far pointer to the selected 0x31-byte record's field at `+0x21` and passes the buffer, a guard-action message. |
| `0x00079AE7` | Another dispatch target has the same two local gates, formats `DS:1EB5` with the selected record's field at `+0x21`, and passes the buffer, a wait-action message. It then calls `0568:0093` with 2 and `DS:426D`. |

Each site passes one far text pointer and removes four argument bytes at
the call or a shared continuation. FND-COMBAT-024 independently locates
the short strings. Together with FND-CONFIG-044, these four paths account
for overlay 190's eleven direct sites in FND-CONFIG-035.

## Interpretation

These paths complete a bounded local reading for the inventoried direct
overlay calls to the shared message entry. Each call still has to pass
overlay 172's `WIND/10501` acquisition and setup before it can enter the
message-delay wait (FND-CONFIG-018).

## Alternatives

The selector and dispatch input producers, complete meanings of the
selected records, and outcomes of the called helpers remain unread here.
The local call conditions do not establish that the messages appear in a
live state or that the later wait executes.

## How to reproduce

Disassemble the approved `DSUN.EXE` in bounded windows at
`0x00078C2B..0x00078C50`, `0x00079145..0x000791C8`,
`0x00079A07..0x00079A58` and `0x00079AA3..0x00079AFA`.
Resolve the `0x0560` FBOV fixups at `0x00078C48`, `0x00079194`,
`0x00079A4B` and `0x00079AE7` to overlay 172's `566A:002A` entry.
Read only the short strings at `DS:1D38`, `DS:1DA0`, `DS:1EAA` and
`DS:1EB5`.
