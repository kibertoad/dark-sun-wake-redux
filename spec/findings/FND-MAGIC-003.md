---
id: FND-MAGIC-003
title: The cast window reaches only spell levels up to overlay 211 +208C, a halving of the greatest qualifying class level that takes 7 off a ranger's level, so a ranger casts level 4 spells at levels 14 and 15 and a Preserver at 11, 13 and 15 holds a count one level above the window; the cast itself passes the full class level from overlay 177 +07AF
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00097D1F..0x00097D89
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00097DC5..0x00097F00
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000628AF..0x00062A10
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062592..0x0006260A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00080580..0x00080617
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00097446..0x00097483
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00097528..0x00097551
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005A1F1..0x0005A27A
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Capstone 5.0.9; Python 3.14.7
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x59430 + x` for overlay 172, `0x62100 + x` for 177,
`0x7E2F0 + x` for 193 and `0x95E10 + x` for 211. Records and class codes are as in FND-MAGIC-002.

**The level a slot reaches.** Overlay 211 `+1FB5` takes a slot and a number `n`
(`+1FB5..+208B`). For `n` below 0xEB it returns the greatest, over the class codes 0 to 19 that
overlay 177 `+0A45` accepts for `n`, of `27E5:00C1` of the slot and the code, less 7 for codes 13
to 16, and 0 when none is above 0 (`+1FBD..+1FFF`); for `n` below 0x10D, `27E5:00C1` for code 12;
for 0x132, the greatest over codes 1 to 4; otherwise the greatest level byte of the three
positions. `27E5:00C1` holds back a human's former class (FND-PARTY-258).

Overlay 211 `+208C` takes a slot and a kind (`+208C..+20EF`). For kind 1 it takes `x`, `+1FB5`
of the slot and spell 0, and returns `(x + 1) >> 1` for `x` below 10 and `x >> 1` from 10
(`+20A3..+20B8`). For kind 2 it takes `x`, `+1FB5` of the slot and spell 0x73, and returns
`(x + 1) >> 1` for `x` below 12, 7 for 12 to 19, `(x + 4) / 3` for 20 to 27 and 10 from 28
(`+20BA..+20EA`). The comparisons are unsigned on the low byte. Values for `x` from 1 to 20:

| `x` | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | 13 | 14 | 15 | 16 | 17 | 18 | 19 | 20 |
| --- | - | - | - | - | - | - | - | - | - | -- | -- | -- | -- | -- | -- | -- | -- | -- | -- | -- |
| kind 1 | 1 | 1 | 2 | 2 | 3 | 3 | 4 | 4 | 5 | 5 | 5 | 6 | 6 | 7 | 7 | 8 | 8 | 9 | 9 | 10 |
| kind 2 | 1 | 1 | 2 | 2 | 3 | 3 | 4 | 4 | 5 | 5 | 6 | 7 | 7 | 7 | 7 | 7 | 7 | 7 | 7 | 8 |

`direct_callers.py <dsun> 211+208C` finds the near call at `+1F63` and far calls from overlay 177
`+057F`, `+0CCE` and `+0E18`, overlay 197 `+32EA` and overlay 209 `+031F` and `+06C0`.

**The window.** Overlay 211 `+1F0F` takes a slot, a kind and a level. From the level after it up
to 22 it takes the first level for which overlay 177 `+0400` of the slot, a null pointer, the
kind and the level returns nonzero, or 0 when none does; unless `DS:13F8` is not 0 it lowers the
result to `+208C` of the slot and the kind (`+1F0F..+1F78`). Overlay 177 `+050A` admits a priest
spell (115 to 234) to the list only when its level from `+09C5` is not above `+208C` of the slot
and 2 (FND-PARTY-110).

**The level the cast carries.** At overlay 211 `+1645..+1673` the window pushes `+1FB5` of the
slot and the spell as the fourth argument of overlay 193 `+2290`, and at `+1718..+1741` as the
fifth argument of overlay 208 `+1612`; overlay 172 does the same at `+0DC1..+0DEC` (to `+2290`)
and `+0E2F..+0E4A` (to `208 +1612`). Overlay 193 `+2290` passes that argument (`bp+0xC`) on to
`+0000` only when its byte at `bp+0x1A` is not 0 or the number is from 0x10D to 0x147
(`+22A9..+22CF`). For a number below 0xEB with that byte 0 it calls overlay 177 `+0492` with the
caster, the target, the number and the rest, and no level (`+22D7..+22F1`). Overlay 177 `+0492`
then passes, as `+0000`'s fourth argument, overlay 177 `+07AF` of the caster and the number
(`+04D8..+04FE`). Overlay 211 `+166E` and overlay 172 `+0DE7` push 0 as the byte at `bp+0x1A`
(FND-RNG-055).

Overlay 177 `+07AF` takes a slot and a number `n` (`+07AF..+090F`). Over the positions 0 to 2 it
passes over a class byte of 0 and a held-back former class of a human, as `+05B7` does, and
returns the greatest level byte among the positions whose class `+0A45` accepts for `n`, with no
subtraction for any code.

## Interpretation

For kind 2 the level that sets the window is the greatest of the slot's cleric and druid levels
and its ranger level less 7, so a ranger reaches spell level 1 at level 8, 2 at 10, 3 at 12 and 4
at 14. With FND-MAGIC-002's ranger counts (one fourth-level spell at 14, two at 15), a ranger of
level 14 or 15 can cast fourth-level priest spells, which the manual's ranger table never gives.
That subtraction is all the ranger casting level changes in the cast window: the cast itself
reaches overlay 193 `+0000` with the full ranger level from `+07AF`, and the value with 7 off
that `+1FB5` hands to `+2290` is dropped for every spell below 0xEB cast from the window.

The window is the only gate on spell level: the pools hold counts at levels the window never
offers. A cleric or druid's Wisdom counts at levels above `(x + 1) >> 1` stay unspent, so the
Wisdom term gives no access to a spell level the class level does not reach. A Preserver at
level 11, 13 or 15 has one spell of level 6, 7 or 8 counted while the window stops at 5, 6 or 7,
and a cleric or druid at 15 has a level 8 count above the window's 7. A cleric or druid at level
12 reaches spell level 7 in the window with a level 7 count of 0 unless byte `+0x1D` is 25.

## Alternatives

- Whether overlay 208's byte at `DS:9BD3`, the `bp+0x1A` byte of its `+179F` call (FND-RNG-055),
  can be nonzero for a party spell, which would pass `+1FB5`'s value instead, was not read.
- What overlay 193 `+003C` does with the level byte, and so whether a ranger's spells grow with
  the full ranger level, is Q-MAGIC-006.
- Overlay 193 `+1A79` sums `+1FB5` over a list at `4E71:024D`, and overlay 197 `+14DA` compares
  `+1FB5` with 7 for a combatant whose byte `+0x0E` is 9 and a number of 0x73 or more; neither
  use was read.
- The level the window starts from (`DS:43E0`, `DS:43E4`) and the wrap at the top are
  FND-PARTY-104's; whether `+1F0F`'s lowering to the limit can leave the window on a level whose
  list is empty was not followed.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python and `<dsun>` the installed
`DSUN.EXE`: `overlay_listing.py <dsun> 211 97D1F 97F00`, `... 211 97440 97490`, `... 211 974F8
97560`, `... 177 626B7 62AC5`, `... 177 62592 6260A`, `... 193 80580 80618`, `... 172 5A1D0
5A290`, `... 193 7FD4A 7FDA0` and `... 197 848A0 84900`; `direct_callers.py <dsun> 211+1FB5
211+208C`. Evaluate `+208C` for `x` from 0 to 30 and both kinds.
