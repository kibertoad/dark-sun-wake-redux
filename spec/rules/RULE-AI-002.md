---
id: RULE-AI-002
title: What a computer-controlled combatant does in its turn
status: unknown
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: []
conflicting: []
split_with: []
related: [RULE-AI-001, RULE-COMBAT-006]
---

## Summary

In combat the computer decides the turn of every hostile creature and of each party member whose
computer control is on (RULE-AI-001): which target it attacks, where it moves, what it does and
when its turn ends. How it decides is not known.

## When it runs

Not known. In a turn whose combatant is not a party member under the player's control.

## Parameters

None known.

## Inputs

`combatants`, with `computer_control`, and `turn_combatant`.

## Procedure

Not known.

## Outputs

Not known.

## Edge cases

None known.

## What the sources say

SRC-MANUAL-1994, page 5: when a character has both a melee and a missile weapon readied, the
computer decides which one an attack uses; non-player characters may flee, fight back or summon
reinforcements when attacked. Page 12: a character under computer control has its actions
decided by the computer during combat.

## Differences between builds

None known.

## Open questions

- Which routine chooses a computer-controlled combatant's target, movement, action and the end of
  its turn. The static paths of FND-AI-001 do not reach one. The readers of `computer_control` in
  FND-AI-003, among them `28C9:0605`, which skips the rest of its routine for a combatant under
  computer control, are the next leads, together with the combat entry that Q-COMBAT-001 looks
  for (Q-AI-001).
- Whether hostile creatures and party members under computer control use the same routine.
- When a non-player character flees, fights back or calls for help, and whether that belongs to
  combat or to the scripts.
