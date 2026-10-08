---
id: FND-EXE-140
title: Word prefix shift zero-extends its input and uses a fresh word count with a retained word-sign gate
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A12C4..0x004A131F
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A21FA..0x004A2202
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A1F30..0x004A1F46
tool: Ghidra 12.1.3 PUBLIC bounded zero-extended word prefix shift reading
environment: null
---

## Observation

FND-EXE-131 grounds selector 38 at `0x004A12C4`. It first
zero-extends word N at `0x01BA29E0`, then zero-extends byte C at
`0x01BA29E4`. A zero-extended copy of N is arithmetically shifted
right by the decremented count, whose effective k is (C minus one) modulo
32. Because the shifted full value has bit thirty-one clear, this arithmetic
shift fills with zero even when N's bit fifteen is set. It tests the result's
low bit, then reads full mask M at `0x0075B204`, setting mask one
for bit one or clearing it for bit zero. The alternate clear at
`0x004A1F38` rejoins at `0x004A12E9`.

It zero-extends word V at `0x01BA29E8`, setting mask `0x40`
for word V zero or clearing it through `0x004A1F30` for V nonzero.
Both join at `0x004A12FC`, where it freshly zero-extends word D
at C's storage. It replaces mask `0x80` from V's bit fifteen,
masking zero-extended V with `0x8000` and arithmetically shifting
right eight, yielding zero or `0x80`.

It masks a copy of D with `0x1F`, decrements it and tests for zero.
Unless D's low five bits equal one, it clears mask `0x800` through
`0x004A131F`. In the equal case it transfers to `0x004A21FA`,
tests retained BX as a word and jumps directly to `0x004A1A02`,
preserving the word test's sign flags. FND-EXE-139 grounds that shared
gate: negative sets mask `0x800`, nonnegative clears it. Thus this
branch sets that bit only when D's low five bits equal one and retained
N's bit fifteen is set. Its sign test consumes BX, not full zero-extended
EBX, which would always be nonnegative.

FND-EXE-139 also grounds the subsequent shared tail at `0x004A1326`:
freshly read V's low byte for the lookup at `0x006F2B70` plus twice
that byte, clear mask four, OR the zero-extended word contribution, then
test retained D's low byte with `0x1F`. Those flags cross the direct
transfer to the gate described in FND-EXE-137: low five bits zero clears
mask `0x10`, nonzero sets it, both reach publication. FND-EXE-133
grounds all physical shipped lookup contributions; runtime writers remain
unresolved. FND-EXE-132 grounds full mask publication, subsequent full
selector clear, register restoration and the full published-mask return.
No calls occur in this bounded path. Other M bits remain retained locally
apart from the stated replacements. Separate N/C/V/D/lookup reads do not
establish atomicity, producer bounds or runtime lifetime.

## Interpretation

Selector 38 mixes a byte shift count with a fresh word mask count and
retained word-sign testing. Its arithmetic shift does not sign-extend N.
Q-EXE-009 in FMT-EXE-006 remains open for count/field/mask/selector
producers, runtime writers, other branches and remaining callee effects.
This is no complete helper reading, named operation model or established
PATH behavior.

## Alternatives

- N `0x8000` shifted by k 16 through 31 gives extracted bit zero.
  Sign-extending N first, as selector 41 does in FND-EXE-137, would give one.
- C zero selects k 31, C one k zero, C 32 k 31 and C 33 k zero.
  No local count rejection precedes the shift.
- D low five bits one permits mask `0x800` only for N bit fifteen
  set. Upper D bits are read but do not affect either low-five-bit gate.
- The shared jump consumes flags from a word test; replacing it with a
  full-width sign test would incorrectly make the set arm unreachable.
- The later D and lookup byte are fresh reads. Their equality with initial
  C and retained V's low byte remains a lifetime question, not an assumption.

## How to reproduce

Verify FND-EXE-011's executable identity, FND-EXE-099's physical controls
and FND-EXE-131's selector slot. Use the saved Ghidra program with
-noanalysis and ReportInstructionWindow.java at `0x004A12C4` limit
30, `0x004A21FA` limit eight and `0x004A1F30` limit five.
Exclude following unrelated paths. Use FND-EXE-139 for the shared sign
gate at `0x004A1A02` and tail, FND-EXE-137 for the final count gate,
and FND-EXE-132 for publication/return. Track zero-extension, full shift
versus word sign test, masked decrement counts, alternate clears, fresh
word D, retained N/V, fresh lookup byte and flags across direct transfers.
Check Alternatives' controls as local arithmetic examples, not native
inputs. No native or emulated execution is part of this finding.

The location ranges use exclusive ends, including the final transfers
already described above. Verify them with additional windows at
`0x004A1319`, `0x004A21FD`, `0x004A1F41`, each with limit two;
the following instruction starts are their declared exclusive ends.
