---
id: RULE-MAGIC-005
title: A DATA record's dice are a count growing with the level, of sides the record gives, plus a bonus per level step
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-035, FND-PARTY-112]
conflicting: []
split_with: []
related: [RULE-RNG-001]
---

## Summary

`data_dice` rolls the dice a `DATA` record gives for a level. Three bytes of the record hold six
small numbers: a base count `C`, a count per step `A`, a bonus per step `B`, the sides `S`, a
level offset `F` and a step length `E`. The level is capped at 10, the step count `k` is
`(level + F) / E`, at least 1, and the roll is `C + A * k` dice of `S` sides plus `B * k`. The
routine can also report the largest total the roll could give. A record whose `A`, `B`, `C` and
`S` are all 0 gives 0 with no roll.

## When it runs

Overlay 193 `+003C`, the routine that applies a `DATA` number to its targets, calls it once for
each target before the hit routine, with its level byte; a second routine of overlay 193 calls it
too (FND-RNG-035).

## Parameters

- `data_number: INT16`: the `DATA` number, -1 for none.
- `level: UINT8`: the level the dice are rolled for.

## Inputs

Bytes `0x1C`, `0x1D` and `0x1E` of the 73-byte `DATA` record `data_number` of `RESOURCE.GFF`.

## Procedure

```text
define data_dice(data_number: INT16, level: UINT8):
    if data_number == -1:
        return (0, 0)  # nothing is stored as the maximum
    let r = data_record(data_number)
    if level > 10:
        level = 10
    let per_step_count = (r[0x1C] >> 5) & 7
    let per_step_bonus = r[0x1C] & 0x1F
    let base_count = (r[0x1D] >> 3) & 0x1F
    let step = r[0x1D] & 7
    let sides = r[0x1E] & 0x0F
    let offset = INT8(r[0x1E]) >> 4  # signed, -8 to 7
    if per_step_count == 0 and per_step_bonus == 0 and base_count == 0 and sides == 0:
        return (0, 0)  # nothing is stored as the maximum
    if step == 0:
        step = 1
    let k: INT16 = INT16(level + offset) / step
    if k < 1:
        k = 1
    let count: INT16 = INT16(base_count + per_step_count * k)
    let bonus: INT16 = INT16(per_step_bonus * k)
    let maximum: INT16 = INT16(bonus + count * sides)
    # roll_sum draws once per die (RULE-RNG-001)
    return (UINT16(roll_sum(count, sides) + bonus), maximum)
```

## Outputs

The total, as a word, and `maximum`, which the routine stores only when its caller passes a place
for it. On the two early returns nothing is stored, and the 0 shown as the maximum there is not
written.

## Edge cases

The division truncates toward zero, so a negative `level + offset` gives `k` of 0 or less, raised
to 1. A record with sides 0 and a count above 0 still draws once per die and adds 1 for each
(RULE-RNG-001). The routine fixes `step` 0 to 1 in its own copy of the record only.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What the caller does with the total and with `maximum`, and which level byte each caller
  passes (FND-RNG-035, Q-MAGIC-006).
