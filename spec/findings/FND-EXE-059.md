---
id: FND-EXE-059
title: Matching byte reader accumulates seven-bit groups with masked shifts and writes one word only at termination
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F4D30..0x005F4D66
tool: Ghidra 12.1.3 PUBLIC, bounded instruction and function reporters
environment: null
---

## Observation

FND-EXE-058 prepares a chained input pointer in the return register and a
local output address in an auxiliary register before each call to this
helper. The helper consumes those incoming register values directly: it
saves the output address before changing that register's low byte and saves
the input pointer in a local word. It does not obtain these inputs from
original stack-argument slots. It initializes a full-word accumulator and
shift-position word to zero.

Every iteration reads exactly one byte at the current input pointer and
advances the saved pointer by one at 32-bit width. It masks that byte to
its low seven bits, shifts the payload left at 32-bit width using the
current shift position, and combines it into the accumulator by bitwise OR.
The shift-position word advances by seven per iteration. The instruction
uses the low-byte shift-count register, and the 32-bit x86 shift masks the
count to five bits. Thus group k, starting at zero, contributes its payload
shifted by `(7 * k) modulo 32`, with bits shifted beyond the word discarded.
The combination is OR, not addition.

The loop tests the original byte's sign bit. Bit seven set repeats the
loop; bit seven clear ends it, after that terminating byte's payload has
already been combined. It performs at least one byte read. There is no
local zero-pointer, source-length, byte-count, overflow, canonical-encoding
or maximum-shift guard. Long input is not rejected when groups exceed the
word width; masked shifts can revisit earlier bit positions. Continued bytes
without an accessible terminating byte do not have a proven safe termination.

Only after a terminating byte does it write the full accumulator word
through the originally saved output address. It returns the advanced input
pointer in the return register and restores its saved registers and frame.
There is no intermediate output store and no extra helper call in this body.
The output may alias input storage: the direct output store occurs after
all input reads for this invocation, but that does not prove later chained
calls see unchanged input.

As arithmetic controls, a single terminating zero byte produces zero and
advances by one; a continuation-only zero payload followed by a terminating
zero also produces zero and advances by two, so this body does not enforce
a shortest encoding. Five continuation bytes with zero payload followed by
a terminating payload of one produce eight: the sixth group's shift count
is thirty-five masked to three. These controls describe the instruction
arithmetic, not a claim that such streams occur in the shipped resources.

For FND-EXE-058's positive-counter loop, normal termination of each helper
call establishes a full-word last writer for the two prepared output locals.
Its first returned pointer becomes the second input and its second returned
pointer becomes the next iteration's input. Under valid storage and the
helper's direct behavior, each pair reads two consecutive variable-length
values and replaces both output words; after the last pair those locals
hold the last two values, not an aggregate of every pair. The caller's
counter bound does not independently bound bytes consumed per value.

## Interpretation

The repeated helper now has a direct register-input, byte-consumption,
full-word output and pointer-return contract. It narrows FND-EXE-058's local
last-writer gaps on normally terminating reads, without establishing where
the input stream begins, how much storage it has, or what decoded values
mean. Q-EXE-009 retains input provenance and bounds, stream structure,
other matching helpers, output aliases, remaining classification and
exceptional behavior. No validated resource schema or runtime parser is
implemented from this reading.

## Alternatives

Reading stack inputs, sign-extending each payload, rejecting after five
bytes, using an unmasked ever-growing shift, adding contributions, or
writing output during every iteration are ruled out by the direct body.
A positive caller loop count is not a source-byte bound. Treating equal
zero results as evidence of equal consumed lengths ignores continuation
bytes. Calling the reader a strict canonical format decoder is unsupported
by its absent local rejection paths.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x005F4D30`
and read thirty-five instructions there, restricting claims to the cited
body and excluding the later helper. Use FND-EXE-058 for incoming register
writers and chained pointer consumption. Track the saved output address,
low-byte overwrite, full accumulator and shift words, payload mask, shift
count width, OR combination, terminating byte, sole output store and pointer
return. Check single-byte, continued zero, boundary-width and long continued
arithmetic cases without asserting shipped-input admission. Keep source
bounds and alias contracts conditional. Keep rich reports local and execute
no interpreter or game.
