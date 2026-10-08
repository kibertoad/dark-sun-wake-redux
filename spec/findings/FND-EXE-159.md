---
id: FND-EXE-159
title: Strict word prefix comparison retains initial words alongside fresh byte and word inputs
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A152C..0x004A15D4
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A2047..0x004A205E
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A2018..0x004A2024
tool: Ghidra 12.1.3 PUBLIC bounded strict-word prefix reading under Temurin 25.0.4.1
environment: null
---

## Observation

FND-EXE-131 grounds selector 2 at `0x004A152C`. It
zero-extends word V from `0x01BA29E8`, then word N from
`0x01BA29E0`, retaining them in ESI and EDI. It compares V
with N unsigned at word width. V below N reads full mask M
from `0x0075B204` and sets mask one. V at or above N takes
`0x004A2051`, reads M and clears mask one. Both paths join
at `0x004A154B`. Neither reads the optional guard.

The join freshly zero-extends byte n from N's storage, then byte
d from `0x01BA29E4` and byte v from V's storage. It clears
mask `0x10`, contributes (n XOR d XOR v) AND `0x10`, and
saves v at current ESP plus `0xB0`. Those fresh bytes do not
replace retained word N/V. Retained V zero sets mask `0x40`;
nonzero clears it through `0x004A2047`. Both join at `0x004A1583`.

The next portion freshly zero-extends word D from `0x01BA29E4`,
then zero-extends retained DI as N and SI as V. It clears mask
`0x80` and contributes V's bit fifteen shifted right eight.
That shift is arithmetic at full width, but its operand has been
masked with `0x8000`, so the contribution is zero or `0x80`
without upper sign filling.

It computes V XOR N and D XOR N, XORs the latter full
zero-extended value with `0x8000`, ANDs the expressions,
logically shifts right fifteen and tests AL. All operands remain
bounded to a word, so the shifted result is zero or one. Nonzero
sets mask `0x800`; zero clears it through `0x004A2018`.
Equivalently that mask is set exactly when:

((V XOR N) AND (D XOR N XOR `0x8000`) AND `0x8000`) is nonzero.

Both paths join at `0x004A15C2`, clear mask four, reload saved
v as a zero-extended byte and transfer directly to `0x004A0E64`,
bypassing the fresh byte load at `0x004A0E5D`.
FND-EXE-132 grounds the common zero-extended word contribution
at `0x006F2B70` plus twice that index, full mask publication,
full selector clear, frame/register restoration and full published-mask
return. FND-EXE-133 records shipped lookup values but leaves runtime
writers unresolved. Other M bits remain locally retained apart from
the stated replacements.

FND-EXE-131 grounds the enclosing 220-byte reservation. Saved v
has one byte writer and no later overwrite before its byte consumer.
No local call or ESP change intervenes. Initial N/V words remain
the inputs for comparison, zero/sign and high-mask work; the byte
contribution uses separate fresh n/d/v reads, and high-mask D comes
from the later fresh word read. The earlier byte d does not become
that word D. Saved v is not established equal to retained V's low
byte. Each register reuse occurs after the prior value's required
consumer: DI supplies retained N before ECX is later replaced by
the outgoing mask, and SI supplies retained V before ESI becomes
its sign contribution. Separate read order does not establish
atomicity, producer meaning, native admission or storage lifetime.

## Interpretation

Selector 2 uses a strict word comparison with no optional equality
admission. It shares FND-EXE-156's retained-word, fresh-byte and
later-word-D shape but does not import selector 8's guard. The
distinct reads must not be collapsed into one snapshot. This is a
local branch contract, not a complete helper reading, originating
operation or PATH outcome. Q-EXE-009 in FMT-EXE-006 remains open
for producers, native admission, lifetime, runtime lookup writers
and remaining branch/callee effects.

## Alternatives

- V zero and N `0x8000` set mask one; reversing them clears
  it. Signed comparison reverses these cases. Equal words clear it
  regardless of a guard value, which this branch never reads.
- V `0x0100` clears mask `0x40` despite a zero low byte.
  V `0x8000` contributes mask `0x80`; V `0x0080` does not.
- Later word D zero, retained N zero and V `0x8000` set
  mask `0x800`; changing only D to `0x8000` clears it.
  D `0x8000`, N `0x8000`, V zero also set it;
  changing only D to zero clears it. Omitting the bit-fifteen XOR
  reverses these high-mask outcomes.
- Fresh n 16, d zero and v zero contribute mask `0x10`;
  retained N/V words do not replace those fresh bytes.
- Upper storage words do not participate in the word tests or
  high-mask expression. The lookup uses saved fresh v rather than
  retained V's low byte. Different observations are a local dataflow
  possibility, not evidence of an admitted native state.

## How to reproduce

Use FND-EXE-011's executable identity, FND-EXE-099's six physical
mapping controls and FND-EXE-131's dispatch mapping. Recheck selector
2's four-byte slot at shipped offset `0x00322188`; it selects
`0x004A152C`. Use the saved Ghidra program with -noanalysis and
ReportInstructionWindow.java at `0x004A152C` limit 44,
`0x004A2047` limit seven and `0x004A2018` limit four.
Exclude instructions at or beyond the declared exclusive ends.
Use FND-EXE-132 for the shared lookup/publication return. Track
V-before-N word order and retention, unsigned comparison direction,
both mask-one paths, absence of guard reads, fresh n/d/v order and
saved v, later fresh word D, bounded masked arithmetic shift and
logically shifted AND with its extra bit-fifteen XOR, register reuse
after prior consumers, unchanged ESP and direct lookup transfer.
Check the Alternatives as local arithmetic controls, not admitted
native inputs. No native or emulated execution is part of this finding.
