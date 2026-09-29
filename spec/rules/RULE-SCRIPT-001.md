---
id: RULE-SCRIPT-001
title: Loading a GPL or MAS script into the script cache
status: superseded
builds: [BLD-GOG-EN-1.1]
superseded_by: [RULE-SCRIPT-010]
evidence: [FND-SCRIPT-001, FND-SCRIPT-003, FND-SCRIPT-006, FND-SCRIPT-007, FND-SCRIPT-008]
conflicting: []
split_with: []
related: [FMT-SCRIPT-001]
---

## Summary

Superseded by RULE-SCRIPT-010. The read-failure path writes bounds and
ages before the transfer and contains no local rollback. The original
procedure below is retained as history; its `historical-text` block does
not define active functions.

Scripts are `GPL ` and `MAS ` resources of `GPLDATA.GFF`. Before the interpreter runs one, it
copies the resource into a shared script buffer, followed by one extra stop instruction, and
remembers where it put it in one of 16 cache slots, so that a script used again is not read
again.

## When it runs

Whenever the interpreter enters a script or returns to the script that called it
(RULE-SCRIPT-002).

## Parameters

`load_script(number, selector)`: `number`, the resource number; `selector`, 1 for a `GPL `
resource and 2 for a `MAS ` resource.

## Inputs

`script_stopped`, `loaded_script_number`, `loaded_script_selector`, `script_cache_numbers`,
`script_cache_selectors`, `script_cache_starts`, `script_cache_ages`, `script_buffer`.

## Procedure

```historical-text
define load_script(number: UINT16, selector: UINT16) -> bool:
    if script_stopped == 1:
        return false
    if number == loaded_script_number and selector == loaded_script_selector:
        return true
    fn_172C_31ED()
    let found = false
    for slot in 0..16:
        if script_cache_numbers[slot] == number and script_cache_selectors[slot] == selector:
            script_code_start = script_cache_starts[slot]
            script_cache_ages[slot] = 0
            found = true
    if not found:
        found = fill_script_slot(number, selector)
    if found:
        loaded_script_number = number
        loaded_script_selector = selector
    age_script_slots()
    return found

define fill_script_slot(number: UINT16, selector: UINT16) -> bool:
    if number == 0xFFFF:
        return false
    if selector != 1 and selector != 2:
        return false
    let slot = 0xFFFF
    for i in 0..16:
        if slot == 0xFFFF and script_cache_starts[i] == 0xFFFF:
            slot = i
    if slot == 0xFFFF:
        slot = fn_172C_07BB()
    if script_cache_numbers[slot] != number:
        let code = resource("GPLDATA.GFF", "GPL", number)
        if selector == 2:
            code = resource("GPLDATA.GFF", "MAS", number)
        let size = count(code)
        let start = fn_172C_0698(size + 1)
        if script_stopped == 1:
            return false
        script_cache_starts[slot] = start
        script_cache_ends[slot] = start + size + 1
        age_script_slots()
        for i in 0..size:
            script_buffer[start + i] = code[i]
        script_buffer[start + size] = 0x31
        script_cache_numbers[slot] = number
        script_cache_selectors[slot] = selector
        script_cache_ages[slot] = 0
    script_code_start = script_cache_starts[slot]
    return true

define age_script_slots():
    for slot in 0..16:
        if script_cache_ages[slot] >= 0 and script_cache_ages[slot] < 0x7F:
            script_cache_ages[slot] = script_cache_ages[slot] + 1
```

## Outputs

`load_script` returns true when the script is ready to run from `script_code_start` in
`script_buffer`. It changes `loaded_script_number`, `loaded_script_selector`, `script_code_start`
and the cache, and `fn_172C_31ED` and `fn_172C_0698` change what they change.

## Edge cases

The same script asked for twice in a row is not looked up again, and ages do not change. A
failed size query or read of the resource calls `fn_5702_00B1` and leaves the slot as it was. A
slot is reused without reading when it already holds the number asked for, whatever its
selector. The byte `0x31` after the code stops a script that runs off its end
(RULE-SCRIPT-003).

## What the sources say

SRC-OPENDS-5C6CBD7 (`docs/gpl-bytecode.md`, section 1) says the scripts live in `GPLDATA.GFF` as
`GPL ` and `MAS ` resources.

## Differences between builds

None known.

## Open questions

- How `fn_172C_07BB` picks a slot to reuse and how `fn_172C_0698` finds room in the buffer, and
  what `fn_172C_31ED` does (Q-SCRIPT-003).
- Which open archive the resource routines read; the game opens `GPLDATA.GFF` at start
  (FND-SCRIPT-003), and the procedure assumes the resource comes from it (Q-SCRIPT-003).
- Whether a slot can hold a script of the other selector with the same number when it is taken
  (Q-SCRIPT-003).
