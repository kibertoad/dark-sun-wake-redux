---
id: FND-EXE-146
title: Word and full-width retained-result prefix branches use opposite nibble gates and distinct exact-result values
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A14E5..0x004A152C
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A2202..0x004A220F
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1EE5..0x004A1EED
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1B75..0x004A1BA6
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1F9E..0x004A1FB3
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A10FF..0x004A1114
tool: Ghidra 12.1.3 PUBLIC bounded retained-result prefix reading under Temurin 25.0.4.1
environment: null
---

## Observation

FND-EXE-131 grounds selector 29 at `0x004A14E5` and selector 27
at `0x004A1B75`. Both retain a value V from `0x01BA29E8`, then
read current full mask M at `0x0075B204` through their respective
nibble-gate paths. Neither path locally replaces mask one. Their input
widths, nibble conditions and exact-result comparisons differ.

Selector 29 zero-extends a word as V and compares its low nibble with
fifteen. An unequal nibble reads M and clears mask `0x10`; equality
takes `0x004A2202`, reads M and sets that mask. Both join at
`0x004A1502`, which tests retained CX for zero. Zero sets mask
`0x40`; nonzero clears it through `0x004A1EE5`. Both join at
`0x004A150E`. The branch clears mask `0x80`, masks a copy of
zero-extended V with `0x8000`, arithmetically shifts that full value
right eight and ORs the resulting zero or `0x80` into the mask.
The full shift operand has its sign bit clear, so no upper sign filling
occurs. It compares retained CX with `0x7FFF` at `0x004A1522`
and directly transfers those equality flags to `0x004A0F4D`.

Selector 27 reads full V, tests its low nibble for zero and sets mask
`0x10` when that nibble is zero. A nonzero nibble instead takes
`0x004A1FA6`, reads M and clears that mask. Both join at
`0x004A1B8C`, which tests full retained V for zero. Zero sets
mask `0x40`; nonzero clears it through `0x004A1F9E`. Both
join at `0x004A1B97`. The branch copies the retained mask into EDX,
clears mask `0x80`, copies retained V into EAX and transfers to
`0x004A10FF`. Retained ECX still holds this path's V there.

FND-EXE-135 records that shared continuation entered by selector 57
with different register producers. On selector 27's path it masks the
copied V with `0x80000000`, logically shifts right twenty-four and
ORs zero or `0x80` into the retained mask. Its comparison of ECX
with `0x80000000` at `0x004A1109` therefore tests retained V,
not the N supplied by selector 57's earlier path. Its direct transfer at
`0x004A110F` preserves equality flags to `0x004A0F4D`.
Shared instruction identity does not make the two paths' producers equal.

FND-EXE-134 grounds the common equality tail: equality sets mask
`0x800`, inequality clears it, and both clear mask four before a
fresh low-byte read from V's storage. The locally replaced masks before
that lookup are:

| Mask | Selector 29 sets when | Selector 27 sets when |
| --- | --- | --- |
| `0x00000010` | V's low nibble equals fifteen | V's low nibble equals zero |
| `0x00000040` | word V is zero | full V is zero |
| `0x00000080` | V's bit fifteen is set | V's bit thirty-one is set |
| `0x00000800` | word V equals `0x7FFF` | full V equals `0x80000000` |

Each listed mask is cleared when its condition is false. Both paths then
clear mask four. Neither locally reads N's storage at `0x01BA29E0`
or count storage at `0x01BA29E4`, and other M bits remain retained
locally apart from these replacements. FND-EXE-132 grounds the common
zero-extended word lookup, full mask publication, subsequent full selector
clear, register/frame restoration and full published-mask return.
FND-EXE-133 records shipped lookup contributions zero and four, which
cannot change mask one; runtime table writers remain unresolved.

There are no calls or local stack changes in these bounded branch paths
before the shared restoration. Each local V test uses its initial retained
value, while the common lookup consumes a fresh byte from that storage.
No atomicity, producer domain or storage lifetime follows from these reads.

## Interpretation

These physically selected branches retain mask one but have different
nibble gates, consumed widths and exact-result values. Selector 29's
word contract resembles FND-EXE-145's selector 30 at a narrower width;
selector 27 has a zero-nibble gate and the opposite full-width exact
boundary. Sharing the continuation with FND-EXE-135's selector 57 does
not import its earlier N producer. Q-EXE-009 in FMT-EXE-006 remains
open for field/selector/mask producers, runtime lookup writers, remaining
prefix branch effects and callee contracts. These observations establish
no named originating operation, complete helper reading or PATH outcome.

## Alternatives

- V zero sets mask `0x10` in selector 27 and clears it in 29.
  V fifteen has the reverse result. V one clears that mask in both;
  neither branch implements a generic nonzero-nibble condition.
- Storage value `0x00010000` is word zero in selector 29 but full
  nonzero in 27, so only 29 sets mask `0x40`.
- Storage value `0x00008000` contributes mask `0x80` in 29 but
  not 27; `0x80000000` has the reverse high-bit contribution.
- Storage values `0x00007FFF` and `0x12347FFF` satisfy 29's
  exact-result gate but not 27's. Only full V `0x80000000` satisfies
  27's gate; `0x80000001` and `0x7FFFFFFF` do not.
- In selector 27 the shared ECX comparison tests its retained V, not
  the unread N storage. Changing N alone cannot affect this local gate.
- Mask one is locally retained in either incoming state, and remains so
  through an unchanged shipped lookup. A fresh lookup-byte read cannot
  be replaced by retained V without a separate lifetime contract.

## How to reproduce

Use FND-EXE-011's verified executable identity and FND-EXE-099's six
physical mapping controls. FND-EXE-131 grounds four-byte selector slots:
27 at shipped offset `0x003221EC`, target `0x004A1B75`; 29 at
`0x003221F4`, target `0x004A14E5`. Recheck those exact physical slots.
Use the saved Ghidra program with -noanalysis and
ReportInstructionWindow.java at `0x004A14E5` limit 22,
`0x004A1B75` limit twenty, `0x004A2202` limit three,
`0x004A1EE5` limit two, `0x004A1F9E` limit five and
`0x004A10FF` limit five. Exclude unrelated instructions after
`0x004A1527` and `0x004A1BA1`, both direct transfers.
Use FND-EXE-134 for the common equality tail and FND-EXE-132 for
publication/return. Track each path's ECX/EAX producers at the shared
entry, retained width, independent zero gate, opposite nibble conditions,
logical versus zero-extended arithmetic shifts, equality flags through
direct transfers and the fresh lookup byte. Check the Alternatives'
width/nibble/boundary cases with incoming mask one set and clear as local
arithmetic controls, not native inputs. No native or emulated execution
is part of this finding.
