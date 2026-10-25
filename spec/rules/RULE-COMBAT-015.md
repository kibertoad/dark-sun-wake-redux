---
id: RULE-COMBAT-015
title: A combatant picks a target at random from the slots of the groups its mask names within a limit, nearest first, retrying when an Entranced or Sanctuary test fails on the drawn number
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-041, FND-PARTY-099, FND-PARTY-098, FND-RNG-040, FND-RNG-089, FND-ACTOR-005, FND-COMBAT-052, FND-PARTY-013, FND-COMBAT-027, FND-EXPLORE-001, FND-EXPLORE-002, FND-COMBAT-022, FND-COMBAT-023, FND-ACTOR-003, FND-EXPLORE-006]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-PARTY-014, RULE-COMBAT-014]
---

## Summary

`pick_target` is overlay 173 `+1ACD`. When its output word is -1 it lists the slots whose
combatant's group is in a mask built from the picker's group, that lie in the view's scrolling
bounds and within a distance limit of the picker, sorted nearest first. It draws a place `p` in
the list, and takes the slot at that place unless one of two tests made with `p` itself as a
slot fails: the picker is under effect 33 (`Entranced`) from slot `p`, or slot `p` is under
effect 87 (`Sanctuary`) and the picker fails the saving throw against it. A failed test draws
again, at most `n + 1` draws in all.

## When it runs

Overlay 173 `+1354`, `+1684`, `+1822` and `+1A55` call it, with the flag 0 and a limit of 24,
the flag 1 and a local limit, the flag 1 and a limit of 1, and the flag 1 and byte `+6` of an
item type's record (FND-RNG-041).

## Parameters

- `picker: INT16`: the picker's object slot.
- `combatant: INT16`: the picker's combatant record number.
- `output: INT16`: the word the pick goes to, given by reference.
- `limit: INT16`: the greatest distance listed.
- `line_test: UINT8`: 0 lists by `fn_1B0B_000C` around the picker's cell; any other value lists
  by `fn_2D40_35EF` with the `fn_1B0B_00A4` test.

## Inputs

`object_slot_kinds`, `object_slot_combatants`, `object_slots` (`position_x`, `position_y`);
`combatants`: `unk_15[0]` and `combat_mark`; `combatant_details`: byte `+0x1E` (`unk_10[14]` of FMT-COMBAT-002) of the picker's
record; `view_left_min`, `view_top_min`, `view_right_limit`, `view_bottom_limit`; the effect
entries through `fn_3143_00A8` and `g_4E71_024F`.

## Procedure

```text
define pick_group_mask(b: UINT8):
    let bit: UINT16 = 1 << b
    let mask: UINT16 = 0
    if bit & 0xF80 != 0: mask = mask | 0x71
    if bit & 0xC58 != 0: mask = mask | 0x184
    if bit & 0x964 != 0: mask = mask | 0x608
    if b == 0: mask = mask | 0xF80
    if b != 11: mask = mask | 0x800
    return mask

define pick_listed(s, mask: UINT16):
    if object_slot_kinds[s] != 2:
        return false
    let c = combatants[object_slot_combatants[s]]
    if (1 << c.unk_15[0]) & mask == 0 or c.combat_mark > 2:
        return false
    let x: UINT16 = object_slots[s].position_x >> 4
    let y: UINT16 = object_slots[s].position_y >> 4
    return UINT16(view_left_min / 16) <= x and x <= UINT16(view_right_limit / 16)
        and UINT16(view_top_min / 16) <= y and y <= UINT16(view_bottom_limit / 16)

define pick_insert(list, n, s, d: INT16):
    # the first place whose distance is d or more; equal distances keep the newer slot first
    let i = 0
    while i < n and list[i].distance < d:
        i = i + 1
    list.insert(i, (distance: d, slot: s))

define pick_list(picker, mask, limit, line_test):
    let list = []
    let n = 0
    let px = object_slots[picker].position_x >> 4
    let py = object_slots[picker].position_y >> 4
    for s in 0..47:
        if not pick_listed(s, mask):
            continue
        let sx = object_slots[s].position_x >> 4
        let sy = object_slots[s].position_y >> 4
        let d: INT16 = fn_2D40_35EF(picker, s) if line_test != 0 else fn_1B0B_000C(px, py, sx, sy)
        if d > limit:
            continue
        if line_test != 0 and fn_1B0B_00A4(px, py, 0, sx, sy, 0) != 0:
            continue
        pick_insert(list, n, s, d)
        n = n + 1
    return (list, n)

define pick_target(picker, combatant, output, limit, line_test):
    if output != -1:
        return
    let (list, n) = pick_list(picker, pick_group_mask(combatants[combatant].unk_15[0]), limit, line_test)
    let k = 0
    while n > 0:
        # one draw of RULE-RNG-001's generator, scaled to 0..n-1
        let p: INT16 = (INT32(draw()) * n) / 32768
        let ok = true
        let e = fn_3143_00A8(picker, 33)
        if e != -1 and g_4E71_024F[e] == p:
            ok = false
        if fn_3143_00A8(p, 87) != -1:
            # the saving throw makes its own draw (FND-PARTY-099)
            if UINT8(fn_56A7_0075(picker, p, 125, combatant_details[picker].unk_10[14])) == 0:
                ok = false
        if ok:
            output = list[p].slot
            g_4C4F_0088[combatant] = 1
            return
        if n <= 1 or k >= n:
            break
        k = k + 1
    output = -1
    g_4C4F_0088[combatant] = 0xFF
```

## Outputs

`output`, the picked slot or -1, and the combatant's byte `g_4C4F_0088`, 1 or 0xFF.

## Edge cases

A list of one slot still draws, and `p` is then 0. The tests read `p`, the place in the list, as
a slot number, so they test slot `p` and not the slot at place `p`; the Sanctuary test then runs
the saving throw against slot `p` too. The list builders do not leave out the picker: with
`line_test` 1 the picker is listed at the distance `fn_2D40_35EF` gives for itself when its own
group is in the mask. The saving throw is reached only for a `p` whose slot has a Sanctuary
entry, after the Entranced test, whatever that test gave. The listing and the `fn_56A7_0075`
argument read the details record through the slot number `picker` directly.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_1B0B_00A4`, `fn_2D40_35EF` and `fn_3143_00A8` compute, which groups the masks name,
  what the effect entry's word `g_4E71_024F` holds for effect 33, what the four callers use the
  pick for and what the local limit of `+1684` holds (FND-RNG-041, Q-COMBAT-029).
- The units of `fn_1B0B_000C`, what the saving throw `fn_56A7_0075` tests with 125 and the level
  byte, and who reads `g_4C4F_0088` (FND-RNG-041, Q-COMBAT-029).
