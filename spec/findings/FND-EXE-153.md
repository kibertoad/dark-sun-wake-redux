---
id: FND-EXE-153
title: Shared full prefix comparison separates fresh byte inputs from later full result tests
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1C40..0x004A1CCD
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A20D8..0x004A20EF
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A2250..0x004A225C
tool: Ghidra 12.1.3 PUBLIC bounded shared-full prefix reading under Temurin 25.0.4.1
environment: null
---

## Observation

FND-EXE-131 grounds selectors 18 and 24 at the shared target
`0x004A1C40`. The branch reads full N from `0x01BA29E0`,
then full D from `0x01BA29E4`, retaining them in ESI and EDI.
It compares N with D unsigned at full width. N below D reads
full mask M from `0x0075B204` and sets mask one. N at or above
D takes `0x004A20E2`, reads M and clears mask one. Both paths
join at `0x004A1C5C`. Neither reads the optional guard.

The join freshly zero-extends byte n from N's storage, then byte d
from D's storage and byte v from `0x01BA29E8`. It clears mask
`0x10`, computes (n XOR d XOR v) AND `0x10` and contributes
that result. It saves v at current ESP plus `0x10` before freshly
reading full W from V's storage at `0x004A1C83`. These byte
reads do not replace the retained full N and D.

W zero sets mask `0x40`; W nonzero clears it through
`0x004A20D8`. Both paths join at `0x004A1C96`, clear mask
`0x80` and contribute W's bit 31 shifted right 24 into bit seven.
The branch then computes D XOR N in EDI and N XOR W in ESI.
It tests their full-width AND for a set sign bit, setting mask
`0x800` through `0x004A2250` when that bit is set, or clearing
the mask locally otherwise. Equivalently that mask is set exactly when:

((D XOR N) AND (N XOR W) AND `0x80000000`) is nonzero.

Both paths join at `0x004A1CBE`, clear mask four, reload saved
v as a zero-extended byte lookup index and transfer directly to
`0x004A0E64`, bypassing the fresh global byte load at
`0x004A0E5D`. FND-EXE-132 grounds the common zero-extended
word contribution at `0x006F2B70` plus twice this index, full
mask publication, full selector clear, frame/register restoration and
full published-mask return. FND-EXE-133 records the shipped lookup
values but leaves runtime writers unresolved. Other M bits remain
locally retained apart from the stated replacements.

FND-EXE-131 grounds the enclosing 220-byte reservation. Saved v
has one byte writer and no later local overwrite before its byte
consumer. No call or local ESP change intervenes. The last writers
of the full-expression inputs are the initial full N/D reads and
the later full W read; the byte-expression inputs instead come from
their separate fresh byte reads. A saved v equal to W's low byte
is not guaranteed by this reading. No atomicity, native admission,
producer meaning or global storage lifetime is established here.

## Interpretation

Selectors 18 and 24 share a full unsigned comparison without an
optional guard. Their later byte contribution and lookup use fresh
byte inputs, while zero/sign tests use a later full W and the high
mask combines it with retained full N/D. These reads must not be
collapsed into one snapshot. This extends the width comparison with
FND-EXE-149 and FND-EXE-152, without establishing an originating
operation, complete helper reading or PATH outcome. Q-EXE-009 in
FMT-EXE-006 remains open for producers, native admission, lifetime,
runtime lookup writers and remaining branch/callee effects.

## Alternatives

- N zero and D `0x80000000` set mask one; reversing them
  clears it. A signed comparison would reverse these cases.
  Equal full values clear it, irrespective of the optional guard.
- W `0x00000100` is nonzero and clears mask `0x40`, even
  though its low byte is zero. W `0x80000000` contributes mask
  `0x80`; W `0x00000080` does not.
- Retained D `0x80000000`, N zero and later W
  `0x80000000` set mask `0x800`; changing only D to zero
  clears it. D zero, N `0x80000000`, W zero also set it;
  changing only D to `0x80000000` clears it.
- Fresh n 16, d zero and v zero contribute mask `0x10`.
  The locally retained full N/D do not replace those fresh bytes.
- If saved v and a later W's low byte differ, the lookup still uses
  saved v. This is a local dataflow distinction, not evidence that
  such differing observations occur in an admitted native state.

## How to reproduce

Use FND-EXE-011's executable identity, FND-EXE-099's six physical
mapping controls and FND-EXE-131's dispatch mapping. Recheck the
four-byte slots at shipped offsets `0x003221C8` and `0x003221E0`
for selectors 18 and 24; both select `0x004A1C40`.
Use the saved Ghidra program with -noanalysis and
ReportInstructionWindow.java at `0x004A1C40` limit 42,
`0x004A20D8` limit seven and `0x004A2250` limit four.
Exclude instructions at or beyond the declared exclusive ends.
Use FND-EXE-132 for the shared lookup/publication return.
Track full N/D read order and retention, unsigned full comparison,
both mask-one paths, fresh n/d/v ordering, saved v before the later
full W read, all zero/sign alternatives, both full XOR operands,
saved-byte writer and consumer with unchanged ESP, and direct lookup
transfer bypassing the fresh byte load. Check the Alternatives as local
arithmetic controls, not admitted native inputs. No native or emulated
execution is part of this finding.
