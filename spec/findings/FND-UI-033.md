---
id: FND-UI-033
title: WIND 16500 places thirteen Preferences buttons and its filmstrip control is 16303
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x5C787..0x5CA4E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5782:0000
tool: bounded GFF and indexed-icon decoding with DarkSunWakeRedux.Resources; ImageMagick 7.1.2-13; Ghidra 12.1.3 mapped-overlay MZ import with ReportInstructionContext
environment: null
---

## Observation

`RESOURCE.GFF#WIND/16500` is 210 by 116, has no image at `0xC2`, and has 15
children: frames `APFM/10201` (132 by 78 at (40, 18), mask 110) and
`APFM/11270` (4 by 4 at (18, 102)), plus these 13 buttons with mask 0.
Positions are relative to the window's corner.

| Child | Resource | Position | Size | Image | Visible mark or evidence |
|---|---|---|---|---|---|
| 2 | `BUTN/16300` | (49, 24) | 16 by 16 | `ICON/16100`, 4 frames | Music note |
| 4 | `BUTN/16305` | (66, 29) | 9 by 8 | `ICON/16105`, 4 frames | Left arrow |
| 3 | `BUTN/16304` | (159, 29) | 9 by 8 | `ICON/16104`, 4 frames | Right arrow |
| 5 | `BUTN/16301` | (49, 43) | 16 by 16 | `ICON/16101`, 4 frames | Sound mark |
| 7 | `BUTN/16307` | (66, 48) | 9 by 8 | `ICON/16105`, 4 frames | Left arrow |
| 6 | `BUTN/16306` | (159, 48) | 9 by 8 | `ICON/16104`, 4 frames | Right arrow |
| 11 | `BUTN/16309` | (56, 67) | 9 by 8 | `ICON/16105`, 4 frames | Left arrow |
| 10 | `BUTN/16308` | (149, 67) | 9 by 8 | `ICON/16104`, 4 frames | Right arrow |
| 9 | `BUTN/16303` | (49, 78) | 16 by 16 | `ICON/16103`, 4 frames | Filmstrip in every frame |
| 8 | `BUTN/16302` | (67, 78) | 16 by 16 | `ICON/16102`, 4 frames | Each frame is one uniform colour |
| 14 | `BUTN/16310` | (85, 78) | 16 by 16 | `ICON/16106`, 4 frames | Voice mark |
| 12 | `BUTN/11308` | (109, 78) | 28 by 16 | `ICON/11101`, 3 frames | Game Menu mark |
| 13 | `BUTN/10308` | (139, 78) | 28 by 16 | `ICON/10108`, 2 frames | Return mark |

In overlay 203, the Preferences renderer reads the byte at `DS:1437` and
passes it with button number `16303` to the same drawing routine it uses for
the music and sound-effect buttons. The read and button argument lie near
`DSUN.EXE+0x0008B90A`, mapped as `9616:054A..0558`. The load routine copies
`PREF/100` offset `0x07` back to `DS:1437` (FND-SAVE-005).

## Interpretation

`BUTN/16303` at (49, 78) is the animation control: its image is a filmstrip
and its drawn state comes from the saved byte at `PREF/100` offset `0x07`.
An earlier icon reading swapped this control with `BUTN/16302` when it
assigned meanings by eye. The purpose of the uniform-colour `BUTN/16302` at
(67, 78) is not established by these facts; it is a candidate for About.

## Alternatives

The button handler was not read, so the setting's polarity and the exact
effect of either button are unproven. The owner capture requested by
Q-CONFIG-001 can show whether (67, 78) opens About and what image-less native
widget treatment, if any, makes that control visible.

## How to reproduce

Decode `WIND/16500` and its child `BUTN` and `ICON` resources from the
installed `RESOURCE.GFF` with `PAL/1002`; count distinct colours in each
`ICON/16102` and `ICON/16103` frame. In the mapped-overlay import of the
approved `DSUN.EXE`, inspect bounded instruction context at `9616:054A` and
convert it to the shipped file offset with `ReportFbovOverlayMap.ps1`.
