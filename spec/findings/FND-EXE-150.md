---
id: FND-EXE-150
title: Guarded word prefix branch retains word comparison inputs while saving a later byte for lookup
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1114..0x004A11CB
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1E95..0x004A1EA9
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A221A..0x004A2226
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A20B8..0x004A20C2
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A216F..0x004A217B
tool: Ghidra 12.1.3 PUBLIC bounded guarded-word prefix reading under Temurin 25.0.4.1
environment: null
---

## Observation

FND-EXE-131 grounds selector 11 at `0x004A1114`. The branch
zero-extends word N from `0x01BA29E0` and word V from
`0x01BA29E8`, retains both, and compares their sixteen-bit values
unsigned. N below V takes `0x004A1E95`, zero-extends word D
from `0x01BA29E4`, reads full mask M at `0x0075B204` and
sets mask one. It does not read the optional guard on this path.

For N at or above V it reads full G at `0x01BA29F4`. G zero
takes `0x004A221A`, reads and zero-extends word D and reaches
the mask-one clear at `0x004A114A`. G nonzero instead reads
and zero-extends D at `0x004A1139`, compares SI with `0xFFFF`
and takes the set path at `0x004A1E9C` only for equality.
Inequality reaches the clear. Each path reads M once through its
selected set/clear route and retains D. All join at `0x004A1152`.
Mask one is therefore set exactly when unsigned word N is below V,
or N is at or above V with full G nonzero and word D equal to
`0xFFFF`. D is read on every path even when it does not decide
that first mask.

At the join the branch freshly zero-extends byte d from D's storage,
clears mask `0x10`, freshly zero-extends byte n from N's storage,
XORs those bytes, then freshly zero-extends byte v from V's storage
and XORs it too. It keeps only bit four of the byte XOR and ORs
the zero or `0x10` contribution into the mask. At `0x004A1173`
it saves freshly read v as one byte at current ESP plus `0x50`.
The local contribution is (n XOR d XOR v) AND `0x10`; the fresh
bytes are not replaced by low bytes of the earlier retained words.

The branch then tests retained V at word width. Zero sets mask
`0x40`; nonzero clears it through `0x004A20B8`. Both join
at `0x004A1187`, retain zero-extended V and replace mask `0x80`
with V's bit fifteen. The contribution masks a full zero-extended copy
with `0x8000` and arithmetically shifts right eight. Its full sign
bit is clear, so it contributes zero or `0x80`, without sign filling.

It next zero-extends the retained N and D words from their registers,
not from global storage, and computes D XOR N and N XOR V. It ANDs
those results, logically shifts right fifteen and tests AL. All inputs
are zero-extended words, so this shifted result is zero or one.
Nonzero sets mask `0x800`; zero takes `0x004A216F` and
clears it. The local condition is:

((D XOR N) AND (N XOR V) AND `0x8000`) is nonzero.

Both paths join at `0x004A11BC`, clear mask four, zero-extend the
saved byte at ESP plus `0x50` as the lookup index and directly transfer
to `0x004A0E64`. This skips the fresh byte load at `0x004A0E5D`
described in FND-EXE-132. FND-EXE-131 grounds the enclosing 220-byte
reservation. No call or local ESP change occurs between the saved-byte
store and its read, and the bounded paths contain no intervening local
store to that slot.

FND-EXE-132 grounds the common zero-extended word contribution at
`0x006F2B70` plus twice saved v, full mask publication, subsequent
full selector clear, register/frame restoration and full published-mask
return. FND-EXE-133 records the shipped lookup contributions, with runtime
writers unresolved. Other M bits remain locally retained apart from the
stated replacements. Initial N/V, path-selected D, full G and later n/d/v
reads establish no atomicity, native producer domain or storage lifetime.

## Interpretation

Selector 11 has word-sized comparison and sentinel gates, a full-sized
optional guard, byte-derived mask arithmetic and retained-word sign inputs.
Its lookup uses the saved fresh v byte. Unlike FND-EXE-149's separate
shared word branch, it initially compares N with V, retains that initial
V for the zero/high-bit gates, and reads no later result word.
FND-EXE-148's corresponding local guard pattern instead consumes full
N/D/V and a full sentinel. These are distinct branch contracts, not
evidence of an originating operation or complete helper reading.
Q-EXE-009 in FMT-EXE-006 remains open for producers, guard/field lifetime,
runtime writers, other prefix branches and callee contracts. Actual PATH
behavior is not established.

## Alternatives

- N zero and V `0x8000` take the below-V set path without reading
  G. N `0x8000` and V zero do not. A signed word comparison would
  reverse that ordering.
- With N equal to V and D `0xFFFF`, G zero clears mask one;
  G `0x00010000` sets it. Narrowing the full guard read to a word
  would change this example.
- Storage D `0x0000FFFF` or `0x1234FFFF` satisfies the word
  sentinel gate; the full sentinel in FND-EXE-148 would reject both.
- On a below-V path, D does not decide mask one but is still read
  and used in the retained-word sign expression later.
- D `0x8000`, N zero, V `0x8000` set mask `0x800`; only
  changing D to zero clears it. D zero, N `0x8000`, V zero also
  set it; only changing D to `0x8000` clears it. A single sign
  test or N/V XOR is insufficient.
- One or three fresh bytes with bit four set contribute `0x10`;
  two cancel. The retained-word reads do not establish that these later
  bytes equal their corresponding initial low bytes.
- Storage V `0x00010000` is word zero and sets mask `0x40`;
  V `0x0100` is word nonzero even when saved v is zero. Neither
  a full-value test nor a saved-byte test can replace this word-zero gate.

## How to reproduce

Use FND-EXE-011's executable identity, FND-EXE-099's six physical
mapping controls and FND-EXE-131's dispatch mapping. Selector 11's
four-byte slot starts at shipped offset `0x003221AC` and selects
`0x004A1114`; recheck that exact physical slot. Use the saved Ghidra
program with -noanalysis and ReportInstructionWindow.java at
`0x004A1114` limit 42, `0x004A11B5` limit six,
`0x004A1E95` limit four, `0x004A221A` limit two,
`0x004A20B8` limit three and `0x004A216F` limit three.
Exclude unrelated instructions at or after the declared exclusive ends.
Use FND-EXE-132 for the shared lookup/publication return. Track word
zero-extension and unsigned admission, optional full guard width, each D
producer and word sentinel, distinct fresh n/d/v loads, saved-byte last
writer and unchanged ESP, retained V zero/high-bit tests, register-only
N/D reloads, the bounded shifted AND result and fresh-tail-read bypass.
Check the Alternatives cases as local arithmetic controls, not native
inputs. No native or emulated execution is part of this finding.
