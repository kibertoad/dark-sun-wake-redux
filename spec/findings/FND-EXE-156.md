---
id: FND-EXE-156
title: Guarded word equality prefix retains initial words alongside fresh byte and word inputs
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1ABA..0x004A1B75
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1E75..0x004A1E82
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1F88..0x004A1F9E
tool: Ghidra 12.1.3 PUBLIC bounded guarded-word equality prefix reading under Temurin 25.0.4.1
environment: null
---

## Observation

FND-EXE-131 grounds selector 8 at `0x004A1ABA`. It
zero-extends word V from `0x01BA29E8`, then word N from
`0x01BA29E0`, retaining them in ESI and EDI. It compares V
with N unsigned at word width. V below N takes `0x004A1E75`,
reads full mask M from `0x0075B204` and sets mask one without
reading the optional guard.

For V at or above N it reads full G from `0x01BA29F4`.
G zero reaches the local mask-one clear. G nonzero compares the
same retained words again, setting mask one through the alternate
path on equality and clearing it otherwise. All paths read M on
their selected set/clear route and join at `0x004A1AEC`.
Thus mask one is set exactly when unsigned V is below N, or
V equals N with full G nonzero. V greater than N clears it
even when G is nonzero.

The join freshly zero-extends byte n from N's storage, then byte
d from `0x01BA29E4` and byte v from V's storage. It clears
mask `0x10`, contributes (n XOR d XOR v) AND `0x10`, and
saves v at current ESP plus `0x80`. The fresh byte reads do
not replace retained word N/V. Retained V zero sets mask `0x40`;
nonzero clears it through `0x004A1F94`. Both join at `0x004A1B24`.

The next portion freshly zero-extends word D from `0x01BA29E4`,
then zero-extends retained SI as V and DI as N. It clears mask
`0x80` and contributes V's bit fifteen shifted right eight.
The shift is arithmetic at full width, but its operand has been
masked with `0x8000`, so the contribution is zero or `0x80`
without upper sign filling.

It computes V XOR N and D XOR N, XORs the latter full
zero-extended value with `0x8000`, ANDs the two expressions,
logically shifts right fifteen and tests AL. The operands remain
bounded to a word, so the shifted result is zero or one. Nonzero
sets mask `0x800`; zero clears it through `0x004A1F88`.
Equivalently that mask is set exactly when:

((V XOR N) AND (D XOR N XOR `0x8000`) AND `0x8000`) is nonzero.

Both paths join at `0x004A1B63`, clear mask four, reload saved
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
No local call or ESP change intervenes. The initial N/V word reads
remain the producers for comparison, zero/sign and high-mask work;
the byte contribution uses its separate fresh n/d/v reads, and the
high-mask D comes from the later fresh word read. The byte d
does not become that word D, and saved v is not established equal
to retained V's low byte. No atomicity, producer meaning, native
admission or global storage lifetime follows from this read order.

## Interpretation

Selector 8 admits word equality conditionally through a full guard,
with strictly below admission bypassing the guard. It shares the
comparison shape and flipped high-mask XOR of FND-EXE-155's
selector 7, but differs in widths and fresh reads: initial word N/V,
fresh byte n/d/v, then fresh word D. These must not be collapsed
into one snapshot. This is a local branch contract, not a complete
helper reading, originating operation or PATH outcome. Q-EXE-009
in FMT-EXE-006 remains open for producers, native admission,
lifetime, runtime lookup writers and remaining branch/callee effects.

## Alternatives

- V zero and N `0x8000` set mask one without G; reversing
  them clears it even with G nonzero. A signed comparison reverses it.
- Equal word V/N with G zero clear mask one; equality with full
  G `0x00010000` sets it. Narrowing G to a word changes that case.
- V `0x0100` clears mask `0x40` despite a zero low byte.
  V `0x8000` contributes mask `0x80`; V `0x0080` does not.
- Fresh word D zero, retained N zero and V `0x8000` set
  mask `0x800`; changing only D to `0x8000` clears it.
  D `0x8000`, N `0x8000`, V zero also set it;
  changing only D to zero clears it. Omitting the bit-fifteen XOR
  reverses these high-mask outcomes.
- Fresh n 16, d zero and v zero contribute mask `0x10`;
  the initially retained N/V words do not replace those fresh bytes.
- The lookup uses saved fresh v, not retained V's low byte or a
  later global V. This is a local dataflow distinction, not evidence
  that differing observations occur in an admitted native state.

## How to reproduce

Use FND-EXE-011's executable identity, FND-EXE-099's six physical
mapping controls and FND-EXE-131's dispatch mapping. Recheck selector
8's four-byte slot at shipped offset `0x003221A0`; it selects
`0x004A1ABA`. Use the saved Ghidra program with -noanalysis and
ReportInstructionWindow.java at `0x004A1ABA` limit 45,
`0x004A1B68` limit three, `0x004A1E75` limit four and
`0x004A1F88` limit seven. Exclude instructions at or beyond the
declared exclusive ends. Use FND-EXE-132 for the shared lookup
and publication return. Track V-before-N word reads and retention,
unsigned comparison direction, optional full guard and equality inputs,
each mask-one route, fresh byte ordering and saved v, later fresh word
D, bounded masked arithmetic shift and logically shifted AND with its
extra bit-fifteen XOR, unchanged ESP and direct lookup transfer.
Check the Alternatives as local arithmetic controls, not admitted native
inputs. No native or emulated execution is part of this finding.
