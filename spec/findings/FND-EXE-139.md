---
id: FND-EXE-139
title: Full-width logical prefix shift reloads its count for a one-count sign gate and final mask replacement
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A19AB..0x004A1A14
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A131F..0x004A1344
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1EED..0x004A1F03
tool: Ghidra 12.1.3 PUBLIC bounded logical full-width prefix branch reading
environment: null
---

## Observation

FND-EXE-131 grounds selector 39's physical target `0x004A19AB`.
It zero-extends byte C at `0x01BA29E4`, reads full N at
`0x01BA29E0`, decrements C and logically shifts a copy of N right
by CL. Its effective count k is (C minus one) modulo 32. It tests the
shifted result's low bit, then reads current full mask M at `0x0075B204`,
setting mask one for extracted bit one or clearing it for bit zero.
The alternate clear at `0x004A1EF5` rejoins at `0x004A19CE`.
It reads full V at `0x01BA29E8`, setting mask `0x40` for V zero
or clearing it through `0x004A1EED` for V nonzero. Both paths
rejoin at `0x004A19DE`; neither skips the following count read.

At that join it freshly reads the full thirty-two-bit value D at C's
storage. It replaces mask `0x80` from retained V's bit thirty-one,
masking V with `0x80000000` and logically shifting right 24.
It then masks a copy of D with `0x1F`, decrements that copy and
checks for zero. Only when D's low five bits equal one does it test
retained full N's sign bit. Mask `0x800` is set only when both
that count condition and N's bit thirty-one hold; otherwise it is
cleared through `0x004A131F`. This is not a V/N XOR gate or
an unconditional clear, and D is not the retained decremented C.

The paths join at `0x004A1326`, freshly zero-extending byte
`0x01BA29E8` for the word lookup at `0x006F2B70` plus twice
that byte. They clear mask four and OR the zero-extended contribution
into the computed mask. FND-EXE-133 grounds all physical shipped
contributions; runtime table writers remain unresolved.

The branch next tests retained D's low byte with `0x1F`, then
transfers directly to `0x004A1015`, preserving those test flags.
FND-EXE-137 grounds that gate: low five bits zero clears mask
`0x10`, nonzero sets it, and both reach `0x004A0E6E`.
There is no further count reload in this tail. FND-EXE-132 grounds
the full mask publication, subsequent full selector clear, saved-register
restoration and full published-mask return. No calls occur in the bounded
branch. Other M bits remain retained locally apart from the stated
replacements. Separate C, N, V, D and lookup-byte reads do not establish
atomicity or producer/lifetime contracts.

## Interpretation

Selector 39 has distinct initial shift-count and later mask-count reads,
with a conditional retained-N sign gate. Q-EXE-009 in FMT-EXE-006
remains open for count/field/mask/selector producers, runtime writers,
other prefix branches and remaining callee effects. This bounded branch
reading establishes no named originating instruction, complete helper
reading or actual PATH behavior.

## Alternatives

- C zero selects bit 31, C one selects bit zero, C 32 selects bit 31
  and C 33 selects bit zero. No local count rejection precedes extraction.
- D with low five bits one permits mask `0x800` only when N's sign
  bit is set. D with low five bits zero or two clears it regardless of N.
  V's sign bit independently controls mask `0x80`.
- If C and D agree at their low byte, C one or 33 satisfies the one-count
  gate while C zero or 32 does not. That equality is not proved by the two
  reads; a later D cannot silently replace C in the earlier extraction.
- The final mask `0x10` test uses retained D, although the lookup byte
  is freshly read. The direct transfer preserves that test's flags rather
  than using a new count test at the shared target.
- Upper D bits are read but do not affect either low-five-bit gate. Full
  N/V upper bits do affect their stated extraction, zero and sign tests.

## How to reproduce

Verify FND-EXE-011's executable identity, FND-EXE-099's physical controls
and FND-EXE-131's selector slot. Use the saved Ghidra program with
-noanalysis and ReportInstructionWindow.java at `0x004A19AB` limit
40, `0x004A131F` limit ten and `0x004A1EED` limit five.
Exclude subsequent unrelated instructions. Use FND-EXE-137 for the gate
at `0x004A1015` and FND-EXE-132 for publication/return. Track
initial byte C versus fresh full D, full retained N/V, masked decrement
counts, alternate clears, one-count and N-sign conditions, fresh lookup
byte and test flags across the direct transfer. Check the count/sign
examples in Alternatives as local arithmetic controls, not native inputs.
No native or emulated execution is part of this finding.

The location ranges use exclusive ends, including the final transfers
already described above. Verify them with additional windows at
`0x004A1A0F`, `0x004A133F`, `0x004A1EFE`, each with limit two;
the following instruction starts are their declared exclusive ends.
