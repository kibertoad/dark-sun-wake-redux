---
id: FND-SAVE-033
title: The fixed Quit cleanup transfer enters the observed type-three callee with the same argument and expected far-return frame
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: shutdown_run.py, shutdown_calls.py and shutdown_snapshot.py at 44944d99; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic15000 cycles; SB16 base220 IRQ5 DMA1; no Ultrasound; muted host output
---

## Observation

This answers the fixed-scenario callee-entry gap retained by FND-SAVE-032.
The contract brief for RULE-SAVE-001/Q-SAVE-007 asks whether the
observed type-three slot transfer actually enters that documented callee
with the pushed argument and ordinary return frame. FND-EXE-396/559
provide the existing caller/callee reading; no unrelated native helper or
original code was newly read for this attempt.

On 2026-11-06 the observer restored the approved saved-map from
v181-q063-native-save-input-20261104, verified its executable, instruction/
time, map fields and frame hash, then followed the same F3/Q flow.
Both input operations reported done. The ordinary exit frame had status0
and both flags0. Twenty-five bounded stops reached the pre-termination
AH4C stop at05BA:01A5, without executing that interrupt. No RNG draw,
seed-setter hit or guest memory write occurred. The owned session closed;
its emulator was absent afterward and the machine run lock was released.

### Call, entered callee and ordinary return

The observer recorded one slot call, one matching fixed callee entry and
the paired dispatcher return:

| Point | Observed state |
| --- | --- |
| Before indirect call, modeled1425:16AB | SI0, BX0; pushed argument1; current table argument1; target09DF:0454; SS:SP=4D9A:FFB8 |
| Entered callee09DF:0454 | SS:SP=4D9A:FFB4; far return09DF:16AF; incoming argument1 |
| Paired return09DF:16AF, before argument pop | AX0001; DS/SS4D9A, SI0 and SPFFB8, matching the observed pre-call values |

The original load segment was05BA, so09DF:0454 is the documented
modeled1425:0454 type-three cleanup entry. The entry stop required real
mode, the admitted DS/SS binding, SP exactly four bytes below the
pre-call pushed-argument position and the expected far return pair.
The entry argument equaled the word observed before the transfer.
The matching return remained in real mode.

This directly links the before-call pointer observation to the callee
actually entered and its normal dispatcher continuation. The earlier
possibility that the observed target pair changed after resume into a
different callee is not a remaining explanation for this recorded transfer.
No type-four target or arbitrary target breakpoint was admitted by this
fixed observer. The result does not establish every internal register,
continuous slot/frame preservation, or the external meaning of AX1.

### Connected controls in the same run

All four fixed stream records again had count0 at both their selector
and signed-count-test points: eight observations in all. All five actual
archive preparations read dirty0. No hit occurred at the four monitored
write sites recorded by FND-SAVE-031/032; no native write result was
therefore exercised.

DARKRUN.GFF, CHARSAVE.GFF, SAVE02.SAV and RGN032.GFF again
had equal before/after existence, lengths and SHA-256 hashes. Their
before observations also exactly matched FND-SAVE-032's recorded
before observations, including its four lengths and hashes. These controls
retain the same interval, other-file, transient-change, buffered-state and
pre-termination limits. They are not a global no-write proof.

## Interpretation

The contract's fixed callee-entry linkage is answered: this slot call enters
the documented type-three target with the same argument and expected
return frame, then rejoins normally with the recorded register values.
Q-SAVE-007 can remove that named remaining observation condition for
this scenario. Its whole question remains open for dirty/other initial
states, other targets, remaining mechanisms and external effects. No
whole-entry complete reading, queue closure or parity promotion follows.

## Alternatives

A target pair sampled before a call alone would not settle the entered
callee. This run observes that entry and its frame separately. An equal
entry argument does not prove that every internal use or native operation
preserves it. The clean saved-map controls cannot settle dirty shutdowns
or different cleanup targets.

## How to reproduce

Use the pinned evidence Python and shutdown_run.py at44944d99 with
`v181-q063-native-save-input-20261104 <new-run-name>`. Reuse the
documented checkpoint and no guest setup writes. The accepted report is
GAME_DIR/captures/rust-dos-shutdown/v181-quit-callee-entry-20261106/shutdown.json,
with session.json and private drive beside it. Require passed=true,
25 stops, two delivered inputs, one cleanup_calls record with its
callee_entry and result, matching argument1 and the exact frame/stack
relation above, eight zero stream counts, five clean preparations and
zero monitored writes/draws/guest writes. Compare all four file-state
records and require normal session cleanup. Existing15 synthetic observer
controls validate tooling admission, not this game observation. Keep all
original-derived reports local.
