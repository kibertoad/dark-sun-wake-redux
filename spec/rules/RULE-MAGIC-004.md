---
id: RULE-MAGIC-004
title: Resting heals the party, restores psionic strength points and spells, and casts condition cures from the party's priests
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-MAGIC-005, FND-MAGIC-006, FND-MAGIC-018, FND-MAGIC-021, FND-MAGIC-024, FND-PARTY-211, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-MAGIC-001]
---

## Summary

The party rests when the player uses the map object numbered 1317, which the manual's fire ring
is taken to be, or when a script asks for a rest. A rest is refused during combat and otherwise
always completes. It sets every standing member's hit points, PSPs and spell pools to full; has the
party's best cleric and best druid of level 5 or more cast Cure Disease, Remove Curse and Remove
Paralysis, and a druid of 7 or more Neutralize Poison, on every member; ends a set of effects and
every psionic power in effect; and adds 60 for each hour the caller gives to the clock that timed
effects run against: 8 hours at the fire ring, 0 from a script. The game keeps no memorized
spells: a caster can cast any spell it knows of a level whose pool is above 0.

## When it runs

When the player uses a map object whose `object_number` is 1317 during exploration (`hours` 8,
`wake` false, every member), and on script requests 1 (`wake` true) and 4 (`wake` false), with
`hours` 0 in every shipped script (FND-MAGIC-005).

## Parameters

`members`, the party slots 0 to 3 in order. `wake`, true or false. `hours`, the hour count.
`filter`, a member, and `everyone`, true when the rest covers every member, which the fire ring
and the scripts that pass 32766 ask for. For each member: `combat_mark`, `hit_points`,
`maximum_hit_points`, `psionic_strength_points`, `maximum_psionic_strength_points`, `cleric_level`
and `druid_level` (the greatest level of its Cleric codes 1 to 4 and of its Druid codes 5 to 8, 0
when it has none, a held-back former class included), and `spells_left[kind][spell_level]`.
`positions` and `wisdom`, as RULE-MAGIC-001 takes them. `clock`, the double word `4C13:035B`. `in_combat`, whether the combat word is not 0.

## Inputs

None beyond the parameters.

## Procedure

```text
define rest(members, wake, hours, filter, everyone, in_combat):
    if in_combat:
        # visible: the message NO RESTING DURING COMBAT
        return
    # visible: the message THE PARTY RESTS, in the map view
    let cleric = -1
    let cleric_best = 0
    let druid = -1
    let druid_best = 0
    for each m in members:
        if everyone or filter == m:
            if wake or m.combat_mark == 2 or m.combat_mark == 3:
                m.combat_mark = 1
            if m.combat_mark == 1:
                if m.cleric_level > cleric_best:
                    cleric_best = m.cleric_level
                    cleric = m
                if m.druid_level > druid_best:
                    druid_best = m.druid_level
                    druid = m
    for each m in members:
        if cleric_best >= 5:
            cast_cure(148, cleric, m, cleric_best)
            cast_cure(157, cleric, m, cleric_best)
            cast_cure(159, cleric, m, cleric_best)
        if druid_best >= 5:
            cast_cure(148, druid, m, druid_best)
            cast_cure(157, druid, m, druid_best)
            cast_cure(159, druid, m, druid_best)
        if druid_best >= 7:
            cast_cure(178, druid, m, druid_best)
        if wake and m.combat_mark != 8:
            m.combat_mark = 1
            if m.hit_points < 1:
                m.hit_points = 1
        # the member's Sunstroke effects and 19 other effect codes end (FND-MAGIC-005)
        if m.combat_mark == 1:
            for kind in 1..3:
                for spell_level in 1..10:
                    m.spells_left[kind][spell_level] = spell_pool(m.positions, m.wisdom, kind, spell_level)
            m.hit_points = m.maximum_hit_points
            m.psionic_strength_points = m.maximum_psionic_strength_points
    # every psionic power in effect ends
    clock = clock + 60 * hours
    # the timed effects now due run
```

`cast_cure(spell, caster, target, level)` casts the priest spell numbered `spell` (RULE-MAGIC-002's
table: 148 Cure Disease, 157 Remove Curse, 159 Remove Paralysis, 178 Neutralize Poison) from
`caster` on `target` at caster level `level`, spending nothing from the pools. Cure Disease
removes every effect of code 27 from `target` and Neutralize Poison every effect of code 71;
Remove Curse sets `target`'s current Strength to its base Strength when it is lower; Remove
Paralysis removes every effect of codes 91, 70, 65, 66, 67 and 68, and of 69 when `target` has no
effect of code 71. The caster and the level change nothing. Each removal ends the effect and
rebuilds `target`'s scores (FND-MAGIC-021). Each cure first removes effects 0x35 and 0x36 from
`caster`, and afterwards plays sound effect 9 (Cure Disease, Remove Curse) or 59 (Remove Paralysis,
Neutralize Poison) for a target it reached, Remove Curse only when it raised Strength; no picture
or object is shown (FND-MAGIC-024).

## Outputs

The changes above; nothing is returned.

## Edge cases

- A rest at the fire ring stands up members marked 2 or 3 and heals them, and leaves members with
  any other mark but 1 unhealed and without refilled spells. A script rest with `wake` stands up
  and heals every member not marked 8, even outside `filter`.
- With no cleric or druid of level 5 or more no cure is cast; a party with both casts each of the
  three cures twice on every member. A cleric never casts Neutralize Poison here.
- A level gain does not refill the pools; they change at the next rest (FND-MAGIC-006).
- A member whose effect block byte 8 has bit 0x80 takes 1000 damage at the rest (FND-MAGIC-005).
- A member with none of the cured effects and with Strength not below its base is not changed by
  the cures, and a second cast of a cure changes nothing more. Remove Curse's Strength is lowered
  again at the next score rebuild while an effect that lowers it remains. Remove Paralysis runs
  before Neutralize Poison, so a poisoned member keeps effect 69 (FND-MAGIC-021).
- A cure does not reach a member that has effect 4, 92 or 74, effect 11 while its word at
  `4C4F:00DC` is -1, or effect 55 whose row byte is above the spell's level; that loses a 75 in 100
  roll with effect 46, which then loses effect 46 and gets `DATA` 307 queued on itself; or, against
  another member's cure, has effect 56, or effect 85 for Cure Disease, Remove Curse or Remove
  Paralysis, which turns the cure back at the caster at the rest's level. With effect 92 a worn
  item of power 312 with capacity left absorbs the cure. The cast level decides none of these
  (FND-MAGIC-024).

## What the sources say

SRC-MANUAL-1994, page 6, says camping lets the party rest, which is needed to recover from
battles and to regain spells and psionic points; that safe places to rest are marked by a fire
ring, on which the player clicks the Look pointer; that as the party rests, characters with cure
spells cast them on wounded characters; and that PSPs are fully restored, as are all the spells
the spellcasters can cast. Page 13 says the number of spells a character can cast before resting
depends on level (RULE-MAGIC-001).
The game heals without a caster and casts only condition cures, chosen by class level and not by
the spells a caster has left (FND-MAGIC-005).

## Differences between builds

None known.

## Open questions

- Whether object number 1317 is the fire ring, which pointer mode starts a rest from a click, and
  how long the fire ring's 8 hours are in the clock's units (FND-MAGIC-005, Q-MAGIC-022).
- What `combat_mark` 2, 3 and 8, effect 0x47, the 19 ended effect codes and the effect block bits
  stand for (FND-MAGIC-005, Q-MAGIC-023).
- Whether the cast queue runs during a rest, so that a cure turned back by effect 85 lands on its
  caster, and what `4C4F:00DC` holds outside a fight (FND-MAGIC-024, Q-MAGIC-028).
