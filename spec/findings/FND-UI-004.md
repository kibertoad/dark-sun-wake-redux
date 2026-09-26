---
id: FND-UI-004
title: A BUTN resource is a 110-byte fixed part with size, event mask, repeated number, icon and a counted tail
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x1E7C4..0x1E97C
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x69AA5..0x6A03B
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

Read byte by byte across all 139 `BUTN` resources of `RESOURCE.GFF` (FND-UI-001), past the
12-byte header:

| Offset | Values |
|---|---|
| `0xC` | 0 in every record |
| `0xD` | 64 in `BUTN/2046`, 0 in the others |
| `0xE` | 1 in `BUTN/14005`, `/14006` and `/14007`, 0 in the others |
| `0xF` to `0x27` | 0 in every record |
| `0x28`, 16 bits | 43 distinct values; with `0x2A`, sizes from 9 x 8 to 320 x 200 |
| `0x2A`, 16 bits | 21 distinct values |
| `0x2C` to `0x57` | 0 in every record |
| `0x58`, 16 bits | 0 (106 records), 2 (1), 4 (1), 80 (1), 84 (5), 144 (6), 160 (9), 208 (10) |
| `0x5A`, 32 bits | the resource's own number in every record |
| `0x5E` to `0x63` | 0 in every record |
| `0x64`, 32 bits | 0 in 19 records; in the other 120 the number of an `ICON` resource of the file |
| `0x68` to `0x6C` | 0 in every record |
| `0x6D` | equal to the record's size less 110 in every record |

The bytes from `0x6E` to the end, present in 36 records, are printable ASCII followed by zeros, or
zeros only. Their text is not copied here.

Examples: `BUTN/19300` at `0x1E7C4` is 110 bytes, 127 x 12, `0x58` 0, `ICON` 19111, which has four
127 x 12 frames. `BUTN/18304` at `0x69AA5` is 143 bytes, 163 x 11, `0x58` 208, `ICON` 18100 with
four 165 x 11 frames, and 33 at `0x6D`. `BUTN/19302` is 192 x 12 and its `ICON/19113` has frames of
191 x 13, 191 x 13, 1 x 1 and 191 x 13.

The 19 records with no icon are 2001, 2010 to 2018, 2027, 2082, 2099, 10309, 12300 and 14004 to
14007. The nonzero values at `0x58` are 2 in `BUTN/2046`, 4 in `/18300`, 80 in `/15304`, 84 in
`/10307`, `/11317`, `/11318`, `/14004` and `/15309`, 144 in `/14001` to `/14003` and `/14005` to
`/14007`, 160 in `/11319`, `/11320`, `/13300`, `/13302` to `/13304` and `/17300` to `/17302`, and
208 in `/18304` to `/18313`.

## Interpretation

`0x28` and `0x2A` are the button's width and height, `0x58` its event mask (FND-UI-006), `0x64` the
image drawn for it, and `0x6D` the length of an optional tail. The art in the `ICON` is not always
the button's size.

## Alternatives

That `0x64` is the image drawn for the button follows here from every value naming an `ICON`; the
captures in FND-UI-016 and FND-UI-018 show it drawn. What the tail and the bytes `0xD` and `0xE`
do is not known.

## How to reproduce

Read each `BUTN` resource of `RESOURCE.GFF` through the directory, tabulate each byte and each
aligned 16-bit value of the first 110 bytes, and look up each value at `0x64` among the `ICON`
numbers.
