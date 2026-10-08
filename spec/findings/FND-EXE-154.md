---
id: FND-EXE-154
title: Retained word prefix branch uses zero nibble and exact minimum-word tests
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1DE1..0x004A1E14
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1F46..0x004A1F5B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A0F3E..0x004A0F60
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A0FAC..0x004A0FBD
tool: Ghidra 12.1.3 PUBLIC bounded retained-word prefix reading under Temurin 25.0.4.1
environment: null
---

## Observation

FND-EXE-131 grounds selector 26 at `0x004A1DE1`. It
zero-extends word V from `0x01BA29E8` into ECX and tests
CL's low four bits for zero. A zero nibble reads full mask M
from `0x0075B204` and sets mask `0x10`. A nonzero nibble
takes `0x004A1F4E`, reads M and clears that mask. Both paths
join at `0x004A1DF9`, which tests retained CX for zero.
V zero sets mask `0x40`; nonzero clears it through
`0x004A1F46`. Both join at `0x004A1E05`.

The branch copies the retained mask to EDX, clears mask `0x80`,
copies zero-extended V to EAX and transfers to `0x004A0F3E`.
That shared continuation masks EAX with `0x8000`, arithmetically
shifts it right eight and ORs the result into EDX. The masked
full operand has its sign bit clear, so the contribution is zero
or `0x80`, without upper sign filling. It then compares retained
CX with `0x8000`. On this path CX is V, unlike selector 56's
N producer described in FND-EXE-134. Shared instructions do not
establish equal earlier producers.

Equality takes `0x004A0FAC`, copies EDX and sets mask `0x800`.
Inequality copies EDX locally and clears that mask. Both clear mask
four, then transfer to `0x004A0E5D` for a fresh byte read from
V's storage. No local path replaces mask one, reads N's storage at
`0x01BA29E0`, reads D/count storage at `0x01BA29E4`, or reads
the optional guard at `0x01BA29F4`. Other M bits remain locally
retained apart from the following replacements:

| Mask | Set when | Cleared when |
| --- | --- | --- |
| `0x00000010` | word V's low nibble is zero | its low nibble is nonzero |
| `0x00000040` | word V is zero | word V is nonzero |
| `0x00000080` | V's bit fifteen is set | that bit is clear |
| `0x00000800` | word V equals `0x8000` | word V differs from `0x8000` |

FND-EXE-132 grounds the common zero-extended word contribution at
`0x006F2B70` plus twice the fresh byte index, full mask publication,
full selector clear, frame/register restoration and full published-mask
return. FND-EXE-133 records the shipped lookup values, which cannot
change mask one, but leaves runtime writers unresolved.
There are no calls or local stack changes before the shared restoration.
Each local V test uses the initially retained word; the lookup reads
a fresh byte. No atomicity, native admission, producer meaning or
global storage lifetime follows from these separate reads.

## Interpretation

Selector 26's retained-word contract has a zero-nibble gate and exact
`0x8000` test. FND-EXE-146's selector 29 instead has a nibble-fifteen
gate and exact `0x7FFF` test. Entering selector 56's shared tail
does not import its N producer or its earlier mask-one replacement.
This is a bounded branch contract, not a complete helper reading,
originating operation or PATH outcome. Q-EXE-009 in FMT-EXE-006
remains open for producers, native admission, lifetime, runtime lookup
writers and remaining branch/callee effects.

## Alternatives

- V zero sets masks `0x10` and `0x40`; V one clears both.
  V sixteen sets `0x10` but clears `0x40`. A zero low nibble
  is not the same condition as a zero word.
- V `0x8000` sets masks `0x80` and `0x800`, while
  V `0x8001` sets only the first of those two.
  V `0x7FFF` clears both despite being selector 29's exact boundary.
- Upper bytes of V's full storage do not participate in these word
  tests. A low word zero remains zero regardless of those upper bytes.
- Changing N, D/count or the optional guard cannot affect this local
  branch because it does not read them. Mask one is locally retained.
- A later global low byte can differ from retained V's low byte;
  the lookup consumes the fresh byte, not retained CL. This describes
  local dataflow, not evidence that such a native state is admitted.

## How to reproduce

Use FND-EXE-011's executable identity, FND-EXE-099's six physical
mapping controls and FND-EXE-131's dispatch mapping. Recheck selector
26's four-byte slot at shipped offset `0x003221E8`; it selects
`0x004A1DE1`. Use the saved Ghidra program with -noanalysis and
ReportInstructionWindow.java at `0x004A1DE1` limit 32,
`0x004A1F46` limit seven, `0x004A0F3E` limit nine,
`0x004A0F56` limit three, `0x004A0FAC` limit four and
`0x004A0FB8` limit two. Exclude instructions at or beyond the
declared exclusive ends. Use FND-EXE-132 for the common lookup
and publication return. Track the retained ECX producer into every
word/nibble test, both mask paths, the bounded masked arithmetic shift,
the exact CX comparison and its equality flags, mask-one retention,
unchanged local stack and both fresh-byte lookup transfers. Check the
Alternatives as local arithmetic controls, not admitted native inputs.
No native or emulated execution is part of this finding.
