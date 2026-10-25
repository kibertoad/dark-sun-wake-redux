---
id: RULE-MAGIC-014
title: Using a psionic power takes its cost in points and a d20 power check below the power score plus the rank, which an effect of code 58 can contest
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-046, FND-RNG-025, FND-PARTY-104, FND-PARTY-108, FND-PARTY-210, FND-RNG-042, FND-PARTY-081, FND-CONFIG-052, FND-COMBAT-022, FND-COMBAT-023, FND-COMBAT-052, FND-RNG-040, FND-EXPLORE-001, FND-EXPLORE-002, FND-MAGIC-007, FND-MAGIC-008]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-MAGIC-003, RULE-MAGIC-015]
---

## Summary

`use_power` is how the `DATA` dispatcher runs a `DATA` number from 235 to 268, psionic power
`p = d - 235`. A slot that may use the power and has the points pays the power's cost and makes
`power_check`: a d20 that succeeds on 1, fails on 20, and otherwise succeeds when it is at most
the power score (a score of the slot, the power's adjustment and its rank, less 1). While an
effect entry of code 58 belongs to another slot, two more d20 rolls against that slot's power 32
come first and can disrupt the power; the owner of that entry is always disrupted. A failed check
costs half the power's table cost. A successful one marks the power in effect for a party member
and applies the `DATA` number. The die is kept in `psionic_check_result`, which the psionic
defence of RULE-MAGIC-015 reads.

## When it runs

Overlay 193 `+2290`, the `DATA` dispatcher, calls it for `d` from 235 to 268 when its byte argument
at `bp+0x1A` is 0 (FND-PARTY-108). The dispatcher's eleven callers are in overlays 172, 173, 174,
189, 208, 211 and 213 (FND-RNG-046).

## Parameters

- `s: INT16`: the slot using the power.
- `t: INT16`: the second slot, the target.
- `p: INT16`: the power, 0 to 33.
- `w: UINT16[4]`: the dispatcher's four word arguments from its `bp+0xE`, passed on.
- `q: UINT8[]`: a near pointer to a byte in the data segment, or 0.
- `b: UINT8`: the dispatcher's byte argument at `bp+0x1C`, passed on.

## Inputs

`psionic_powers`, `psionic_ranks`, `psionic_check_result`, `effect_entries`; through
`object_slot_combatants`, the word `+0x02` of a slot's `combatants` record (`unk_02` of
FMT-COMBAT-001, its points) and its scores `fn_27E5_04A0`; for slots 0 to 3, `combat_mark` of
`combatants` record `s` and word `+0x0E` (`unk_0E` of FMT-COMBAT-002) of `combatant_details`
record `s`; `g_4C4F_00B2`, `combat_state`; for powers 15, 20 and 21, the byte `+0x15` (`unk_10[5]`)
of `t`'s details record and `fn_27E5_0154`; for the applied number, the class bytes `+0x1B..+0x20`
(`unk_10[11..16]`) of `s`'s details record.

## Procedure

```text
define psionic_rank(s, p) -> UINT8:
    if s >= 4:
        return 1
    return (psionic_ranks[s][p] >> 1) & 0x7F

define power_known(s, p) -> UINT8:
    if s >= 4:
        return 1
    return 1 if psionic_rank(s, p) != 0 else 0

define points_of(s) -> UINT16:
    return UINT16(combatants[object_slot_combatants[s]].unk_02)

define power_usable(s, p) -> UINT8:
    let in_effect = 0
    if s < 4:
        if combatant_details[s].unk_0E == 0:
            return 0
        if combatants[s].combat_mark != 1:
            return 0
        if fn_573B_0020(s, 0x43) != 0:
            return 0
        in_effect = psionic_ranks[s][p] & 1
    if points_of(s) < psionic_powers[p].cost:
        return 0
    if in_effect != 0:
        return 0
    if INT8(g_4C4F_00B2[s]) == 0 or combat_state == 0:
        return 1
    return 0

define power_check(s, p, extra: UINT8) -> INT8:
    if UINT8(power_known(s, p)) == 0:
        return 0
    let o: UINT16 = 9999
    for e in 0..192:
        if effect_entries[e].code == 58:
            o = effect_entries[e].slot
            break
    if o != 9999:
        if o == s:
            return -1
        let opposing: INT16 = INT16(fn_27E5_04A0(o, 3)) + psionic_powers[32].adjust + psionic_rank(o, 32) - 1
        let rank: INT16 = psionic_rank(s, p)
        # random_between draws once each (RULE-RNG-001); only the low bytes are kept
        let a: INT8 = INT8(random_between(1, 20))
        let c: INT8 = INT8(random_between(1, 20))
        if a > rank:
            return -1
        if not (c > opposing or c < a):
            return -1
    let k = psionic_powers[p].score_code
    let threshold: INT8 = INT8(UINT8(fn_27E5_04A0(s, k) + extra + psionic_powers[p].adjust + psionic_rank(s, p) - 1))
    # random_between draws once (RULE-RNG-001)
    let r: INT8 = INT8(random_between(1, 20))
    if r == 20:
        return 0
    if threshold >= r or r == 1:
        return r
    return 0

define class_code_level(s, code) -> UINT8:
    let d = combatant_details[combatants[object_slot_combatants[s]].details_index]
    for i in 0..3:
        if d.unk_10[11 + i] == code:
            return d.unk_10[14 + i]
    return 1 if code == 12 else 0

define use_power(s, t, p, w, q, b) -> UINT8:
    let charge: UINT16 = w[0]
    let held = q != 0 and q[0] != 0
    if not held:
        if UINT8(power_usable(s, p)) != 0:
            if p == 15:
                let e = combatants[object_slot_combatants[t]].details_index
                charge = (24 - combatant_details[e].unk_10[5]) * 2
            else if p == 20 or p == 21:
                let level: INT16 = fn_27E5_0154(t)
                charge = 3 if level < 6 else 8 if level < 11 else 13 if level < 16 else 18
            else:
                charge = psionic_powers[p].cost
            if INT16(charge) < 0:
                charge = 0
            if points_of(s) < charge:
                if s < 4:
                    emit MessageShown("Insufficient psps")
                return 0
            psionic_check_result = power_check(s, p, 0)
            # visible: psionic_check_result is read by later uses and by RULE-MAGIC-015
            if psionic_check_result > 0:
                if psionic_powers[p].byte_2 != 99 and s < 4:
                    psionic_ranks[s][p] = psionic_ranks[s][p] | 1
            else:
                if psionic_check_result == -1:
                    emit MessageShown("psionics disrupted by static")
                else if s < 4:
                    emit MessageShown("psionic power check fails")
                combatants[object_slot_combatants[s]].unk_02 = points_of(s) - psionic_powers[p].cost / 2
    if psionic_check_result <= 0:
        return 0
    if not held:
        combatants[object_slot_combatants[s]].unk_02 = points_of(s) - charge
    return fn_573B_007F(s, t, p + 235, class_code_level(s, 12), w, q, b)
```

`charge` starts as `w[0]` because the routine keeps the cost in a register that holds the
dispatcher's word argument until the cost is computed. When `power_usable` returns 0, or `q`
points to a nonzero byte, the use goes on with the `psionic_check_result` an earlier use left: it
applies the number when that is above 0, taking `w[0]` from the points in the first case and
nothing in the second.

## Outputs

The low byte `fn_573B_007F` returns when the number is applied, and 0 otherwise. Changes the points
word of `s`'s combatant record, `psionic_check_result`, and bit 0 of `psionic_ranks[s][p]`.

## Edge cases

`power_check` makes no draw for a power the slot does not know, or when the owner of the first
effect entry of code 58 is `s`; one draw with no such entry; three with another owner, even when
the first contest roll already decides it. A check that returns -1 or 0 leaves bit 0 alone and
charges `cost / 2`, rounded down, even for powers 15, 20 and 21, whose success charge is computed.
Points are compared and subtracted as unsigned words. A failed check's half cost cannot wrap them,
since `power_usable` needed the points to be at least the cost; `w[0]`, taken on the path that
skips `power_usable`'s success, can.
The threshold's sum keeps only a byte and is compared signed, so a sum of 128 or more wraps
negative and only a 1 then succeeds. The effect-entry scan reads all 192 entries whatever the count
of entries in use. `psionic_check_result` is 0 when the game starts.

## What the sources say

SRC-MANUAL-1994, page 69, gives the chance of activating a power as five percent per point of the
power score, the ability score plus the power's amount, and half the initial cost for a failed
check (RULE-MAGIC-003). The game's check adds the rank less 1 to that score, always succeeds on a 1
and always fails on a 20.

## Differences between builds

None known.

## Open questions

- What effects the codes 58 and 0x43 are and what puts them on a slot, and so what contests or
  blocks a use through `effect_entries` and `fn_573B_0020` (FND-RNG-046, Q-MAGIC-015).
- What `g_4C4F_00B2` stands for, and what `fn_27E5_0154` returns beyond FND-PARTY-081's reading
  (FND-RNG-040, FND-PARTY-081, Q-MAGIC-015).
- What `fn_27E5_04A0` returns for each `score_code`: read as the combatant record's bytes `+0x19`
  to `+0x1D`, codes 2 to 4 being Constitution, Intelligence and Wisdom (FND-MAGIC-002,
  FND-MAGIC-007, Q-MAGIC-018).
- Which callers of the dispatcher pass a psionic number, with which `w` and `q`, whether any
  reaches the path that applies an earlier check's result, and what `fn_573B_007F` does with the
  number (FND-RNG-046, Q-MAGIC-013).
