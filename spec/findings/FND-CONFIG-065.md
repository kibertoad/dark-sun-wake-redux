---
id: FND-CONFIG-065
title: The shipped message window follows the indexed resource lookup path
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x574013..0x574087
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x570B71..0x570C55
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:0615
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:07B1
tool: Python 3.14.7 bounded GFF directory inspection; Capstone 5.0.7 16-bit disassembly of bounded resident windows
environment: null
---

## Observation

The installed `RESOURCE.GFF` has an indexed `WIND` tag record at `0x574013`.
Its high-bit count word is `0x8000001C`: 28 entries, with index number 10
and 12 number ranges. The second range is 10500 through 10501. Expanding
the preceding one-entry range places `WIND/10501` at zero-based position 2.
The plain `GFFI` tag's resource 10 points to a 228-byte index at `0x570B71`.
That index has count 28, and entry 2 gives `WIND/10501` at file offset
`0x5B6AB`, length `0x141` (FND-GFF-003).

The resource reader at `38FF:07B1` tests the type record's high bit and
calls `38FF:0615` for the indexed case (FND-CONFIG-038). That helper tests
the requested number against each range, accumulates the preceding range
lengths, and calculates `4 + 8 * position` within the selected `GFFI`
resource. It looks up `GFFI` and the record's index number, seeks to the
calculated pair, requires an eight-byte read, and returns its offset and
size only after these operations succeed. A missing `GFFI` entry or failed
seek or read returns failure.

## Interpretation

For the shipped directory and an available archive, `WIND/10501` takes
the indexed lookup branch and passes its range-membership test. The
ordinary twelve-byte numbered-entry search is not its path. The directory
alone does not establish that a particular message call reaches this
archive or that the index and resource I/O succeed in every live state.

## Alternatives

The possibility that `WIND/10501` takes the ordinary numbered-entry
branch is ruled out by the high-bit record and the reader's branch. A
later archive change, I/O error or allocation failure could still prevent
the resource pointer from reaching the message wait gate. This static
reading does not establish any such failure during play.

## How to reproduce

Read the installed `RESOURCE.GFF` directory beginning at `0x573CE9`,
including the `WIND` record at `0x574013` and the plain `GFFI` entry
number 10. Expand the `WIND` ranges to locate number 10501, then read
position 2 from `GFFI/10`. Disassemble the approved `DSUN.EXE` at
`0x0002E805..0x0002E9A1` and `0x0002E9F0..0x0002EA52`; follow the
high-bit branch and the indexed helper's range, index, seek and read
checks.
