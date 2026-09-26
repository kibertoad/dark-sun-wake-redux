---
id: FMT-IMAGE-003
title: Palette resource
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["RESOURCE.GFF", "RGN*.GFF"]
byte_order: little
size: 768
text: false
definition: fmt_image_003.ksy
evidence: [FND-IMAGE-004, FND-IMAGE-010, SRC-DSUN-MUSIC-79B6927]
conflicting: []
split_with: []
related: []
---

## Layout

The layout of every resource under the tag `PAL ` [FND-IMAGE-004]. Entry `i` gives the colour of
palette index `i` in every frame drawn with this palette (FMT-IMAGE-002).

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x000` | 768 | `FMT-IMAGE-004[256]` | `colors` | The 256 colours, in index order. | supported | FND-IMAGE-004, FND-IMAGE-010 |
| | | | | Total size 768 bytes | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 40 `PAL ` resources of the installed `.GFF` files of BLD-GOG-EN-1.1: 20 in `RESOURCE.GFF` and
one in each of the 20 region files [FND-IMAGE-004]. The first gameplay frame of the opening
region shows the colours of `RGN032.GFF#PAL/50` [FND-IMAGE-010]. SRC-DSUN-MUSIC-79B6927 reads
the same layout.

## Open questions

- Which palette each image is drawn with (FMT-IMAGE-001, Q-IMAGE-003).
