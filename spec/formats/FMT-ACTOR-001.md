---
id: FMT-ACTOR-001
title: Object definition
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["OBJEX.GFF"]
byte_order: little
size: 16
text: false
definition: fmt_actor_001.ksy
evidence: [FND-ACTOR-001, FND-ACTOR-002, FND-ACTOR-012, FND-ACTOR-003, FND-ACTOR-004, FND-IMAGE-010, SRC-DSUN-MUSIC-79B6927]
conflicting: []
split_with: []
related: [RULE-ACTOR-001]
---

## Layout

The layout of an `OJFF` resource of `OBJEX.GFF`: the definition of an object that a region can
place, named by the object's number [FND-ACTOR-001]. `31E0:0EFF` requests it by the number a
region entity or a placed-object entry carries, and `31E0:0E1B` reads the fields marked below
[FND-ACTOR-003].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x0` | 1 | `UINT8` | `unk_00` | Purpose unknown. 0, 2, 16 or 18 in the shipped records. `31E0:0E1B` copies it into the object's slot record. | supported | FND-ACTOR-001, FND-ACTOR-003 |
| `0x1` | 1 | `UINT8` | `unk_01` | Purpose unknown. 0 in every shipped record, and not read by `31E0:0E1B`. | supported | FND-ACTOR-001, FND-ACTOR-003 |
| `0x2` | 2 | `INT16LE` | `x_offset` | Pixels from the left edge of the object's image to its position: the image's left edge is the position's x less this. 0 to 64 in the shipped records. | supported | FND-ACTOR-001, FND-ACTOR-003, FND-IMAGE-010 |
| `0x4` | 2 | `INT16LE` | `y_offset` | Pixels from the top edge of the object's image to its position, before `vertical_offset`: the image's top edge is the position's y less this and less `vertical_offset`. -1 to 64 in the shipped records. | supported | FND-ACTOR-001, FND-ACTOR-003, FND-IMAGE-010 |
| `0x6` | 2 | `UINT16LE` | `unk_06` | Purpose unknown. 1,788 distinct values in the shipped records; not read by `31E0:0E1B`. | supported | FND-ACTOR-001, FND-ACTOR-003 |
| `0x8` | 2 | `UINT16LE` | `unk_08` | Purpose unknown. 1,361 distinct values in the shipped records; not read by `31E0:0E1B`. | supported | FND-ACTOR-001, FND-ACTOR-003 |
| `0xA` | 1 | `INT8` | `vertical_offset` | Further pixels the object's image is drawn above its position, sign-extended and subtracted from y with `y_offset`. 0, 10, 24, 30, 32, 64 or 85 in the shipped records, and equal to byte 4 of every `ETAB` record that names the object. | supported | FND-ACTOR-001, FND-ACTOR-003, FND-IMAGE-010 |
| `0xB` | 1 | `UINT8` | `unk_0B` | Purpose unknown. 0 in every shipped record but one, where it is 5. `31E0:0E1B` copies it into the object's slot record. | supported | FND-ACTOR-001, FND-ACTOR-003 |
| `0xC` | 2 | `UINT16LE` | `image` | Number of the object's image, a `BMP ` resource of `OBJEX.GFF`, 1 to 3,953 in the shipped records. `31E0:0EFF` replaces it for seven object numbers, and `31E0:426E` returns it. | supported | FND-ACTOR-001, FND-ACTOR-002, FND-ACTOR-003, FND-ACTOR-004, FND-IMAGE-010 |
| `0xE` | 2 | `UINT16LE` | `unk_0E` | Purpose unknown. 0 in every shipped record. `31E0:0EFF` overwrites it with `image` in its copy of the record in most cases. | supported | FND-ACTOR-001, FND-ACTOR-003 |
| `0x10` | | | | Total size 16 bytes | | |

The value file `FMT-ACTOR-001.objects.csv` gives every field of all 4,479 `OJFF` resources of
`OBJEX.GFF`, one row per resource in order of its number, with the columns `object` (the
resource number, 1 to 32,003) and the field names of the layout, each field read with its type
[FND-ACTOR-001].

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 4,479 `OJFF` resources of `OBJEX.GFF` in BLD-GOG-EN-1.1: each is 16 bytes, and each `image`
names a `BMP ` resource that decodes [FND-ACTOR-001]. The 287 definitions the opening region
names use 246 images with 477 frames [FND-ACTOR-012]. The placement of 22 objects in the first
gameplay frame of the opening region matches `x_offset`, `y_offset` and `vertical_offset`
[FND-IMAGE-010], and the opening party leader is drawn with the image of
`OBJEX.GFF#OJFF/305` [FND-ACTOR-002].

## Open questions

- What `unk_00`, `unk_01`, `unk_06`, `unk_08`, `unk_0B` and `unk_0E` do. `unk_00` uses only bits
  1 and 4, and `unk_06` and `unk_08` may each be two bytes, since their low bytes take only 10 and
  8 values (FND-ACTOR-001). SRC-DSUN-MUSIC-79B6927 reads them as raw words. (Q-ACTOR-002)

- What the image numbers 11,001 to 11,008 and 13,009 that `31E0:0EFF` puts in place of `image`
  for some objects and slots name, and what the object numbers 430, 5,879, 415, 561, 541, 547
  and 1,339 are (FND-ACTOR-003). (Q-ACTOR-002)

- Why `vertical_offset` is kept both here and in each `ETAB` record (FMT-REGION-006), and whether
  any code reads the `ETAB` copy for drawing; `31E0:0E1B` stores that copy at `0xE` of the slot
  record (FND-ACTOR-003). (Q-ACTOR-002)

- How the overlay code that names the tag reads the record (FND-ACTOR-006). (Q-ACTOR-002)
