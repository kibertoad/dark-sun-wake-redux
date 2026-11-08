---
id: FND-CONFIG-252
title: Forwarded P reaches its turn comparison with state two and turn word 9999 on the checked menu route
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/preferences_key.py and preferences_readback.py at 8075c75b; initial diagnostic at dc4eca6a; existing strict restore/history/RNG helpers; dinorefurb-dosbox-session 1.0.0; checkout-local evidence Python 3.12.10
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

FND-CONFIG-243 reads the P state/turn gates and the later callback/key
reload. FND-CONFIG-250/251 observes the Game Menu's button route. This
attempt instead forwards P through the ready Game Menu to obtain actual
keyboard gate and callback admission. Addresses, stack values and key words
below are hexadecimal; mode/turn values, counts and nanosecond times are decimal.

On 2026-11-08 the fixed keyboard mode independently restores the checked
play-ready state from v181-q046-play-ready-20261104. It verifies the same
emulator/executable identity, instruction/time, named snapshot, frame hash
and input-map readiness as the existing Preferences observer. The approved
embedded DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. The restored time is 195166973150ns and
initial RNG state 1718646187. No guest setup write or supplied seed occurs.

The existing Tab gesture completes and ready Game Menu 10500 is required.
The observer binds the loaded key routine through 571F:0020 and the known
menu/close trampolines, verifies the P opener call, removes the repeated
input-builder poll and sends one P key with a one-millisecond hold. This
mode supplies no mouse motion or click. Its positive-path completion would
require the full keyboard gate/callback/opener/getter/store sequence and
completed P delivery; no successful marker is granted merely for sending it.

### Actual gate samples in two retained repetitions

Both corrected-tooling runs have the following identical keyboard records:

| Phase | Actual stop | Sequence | Time ns | AX | DS:0DAB | Signed DS:426D | Accepted by observer |
|---|---|---|---|---|---|---|---|
| Before state query | 5D9C:1A82 | 41 | 195480438350 | 4000 | 2 | 9999 | yes |
| State query return | 5D9C:1A87 | 43 | 195480438750 | 0002 | 2 | 9999 | yes |
| Before turn comparison | 5D9C:1A8E | 45 | 195480438883 | 0002 | 2 | 9999 | no |

All three are real-mode execution breakpoints with DS/SS 4D9A, SP FE6E
and keyboard BP FECC. The incoming event kind at SS:BP+6 is 6, key word
at+12 is 1970, and close callback at+1E is 4C73:002A. The key's character
byte is lower-case p. The keyboard handler's return pair at BP+2 is
614A:0039, matching the known menu forwarding call's continuation. Both
menu pointer fields DS:1431 and relocated 4E46:0000 contain 81CF:0004.
The admitted close trampoline names the loaded menu's +0612 helper.

The full state-query AX result is two, equal to the sampled mode and
nonzero, so it passes that local state test. The next sampled signed turn
word is 9999. Under FND-CONFIG-243's unchanged branch reading, 9999 is not
less than four and selects the rejection path. The observer refuses to
continue its positive-route contract at that comparison.

The original comparison instruction itself has not executed at the final
stop. There is no native trace of its taken branch, later key epilogue,
callback, post-callback reload or Preferences opener in these attempts.
The callback pointer's presence establishes no callback execution.

### Incomplete attempt and input/history limits

All attempts have passed false, keyboard.complete false and incomplete
value history, with error keyboard turn gate rejected. The readback array
is empty. The completed input list contains Tab only. The P key-down path
has produced the observed native event/key word, but P release/full delivery
is not verified by this early stop; the observer's last stored delivery
record still refers to Tab. No later readiness or completed-P claim follows.

The restored nine watched ranges have no reported change before the stop;
there is no positive music-byte event or final history value. Armed but
untriggered watches are not positive mutation-delivery controls. The known
RNG/seed guards remain active. One RULE-SOUND-003 draw has bound 3, result 0
and seed 1718646187 to 2185262696, checked through the existing recorder;
no other draw or reseed is admitted. This is no rebuild replay or parity check.

The initial a attempt with dc4eca6a also stops at the turn comparison, but
does not retain the rejected candidate's named fields: its two earlier
samples already have turn 9999. The diagnostic-only 8075c75b correction
retains each candidate before validating its gate, marks it accepted false
until successful, and never advances phase or grants completion on failure.
The corrected b/c keyboard, draws and empty readback records match exactly.
Volatile request/session metadata is not part of that comparison.

All three owned muted session contexts close normally with run_failure null
and no event journal. Emulator PIDs 656, 3416 and 6936 are absent after
completion, as is the machine run lock. Original-derived records remain local.

## Interpretation

The actual forwarded P event, state-query result and pre-comparison turn
value are now admitted for this checked menu profile. It cannot be used
as an admitted positive keyboard-to-Preferences/callback route with the
observed turn binding. This differs from the already-observed button route's
local admission in FND-CONFIG-243/250/251; it establishes no universal claim
that P never opens Preferences or that all menu contexts have these values.

Q-CONFIG-014 retains actual taken rejection/epilogue and input completion,
or a naturally admitted keyboard state with signed turn below four for the
positive callback/reload/opener path. The pre-menu turn value and its writer
history were not read in this probe, so no cause or time of its 9999 value
is assigned. Do not repeat the same positive-route attempt on the unchanged
checkpoint without a new question/tool/state. Other keys/callers, gate
branches, reentrant and between-stop effects and remaining driver/storage
mechanisms remain. RULE-CONFIG-003 stays supported without complete_reading;
no whole queue item or parity status closes.

## Alternatives

- P was never recognized as the forwarded key: the native kind 6/key 1970
  packet reaches the P-specific state-query and common turn comparison.
- The state gate failed because AX was zero: the actual full return is two.
- A nonnull close callback proves its execution or later key retention:
  this route stops before the turn comparison executes and never reaches it.
- The observer rejection is a native completed rejection branch: it is an
  explicit pre-instruction stop; the branch continuation remains unobserved.

## How to reproduce

Use checkout-local evidence Python with -I and
tools/research/rust-dos-runs/preferences_readback.py, arguments
v181-q046-play-ready-20261104, a new run name and keyboard. Expect nonzero
observer exit at keyboard turn gate rejected, not positive-route completion.
Retained outputs are under GAME_DIR/captures/rust-dos-preferences-readback/
as v181-q014-keyboard-a-20261108, v181-q014-keyboard-b-20261108 and
v181-q014-keyboard-c-20261108. The latter two include the rejected candidate.

Require the strict restore/input guards, identical three keyboard records
with the final candidate unaccepted, final stop before +1A8E, all incomplete
markers, empty readback, completed Tab only, one checked draw and no guest
writes. Compare parsed keyboard, draws and readback fields for b/c. Review
actual pins, muted output, run_failure and process/lock closure separately.
The existing observation plan records the fixed gesture, fields and failure
controls. Keep all original-derived output local and reconstruct no journal.
