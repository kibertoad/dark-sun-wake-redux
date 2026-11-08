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
[Running the original](../vendor/upstream/work-protocol.md#running-the-original) (lines 437-471)
says.

Setup: `./tools/Restore-ToolDependencies.ps1` installs
`dinorefurb-dosbox-session` 1.0.0 into `artifacts/evidence-python`, and
`./tools/Install-RustDos.ps1` installs the pinned rust-dos v1.8.1 Windows
build into `artifacts/rust-dos/`, refusing an archive or executable whose
SHA-256 differs from the package's pins.

preferences_readback.py passed twice on 2026-11-08 under current pins, in
captures/rust-dos-preferences-readback/v181-q014-preferences-e-20261108/ and
v181-q014-preferences-f-20261108/. Both strictly restored the checked
v181-q046-play-ready-20261104 checkpoint, delivered Tab/motion/left-click
through the ready Game Menu and native BUTN/10303 selection, and paired the
known getter's actual caller, return and byte store. Those fixed restore,
input, named-read and paired-boundary parts are agent. Each recorded the
same one admitted sound-rule draw and made no guest setup write. Both owned
sessions closed and released their locks. FND-CONFIG-238 records behavior;
PREFERENCES-READBACK-OBSERVATION records the scope and command. This admits
no uninterrupted fresh-startup history, other callers/driver paths, later UI,
audible output or rebuild parity.

The optional fixed `history` mode passed twice under current pins on
2026-11-08, in sibling v181-q014-history-a-20261108/ and
v181-q014-history-b-20261108/. Strict restores and the same admitted input
route armed nine named nonoverlapping value-change ranges through the paired
store. Both reports have one positive music-byte watch event at the exact
post-store boundary, matching final paired snapshots, complete watch cleanup
and no guest setup writes. This fixed byte-watch delivery and associated
named reads are agent. Untriggered word/pointer ranges are not independent
positive mutation-delivery tests. No fresh-startup, arbitrary-address,
unchanged-write or intra-instruction history capability is admitted.
FND-CONFIG-245 records behavior and limits; the observer plan gives the command.

Retain the sibling v181-q014-preferences-a/b/c/d-20261108 reports as failed
capability attempts. The a attempt used the modal-choice poll and exhausted
its same-operation observation budget. The b attempt exhausted 512 builder
stops before click completion. The c attempt stopped on native selection
not yet updated after motion delivery. The d attempt reached the getter but
mistook the overlay header prefix for the actual entry stub. None supplied
a getter/store pair. All closed and released their locks. Current tooling
verifies main's builder call, waits separately for native selection, removes
the poll before clicking, and binds the actual Preferences entry trampoline.

The fixed read-only disc_startup.py observer passed twice on 2026-11-08,
in captures/rust-dos-disc-startup/v181-q014-disc-startup-a-20261108/ and
v181-q014-disc-startup-b-20261108/. Fresh unchanged manifest-verified drives
recorded 54 events at named initializer/callback/metadata/allocation/table-write
and music-gate stops with the required real-mode DS/SS and return guards.
These fixed startup, named-field and paired-write observation parts are agent
under the current pins. Both RNG/seed guards stayed unhit; no input, guest
writes, draws or captures occurred. Both owned sessions closed and released
their lock. This admits neither later startup/getter access, continuous
preservation, heap capacity nor audible output. FND-SOUND-096 records results;
DISC-STARTUP-OBSERVATION records the fixed scope and command.

disc_startup.py's optional fixed `history` mode passed twice on 2026-11-08,
in sibling v181-q014-startup-history-a-20261108/ and
v181-q014-startup-history-b-20261108/. From actual program entry to the
existing first music gate it armed nine fixed fields, including internal
enable and configuration pointer, with no derived watch target. Both runs
retained 54 guarded startup snapshots and two positive, matching two-/four-
byte watch events, then removed their owned watches and closed normally.
This fixed word/pointer mutation delivery and named prefix reads are agent.
No input, restored state, draw, guest write or seed mutation occurred.
The later startup/map/getter interval and unchanged/intra-instruction or
unmonitored-state histories remain unadmitted. FND-CONFIG-246 records the
exact ordering and limits; DISC-STARTUP-OBSERVATION gives the fixed command.

The fixed `opener-history` mode additionally passed twice on 2026-11-08 in
siblings v181-q014-opener-history-b-20261108/ and
v181-q014-opener-history-c-20261108/. Its same nine watches stay active from
fresh entry through the checked native seed call/return to the opener call,
without a checkpoint restore, input or guest setup write. The native seed
argument/result and opener argument reads, uninterrupted named watches and
fixed continuation are agent under these pins (FND-CONFIG-247). This is no
continuous native segment/seed-word history or later opener/map/getter access.
The earlier sibling a attempt is incomplete: its short observation budget
ended at a request stop during the cinematic interval. Corrected tooling
5b4d68ed uses the existing startup-window bounded same-operation budget;
neither exhaustion nor a pause is treated as reaching the opener. All three
sessions closed with muted host output; derived logs remain local.

The fixed `preferences-history` mode passed twice on 2026-11-08 in siblings
v181-q014-fresh-preferences-a-20261108/ and
v181-q014-fresh-preferences-b-20261108/. Nine watches stay armed from
fresh entry through native seeding/opener, the existing two-character setup,
mode-one/leader-one/input-map readiness and paired Preferences getter/store.
All 24 inputs complete and eight caller-checked RNG draws are recorded;
no restore, checkpoint, supplied seed or guest setup write is used. The
music byte changes 100 to 255 at the actual paired store, supplying a positive
byte control beside the startup word/pointer controls (FND-CONFIG-248).
This exact continuous route and its declared reads/gestures are agent under
current pins. Other routes, unmonitored/unchanged/intra-instruction effects,
continuous native segment/frame identity, audible output and later Preferences
interaction remain unadmitted. FRESH-PREFERENCES-HISTORY documents the bounded
command and controls. Both owned muted sessions closed normally.

The strict restored-map Preferences command's fixed `parent` mode passed
twice on 2026-11-08 in captures/rust-dos-preferences-readback/
v181-q014-parent-a-20261108/ and v181-q014-parent-b-20261108/. It retains the
existing checked play-ready restore, history/input/RNG and paired-store controls,
and additionally samples menu+025E/+0281, the resident Preferences trampoline,
getter and byte store under CONFIG-249's named frame contract. Actual event,
handler/outer-return, stack/saved-BP and pointer-clear reads at those points
are agent (FND-CONFIG-250); all three inputs complete, with one checked draw
and no guest writes. Both owned muted sessions close normally. The earlier
dispatcher call/window/target itself is not observed; other roots, keyboard/
reentrant entry, between-sample frame writes and later menu return remain
unadmitted. The existing observation plan gives the exact command and controls.

The same strict command's fixed `dispatch` mode passed twice on 2026-11-08
in sibling v181-q014-dispatch-a-20261108/ and
v181-q014-dispatch-b-20261108/. It additionally stops at the known window-
specific call before execution, requires the actual copied event, live menu
and handler target, and pairs the next menu-arm stack/return with that call.
Those exact outgoing-argument/window/target/frame reads and the six-point
continuation are agent (FND-CONFIG-251). All existing input/RNG/history and
paired-store controls pass, with no guest writes; both owned muted sessions
close normally. Other windows/handlers, global-hook and keyboard/reentrant
routes, between-stop changes and later returns remain unadmitted. The plan's
Direct dispatcher admission section gives the fixed scope and controls.

The first failed-Save lock probe, v181-q009-native-write-failure-20261106,
stopped during return-segment relocation after7 observations, before any
file lock, payload fault or guest setup write. The probe had not installed
its initial Save-return breakpoint, so removing it was refused. This is a
tooling failure and admits no native write-error or failed-map capability.
The owned session closed and its run lock was released. Retained under
GAME_DIR/captures/rust-dos-save-failure/. The next tooling revision arms
that return and its relocation watch together, with a synthetic control.
The corrected probe passed in v181-q009-native-write-failure-b-20261106
under that root. Its private single-payload lock, observed native failure,
released-lock continuation, Save return and checked failed-map checkpoint
are agent under the current pins. The fixed failed-save source stage also
passed in captures/rust-dos-shutdown/v181-q009-failed-save-quit-20261106/.
It restored that checkpoint and reached ordinary Quit's known metadata
write/commit/result observations. This admits those fixed filesystem-input
and continuation parts only; it made no guest setup write. Both owned
sessions closed and released their locks. At376dc27f, the pending-byte
watch stages also passed: v181-q009-native-write-failure-c-20261106
and v181-q009-failed-save-quit-watch-20261106 in those respective roots.
Their fixed record watch, checked source restore and normal-input
continuation are agent under the current pins (FND-SAVE-041). This
admits the observed value-change mechanism only, not arbitrary watched
fields or unchanged-value writes. Both sessions closed and released locks.

The fixed retry observer command is `failed_save.py <failed-source-run>
<new-run-name> retry`. It accepts only the checked failed-map source under
captures/rust-dos-save-failure and writes under captures/rust-dos-save-retry.
It adds no host lock or guest setup write. Its copy-boundary metadata
observation and retry-map checkpoint are not yet runtime-admitted; the
next actual attempt must establish their capabilities. The first attempt,
v181-q008-reused-save-20261106, stopped at the copy helper before
metadata capture: the new reader exceeded checked_read's existing261-byte
per-call bound. Its report remains under captures/rust-dos-save-retry;
the owned emulator closed and machine lock was released. No completed-copy
capability is admitted. The correction reads the already bounded directory
in chunks and keeps the original single-read limit unchanged.
The ordinary Quit observer also has a fixed `retried-save` source stage:
it requires a checked retry-map, no guest seed or retry lock, successful
writer/copy evidence and the same pending working record. Its source root
is captures/rust-dos-save-retry, with the existing restore guards and
fixed pending watch. The corrected retry passed at0b88cb7d in
captures/rust-dos-save-retry/v181-q008-reused-save-b-20261106;
its copy metadata and checked retry-map parts are agent under current
pins. Atc8d925e3 the later-close stage also passed in
captures/rust-dos-shutdown/v181-q008-reused-save-quit-20261106.
Its fixed source restore, ordinary input and native metadata write/commit
parts are agent (FND-SAVE-042). Both owned sessions closed and released
locks. This does not admit arbitrary sources or establish save loadability.

On 2026-11-06 the first fixed shutdown observer restored the verified
saved-map and issued F3, but its builder breakpoint did not recur while
the modal waited. Its bounded continuation was paused and the owned session
closed, with no RNG draws or guest writes and no exit-entry snapshot.
Retained: GAME_DIR/captures/rust-dos-shutdown/v181-quit-entry-20261106/.
This failed attempt does not admit the F3/Quit or shutdown-observation part.
The corrected event-fetch runner subsequently passed the fixed F3/Quit
flow in v181-quit-event-poll-20261106 under that same capture root.
FND-SAVE-031 records its accepted evidence: verified input delivery,
ordinary exit entry, named archive/slot/stream reads and reached preparation
stops, then the pre-termination breakpoint and fixed private-file hashes.
These fixed parts are agent under session1.0.0/rust-dos1.8.1. No draw or
guest write occurred. Native write-result capture remains unverified because
no monitored write request hit; dirty shutdowns, other indirect slot calls
and actual DOS termination are also unverified. Both sessions closed.
The per-call extension passed in v181-quit-use-points-20261106 under
the same capture root. FND-SAVE-032 admits its one slot argument/target
and paired return stop, plus the four fixed stream selector/count-test
reads. These named parts are agent; other slot targets, dirty writes and
continuous state preservation remain unverified. That owned session closed.
The fixed callee-entry extension passed in v181-quit-callee-entry-20261106
under the same capture root. FND-SAVE-033 admits its known type-three
entry breakpoint, incoming argument, expected far-return frame and paired
normal rejoin. These fixed parts are agent; arbitrary/other target entries,
dirty writes and internal native effects remain unverified. The session closed.

The controlled stage passed in v181-quit-dirty-active-20261106 under
that capture root (FND-SAVE-039). The fixed active-head named-byte
write, including expected-old/readback guards, is now agent under the
current pins. So are the two observed full-count native write results,
the AH68 request/result and five paired preparation returns. This admits
only that controlled pending-metadata setup, not arbitrary writes or an
ordinary dirty-state history. Failure cases, other fields/callers and actual
DOS termination remain unverified. The owned session closed and released
its lock. Reports and writable copies remain in that private run directory.

The XMS linkage extension passed in v181-quit-xms-entry-20261106 under
the same capture root (FND-SAVE-040). Its installation/entry queries,
free-call pointer and handle reads, fixed0160:1F00 native-entry frame and
paired return are agent under the current pins. The fixed scenario entered
that service with function0A/handle1 and returned AX1. This admits neither
arbitrary native targets nor other hooks, functions or handles. The run
made no guest setup write or RNG draw, stopped before DOS termination,
and closed its owned session and lock normally.

The historical per-part attempts below retain their rust-dos v1.7.2 and
session-version provenance. The v1.7.5 attempts below remain historical;
an untested part is not admitted by a historical success. A save state
records the emulator SHA-256, and the approved session package refuses a
different digest before starting restore. Native cross-release format
compatibility has not been tested: the package guard alone does not prove
the old state format incompatible. Historical checkpoints are preserved.

The fixed SOUND_DS.EXE parser capability script is setup_parser.py under
tools/research/rust-dos-runs/. Its two-file identity, entry/return and
error/driver-guard contract is in docs/implementation-plans/ADV-SURVEY.md.
On 2026-11-05 two fresh owned runs validated setup-specific launch to the
declared parser entry and return, the incoming far frame and named
count/pointer/mode reads (FND-SOUND-039). These fixed parts are agent;
input, guest writes, frames, saves/restores, drivers and later UI remain
unverified and outside this probe. The utility's DS/SS mapping is separate
from DSUN.EXE. Both attempts used package 1.0.0/rust-dos v1.8.1, default
muted host output, private two-file C drives and no other media; owned
sessions closed. Reproduce with the project-local Python and
setup_parser.py <new-run-name>. Retained records are under
GAME_DIR/captures/rust-dos-setup-parser/v181-setup-parser-20261105-a/ and
v181-setup-parser-20261105-b/. No gameplay/rule replay capability follows.
Two further fresh runs, v181-setup-records-20261105-a/ and
v181-setup-records-20261105-b/ under the same root, verified the bounded
first-prefix size reads and populated record comparisons at that same
return (FND-SOUND-041). These fixed reads are agent. All headers must
match the predicted bounds before populated payload reads; negative-count
payloads, slack, second-prefix words and later lifetime remain outside
the probe. No additional continuation or input capability is admitted.

On 2026-11-04, session 1.0.0/rust-dos v1.8.1 passed the fixed
startup_segments.py startup-to-main attempt with its RNG guard. This verifies
owned process startup, declared breakpoint stops and the probe's memory reads,
with host output recorded muted. The local run is
GAME_DIR/captures/rust-dos-startup-segments/runtime-upgrade-20261104-v181/.
The same fixed probe's named equipment-flag read was verified under these
pins in v181-q118-main-flag-20261104, retained under the same capture root
(FND-ITEM-071). It requires the exact main stop and relocated DS before
reading one byte; no continuation, input or guest write is admitted.
On 2026-11-07 the unchanged fixed clock_source.py also passed under session
1.0.0/rust-dos v1.8.1 in
GAME_DIR/captures/rust-dos-clock-source/v181-q037-constructor-point-20261107/.
Its existing constructor allocation/return stops, caller/root metadata checks
and final pre-seeding boundary are agent under these pins. FND-EXE-654 uses
only the constructor point. No input, guest setup write, RNG draw, arbitrary
profile or continuous native-register preservation capability is admitted.
The owned session closed and released its run lock.

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
FND-COMBAT-169 extends paused saved-map verification with fixed getter metadata
and free-list observations. Both FNFO buffers match the canonical source;
scratch/selected are 520/523, temporary kind is zero, and sampled pools are
finite and disjoint. Restore/roundtrip comparisons pass without gameplay
continuation, input or guest writes. Reproduce with native_save.py
v181-q063-native-save-input-20261104 <new-run> verify. Accepted output:
GAME_DIR/captures/rust-dos-native-save/v181-q067-getter-metadata-20261104/.
This admits one paused boundary, not later cleanup or continuous lifetime.
FND-COMBAT-170 adds five fixed record-base header/marker observations and
roundtrip equality. Known fixed table requests fit mutually separate described
extents. Reproduce with the same verify command and a fresh output name;
accepted run: v181-q067-record-headers-20261104 under the native-save root.
No allocation history, whole-heap traversal or later lifetime is admitted.
FND-COMBAT-171 adds the bounded 1530-byte shared item-type capture, exact
canonical ALL/4 equality and roundtrip preservation. Accepted native-save
run: v181-q067-item-types-20261104. Use the same verify command with a fresh
name; compare its item_types report member with getter_metadata.compare_item_types.
No getter/consumer invocation or later gameplay lifetime was observed.
FND-ITEM-067 extends the same paused verify command with the bounded placed
table and one following row after modeled header/marker checks. The complete
observation matches after strict roundtrip, without continuation, input or
guest writes. Accepted native-save run: v181-q109-placed-table-20261104.
This admits a paused read of these fields, not a constructor/removal call
or their later lifetime; CHECKPOINT-METADATA defines the fixed bounds.
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
The fixed saved-map-writer stage in FND-COMBAT-158 admits a one-byte change
watch on cell (107,36) after reader return. Its one observed write matches
the occupancy setter's post-store instruction, target, coordinates and
placement-wrapper return; initiating slot and earlier caller remain open.
Accepted: GAME_DIR/captures/rust-dos-load-entry/v181-q066-map-writer-20261104/.
The finding records its fresh-output command and endpoint/lifetime limits.

FND-COMBAT-157's bounded diagnostic repeat locates that later caller's sole
map change at cell (107,36), 00 to 60, without writer attribution. Accepted:
GAME_DIR/captures/rust-dos-load-entry/v181-q066-map-difference-20261104/.
Its saved-caller-epilogue command and limits are recorded in the finding.

The fixed saved-caller-epilogue stage in FND-COMBAT-156 continues through
the enclosing restore caller's post-setup, successful working-close and
matched +0FB5 epilogue. Reader and final AL are one; cleanup clears reuse.
Item/script/effect endpoints match, while one map byte and archive state
change. This is not an observed far return or writer attribution. Accepted:
GAME_DIR/captures/rust-dos-load-entry/v181-q066-caller-complete-20261104/.
Diagnostic v181-q066-caller-20261104 reached the epilogue but failed probe
breakpoint cleanup before final comparisons; it is retained separately.
Reproduce with load_entry.py v181-q063-native-save-input-20261104
<fresh-output-name> saved-caller-epilogue and the pinned project interpreter.

The fixed saved-items stage in FND-COMBAT-155 admits the before-clear,
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
FND-SCRIPT-058 adds a fixed four-byte prepared SAVE/38 slot read at the
same three stops. Its words match the live trigger pointer and remain
unchanged at those samples in
GAME_DIR/captures/rust-dos-load-entry/v181-script-save-slot-20261106/.
Reproduce with load_entry.py v181-q063-native-save-input-20261104
<fresh-output-name> saved-script-conversion using the pinned interpreter.
This does not observe the earlier SAVE/38 transfer destination.
The separate fixed saved-trigger-transfer stage in FND-SCRIPT-061 admits
that earlier request, selected plain directory row, complete 2600-byte
private-span equality and matching reader-local/prepared/live pointer words
at request entry, selection and matched core return. It stops there, before
conversion. Output is
GAME_DIR/captures/rust-dos-load-entry/v181-trigger-transfer-20261106/;
reproduce with load_entry.py v181-q063-native-save-input-20261104
<fresh-output-name> saved-trigger-transfer using the pinned interpreter.
Its samples do not prove continuous lifetime or failed-transfer behavior.
The fixed saved-trigger-conversion stage in FND-SCRIPT-065 connects those
transfer checks to the pre-conversion pointer/2600-byte input and predicted
output before release and at reader return within one run. Reproduce with
load_entry.py v181-q063-native-save-input-20261104 <fresh-output-name>
saved-trigger-conversion. Accepted output is
GAME_DIR/captures/rust-dos-load-entry/v181-trigger-conversion-fixed-20261106/.
The earlier v181-trigger-conversion-20261106 directory retains a stopped
overlap-extent tooling diagnostic. The accepted run's target38 expected
hash uses the legacy private_span_sha256 key with an explicit prediction
basis; dfdf8fba renames that output key only. These samples admit the
normal route, not continuous lifetime or other inputs/failures.
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
| Write supported memory fields | agent for the fixed active-head archive_pending_metadata setup only | FND-SAVE-039 verifies expected-old/readback under current pins. Earlier hit-point writes in FND-PARTY-212 remain historical. | Other fields need their own current guarded-write capability attempt. |
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
[Recorded runs](../vendor/upstream/work-protocol.md#recorded-runs) (lines 447-459). Each emulated-call setup must document port
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

## Controlled memory profiles

On 2026-11-06, inspecting the pinned session1.0.0 RustDosConfig dataclass
confirmed that it has no memory-size or EMS setting. The package owns its
config and refuses a replacement --config argument. The rust-dos1.8.1
source at3bdb0eff305cf827c07414ce30b9616ef450562c exposes memsize and ems
through configuration; its run-subcommand memory option is not a global
debugger-frontend option. No XMS toggle was identified or requested.

The file-backed producer and cleanup in FND-EXE-398 are a candidate for
controlled memory-profile research under Q-SAVE-007, reusing the record
sequence and manager in FND-EXE-403/406. No alternate profile was run,
and changing a profile is not assumed to reach that producer. This
specific package configuration capability is unavailable; existing static
work and default-profile sessions remain available. No guest seed,
package patch or config substitution was used to bypass the limitation.

Reported as [toolkit issue552](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/552).
After shared support is reviewed and explicitly adopted, validate the exact
rendered config, owned startup and actual producer/cleanup state before
admitting any alternate-profile runtime part. Default-profile checkpoints
are not proof of a different profile's behavior.

## Fixed registered-delete observation

The default-profile shutdown observer now has read-only points for the
registered library callback, its deletion-wrapper entry, native AH41
request/result and normalized return. It requires the captured registered
target, known caller/frame/path argument, equal bounded request path,
restored registers and the wrapper's conditional error-word/AX/carry mapping.
This extension is not yet runtime-admitted. The first attempt,
v181-q007-registered-delete-20261106 under captures/rust-dos-shutdown,
recorded one request/result but stopped on the second cleanup call. Its
one-call guard omitted FND-SAVE-027's already documented game-cleanup
call. The session closed and released its lock; this attempt admits no
complete interval. The corrected guard distinguishes the registered caller
from the exact game-cleanup call instruction and requires both completed
invocations, rejecting a third. At0b4ba5bc the repeat passed in
captures/rust-dos-shutdown/v181-q007-registered-delete-b-20261106.
The fixed caller/frame/path reads, paired native result and normalized
return parts are agent under current pins (FND-SAVE-043). The owned
session closed and released its lock. Nonempty paths and other caller
sequences remain outside this admitted scenario. No arbitrary host path is opened and no guest field
is written. The static point check uses resident_listing.py over the two
already established FND-EXE-558/559 ranges; its retained listing is
GAME_DIR/analysis/handler-census-20261106/registered-delete-observer-points.txt.


## Paused overlay and queue backing

At1d638bc3, verify_checkpoints.py adds fixed read-only metadata to its
admitted play/play-ready stages. The strict play-ready verification passed
at captures/rust-dos-regenerated/v181-q137-overlay-backing-20261106-play-ready/,
with source v181-q046-play-ready-20261104: exact restored state/frame/time,
checked files, deliberate IRQ mismatch refusal and identical named backing
across the same-process roundtrip. FND-PARTY-340 records the observation.
This admits those paused named reads and comparisons only, without input,
guest continuation or setup writes. It does not admit a reflection probe,
BP-chain validation, a cache copy or interval preservation. Five synthetic
controls and the full integration gate passed; the owned session closed.


At55fbe24d, the same paused verifier also observes the documented SS:BP
link graph, with a 64-node bound and explicit cycle/crossing/limit results.
The new strict verification in
captures/rust-dos-regenerated/v181-q137-frame-links-20261106-play-ready/
passed with an identical observation across the same-process roundtrip;
FND-PARTY-340 records the result. These bounded read-only link and return
field observations are agent for the admitted paused states. They do not
validate frame authenticity or later interrupt-entry links. Six new
synthetic controls, five existing backing controls and integration passed.
The owned muted session closed without resuming gameplay or writing fields.


At448da523, paused play/play-ready verification also observes the loaded
header queue through registered FMT-EXE-003 links, bounded to the 49
fingerprint-checked source headers and code sizes. The strict observation
in captures/rust-dos-regenerated/v181-q137-loaded-headers-20261106-play-ready/
passed with identical loaded-queue metadata after checked roundtrip
(FND-PARTY-340). These registered metadata reads and point range comparisons
are agent; cache lists, later graph preservation and native transfers are
not admitted by them. Eight new controls, eleven prior snapshot controls
and full integration passed. The owned muted session closed without
input, guest continuation or setup writes.
