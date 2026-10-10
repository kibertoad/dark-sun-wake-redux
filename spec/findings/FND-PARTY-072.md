---
id: FND-PARTY-072
title: The generation screen steps alignment through codes 1 to 9 with a wrap from 1 down to 8, and moves on past any alignment a per-class table at 4E68:001C forbids unless Ctrl is held; the table allows no evil alignment, only true neutral to a druid, only good to a ranger and no lawful good to a thief
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006B522..0x0006B581
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006C5CE..0x0006C753
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006C753..0x0006C763
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004389C..0x000438E4
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006D40E..0x0006D415
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, direct_callers.py, trampoline_target.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. `DS:1429` points at the generation details record, whose alignment byte at `+0x14`
counts from 1 in the order of FND-PARTY-017 (FND-PARTY-055).

**The button.** Overlay 183 `+0542`, for button `0x7DB` (FND-PARTY-070), calls `+15EE` with the
result of `+0000`, 1 or -1, and redraws the field (`+0548..+059C`).

**Stepping.** Overlay 183 `+15EE` (trampoline `56CC:00A7`) adds its argument to the alignment
byte, stores 1 when the result is above 9 and then 8 when it is below 1 (`+15F1..+1617`). It then
calls `44B6:0011`, which returns the BIOS keyboard flags (FND-INPUT-005), and unless bit 4 is set
calls `+1631` with its argument (`+161C..+162E`).

**Checking.** Overlay 183 `+1631` (trampoline `56CC:00A2`) passes the class bytes to `+08DB`
(FND-PARTY-065) and jumps through the eight words at `+1773` (file `0x6C753`): mask 0 goes to
`+1669`, which stores 5 in the alignment byte; masks 1, 2, 3 and 5 go to `+1770`, which returns;
mask 4 to `+1675`, mask 6 to `+169F` and mask 7 to `+16F2` (`+1638..+1664`). For each of the first
one, two or three class bytes in order, those branches read the byte at `4E68:0012` plus 9 times
the class byte plus the alignment byte, as it is at that moment, and when it is 0 call `+15EE`
with the argument (`+1675..+176C`). Overlay 184 `+0380`, in the roll routine (FND-PARTY-071), calls
`+1631` with 1 (`+037E..+0385`).

**The table.** The nine bytes at `4E68:0012` plus 9 times the class code plus 1 (file `0x4389C`
for code 1), for alignments 1 to 9:

| Class | Lawful good | Lawful neutral | Lawful evil | Neutral good | True neutral | Neutral evil | Chaotic good | Chaotic neutral | Chaotic evil |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Cleric | 1 | 1 | 0 | 1 | 1 | 0 | 1 | 1 | 0 |
| Druid | 0 | 0 | 0 | 0 | 1 | 0 | 0 | 0 | 0 |
| Fighter | 1 | 1 | 0 | 1 | 1 | 0 | 1 | 1 | 0 |
| Gladiator | 1 | 1 | 0 | 1 | 1 | 0 | 1 | 1 | 0 |
| Preserver | 1 | 1 | 0 | 1 | 1 | 0 | 1 | 1 | 0 |
| Psionicist | 1 | 1 | 0 | 1 | 1 | 0 | 1 | 1 | 0 |
| Ranger | 1 | 0 | 0 | 1 | 0 | 0 | 1 | 0 | 0 |
| Thief | 0 | 1 | 0 | 1 | 1 | 0 | 1 | 1 | 0 |

The segment holds no write to these bytes (FND-PARTY-069).

## Interpretation

The alignment button moves forward or back through the nine alignments and skips each one a class
of the character forbids, so a character can hold only an alignment every one of its classes
allows: no class allows evil, a druid must be true neutral, a ranger good, and a thief anything but
lawful good. Holding Ctrl while pressing the button skips the check, so it can leave any alignment,
evil among them, until the next roll checks it again. Rolling checks the alignment and moves it
forward past forbidden ones; a character with no class is true neutral. Stepping back from lawful
good goes to chaotic neutral, skipping chaotic evil, which no class allows anyway.

## Alternatives

- Going over every class set the screen offers (FND-PARTY-068) with this table finds none that
  allows no alignment, so the recursion between `+15EE` and `+1631` ends without Ctrl.
- Bit 4 of the BIOS keyboard flags is Ctrl in the BIOS's definition; no capture shows the effect.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 183 6B522 6B581`,
`183 6C5CE 6C753` and `184 6D3CE 6D420`; `direct_callers.py <dsun> 183+15EE 183+1631`;
`trampoline_target.py <dsun> 56CC:00A2 56CC:00A7`. Read the eight words at file `0x6C753` and the
72 bytes at file `0x4389C` as eight rows of nine.
