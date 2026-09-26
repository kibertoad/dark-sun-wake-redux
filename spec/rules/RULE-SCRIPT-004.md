---
id: RULE-SCRIPT-004
title: Reading instruction parameters, expressions, variables and strings
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-SCRIPT-006, FND-SCRIPT-009, FND-SCRIPT-010]
conflicting: []
split_with: []
related: [RULE-SCRIPT-002, FMT-SCRIPT-002]
---

## Summary

Every parameter of a script instruction is an expression: numbers, variables, strings and the
result of another instruction, joined by operators that are applied strictly from left to right,
with parentheses. Variables are flags, 16-bit numbers, 32-bit numbers and strings, each kind in
a global and a local set.

## When it runs

Whenever an instruction reads a parameter or assigns a variable.

## Parameters

`read_parameters(n)`: how many parameters to read, at most 8. `read_word(p)`,
`write_word(p, value)` and `read_long(p)`: an address and a value. `write_variable(value)`: the
value to store.

## Inputs

`script_accumulator`, `script_parameter_stack_depth`, `global_flags`, `local_flags`,
`global_numbers`, `local_numbers`, `global_big_numbers`, `local_big_numbers`, `global_strings`,
`local_strings`, `global_name_pointers`, `script_string`, `script_saved_parameters`,
`script_saved_parameter_addresses`.

## Procedure

```text
define read_word(p: FARPTR<UINT8>) -> INT16:
    return INT16(p[0] + p[1] * 256)

define write_word(p: FARPTR<UINT8>, value):
    p[0] = UINT8(value)
    p[1] = UINT8(value >> 8)

define read_long(p: FARPTR<UINT8>) -> INT32:
    return INT32(p[0] + p[1] * 0x100 + p[2] * 0x10000 + UINT32(p[3]) * 0x1000000)

define read_parameters(n):
    for i in 0..n:
        script_parameters[i] = read_number()
        script_parameter_addresses[i] = script_operand_address

define read_variable(code: UINT8) -> INT32:
    # code: the expression byte with its top bit removed; each assignment to
    # script_operand_address stores the address of the variable named
    let kind = code & 0x3F
    let number: UINT16 = fetch_byte()
    if code & 0x40 != 0:
        number = number * 256 + fetch_byte()
    let value: INT32 = kind
    if kind == 1:
        script_operand_address = local_strings[number]
    else if kind == 2:
        script_operand_address = local_numbers[number]
        value = local_numbers[number]
    else if kind == 5:
        script_operand_address = local_big_numbers[number]
        value = local_big_numbers[number]
    else if kind == 6:
        script_operand_address = global_strings[number]
    else if kind == 7:
        script_operand_address = global_numbers[number]
        value = global_numbers[number]
    else if kind == 9:
        if number >= 0x20 and number < 0x2F:
            let p = global_name_pointers[number - 0x20]
            script_operand_address = p
            if number == 0x29 or number == 0x2A:
                value = read_long(p)
            else:
                value = read_word(p)
        else:
            value = 9999
            fn_5702_00B1()
    else if kind == 0x0A:
        script_operand_address = global_big_numbers[number]
        value = global_big_numbers[number]
    else if kind == 0x0D:
        script_operand_address = global_flags[number / 8]
        value = 0
        if global_flags[number / 8] & (1 << (number % 8)) != 0:
            value = 1
    else if kind == 0x0E:
        script_operand_address = local_flags[number / 8]
        value = 0
        if local_flags[number / 8] & (1 << (number % 8)) != 0:
            value = 1
    return value

define write_variable(value: INT32):
    let code = fetch_byte() & 0x7F
    let wide = false
    if code & 0x40 != 0:
        wide = true
        code = code - 0x40
    if code >= 0x10:
        fn_172C_2914(value)
        return
    let number: UINT16 = fetch_byte()
    if wide:
        number = number * 256 + fetch_byte()
    if code == 2:
        local_numbers[number] = value
        script_locals_changed = 1
    else if code == 5:
        local_big_numbers[number] = value
        script_locals_changed = 1
    else if code == 7:
        global_numbers[number] = value
    else if code == 0x0A:
        global_big_numbers[number] = value
    else if code == 0x0D:
        if value != 0:
            global_flags[number / 8] = global_flags[number / 8] | (1 << (number % 8))
        else:
            global_flags[number / 8] = global_flags[number / 8] & ~(1 << (number % 8))
    else if code == 0x0E:
        if value != 0:
            local_flags[number / 8] = local_flags[number / 8] | (1 << (number % 8))
        else:
            local_flags[number / 8] = local_flags[number / 8] & ~(1 << (number % 8))
        script_locals_changed = 1

define read_string() -> FARPTR<UINT8>:
    let kind = peek_byte(0)
    if kind == 1:
        fetch_byte()
        # copies the NUL-terminated string the routine returns
        script_string = fn_172C_31B7()
    else if kind == 2:
        fetch_byte()
        let i = 0
        let done = false
        while not done and i < 299:
            let c = fn_172C_325F(fetch_byte())
            script_string[i] = c
            if c == 3:
                done = true
            else:
                i = i + 1
        script_string[i] = 0
    else if kind == 5:
        fetch_byte()
        let buffer: UINT32 = 0
        let shift = 1
        let i = 0
        let done = false
        while not done and i < 299:
            if shift > 0:
                buffer = ((buffer << 8) & 0xFF00) | fetch_byte()
            let c = (buffer >> shift) & 0x7F
            if c == 3:
                done = true
            else:
                if c < 0x20 or c > 0x7E:
                    c = 0x20
                script_string[i] = c
                i = i + 1
                shift = shift + 1
                if shift > 7:
                    shift = 0
        script_string[i] = 0
        if not done:
            g_4C0E_0002 = 0
            if peek_byte(0) == 0:
                g_4C0E_0002 = 1
    return script_string

define read_number() -> INT32:
    let values: INT32[8] = [0, 0, 0, 0, 0, 0, 0, 0]
    let pending: UINT8[8] = [0, 0, 0, 0, 0, 0, 0, 0]
    let level = 0
    script_operand_address = script_accumulator
    let more = true
    while more:
        let b = fetch_byte()
        let v: INT32 = 0
        let combine = true
        let go_on = false
        if b < 0x80:
            v = INT16(b * 256 + fetch_byte())
        else if b >= 0xD1 and b <= 0xDF:
            pending[level] = b
            combine = false
            go_on = true
        else if b == 0xE2:
            level = level + 1
            if level >= 8:
                fn_5702_00B1()
            go_on = true
        else if b == 0xE1:
            v = values[level]
            level = level - 1
            if level < 0:
                fn_5702_00B1()
        else if b == 0x80 or b == 0xC0:
            v = script_accumulator
        else if (b >= 0x81 and b <= 0x8A) or b == 0x8D or b == 0x8E:
            v = read_variable(b & 0x7F)
        else if (b >= 0xC1 and b <= 0xCA) or b == 0xCD or b == 0xCE:
            v = read_variable(b & 0x7F)
        else if b == 0x8B or b == 0xCB:
            let high = INT16(fetch_word())
            v = INT32(high) * 0x10000 + fetch_word()
        else if b == 0x8C or b == 0xCC:
            save_parameters()
            execute_instruction(fetch_opcode())
            restore_parameters()
            v = script_accumulator
        else if b == 0x8F or b == 0xCF:
            v = INT8(fetch_byte())
        else if b == 0x90:
            v = INT16(fetch_word())
        else if b == 0x91:
            v = INT16(-fetch_word())
        else if b == 0x92:
            script_operand_address = read_string()
        else if b == 0xB1:
            v = fn_172C_284D()
        else:
            fn_5702_00B1()
        if combine:
            values[level] = apply_operator(pending[level], values[level], v)
            pending[level] = 0
        if not go_on:
            let next = peek_byte(0)
            go_on = next >= 0xD1 and next <= 0xDF
            if level > 0 and next == 0xE1:
                go_on = true
        more = go_on
    return values[0]

define apply_operator(op: UINT8, a: INT32, v: INT32) -> INT32:
    if op == 0xD1:
        return a + v
    if op == 0xD2:
        return a - v
    if op == 0xD3:
        return a * v
    if op == 0xD4:
        return a / v
    if op == 0xD5:
        return a != 0 and v != 0
    if op == 0xD6:
        return a != 0 or v != 0
    if op == 0xD7:
        return a == v
    if op == 0xD8:
        return a != v
    if op == 0xD9:
        return a > v
    if op == 0xDA:
        return a < v
    if op == 0xDB:
        return a >= v
    if op == 0xDC:
        return a <= v
    if op == 0xDD:
        return a & v
    if op == 0xDE:
        return a | v
    if op == 0xDF:
        return a & ~v
    return v

define save_parameters():
    if script_parameter_stack_depth >= 2:
        fn_5702_00B1()
        return
    script_saved_parameters[script_parameter_stack_depth] = copy(script_parameters)
    script_saved_parameter_addresses[script_parameter_stack_depth] = copy(script_parameter_addresses)
    script_parameter_stack_depth = script_parameter_stack_depth + 1

define restore_parameters():
    if script_parameter_stack_depth > 0:
        script_parameter_stack_depth = script_parameter_stack_depth - 1
        script_parameters = copy(script_saved_parameters[script_parameter_stack_depth])
        script_parameter_addresses = copy(script_saved_parameter_addresses[script_parameter_stack_depth])
```

## Outputs

`read_number` returns the expression's value as an `INT32` and leaves in `script_operand_address`
the address of the last variable or string it read, or of the accumulator. `read_parameters`
fills `script_parameters` and `script_parameter_addresses`. `read_variable` returns a variable's
value. `read_string` returns the address of `script_string`. `write_variable` changes one
variable. `read_word`, `write_word` and `read_long` read and write memory as described.

## Edge cases

There is no precedence: `a + b * c` is `(a + b) * c`. An operator right after `0xE2` combines
with 0, the value an open parenthesis gives. Literals below `0x80` are 0 to 32,767; `0x91` gives
negative numbers, which the game uses for names. Division by 0 is not checked (the 32-bit `idiv`
raises the processor's divide error). A string whose first byte is none of 1, 2 and 5 reads
nothing and leaves `script_string` as it was. A packed string longer than 299 characters stops
there, and `g_4C0E_0002` then records whether the next byte is 0. Kinds 3, 4, 8, `0x0B` and `0x0C`
return their kind code as the value; kinds 1 and 6 return theirs and point the operand address at
the string. A variable number outside the arrays is not checked.

## What the sources say

SRC-LIBGFF-839B11D (`include/gff/var.h`) names the operators `0xD1` to `0xDF` and the kinds: 1
local string, 2 local number, 3 local byte, 4 local name, 5 local big number, 6 global string, 7
global number, 8 global byte, 9 global name, `0x0A` global big number, `0x0D` global flag and
`0x0E` local flag, and `0x8B`, `0x8C`, `0x8F`, `0x90`, `0x91` and `0x92` as immediate 32-bit,
return value, immediate byte, immediate word, immediate name and immediate string. Its limit of 8
parentheses agrees. SRC-OPENDS-5C6CBD7 uses the same names.

## Differences between builds

None known.

## Open questions

- What `fn_172C_284D` (`0xB1`), `fn_172C_31B7` (the string of kind 1), `fn_172C_325F` (the
  translation of kind 2 strings) and `fn_172C_2914` (assignments to kinds from `0x10`) do
  (Q-SCRIPT-004).
- How many variables of each kind there are, in `global_flags`, `local_flags`,
  `global_numbers`, `local_numbers`, `global_big_numbers`, `local_big_numbers`, `global_strings`
  and `local_strings`, and where `global_name_pointers` point (Q-SCRIPT-005).
- What `fn_5702_00B1` does after reporting an error, and what reads `g_4C0E_0002` (Q-SCRIPT-003).
