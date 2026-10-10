---
id: FND-PARTY-071
title: The generation screen rolls each ability score as 4 plus the origin modifier plus the best of four rolls of 4d4, holds it between a class minimum (17 for the class's prime ability, a per-class figure for the others) and 20 plus the origin modifier, and steps it with wrapping inside those bounds
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006D090..0x0006D1ED
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006F12D..0x0006F197
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006D3CE..0x0006D5F1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006C504..0x0006C5CE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006B581..0x0006B6BA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006B7A1..0x0006B7AD
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006C0DF..0x0006C17A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006BC65..0x0006BC71
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00043816..0x0004385E
    kind: file-data
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, direct_callers.py, trampoline_target.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Segment `4E4F` starts at file `0x436F0`. `DS:1429` points at the generation details
record, whose origin byte at `+0x12` counts from 1, and `DS:142D` at its combatant record, whose
six bytes at `+0x19` the finish copies to `+0x15` of the details record (FND-PARTY-070), the
scores FMT-PARTY-001 stores in the order strength, dexterity, constitution, intelligence, wisdom,
charisma.

**Two tables in `4E4F`.** Six signed bytes per origin at `4E4F:0120` plus 6 times the origin byte
(file `0x43816` for human):

| Origin | Bytes |
| --- | --- |
| human | 0, 0, 0, 0, 0, 0 |
| dwarf | 1, -1, 2, 0, 0, -2 |
| elf | 0, 2, -2, 1, -1, 0 |
| half-elf | 0, 1, -1, 0, 0, 0 |
| half-giant | 4, -5, 2, -5, -3, -3 |
| halfling | -2, 2, -1, 0, 2, -1 |
| mul | 2, 0, 1, -1, 0, -2 |
| thri-kreen | 0, 2, 0, -1, 1, -2 |

Three bytes per class at `4E4F:0153` plus 3 times the generation class code (file `0x43846` for
code 1), a word and a signed byte: Cleric 4 and 9, Druid 4 and 12, Fighter 0 and 9, Gladiator 0
and 13, Preserver 3 and 9, Psionicist 4 and 12, Ranger 4 and 14, Thief 1 and 9.

**The minimum.** Overlay 184 `+209D` (trampoline `56DD:0052`) takes a count and a score position.
For each of the first count class bytes at `+0x1B` it takes 17 when the class's word equals the
position and the class's byte otherwise, and returns the greatest of these, or 0 for a count of 0
(`+20A2..+2101`).

**Setting one score.** Overlay 183 `+1524` (trampoline `56CC:00BB`) takes the first class byte, a
position, a flag, the current score and a count. It keeps the result of `+209D` for the count and
position in `DS:42D4`, and 20 plus the origin's byte for the position in `DS:42D2`
(`+152C..+155D`). With the flag nonzero it calls `28C9:391D` with 4 and 4 four times, adds 4 plus
the origin's byte to each result, keeps the greatest (signed, starting from 0), and returns it, or
`DS:42D4` when it is less (`+1566..+15C3`). With the flag 0 it returns `DS:42D4` when the score is
less than that, `DS:42D2` when the score is greater than that, and the score otherwise
(`+15C5..+15E6`). The first class byte is not read.

**The roll routine.** Overlay 184 `+033E` (trampoline `56DD:005C`) takes a flag. It draws the
screen's fields, calls overlay 183 `+1631` with 1 (FND-PARTY-072), and passes the class bytes to
overlay 183 `+08DB` (FND-PARTY-065). For mask 4, 6 or 7 it stores in each of the six bytes at
`+0x19`, in order, the result of `+1524` with the first class byte, the position, the flag, the
byte and 1, 2 or 3; for any other mask it leaves them (`+048B..+0544`). It then calls overlay 183
`+1D9B` and `+015D` with the flag (`+0546..+055E`), and goes on to the hit points. Its three calls
are overlay 183 `+0C89`, at the end of `+0A3E` with `+0A3E`'s second argument, and `+1137` and
`+118C` with 1.

**The reroll button.** Overlay 183 `+10FF`, for button `0x7DA` (FND-PARTY-070), five times draws
the icon whose number is the byte at `4E68:0010` plus the pass and calls `+033E` with 1; it then
takes `1000:0822` times 6 divided by 32768 as a number from 0 to 5, draws the icon whose number is
the byte at `4E68:0016` plus that, and calls `+033E` with 1 again (`+1103..+1192`).

**The score buttons.** Overlay 183 `+05A1`, for buttons `0x7DC` to `0x7E1`, jumps through the six
words at `+07C1` (file `0x6B7A1`), `+05BD`, `+0620`, `+06BE`, `+069F`, `+069F` and `+069F`; each
calls overlay 184 `+0000` (trampoline `56DD:0061`) with the event and the button less `0x7DC`, and
the first three then redraw other fields. Overlay 184 `+0000` takes the result of overlay 183
`+0000` (1 or -1, FND-PARTY-070) as a step, keeps 20 plus the origin's byte for the position in
`DS:42D2`, and for mask 4, 6 or 7 adds the step to the position's byte at `+0x19`, keeps `+209D`
for 1, 2 or 3 and the position in `DS:42D4`, sets the sum to `DS:42D4` when it is greater than
`DS:42D2`, then to `DS:42D2` when it is less than `DS:42D4`, and stores it (`+0008..+0150`); for
any other mask it changes nothing.

**Who calls `+0A3E` with a second argument of 1.** Overlay 183 `+01AE` in the portrait routine and
overlay 184 `+1067` in the finish (FND-PARTY-070) pass 3, 1 and 1; the class buttons pass 0
(FND-PARTY-066, FND-PARTY-070), and overlay 184 `+0C80` passes 0, 0 and 0.

## Interpretation

The scores belong to the character's classes. Each class asks 17 of its prime ability (wisdom for
Cleric, Druid, Psionicist and Ranger, strength for Fighter and Gladiator, intelligence for
Preserver, dexterity for Thief) and its own figure of every other score, and a character with
several classes must meet the highest demand on each score. The highest a score can be is 20 plus
the origin modifier, so a half-giant reaches 24 strength. A roll gives 4 plus the modifier plus the
best of four rolls of four four-sided dice, which never passes the maximum, raised to the minimum.
Choosing a class keeps the scores and moves each into the new bounds; the reroll button and
choosing a portrait roll them again, the button six times with only the last kept. The score
buttons step a score by one and wrap from the maximum to the minimum and back. With no class the
scores cannot change.

The origin bytes are the modifiers of RULE-PARTY-005, except that the half-giant's are 4, -5, 2,
-5, -3 and -3 where SRC-MANUAL-1994 gives 4, 0, 2, -2, -2 and -2.

## Alternatives

- Going over every class set the screen offers (FND-PARTY-068) with these tables finds no score
  whose minimum is above its maximum.
- `+015D` and what follows it in `+033E` were not read, so whether they draw random numbers, and
  so the full order of draws in a roll, is open.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 184 6D090 6D1ED`,
`184 6F12D 6F197`, `184 6D3CE 6D5F1`, `183 6C504 6C5CE`, `183 6B581 6B6BA`, `183 6C0DF 6C17A` and
`183 6BC40 6BC71`; `direct_callers.py <dsun> 184+033E 183+1524 184+209D 184+0000`;
`trampoline_target.py <dsun> 56CC:00BB 56DD:0052 56DD:005C 56DD:0061`. Read the 48 signed bytes at
file `0x43816`, the 24 bytes at file `0x43846` as eight word and signed byte pairs, and the six
words at file `0x6B7A1`.
