---
id: FND-COMBAT-142
title: One copied native save reaches selector-zero reader entry with working handle fourteen and game mode four
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/load_entry.py at 13dc6444; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

On 2026-11-04 the fixed probe repeated FND-COMBAT-140/141's verified
native-file F2/Enter route through copy return one. It continued only
that successful copy result, following the wrapper's declared region
preparation and near reader call. No setup memory writes or additional
input were introduced. The reader body did not execute.

It captured the following named resident fields at each boundary.
Handles below are decimal numeric values, not far pointers.

| Field | Copy return | Region-preparation entry | Region return and reader entry |
| --- | ---: | ---: | ---: |
| Reuse byte DS:167A | 0 | 1 | 1 |
| Saved working handle DS:144E | 10 | 14 | 14 |
| Region handle DS:145A | 0 | 0 | 0 |
| Game mode DS:0DAB | 2 | 2 | 4 |
| Region DS:140C | 50 | 50 | 50 |
| Traversal mode DS:9D9B | 2 | 2 | 2 |
| Issuance counter DS:9D8E | 14 | 15 | 15 |
| Active record's +28 handle field | 4 | 14 | 14 |
| Head record's +28 handle field | 4 | 14 | 14 |

At each stop active and head pointers were equal. Their observed pointer
changed between copy return and region entry, then matched at region
entry, region return and reader entry. The only dynamic record reads
were the evidenced four-byte +28 fields through bounded nonnull pointers.
The probe did not traverse links, validate allocation headers or bind a
record to an open host file merely from this equality.

At resident region trampoline 56BD:00C0, the far return named overlay
187 +0F26, original wrapper SS/BP matched and the loaded +0F21 call
instruction matched. Its four argument words were (50, 0, 1, 0), as in
FND-COMBAT-128's pre-read preparation call.

The wrapper overlay was unloaded while that call ran. The probe observed
its saved return segment change to resident header 56EF, reached that
header's offset-zero trap with the expected post-far-return stack, and
validated the retained +0F26 offset. It removed the stack-word watch
before INT 3F reused that storage. At manager 4AE5:0559, it verified
the corresponding interrupt frame and reloaded parent segment, then
stopped at +0F26 with the original far-return stack identity. This is
one observed instance of FND-EXE-562's documented trap-return mechanism,
not a complete reading of every overlay-manager path.

The loaded cleanup and near call to +0A6C matched. Reader entry had
far return +0F2F in the same current overlay segment, selector argument
zero and the expected relocated resident DS. This admits the snapshot
at the first reader instruction, before its prologue or any payload write.
RNG guards recorded no draw; the owned muted session closed there.

The first attempt stopped safely when a watched frame no longer held
the expected far return during the trap transition. Its copy and
region-entry snapshots remain diagnostic. The final attempt explicitly
handled the documented header/IRET path and passed all entry guards.

## Interpretation

This supplies one actual pre-reader mode/reuse/handle tuple after the
admitted native copy, beyond FND-COMBAT-110/128's conditional own-body
readings. At reader entry the saved working handle equals the observed
active/head +28 handle field, game mode is four and region handle is zero.
Those values are no longer inferred solely from the initial checkpoint
or from a successful file-copy result.

Matching snapshots do not prove absence of intervening writes, closure,
replacement or aliasing. In particular, region-handle zero at both ends
does not exclude an intervening child changing it. No SAVE/24 or SAVE/26
request, directory entry, payload length, fallback or provider identity was
observed. Q-COMBAT-064 retains those request-time and lifetime obligations;
Q-COMBAT-065/066 retains destination and child effects. No restoration
success or complete-reading promotion follows from this entry observation.

## Alternatives

- Selector-zero reader entry requires game mode zero: this admitted
  invocation entered with selector zero and mode four.
- A successful copy alone identifies the later active record: its observed
  active/head pointers and handle fields changed before region preparation.
- A watched stack word remains a far-return segment throughout overlay
  reloading: the header trap reuses that storage for the interrupt frame;
  treating it as the original far frame caused the first guard to reject.

## How to reproduce

Prepare and independently verify FND-COMBAT-140's native candidate. Run
the project-local evidence Python with `-I` and
`tools/research/rust-dos-runs/load_entry.py v181-q063-native-save-input-20261104 v181-q064-reader-trap-20261104 saved-reader-entry`.
Choose a new output name on repetition. Reports, journals and private
files remain under
GAME_DIR/captures/rust-dos-load-entry/v181-q064-reader-trap-20261104/.
The first diagnostic is v181-q064-reader-entry-20261104 under that parent.
The bounded driver verifies declared frames, current code, trap metadata
and DS before stopping; synthetic trap/frame/record-field controls establish
tooling behavior, not resource-provider or native payload evidence.
