---
id: FND-RNG-036
title: Overlay 195's ten unread roll_sum calls are a d10 confusion table, a side roll, a d100 test against a table on combatant byte +0x19, and the dice of seven per-round effect damages
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008177E..0x00081EF9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00082C9F..0x00082DAA
tool: tools/research/exec-census/overlay_listing.py, direct_callers.py and trampoline_target.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 195's code starts at file `0x81770`
and is `0x163A` bytes. Decoding `+000E`, `+00D1` and `+152F`, each a trampoline target
(`574E:0025`, `574E:002A`, `574E:005C`), from their entries to their far returns shows
FND-RNG-028's ten overlay 195 `roll_sum` sites as instructions: `+005F` and `+0087` in `+000E`,
`+020D`, `+03E9`, `+0585`, `+05DE`, `+0637`, `+06E5` and `+0705` in `+00D1`, and `+1624` in
`+152F`. `roll_sum` is `28C9:391D`, count first and sides second (RULE-RNG-001). Each routine takes
a slot `s`; `R` is the combatant record of `s` (`DS:19C9`, 49 bytes, through the word at
`4F49:0C34 + 3 * s`) and `D` its details record (`DS:19C5`, 66 bytes). `B` is the 16-byte block
overlay 193 `+1BA3` fills for `s`. "Entry `e` of effect `k`" is `3143:00A8` of a slot and `k`,
indexing the ten-byte records at `4E71:024F`, whose word `+0` is the effect's source slot and
byte `+7` (`4E71:0256 + 10 * e`) a strength byte. "Damage" is overlay 179 `+0C7F` (trampoline
`56A7:0066`) with the target, the source, -1, a code, a pointer to the amount and the amount.

### `+000E`: the confusion table

Its one direct caller is overlay 173 `+127B`, in `+1238`, when bit 0x100 of the word at
`4C4F:01D8 + 2 * c` is set, `c` the combatant; it stores the result in `DS:420C`. It clears that
bit, then (`+000E..+00D1`):

1. returns 1 when bit 0x08 of `B` byte 4 is set;
2. when bit 0x80 of `B` byte 0 is set, goes to step 4;
3. when bit 0x20 of `B` byte 2 is set, rolls 1 and 10 at `+005F`: 1 returns 1, 2 to 6 return 0,
   7 to 9 go to step 4 and 10 returns 2; with neither bit it returns 2;
4. rolls 1 and 2 at `+0087`, stores the total in byte `+0x15` of `R`, stores -1 in the word at
   `4E71:0AC6 + 2 * c`, and returns 7.

### `+152F`: a d100 test

`+152F` sets a chance `p` from `b`, byte `+0x19` of `R`: 25 gives 99, 24 gives 95, 19 to 23 give
`10 * (b - 14)`, 18 gives 25, 17 gives 13, 16 gives 10, 14 and 15 give 7, 12 and 13 give 2, 8 to
11 give 1, and 7 or less gives 0 (unsigned byte compares). It rolls 1 and 100 at `+1624` and returns
0 when the total is below `p` and 1 otherwise (`+152F..+163A`). Its direct callers are the three
near calls in `+00D1`.

### `+00D1`: per-round effects

Its one direct caller is overlay 173 `+1168`, in `+0C65`, when overlay 173 `+2EC7` of the slot
returns a value with a bit other than 0x10 set. It fills `B`, calls overlay 193 `+0DC7` with `s`
and 1, and removes effect 0x1E from each of combatants 0 to 47 whose byte `+0x14` is 3 or more
and who hold it (`+00D1..+013A`). Steps 1 to 4 below form a chain in which only the first whose
condition holds runs; steps 5 to 9 each run when their condition holds:

| Step | Bit | Effect `k` | Site, count and sides | Damage |
|---|---|---|---|---|
| 1 | `B` byte 3, 0x40 | 0x1E | `+020D`: strength byte of `k`'s entry, 20 | code 0x40 |
| 2 | `B` byte 12, 0x10 | 0x64 | none | none |
| 3 | `B` byte 2, 0x01 | 0x10 | `+03E9`: 1, 4, plus overlay 173 `+2C00` of `s`, `s` and 0 | code 0x20, when the sum is above 0 |
| 4 | `B` byte 6, 0x04 | 0x32 | none | none |
| 5 | byte `+0x12` of `D` is 16 and `B` byte 3, 0x01 | 0x18 | none | none |
| 6 | `B` byte 12, 0x20 | 0x65 | `+0585`: 1, 4 | code 2 |
| 7 | `B` byte 0, 0x02 | 1 | `+05DE`: 2, 4 | code 0x40 |
| 8 | `B` byte 2, 0x08 | 0x13 | `+0637`: 2, 4 | code 2 |
| 9 | `B` byte 13, 0x80 | 0x6F | `+06E5`: 2, 4 as a count `m`; `+0705`: 2, 4 per hit | code 0x10, to the source |

Further detail:

- Step 1 does nothing for a slot of 4 or more. Otherwise it shows a text built from the name
  and `DS:2128` when `+152F` of `s` returns 0, and otherwise removes effect 0x1E from `s` and
  shows one built from `DS:2143`. It then takes the entry of effect 0x1E; with none, as after the
  removal, it makes no roll (`+0146..+024A`).
- Step 2 lowers byte `+0x1B` of `R` by 1, damages `s` by 1000 through overlay 173 `+22C4` when
  it reaches 0, and, for a party slot whose effect 0x64 entry's word `+2` is 0x122, calls overlay
  197 `+30ED` with 0x122 and `s` when `+152F` returns other than 0 (`+0256..+035D`).
- Step 3 removes effect 0x45 from `s` and runs the saving throw overlay 179 `+2A46` for `s`, the
  source, `DATA` 188 and overlay 193 `+1A2D` of `s` and 0x10; on a success it removes effect 0x10,
  and on a failure it applies effect 0x45 with `DATA` 188 and duration -9999. It rolls either way
  (`+0369..+0421`).
- Step 4 calls overlay 193 `+1AA4` with `s` and 0x32 when `+152F` returns other than 0, and then
  removes effect 0x45 when the effect 0x32 entry's byte `+6` is below 2 (`+0424..+046C`).
- Step 9 does nothing when the source's byte `+0x14` is 3 or more. Otherwise, with the source's
  entry of effect 0x6E, it rolls `m` and damages the source `m + 1` times: 8 the first time and a
  2-and-4 roll each later time. It then removes effect 0x6F from `s` when the source's byte
  `+0x14` has become 3 or more, and also, with no roll, when the source holds no effect 0x6E
  (`+067F..+076B`).

Last, when the byte at `50C2:0000` is 0 it calls `+0A1F` with `s`, and in every case overlay 173
`+24C7` (`+076E..+0789`).

## Interpretation

`+000E` is a d10 table for a confused combatant: 1 gives result 1, 2 to 6 result 0, 7 to 9 and an
unconditional bit a random side byte (1 or 2) with result 7, and 10 result 2, the same as an
unaffected combatant. `+152F` is a percentile test against a table of combatant byte `+0x19`
that rises from 1 at 8 to 99 at 25. `+00D1` applies the damage of up to five effects each round,
from 1d4 to Nd20, and makes no draw for the others. No rule entry describes these yet.

## Alternatives

- What the effects, the damage codes, `B`'s bits, byte `+0x19` and the results of `+000E` stand
  for, and what overlay 179 `+0C7F`, overlay 173 `+2EC7` and `+2C00` (for these arguments),
  `+0A1F` and overlay 193 `+0DC7` do, were not read. Reading byte `+0x19` as a score rests on the
  table's range alone.
- The text at `DS:2128`, `DS:213D`, `DS:2143` and `DS:2150` was not read.
- The overlay is outside the resident harness; no emulated call or run was made.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`: `trampoline_target.py <dsun>
574E:0025 574E:002A 574E:005C`; `overlay_listing.py <dsun> 195 81770 82DAA` and read the ranges
above; `direct_callers.py <dsun> 195+000E 195+00D1 195+152F 195+0000`. Output is kept under
`GAME_DIR/analysis/rng-callers-20261020/`. Nothing was executed.
