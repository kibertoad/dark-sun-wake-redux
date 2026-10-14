# Runtime access

What can be done with the original game running, and who can do it. The
`runtime-access` skill keeps this file current; see the
[work protocol](../vendor/upstream/work-protocol.md#runtime-access) (lines 42-62). This
file says what is true now: replace an answer when a tool, emulator, machine or
owner rule changes it. Findings from runs go in `spec/`, never here.

Static analysis is the main source of evidence. The owner's rule
(`AGENTS.md`, "Runs of the original") lets a coding agent run the original
only through the toolkit's `dinorefurb-dosbox-session` package, in an
emulator process the package starts: rust-dos v1.7.2 through `RustDosSession`,
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
`dinorefurb-dosbox-session` 0.9.0 into `artifacts/evidence-python`, and
`./tools/Install-RustDos.ps1` installs the pinned rust-dos v1.7.2 Windows
build into `artifacts/rust-dos/`, refusing an archive or executable whose
SHA-256 differs from the package's pins.

How an agent run starts the game (2026-10-14, `tools/research/rust-dos-runs/`):
a deterministic run at 15000 instructions a millisecond, C: a copy of the
installed game's files, D: the CD image `game.ins` mounted read-only, and the
target `DSUN.EXE -W0 -L` as `RAVAGER.BAT` gives it. The installed `SOUND.CFG`
names a card at 0x220, IRQ 5 and DMA 1. With rust-dos's default card at IRQ 7
the game stops at once with `Mel Fatal Error #: 26` and `DSP Detect Fail` (exit
code 0x1A), so the run gives rust-dos a Sound Blaster 16 at 0x220, IRQ 5, DMA 1
and HDMA 5 and no Ultrasound (`RustDosConfig.sound_blaster` and `ultrasound`,
dinorefurb-dosbox-session 0.10.0), and C: holds the installed `SOUND.CFG`
unchanged. The program loads with
its first segment at `05BA` (the repository's load image puts it at `1000`).

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
| Start it and bring it to a given state without a person | agent | 2026-10-14: the run above reaches the start window (SCR-UI-001), fully drawn by 180 s of emulated time after the start, with Esc pressed every 10 s through the intro, and the screens after it by mouse clicks; checkpoints (`save_state` with `check_files`) taken at the start window, after an `ADD` and after a `DROP`, started later runs that reached the same screens. | A game screen or state the run cannot reach. |
| Send it input | agent (keyboard, mouse) | 2026-10-14: keys (Esc), left, right and middle mouse clicks (the game's mouse handler gets the driver's bits for each, FND-PARTY-204), and a key held across a click through a `batch` of key down, click and key up (left Ctrl, FND-PARTY-206), given in the mouse driver's coordinates (`"coords": "virtual"`, the game's 320 x 200); a click in screenshot coordinates lands off target because the game sets the driver's range. In a deterministic run input is taken while paused and delivered once the run resumes. The owner still plays each live session by its script (`docs/live-sessions/`). | A device the game reads that the session cannot drive. |
| Read memory, set breakpoints, dump structures while it runs | agent | 2026-10-14: memory reads at a stop gave the party kind bytes, a 66-byte character box record and the leader word, which held 0 at the start window as FND-PARTY-171 reads it from the load image; a breakpoint at the generator `1000:0822` (here `05BA:0822`) stopped there and the stack gave the return address. GOG's DOSBox 0.74-2 has no debugger and agents never attach to it. Guarded writes, the event log and gated breakpoints are as the package describes; no write to the game's memory has been made yet. | A field whose read or write the server refuses. |
| Load a patched save | person | Not tried. The owner can place a save in the installation's save slots and load it from the game's menu. | A live session that asks for it. |
| Capture frames and sound | agent, frames only; person, frames only | Agent: `screenshot()` gives a 640 x 400 PNG of the 320 x 200 screen (2026-10-14), muted host sound, no sound capture tried. Person: the owner's Ctrl+F5 screenshots, 320x200 in the game's palette, cited by the dynamic findings of the COMBAT, PARTY and UI areas; DOSBox 0.74-2 records sound with Ctrl+F6, not tried. | A sound capture from a session-package run. |
| Play back a recording the original made | none | The game has no recording feature the spec knows of, and DOSBox 0.74-2 does not replay input. | A finding that the game records and replays input. |
| Call a single function in the emulator harness (no run lock) | agent, resident far roots within the current no-argument contract | Unicorn 2.1.4 loads the hash-verified GOG resident MZ and applies its relocations. FND-CONFIG-193 initializer controls return with source-written roots/free flags, unchanged loaded high bytes and restored registers. | Supported named parameter/layout bindings extend input cases. Overlay code, native hardware and timing remain outside this harness. |


Probe: `tools/research/rust-dos-runs/run_plan.py <run-dir> <plan.json>`, run with
the evidence Python and `GAME_DIR` set to the installed build (README beside it).
It follows a plan of inputs from a checkpoint, records each draw of `rng` as
`{rule, bound, result}` (script instruction `0x52` under RULE-SCRIPT-006,
`random_mod` under the rule of its calling routine), checks each result against
the original's, can write a seed into `rng_state`, and reads memory and takes
screenshots at its stops. It waits for emulated times of a deterministic run,
not for states the game reaches. EXP-PARTY-001 is its first experiment. It
follows the protocol's
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
