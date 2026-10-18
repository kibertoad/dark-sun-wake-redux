---
id: RULE-SCRIPT-011
title: The script instructions that change a variable in place
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-SCRIPT-005, FND-SCRIPT-009, FND-SCRIPT-010, FND-SCRIPT-025]
conflicting: []
split_with: []
related: [RULE-SCRIPT-002, RULE-SCRIPT-003, RULE-SCRIPT-004]
---

## Summary

Seventeen instructions change the variable their first parameter names, in place: they add 1
to it, take 1 from it, or add, subtract, multiply or divide it by their second parameter, as a
byte, a word or a double word chosen by the opcode.

## When it runs

When `execute_instruction` (RULE-SCRIPT-002) meets one of the opcodes `0x01` to `0x05`, `0x07`,
`0x11` or `0x76` to `0x7F`.

## Parameters

- `op`: the opcode, one of `0x01` to `0x05`, `0x07`, `0x11` and `0x76` to `0x7F`.

## Inputs

`script_parameters`, `script_parameter_addresses`.

## Procedure

```text
define read_unsigned_word(p: FARPTR<UINT8>) -> UINT16:
    return UINT16(p[0] + p[1] * 256)

define write_long(p: FARPTR<UINT8>, value):
    p[0] = UINT8(value)
    p[1] = UINT8(value >> 8)
    p[2] = UINT8(value >> 16)
    p[3] = UINT8(value >> 24)

if op == 0x02 or op == 0x03 or op == 0x04 or op == 0x05 or op == 0x07:
    read_parameters(1)
else:
    read_parameters(2)
let p = script_parameter_addresses[0]
let v: INT32 = script_parameters[1]
if op == 0x01:
    # signed division with truncation; a zero divisor or an overflowing quotient raises the
    # processor's divide error
    write_long(p, read_long(p) / v)
else if op == 0x02:
    p[0] = UINT8(p[0] - 1)
else if op == 0x03:
    write_word(p, read_word(p) - 1)
else if op == 0x04:
    write_long(p, read_long(p) - 1)
else if op == 0x05:
    p[0] = UINT8(p[0] + 1)
else if op == 0x07:
    write_long(p, read_long(p) + 1)
else if op == 0x11:
    write_long(p, read_long(p) * v)
else if op == 0x76:
    p[0] = UINT8(p[0] + v)
else if op == 0x77:
    p[0] = UINT8(p[0] - v)
else if op == 0x78:
    p[0] = UINT8(INT32(p[0]) * v)
else if op == 0x79:
    # a zero divisor raises the processor's divide error
    p[0] = UINT8(INT32(p[0]) / v)
else if op == 0x7A:
    write_word(p, read_word(p) + v)
else if op == 0x7B:
    write_word(p, read_word(p) - v)
else if op == 0x7C:
    write_word(p, INT32(read_unsigned_word(p)) * v)
else if op == 0x7D:
    # a zero divisor raises the processor's divide error
    write_word(p, INT32(read_unsigned_word(p)) / v)
else if op == 0x7E:
    write_long(p, read_long(p) + v)
else if op == 0x7F:
    write_long(p, read_long(p) - v)
```

## Outputs

The byte, word or double word at the first parameter's address holds the result; nothing else
changes. The opcodes with one parameter do not use `v`.

## Edge cases

The width comes from the opcode, whatever kind of variable the first parameter names: a word
operation on a flag changes the 16 flags of two bytes, a byte operation on a number changes its
low byte, and a literal first parameter makes the address the accumulator's, so the operation
changes `script_accumulator`. Byte and word results wrap; the byte and word products and
quotients use the stored value as an unsigned number and keep the low 8 or 16 bits of a
signed 32-bit result, so a negative divisor gives the low bits of a negative quotient. Division
truncates toward zero, and no divisor is checked. A change to a local variable through these
instructions does not set `script_locals_changed`.

## What the sources say

None consulted.

## Differences between builds

None known.

## Open questions

None known.
