---
id: FND-COMBAT-139
title: Numbered loading returns one after an empty-source copy rejection and subsequent settings requests
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/load_entry.py at 21f7c9d6; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

On 2026-11-04 the fixed probe repeated FND-COMBAT-136/137/138's
admitted F2/Enter route: selected row one, top zero, empty source path,
source normalization returning FFFF and file copy returning zero. It
continued only that copy-rejection branch, making no setup memory writes
and sending no additional input. Enter delivery had completed.

The parent overlay had moved since numbered-load entry. Rereading the
declared wrapper and numbered-load caller frames identified one current
segment for continuations +0659 and +0BE2. The probe verified the loaded
argument-cleanup/CHARSAVE-call instructions at +0659 before resuming.
The mapping was not inferred from the segment sampled before preparation.

All seven configured internal stops matched the original numbered-load
SS/BP. Offsets are in overlay 192, with the same meanings as FND-SAVE-022:

| Stop | Observed request or return |
| --- | --- |
| +0659, after copy wrapper | AX 0000 |
| +0661, after CHARSAVE helper | AX 0001 |
| +066F, before resource request | PREF/100, declared stack output pointer |
| +0674, after PREF request | AX 0000 |
| +0743, before resource request | GREQ/2, declared stack output pointer |
| +0748, after GREQ request | AX 0000 |
| +077D, after close helper, before final helper | AX 0000 |

Resource payloads were not read. Request tags/IDs and return words do
not independently identify their supplying archive, returned lengths or
settings ultimately applied. The final helper's internals were not traced.

After 534 continuation observations, the run reached the caller at +0BE2
with AX 0001. Its SS matched numbered-load entry and SP was entry SP plus
four. The loaded caller instruction was the declared short jump to shared
cleanup at +0BF2. Execution stopped before that jump and the outer action's
list release or later cleanup.

At this stop selected row one, top zero, Load mode one, nonnull list pointer,
Load global hook/mirror, count one, map head, null Save/Load pointer, null
event target, null shared saved-handler and zero exit byte matched their
copy-entry values. Game mode remained two; current window was null, party
slot kinds remained 0,2,0,2 and leader remained one. No restored-game or
gameplay-ready predicate was asserted.

The run recorded only the one checked RULE-SOUND-003 draw on the F2 route.
The narrow watch on the saved return-segment word was armed before the
final helper and recorded no changes; this does not verify watchpoint
delivery on an actual mutation. The owned muted session closed at return.

Two earlier 512-observation attempts exhausted their limits: the first
used the stale parent segment; the mapped one reached the settings stops
but not the final return within that limit. A later watched attempt reached
the same return and stack frame but failed an incorrect opcode guard that
expected cleanup directly at +0BE2. Static reading showed its jump to
+0BF2; the final run passed the corrected guard. Those diagnostic reports
are retained and are not treated as game failures.

## Interpretation

This is an actual rejected-copy continuation: the wrapper returned zero,
the outer routine requested PREF/100 and GREQ/2 and ultimately returned
one to its caller. It confirms that copy rejection need not stop this
numbered-load routine before settings work, beyond the static ignored-result
reading of FND-SAVE-022 and FND-COMBAT-084/129.

Returned AX one is not proof that a saved game was copied or restored.
The empty-source case, its unchanged working-file comparison and these
settings request results are separate observations. Other copy outcomes,
archive/resource provenance, payload effects, later action cleanup and
other rows/tops remain Q-COMBAT-063 and its related questions.

## Alternatives

- Zero from copying necessarily returns immediately from numbered loading:
  both settings requests and the matched outer return were observed afterward.
- Returning one proves restoration succeeded: this invocation's source was
  empty and its copy was explicitly rejected.
- A segment captured at entry remains a valid continuation mapping: the
  observed caller frames and loaded code required the current parent segment.

## How to reproduce

Prepare the verified current Play checkpoint under docs/RUNTIME.md. Run
the project-local evidence Python with `-I` and
`tools/research/rust-dos-runs/load_entry.py v181-q046-play-ready-20261104 v181-q063-load-return-verified-20261104 load-return`.
Choose a new output name on repetition. Reports, journals and the earlier
Load-entry state remain under
GAME_DIR/captures/rust-dos-load-entry/v181-q063-load-return-verified-20261104/.
Earlier diagnostic runs under that parent directory use the names
v181-q063-load-return-20261104, v181-q063-load-return-mapped-20261104 and
v181-q063-load-return-watched-20261104. The fixed extended stage is bounded
to 1024 observations; matched frames and loaded instructions, not elapsed
time, admit its result. Synthetic request/mapping/frame controls validate
the tooling only.
