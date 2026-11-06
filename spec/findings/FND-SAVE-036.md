---
id: FND-SAVE-036
title: The observed type-three Quit cleanup enters the queried XMS driver with free-handle request one and reports success
status: superseded
builds: [BLD-GOG-EN-1.1]
superseded_by: [FND-SAVE-040]
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: shutdown_run.py and shutdown_xms.py at 79d51762; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic15000 cycles; SB16 base220 IRQ5 DMA1; no Ultrasound; muted host output
---

## Observation

This answers the fixed-scenario native-service linkage left by
FND-SAVE-033. FND-EXE-394/396/581/584 already identify the installation
probe, entry-pointer publication and type-three cleanup's free request.
SRC-RUSTDOS-181 identifies the chosen emulator source's matching XMS
entry and function semantics. This run observes the live query, call,
entered target and return separately; it does not identify the target
merely from a pointer sampled before resuming.

On 2026-11-06 the observer restored the same verified saved-map checkpoint
from v181-q063-native-save-input-20261104 and followed the existing
F3/Q flow. Both inputs completed. Exit status and both flags were0.
No guest setup write, RNG draw or seed-setter hit occurred. Thirty-two
bounded stops reached the pre-termination AH4C stop at05BA:01A5.

### Query, call, entered service and return

The already observed outer type-three cleanup entered09DF:0454 with
argument1 and its ordinary far-return frame. The inner15F3 helper's
runtime segment was0BAD, with SS4D9A and BPFFAA. Its saved return
pair named09DF:045F; its incoming word equaled the outer argument.

| Point | Observed state |
| --- | --- |
| Installation request at modeled15F3:00CC | AX4300, followed by returned AL80 |
| Entry request at modeled15F3:00D8 | AX4310, followed by ES:BX=0160:1F00 |
| Free call at modeled15F3:0629 | AX0A01, DX0001; CS-relative pointer0160:1F00 equals the queried pointer |
| Native entry0160:1F00 | AX0A01, DX0001, SS:SP=4D9A:FFA2; far return0BAD:062E |
| Paired return at modeled15F3:062E | AX0001, BX0000, DX0001; preserved helper frame |

The observer admitted only the pinned emulator's fixed native entry,
real mode and the expected caller/frame; it did not follow arbitrary
returned pointers. Native entry SP was four bytes below the pre-call
SPFFA6. The report separately records matching argument values at the
outer entry, helper frame, free call and native entry.

FND-EXE-396's wrapper returns the native AX without checking it. The
outer dispatcher return again had AX1 and the same observed DS, SS,
SI and SP as before the slot call. Thus the earlier AX1 is now linked
to this service result rather than an unspecified internal helper.

### API meaning and connected controls

SRC-RUSTDOS-181's XMS source selects its function from AH. Function0A
takes a memory-block handle from DX, rejects an invalid or locked block,
and reports AX1 after removing a valid unlocked block from its records.
This identifies the request and its reported success. The word1 here is
an XMS memory handle; it is not thereby a DOS file handle or numeric
archive handle. The source snapshot and release pin remain distinct
identities; no independent source-to-binary build match is claimed.

All four streams had count0 at both observed use points. Five archive
preparations entered dirty0. No monitored native write request hit.
DARKRUN.GFF, CHARSAVE.GFF, SAVE02.SAV and RGN032.GFF had equal
before/after existence, lengths and hashes, matching the clean controls
in FND-SAVE-033. This retains the same limited interval and monitored
mechanisms, not a global no-write assertion. The owned session closed;
its emulator was absent and the machine run lock released afterward.

## Interpretation

The fixed slot0 cleanup queries, calls and enters the XMS free service
with handle1, receives reported success and rejoins the dispatcher.
Q-SAVE-007 need not retain an unspecified native API or a possible
different post-resume target for this recorded transfer. Other targets,
hooks, changed frames, continuous preservation and external effects beyond
the observed contract remain separate. No ordinary dirty-state producer,
complete reading, queue closure or parity promotion follows.

## Alternatives

A pre-call pointer alone would leave the entered target unresolved;
the fixed native-entry stop supplies that separate observation. API
success does not prove every unrelated memory field is preserved or
that no other cleanup mechanism affects files. An XMS handle number
does not bind a same-numbered file/archive handle.

## How to reproduce

Run the pinned evidence Python with shutdown_run.py at79d51762 and
arguments `v181-q063-native-save-input-20261104 <new-run-name>`.
Retained: GAME_DIR/captures/rust-dos-shutdown/
v181-quit-xms-entry-20261106/shutdown.json, session.json and journals,
with private files beside them. Require passed=true,32 stops, no guest
writes/draws, one xms_calls record with AL80, equal queried/free/native
target0160:1F00, handle1 and native AX1; verify the entry/return frames,
outer normal rejoin and clean-file controls above. Existing27 synthetic
controls cover tooling admission, not original-game behavior. Keep all
original-derived reports local.
