---
id: FND-COMBAT-135
title: An admitted Down callback returns row zero from row one when all ten Load markers are empty
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/load_entry.py at fac62451; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

On 2026-11-04 the fixed probe repeated the admitted F2/Down route of
FND-COMBAT-133/134 from the verified two-character map checkpoint. It
matched the outgoing event at 39D1:083B to the kind-six/key-5000 argument
at Load callback trampoline 5736:002F and verified its far return frame.
This attempt then continued that callback to resident 39D1:083F, before
the dispatcher removed its outgoing argument copy or ran its post-call helper.

At callback entry top was zero, selected row one and Load mode one.
The probe read only each record's byte-zero marker at strides of 125
through the nonnull list pointer. All ten markers were zero. At the
callback's return they were again all zero, top remained zero and
selected row was zero. Returned AX was 0003. The return's SS matched
entry SS and its SP equaled entry SP plus four, confirming the matched
far-call frame rather than accepting the return instruction alone.

Count two, null current event-window target, the global hook and mirror,
Save/Load pointer, head pointer, list pointer, Load-mode byte and zero
exit byte matched their respective entry observations. The hook mapped
to 5736:002F and the Save/Load pointer equaled the head at both stops.
Equality at these two stops does not exclude intermediate changes or
establish allocation capacity, graph membership or lifetime after return.

The F2 route recorded the same single checked RULE-SOUND-003 draw as
FND-COMBAT-133. No additional draw was recorded during Down's callback.
There were no setup writes and no load confirmation. Down's release
operation remained pending; no checkpoint was saved at this pending-input
return. The owned muted session closed after recording the result.

## Interpretation

This is an actual returned navigation outcome for FND-COMBAT-085's
all-empty-marker case: Down from row one reaches empty row zero. It
removes the corresponding execution gap for this admitted state, beyond
FND-COMBAT-134's callback-entry observation. It does not establish painted
selection, completed key release, other starting rows, Up, nonempty markers,
nonzero tops, sentinel selection or numbered-load delivery. AX 0003 is
the observed callback result, not a load-success flag. The later dispatch
result and post-call helper are outside the run's final stop.

## Alternatives

- Down cannot leave an empty row selected: row zero's marker and selected
  word were both zero at the matched callback return.
- The stop belongs to a nested invocation using the same return address:
  the stack segment and four-byte far-return stack change matched this entry.
- The entire list was proved unchanged throughout navigation: only the
  ten marker bytes and named fields were sampled at entry and return.

## How to reproduce

Prepare the verified Play checkpoint as docs/RUNTIME.md describes. Run
the project-local evidence Python with `-I` and
`tools/research/rust-dos-runs/load_entry.py v181-q046-play-ready-20261104 v181-q046-down-return-frame-20261104 down-return`.
Choose a new output name on repetition. Reports, journals and the earlier
Load-entry checkpoint remain under
GAME_DIR/captures/rust-dos-load-entry/v181-q046-down-return-frame-20261104/.
The prior diagnostic v181-q046-down-return-20261104/ retained the same
entry/return stack identity, markers and outcome before the explicit guard
was added. The final probe rejects other frames, null or wrapping lists,
nonzero tops, out-of-range rows, short reads and unadmitted RNG callers.
