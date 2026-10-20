---
id: RULE-MAGIC-013
title: DATA 71 rolls a 4d20 pool and kills listed combatants below level 9, lowest band first, each costing 1, 2, 10 or 20 of the pool
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-040, FND-RNG-034, FND-PARTY-086, FND-ACTOR-005, FND-EXPLORE-001, FND-EXPLORE-002, FND-COMBAT-022, FND-COMBAT-052, FND-COMBAT-023]
conflicting: []
split_with: []
related: [RULE-RNG-001]
---

## Summary

`data_71_pool` rolls a pool of 4d20. It goes through four level bands in turn, and in each through
the listed slots in order, while the pool is above 0. A slot whose kind is 2 has a level: its
details byte `+0x1E` when its details byte `+0x12` is 1, and otherwise the greatest of its bytes
`+0x1E`, `+0x1F` and `+0x20`. A level below 2 is taken in band 0 at a cost of 1, 2 to 4 in band 1
at 2, 5 and 6 in band 2 at 10, and 7 and 8 in band 3 at 20. A taken slot takes 10,000 damage, and
its cost is taken from the pool even when that leaves the pool below 0.

## When it runs

Overlay 193 `+003C`, which runs a queued record (FND-PARTY-107), calls it at `+046B` for `DATA`
71, with one entry for a single target or the list overlay 208 `+0000` builds (FND-RNG-040).

## Parameters

- `count: INT16`: the number of entries.
- `entries`: the list, four-byte entries whose word `+2` is a slot.

## Inputs

`object_slot_kinds`; `combatant_details`: bytes `+0x12`, `+0x1E`, `+0x1F` and `+0x20`
(`unk_10[2]`, `unk_10[14]`, `unk_10[15]` and `unk_10[16]` of FMT-COMBAT-002) of each slot's details
record, through `object_slot_combatants` and `details_index`.

## Procedure

```text
define data_71_pool(count: INT16, entries):
    # roll_sum draws four times (RULE-RNG-001)
    let pool: INT16 = INT16(roll_sum(4, 20))
    let band: INT16 = 0
    while band < 4 and pool > 0:
        let j: INT16 = 0
        while j < count and pool > 0:
            let slot = entries[j].word(2)
            if object_slot_kinds[slot] == 2:
                let d = combatant_details[combatants[object_slot_combatants[slot]].details_index]
                let level: UINT8 = d.unk_10[14]
                if d.unk_10[2] != 1:
                    level = max(d.unk_10[14], d.unk_10[15], d.unk_10[16])
                let cost: INT16 = 0
                if level < 2:
                    if band == 0:
                        cost = 1
                else if level < 5:
                    if band == 1:
                        cost = 2
                else if level < 7:
                    if band == 2:
                        cost = 10
                else if level < 9:
                    if band == 3:
                        cost = 20
                if cost != 0:
                    pool = pool - cost
                    emit DamageDealt(slot, 10000)
            j = j + 1
        band = band + 1
```

## Outputs

`DamageDealt` for each taken slot, through overlay 179 `+2A05` with `DATA` number 71 and code -1
(FND-RNG-034).

## Edge cases

The pool is 4 to 80. A level of 9 or more is never taken. Each slot falls in one band, so it is
taken at most once a call. A pool of 1 still takes a band-3 slot when it reaches one, and the
call then stops.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Which spell `DATA` 71 is, what details byte `+0x12` 1 and bytes `+0x1E` to `+0x20` stand for,
  and which slots overlay 208 `+0000` lists (FND-RNG-040, Q-MAGIC-011).
