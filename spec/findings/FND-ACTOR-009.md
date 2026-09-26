---
id: FND-ACTOR-009
title: Object 9,258 is not named by a constant in the resident image or by its own OJFF words
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:9740..1000:9761
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x13BC..0x13CC
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

`OBJEX.GFF#OJFF/9258`, at the offset located above, reads as the 16-bit values 2, 8, 44, 25,089,
29,440, 64, 346 and 0. Its values at offsets `0x0`, `0x6`, `0x8` and `0xA` were looked up among the
resource numbers of four tags of `OBJEX.GFF`:

| Offset | Value | `SCMD` (538) | `RDFF` (1,643) | `OJFF` (4,479) | `BMP ` (3,727) |
|---|---|---|---|---|---|
| `0x0` | 2 | no | yes | yes | yes |
| `0x6` | 25,089 | no | no | no | no |
| `0x8` | 29,440 | no | no | no | no |
| `0xA` | 64 | no | yes | yes | yes |

None of the four values is the number of one of the 23 `RDFF` resources that hold the label at
`OBJEX.GFF#ALL/2` offset 3,611 (FND-ACTOR-007).

The bytes `2A 24`, 9,258 as a 16-bit value, occur three times in `DSUN.EXE`: at file offset
`0x30B1` in the header, at `DSUN.EXE+0x0008722C` in the `FBOV` pack between the code of overlays
197 and 198, and once in the resident load image, at `1000:9754`. There they are the fourth of
nine 16-bit values from `1000:974E` to `1000:975F`, `0x2441`, `0x22C5`, `0x2356`, `0x242A`,
`0x227D`, `0x239F`, `0x230D`, `0x2179` and `0x21FB`, which end where a routine begins at
`1000:9760`.

## Interpretation

No field of the object's `OJFF` record names a script or a record that holds the label; the two
matches are small numbers that every one of these tags uses. The one resident occurrence of the
value is one of a run of near offsets into segment `1000`, so no resident code names object 9,258
by a constant. Object 9,258 lies in the range 9,000 to 13,998 that `31E0:0EFF` requests no `RDFF`
for (FND-ACTOR-003), and it has no `RDFF` resource (FND-ACTOR-007).

## Alternatives

The object number can reach the code as data, from a region's entity table or a script, so the
absence of a constant does not show that no code treats it apart. The run of values at
`1000:974E` was not traced to the code that reads it.

## How to reproduce

Read `OBJEX.GFF#OJFF/9258` and the resource numbers of `SCMD`, `RDFF`, `OJFF` and `BMP ` from the
directory of `OBJEX.GFF`. Search `DSUN.EXE` for `2A 24` and place each match.
