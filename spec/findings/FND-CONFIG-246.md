---
id: FND-CONFIG-246
title: Fresh startup watches record the music-enable clear before configuration-pointer publication
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/disc_startup.py and preferences_history.py at 8def4547; report.mjs table for source-field admission; dinorefurb-dosbox-session 1.0.0; project-local evidence Python 3.12.10
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

FND-SOUND-096 supplies point observations through the first music gate
of fresh startup. FND-CONFIG-245 supplies a later, separately restored
Preferences watch interval. This finding adds changed-value watches from
program entry through the earlier gate. It does not connect the two
separate sessions into an uninterrupted startup-to-Preferences history.

On 2026-11-08 two independent owned sessions launched the unchanged,
manifest-verified private drive with the fixed DSUN.EXE -W0 -L target.
The executable is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. No saved state, input, guest setup
write or seed mutation was supplied. Addresses, encoded sentinel words
and offsets below are hexadecimal; counts, levels and times are decimal.

### Admission and fixed field set

Both program-start stops are at 05BA:0000, emulated time 1500750ns.
The observer derives the source relocation from that actual entry and
arms nine fixed nonoverlapping physical ranges while paused, before its
first continuation. No pointer-derived watch is installed. In particular,
the configuration pointer is a watched four-byte value; a value change
does not select a new watch address.

The source-guarded table reader independently confirms the initial null
configuration pointer and driver-enable word one in the approved image,
using the established data-segment file mapping in FND-EXE-269 and the
field identities in FND-SOUND-092. The actual program-entry readings
agree with those two preconditions.

| Watched field | Width | Program entry | First music gate |
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

The last word is DS:3444, distinct from the one-byte Preferences
music gate at DS:1436. The startup field set replaces the restored
observer's derived card watch with this fixed internal enable word.
There is no card/configuration-buffer value history in this experiment.

### Two positive value-change events in the observed order

Each history has exactly the following two primary watch events:

| Field | Actual post-instruction stop | Sequence | Emulated time ns | Old | New |
|---|---|---|---|---|---|
| Internal driver-enable | 3CEE:0131 | 33 | 24820950 | 1 | 0 |
| Configuration pointer | 3CEE:00DC | 35 | 24849950 | null | 68D0:0004 |

The first stop is the known enable setter's post-store boundary
(FND-SOUND-092). The second is the outer initializer wrapper's pointer
publication boundary (FND-PARTY-320). Their actual watch payload lengths
are two and four bytes, with old/new values matching independently read
named storage. These supply positive mutation delivery for those exact
word and pointer watches, beyond watches merely being armed.

The internal enable is therefore already zero before the observed
configuration-pointer publication. Its first clear in this run must not
be attributed to a later change observed inside the profile override:
that later write can store zero over zero and be absent from a value-change
record. This does not show the override was skipped. The setter's caller
frame was not captured at the watch stop, so no new caller identity is
assigned solely from its post-store instruction address.

No other change is reported for the nine named ranges under the pinned
debugger's instruction-boundary value-change contract. Every ordinary
observation rechecks all watched values; unexplained sampled differences
abort. The observer retains all changed fields at a watch stop, requires
both positive controls, and finishes only at the declared music gate with
owned-watch cleanup. It does not record same-value writes or every
intra-instruction transient access. Untriggered ranges are not independent
positive delivery controls for each address.

### Existing startup observations remain admitted

Both runs retain 54 named startup snapshots with the same phase/frame,
configuration, callback, allocation, per-track and final-write guards as
FND-SOUND-096. Those observations still end at 3CEE:0293, before that
music-gate instruction is resumed. The endpoint is an execution breakpoint
at emulated time 25635350ns, sequence 143. The two watch stops are
additional to the 54 snapshots, not replacement synthetic phases.

The generator and seed-setter guards remain armed before continuation and
are checked before dispatching a watch event. Neither guard is reached;
the initial RNG state is one, with empty draw, input and setup-write lists.
The complete value-history objects and 54-event arrays are identical
between the independent runs. Session metadata is not part of that equality.

Both owned, muted session contexts close normally, with run_failure null.
Their emulator processes are absent and the run lock is absent after
completion. The session records have no event journal; none was reconstructed.
No host sound, native capture or rebuild comparison was performed.

## Interpretation

Q-CONFIG-014 can remove an entirely unobserved named-value prefix from
program entry through this first music gate for the fixed fresh-drive
profile. The two actual mutations give word/pointer watch controls and
their ordering; the report adds no other named-range change. This does
not prove every possible writer absent or continuous native segment identity.

The interval after this gate through seed/opener, map setup and Preferences
remains unobserved as one history. FND-CONFIG-245's independently restored
map interval cannot fill that gap merely because endpoint values agree.
Retain other configurations, roots, reentrant/indirect entry and reader
mechanisms, unmonitored configuration and storage, same-value and
intra-instruction effects, and the existing driver/writer/lifetime conditions.
RULE-CONFIG-003 remains supported without complete_reading; no whole queue
item closes and no parity promotion follows.

## Alternatives

- The first recorded enable clear follows configuration publication:
  its earlier sequence and emulated time precede that pointer event.
- No later enable event proves the profile override absent: unchanged
  zero writes are invisible to this changed-value mechanism.
- Startup completion means reaching the map/getter: this fixed observer
  stops at the first music gate without resuming it.
- Matching separate histories prove one continuous run: their later
  intervening interval is still unobserved.

## How to reproduce

Use the checkout-local evidence Python with `-I` and
tools/research/rust-dos-runs/disc_startup.py, a new run name and history.
The two retained outputs are under GAME_DIR/captures/rust-dos-disc-startup/
as v181-q014-startup-history-a-20261108 and
v181-q014-startup-history-b-20261108. Require both complete markers,
nine fixed fields, the two positive events, all 54 startup snapshots and
the exact endpoint, with no draws, input or guest setup writes.

Compare the parsed value_history objects and events arrays, not whole-file
metadata hashes. Check the session's emulator/package/media/audio pins,
fresh source preparation, run_failure and actual owned-process closure.
The DISC-STARTUP-OBSERVATION plan documents the bounds and synthetic controls.

The source admission config and numeric result remain under
GAME_DIR/analysis/startup-history-admission-20261108/. Reproduce with the
existing fingerprint-guarded table reader: one row at file 50411, stride
53 decimal, pointer field offset zero/width four and enable field offset
51 decimal/width two. It checks field values, not native memory continuity.
Keep all original-derived reports, private drives and runtime records local.
