---
id: RULE-SCRIPT-008
title: The script instructions that register attack and move-tile triggers
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-SCRIPT-005, FND-SCRIPT-010, FND-SCRIPT-014, FND-SCRIPT-015]
conflicting: []
split_with: []
related: [RULE-SCRIPT-002, RULE-SCRIPT-004, FMT-SCRIPT-004]
---

## Summary

A script registers a trigger: a script entry point that the game runs when a condition is met.
Instruction `0x65` keys its trigger by one number, and `0x68` by two numbers and a range byte.
Registering a trigger with the key of one already on its list replaces that one.

## When it runs

When `execute_instruction` (RULE-SCRIPT-002) meets opcode `0x65` or `0x68`.

## Parameters

`op`, the opcode.

## Inputs

`script_parameters`, `loaded_script_number`, `loaded_script_selector`, `trigger_free_head`,
`trigger_records`, `attack_trigger_head`, `move_tile_trigger_head`.

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

if op == 0x65:
    read_parameters(3)
    let record = new FMT-SCRIPT-004
    record.entry_offset = UINT16(script_parameters[0])
    record.entry_script = UINT16(script_parameters[1])
    record.key_1 = INT16(script_parameters[2])
    if loaded_script_number == 99 and loaded_script_selector == 2:
        record.key_2 = 1
    attack_trigger_head = insert_by_one_key(attack_trigger_head, record)
else if op == 0x68:
    read_parameters(5)
    let record = new FMT-SCRIPT-004
    record.entry_offset = UINT16(script_parameters[2])
    record.entry_script = UINT16(script_parameters[3])
    record.key_1 = INT16(script_parameters[0])
    record.key_2 = INT16(script_parameters[1])
    record.unk_08 = UINT8(script_parameters[4])
    move_tile_trigger_head = insert_by_two_keys(move_tile_trigger_head, record)
```

## Outputs

Nothing is returned. A record moves from the free list to the list that
`attack_trigger_head` or `move_tile_trigger_head` starts, or an existing record on that list is
overwritten.

## Edge cases

When the free list is empty and no record has the key, nothing is registered. The one-key list
is kept in ascending order of `key_1` compared as signed numbers. The two-key walk moves past
every record whose `key_1` or `key_2` is below the new record's, compared as unsigned numbers,
so a record with a larger `key_1` and a smaller `key_2` does not stop it; the list is in pair
order only while no such record exists. Only the low byte of `key_2` is written by `0x65`: it is 1
when `MAS ` resource 99 registered the trigger and 0 otherwise, and a region change removes the
triggers on this list whose byte is 0 (FND-SCRIPT-013, FND-SCRIPT-014).

## What the sources say

SRC-OPENDS-5C6CBD7 (`docs/gpl-opcodes.md`) names `0x65` attacktrigger with 3 parameters and
`0x68` move tiletrigger with 5.

## Differences between builds

None known.

## Open questions

- What the keys and `unk_08` stand for, and when the game tests each list and runs the trigger's
  script (Q-SCRIPT-002).
- The other trigger instructions, `0x66`, `0x69` to `0x6F` and `0x70` (Q-SCRIPT-004).
