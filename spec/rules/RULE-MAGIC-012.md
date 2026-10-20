---
id: RULE-MAGIC-012
title: Eighteen DATA records summon a fixed creature or one picked by a d4 or d12, placed near the target point as combatant 4
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-040, FND-PARTY-107, FND-COMBAT-023, FND-COMBAT-052, FND-COMBAT-022, FND-PARTY-133, FND-RNG-041, FND-RNG-035]
conflicting: []
split_with: []
related: [RULE-RNG-001]
---

## Summary

`summon_creature` runs for a `DATA` record whose byte `+0x17` is -1 and whose word `+0x11` has bit
0x0001; in the shipped `RESOURCE.GFF` those are records 59, 65, 76, 81, 87, 88, 92, 96, 106, 132,
189 to 192 and 217 to 220. While the byte `g_4F49_0C3F` is not 0 the summoning only shows that it
goes unanswered. Otherwise the record's number picks a creature number, by a d4 for 59 and 81 and a
d12 for 76 and 87; overlay 188 `+0AA8` looks for a place near the point given, in 16-pixel cells;
and a creature placed there becomes combatant 4. The call ends with `2D40:06E1` of the byte it was
given.

## When it runs

Overlay 193 `+003C`, which runs a queued record (FND-PARTY-107), calls it at `+03D5` with the
`DATA` number, its words at `bp+0xE` and `bp+0x10` and the byte `+0x15` of the caster's combatant
record, or 11 for a caster below 0 (FND-RNG-040).

## Parameters

- `data_number: INT16`: the `DATA` number.
- `x: INT16`: the point's first coordinate.
- `y: INT16`: the point's second coordinate.
- `side: UINT8`: the byte passed on to `fn_2D40_06E1`.

## Inputs

`g_4F49_0C3F` and `g_4C13_0369`.

## Procedure

```text
define summon_creature(data_number: INT16, x: INT16, y: INT16, side: UINT8):
    if g_4F49_0C3F != 0:
        emit MessageShown("Your summoning goes unanswered")
        return
    let p: INT16 = 414
    # each roll_sum draws once per die (RULE-RNG-001)
    if data_number == 59:
        p = [422, 424, 425, 423][roll_sum(1, 4) - 1]
    else if data_number == 81:
        p = [426, 428, 429, 427][roll_sum(1, 4) - 1]
    else if data_number == 76:
        p = [400, 550, 551, 406, 407, 552, 418, 419, 403, 404, 405, 523][roll_sum(1, 12) - 1]
    else if data_number == 87:
        p = [421, 421, 421, 522, 522, 522, 415, 452, 453, 454, 441, 441][roll_sum(1, 12) - 1]
    else if data_number == 65:
        p = 567
    else if data_number == 88:
        p = 559
    else if data_number == 132:
        p = 569
    else if data_number >= 189 and data_number <= 192:
        p = 422 + (data_number - 189)
    else if data_number >= 217 and data_number <= 220:
        p = 426 + (data_number - 217)
    let (found, px, py) = fn_5702_00CF(x / 16, y / 16, 0, 0, 0)
    if found:
        if fn_31E0_0121(p, 4, px, py, 0, 4, 6) >= 0:
            combatants[4].unk_15[0] = 4
            combatant_details[4].unk_10[38] = 4
            combatant_details[4].unk_10[2] = 14
            fn_2D40_085B(4, 11, g_4C13_0369, 0)
            if data_number == 92 or data_number == 88:
                fn_197_32A1(data_number)
    fn_2D40_06E1(side)
```

## Outputs

The placed creature and the stores to combatant record 4 and details record 4.

## Edge cases

Every other record of the eighteen (92, 96 and 106) gets creature 414. A d4 total of 1 to 4 and a
d12 total of 1 to 12 always find a table entry; the jump tables send any other value to 540, which
no total reaches. The division by 16 is signed and rounds toward 0. The creature is always made
combatant 4, so a later summoning overwrites the same records. A summoning that finds no place, or
whose placement returns below 0, makes no store.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Which creatures the numbers 400 to 569 are, which spells the eighteen `DATA` records are, what
  `g_4F49_0C3F` stands for, and what `fn_5702_00CF`, `fn_31E0_0121`, `fn_2D40_085B`,
  `fn_2D40_06E1` and `fn_197_32A1` do with their arguments (FND-RNG-040, Q-MAGIC-011).
- What `g_4C13_0369` holds (FND-RNG-040, Q-MAGIC-011).
