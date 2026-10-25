---
id: FND-MAGIC-018
title: The cast routine overlay 193 +003C hands the hit routine the address of the DATA dice total and the maximum; overlay 197 +0000 turns them into the damage, taking the maximum for a target at combat_mark 3 or with effect block bit 0x20 of byte 8, and the hit routine scales it by the resistance percentage and deals it through +2A05; the level also sets the effect's duration with no cap of 10, and the target list never sees it
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E725..0x0007E7D8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006500F..0x000654BC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00083400..0x00083606
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00080171..0x00080278
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D074..0x0008D0E2
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Capstone 5.0.7; Python 3.14.7
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x7E2F0 + x` for overlay 193, `0x642E0 + x` for 179,
`0x83400 + x` for 197 and `0x8BDC0 + x` for 204. `data_dice` is overlay 193 `+1F88(n, L, p)`
(FND-RNG-035, RULE-MAGIC-005); `direct_callers.py <dsun> 193+1F88` finds only the near calls at
`+04B0` and `+1431`. The hit routine is overlay 179 `+0D2F`; its thirteen argument words, from
the pushes at `+04A5..+04DE`, are: `bp+6` the target, `bp+8` the second slot (the caster), `bp+0xA`
the `DATA` number `n`, `bp+0xC` the word `m` at `+0x1A` of `DATA n`, `bp+0xE` a far pointer to the
total, `bp+0x12` the maximum, `bp+0x14` and `bp+0x16` 0, `bp+0x18` the list word, `bp+0x1A` 0x63,
`bp+0x1C` the level byte `L` and `bp+0x1E` the kind byte `g` (FND-RNG-051).

**Overlay 193 `+003C`.** It zeroes its word at `bp-0x16` on entry (`+004B`). With no single
target (`bp+8` -1) it fills its target list through overlay 208 `+0000` with the caster, `n`, the
position words `bp+0xE` and `bp+0x10`, the result of overlay 208 `+0B8C` of `n` and `g`, the list
and 0 (`+0435..+045A`); its level byte `bp+0xC` is not among the pushes. For each list entry it
calls `+1F88` with `n`, `L` and the address of `bp-0x16`, and stores the result at `bp-0x14`
(`+04A5..+04B6`). It then calls the hit routine with the address of `bp-0x14` as the total
pointer and the word `bp-0x16` as the maximum (`+04B9..+04DE`) and ORs the hit's result into
`bp-0x22` (`+04E6..+04E8`). It reads neither word again.

**The hit routine, overlay 179 `+0D2F`** (`+0D2F..+11DC`), uses the pair as follows.

1. When the target's state byte (`4F49:0C33 + 3 * t`) is 1, it calls overlay 173 `+22C4` with the
   target and the total when the total is above 0, sets bit 0 of its result byte `bp-7`, and
   returns (`+0D84..+0DA8`). FND-PARTY-086 reads `+22C4` as acting only on state 2.
2. It stores in `bp-0xC` the result of overlay 193 `+1E81` of `n` and `L`, or -9999 while
   `DS:6174` is not 0 (`+0E32..+0E4D`). `+1E81` raises `L` to 1 when it is 0 (`+1E95..+1E9B`) and
   has no upper cap; it returns -9999 for a record whose word `+7` is `0xD8F1`, and otherwise
   `word(+7) * ((word(+5) * (L + F)) / E + overlay 173 +2A5D(low nibble of +4, high nibble of +4,
   0))`, `F` and `E` the same fields as in `data_dice`, capped at `0x7FFF` (`+1EC9..+1F7D`).
3. It passes the total's value (not the maximum) to overlay 197 `+1760` as that routine's
   `bp+0x16` (`+0E77..+0E9E`). `+1760` reads `bp+0x16` once, passing it to overlay 198 `+0000`
   on its path for `n` of -1, a weapon hit (`+1932..+1965`).
4. Overlay 197 `+111A` writes the percentage dword `bp-6` (FND-PARTY-112). When the saving throw
   `+2A46` of the target, the caster, `n` and `L` succeeds, `bp-6` becomes 0 when word `+0x11` of
   `DATA n` has bit 0x8000 and is halved otherwise (`+0ED4..+0F1C`, FND-PARTY-166).
5. It calls overlay 197 `+0000` with the total, the maximum, the target, the caster, the target's
   effect block, `m`, `n` and the save result byte `bp-1`, and stores the result through the
   total pointer (`+0F42..+0F67`). Then it stores `total * bp-6 / 100`, the product taken as an
   unsigned 32-bit number, through the pointer (`+0F6A..+0F8A`).
6. When that is above 0 and `n` is not -1, it goes to `+1067`, sets bit 0 of `bp-7` and calls
   `+2A05` with the target, `n`, `m` and the amount (`+0F8D..+0F9B`, `+1067..+107B`); FND-RNG-034
   reads `+2A05` as the damage step that ends in overlay 173 `+22C4`. Then it calls the effect
   switch `+1373` with `L` as `a` (`+1115..+1134`, FND-RNG-044).

**Overlay 197 `+0000`** (`+0000..+0206`; one direct caller, `+0F5C`) takes `d` = the total and:

- uses the maximum instead when bit 0x20 of the target block's byte 8 is set or the target's
  `combat_mark` (combatant byte `+0x14`) is 3 (`+004A..+005E`);
- doubles `d` when `n` is 0x79 and the target's details byte `+0x12` is 9 (`+0061..+0078`);
- takes 2 off when `m` has a bit of 0x38 and bit 1 of the block's byte 1 is set
  (`+007A..+008B`);
- when `d` is above 0 and the first row `3143:0061` finds for the caster and code 0x48 belongs to
  the caster with an index below `DS:2100`, adds 1 when the caster's combatant byte `+0x15` equals
  that byte of the row's source slot (row word `+2`) and subtracts 1 otherwise (`+008E..+00F9`);
- adds overlay 173 `+2C00` of the target, the target and 1 when `n` is 0xED (`+00FA..+010D`);
- when the save byte is 0 and `m` has bit 0x800, replaces `d` with the target's hit points
  (combatant word `+0`) plus 10 (`+010F..+0125`);
- when bit 0x40 of the block's byte 0x0C is set and `3143:00A8` finds a row of code 0x66 for the
  target whose signed byte `+9` is above the target's hit points, doubles `d` (`+0128..+0178`);
- lowers `d` to 1 when it is above 1 and either the target's combatant byte `+0x0F` is 2 and
  `2D40:35EF` of the caster and the target returns above 1, or the byte is 0x0C and `2D40:35EF`
  returns 1 or less (`+017A..+01F7`);
- returns `d`, 0 when negative (`+01FA..+0200`).

**The rest's cures.** Overlay 204 `+12B4`, which casts the rest's cures (FND-MAGIC-005), calls
overlay 179 `+0C7F` with a total pointer to its local word `bp-2`, set to 0 at `+12BE`, a maximum
of 0 and an `m` of 0, and makes no call of `+1F88` (`+12B4..+1322`).

## Interpretation

The `DATA` dice total is the damage of a cast. For each target the cast routine rolls it at the
cast level (capped at 10 inside `data_dice`), and the hit routine passes it through overlay 197
`+0000`, scales it by the target's resistance percentage, which a successful save halves or
zeroes, and deals what is left as damage of type `m`. The maximum is used in one place: a target
already at 0 hit points (`combat_mark` 3, FND-PARTY-086) or with bit 0x20 of effect block byte 8
takes the most the dice could give, with no roll. The same pair of conditions makes the saving
throw fail (FND-PARTY-099, FND-RNG-035), so such a target takes full maximum damage.

A record with bit 0x800 in `m` kills a target that fails its save whatever the dice give: the
amount becomes hit points plus 10, which `+22C4` turns into `combat_mark` 7 (FND-PARTY-086).

The level `L` of a cast changes, for each target:

- the damage dice, through `k = (min(L, 10) + F) / E` (RULE-MAGIC-005);
- the effect's duration, through `+1E81`, linearly in `L` with no cap of 10;
- the saving throw, where overlay 197 `+0206` reads `L` for `DATA` 60 (FND-PARTY-164);
- the cases of the effect switch that read `a` (FND-RNG-044).

It does not change which or how many targets a cast reaches: `+003C` builds its list from `n`,
the position and `g` only. The rest's cures roll no dice and deal no damage; their level reaches
only the duration, the save and the effect switch.

## Alternatives

- What sets bit 0x20 of effect block byte 8, bit 1 of byte 1 and bit 0x40 of byte 0x0C, what
  effects 0x48 and 0x66 and `2D40:35EF` stand for, and which spells are `DATA` 0x79 and 0xED,
  were not read; Q-MAGIC-008 holds the block bits and `2D40:35EF`.
- That `bp-6` is a percentage rests on FND-PARTY-112's reading of overlay 197 `+1C52` (100, 50
  or 0); its other writers in `+111A` were not reread here.
- When `+1F88` returns early (an all-zero record) it stores nothing, so the maximum passed is
  the 0 from `+004B`; the reading does not depend on a value left from an earlier target, since
  every target of one cast has the same `n`.
- No emulated call or run was made.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python and `<dsun>` the installed
`DSUN.EXE`: `overlay_listing.py <dsun> 193 7E2F0 80ECE`, `... 179 642E0 66F95`, `... 197 83400
86F67` and `... 204 8D074 8D0E2`, and read the ranges above; `direct_callers.py <dsun> 193+1F88
193+003C 179+0D2F 197+0000 193+1E81 197+1760`. Map each call's pushes to the callee's `bp`
offsets.
