---
id: FND-MAGIC-021
title: The effect switch has no case for DATA 148 or 178 and reads no level for 157 or 159; the hit routine removes every effect of code 27 for 148 and of code 71 for 178 through a negative effect code, case 157 raises current Strength to base Strength when it is below, and case 159 removes effects 91, 70, 65 to 68 and, unless the target has effect 71, 69; nothing changes a member that has none of them
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000656D0..0x000656F1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00066BA1..0x00066CE5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000661A0..0x000661FF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065817..0x0006581D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066311..0x00066332
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066A2C..0x00066B11
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00066B11..0x00066B25
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000651C8..0x00065222
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065360..0x000654BC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00064F5F..0x0006500F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F95F..0x0007F9E6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FC3F..0x0007FD1D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FE93..0x0007FF07
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00080171..0x00080278
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D074..0x0008D0E2
tool: tools/research/exec-census/overlay_listing.py and data_bytes.py at the commit that adds this finding; Capstone 5.0.7; Python 3.14.7; a short read of the 81 key words at file 0x66BA1, the 81 target words at 0x66C43 and the ten words at 0x66B11
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x642E0 + x` for overlay 179, `0x7E2F0 + x` for 193 and
`0x8BDC0 + x` for 204. The hit routine is overlay 179 `+0D2F`, the effect switch `+1373`
(FND-RNG-044), `R` and `D` the target's combatant and details records, and `a` the switch's byte
argument at `bp+0x14`, which `+0D2F` pushes from its level byte `L` at `bp+0x1C`
(`+1115..+1134`, FND-MAGIC-018).

**The rest's call.** Overlay 204 `+12B4` (`+12B4..+1322`) calls overlay 179 `+0C7F` with, from
the pushes at `+12EB..+130E`: the target its argument `bp+8`, the caster its argument `bp+0xA`,
`DATA` number its argument `bp+6`, a total pointer to its local word (0), a maximum of 0, a list
word of 8, 0x63, its byte argument `bp+0xC` as `L`, and the caster's combatant byte `+0x15` as `g`.
`+0C7F` (`+0C7F..+0D2F`) passes its arguments unchanged to `+0D2F` (`+0CF3..+0D1D`).

**The switch's table.** The 81 keys at `cs:28C1` (file `0x66BA1`) hold 157 (target `+2031`) and
159 (target `+1EC0`, shared with 20 and 175). They hold neither 148 nor 178, so for those two the
search at `+13F0..+1409` ends at the jump to `+2752`. Nothing before the search (`+1373..+13F0`)
reads `a`; it only finds `R` and `D` and clears three locals, among them the flag byte `bp-0x13`.

**Case 157** (`+2031..+2052`): when `R` byte `+0x19` is below `D` byte `+0x15` (unsigned), it stores
`D` byte `+0x15` in `R` byte `+0x19` and jumps to `+274C`; otherwise it jumps to `+2752`.

**Case 159** (`+1EC0..+1F1F`): it calls overlay 193 `+194F` with the target and, in order, codes
0x5B, 0x45, 0x46, 0x41, 0x42, 0x44 and 0x43. The call with 0x45 is skipped when bit 0x80 of byte
8 of the block at `bp+0xA` is set (`+1ECB..+1ED9`). That block is `+0D2F`'s local `bp-0x20`,
which overlay 193 `+1BA3` fills for the target at `+0E04..+0E0E`. `+1BA3` (`+1BA3..+1C17`) clears
15 bytes and, for each effect row of the slot, sets bit `1 << (code & 7)` of byte `code / 8`
(`+1BD0..+1BFB`), so byte 8 bit 0x80 is code 71. The case then goes through `+1537` to `+274C`.

**The tail** (`+274C..+2831`). `+274C` sets bit 2 of the byte the flags pointer `bp+0x12` names.
`+2752` goes to `+27E7` when the save byte `s` (`bp+0x10`) is not 0; otherwise it looks `n` up in
the five words at `cs:2831` (3, 62, 204, 236, 280; targets at `cs:283B`), none of which is 148, 157,
159 or 178, and goes to `+27E7`. `+27E7..+282D` calls overlay 197 `+1E7B` for the target and for the
second slot, and `+23AA` for each, only when bits 1, 2, 8 and 4 of `bp-0x13` are set; nothing on the
paths of the four numbers sets them. No instruction from `+274C` to the `retf` at `+2830` reads
`bp+0x14`.

**The hit routine's effect code.** Before the switch, `+0D2F` keeps the signed byte `+0x19` of
`DATA n` in `bp-0xA` (`+0E1B..+0E32`). When the save result `bp-1` is 0 and `bp-0xA` is not 0, it
calls overlay 193 `+166F` with the target, the second slot, `n`, the code byte, the duration from
overlay 193 `+1E81(n, L)` and 0, then sets bit 2 of its result byte `bp-7` (`+0EE8..+0F42`).
`+166F` (`+166F..+16F6`) returns at once for code 0, and for a negative code calls `+194F` with the
target and the negated code, using neither the duration nor `L` (`+167A..+1697`).

**Overlay 193 `+194F(t, k)`** (`+194F..+1A2D`) walks the effect rows at `4E71:024D` (10 bytes each,
count `DS:2100`) from the index `3143:0061` returns for `t` and `k`, while a row's slot word is
`t`. For each row whose code byte (`+6`) is `k` it calls overlay 195 `+1000` with `t` and `k`,
moves the later rows down one, lowers `DS:2100`, calls `371D:0002` with `t` and 7, and starts the
walk again (`+1972..+1A19`). When it removed at least one row it calls overlay 197 `+1E7B(t)`
(`+1A1C..+1A29`). A target with no row of code `k` is left unchanged.

**The four records.** `data_bytes.py <resource.gff> <offset>` gives, for `DATA` 148, 157, 159 and
178:

| `DATA` | `+0x04` | `+0x05` | word `+0x07` | `+0x11`, `+0x12` | `+0x19` | `+0x1F` |
|---|---|---|---|---|---|---|
| 148 | 16 | 0 | 60 | 0, 0 | -27 | -96 (0xA0) |
| 157 | 16 | 0 | -9999 (0xD8F1) | 0, 0 | 0 | -96 (0xA0) |
| 159 | 16 | 0 | -9999 (0xD8F1) | 0, 0 | 0 | 0 |
| 178 | 16 | 0 | 60 | 0, 0 | -71 | -96 (0xA0) |

Bit 0 of byte `+0x1F` is clear in all four, so the saving throw `+2A46` returns 0 at its first
test, before it reads `L` (FND-PARTY-099 step 1): `s` and `bp-1` are 0 for every target. Bit
0x200 of word `+0x11` is clear, so the test at `+0DB3..+0DC6` does not end the routine. `+1E81`
(`+1E81..+1F88`) returns -9999 for 157 and 159; for 148 and 178 it returns `60 *` overlay 173
`+2A5D(0, 1, 0)`, since its level term is word `+5` (0) times `L + F`.

**After the switch.** With a total of 0, `+0F8D` goes to `+1080` and, `n` not being -1, through
`+1095..+1110` to the switch call (`+1115..+1134`). Back from it, `+113A..+1168` goes to
`+1168` when bit 2 of `bp-7` is set and bit 0 clear, or when word `+0` of `R` is 0, and returns
otherwise. `+1168..+11D5` calls `2C5F:0A22` with byte `+0x16` of `DATA n`, then overlay 206
`+1073` (when `DS:0DAB` is 1 and other tests pass) or overlay 206 `+1121` (unless `DS:0DAB` is 5).

## Interpretation

None of the four cures reads its level in the effect switch. 148 and 178 have no case at all, and
the cases of 157 and 159 never read `a`. In the rest of the hit routine, `L` reaches the duration,
which a removal ignores, and the saving throw, which the four records turn off; so within what
was read, a cure at level 5 and one at level 15 do the same thing.

What each cure changes on its target:

- 148 (Cure Disease) removes every effect of code 27.
- 178 (Neutralize Poison) removes every effect of code 71.
- 157 (Remove Curse) sets current Strength (combatant byte `+0x19`, FND-MAGIC-013) to base
  Strength (details byte `+0x15`) when it is lower. It removes no effect and does not rebuild the
  scores, so the next rebuild by overlay 197 `+1E7B` lowers Strength again while an effect that
  lowers it is still in place.
- 159 (Remove Paralysis) removes every effect of codes 91, 70, 65, 66, 67 and 68, and of 69 unless
  the target has an effect of code 71 at the time of the cast.

A member with none of those effects and with Strength not below its base keeps all its records
as they were. A removal also runs overlay 195 `+1000`, `371D:0002` and the score rebuild for that
member, which a member without the effect does not get.

The effects the hit routine runs after the switch are presentation: for 148, 159 and 178 bit 2 of
`bp-7` is set for every target, so `+1168` runs for every member whether or not it had the
condition; for 157 only when Strength was raised. What `2C5F:0A22` and overlay 206 `+1073` and
`+1121` show or play was not read.

## Alternatives

- The gates of `+0D2F` before the switch (the table at `DS:2588` indexed by combatant byte
  `+0x0F`, overlay 197 `+0E8F`, `+1760` and `+111A`, FND-RNG-040) can end the hit routine before
  the switch for some targets; `+1760` and `+111A` receive `L`. They were not read here, so a
  target they turn away gets no cure at all, and whether `L` changes that is open.
- That codes 27 and 71 are disease and poison, and 65 to 70 and 91 forms of paralysis, rests on
  the spell names of RULE-MAGIC-002's table only. Codes 27 and 71 are among the codes `DATA` 26
  and 150 never remove (FND-RNG-044), and code 71 is the bit the rest tests for its 1,000 damage
  (FND-MAGIC-005).
- What overlay 195 `+1000` and `371D:0002` do when a row is removed was not read.
- `3143:0061` is taken to return the first row of the slot, as the walk's loop on the slot word
  implies; its body was not listed here.
- The overlay is outside the resident harness; no emulated call or run was made.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python and `<dsun>` the installed `DSUN.EXE`:
`overlay_listing.py <dsun> 179 642E0 66F95` and read `+0C7F..+0D2F`, `+0D2F..+11DC`,
`+1373..+1411`, `+1537..+153D`, `+1EC0..+1F1F`, `+2031..+2052` and `+274C..+2831`; read the 81 keys
at file `0x66BA1` and the 81 targets at `0x66C43`, and the ten words at `0x66B11`, as little-endian
words. `overlay_listing.py <dsun> 193 7E2F0 80B00` and read `+166F..+16F6`, `+194F..+1A2D`,
`+1BA3..+1C17` and `+1E81..+1F88`. `overlay_listing.py <dsun> 204 8D074 8D0E3`. With
`<resource.gff>` the installed `RESOURCE.GFF`, `data_bytes.py <resource.gff> OFF` for `OFF` 4, 5, 7,
8, 11, 12, 19 and 1F, keeping `DATA` 148, 157, 159 and 178. Nothing was executed.
