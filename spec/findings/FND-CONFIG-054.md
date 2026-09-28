---
id: FND-CONFIG-054
title: Overlay 182 sends secret-door and blocked-door messages through one path
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:002A
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV windows; FBOV fixup inspection
environment: null
---

## Observation

The routine around physical file offset `0x000694A9` in overlay 182 has
two direct calls to overlay 172's `566A:002A` message entry
(FND-CONFIG-035). Its local path calls `0028:02E7` and a helper at
`0x00069450`, compares a selected record's flag word at `+0x10`, and
returns without either message under several earlier guards.

| Call file offset | Local condition and text argument |
|---|---|
| `0x000695DD` | The local helper at `0x00069450` returns nonzero, bit `0x01` of the record's `+0x10` word matches the branch's local byte, `DS:655C` is zero, and bit `0x08` of that word is set. It passes `DS:0FFC`, a secret-door discovery message, then clears bit `0x08` and exits. |
| `0x0006962A` | The same preceding gates pass and bit `0x08` is clear. Bit `0x02` of the record word selects `DS:1013`, a locked-door message; otherwise bit `0x01` selects `DS:1026`, a door-that-will-not-open message. If neither bit is set, the call is skipped. |

Both sites pass one far text pointer and remove four argument bytes. The
second call is a shared sink for the two blocked-door strings. This is a
bounded reading of message selection, not a full door-interaction rule.

## Interpretation

The two guarded paths enter the shared message routine. Its later
`WIND/10501` acquisition and setup still control whether the
message-delay wait runs (FND-CONFIG-018).

## Alternatives

The selected record's complete format, the helper's caller inputs, the
other interaction branches and live resource acquisition remain open.
Clearing the local bit after the first message call does not prove that
the message window opened or was seen.

## How to reproduce

Disassemble the approved `DSUN.EXE` at physical offsets
`0x000694A9..0x00069582` and `0x00069582..0x00069636`. Resolve the
calls at `0x000695DD` and `0x0006962A` through their `0x0560` FBOV
fixups to overlay 172's `566A:002A` entry. Read only the short strings
at `DS:0FFC`, `DS:1013` and `DS:1026`.
