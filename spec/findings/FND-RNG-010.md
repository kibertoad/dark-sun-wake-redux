---
id: FND-RNG-010
title: Fresh startup passes 44976 from overlay 180 to the seed setter and retains it through the opener checkpoint
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
tool: dinorefurb-dosbox-session 0.10.0 RustDosSession and read_saved_state; tools/research/rust-dos-runs/startup_window.py with declared seed-caller checks
environment: Windows; Python 3.12; pinned rust-dos v1.7.2 with executable SHA-256 310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4; deterministic 15000 instructions per millisecond; Sound Blaster 16 at 220, IRQ 5, DMA 1; no Ultrasound; default muted host audio
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0811..1000:0822
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067835..0x00067844
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:02E4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005084A..0x0005084E
    kind: file-data
---

## Observation

On 2026-10-15 a fresh DSUN.EXE -W0 -L run used the manifest-verified
private drive, licensed game.ins on D: and the pinned muted session
configuration of FND-UI-039. No prior checkpoint, input, memory write,
image capture or fixed-time continuation was used. The session acquired
the normal machine lock and closed normally after its checkpoint save.

FND-RNG-009 supplies the static startup caller, seed argument ABI and
return offset. The probe stopped at seed_random's entry, mapped from
1000:0811 to 05BA:0811. It read the far return and word argument from
SS:SP and validated the return against overlay 180's loaded trampoline
56B2:0020: its jump target was +0141 and its loaded segment matched the
return segment. The five bytes before that return matched a far call to
the relocated seed setter. Another caller would have been refused.

| Stop | CS:IP | DS | SS | SP | BP | AX | DX | RNG state |
|---|---|---|---|---|---|---|---|---|
| Seed setter entry | 05BA:0811 | 4D9A | 4D9A | FE54 | FF4C | AFB0 | 2F8A | 1 |
| Declared startup return | 611B:0663 | 4D9A | 4D9A | FE58 | FF4C | AFB0 | 2F8A | 44976 |

The six-byte frame held return 611B:0663 and argument 44976 (AFB0).
The entry instruction count was 2,884,253 at emulated time 697,542,616 ns;
the return count was 2,884,260 at 697,543,083 ns. The setter completed
seven instructions and its result equaled the argument as a 32-bit state,
with high word zero. The caller's separate DX word was not part of that seed.

The same session then reached the established opener call 1D35:02E4
with argument 1, instruction count 1,926,739,648 and emulated time
129,199,843,550 ns. The RNG state remained 44976. The known generator
entry and any further call to the observed seed setter were guarded;
neither guard interrupted this continuation. The admitted draw list was
empty. Emulated times were observations, never wait targets.

At that stop the package saved state opener with strict_match and
check_files true, copying the private C: alongside machine.state.
read_saved_state subsequently verified the saved folder against the
package journal: 194 hashed files, zero guarded writes, stop 1D35:02E4,
and checkpoint eligibility true. This was file/journal verification;
no emulator restore of the checkpoint was attempted in this finding.

The run and checkpoint remain in
GAME_DIR/captures/rust-dos-startup-window/seed-and-opener-20261015/.
The first startup-window.json mislabeled the checkpoint boolean as a
path string True. The package's independent journal recorded the real
state path and hashes correctly. The committed probe now reports that
boolean as checked and the actual directory separately; a synthetic
control covers this correction. The historical local report was retained.

## Interpretation

The recovered overlay-180 call actually seeds this fresh startup. It
explains the observed change from 1 to 44976 through a measured setter
execution, rather than inferring a draw from the changed state. It agrees
with FND-RNG-009's low-word argument reading and FND-UI-039's later state.
The captured seed and no-draw opener state provide a fixed starting point
for further recorded-run tooling once checkpoint restore is verified.

## Alternatives

- This one admitted startup path does not establish all reseeding paths,
  indirect writers, alternate editions or configurations (Q-RNG-003).
- DS/SS equality at the setter and its return does not establish the earlier
  clock helper's near-buffer identity or later deletion-map identity.
- The exact clock conversion, environment inputs and seed formula remain
  unread obligations of Q-RNG-003. This finding does not define an epoch
  or promise a portable seed for every runtime.
- The known generator guard is not a complete randomness census; unresolved
  rule callers and indirect state access remain Q-RNG-001.
- The checkpoint precedes the opener call. It is not a drawn window,
  gameplay state, deletion observation or validated rebuild replay.
- The verified saved files do not prove successful native restore; that
  capability needs its own actual session-package attempt.

## How to reproduce

Use the locked evidence Python and run startup_window.py with a new
alphanumeric, underscore or hyphen name. The recorded name was
seed-and-opener-20261015. The fixed probe resolves the approved installation,
verifies its selected manifest files and retains the standard session lock,
muted host audio and emulator fingerprint.

It first waits for seed_random, reads its named argument/return frame and
validates the startup trampoline and call target. It waits for only that
declared return, checks the result, then waits for the opener and saves the
checked local checkpoint. A different stop or caller is refused. All outputs
remain beneath the licensed installation; none are committed. Run
check_startup_window.py for synthetic caller rejection, unchanged pending
operation handles, diagnostic pause and checkpoint-metadata controls.
