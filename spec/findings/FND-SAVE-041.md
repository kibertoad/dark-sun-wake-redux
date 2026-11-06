---
id: FND-SAVE-041
title: A failed native Save leaves working metadata pending until ordinary Quit writes and clears it
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: failed_save.py and shutdown_run.py at 376dc27f; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic15000 cycles; SB16 base220 IRQ5 DMA1; no Ultrasound; muted host output
---

## Observation

This answers Q-SAVE-009's existential failed-Save continuation question
for RULE-SAVE-001. FND-SAVE-037/038 supply the static close/copy bypass,
return suffix and provider-dependent preparation alternatives. The
existing successful native Save control is FND-COMBAT-140. This observation
uses the same verified play-ready checkpoint, normal F1/Enter, one declared
private-filesystem failure input, then normal F3/Q. Neither stage seeds a
guest field. The cause of the native failure is controlled and explicit.

### Failed payload and return to play

On 2026-11-06 the first stage restored the checked play-ready state from
v181-q046-play-ready-20261104. Executable, mapping, state, emulated time
and frame guards passed. The ordinary working open/publication identified
C:\darkrun.GFF, options2, native handle10 and record8161:0004,
logical archive handle10, runtime flags0 and pending byte0. These two
handle domains are bound by the observed open/publication, not by their
coincident numeric values.

The first SAVE/1 payload requested11840 bytes from5415:000B at file
position959 through AH40 at3A98:0285. Immediately before that request,
the observer acquired a nonblocking host lock over bytes0..134217727 of
only this run's private drive-c/DARKRUN.GFF. The native return was AX0005
with carry set. The observer released the lock before continuing the game;
the file's length/hash were unchanged across the failed request.

A watch on record offset2C recorded exactly one value change in this
stage:0 to1, stopped at2DB6:0511. The resource writer subsequently
returned AXFFFF with archive error21 decimal and that record pending1.
The numbered writer's saved success byte was0 and the same record
remained pending1. Its native handle, options, runtime flags and logical
handle matched the published record.

The returning Save callback was reached at5FE8:0BF2. The unchanged-value
interval then reached the verified play input-map gate2F8B:097D and a
checked failed-map checkpoint. There was no later pending-byte change.
The working record remained8161:0004/handle10/pending1 even though the
active/head record was now8240:0004/handle11. Twenty-four observed
preparation entries belonged only to clean records6855:0004 and662E:0004;
none was for this working record. This directly observes the positive
case through the return/UI interval rather than inferring survival solely
from a partial caller, alias or provider census. It does not identify every
unchanged-value write or prove every service preserves unrelated fields.

All four monitored files retained their initial existence/length/hash;
SAVE02.SAV was absent. Sixty-one bounded observations and three recorded
RNG draws ended at the checked checkpoint. Both input deliveries completed.

### Same-record ordinary Quit

A second owned process restored that failed-map checkpoint. The restored
instruction/time, frame/state and all four private file states matched.
The fixed failed-save source guard admitted the released lock, native
failure, numbered failure, same record and missing numbered save. F3 and
Q both completed. Ordinary exit status and both flags were0. There were
no guest setup writes or RNG draws.

The bounded exit list contained the same pending record8161:0004,
options2, native handle10 and logical handle10. Its close-all preparation
then issued the following native operations:

| Operation | Result |
| --- | --- |
| AH40 handle10,28 bytes from8162:0004 | AX001C, carry clear |
| AH40 handle10,52 bytes from8164:0005 | AX0034, carry clear |
| AH68 handle10 | AX6834, carry clear immediately after the interrupt |
| Paired preparation return | AX0000, pending0, native handle still10 |

The source pointers alias record+10 and record+31. FND-COMBAT-124 supplies
the header/directory ordering and checks. The watch recorded exactly one
change,1 to0, stopped at2DB6:09BC after the successful native operations.
The five other preparations entered clean and returned AX0/pending0.
Forty-seven observations reached05BA:01A5 before executing AH4C termination.

DARKRUN.GFF grew from991 to1011 bytes. Its SHA-256 changed from
`b0f2d474a62a541c5adb05e272d25dba7a539cd86f4507b67879cd00faa6c195`
to `073ca2057fd24538a3cb2f25a11849c81ec577274d86e82c1f06c5cd889adacd`.
CHARSAVE.GFF and RGN032.GFF retained their lengths/hashes; SAVE02.SAV
remained absent. Both owned sessions closed and released their machine
run locks; neither emulator remained afterward.

## Interpretation

A native payload-write failure can leave game-produced pending working
metadata through normal Save return to play, and ordinary Quit can later
write that metadata and clear its flag. This answers Q-SAVE-009; no
separate proof of all possible aliases/providers is needed to establish
this observed positive case. It also supplies a positive working-file
case for Q-SAVE-007. Other shutdown histories, targets and mechanisms
remain there. This does not establish a valid new gameplay snapshot,
numbered save, physical durability, whole-entry complete reading or parity.

## Alternatives

An unconditional flush or nonreturning failure in the return suffix would
prevent this trace; neither occurred. Earlier seeded success in
FND-SAVE-039 is distinct: here the game produced the pending value after
a controlled filesystem failure. The watch and boundary bindings do not
prove continuous identity of every unobserved field, all original-DOS
failure behavior, the save-and-exit-mark variant, or all shutdown effects.
The existential question is answered by the ordinary Quit variant.

## How to reproduce

Use the pinned evidence Python and scripts at376dc27f:

1. Run `failed_save.py v181-q046-play-ready-20261104 <new-failure-run>`.
2. Run `shutdown_run.py <new-failure-run> <new-quit-run> failed-save`.

Retained first stage: GAME_DIR/captures/rust-dos-save-failure/
v181-q009-native-write-failure-c-20261106/failure.json and checked
states/failed-map. Retained second stage: GAME_DIR/captures/rust-dos-shutdown/
v181-q009-failed-save-quit-watch-20261106/shutdown.json. Each run also
retains session.json, emulator configuration/logs and its private drive.
No exported event/request journal is claimed. Require both passed flags,
no guest writes, matching checkpoint/restore guards, the two single-change
watch sequences, bound native results and file outcomes above. The earlier
unwatched runs v181-q009-native-write-failure-b-20261106 and
v181-q009-failed-save-quit-20261106 retain the same boundary outcomes.
Thirty-four synthetic controls validate tooling admission, not game behavior.
Keep all original-derived output in the approved local store.
