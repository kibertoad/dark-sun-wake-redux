---
id: FND-CONFIG-121
title: Overlay 195 routes guarded combat feedback through the shared message entry
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 574E:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:002A
tool: Python 3.14.7 and Capstone 5.0.7 bounded disassembly; verified MZ relocation and FBOV fixup segment mapping
environment: null
---

## Observation

This entry corrects the raw segment labels in FND-CONFIG-047.
Overlay 195 fixup at 0x00081A20 decodes descriptor 110 to mapped segment 4E71.
The bounded control-flow description is retained with mapped addresses.

Four direct calls in overlay 195 target overlay 172's `566A:002A` message
entry (FND-CONFIG-035):

| Call file offset | Local path and passed text |
|---|---|
| `0x0008194A` | The branch requires bit `0x40` in `[BP-17]` and `SI < 4`. It formats `DS:2128`, an escape-attempt line, in `DS:43FB`. When the local helper at file offset `0x00082C9F` returns nonzero, it also formats `DS:2143`, an escape-result line, into that buffer. The call occurs only if the final buffer-length call returns at most `0x1D`. |
| `0x00081AC8` | A later branch requires bit `0x10` in `[BP-0E]`, the decremented selected-record byte at `+0x1B` to equal zero, a selected word at `4E71:[index*10+0251]` equal to `0x0122`, and `SI < 4`. It similarly formats escape-attempt and optional escape-result text in `DS:43FB`, and calls the message entry only when the final length is at most `0x1D`. |
| `0x00081C54` | A branch requires the selected record byte at `+0x12` to equal `0x10`, bit `0x01` in `[BP-17]`, and the selected actor byte at `+0x14` to be at least `3`. It passes `DS:2150`, a brain-consumption message, before later local state and helper calls. |
| `0x00082509` | A dispatch target first tests `SI < 4`, formats `DS:216B`, a charm-state line with the selected actor's name, into a stack buffer, and passes that buffer. The call is skipped when `SI >= 4`, although later helper calls still run. |

The first two sites share a data-segment buffer, and the helper return
selects whether another line is formatted before the final length check. All four sites pass a far
text pointer; none supplies a delay value to the shared routine.

## Interpretation

These sites add conditional combat feedback to the direct-call inventory.
Their local branches can enter overlay 172's message routine, but its
`WIND/10501` acquisition and setup still determine whether the
message-delay wait runs (FND-CONFIG-018).

## Alternatives

The broader combat state, the escape helper's complete behavior, and the
other helpers' effects were not established by these bounded windows. The
strings and calls do not prove a particular combat outcome or successful
message-window acquisition in a live state.

## How to reproduce

Disassemble the approved `DSUN.EXE` at physical offsets
`0x00081841..0x00081952`, `0x000819BD..0x00081AD0`,
`0x00081BDF..0x00081C85`, and `0x00082469..0x00082511`, using windows
no larger than 512 bytes. Resolve the four direct calls' `0x0560` FBOV
fixups to overlay 172's `566A:002A` entry. Read the short data-segment
strings at `DS:2128`, `DS:213D`, `DS:2143`, `DS:2150` and `DS:216B`.

For the segment-label correction, select each named operand from the
MZ relocation list or its overlay's declared FBOV fixup list. Apply the
recorded load segment to MZ operands; decode an overlay operand's shifted
descriptor index and resolve its segment-table entry before labelling an
address. Raw segment operands and mapped addresses are different forms.
