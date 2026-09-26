---
id: RULE-SCRIPT-007
title: The script instructions that print text and numbers, show a portrait and play sound and music
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-SCRIPT-005, FND-SCRIPT-010, FND-SCRIPT-012, FND-SOUND-009]
conflicting: []
split_with: []
related: [RULE-SCRIPT-002, RULE-SCRIPT-004]
---

## Summary

Scripts put text and numbers on screen, start new lines, show a character's portrait, and start
sounds and music, each with one instruction.

## When it runs

When `execute_instruction` (RULE-SCRIPT-002) meets opcode `0x4F`, `0x50`, `0x51`, `0x54`, `0x5D`
or `0x5F`.

## Parameters

`op`, the opcode.

## Inputs

`script_parameters`, `script_parameter_addresses`.

## Procedure

```text
if op == 0x4F:
    read_parameters(2)
    emit TextPrinted(script_parameter_addresses[1], UINT8(script_parameters[0]))
else if op == 0x50:
    read_parameters(2)
    emit NumberPrinted(script_parameters[1], UINT8(script_parameters[0]))
else if op == 0x51:
    emit NewLinePrinted()
else if op == 0x54:
    emit PortraitShown(UINT16(read_number()))
else if op == 0x5D:
    emit SoundRequested(UINT16(read_number()))
else if op == 0x5F:
    emit MusicRequested(UINT16(read_number()))
```

## Outputs

The events above, in the order the script runs the instructions. Nothing else changes here.

## Edge cases

`0x4F` passes the address its second parameter left, which is the text of a string parameter
(RULE-SCRIPT-004) or the variable a variable parameter named. The first parameter of `0x4F` and
`0x50` is passed on as a byte.

## What the sources say

SRC-OPENDS-5C6CBD7 (`docs/gpl-opcodes.md`) names `0x4F` print string and `0x50` print number with
two parameters, `0x51` printnl, `0x54` showpic, `0x5D` sound and `0x5F` music.

## Differences between builds

None known.

## Open questions

- What the far routines behind the events do: where text goes and what the byte selects, how a
  portrait number maps to a `PORT` resource, and what the sound and music routines play
  (Q-SCRIPT-006).
