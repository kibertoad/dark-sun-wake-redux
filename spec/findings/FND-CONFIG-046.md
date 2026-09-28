---
id: FND-CONFIG-046
title: Save Game reuses one message call for copy failure and completion
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
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

Overlay 192's Save Game routine has one direct call to overlay 172's
`566A:002A` message entry, at file offset `0x0007D8E2` (FND-CONFIG-035).
Two local paths reach it with different far text pointers:

| Branch | Text passed to the shared call |
|---|---|
| The `56EF:0084` save-file update and copy wrapper returns zero to the test at `0x0007D852..0x0007D856` (FND-SAVE-008). | `DS:20CF`, the save-failure message; the branch at `0x0007D85C` jumps directly to the shared call. |
| The wrapper returns nonzero. | After the optional `PREF` and `GREQ` resource writes, or after bypassing them when resource-handle selection returns nonzero, the branch at `0x0007D8D9` passes `DS:20E8`, the save-completion message. |

The latter branch does not test the individual remove or write results
(FND-SAVE-004). Both paths remove four bytes of far-pointer argument after
the same call, then return. The message text and the save-file outcome are
separate from the later `WIND/10501` gate inside overlay 172
(FND-CONFIG-018).

## Interpretation

This direct call site can enter the message-delay path for either a reported
save failure or reported completion. A completion message alone does not
prove that the optional resource writes succeeded. This reading does not
claim that the message window is acquired in a live state.

## Alternatives

The resource-handle selection's full return contract, each optional write's
effects, and live message-window acquisition remain open (FND-SAVE-009,
Q-CONFIG-008). This call-site reading does not establish the final contents
of a failed or interrupted save.

## How to reproduce

Disassemble the approved `DSUN.EXE` at physical offsets
`0x0007D84A..0x0007D8ED`, resolving the direct far call's `0x0560` FBOV
fixup to overlay 172's `566A:002A` entry. Follow the two pushes at
`0x0007D859` and `0x0007D8DF` and the branch around the optional writes.
FND-SAVE-004 and FND-SAVE-008 give the surrounding save routine and copy
wrapper.
