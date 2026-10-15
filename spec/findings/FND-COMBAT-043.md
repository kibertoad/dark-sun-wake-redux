---
id: FND-COMBAT-043
title: The numbered restore caller postprocesses placed objects and can replace its saved reader result with archive cleanup
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070CEE..0x00070DE7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071A17..0x00071A87
tool: tools/research/exec-census/overlay_listing.py; installed-build fingerprint and overlay fixup checks
environment: null
---

## Observation

FND-COMBAT-035 identifies overlay 187 +0EBE, exported through
56EF:0089, as a caller of the numbered SAVE reader +0A6C.
The whole caller body ends at +0FB6. Following its earlier pathname
and file-copy requests, zero returned AL from 4544:0000 returns
AL zero immediately. A nonzero byte continues through window/setup
calls and the local helper +1BE7. Its returned AL is saved at BP-1.
Further setup is followed by +0A6C with selector word zero; returned
AL is saved separately at BP-2.

### The immediate continuation is not an effect-count validation loop

The caller does not test BP-2 before its next loop. It sets DX zero
and DS:13FA to one, then tests byte five of the eight-byte entry at
the far pointer DS:67B7 plus the low word of 8*DX. While bit three
is set, it reads that entry's signed word at six. For a nonnegative
word s, it uses low-word 37*s indexing into current DS:67BB:

- It writes FFFF to the indexed word at record offset 10 hex.
- It writes zero to the indexed double-word at offset 1D hex.
- If the indexed first byte has bit seven set, it clears DS:13FA.

The loop increments DX at word width and retests the next entry's
byte five. It does not compare this loop index with DS:264E or a
literal capacity. A negative entry word skips the record writes but
still advances to the next entry. The loop's termination and table
extents therefore require admitted restored state.

FND-PARTY-022 and FMT-REGION-006 supply the placed-object table
context. This caller's own instructions do not walk the ten-byte
effect rows, read or cap effect count DS:2100, or validate effect
IDs/owner ordering. This is a statement about this body, not about
unread child effects or every restoration path.

### Archive-helper state controls which result is returned

After the object loop, the caller makes a further window/setup call.
It then tests saved BP-1. Zero returns saved BP-2. Nonzero calls
local +1C26 and replaces BP-2 with that helper's returned AL before
returning it. Thus the earlier numbered-reader result is not combined
with the later helper result by an AND or preserved failure test.

Helper +1BE7 first tests DS:167A. Nonzero returns AL zero without
its open request. Zero requests opening the named working archive
through 56BD:002F, supplying DS:144E as the output-handle address.
A zero open-result byte calls overlay 180 +08A3. On the encoded
continuation it sets DS:167A to one and returns AL one. Diagnostic
termination and actual opening are separate dependencies; an encoded
continuation is not evidence that a failed open returns normally.

Helper +1C26 tests DS:167A. Zero returns AL one. Nonzero passes the
double-word at DS:144E to 38FF:02B5 and tests full AX. Zero clears
DS:167A and returns AL one. Nonzero calls overlay 180 +0867 and,
if that call returns, returns AL zero without clearing the flag.
This finding records the caller and helper branches, not a complete
close/flush backend contract or an independently verified handle.

Under admitted normal return, stable saved locals, a terminating object
loop and the required child effects, a zero numbered-reader byte can
therefore be replaced by cleanup's one when saved BP-1 is nonzero.
When BP-1 is zero, that reader byte remains the return source. This
conditional result propagation does not prove a visible failed-load
bug, successful recovery, valid effect state or rollback.

## Interpretation

This answers Q-COMBAT-009's immediate +0EBE continuation and local
result-propagation parts. The post-read object loop does not itself
provide the missing effect-count/ID/order validator, and a nonzero
wrapper result is not by itself proof that +0A6C returned nonzero.

Q-COMBAT-009 retains the other restore caller, complete validation
and writer coverage, admitted archive selection/destination backing,
intervening callbacks, native I/O and actual caller state. No original
game, emulated call, parser or gameplay implementation was run or added.

## Alternatives

- Treating every nonzero final byte as the numbered reader's success
  ignores the saved-open-state branch and replacement by +1C26.
- Treating the object loop as an effect-row bounds check confuses the
  eight-byte placed entries and 37-byte records with the effect table.
- Treating a failed reader as stopping all later caller work contradicts
  the unconditional post-read loop. Child failure/termination can still
  prevent any particular continuation from being reached.

## How to reproduce

Use the locked evidence environment and supported DSUN.EXE, size
634416 and XXH3 e296af55ba2ecde7e77f555c90f33d0b.
Read overlay_listing.py descriptor 187, 70CEE..70DE7 and
71A17..71A87. Bounds are hexadecimal and half-open. Follow BP-1
and BP-2 separately from their first assignments through the two
return sources. Read the loop's test-before-body byte flag, signed
object-word gate, wrapping indices and lack of a local count test.
Keep the reader contract in FND-COMBAT-035 and all child/native
dependencies distinct from these caller-side branches. No original
bytes or listings are committed.
