---
id: RULE-PARTY-015
title: How experience is given to party members
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-PARTY-081, FND-PARTY-086, FND-SCRIPT-005]
conflicting: []
split_with: []
related: [RULE-PARTY-013, FMT-PARTY-001]
---

## Summary

A party member who is still in the fight gets an award divided among the classes it counts,
added to its experience up to 2,000,000,000. A script gives an award to one member or to the
whole party. A party member killing an enemy brings each member the enemy's `kill_experience`
divided among the filled party slots. After each award the party gains levels unless the gain is
deferred.

## When it runs

`script_experience` for each script instruction 0x21, with `script_parameters` 0 and 1. `share_kill`
when damage leaves a combatant out of the fight, with its `combat_mark` above 2 (FND-PARTY-086).
`give_experience` from both.

## Parameters

`party`, the four party slots in order, each holding a member or nothing; for a member, its
`combat_mark` and `kind`, the combatant record's bytes at `0x14` and `0x15`; `counted`, the number
of its classes counted (FND-PARTY-081); `experience` and `kill_experience`, the FMT-PARTY-001
fields. `amount`, a signed 16-bit award. `gain_deferred`, true while the word at `4C10:0019` in
BLD-GOG-EN-1.1 is not 0. For `script_experience`, `character` and `value`, `script_parameters` 0
and 1. For `share_kill`, `dead_kind` and `dead_kill_experience`, the killed combatant's `kind` and
the `kill_experience` of its details record, and `filled`, the number of party slots whose state
byte is not 0.

## Inputs

None beyond the parameters.

## Procedure

```text
define give_experience(party, slot, amount, gain_deferred):
    let member = party[slot]
    if slot < 4 and member != none and member.combat_mark <= 2:
        if member.kind == 0 or member.kind == 4 or member.kind == 5 or member.kind == 6:
            let total = member.experience + amount / member.counted
            if total > 2000000000:
                total = 2000000000
            if member.kill_experience < total:
                member.kill_experience = total
            member.experience = total
            # visible: the experience
    if not gain_deferred:
        for m in party:
            if m != none:
                gain_levels(m, false)

define to_int16(value):
    let low = value % 65536
    if low >= 32768:
        return low - 65536
    return low

define script_experience(party, character, value, gain_deferred):
    let amount = to_int16(value)
    if character == 32766:
        for slot in 0..4:
            if party[slot] != none:
                give_experience(party, slot, amount, gain_deferred)
    else if character != 32767:
        give_experience(party, character % 65536, amount, gain_deferred)

define share_kill(party, dead_kind, dead_kill_experience, filled, gain_deferred):
    if dead_kind >= 7 and dead_kind <= 11:
        let amount = to_int16(dead_kill_experience / filled)
        for slot in 0..4:
            give_experience(party, slot, amount, gain_deferred)
```

## Outputs

The `experience` and `kill_experience` of the members given an award, and what `gain_levels` of
RULE-PARTY-013 changes. No draws of its own.

## Edge cases

The division by `counted` rounds toward zero, and the division by `filled` is unsigned and rounds
down. A share of 32,768 or more becomes negative through `to_int16` and lowers each member's
experience (FND-PARTY-086). A member out of the fight, or of another kind, gets nothing and its
share is lost; the level gain still runs after its award. `kill_experience` only rises.
`share_kill` runs the level gain once for each of the four slots when the gain is not deferred.

## What the sources say

No source read for this entry describes how experience is given.

## Differences between builds

None known.

## Open questions

- Whether any enemy's `kill_experience` divided by the filled slots reaches 32,768, so that a
  kill lowers the party's experience: the details records of enemies were not read (Q-PARTY-045).
- What sets the word at `4C10:0019` that defers the gain, and so whether the gain after a fight
  is the one overlay 173 runs when it ends (Q-PARTY-047).
- That `kind` 7 to 11 are the enemies and 0, 4, 5 and 6 the party and its allies rests on the
  two masks (Q-PARTY-046).
