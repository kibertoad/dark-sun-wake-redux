---
id: FND-SOUND-094
title: Known disc-initialization failures return a nonzero word despite a zero result byte
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:0185..4734:01D6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2660:01C4..2660:020B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2660:052D..2660:05CA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2660:0614..2660:066C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00050486..0x0005048A
tool: resident_listing.py at 5f7d7c32; pinned evidence Python and Capstone 5.0.9; Ghidra 12.1.3 focused decompiler/data reports; reused FND-SOUND-013/067/092/093 and FND-EXE-583
environment: null
---

## Observation

Q-CONFIG-014 retains the configuration callback before the paired-buffer
tests. This connected reading classifies its known producer, selection,
ordinary failure results and caller consumption. Installed DSUN.EXE has
634416 bytes and XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Addresses,
field offsets and result constants are hexadecimal.

### Source-null and known installed target are distinct routes

The initialized pointer DS:3486 is null. FND-SOUND-011/CONFIG-159 locate
the known overlay-180 call of 2660:01C4 on its nonzero DS:14E3 route.
That producer stores callback 2660:052D at DS:3486 and the three other
disc callbacks already recorded in FND-SOUND-013/EXE-583.

Its initial configuration-word calculation uses NEG, SBB AX,AX and INC,
producing zero or one; testing result bit 02 cannot select its skip branch
under an admitted unchanged instruction/flags continuation. This is the
existing logical-not-and-two observation, not a new global writer census.
Actual input-pointer access, caller admission and later pointer writers
remain required.

At 4734:0189 the library tests configuration bit 02; clear skips the
callback. Set followed by a null DS:3486 also skips it. A reached nonnull
call at 0199 is followed by OR AX,AX at 019D: exactly the full word zero
selects local error byte 1D and the initializer epilogue. Any nonzero
word joins 01A6. This caller does not test AL alone.

### Full ordinary-return classification of the known target

052D has the same ineffective logical-not-and-two gate, so its early
gate-result-zero return is not selected from its ordinary entry with
unchanged local execution. Its reached paths are:

| Path | Result and local publication |
| --- | --- |
| Presence helper 0614 returns AL zero | 054F branches to 0543, which writes only AL zero and returns with the helper's AH retained. |
| Presence succeeds, then 066C returns AL zero | Stores disc-enable byte 4E71:0C3F zero, then branches to 0543. The preceding MOV AX,4E71 makes the returned word 4E00. |
| Both helpers return AL nonzero and the following work returns normally | Iterates the track-position requests, publishes the lead-out dword to the table, sets AL one at 05C5 and returns. Whatever AH is, the returned word is nonzero. |

0614's ordinary presence-request path invokes the existing interrupt
wrapper, then stores returned fields through explicitly loaded ES=4E71.
The final MOV AX,4E71 at 0651 is not overwritten as a word afterward.
It sets AL one only when stored presence word 0C39 equals one; otherwise
it sets AL zero at 062C. Thus its ordinary failure word is 4E00, and
052D's first failure path also returns 4E00. These exact result words do
not depend on assuming that the external request succeeds. They require
normal child return, stable code and admitted frames/register continuation.

Consequently every ordinary returning path through the known 052D target
produces a nonzero AX: the two failure paths have AL zero but AX 4E00,
and the success continuation has AL one. Under that target and continuation
contract, the caller's exact-zero error successor at 01A1 is not selected.
A different callback target, altered return/frame, independent entry or
nonlocal rejoin remains outside this exclusion. Nonreturning device/child
paths are not silently treated as successful callback returns.

### State effects remain separate from result acceptance

On presence failure, 052D does not locally clear an earlier 0C3F; the
enable-byte one store comes only after presence succeeds. After that
store, a failed 066C explicitly clears it. This distinguishes retained
state from the result byte and from the caller's acceptance of the word.

The success continuation reloads first/last track bytes while iterating
06F8 and finally stores a dword at DS:3482 plus seven times last-track+one
plus two. FND-SOUND-067 already reads that helper's conditional allocation,
selected entry writes, request/response and external dependencies. Actual
table extent, response initialization, pointer/frame aliases and child
effects remain before claiming startup gate/slot preservation or successful
device output. Passing the caller's nonzero-word test establishes none of
those properties. Other startup gates remain FND-SOUND-092/093.

## Interpretation

Q-CONFIG-014 can remove unread known configuration-callback selection and
ordinary return-result propagation. It can also exclude the exact-zero
error successor for that admitted target without proving device success.
Retain actual pointer/target/DS histories, device/heap/table backing and
the named state-write effects; the no-call paths require none of that
callback's execution effects.

This is a local result-width contract, not an observed native bug, a
universal initialization-success claim, a queue closure or a complete
reading. No original process or emulated function was run.

## Alternatives

- AL zero necessarily aborts library startup: the caller tests AX, and
  the known ordinary failure word is 4E00.
- A nonzero callback word proves disc readiness: a failed presence or
  track request can return that word.
- Every failure clears the disc-enable byte: the first failure bypasses
  its store/clear pair and can retain earlier state.
- Configured disc mode always invokes this callback: both its flag and
  its current nonnull pointer are required at the caller.

## How to reproduce

Use resident_listing.py for the four code ranges above. Verify the
caller's word test, the target's byte tests and partial AL assignments,
0614's final AX load and the nonzero-success assignment. Reuse the
existing interrupt, request and track-position findings; do not infer
hardware outcomes from those instructions.

In the existing isolated resident Ghidra project, read-only without
analysis, decompile 2660:052D, 0614 and 01C4 and inspect initialized
57E0:3486, four bytes. Retained reports under
GAME_DIR/analysis/disc-init-contract-20261107 are disc-init.log and
width-and-selection-controls.txt. Decompiled register-block symbols are
not source field identities; the explicit instruction widths decide this
result contract. No original-derived code or data is committed.
