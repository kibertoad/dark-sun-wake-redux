---
id: FND-CONFIG-256
title: Positive forwarded P completes delivery with ready Preferences and the watched getter result retained
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/preferences_readback.py, preferences_saved.py and named history helpers at c61798c1; dinorefurb-dosbox-session 1.0.0
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

FND-CONFIG-255 admits the positive keyboard route and paired getter/store,
but stops with P delivery pending. The extended fixed saved-keyboard observer
keeps the same source, strict restore/frame/private-save checks, gestures,
ten accepted keyboard samples and paired readback, then continues in fixed
one-millisecond steps without more input. SCR-UI-007 and FND-UI-065 supply
the named Preferences window and handler used to recognize readiness.

Two independent 2026-11-08 runs restore the checked saved-map from
v181-q063-native-save-input-20261104 (FND-COMBAT-140). Their actual initial
mode1, combat0, turn=leader1 and first ten accepted keyboard samples agree
with FND-CONFIG-254/255. The getter returns255 and the original stores that
byte at5FE8:0041, sequence62,197334991616ns, changing music100 to255.
Both samples of the active outer frame satisfy the exact unloaded keyboard
header representation from FND-EXE-562, as FND-CONFIG-255 describes.

The original then reaches the same time-stop endpoint05BA:0463, sequence109,
197357001950ns, after38 outer observations. The actual named UI snapshot has
WIND16500, flags8, mouse enabled1, mode2, normalized handler5782:0043,
leader1, slot kinds[0,2,0,2], null selected control and RNG state1913940318.
Both queued gestures are complete: Tab at197130000016ns and P at197350000016ns.
The ready Preferences state and input operation's completed result, rather
than elapsed time alone, admit this endpoint. No expired keyboard or opener
frame is read after the paired store.

The nine-field changed-value history remains active from strict restore
through this endpoint. Its only observed mutation is the music byte100 to255
at the paired store. Final master/music enables are1, music255, music slot,
source and getter handlesFFFF, library mode4 and card122. The configuration
pointer remains unchanged. This is a positive byte-watch delivery control;
it does not observe unchanged writes or intra-instruction effects. The
positive mutation predicate, final named values and owned-watch cleanup all
pass before history.complete is set. No RNG draw, reseed, guest setup write,
input retry or mouse gesture occurs. Both muted owned sessions close normally.

Overall passed, keyboard.complete and value_history.complete are true with
explicit result keyboard-preferences-ready. The readback still consists of
the paired entry, return and store. Keyboard, readback, draws, named history
and endpoint records agree between runs; input request identifiers are
process-specific and are not required to match.

## Interpretation

This removes the fixed positive P route's completed-delivery and ready-native-
endpoint obligations from Q-CONFIG-014. It joins the already observed
callback/reload/opener and sentinel-handle getter/store to an actual ready
Preferences screen in one restored run. It adds no driver query, audible
output, fresh-startup proof, continuous frame/state-writer proof, complete
reading or rule-status promotion. Other roots/keys/gate branches, reentrant
entries, unmonitored writers and driver/storage/source obligations remain.

## Alternatives

A timer stop alone would not prove readiness or completed input; both are
checked independently here. Matching repeated runs do not supply missing
caller or write histories. The readiness check is a named window-state
observation, not visual parity or evidence that every displayed pixel is correct.

## How to reproduce

Use checkout-local evidence Python with -I and
tools/research/rust-dos-runs/preferences_readback.py, source
v181-q063-native-save-input-20261104, an unused run name and saved-keyboard.
Tooling at c61798c1 includes five synthetic delivery/endpoint controls and
retains prior source/header/key/rejection/history checks. Reviewed local
outputs are GAME_DIR/captures/rust-dos-preferences-readback/
v181-q014-saved-ready-a-20261108/ and sibling b. Earlier saved-key-a..e
remain explicitly incomplete under FND-CONFIG-254/255.
