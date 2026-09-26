---
id: RULE-TALK-001
title: The script instruction that offers a menu of responses and runs the chosen one
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-SCRIPT-005, FND-SCRIPT-006, FND-SCRIPT-010, FND-TALK-001, FND-TALK-002, FND-TALK-003, FND-TALK-004]
conflicting: []
split_with: []
related: [RULE-SCRIPT-002, RULE-SCRIPT-003, RULE-SCRIPT-004, RULE-SCRIPT-009, SCR-UI-012]
---

## Summary

Instruction `0x48` shows a title and a list of responses, each with a condition and a target
offset. The responses whose condition is 1 are offered, in the order the script lists them, up
to 25. When the player has chosen one, the script runs its target as a local subroutine and then
goes on after the menu.

## When it runs

When `execute_instruction` (RULE-SCRIPT-002) meets opcode `0x48`.

## Parameters

None. The instruction's operands follow it in the script code: a title, then for each entry a
label, a target and a condition, each an expression (RULE-SCRIPT-004), and the byte `0x4A` after
the last entry.

## Inputs

`script_menu_targets`, `script_menu_conditions`, and the row number `fn_5702_0025` returns.

## Procedure

```text
let offered: UINT8 = 0
run_trace_instructions()
read_number()
emit MenuRowShown(0, script_operand_address)
while peek_byte(0) != 0x4A and offered <= 24:
    run_trace_instructions()
    read_number()
    let label = script_operand_address
    script_menu_targets[offered] = UINT16(read_number())
    script_menu_conditions[offered] = UINT8(read_number())
    if script_menu_conditions[offered] == 1:
        emit MenuRowShown(offered + 1, label)
        offered = offered + 1
    run_trace_instructions()
fetch_byte()
let row: INT16 = -1
while row < 0 or row >= offered:
    row = INT16(fn_5702_0025()) - 1
push_frame(script_menu_targets[row])
```

## Outputs

`MenuRowShown` for the title and each offered entry, in that order. A new frame starts at the
chosen entry's target, so the next instruction the interpreter runs is the first of that target.

## Edge cases

- The condition is compared as its low byte, so a condition of 257 offers the entry and one of 2
  or 256 does not. A comparison or a flag gives 0 or 1 (RULE-SCRIPT-004).
- An entry that is not offered still has its target and condition written, at the index the next
  offered entry will use.
- After 25 offered entries the instruction stops reading entries and consumes the next byte,
  whatever it is. Its later entries are then read as instructions. No menu in the scripts has
  more than 24 entries (FND-TALK-003).
- `script_menu_targets` has room for 12 targets. The targets of offered-count 12 to 24 are
  written over `script_frame_offsets[0]` to `script_frame_offsets[12]` (RULE-SCRIPT-002), so a menu
  whose offered count reaches 12 plus the current frame depth overwrites the offset the
  interpreter is reading from with that entry's target, and the rest of the menu is read from
  there.
- With no entry offered, every row fails the test and the instruction calls `fn_5702_0025` again
  without end. One menu in the scripts has no entries (FND-TALK-003).
- The target runs in a new frame of the same script. A local return (`0x15`, RULE-SCRIPT-003) at
  its end comes back to the byte after the menu's `0x4A`.

## What the sources say

SRC-OPENDS-5C6CBD7 (`docs/gpl-opcodes.md`) names `0x48` menu and gives its operands as one
expression for the menu name and three per entry until `0x4A`, as here. SRC-MANUAL-1994, page 6,
says the lower window lists the responses the player may make, and that the player selects one
by clicking it; page 77 adds the keys `1` to `5` (SCR-UI-012).

## Differences between builds

None known.

## Open questions

- What `fn_5702_0025` and the row routine behind `MenuRowShown` do: how rows beyond the five on
  screen are reached, how a click or a key becomes a row number, and why the title is drawn in
  capitals (Q-TALK-001).
