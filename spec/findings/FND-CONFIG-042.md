---
id: FND-CONFIG-042
title: Save-capacity and Save Game branches call the shared message routine
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56B2:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5736:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:002A
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV windows; FBOV fixup inspection
environment: null
---

## Observation

Overlay 180's save-capacity helper counts numbered save files and
calculates how many more the reported drive space can hold
(FND-SAVE-006). At `DSUN.EXE+0x00067C77` it compares the resulting
double word with ten. A value at least ten skips its message call.
Otherwise it formats the message at `DS:0D6B` with that value into a
stack buffer and passes the buffer to overlay 172's `566A:002A` entry
at `DSUN.EXE+0x00067C96`.

Overlay 192's Save Game routine (FND-SAVE-004) has a single direct call
to that same entry at `DSUN.EXE+0x0007D8E2`. If its numbered-file copy
returns zero, the branch at `0x0007D852..0x0007D85C` pushes the failure
text at `DS:20CF` and jumps to that call. If the copy succeeds, it
selects `CHARSAVE.GFF` and conditionally attempts `PREF` and `GREQ`
writes (FND-SAVE-009), then pushes `DS:20E8`, `GAME SAVED`, and falls
through to the same call. The visible code does not check the results of
the individual `PREF` and `GREQ` writes before selecting that message.

Both call instructions are among the direct overlay callers inventoried
by FND-CONFIG-035. They pass a far text pointer and remove four argument
bytes afterward. The called routine may then acquire and set up
`WIND/10501` before its message-delay wait (FND-CONFIG-018).

## Interpretation

The save-capacity call is conditional on a result below ten; the Save
Game call is shared by its copy-failure and copy-success branches.
Entering the shared message routine is established for those branches,
while reaching its later wait still depends on the window acquisition
and setup result.

## Alternatives

FND-CONFIG-071 identifies a direct startup caller of the helper, while
indirect callers and the actual calculated capacity in a particular
game state remain unread. A message call does not prove a visible
window or elapsed wait when acquisition, allocation, registration or I/O
fails. The Save Game success text does not by itself prove that each
settings resource write succeeded.

## How to reproduce

Disassemble the approved `DSUN.EXE` at physical offsets
`0x00067BDA..0x00067CA2` and `0x0007D840..0x0007D8ED`. Follow the
save-capacity comparison and the two Save Game branches to their far
calls at `0x00067C96` and `0x0007D8E2`. Resolve each `0x0560` FBOV
segment word to overlay 172's `566A:002A` entry, then compare its
window-acquisition gate with FND-CONFIG-018.
