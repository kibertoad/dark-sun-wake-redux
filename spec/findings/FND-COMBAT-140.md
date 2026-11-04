---
id: FND-COMBAT-140
title: Native F1 and Enter create a row-one save and a strictly restorable post-Save keyboard-loop checkpoint
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/native_save.py at 1ec1c8a4; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

On 2026-11-04 the fixed probe restored the verified two-character map
checkpoint and sent only F1 and Enter. Before Enter, it required ready
window 18500, Save-mode byte zero, selected row one, top zero and exit
byte zero. No text, mouse action, setup memory write or imported save
was supplied. The game wrote only inside its owned private drive.

The probe verified the callback trampoline's loaded overlay entry and
the declared +0BEF call to overlay 192 +03FB (FND-SAVE-004, FND-UI-037).
It reached that Save entry with row argument one and far return +0BF2.
The saved return-segment watch recorded ten changes while the original
worked; it updated only the declared caller breakpoint. At the eventual
Save return SS matched entry and SP was entry SP plus four. The loaded
caller instruction was the expected POP CX. The callback's own incoming
event kind was not captured, so no specific Save event-kind mapping is
inferred merely from Enter causing the action.

The private SAVE02.SAV did not exist initially. After Save returned and
the input-loop predicate passed, it existed with length 49,739. Its bytes
were produced by the original, not by a synthetic resource writer. The
probe recorded a bounded SHA-256 locally. This is not an inventory of
every resource in that file or proof of successful subsequent loading.

Both key operations had completed before checkpoint creation. At the
native input builder 39D1:097D, mode was one, leader one, party slot kinds
0,2,0,2 and mouse enabled. A11D remained null. Independently, the observed
head was a tagged WIND/19501 with ready low flags, count was one, the
global hook was the map handler, event target and Save/Load window/list
pointers were null, and exit byte was zero. The probe retained the null
current-window value rather than substituting the head for it. These
checks define this candidate keyboard-loop state, not general window-graph
membership or a new F2 admission claim.

The route completed in 32 observations and recorded one RULE-SOUND-003
draw and two RULE-SCRIPT-006 draws, each checked with its next generator
state. It saved a file-checked saved-map checkpoint and frame.

A second owned process restored that checkpoint with strict matching and
file checks. Instruction/time, named state, frame digest and native-save
digest matched. A checked save/load roundtrip in that process matched the
same fields, frame and file digest. No gameplay input was sent during
verification. Both owned muted sessions closed afterward.

The earlier attempt had already returned from Save and created its private
file, but its old A11D-based predicate did not pass. It eventually queried
a completed input operation after it had left the bounded debugger journal
and was refused. That failure produced no accepted checkpoint. The final
probe caches terminal delivery observations and independently validates
the post-Save head/input-loop state; the diagnostic run is retained.

## Interpretation

This supplies an authentic native-file candidate and reproducible state
for Q-COMBAT-063's nonempty-source case. Save's physical F1/Enter route,
one row argument, actual output file and checkpoint compatibility were
observed without editing an original-derived record or pathname.

An actual F2 entry and selected-path binding from this checkpoint remain
required before it supplies the nonempty copy experiment. No restored
saved-game gameplay, complete file-resource contract, arbitrary Save slot,
overwrite behavior, native patched-save capability or parity validation
is established. The narrow return-segment watch was exercised on actual
changes; that does not admit arbitrary memory watches or writes.

## Alternatives

- A native saved file requires a host-written fixture or seeded record:
  this initially absent file was produced through the original's Save action.
- Null A11D proves no usable map head or keyboard-loop state: those were
  independently checked at the native input builder; F2 remains untested here.
- Strict checkpoint restoration proves the native save will load correctly:
  only emulator state, frame and file identity were compared.

## How to reproduce

Prepare the verified current Play checkpoint under docs/RUNTIME.md. Run
the project-local evidence Python with `-I` and
`tools/research/rust-dos-runs/native_save.py v181-q046-play-ready-20261104 v181-q063-native-save-input-20261104`.
Then run that script with
`v181-q063-native-save-input-20261104 v181-q063-native-save-verify-20261104 verify`.
Choose new output names on repetition. Reports, journals, native private
files, frame and checkpoint sidecars remain under
GAME_DIR/captures/rust-dos-native-save/ in those two named directories.
The earlier diagnostic directory is v181-q063-native-save-20261104.
The driver is bounded to 1024 observations, permits only F1/Enter and the
declared file/frame reads, and rejects unexpected frames or RNG callers.
Synthetic mode, head, return, path-name and terminal-delivery controls
validate the tooling only.
