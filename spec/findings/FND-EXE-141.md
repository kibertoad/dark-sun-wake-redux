---
id: FND-EXE-141
title: Zero-extended byte prefix shift retains count and lookup input while a saved byte supplies its sign gate
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1344..0x004A13BB
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A21E3..0x004A21F5
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A2134..0x004A2144
tool: Ghidra 12.1.3 PUBLIC bounded zero-extended byte prefix shift reading
environment: null
---

## Observation

FND-EXE-131 grounds selector 37 at `0x004A1344`. It zero-extends
byte C at `0x01BA29E4` into retained EBX and byte N at
`0x01BA29E0` into EDX. It zero-extends retained BL into a separate
count, decrements it, and saves DL as one byte at current ESP plus
`0x0D`. It then zero-extends DL into a full value and arithmetically
shifts right by CL. The effective k is (C minus one) modulo 32; the
full value's bit thirty-one is clear, so this shift fills with zero even
when N's bit seven is set.

It tests the shifted result's low bit, reads current full mask M at
`0x0075B204`, and sets mask one for bit one or clears it for bit
zero. The alternate clear at `0x004A213C` rejoins at
`0x004A136F`. It zero-extends byte V at `0x01BA29E8` into ECX,
sets mask `0x40` when V is zero, or clears it through
`0x004A2134` otherwise. Both rejoin at `0x004A1381`.
It replaces mask `0x80` from retained V's bit seven.

It masks a copy of retained C with `0x1F`, decrements that copy
and tests for zero. Unless C's low five bits equal one, it clears
mask `0x800` at `0x004A139F`. When they equal one, the alternate
at `0x004A21E3` compares the saved byte at ESP plus `0x0D`
with zero and tests the resulting sign flag. Nonnegative takes the
same clear path; negative sets mask `0x800`. Thus that bit is set
only when C's low five bits equal one and saved N's bit seven is set.
No intervening call or local ESP change occurs between the byte store
and comparison; the bounded paths show no intervening write to that
slot. FND-EXE-131 grounds the enclosing reserved frame. This is the
branch's saved byte, not a fresh global read or full-width sign test.

Both paths join at `0x004A13A6`, zero-extending retained CL as
lookup byte V. They clear mask four and OR the zero-extended word at
`0x006F2B70` plus twice V into the computed mask. FND-EXE-133
grounds all physical shipped contributions; runtime writers remain
unresolved. They test retained BL with `0x1F` and directly transfer
at `0x004A13BB` to `0x004A1015`, preserving those test flags.
FND-EXE-137 grounds that shared gate: low five bits zero clears mask
`0x10`, nonzero sets it, then both publish. FND-EXE-132 grounds
full mask publication, subsequent full selector clear, saved-register
restoration and full published-mask return. No calls occur in these
bounded paths. Other M bits remain locally retained apart from the
stated replacements. No atomicity, producer bounds or runtime lifetime
is established by the separate initial reads.

## Interpretation

Selector 37's retained byte/count contracts and saved-byte sign gate
are explicit. It differs from FND-EXE-138's sign-extended byte shift
and FND-EXE-140's fresh word count/lookup reads. Q-EXE-009 in
FMT-EXE-006 remains open for producers, runtime writers, other prefix
branches and remaining callee effects. No complete helper reading,
named operation model or actual PATH behavior is established.

## Alternatives

- N `0x80` shifted at k eight through 31 gives extracted bit zero;
  sign-extending N before the shift would give one.
- C zero selects k 31, C one k zero, C 32 k 31 and C 33 k zero.
  No local count rejection precedes extraction.
- C one or 33 permits mask `0x800` only for saved N bit seven set.
  The later sign test is byte-sized even though the shift was full-width.
- Retained C zero or 32 clears mask `0x10`; one or 33 sets it.
  Neither the gate nor the V lookup reloads its global input.
- Upper storage bytes do not participate in these byte-width tests.
  Initial N/C/V reads are separate and retain unresolved producer/lifetime
  contracts; runtime table immutability is not proved by shipped values.

## How to reproduce

Verify FND-EXE-011's executable identity, FND-EXE-099's physical controls
and FND-EXE-131's selector slot and enclosing frame. Use the saved Ghidra
program with -noanalysis and ReportInstructionWindow.java at
`0x004A1344` limit 32, `0x004A21E3` limit six,
`0x004A2134` limit five and `0x004A13B8` limit two.
Exclude following unrelated instructions. Use FND-EXE-137 for the final
count gate and FND-EXE-132 for publication/return. Track zero-extension,
retained BL/CL, the saved byte's store and comparison through unchanged
ESP, masked decrement count, alternate clears and joins, and flags across
the direct final transfer. Check Alternatives' count/sign cases as local
arithmetic controls, not native inputs. No native or emulated execution
is part of this finding.