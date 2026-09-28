---
id: FND-CONFIG-072
title: Resident relocations to the message overlay contain four direct message calls
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
tool: Python 3.14.7 MZ relocation-table inspection; Capstone 5.0.7 bounded 16-bit disassembly
environment: null
---

## Observation

The approved `DSUN.EXE` MZ header declares 4,703 resident relocation
entries. Seven relocate a word whose unrelocated value is `0x466A`, the
segment of overlay 172's header before adding the `0x1000` load segment.
Six are segment operands of direct far calls:

| Call file offset | Overlay 172 entry | Role in this inventory |
|---|---|---|
| `0x0001DF65` | `566A:0034` | Another entry. |
| `0x0001E383` | `566A:002A` | Message call in FND-CONFIG-017. |
| `0x0001EB4E` | `566A:002A` | Message call in FND-CONFIG-017. |
| `0x0001F025` | `566A:002F` | Another entry. |
| `0x0002020A` | `566A:002A` | Message call in FND-CONFIG-017. |
| `0x0002036E` | `566A:002A` | Message call in FND-CONFIG-017. |

The seventh relocation is at file offset `0x0004B5E0`, the segment word
of overlay descriptor 172 in the FBOV segment table (FMT-EXE-002). It is
not a call site or a stored message-entry pointer.

## Interpretation

The resident MZ relocation table adds no address-taking reference to the
message entry beyond the four direct resident calls already identified by
FND-CONFIG-017. Together with FND-CONFIG-070, the declared relocations and
overlay fixups supply direct calls only for the known message entry.

## Alternatives

Unrelocated values, a far pointer assembled or copied at runtime, or a
computed call could still reach the entry. FND-CONFIG-073 shows that the
`0034` call target contains an internal call to the message entry; the
`002F` target does not contain one in its bounded routine.

## How to reproduce

Read the MZ relocation count and table offset from header fields `0x06`
and `0x18`. For each relocation, convert its segment:offset pair to a
physical file offset using the MZ header size at `0x08`, then select words
equal to unrelocated `0x466A`. Inspect the preceding direct-far-call opcode
and offset words at the six code sites above. Verify that
`0x0004B5E0 = 0x0004B080 + 172 * 8` is overlay descriptor 172's segment
word. This search is limited to declared resident relocations.
