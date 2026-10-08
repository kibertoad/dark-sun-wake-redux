---
id: FND-EXE-149
title: Shared word prefix branch keeps initial comparison words separate from fresh bytes and a later result word
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A16C0..0x004A175F
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A2030..0x004A2047
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A2024..0x004A2030
tool: Ghidra 12.1.3 PUBLIC bounded shared-word prefix reading under Temurin 25.0.4.1
environment: null
---

## Observation

FND-EXE-131 grounds selectors 17 and 23 at the same physical target
`0x004A16C0`. The branch zero-extends word N at `0x01BA29E0`
and word D at `0x01BA29E4`, retains both, and compares their
sixteen-bit values unsigned. N below D reads current full mask M at
`0x0075B204` and sets mask one. N at or above D takes
`0x004A203A`, reads M, clears mask one and rejoins at
`0x004A16DF`. No optional guard read or full-value sentinel comparison
occurs on these local paths; upper storage bytes do not affect this gate.

At the join, the branch freshly zero-extends byte d from D's storage,
clears mask `0x10`, then freshly zero-extends byte n from N's storage
and byte v from `0x01BA29E8`. It XORs n with d and then v.
At `0x004A16FD` it saves freshly read v as one byte at current
ESP plus `0x20`. Only afterward, at `0x004A1701`, it freshly
zero-extends word W from that same V storage. It masks the earlier
byte XOR with `0x10` and ORs its zero or `0x10` contribution
into the retained mask. That contribution uses n/d/v, not substituted
low bytes from initial N/D or later W.

The branch tests W at word width. W zero sets mask `0x40`;
nonzero clears it through `0x004A2030`. Both join at
`0x004A171B`, retain zero-extended W, clear mask `0x80`, mask
a copy of W with `0x8000` and arithmetically shift that full value
right eight. Its full sign bit is clear, so the contribution is zero or
`0x80`, without upper sign filling.

It next zero-extends the initially retained N and D words afresh from
their registers, not from global storage, and computes D XOR N and
N XOR W. It ANDs those results and logically shifts right fifteen.
All operands came from zero-extended words, so the shifted result is
zero or one; its AL test is sufficient for this local domain. Nonzero
sets mask `0x800`; zero takes `0x004A2024` and clears it.
The local condition is therefore:

((D XOR N) AND (N XOR W) AND `0x8000`) is nonzero.

Both paths join at `0x004A1750`, clear mask four, zero-extend the
saved byte at ESP plus `0x20` as the lookup index and directly transfer
to `0x004A0E64`. They skip the fresh byte load at `0x004A0E5D`
described in FND-EXE-132. FND-EXE-131 grounds the enclosing 220-byte
reservation. No call or local ESP change occurs between the saved-byte
store and its read, and the bounded paths contain no intervening local
store to that slot.

The masks replaced before the lookup are:

| Mask | Set when | Cleared when |
| --- | --- | --- |
| `0x00000001` | unsigned retained word N is below D | N is at or above D |
| `0x00000010` | fresh n XOR d XOR v has bit four set | that bit is clear |
| `0x00000040` | later word W is zero | W is nonzero |
| `0x00000080` | W's bit fifteen is set | that bit is clear |
| `0x00000800` | the stated retained-word AND/XOR has bit fifteen set | that bit is clear |

Both paths also clear mask four. FND-EXE-132 grounds the common
zero-extended word contribution at `0x006F2B70` plus twice saved v,
full mask publication, subsequent full selector clear, register/frame
restoration and full published-mask return. FND-EXE-133 records all
shipped lookup contributions, with runtime writers unresolved. Other M
bits remain locally retained apart from the stated replacements.
The separate N/D, n/d/v and W reads establish no atomicity, native
producer bounds or storage lifetime. Equal dispatch targets do not establish
equal caller admission or the same originating operation for both selectors.

## Interpretation

Selectors 17 and 23 share a locally read word-width contract with distinct
initial comparison inputs, fresh byte inputs, a later word and a saved
lookup byte. Unlike FND-EXE-148's guarded full-width branch, this path
compares N with D directly, reads no optional guard and uses word-width
high-bit gates. These observations do not establish an originating
arithmetic operation, complete helper reading or PATH outcome. Q-EXE-009
in FMT-EXE-006 remains open for producers, native admission/lifetime,
runtime writers, other prefix branches and remaining callee contracts.

## Alternatives

- N `0x8000` and D zero clear mask one; N zero and D `0x8000`
  set it. A signed word comparison would reverse that ordering.
- Storage N `0x00010000` and D one compare as word zero and one,
  setting mask one. Widening this same comparison would change the result.
- W `0x0100` is nonzero even when saved v is zero. The zero gate
  does not test the saved byte, and the lookup does not reload W's byte.
- W `0x8000` contributes mask `0x80`; upper-only storage value
  `0x80000000` is word zero and does not. The high-bit gate is word-sized.
- D `0x8000`, N zero, W `0x8000` set mask `0x800`; only
  changing D to zero clears it. D zero, N `0x8000`, W zero also
  set it; only changing D to `0x8000` clears it. One sign test or
  an N/W XOR alone cannot replace the two-XOR AND.
- One or three fresh bytes with bit four set contribute `0x10`;
  two cancel. Substituting the earlier full-word low bytes would require
  a lifetime contract not established here.

## How to reproduce

Use FND-EXE-011's executable identity, FND-EXE-099's six physical
mapping controls and FND-EXE-131's dispatch mapping. Four-byte selector
slots 17 and 23 start at shipped offsets `0x003221C4` and
`0x003221DC`, respectively, and both select `0x004A16C0`.
Recheck those exact physical slots. Use the saved Ghidra program with
-noanalysis and ReportInstructionWindow.java at `0x004A16C0` limit
forty, `0x004A1750` limit four, `0x004A2030` limit five,
`0x004A203A` limit three and `0x004A2024` limit three.
Exclude unrelated instructions at or after `0x004A175F` and
`0x004A2047`. Use FND-EXE-132 for the common publication/return.
Track word zero-extension and unsigned comparison, initial N/D retention,
fresh d/n/v reads, saved-byte publication before W's read, distinct W
zero/high-bit tests, register-only N/D reloads, the bounded shifted AND
result, saved-byte last writer and unchanged ESP, and the fresh-tail-read
bypass. Check the Alternatives cases as local arithmetic controls, not
native inputs. No native or emulated execution is part of this finding.
