---
id: FND-COMBAT-071
title: The computer's target choice skips combatants at combat_mark 3 or above, a combatant at mark 3 gets no turn and a non-party one is unloaded from the map when the removal list empties; it leaves mark 3 by a blow of 10 or more, by overlay 197's three heal routines, by the end of the fight or by a rest, and nothing reads the nine state names
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005A920..0x0005AA79
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005AA79..0x0005ABCA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005BBBA..0x0005BC21
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005C3ED..0x0005C5E1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B5EB..0x0005B60F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B8D9..0x0005B8EA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B9B8..0x0005BA75
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D98B..0x0005D9E7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005CC69..0x0005CD71
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005E5FA..0x0005E6DB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00086105..0x00086352
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000820F2..0x00082113
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00082196..0x00082208
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:11C1..2D40:11F3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:1397..2D40:1449
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:19ED..2D40:1A42
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:1AE5..2D40:1B16
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:1C36..2D40:1C69
tool: tools/research/exec-census/overlay_listing.py, resident_listing.py, direct_callers.py, record_base_accesses.py, field_stores.py and immediate_search.py at the commit that adds this finding; Capstone 5.0.7; Python 3.14.7
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 173's code starts at file `0x5A920`,
195's at `0x81770` and 197's at `0x83400`. Slots, state bytes, combatant and details records are
as FND-COMBAT-065 gives; `combat_mark` is combatant byte `+0x14`.

**The writers.** `record_base_accesses.py <dsun> <inventory> --source 19C9 14` lists 14 stores to
byte `+0x14` through `DS:19C9`, and the same tool without `--source` lists seven more through a
local pointer to a combatant record; `field_stores.py <dsun> <inventory> --es 14` finds no byte
store outside the inventory other than one inside another instruction. Overlay 173 `+2385`, in
the damage routine `+22C4`, is the only store of 3 (FND-PARTY-086). The others:

| Store | Routine | Value | When |
|---|---|---|---|
| overlay 173 `+2349`, `+235B` | `+22C4` | 8, 7 | a blow of at least the hit points plus 10, at any mark (FND-PARTY-086) |
| overlay 173 `+3D94` | `+3CDA`, end of the fight | 1 | party slots 0 to 3 at mark 1, 2 or 3; hit points below 1 become 1 (`+3D5D..+3D94`) |
| overlay 197 `+2E38` | `+2D05(slot, amount)` | 1 | mark 2 or 3, after the heal (below) |
| overlay 197 `+2EB8` | `+2E40` | 1 | mark 2 or 3, after overlay 193 `+166F` |
| overlay 197 `+2F4A` | `+2EC0` | 1 | mark 2 or 3, after overlay 179 `+2A05` and overlay 193 `+166F` |
| overlay 204 `+0D57`, `+103E` | the rest `+0CBA` | 1 | mark 2 or 3, or any mark (`+103E`: not 8) when its flag is 1 (FND-MAGIC-005) |
| overlay 179 `+2515`, `+25AD`, `+2668` | `+1373` | 1, 8, 1 | only from mark 7, `DATA` 203 and 77 (FND-RNG-034) |
| overlay 182 `+11B3`, `+1A40`; overlay 190 `+0AC9`, `+0B25`, `+1998` | | 1 | only from mark 0, slots 0 to 3 (FND-COMBAT-052) |
| overlay 187 `+04E1` | `+039A` | 1 | only from mark 0, on load (FND-COMBAT-070) |
| overlay 184 `+1391` | `+1029` | 0 | end of character generation (FND-PARTY-070) |

The remaining hits at displacement 0x14 are other records: overlay 172 `+01AE` cuts a string at
20 characters, `31E0:2350` writes a 37-byte object record and overlay 183's stores go through
`DS:1429`, a details record (alignment).

**Overlay 197 `+2D05(s, a)`** (`0x86105..0x86240`) returns when the mark is 7 or more. With `h`
the hit points and `M` the details word `+0x08`, it takes `g = M - h` and, when `g` is above `a`
(unsigned), `g = a`; it adds `g` to `h` when `g` is above 0 (signed); then, when the mark is 2 or
3, it stores 1 (`+2D27..+2E38`). The mark changes whatever `g` was. With FND-RNG-034's argument names
`t`, `c`, `a` and `n`, **`+2E40(t, c, a, n)`** (`0x86240..0x862C0`) calls overlay 193 `+166F`
with `t`, `c`, `n`, code 0x22, duration 600 and the low byte of `a`, and stores 1 in `t`'s mark
when it is 2 or 3; **`+2EC0(t, c, a, n)`** (`0x862C0..0x86352`) calls overlay 179 `+2A05` with
`t`, -1, code 0x200 and `a`, then `+166F` with `c`, `c`, `n`, 0x22, 600 and the low byte of `a`,
and stores 1 in `c`'s mark when it is 2 or 3. `direct_callers.py` lists `+2D05`'s callers as overlay 179 `+1532` and
`+2234` and overlay 195 `+0A8D`, `+2E40`'s as overlay 179 `+22AA` and `+2EC0`'s as overlay 179
`+22E0` and `+2315`; FND-RNG-034 gives the `DATA` cases in overlay 179 (210, 170, 194, 166 and
117 for `+2D05`, 127 for `+2E40`, 38 and 243 for `+2EC0`).

**Overlay 195 `+0A26`** (`0x82196..0x82208`) has one caller, the near call at `+099D` at the end
of `+0789`, made only when the byte `50C2:0000` is 10 or more (`+0982..+09A0`). With `s`, the
details record `D`, the combatant record `R` and the flag block: when `D`'s word `+0x08` is below
`R`'s word `+0` (unsigned) it lowers `R`'s word `+0` by 1; when block byte 3 has bit 8 it passes
`(R's word +0 + 8) / 10` to overlay 173 `+22C4`; otherwise, with `k = 26 -` byte `+0x1B` of `R`,
when `k` is at most 6 and either `k` is 0 or less or byte `50C2:0001` is a multiple of `k`, it
calls overlay 197 `+2D05` with `s` and 1. FND-MAGIC-008 has overlay 173 `+0688` call `+0789` for
the slot whose turn it is; `direct_callers.py <dsun> 195+0789` adds overlay 173 `+0844`.

**Turns.** The round step gives a slot whose state byte is 2 and whose mark is not 1 the
initiative word -1 at `4C4F:00DC + 4 * c` and resets its other words (`+0CE5..+0CEC`,
`+0FB9..+0FCA`). The damage routine calls `+306B(slot, 0)` when the mark is not 1 after the blow
(FND-PARTY-086), and `+306B` (`0x5D98B..0x5D9E7`) with a mask of 0 stores -1 in the slot's
`4C4F:00DC` word and 0 in its `4C4F:01D8` word. `+1098` (`0x5B9B8..`) picks the slot of state 2
with the greatest `4C4F:00DC` word above 0, or equal to the best so far and with a greater
`4C4F:00DE` word, starting from 0 (`+10BC..+1150`).

**The computer's target** in overlay 173 `+1238`. The remembered target at `4E71:0AC6 + 2 * c`
is kept only when `2D40:3E64` finds it and its mark is 2 or less; otherwise it is set to -1
(`+129A..+1301`). A new one comes from `+1ACD` (`0x5C3ED..0x5C5E1`), which takes candidates from
`+0000` or `+0159` (by its flag byte) with a mask built from the attacker's byte `+0x15`, and
picks one of them with a draw of `1000:0822`, subject to two lookups through `3143:00A8` and
overlay 179 `+2A46` (`+1BE2..+1CA7`). `+0000` (`0x5A920..0x5AA79`) and `+0159`
(`0x5AA79..0x5ABCA`) walk slots 0 to 47 and skip a slot whose state byte is not 2, whose byte
`+0x15` is not in the mask, or whose mark is above 2 (`+0059..+0060`, `+01D2..+01D9`), before
their position tests. `+22C4` also clears every remembered target equal to a slot it leaves above
mark 2 (`+241A..+243D`).

**Other attacks.** The guard attack in `2D40:10AE` (`2D40:13D9`) has attacker `di`, which needs
mark 1 and the byte `4E71:0B1A + di` set (`2D40:11C1..+11F3`), and target `si`, the routine's
argument; it tests no mark of `si` before the call and ends the loop when `si`'s mark is above 1
after it (`2D40:1428`). The walker sweep `2D40:197A` (`2D40:1C61`) has attacker `si`, which needs
state byte 2 and mark 1 (`2D40:19ED..+1A42`), and as target the word `4F49:08A3 + 0x13 * si`
(`2D40:1AE5..+1B16`); no mark of that target is tested between `2D40:1A42` and the call. The
direction key's `28C9:305F` tests no mark of its target (FND-EXPLORE-017). The area gatherers
`+356A`, `+3688`, `+389F` and `+39CC` (FND-EXE-600) contain no read of byte `+0x14`.

**Removal.** When the mark is above 2 after the blow, `+22C4` adds the slot to the removal list
through `+2495` (`+244A..+2450`, FND-RNG-043). `+24C7` empties the list into overlay 206 `+0E1E`,
which unloads the object of a slot of 4 or more through `31E0:05BB` and returns before the unload
for a slot below 4 (FND-PARTY-140). The attack routine `+1CC1` calls `+24C7` before it returns
(FND-COMBAT-065).

**The names.** `immediate_search.py <dsun>` for the table offsets `0663`, `0665`, `0667`, `066B`,
`066F`, `0673`, `0677`, `067B`, `067F` and `0683`, the base `065F` and `0661`, and the string
offsets `14E9`, `14ED`, `14F2`, `14FA`, `1503`, `1509`, `1512`, `151C` and `1521` finds no
instruction that reads the list or a name: the hits are decodings inside other instructions
(for example overlay 190 file `0x798F1`), near calls and jumps whose target happens to equal the
word, a `push 0x167b` at `168C:000A`, and overlay 177 `+0446` and `+09E5`, which index the
11-byte table at `DS:0661` (FND-MAGIC-004).

## Interpretation

The computer never picks a combatant at mark 3 or above as its target, and drops a remembered
target that has fallen to it. A combatant at mark 3 takes no turn, since the round step and the
damage routine give it initiative -1 and `+1098` never picks a slot at -1. A non-party combatant
at mark 3 is unloaded from the map as soon as the attack routine empties the removal list, so in
practice only a party member stays on the map at mark 3. Such a member can still be hit by an
area spell, whose gatherers do not test the mark and whose damage takes the maximum at mark 3
(FND-MAGIC-018), and by an attack whose target was set before it fell: the walker sweep and the
direction key do not test the target's mark. The guard attack's target is the combatant that is
moving, which has mark 1. A weapon hit's doubled damage
(FND-COMBAT-069) needs one of these.

A combatant leaves mark 3:

- to 7 or 8, dead, through one blow of 10 or more (FND-PARTY-086);
- to 1 through any healing that goes through overlay 197 `+2D05`, `+2E40` or `+2EC0`, which
  includes the healing `DATA` cases of FND-RNG-034; `+2D05` stands the combatant up even when it
  adds nothing;
- to 1 with at least 1 hit point at the end of the fight, for party slots 0 to 3;
- to 1 at a rest (FND-MAGIC-005).

The regeneration of overlay 195 `+0A26` heals 1 through `+2D05` for a Constitution byte of 20 or
more, and would stand a member up, but it runs from `+0789`, which overlay 173 `+0688` calls for
the slot whose turn it is; a member at mark 3 gets no turn.

FND-COMBAT-028's nine names are not read through any literal offset of the list or the strings.

## Alternatives

- Whether `31E0:05BB` clears the slot's state byte, which would also take the unloaded combatant
  out of the area gatherers, was not read past `31E0:063E`.
- How the player orders an attack on a chosen combatant, and whether that path tests the target's
  mark, was not found; the walker sweep's target word `4F49:08A3` has writers that were not
  listed. Whether a party member at mark 3 still blocks a cell for the direction key was not read.
- Overlay 173 `+0844`'s call of overlay 195 `+0789` was not read, so a path on which the
  regeneration reaches a slot without a turn is not ruled out.
- The far pointers in the name list could be reached through a computed offset or a copy; only
  literal displacements and immediates were searched.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python, `<dsun>` the installed `DSUN.EXE`
and `<inv>` `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`: `record_base_accesses.py <dsun> <inv>
--source 19C9 14` and `record_base_accesses.py <dsun> <inv> 14`; `field_stores.py <dsun> <inv>
--es 14`; `overlay_listing.py <dsun> 173 5A920 5E700`, `173 5B9B8 5BB58`, `197 86105 86360`,
`195 81EF9 81F60`, `195 820C0 82117`, `195 82190 82210`, `190 78D80 78E80`, `190 79C90 79CE0`,
`182 699B7 69A10`, `184 6E3D0 6E430` and `172 595BF 595E8`; `resident_listing.py <dsun>
2D40:1000..2D40:2196 31E0:2320..31E0:2355 31E0:05BB..31E0:0641`; `direct_callers.py <dsun>
197+2D05`, `197+2E40`, `197+2EC0`, `195+0A26` and `195+0789`; `immediate_search.py <dsun>` with
the offsets listed above.
