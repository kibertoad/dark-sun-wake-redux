---
id: FND-SOUND-036
title: Setup numeric overflow still assigns a word and bracketed-value failure retains partial copying
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0C22..1000:0DEF
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0854..1000:08BE
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:0AC1..1AF6:0BB3
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:0C8F..1AF6:0CDE
tool: tools/research/exec-census/resident_listing.py at 6dddfe40; Python 3.12.10, Capstone 5.0.7
environment: null
---

## Observation

This removes the local numeric-overflow and bracketed-value paths from
Q-CONFIG-006's remaining helper reading. FND-SOUND-025/034/035 establish
the actual numeric wrappers, formats, callbacks, word assignment and
representative non-assignment cases. Here numeric tokens are short ASCII
strings fitting the twenty-byte local array with NUL, with valid retained
classification/format/input/output/stack storage. Neither oversized
tokens nor high-byte classification inputs are admitted by this reading.

### Numeric magnitude and overflow

For the decimal and hexadecimal bases selected by the actual wrapper,
1000:0C4D accumulates the magnitude in an unsigned two-word value.
While its high word is zero it uses the low-word multiplication path;
once the high word becomes nonzero, subsequent digits include the high
word in the product. The latter path tests the carry above the combined
32-bit result. There is no signed-31-bit limit test on ordinary magnitude
accumulation. For bases ten and sixteen, the tested upper product and
carry cannot wrap their byte check back to zero.

A magnitude at most 4294967295 can therefore complete normally, even
above the signed-positive limit. Normal completion applies the recorded
minus by two-word negation and returns numeric status one. The plain
percent-d/percent-x scanner path stores only the returned low word.

When a digit makes the magnitude exceed 4294967295, the overflow path
instead constructs double-word 7FFFFFFF for a positive sign or 80000000
for a minus sign and sets numeric status two. It skips ordinary final
sign negation. Both statuses one and two pass the scanner's positive
status test, increment its assignment count and assign a word. Thus
overflow is not this wrapper's no-assignment path: the positive case
assigns FFFF and the negative case assigns 0000. The overflow branch
returns immediately rather than consuming the rest of the numeric run;
the token has already been copied by the outer extractor.

The following decimal tokens are synthetic arithmetic reading controls,
not shipped source values or executed cases. All fit the local array:

| Token | Numeric status | Word assigned, hexadecimal |
| --- | ---: | --- |
| 2147483648 | 1 | 0000 |
| 4294967295 | 1 | FFFF |
| -4294967295 | 1 | 0001 |
| 4294967296 | 2 | FFFF |
| -4294967296 | 2 | 0000 |

The same unsigned-magnitude boundary applies to an admitted hexadecimal
token. Which spelling reaches the scanner still follows the earlier
digit/minus/x extractor, not the scanner's larger alphabet. The outer
numeric helper ignores assignment count and numeric status, writes the
returned word and reports one, as FND-SOUND-035 records.

### Bracketed string value

1AF6:0AC1 takes no stack arguments. It reads the retained line at
DS:EA7E and signed length at DS:1886 and writes the value at DS:EA06.
Under a nonnegative length below 7FFF, a NUL at that length, disjoint
valid spans and nonwrapping indices, it behaves as follows:

1. Search from line index zero for the first closing bracket. If the
   source-index bound is exceeded first, return zero without value writes.
2. From that closing bracket, search for the next opening bracket. If
   the bound is exceeded, return zero without value writes.
3. Starting just after the opening bracket, copy bytes until the first
   closing bracket. On finding it, append NUL and return one. Empty
   brackets therefore produce an empty terminated string successfully.
4. After each copied byte increment the source and destination indices
   and test only the source index against the signed line length. If
   the source bound is exceeded, return zero without undoing copied
   bytes. With the assumed source NUL at its length and no closing
   bracket, that NUL itself is copied before the failure exit.

The helper checks neither a destination capacity nor bracket nesting or
escaping. It preserves spaces and tabs inside the chosen value and does
not search for another closing bracket once its first one ends the value.
Its explicit success terminator belongs only to the successful path;
failure before copying leaves any prior value buffer intact. SI, DI and
BP are restored; there are no child calls, interrupts or port accesses.
Lengths/indices at signed wrap, invalid source termination and output
spans beyond their valid storage are outside the stated bounded reading.

FND-SOUND-020/028 establish the record and header callers that test this
return before copying a successful value onward. Their failure test does
not restore the shared value buffer or earlier record/header stores.

## Interpretation

Unsigned-magnitude overflow with an assigned low word is now distinct
from FND-SOUND-035's failed scans with an unassigned wrapper local.
Bracketed-value failure is likewise not a guarantee of unchanged shared
scratch storage. These local branch and width obligations are removed
from Q-CONFIG-006. The installed source's smaller spans and magnitudes
remain the separate measurement in FND-SOUND-034.

## Alternatives

Saturating every number at 32767 or rejecting every overflow would add
guards absent from these paths. Treating the value helper as bounded by
its eighty-byte gap would add a destination comparison it does not make.
No outcome outside the declared spans, runtime stack default, native
heap/DOS result or driver effect is inferred. Remaining classification,
context, alias/lifetime and outer-child obligations remain Q-CONFIG-006
and Q-CONFIG-011. No original was executed and no complete reading or
parity status is promoted.

## How to reproduce

Use approved installed SOUND_DS.EXE length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57. Run resident_listing.py with -I and
the four location ranges above in order. Retain UTF-8 reports as
setup-overflow-value-spans.txt and setup-overflow-value-spans-repeat.txt
under GAME_DIR/analysis/disc-adv-survey-20261104/. Both have SHA-256
2605cd5d32163ce3fddaf14e9b4ec37e9210a7f9cc4af8c916bd7969d4138a18.
Reuse FND-SOUND-025's format/classification controls and caller bindings.
Track the high-word product and carry at the transition to the wide
accumulator, the overflow branch's bypass of final sign negation and
the scanner's assignment width. For the value helper, compare each
source-bound test with the destination writes before it. Source-derived
listings remain local and no malformed game file is created.
