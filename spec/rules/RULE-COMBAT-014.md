---
id: RULE-COMBAT-014
title: At fight set-up each combatant gets an order word of a d10 plus a dexterity term, six effect terms and 20, and a d200 tie word, unless the side byte spares its group the draws
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-041, FND-RNG-042, FND-PARTY-244, FND-PARTY-166, FND-COMBAT-036, FND-EXPLORE-001, FND-EXPLORE-002, FND-COMBAT-022, FND-PARTY-112, FND-RNG-089, FND-ACTOR-005, FND-COMBAT-052]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-COMBAT-012]
---

## Summary

`set_order_words` is a step of overlay 173 `+0C65`. For a slot it sets up, it writes two words
for the slot's combatant. The first is a draw of 0 to 9, plus a term from the dexterity table at
`DS:084C`, plus the effect terms of `order_effect_terms`, plus 20; the second is a draw of 0 to
199. When the byte `g_4C4F_0006` is 0, 1 or 2 and the combatant's group is in the matching mask,
there are no draws: the first word is the two terms plus 30 and the second 200. The routine that
picks the next combatant to act takes the greatest first word, and the greater second word on a
tie (FND-RNG-042).

## When it runs

In `+0C65`, which the fight routine calls (FND-PARTY-097), for each slot from 0 to 47 that meets
FND-PARTY-244's conditions, after the slot's trigger call (FND-RNG-041). A slot that fails them
gets -1 as its first word and keeps its second.

## Parameters

- `slot: INT16`: the object slot.

## Inputs

`object_slot_kinds`, `object_slot_combatants`; `combatants`: byte `+0x15` (`unk_15[0]` of
FMT-COMBAT-001) and byte `+0x1A` (`unk_19[1]`) of the slot's record; the slot's `effect_block`;
`g_4C4F_0006`; the 26 signed bytes of `g_57E0_084C`: 0, -6, -4, -3, -2, -1, 0 for 6 to 15, 1, 2,
2, 3, 3, 4, 4, 4, 5, 5 (FND-RNG-042).

## Procedure

```text
define dexterity_term(slot):
    let score: UINT16 = 9
    if UINT16(slot) < 9999 and object_slot_kinds[slot] == 2 and UINT16(object_slot_combatants[slot]) < 9999:
        score = combatants[object_slot_combatants[slot]].unk_19[1]
    return INT8(g_57E0_084C[score])  # no bound test; above 25 reads the bytes after the table

define order_effect_terms(slot):
    let block = effect_block(slot)
    let t: INT16 = 0
    if block[14] & 0x01 != 0: t = t - 4    # effect 112
    if block[1] & 0x04 != 0: t = t - 2     # effect 10
    if block[5] & 0x04 != 0: t = t + 2     # effect 42
    if block[6] & 0x08 != 0: t = t + 40    # effect 51
    if block[11] & 0x08 != 0: t = t - 2    # effect 91
    if block[12] & 0x80 != 0: t = t + 50   # effect 103
    return t

define set_order_words(slot):
    let c = object_slot_combatants[slot]
    let bit: UINT16 = 1 << combatants[c].unk_15[0]
    let spared = (g_4C4F_0006 == 0 and bit & 0x71 != 0)
        or (g_4C4F_0006 == 1 and bit & 0x184 != 0)
        or (g_4C4F_0006 == 2 and bit & 0x608 != 0)
    if spared:
        g_4C4F_00DC[c].first = dexterity_term(slot) + order_effect_terms(slot) + 30
        g_4C4F_00DC[c].second = 200
        return
    # one draw of RULE-RNG-001's generator, scaled to 0..9
    let r: INT16 = (INT32(draw()) * 10) / 32768
    g_4C4F_00DC[c].first = r + dexterity_term(slot) + order_effect_terms(slot) + 20
    # a second draw, scaled to 0..199
    g_4C4F_00DC[c].second = (INT32(draw()) * 200) / 32768
```

## Outputs

The two words of the combatant's four-byte entry at `4C4F:00DC + 4 * c`.

## Edge cases

The effect terms lie between -8 and 92, so with draws the first word is `dexterity_term` plus 12
to 121. A spared combatant's first
word is one above the greatest a draw gives for the same terms, and its second word 200 is above
every drawn one. The byte `g_4C4F_0006` is 0xFF from the fight start unless a script set it
(FND-RNG-042), and values other than 0, 1 and 2 spare no group. The group bit is a 16-bit shift by
the byte modulo 32, as the processor does it, so a byte of 16 to 31 gives no bit. A combatant
number from the slot map is used as given; `dexterity_term` returns the table's byte 9, 0, for a slot it cannot read.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Which groups the masks 0x71, 0x184 and 0x608 name, what script request 25's values 7 and 11
  do, what `DS:054E` stands for, and whether a combatant that has acted gets -1 as its first word
  in the routine that picks the next one (FND-RNG-042, Q-COMBAT-028).
- What the dexterity table `g_57E0_084C` is indexed by beyond the record byte `+0x1A`, what the
  order words in `g_4C4F_00DC` lead to after the pick, and what sets `g_4C4F_0006` before a
  fight other than script request 25 (FND-RNG-042, Q-COMBAT-028).
- What the bits of `effect_block` stand for beyond the effect numbers FND-COMBAT-029's panel
  reads (FND-COMBAT-036, Q-COMBAT-028).
