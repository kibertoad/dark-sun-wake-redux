---
id: FND-COMBAT-060
title: With combat word 0 a pass of overlay 173 +059A adds 60 to the clock that timed effect ends wait on, ends maintained psionics, effect 0x45 and the fight-end effects, re-targets creatures to their fight-start walk records, stops the playing sample, flushes queued input and flips the drawn page
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005AEBA..0x0005B175
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B585..0x0005B9A8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B9B8..0x0005BB58
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005E5FA..0x0005E78B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006190D..0x00061AC6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000746C0..0x00074716
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000884C1..0x00088675
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00082208..0x00082269
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 371D:0334..371D:050F
tool: tools/research/exec-census/overlay_listing.py, resident_listing.py, direct_callers.py, trampoline_target.py, immediate_search.py and segment_offset_refs.py at the commit that adds this finding; Capstone 5.0.9; Python 3.14.7
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x5A920 + x` for overlay 173, `0x81770 + x` for 195,
`0x72EA0 + x` for 188, `0x61540 + x` for 176 and `0x87900 + x` for 199. The pass is
`173 +059A(0, &DS:426D)` from `28C9:1EDB` (FND-RNG-074).

**Entry.** `+05C3` stores 9999 in `DS:44E6`. With the first argument 0 the routine reaches
`+06E0`, which calls `+1098`. `+1098` is a routine of its own (from file `0x5B9B8`; its only
direct caller is `+06E6`). It calls `+3304` and, on a zero result, returns 9999 with no store
outside its frame; its call of overlay 195 `+00D1` at `+1168` is on the other path. `+3304` and
`+0958` store only into their frames. A nonzero result of `+3304` would send the pass back to
`+06E0`.

**Round step `+0C65..+1088`.** With `DS:054E` 0 it stores `0xFF` in `4C4F:0006`, adds `0x3C` to
the double word `4C13:035B` and calls `371D:0334`, overlay 195 `+0AAD` and `+0A98`. For each
slot 0 to 47 whose kind byte is 2 it either stores initiative values in `4C4F:00DC`/`00DE`
(through two draws of `1000:0822` at `+0E99` and `+0EDB`) and `movement_tenths`, or `0xFFFF` and
0; then it stores `0xFF` in `4C4F:0088` and `4C4F:005E`, `0x03FF` in `4C4F:01D8`, 0 in
`4C4F:0184`, `4C4F:01AE` and `4C4F:00B2`, and calls overlay 176 `+03CD(slot)`. It stores 9999 in
`4C4F:022C` and returns `+3304()`.

**`371D:0334`.** Resets the dirty rectangle `DS:44E8..44EE` (`31E0:0AFC`), then for queue 0 and
queue 1 runs each 17-byte record at the head of the list at `DS:2A63 + 4i` whose double-word time
is not above the clock its far pointer `DS:9C0C + 4i` names, removing it and dispatching its kind
at `371D:044D` (3 an object unload, 5 overlay 193 `+0DC7`, 6 `+194F`, 7 `+1873`), and redraws
the rectangle (`31E0:0B12`). `371D:0770` and `3794:0000` point queue 0 at `4C13:035B` and queue 1
at `game_tick`. `direct_callers.py <dsun> 371D:01F9` finds overlay 193 `+1810` adding kind 7
records to queue 0 at the clock plus a duration, `+0D58` kind 5 to queue 1, `371D:0766` kind 3 to
queue 1 and `+2BCB` kind 3 to the queue its argument names.

**Overlay 195 `+0AAD` and `+0A98`.** `+0AAD` steps the byte `50C2:0000` from 0 to 10 and, each
time it wraps, the byte `50C2:0001` from 0 to 60. `+0A98` calls a bare return. The readers of
`50C2:0000` are all in overlay 195.

**Overlay 176.** `+03CD(slot)` acts on party slots 0 to 3 with `combat_mark` below 3: for each
psionic power whose in-effect bit is set and whose cost byte `51F1:07E2 + 3 × power` is not above
the combatant's word `+0x02`, it re-applies the power through overlay 193 `+0000` and subtracts
the cost from word `+0x02`; where the cost cannot be paid it calls `+050E`. `+050E(slot, power)`
clears the in-effect bit and calls overlay 193 `+1873` for each of the 192 effect entries whose
word `+4` is `power + 0xEB` and word `+2` the slot.

**End of combat `+0729..+07D1`.** In order: when `DS:13F7` is set, `4A6E:0064(4E71:0C14)` and
overlay 187 `+2B91(2)`, which stores 2 in `music_mode`; `+3E30`, which calls overlay 176
`+050E(slot, power)` for each party slot and each power whose bit 0 of `psionic_ranks` is set;
overlay 195 `+14D5`, which for each slot 0 to `0x13F` of kind 2 fills an effect block and runs
overlay 197 `+29E0`, removing about 25 effect codes (`+194F`) and, under some block bits, running
the slot's `1695:02E7(slot, 0xF460)` trigger and unloading it; overlay 206 `+0B49`, which frees
`DS:2904` and closes `DS:2908` once; `+3CDA`, which removes effect `0x45` from each party slot,
stores 0 in word `+4` of its walk record, gives each party member with `combat_mark` 1 to 3
`combat_mark` 1 and at least 1 hit point, and calls overlay 182 `+1D3E`; overlay 210 `+0B44`,
which runs overlay 199 `+0C21` and the level-gain check `+08BC(0)` (FND-PARTY-081); overlay 188
`+1820`; and the store of 9999 at `+07CF`.

**Overlay 199 `+0BC1..+0D75`**, which `+0C21` reaches, stores 0 in `DS:0DA4` and `DS:143D`, sets
the key hook to `28C9:0CFF` through overlay 190 `+11A1`, waits through `45B9:0034` (INT 33h
function 3) while the left button is down, calls `4611:03A5`, which stops the playing sample, frees
`DS:6554`, `DS:6558` and three windows at `52A1:0000..0008`, calls overlay 182 `+19F8` (which stores
0 in `right_button_held`), `show_drawn_page` and `flush_input`, and waits for the left button again.

**Overlay 188 `+1820..+1876`.** For slots 5 to 47 whose word `DS:55CF + 6 × slot` is not negative
it copies the 6 bytes at `DS:55CB + 6 × slot` over the slot's walk record `4F49:08A3 + 19 × slot`
with `1000:3F5A`, then calls `2D40:3D4C(byte DS:55CA, leader)` at `+186B`, which hands every walk
record that follows slot `DS:55CA` to the leader (FND-RNG-071). The return address `+1870` is
FND-COMBAT-057's `61B2:1870`.

**`4C13:0367..037A`.** `1B0B:000C` runs with `DS` set to `4C49` and stores through `1B0B:0088`
each coordinate, the absolute difference and its sign at `4C49:0019`, `000D` and `0013` (and
`0017`, `000B`, `0011` for the other coordinate), which are `4C13:0379`, `036D`, `0373`, `0377`,
`036B` and `0371`. Overlay 173 `+0000`, which `+3304` calls, calls `1B0B:000C` at `+00F0`.

## Interpretation

Each pass ends, whatever their time left, every maintained psionic power (after charging its
upkeep once), effect `0x45` on the party and the effects the end of a fight removes, and the extra
60 on `4C13:035B` makes timed effect ends on queue 0 come due about 600 times faster than the
clock's own rate. The sample playing is stopped and the queued keys and clicks are dropped on
every pass, a held left button stalls the pass, and creatures from slot 5 up are put back on their
fight-start walk records. The `4C4F` tables and the `50C2` counters are combat state that nothing
outside a fight shows. The slot record whose word `+0` FND-COMBAT-057 sees go from 0 to 1 is set
by the `2D40:3D4C` call at overlay 188 `+186B`, and `4C13:0373` is the distance routine's scratch.

## Alternatives

FND-RNG-039 places overlay 173 `+1168` in `+0C65`; `+0C65` returns at `+1087` and `+1168` is in
`+1098`. Which code stores 0 in slot record 21's word `+0` each pass (`1000:3F9C`) was not found;
the slot release `2D40:03C1`, reached from overlay 197 `+29E0` through `31E0:05BB` and `31E0:044C`,
fills a low slot's walk record and its saved entry with zeros, but that it runs for slot 21 on
each pass was not shown. Whether `+0C65` takes the initiative branch, with its two draws per slot,
was not settled: FND-COMBAT-056 finds no change in `4C4F` against the run without the click, which
favours the other branch. That word `+0x02` of a combatant is its psionic points rests on
FND-PARTY-081. Overlay 193 `+0000`, `+1873`, `+194F`, `+1BA3`, `+1C95`, overlay 201 `+0410`,
`1BF3:4FEB`, `1BF3:4723` and the readers of `4C13:035B` other than `371D:0334` were not read.
Whether the input flush explains the refused keys depends on where the main loop reads keys,
which was not read.

## How to reproduce

`overlay_listing.py <dsun> 173 5AEBA 5B175`, `... 173 5B585 5B9A8`, `... 173 5B9B8 5BB58`, `...
173 5DC24 5DE0C`, `... 173 5E5FA 5E78B`, `... 176 6190D 61AC6`, `... 188 746C0 74716`, `... 195
82208 82269`, `... 195 82C45 82C85`, `... 199 884C1 88675`, `... 197 85DE0 860F2`, `... 210 95AA4
95AB5`; `resident_listing.py <dsun> 371D:0334..371D:050F 371D:0770..371D:078E 3794:0000..3794:0010
2D40:3D4C..2D40:3DC0 2D40:03C1..2D40:0453 1B0B:0000..1B0B:00B0 45B9:0034..45B9:0059`;
`direct_callers.py <dsun> 371D:01F9 173+1098 195+00D1 1B0B:000C`; `segment_offset_refs.py <dsun>
50C2 0000`.
