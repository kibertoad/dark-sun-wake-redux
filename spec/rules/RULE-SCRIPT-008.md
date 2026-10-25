---
id: RULE-SCRIPT-008
title: The script instructions that register triggers
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-SCRIPT-005, FND-SCRIPT-010, FND-SCRIPT-014, FND-SCRIPT-015, FND-SCRIPT-027, FND-SCRIPT-028, FND-SCRIPT-029]
conflicting: []
split_with: []
related: [RULE-SCRIPT-002, RULE-SCRIPT-004, RULE-SCRIPT-013, FMT-SCRIPT-004]
---

## Summary

A script registers a trigger: a script entry point that the game runs when a condition is met
(RULE-SCRIPT-013). Instructions `0x65`, `0x66`, `0x6C`, `0x6D` and `0x6E` key a trigger by one
number, `0x68` and `0x69` by a cell and a byte saying who may set it off, `0x6A` and `0x6B` by a box
of cells and such a byte, and `0x70` by two numbers. Registering a one-key, cell or box trigger with
the key of one already on its list replaces that one; each `0x70` adds a trigger.

## When it runs

When `execute_instruction` (RULE-SCRIPT-002) meets one of those opcodes.

## Parameters

`op`, the opcode.

## Inputs

`script_parameters`, `loaded_script_number`, `loaded_script_selector`, `trigger_free_head`,
`trigger_records`, `attack_trigger_head`, `move_tile_trigger_head`, `box_trigger_head`,
`pick_up_trigger_head`, `talk_trigger_head`, `look_trigger_head`, `use_trigger_head`,
`use_with_trigger_head`, `g_4C10_0007`, `g_4C10_000B`.

## Procedure

```text
define take_trigger_record() -> INT16:
    if trigger_free_head == -1:
        return -1
    let n = trigger_free_head
    trigger_free_head = trigger_records[n].next
    return n

# head: a list head or the next field of the record before the place to insert
define link_new_record(head: INT16) -> INT16:
    if head == -1:
        return take_trigger_record()
    let n = take_trigger_record()
    if n != -1:
        trigger_records[n].next = head
        return n
    return head

define insert_by_one_key(head: INT16, record: FMT-SCRIPT-004) -> INT16:
    if head != -1 and trigger_records[head].key_1 < record.key_1:
        trigger_records[head].next = insert_by_one_key(trigger_records[head].next, record)
        return head
    let place = head
    if place == -1 or trigger_records[place].key_1 != record.key_1:
        place = link_new_record(place)
    if place != -1:
        # the first 7 bytes: entry_offset, entry_script, key_1 and the low byte of key_2
        trigger_records[place].entry_offset = record.entry_offset
        trigger_records[place].entry_script = record.entry_script
        trigger_records[place].key_1 = record.key_1
        trigger_records[place].key_2 = (trigger_records[place].key_2 & 0xFF00) | (record.key_2 & 0xFF)
    return place

define insert_by_two_keys(head: INT16, record: FMT-SCRIPT-004) -> INT16:
    if head != -1:
        let node = trigger_records[head]
        if UINT16(node.key_1) < UINT16(record.key_1) or UINT16(node.key_2) < UINT16(record.key_2):
            node.next = insert_by_two_keys(node.next, record)
            return head
    let place = head
    if place == -1 or trigger_records[place].key_1 != record.key_1 or trigger_records[place].key_2 != record.key_2:
        place = link_new_record(place)
    if place != -1:
        # the first 9 bytes
        trigger_records[place].entry_offset = record.entry_offset
        trigger_records[place].entry_script = record.entry_script
        trigger_records[place].key_1 = record.key_1
        trigger_records[place].key_2 = record.key_2
        trigger_records[place].unk_08 = record.unk_08
    return place

define insert_box(head: INT16, record: FMT-SCRIPT-004) -> INT16:
    if head != -1:
        let node = trigger_records[head]
        if UINT16(node.key_1) < UINT16(record.key_1) or UINT16(node.key_2) < UINT16(record.key_2):
            node.next = insert_box(node.next, record)
            return head
    let place = head
    if place == -1 or trigger_records[place].key_1 != record.key_1 or trigger_records[place].key_2 != record.key_2:
        place = link_new_record(place)
    if place != -1:
        # the first 11 bytes
        trigger_records[place].entry_offset = record.entry_offset
        trigger_records[place].entry_script = record.entry_script
        trigger_records[place].key_1 = record.key_1
        trigger_records[place].key_2 = record.key_2
        trigger_records[place].unk_08 = record.unk_08
        trigger_records[place].unk_09 = record.unk_09
        trigger_records[place].unk_0A = record.unk_0A
    return place

define insert_pair(head: INT16, record: FMT-SCRIPT-004) -> INT16:
    if head != -1:
        let node = trigger_records[head]
        if node.key_1 < record.key_1 or node.key_2 < record.key_2:
            node.next = insert_pair(node.next, record)
            return head
    let place = link_new_record(head)
    if place != -1:
        # the first 9 bytes
        trigger_records[place].entry_offset = record.entry_offset
        trigger_records[place].entry_script = record.entry_script
        trigger_records[place].key_1 = record.key_1
        trigger_records[place].key_2 = record.key_2
        trigger_records[place].unk_08 = record.unk_08
    return place

define from_mas_99() -> UINT8:
    if loaded_script_number == 99 and loaded_script_selector == 2:
        return 1
    return 0

define one_key_record() -> FMT-SCRIPT-004:
    read_parameters(3)
    let record = new FMT-SCRIPT-004
    record.entry_offset = UINT16(script_parameters[0])
    record.entry_script = UINT16(script_parameters[1])
    record.key_1 = INT16(script_parameters[2])
    record.key_2 = from_mas_99()
    return record

define cell_record() -> FMT-SCRIPT-004:
    read_parameters(5)
    let record = new FMT-SCRIPT-004
    record.entry_offset = UINT16(script_parameters[2])
    record.entry_script = UINT16(script_parameters[3])
    record.key_1 = INT16(script_parameters[0])
    record.key_2 = INT16(script_parameters[1])
    record.unk_08 = UINT8(script_parameters[4])
    return record

define box_record() -> FMT-SCRIPT-004:
    read_parameters(7)
    let record = new FMT-SCRIPT-004
    record.entry_offset = UINT16(script_parameters[4])
    record.entry_script = UINT16(script_parameters[5])
    record.key_1 = INT16(script_parameters[0])
    record.key_2 = INT16(script_parameters[1])
    record.unk_08 = UINT8(script_parameters[2])
    record.unk_09 = UINT8(script_parameters[3])
    record.unk_0A = UINT8(script_parameters[6])
    return record

if op == 0x65:
    attack_trigger_head = insert_by_one_key(attack_trigger_head, one_key_record())
else if op == 0x66:
    look_trigger_head = insert_by_one_key(look_trigger_head, one_key_record())
else if op == 0x6C:
    pick_up_trigger_head = insert_by_one_key(pick_up_trigger_head, one_key_record())
else if op == 0x6D:
    use_trigger_head = insert_by_one_key(use_trigger_head, one_key_record())
else if op == 0x6E:
    talk_trigger_head = insert_by_one_key(talk_trigger_head, one_key_record())
else if op == 0x68:
    move_tile_trigger_head = insert_by_two_keys(move_tile_trigger_head, cell_record())
else if op == 0x69:
    g_4C10_0007 = insert_by_two_keys(g_4C10_0007, cell_record())
else if op == 0x6A:
    box_trigger_head = insert_box(box_trigger_head, box_record())
else if op == 0x6B:
    g_4C10_000B = insert_box(g_4C10_000B, box_record())
else if op == 0x70:
    read_parameters(4)
    let record = new FMT-SCRIPT-004
    record.entry_offset = UINT16(script_parameters[2])
    record.entry_script = UINT16(script_parameters[3])
    record.key_1 = INT16(script_parameters[0])
    record.key_2 = INT16(script_parameters[1])
    record.unk_08 = from_mas_99()
    use_with_trigger_head = insert_pair(use_with_trigger_head, record)
```

## Outputs

Nothing is returned. A record moves from the free list to the list the opcode names, or an
existing record on that list is overwritten.

## Edge cases

When the free list is empty and no record has the key, nothing is registered. The one-key list
is kept in ascending order of `key_1` compared as signed numbers. The two-key walk moves past
every record whose `key_1` or `key_2` is below the new record's, compared as unsigned numbers,
so a record with a larger `key_1` and a smaller `key_2` does not stop it; the list is in pair
order only while no such record exists. Only the low byte of `key_2` is written by `0x65`: it is 1
when `MAS ` resource 99 registered the trigger and 0 otherwise, and a region change removes the
triggers on this list whose byte is 0 (FND-SCRIPT-013, FND-SCRIPT-014). The same holds for the
other one-key lists, and for byte `unk_08` of a `0x70` record (FND-SCRIPT-028).

The box lists are walked like the cell lists and copy 11 bytes. `0x70` compares its keys as signed
numbers and always takes a new record, so the same pair registered twice is on the list twice, and
when the free list is empty the record at its place is overwritten (FND-SCRIPT-027). Opcodes `0x69`
and `0x6B` fill lists no event found runs, and no shipped script uses them.

## What the sources say

SRC-OPENDS-5C6CBD7 (`docs/gpl-opcodes.md`) names `0x65` attacktrigger with 3 parameters and
`0x68` move tiletrigger with 5.

## Differences between builds

None known.

## Open questions

- Whether anything runs the lists at `g_4C10_0007` and `g_4C10_000B`, which `0x69` and `0x6B` fill:
  the only walker found for them, `1695:0235`, has no direct caller (FND-SCRIPT-028; Q-SCRIPT-016).
