---
id: RULE-PARTY-016
title: The spells a Preserver is given when the generation screen stores it
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-029, FND-RNG-033, FND-PARTY-073, FND-PARTY-070, FND-PARTY-104]
conflicting: []
split_with: []
related: [RULE-PARTY-002, RULE-RNG-001, SCR-UI-004]
---

## Summary

When DONE stores a character with a preserver class whose level differs from the preserver level
the generation screen saved on opening (0 for a new character), the game clears the spells it
knows and gives it a fixed set by level: spell 11 at level 1; at level 2 or more spells 8, 11, 13,
27 and 29 and one spell picked at random from each of four groups of four; and spell 43 as well
at level 7.

## When it runs

Once each time DONE finishes the generation screen, after the screen's other storing steps
(FND-PARTY-070, FND-RNG-029). An edit that leaves the preserver level as it was changes nothing.
For a character with a first class, DONE's storing steps before this one make no draw, so its
picks are the first draws DONE makes (FND-RNG-033).

## Parameters

`known`, the character's known spells, a set of spell numbers held as the `SPST` bits
(FND-PARTY-104). `classes` and `levels`, the character's stored class codes (the 1-to-17
numbering of `character_class`) and the level of each, in the order the record keeps them.
`saved_level`, the preserver level the screen saved when it opened: 0 for a new character, and
for an edited one the level of its preserver class then. An edited character with no preserver
class on opening leaves it as the last opening that set it: 0 after a new character or at program
start, or the preserver level of the character edited then (FND-RNG-029).

## Inputs

None beyond the parameters.

## Procedure

```text
define give_first_spells(known, classes, levels, saved_level):
    # the first position holding the preserver code, or -1 when there is none
    let p = first_preserver_position(classes)
    if p == -1 or levels[p] == saved_level:
        return known
    let level = levels[p]
    known = known minus every spell 0 to 114
    if level == 7:
        known = known + {43}
    if level <= 1:
        return known + {11}
    known = known + {27, 29}
    known = known + {[12, 16, 17, 19][pick_one_of_four()]}
    known = known + {[14, 18, 22, 24][pick_one_of_four()]}
    known = known + {13}
    known = known + {[0, 9, 7, 4][pick_one_of_four()]}
    known = known + {[1, 2, 3, 10][pick_one_of_four()]}
    return known + {8, 11}

define first_preserver_position(classes):
    for i in 0..count(classes):
        if classes[i] == 11:
            return i
    return -1

define pick_one_of_four():
    # one draw of RULE-RNG-001's generator, scaled to 0..3
    return (INT32(draw()) * 4) / 32768
```

## Outputs

The new `known` set, stored back as the character's `SPST` bits. Nothing else changes.

## Edge cases

Only the first class position holding the preserver code is looked at. Levels 0 and 1 make no
draw; level 2 or more makes exactly four draws, in the order above, each choosing each of its
four spells for 8192 of the 32768 draw values. A preserver of level 7 is given spell 43 and then
everything level 2 or more gives. Spells 115 and above are not cleared.

Editing a character whose preserver level changed clears the spells it had learned, including
those chosen in the learning window (SCR-PARTY-001), and gives it this set instead.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Which spells the numbers name and what spell levels the game gives them; FND-RNG-029 reads only
  the bit numbers (No item: naming only; the numbers are what the rule uses).
