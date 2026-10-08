---
id: FND-EXE-142
title: Complementary-count full-width prefix branch replaces mask bits from retained sign disagreement and fresh lookup/count reads
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A180E..0x004A1870
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1FB3..0x004A1FBB
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1FD6..0x004A1FD9
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A2280..0x004A2287
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A18D2..0x004A18DC
tool: Ghidra 12.1.3 PUBLIC bounded complementary-count full-width prefix branch reading
environment: null
---

## Observation

FND-EXE-131 grounds selector 36 at `0x004A180E`. It zero-extends
byte C at `0x01BA29E4`, reads full N at `0x01BA29E0`, subtracts
C from 32 and logically shifts a copy of N right by CL. The effective
count k is (32 minus C) modulo 32. It tests the shifted result's low bit,
then reads current full mask M at `0x0075B204`, setting mask one for
bit one or clearing it through `0x004A1FB3` for bit zero. Both paths
join at `0x004A1836` before reading full V at `0x01BA29E8`.
V zero sets mask `0x40`; V nonzero clears it through `0x004A1FD6`.
Both rejoin at `0x004A1847`.

The branch replaces mask `0x80` from retained V's bit thirty-one,
masking it with `0x80000000` and logically shifting right 24. It
then XORs retained full V and N. If the XOR's sign bit is clear, it
clears mask `0x800` locally. If set, it transfers through
`0x004A2280` to `0x004A18D2`, which sets that mask. Thus mask
`0x800` represents differing retained N/V bit thirty-one values here,
without a one-count or exact-N gate. Both paths clear mask four and
transfer directly to `0x004A0FFD`.

FND-EXE-137 grounds that shared tail: freshly read V's low byte for the
zero-extended word lookup at `0x006F2B70` plus twice that byte, OR
its contribution into the computed mask, then freshly test byte C's
storage with `0x1F`. Low five bits zero clears mask `0x10`;
nonzero sets it. FND-EXE-133 records the physical shipped lookup
contributions; runtime writers remain unresolved. FND-EXE-132 grounds
the full mask publication, subsequent full selector clear, register
restoration and full published-mask return. No calls occur in these
bounded paths. Other M bits remain locally retained apart from the
stated replacements. The final count and lookup inputs are fresh reads,
not assumed equal to the initial C or retained V's low byte.

## Interpretation

Selector 36's complementary shift count, retained full-width sign gate and
fresh tail inputs are explicit. It differs from FND-EXE-139's decremented
count and one-count sign condition. Q-EXE-009 in FMT-EXE-006 remains open
for producers, runtime writers, other prefix branches and remaining callee
contracts. No complete helper reading, named operation model, atomicity
or actual PATH behavior is established.

## Alternatives

- C zero or 32 selects k zero; C one or 33 selects k 31. No local
  count rejection precedes the extraction.
- Differing N/V high bits set mask `0x800` regardless of C. Matching
  high bits clear it; V's own high bit independently controls mask `0x80`.
- The late mask `0x10` condition tests a fresh original count byte,
  not the complementary count or a retained decremented count.
- Initial C and the fresh count, and retained V versus the fresh lookup
  byte, remain separate reads under unresolved lifetime. No local call
  intervenes, which does not establish atomicity or table immutability.

## How to reproduce

Verify FND-EXE-011's executable identity, FND-EXE-099's physical controls
and FND-EXE-131's selector slot. Use the saved Ghidra program with
-noanalysis and ReportInstructionWindow.java at `0x004A180E` limit
32, `0x004A1FB3` limit three, `0x004A1FD6` limit two,
`0x004A2280` limit three and `0x004A18D2` limit four.
The initial set-tail query used limit three; limit four confirms its final
transfer. Exclude following unrelated paths and the analyzer's gap after
`0x004A2282`; that gap is not an absence claim. Use FND-EXE-137
for the shared tail and FND-EXE-132 for publication/return. Track full
subtraction before masked shift counts, retained N/V, alternate clears,
XOR sign flags and both direct joins. Check Alternatives' count/sign cases
as local arithmetic controls, not native inputs. No native or emulated
execution is part of this finding.