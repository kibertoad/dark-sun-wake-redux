---
id: FND-CONFIG-045
title: Overlay 191 item feedback calls the shared message entry on bounded branches
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 572F:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:002A
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV windows; FBOV fixup inspection
environment: null
---

## Observation

Four direct calls in overlay 191 target overlay 172's `566A:002A` message
entry (FND-CONFIG-035). FND-ITEM-009 identifies their text resources; the
bounded call contexts add these conditions:

| Call file offset | Passed text and local condition |
|---|---|
| `0x0007C7C0` | A stack buffer formats `DS:2018` with a selected record's word at `+6`. The surrounding branch has checked the record's transformed first word against `0x0384..0x038E`, added a value derived from the selected record to `02E8:0357`, called `00C8:2410`, and set `DS:1A34` to `0xFFFF`. |
| `0x0007CBAB` | A shared call sink receives one of three far pointers: `DS:2025` after a record flag branch, `DS:2036` when no eligible placement slot remains, or a stack buffer formatted from `DS:204E` after `00C8:288C` returns nonzero and a selected-index comparison differs. The first two texts report carrying limits; the formatted text names a recipient. |
| `0x0007CF30` | The text at `DS:205C` is passed only when `0568:0084` returns an index other than `0xFFFF` and the result from `00B0:391D` compares below `8` minus a sign-extended byte at the selected record's `+0x16`. The next call is `00C8:2452` with that index and a stack record buffer. |
| `0x0007D0CC` | For a scanned record whose first word is `1` and whose byte at `+0x13` is `0`, `6`, or `9`, the text at `DS:2070` is passed when the word at `+0x10` is zero, or when the result from `00B0:391D` compares below `10` minus that word. The next call is `00C8:2452` with the scanned index and stack record buffer. |

Each site passes a far pointer and removes four argument bytes. The two
corrosion texts are followed by a helper call and a return value of one on
their local success paths; this reading does not establish all effects of
that helper or the full item rule.

## Interpretation

These four syntactic sites are conditional feedback from money, item-placement
and corrosion paths. They enter the same overlay 172 routine whose later
`WIND/10501` setup controls whether its message-delay wait runs
(FND-CONFIG-018). The call-site conditions alone do not prove a successful
message-window acquisition or a visible message in a live state.

## Alternatives

The caller inputs, record-field meanings and the two helpers' complete
effects were not read here. In particular, the comparisons with
`00B0:391D` do not by themselves establish a probability or a complete
corrosion rule. Indirect messages and later failures remain possible.

## How to reproduce

Disassemble the approved `DSUN.EXE` at physical offsets
`0x0007C560..0x0007C7CB`, `0x0007C960..0x0007CBB7`,
`0x0007CED0..0x0007CF50`, and `0x0007D02A..0x0007D0F3`, keeping each
window bounded to at most 512 bytes. Resolve calls at the four offsets in
the table through their `0x0560` FBOV fixups. Compare the pushed data-segment
offsets with FND-ITEM-009.
