---
id: FND-CONFIG-044
title: Overlay 190 routes diagnostic and animation feedback through the shared message entry
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

Overlay 190's dispatch routine compares its input word against 33 words in a
code-segment table and jumps through a parallel target table. Several targets
reach the shared overlay 172 message entry (FND-CONFIG-035):

| Call file offset | Local path and passed text |
|---|---|
| `0x00079827` | A branch requires `DS:143C` to be nonzero and formats `DS:1E42` with a region number and position values into a stack buffer. |
| `0x00079864` | The same branch formats `DS:1E55` with four `DS:14D7..14DD` words divided by 16. |
| `0x00079894` | The same branch formats `DS:1E6A` with a computed value. If `DS:143C` is zero, its earlier branch jumps here with the title string at `DS:1E77` instead. Other dispatch targets also jump here with their own strings. |
| `0x000798AE` | Another dispatch target passes the far pointer returned by `0090:05D5`, after a call to `00B8:0D30`. |
| `0x000798D3` | That target next formats `DS:1E97` with the result of a second `00B8:0D30` call and passes the stack buffer. |
| `0x00079C00` | If `DS:1438` is zero, this target toggles `DS:1437` and passes the corresponding animation-state string at `DS:1F23` or `DS:1F31`; if `DS:1438` is nonzero, it jumps to the shared `0x00079894` call with a different string. |
| `0x0007A1DB` | A target calls `01E8:0011`; only when its return has bit 0 or 1 set and `DS:143C` is nonzero does it pass `DS:1F66`, a subregion-off message. It then writes four words at `DS:14D7..14DD`. Otherwise it calls `0648:0020` without this message. |

These sites pass a far text pointer and clean up four argument bytes either
immediately or at a shared continuation. The conditions above are local to
these dispatch targets; the table's input producer and every path reaching
the shared `0x00079894` sink were not completely read here.

## Interpretation

The syntactic calls in FND-CONFIG-035 include debug-gated diagnostic text,
animation feedback, a conditional subregion message and a pair of values
returned by other routines. A call still has to pass overlay 172's later
`WIND/10501` acquisition and setup before it can enter the message-delay wait
(FND-CONFIG-018).

## Alternatives

The far-pointer helper's returned text, the meaning of its preceding call,
the dispatch input's origin, and the effects of the subregion helper remain
unresolved. The call sites do not prove that any particular target is reached
in a live state or that the window acquisition succeeds.

## How to reproduce

Disassemble the approved `DSUN.EXE` at physical offsets
`0x000796DB..0x00079710`, `0x000797B0..0x000798E3`,
`0x00079BD4..0x00079C31`, and `0x0007A1C6..0x0007A202`. Resolve the seven
far calls through their `0x0560` FBOV fixups to overlay 172's `566A:002A`
entry. Read only the short format and state strings at the data-segment
offsets listed above.
