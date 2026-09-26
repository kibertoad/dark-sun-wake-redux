---
id: RULE-INPUT-002
title: Which image the pointer shows for each mode, and its hotspot
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-INPUT-001, FND-INPUT-002, FND-INPUT-003, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-INPUT-001]
---

## Summary

On the map the pointer shows one of eight images: one for each of Walk, hand-to-hand attack,
ranged attack and Look, and an invalid version of each shown when the action is not possible at
the pointer. The image is drawn with its top-left pixel at the pointer's position, and that pixel
is where the action aims. While the game is busy the pointer is an hourglass, and a spell aimed
where it cannot be cast shows a ninth image.

## When it runs

`pointer_image` runs when the game redraws the pointer over the map in one of the three modes
[FND-INPUT-003].

## Parameters

`mode`, the pointer mode as RULE-INPUT-001 numbers it. `ranged`, whether an attack at the pointer
would be a ranged attack. `can_act`, whether the action of the mode is possible at the pointer.

## Inputs

None beyond the parameters.

## Procedure

```text
define pointer_image(mode, ranged, can_act):
    let number = 19101
    if mode == 1:
        number = 19103
        if ranged:
            number = 19105
    else if mode == 2:
        number = 19107
    if not can_act:
        number = number + 1
    return number
```

## Outputs

The number of the `RESOURCE.GFF#ICON` resource the pointer shows. Its one frame is drawn with its
top-left pixel at the pointer's position [FND-INPUT-001, FND-INPUT-002].

## Edge cases

While the game is processing commands the pointer is `ICON/19110`, and a spell aimed where it
cannot be cast shows `ICON/19109` [SRC-MANUAL-1994]; neither number is in the routine that picks
the other eight [FND-INPUT-003].

## What the sources say

The manual pictures the Walk, Attack and Look pointers and describes the invalid versions, the
hand-to-hand attack that works only next to the enemy, the ranged attack that needs a readied
missile weapon, the hourglass, and aiming with the upper-left corner (SRC-MANUAL-1994, pages 4
and 5). The captures agree for Walk, ranged attack and Look.

## Differences between builds

None known.

## Open questions

- How the game decides `ranged` and `can_act`. In the captures the Attack pointer over the first
  hostile character was the ranged image (FND-INPUT-002, Q-INPUT-001).
- Which code shows `ICON/19109` and `ICON/19110` (FND-INPUT-003, Q-INPUT-002).
