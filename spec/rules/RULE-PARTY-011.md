---
id: RULE-PARTY-011
title: Which alignments the generation screen allows
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-PARTY-065, FND-PARTY-069, FND-PARTY-070, FND-PARTY-071, FND-PARTY-072, FND-PARTY-078, FND-PARTY-079]
conflicting: []
split_with: []
related: [RULE-PARTY-002, SCR-UI-004]
---

## Summary

On the character generation screen a character can hold only an alignment every one of its
classes allows. No class allows an evil alignment, a druid must be true neutral, a ranger must be
good, and a thief cannot be lawful good. The alignment button steps forward or back past the
alignments the classes forbid, unless Ctrl is held. A character with no class is true neutral.

## When it runs

`step_alignment` when the player presses the alignment button, and `check_alignment` with `step`
1 whenever the screen rolls the scores (RULE-PARTY-010), which includes choosing a class
(FND-PARTY-071, FND-PARTY-072).

## Parameters

`classes`, the list of the character's `character_class` codes (RULE-PARTY-002), in the order the
screen keeps them. `alignment`, the character's `alignment` code, 0 to 8. `step`, 1 when the player presses the button with the left mouse button and -1 with the right
or middle one (FND-PARTY-078); no key presses these buttons (FND-PARTY-079).
`ctrl_held`, true when the keyboard's Ctrl flag is set as the step is made.

## Inputs

None beyond the parameters.

## Procedure

```text
define alignment_allowed(character_class, alignment):
    # One mask per class code; bit a allows alignment code a
    let allowed: UINT16[8] = [0x0DB, 0x010, 0x0DB, 0x0DB, 0x0DB, 0x0DB, 0x049, 0x0DA]
    return (allowed[character_class] >> alignment) & 1 == 1

define check_alignment(classes, alignment, step, ctrl_held):
    if count(classes) == 0:
        return 4
    let current = alignment
    for each c in classes:
        if not alignment_allowed(c, current):
            current = step_alignment(classes, current, step, ctrl_held)
    return current

define step_alignment(classes, alignment, step, ctrl_held):
    # The screen counts alignments from 1; past 9 it goes to 1, below 1 to 8
    let counted = alignment + 1 + step
    if counted > 9:
        counted = 1
    if counted < 1:
        counted = 8
    if ctrl_held:
        return counted - 1
    return check_alignment(classes, counted - 1, step, ctrl_held)
```

## Outputs

Both functions return the new `alignment` code, which the screen stores in the character's
details record. Neither changes other state.

## Edge cases

Each class is checked in turn against the alignment as it stands after the steps the earlier
classes caused; once a step returns, the alignment satisfies every class unless Ctrl is held. No
set of classes the screen offers (RULE-PARTY-009) forbids every alignment (FND-PARTY-072), so the
recursion ends. Stepping back from lawful good gives chaotic neutral, skipping chaotic evil.

Holding Ctrl lets the button leave the alignment at one the classes forbid, an evil one among
them; the next roll moves it forward to an allowed one. DONE does not check the alignment
(FND-PARTY-070), so a character finished right after such a step keeps it.

## What the sources say

SRC-MANUAL-1994, page 22, says a player character must be good or neutral; its class descriptions
give druids true neutral, rangers good alignments and thieves any but lawful good.

## Differences between builds

None known.

## Open questions

- That holding Ctrl skips the check, which rests on the BIOS meaning of the flag bit: a capture
  of the screen after pressing the alignment button with Ctrl held, and of the character after
  DONE, would confirm it (Q-PARTY-032).
- That the left mouse button gives `step` 1 and the right or middle one -1, which rests on the
  mouse driver's meaning of the event bits: a capture after a left and a right press would confirm
  it (Q-PARTY-035).
