---
id: FND-CONFIG-049
title: Overlay 204's rest entry sends combat refusal or party-rest feedback
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5787:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:002A
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV windows; FBOV fixup inspection
environment: null
---

## Observation

The routine beginning at physical file offset `0x0008CA7A` in overlay 204
has two direct calls to overlay 172's `566A:002A` message entry
(FND-CONFIG-035):

| Call file offset | Local condition and text argument |
|---|---|
| `0x0008CAA2` | The word at `4C10:0019` is nonzero. The routine passes `DS:28DA`, a refusal to rest during combat, then jumps to its exit path without taking the second message branch. FND-COMBAT-023 identifies this combat-state gate. |
| `0x0008CABB` | The word at `4C10:0019` is zero and the subsequent call to `00B0:2522` returns exactly one. The routine passes `DS:28F3`, a party-rest announcement, then continues through other helper calls. If that helper returns another value, this message is skipped but the helper sequence still runs. |

Both sites pass a far text pointer and remove four argument bytes. The second
call occurs before the routine's longer rest-processing path; this bounded
reading does not establish that path's complete effects or a completed rest.

## Interpretation

The combat refusal and the conditional rest announcement are separate
sources for the shared message routine. FND-CONFIG-077 traces the two
direct caller routes to this rest entry. Its later `WIND/10501` acquisition
and setup still determine whether either call enters the message-delay wait
(FND-CONFIG-018).

## Alternatives

The return contract of `00B0:2522`, the rest routine's remaining branches,
and live window acquisition are unresolved here. A passed announcement
string does not by itself establish that time advanced or resources were
restored.

## How to reproduce

Disassemble the approved `DSUN.EXE` at physical offsets
`0x0008CA7A..0x0008CAD9`. Resolve calls at `0x0008CAA2` and
`0x0008CABB` through their `0x0560` FBOV fixups to overlay 172's
`566A:002A` entry. Read only the short strings at `DS:28DA` and
`DS:28F3`.
