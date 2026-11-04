---
id: FND-COMBAT-137
title: One admitted empty-row Load supplies an empty source path and a terminated working path to the copy helper
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/load_entry.py at 12dc6d02; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

On 2026-11-04 the fixed probe repeated FND-COMBAT-136's F2/Enter route
from the verified two-character map checkpoint. It verified the Enter
event and numbered-load caller frame, then continued +05ED through its
preparation children and the working-path construction in overlay 187
+0EBE. It stopped at resident 4544:0000 before its first instruction.

The copy-entry far return frame pointed to overlay 187 +0EFC. The probe
matched that segment against the loaded 56EF:0089 trampoline and verified
the +0EF7 far call targeted the current relocated 4544:0000. It decoded
only the declared source, destination, optional buffer and capacity arguments.
The latter two were null and zero, respectively, as FND-COMBAT-113's
caller contract describes. The copy helper's internal-buffer branch had
not yet executed.

The source pointer equaled the current list pointer plus 125 bytes without
offset wrap: row one with top zero. Its first byte was NUL, establishing
an empty terminated source at this stop. The probe read no later source
bytes. The destination pointed into the wrapper's stack buffer. It held
the fourteen-character path `C:\darkrun.GFF`, followed by NUL within the
82-byte local buffer described by the static wrapper reading. Paths were
read one byte at a time, with source bound 80 and destination bound 82.
Their storage pointers were distinct; filesystem identity was not queried.

Compared with numbered-load entry, selected row one, top zero, Load mode
one, list pointer, global hook/mirror, count one, map head pointer, null
Save/Load pointer, null current event target, null shared saved-handler
and zero exit byte matched at copy entry. This observes those fields
after the preparation children; it does not prove their entire lifetime.

Enter delivery had completed by the final stop. The run recorded the one
checked RULE-SOUND-003 draw on F2's route and no further generator draw.
There were no setup memory writes. The owned muted session closed without
executing normalization or I/O inside this copy helper.

## Interpretation

This binds actual terminated copy inputs for one admitted empty-row case,
beyond FND-COMBAT-129's conditional pointer formula and own release ordering.
The selected source is an empty string, not a proved existing saved game.
Its terminator remained present at copy entry after the preparation work.

The normalizer, its character tables, drive/directory state and subsequent
source/destination file requests were not observed. No copy rejection,
destination truncation, successful copy, archive selection or reader entry
is established. Distinct memory pointers do not prove distinct filesystem
objects. Q-COMBAT-063 retains these outcomes and other path/row/top cases.

## Alternatives

- The selected source must be a nonempty filename because numbered loading
  was entered: the actual source's first byte at copy entry was NUL.
- Preparation necessarily invalidates or replaces the list pointer before
  path formation: this run reached the copy helper with the same pointer
  and the expected row-one offset. Other histories remain open.
- The empty source guarantees an early return without destination changes:
  this run stops before normalization and file requests, so it cannot settle
  that outcome; FND-COMBAT-113 separately describes destination-open ordering.

## How to reproduce

Prepare the verified current Play checkpoint under docs/RUNTIME.md. Run
the project-local evidence Python with `-I` and
`tools/research/rust-dos-runs/load_entry.py v181-q046-play-ready-20261104 v181-q063-copy-inputs-20261104 copy-inputs`.
Choose a new output name on repetition. Reports, journals and the earlier
Load-entry state remain under
GAME_DIR/captures/rust-dos-load-entry/v181-q063-copy-inputs-20261104/.
The probe verifies the loaded wrapper/call and argument contract, rejects
missing terminators and short/wrapping reads, guards RNG and stops before
the copy body. Synthetic bounded-string and frame controls validate tooling,
not the original's file behavior.
