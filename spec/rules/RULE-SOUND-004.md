---
id: RULE-SOUND-004
title: A combatant's cry is its own sound, a fixed sound or a random one of its gender's voice sounds
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-043, FND-SOUND-007]
conflicting: []
split_with: []
related: [RULE-SOUND-001, RULE-RNG-001]
---

## Summary

When a combatant cries out, the game picks the sound number: a fixed number for a combatant whose
state is not 2, the combatant's own sound for a non-party combatant that has one, and otherwise a
number by gender, chosen at random from the voice sounds of that gender. There are two kinds of
cry, the one overlay 206 `+0E1E` gives (kind 1) and the one `+0B81` gives (kind 0).

## When it runs

`cry_sound` runs at overlay 206 `+0C2C` with kind 0 and at `+0E77`, in `+0E1E`, with kind 1; the
number goes to the sound-effect player (FND-RNG-043, FND-SOUND-007), which plays it as
RULE-SOUND-001 gives. In `+0E1E` it runs when the slot's state byte is 2 and the absolute value of
the word at `DS:67D6 + 37 * slot` is not 430.

## Parameters

`slot`, the combat slot. `kind`, 1 or 0 as above. `state`, the byte at `4F49:0C33 + 3 * slot`.
`own_sound`, byte `+0x3C` of the slot's combatant details record. `gender`, byte `+0x13` of that
record, and `origin`, byte `+0x12`.

## Inputs

None beyond the parameters.

## Procedure

```text
define cry_sound(slot, kind, state, own_sound, gender, origin):
    if state != 2:
        if kind == 1:
            return 48
        return 98
    if own_sound != 0 and slot >= 4:
        return own_sound
    if gender == 1:
        if kind == 1:
            return [18, 133, 177, 178, 179, 180, 182, 183, 190, 195, 211, 172][roll_sum(1, 12) - 1]
        return 153 + roll_sum(1, 5)
    if gender == 2:
        if origin == 8:
            return 228
        if kind == 1:
            return [19, 137, 138, 139, 140, 145, 147, 148][roll_sum(1, 8) - 1]
        return 140 + roll_sum(1, 4)
    if kind == 1:
        return 61
    return 98
```

## Outputs

The sound number, a byte, given to the sound-effect player.

## Edge cases

At most one draw is made, through `roll_sum`, and only on the gender branches other than origin 8.
A party member never uses `own_sound`. Kind 0 gives 154 to 158 for gender 1 and 141 to 144 for
gender 2.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Which game events reach overlay 206 `+0B81` and `+0E1E`, and so when each kind of cry is given
  (Q-RNG-014).
- The voice tables have no direct writer in the immediate search; a computed or bulk write was not
  ruled out (Q-RNG-014).
