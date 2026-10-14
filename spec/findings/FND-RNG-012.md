---
id: FND-RNG-012
title: The inclusive-range reducer wraps its span to a signed word before signed 32-bit scaling
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3A03..2D40:3A43
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:07D9..1000:07F0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0822..1000:0848
tool: tools/research/exec-census/resident_listing.py and direct_callers.py; installed-build fingerprint and MZ relocation validation; Capstone 5.0.9 with operand-size prefixes checked
environment: null
---

## Observation

The complete range-helper body reads lower and upper as words and compares
them with a signed conditional branch. If lower is at least upper it
returns lower without a draw. Otherwise it calls the known generator once.
The generator and its bounded multiplication helper preserve the saved
lower/upper registers used after that call.

The helper computes upper minus lower plus one using word-width
subtraction and increment. It sign-extends that wrapped word to 32 bits,
sign-extends the nonnegative draw to 32 bits, forms a signed 32-bit product
and divides it by positive 32768 with signed 32-bit IDIV. This division
truncates toward zero. It adds only the quotient's low word to lower with
a word-width addition and returns that word.

Thus the span is INT16(UINT16(upper-lower+1)), not an unbounded positive
range length. The product fits signed 32 bits for every admitted draw and
span, so the scaling multiplication loses no significant product bits.
The final addition deliberately retains only 16 bits.

Synthetic numeric examples for draw 32767 illustrate the boundary:

| Lower | Upper | Wrapped signed span | Quotient | Signed view of result word |
|---|---|---|---|---|
| 0 | 32766 | 32767 | 32766 | 32766 |
| 0 | 32767 | -32768 | -32767 | -32767 |
| -32768 | 32767 | 0 | 0 | -32768 |

The last case still costs one draw because the signed lower/upper test
passes before the span becomes zero. These examples describe arithmetic,
not observed gameplay arguments or a native replay.

The earlier Ghidra import recorded no direct references. The current
relocation-aware query produces overlay candidates in descriptors 176 and
179; their instruction boundaries, argument admission and rule effects
remain a caller-reading obligation rather than a negative absence claim.

## Interpretation

This replaces FND-RNG-004. Its signed lower/upper gate and one-draw
behavior remain, but the simple inclusive-range description applies only
where the computed span is positive. Word-span wrap can produce a result
outside the requested interval, or the lower endpoint despite consuming
a draw. Q-RNG-002's operand-width and division questions are settled by
the reached arithmetic instructions, without asserting complete callers.

## Alternatives

- Which game rules use this helper and which endpoint pairs they admit
  remain Q-RNG-001. No gameplay defect is claimed solely from a synthetic
  wide-span example.
- No original function was executed for these examples. Supported named
  parameter bindings are still needed before the current no-argument
  resident harness can exercise this helper.
- Return-word interpretation by every caller is not established; the table
  explicitly reports a signed view, matching the entry's endpoint notation.

## How to reproduce

Read 2D40:3A03..2D40:3A43 with resident_listing.py on the approved
installed executable, together with the listed generator/helper ranges.
Check the unprefixed word span arithmetic, sign-extending moves, 32-bit
operand prefixes on the product and divide, and unprefixed final addition.
Query direct_callers.py target 2D40:3A03 to retain the bounded overlay leads
locally; do not treat byte candidates as a completed caller census.
