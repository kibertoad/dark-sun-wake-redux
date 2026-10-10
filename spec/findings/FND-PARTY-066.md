---
id: FND-PARTY-066
title: The generation screen's sphere window sets DS:42C2 to 0x80, 0x40, 0x20 or 0x10 for its buttons 2042 to 2045, whose icons read AIR, EARTH, FIRE and WATER, and the Cleric, Druid and Ranger buttons open it with 0x80
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006B24D..0x0006B30A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006B4C8..0x0006B520
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006BC81..0x0006BCE3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006BE6B..0x0006BF77
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006C01D..0x0006C071
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006C071..0x0006C07D
    kind: file-data
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, trampoline_target.py), and a Python 3.14.7 reading of RESOURCE.GFF through its directory (FMT-GFF-002, FMT-GFF-003, FMT-GFF-007) decoding ICON frames by RULE-IMAGE-001 and RULE-IMAGE-002
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`.

**The window.** Overlay 183 `+0E8B` closes the windows kept in `DS:1134` and `DS:1138`, storing
first the result of `+0CA1` for the first one's buttons `0x7F6` to `0x7F8` in `DS:42C0`
(`+0E8E..+0ED3`). It then opens a window with the handler `56CC:0057` and keeps it in
`DS:1138` (`+0ED6..+0EF1`), prints the string at `DS:10A8` (`CLERICAL SPHERE`) in it, and calls
`+0D95` with the window, buttons `0x7FA` to `0x7FD` and the word at `DS:42C2` (`+0EF4..+0F4D`).

**Its handler.** `56CC:0057` is overlay 183 `+103D`. For event 1 it stores 0 in `DS:1462` and
returns -1; for event 2 it takes the button number less `0x7FA` and, when that is at most 5,
jumps through the six words at `+1091` (file `0x6C071`): buttons `0x7FA`, `0x7FB`, `0x7FC` and
`0x7FD` call `+04E8` with the button and 0x80, 0x40, 0x20 and 0x10, and button `0x7FF` calls
`+0F5F` (`+1040..+1085`).

**Reading the choice.** `+04E8` calls `+0D95` with the button's bit when `+0E5E` reports the
button on and `+0D2C` otherwise, then stores the result of `+0CA1` for buttons `0x7FA` to `0x7FD`
in `DS:42C2` (`+04EB..+053D`). `+0CA1` takes a window and a first and last button, and returns the
OR of 0x80 shifted right by `k` for each button first plus `k` whose state, read through
`3EBE:091B`, is 2 (`+0CB4..+0CFD`), or -1 when the window is 0. `+0F5F`, when `DS:1138` is
nonzero, stores the same result in `DS:42C2` and closes the window (`+0F63..+0F8D`).

**Who opens it.** The class handlers at overlay 183 `+0278` (code 1), `+02B7` (code 7) and `+02F6`
(code 2), after calling `+0A3E` with their code (FND-PARTY-063), store 0x80 in `DS:42C2` and call
`+0E8B` when `+0E5E` reports their button on, and otherwise call `+0F5F` and store 0 in `DS:42C2`
(`+027E..+02A4`, `+02BD..+02E3`, `+02FC..+0322`).

**The buttons.** `RESOURCE.GFF` holds `BUTN` resources 2042 to 2045 (`0x7FA` to `0x7FD`) with no
text tail and `icon` 2042 to 2045 (FMT-UI-003). The first frame of each of those `ICON` resources,
decoded by RULE-IMAGE-001, shows one word in capitals: AIR (2042, 33 by 7 pixels), EARTH (2043),
FIRE (2044) and WATER (2045).

## Interpretation

The sphere window is a set of four radio buttons, air, earth, fire and water in that order, and
`DS:42C2` holds the chosen one's bit, 0x80 for air down to 0x10 for water. Choosing Cleric, Druid
or Ranger opens it with air chosen. Overlay 184 `+1648` turns that bit into variants 0 to 3 and so
stores a Cleric as code 1 (air), 2 (earth), 3 (fire) or 4 (water), a Druid as 5 to 8 and a Ranger
as 13 to 16 in the same order (FND-PARTY-063). The four codes of each of those classes are its
four spheres.

## Alternatives

FND-PARTY-057 noted that the four codes differ only in their colour on the class line, and that
FND-PARTY-020's red Druid and darker yellow Cleric fit a colour per code without showing spheres.
The supplied Druid (record 42) holds code 7, fire by this reading, the colour SRC-README-1.1 gives
fire, and the supplied Cleric (record 41) code 2, earth.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 183 6BE6B 6BF77`,
`183 6C01D 6C071`, `183 6B4C8 6B520`, `183 6BC81 6BCE3` and `183 6B24D 6B30A`;
`trampoline_target.py <dsun> 56CC:0057`. Read the six words at file `0x6C071`. In the installed
`RESOURCE.GFF`, read `BUTN` 2042 to 2045 (FMT-UI-003) and decode the first frame of `ICON` 2042
to 2045 through the `GFFI` index of the `ICON` table (FMT-GFF-007), by RULE-IMAGE-001 and
RULE-IMAGE-002, as a mask of pixels that differ from the most common index.
