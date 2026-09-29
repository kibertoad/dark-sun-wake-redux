---
id: FND-CONFIG-201
title: A fixed text wrapper forwards two coordinates and sixteen extra argument bytes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:09F3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 401B:0172
tool: Python 3.14.7 and Capstone 5.0.7 complete bounded resident 16-bit wrapper reading with operand widths and MZ relocation mapping
environment: null
---

## Observation

A declared MZ relocated-call query for 401B:0172 identifies the
resident site at file 0x00022208. Its complete containing wrapper
2C5F:09F3 spans file 0x000221E3..0x00022211 inclusive.
It saves BP, sets its frame, pushes 28 argument bytes, calls
401B:0172, removes exactly those 28 bytes, restores BP and
far-returns. There is no local branch, stack-limit guard, supplied
pointer check or post-call AX normalization. The call's segment
operand is a declared MZ relocation to relative segment 301B.

The operand widths and reversed push order give this argument map.
BP below names this wrapper's frame before its call; DS names the
segment read by its push instruction, not an inferred fixed segment.

| Argument to 401B:0172 | Producer |
|---|---|
| First far pointer, child BP+06 | Wrapper double word BP+06 |
| First coordinate word, child BP+0A | Wrapper word BP+0E |
| Second coordinate word, child BP+0C | Wrapper word BP+10 |
| Far format pointer, child BP+0E | Current DS with offset 0F6A |
| Extra word, child BP+12 | 0 |
| Extra word, child BP+14 | 00FF |
| Extra word, child BP+16 | 00FE |
| Extra word, child BP+18 | Wrapper word BP+12 |
| Extra word, child BP+1A | 20 decimal |
| Extra word, child BP+1C | Wrapper word BP+14 |
| Extra double word, child BP+1E | Wrapper double word BP+0A |

The immediate 00FE00FF is a four-byte push, not one word.
Its two words occupy separate consecutive extra-argument positions.
The format pointer is supplied by two word pushes. The first and
last double-word inputs are forwarded verbatim without dereference
in this wrapper. The sixteen extra bytes follow the fixed twelve
bytes consumed before the text routine's extra-argument cursor.
Their use depends on the format bytes and dispatch, not solely on
the presence of these constants.

FND-CONFIG-200 reads the callee's null-pointer FFFF return and
its zero result after a returning text/rectangle continuation. This
wrapper forwards that AX unchanged. It neither proves which branch
runs nor supplies the rectangle helper's unassigned second frame word
from FND-CONFIG-199. Incoming wrapper arguments and actual DS:0F6A
contents remain separate provenance questions.

## Interpretation

The text routine has a concrete wrapper supplying a fixed DS-relative
format address and a bounded argument shape. Record, coordinate,
extra-word and far-pointer inputs still come from this wrapper's
caller. The stack layout can be described without inventing the
format pointer's semantic identity, actual text, accepted handles or
rendering outcome.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain incoming wrapper callers,
DS and format-pointer provenance, accepted record and coordinate
inputs, extra-argument interpretation and external text/graphics
outcomes. One reading supplies a valid zero-terminated format and
accepted record; another reaches a null-record return or invalid
format/state. Complete callers and format/callee evidence would
separate code-decided parts; native VGA output still needs owner
evidence. No native or emulated result is claimed.

The reading that the four-byte immediate supplies one extra word is
ruled out by its operand width and the 28-byte cleanup. A reading
that this wrapper independently validates its inputs or normalizes
returning completion to zero is ruled out by its complete body.

## How to reproduce

Read 2C5F:09F3 through far return 0A21 from the entry, not a
mid-instruction window. Check each push width, map the 28 bytes
through the callee's far-return/BP frame, and verify the declared
segment relocation at file 0x0002220B. Compare FND-CONFIG-200
for fixed argument widths and the extra-argument cursor. Keep the
supplied DS-relative address distinct from format contents until its
segment and upstream state are established.
