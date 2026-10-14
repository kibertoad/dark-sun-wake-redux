---
id: FND-RNG-011
title: The modulo reducer uses a signed word divisor and signed remainder, with a zero-divisor no-draw path
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2834:061C..2834:063F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0822..1000:0848
tool: tools/research/exec-census/resident_listing.py and direct_callers.py; installed-build fingerprint and MZ relocation validation; Capstone 5.0.9 in 16-bit mode with operand-size prefixes checked
environment: null
---

## Observation

The complete bounded body of the modulo helper reads its argument as a
word. A zero word returns zero without calling the generator. Every other
word calls the known generator once, sign-extends its returned AX into
the DX:AX dividend with the unprefixed 16-bit instruction, and divides by
the argument using signed 16-bit IDIV. It copies the remainder word to
a local and then to AX before restoring its frame and returning far.

The generator masks its result to 0..32767 (FND-RNG-001). Consequently
the dividend is nonnegative and the remainder is nonnegative even when
the divisor word represents a negative number. For a nonzero argument
word n the result is d modulo abs(INT16(n)), where d is that one draw.
The quotient stays within the signed word range, including divisor -1;
the generator's admitted result cannot trigger quotient overflow here.

Examples of the arithmetic for synthetic numeric draw d=32767:

| Argument word | Signed divisor | Remainder |
|---|---|---|
| 10 | 10 | 7 |
| 32768 | -32768 | 32767 |
| 32769 | -32767 | 0 |
| 65535 | -1 | 0 |

These are numeric boundary examples, not native recordings or claims about
which caller arguments occur. Zero remains a separate no-draw case.

The relocation-aware direct-call candidate query recovers eleven
push-CS/near-call sites in the resident segment: two within the table
selector and nine within chance_in_ten, consistent with FND-RNG-007 and
FND-RNG-006. This retains the earlier query's bounded caller observation;
it is not a census of indirect calls or aliased transfers.

## Interpretation

This replaces FND-RNG-003. Its draw count and zero behavior remain valid,
but the previously unrecorded division signedness is now explicit. A word
above 32767 is not an unsigned upper bound. Positive divisors preserve
the ordinary biased remainder behavior; other words use their signed
word interpretation and absolute magnitude.

## Alternatives

- Argument admission, rule/draw ordering and indirect state access remain
  Q-RNG-001; this reading does not establish that a negative divisor occurs
  in actual gameplay.
- The function body and arithmetic widths are read; all callers and complete
  rule inputs are not. No complete-reading or validated replay claim is made.
- Historical FND-RNG-001 and FND-SOUND-012 citations retain their original
  evidence records. Active rule and probe citations move to this replacement.

## How to reproduce

Run resident_listing.py against approved installed DSUN.EXE for
2834:061C..2834:063F and the listed generator range. Interpret instruction
widths from the 16-bit code mode and actual operand-size prefixes: the
dividend extension and divisor operation have no 32-bit override. Query
direct_callers.py target 2834:061C and compare the returned candidates
with the established table/chance routines. Retain any listings locally
under GAME_DIR/analysis/, never Git.
