---
id: FND-EXE-138
title: Signed byte prefix shift retains its count and lookup input through the final mask gate
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A125E..0x004A12C4
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1F19..0x004A1F30
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A101D..0x004A1025
tool: Ghidra 12.1.3 PUBLIC bounded signed byte prefix shift reading
environment: null
---

## Observation

FND-EXE-131 grounds selector 40's physical target `0x004A125E`.
It zero-extends byte C at `0x01BA29E4` into retained EBX and
sign-extends byte N at `0x01BA29E0` into full EAX. A separate
zero-extension of BL supplies the count, which is decremented before the
thirty-two-bit arithmetic right shift. The effective count k is (C minus
one) modulo 32. The branch tests the shifted result's low bit, then reads
current full mask M at `0x0075B204`, setting mask one for bit one
or clearing it for bit zero. The alternate clear at `0x004A1F23`
rejoins at `0x004A1282`; it does not skip subsequent input reads.

The branch zero-extends byte V at `0x01BA29E8` and tests DL.
V zero sets mask `0x40`; V nonzero clears it through
`0x004A1F19`. Both paths join at `0x004A1296`, where V is
again zero-extended from retained DL, not freshly read from storage.
The branch ANDs its mask with `0xFFFFF77B`, clearing masks
`0x800`, `0x80` and four. It reads the zero-extended word at
`0x006F2B70` plus twice retained V, derives zero or `0x80`
from V's bit seven, and ORs both contributions into the mask. FND-EXE-133
records the complete physical shipped lookup values. Runtime table writers
remain unresolved; the physical values do not establish immutability.

It then tests retained BL with `0x1F`. Nonzero low five bits of the
original retained C set mask `0x10` through `0x004A101D`; zero
low five bits clear that mask locally at `0x004A12BC`. Both transfer
to publication at `0x004A0E6E`. FND-EXE-132 grounds the full mask
store, subsequent full selector clear, saved-register restoration and full
published-mask return. The count gate does not use decremented C and does
not freshly read its storage. The lookup likewise does not reload V.
No calls occur in this bounded path. Other M bits remain retained locally
apart from the stated replacements; mask `0x800` has no independent
set gate here before the lookup contribution.

## Interpretation

Selector 40's signed byte extraction and retained count/lookup inputs are
explicit. FND-EXE-137's word/full-width arithmetic-shift branches share
some mask positions but freshly read the lookup byte and final count.
Those read contracts must remain distinct. Q-EXE-009 in FMT-EXE-006
remains open for count/field/mask/selector producers, lifetime, other prefix
branches and remaining callee effects. No named originating instruction,
complete helper reading or actual PATH behavior is established.

## Alternatives

- Byte N `0x80` is sign-extended before shifting. At k eight through
  31 its extracted bit is one; zero-extending it would give zero.
- C zero gives k 31, C one gives k zero, C 32 gives k 31 and C 33
  gives k zero. No local count rejection precedes the shift.
- Retained C one or 33 sets mask `0x10`; C zero or 32 clears it.
  Testing the decremented count would give different answers for those cases.
- Upper bytes in N/V storage do not affect this branch's byte tests or
  sign contribution. V `0x80` contributes mask `0x80`; V zero
  sets mask `0x40` independently of the lookup's contribution.
- Retained BL and DL, rather than later global reads, drive the final
  gate and lookup. That does not prove the separate initial reads are atomic
  or establish any producer or runtime table lifetime.

## How to reproduce

Verify FND-EXE-011's executable identity, FND-EXE-099's physical controls
and FND-EXE-131's selector slot. Use the saved Ghidra program with
-noanalysis and ReportInstructionWindow.java at `0x004A125E` limit
28, `0x004A1F19` limit six and `0x004A101D` limit two.
Exclude instructions after the declared branch. Use FND-EXE-132 for shared
publication and return, and FND-EXE-133 for the physical lookup values.
Track signed byte extension, retained EBX versus the separate decremented
ECX, low-five-bit shift counts, alternate mask clears and joins, retained
DL's lookup/high-bit uses, final BL test and publication order. Check the
count/sign examples in Alternatives as local arithmetic controls; they are
not native input observations. No native or emulated execution is part of
this finding.

The location ranges use exclusive ends, including the final transfers
already described above. Verify them with additional windows at
`0x004A12BF`, `0x004A1F2B` and `0x004A1020`, each with limit
two; the following instruction starts are their declared exclusive ends.
