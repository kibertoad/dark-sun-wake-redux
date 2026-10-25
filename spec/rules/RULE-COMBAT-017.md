---
id: RULE-COMBAT-017
title: At the start of its round a combatant's effect block picks one of four holds or effects to handle, then up to five more, with d20, d4 and 2d4 damage, escape checks and a returned 8 plus 2d4 hits
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-049, FND-RNG-089, FND-PARTY-102, FND-PARTY-099, FND-PARTY-112, FND-RNG-040, FND-RNG-041, FND-RNG-044, FND-RNG-047, FND-PARTY-086, FND-PARTY-096, FND-EXPLORE-001, FND-EXPLORE-002, FND-COMBAT-052, FND-COMBAT-022, FND-COMBAT-023, FND-COMBAT-025]
conflicting: []
split_with: []
related: [RULE-COMBAT-012, RULE-COMBAT-013, RULE-COMBAT-005, RULE-RNG-001]
---

## Summary

`effect_round` is overlay 195 `+00D1`, run for a combatant at the start of its round. It first
clears effect 0x1E from every slot below 48 whose `combat_mark` is 3 or more. Then the first of
four bits of the combatant's effect block that is set picks one step: a hold of effect 0x1E that
a party member tries to escape on `byte_19_check` and that deals the entry's byte `+9` in d20 a
round; a hold of effect 0x64 that lowers a byte each round, kills at 0, and lets a party member
try to escape when the entry's word `+4` is 0x122; an effect 0x10 that offers a saving throw
against `DATA` 188 and deals 1d4 plus a term; or an effect 0x32 that may end effect 0x45. Five
more steps follow, each when its condition holds: a kind-16 creature's hold of effect 0x18 that
counts to 4 and then kills the held slot, 1d4 for effect 0x65, 2d4 for effects 1 and 0x13, and
for effect 0x6F hits of 8 and then 2d4 on the effect's source while the source holds effect 0x6E.

## When it runs

Overlay 173 `+1098`, which picks the combatant to act, calls it for a slot when overlay 173 `+2EC7` of the slot returns a value with
a bit other than 0x10 set (FND-RNG-089).

## Parameters

- `s: INT16`: the combatant's object slot.

## Inputs

`effect_block` of `s` and of the source in step 5; `fn_3143_00A8`'s entries and their words
`g_4E71_024F` and `g_4E71_0251` and bytes `g_4E71_0255` and `g_4E71_0256`; through
`object_slot_combatants`, the `combatants` records' `combat_mark` and byte `+0x1B` (`unk_19[2]`
of FMT-COMBAT-001), and `s`'s `combatant_details` byte `+0x12` (`unk_10[2]` of FMT-COMBAT-002);
`combatants` record `s`'s `name`, read by its own number.

## Procedure

```text
define escape_messages(s, escaped: UINT8):
    let name = combatants[s].name
    if escaped != 0:
        emit NamedMessageShown(message_escapes, name)
    else if name_length(name) + 17 > 29:
        emit MessageShown(message_help)
    else:
        emit NamedMessageShown(message_tries_to_escape, name)

define round_damage(target, source, code, amount: INT16):
    fn_56A7_0066(target, source, -1, code, amount, amount, 0, 0, 1, 0xFF, 1, 0)

define effect_round(s):
    let B = effect_block(s)
    fn_573B_005C(s, 1)
    for c in 0..48:
        if fn_3143_00A8(c, 0x1E) != -1 and combatants[object_slot_combatants[c]].combat_mark >= 3:
            emit EffectRemoved(c, 0x1E)
    let R = combatants[object_slot_combatants[s]]
    let D = combatant_details[R.details_index]
    if B[3] & 0x40 != 0:
        if s < 4:
            let escaped = byte_19_check(s)
            if escaped != 0:
                emit EffectRemoved(s, 0x1E)
            escape_messages(s, escaped)
            let e = fn_3143_00A8(s, 0x1E)
            if e != -1:
                # roll_sum draws once per die (RULE-RNG-001)
                round_damage(s, g_4E71_024F[e], 0x40, roll_sum(INT16(INT8(g_4E71_0256[e])), 20))
    else if B[12] & 0x10 != 0:
        R.unk_19[2] = UINT8(R.unk_19[2] - 1)
        if R.unk_19[2] == 0:
            fn_5671_005C(s, 1000)
        let e = fn_3143_00A8(s, 0x64)
        if g_4E71_0251[e] == 0x122 and s < 4:
            let escaped = byte_19_check(s)
            if escaped != 0:
                fn_575A_008E(0x122, s)
            escape_messages(s, escaped)
    else if B[2] & 0x01 != 0:
        let e = fn_3143_00A8(s, 0x10)
        emit EffectRemoved(s, 0x45)
        let save = UINT8(fn_56A7_0075(s, g_4E71_024F[e], 188, fn_573B_0052(s, 0x10)))
        if save == 0:
            emit EffectApplied(s, g_4E71_024F[e], 188, 0x45, -9999, 0)
        else:
            emit EffectRemoved(s, 0x10)
        # roll_sum draws once (RULE-RNG-001)
        let a: INT16 = INT16(roll_sum(1, 4) + fn_5671_007A(s, s, 0))
        if a > 0:
            round_damage(s, g_4E71_024F[e], 0x20, a)
    else if B[6] & 0x04 != 0:
        if byte_19_check(s) != 0:
            fn_573B_0057(s, 0x32)
            let e = fn_3143_00A8(s, 0x32)
            if e != -1 and g_4E71_0255[e] < 2:
                emit EffectRemoved(s, 0x45)
    if D.unk_10[2] == 0x10 and B[3] & 0x01 != 0:
        let e = fn_3143_00A8(s, 0x18)
        let v = g_4E71_024F[e]
        let eaten = combatants[object_slot_combatants[v]].combat_mark >= 3
        if not eaten:
            let held = effect_block(v)
            if held[8] & 0x24 != 0:
                if g_4E71_0255[e] >= 4:
                    eaten = true
                else:
                    g_4E71_0255[e] = g_4E71_0255[e] + 1
            else:
                fn_575A_008E(0x12D, s)
        if eaten:
            emit MessageShown(message_brain_eaten)
            g_57E0_444B = s
            fn_575A_008E(0x12D, s)
            fn_5671_005C(v, 1000)
    if B[12] & 0x20 != 0:
        let e = fn_3143_00A8(s, 0x65)
        # roll_sum draws once (RULE-RNG-001)
        round_damage(s, g_4E71_024F[e], 2, roll_sum(1, 4))
    if B[0] & 0x02 != 0:
        let e = fn_3143_00A8(s, 1)
        # roll_sum draws twice (RULE-RNG-001)
        round_damage(s, g_4E71_024F[e], 0x40, roll_sum(2, 4))
    if B[2] & 0x08 != 0:
        let e = fn_3143_00A8(s, 0x13)
        # roll_sum draws twice (RULE-RNG-001)
        round_damage(s, g_4E71_024F[e], 2, roll_sum(2, 4))
    if B[13] & 0x80 != 0:
        let v = g_4E71_024F[fn_3143_00A8(s, 0x6F)]
        if combatants[object_slot_combatants[v]].combat_mark < 3:
            if fn_3143_00A8(v, 0x6E) == -1:
                emit EffectRemoved(s, 0x6F)
            else:
                # roll_sum draws twice for m and twice for each later hit (RULE-RNG-001)
                let m: INT16 = INT16(roll_sum(2, 4))
                for i in 0..m + 1:
                    let a: INT16 = 8
                    if i > 0:
                        a = INT16(roll_sum(2, 4))
                    round_damage(v, s, 0x10, a)
                if combatants[object_slot_combatants[v]].combat_mark >= 3:
                    emit EffectRemoved(s, 0x6F)
    fn_5671_00B1()
```

`round_damage` passes the amount twice, first as a far pointer to a local word that holds it.
When the byte `g_50C2_0000` is 0 the original also near-calls overlay 195 `+0A1F` with `s` before
the last call; that routine returns at once (FND-RNG-049).

## Outputs

The removed and applied effects, the damage through `fn_56A7_0066` and `fn_5671_005C`, the
messages, the lowered byte `+0x1B`, the counted entry byte `+8` and `g_57E0_444B`.

## Edge cases

Only one of the four chain steps runs, even when several of their bits are set, and the five
later steps read the block filled at the start, before the chain changes any effect. An entry
that `fn_3143_00A8` does not find is -1 in steps 2, 3, 5, 6, 7, 8 and the first lookup of step
9, which read the bytes before the entry table; steps 1 and 4 and the lookup of effect 0x6E test
for it. Step 1 looks the entry up after an escape has removed effect 0x1E, so an escape makes no
roll. Step 3 reads the source word again for the damage after the effect may have been removed.
A name of 13 to 16 characters gives the short message in place of the failed-escape one; the
escape message is never too long. The d20 count is byte `+9` sign-extended, so a byte above 127
asks `roll_sum` for a negative count. Step 9 makes `m + 1` hits, one more than its 2d4.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What effects 0x1E, 0x64, 0x10, 0x32, 0x45, 0x18, 0x65, 1, 0x13, 0x6E and 0x6F are, what
  `effect_block`'s bits, `g_4E71_0251` 0x122, `g_4E71_0255` and details kind 16 stand for, what
  reads `g_57E0_444B`, and what `fn_573B_005C`, `fn_5671_007A`, `fn_5671_00B1`, `fn_575A_008E`
  with 0x122 and 0x12D, and the words `fn_56A7_0066` takes after the code do (FND-RNG-049,
  Q-COMBAT-033).
- Whether `g_4E71_024F` is the effect's source and `g_4E71_0256` its strength, what
  `fn_3143_00A8` returns beyond an entry number or -1, what `fn_573B_0052` and `fn_573B_0057`
  return and do, what `fn_5671_005C` does with 1000 beyond the damage, what the saving throw
  `fn_56A7_0075` adds through overlay 197, and what `EffectApplied` and `EffectRemoved` do in
  overlay 193 (FND-RNG-089, FND-RNG-049, Q-COMBAT-033).
