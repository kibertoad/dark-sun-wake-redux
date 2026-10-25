---
id: FND-MAGIC-024
title: A rest cure reaches a member through the hit routine's gates unless the member has one of effects 4, 11, 46, 55, 74 or 92, or effect 56 or 85 against another member's cure, or a type, special attack or resistance no shipped character has; the level decides none of these and only rides along when the cure is queued back on its caster; the rest sets DS:0DAB to 5, so after the switch only the sound of DATA byte +0x16 plays
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D074..0x0008D0E2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00064F5F..0x0006500F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006500F..0x000651AF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000653F5..0x000654BC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066A2C..0x00066A32
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008428F..0x00084502
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00084502..0x0008451A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00084B60..0x00084E94
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008451A..0x00084AB5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00085052..0x000851C6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00086A93..0x00086BA5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00092115..0x000921C3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066EB7..0x00066EFA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008F9C3..0x0008FA71
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008FA71..0x0008FB9F
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00014B1C..0x00014B65
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00014DAD..0x00014DF6
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00014E3F..0x00014E88
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x000153AA..0x000153F3
tool: tools/research/exec-census/overlay_listing.py, direct_callers.py, trampoline_target.py, data_bytes.py and gff_tag_numbers.py at the commit that adds this finding; Capstone 5.0.7; Python 3.14.7; a short read of DATA records, described under How to reproduce
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x8BDC0 + x` for overlay 204, `0x642E0 + x` for 179,
`0x83400 + x` for 197, `0x91890 + x` for 208 and `0x8E950 + x` for 206. `R` and `D` are the
target's combatant and details records. Effect code `k` is bit `1 << (k & 7)` of byte `k / 8` of
the 16-byte block overlay 193 `+1BA3` fills for a slot (FND-MAGIC-021). "Spell level" is overlay
177 `+09C5` of the number, 3 for 148, 157 and 159 and 4 for 178 (FND-MAGIC-004).

**The arguments.** Overlay 204 `+12B4` (`+12B4..+1322`) reads the word `DS:0DAB` through
`28C9:2522`, sets it to 5 through `28C9:252A`, calls overlay 179 `+0C7F`, and sets it back
(`+12C3..+131E`; FND-CONFIG-117). Its pushes give `+0C7F`, by stack offset: `+6` the target, `+8`
the caster, `+0A` the number, `+0C` 0, `+0E` a far pointer to a local word 0, `+12` 0, `+14` 0,
`+16` 0, `+18` 8, `+1A` 0x63, `+1C` the level `L` and `+1E` the caster's combatant byte `+0x15`
(`+12EB..+130E`). `+0C7F` (`+0C7F..+0D2F`) removes effects 0x35 and 0x36 from the caster through
overlay 193 `+1AA4` when the low two bits of byte 0 of `DATA n` are 0 or 2, or otherwise when the
word at `+0E` (the pointer's offset) is above 0 (`+0C87..+0CF0`); byte 0 is 5 for 148, 157 and
178 and 17 for 159, and the offset is that of a stack local, so the rest takes the second way. It
then passes all its
arguments unchanged to `+0D2F` (`+0CF3..+0D1D`). In `+0D2F` the word at `+0C` is the mask `m`
that it passes to `+1760` and `+111A`, and the byte at `+14` is the byte it passes to `+0E8F`
(`+0E57..+0EC0`).

**The four records.** A read of `DATA` 148, 157, 159 and 178 gives, besides FND-MAGIC-021's table:
byte `+0x0C` 3, 5, 3, 3; word `+0x1A` 0; byte `+0x17` -1; word `+1` -2, -2, 0, -2; byte `+0x16`
9, 9, 59, 59; word `+0x14` 8307 for all four.

**Overlay 208 `+0885(n, k)`** (`+0885..+0933`) copies `DATA n` and returns 8 when its byte `+0x0C`
is 3, 4, 5, 7 or 8 (`+08BF..+08E0`). It returns 8 for all four cures.

**`+0D2F` before the gates** (`+0D2F..+0E57`) ends the hit when the word `DS:2588 + 2 * R[+0x0F]`
is -1, when `n` is 0x134, when the party state byte `4F49:0C33 + 3 * t` is 1 (after passing a
total above 0 to overlay 173 `+22C4`), and when word `+0x11` of `DATA n` has bit 0x200 and the
word `4C10:0019` is 0 or 1. None reads `L`, and bit 0x200 is clear in all four records.

**Overlay 197 `+0E8F(t, c, block, n, a, w)`** (`+0E8F..+1102`) with `a` 0 and `w` 0 returns 1, which
ends the hit, when `2D40:3E64` does not find `t`; when the type word is -1; when overlay 208
`+0F07` returns 0, which it never does for these numbers (FND-MAGIC-023); for effect 4 and `n`
below 235; for effect 56 when `t` is not `c` and `n` is none of 26, 150, 72, 236; for effect 92 and
`n` below 235, the value of `+3693(t, n)`; for effect 11 when the word `4C4F:00DC + 4 * j` is -1,
`j` being the combatant number `2D40:3E64` returns, since `+0885` is not above 8; and for effect 55
when the spell level is below the signed byte `+9` of the effect's row (`4E71:0256 + 10 * e`, `e`
from `3143:00A8(t, 0x37)`). With `a` 0 the `+314E` test and the effect 46 queue at `+0F65..+0F92`
are skipped; bits 5 to 7 of byte `+0x1F` are 5 or 0, not 4; and none of the four numbers is among
the six words at `+1102` (8, 18, 53, 121, 169, 277). Otherwise it returns 0.

**Overlay 197 `+3693(t, n)`** (`+3693..+37A5`) walks `t`'s item list through `2D40:2196` and, for
each entry of kind 1 that overlay 173 `+2DD0` gives 0x10 and whose item word `+0x10` less 1 is
0x138 (312), with the item's byte `+0x0E` above 0, adds the spell level to its byte `+0x0F`, keeps
it at most byte `+0x0E`, and returns 1; otherwise 0.

**Overlay 197 `+1760`** (`+1760..+1A94`) starts with a result of 1 and, for these arguments:

- removes effect 5 from `t` when `t` has it, and for `c` below 48 and effect 36 on `t` queues `c`,
  `t`, 124, 6 and removes effect 36 (`+1777..+17B3`); the hit goes on;
- for effect 46 on `t`, rolls 1d100 and on 75 or less removes effect 46, queues `t`, `t`, 307, 6 and
  returns 0 (`+17E0..+1819`);
- for `c` below 48, `c` not `t`, effect 85 on `t`, a spell level of 1 to 3 and byte `+0x17` -1,
  removes effect 85 from `t`, queues `c`, `t`, `n`, `L` through overlay 179 `+12B1` and returns 0
  (`+1859..+18D3`, `+180F..+1819`); effect 84 needs bit 0x80 of word `+0x11`, which is 0;
- returns 1 when `2D40:3E64` does not find `c` or `2D40:35EF(c, t)` is above 1; the test of block
  byte 9 bit 0x80 needs bit 0x100 of `m`, which is 0; the last step deals 1,000 to `t` only for
  details byte `+0x12` 14 (`+18D6..+1A94`).

**Overlay 197 `+111A(t, block, n, m, p, L)`** (`+111A..+16B5`) with `m` 0 returns 1 when the type
word is -1; when `R` byte `+0x0E` is 13 and, with `DATA n` byte `+0x17` -1 and word `+1` above -2
(only 159), a 1d6 is not 6, or is 6 after queueing `DS:426D`, `t`, `n`, `L` (`+11AC..+120A`); when
`+1C52(t, block, 0)` is 0; for effect 92; when overlay 179 `+2BD7(t, n, L)` returns nonzero; for
effect 55 under the same row byte test as `+0E8F`; when `R` byte `+0x0E` is 9 and
`211 +1FB5(DS:426D, n)` is below 7; and when `R` byte `+0x0E` is not 0 and overlay 198
`+0149(byte, k)` is `n` for `k` 0, 1 or 4. The tests on `m`, on bits 0x26, 8 and 4 of word `+0x11`,
on byte `+0x19` 0x45 or 0x5B, on details byte `+0x12` 9, 0x0B or 0x0E, and the tail for `+0885`
above 8 or `n` 0xFC or 0x120 do not apply. Otherwise it returns 0.

**Overlay 197 `+1C52(t, block, m)`** (`+1C52..+1DC6`) with `m` 0 returns 100, except 0 when the
block has effect 74 (byte 9 bit 4, the test at `+1D92..+1DA3` for an `m` with no bit of 0xFD79).

**Overlay 179 `+2BD7(t, n, L)`** (`+2BD7..+2C1A`) returns 1 when 1d100 is below overlay 197
`+16B5(t, n, L)`, the resistance in details byte `+0x23` adjusted as FND-PARTY-150 reads; `L`
enters only for `n` 0x40.

**After the switch** (`+1115..+11DC`). Case 159 and the effect code call for 148 and 178 set flag
0x02 of the hit's byte `bp-7` (`+274C..+2752`; FND-MAGIC-021); 157 sets it only when it raises
Strength. With 0x02 set and 0x01 clear, or with `R` word `+0` 0, the routine calls `2C5F:0A22`
with byte `+0x16` of `DATA n` (`+1168..+117D`). It then calls overlay 206 `+1073` with word `+0x14`
of `DATA n` (or a table word, or -1 when neither is set, `+1095..+1110`) and the target's map
position `DS:67C5 + 0x25 * t`, `DS:67C7 + 0x25 * t` only when `DS:0DAB` is 1, that word is not -1
and bit 0x80 of `DS:67BB + 0x25 * t` is clear; and when `DS:0DAB` is not 1, overlay 206 `+1121`
with the target unless `DS:0DAB` is 5 (`+117E..+11D4`). `2C5F:0A22` plays sound effect `n` (FND-SOUND-007);
`gff_tag_numbers.py <install> BVOC` lists `BVOC` 9 and 59. Overlay 206 `+1073` places object
number `w` at a point and draws it, and `+1121` shows frame 4 of `BMP` 19001 over member box `n`
for 40 ticks (FND-EXE-602; listed again here).

## Interpretation

During a rest a cure reaches a member, and the switch runs, unless one of these holds for that
member:

- it has effect 4, 92 or 74, or effect 11 while its word at `4C4F:00DC` is -1, or effect 55 whose
  row byte is above the spell's level (3, or 4 for Neutralize Poison);
- it has effect 46 and loses a 75 in 100 roll; it then loses effect 46 and gets `DATA` 307 queued on
  itself;
- the cure comes from another member and the target has effect 56, or effect 85 for Cure Disease,
  Remove Curse or Remove Paralysis; with 85 the target loses it and the cure is queued back at the
  caster with the rest's level;
- its immunity type is 16, its special attack byte is 13 (and the cure is Remove Paralysis), 9, or
  one whose `MONS` record is the cure's number, or its resistance roll succeeds. No shipped `CHAR`
  record has a type, special attack or resistance other than 0 (FMT-PARTY-001, FND-PARTY-150).

With effect 92, a worn item of power 312 with capacity left absorbs the cure into its byte
`+0x0F` in `+0E8F`; without one, `+111A` stops the cure for the same effect.

The level `L` decides none of the gates. The tests that use a level use the spell's fixed level from
overlay 177 `+09C5`; `+2BD7` passes `L` on but uses it only against number 0x40. `L` matters only
as the level of a cure queued back on its caster by effect 85, or queued by the special attack 13
path, and the cures' effect switch cases do not read it (FND-MAGIC-021).

After a cure that changed nothing visible the hit still plays sound effect 9 (Cure Disease, Remove
Curse) or 59 (Remove Paralysis, Neutralize Poison), subject to the sound settings, for every member
reached by 148, 159 or 178 and for a member whose Strength 157 raised. Because the rest holds
`DS:0DAB` at 5 for the call, neither overlay 206 `+1073` nor `+1121` runs: no object is placed and
no frame of `BMP` 19001 is shown.

## Alternatives

- What effects 4, 11, 46, 55, 56, 74, 85 and 92 are in play, and how a member gets them, was not
  read; FND-MAGIC-020 reads 84 and 85 as turning a spell back.
- `3143:00A8` is taken to return the row of an effect the slot has and -1 otherwise, as the tests'
  pairing with the block bits implies; its body was not listed.
- What `4C4F:00DC + 4 * j` holds outside a fight was not read; FND-COMBAT-047 reads stores of -1 to
  it in the guard and wait handling.
- Whether the cast queue runs a queued cure outside a fight, and so whether a turned-back cure takes
  effect during a rest, was not read.
- A script can set a member's immunity type and special attack (fields 18 and 19 of script
  functions 48 and 49, FMT-PARTY-001); with those, the type, 13 and 9 tests apply.
- The overlays are outside the resident harness; no emulated call or run was made.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python, `<dsun>` the installed `DSUN.EXE` and
`<install>` its folder: `overlay_listing.py <dsun> 204 8D074 8D0E2`, `179 64F5F 651B0`,
`179 651AF 653F5`, `179 653F5 654BC`, `179 66A2C 66A40`, `179 66EB7 66F95`, `197 8428F 8451A`,
`197 8451A 847C0`, `197 847B8 84AB5`, `197 84B60 84E94`, `197 85052 851C6`, `197 86A93 86BA5`,
`208 92115 921C3`, `206 8F9C3 8FB60` and `206 8FB5A 8FBA7`; `trampoline_target.py <dsun>
5799:0043 5799:004D`; `data_bytes.py <install>/RESOURCE.GFF C` and `17`; `gff_tag_numbers.py
<install> BVOC`. Read the 12 words at file `0x84502`. The record read takes the `DATA` directory as
`data_bytes.py` does and prints, for 148, 157, 159 and 178, the bytes `+0`, `+0x0C`, `+0x16`,
`+0x17`, `+0x19`, `+0x1F` and the words `+1`, `+9`, `+0x11`, `+0x14` and `+0x1A`.
