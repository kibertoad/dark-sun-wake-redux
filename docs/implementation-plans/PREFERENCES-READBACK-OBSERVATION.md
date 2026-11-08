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
entry trampoline (FND-INPUT-012's stub8, not the overlay header prefix)
must name offset zero in that same segment. Verify the
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
header-prefix substitution for the actual entry trampoline,
wrong mode/segments, short/wrapping reads, AL-versus-AX and entry/return/store
pairing. Exit for tooling: controls and Test.ps1 pass, fixed command documented.
A later research batch attempts two independent restores, records only the
capabilities and behavior actually reached, and narrows Q-CONFIG-014's admitted
path. It cannot close the whole entry from one checkpoint or promote parity.

The optional fixed `history` mode is described under Value history below. It adds changed-value watches only
for that plan's named fields between strict restore and the paired store.
FND-CONFIG-245 and docs/RUNTIME.md record the actual native history scope;
an armed watch alone is not mutation-delivery evidence.

Tooling control command: run `check_preferences_history.py` and
`check_preferences_readback.py` with the checkout-local evidence Python and
`-I`. The fixed native command is `preferences_readback.py
v181-q046-play-ready-20261104 <new-run-name> history`. It uses the same
session-owned output root as the legacy command, with optional `value_history`
in preferences-readback.json. Compare complete reports from two independent
owned runs in a subsequent research batch; do not treat synthetic success as
native history capability or a readback finding.

## Value history

For Q-CONFIG-014, extend the existing strict play-ready Preferences observer
with an optional fixed `history` mode. Its research outcome is a named-field
value history from admitted restore through the getter's AL store, not a new
player behavior. Reuse CONFIG-235/236/238/243, SOUND-085/092 and the existing
PREFERENCES-READBACK-OBSERVATION plan and fixed input/RNG contract.
Before input, arm only the master/music gates, music byte, configuration
pointer, driver slot, source/getter handles, library-mode word and the card
word reached through the admitted configuration pointer. Reject overlapping,
wrapping or short spans. Record initial values and every observed change with
the actual post-instruction stop, sequence/time and old/new values. Validate
all fields at ordinary stops; an unexplained change aborts. A configuration
pointer change aborts rather than following an arbitrary new location.
Watchpoints observe changed instruction-boundary values, not unchanged writes
or all intra-instruction/transient accesses. They establish no startup-to-map
history, unmonitored bytes, arbitrary caller, hardware effect or whole reading.

Keep old probe invocation and report unchanged. The optional report adds
`value_history` with fixed field metadata, initial/change/final values and a
completion marker. No successful marker before the paired store and watch
cleanup. At the known changed-byte store, admit only the exact music-byte
watch at the already-verified caller's next boundary, preserving the actual
stop reason; avoid a competing post-store execution breakpoint. An unchanged
store uses its ordinary breakpoint and cannot supply the positive watch control.
RNG continuation interrupted by a watch fails through the existing recorder;
do not resume past an unrecorded nested event. No guest writes or new input.

Acceptance: synthetic controls for all fixed spans, arm/cleanup and partial
failure, real-mode/sequence/value checks, unknown or malformed watch records,
missing changes, simultaneous changed fields, pointer mutation, exact watched
store versus wrong field/phase/boundary, and unchanged legacy mode. Run the
new and existing observer controls and the Research gate before tooling commit.
Exit: documented bounded command and passing controls. A separate research
batch attempts native observation with the pinned muted owned session, records
the actual capability/result and narrows only the admitted interval.

## Menu parent and event frame

Q-CONFIG-014 retains actual menu parent/event and post-child admission after
CONFIG-248. CONFIG-249 supplies the finite named frame bindings. Add only
optional `parent` mode to the strict restored-map command, retaining its
existing history, source/frame checks, input route, RNG recorder and endpoint.
No fresh run, new gesture, guest write, arbitrary frame read or extra
continuation after the paired byte store is admitted.

After native selection of BUTN10303 and before its existing click, resolve
the loaded menu handler through resident trampoline56B9:0034. Require its
far jump to offset0 and verify the known call at menu+0281 targets5782:0048.
Arm only menu+025E, menu+0281 and that fixed Preferences trampoline. At the
arm, read event kind/control at SS:BP+6/+8, handler return at BP+2, SP/BP/SS,
and the two known menu pointers DS:1431 and4E46:0000. Require kind2/control10303,
the ordinary handler SP=BP-4, equal nonnull menu pointers, and bounded
WIND10500 tag/number/flags reads while that backing is still live.

At the pre-Preferences call require the same actual event/frame/handler
return pair after the frame/flush/close children; require both menu pointers
cleared and never dereference the old window again. At the resident
Preferences trampoline require the native outer return menu:+0286, unchanged
parent BP/SS and SP decreased by four. Do not assume a final jump was patched
before this first trampoline entry. At the existing getter and store points,
reuse Readback's actual loaded Preferences binding, require the CONFIG-249
nested SP/BP relationships, saved parent BP and unchanged outer return/event
slots. Record each actual stop, pair and named value; these are sampled frame
observations, not a continuous frame-write or arbitrary-root census.

Require all five parent observations, completed click, paired getter/store
and owned watch cleanup before parent and overall completion. Reject wrong
trampolines, calls, mode, segments, wraps, short reads, phase/frame/event
changes, unexpected parent/watch collisions or a missing point. Keep the
old commands/reports and fresh-route behavior unchanged unless parent mode
is explicitly selected. Add synthetic controls for these boundaries and the
shared-loop integration; run all focused controls and Test.ps1 Research.
A separate research batch repeats the restored route twice, records actual
capabilities and removes only this route's missing parent/event samples.
Keyboard gates, other roots/entries, exit-time reentrancy, between-sample
writes and the remaining driver/storage conditions stay open.

## Direct dispatcher admission

CONFIG-250's saved return matches the window-specific dispatch continuation,
but does not observe that earlier call. Add only fixed `dispatch` mode to
the same strict restored command. It retains parent/history observations
and inserts one pre-call breakpoint at relocated39D1:0802 after native button
selection and before the existing click. Reuse CONFIG-250's source operand
reading and PARTY-078's copied event-by-value contract, not another census.

Verify the declared instruction is the far window-handler call at ES:BX+F5.
At that stop require real mode, expected DS/SS and bounded SP/ES/BX; read only
the first two words of the already-copied event at SS:SP and the current
window's tag/number/flags and handler pair. Require event2/control10303,
WIND10500/current flags, and ES:BX equal to both known live menu pointers.
Require that actual handler is the known resident menu trampoline, whose
admitted jump names the loaded menu segment already bound by parent mode.
Do not follow another target or continue an unexpected packet/call.

Pair that actual call with the menu-arm stop: same SS/window, handler return
at the declared dispatch continuation, SP decreased by ten and BP decreased
by six from the pre-call SP under CONFIG-249's far-call/menu-prologue layout.
Then retain the existing post-close, trampoline, getter/store and cleanup
checks. Completion requires all six observations. No new input, writes,
arbitrary target, later return or whole-entry claim. Keep other modes unchanged.
Synthetic controls cover opcode, target/window/event bindings, real-mode and
bounds checks, arm pairing, wrong/duplicate phase and incomplete endpoints;
all prior controls and Test.ps1 must pass before tooling commit. Separate
native research repeats this exact route twice and records only actual
dispatcher/caller admission. Other roots, between-stop writes, keyboard/
reentrant paths and remaining driver/storage obligations stay open.

## Forwarded P-key gate and callback admission

Q-CONFIG-014 retains keyboard post-child key/gate admission. CONFIG-243 and
INPUT-012 bind overlay190 +139B's event kind at SS:BP+6, key word at+12,
close callback at+1E and the P state/turn/callback/reload/opener sites. Add
only fixed `keyboard` mode to the same strict restored command and history.
After the existing Tab delivery and ready Game Menu10500 gate, replace the
mouse motion/click with a single lower-case P key held1ms. This new gesture
is a capability attempt; require actual delivery by the paired store and
retain an incomplete result if that fails. No timed wait establishes readiness,
input retry, guest write, new configuration or arbitrary key is admitted.

Resolve only the known key trampoline571F:0020 to offset139B and the known
menu-handler/close trampolines. Verify the fixed P opener call and require
the forwarded event kind6, folded key p and close callback56B9:002A. Require
that close trampoline's known jump targets the loaded menu's +0612; do not
follow a different callback. Observe key+1A82/+1A87/+1A8E, +1CBE/+1CC5/
+1CC8, +1EC4, the resident Preferences trampoline, getter and store. Record
actual event/key/callback/frame, DS:0DAB mode, signed DS:426D turn word and
the two menu pointer fields at these named boundaries. Require the state
getter's full AX nonzero and equal to the sampled mode, and signed turn<4
at its actual comparison; do not invent a valid negative gameplay domain.

Bind one actual keyboard parent BP/SS/handler-return and its event/key/
callback slots. Require key retention across the reached callback, matching
pre-call/post-return SP, pointer clears, and the final P opener arm after
the reload. Pair the outer Preferences return and nested saved keyboard BP
with the existing Readback contract, retaining the actual trampoline and
getter/store checks. Old modes keep their existing input and report scopes.

Keep all history/RNG/unknown-stop controls. Require all ten ordered samples,
both input deliveries, positive music-byte change and watch cleanup before
completion. Observe named fields and actual frames only, never arbitrary
stack contents or released backing. Synthetic controls cover exact targets,
state/turn rejection, changed key/callback/frame, callback SP/pointer changes,
wrong nested returns, pending input and missing endpoints; keep every existing
control passing and run Test.ps1 Research. Separate native research repeats
the fixed route twice and narrows only actual observed keyboard admission.
Other keys, callers, gate branches, reentrant paths, between-stop changes
and remaining driver/storage mechanisms stay open.
Retain each candidate named sample before applying a gate that can reject it,
marking it unaccepted until all checks pass. A failed gate must preserve its
actual selecting value without advancing phase or granting completion.

## Native turn-rejection completion

CONFIG-252 admits the forwarded-P/state/turn samples but stops before the
comparison. Add only fixed `keyboard-rejection` mode to observe the actual
rejecting continuation. Reuse the same checked restore, Tab/P1ms gestures,
KeyTrace fields, source/trampoline, history/RNG and unknown-stop controls.
Read the two known mode/turn words at the strict restore before input,
retaining that baseline without assigning a writer or cause to differences.

Require a nonzero state result followed by actual signed turn>=4 at its
comparison. Verify the declared comparison operand, then resume only with
the existing positive callback/opener/getter points still armed as guards.
Require the known +1EC9 epilogue before its pops/leave/return; match the
keyboard frame, event/key/callback, stack, pointers and sampled mode/turn.
Read only the active frame's saved caller BP. Require the matching menu
return +0039 with SS unchanged, SP=keyboardBP+6 and the saved caller BP
restored. Do not inspect the expired keyboard frame after return.

After that return, remove the input-builder poll and advance in fixed
one-millisecond sampling quanta, keeping every callback/opener/getter guard.
The first builder-only attempt exhausted512 stops while input was pending;
frequent builder entries do not imply completed queued delivery. Complete only
when P delivery is done and the same ready Game Menu10500 remains, with
unchanged named menu pointers and no observed getter or callback/opener
guard reached. The final native state and input delivery, not elapsed time,
establish this endpoint. Keep the512-stop cap and original operation handles;
never retry input or continue after an unknown stop. No guest writes or seeds.

This mode's named-field history has a distinct no-observed-change predicate:
require all watched values unchanged through the endpoint, retain empty
changes and final values, and remove owned watches before completion. It
does not provide a positive mutation-delivery control and must say so.
Other modes retain their positive music-byte predicate. Overall pass in this
mode means completed turn rejection only, with explicit result and limits;
the readback array stays empty and no callback/getter/store is admitted.

Synthetic controls cover baseline/branch predicate, epilogue/return pairing,
changed frame/event/menu, positive-path guard refusal, pending input/window
readiness, no-change history versus mutation, source/bounds checks and
completion/cleanup, including time stops admitted only after the paired return
and one fixed sampling deadline per continuation. Keep all prior controls passing and run Test.ps1 Research
before tooling commit. Separate research repeats the completed rejection
twice and records actual capabilities. Positive callback/reload admission
still needs a naturally admitted turn state and its producers; other roots,
branches, between-stop changes and driver/storage obligations remain open.
