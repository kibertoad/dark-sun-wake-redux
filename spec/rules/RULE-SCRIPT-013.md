---
id: RULE-SCRIPT-013
title: The game runs the first script trigger whose cell, box, key or key pair matches an event, and a region change drops the triggers the region set
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-SCRIPT-007, FND-SCRIPT-013, FND-SCRIPT-014, FND-SCRIPT-015, FND-SCRIPT-027, FND-SCRIPT-028, FND-SCRIPT-029, FND-EXPLORE-014, FND-CONFIG-156, FND-SCRIPT-005]
conflicting: []
split_with: []
related: [RULE-SCRIPT-002, RULE-SCRIPT-008, FMT-SCRIPT-004]
---

## Summary

The trigger lists that scripts fill (RULE-SCRIPT-008) are tested when something happens in the
game. An object entering a cell runs the first tile trigger on that cell that its kind may set
off, or else the first box trigger whose box holds the cell. Picking an item up, talking to,
attacking, looking at or using an object runs the first trigger keyed by that object's number on
the matching list, and using one item on another runs the first trigger keyed by the two numbers
in either order. Before the trigger's script runs, the game stores who set it off, the objects it
concerns and the actor's cell where the script reads them. Nothing runs until the script system is
set up. A region change drops all tile and box triggers and every other trigger not registered by
`MAS ` resource 99, then runs the new region's `MAS ` script.

## When it runs

`run_tile_triggers` runs on each plan the walk planner makes from a walker's cell (RULE-EXPLORE-005),
when an object is put on the map, for each party member after a region's script, and when script
instruction `0x5E` moves an object. `run_key_triggers` runs with `pick_up_trigger_head` when an item
is picked up into the hand, `talk_trigger_head` when the player talks to an object,
`attack_trigger_head` after each attack, `look_trigger_head` when the player clicks an object, and
`use_trigger_head` when the player uses a map object or an item, at the end of a fight with the key
-2975 and when a creature dies with the key -2976. `run_pair_triggers` runs when an item held in the
hand is dropped on another item or on a map object. `trigger_mask` runs when the pointer image and
the action window's buttons are chosen. `region_triggers_change` runs at a region change
(FND-SCRIPT-029).

## Parameters

`slot`, `x`, `y` and `kind` for `run_tile_triggers`: the object, the cell it enters, and 0 for the
leader, 1 for another party member, 2 for anything else. `head`, `a` and `b` for `run_key_triggers`:
the list, the actor and the object. `a`, `b` and `c` for `run_pair_triggers`: the actor and the two
items. `object` for `trigger_mask`. `region` for `region_triggers_change`.

## Inputs

`script_state`, `trigger_records`, `trigger_free_head`, the list heads `move_tile_trigger_head`,
`box_trigger_head`, `pick_up_trigger_head`, `talk_trigger_head`, `attack_trigger_head`,
`look_trigger_head`, `use_trigger_head`, `use_with_trigger_head`, `g_4C10_0007` and
`g_4C10_000B`.

## Procedure

```text
define object_number(x: INT16) -> INT16:
    if x < 0:
        return x
    return INT16(fn_1AA0_0009(x, 0))

define run_record(n: INT16) -> UINT8:
    run_script(trigger_records[n].entry_script, trigger_records[n].entry_offset, 1)
    return 1

define run_cell_list(head: INT16) -> UINT8:
    let n = head
    while n != -1:
        let r = trigger_records[n]
        if trigger_cell_x == r.key_1 and trigger_cell_y == r.key_2 and INT16(r.unk_08) >= trigger_kind:
            return run_record(n)
        n = r.next
    return 0

define run_box_list(head: INT16) -> UINT8:
    let n = head
    while n != -1:
        let r = trigger_records[n]
        if trigger_cell_x >= r.key_1 and trigger_cell_x <= r.key_1 + r.unk_08 - 1:
            if trigger_cell_y >= r.key_2 and trigger_cell_y <= r.key_2 + r.unk_09 - 1:
                if INT16(r.unk_0A) >= trigger_kind:
                    return run_record(n)
        n = r.next
    return 0

define set_cell_trigger_state(slot: INT16, x: INT16, y: INT16, kind: INT16):
    trigger_cell_x = x
    trigger_cell_y = y
    trigger_kind = kind
    trigger_actor = slot
    trigger_object = slot
    trigger_object_2 = slot
    fn_2D40_06E6(slot)

define run_tile_triggers(slot: INT16, x: INT16, y: INT16, kind: INT16) -> UINT8:
    if script_state != 2:
        return 0
    set_cell_trigger_state(slot, x, y, kind)
    if run_cell_list(move_tile_trigger_head) != 0:
        return 1
    if script_state != 2:
        return 0
    set_cell_trigger_state(slot, x, y, kind)
    return run_box_list(box_trigger_head)

define run_key_triggers(head: INT16, a: INT16, b: INT16) -> UINT8:
    if script_state != 2:
        return 0
    trigger_key = object_number(b)
    trigger_actor = a
    trigger_object = b
    trigger_object_2 = b
    fn_2D40_06E6(a)
    let n = head
    while n != -1:
        if trigger_records[n].key_1 == trigger_key:
            return run_record(n)
        n = trigger_records[n].next
    return 0

define run_pair_triggers(a: INT16, b: INT16, c: INT16) -> UINT8:
    if script_state != 2:
        return 0
    trigger_key = object_number(b)
    trigger_key_2 = object_number(c)
    trigger_actor = a
    trigger_object = b
    trigger_object_2 = c
    fn_2D40_06E6(a)
    let n = use_with_trigger_head
    while n != -1:
        let r = trigger_records[n]
        if r.key_1 == trigger_key and r.key_2 == trigger_key_2:
            return run_record(n)
        if r.key_1 == trigger_key_2 and r.key_2 == trigger_key:
            return run_record(n)
        n = r.next
    return 0

define trigger_keyed(head: INT16, both: UINT8) -> UINT8:
    let n = head
    while n != -1:
        if trigger_records[n].key_1 == trigger_key:
            return 1
        if both != 0 and n == use_with_trigger_head and trigger_records[n].key_2 == trigger_key:
            return 1
        n = trigger_records[n].next
    return 0

define trigger_mask(object: INT16) -> UINT16:
    trigger_key = object_number(object)
    let mask: UINT16 = 0
    if trigger_keyed(pick_up_trigger_head, 0) != 0:
        mask = mask | 0x01
    if trigger_keyed(attack_trigger_head, 0) != 0:
        mask = mask | 0x02
    if trigger_keyed(talk_trigger_head, 0) != 0:
        mask = mask | 0x04
    if trigger_keyed(look_trigger_head, 0) != 0:
        mask = mask | 0x08
    if trigger_keyed(use_trigger_head, 0) != 0:
        mask = mask | 0x10
    if trigger_keyed(use_with_trigger_head, 1) != 0:
        mask = mask | 0x20
    return mask

define free_all_triggers(head: INT16) -> INT16:
    let n = head
    while n != -1:
        let next = trigger_records[n].next
        trigger_records[n].next = trigger_free_head
        trigger_free_head = n
        n = next
    return -1

# by_unk_08: test byte 8 instead of the low byte of key_2
define free_unkept_triggers(head: INT16, by_unk_08: UINT8) -> INT16:
    let first = -1
    let last = -1
    let n = head
    while n != -1:
        let next = trigger_records[n].next
        let kept = trigger_records[n].key_2 & 0xFF
        if by_unk_08 != 0:
            kept = trigger_records[n].unk_08
        if kept == 0:
            trigger_records[n].next = trigger_free_head
            trigger_free_head = n
        else:
            if last == -1:
                first = n
            else:
                trigger_records[last].next = n
            last = n
        n = next
    if last != -1:
        trigger_records[last].next = -1
    return first

define region_triggers_change(region: INT16):
    script_region = region
    if script_state != 2:
        return
    move_tile_trigger_head = free_all_triggers(move_tile_trigger_head)
    g_4C10_0007 = free_all_triggers(g_4C10_0007)
    box_trigger_head = free_all_triggers(box_trigger_head)
    g_4C10_000B = free_all_triggers(g_4C10_000B)
    pick_up_trigger_head = free_unkept_triggers(pick_up_trigger_head, 0)
    attack_trigger_head = free_unkept_triggers(attack_trigger_head, 0)
    look_trigger_head = free_unkept_triggers(look_trigger_head, 0)
    use_trigger_head = free_unkept_triggers(use_trigger_head, 0)
    talk_trigger_head = free_unkept_triggers(talk_trigger_head, 0)
    use_with_trigger_head = free_unkept_triggers(use_with_trigger_head, 1)
    run_script(region, 0, 2)
    script_stopped = 0
```

## Outputs

`run_tile_triggers`, `run_key_triggers` and `run_pair_triggers` return 1 when they ran a trigger's
script and 0 otherwise, and set the trigger variables they store. `trigger_mask` returns its mask
and sets `trigger_key`. `region_triggers_change` moves records to the free list and runs the
region's script.

## Edge cases

- A tile or box trigger's byte decides who sets it off: 0 the leader only, 1 any party member, 2
  anyone. Shipped tile triggers use 0 and 2, box triggers 0, 1 and 2 (FND-SCRIPT-027).
- Only the first matching record of a list runs, and a box trigger is tested only when no tile
  trigger ran. The tile and box tests compare signed words.
- `trigger_keyed` tests the second key only on the record at the head of the use-with list, so a later
  use-with record keyed only on its second number does not set bit `0x20` (FND-SCRIPT-028).
- The lists at `g_4C10_0007` and `g_4C10_000B` are emptied at a region change but no event found
  runs them (FND-SCRIPT-028).
- Records go to the free list one at a time in list order, each becoming its head, so the free list
  is left in the reverse of their list order.

## What the sources say

None known.

## Differences between builds

None known.

## Open questions

- Whether field 0 that `fn_1AA0_0009` reads is the object's number, which the shipped keys suggest
  (FND-SCRIPT-028; Q-SCRIPT-002).
- Whether `1695:0235`, which runs the lists at `g_4C10_0007` and `g_4C10_000B` and returns
  `g_4C13_0325`, is reached by a computed call (FND-SCRIPT-028; Q-SCRIPT-016).
- What `fn_2D40_06E6` does when its argument names more than one slot, and what the word at
  `4C10:001F` it clears is for (FND-SCRIPT-028; Q-SCRIPT-002).
