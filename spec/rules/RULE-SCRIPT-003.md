---
id: RULE-SCRIPT-003
title: The script instructions for jumps, calls, returns, if, while, compare, the accumulator and assignment
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-SCRIPT-001, FND-SCRIPT-005, FND-SCRIPT-006, FND-SCRIPT-008, FND-SCRIPT-009, FND-SCRIPT-010]
conflicting: []
split_with: []
related: [RULE-SCRIPT-002, RULE-SCRIPT-004]
---

## Summary

These instructions steer a script: jumps, calls of subroutines in the same script and in other
scripts, returns, `if`/`else`/`endif`, `while` loops, and `compare` blocks that pick one of
several cases. They test the accumulator, a number most instructions leave their result in, and
two of them change it or copy it into a variable.

## When it runs

When `execute_instruction` (RULE-SCRIPT-002) meets one of the opcodes below.

## Parameters

`op`, the opcode, one of `0x06`, `0x0E`, `0x12` to `0x19`, `0x27`, `0x29`, `0x31`, `0x3E`,
`0x3F`, `0x61`, `0x63`, `0x64` and `0x67`.

## Inputs

`script_accumulator`, `script_parameters`, `script_parameter_addresses`, `script_if_depth`,
`script_if_results`, `script_compare_depth`, `script_compare_values`, `script_compare_matched`.

## Procedure

```text
if op == 0x06:
    read_parameters(1)
    let p = script_parameter_addresses[0]
    write_word(p, read_word(p) + 1)
else if op == 0x0E:
    if script_accumulator == 0:
        script_accumulator = 1
    else:
        script_accumulator = 0
else if op == 0x12:
    jump_to(UINT16(read_number()))
else if op == 0x13:
    push_frame(UINT16(read_number()))
else if op == 0x14:
    read_parameters(2)
    call_script(UINT16(script_parameters[1]), UINT16(script_parameters[0]), 1)
else if op == 0x15:
    pop_frame()
else if op == 0x16:
    script_accumulator = read_number()
    write_variable(script_accumulator)
else if op == 0x17:
    script_accumulator = read_number()
    script_compare_depth = script_compare_depth + 1
    if script_compare_depth >= 8:
        fn_5702_00B1()
    script_compare_matched[script_compare_depth] = 0
    script_compare_values[script_compare_depth] = script_accumulator
else if op == 0x18:
    script_accumulator = read_number()
else if op == 0x19:
    return_from_script()
else if op == 0x27:
    read_parameters(2)
    if script_compare_values[script_compare_depth] != script_parameters[0]:
        jump_to(UINT16(script_parameters[1]))
    else:
        script_compare_matched[script_compare_depth] = 1
else if op == 0x29:
    let target = UINT16(read_number())
    if script_compare_matched[script_compare_depth] == 1:
        jump_to(target)
else if op == 0x31:
    stop_script()
else if op == 0x3E:
    read_parameters(1)
    script_if_depth = script_if_depth + 1
    if script_if_depth >= 32:
        fn_5702_00B1()
    script_if_results[script_if_depth] = UINT8(script_accumulator)
    if script_accumulator == 0:
        jump_to(UINT16(script_parameters[0]))
else if op == 0x3F:
    read_parameters(1)
    if script_if_results[script_if_depth] != 0:
        jump_to(UINT16(script_parameters[0]))
else if op == 0x61:
    script_compare_depth = script_compare_depth - 1
    if script_compare_depth < 0:
        fn_5702_00B1()
else if op == 0x63:
    read_parameters(1)
    if script_accumulator == 0:
        jump_to(UINT16(script_parameters[0]))
else if op == 0x64:
    jump_to(UINT16(read_number()))
else if op == 0x67:
    script_if_depth = script_if_depth - 1
    if script_if_depth < 0:
        fn_5702_00B1()
```

## Outputs

Nothing is returned. The instructions change the current frame's offset, the frame and script
stacks, the accumulator, the `if` and `compare` state and the variable `0x06` or `0x16` names, as
the procedure assigns.

## Edge cases

`if` (`0x3E`) tests the accumulator the instructions before it left; its parameter is the
offset to jump to. It keeps only the accumulator's low byte for its `else` (`0x3F`), so an
accumulator of 256 is true for the `if`, which does not jump, and false for the `else`, which
does not jump either, and both branches run. `while` (`0x63`) jumps
out when the accumulator is 0, and `0x64` jumps back to the head. A `compare` block: `0x17` sets
the value; each case `0x27` jumps past its body to its second parameter unless the value equals
its first, and marks the block matched when it does; `0x29`, at the end of a case's body, jumps to
the block's end once a case has matched; `0x61` closes the block. Levels are counted from 1; the
32nd nested `if` and the 8th nested `compare` report an error through `fn_5702_00B1` and go on
writing past their arrays. `0x06` adds 1 to a 16-bit word at the address its parameter left,
whatever kind of variable that is. `0x16` writes nothing for kinds it does not handle
(RULE-SCRIPT-004). Each frame keeps its caller's `if` and `compare` depths, and a return restores
them (RULE-SCRIPT-002). Jump targets are offsets in the current script and use the low 16 bits of
the parameter.

## What the sources say

SRC-OPENDS-5C6CBD7 (`docs/gpl-opcodes.md`) names these opcodes, from SRC-LIBGFF-839B11D: `0x06`
word inc, `0x0E` toggle accum, `0x12` jump, `0x13` local sub, `0x14` global sub, `0x15` local
ret, `0x16` load variable, `0x17` compare, `0x18` load accum, `0x19` global ret, `0x27`
ifcompare, `0x29` orelse, `0x31` exit gpl, `0x3E` if, `0x3F` else, `0x61` cmpend, `0x63` while,
`0x64` wend and `0x67` endif. It agrees with the parameter counts above.

## Differences between builds

None known.

## Open questions

- What `fn_5702_00B1` does after an error, and so what the overflowing `if` and `compare` levels
  do (Q-SCRIPT-003).
