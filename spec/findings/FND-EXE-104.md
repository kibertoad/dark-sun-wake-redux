---
id: FND-EXE-104
title: Wait admission retains distinct sum exits and publishes callback-list removal before free-list insertion
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F26E0..0x004F2843
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F2110..0x004F214D
tool: Ghidra 12.1.3 PUBLIC bounded floating-branch, callback-order and return reading
environment: null
---

## Observation

FND-EXE-103 identifies `0x004F26E0` as the wait target's admission helper
and records its AL consumer. Its entry reads full words `0x0075B0E8`
and `0x006F00A4`, adds them modulo thirty-two bits and tests the result
signed. A nonpositive sum writes that sum to `0x006F00A4`, full zero to
`0x0075B0E8`, and returns full zero. That route does not enter the later
record callback or optional downstream helper.

A positive sum retains full `0x006F00A0` minus sum modulo thirty-two
bits in ESI and selects the current list head at `0x01D291E4`. With no
head, it clears byte `0x01D292D0` and proceeds with the initial sum as
retained budget B. With a head, the first floating comparison uses signed
integer conversions, the head's single-precision field zero and a local
single-precision store at stack offset four. The comparison status is transferred
to integer flags. Carry set, including the unordered flag combination, selects
a floating pop then the same cleared-byte continuation, retaining the initial
sum. Carry clear enters callback processing. Exact numeric meaning of the
floating stack and exceptional inputs remains separate from these observed
flag branches; no finite-value or floating-environment assumption is established.

Before the first record callback, processing publishes the positive sum to
`0x006F00A4`, full zero to `0x0075B0E8`, and byte one to
`0x01D292D0`. At each record-processing iteration it stores the current
floating top to single-precision `0x01D292C0` with a pop. It reads the
selected node's full link at offset twelve and full argument at offset four,
publishes that link as the new active head `0x01D291E4`, then calls the
full target stored at node offset eight with the argument in its first outgoing
slot. Head publication precedes callback invocation; no rollback or callback-
result test intervenes. Node validity, target validity and object lifetime are
not locally established.

After normal callback return, it freshly reads free-list head `0x01D291E0`,
writes that word to the retained processed node at offset twelve, freshly
reads active head `0x01D291E4`, then publishes the processed node as free
head. Thus the next selected node comes from post-call active storage, not
necessarily the original node's saved link, and free insertion uses post-call
free storage. Aliases and callback modifications are unresolved. The next
iteration's comparison freshly converts `0x006F00A0` and reads the new
head's single-precision field, transfers floating status to integer flags,
and repeats record processing on below-or-equal flags, including unordered.
A missing head or failed repeat comparison leaves that loop and freshly reads
`0x006F00A4` as budget B before the cleared-byte continuation. No local
list-cycle or iteration bound proves termination.

The common continuation clears byte `0x01D292D0`. A null retained head
publishes budget B directly to `0x0075B0E8`. A nonnull retained head
computes a floating difference from freshly converted `0x006F00A0`, its
single-precision field and the earlier retained integer difference in ESI.
It saves the current x87 control word in a local two-byte slot, creates another
word by OR with mask `0x0C00`, loads that modified word for one conversion
to a signed full integer local, then reloads the saved control word. No
exception/precision contract or input-range check is proved here; the returned
integer's value cannot be replaced by an ideal real-arithmetic formula.

A converted integer zero is explicitly replaced by full one before a signed
comparison with retained budget B. An integer signed less than B is published
to `0x0075B0E8`; otherwise B is published there. This does not separately
reject a negative converted integer or establish a nonnegative clamp. The
common tail then freshly reloads `0x0075B0E8`, reads full
`0x01D271C8`, subtracts the reloaded value from retained B modulo
thirty-two bits, and publishes that difference to `0x006F00A4`.
Freshly loaded nonzero `0x01D271C8` selects a call to `0x004F2110`.
After its normal return, or without that call, the helper explicitly returns
full one. The optional callee's EAX is discarded; its effects and exceptional
completion remain conditional. Earlier callback effects are not undone.

The optional callee's directly read prefix checks mask two in byte
`0x0075B205`, then freshly checks full `0x01D271C8`, then compares
full callback slot `0x0075B0D0` with `0x004A06D0`. An absent byte
mask, zero word or equal callback pointer branches to its common exit at
`0x004F2230`. Otherwise it reads the sixteen-bit word at `0x01D271C0`,
zero-extends it and compares its low word with 255. Further dispatch lies
outside this prefix finding. These guards can suppress work even after the
admission helper's earlier nonzero-word test; they do not alter its explicit
one return after normal optional completion.

## Interpretation

Admission has a direct zero-return path and a conditionally completing one-
return path. The latter can publish list removal, invoke callbacks, insert into
a separately reloaded free list, update budget storage and invoke more work
before returning. This is not an effect-free availability predicate. Callback
mutation, floating inputs/environment, indirect targets and optional dispatch
remain Q-EXE-009 in FMT-EXE-006; no native scheduling or termination claim
is established.

## Alternatives

- The initial signed nonpositive gate concerns the wrapped full sum, not
  each input independently. Its zero return still follows two global stores.
- The active head is published before the callback and the free insertion
  occurs after it. Reordering these effects changes what a callback may see.
- The next active head and free head are fresh reads after the callback;
  following only the original node link misses mutable list selection.
- Floating unordered status takes the carry/below-or-equal branches here.
  A finite-only or ideal-arithmetic reading needs independent evidence.
- Replacing a converted zero with one does not make every converted value
  positive, and the following minimum-like selection is signed.
- The initial sum and post-callback budget reload have different provenance.
  One unconditional fresh read would not describe both routes.
- Optional helper results do not become admission results: full one is
  assigned after normal return. That assignment does not undo optional effects.

## How to reproduce

Recheck FND-EXE-011's identity and FND-EXE-099's physical target controls.
Use the saved Ghidra program with -noanalysis. Read `0x004F2794` limit
100, `0x004F26E0` limit 140 and `0x004F2110` limit 85. Restrict
observations to the locations above; the optional helper's later body and
following unrelated functions are not claimed complete. FND-EXE-103 grounds
the actual admission call and caller width. No negative search is made.

Track full signed sum, ESI difference, retained versus fresh budget, list
publication before the callback, and fresh list loads afterwards. Keep x87
status-to-flags branches and control-word save/modify/restore explicit; retain
numeric and exceptional uncertainties rather than infer float behavior from
integer tests. Follow converted zero replacement and signed budget comparison
separately. Trace optional guards and the caller's explicit one assignment.
Keep full reports and identity controls in GAME_DIR/analysis/exe-batches and
commit no original bytes, code listings, resources or database. Run neither
the interpreter nor the game.