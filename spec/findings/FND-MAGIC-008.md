---
id: FND-MAGIC-008
title: Overlay 176 +03CD keeps each power a party member has in effect by applying its DATA number again and taking the third table byte from the member's points when they cover it, so a power whose byte is 0 stays in effect for nothing, and ends it otherwise; the round step of the fight routine calls it for every slot and overlay 195 +0789 again when a block bit is set, and 2D40:37C1 ends all but the zero-cost powers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006190D..0x00061A4E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00061674..0x000616D6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00061A4E..0x00061AC6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B000..0x0005B049
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B585..0x0005B5D6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B8D9..0x0005B9A8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008206F..0x000820F2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:19B5..2D40:19E8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:1F9B..2D40:1FB5
tool: tools/research/exec-census/overlay_listing.py, resident_listing.py, direct_callers.py and segment_offset_refs.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x61540 + x` for overlay 176, `0x5A920 + x` for 173 and
`0x81770 + x` for 195. Party slot `s` has its state byte at `4F49:0C33 + 3 * s` and its combatant
number at `4F49:0C34 + 3 * s`; the combatant record's word `+0x02` is its current PSPs
(FND-PARTY-059) and byte `+0x14` its `combat_mark`. Byte 2 of power `p`'s row is its maintenance
(FND-MAGIC-007). Effect entries are the 10-byte rows at `4E71:024D`, with the affected slot in
word `+0`, the slot that caused it in word `+2`, the source number in word `+4` and a code in byte
`+6`; `DS:2100` is their count (FND-RNG-046, FND-COMBAT-060).

**`+0134`** (`+0134..+0196`) takes `m` and `p`. It returns 0 for `m` of 4 or more and when bit 0 of
`m`'s `psionic_ranks` byte for `p` is clear; otherwise 1 when byte 2 of `p` is at most the word
`+0x02` of `m`'s combatant (unsigned), and 0 when it is above (FND-EXE-611).

**`+03CD`** (`+03CD..+050E`, trampoline `5691:0048`) takes a slot `s` and returns at once for `s`
of 4 or more. It sets a local `u` to `s` once (`+03D8`), then for each power `p` from 0 to 33:

1. When the state byte of `s` is 0, or its `combat_mark` is 3 or more, or `+0134(s, p)` returns 0,
   it goes to step 4 (`+03E5..+041C`).
2. For `p` 19 only, it scans the effect entries from the first, while the index is below the
   count and `u` still equals `s`, and sets `u` to word `+0` of an entry whose byte `+6` is 15 and
   word `+2` is `s` (`+041F..+0467`).
3. It calls overlay 193 `+0000` (trampoline `573B:007F`) with `s`, `u`, `p + 235`, the level
   `+076C(s, 12)` (FND-PARTY-169), four zero words, a null far pointer and the byte 1
   (`+0469..+048F`), then takes byte 2 of `p` from the word `+0x02` of `s`'s combatant
   (`+0492..+04D9`), and goes on with the next power.
4. When bit 0 of `s`'s byte for `p` is set, it calls `+050E(s, p)` (`+04DF..+04FE`).

`u` is not set back to `s` between powers, so once step 2 has found an entry, every later power
of the same call is applied with that slot as the second argument.

**`+050E`** (`+050E..+0586`) clears bit 0 of `s`'s byte for `p` and calls overlay 193 `+1873` with
`s` and byte `+7` for each of the 192 effect entries whose word `+4` is `p + 235` and word `+2` is
`s` (FND-COMBAT-060).

**The callers.** `direct_callers.py <dsun> 176+03CD` finds two far calls through `5691:0048`:

- Overlay 173 `+1022`, in the round step `+0C65`. `+0C65` adds 60 to the clock `4C13:035B` and
  runs the timed queues only when `DS:054E` is 0, and otherwise stores 0 there (`+0C72..+0CA6`);
  either way it walks slots 0 to 47, and every slot whose state byte is 2 reaches `+1021..+1027`,
  the call with the slot, whichever initiative branch it took (`+0CB6..+0FB9`, `+0FB9..+1022`).
  The fight routine `+059A` calls `+0C65` at `+071D` when `+1098` names no slot left to act, and
  goes on to the end of the fight at `+0729`, which calls `+3E30`, when `+0C65` returns 0
  (`+06E0..+0729`). `+085D` sets `DS:054E` to 1 (FND-EXE-600).
- Overlay 195 `+095E`, in `+0789`, the routine overlay 173 `+0688` calls for the slot whose turn it
  is (FND-RNG-063). `+0789` fills the slot's 16-byte flag block through overlay 193 `+1BA3` at
  `bp-0x1C`, and only when block byte 12 has bit 0x80 (`+08FF`) it sets the slot's initiative word
  `4C4F:00DC + 4 * slot` to 50, resets the bytes and words the round step resets, and calls
  `+03CD` with the slot (`+08FF..+0963`).

`direct_callers.py <dsun> 176+0134` finds only the near call at `+0412`.

**`2D40:37C1`** (FND-MAGIC-007) has one direct caller, the near call at `2D40:1FA4` in the
walker sweep `2D40:197A` (FND-COMBAT-062). The sweep sets a flag when the double word
`4F49:0002` less the clock `4C13:035B` is above 60 (signed), and then stores the clock in
`4F49:0002` (`2D40:19B5..2D40:19E8`); in its loop over party slots 0 to 3 it calls `2D40:37C1`
with each slot when the flag is set (`2D40:1F9B..2D40:1FB5`). `segment_offset_refs.py <dsun> 4F49
0002` finds only that load and that store; the double word is 0 in the file (`0x44692`).

## Interpretation

A power a party member keeps in effect is renewed at every round step of a fight: when its
maintenance is not above the member's PSPs, its `DATA` number is applied again with the member's
psionicist level and no power check, and the maintenance is paid; otherwise the power ends. A
member whose state byte is 0 or whose `combat_mark` is 3 or more loses every power it has in
effect at the round step. The step runs for party slots only, since `+03CD` returns for others,
and FND-COMBAT-060 reads that each pass of `+059A` with combat off runs it once before ending
every power through `+3E30`. A slot whose block byte 12 has bit 0x80 pays and renews again at its
own turn.

A maintenance of 0 passes the test with any number of PSPs, 0 included, and takes nothing, so
Mind Blank, Tower of Iron Will, Intellect Fortress, Mental Barrier and Thought Shield stay in
effect for free until the member falls, the fight ends (`+3E30`), the party rests (FND-MAGIC-005)
or `2D40:37C1` runs, which spares them anyway. Their cost is paid again each time the psionic
defence uses one (RULE-MAGIC-015).

Share Strength (19) is renewed on the member whose effect it caused, the slot of the first effect
entry of code 15 that the member caused; every other power is renewed on the member itself, and
any power after 19 in the same pass on the Share Strength target.

`2D40:37C1` is meant to end the paid powers after time passes outside a fight, but as written the
sweep tests the stored time minus the clock, which the clock's growth does not make positive, so
it runs only after `4F49:0002` has been left above the clock.

## Alternatives

- What overlay 193 `+0000` does when it applies a number again (renewal of a duration, a new
  effect entry, a second application of the effect), and what its last byte argument 1 selects
  where `+0196` passes its caller's byte, were not read.
- Which effect sets bit 0x80 of block byte 12 was not read (Q-MAGIC-023 covers the block bits).
- Whether the pass with combat off reaches `+0C65` rests on FND-COMBAT-060; FND-COMBAT-056 saw no
  change in `4C4F` in a run, which favours a path that skips the initiative stores but not the
  call at `+1022`, which every kind-2 slot reaches.
- When `4F49:0002` can be above the clock (a load of an earlier save that keeps the segment, for
  instance) was not read; a reading of what the save and load copy, or a run, settles it.
- That `u` carrying past power 19 changes anything depends on what overlay 193 `+0000` does with
  its second argument for the later powers.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python: `overlay_listing.py <dsun> 176 61540
62067` (keep `+0134..+0196` and `+03CD..+0586`), `173 5A920 5BA00` (keep `+059A..+0800` and
`+0C65..+1088`) and `195 81770 82300` (keep `+0789..+09A7`); `resident_listing.py <dsun>
2D40:197A..2D40:19E8 2D40:1F70..2D40:1FB5 2D40:37C1..2D40:37F6`; `direct_callers.py <dsun>
176+03CD 176+0134 176+050E 173+0C65 173+3E30 2D40:37C1`; `segment_offset_refs.py <dsun> 4F49
0002`.
