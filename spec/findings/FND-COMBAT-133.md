---
id: FND-COMBAT-133
title: One admitted map F2 event reaches Load with a null event target and preserved top zero and selected row one
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/load_entry.py at 89e49739; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; pinned rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

On 2026-11-04 the fixed probe restored the independently verified current-pin
two-character map checkpoint. It verified the installed executable fingerprint,
saved instruction/time, window 19501 ready flags, mode one, occupied slots 1
and 3, and leader 1 before delivering only F2. It made no setup memory writes,
selected no row and sent no Load confirmation. Static attempts for the event
question are recorded under Q-COMBAT-046; FND-CONFIG-221/222 supplies the
branch/count reading and FND-COMBAT-132 identifies the fields observed here.

The initial snapshot had count one, null current event-window target, a
nonnull window-list head, global handler and mirror both mapping to
28C9:0CFF, selected row one, top zero, null Save/Load window and list pointers,
Load-mode byte zero and exit byte zero. The shared saved-handler slot was null.

At entry to resident 39D1:0752 the probe captured three 24-byte by-value
argument records. Their first words were three, six and three. The middle
record's key word was 3C00 (F2). At that stop count remained one, current
event-window target was null, and the global handler and mirror still mapped
to 28C9:0CFF. This is an observed nonzero-count/null-target incoming state,
not the count-zero route that rewrites kind to three. FND-CONFIG-221's
own branch selects the global hook for these initial inputs. The probe did
not stop at the later indirect call or prove preservation across its helpers.

The final matching snapshots had ready window 18500, game mode two,
Load-mode byte one, count two, selected row one and top zero. The nonnull
Save/Load window pointer equaled the current list-head pointer. The global
handler and its mirror both mapped to 5736:002F. The list pointer was nonnull,
the shared saved-handler slot and current event target remained null, and
the exit byte remained zero. Pointer equality is not a list-membership or
allocation-lifetime proof. The probe did not traverse records or graph links.

F2 delivery completed before the final state was accepted. The run recorded
one RULE-SOUND-003 draw with bound three and result zero; its seed changed
from 1718646187 to 2185262696 and agreed with the recorder's arithmetic.
There was no unknown generator stop. Seven continuation observations met
the entry predicate. A frame and file-checked load-entry checkpoint were
retained; independent restore of this new checkpoint has not been attempted.

## Interpretation

This supplies an actual F2 entry route and actual initial count, hook,
selection and top values for this checkpoint. A nonzero count does not
require a nonnull current event-window target: the observed entry used the
null-target alternative. It removes the need to infer those initial values
from fixed SAVE payload exclusions or the shipped initial data alone.

This does not admit a numbered-load action, every F2 route, Save entry,
navigation, another row/top, later callback identity, payload preservation
or any child behavior. No numbered-load action was requested, and
no rebuild RNG replay or parity validation was performed. Q-COMBAT-046
retains those independent event/state/lifetime obligations.

## Alternatives

- Count zero rewrote this F2 event to kind three: the observed incoming
  event was kind six and the simultaneous count was one.
- A nonnull current event window selected the window-callback branch:
  the captured target at this dispatch entry was null. A later mutation
  remains outside the entry snapshot's proof.
- Opening Load necessarily resets selection to zero: this run ended at
  selection one. Other histories and intervening children remain open.

## How to reproduce

Use the current runtime preparation and Play regeneration commands in
docs/RUNTIME.md, retaining their strict file and emulator identities. Run
the project-local evidence Python with `-I` and
`tools/research/rust-dos-runs/load_entry.py v181-q046-play-ready-20261104 v181-q046-load-entry-20261104`.
Choose a new output name on repetition. Retained report, journals, frame
and checkpoint sidecars are under
GAME_DIR/captures/rust-dos-load-entry/v181-q046-load-entry-20261104/.
The script guards both RNG entries, bounds continuations and named reads,
and closes only its owned muted session. Synthetic short-read, tag, width
and readiness controls establish tooling behavior, not original-game evidence.
