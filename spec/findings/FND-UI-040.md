---
id: FND-UI-040
title: A strict native restore of the opener checkpoint runs its declared call to return with unchanged RNG state
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
tool: dinorefurb-dosbox-session 0.10.0 RustDosSession and read_saved_state; tools/research/rust-dos-runs/restore_opener.py; bounded resident_listing.py and overlay_listing.py prerequisite reads
environment: Windows; Python 3.12; pinned rust-dos v1.7.2 with executable SHA-256 310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4; deterministic 15000 instructions per millisecond; Sound Blaster 16 at 220, IRQ 5, DMA 1; no Ultrasound; default muted host audio
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:02E2..277B:02F2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000815C2..0x00081634
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005084A..0x0005084E
    kind: file-data
---

## Observation

FND-RNG-010 created the strict, file-checked opener checkpoint before
main calls the start-window opener. The prerequisite static reading
confirms that main's far call at 277B:02E4 returns to 277B:02E9, before
the caller removes its argument word. FND-UI-035 identifies the opener.
Its bounded tail requests WIND 19500 with the overlay callback, stores
the acquired pointer, makes further window/setup calls, restores its
frame and returns far. Those downstream calls are not fully read here.

On 2026-10-15 restore_opener.py verified the source checkpoint with
read_saved_state, then started a new owned pinned rust-dos through
RustDosSession with that start_state. The package copied the checkpoint's
C: into the new run and loaded it with strict hardware/media matching and
checked files. D: still mounted licensed game.ins; SOUND.CFG and emulated
sound settings were unchanged, and host audio remained muted. There were
no inputs, guarded memory writes, screenshots or fixed-time continuations.

The package verified the load stop against the saved CS:IP, instruction
count and emulated time. The probe required reason load and verified the
instruction there as the declared far call to the relocated 574B:0020
trampoline. It reinstalled guards on the known generator and seed setter
after strict load reset the debugger, then waited for the declared return.

| Stop | Reason | CS:IP | DS | SS | SP | BP | Instruction count | Emulated ns |
|---|---|---|---|---|---|---|---|---|
| Restored opener call | load | 1D35:02E4 | 4D9A | 4D9A | FF52 | FFD6 | 1,926,739,648 | 129,199,843,550 |
| Opener returned | breakpoint | 1D35:02E9 | 4D9A | 4D9A | FF52 | FFD6 | 1,933,205,053 | 130,771,587,350 |

The RNG state was 44976 before and after this call. Neither the known
generator nor seed-setter guard interrupted the continuation; the admitted
draw list was empty. No wall-clock diagnostic limit was reached. Emulated
times were observations, not readiness conditions.

At the return stop the package saved a second strict, file-checked
checkpoint named opened. Its read_saved_state verification succeeded:
194 hashed files, checkpoint eligibility true and zero guarded writes.
This second checkpoint was saved and file-verified, not natively restored
in this finding. The source opener checkpoint was actually restored.

The new report, package journal and opened checkpoint remain under
GAME_DIR/captures/rust-dos-restore-opener/restore-and-return-20261015/.
The source is the opener checkpoint of FND-RNG-010. The session closed
normally and released its normal run lock. No original-derived state,
private drive or capture was committed.

A second run, restore-and-return-20261015-verified, enforced the configured
DSUN.EXE size and XXH3 fingerprint on the checkpoint's copied executable
before launch. It reached the same return with the same instruction count,
emulated time and unchanged seed 44976, and saved another checked opened
state. Both guards again admitted no known generator or setter call. This
repeat tests the restore/call path, not a broader gameplay replay.

## Interpretation

The state-based startup route now has a verified native checkpoint restore
and a measured return from the opener call. Future probes can start from
the saved source state rather than repeat the intro, subject to the
package's file, media, hardware and debugger-reset checks. This is a
runtime/tooling prerequisite for Q-PARTY-081, not its deletion-map answer.

## Alternatives

- A call returning does not establish displayed pixels, successful input
  handling, a gameplay state or any button's complete effect.
- The acquired window pointer and internal state were not read; no complete
  resource-loading, callback or presentation claim is made.
- Equality at these stops is not complete intervening segment-preservation
  coverage and does not identify the later deletion map's storage.
- The guards cover the known generator and setter, not a complete census
  of independent randomness or indirect state writes.
- The new opened checkpoint still needs a native restore attempt before
  its own restore route is relied on. Success of the source opener state
  does not prove every checkpoint or platform works.

## How to reproduce

First create the opener checkpoint with startup_window.py as FND-RNG-010
describes. With the locked evidence Python, run restore_opener.py with
that source startup-run name and a new plain run name. The recorded
arguments were seed-and-opener-20261015 and restore-and-return-20261015.
Both paths are confined to the licensed installation's captures roots.

The tool accepts only a pinned-emulator checkpoint named opener, requires
the copied executable's approved build fingerprint and the documented
call-site offset and instruction, installs its guards and
waits by address for the return. It compares RNG state and saves opened
only after that stop passes. Synthetic check_restore_opener.py controls
reject another stop, target, opcode or short instruction observation.
Synthetic copied-executable controls reject missing, resized or changed
same-size contents before any session is started.
