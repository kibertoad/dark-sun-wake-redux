# Runtime access

What can be done with the original game running, and who can do it. The
`runtime-access` skill keeps this file current; see the
[work protocol](../vendor/upstream/work-protocol.md#runtime-access) (lines 42-62). This
file says what is true now: replace an answer when a tool, emulator, machine or
owner rule changes it. Findings from runs go in `spec/`, never here.

Static analysis is the main source of evidence. The owner's rule
(`AGENTS.md`, "Runs of the original") lets a coding agent run the original
only through the toolkit's `dinorefurb-dosbox-session` package, in an
emulator process the package starts: rust-dos v1.7.1 through `RustDosSession`,
or DOSBox-X through `DosboxSession` as a recorded fallback for work a known
rust-dos incompatibility blocks. Host sound stays muted (rust-dos on SDL's
dummy audio driver; DOSBox-X with `mixer master 0:0` and MIDI output `none`),
the emulated sound devices stay configured, `keep_host_sound` is never passed
and `nosound` never set. Agents never operate GOG's DOSBox 0.74-2; runs of it
are live sessions the owner plays and captures alone. The package takes the
machine's run lock (`C:\ProgramData\refurbished-dinosaurs\run.lock`, or the
path in `REFURBISHED_DINOSAURS_RUN_LOCK`) as the protocol's
[Running the original](../vendor/upstream/work-protocol.md#running-the-original) (lines 391-425)
says.

Setup: `./tools/Restore-ToolDependencies.ps1` installs
`dinorefurb-dosbox-session` 0.8.0 into `artifacts/evidence-python`, and
`./tools/Install-RustDos.ps1` installs the pinned rust-dos v1.7.1 Windows
build into `artifacts/rust-dos/`, refusing an archive or executable whose
SHA-256 differs from the package's pins. On 2026-10-13 the toolkit's
`native/verify_rustdos.py` at `dinorefurb-dosbox-session@0.8.0` passed with
that build here, in a normal and a deterministic run: a synthetic `.COM`
stopped at its entry and at a breakpoint, registers read back, a stale write
was refused (409, `precondition: state`), a guarded write verified, and the
program exited with code 0. That run used no game file, so the game
capabilities below that need a run of the original are still `none`.

## BLD-GOG-EN-1.1

How it runs: GOG's DOSBox 0.74-2 on Windows 11, with `dosbox_darksun2.conf`
and `dosbox_darksun2_single.conf` (`machine=svga_s3`, `memsize 16`,
`cycles fixed 15000`, `sbtype sb16`), started through `RAVAGER.BAT`, as the
build entry and the environment of the owner's captures record.

Emulator harness: tools/emu/resident-call.mjs with Unicorn 2.1.4, restored
from hash-locked test-only wheels. The published executable-reader loads the
MZ resident image and source relocations. A declared far, no-argument root can
be called without a game process or run lock. Raw memory and argument seeding
wait for supported layout/parameter bindings; FBOV code stays out of reach.

| Capability | Who | Tried | What would change it |
|---|---|---|---|
| Start it and bring it to a given state without a person | none | Not tried with the game. `dinorefurb-dosbox-session` 0.8.0 and rust-dos v1.7.1 are installed and passed the synthetic native check above: the package takes and releases the run lock, prepares an empty C:, mounts media read-only, mutes host audio, starts the target stopped at its entry and records each session. Deterministic runs fix the start time and speed and deliver input at emulated times. | A rust-dos run showing the game reaches a given state as it does under GOG's DOSBox 0.74-2. |
| Send it input | person | The owner plays each live session by its script (`docs/live-sessions/`). | The same rule. |
| Read memory, set breakpoints, dump structures while it runs | none | Not tried with the game. GOG's DOSBox 0.74-2 is a release build without the debugger, and agents never attach to it. A `RustDosSession` reaches rust-dos's HTTP debug server: registers, memory, breakpoints and operation observation, plus guarded writes to stopped guest memory, where each write names a field in a caller-supplied contract, checks the field's current SHA-256, reads it back, and fails the run on any refusal. The synthetic check above exercised breakpoints, register reads and a guarded write. A session can keep an event log, `events.jsonl` in the run directory, with event kinds and an outcome contract the restoration supplies. Gated breakpoints exist for DOSBox-X sessions only; neither backend has save states. | A session-package run that reads a known field and stops at a breakpoint. |
| Load a patched save | person | Not tried. The owner can place a save in the installation's save slots and load it from the game's menu. | A live session that asks for it. |
| Capture frames and sound | person, frames only | Frames: the owner's Ctrl+F5 screenshots, 320x200 in the game's palette, cited by the dynamic findings of the COMBAT, PARTY and UI areas. Sound: not tried; DOSBox 0.74-2 records sound with Ctrl+F6. | A live session that asks for a sound recording. |
| Play back a recording the original made | none | The game has no recording feature the spec knows of, and DOSBox 0.74-2 does not replay input. | A finding that the game records and replays input. |
| Call a single function in the emulator harness (no run lock) | agent, resident far roots within the current no-argument contract | Unicorn 2.1.4 loads the hash-verified GOG resident MZ and applies its relocations. FND-CONFIG-193 initializer controls return with source-written roots/free flags, unchanged loaded high bytes and restored registers. | Supported named parameter/layout bindings extend input cases. Overlay code, native hardware and timing remain outside this harness. |


Probe: none. No probe exists yet. The owner's rule allows one only inside a
session-package DOSBox-X process with host sound muted; it is recorded here once
a run shows it working, and then follows the protocol's
[Recorded runs](../vendor/upstream/work-protocol.md#recorded-runs) (lines 401-413). Each emulated-call setup must document port
models, video memory substituted with RAM and the limits of each comparison,
within the current resident-call contract below.

## Resident-call stubs and limits

The harness permits named interrupt stops only. It cannot assume an unknown
handler returned or retained memory/registers. Port substitutions separately
name read/write direction, width and the glossary name of each supplied value.
An undeclared interrupt, port, code path or instruction boundary stops the call;
an FBOV overlay interrupt cannot be supplied as a stub. Synthetic controls cover
these failures. The actual initializer reaches no hardware service or port.

Loaded memory comes from the resident MZ, with synthetic zero-filled RAM outside
it and a harness-owned far-return frame. Video addresses are ordinary RAM; no
pixels, device state, sound, input or timer are emulated. Instructions, writes,
observations and branch-direction coverage are bounded and recorded locally.
The initializer controls seed no original memory field. No game claim/parity
status or startup-to-transfer/caller admission changes merely from these tests.
See tools/emu/README.md and docs/VALIDATION.md.
