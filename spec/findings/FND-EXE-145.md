---
id: FND-EXE-145
title: Full-width prefix branch uses a low-nibble equality gate and an exact-result high-mask gate while retaining mask one
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1964..0x004A19AB
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A21C9..0x004A21D6
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A212C..0x004A2134
tool: Ghidra 12.1.3 PUBLIC bounded selected-prefix branch reading under Temurin 25.0.4.1
environment: null
---

## Observation

FND-EXE-131 grounds selector 30 at `0x004A1964`. The branch reads
full V at `0x01BA29E8`, retains it and masks a copy with fifteen.
It compares that low nibble with fifteen, not with zero. An unequal nibble
reads current full mask M at `0x0075B204` and clears mask `0x10`.
An equal nibble takes `0x004A21C9`, reads M and sets that mask.
Both paths join at `0x004A1980` without changing retained V.

The join tests full V for zero. Zero sets mask `0x40`; nonzero
clears it through `0x004A212C`. Both join at `0x004A198B`.
The branch copies retained V, masks it with `0x80000000` and
logically shifts that full value right twenty-four. It clears mask `0x80`
from the retained mask and ORs the contribution, which is zero or `0x80`.
It then compares full retained V with `0x7FFFFFFF` at `0x004A19A0`
and jumps directly to `0x004A0F4D`, preserving the equality flags.

FND-EXE-134 grounds that shared tail: equality sets mask `0x800`,
inequality clears it, and both clear mask four before freshly reading
V's low byte for the common word lookup. FND-EXE-132 grounds the
zero-extended lookup contribution, full mask publication, subsequent full
selector clear, saved-register restoration and full published-mask return.
FND-EXE-133 records every shipped lookup contribution; runtime table
writers remain unresolved.

The locally replaced masks before that lookup are therefore:

| Mask | Set when | Cleared when |
| --- | --- | --- |
| `0x00000010` | V's low nibble equals fifteen | that nibble differs from fifteen |
| `0x00000040` | full V is zero | full V is nonzero |
| `0x00000080` | retained V's bit thirty-one is set | that bit is clear |
| `0x00000800` | full retained V equals `0x7FFFFFFF` | that value differs |
| `0x00000004` | not set locally before the lookup | unconditionally cleared before the lookup |

The path neither reads N's storage at `0x01BA29E0` nor the count
storage at `0x01BA29E4`, and does not locally replace mask one.
Other M bits remain retained locally apart from the stated replacements.
The four/zero shipped lookup values cannot set mask one; this does not
prove runtime immutability. No calls or local stack changes occur in these
bounded branch paths before the shared restoration. Retained V is used
for every local comparison, while the lookup consumes a fresh byte read;
these separate reads establish no atomicity or producer/lifetime contract.

## Interpretation

Selector 30 replaces mask `0x10` from low-nibble equality with fifteen
and mask `0x800` from one exact retained full V value. Unlike the exact-N
branches in FND-EXE-134 and FND-EXE-135, it does not read or test N or
replace mask one. These are local branch contracts, not evidence of an
originating arithmetic operation or named processor flags. Q-EXE-009 in
FMT-EXE-006 remains open for producers, runtime writers, other prefix
branches and remaining callee effects. No complete helper reading or actual
PATH behavior is established.

## Alternatives

- V zero and one both clear mask `0x10`; V fifteen and 31 set it.
  A generic nonzero-nibble test would disagree for V one.
- V `0x00010000` has a zero low word and byte but does not set mask
  `0x40`, because this branch tests full V.
- V `0x00000080` does not contribute mask `0x80`; V `0x80000000`
  does. The high-bit contribution consumes full V, not its low byte.
- Only V `0x7FFFFFFF` sets mask `0x800`. Values `0x7FFFFFFE`,
  `0x00007FFF` and `0xFFFFFFFF` do not. Neither a word-sized equality
  nor a sign test can replace this exact full-value comparison.
- For unchanged shipped lookup contents, an incoming mask one remains
  set or clear regardless of V. Runtime writers and fresh lookup-byte
  admission remain separate questions.

## How to reproduce

Verify FND-EXE-011's executable length and XXH3-128 identity,
FND-EXE-099's six physical mapping controls and FND-EXE-131's selector
slot. Selector 30's four-byte slot starts at shipped offset `0x003221F8`
and contains target `0x004A1964`. Use the saved Ghidra program with
-noanalysis and ReportInstructionWindow.java at `0x004A1964` limit
24, `0x004A21C9` limit three and `0x004A212C` limit two.
Exclude instructions after the direct transfer at `0x004A19A6`.
Recheck the shared equality gate at `0x004A0F4D` limit five and
its set at `0x004A0FAE` limit two, using FND-EXE-134 for their full
continuations and FND-EXE-132 for publication/return. Track retained V,
the nibble equality, independent full-zero test, logical high-bit shift,
flags preserved through the direct jump, fresh lookup byte and locally
retained mask one. Check every Alternatives example with incoming mask
one set and clear as local arithmetic controls, not native inputs.
No native or emulated execution is part of this finding.
