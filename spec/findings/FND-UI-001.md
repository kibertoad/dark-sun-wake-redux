---
id: FND-UI-001
title: RESOURCE.GFF holds 28 WIND, 139 BUTN, 97 APFM and 7 EBOX resources, each opening with its tag, size and number
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x1E5F4..0x6A737
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

`RESOURCE.GFF` (5,724,669 bytes, XXH3-128 `92fe031616f36062f99484b0f728210e`) holds, read through
its directory (FMT-GFF-001):

| Tag | Resources | Numbers | Sizes in bytes |
|---|---|---|---|
| `WIND` | 28 | 3020 to 19505 | 291 to 2,931, each 261 plus a multiple of 30 |
| `BUTN` | 139 | 2001 to 19304 | 110 (103 resources), 114 (7), 115 (1), 125 (6), 130 (4), 135 (1), 140 (1), 143 (10), 150 (1), 163 (5) |
| `APFM` | 97 | 1010 to 19201 | 116 in every one |
| `EBOX` | 7 | 4003 to 18401 | 168 in every one |

All 271 lie between the offsets located above. In every one the first four bytes are the tag, the
32-bit value at `0x4` equals the resource's size in the directory, and the 32-bit value at `0x8`
equals its number in the directory. The `WIND` numbers are 3020, 10500, 10501, 11500, 12500 to
12503, 13500, 13501, 14000 to 14002, 15500, 15502, 15503, 16500, 17500 to 17502, 18500, 18501
and 19500 to 19505; the `EBOX` numbers are 4003, 12400 to 12402, 15400, 18400 and 18401.

`CD:RESOURCE.GFF` (5,782,746 bytes, XXH3-128 `b98e6a1d2f2e233cc767717e7b170f4b`) holds the same
271 resources under the same numbers, byte for byte the same. The two files differ in the `ICON`
resources 19115 to 19122 only.

## Interpretation

These four tags are the game's window, button, frame and edit box records, and each record repeats
the directory's tag, size and number in its first 12 bytes.

## Alternatives

None known.

## How to reproduce

Read the directory of `RESOURCE.GFF` (FMT-GFF-001), list the resources of the four tags with their
offsets and sizes, and compare bytes `0x0` to `0xB` of each with its directory entry. Repeat for
`CD:RESOURCE.GFF` and compare the resources byte for byte.
