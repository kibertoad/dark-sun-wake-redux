---
id: FND-UI-026
title: WIND 19503 places the character generation controls, and WIND 19504 and 19505 the discipline and sphere lists
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x20AF2..0x211C1
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

`RESOURCE.GFF#WIND/19503` (FMT-UI-001), at `0x20AF2`, is 320 x 200 with no image at `0xC2` and 22
children:

| Child | Resource | Position | Size | Mask | Image |
|---|---|---|---|---|---|
| 0 | `BUTN/2001` | (0, 0) | 100 x 116 | 0 | None |
| 1 to 8 | `BUTN/2002` to `/2009` | (217, 10) to (217, 66), 8 apart | 56 to 94 x 6 | 0 | `ICON/2002` to `/2009`, 3 frames each, 48 to 88 x 7 |
| 9 | `BUTN/2010` | (135, 75) | 50 x 42 | 0 | None |
| 10 | `BUTN/2027` | (135, 20) | 45 x 35 | 0 | None |
| 11 | `BUTN/18302` | (258, 154) | 44 x 15 | 0 | `ICON/18109`, 44 x 15, 4 frames |
| 12 | `BUTN/19304` | (243, 174) | 59 x 18 | 0 | `ICON/19100`, 59 x 18, 4 frames |
| 13 | `BUTN/2011` | (79, 145) | 82 x 7 | 0 | None |
| 14 to 19 | `BUTN/2012` to `/2017` | (4, 139) to (4, 174), 7 apart | 50 x 5 | 0 | None |
| 20 | `BUTN/2018` | (79, 174) | 58 x 5 | 0 | None |
| 21 | `EBOX/4003` | (40, 125) | 95 x 8 | 10 | `BMP/19004`, 96 x 9 |

The icons of `BUTN/2002` to `/2009`, decoded with `PAL/1000`, show the eight class names; those of
`BUTN/18302` and `BUTN/19304` show the words for leaving and for finishing.

`RESOURCE.GFF#WIND/19504` and `WIND/19505` are 110 x 64 and name `BMP/20087` (111 x 64) at
`0xC2`. Each places five buttons at (7, 15) to (7, 47), 8 apart, 7 pixels high, each with a
three-frame `ICON` of the same number: `BUTN/2038` to `/2041` and `/2046` in `WIND/19504`, and
`BUTN/2042` to `/2045` and `/2047` in `WIND/19505`. `BUTN/2046` has mask 2 and the others 0. The
icons of `WIND/19504` show the three psionic disciplines, a fourth label whose first and third
frames are empty and whose second frame names a fourth discipline, and a label for viewing the
clerical spheres. Those of `WIND/19505` show the four elemental spheres and a label for viewing
the psionic disciplines. Frame 0 of each label is dark brown, frame 1 bright and frame 2 grey.

## Interpretation

`WIND/19503` is the character generation screen the manual shows (SRC-MANUAL-1994, page 8): the
body portrait at (0, 0), the character icon at (135, 20), the die at (135, 75), the class list at
the upper right, the name box and six ability rows at the lower left, and the leave and finish
buttons. `WIND/19504` and `WIND/19505` are the list of psionic disciplines and the list of clerical
spheres that the screen shows in turn, each with a label that switches to the other.
`BUTN/2046`'s mask of 2 keeps the pointer search from choosing it (RULE-UI-001). The three frames
of a label look like the unselected, selected and unavailable states.

## Alternatives

Earlier notes read the labels of `WIND/19504` as psionics, spells, half-giants, a blank and a view
of spheres; the decoded frames show the disciplines. Which parts of the screen the unlabelled
buttons are, other than by their places in the manual's picture, and where the game places the
two lists, are not known; no code that names these numbers was found (FND-UI-012).

## How to reproduce

Read the three windows and their children through the directory, and decode each icon with
`PAL/1000`.
