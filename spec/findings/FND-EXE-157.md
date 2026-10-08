---
id: FND-EXE-157
title: Guarded full equality prefix combines retained inputs with fresh byte and full reads
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1A14..0x004A1ABA
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1EC1..0x004A1ECE
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1FCC..0x004A1FD6
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A2274..0x004A2280
tool: Ghidra 12.1.3 PUBLIC bounded guarded-full equality prefix reading under Temurin 25.0.4.1
environment: null
---

## Observation

FND-EXE-131 grounds selector 9 at `0x004A1A14`. It reads
full V from `0x01BA29E8`, then full N from `0x01BA29E0`,
retaining them in ECX and ESI. It compares V with N unsigned
at full width. V below N takes `0x004A1EC1`, reads full mask
M from `0x0075B204` and sets mask one without reading the guard.

For V at or above N it reads full G from `0x01BA29F4`.
G zero reaches the local mask-one clear. G nonzero compares the
same retained inputs again, setting mask one through the alternate
path on equality and clearing it otherwise. All paths read M on
their chosen set/clear routes and join at `0x004A1A42`.
Mask one is therefore set exactly when unsigned V is below N,
or V equals N with full G nonzero. V greater than N clears it
even with G nonzero.

The join freshly zero-extends byte d from `0x01BA29E4`, then
byte n from N's storage and byte v from V's storage. It clears
mask `0x10`, contributes (n XOR d XOR v) AND `0x10`, and
saves v at current ESP plus `0x70`. Those byte reads do not
replace the retained full N/V. Retained full V zero sets mask
`0x40`; nonzero clears it through `0x004A1FCC`. Both paths
join at `0x004A1A76`, which freshly reads full D from
`0x01BA29E4`.

The next portion clears mask `0x80` and contributes retained
V's bit thirty-one shifted logically right twenty-four. It computes
N XOR D, XORs that full value with `0x80000000`, and computes
V XOR N separately. It tests their full-width AND for a set
sign bit. A set bit sets mask `0x800` through `0x004A2274`;
otherwise the local path clears it. Equivalently the condition is:

((V XOR N) AND (N XOR D XOR `0x80000000`) AND `0x80000000`) is nonzero.

Both paths join at `0x004A1AAB`, clear mask four, reload saved
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
No local call or ESP change intervenes. Initial full N/V remain
the inputs for comparison, zero/sign and high-mask work; the byte
contribution uses separate fresh d/n/v reads, and high-mask D comes
from the later fresh full read. The earlier byte d does not become
that full D. Saved v is not established equal to retained V's low
byte. The guard register is reused for d after its last guard
consumer; this does not change retained N/V. Separate read order
does not establish atomicity, native admission or storage lifetime.

## Interpretation

Selector 9 admits full-width equality conditionally through a full
guard, with strictly below admission bypassing the guard. It shares
the comparison shape and flipped high-mask XOR of FND-EXE-155
and FND-EXE-156, but consumes full retained N/V and later D,
and orders the fresh bytes d/n/v rather than n/d/v. These reads
must not be collapsed into one snapshot. This is a local branch
contract, not a complete helper reading, originating operation or
PATH outcome. Q-EXE-009 in FMT-EXE-006 remains open for producers,
native admission, lifetime, runtime lookup writers and remaining
branch/callee effects.

## Alternatives

- V zero and N `0x80000000` set mask one without G; reversing
  them clears it even with G nonzero. Signed comparison reverses admission.
- Equal full V/N with G zero clear mask one; equality with full
  G `0x00010000` sets it. Narrowing G to a word changes that case.
- V `0x00010000` clears mask `0x40` despite a zero low word.
  V `0x80000000` contributes mask `0x80`; V `0x00008000` does not.
- Later D zero, retained N zero and V `0x80000000` set
  mask `0x800`; changing only D to `0x80000000` clears it.
  D `0x80000000`, N `0x80000000`, V zero also set it;
  changing only D to zero clears it. Omitting the bit-thirty-one
  XOR reverses those high-mask outcomes.
- Fresh n 16, d zero and v zero contribute mask `0x10`;
  retained full N/V do not replace the fresh bytes.
- The lookup uses saved fresh v, not retained V's low byte or a
  later global V. This is local dataflow, not evidence that differing
  observations occur in an admitted native state.

## How to reproduce

Use FND-EXE-011's executable identity, FND-EXE-099's six physical
mapping controls and FND-EXE-131's dispatch mapping. Recheck selector
9's four-byte slot at shipped offset `0x003221A4`; it selects
`0x004A1A14`. Use the saved Ghidra program with -noanalysis and
ReportInstructionWindow.java at `0x004A1A14` limit 45,
`0x004A1EC1` limit four, `0x004A1FCC` limit four and
`0x004A2274` limit four. Exclude instructions at or beyond the
declared exclusive ends. Use FND-EXE-132 for the shared lookup
and publication return. Track full V-before-N reads and retention,
unsigned comparison direction, optional full guard and equality inputs,
each mask-one route, fresh d/n/v order and saved v, later fresh full
D, full sign-bit shift and AND with the extra bit-thirty-one XOR,
register reuse after final consumers, unchanged ESP and direct lookup
transfer. Check the Alternatives as local arithmetic controls, not
admitted native inputs. No native or emulated execution is part of
this finding.
