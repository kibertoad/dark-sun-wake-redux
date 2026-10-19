---
id: FND-UI-054
title: Pinned rust-dos 1.7.5 regenerates and independently restores all six retained gameplay checkpoint routes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession and read_saved_state; tools/research/rust-dos-runs/startup_window.py, restore_opener.py, regenerate_checkpoints.py, checkpoint_rng.py and verify_checkpoints.py; Python 3.14.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; Sound Blaster 16 at 220 IRQ 5 DMA 1 HDMA 5, no Ultrasound, default muted host audio
---

## Observation

The owner requested fresh checkpoints under the pinned runtime on 2026-10-19.
The six retained historical states were made with rust-dos 1.7.2 and retain
their emulator SHA-256. The session package rejects a different recorded
emulator digest before starting a restore. That guard does not establish
whether the emulator's native state format can decode an older release's
state. The historical checkpoints were preserved without metadata changes.

The approved installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. startup_window.py prepared its verified
drive, reached the seed and opener stops of FND-RNG-010 and saved opener.
restore_opener.py restored it in a new owned process and reached the
return of FND-UI-040 with unchanged RNG state 44976, saving opened.
The new menu driver independently restored opened before acting.

The driver read the current window and its state bits (FMT-UI-001,
FND-UI-045), the registered global handler (FND-PARTY-044), the slot-kind
table (FND-PARTY-049) and the 66-byte box records through their far pointer
(FND-PARTY-046). A bounded polling quantum only advanced execution; the
observed state determined readiness. No guest-memory writes were used.
The seed setter and generator were guarded throughout.

| Checkpoint | State at save | Independent native restore |
|---|---|---|
| start | current window 19500, mode 0, all four slot kinds 0 | passed |
| added | current window 11500, slot kinds 0,0,2,0 | passed |
| dropped | window 14000 showing the drop confirmation; character still present | passed |
| afterdrop | current window 11500, all slot kinds and box-presence words 0 | passed |
| talk | speech window 12500, mode 2, four default party members, opening response selected | passed |
| play-ready | current window 19501, mode 1, slot kinds 0,2,0,2 and leader 1 | passed |

Each save used strict matching and file checks. Each restore used a new
owned process and verified the saved instruction, emulated time, named
state fields and SHA-256 of the captured PNG. The Talk verification also
saved and loaded a checked roundtrip state within the verification process
and compared its fields and frame again. Final verification repeated that
same-process roundtrip for all six states. Each also deliberately changed
the proposed Sound Blaster IRQ from 5 to 7; the session rejected that
hardware mismatch before launching. Checkpoints and PNGs remain local.

Mouse actions were confirmed by their input-delivered operation receipts.
Combined movement-and-click requests selected the intended character
button but did not open its menu in the bounded attempts. A callback stop
observed the right-button event at coordinates 70,107; a later selection
stop observed BUTN/11302. At its release branch, the remembered press
control differed from the current selected control. Separating a completed
motion operation from the click opened window 14000 with the expected
566A:0052 handler. This is a working input sequence, not a complete causal
analysis of the failed combined gesture or a claim of an emulator defect.

Window registration alone preceded the visible conversation panel. The
driver therefore restored conversation-entry and continued to the event
builder 39D1:097D, which overlay 199's text wait calls directly. The earlier
attempt to reach 39D1:0854 exhausted its bounded observation budget and
closed its owned session; it supplied no successful readiness claim.
At the event-builder stop, window 12501 held the responses. Selecting the
third visible response at 100,175 reached the speech window and reproduced
the retained Talk checkpoint's dialogue frame.

The default-party startup trace recorded eight RULE-SCRIPT-006 draws.
Every recorded bound/result and next generator state agreed with the
declared arithmetic. Unknown generator callers stop rather than silently
discard a draw. These capability checks do not compare the rebuild's RNG
trace and therefore establish no gameplay parity row.
The two-character setup also confirmed delivery of Esc and returned from
Create Characters to window 19500; this admits that tested key gesture.
Its first map input stop had the expected room and occupied slots but
leader zero, so it was not accepted as the retained Play state. Restoring
that state and delivering key 2 selected leader 1 through the route already
recorded by FND-PARTY-178. The final state is named play-ready because the
session reserves the loaded name play. This route recorded four draws
before that restore (three RULE-SCRIPT-006 and one RULE-SOUND-003), then
three RULE-SCRIPT-006 draws during leader selection. All calculations and
the seed continuation across the restore agreed. An earlier custom-party
attempt exhausted its conversation-window predicate; the corrected route
uses the native map input builder, mode and party fields instead.

## Interpretation

The pinned runtime supports these six state routes, left/right mouse
delivery, frames, named memory reads, breakpoints and strict independent
checkpoint restore. The scope is the tested states and fields. A saved
emulator state is not a patched game save; frames do not validate sound.

## Alternatives

Native cross-release state-format compatibility remains untested. The
package's digest guard is sufficient reason to regenerate states for the
approved workflow, without asserting intrinsic format incompatibility.
Other keyboard gestures are not inferred from the tested Esc and 2 routes.

## How to reproduce

Use the pinned tools and approved installed build. Create opener with
startup_window.py v175-regenerate-20261019, then opened with
restore_opener.py v175-regenerate-20261019 v175-regenerate-opened-20261019.
Run regenerate_checkpoints.py v175-party-d-20261019 party; verify with
verify_checkpoints.py v175-party-d-20261019 followed by a new verification
prefix and start added dropped afterdrop. Run the talk-entry stage under
a fresh name, then talk-resume with that source run and a new run name;
verify its talk state with verify_checkpoints.py.
For Play, run play-entry under a fresh name, then play-resume under another
fresh name with that source run. Verify its play-ready state. This preserves
the initial map state and its trace separately.

Retained successful runs are under GAME_DIR/captures/rust-dos-regenerated/:
v175-party-d-20261019, v175-talk-entry-20261019 and
v175-talk-final-20261019, v175-play-final-20261019 and
v175-play-ready-b-20261019. Final independent and same-process checks use
v175-full-verify-20261019 for start, added, dropped, afterdrop and talk;
Play uses v175-final-play-verify-20261019-play-ready. Each run retains
session.json and regeneration.json or restore-check.json, with checked
state directories and local frames. Historical gameplay states remain in
the ignored artifacts/runs directories named by their older findings.
