# Runtime access

What can be done with the original game running, and who can do it. The
`runtime-access` skill keeps this file current; see the
[work protocol](../vendor/upstream/work-protocol.md#runtime-access) (lines 42-62). This
file says what is true now: replace an answer when a tool, emulator, machine or
owner rule changes it. Findings from runs go in `spec/`, never here.

Static analysis is the main source of evidence. In this repository coding
agents never launch, control, capture or stop DOSBox (`AGENTS.md`, "Native
runtime visual validation"), so every run of the original is a live session
the owner plays and captures alone, and the agent reads the captures
afterwards. No agent run takes place, and the machine's run lock
(`C:\ProgramData\refurbished-dinosaurs\run.lock`, or the path in
`REFURBISHED_DINOSAURS_RUN_LOCK`) is never taken from this repository. If the
owner lifts that rule, an agent run takes the lock as the protocol's
[Running the original](../vendor/upstream/work-protocol.md#running-the-original) (lines 331-365)
says.

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
| Start it and bring it to a given state without a person | none | Not tried: the owner's rule in `AGENTS.md` bars agents from launching DOSBox. | The owner lifting that rule. |
| Send it input | person | The owner plays each live session by its script (`docs/live-sessions/`). | The same rule. |
| Read memory, set breakpoints, dump structures while it runs | none | Not tried. GOG's DOSBox 0.74-2 is a release build without the debugger, and agents may not attach to DOSBox. | A debugger-enabled DOSBox or DOSBox-X build, and the owner allowing an agent to attach to the process of a live session. |
| Load a patched save | person | Not tried. The owner can place a save in the installation's save slots and load it from the game's menu. | A live session that asks for it. |
| Capture frames and sound | person, frames only | Frames: the owner's Ctrl+F5 screenshots, 320x200 in the game's palette, cited by the dynamic findings of the COMBAT, PARTY and UI areas. Sound: not tried; DOSBox 0.74-2 records sound with Ctrl+F6. | A live session that asks for a sound recording. |
| Play back a recording the original made | none | The game has no recording feature the spec knows of, and DOSBox 0.74-2 does not replay input. | A finding that the game records and replays input. |
| Call a single function in the emulator harness (no run lock) | agent, resident far roots within the current no-argument contract | Unicorn 2.1.4 loads the hash-verified GOG resident MZ and applies its relocations. FND-CONFIG-193 initializer controls return with source-written roots/free flags, unchanged loaded high bytes and restored registers. | Supported named parameter/layout bindings extend input cases. Overlay code, native hardware and timing remain outside this harness. |


Probe: none. Native process attachment and memory instrumentation are barred
by the owner-only DOSBox policy. Recorded runs are not available; the protocol's
[Recorded runs](../vendor/upstream/work-protocol.md#recorded-runs) (lines 341-353) guidance
does not override those limits. Each emulated-call setup must document port
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
