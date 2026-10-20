---
id: FND-RNG-046
title: Overlay 176's three d20 range calls are the power check of a psionic DATA number that overlay 193 +2290 hands to +0196, and its two 1-to-U calls are the defence a target makes when overlay 197 +1C52 tests a type word of 0x400
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006159B..0x00061674
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000616D6..0x0006190D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00061CAC..0x00061E8F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00061E94..0x00062024
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FF4F..0x0007FFA6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00080580..0x00080617
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000851A5..0x000851C6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00051224..0x00051225
tool: tools/research/exec-census/direct_callers.py, overlay_listing.py and immediate_search.py at commit e1047cfa; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 176's code starts at file `0x61540`.
FND-RNG-025 reads the range calls of `+07EE` and `+0954`; this reads what reaches them and what
their inputs are. Below, `rank(s, p)` is overlay 176 `+0033` and `known(s, p)` is `+0000`
(FND-PARTY-104), `score(s, k)` is `27E5:04A0`, the byte `+0x19 + k` of the slot's combatant record
(FND-RNG-042), and `points(s)` is the word `+0x02` of that record, through
`object_slot_combatants`. The power table is three bytes per power at `51F1:07E0`: byte 0, whose
bits 0 to 2 are a score number `k` and whose upper five bits, read as a signed byte shifted right
three, are an adjustment `m`; byte 1, `cost`; and byte 2, which FND-PARTY-210 tests against 99.

**Callers.** `direct_callers.py` finds one caller of each: `+07EE` only from the near call at
`+0294`, inside `+0196`; `+0196` only through trampoline `5691:0043`, from overlay 193 `+2317`;
`+0954` only through trampoline `5691:0066`, from overlay 197 `+1DB2`; and overlay 193 `+1C5F`
only from `+0812`. Overlay 193 `+2290` sends a `DATA` number `d` from 235 to 268, when its byte
argument at `bp+0x1A` is 0, to `+0196` with `d - 235` as the power (FND-PARTY-108); its `DI`, its
word argument at `bp+0xE`, is unchanged on that path and is also pushed as `+0196`'s argument at
`bp+0xC`. `+2290` has eleven far callers, in overlays 172, 173, 174 (three), 189 (two), 208, 211
(two) and 213.

**`+0196`** takes the slot `s`, a second slot `t`, the power `p` and six more arguments, of which
the sixth, `q` at `bp+0x14`, is a near pointer to a byte.

1. When `q` is not 0 and the byte it points to is not 0, it goes straight to step 6
   (`+019E..+01AE`).
2. It calls `+005B` with `s` and `p`; on a zero low byte it goes to step 6 (`+01B1..+01C0`).
   `+005B` returns 0, for `s` below 4, when the word `+0x0E` of `s`'s `combatant_details` record is
   0, when `s`'s `combat_mark` is not 1, or when overlay 193 `+1C95` of `s` and 0x43 is not 0;
   `+1C95` returns 1 when `3143:0061` finds an effect entry of `s` with code 0x43. Then, for any
   slot, it returns 0 when `points(s)` is below `cost` (unsigned) or, for `s` below 4, when bit 0
   of the `PSST` byte of `s` and `p` is set; otherwise 1 when `g_4C4F_00B2[s]` is 0 or
   `combat_state` is 0, and 0 when neither is (`+005B..+0133`).
3. It sets the cost `c`: for `p` 15, twice 24 less the byte `+0x15` of the `combatant_details`
   record of `t`'s combatant; for `p` 20 or 21, 3, 8, 13 or 18 as `27E5:0154` of `t` is below 6,
   below 11, below 16 or more (signed); otherwise `cost`; then 0 when it is negative
   (`+01C3..+0252`).
4. When `points(s)` is below `c` (unsigned), it shows `DS:0600` (insufficient points) for `s`
   below 4 and returns 0 (`+0254..+0286`).
5. It calls `+07EE` with `s`, `p` and 0 and stores the low byte at `DS:4224` (`+028C..+029A`).
   Above 0: for byte 2 of `p` not 99 and `s` below 4, it sets bit 0 of the `PSST` byte
   (FND-PARTY-210). Otherwise it shows `DS:0614` for 0xFF, or `DS:0633` for `s` below 4, and takes
   `cost / 2` (rounded down) from `points(s)` (`+02D4..+033F`).
6. When the signed byte `DS:4224` is 0 or less, it returns 0. Otherwise, when `q` is 0 or its
   byte is 0, it takes `DI` from `points(s)` as a word, and calls overlay 193 `+0000` (trampoline
   `573B:007F`) with `s`, `t`, `p + 235`, `+076C` of `s` and 12, its four word arguments from
   `bp+0xC`, the far pointer `DS:q` and its byte argument, and returns that call's low byte
   (`+0343..+03C5`).

`DI` is `c` when step 3 ran. When step 2 sent the routine to step 6, step 3 did not run and `DI`
still holds `+2290`'s word argument. `DS:4224` is 0 in the file and `+029A` is its only writer:
`immediate_search.py <dsun> 4224` finds, besides that store, only the two compares at `+02D4` and
`+0343` and the two loads at `+0AB0` and `+0AC5`. `+076C` returns the level byte `+0x1E + i` of
`s`'s `combatant_details` record for the first `i` from 0 to 2 whose byte `+0x1B + i` is the class
code it is given, or, when none is, 1 for the code 12 and 0 for others.

**`+07EE`'s contest.** Overlay 193 `+1C5F` of 58 scans the 192 ten-byte effect entries at
`4E71:024D` from the first and returns the slot word `+0` of the first whose byte `+6` is 58, or
9999; it does not stop at the count at `DS:2100`. So the contest of FND-RNG-025 runs when an effect
entry of code 58 exists and its slot `o` is not `s`, and returns 0xFF at once when `o` is `s`. Its
opposing threshold is `score(o, 3)` plus power 32's `m` plus `rank(o, 32)`, less 1, and the rank it
compares the first roll with is `rank(s, p)`. The final check's threshold is the low byte of
`score(s, k)` plus the third argument's low byte plus `m` plus `rank(s, p)`, less 1, with `k` and
`m` those of `p`; `+0196` passes 0 as the third argument.

**`+0954`** is called by overlay 197 `+1C52` only when its type word argument is 0x400, with the
slot and 0x400; a nonzero result makes `+1C52` return 0 in place of the percentage it computed
(`+1DA5..+1DC5`). FND-PARTY-112 reads `+1C52`'s other tests, and its one caller, the near call at
`+1289` in overlay 197 `+111A`. In `+0954`, `2D40:3E64` gives the slot's combatant `c` and details
record `e`. The starting bound U is the byte `+0x1D` of `c`'s record less 7. For a slot below 4,
for each power `p` 24, 27, 28, 30 and 33 whose `PSST` bit 0 is set for the slot, the candidate
`rank(slot, p) + cost(p) / 2` replaces U, and `cost(p)` the kept cost, when it is above U (signed)
and `cost(p)` is not above `points` of `c` (unsigned); after the scan the kept cost, 0 when none
was kept, is taken from that word. For a slot of 4 or more, the byte `+0x1E` of `e`'s
`combatant_details` record replaces U when it is above it. U is then at least 1. The two rolls of
1 to U are compared with the signed byte `DS:4224`, the last power check's result.

## Interpretation

The three d20 calls of `+07EE` are the power check of a psionic power that a character or
creature uses through the `DATA` dispatcher, and the check's die is kept at `DS:4224`. The two
calls of `+0954` are a psionic defence: when an effect with type word 0x400 reaches a target, the
target ignores it if two rolls of 1 to its defence score each beat that die. The defence score of
a party member is raised by a defence mode in effect, which costs the mode's table cost again; another
creature's comes from its details record.

## Alternatives

- `DS:4224` is the check of the same use. It is the last check made by any slot: `+0954` reads it
  for every 0x400 test, and step 1 and step 2's failure path use the value an earlier use left.
- The 0x400 type word marks psionic attacks. Not shown: which `DATA` records the hit routine passes
  with that word was not read.
- Step 2's failure path is unreachable. Not ruled out: `+005B` fails, for instance, when the slot's
  points are below `cost`, and the path then takes `DI` rather than `c`; whether any of the eleven
  callers of `+2290` reach it with a positive `DS:4224` was not read.
- The power table is filled from a resource at run time: not read; its bytes at load are not the
  evidence here.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`, with `<dsun>` the installed
`DSUN.EXE`: `direct_callers.py <dsun> 176+07EE 176+0954 176+0196 193+1C5F 193+2290`;
`overlay_listing.py <dsun> 176 61540 62067`, and the same for overlay 193 over `7FF4F..7FFA6` and
`80580..80617` and overlay 197 over `85052..851C6`; `immediate_search.py <dsun> 4224`. The byte at
file `0x51224` is `DS:4224` at load. Outputs are kept under
`GAME_DIR/analysis/rng-callers-20261020/` (`callers-176-179.txt`, `ovl176-all.txt`,
`imm-4224.txt`). Nothing was executed.
