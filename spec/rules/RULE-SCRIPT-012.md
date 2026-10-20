---
id: RULE-SCRIPT-012
title: Function 41 of script opcode 0x22 rolls a saving throw for each slot its target names and returns how many saved
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-048, FND-RNG-035, FND-PARTY-108, FND-PARTY-156, FND-PARTY-237, FND-PARTY-166, FND-CONFIG-141, FND-PARTY-112, FND-RNG-039, FND-ACTOR-005, FND-EXPLORE-001, FND-EXPLORE-002, FND-COMBAT-052, FND-COMBAT-022, FND-COMBAT-023, FND-EXPLORE-005, FND-PARTY-013, FND-SOUND-010]
conflicting: []
split_with: []
related: [RULE-SCRIPT-002, RULE-PARTY-014, RULE-RNG-001]
---

## Summary

`count_saves` is function 41 of script opcode `0x22`, overlay 204 `+0169`'s case at `+054F`. It
lists the slots that parameter 1 names and that hold a combatant, and for each one rolls a d20
saving throw against save number parameter 2, with the low byte of parameter 3 as a bonus. A
natural 1 fails and a natural 20 saves; otherwise the roll plus a modifier from the target's
effect block and ability tables, plus the bonus, saves when it reaches the save. The instruction's
value is the number of slots that saved. No shipped script calls function 41.

## When it runs

When a script runs an opcode `0x22` instruction whose function is 41 (FND-PARTY-108,
FND-RNG-048). No `GPL ` or `MAS ` resource has such an instruction (FND-RNG-048).

## Parameters

- `t: INT16`: parameter 1, the target word: an object slot, 0x7FFE for the party, or a search
  word for `fn_2D40_0D94`.
- `p2: INT32`: parameter 2; its low word is the save number.
- `p3: INT32`: parameter 3; its low byte is the bonus.

## Inputs

`object_slot_kinds`, `object_slot_combatants`, `party_shown`, `leader`, `effect_block`; through
`object_slot_combatants`, the slot's `combatants` record: `combat_mark`, `character_id` and byte
`+0x1B` (`unk_19[2]` of FMT-COMBAT-001); its `combatant_details` record: bytes `+0x12`
(`unk_10[2]` of FMT-COMBAT-002) and `+0x30 + s` (`unk_10[0x20 + s]`).

## Procedure

```text
define listed_slots(t: INT16, kind: INT16) -> INT16[]:
    let out: INT16[] = []
    if t > 0 and t < 9999:
        if object_slot_kinds[t] == kind or kind == -1:
            append(out, t)
        return out
    if t == 0x7FFE:
        if party_shown != 0:
            for p in 0..4:
                if combatants[p].character_id != 0:
                    append(out, p)
        else:
            append(out, leader)
        return out
    let cursor: INT16 = 0
    while cursor < 320:
        # fn_2D40_0D94 advances cursor
        let u = fn_2D40_0D94(t, cursor)
        if u == 9999:
            break
        if object_slot_kinds[u] == kind or kind == -1:
            append(out, u)
    return out

define script_save_modifier(t: INT16, block: UINT8[], s: UINT16) -> INT16:
    let R = combatants[object_slot_combatants[t]]
    let D = combatant_details[R.details_index]
    let m: INT16 = 0
    if block[5] & 0x10 != 0:
        m = m - 2
    if block[11] & 0x01 != 0:
        m = m - 1
    if block[11] & 0x40 != 0 and s != 1:
        m = m + 3
    if block[0] & 0x40 != 0:
        m = m + 1
    if block[1] & 0x02 != 0:
        m = m + 1
    if block[9] & 0x01 != 0:
        m = m + 1
    if s != 1:
        m = m - fn_27E5_02F2(t)
    else:
        if D.unk_10[2] == 2 or D.unk_10[2] == 6:
            m = m + INT16(R.unk_19[2]) * 2 / 7
        m = m + fn_27E5_030B(t)
    return m

define script_saving_throw(t: INT16, s: UINT16, x: UINT8) -> UINT8:
    let block = effect_block(t)
    let R = combatants[object_slot_combatants[t]]
    let D = combatant_details[R.details_index]
    if block[8] & 0x20 != 0 or R.combat_mark == 3:
        return 0
    let need: UINT8 = D.unk_10[0x20 + s]
    # roll_sum draws once (RULE-RNG-001); its low byte is kept
    let r: UINT8 = UINT8(roll_sum(1, 20))
    if r == 1:
        return 0
    if r == 20:
        return 1
    if block[2] & 0x80 != 0:
        return 0
    let sum: UINT8 = UINT8(r + UINT8(script_save_modifier(t, block, s)) + x)
    if sum >= need:
        return 1
    return 0

define count_saves(t: INT16, p2: INT32, p3: INT32) -> UINT16:
    let n: UINT16 = 0
    for u in listed_slots(t, 2):
        if script_saving_throw(u, UINT16(p2), UINT8(p3)) != 0:
            n = n + 1
    return n
```

## Outputs

The count, as the value of the opcode `0x22` instruction. Nothing is stored. One draw for each
listed slot that passes the first test of `script_saving_throw`.

## Edge cases

The sum is a byte, so a large modifier or bonus wraps: a negative modifier can make the sum 200
or more and save, and a sum above 255 starts again from 0. The comparison with the save is
unsigned. Save numbers 1 to 5 read the five saves of RULE-PARTY-014; 6 reads byte `+0x36`, and 7
or more reads past `unk_10` into `footprint` and the bytes after it. Save number 1 takes the
constitution terms, and every other number takes the dexterity term instead. For a target of
0x7FFE the kind test is skipped, and the party slots are tested by their own `combatants` record
numbers 0 to 3, while `script_saving_throw` reads the record through `object_slot_combatants`.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Which slots `fn_2D40_0D94` gives for a negative target word, whether `fn_27E5_02F2` and
  `fn_27E5_030B` are the dexterity and constitution adjustments, what the `effect_block` bits,
  details bytes `+0x12` 2 and 6 and `combat_mark` 3 stand for, and whether any script reaches
  function 41 (FND-RNG-048, Q-SCRIPT-010).
