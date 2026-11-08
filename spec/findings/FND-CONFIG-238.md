---
id: FND-CONFIG-238
title: Restored Preferences entry stores getter result 255 over the music-level byte 100
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/preferences_readback.py at bdd2f514; dinorefurb-dosbox-session 1.0.0; project-local evidence Python 3.12.10
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

FND-CONFIG-235/236 describe the local Preferences entry, getter sentinel
exits and low-byte store. FND-SOUND-096 observes the earlier fresh startup
only through its first music gate. This finding supplies an actual paired
getter/result/store observation on a separately restored play-ready route;
it does not connect those two observations into an uninterrupted history.

On 2026-11-08 two independent owned sessions strictly restored the
play-ready checkpoint from
GAME_DIR/captures/rust-dos-regenerated/v181-q046-play-ready-20261104/.
The probe verified the pinned emulator, embedded approved DSUN.EXE identity,
saved stop and emulated time, complete named snapshot, frame hash and input-map
readiness before input. DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. The source checkpoint and canonical files
were not changed. Addresses and encoded sentinel words below are hexadecimal;
levels, sizes, counts, card/mode values and RNG values are decimal.

### Delivered input and actual caller admission

Each run delivered Tab, awaited the ready Game Menu, delivered motion to
virtual (160,101), and waited until the native selected control was BUTN/10303
before sending a single left click. The menu's resource number was 10500
and its flags were 8. Tab and click each used a 100ms hold. All three input
operations completed; no input was retried. The probe removed its UI poll
before the click, retaining getter and RNG guards.

The known resident getter was reached at 4028:011F. Its checked real-mode
DS and SS were both 4D9A, and SP was FEE4. The actual four-byte return frame
named 641F:003E. FND-INPUT-012's Preferences entry trampoline, relocated
from 5782:0048, named entry zero in that same live overlay segment. The
probe verified the caller's direct far-call target and following AL-store
operand against FND-CONFIG-235/236, rather than following an arbitrary return.

At getter entry both the master-sound and music-on bytes were one.
The old music-level byte was 100. The current library slot and getter handle
were both FFFF. The loaded configuration was at 68D0:0004, with card ID 122;
the library-mode word was 4. All 59 configuration bytes matched installed
SOUND.CFG and were unchanged at the three paired snapshots.

### Return word and following byte store

| Boundary | Actual stop | AX | Music-level byte |
| --- | --- | --- | --- |
| Getter entry | 4028:011F | 0 | 100 |
| Getter return, before store | 641F:003E | 255 | 100 |
| Immediately after the store | 641F:0041 | 255 | 255 |

The return and post-store SP values were FEE8, exactly four bytes above
entry SP. The live trampoline identity, DS/SS and full return word remained
admitted at those boundaries. The post-store byte equaled the observed AL.
Execution stopped before the following call; no Preferences frame or later
redraw, button, save or audible effect was measured.

The card and mode values do not select FND-CONFIG-236's early card-113 or
mode-3/5 exits. The observed FFFF getter handle and returned 255 agree with
its local sentinel contract. The probe did not trace the intermediate
comparisons or driver-query instructions, so this is not a universal proof
of their exclusion, nor evidence for non-sentinel driver result domains.

### Repetition and RNG record

Each run reached the endpoint in 40 named stops. The three readback snapshots
are identical between the independent sessions. Both started from RNG state
1718646187 and recorded the same one draw under RULE-SOUND-003: bound 3,
result 0, next state 2185262696. The existing recorder checked the original's
arithmetic and stopped on unknown callers; no reseeding was admitted.
All guest setup-write lists are empty. Both owned sessions closed normally
and released their run locks.

This is a repeat of the same strict checkpoint and seed, not a multi-seed
gameplay experiment or rebuild replay validation. Input-operation identifiers
and session metadata differ; no event journal was reconstructed when the
session record's event_log was null.

## Interpretation

Q-CONFIG-014 can remove unobserved getter caller/frame, result-word and
entry-byte-store admission for this fixed restored route. Preferences really
can store 255 here: the previously held 100 is not retained or clamped to
100 by this entry's store. This is a readback result, not a new-game default,
user-selected volume, drawn fill width or loudness measurement.

Retain other entry gates/callers, non-sentinel handle/driver-query admission,
later changes, computed/block/alias writers and the uninterrupted startup
history that a restored checkpoint does not provide. No whole-entry complete
reading, queue closure, parity validation or implementation result follows.
RULE-CONFIG-003 remains supported.

## Alternatives

- Preferences preserves the existing 100 on this route: the paired native
  before/after store reads show 100 then 255.
- A 255 result necessarily identifies card ID 113: the captured card is 122
  and the getter has other sentinel exits.
- The returned word is directly a bounded 0..100 UI setting: the observed
  byte store retains 255 without such a clamp.
- A completed pointer operation proves a native control selection: the probe
  separately waited for BUTN/10303 before clicking.
- This observation proves later bar appearance or continuous startup
  preservation: execution stopped before the next call after the store.

## How to reproduce

Use the pinned project-local evidence Python and current rust-dos. With the
unchanged source checkpoint above, run in separate new owned sessions:

```text
python -I tools/research/rust-dos-runs/preferences_readback.py v181-q046-play-ready-20261104 <fresh-run-name>
```

Accepted outputs under GAME_DIR/captures/rust-dos-preferences-readback/ are
v181-q014-preferences-e-20261108 and v181-q014-preferences-f-20261108.
Their preferences-readback.json SHA-256 values respectively are
0695c717064075a6d693e8a68ffa3a0b8dc8e3bb28d42048dc8073e225b4deb6 and
0ebfb4260747679d7dfe6b6dc09d9e7666e677819911c0fe8b904a08c8951a42.
Check passed=true, completed Tab/move/click delivery, the three paired
readback snapshots, initial seed and draw. Compare readback and draws arrays
between reports; decode each configuration field for equality with the
verified installed file. Earlier a..d outputs record tooling admission
failures and supply no getter/store result. Original-derived reports stay local.

The fourteen synthetic controls in check_preferences_readback.py verify the
source-bound poll, native-selection wait, actual entry stub, frame and result/
store guards. They are tooling controls, not original-game evidence.
