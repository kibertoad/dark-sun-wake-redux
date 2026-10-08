---
id: FND-EXE-162
title: Stored callback separates signed callee returns from the last-pair comparison
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F96E0..0x004F973F
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F9740..0x004F976B
tool: Ghidra 12.1.3 PUBLIC bounded stored-callback reading under Temurin 25.0.4.1
environment: null
---

## Observation

FND-EXE-161 grounds the full publication of exact target
`0x004F96E0` to `0x0075B0D0`. The target reserves twelve
bytes, reads full `0x0075B0E8` and adds it to the current full
`0x006F00A4` with wrapping thirty-two-bit arithmetic. It writes
full one to `0x0075B0E8` and calls `0x00463410`. No local
outgoing argument is written before that call. On its ordinary
return it retains full EAX in EDX, freshly reads full
`0x0075B0E8` and adds it again to current `0x006F00A4`.
The second addition does not assume the callee left one in the
source or preserved the first sum in the destination.

It tests the retained full return as signed. A negative value takes
`0x004F9753`. A positive value goes to `0x004F9739`, copies
that full value into EAX, releases twelve bytes and returns. Only
full zero reads full depth H from `0x01EDE478`. Zero H takes
`0x004F975F`. Nonzero H forms wrapped eight times H and reads
a full first field at that displacement plus `0x01EDE474`.
It compares that field with a freshly zero-extended word from
`0x0075B102`. A mismatch sets EDX zero and uses the common
return; it does not read the second field.

A first-field match reaches `0x004F9740`, reads full
`0x0075B200`, sets EDX to `0xFFFFFFFF`, and compares that
full value with the full second field at first-field address plus
four. Equality returns full `0xFFFFFFFF`; inequality replaces
EDX with zero and returns zero. Both comparisons are equality
at full width. The word load's zero extension is not a word-width
comparison against the stored first field. There is no local depth
increment or decrement, and no local prefix-field restoration.

For nonzero H, the wrapped first-field address is identical to
`0x01EDE47C` plus eight times wrapped (H minus one). This is
the preceding pair in FND-EXE-161's append layout when that
writer's incremented depth is still current. It is an arithmetic
relationship, not proof that the pair is initialized, in bounds or
unchanged by the intervening callees. The callback checks H only
for nonzero; it does not bound the multiplication or accesses.

The negative-result edge writes full `0x0072C058` to the first
outgoing slot and calls `0x0058F890`. The zero-depth edge writes
full `0x0072C085` there and calls the same target. If the first
call returns ordinarily to its next instruction, it proceeds into
the second edge and replaces the slot before the second call.
No local branch rejoins the normal return after the second call.
The report shows a gap from `0x004F976B` to the next decoded
entry at `0x004F9770`; this gap is not a proved return or a
termination mechanism. Diagnostic target termination, exception
behaviour and any continuation through those bytes are unresolved.
No message text is retained here.

There is no local ESP adjustment between reservation and the
common twelve-byte release. That statement alone does not prove
the callees' stack contracts. Both additions precede every local
return-value or depth decision after the first call's normal return.
The common return copies EDX, so positive values preserve the
callee's full return, whereas pair comparison constructs zero or
full minus one. Neither decision uses only AL. There are no local
saved prefix slots in this target; this does not rule out writes
through either callee, runtime aliases or other callback targets.

## Interpretation

The physically selected callback has distinct positive forwarding,
zero-result comparison and diagnostic edges. It does not uniformly
return a truthy equality result or uniformly forward its callee.
The last-pair relationship narrows FND-EXE-161's callback dependency,
but this is not a complete native callback or helper reading.
Q-EXE-009 remains open for caller admission, mutable depth/pair
writers, both callees' effects and exceptional/stack contracts.

## Alternatives

- Full callee result `0x00000100` is positive and returns unchanged;
  its zero AL does not admit the pair comparison. Full
  `0x80000000` takes the negative diagnostic edge.
- With full-zero callee result, a first field `0x00010000`
  mismatches a current zero word, despite equal low words.
- A first-field match and differing full second fields return zero;
  both matches return `0xFFFFFFFF`, not full one.
- H one selects base `0x01EDE47C`. H `0x20000000`
  wraps its displacement to zero and selects `0x01EDE474`;
  the nonzero-depth test does not reject this wrap.
- A callee changing `0x0075B0E8` to seven makes the second
  addition use seven. A callee replacing `0x006F00A4` makes
  the second addition start from that replacement, not the old sum.

## How to reproduce

Use FND-EXE-011's executable identity and FND-EXE-099's six physical
mapping controls. FND-EXE-161's exact stored-target publication is
the selection evidence. In the saved Ghidra program with -noanalysis,
read ReportInstructionWindow.java at `0x004F96E0` with limit 90,
excluding instructions at or beyond `0x004F976B`. Preserve the
one-byte gap before `0x004F9740` as a separate range. Track both
fresh counter additions, full signed result branches, zero-only
depth admission, wrapped preceding-pair relationship, the full-width
first equality against a zero-extended word, second-load admission,
full second equality and distinct return sources. Follow the
negative diagnostic call's ordinary continuation into the zero-depth
edge without assuming either diagnostic call returns. Check the
Alternatives as local arithmetic controls, not admitted native states.
No complete caller enumeration or negative reference search is claimed.
No native or emulated execution is part of this finding.
