---
id: FND-RNG-037
title: Overlay 197's ten unread roll_sum calls are five tests in the hit routine's gates and a check routine, four picks of a summoned creature and a 4d20 pool for DATA 71
status: superseded
builds: [BLD-GOG-EN-1.1]
superseded_by: [FND-RNG-040]
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00084365..0x00084395
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008451A..0x00084610
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00084B60..0x00085044
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00085044..0x00085052
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008654E..0x000865AA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000866A1..0x00086928
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00086928..0x000869A4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00086D9F..0x00086F09
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E64A..0x0007E763
tool: tools/research/exec-census/overlay_listing.py, direct_callers.py, trampoline_target.py and data_bytes.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 197's code starts at file `0x83400`
and is `0x3B67` bytes. Decoding six routines from their entries to their far returns shows
FND-RNG-028's ten overlay 197 `roll_sum` sites as instructions: `+11E8` in `+111A` (trampoline
`575A:009D`), `+17F0` and `+1A18` in `+1760` (`575A:0098`), `+1B55` in `+1A94` (`575A:0020`),
`+3194` in `+314E` (`575A:00C0`, and a near call after `push cs` at `+0F8A` in `+0E8F`), `+3368`,
`+33B5`, `+3426` and `+3445` in `+3315` (`575A:002A`), and `+39AD` in `+399F` (`575A:00D9`).
`roll_sum` is `28C9:391D`, count first and sides second (RULE-RNG-001). `R` is a combatant record
(`DS:19C9`, 49 bytes), `D` the details record its word `+4` numbers (`DS:19C5`, 66 bytes), and
"the block" the 16-byte block overlay 193 `+1BA3` fills for a slot. "Queue `x, y, n, 6`" is a
call of overlay 179 `+12B1`, which adds a record that overlay 193 `+003C` later runs with `y`, `x`
and `n` as its first three arguments (FND-PARTY-107).

### The hit routine's gates

The hit routine overlay 179 `+0D2F`, with target `t`, caster `c`, `DATA` number `i` and its byte
argument `a` at `bp+0x1C` (FND-RNG-034), calls `+0E8F`, `+1760` and `+111A` in turn
(FND-PARTY-112); the direct-call query finds no other caller of `+1760` and one more of `+111A`,
overlay 174 `+04D2` (FND-PARTY-122).

- `+11E8`, in `+111A` (`t`, `t`'s block, `i`, `m`, a pointer and `a`): when byte `+0x0E` of `t`'s
  `R` is 13, `i` is not -1, byte `+0x17` of `DATA i` is -1, its word `+1` is above -2 (signed)
  and `i` was not passed negated, it rolls 1 and 6. A total other than 6 returns 1, ending the
  hit; a 6 queues the slot at `DS:426D`, `t`, `i`, `a` and then returns 1 (`+11AC..+1210`). The
  total is compared as a word with 6.
- `+17F0`, in `+1760` (`t`, `c`, both blocks, `i`, `m` and four more): after `+1760`'s earlier
  steps (FND-PARTY-112), when `i` is not -1, overlay 208 `+0885` of `i` and 1 is 8 or less, `i` is
  not 235 to 268 and bit 0x40 of `t`'s block byte 5 is set, it rolls 1 and 100. A total of 75 or
  less (signed word compare) calls overlay 193 `+1AA4` with `t` and 0x2E, queues `t`, `t`, 307, 6
  and returns 0, ending the hit; above 75 it goes on to `+181C` (`+17B6..+181C`).
- `+1A18`, in `+1760`: when `2D40:3E64` finds `c`, `2D40:35EF` of `c` and `t` returns 1 or less,
  bit 0x80 of `t`'s block byte 9 and bit 0x100 of `m` are set, byte `+0x12` of `c`'s `D` is 9 and
  the saving throw overlay 179 `+2A46` of `t`, `c`, 38 and 6 returns other than 0 (FND-PARTY-112),
  it calls `+1AA4` with `t` and 0x4F and rolls 2 and 6. It passes the total, as a word, as the
  fifth of eight arguments of overlay 179 `+1304`: `c`, `t`, -1, 0x200, the total, 1, 6 and byte
  `+0x15` of `t`'s `R`. It then queues `t`, `c`, 308, 6 and makes `+1760` return 0
  (`+18D6..+1A48`).
- `+3194`, in `+314E` (a slot): when `2D40:3E64` finds the slot, byte `+0x12` of its `D` is 8 and
  `27E5:0154`, its highest level (FND-PARTY-081), is 7 or more, it rolls 1 and 20 and returns 1
  when the total is above 8 (signed word compare) and 0 otherwise; every other path returns 0
  with no roll (`+314E..+31AA`). `+0E8F` calls it when its byte argument at `bp+0x10`, the hit
  routine's byte at `bp+0x14`, is not 0 and bit 0x80 of `t`'s block byte 5 is clear, and ends the
  hit when it returns 1 (`+0F65..+0F95`). With bit 0x80 set, `+0E8F` queues `t`, `t`, 307, 6
  instead and ends the hit with no roll.

### `+1A94`: a check with a d100 under a class level

`+1A94` takes a case `k`, a slot `s` and a number, fills `s`'s block, and returns 0 when bit 0x20
of block byte 8 is set. Otherwise it jumps through the seven words at `cs:1C44` (file `0x85044`)
for `k` from 0 to 6 and returns 1 for `k` above 6 (`+1A94..+1ACC`). All five direct callers,
overlay 172 `+0A73`, `+0D44` and `+1140` and overlay 211 `+159B` and `+1C46`, pass `k` 4 and the
number 0. Case 4 (`+1AE7..+1B78`) returns 0 when word `+0x0E` of `s`'s `D` is 0, when byte `+0x14`
of `s`'s `R` is not 1, when the signed byte at `4C4F:00B2 + s` and the word at `4C10:0019` are
both not 0, or when any of these bits of the block is set: byte 8 0x10, byte 0 0x10 or 0x80,
byte 4 0x08 or 0x20. Then, when bit 0x40 of block byte 7 is clear, it returns 1 with no roll;
when it is set, it rolls 1 and 100 at `+1B55` and returns 1 when the total is below `27E5:00C1`
of `s` and 11, the level `s` holds in class code 11 (FND-PARTY-081), signed word compare, and 0
otherwise.

### `+3315`: a summoned creature

`+3315` takes a number `n`, two coordinates and a byte; its one direct caller is overlay 193
`+03D5`, in `+003C`, which passes `i`, its words at `bp+0xE` and `bp+0x10` and the byte `+0x15` of
the caster's `R` (11 for a caster below 0), when byte `+0x17` of `DATA i` is -1 and bit 0x0001 of
its word `+0x11` is set (`+035A..+03DD`). In the shipped `RESOURCE.GFF` the 18 `DATA` records
with both are 59, 65, 76, 81, 87, 88, 92, 96, 106, 132, 189, 190, 191, 192, 217, 218, 219 and 220.

When the byte at `4F49:0C3F` is not 0 it shows the text at `DS:252D` through overlay 172 `+05DB`
and returns. Otherwise it picks a creature number `p` by looking `n` up in the 15 words at
`cs:3568` and jumping through the 15 after them, `p` being 414 when `n` is not there
(`+331D..+335C`):

| `n` | `p` |
|---|---|
| 59 | `roll_sum(1, 4)` at `+3426`: 1 gives 422, 2 gives 424, 3 gives 425, 4 gives 423 |
| 65 | 567 |
| 76 | `roll_sum(1, 12)` at `+33B5`: 1 to 12 give 400, 550, 551, 406, 407, 552, 418, 419, 403, 404, 405 and 523 |
| 81 | `roll_sum(1, 4)` at `+3445`: 1 gives 426, 2 gives 428, 3 gives 429, 4 gives 427 |
| 87 | `roll_sum(1, 12)` at `+3368`: 1 to 3 give 421, 4 to 6 give 522, 7 gives 415, 8 gives 452, 9 gives 453, 10 gives 454, 11 and 12 give 441 |
| 88 | 559 |
| 132 | 569 |
| 189, 190, 191, 192 | 422, 423, 424, 425 |
| 217, 218, 219, 220 | 426, 427, 428, 429 |

Each roll's total less 1 indexes a jump table (`cs:3550` for 87, `cs:3538` for 76, `cs:3530` for
59, `cs:3528` for 81), bounded by an unsigned compare that sends any other value to 540; a total
of 1 to the sides never does. With the coordinates each divided by 16 (signed), it asks overlay
188 `+0AA8` for a place; when it finds one, it places `p` through `31E0:0121` with 4 and the place,
and when that returns 0 or more it stores 4 in byte `+0x15` of combatant record 4, 4 in byte
`+0x36` and 14 in byte `+0x12` of details record 4, calls `2D40:085B` with 4, 11, the word at
`4C13:0369` and 0, and for `n` of 92 or 88 calls `+32A1` with `n`. It ends with `2D40:06E1` of the
byte argument (`+3487..+3528`). The 62 words are at file `0x86928..0x869A4`.

### `+399F`: the pool of `DATA` 71

`+399F` takes a count `N` and a far pointer to a list of four-byte entries whose word `+2` is a
slot; its one direct caller is overlay 193 `+046B`, in `+003C`, for `i` of 71, with the list
`+003C` builds: one entry for a single target, or overlay 208 `+0000`'s list
(`+041B..+0473`). It rolls 4 and 20 at `+39AD` and keeps the total `P` as a signed word. For each
band `b` from 0 to 3 while `P` is above 0, it goes through the entries in order while `P` is above
0, and for each whose slot's state byte (`4F49:0C33 + 3 * slot`) is 2 takes a level `L`: byte
`+0x1E` of the slot's `D` when its byte `+0x12` is 1, and otherwise the greatest of its bytes
`+0x1E`, `+0x1F` and `+0x20`. The entry is taken in band 0 when `L` is below 2, band 1 from 2 to 4,
band 2 from 5 to 6 and band 3 from 7 to 8, at a cost of 1, 2, 10 and 20; a taken entry lowers `P`
by the cost and is damaged by 10,000 through overlay 179 `+2A05` with `DATA` number 71 and code -1
(FND-RNG-034). An entry is taken even when its cost is above what is left of `P`, and `L` of 9 or
more is never taken (`+399F..+3B09`).

## Interpretation

With FND-RNG-028..036, every `roll_sum` and generator site of the direct-call census is now read.
Overlay 197's ten are: a d6 that, for a target with record byte `+0x0E` 13, sends a spell back
at the combatant in `DS:426D` on a 6, the target being spared on any roll; a d100 that blocks a
spell for a target with block byte 5 bit 0x40 on 75 or less and has the target cast `DATA` 307 on
itself; a 2d6 total handed to the cast queue when a target with block byte 9 bit 0x80 makes a
saving throw against a caster whose `D` byte `+0x12` is 9; a d20 above 8 that spares a target
whose `D` byte `+0x12` is 8 and whose level is 7 or more, when the hit routine's byte at `bp+0x14`
is set; a d100 under the level in class 11 that a check makes for a slot with block byte 7 bit
0x40; the creature that 18 `DATA` numbers summon, picked by a d4 or d12 for 59, 81, 76 and 87; and
the 4d20 pool that `DATA` 71 spends to kill combatants of level below 9, lowest bands first. No
rule entry describes these yet.

## Alternatives

- What `m`, the block's bits, record byte `+0x0E` 13, `D` byte `+0x12` 8, 9 and 14, class code 11,
  and the creature numbers stand for, what the text at `DS:252D` says, and what overlay 172 and
  211 do with `+1A94`'s result, were not read.
- What overlay 179 `+1304` and the queue do with the 2d6 total, given that FND-PARTY-107 has the
  queue run overlay 193 `+003C` with a `DATA` number of -1 for such a record, was not read.
- Reading `+11E8` as turning a spell back rests on FND-PARTY-107's account of the queue's argument
  order alone.
- `2D40:35EF`, overlay 188 `+0AA8`, `31E0:0121`, `2D40:085B`, `2D40:06E1`, `+32A1` and overlay 208
  `+0000` were not read beyond their arguments here.
- `+314E`'s trampoline has no direct caller; a computed call was not searched for.
- The overlay is outside the resident harness; no emulated call or run was made.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`: `trampoline_target.py <dsun>
575A:0020 575A:002A 575A:0098 575A:009D 575A:00C0 575A:00D9`; `overlay_listing.py <dsun> 197 83400
86F67` and read the ranges above, the seven words at file `0x85044` and the 62 at `0x86928`;
`overlay_listing.py <dsun> 193 7E2F0 80ECE` for `+035A..+0473` and `172 59430 5A6FE` and `211 95E10
97F00` for the five calls of `+1A94`; `direct_callers.py <dsun> 197+111A 197+1760 197+1A94 197+314E
197+3315 197+399F`; and `data_bytes.py <install>/RESOURCE.GFF 11` and `17`, taking the records with
bit 0 of the first and -1 in the second. Output is kept under
`GAME_DIR/analysis/rng-callers-20261020/`. Nothing was executed.
