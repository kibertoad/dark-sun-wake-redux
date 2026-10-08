---
id: FND-EXE-160
title: Strict full prefix comparison separates retained inputs from fresh byte and full reads
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1BA6..0x004A1C40
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A21B2..0x004A21C9
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A225C..0x004A2268
tool: Ghidra 12.1.3 PUBLIC bounded strict-full prefix reading under Temurin 25.0.4.1
environment: null
---

## Observation

FND-EXE-131 grounds selector 3 at `0x004A1BA6`. It reads
full V from `0x01BA29E8`, then full N from `0x01BA29E0`,
retaining them in EBX and ESI. It compares V with N unsigned
at full width. V below N reads full mask M from `0x0075B204`
and sets mask one. V at or above N takes `0x004A21BC`,
reads M and clears mask one. Both join at `0x004A1BC2`.
Neither path reads the optional guard.

The join freshly zero-extends byte n from N's storage, then byte
d from `0x01BA29E4` and byte v from V's storage. It clears
mask `0x10`, contributes (n XOR d XOR v) AND `0x10`, and
saves v at current ESP plus `0xA0`. These fresh bytes do not
replace retained full N/V. Retained V zero sets mask `0x40`;
nonzero clears it through `0x004A21B2`. Both join at `0x004A1BF9`,
which freshly reads full D from `0x01BA29E4` into ECX.

The next portion clears mask `0x80` and contributes retained
V's bit thirty-one shifted logically right twenty-four. It computes
N XOR D in EDI, XORs that full value with `0x80000000`,
and computes V XOR N in EBX. It tests their full-width AND
for a set sign bit. A set bit sets mask `0x800` through
`0x004A225C`; otherwise the local path clears it. Equivalently
that mask is set exactly when:

((V XOR N) AND (N XOR D XOR `0x80000000`) AND `0x80000000`) is nonzero.

Both paths join at `0x004A1C2E`, clear mask four, reload saved
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
contribution uses separate fresh n/d/v reads, and high-mask D comes
from the later fresh full read. Earlier byte d does not become that
full D. Saved v is not established equal to retained V's low byte.
ECX's mask is copied to EAX on both zero-test paths before ECX
is reused for full D; full D is consumed before ECX becomes the
outgoing mask. EDI's byte-XOR contribution is consumed before EDI
is reused for N XOR D, and retained V's sign contribution is
formed before EBX becomes V XOR N. Read order does not establish
atomicity, native admission, producer meaning or storage lifetime.

## Interpretation

Selector 3 uses a strict full-width comparison with no optional
equality admission. It shares FND-EXE-157's retained-full and later
full-D shape but does not import selector 9's guard, and its fresh
bytes are read n/d/v rather than d/n/v. The distinct reads must
not be collapsed into one snapshot. This is a local branch contract,
not a complete helper reading, originating operation or PATH outcome.
Q-EXE-009 in FMT-EXE-006 remains open for producers, native admission,
lifetime, runtime lookup writers and remaining branch/callee effects.

## Alternatives

- V zero and N `0x80000000` set mask one; reversing them
  clears it. Signed comparison reverses these cases. Equal full inputs
  clear it regardless of a guard value, which this path never reads.
- V `0x00010000` clears mask `0x40` despite a zero low word.
  V `0x80000000` contributes mask `0x80`; V `0x00008000` does not.
- Later D zero, retained N zero and V `0x80000000` set
  mask `0x800`; changing only D to `0x80000000` clears it.
  D `0x80000000`, N `0x80000000`, V zero also set it;
  changing only D to zero clears it. Omitting the bit-thirty-one
  XOR reverses these high-mask outcomes.
- Fresh n 16, d zero and v zero contribute mask `0x10`;
  retained full N/V do not replace those fresh bytes.
- The lookup uses saved fresh v rather than retained V's low byte.
  Different observations are a local dataflow possibility, not evidence
  of an admitted native state.

## How to reproduce

Use FND-EXE-011's executable identity, FND-EXE-099's six physical
mapping controls and FND-EXE-131's dispatch mapping. Recheck selector
3's four-byte slot at shipped offset `0x0032218C`; it selects
`0x004A1BA6`. Use the saved Ghidra program with -noanalysis and
ReportInstructionWindow.java at `0x004A1BA6` limit 43,
`0x004A21B2` limit seven and `0x004A225C` limit four.
Exclude instructions at or beyond the declared exclusive ends.
Use FND-EXE-132 for the shared lookup/publication return. Track
full V-before-N order and retention, unsigned comparison direction,
both mask-one paths and absence of guard reads, fresh n/d/v order
and saved v, later fresh full D, full sign-bit shift and AND with
its extra bit-thirty-one XOR, register reuse after prior consumers,
unchanged ESP and direct lookup transfer. Check the Alternatives
as local arithmetic controls, not admitted native inputs. No native
or emulated execution is part of this finding.
