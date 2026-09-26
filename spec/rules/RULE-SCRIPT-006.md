---
id: RULE-SCRIPT-006
title: The script instruction that draws a random number
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-001, FND-SCRIPT-005, FND-SCRIPT-009, FND-SCRIPT-010, FND-SCRIPT-011]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-SCRIPT-002, RULE-SCRIPT-004]
---

## Summary

Instruction `0x52` sets the accumulator to a random number from 0 to its parameter.

## When it runs

When `execute_instruction` (RULE-SCRIPT-002) meets opcode `0x52`.

## Parameters

None.

## Inputs

`rng_state`.

## Procedure

```text
let d: INT32 = draw()
let n = read_number()
script_accumulator = INT32(INT16((d * (n + 1)) / 0x8000))
```

## Outputs

`script_accumulator` holds the result. The draw moves `rng_state` on.

## Edge cases

The draw comes before the parameter is read, so a parameter that itself draws (through `0x8C`,
RULE-SCRIPT-004) draws second. For n from 0 to 32,767 the result is 0 to n. Above that, only the
quotient's low 16 bits are kept, so large results turn negative, and from n = 65,536 on the
32-bit product can overflow. n = -1 gives 0, and n below -1 gives n + 2 to 0. Every case makes
one draw.

## What the sources say

SRC-OPENDS-5C6CBD7 (`docs/gpl-opcodes.md`) names `0x52` rand with one parameter.

## Differences between builds

None known.

## Open questions

None known.
