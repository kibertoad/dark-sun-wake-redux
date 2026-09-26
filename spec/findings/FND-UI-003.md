---
id: FND-UI-003
title: Bytes 0xC to 0xA7 of a WIND resource copy an EBOX or a BUTN record
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x20711..0x211C1
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x5B222..0x5B6AB
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

In 16 of the 28 `WIND` resources of `RESOURCE.GFF` the 32-bit value at `0x18` is the number of an
`EBOX` resource, and bytes `0xC` to `0xA7` of the window equal bytes `0xC` to `0xA7` of that edit
box, except byte `0x9F` in two windows:

| Windows | `EBOX` copied |
|---|---|
| 19500 to 19505 | 4003 |
| 18500, 18501 | 18401 |
| 12500 to 12503 | 12402 |
| 3020, 15500, 15502, 15503 | 15400 |

Byte `0x9F` is 32 in `WIND/19500` and 8 in `WIND/19501`, where the edit box has 0. The copied
edit box is a child of the window only in `WIND/19503`, `/18501`, `/12503` and `/15503`.

In the other 12 windows the value at `0x18` is 0, and bytes `0xC` to `0x6D` equal bytes `0xC` to
`0x6D` of the `BUTN` resource whose number is the 16-bit value at `0x5A`: 10315 in `WIND/10500`
and `/10501`, 11320 in `/11500`, 13304 in `/13500` and `/13501`, 14007 in `/14000` to `/14002`,
16310 in `/16500`, and 17310 in `/17500` to `/17502`. Their bytes `0x6E` to `0xA7` are 0, or equal
the button's tail (`/11500`, `/13500`, `/13501`), or hold 6 printable bytes at `0x6E` to `0x73`
(`/17500` to `/17502`); byte `0x9F` is 1 in `/14000` to `/14002` and byte `0xA4` is 1 in `/14001`
and `/14002`.

So the 32-bit value at `0x3A` of a window, the `image` field of a copied edit box, is 19004 in
`WIND/19500` to `/19505`, 10002 in `/18500` and `/18501`, and 0 in the other 20.

## Interpretation

These bytes do not describe the window. They are a copy of the last edit box or button record in
some buffer of the tool that wrote the file, left in each window record. The value at `0x3A` is the
image of the copied edit box, and says nothing about the window's own picture.

## Alternatives

The game could still read some of these bytes; FND-UI-008 shows that the generic window code does
not read `0x3A`, and no other reader is known. Why byte `0x9F` differs in two windows is not
known.

## How to reproduce

For each `WIND` resource, look up the `EBOX` numbered by its 32-bit value at `0x18` and compare
bytes `0xC` to `0xA7`; where that value is 0, look up the `BUTN` numbered by the 16-bit value at
`0x5A` and compare bytes `0xC` to `0x6D`.
