---
id: FND-UI-039
title: Fresh rust-dos startup reaches the start-window opener call with argument one and matching data and stack segments
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
tool: dinorefurb-dosbox-session 0.10.0 RustDosSession; tools/research/rust-dos-runs/startup_window.py; fingerprint-verified private drive from probe_drive.py; bounded resident_listing.py and overlay_listing.py prerequisite reads
environment: Windows; Python 3.12; pinned rust-dos v1.7.2; deterministic 15000 instructions per millisecond; Sound Blaster 16 at 220, IRQ 5, DMA 1; no Ultrasound; default muted host audio
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:02C8..277B:02E9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000814AB..0x000814E2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005084A..0x0005084E
    kind: file-data
---

## Observation

The prerequisite static reading confirms main pushes the word 1 at
277B:02E2, then calls the 574B:0020 trampoline at 277B:02E4. FND-UI-035
identifies that trampoline's start-window opener. The opener establishes
its frame and tests the low byte of its argument at BP+6. Before the call,
the word at SS:SP therefore holds the argument, not a return address.
The preceding main path returns from the cinematic selection described
by FND-VIDEO-010 and clears two state bytes before this call.

On 2026-10-15 startup_window.py launched the installed fingerprint-matching
DSUN.EXE -W0 -L through the pinned session package. Its fresh private C:
contained the 191 manifest-verified selected files, including unchanged
SOUND.CFG; D: mounted game.ins read-only. No old checkpoint, input, memory
write, image capture or fixed-time continuation was used. The normal machine
lock was acquired and the session closed normally after the stop.

The probe translated static segments from the observed program-entry CS
05BA and waited for 277B:02E4, mapped to 1D35:02E4. It stopped there with
reason breakpoint, instruction count 1,926,739,648 and emulated time
129,199,843,550 ns. This time is an observation, not a readiness condition.
At the stop DS and SS were both 4D9A, ES was 442B, SP was FF52 and BP was
FFD6. Flags were 0292, with IF, SF and AF set and DF clear. The argument
word read at SS:SP was 1. The call itself had not executed.

The four-byte RNG state at mapped 57E0:384A was 1 at program entry and
44976 at the call-site stop. The guard at mapped RNG entry 1000:0822
was not reached; the admitted draw list remained empty. The changed state
is not reported as a generator draw or as a fully explained seed producer.

The local report is startup-window.json in
GAME_DIR/captures/rust-dos-startup-window/opener-20261015/, alongside the
session journal. These original-derived records are not committed.

## Interpretation

This establishes a state-based, no-input route on this configured machine
from program entry to the start-window opener call. Its argument agrees
with the bounded static reading, and DS/SS equality extends beyond main
entry's observation in FND-PARTY-219 to this later stop.

It supplies a further prerequisite for Q-PARTY-081; the actual deletion-map
call and intervening preservation obligations remain unanswered.

## Alternatives

- The opener has not executed. No drawn start window, callback readiness,
  Create Characters route or gameplay state is established by this stop.
- Equality at two stops does not prove that every intervening routine
  preserves the relationship or that the later deletion map uses SS storage.
- The RNG guard covers the known function in FND-RNG-001. It does not prove
  a census of other randomness mechanisms or explain every seed-state write.
- Only this edition, arguments, media and emulator configuration were tested.
  There is no rebuild replay or validated parity claim in this finding.

## How to reproduce

With the locked evidence Python, run
tools/research/rust-dos-runs/startup_window.py with a new alphanumeric,
underscore or hyphen run name. The recorded name was opener-20261015.
The helper verifies selected source fingerprints and writes only a new
private drive and reports beneath the licensed installation's captures.
The probe uses the normal lock, pinned muted session, destination
277B:02E4, guard 1000:0822, seed read 57E0:384A and argument read SS:SP.

Synthetic check_startup_window.py verifies that pending observations keep
the original operation handle and that a diagnostic pause observes that same
continuation, rather than treating pause's reply mapping as a new operation.
A wall-clock exhaustion is a diagnostic stop, never evidence of arrival.
