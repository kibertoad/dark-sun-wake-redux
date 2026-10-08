---
id: FND-EXE-165
title: Stored callback builds a nested saved-state record and returns separately saved early-exit statuses after cleanup
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F50A0..0x005F5156
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5322..0x005F533F
tool: Ghidra 12.1.3 PUBLIC, bounded instruction and function reporters
environment: null
---

## Observation

FND-EXE-050 records construction callers storing `0x005F50A0` as a local
record target. This callback begins with its own conventional frame and
saved-register prologue, reserves 172 local stack bytes, then constructs a
nested record at frame offset minus 108 before calling FND-EXE-045's setup
helper with that address.

The direct pre-setup writers relative to this nested record are:

| Record offset | Prepared 32-bit value |
|---|---|
| 24 | Callback entry `0x005F50A0` |
| 28 | `0x006EE824`, whose contents and consumer remain unread |
| 32 | Address of this frame's offset-minus-24 local |
| 36 | Stored handler entry `0x005F5562`, unread here |
| 40 | Stack pointer after the local reservation, before outgoing setup space |

These are local writers, not a complete record layout. The first record word
is not directly initialized by this prefix before setup; setup supplies its
mode-dependent link as described by FND-EXE-045. The offset-32, offset-36 and
offset-40 writers have the same relative offsets consumed by FND-EXE-052,
but this does not prove that its selector chooses this particular nested record.
The frame address is an adjusted local address, not this callback's unadjusted
frame pointer.

On normal setup return the callback immediately loads its fifth original
32-bit stack argument into registers, replacing setup's return register
without testing it. It computes fifth-argument minus 48 and plus 32 at
32-bit width and saves both derived values in locals. Their provenance does
not establish that either addresses a particular outer record or valid storage.
It initializes a separate local return-status word to three.

A first original argument different from one jumps to the cited cleanup
join with that status three. For a first argument of one, the prefix tests
whether the second argument is exactly six and whether the third and fourth
arguments equal `0x432B2B00` and `0x474E5543` respectively. Equality of all
three selects a later path at `0x005F5268`, outside this finding's claims.
These are full-word equality tests; no semantic name for that signature is
inferred here.

Otherwise it reads the sixth original argument, prepares it as the first
outgoing word after reserving twelve outgoing stack bytes, writes all ones
at its nested record offset four, then calls `0x00600A80`. That helper's
input consumption and effects remain unread. After normal return it removes
sixteen outgoing bytes, sets the saved return status to eight, saves the
helper's full returned word in a separate local, and tests it for zero.
Zero reaches the cited cleanup join; nonzero enters later matching work
outside this bounded finding. No semantic success claim follows from truthiness.

At the cleanup join it passes its nested record address to FND-EXE-049's
cleanup helper. After normal return it reloads the saved status into the
return register, adjusts the outgoing stack and restores its original frame
and saved registers before returning. Thus these bounded early exits return
three or eight rather than cleanup's returned word, conditional on unread
callees preserving the saved local status and completing normally. The local
body does not test cleanup's result before overwriting it.

## Interpretation

This callback supplies concrete nested-record saved-state writers and two
bounded early-exit paths. It also distinguishes separate return-status
preservation from FND-EXE-050's callers that leave cleanup's return register
unchanged. FND-EXE-053 treats callback result eight as an advance path and
other results apart from seven as two, while FND-EXE-054's second callback
has its own result gates. Whether those consumers invoke this target on a
particular record still requires selection and target provenance.
Q-EXE-009 retains the later signature/matching paths, helper input and result
contract, stored handler, selected-record identity, argument admission,
remaining field writers and exceptional effects. No complete callback or
record lifecycle is established.

## Alternatives

This replaces FND-EXE-055, whose two half-open locations omitted the last
byte of the conditional jump at the prefix end and the cleanup RET. The
prefix ends exclusively at `0x005F5156`, after the six-byte conditional jump
starting at `0x005F5150`; cleanup ends exclusively at `0x005F533F`, after the
one-byte return at `0x005F533E`. The previous bounded observations remain
supported by these corrected complete instruction spans. This correction
adds no full-callback, caller-completeness or valid-storage claim.


Treating this callback as a leaf that has no nested record, storing its
unadjusted frame pointer at record offset 32, using setup's return to gate
these argument reads, or returning cleanup's result on these early exits
are ruled out by the cited instructions. Assuming a zero helper result
returns zero is also ruled out: the saved local status is eight. Fifth-argument
arithmetic alone does not identify an outer record or validate its address.
The later signature-selected path remains separate, not a guessed equivalent
of either early exit.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x005F50A0`,
read ninety instructions there and twelve from `0x005F5322`.
Check both final instructions through their last byte and treat range ends as exclusive. Restrict claims to the two cited ranges; exclude later matching, publication
and handler code even where the windows print it. Follow pre-setup stack
capture, nested-record-relative writers, original argument slots, separate
status and helper-result locals, exact signature guard, both early joins,
cleanup argument and the status reload after cleanup. Use FND-EXE-045 and
FND-EXE-049 for local record helpers and FND-EXE-052 through FND-EXE-054
for consumers without assuming selected-record identity. Keep callee, alias
and exceptional effects conditional. Keep rich reports local and execute no
interpreter or game.
