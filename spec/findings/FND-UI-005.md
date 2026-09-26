---
id: FND-UI-005
title: APFM resources vary only in size and event mask, and EBOX resources name a BMP at 0x3A
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x1E5F4..0x1E6DC
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x20669..0x20711
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

All 97 `APFM` resources of `RESOURCE.GFF` (FND-UI-001) are 116 bytes. Past the 12-byte header,
every byte is 0 except the 16-bit values at `0x28` (15 distinct low bytes), `0x2A` (14) and
`0x58`. The values at `0x58` are 0 (40 records), 32 (1), 38 (1), 70 (1), 110 (1), 160 (1), 224 (1),
230 (42), 486 (8) and 494 (1). `APFM/19200` at `0x1E5F4` is 320 x 200 with 494 at `0x58`, and
`APFM/19201` is 28 x 16 with 486. Neither 494 nor 486 is the number of any resource in the file.

All 7 `EBOX` resources are 168 bytes. Past the header:

| Offset | Values |
|---|---|
| `0xC` to `0xF` | 0 |
| `0x10`, `0x12`, `0x14`, 16 bits each | 1 in every record |
| `0x16`, 16 bits | 16 (`EBOX/4003`), 11,226 (`/12400`, `/12402`), 64 (`/12401`, `/18400`, `/18401`), 200 (`/15400`) |
| `0x18`, 32 bits | the resource's own number |
| `0x1C` to `0x21` | 0 |
| `0x22`, `0x24`, 16 bits each | 95 x 8, 236 x 46, 239 x 46, 251 x 127, 133 x 85, 164 x 12, 164 x 12 in number order |
| `0x26` to `0x39` | 0 |
| `0x3A`, 32 bits | `BMP ` 19004 (`/4003`), 12002 (`/12400`, `/12401`), 10002 (`/18400`, `/18401`), 0 (`/12402`, `/15400`) |
| `0x3E` to `0x57` | 0 |
| `0x58`, 16 bits | 0, 84 (`/15400`) or 208 (`/18400`, `/18401`) |
| `0x5A`, 32 bits | `BUTN` numbers 2099 (`/4003`), 2082 (`/12400` to `/12402`), 15309 (`/15400`), 18313 (`/18400`, `/18401`) |
| `0x5E` to `0x7D` | the same in every record: 0 except 128 at `0x6E`, 255 at `0x72` and `0x73`, and 2 at `0x78` |
| `0x7E`, `0x80`, 16 bits each | 229 and 199 in `/12400`, 251 and 127 in `/12402`, 0 in the others |
| `0x82` to `0x95` | 0 |
| `0x96`, 16 bits | 10 (`/4003`, `/12401`, `/18400`, `/18401`), 4 (`/12400`), 0 (`/12402`, `/15400`) |
| `0x98` to `0xA7` | 0 |

`EBOX/4003` is at `0x20669`. The value at `0x58` of each edit box equals the value at `0x58` of
the button its `0x5A` names. The images at `0x3A` are 96 x 9 (`BMP/19004`), 243 x 47
(`BMP/12002`) and 161 x 12 (`BMP/10002`).

## Interpretation

An `APFM` record is a bare rectangle with an event mask. An `EBOX` record has its size at `0x22`
and `0x24`, an image at `0x3A`, and an event mask at `0x96` (FND-UI-006). Its bytes `0x58` to
`0x5D` look like two fields of a button, as a window's copied bytes do (FND-UI-003).

## Alternatives

That the `APFM` masks are not resource numbers rests on their values and on the code in
FND-UI-006. That `0x3A` of an edit box is drawn is shown for `EBOX/12400` only (FND-UI-016).

## How to reproduce

Read each `APFM` and `EBOX` resource of `RESOURCE.GFF` through the directory, tabulate each byte
and each aligned 16-bit value past the header, and look up the values at `0x3A` and `0x5A` among
the `BMP ` and `BUTN` numbers.
