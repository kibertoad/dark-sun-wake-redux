---
id: FND-COMBAT-138
title: The admitted empty source is rejected before destination normalization and the working file has no net change
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/load_entry.py at 0fd52531; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

On 2026-11-04 the fixed probe repeated FND-COMBAT-137's admitted
F2/Enter-to-copy route. The source was empty and the destination was
the terminated working pathname. The declared source/destination,
null-buffer/zero-capacity arguments and wrapper return were verified
before continuing the resident copy helper 4544:0000.

The probe stopped at 4544:004F, immediately after the first 455B:0001
call. AX was FFFF and its source-normalization output was empty and
terminated. SS and BP identified the original copy frame. The run did
not hit the configured destination-normalization return at 0067 or the
helper's direct source/destination open-call sites at 0078 and 008C.
These are the call sites in the bounded body of FND-COMBAT-113/114,
not a census of every possible filesystem operation in other helpers.

The copy helper reached its verified overlay-187 +0EFC return with AX
0000. The stack segment matched copy entry and SP was entry SP plus
four. This stop preceded the wrapper's argument cleanup and copy-result
test, and did not continue the wrapper's archive/reader work or the
outer numbered-load continuation.

Only the owned run's private drive-c/darkrun.GFF was inspected on the
host, at copy entry and return. It existed with length 991 at both stops
and its SHA-256 matched. The metadata reader rejected links, junctions,
directories and files above its 128 MiB bound; it made no writes and
never inspected the installed game's working file. This is equality of
the observed host-file state, not proof against transient writes, open
handles, buffered state or effects on other files.

The run recorded the one checked RULE-SOUND-003 draw on F2's route,
with no additional draw in this copy invocation. No setup memory writes
were made. Enter delivery had completed before copy entry. The owned
muted session closed at the copy return.

## Interpretation

For this admitted empty-source case, the source normalization returned
failure and the copy returned zero without reaching its destination
normalizer or direct file opens. The observed working file had no net
change. This agrees with FND-COMBAT-114's empty-input rejection and
FND-COMBAT-113's nonzero-normalization bypass, rather than the branch
that opens the destination after a failed source-file open.

An empty-input normalization failure and a nonempty input whose source
open fails are different cases. This run does not settle the latter,
successful copying, partial writes, close failures, filesystem aliases
or later loading of preexisting working contents. The outer callers'
result handling remains the static reading of FND-COMBAT-084/129;
there is no observed whole-load success or failure here. Q-COMBAT-063
retains those independent cases and external outcomes.

## Alternatives

- This empty source reached destination creation before failing: neither
  the destination normalizer nor the direct destination-open call was
  reached in the observed copy invocation.
- A copy return of zero necessarily means the whole load stopped: the
  run ended before wrapper and outer-caller continuation.
- Matching working-file hashes prove no transient effects: only its
  bounded host-file state at entry and return was compared.

## How to reproduce

Prepare the verified current Play checkpoint under docs/RUNTIME.md. Run
the project-local evidence Python with `-I` and
`tools/research/rust-dos-runs/load_entry.py v181-q046-play-ready-20261104 v181-q063-copy-return-20261104 copy-return`.
Choose a new output name on repetition. Reports, journals and the earlier
Load-entry state remain under
GAME_DIR/captures/rust-dos-load-entry/v181-q063-copy-return-20261104/.
The probe validates its internal/return frames, bounds normalized-output
reads and private-file metadata, and stops before wrapper continuation.
Synthetic file-state, link, length and frame controls validate tooling only.
