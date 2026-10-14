---
id: FND-RNG-013
title: Repeated rolls use signed 32-bit scaling and a wrapping word accumulator
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:391D..28C9:3963
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

The complete repeated-roll body tests its count with signed word
comparisons. Counts below one return zero without a draw. For a positive
count it initializes a word index and a word total, then makes one known
generator call per iteration until the signed index reaches count. The
generator's arithmetic helper preserves the index and total registers.

Each nonnegative draw and the signed word sides argument are sign-extended
to 32 bits. Their signed 32-bit product fits that width. Signed 32-bit
division by positive 32768 truncates the result toward zero. The helper
then increments the quotient's low word and adds that word to the total
using word-width arithmetic. It returns the total word in AX. There is
no wider accumulator and no saturation or sides-admission check.

For sides 1..32767 each term is 1..sides. Sides zero still consumes a
draw for each positive-count iteration and contributes one. Negative
sides use signed scaling rather than an ordinary positive die range:
with synthetic numeric draw 32767 and sides -32768 the quotient is
-32767 and the incremented term has signed view -32766. Accumulation
retains low 16 bits; for any terms summing to 65536 the returned word is
zero, regardless of their wider mathematical sum. This is an arithmetic
example, not a claim that a particular RNG sequence occurred.

For counts 1..32767 the loop index reaches the stopping count before
word overflow, so the body makes exactly that many generator calls.
The caller decides whether to interpret the returned word as signed or
unsigned; the local addition alone establishes its width and bits.

The old Ghidra query found no direct references. The relocation-aware
query now yields overlay candidates, including descriptor 179 and
other overlays. Their verified instruction boundaries,
argument admission and draw/rule mapping remain Q-RNG-001. Candidates
alone are not evidence of each call's gameplay role.

## Interpretation

This replaces FND-RNG-005. The positive-count draw count remains valid,
while product/divide signedness, zero/negative sides and word-total wrap
are now explicit. The mathematical sum of positive dice is not necessarily
the returned value when it exceeds the word width.

## Alternatives

- This settles the arithmetic widths and branches of Q-RNG-002, not all
  caller types or gameplay argument constraints.
- No native or emulated helper fixture was executed. The current resident
  harness lacks the required named argument bindings for this root.
- Full caller/field readings and replay fixtures remain prerequisites for
  any complete-reading or validated parity claim.

## How to reproduce

Read 28C9:391D..28C9:3963 and the listed generator/multiplication helper
ranges with resident_listing.py on the approved installed DSUN.EXE.
Check signed word count branches, the sign-extending moves and explicit
32-bit product/division prefixes, then the unprefixed word increment,
accumulation and return. Query direct_callers.py target 28C9:391D for
bounded overlay leads, keeping source-derived output only in GAME_DIR.
