# Fixed Preferences readback observation

Q-CONFIG-014 and RULE-CONFIG-003 need actual entry/getter/result/store
admission after FND-CONFIG-235/236/237's local reading. FND-SOUND-096
admits the unchanged first startup only through its first music gate.
The player-visible question is which music-level byte Preferences obtains
on entry, distinct from its drawn bar and audible output.

Existing shutdown_run.py demonstrates strict current-pin checkpoint restore,
state-driven input polling and RNG recording. It opens the Quit choice,
not the Game Menu or Preferences. Add one fixed preferences_readback.py
observer, reusing those checkpoint, source, input, bounded-read and RNG
helpers. Do not generalize it into a menu, breakpoint or memory runner.

Use the existing manifest-derived play-ready checkpoint under
GAME_DIR/captures/rust-dos-regenerated/. Verify its pinned emulator,
embedded executable identity, load stop/time, full named snapshot,
frame hash and input-map readiness before any input. Restore only through
RustDosSession with pinned rust-dos, default muted host audio and the
documented deterministic Sound Blaster/media configuration. The restored
private drive remains session-owned; no canonical source or checkpoint changes.

SCR-UI-006, FND-INPUT-012 and FND-UI-055 give the fixed Tab/Game Menu and
Preferences button route. Verify main's relocated call to FND-UI-010/054's
key-event builder before arming that poll; the modal choice loop is a separate
routine. Send Tab, await a ready WIND/10500 at that named builder stop,
move to virtual(160,101), await motion delivery and recheck
that menu and selected BUTN/10303, then send one left click. Delivery alone
does not establish native selection: keep waiting at the builder when the
selection has not yet updated, within the same stop cap. Remove the UI
poll before that click and wait only on the getter/RNG guards: repeated
builder stops are unnecessary while the button's hold/release completes.
Keep the stop budget and gesture unchanged. Never retry an input because observation
timed out. Record every input delivery and every admitted RNG draw through
Recorder; unknown draws, reseeding, stops, windows or exhausted bounds stop.
Do not use elapsed time as a readiness condition. No guest writes or setup
seed changes are permitted.

Observe only the documented resident getter. Its actual far return must
name overlay203's Preferences store at relative003E; the relocated fixed
entry trampoline must name offset zero in that same segment. Verify the
call's resident target and the following direct AL store using their
documented operands, not an inferred decompiler parameter. Record source
configuration, master/music gates, slot/getter handles and old music-level
byte. Require real mode, expected DS/SS, nonwrapping reads and exact widths.

Pair the actual getter entry, return before the byte store, and the next
instruction boundary at relative0041. Require matching caller segment,
return-stack displacement and live trampoline identity. Record the return
word separately from the old and new byte; require the new byte to equal
the observed AL. Stop before the following call, with no drawn Preferences
frame or later button effects claimed. Refuse other callers instead of
following arbitrary return addresses. Native observed equality does not
settle other getters, indirect targets, aliases or continuous preservation.

Synthetic acceptance covers the fixed menu state machine, input ordering,
main-call poll admission with the modal-choice target as a negative control,
selected-control admission and poll removal before the fixed click,
wrong windows, incomplete delivery, wrong trampoline/call/store/frame,
wrong mode/segments, short/wrapping reads, AL-versus-AX and entry/return/store
pairing. Exit for tooling: controls and Test.ps1 pass, fixed command documented.
A later research batch attempts two independent restores, records only the
capabilities and behavior actually reached, and narrows Q-CONFIG-014's admitted
path. It cannot close the whole entry from one checkpoint or promote parity.
