# Fresh startup through Preferences value history

Q-CONFIG-014 still lacks one continuous history after FND-CONFIG-247's
pre-opener call. The player-visible question remains which music-level byte
Preferences reads on its admitted startup route. This is research tooling;
it introduces no rebuild behavior, new gameplay rule or persisted asset layout.

Reuse disc_startup.py's nine fixed program-entry watches, native seed caller
and disc/opener guards. Add only the fixed `preferences-history` endpoint.
Keep watches installed until the paired Preferences byte store. No checkpoint
restore, guest write, configuration change, supplied seed, arbitrary target
or additional program is admitted. Source verification, pinned RustDosSession,
muted output, private drive, read-only media and cleanup remain unchanged.

The existing restore_opener.py, regenerate_checkpoints.py play/play-resume
stages and preferences_readback.py supply the route, but cannot connect it
without restores. Reuse opener_mapping, snapshot/ready, input_map_ready,
MenuFlow, Readback, input_poll and Recorder instead of interpreting new
original code. FND-UI-039/040 and docs/RUNTIME.md admit the opener and menu
parts; CONFIG-238/245 admit the later getter/store. Preserve their limits.

Validate the actual opener call before continuation; remove its breakpoint
and require the known return. Reach stable Start/19500, click(160,93), then
stable Create/11500. Reproduce only the existing two-character play route:
right click(121,47), select(158,69), accept(252,47); right click(121,107),
select(158,129), choose(125,56), accept(252,47). Require each intervening
14000/18501/11500 ready window. Esc returns to Start, click(157,76) starts.
The established mode-zero intro may receive Esc only after its stable ready
19501 state. Require mode one, ready 19501 and slot kinds[0,2,0,2] at the
native input builder; deliver key2 and require leader1 at that builder.
Never treat input delivery or an intermediate ready window as map readiness.

Use only the existing virtual mouse move/click (500ms hold), Esc/key2
(100ms hold), bounded 100ms polling steps and stable state comparisons.
Elapsed time is a sampling bound, never a readiness result. No new mouse
coordinates, key or input retry is admitted. Retain every input request and
delivery result, changed named UI snapshots and stop metadata in GAME_DIR;
no screenshots/checkpoints are needed. Cap window readiness at100 polls,
input delivery at the existing100 move/10 click-or-key polls, map transition
at1000 polls and leader readiness at64 builder stops. Retain a total cap.

Keep seed/generator guards. After the one already checked startup seed call,
any reseed fails. Record every admitted draw with the existing sound/script52
caller recorder, bound/result and seed before/after. Check the seed chain at
each outer boundary. A watch interrupt inside that recorder fails through its
existing guarded continuation; do not skip it or reconstruct a missing draw.
Unknown callers and generator/watch collisions fail. Keep all nine watches
active around and inside draws, retaining post-instruction changes. Ordinary
stops must have ordered real-mode metadata and no unexplained watched change.

From admitted map state use the unchanged Tab/move(160,101)/selected
BUTN10303/left-click route (100ms holds). Extract its bounded loop as a shared
function so the strict restored observer retains the same behavior and
controls. Require the actual getter frame, trampoline, direct call, return,
AL byte store and completed click delivery. At this full endpoint require a
positive music-byte watch as well as the startup word/pointer controls; then
finalize and remove watches. Mark completion only after the paired store and
cleanup. No native screen, audible output, other caller or whole-reading
claim follows from a successful route.

Synthetic acceptance covers fixed route ordering, window/slot/leader gates,
exact gesture allowlist and pending delivery, interleaved watch/RNG events,
same-operation time targets, seed-chain mismatch, reseed and unknown stops,
exhaustion, partial endpoint and cleanup, plus shared Preferences loop behavior.
Keep all legacy controls passing and run Test.ps1 Research before the tooling
commit. A separate research batch attempts two fresh routes, records actual
runtime parts and narrows Q-CONFIG-014 only where continuity is observed.
Unmonitored/unchanged/intra-instruction state, other profiles and remaining
caller/reader/driver/storage obligations remain explicit.
