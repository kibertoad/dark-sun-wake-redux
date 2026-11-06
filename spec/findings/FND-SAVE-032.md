---
id: FND-SAVE-032
title: One fixed Quit observes its type-three slot argument and return and all four stream counts at their actual use points
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: shutdown_run.py, shutdown_calls.py and shutdown_snapshot.py at 2c40b0ad; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic15000 cycles; SB16 base220 IRQ5 DMA1; no Ultrasound; muted host output
---

## Observation

On 2026-11-06 the fixed saved-map F3/Quit flow of FND-SAVE-031
was repeated with stops at actual slot-call/return and stream-use points.
The static attempt is FND-EXE-396/503/504/559 and FND-SAVE-030;
the earlier native run supplies the fixed-flow admission, not a substitute
for observing these later states. This run removes the entry-only limitation
for the named call and stream values in this one scenario.

### Admission and reached flow

The source was the same approved saved-map checkpoint in
v181-q063-native-save-input-20261104. The observer verified source,
checkpoint instruction/time, map fields and frame hash before sending input.
Both F3 and Q delivery operations reported done. It observed24 declared
stops through the ordinary zero-status exit and the pre-termination AH4C
stop at05BA:01A5, without executing that final interrupt. No RNG draw,
seed-setter hit or guest memory write occurred. The owned session closed,
its emulator was absent afterward and the machine lock was released.

### Actual indirect slot call and return

At modeled1425:16AB, immediately before the dispatcher reads its far
target, the observer required real mode, admitted equal DS/SS, SI below16
and BX equal to14*SI. It separately read the already-pushed argument
at SS:SP, the current table argument and the current target pair.

Exactly one slot-call stop occurred. SI was0, the pushed argument was1,
the current table argument was1, and the current target was09DF:0454,
corresponding to modeled1425:0454. FND-EXE-396 independently reads
that type-three cleanup interface. This is a use-point target observation,
not an inference from the earlier exit-entry table or the type-four producer.

The matching return stop at modeled1425:16AF occurred after this call
and before the dispatcher's outgoing-argument pop. AX was0001. The
following registers matched at the two stops:

| Register | Before call | At ordinary return |
| --- | --- | --- |
| DS | 4D9A | 4D9A |
| SS | 4D9A | 4D9A |
| SI | 0000 | 0000 |
| SP | FFB8 | FFB8 |

The return remained in real mode. These observations establish the
paired normal rejoin and these register values for that invocation. They
do not establish every register, unchanged slots between all instructions,
absence of temporary changes inside the callee, or the external meaning
of AX1. The observed target pair did not name the type-four007F
target; no callee-entry stop was taken to exclude changes after resume.

### Four stream selector and count-test observations

The observer stopped at modeled1000:3F1E before the fixed exit hook's
flags test, and at1000:2F30 before each selected flush's signed-count
comparison. Reads were limited to the four fixed records and required
the admitted real-mode DS/SS binding. Each record was observed once
at each boundary:

| Near record offset | Count at selector / count test | Flags at both | Handle byte at both |
| --- | --- | --- | --- |
| 3682 | 0 / 0 | 020D | 0 |
| 3692 | 0 / 0 | 0202 | 1 |
| 36A2 | 0 / 0 | 0202 | 2 |
| 36B2 | 0 / 0 | 0243 | 3 |

The recorded capacities, buffer/cursor words and self words also matched
between each pair. With the static2F13 branch of FND-EXE-503, these
observed nonnegative counts select its local no-negative-count-write
paths. The zero-count state is therefore observed at the comparison
boundary, rather than assumed to survive from exit entry. This does not
establish every asynchronous possibility or other stream inputs.

### Archive and file observations retained in the same run

All five actual08FB preparation stops again read dirty0, with the
same record/handle sequence as FND-SAVE-031. No hit occurred at the
four monitored write sites44DE:0264/0285,1000:3EFB or15F3:01A4.
Their native write-result capability remains unexercised.

All four fixed private files again had equal before/after existence,
length and SHA-256: DARKRUN.GFF and SAVE02.SAV were49739 bytes
with43098e9bc5d678212f81f9deb4970b7904424d49a33e6b9938ff4d9dbf95c80c;
CHARSAVE.GFF was11735 bytes with
173e2affe2048197be8dc2e221b0c4cca74ad39ebe1c6e53d795214fadaa9fc1;
RGN032.GFF was64641 bytes with
6224aefe947062141f2102bfa1098d9e7fa0e1f6eab8d385eaf2a7d2a434d04e.
The same transient-change, other-file, buffered-state and pre-termination
limits of FND-SAVE-031 apply. Entry counts do not bind every handle
to a filename or prove every possible callback target.

## Interpretation

For this fixed saved-map Quit, Q-SAVE-007 no longer lacks actual slot
argument/target values at the observed indirect call, its paired register
rejoin, or the four stream counts at their selector/flush test points.
Dirty archives, other initial states and targets, unmonitored mechanisms,
all external effects and continuous preservation remain outside this result.
RULE-SAVE-001 remains supported, with no whole queue closure or
complete-reading declaration.

## Alternatives

The call target cannot be substituted with the known type-four target;
the observed pair names0454. A paired equal register is not continuous
preservation inside the callee. Equal entry counts alone would not settle
the later branch; this run observes those later comparisons separately.
The clean run does not refute conditional dirty-close writes or prove
universal no-write behavior.

## How to reproduce

Use the pinned evidence Python and shutdown_run.py at2c40b0ad with
`v181-q063-native-save-input-20261104 <new-run-name>`. No guest seeding
or additional arguments are admitted. The accepted report is
GAME_DIR/captures/rust-dos-shutdown/v181-quit-use-points-20261106/shutdown.json,
with session.json and private files beside it. Require passed=true,
24 stops, two delivered inputs, one paired cleanup call, eight stream-use
observations, five clean preparations and zero monitored writes/draws/
guest writes, plus the four exact file comparisons.

Reuse FND-EXE-559's dispatcher reading and the bounded
shutdown-slot-call-boundary.txt report under
GAME_DIR/analysis/handler-census-20261106/. Distinguish the pushed
argument, current table word, far target and ordinary return before its pop.
Reuse FND-EXE-503 for the signed-count branch. The13 synthetic observer
controls verify tooling admission and mismatch reporting, not this native
result or game parity. Keep all original-derived reports local.
