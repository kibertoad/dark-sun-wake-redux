# Runtime access

What can be done with the original game running, and who can do it. The
`runtime-access` skill keeps this file current; see the
[work protocol](../vendor/upstream/work-protocol.md#runtime-access) (lines 42-62). This
file says what is true now: replace an answer when a tool, emulator, machine or
owner rule changes it. Findings from runs go in `spec/`, never here.

Static analysis is the main source of evidence. The owner's rule
(`AGENTS.md`, "Runs of the original") lets a coding agent run the original
only through the toolkit's `dinorefurb-dosbox-session` package, in an
emulator process the package starts: rust-dos v1.8.1 through `RustDosSession`.
Session 1.0.0 supports rust-dos only; DOSBox-X is no longer an available
package backend. Host sound stays muted with `--audio-output null`,
the emulated sound devices stay configured, `keep_host_sound` is never passed
and `nosound` never set. Agents never operate GOG's DOSBox 0.74-2; runs of it
are live sessions the owner plays and captures alone. The package takes the
machine's run lock (`C:\ProgramData\refurbished-dinosaurs\run.lock`, or the
path in `REFURBISHED_DINOSAURS_RUN_LOCK`) as the protocol's
[Running the original](../vendor/upstream/work-protocol.md#running-the-original) (lines 421-455)
says.

Setup: `./tools/Restore-ToolDependencies.ps1` installs
`dinorefurb-dosbox-session` 1.0.0 into `artifacts/evidence-python`, and
`./tools/Install-RustDos.ps1` installs the pinned rust-dos v1.8.1 Windows
build into `artifacts/rust-dos/`, refusing an archive or executable whose
SHA-256 differs from the package's pins.

The historical per-part attempts below retain their rust-dos v1.7.2 and
session-version provenance. The v1.7.5 attempts below remain historical;
an untested part is not admitted by a historical success. A save state
records the emulator SHA-256, and the approved session package refuses a
different digest before starting restore. Native cross-release format
compatibility has not been tested: the package guard alone does not prove
the old state format incompatible. Historical checkpoints are preserved.

On 2026-11-04, session 1.0.0/rust-dos v1.8.1 passed the fixed
startup_segments.py startup-to-main attempt with its RNG guard. This verifies
owned process startup, declared breakpoint stops and the probe's memory reads,
with host output recorded muted. The local run is
GAME_DIR/captures/rust-dos-startup-segments/runtime-upgrade-20261104-v181/.
On the same date, startup_window.py regenerated the opener checkpoint under
these pins, and restore_opener.py restored it in a second owned process at
the declared opener call and reached its return. Both saves used file checks;
the return retained the restored RNG seed, with no inputs, setup writes or
RNG draws. Reproduce with the project-local evidence Python:
`startup_window.py v181-q046-opener-20261104`, then
`restore_opener.py v181-q046-opener-20261104 v181-q046-opened-20261104`
(scripts under tools/research/rust-dos-runs/; choose new run names on repeat).
Reports and checkpoint sidecars are retained under
GAME_DIR/captures/rust-dos-startup-window/v181-q046-opener-20261104/ and
GAME_DIR/captures/rust-dos-restore-opener/v181-q046-opened-20261104/.
The opener attempt alone admits only that bounded restore/continuation.
Subsequent current-pin regeneration reached Start (window 19500), then a
separate run delivered the declared mouse move and left click and reached
Create (window 11500). Each readiness gate required matching successive
named-field snapshots, ready flags and mouse enabled; neither run made setup
writes or recorded RNG draws. Both saved states passed verify_checkpoints.py:
new-process instruction/time, named fields and frame matched, same-process
save/load matched, and a sound-IRQ mismatch was refused before launch.
Reproduce with regenerate_checkpoints.py arguments
`v181-q046-start-20261104 start v181-q046-opened-20261104` and
`v181-q046-create-20261104 create v181-q046-opened-20261104`, then
verify_checkpoints.py arguments
`v181-q046-start-20261104 v181-q046-verify-20261104 start` and
`v181-q046-create-20261104 v181-q046-create-verify-20261104 create`.
Reports, frames and sidecars are under GAME_DIR/captures/rust-dos-regenerated/
with those run names (verification directories append the state name).
Current admission therefore includes these two states, their named memory
reads, frame capture, strict restores and this mouse move/left-click route.
It does not by itself establish right click, keyboard or gameplay.
The subsequent v181-q046-play-20261104 run used the existing `play` stage
from v181-q046-opened-20261104. It confirmed the prescribed right/left mouse
operations and Esc delivery, reached mode one with window 19501 and occupied
slots 1 and 3, and stopped at the native input builder. The mode-zero interval
eventually ended within the existing budget (571 total snapshots); intermediate
registration and delivered Esc were not accepted as gameplay readiness.
The run recorded three RULE-SCRIPT-006 draws and one RULE-SOUND-003 draw.
The `play-resume` stage in v181-q046-play-ready-20261104 restored that run,
delivered key 2 and verified leader 1 at the input builder, recording three
more RULE-SCRIPT-006 draws. Neither run made setup writes. The existing RNG
recorder checked every admitted draw's result and next seed; this is not a
rebuild replay or parity comparison. Both run directories are under
GAME_DIR/captures/rust-dos-regenerated/.
verify_checkpoints.py arguments
`v181-q046-play-ready-20261104 v181-q046-play-ready-verify-20261104 play-ready`
passed new-process instruction/time, fields/frame, same-process roundtrip and
sound-IRQ mismatch refusal. The final directory appends `-play-ready` to the
verification prefix. These attempts admit the tested two-character map state,
right click and the Esc/key-2 gestures under current pins. They do not admit
other keys or Save/Load entry by themselves. The subsequent fixed F2 attempt
in FND-COMBAT-133 verifies delivery and Load-entry readiness from this map
checkpoint, plus the named dispatch-field reads in load_entry.py. Its report
and checked load-entry state remain under
GAME_DIR/captures/rust-dos-load-entry/v181-q046-load-entry-20261104/.
This admits that entry observation, not a numbered load, Save action, every
dispatch target or independent restore of the new Load checkpoint.
The fixed `down` stage additionally observes Down's key-down event at the
actual global call and Load callback entry (FND-COMBAT-134), then stops.
Its pending release is not admitted as completed delivery. Scoped output is
GAME_DIR/captures/rust-dos-load-entry/v181-q046-load-down-scoped-20261104/;
this does not admit navigation outcome or a numbered load.
The `down-return` stage in FND-COMBAT-135 admits one returned empty-list
navigation case, with matching far-call stack identity and bounded marker
reads. Output is GAME_DIR/captures/rust-dos-load-entry/v181-q046-down-return-frame-20261104/.
It stops before dispatcher cleanup; key release remains pending and no
pending-input checkpoint is saved. Other navigation cases remain unverified.
The `load-call` stage in FND-COMBAT-136 admits Enter key-down through the
Load callback to numbered-load +05ED, with a verified loaded call/return
frame and row argument. Output is
GAME_DIR/captures/rust-dos-load-entry/v181-q046-load-call-20261104/.
The loader body is not executed and Enter release remains pending; this
does not admit reading or loading a saved game.
The `copy-inputs` stage in FND-COMBAT-137 additionally admits the fixed
continuation to the copy helper's entry, its declared frame and bounded
terminated-path reads. Enter delivery completed by this stop. Output is
GAME_DIR/captures/rust-dos-load-entry/v181-q063-copy-inputs-20261104/.
The copy body is not executed; no normalization, file identity or I/O
outcome is admitted by this attempt.
The `copy-return` stage in FND-COMBAT-138 additionally admits this copy
invocation's internal normalization stop and matched return, with bounded
private working-file metadata at entry/return. Output is
GAME_DIR/captures/rust-dos-load-entry/v181-q063-copy-return-20261104/.
It does not admit other copy outcomes or the wrapper's later continuation.
The fixed `load-return` stage in FND-COMBAT-139 admits the observed rejected-copy
wrapper/settings continuation and matched numbered-load return, using current
caller-frame/loaded-code mappings. Output is
GAME_DIR/captures/rust-dos-load-entry/v181-q063-load-return-verified-20261104/.
The extended bound is 1024 observations; this run used 534. A saved return-segment
watch was armed but never triggered, so mutation delivery remains unverified.
No successful save restoration or later action cleanup is admitted by this result.
The fixed native_save.py F1/Enter route in FND-COMBAT-140 admits one Save
entry/return, private native-file creation and post-Save keyboard-loop
checkpoint. A11D may be null there; the bounded tagged map head, global
hook and native input builder are independently checked. Native saved-map
creation and new-process verification runs are
GAME_DIR/captures/rust-dos-native-save/v181-q063-native-save-input-20261104/ and
GAME_DIR/captures/rust-dos-native-save/v181-q063-native-save-verify-20261104/.
Instruction/time, named state, frame and native-file digest matched at
restore and same-process roundtrip. The narrow saved return-segment watch
actually handled ten relocations. Whole-game loading of the new native save
remains unverified; its bounded F2/copy route is described below. No patched
native save was used.
The fixed saved-copy-return stage in FND-COMBAT-141 admits actual F2/Enter
from that native candidate through the observed nonempty copy return, with
bounded normalization/open/length and private-file-state checks. Output is
GAME_DIR/captures/rust-dos-load-entry/v181-q063-native-copy-truncate-20261104/.
This extends the candidate's admission for that route only;
archive restoration and other copy outcomes remain unverified.
The fixed saved-reader-entry stage in FND-COMBAT-142 admits continuation
through the declared region-preparation call and Borland header-trap/IRET
return to the selector-zero reader's first instruction. Output is
GAME_DIR/captures/rust-dos-load-entry/v181-q064-reader-trap-20261104/.
Its narrowly watched return field is removed before interrupt-frame reuse.
Only the named archive-state snapshots are admitted: no SAVE request,
payload restoration, provider identity or completed loading was observed.
The fixed saved-providers stage in FND-COMBAT-143 extends that route to
actual SAVE/24 and SAVE/26 pre-preparation provider/length selection, with
a matched successful SAVE/24 core result. SAVE/26 transfer, payload/file
identity, all fallback paths and completed loading remain unverified.
Output is GAME_DIR/captures/rust-dos-load-entry/v181-q064-providers-scoped-20261104/;
the earlier v181-q064-providers-20261104 diagnostic remains under that parent.
The fixed saved-payloads stage in FND-COMBAT-144 observes both target core
returns and byte equality with their selected private working-file spans.
Output is GAME_DIR/captures/rust-dos-load-entry/v181-q064-payloads-20261104/.
It stops at SAVE/26's core return: later preservation, unique file-handle
ownership, other paths and completed loading remain unverified.
The fixed saved-origin stage in FND-COMBAT-145 observes the working-path
archive-open entry, I/O result and publication, then matches both selected
provider/handle tuples and plain directory rows to that publication and
unchanged copied-file rows. Output is
GAME_DIR/captures/rust-dos-load-entry/v181-q064-origin-20261104/.
Continuous lifetime, full graph/directory identity and later loading remain
unverified; the run still ends at SAVE/26's core return.
The fixed saved-reader-return stage in FND-COMBAT-146 extends that route
to the full selector-zero reader's matched caller return. Both bounded
target spans match cached transfer-time references and their source-slot
bindings match at return. Output is
GAME_DIR/captures/rust-dos-load-entry/v181-q065-reader-return-20261104/.
No native reader return relocation occurred; trap handling has synthetic
controls. Continuous lifetime, child attribution and completed loading
remain unverified. The probe stops before the caller consumes AL.
The fixed saved-map-request stage in FND-COMBAT-148 admits a region-open
publication, actual GMAP/50 selected row/transfer and full map comparison
with the saved-plane prediction at matched reader return. Output is
GAME_DIR/captures/rust-dos-load-entry/v181-q066-map-request-20261104/.
The sampled map, planes and effect spans do not overlap; continuous backing,
other paths and completed loading remain unverified. No guest setup writes
are introduced, and the probe still stops before the caller consumes AL.
The fixed saved-objects stage in FND-COMBAT-150 admits a pre-loop marker
census, 125 matching flags-zero loader entries/epilogues and selected
sixteen-byte OJFF local spans with matched core results. Output is
GAME_DIR/captures/rust-dos-load-entry/v181-q066-objects-20261104/.
It reports only the fixed named child entries and retains map/effect endpoint
checks. Temporary-slot allocation, other object/failure states, continuous
lifetime and completed loading remain unverified.
The fixed saved-items stage in FND-COMBAT-153 admits the before-clear,
enumeration entry/epilogue and reader-return boundaries for one native
400-record item table. Three nonzero cache words become zero; the complete
9200-byte clear prediction matches at all later boundaries. Enumeration
returns zero without reaching either monitored traversal helper. The
sampled item/output/effect/local spans are disjoint; populated traversal,
continuous lifetime and other aliases remain open. Its accepted report is
GAME_DIR/captures/rust-dos-load-entry/v181-q066-items-20261104/.
Reproduce with load_entry.py v181-q063-native-save-input-20261104
<fresh-output-name> saved-items using the pinned project interpreter.

The fixed saved-script-conversion stage in FND-COMBAT-152 admits native
GPLI size/data selection, retained scan inputs and DS/SS/chain/trigger
bindings, with predicted conversion fields matching before release and
at matched reader return. Output is
GAME_DIR/captures/rust-dos-load-entry/v181-q066-script-bound-20261104/;
the earlier v181-q066-script-conversion-20261104 observation is retained.
The stage still stops before the reader caller consumes AL; file provenance,
continuous lifetime, other inputs/failures and completed loading remain open.
Guarded writes were not reattempted under these new pins.
Historical capabilities below retain their original version scope.
The previous emulator is preserved at artifacts/rust-dos-v1.7.5; old state
identities are retained, not rewritten to bypass strict matching.

On 2026-10-19 the pinned v1.7.5/session 0.11.0 probes regenerated opener,
opened and all six retained gameplay routes (FND-UI-054). Start,
Added, Drop confirmation, After Drop, Talk and Play each passed a new-process
strict restore with file checks and matching instruction/time, named fields
and frame, a same-process save/load roundtrip and a deliberate sound-IRQ
mismatch refusal before launch. Play's final state is named play-ready;
it has occupied slots 1 and 3 and leader 1. Captures and state sidecars
are under GAME_DIR/captures/rust-dos-regenerated/, with exact successful
names and commands in FND-UI-054. The historical gameplay states actually
remain in ignored artifacts/runs/, as their findings name.

Historical v1.7.5 admitted parts: agent launch/state setup for those six states;
left/right mouse, memory reads, breakpoints and frame capture. Movement
and click are separate completed operations, and window/party predicates
gate actions. The conversation gate also reaches its native event builder,
because a registered current window can precede its visible panel.
Esc returned the two-character setup to Start and key 2 selected leader 1,
both with confirmed delivery.
Other keys, middle button, held modifiers, guarded writes, sound capture,
patched native saves and original recording playback retain their separate
historical provenance and need their own current attempt before reliance.

The following fixed-probe summaries retain their historical v1.7.2 scope.
Current machine availability (2026-10-15): the fixed startup-segment probe
successfully acquired the default machine lock, launched its own pinned
rust-dos, stopped at game-main entry, read registers and RNG state, and
closed the session (FND-PARTY-219). The earlier attempt was refused by an
unrecognized lock; later read-only inspection reported no lock. No lock was
modified and no alternate path used. A later run must still take the normal
lock and refuse if another session holds it.

`tools/research/rust-dos-runs/startup_segments.py` waits for the documented
main entry by address, rather than fixed emulated time, and guards the known
RNG entry. It makes no writes, inputs or image captures. It copies only
DSUN.EXE onto C: and therefore must not be continued to gameplay. Its
successful startup stop verifies this fixed probe, not a state-driven route
to a gameplay screen or deletion call. The earlier table's menu/input/frame
attempts retain their separate provenance.

The fresh-drive startup_window.py attempt (FND-UI-039) also reached main's
start-window opener call by address without input. It verifies this later
fixed stop and its stack-argument read. The opener was not executed; a
drawn window and subsequent state-driven gameplay route remain unverified.

The seed-and-opener extension (FND-RNG-010) additionally verified the fixed
seed caller/argument reads and return stop, then saved a strict, file-checked
opener checkpoint. read_saved_state verified its journal and files afterward.
FND-UI-040 then actually restored this opener checkpoint in a new owned
session, verified the load stop and ran its declared opener call to return.
It saved and file-verified an opened checkpoint afterward; native restore
of that second state remains unattempted. No displayed pixels or input
readiness were established by the call returning.

clock_source.py (FND-RNG-014) verified the declared startup clock caller,
service/converter return stops and bounded near-buffer/pointer reads on
this machine. It used no input or writes and stopped before seeding.
It extends the fixed read/stop capability record, not the gameplay route.
The extension in FND-RNG-017 also reached the declared environment lookup,
initializer and daylight returns and read their fixed name/global records.
Only the zero-lookup default startup path is admitted; no environment mutation
or broader gameplay capability follows.
FND-RNG-020 additionally verifies the constructor's near initialization frame,
requested-size/root metadata and terminated pointer array, then the separate
startup timezone far frame. Its repeated final run passed with no input or
writes; it reads no environment-string contents and admits at most 1024
pointer words. This extends only the fixed startup observation contract.

How an agent run starts the game (2026-10-14, `tools/research/rust-dos-runs/`):
a deterministic run at 15000 instructions a millisecond, C: a copy of the
installed game's files, D: the CD image `game.ins` mounted read-only, and the
target `DSUN.EXE -W0 -L` as `RAVAGER.BAT` gives it. The installed `SOUND.CFG`
names a card at 0x220, IRQ 5 and DMA 1. With rust-dos's default card at IRQ 7
the game stops at once with `Mel Fatal Error #: 26` and `DSP Detect Fail` (exit
code 0x1A), so the run gives rust-dos a Sound Blaster 16 at 0x220, IRQ 5, DMA 1
and HDMA 5 and no Ultrasound (`RustDosConfig.sound_blaster` and `ultrasound`,
dinorefurb-dosbox-session 0.10.0), and C: holds the installed `SOUND.CFG`
unchanged. The program loads with
its first segment at `05BA` (the repository's load image puts it at `1000`).

## BLD-GOG-EN-1.1

How it runs: GOG's DOSBox 0.74-2 on Windows 11, with `dosbox_darksun2.conf`
and `dosbox_darksun2_single.conf` (`machine=svga_s3`, `memsize 16`,
`cycles fixed 15000`, `sbtype sb16`), started through `RAVAGER.BAT`, as the
build entry and the environment of the owner's captures record.

Emulator harness: tools/emu/resident-call.mjs with Unicorn 2.1.4, restored
from hash-locked test-only wheels. The published executable-reader loads the
MZ resident image and source relocations. A declared far, no-argument root can
be called without a game process or run lock. Raw memory and argument seeding
wait for supported layout/parameter bindings; FBOV code stays out of reach.

The matrix below records the previous runtime attempts. For current pins,
only the bounded startup/breakpoint/memory attempt above is newly verified.
Revalidate any further required part before admitting a new run.

| Capability | Who | Tried | What would change it |
|---|---|---|---|
| Start it and bring it to a given state without a person | agent, tested states only | 2026-10-19, pinned v1.7.5/session 0.11.0: fresh Start, Added, Drop confirmation, After Drop, Talk and two-character Play; all independently restored (FND-UI-054). | A required state outside the verified routes. |
| Send mouse input | agent, left/right only | 2026-10-19: separate completed motion and click operations reached character and conversation controls (FND-UI-054), using virtual 320 x 200 coordinates. Middle button and held modifiers have only v1.7.2 provenance (FND-PARTY-204, FND-PARTY-206) and are not currently admitted. | A current attempt for each additional gesture. |
| Send keyboard input | agent, Esc and 2 only | 2026-10-19: Esc returned from Create Characters to Start; 2 selected occupied slot 1 as leader in Play (FND-UI-054). Other keys and held modifiers retain historical provenance only. | A current attempt for another required key or modifier. |
| Read memory and set breakpoints | agent, named fields and declared stops | 2026-10-19: current window/handler, party kinds/box records, seed, callback arguments and strict restore fields were read; generator, setter and native input stops were reached (FND-UI-054). 2026-10-25: once-breakpoints on all 854 overlay trampolines at a time, with return addresses read from the stack, ran from play-ready (FND-EXE-632); breakpoints kept armed on indirect transfers, with operand addresses formed from the stop's registers, and memory reads of saved vectors (FND-RNG-065); write watchpoints over 115 ranges of 1 to 4 bytes, with the stack walked at a stop (FND-RNG-067); armed breakpoints counting 13 branch addresses, at most 50 hits each because every stop opens new TCP connections and a run with uncapped counts used up the local ephemeral ports (FND-RNG-075). | A field or stop outside the tested contract. |
| Write supported memory fields | none, current pin untested | Earlier guarded hit-point writes are recorded by FND-PARTY-212 under v1.7.2. The fresh probes make no writes. | A current guarded-write capability attempt. |
| Load a patched save | person | Not tried. The owner can place a save in the installation's save slots and load it from the game's menu. | A live session that asks for it. |
| Capture frames and sound | agent, frames only; person, frames only | 2026-10-19: `screenshot()` produced 640 x 400 PNGs and independent strict restores reproduced the checked frames (FND-UI-054). Host audio stayed muted; sound capture is untested. Owner screenshots retain their own DOSBox provenance. | A sound capture from a session-package run. |
| Play back a recording the original made | none | The game has no recording feature the spec knows of, and DOSBox 0.74-2 does not replay input. | A finding that the game records and replays input. |
| Call a single function in the emulator harness (no run lock) | agent, resident far roots within the current no-argument contract | Unicorn 2.1.4 loads the hash-verified GOG resident MZ and applies its relocations. FND-CONFIG-193 initializer controls return with source-written roots/free flags, unchanged loaded high bytes and restored registers. | Supported named parameter/layout bindings extend input cases. Overlay code, native hardware and timing remain outside this harness. |


Checkpoint-regeneration probes: regenerate_checkpoints.py and verify_checkpoints.py,
with exact commands and capture paths in FND-UI-054. The latter compares
native restore fields and frames; this is not rebuild parity validation.

Historical probe: `tools/research/rust-dos-runs/run_plan.py <run-dir> <plan.json>`, run with
the evidence Python and `GAME_DIR` set to the installed build (README beside it).
It follows a plan of inputs from a checkpoint, records each draw of `rng` as
`{rule, bound, result}` (script instruction `0x52` under RULE-SCRIPT-006,
`random_mod` under the rule of its calling routine), checks each result against
the original's, can write a seed into `rng_state`, and reads memory and takes
screenshots at its stops. It waits for emulated times of a deterministic run,
not for states the game reaches. EXP-PARTY-001 is its first experiment. It
follows the protocol's
[Recorded runs](../vendor/upstream/work-protocol.md#recorded-runs) (lines 431-443). Each emulated-call setup must document port
models, video memory substituted with RAM and the limits of each comparison,
within the current resident-call contract below.

## Resident-call stubs and limits

The harness permits named interrupt stops only. It cannot assume an unknown
handler returned or retained memory/registers. Port substitutions separately
name read/write direction, width and the glossary name of each supplied value.
An undeclared interrupt, port, code path or instruction boundary stops the call;
an FBOV overlay interrupt cannot be supplied as a stub. Synthetic controls cover
these failures. The actual initializer reaches no hardware service or port.

Loaded memory comes from the resident MZ, with synthetic zero-filled RAM outside
it and a harness-owned far-return frame. Video addresses are ordinary RAM; no
pixels, device state, sound, input or timer are emulated. Instructions, writes,
observations and branch-direction coverage are bounded and recorded locally.
The initializer controls seed no original memory field. No game claim/parity
status or startup-to-transfer/caller admission changes merely from these tests.
See tools/emu/README.md and docs/VALIDATION.md.

Output retention follows [ANALYSIS-OUTPUTS](ANALYSIS-OUTPUTS.md). [WINDOWS-PROBES](WINDOWS-PROBES.md) is a reference for Windows guest projects and changes none of this DOS restoration's recorded capabilities or capture restrictions.
