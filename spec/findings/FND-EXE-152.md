---
id: FND-EXE-152
title: Shared byte prefix branch saves comparison and result bytes for subsequent masks
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A11CB..0x004A125E
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A2115..0x004A212C
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1FC0..0x004A1FCC
tool: Ghidra 12.1.3 PUBLIC bounded shared-byte prefix reading under Temurin 25.0.4.1
environment: null
---

## Observation

FND-EXE-131 grounds selectors 16 and 22 at the same target,
`0x004A11CB`. This branch zero-extends byte D from `0x01BA29E4`,
then byte N from `0x01BA29E0`, and saves D at current ESP plus
`0x3F`. It compares N with D unsigned at byte width. N below D
reads full mask M from `0x0075B204` and sets mask one. N at or
above D takes `0x004A211F`, reads M and clears mask one. Both
join at `0x004A11ED`. Neither path reads the optional guard.

The join reads and zero-extends byte V from `0x01BA29E8`, clears
mask `0x10`, reloads saved D, and saves V at ESP plus `0x3E`.
It XORs D with retained N in DL and then retained V in BL,
contributing only (D XOR N XOR V) AND `0x10`.
V zero sets mask `0x40`; V nonzero clears it through
`0x004A2115`. Both paths join at `0x004A1218`.

The next portion zero-extends saved V, retained DL as N, and saved
D. It clears mask `0x80` and contributes V AND `0x80`.
It computes (D XOR N) AND (N XOR V), logically shifts that
zero-extended byte result right seven and tests AL. The result is
bounded to zero or one. Nonzero sets mask `0x800`; zero clears
it through `0x004A1FC0`. Equivalently that mask is set exactly when:

((D XOR N) AND (N XOR V) AND `0x80`) is nonzero.

Both paths join at `0x004A124F`, clear mask four, zero-extend
saved V as the lookup index and directly transfer to `0x004A0E64`.
They skip the fresh global byte load at `0x004A0E5D`.
FND-EXE-132 grounds the shared zero-extended word contribution at
`0x006F2B70` plus twice that index, full mask publication, full
selector clear, frame/register restoration and full published-mask
return. FND-EXE-133 records the shipped lookup words but leaves
runtime writers unresolved. Other M bits remain locally retained
apart from the stated replacements.

FND-EXE-131 grounds the enclosing 220-byte reservation. D's slot
is written once before comparison and V's once after comparison.
Neither is overwritten before its later reads, and no local call or
ESP change intervenes. Their adjacent byte slots are independently
read as bytes, never combined into a word. Retained DL is not changed
between N's initial read and its later consumers. There are no fresh
global N/D/V reads after these initial byte reads. Separate read order
does not establish atomicity, originating operations, admitted native
inputs or storage lifetime.

## Interpretation

Selectors 16 and 22 share a byte comparison without optional-guard
admission. Saved D survives EBX's reuse for V, and saved V survives
the later mask work. This contrasts with FND-EXE-149's separate
fresh-byte and later-word reads. This is a bounded local branch
contract, not a complete helper or PATH outcome. Q-EXE-009 in
FMT-EXE-006 remains open for producers, native admission, lifetime,
runtime lookup writers and remaining branch/callee effects.

## Alternatives

- N zero and D 128 set mask one; N 128 and D zero clear it.
  A signed comparison would reverse these cases. Equal bytes clear it.
- Different full guard values cannot affect this local branch because
  it never reads that guard. Upper N/D/V storage bytes are also excluded.
- D 128, N zero, V 128 set mask `0x800`; changing only D to
  zero clears it. D zero, N 128, V zero also set it; changing
  only D to 128 clears it. A single sign test is insufficient.
- D 16, N zero and V zero contribute mask `0x10`, while all
  three zero do not. Reloading D from BL after its reuse would use V
  and lose that distinction.
- The lookup uses saved V, not a newly read global V or a combined
  word from the adjacent D/V stack slots.

## How to reproduce

Use FND-EXE-011's executable identity, FND-EXE-099's six physical
mapping controls and FND-EXE-131's dispatch mapping. Recheck the
four-byte slots at shipped offsets `0x003221C0` and `0x003221D8`
for selectors 16 and 22; both select `0x004A11CB`.
Use the saved Ghidra program with -noanalysis and
ReportInstructionWindow.java at `0x004A11CB` limit 65,
`0x004A2115` limit eight and `0x004A1FC0` limit five.
Exclude instructions at or beyond the declared exclusive ends.
Use FND-EXE-132 for the shared lookup/publication return.
Track D-before-N-before-V read order, unsigned byte comparison,
both mask-one paths, each saved-byte writer and consumer, retained DL,
EBX reuse, unchanged ESP, bounded shifted AND result and the direct
lookup transfer that bypasses a fresh byte read. Check the Alternatives
as local arithmetic controls, not admitted native inputs. No native
or emulated execution is part of this finding.
