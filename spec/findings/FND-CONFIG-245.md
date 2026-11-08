---
id: FND-CONFIG-245
title: Fixed Preferences watches repeat one music-byte change from strict restore through the paired store
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/preferences_readback.py and preferences_history.py at 26e891ad; dinorefurb-dosbox-session 1.0.0; project-local evidence Python 3.12.10
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

FND-CONFIG-238 admits a strict restored Preferences caller/result/store
route but does not track the named values between those boundaries.
FND-CONFIG-243 supplies the ordinary event-to-store contracts;
FND-SOUND-085/092 identify the source/getter handles and profile gates.
This finding adds a fixed watch interval on that same restored route,
not an uninterrupted fresh-startup history.

On 2026-11-08 two independent owned sessions restored the checked
v181-q046-play-ready-20261104 play-ready checkpoint. The probe verified
the pinned emulator, embedded approved executable identity, instruction
and emulated time, full named snapshot, frame hash and input-map readiness
before input. DSUN.EXE is the approved 634416-byte image, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. No canonical checkpoint or source was
changed. Addresses and encoded sentinel values below are hexadecimal;
counts, levels, card/mode values, times and RNG values are decimal.

### Fixed watch scope and positive mutation control

While paused at the admitted restore, the probe read and armed exactly
nine nonoverlapping named ranges. Eight come from the fixed relocated
game/library fields; the ninth is the card word reached through the
admitted configuration pointer. Each field has its explicit one-, two-
or four-byte width. The probe rejects a changed configuration pointer
rather than following a new watch target.

| Named field | Width | Initial | Final |
|---|---|---|---|
| Master sound gate | 1 | 1 | 1 |
| Music gate | 1 | 1 | 1 |
| Music-level byte | 1 | 100 | 255 |
| Configuration pointer | 4 | 68D0:0004 | 68D0:0004 |
| Driver slot | 2 | FFFF | FFFF |
| Source sequence handle | 2 | FFFF | FFFF |
| Getter sequence handle | 2 | FFFF | FFFF |
| Library mode | 2 | 4 | 4 |
| Configuration card word | 2 | 122 | 122 |

Under the pinned debugger's changed-value watch contract, both reports
record exactly one change. The primary range is the music-level byte;
the actual post-instruction stop is 641F:0041, sequence 109, at emulated
time 195622786816ns. Its old/new values are 100/255. No other named
field differs in that stop's complete watched-field snapshot.

The baseline restore is at emulated time 195166973150ns. Watches remain
armed through the Tab, motion, selected-control wait, click, RNG handling
and paired getter/store observation. Ordinary stops recheck all current
watched values; an unreported difference aborts. A watch interrupting the
RNG recorder's nested continuation also aborts rather than being skipped.
The positive music-byte event is required before a successful history
marker. It establishes actual byte-watch delivery on this route; merely
arming the other ranges is not independent positive mutation delivery for
each word/pointer range.

The observer does not equate this with every memory write. Same-value
writes and intra-instruction transient accesses are outside its value-change
record. Other bytes of the 59-byte configuration are read at paired stops,
but only its card word is watched. No continuous DS/SS, heap-capacity,
all-configuration-byte or arbitrary-address watch capability follows.

### Paired caller, result and actual watch endpoint

The fixed input route is the existing one: Tab to ready WIND/10500,
motion to virtual (160,101), native BUTN/10303 selection, then one left
click. No gesture is retried and no guest setup write is made. The strict
source checkpoint and canonical files remain unchanged.

Both sessions reach getter entry 4028:011F with DS=SS=4D9A and SP=FEE4.
The checked return frame and actual entry trampoline bind the caller to
overlay segment 641F and its following AL store. Getter return is at
641F:003E with AX=255 and SP=FEE8. The byte is still 100 there.
The subsequent music-byte watch, with its actual reason retained as
watchpoint, stops at 641F:0041 with AX=255, SP=FEE8 and byte 255.
There is no competing post-store execution breakpoint on this changed-byte
path. The paired-frame, call/store operand and live-trampoline guards all
pass, and the final history snapshot agrees with the paired stored snapshot.

No other value-change event is reported for the watched physical ranges
in this interval. This is stronger than comparing only the two endpoint
values, but remains a report under the bounded debugger contract and its
stated exclusions. It does not classify all possible writers, aliases or
reentrant readers elsewhere.

### Repetition, RNG and session closure

Each run uses forty named stops and records one admitted RULE-SOUND-003
draw: bound 3, result 0, seed 1718646187 to 2185262696. The complete
value-history objects, paired readback arrays and draw arrays are identical
between the two sessions, including the watch event's sequence and time.
The run metadata and input-operation identifiers are not part of that equality.

Both histories finish only after the paired store and removal of their
nine owned watches. Their successful reports have empty setup-write lists.
Both session contexts close normally; host sound is muted and run_failure
is null. The session record has no event journal, so none was reconstructed.
This repeats one checkpoint and seed, not a multi-seed experiment, fresh
startup observation or rebuild replay/parity validation.

## Interpretation

Q-CONFIG-014 can remove an entirely unobserved named-value interval from
strict play-ready restore through this particular Preferences store. The
positive mutation is the already-evidenced 100-to-255 music-byte store;
the report adds the armed field set and instruction-boundary change history
through the intervening fixed input route. The source and getter handles
are sampled FFFF and have no reported changes in that bounded history.

Retain actual fresh-startup-to-restore history, other roots/callers and
non-sentinel driver paths, later intervals, unmonitored fields, unchanged
writes and intra-instruction effects, plus the existing pointer/alias,
storage, writer and reentrant-entry obligations outside this observation.
The positive byte event does not validate every untriggered watch width.
RULE-CONFIG-003 remains supported with no complete_reading declaration;
no whole queue item closes.

## Alternatives

- The history succeeds with watches merely armed: completion requires an
  actual music-byte mutation and the paired store, then cleanup.
- Equal handle endpoints alone describe the whole measured output: the
  fixed reports also retain every delivered named-field value-change event
  and reject unexplained sampled differences.
- The reported history covers all writes or startup: it starts at a
  restored checkpoint and excludes same-value/intra-instruction effects.
- The watched endpoint is a fabricated execution stop: the actual stop
  reason remains watchpoint, with the checked post-store frame and boundary.

## How to reproduce

Use the checkout-local evidence Python with `-I` and
tools/research/rust-dos-runs/preferences_readback.py, arguments
v181-q046-play-ready-20261104, a new run name, and history.
The two retained outputs are under
GAME_DIR/captures/rust-dos-preferences-readback/ with names
v181-q014-history-a-20261108 and v181-q014-history-b-20261108.
Each contains preferences-readback.json and session-owned runtime records.

Require passed and value_history.complete, nine field definitions, the
single actual music-byte event, the paired entry/return/store and the
admitted draw with no guest writes. Compare value_history, readback and
draws from the parsed reports, not whole-file metadata hashes. Require the
same checked source, strict restore controls, emulator/media/audio pins and
fixed input route. The existing observation plan documents all bounds and
the separate synthetic control commands. Keep reports, frames, checkpoints
and original-derived data only in GAME_DIR.
