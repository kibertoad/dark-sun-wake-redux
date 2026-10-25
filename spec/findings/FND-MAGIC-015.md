---
id: FND-MAGIC-015
title: Overlay 197 +3060 skips the spend of a wizard spell only when +37A5 returns nonzero, and +37A5 returns nonzero only for a spell whose overlay 177 +09C5 level is 0, which +09C5 never gives a number from 0 to 114; so effect 0x5C never saves a wizard spell, and the walk only rewrites word 0x0E of items whose word 0x10 is 313
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00086460..0x000864ED
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00086BA5..0x00086CC7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FF85..0x0007FFA6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3143:0061..3143:00A8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062AC5..0x00062B45
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D656..0x0004D660
tool: tools/research/exec-census/overlay_listing.py, resident_listing.py, direct_callers.py and item_words.py at the commit that adds this finding; Capstone 5.0.7; Python 3.14.7; a short read of the 10 bytes at file 0x4D656
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x83400 + x` for overlay 197, `0x7E2F0 + x` for 193 and
`0x62100 + x` for 177. Effect rows are the 10-byte rows at `4E71:024D`: word `+0` the owner slot,
word `+2` (`4E71:024F`) the source slot (FND-RNG-036), word `+4` (`4E71:0251`) the number, byte
`+6` (`4E71:0253`) the effect code; `DS:2100` is the row count (FND-COMBAT-032).

**The spend.** Overlay 197 `+3060` (`+3060..+30ED`) takes a slot `s` and a number `n`. It calls
overlay 193 `+1C95` with `s` and 0x5C (`+306B..+3073`). When that is nonzero and overlay 177
`+0A1E` of `n` is 1 (`+307A..+3084`), it calls the near routine `+37A5` with `s` and `n`
(`+3086..+308A`) and returns without a change when `AL` is nonzero (`+3090..+3092`). Otherwise,
when `+300D` of `s` and `n` is nonzero, it lowers the pool byte of `n`'s kind and level by 1
(`+3094..+30E7`), as FND-MAGIC-002 reads. `direct_callers.py <dsun> 197+3060` finds far calls at
overlay 174 `+0D2E` and overlay 190 `+2632`.

**Effect 0x5C.** Overlay 193 `+1C95` (`+1C95..+1CB6`) calls resident `3143:0061` with the slot and
the code byte and returns 1 when the result differs from `DS:2100`, 0 otherwise. `3143:0061`
(`3143:0061..3143:00A8`) scans the slot's rows from `3143:000D`'s index and returns the index of
the first whose code byte `4E71:0253` is the code, or `DS:2100` when there is none. So the test is
"the slot holds an effect row with code 0x5C". RULE-MAGIC-017 reads `DATA` 312 cast on the caster
itself as applying code 0x5C with duration -9999 (FND-RNG-047), and FND-PARTY-112 reads overlay
197 `+111A` ending any hit of a number below 235 on a target that holds code 0x5C.

**`+37A5`** (`+37A5..+38C7`) takes the slot (`bp+6`) and `n` (`DI`). It clears its result byte
`bp-5` (`+37B1`), has `2D40:2196` list the slot's held objects into an 800-byte stack buffer
(`+37B5..+37C6`), and for each entry (`+37C9..+38BD`) keeps those with entry word `+2` equal to 1
(`+37DE`), overlay 173 `+2DD0` of the buffer and the index equal to 0x10 (`+37E6..+37F8`), and
word `+0x10` of the entry's 23-byte record at `DS:19C1` less 1 equal to 0x138, that is word
`+0x10` of 313 (`+37FD..+3820`). For a kept entry it saves the record's byte `+0x0E` in `bp-7`
(`+3825..+3841`) and sets the byte `bp-6` to 0 (`+385A`). It calls overlay 177 `+09C5` of `n`
and, when the result is above 0 unsigned, goes straight to the store (`+385E..+3868`). Only when
the result is 0 does it subtract a second `+09C5` result from `bp-6`, set `bp-5` to 1, and lower
`bp-7` by 1 when `28C9:391D` of 1 and 100 is below 10 (`+386A..+388B`). It then stores the word
`bp-7 | bp-6 << 8` at record word `+0x0E` (`+388E..+38B3`) and, after the walk, returns `bp-5`
(`+38C0..+38C6`). FND-PARTY-249 reads the same body. `direct_callers.py <dsun> 197+37A5` finds the
near call at `+308A` and the trampoline `575A:00B1`, with no far caller.

**The level.** Overlay 177 `+09C5` (`+09C5..+0A1E`) takes `n`. For 0 to 114 it uses the ten bytes
at `DS:0656`, for 115 to 234 those at `DS:0661`, returns 3 for 269 to 304 and 0 otherwise
(`+09C9..+09FC`). With a table it starts at 10 and returns the first `i` whose byte at
`table + i - 1` is not above `n` (unsigned compare against `n`), or 0 when `i` reaches 0
(`+09FE..+0A19`). The ten bytes at `DS:0656` (file `0x4D656`) are 0, 12, 25, 39, 56, 68, 81, 92,
102 and 111. The first is 0, so for every `n` from 0 to 114 the loop stops at `i` 1 or above:
`+09C5` gives 1 to 10 and never 0. Overlay 177 `+0A1E` (`+0A1E..+0A45`) returns 1 for 0 to 114,
2 for 115 to 234 and 0 otherwise.

**Shipped items.** `item_words.py` with 313 over `OBJEX.GFF` finds 708 item chunks and over
`CHARSAVE.GFF` 172, and neither has an item whose word `0x10` is 313.

## Interpretation

`+3060` reaches `+37A5` only for a number of kind 1, so `n` is 0 to 114 and `+09C5` of `n` is 1 to
10. `+37A5` then takes the branch that skips the change byte for every kept item, and returns 0.
`+3060` goes on to the normal spend. A wizard spell is always spent from its pool, with or without
effect 0x5C on the caster; priest spells never reach the test. The branch that would return 1 is
reachable only for a number whose `+09C5` level is 0 (235 to 268, 305 and above, negative), and
`+3060` never passes such a number to `+37A5`.

What the call does do, when the caster holds effect 0x5C and spends a wizard spell, is rewrite
word `+0x0E` of each kept item to its own low byte with a high byte of 0. No shipped item has word
`0x10` 313, so in the shipped data that rewrite touches nothing unless an item gets that word at
run time, or a kind-1 root entry numbered 16 passes the selector (the root exception of
FND-PARTY-249).


## Alternatives

- Calls of `+37A5` through its trampoline built at run time are outside `direct_callers.py`.
- A change of the bytes at `DS:0656` at run time would change `+09C5`; no store to them was
  searched for here. FND-PARTY-110 and FND-MAGIC-004 read the same table as fixed.
- What the item with word `0x10` 313 is, and what its word `+0x0E` bytes mean, stays with
  Q-PARTY-106 (FND-PARTY-249).

## How to reproduce

From `tools/research/exec-census/` with the evidence Python and `<dsun>` the installed
`DSUN.EXE`: `overlay_listing.py <dsun> 197 86460 864F0`, `... 197 86BA5 86D00`,
`... 193 7FF85 80040`, `... 177 62AC5 62B45`; `resident_listing.py <dsun> 3143:0061..3143:00A8`;
`direct_callers.py <dsun> 197+37A5 197+3060`; `item_words.py <OBJEX.GFF> 313` and
`item_words.py <CHARSAVE.GFF> 313`. Read the 10 bytes at file `0x4D656`. Evaluate `+09C5` for `n`
from 0 to 114 and check that no value is 0.
