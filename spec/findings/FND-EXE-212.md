---
id: FND-EXE-212
title: Concrete record count writers select distinct finite matching-prefix paths
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D6CF0..0x006D6D01
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D6D42..0x006D6D64
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FE669..0x005FE69E
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5156..0x005F51CC
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5340..0x005F53A7
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x002EDC68..0x002EDC71
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x002EEAD3..0x002EEAD9
tool: scientific-method-engine 13.6.0, executable-reader 2.5.0 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-209's field-helper record starts at its frame F -124.
The negative-selector arm writes two at F -120, record offset four,
before calling `0x006D5030`. Its other nonnegative, non-fixed-address
arm writes one at that same field before calling `0x005F5770`.
These stores occur after setup. The fixed-address arm does not execute
either store; neither value is assumed for that arm.

The nested-object record starts at its own frame F -64. It writes one
at F -60, record offset four, before calling the field helper. The object
word store precedes this count store. The following field-helper call
precedes record cleanup, as recorded in FND-EXE-050. These are local
writers, not proof of preservation across those calls or actual selection.

For the field-helper metadata address B = `0x006EE868`, the four bytes
following FND-EXE-210's five-byte prefix supply terminating payloads
zero, three, one and zero. For the nested-object metadata address
B = `0x006EF6D3`, the two bytes following its four-byte prefix supply
terminating payloads zero and zero. Every listed byte has its continuation
bit clear. The independently hash-checked PE maps these source intervals
to the cited file-data ranges.

Compose the ordinary callback path in FND-EXE-196 with these producers,
requiring that the selected record, count, source and local state retain
their values, with valid disjoint source/output/stack storage, admitted
segments and normal callee/frame behavior. The metadata result supplies
cursor B +5 or B +4. The later modifier receives the second marker,
zero or 255 respectively; FND-EXE-061 returns zero for both, with its
exact-255 bypass preceding its mask-class guard. The count accessor
increments and the caller decrements at 32-bit width, recovering the
fetched offset-four word without modifying that record field.

| Selected producer and count | Decoded pairs | Final cursor | Incremented last first value | Derived last second value | Next local path |
| --- | --- | --- | --- | --- | --- |
| Field helper, two | (0, 3), (1, 0) | B +9 | 2 | 0 | Classification two join |
| Field helper, one | (0, 3) | B +7 | 1 | B +11 | Later matching work |
| Nested object, one | (0, 0) | B +6 | 1 | 0 | Classification two join |

Each pair invokes the byte decoder twice. Under those conditions the
positive local counter requires exactly two or one iterations, and each
decoder consumes one terminating byte. The last pair replaces both output
locals rather than accumulating earlier pairs. For field-helper count one,
the nonzero second value three is added to metadata output offset sixteen,
B +9, then decremented, giving B +11. For the other two cases the second
value is zero, so the derived local retains its earlier zero initialization.
All arithmetic is at 32-bit width. No later matching result is inferred.

## Interpretation

This supplies concrete count writers and finite conditional consumption of
the first downstream pairs for FND-EXE-210's two metadata origins.
Q-EXE-009 retains selection, count/field writers, source preservation,
aliases, intervening calls, the fixed-address arm's admission and later
matching/cleanup effects. These conditional compositions are not native
observations or a complete reading of the record lifecycle.

## Alternatives

Using one count for every field-helper arm loses its branch-specific stores.
Using the first pair after a two-iteration loop loses the second pair's
replacement writes. Masking marker 255 before its modifier bypass would
incorrectly infer an abort boundary. A finite local byte path does not
prove that setup or other calls preserve the selected record.

## How to reproduce

Verify shipped XXH3-128 `09861838aa3018346f9f15c9a4f5925c`.
Map and inspect exactly nine bytes at `0x006EE868` and six at
`0x006EF6D3` with the pinned PE mapping. In the saved project read-only,
analysis disabled, run ReportInstructionWindow at `0x006D6CA3`, count
60; `0x006D6D42`, count ten; `0x005FE669`, count twenty;
`0x005F5156`, count twenty-eight; and `0x005F5340`, count thirty.
Restrict claims to cited spans, excluding adjacent handlers and later matching.
Compose FND-EXE-056/059/061/196/209/210, following each field writer,
exact marker gate, returned cursor, loop counter, output replacement and
post-loop arithmetic. Keep rich reports in the licensed-source store,
outside Git. Execute no original program.
