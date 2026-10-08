---
id: FND-EXE-148
title: Full-width prefix branch combines guarded unsigned admission with fresh byte masks and retained sign inputs
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A175F..0x004A1803
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1E82..0x004A1E95
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A220F..0x004A221A
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1EDB..0x004A1EE5
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A2268..0x004A2274
tool: Ghidra 12.1.3 PUBLIC bounded guarded-prefix reading under Temurin 25.0.4.1
environment: null
---

## Observation

FND-EXE-131 grounds selector 12 at `0x004A175F`. The branch reads
full N from `0x01BA29E0` and full V from `0x01BA29E8` into
retained registers and compares N with V unsigned. N below V takes
`0x004A1E82`, reads full D from `0x01BA29E4`, reads current
full mask M at `0x0075B204` and sets mask one. It does not read
the optional guard on that path.

For N at or above V, the branch reads full G at `0x01BA29F4`.
G zero takes `0x004A220F`, reads D and reaches the mask-one clear
at `0x004A1790`. G nonzero instead reads D at `0x004A1781`
and compares the full value with `0xFFFFFFFF`. Equality takes the
mask-one set at `0x004A1E88`; inequality reaches the clear. Each
path reads M once through its selected set/clear route and retains D.
All join at `0x004A1798`. Therefore mask one is set exactly when
unsigned N is below V, or N is at or above V with nonzero G and
retained D equal to `0xFFFFFFFF`. D is read on every path even
where it does not decide this first mask; its later use is not bypassed.

At the join, the branch freshly zero-extends byte d from D's storage,
clears mask `0x10`, freshly zero-extends byte n from N's storage and
XORs those bytes, then freshly zero-extends byte v from V's storage and
XORs it too. It retains only bit four of that byte XOR and ORs the
zero or `0x10` contribution into the mask. At `0x004A17B9`
it also saves freshly read v as one byte at current ESP plus `0x40`.
The byte contribution is therefore (n XOR d XOR v) AND `0x10`,
not an expression over silently substituted earlier full N/D/V reads.

The branch then tests retained full V for zero. Zero sets mask `0x40`;
nonzero clears it through `0x004A1EDB`. Both join at `0x004A17CC`.
It replaces mask `0x80` from retained V's bit thirty-one, masked and
logically shifted right twenty-four. The contribution is zero or `0x80`.
It then XORs retained D with N and retained N with V, and tests the
full AND of those XOR results for a set sign bit. A set bit thirty-one
takes `0x004A2268` and sets mask `0x800`; otherwise the branch
clears it. Thus the local high-mask condition is:

((D XOR N) AND (N XOR V) AND `0x80000000`) is nonzero.

Both high-mask paths join at `0x004A17F4`, clear mask four, read
the saved byte at ESP plus `0x40` as the zero-extended lookup index
and directly transfer to `0x004A0E64`. This skips the fresh byte load
at `0x004A0E5D` described in FND-EXE-132. FND-EXE-131 grounds
the enclosing 220-byte reservation. No local ESP change or call occurs
between the saved-byte store and its read, and the bounded instruction
paths contain no intervening store to that slot.

FND-EXE-132 grounds the common zero-extended word contribution at
`0x006F2B70` plus twice the selected byte, full mask publication,
subsequent full selector clear, register/frame restoration and full
published-mask return. FND-EXE-133 records the shipped lookup values,
with runtime writers unresolved. Other M bits remain locally retained
apart from the stated replacements. Initial full reads, later byte reads,
guard admission and global publication establish no atomicity, actual
producer domain or storage lifetime.

## Interpretation

Selector 12 has a conditional guard read, a full-value equality boundary,
fresh byte-derived mask arithmetic and separate retained full-value sign
inputs. Its lookup index is the saved fresh v byte, rather than another
global reload at the tail. These are local contracts, not evidence of a
named arithmetic operation, a caller invariant or a complete helper reading.
Q-EXE-009 in FMT-EXE-006 remains open for producers, guard/field lifetime,
runtime writers, remaining prefix branches and callee contracts. Actual
PATH behavior is not established.

## Alternatives

- N zero and V `0x80000000` take the below-V set path without
  reading G. N `0x80000000` and V zero do not. A signed comparison
  would reverse this ordering.
- With N equal to V and D `0xFFFFFFFF`, G zero clears mask one;
  any nonzero G sets it. D `0x0000FFFF` does not satisfy the full
  equality, and a nonzero guard is not limited to value one.
- On a below-V path, D does not decide mask one but is still read
  and participates in the retained full-value sign expression later.
- The high mask is set for D `0x80000000`, N zero, V
  `0x80000000`, but clear when only D becomes zero. It is also
  set for D zero, N `0x80000000`, V zero, and clear when only
  D becomes `0x80000000`. A single sign test or N/V XOR is insufficient.
- Fresh bytes all zero clear mask `0x10`; one or three bytes with
  bit four set contribute `0x10`, while two cancel. Other byte bits
  do not contribute to that local mask.
- Retained full V `0x00010000` is nonzero even with saved v zero.
  Upper-only V therefore does not set mask `0x40`. Changing a later
  global V byte cannot replace the already saved lookup index on this path.

## How to reproduce

Use FND-EXE-011's executable identity, FND-EXE-099's six physical
mapping controls and FND-EXE-131's dispatch mapping. Selector 12's
four-byte slot starts at shipped offset `0x003221B0` and selects
`0x004A175F`; recheck that exact physical slot. Use the saved Ghidra
program with -noanalysis and ReportInstructionWindow.java at
`0x004A175F` limit 28, `0x004A17CC` limit sixteen,
`0x004A1E82` limit six, `0x004A220F` limit four,
`0x004A1EDB` limits two and three, and `0x004A2268` limit three.
Exclude unrelated instructions at or after `0x004A1803`,
`0x004A1E95` and `0x004A221A`. Use FND-EXE-132 for the shared
lookup/publication return. Track unsigned comparison admission, the
optional guard read, each D producer, its full equality width, independent
fresh n/d/v loads, saved-byte writer and unchanged ESP, retained full V
zero/high-bit tests, the two XOR inputs and sign test, and the bypass of
the fresh tail read. Check every Alternatives case as local arithmetic
controls, not native inputs. No native or emulated execution is part of this finding.
