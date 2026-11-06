---
id: FND-SAVE-043
title: Both known cleanup invocations request deletion of an empty path and normalize the native error to FFFF
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: shutdown_run.py and shutdown_delete.py at 0b4ba5bc; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic15000 cycles; SB16 base220 IRQ5 DMA1; no Ultrasound; muted host output
---

## Observation

This answers Q-SAVE-007's fixed default-state registered-delete
path/result obligation for RULE-SAVE-001. FND-EXE-558/559 read the
library callback and native wrapper; FND-SAVE-027 also identifies the
game cleanup's call of that same library callback. FND-SAVE-040 admits
the default saved-map slot/XMS linkage. Neither a callback pointer nor
the deletion wrapper's cleared return carry proves native success.

On 2026-11-06 the observer restored the checked saved-map from
v181-q063-native-save-input-20261104. Executable, instruction/time,
state/frame and private-file guards passed. Normal F3/Q deliveries
completed; ordinary exit status and both flags were0. The captured
registered callback table contained4C6C:0025 and0C35:0029.
No guest setup write, RNG draw or seed-setter hit occurred.

### Two independently bound invocations

The library callback0C35:0029 was entered twice, with different actual
return frames:

| Source | Return pair | Callback SS:SP |
| --- | --- | --- |
| Registered runtime exit dispatch | 05BA:03A5 | 4D9A:FFC4 |
| Game cleanup's explicit call | 5F31:07B4 | 4D9A:FFBA |

The second frame was admitted only after verifying the current immediate
far call at the caller's offset07AF targets0C35:0029. FND-SAVE-027's
game cleanup places that call after its known service prefix. The bounded
overlay check confirms its return at07B4; the observer does not assume
an unchanged overlay segment. Each library callback made the separately
observed deletion-wrapper call returning to0C35:003A, with the expected
stack displacement and BP. Its argument was4D9A:4022, matching the
wrapper's current DS and the known callback's outgoing pointer.

For each invocation, that argument named an empty terminated string.
At the native AH41 point3A98:02B3, the actual DS:DX and bounded path
matched the wrapper argument and earlier path read. The immediately
following result at3A98:02B5 was AX0002/carry set in both cases.
The observer retained native and normalized results separately.

| Invocation | Error word before | Native result | Wrapper return at0C35:003A |
| --- | --- | --- | --- |
| Registered | 18 decimal | AX0002, carry set | AXFFFF, carry clear; error word2 |
| Game cleanup | 2 | AX0002, carry set | AXFFFF, carry clear; error word2 |

The word is the native wrapper's separate stored error at data offset33BE,
not the archive writer's error word or pending byte. Both wrapper returns
restored the observed DS, DX, SS/BP and far-return stack displacement.
FND-EXE-558's remaining callback instructions remove the pointer argument
and return without testing AX. Thus cleared wrapper carry is observed
alongside a reported native failure, not evidence of successful deletion.

SRC-RUSTDOS-181 identifies the chosen emulator source's AH41
pointer/result interface. Its file helper returns error2 for missing
or nonfile targets after its path lookup. That source interface does not
independently prove every active guest vector or original-DOS behavior;
the actual empty argument and AX/carry responses are observed separately.

### Whole observed shutdown interval

One slot call and its XMS linkage were observed, retaining the same
fixed scenario as FND-SAVE-040. Five archive preparations entered clean
with native handles10,9,8,7,6. No monitored native write request hit.
DARKRUN.GFF, CHARSAVE.GFF, SAVE02.SAV and RGN032.GFF retained their
before/after existence, lengths and hashes. This is a four-file comparison,
not a global absence-of-file-effects claim.

Forty-two bounded observations reached05BA:01A5 with AX4C00 before
executing DOS termination. Both deletion invocations were complete;
the owned session closed, its emulator was absent and the machine run
lock released afterward.

## Interpretation

For this verified default saved-map Quit, both known callers reach the
same empty-path deletion request and reported native failure, then receive
FFFF from the wrapper despite its cleared carry. The actual argument,
two caller bindings and paired native/normalized outcomes are answered
for this scenario. Q-SAVE-007 retains nonempty-path histories, different
slot/stream states, other callback/service mechanisms and their external
effects. This does not close that whole question, establish every
shutdown state, add a complete reading or promote parity.

## Alternatives

Counting the library callback only once omits the game cleanup's known
explicit call. Treating cleared wrapper carry as deletion success loses
the native carry and AX-to-error-word mapping. A stored pointer alone
would leave the request's actual contents unresolved; the native point
here used the same observed empty string. Equal hashes of four files
do not exclude effects on unmonitored targets or through other mechanisms.

## How to reproduce

Run the pinned evidence Python with the observer at0b4ba5bc:
`shutdown_run.py v181-q063-native-save-input-20261104 <new-run-name>`.
Retained: GAME_DIR/captures/rust-dos-shutdown/
v181-q007-registered-delete-b-20261106/shutdown.json, session.json,
emulator configuration/logs and private drive. No exported event/request
journal is claimed. Require passed=true,42 observations, two completed
inputs, no guest writes/draws, two complete delete_calls with the distinct
caller bindings and empty paths, both native AX2/carry-set results, both
FFFF/carry-clear wrapper returns, and the fixed-file controls above.

Static observer-point reports remain under GAME_DIR/analysis/
handler-census-20261106/registered-delete-observer-points.txt and
registered-delete-game-caller.txt. Regenerate with committed
resident_listing.py over FND-EXE-558/559's two bodies and
overlay_listing.py descriptor180,file67942..67A28, checking the
established game-cleanup body and explicit call. The earlier
v181-q007-registered-delete-20261106 attempt stopped after its first
completed deletion because its initial one-caller guard was too narrow;
it supplies no successful whole-interval result. Six deletion controls
validate tooling, not the game behavior. Keep original-derived output local.
