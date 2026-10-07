---
id: FND-EXE-111
title: Callback value one captures the indexed byte before record progress and reinserts after gated calls
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4800..0x005A48D7
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4AAD..0x005A4AF2
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4B03..0x005A4B16
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4B38..0x005A4B44
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4B9C..0x005A4BA6
tool: Ghidra 12.1.3 PUBLIC bounded value-one capture, progress and reinsertion reading
environment: null
---

## Observation

FND-EXE-109 records dispatch to `0x005A4800` for word one with retained
object O. This branch first compares byte O plus `0x0165` with zero,
reads full record pointer R from O plus `0x0150`, reads R's full base at
zero and index at twelve, then reads the byte at base plus index and
publishes it to O plus `0x0149`. These moves preserve the initial compare
flags: the branch on O's earlier `0x0165` test occurs after that capture.
There is no preceding local bound or null guard for the record/base/index.

An initially zero `0x0165` goes directly to the full R plus sixteen test.
Otherwise the path clears that byte, reads R plus sixteen as count C and
leaves count/index untouched when C is zero. For nonzero C it decrements
and publishes C modulo thirty-two bits. If the decremented count remains
nonzero it increments and publishes R plus twelve modulo thirty-two bits;
if it becomes zero it reloads the existing index without incrementing.
Both paths then compare the selected index with full R plus eight unsigned.
An index at least that field subtracts the field once and publishes the
result; an index below it leaves the existing value. This is not a proven
modulo reduction: no nonzero-size or less-than-twice-size admission is shown.
The byte already captured is not reread after these updates.

It then reads the current full count at R plus sixteen. Nonzero skips the
flag/state path and goes directly to reinsertion. Zero zero-extends O's
bytes `0x0118` and `0x011C`, ORs the former with mask two, computes
intersection M, and publishes the modified `0x0118` byte. It publishes
byte `0x011E` with ordered priority: mask four in M gives six; otherwise
mask sixteen gives twelve; otherwise mask one gives four; otherwise mask
two gives two; otherwise mask eight absent gives one and present gives zero.
Bits outside these masks still participate in M's full zero test.

M nonzero checks byte `0x011D`. Already nonzero goes to reinsertion;
zero tests `0x0123`, publishes one to `0x011D`, and a nonzero gate
calls `0x004F2060` with current full O plus `0x0110`. M zero instead
checks `0x011D`; already zero goes to reinsertion, otherwise tests
`0x0123`, clears `0x011D`, and a nonzero gate calls `0x004F20E0`
with current full `0x0110`. A zero gate suppresses the call but does not
cancel the state write. These are ordinary calls, unlike value seven's
tail transfers in FND-EXE-110. Neither result is tested; after normal return
the path continues to reinsertion.

At reinsertion it freshly reads full O plus `0x0108` and `0x010C`,
then calls `0x004F2490` with fixed target `0x005A4BB0`, the first
field's raw bits as its float second argument, and the second field as its
full callback argument. It restores the local frame and returns without
normalizing the insertion result. Fresh reads follow any gated call, so
those call effects cannot be replaced with an earlier snapshot. FND-EXE-105
shows insertion can silently do nothing when no free node exists; this
branch does not test a failure result or locally retry. Exceptional and
nonreturning callees do not prove reinsertion occurs.

## Interpretation

Value one captures before progress, subtracts a size field at most once,
and reaches reinsertion after optional state-changing calls on ordinary
paths. Storage validity, record/object aliases, field meanings, producer
bounds and callee effects remain Q-EXE-009 in FMT-EXE-006, together with
the separate zero branch. No complete reading or native PATH outcome follows.

## Alternatives

- Clearing the one-shot byte does not precede the captured-byte read.
- Count becoming zero does not increment the index on this path.
- One subtraction is not general modulo; size zero is not locally rejected.
- Reinsertion arguments are freshly read after gated calls, whose results
  are ignored rather than used as success predicates.
- Reinsertion is not guaranteed to create an active node when the free list
  is empty, nor is it guaranteed after an exceptional callee exit.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-099's physical controls.
Use the saved Ghidra program with -noanalysis and
ReportInstructionWindow.java at `0x005A47C0` limit 110 and
`0x005A4970` limit 150. FND-EXE-109 independently supplies dispatch and
frame context. Restrict observations to the declared ranges, following
priority paths at `0x005A4AE1`, `0x005A4B03`, `0x005A4B38` and
`0x005A4B9C`, count progress at `0x005A4B0F`, and the shared return
at `0x005A48A0`. Track flags surviving moves, retained record versus fresh
object reads, full decrement/index widths, single subtraction, call ordering
and reinsertion inputs. Keep reports in GAME_DIR/analysis/exe-batches; commit
no original listings or bytes and execute neither interpreter nor game.