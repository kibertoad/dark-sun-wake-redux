---
id: FND-CONFIG-254
title: Native post-Save turn admits forwarded P through callback reload and Preferences trampoline
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/preferences_readback.py and preferences_saved.py at b77eacbd; existing KeyTrace, history and strict restore helpers; dinorefurb-dosbox-session 1.0.0
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

FND-COMBAT-063 describes the ordinary full-screen return's combat-off copy
from leader to turn. FND-COMBAT-064 supplies earlier native confirmation.
FND-COMBAT-140 supplies a current-pin checked post-Save checkpoint produced
by completed F1/Enter input without guest setup writes. This finding reuses
that checkpoint, rather than assigning or repairing the turn field.

Two independent runs on 2026-11-08 use fixed saved-keyboard mode. Source
v181-q063-native-save-input-20261104 has successful native-save metadata,
empty writes, completed F1/Enter and checked saved-map. Strict pinned
restore verifies executable identity, exact instruction/time, named snapshot,
frame digest and input-map readiness. The private SAVE02.SAV is49739 bytes
with SHA-25643098e9bc5d678212f81f9deb4970b7904424d49a33e6b9938ff4d9dbf95c80c,
matching its source. No original save-file load or guest write is performed.

At restore 2F8B:097D, sequence6 and197002506750ns, actual mode is1,
signed turn1, leader1 and combat0. These are sampled named fields, not
continuous producer histories. The fixed gestures are Tab100ms followed by
P1ms only after ready Game Menu10500. Tab completes at197130000016ns.
P delivery remains pending at the final observed getter boundary.

Addresses and stack/key values below are hexadecimal; times are decimal ns.
Both runs have the same eight accepted samples:

| Phase | Stop | Sequence | Time | SP |
|---|---|---|---|---|
| State call | 61F7:1A82 | 41 | 197320439016 | FE6E |
| State return | 61F7:1A87 | 43 | 197320439416 | FE6E |
| Turn comparison | 61F7:1A8E | 45 | 197320439550 | FE6E |
| Close test | 61F7:1CBE | 47 | 197331162283 | FE6E |
| Close call | 61F7:1CC5 | 49 | 197331162416 | FE6E |
| Reload | 61F7:1CC8 | 51 | 197332649350 | FE6E |
| Opener call | 61F7:1EC4 | 53 | 197332649883 | FE6E |
| Preferences trampoline | 4D3C:0048 | 55 | 197332649950 | FE6A |

All have DS/SS4D9A, BPFECC, event kind6, key1970, callback4C73:002A,
handler return65A5:0039, mode2 and signed turn1. The state return AX is2.
Both menu pointers are81CF:0004 through close-call and null at reload and
later samples. SP agrees across the callback; the opener's far-call frame
is four bytes deeper at the trampoline, with outer pair61F7:1EC9. Existing
binding checks verify the key/menu trampolines and the declared opener call.
These samples admit this fixed callback/reload/opener route, not every
instruction or transfer within the child callback.

The resident music getter4028:011F is reached with SS4D9A, SPFE64,
master/music enabled, music byte100, slot/handleFFFF, card122 and mode4.
Its declared caller/store check succeeds, but KeyTrace's nested outer-pair
check rejects the run as keyboard outer return differs. The rejected actual
pair was not retained; neither its changed component nor relocation as its
cause is established. No native getter return, byte store or complete P
delivery is admitted. There are13 outer observations and no RNG draws.
The nine-field history has no observed changes through this boundary;
its positive mutation predicate and overall/keyboard/history completion
remain false. Owned watches and muted sessions close normally.

## Interpretation

The naturally produced turn supplies the missing positive gate state from
FND-CONFIG-253. Q-CONFIG-014 retains nested outer-pair lifetime/relocation
admission, paired getter return/store and completed P delivery on this route,
pre-baseline and between-sample writers, other keys/callers/gate branches,
reentrant entries and independent driver/storage/source obligations.
This adds no complete reading or rule-status promotion.

## Alternatives

An outer return-segment relocation is plausible given FND-COMBAT-140, but
the failing check retained neither component of the rejected pair. Changed
offset, changed segment or another frame-lifetime difference remains open.
The failure is an observer boundary, not evidence of original-game failure.

## How to reproduce

Use the pinned checkout-local evidence Python with -I and
tools/research/rust-dos-runs/preferences_readback.py, source
v181-q063-native-save-input-20261104, an unused run name, and saved-keyboard.
Reviewed local outputs are GAME_DIR/captures/rust-dos-preferences-readback/
v181-q014-saved-key-a-20261108/ and sibling b. Their keyboard, readback,
draw and named-history records agree. Both are deliberately incomplete.

