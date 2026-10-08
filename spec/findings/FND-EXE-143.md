---
id: FND-EXE-143
title: Word complementary-count prefix branch clears its extracted-bit mask above sixteen while retaining later sign comparisons
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1875..0x004A18D7
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1FE6..0x004A2018
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1FDE..0x004A1FE6
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A2226..0x004A2233
tool: Ghidra 12.1.3 PUBLIC bounded complementary word-count prefix branch reading
environment: null
---

## Observation

FND-EXE-131 grounds selector 35 at `0x004A1875`. It zero-extends
byte C at `0x01BA29E4` and compares DL unsigned with 16.
For C above 16 it zero-extends word N at `0x01BA29E0`, reads
current full mask M at `0x0075B204` and clears mask one without
shifting N. For C at or below 16, the alternate at `0x004A1FE6`
reads and zero-extends word N, subtracts C from 16 and arithmetically
shifts a zero-extended copy of N right by that count. The full shifted
value has its sign bit clear, so this is zero filling. The count is
locally bounded zero through 16. It tests the result's low bit, then
reads M and sets mask one for bit one or clears it through
`0x004A2226` for bit zero. Both cases join at `0x004A1894`.
C zero shifts by 16, giving extracted bit zero; C 16 selects bit zero
of N. The above-16 path is not a rejection or early return.

The join zero-extends word V at `0x01BA29E8`. V zero sets mask
`0x40`; V nonzero clears it through `0x004A1FDE`. Both paths
join at `0x004A18A7`, replacing mask `0x80` from retained V's
bit fifteen. The contribution masks zero-extended V with `0x8000`
and arithmetically shifts right eight, giving zero or `0x80`.
They XOR zero-extended retained N and V and logically shift right
fifteen. The result is zero or one, so its low-byte test sets mask
`0x800` exactly when N/V bit fifteen differ. A zero result takes
`0x004A2011`, transfers the current mask to the shared clear at
`0x004A1866`; a nonzero result sets it at `0x004A18D2`.
FND-EXE-142 grounds both shared paths' subsequent clear of mask four
and direct transfer to `0x004A0FFD`.

FND-EXE-137 grounds that tail: freshly read V's low byte for the
zero-extended word lookup at `0x006F2B70` plus twice that byte,
OR the contribution, then freshly test byte C's storage with `0x1F`.
Nonzero low five bits set mask `0x10`; zero clears it. FND-EXE-133
grounds physical shipped lookup contributions, with runtime writers
unresolved. FND-EXE-132 grounds full mask publication, subsequent full
selector clear, register restoration and full published-mask return.
No calls occur in these bounded paths. Other M bits remain retained
locally apart from the stated replacements. Separate N/V/C and tail
reads establish no atomicity, producer ranges or runtime lifetime.

## Interpretation

Selector 35 has an explicit byte-count boundary for mask one, while
later word tests remain active above that boundary. It differs from
FND-EXE-142's full-width modulo-count extraction. Q-EXE-009 in
FMT-EXE-006 remains open for producers, runtime writers, other prefix
branches and remaining callee effects. No complete helper reading,
named operation model or actual PATH behavior is established.

## Alternatives

- C zero yields extracted bit zero, C one selects N bit fifteen,
  C 16 selects N bit zero, and C 17 or 33 clears mask one without
  a shift. Applying modulo-32 extraction above 16 would change this result.
- C above 16 does not skip V's zero/sign tests or the retained N/V
  high-bit comparison. Mask `0x800` can still be set there.
- Upper N/V storage bytes do not affect these word tests. Zero-extension
  before the full arithmetic shift does not create word sign filling.
- The final count and lookup byte are fresh reads. The final mask
  `0x10` gate does not use the initial bounded complementary count.

## How to reproduce

Verify FND-EXE-011's executable identity, FND-EXE-099's physical controls
and FND-EXE-131's selector slot. Use the saved Ghidra program with
-noanalysis and ReportInstructionWindow.java at `0x004A1875` limit
26, `0x004A1FE6` limit fifteen, `0x004A1FDE` limit two and
`0x004A2226` limit three. Exclude following unrelated instructions.
Use FND-EXE-142 for shared mask set/clear and tail transfers,
FND-EXE-137 for fresh lookup/count gates, and FND-EXE-132 for return.
Track unsigned byte boundary, each N read, zero-extension, bounded
subtraction and shift, alternate mask clears, retained N/V XOR and
fresh tail reads. Check counts zero, one, 16, 17 and 33 and each word
high-bit combination as local arithmetic controls, not native inputs.
No native or emulated execution is part of this finding.

The location ranges use exclusive ends, including the final transfers
already described above. Verify the corrected boundaries with additional
windows at `0x004A2013`, `0x004A1FE1`, `0x004A222E`,
each with limit two; the following instruction starts are their exclusive ends.
The range ending at `0x004A18D7` delegates its remaining shared suffix
to FND-EXE-142 as described above; it does not truncate the final instruction
of the locally described portion.
