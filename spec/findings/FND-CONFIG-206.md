---
id: FND-CONFIG-206
title: The text-width helper sums unchecked per-character words with sixteen-bit wrapping
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:03B7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:599E
tool: Python 3.14.7 and Capstone 5.0.7 complete bounded resident 16-bit helper readings and declared MZ call relocation check
environment: null
---

## Observation

FND-CONFIG-202 passes its formatted far pointer to 2C5F:03B7
and uses returned AX in its coordinate and screen-edge calculations.
The complete local width body spans file 0x00021BA7..0x00021BE0.
It saves BP/SI/DI and tests the supplied far pointer for zero.
A null pointer returns AX zero without dereference or child calls.

For a nonnull pointer it initializes SI and DI to zero. It repeatedly
reads the byte at the supplied segment and word-wrapped base offset
plus SI. A zero byte ends the loop. Every other byte is sign-extended
from AL to AX in the 16-bit operand context and passed as a word
to 1BF3:599E. The returned AX is added to DI as a word, SI is
incremented as a word, and the byte test repeats. Completion returns
DI in AX and restores BP/SI/DI. The child call's segment operand
at file 0x00021BCA is a declared MZ relocation.

There is no own maximum input length, segment-carry advancement,
output-overflow rejection, newline special case or extra spacing
addition. A nonnull zero-byte-first input returns zero without the
lookup. Under valid nonaliasing terminated input and preserved loop
state, the result is the sum of the child words modulo 65536.
Actual input validity and finite termination are not established by
an initial nonnull pointer.

The complete 1BF3:599E body spans file 0x00016ACE..0x00016AF0.
It saves BP/DS/SI/DI, loads a far pointer from CS:1052 into ES:SI,
then loads DS:BX from the far field at that object's +6. It doubles
the supplied word character into SI, adds BX with word arithmetic,
reads a word at resulting DS:SI+0108 as a near offset, adds BX
again and returns the word at that resulting address in AX.
It restores BP/DS/SI/DI but does not restore ES. It has no callee,
port access, null-pointer, character-range, table-capacity or final
storage check. The object/table/returned-word interpretation remains
conditional on their producer contracts.

This local child preservation supports the outer helper's retained
SI/DI and DS on normal valid returns. It does not prove valid table
storage or absence of aliases with frame writes. Bytes at least 80
hexadecimal are sign-extended to high-bit word arguments before the
word-doubling lookup; they are not silently zero-extended indices.
The decimal bytes from FND-CONFIG-205 are below that boundary,
conditional on the selected format and output actually supplied.

## Interpretation

The width used by FND-CONFIG-202 has a concrete modular sum
contract with a conditional unchecked lookup. It does not by itself
prove rendered advance, safe glyph access, positive width or accepted
screen coordinates. FND-CONFIG-200's separate use of 599E with
0048 and a later plus-one assignment is not this helper's formula.
Native presentation and the current pointer/table state remain open.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain CS:1052 and object+6
producers, table/near-offset capacities, accepted text and high-byte
inputs, aliases, current format and native output. One reading supplies
finite decimal text and valid table words; another supplies changed
pointer/table state or wrapped sums. Complete producers and input
readings would separate those code-decided cases. No native or
emulated result is claimed.

The reading that this helper adds one spacing unit per character is
ruled out by the complete accumulator sequence. A reading that the
lookup validates every character is ruled out by its unchecked index
and pointer reads. Zero return alone does not distinguish empty text
from a wrapped or zero-valued sum.

## How to reproduce

Read 2C5F:03B7 through far return 03F0, checking null and
zero-byte exits, AL-to-AX extension, pointer/index wrapping, child
argument, DI accumulation and saved registers. Verify the far-call
relocation. Read 1BF3:599E through 59C0, following both far
pointer loads, doubled-word table index, near-offset addition and
returned word. Compare the coordinate consumer in FND-CONFIG-202
and selected decimal output in FND-CONFIG-205 while retaining
pointer/table, alias, termination and native-presentation conditions.
