---
id: FND-CONFIG-247
title: Fresh startup watches remain active through the checked native seed call to the opener boundary
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/disc_startup.py and opener_history.py at 5b4d68ed; preferences_history.py; dinorefurb-dosbox-session 1.0.0; checkout-local evidence Python 3.12.10
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

FND-CONFIG-246 ends its fresh named-value history at the first music gate.
FND-CONFIG-245 observes a later, separately restored map-to-Preferences
interval. This observation extends the former history through the native
seed call to the pre-opener call, without restoring or joining sessions.
FND-UI-039 supplies the static opener argument/call interpretation and the
earlier native route; startup_window.py supplies the checked seed caller.

On 2026-11-08 two independent fixed opener-history runs used the unchanged, manifest-
verified private drive, target DSUN.EXE -W0 -L and read-only game.ins media.
The approved executable is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. No input, checkpoint restore, guest setup
write or supplied seed was used. Addresses and sentinel words are hexadecimal;
counts, levels, seeds and nanosecond times below are decimal.

### One continuous fixed-field observation

The program entry is 05BA:0000 at 1500750ns. Before its first continuation,
the observer arms the same nine fixed nonoverlapping physical ranges as
FND-CONFIG-246. No watch follows the configuration pointer. All 54 guarded
disc startup snapshots remain, with the first music gate at 3CEE:0293,
sequence 143 and time 25635350ns. At that gate the execution breakpoints
for those fourteen startup sites are removed; the watches and known RNG/
seed guards remain installed. No watch is removed and rearmed across this
handoff. All ordinary later stops recheck the nine values.

The same two primary mutations occur: internal enable one to zero at
3CEE:0131, sequence 33, time 24820950ns, and configuration pointer null to
68D0:0004 at 3CEE:00DC, sequence 35, time 24849950ns. Their positive watch
payloads have widths two and four respectively. There are no further
reported changes to these ranges before the opener call.

| Watched field | Width | Program entry | Music gate and opener call |
|---|---|---|---|
| Master sound gate | 1 | 1 | 1 |
| Music gate | 1 | 1 | 1 |
| Music-level byte | 1 | 100 | 100 |
| Configuration pointer | 4 | null | 68D0:0004 |
| Driver slot | 2 | FFFF | FFFF |
| Source sequence handle | 2 | FFFF | FFFF |
| Getter sequence handle | 2 | FFFF | FFFF |
| Library mode | 2 | 4 | 4 |
| Internal driver-enable word | 2 | 1 | 0 |

These are changed instruction-boundary value observations under the pinned
debugger's watch contract, not records of unchanged writes, every transient
access, continuous segment identity or unmonitored configuration contents.
Only the two actually mutated ranges supply positive delivery controls.

### Native seed entry, return and opener

The observer accepts only one native seed-setter entry, with its actual
six-byte return/argument frame. It checks the startup overlay-180 trampoline
at relocated 56B2:0020, whose target offset is 0141, against the return
segment and required return offset 0663. The instruction before that return
is checked as the direct far call to the relocated resident seed setter.
The frame's word argument is read from the game, never written by the probe.

| Boundary | Actual stop | Sequence | Time ns | Seed | SP |
|---|---|---|---|---|---|
| Seed entry | 05BA:0811 | 160 | 697542616 | 1 | FE54 |
| Checked native return | 611B:0663 | 163 | 697543083 | 44976 | FE58 |
| Before opener call | 1D35:02E4 | 166 | 129199843550 | 44976 | FF52 |

DS and SS are 4D9A at all three stops, and each stop is real mode with
reason breakpoint. The seed frame names return 611B:0663 and argument
44976. The returned four-byte RNG state equals that argument. At the
opener boundary the argument at SS:SP is the word one. The call has not
executed, so this is not evidence of the opened window or later map state.

The known generator guard is never reached; the draw list is empty. The
seed-setter guard remains installed after the accepted call, and a second
entry would fail. Initial, seed-entry, return and opener reads account for
the sampled seed values; there is no continuous seed-word watch or census
of other randomness mechanisms. The nine fixed watches remain active
during the native seed call and cinematic continuation.

Both runs have overall and value-history completion. Their parsed value_history
objects and 54-event startup arrays are identical. The three later boundaries
also match in phase, instruction, sequence, emulated time, DS/SS/SP, seed,
caller/argument and named values. Volatile session/request metadata is not
included in that latter comparison. Both owned sessions close normally with
muted host output, run_failure null and no event journal. Their emulator PIDs
22620 and 23100 are absent after completion, as is the machine run lock.

### Diagnostic limit

The first attempted route with tooling 0c4625fe ended incomplete after its
short two-observation budget. Its pause was a request stop at 6377:03B6,
time 60780000016ns, not the opener. The report retains the checked seed
entry/return but has neither overall nor value-history completion. The owned
session closed; the same operation was not restarted after exhaustion.
Tooling 5b4d68ed uses startup_window.py's existing ten thirty-second bounded
observations per continuation, retaining the original operation handle.
An exhausted observation still fails rather than claiming a destination.

## Interpretation

Q-CONFIG-014 can remove the entirely unobserved first-music-gate-to-opener
interval for these nine fields on this fixed fresh route. It is connected
to program entry by watches that remain armed, rather than by matching
endpoints from independently restored sessions. The music-level byte is
still 100 and getter handle FFFF before the opener call.

The opener has not run. Its continuation through character creation/map
setup and Preferences remains unobserved as one history; FND-CONFIG-245
cannot bridge that gap by itself. Other configurations, callers and
reentrant/indirect entry or reader mechanisms, non-sentinel driver results,
unmonitored data, actual storage extents, same-value/intra-instruction effects
and the existing writer/lifetime obligations remain. RULE-CONFIG-003 stays
supported without complete_reading. No whole queue item or parity closes.

## Alternatives

- Matching disconnected endpoints establish continuity: here the same nine
  physical watches stay armed in one session; this does not extend beyond
  the observed opener boundary or connect the separate restored-map run.
- The changed RNG state is an unrecorded generator draw: the checked native
  setter receives word 44976 and returns that state; the known generator
  guard is not reached. Other seed writers are not globally excluded.
- The first timed-out attempt reached the opener: its reason, address and
  incomplete markers explicitly reject that conclusion.
- Stable game handles prove no driver or memory activity: only the listed
  ranges are monitored, with the value-change limits above.

## How to reproduce

Use checkout-local evidence Python with -I and
tools/research/rust-dos-runs/disc_startup.py, a new run name and opener-history.
The retained attempts are under GAME_DIR/captures/rust-dos-disc-startup/:
v181-q014-opener-history-a-20261108 is the incomplete short-budget diagnostic;
v181-q014-opener-history-b-20261108 and
v181-q014-opener-history-c-20261108 are the completed corrected-budget routes.
Retain disc-startup.json, session.json, private drive and owned-session logs.

Require overall and value_history completion, nine fields, two positive
mutations, 54 startup snapshots, all three opener_events and the checked
seed argument/result and opener word one. Require empty draw/input/setup-
write lists. Review actual pins, muted output, run_failure and process/lock
closure separately from the observation report. No event journal is rebuilt.
The DISC-STARTUP-OBSERVATION plan and synthetic check_opener_history.py
document the fixed route and refusal controls. Keep derived records local.
