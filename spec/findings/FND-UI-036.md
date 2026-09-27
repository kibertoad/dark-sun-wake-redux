---
id: FND-UI-036
title: WIND 18500 serves Save and Load modes with ten list rows
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x6A18B..0x6A433
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5736:0020..5736:0025
tool: DarkSunWakeRedux.Inspect UI and image catalogs; bounded RESOURCE.GFF byte inspection; Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of FBOV overlay 192; ReportFbovOverlayMap.ps1
environment: null
---

## Observation

Overlay 192's entry at `DSUN.EXE+0x0007D300` sets a mode byte to one and
enters a shared window routine at `0x0007D54B`; the adjacent entry at
`0x0007D315` clears the byte and enters the same routine. The Start Game
window's Load Saved Game branch calls the first entry (FND-UI-035).

The shared routine passes 18500 to a window acquisition call, allocates
space for ten 125-byte records, and calls a row-population routine with
indexes 0 through 9. `RESOURCE.GFF#WIND/18500` is 320 x 181 and has these
ordered children, at positions relative to the window:

| Child | Resource | Position | Control size | Resource image |
|---|---|---|---|---|
| 0 | `BUTN/18300` | (126, 0) | 67 x 23 | `ICON/18107`, SAVE title |
| 1 | `BUTN/18301` | (231, 30) | 44 x 15 | `ICON/18108`, SAVE action |
| 2 | `BUTN/18302` | (231, 50) | 44 x 15 | `ICON/18109`, EXIT |
| 3 to 12 | `BUTN/18304` to `/18313` | (46, 31) to (46, 130), 11 apart | 163 x 11 | `ICON/18100`, four frames |
| 13 | `EBOX/18400` | (49, 147) | 164 x 12 | `BMP/10002`, 161 x 12 |

When the mode byte is one, the shared routine supplies `ICON/18101` (LOAD
title, 70 x 23) for button 18300 and `ICON/18102` (LOAD action, 44 x 15)
for button 18301. The record's SAVE title and action images remain in the
zero-mode path. The ten row buttons are populated from the allocated
records; a separate routine marks one row and passes its text to a control.

The window's own image field at offset `0xC2` is zero. The value 10002 at
offset `0x3A` is copied edit-box data (FND-UI-003), not a window background.
The UI catalog currently exposes that copied value as
`Window.ImageResourceNumber`; that output is not used here as evidence for
window paint.

## Interpretation

One window graph supplies both the Save and Load lists, with ten visible
row controls. The entry reached from the start window selects the LOAD
title and action images. The other entry leaves the SAVE images named by
the resource. The shipped LOAD and SAVE action art differs from the
manual's generic OKAY wording.

## Alternatives

The exact window position, native fill and bevel, icon frame selection,
text in each row, and the final effect of clicking a row or action button
were not established by this reading. A row control's dimensions need not
equal every frame's painted bounds. The zero-mode entry's caller was not
traced here.

## How to reproduce

Use the read-only UI and image catalogs on the approved `RESOURCE.GFF` and
inspect `WIND/18500` at `0x6A18B..0x6A433`, its children and the named
icons with palette 1000. Check the true window image field at offset
`0xC2` and the copied edit-box image at `0x3A`. Map overlay 192, then
disassemble bounded windows at `0x0007D300..0x0007D329`,
`0x0007D54B..0x0007D6AB`, `0x0007E058..0x0007E0F3` and
`0x0007E0F4..0x0007E15F`.
