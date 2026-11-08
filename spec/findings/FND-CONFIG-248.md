---
id: FND-CONFIG-248
title: A fresh uninterrupted startup route reaches Preferences with sentinel handles and stores music byte 255
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/disc_startup.py, fresh_preferences.py and shared preferences_readback.py at 7f5baded; preferences_history.py; dinorefurb-dosbox-session 1.0.0; checkout-local evidence Python 3.12.10
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

FND-CONFIG-245 observes a restored-map interval; FND-CONFIG-246/247 observe
fresh startup through the first music gate and pre-opener call. This finding
adds a single-session changed-value history from fresh program entry through
the native Preferences getter and paired byte store. No endpoint matching
between separately restored sessions is used to establish this continuity.

On 2026-11-08 two independent fixed preferences-history runs launched the unchanged,
manifest-verified private drive with target DSUN.EXE -W0 -L and read-only
game.ins media. The executable is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. No start_state, saved state, supplied seed
or guest setup write was used. The session's writes and states lists are
empty. Addresses and sentinel words below are hexadecimal; counts, levels,
seeds and nanosecond times are decimal.

### Continuous named ranges and actual mutations

The program-start stop is 05BA:0000 at 1500750ns. Nine fixed nonoverlapping
physical watches are armed while paused before the first continuation.
The field set is the same as FND-CONFIG-246/247: master sound, music gate,
music-level byte, configuration pointer, music slot, source/getter handles,
library mode and internal driver enable. There is no pointer-derived watch
and no rearming or restore between startup and the final byte store.

Exactly three primary watch events occur:

| Field | Width | Post-instruction stop | Sequence | Time ns | Old | New |
|---|---|---|---|---|---|---|
| Internal driver enable | 2 | 3CEE:0131 | 33 | 24820950 | 1 | 0 |
| Configuration pointer | 4 | 3CEE:00DC | 35 | 24849950 | null | 68D0:0004 |
| Music-level byte | 1 | 641F:0041 | 1603 | 195622786816 | 100 | 255 |

Each primary watch's old/new payload matches independently read named
storage. These provide actual positive delivery controls for the exact
one-, two- and four-byte ranges that changed. There are no further reported
changes to the nine watched ranges under the pinned instruction-boundary
value-change contract. Every ordinary outer stop rechecks them, and an
unexplained sampled difference aborts.

| Field | Program entry | Paired Preferences store |
|---|---|---|
| Master sound | 1 | 1 |
| Music gate | 1 | 1 |
| Music-level byte | 100 | 255 |
| Configuration pointer | null | 68D0:0004 |
| Music slot | FFFF | FFFF |
| Source sequence handle | FFFF | FFFF |
| Getter sequence handle | FFFF | FFFF |
| Library mode | 4 | 4 |
| Internal driver enable | 1 | 0 |

### Checked startup, menu and map path

The 54 guarded disc startup snapshots and checked native seed entry/return
remain as in FND-CONFIG-247. The seed call receives native word 44976 and
changes RNG state from one to 44976. The pre-opener call is at 1D35:02E4,
time 129199843550ns. The observer validates that actual call and reaches its
known return before proceeding. No startup checkpoint is saved or restored.

Only the existing fixed two-character setup route is used. From ready
Start/19500 it clicks(160,93) and waits for Create/11500. It right-clicks
(121,47), selects(158,69), accepts(252,47), then right-clicks(121,107),
selects(158,129), chooses(125,56) and accepts(252,47). Each intervening
14000/18501/11500 readiness gate is required. Esc returns to Start and
click(157,76) begins. The known mode-zero intro receives one Esc only after
its stable ready 19501 state. Delivery alone is not accepted as map readiness.

At the native input builder the observer requires mode one, ready window
19501 and slot kinds[0,2,0,2], then delivers key2 and requires leader one
and the existing input-map structure checks. This final builder stop is
2F8B:097D, sequence 1509, time 195166973150ns. Its ready map has mouse
enabled one, mode one, leader one, handler 28C9:0CFF and RNG state
1718646187. The route retains 644 accepted outer menu stops. Polling time
steps are sampling bounds; only the checked native state admits progress.

The final route sends Tab, waits at main's verified key-event builder for
ready Game Menu/10500, moves to virtual(160,101), requires delivery and
selected BUTN/10303, removes the repeated builder poll, and sends one left
click. Every earlier fixed gesture and the final click complete delivery;
there are 24 input operations in total. Inputs are not retried on a timeout.
No screenshot or audible observation is used to infer the getter result.

### Every admitted generator draw

The native startup seed is followed by the recorded chain below. Before and
after outer continuations, sampled seed values must join this chain. Each
draw uses the existing caller/frame checks and verifies the native result
and next seed. Unknown callers, another seed-setter entry or a watch stop
inside an uncompleted recorder continuation would fail the observation.

| Rule | Bound | Result | Seed before | Seed after |
|---|---|---|---|---|
| RULE-SCRIPT-006 | 99 | 32 | 44976 | 2844524401 |
| RULE-SCRIPT-006 | 1 | 0 | 2844524401 | 9064550 |
| RULE-SCRIPT-006 | 1 | 1 | 9064550 | 3942496543 |
| RULE-SOUND-003 | 8 | 4 | 3942496543 | 685032812 |
| RULE-SCRIPT-006 | 4 | 1 | 685032812 | 472631133 |
| RULE-SCRIPT-006 | 99 | 24 | 472631133 | 2676005954 |
| RULE-SCRIPT-006 | 2 | 2 | 2676005954 | 1718646187 |
| RULE-SOUND-003 | 3 | 0 | 1718646187 | 2185262696 |

The first seven draws precede the admitted map/leader state. The last is
on the menu-to-Preferences route. The known generator/seed-setter guards
remain armed throughout; no other draw or reseed is accepted. This is not
a census of all possible randomness mechanisms or a continuous seed watch.
No rebuild replay or parity comparison is performed.

### Actual paired getter return and byte store

The checked getter return names the actual loaded Preferences segment.
Its fixed entry trampoline, direct resident getter call and following AL
store are verified, as are real mode, expected DS/SS and the paired stack.
The source configuration is 68D0:0004, with sampled card ID 122 and library
mode 4. Master sound and music gate are one; slot/getter handles are FFFF.

| Boundary | Actual stop | SP | AX | Music byte |
|---|---|---|---|---|
| Getter entry | 4028:011F | FEE4 | 0 | 100 |
| Paired return before store | 641F:003E | FEE8 | 255 | 100 |
| Post-store watch | 641F:0041 | FEE8 | 255 | 255 |

DS and SS are 4D9A at each paired stop. The stored byte equals the observed
AL result; the watch is the actual music-byte change, not an execution
breakpoint substituted for a mutation. Its time is 195622786816ns. The
completed final click, positive byte/word/pointer controls and owned-watch
cleanup are required before overall, passed and value-history completion.

Both runs have overall, passed and value-history completion. Their parsed
value_history, events, draws, readback, menu_stops, menu_states and
fresh_map_ready objects/arrays are identical. Whole-file hashes and volatile
session/request metadata are not part of that comparison.

Both owned muted sessions close normally, with run_failure null and no event
journal. Their emulator PIDs 3104 and 27880 are absent after completion,
as is the machine run lock. Original-derived reports, private drives and
emulator logs remain in the licensed local store.

## Interpretation

Q-CONFIG-014 can remove the entirely unobserved fresh-startup-to-Preferences
interval for these nine ranges on this exact admitted route. It is now one
continuous observation, not a join of FND-CONFIG-245's restored interval to
FND-CONFIG-247's prefix. The music byte remains 100 until the sentinel getter
returns 255 and the Preferences entry stores that byte, consistent with
FND-CONFIG-236/238. The fixed slot/source/getter words remain FFFF throughout
the monitored interval under this value-change mechanism.

This answers neither every incoming caller nor non-sentinel driver behavior.
Retain other profiles, media, targets, reinitialization and reentrant entry,
unmonitored configuration/card and storage extents, same-value writes,
intra-instruction effects, continuous native segment/frame admission and
the remaining reader/alias/writer/driver/lifetime mechanisms in Q-CONFIG-014.
Untouched ranges are not individual positive mutation controls. The probe
ends after the entry store, before later Preferences interaction. The
drawn bar and audible output remain separate. RULE-CONFIG-003 remains
supported without complete_reading; no whole queue or parity closes.

## Alternatives

- A restored checkpoint supplied the later state: start_state is null,
  states is empty and the same physical watches remain armed from entry.
- Initial music 100 proves Preferences retains 100: the actual paired return
  is 255 and its later byte watch records 100 to 255.
- Stable sentinel handles establish all driver inactivity: only the named
  words are watched; other state and interfaces are outside this result.
- This route proves all writers or incoming transfers absent: it observes
  this admitted run, with the value-change and mechanism limits above.

## How to reproduce

Use checkout-local evidence Python with -I and
tools/research/rust-dos-runs/disc_startup.py, a new run name and
preferences-history. Completed outputs are under
GAME_DIR/captures/rust-dos-disc-startup/v181-q014-fresh-preferences-a-20261108/
and sibling v181-q014-fresh-preferences-b-20261108/.
Retain disc-startup.json, session.json, private drive and emulator logs.

Require overall, passed and value-history completion, all nine fields and
three positive changes, 54 startup snapshots, three opener events, native
map/leader readiness, eight checked draws, 24 delivered input operations and
the paired three readback phases. Require no start_state, saved state or
guest setup write. Review pins, muted output, run_failure and actual owned-
process/run-lock closure separately. No event journal is reconstructed.
FRESH-PREFERENCES-HISTORY documents the bounded route and synthetic controls.
Compare the parsed named objects/arrays listed above, not whole-file metadata.
Keep all original-derived material local.
