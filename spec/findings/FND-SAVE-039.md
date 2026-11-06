---
id: FND-SAVE-039
title: Controlled dirty Quit writes metadata and clears its pending byte after native success
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: shutdown_run.py and shutdown_dirty.py at d90c6c70; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic15000 cycles; SB16 base220 IRQ5 DMA1; no Ultrasound; muted host output
---

## Observation

This supersedes FND-SAVE-034 to correct the retained-artifact
description. Its measured observations remain unchanged. The retained
JSON reports and emulator logs exist; exported event/request journals
do not, and session.json records event_log as null.

This tests RULE-SAVE-001/Q-SAVE-007's conditional dirty-close outcome.
FND-SAVE-028 and FND-COMBAT-124 supply the preparation order;
FND-SAVE-031..033 observe only clean records. The fixed observer restored
the same approved saved-map checkpoint from
v181-q063-native-save-input-20261104 on 2026-11-06, verified its
executable, instruction/time, map fields and frame hash, then delivered
F3 and Q. Both input operations completed. Exit status and both flags
were0. No RNG draw or seed-setter hit occurred.

### Explicit controlled setup

The active archive equaled the linked head, record81CF:0004, with native
handle10, logical handle13, options000A and pending byte0. After the
validated entry snapshot, the session's named-field contract changed
`archive_pending_metadata` from0 to1 at81CF:0030, record offset2C.
The one-byte expected-old digest and readback checks passed. Both
shutdown.json and session.json record the verified write; no other field
was seeded. This is not evidence that ordinary Quit produced dirty state.

### Connected requests and result

Close-all reached preparation for that same record with handle10 and
pending byte1. The observer recorded the following native operations:

| Point | Observed result |
| --- | --- |
| AH40 at3A98:0285, handle10, count28, source81D0:0004 | AX001C, carry clear |
| Same AH40 site, handle10, count1248, source81D2:0005 | AX04E0, carry clear |
| AH68 at3A98:0305, handle10 | AX68E0, carry clear immediately after interrupt |
| Preparation's paired close-worker return2EB9:0324 | AX0000, pending byte0, handle still10 |

The source pointers alias record+10 and record+31 respectively. The
first count is the documented header size; the second is this directory's
transfer count. FND-COMBAT-124 supplies the checked seek/write ordering
and wrapper conversion. Raw AX68E0 is neither a byte count nor that
wrapper's normalized return. The four subsequent preparations entered
clean and returned AX0/dirty0 with handles9,8,7,6 unchanged.

Thirty-six bounded stops reached05BA:01A5 with AH4C, before executing
termination. The owned session closed; no rust-dos process or machine
run lock remained.

The private RGN032.GFF retained length64641 but its SHA-256 changed:

- Before: `6224aefe947062141f2102bfa1098d9e7fa0e1f6eab8d385eaf2a7d2a434d04e`.
- After: `a96124435660769d284a3802267485a698e0596b59fa126760e182f503e36cf4`.

DARKRUN.GFF, CHARSAVE.GFF and SAVE02.SAV retained their before/after
lengths and hashes from FND-SAVE-033. Only the restored private drive
was writable. This whole-interval comparison does not itself bind handle10
to a filename or exclude transient changes and other mechanisms.

## Interpretation

The controlled dirty path performs successful native metadata requests
and commit, returns success and clears its flag. That conditional outcome
is no longer unexercised. Ordinary producer reachability, failure cases,
durability, filename bindings and other callbacks remain open. This does
not establish a new gameplay snapshot, complete reading or parity, and
does not close Q-SAVE-007.

## Alternatives

The clean checkpoint alone skips these preparation requests; here the
pending byte was explicitly seeded and its dirty preparation entry observed.
A changed archive hash does not establish new gameplay data or identify
every mechanism that changed it. Native success in this emulated state
does not establish failures or ordinary dirty-state histories.

## How to reproduce

Use the pinned evidence Python and observer atd90c6c70:
`shutdown_run.py v181-q063-native-save-input-20261104 <new-run-name> dirty-active`.
Retained: GAME_DIR/captures/rust-dos-shutdown/
v181-quit-dirty-active-20261106/shutdown.json, with session.json,
rust-dos.conf, rust-dos.log, rust-dos.out and the private drive beside it. Require passed=true,
36 stops, two completed inputs, exactly one verified named setup write,
two full-count AH40 results, one carry-clear AH68 result and five paired
preparation returns. Compare the same-record dirty transition and all four
file states above. Existing20 synthetic controls validate the tooling,
not the game result. Keep all original-derived output local.
