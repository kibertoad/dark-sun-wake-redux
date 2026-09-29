# Runtime access

What can be done with the original game running, and who can do it. The
`runtime-access` skill keeps this file current; see the
[work protocol](../vendor/upstream/work-protocol.md#runtime-access) (lines 42-60). This
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
[Running the original](../vendor/upstream/work-protocol.md#running-the-original) (lines 195-215)
says.

## BLD-GOG-EN-1.1

How it runs: GOG's DOSBox 0.74-2 on Windows 11, with `dosbox_darksun2.conf`
and `dosbox_darksun2_single.conf` (`machine=svga_s3`, `memsize 16`,
`cycles fixed 15000`, `sbtype sb16`), started through `RAVAGER.BAT`, as the
build entry and the environment of the owner's captures record.

Emulator harness: none. `tools/emu/` does not exist yet. Emulated calls are
always allowed, whatever the rules on running the game, so the last row
becomes `agent` once a tooling batch builds a harness that loads this build.

| Capability | Who | Tried | What would change it |
|---|---|---|---|
| Start it and bring it to a given state without a person | none | Not tried: the owner's rule in `AGENTS.md` bars agents from launching DOSBox. | The owner lifting that rule. |
| Send it input | person | The owner plays each live session by its script (`docs/live-sessions/`). | The same rule. |
| Read memory, set breakpoints, dump structures while it runs | none | Not tried. GOG's DOSBox 0.74-2 is a release build without the debugger, and agents may not attach to DOSBox. | A debugger-enabled DOSBox or DOSBox-X build, and the owner allowing an agent to attach to the process of a live session. |
| Load a patched save | person | Not tried. The owner can place a save in the installation's save slots and load it from the game's menu. | A live session that asks for it. |
| Capture frames and sound | person, frames only | Frames: the owner's Ctrl+F5 screenshots, 320x200 in the game's palette, cited by the dynamic findings of the COMBAT, PARTY and UI areas. Sound: not tried; DOSBox 0.74-2 records sound with Ctrl+F6. | A live session that asks for a sound recording. |
| Play back a recording the original made | none | The game has no recording feature the spec knows of, and DOSBox 0.74-2 does not replay input. | A finding that the game records and replays input. |
| Call a single function in the emulator harness (no run lock) | none | No harness. | A tooling batch that builds `tools/emu/` with Unicorn in 16-bit real mode, loading the resident MZ image of `DSUN.EXE`, which carries no packer. Code in the `FBOV` overlay pack stays out of reach, as the protocol says of overlay code. |
