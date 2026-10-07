---
id: FND-EXE-028
title: Bounded list-producer output dispatch converges on a value reread
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006B0B6F..0x006B0C0E
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006B0E89..0x006B0EAD
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00325AD8..0x00325AF4
tool: Ghidra 12.1.3 PUBLIC and bounded physical PE table reading
environment: null
---

## Observation

FND-EXE-027 reads the unsigned guard admitting output values zero through
six to the four-byte indexed table at `0x007288D8`, after local insertion.
The seven little-endian pointer values at shipped-file offset `0x00325AD8`
match the mapped table. The file's verified preferred image base and raw
section mapping are used; these are full near addresses, not file offsets.

| Output value | Direct table target | Selected data pointer | Shared helper path |
| --- | --- | --- | --- |
| 0 | `0x006B0B9A` | None in this path | Output reread directly |
| 1 | `0x006B0E95` | `0x00726477` | `0x006B0BBC` |
| 2 | `0x006B0E89` | `0x007264E4` | `0x006B0BE4` |
| 3 | `0x006B0EA1` | `0x00726525` | `0x006B0B76` |
| 4 | `0x006B0BB5` | `0x0072656A` | `0x006B0BBC` |
| 5 | `0x006B0BDD` | `0x007265D2` | `0x006B0BE4` |
| 6 | `0x006B0B6F` | `0x0072661C` | `0x006B0B76` |

The unsigned out-of-range branch from FND-EXE-027 enters `0x006B0C05`,
selects data pointer `0x0072667E` and joins `0x006B0BBC`. This table gives
selection addresses only: no contents or message meanings of these data
pointers are read or retained here.

Each of the three shared helper paths writes 131 to the local state word
at stack offset 1072, then calls `0x0058B2D0` with its selected data pointer.
After normal return it passes that returned 32-bit value as the second
argument to `0x0058C430`, with the local word at stack offset 1040 as the
first. The paths then enter `0x006B0B9A`. The two callees, local word's
provenance, indirect aliases and exceptional behavior remain unread; neither
callee is assigned display or error semantics from its call shape.

Value zero enters `0x006B0B9A` directly without either helper call in its
own dispatch path. At that join the code rereads the full 32-bit output word
at stack offset 1048 rather than a register holding the dispatched value.
Nonzero branches to `0x006B0EAD`. Zero increments the source index at offset
824 and returns to the producer loop test in FND-EXE-027. Later behavior at
the nonzero destination is outside this finding. In particular, helper
paths do not locally preserve a separate copy of the original value for
the continuation test, and their possible effects are conditional.

## Interpretation

The local dispatch is read across every admitted index and the default
arm, but its output meanings and final outcomes remain open under
Q-EXE-009. The direct zero path bypasses the two helpers; every other arm
converges on their ordered calls and then a shared-memory reread. Mapping
nonzero dispatch values straight to a final stop or failure would omit that
reread and the unread helper effects. This is not a complete reading of the
producer, the helper routines, data resources or cleanup destination.

## Alternatives

Calling the two helpers on the direct zero arm, testing only a byte of the
output, using a cached dispatch register for the continuation, or using the
same selected data pointer for all admitted nonzero values are ruled out
by the table and bounded instructions. Whether the helper calls change
that word, and whether its nonzero destination releases the inserted
pointer, require direct callee and alias evidence. Earlier entry guards,
output producers and exceptional paths remain conditional rather than
inferred from apparent message-selection structure.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Read 28 mapped bytes from
`0x007288D8`, and independently map the same range to bounded raw section
bytes in the verified shipped file. Decode exactly seven little-endian
32-bit targets. Use FND-EXE-027's unsigned guard and four-byte indexed jump
to establish what selects each row. Read 16 instructions from `0x006B0E89`,
24 from `0x006B0B9A`, and FND-EXE-027's 65-instruction window from
`0x006B0A5B` and 40-instruction window from `0x006B0B95`. Restrict the new
code reading to the cited ranges, excluding the later destination. Follow
each target, selected pointer, outgoing arguments and last writers into
the shared reread, including the default arm. Preserve callee and alias
conditions rather than labeling numeric values by inferred message roles.
Keep rich reports local and execute no interpreter or game.
