---
id: FND-EXE-147
title: Byte retained-result prefix branches use opposite nibble gates and keep their initial byte for the common lookup
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1683..0x004A16C0
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A205E..0x004A2073
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1CCD..0x004A1D0F
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A21D6..0x004A21E3
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A2149..0x004A2151
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A10AD..0x004A10C7
tool: Ghidra 12.1.3 PUBLIC bounded retained-byte prefix reading under Temurin 25.0.4.1
environment: null
---

## Observation

FND-EXE-131 grounds selector 25 at `0x004A1683` and selector 28
at `0x004A1CCD`. Each zero-extends byte B from `0x01BA29E8`
into EBX and retains that byte through its local comparisons and lookup.
Neither locally replaces mask one or reads N/count storage at
`0x01BA29E0` / `0x01BA29E4`.

Selector 25 tests B's low nibble for zero. Zero reads current full mask
M at `0x0075B204` and sets mask `0x10`; a nonzero nibble
takes `0x004A2066`, reads M and clears it. Both join at
`0x004A169B`, which tests retained BL for zero. Zero sets mask
`0x40`; nonzero clears it through `0x004A205E`. Both join
at `0x004A16A6`. The branch copies the retained mask into EDX,
clears mask `0x80`, masks a copy of B with `0x80` and ORs
that zero or `0x80` contribution into the mask. It compares retained
BL with `0x80` at `0x004A16B8` and transfers those equality
flags directly to `0x004A10AD`.

Selector 28 instead compares a copy of B's low nibble with fifteen.
Inequality reads M and clears mask `0x10`; equality takes
`0x004A21D6`, reads M and sets it. Both join at `0x004A1CEA`,
which tests retained BL for zero. Zero sets mask `0x40`; nonzero
clears it through `0x004A2149`. Both join at `0x004A1CF5`.
The branch copies the retained mask into EDX, clears mask `0x80`
and ORs retained B's bit-seven contribution into it. It compares BL
with `0x7F` at `0x004A1D07` and transfers those equality flags
directly to the same `0x004A10AD` tail.

FND-EXE-135 grounds this shared equality tail and its separate set path
at `0x004A1E69`: equality sets mask `0x800`, inequality clears
it, and both join at `0x004A10BA`. That join clears mask four,
zero-extends retained BL as the lookup index and transfers directly to
`0x004A0E64`. The freshly read byte at `0x004A0E5D` is skipped.
Neither local branch changed BL after its initial load, and no call
intervenes. The local replacements before the lookup are therefore:

| Mask | Selector 25 sets when | Selector 28 sets when |
| --- | --- | --- |
| `0x00000010` | B's low nibble equals zero | B's low nibble equals fifteen |
| `0x00000040` | B is zero | B is zero |
| `0x00000080` | B's bit seven is set | B's bit seven is set |
| `0x00000800` | B equals `0x80` | B equals `0x7F` |

Each listed mask is cleared when its condition is false. Both then clear
mask four; other M bits remain locally retained apart from these replacements.
FND-EXE-132 grounds the zero-extended word contribution at
`0x006F2B70` plus twice the chosen byte, full mask publication,
subsequent full selector clear, register/frame restoration and full
published-mask return. FND-EXE-133 records all shipped contributions as
zero or four, which cannot change mask one. Runtime table writers and
actual producer domains remain unresolved.

Only the initially read byte affects these local comparisons and the
lookup index; upper bytes in V's storage do not participate. No calls or
local stack changes occur in these bounded branch paths before the common
restoration. Retaining one byte does not establish the global lifetime,
atomicity of publication or immutability of the lookup table.

## Interpretation

These two grounded byte branches retain mask one and their lookup byte,
with opposite nibble gates and different exact-result values. Their lookup
read ordering differs from the fresh-byte tails in FND-EXE-145 and
FND-EXE-146. Sharing a tail with FND-EXE-135's earlier exact-N path
does not import its N producer: these branches supply their own B
comparisons and flags. Q-EXE-009 in FMT-EXE-006 remains open for
producers, runtime writers, other prefix branches and callee contracts.
No originating operation, complete helper reading or PATH outcome is established.

## Alternatives

- B zero sets mask `0x10` in selector 25 and clears it in 28;
  B fifteen has the reverse result. B one clears it in both.
- B sixteen sets mask `0x10` only in 25; B 255 sets it only
  in 28. A generic nonzero-nibble gate disagrees with both contracts.
- B 128 sets mask `0x800` only in 25; B 127 sets it only in
  28. Neither 129 nor 255 satisfies either exact-result gate.
- Storage value `0x00010000` is byte zero in both branches and
  sets mask `0x40`. Upper-only storage bits cannot make it nonzero.
- The lookup index is retained B even if a later byte at its global
  storage would differ. No fresh load precedes this lookup on these paths.
- Incoming mask one is retained locally and through an unchanged shipped
  lookup. That conditional result is not a proof about runtime table writers.

## How to reproduce

Use FND-EXE-011's executable identity, FND-EXE-099's six physical
mapping controls and FND-EXE-131's four-byte selector slots. Selector 25
starts at shipped offset `0x003221E4` and selects `0x004A1683`;
selector 28 starts at `0x003221F0` and selects `0x004A1CCD`.
Recheck those exact physical slots. Use the saved Ghidra program with
-noanalysis and ReportInstructionWindow.java at `0x004A1683` limit
21, `0x004A1CCD` limit 21, `0x004A205E` limit five,
`0x004A21D6` limit three, `0x004A2149` limit two,
`0x004A10AD` limit eight and `0x004A1E69` limit two.
Exclude unrelated instructions after the direct transfers at
`0x004A16BB`, `0x004A1D0A` and `0x004A10C2`.
Use FND-EXE-135 for the set-path continuation and FND-EXE-132 for
publication/return. Track retained BL on every path, opposite nibble gates,
independent zero/high-bit conditions, comparison flags through direct
transfers and the lookup's bypass of the fresh byte read. Check every
Alternatives case with incoming mask one set and clear as local arithmetic
controls, not native inputs. No native or emulated execution is part of this finding.
