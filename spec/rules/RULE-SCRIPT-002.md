---
id: RULE-SCRIPT-002
title: Running a script, reading its code and calling other scripts
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-SCRIPT-001, FND-SCRIPT-005, FND-SCRIPT-006, FND-SCRIPT-007, FND-SCRIPT-008, FND-SCRIPT-009, FND-SCRIPT-013]
conflicting: []
split_with: []
related: [RULE-SCRIPT-001, RULE-SCRIPT-003, RULE-SCRIPT-006, RULE-SCRIPT-007, RULE-SCRIPT-008, FMT-SCRIPT-001]
---

## Summary

The game runs a script from a given offset. The interpreter reads one instruction byte at a time
and carries it out, until the script it was started in returns to the game or an instruction
stops it. Scripts call subroutines inside themselves and other scripts; each call pushes a frame,
up to 50.

## When it runs

`run_script` runs when the game starts a script: `MAS ` resource 99 once the game has made its
working save archive, the `MAS ` resource of a region when the region changes, and `GPL `
entry points held by the script triggers (FND-SCRIPT-013).

## Parameters

`run_script(number, start, selector)`: the resource number, the offset in it to start at, and
the selector of RULE-SCRIPT-001. `call_script` takes the same. `push_frame(offset)` takes an
offset in the current script, and `peek_byte(ahead)` a count of bytes past the current one.

## Inputs

`g_4C0E_000B`, `script_stopped`, `script_frame_depth`, `script_frame_offsets`, `script_depth`,
`script_stack_numbers`, `script_stack_selectors`, `script_code_start`, `script_buffer`,
`script_buffer_size`, `script_if_depth`, `script_compare_depth`, `g_57E0_02F6`.

## Procedure

```text
define run_script(number: UINT16, start: UINT16, selector: UINT16):
    if g_4C0E_000B != 2:
        return
    fn_172C_31ED()
    script_stopped = 0
    script_status = 1
    g_4C13_032B = 0
    g_4C13_0325 = 0
    script_depth = 0
    script_frame_depth = 0
    script_if_depth = 0
    script_compare_depth = 0
    if number != 0 and script_stopped != 1:
        call_script(number, start, selector)
        while script_frame_depth >= 0 and script_stopped == 0:
            execute_instruction(fetch_opcode())
    script_stopped = 0

define call_script(number: UINT16, start: UINT16, selector: UINT16):
    if script_stopped == 1:
        return
    script_status = 1
    script_depth = script_depth + 1
    script_stack_numbers[script_depth] = number
    script_stack_selectors[script_depth] = selector
    if not load_script(number, selector):
        fn_5702_00B1()
    if script_stopped == 0:
        push_frame(start)

define return_from_script():
    script_depth = script_depth - 1
    if script_depth > 0:
        if not load_script(script_stack_numbers[script_depth], script_stack_selectors[script_depth]):
            fn_5702_00B1()
    else:
        stop_script()
    if script_stopped == 0:
        pop_frame()

define stop_script():
    script_stopped = 1

define push_frame(offset: UINT16):
    script_frame_depth = script_frame_depth + 1
    if script_frame_depth >= 50:
        script_frame_depth = 0
        fn_5702_00B1()
    script_frame_if_depths[script_frame_depth] = script_if_depth
    script_frame_compare_depths[script_frame_depth] = script_compare_depth
    script_frame_offsets[script_frame_depth] = offset

define pop_frame():
    script_if_depth = script_frame_if_depths[script_frame_depth]
    script_compare_depth = script_frame_compare_depths[script_frame_depth]
    script_frame_depth = script_frame_depth - 1
    if script_frame_depth < 0:
        fn_5702_00B1()

define jump_to(offset: UINT16):
    script_frame_offsets[script_frame_depth] = offset

define peek_byte(ahead: UINT8) -> UINT8:
    return script_buffer[UINT16(script_code_start + script_frame_offsets[script_frame_depth] + ahead)]

define fetch_byte() -> UINT8:
    let b = peek_byte(0)
    script_frame_offsets[script_frame_depth] = script_frame_offsets[script_frame_depth] + 1
    if UINT32(script_frame_offsets[script_frame_depth]) >= script_buffer_size:
        fn_5702_00B1()
    return b

define fetch_opcode() -> UINT8:
    script_opcode = fetch_byte()
    return script_opcode

define fetch_word() -> UINT16:
    let high = fetch_byte()
    let low = fetch_byte()
    return UINT16(high * 256 + low)

define stops_script(op: UINT8) -> bool:
    let stopping: UINT8[15] = [0x26, 0x4A, 0x4C, 0x4D, 0x4E, 0x53, 0x55, 0x56, 0x57, 0x60, 0x71, 0x72, 0x73, 0x74, 0x75]
    for each s in stopping:
        if op == s:
            return true
    return false

define execute_instruction(op: UINT8):
    if op > 0x80 or stops_script(op):
        script_status = 0xFFFF
        stop_script()
        return
    # when g_57E0_02F6 is not 0, the far routine it holds runs here with op
    if op == 0x06 or op == 0x0E or op == 0x27 or op == 0x29 or op == 0x31:
        call RULE-SCRIPT-003(op)
    else if (op >= 0x12 and op <= 0x19) or op == 0x3E or op == 0x3F or op == 0x61:
        call RULE-SCRIPT-003(op)
    else if op == 0x63 or op == 0x64 or op == 0x67:
        call RULE-SCRIPT-003(op)
    else if op == 0x4F or op == 0x50 or op == 0x51 or op == 0x54 or op == 0x5D or op == 0x5F:
        call RULE-SCRIPT-007(op)
    else if op == 0x52:
        call RULE-SCRIPT-006()
    else if op == 0x65 or op == 0x68:
        call RULE-SCRIPT-008(op)
    # every other opcode from 0x00 to 0x80 runs the handler FND-SCRIPT-005 lists for it,
    # which this spec does not describe yet
```

## Outputs

`run_script` returns nothing. It runs the instructions of the script and leaves `script_stopped`
0. `fetch_byte`, `fetch_opcode`, `fetch_word` and `peek_byte` return the code bytes described
above; the other functions return nothing and change the state their procedures assign.

## Edge cases

`run_script` does nothing in any state but `g_4C0E_000B == 2`, and nothing for script number 0.
The loop ends when the stop flag is set, by a stop instruction, by an unknown opcode, or by a
script return from the first script of the run, which stops instead of popping, or when the frame
depth falls below 0. A local return from the first frame of the run leaves the depth 0, and the
loop goes on reading at the offset frame 0 held before the run; the next local return reports an
error and ends the loop. Words in the code are stored
high byte first. The 51st nested frame wraps the depth to 0 after reporting the error, and a pop
below 0 reports it after the depth has become -1. Nothing limits `script_depth`: from 28 on,
`script_stack_numbers` overlaps `script_frame_offsets`. The offset check compares an offset
inside one script with the size of the whole buffer. A callee script replaces the caller in the
cache's current slot; `return_from_script` loads the caller again.

## What the sources say

SRC-OPENDS-5C6CBD7 (`docs/gpl-opcodes.md`) names `0x13` local sub, `0x14` global sub, `0x15`
local ret, `0x19` global ret and `0x31` exit gpl, and lists the fifteen opcodes that stop here as
placeholders it could not name.

## Differences between builds

None known.

## Open questions

- What `fn_172C_31ED` and `fn_5702_00B1` do, whether `fn_5702_00B1` returns, and what the far
  routine at `g_57E0_02F6` is when one is set (Q-SCRIPT-003).
- What `g_4C0E_000B`, `g_4C13_032B` and `g_4C13_0325` mean (Q-SCRIPT-003).
- The instructions whose handlers this spec does not yet describe (Q-SCRIPT-004).
