---
id: FND-COMBAT-134
title: Down reaches the Load global callback with its outgoing event and return frame intact in one admitted state
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/load_entry.py at 3bbc26da; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

The 2026-11-04 fixed probe repeated FND-COMBAT-133's verified map-to-Load
route, then delivered only Down. It stopped at the global far call
39D1:083B and at the Load callback trampoline 5736:002F, before that
callback executed. The outgoing 24-byte event and callback argument were
identical. Their kind was six and key word 5000. The entry's far return
frame pointed to 39D1:083F, the instruction after the observed call.
Addresses here use the established modeled load mapping, not the actual
relocated segments retained in the local report.

At both call and entry, the global hook and mirror pointed to 5736:002F,
count was two, current event-window target was null, Load mode was one,
selected row was one, top was zero and exit byte was zero. The nonnull
Save/Load pointer equaled the window-list head; the list pointer was
nonnull and the shared saved-handler slot was null. These are simultaneous
named-field observations, not evidence of every intervening write or
window-graph membership.

The scoped run reached the boundary in nine continuation observations,
with three dispatch-entry captures before the probe removed that broad
breakpoint. An earlier attempt that kept it active also reached the same
boundary, but captured 204 mostly idle dispatches. Both attempts were
terminal before another run started. The scoped run retained one checked
RULE-SOUND-003 draw on the F2 route, as FND-COMBAT-133 describes, and no
setup writes. The Down operation was still pending at callback entry:
the observed game event proves key-down arrival, not completed key release.
No checkpoint was saved with that pending operation; the Load-entry state
was saved earlier, after F2 delivery completed. The owned session then closed.

## Interpretation

This supplies actual navigation-event delivery through the nonzero-count,
null-target global-call route. The hook and event are observed at the call
and callback entry, beyond FND-COMBAT-133's dispatch-entry snapshot.
FND-COMBAT-085's navigation branch therefore has one admitted incoming
kind-six/5000 event at its trampoline with row one and top zero. The probe
does not execute that branch or establish its outcome, indexed marker
validity, child behavior, return result, later hook lifetime, other inputs
or numbered-load confirmation. Q-COMBAT-046 retains those obligations.

## Alternatives

- A count-zero rewrite converted this key to kind three: count two and
  kind six were observed at the actual call and callback entry.
- The event used a window callback or another global hook: the actual
  global call target, reached trampoline and far return frame matched.
- Completed Down delivery or successful movement was established: release
  remained pending and execution stopped before the callback body.

## How to reproduce

Prepare the verified current-pin Play checkpoint under docs/RUNTIME.md.
Run the project-local evidence Python with `-I` and
`tools/research/rust-dos-runs/load_entry.py v181-q046-play-ready-20261104 v181-q046-load-down-scoped-20261104 down`.
Choose a new output name on repetition. Reports, journals and the earlier
Load-entry checkpoint remain under
GAME_DIR/captures/rust-dos-load-entry/v181-q046-load-down-scoped-20261104/;
the unscoped diagnostic is beside it as v181-q046-load-down-20261104/.
The fixed probe checks both event frames and the return address, guards RNG,
bounds all named reads and stops before callback execution. Synthetic
wrong-frame, key, target and short-read controls establish tooling only.
