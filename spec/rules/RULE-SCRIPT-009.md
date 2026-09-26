---
id: RULE-SCRIPT-009
title: The script trace instructions
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-SCRIPT-005, FND-SCRIPT-006, FND-SCRIPT-007, FND-SCRIPT-010, FND-TALK-001]
conflicting: []
split_with: []
related: [RULE-SCRIPT-002, RULE-SCRIPT-004, RULE-TALK-001]
---

## Summary

Four instructions, `0x23`, `0x28`, `0x2E` and `0x4B`, read their parameters and set one flag.
They do nothing else in this build. The menu instruction runs any that stand before or between
its entries.

## When it runs

When `execute_instruction` (RULE-SCRIPT-002) meets one of the four opcodes, and when
`run_trace_instructions` runs for the menu instruction (RULE-TALK-001).

## Parameters

`op`, the opcode, one of `0x23`, `0x28`, `0x2E` and `0x4B`.

## Inputs

`script_parameters`, `script_parameter_addresses`.

## Procedure

```text
define trace_instruction(op: UINT8):
    if op == 0x23:
        g_4C13_032B = 1
        read_string()
    else if op == 0x28:
        g_4C13_032B = 1
        read_number()
        read_number()
        read_string()
    else if op == 0x2E:
        g_4C13_032B = 1
        read_number()
    else if op == 0x4B:
        read_parameters(3)
        g_4C13_032B = 1

define is_trace_opcode(op: UINT8) -> bool:
    return op == 0x23 or op == 0x28 or op == 0x2E or op == 0x4B

define run_trace_instructions():
    while is_trace_opcode(peek_byte(0)):
        trace_instruction(fetch_opcode())

trace_instruction(op)
```

## Outputs

Nothing is returned. `g_4C13_032B` is 1, the parameters are read past, and `0x4B` leaves its
three in `script_parameters` and `script_parameter_addresses`.

## Edge cases

`0x23` and the last parameter of `0x28` are read by `read_string` directly, without the `0x92`
prefix an expression would need (RULE-SCRIPT-004). A string parameter's kind byte is its first
byte. `run_trace_instructions` runs the same code without passing through `execute_instruction`,
so the far routine at `g_57E0_02F6` is not called for these.

## What the sources say

SRC-OPENDS-5C6CBD7 (`docs/gpl-opcodes.md`) names `0x23` source trace, `0x28` trace var, `0x2E`
source line num and `0x4B` local sub trace, and says the libgff handler of each is a placeholder.

## Differences between builds

None known.

## Open questions

- What `g_4C13_032B` means and what reads it (Q-SCRIPT-003).
