---
id: FND-SAVE-031
title: One fixed saved-map Quit closes five clean archives with no monitored write requests and unchanged hashes for four private files
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: shutdown_run.py and shutdown_snapshot.py at c06a23f9; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic15000 cycles; SB16 base220 IRQ5 DMA1; no Ultrasound; muted host output
---

## Observation

On 2026-11-06 the fixed observer restored the approved saved-map from
v181-q063-native-save-input-20261104 and followed F3 then Q. The
static attempt is FND-SAVE-027..030 and the connected slot evidence
in FND-EXE-396/398/558/559. This run observes one clean saved-map
state, not the dirty-archive or every-callback cases in Q-SAVE-007.

### Admission and native flow

The observer verified checkpoint executable identity, current-pin state
admission, restored instruction/time, the recorded map fields and frame
hash. It used one package-owned emulator and private drive with the
normal run lock, default muted host audio and no guest setup writes.
Both input operations reported done. The observed choice window was
14000, and the ordinary exit-entry frame had status0 and both flags0.
Expected equal DS/SS and the03DF near return were checked by the
snapshot helper. The original's load segment was05BA.

Fourteen bounded stops reached the pre-termination interrupt at
05BA:01A5 with AH4C. The observer did not execute that termination
request. The initial RNG state was1913940318; no generator draws or
seed-setter hits occurred, and the report's guest-write list is empty.
The session closed afterward; the owned emulator was absent and its
machine lock released. This proves no gameplay replay or native exit
beyond that final stopped request.

### Archive state at entry and preparation

At exit entry the archive count was5, the active pointer was also the
head, and the bounded forward list contained these records:

| Record pointer | Native I/O handle | Logical handle | Options | Dirty byte |
| --- | --- | --- | --- | --- |
| 81CF:0004 | 10 | 13 | 10 | 0 |
| 6855:0004 | 9 | 4 | 0 | 0 |
| 6659:0004 | 8 | 3 | 0 | 0 |
| 664B:0004 | 7 | 2 | 0 | 0 |
| 662E:0004 | 6 | 1 | 0 | 0 |

These are live field observations, not file-name bindings or an assumption
that all archives in other states are clean. The subsequent close-all
entry had argumentFFFFFFFF and return5F31:082E, matching the
known shutdown call. Its recursive ordinary closes used logical handles
13,4,3,2,1 with return2EB9:02EB. Each08FB preparation stop returned
to2EB9:0324, named the corresponding table record and reread dirty0
with native handle10,9,8,7,6 respectively. Thus clean state was observed
at actual preparation, not inferred only from the earlier entry snapshot.

The four monitored write-interrupt sites were44DE:0264/0285,
1000:3EFB and15F3:01A4 under the verified relocation. None hit after
exit entry. No native write result is recorded because none of these
requests was reached. This is not a census of all possible write service
encodings, unmonitored paths or callbacks.

### Other entry-state bindings

The callback table held4C6C:0025 and0C35:0029, the relocated known
overlay shutdown and library callbacks. The stored stream hook was
05BA:3F14. All four fixed stream records had signed count0 at entry;
their handle bytes were0,1,2,3. The entry snapshot does not establish
their state at every subsequent callback or flush call.

Slot gate00C4 was1. Only slot0 had a nonzero argument:1, paired
with09DF:0454. This corresponds to modeled1425:0454, the type-three
cleanup target already read in FND-EXE-396. It is not the type-four
007F zero-count file-write target of FND-EXE-398. The other fifteen
argument words were zero at entry. This removes the unknown entry-target
value for this run, but the observer did not break at each indirect slot
call or prove that arguments/targets remained unchanged through callbacks.

### Four monitored private files

The private-drive before/after observations were identical in existence,
length and SHA-256 for all four fixed files:

| File | Bytes | SHA-256 before and after |
| --- | --- | --- |
| DARKRUN.GFF | 49739 | 43098e9bc5d678212f81f9deb4970b7904424d49a33e6b9938ff4d9dbf95c80c |
| SAVE02.SAV | 49739 | 43098e9bc5d678212f81f9deb4970b7904424d49a33e6b9938ff4d9dbf95c80c |
| CHARSAVE.GFF | 11735 | 173e2affe2048197be8dc2e221b0c4cca74ad39ebe1c6e53d795214fadaa9fc1 |
| RGN032.GFF | 64641 | 6224aefe947062141f2102bfa1098d9e7fa0e1f6eab8d385eaf2a7d2a434d04e |

The interval is from restored saved-map to the stopped termination request.
Equal final hashes do not exclude transient changes restored before the
second read, writes to other files, unobserved buffered state, or different
results on a different initial state. No mapping from the archive table's
handles to these filenames is inferred from matching order or count.

## Interpretation

This admits the fixed F3/Quit flow and named shutdown observations under
the recorded pins. It supplies concrete clean-archive and entry-slot state
for one run and agrees with the known no-payload-write branches under
those observations. It does not settle dirty shutdowns, actual slot-call
preservation, all file effects or the whole Quit contract. RULE-SAVE-001
remains supported; Q-SAVE-007 remains open and no complete reading is
declared.

## Alternatives

The clean run cannot refute FND-SAVE-028's conditional dirty flush.
No monitored write hit is not a global negative census. A nonzero slot
gate does not imply the type-four file target was selected. Equal file
hashes do not establish absence of all intermediate or external effects.

## How to reproduce

Use the pinned evidence Python and shutdown_run.py at c06a23f9:
`shutdown_run.py v181-q063-native-save-input-20261104 <new-run-name>`.
The package checks the native saved-map state and source; use no memory
seeding or new arguments. The accepted report is
GAME_DIR/captures/rust-dos-shutdown/v181-quit-event-poll-20261106/shutdown.json,
with session.json and the retained private drive beside it. Require
passed=true, both delivered inputs,14 stops, admitted ordinary entry,
five clean preparation observations, zero recorded write requests/draws/
guest writes and the four exact file comparisons. The package must close.

The earlier v181-quit-entry-20261106 attempt used the builder poll and
ended at its bounded observation limit without an exit snapshot. It is a
retained tooling diagnostic, not a second accepted Quit observation.
All original-derived reports, frame hashes and private files remain local.
