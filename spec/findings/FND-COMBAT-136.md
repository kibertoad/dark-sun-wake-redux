---
id: FND-COMBAT-136
title: Enter reaches the numbered-load entry for an empty selected row after Load-window cleanup
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/load_entry.py at ac445421; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

On 2026-11-04 the fixed probe restored the verified two-character map
checkpoint and repeated FND-COMBAT-133's F2 entry into Load. After F2
delivery completed and the Load-state predicate matched twice, it sent
only Enter. The outgoing event at 39D1:083B matched the callback-entry
argument at the relocated 5736:002F trampoline: kind six, key word 1C0D.
Its far return frame identified 39D1:083F. No row-selection input was sent.

At callback entry selected row was one, top zero, Load mode one and count
two. Current event-window target was null. The global hook and mirror
mapped to 5736:002F, the Save/Load pointer equaled the nonnull list head,
the list pointer was nonnull, the shared saved-handler slot was null and
the exit byte was zero. Byte-zero markers of all ten 125-byte records were
zero, including the selected row. Names and paths were not read.

Before sending Enter, the probe verified the trampoline's far jump to
overlay 192 +0785 and the declared near call at +0BDF targeting +05ED.
It stopped at that loaded +05ED entry before the function body executed.
The incoming far return frame identified +0BE2 in the same overlay segment,
and the row argument was one. This binds the reached entry to the action
call described by FND-COMBAT-081/129, rather than an inferred window route.

At the numbered-load entry, the Save/Load window pointer was null and
count was one. The head equaled the map's original head. Selected row one,
top zero, Load mode one, list pointer, global hook/mirror, null event target,
null shared saved-handler and zero exit byte matched the callback-entry
observations. This samples the fields after the action's cleanup children;
it does not prove their absence of intervening writes or future preservation.

The run recorded one checked RULE-SOUND-003 draw on the F2 route and no
additional generator draw before the final stop. No setup writes were made.
Enter's release operation was still pending; it was not claimed completed
or saved in a checkpoint. The owned muted session closed at this stop.

## Interpretation

This supplies an actual Enter-to-numbered-load call for the admitted row-one,
top-zero state. An empty marker did not prevent that call on this route.
The row/mode argument after window cleanup is observed, beyond the earlier
static own-body ordering and callback-delivery snapshots.

The loader body did not run. This establishes neither a readable saved game,
the source path, copy success, reader entry, load failure handling nor a
completed load. Other rows, tops, input devices, callback routes and cleanup
histories remain unadmitted. Q-COMBAT-046 retains those state/route questions;
Q-COMBAT-063 retains source/copy admission and Q-COMBAT-064/065/066 retain
the reader's resource, destination and child obligations.

## Alternatives

- Empty selected markers always prevent reaching the numbered-load caller:
  the selected marker was zero at callback entry and +05ED was reached.
- Window cleanup necessarily resets the selected row or removes the list
  pointer before the call: this run retained row one and the same list pointer.
- The observed call proves loading an empty slot succeeds: execution stopped
  before the loader body, so no success or failure outcome was observed.

## How to reproduce

Prepare the verified current Play checkpoint under docs/RUNTIME.md. Run
the project-local evidence Python with `-I` and
`tools/research/rust-dos-runs/load_entry.py v181-q046-play-ready-20261104 v181-q046-load-call-20261104 load-call`.
Choose a new run name on repetition. Reports, journals and the earlier
file-checked Load-entry state remain under
GAME_DIR/captures/rust-dos-load-entry/v181-q046-load-call-20261104/.
The script verifies the loaded transfer and caller, bounds marker reads,
guards RNG and makes no direct emulated function invocation. Synthetic
wrong-jump, wrong-call and wrong-key controls validate the tooling only.
