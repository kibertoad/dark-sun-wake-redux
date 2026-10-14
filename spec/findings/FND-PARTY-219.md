---
id: FND-PARTY-219
title: Rust-dos reaches game-main entry with DS equal to SS and no RNG draw after the initial MZ stack is replaced
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
tool: dinorefurb-dosbox-session 0.10.0 RustDosSession and tools/research/rust-dos-runs/startup_segments.py
environment: Windows; Python 3.12; rust-dos v1.7.2, executable SHA-256 310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4; deterministic 15000 instructions per millisecond; Sound Blaster 16 at 220, IRQ 5, DMA 1; no Ultrasound; default muted host audio
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..1000:00C0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:0024
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005084A..0x0005084E
    kind: file-data
---

## Observation

On 2026-10-15 the session package's read_lock reported no default machine
run lock. No lock was modified and no alternate lock path was used. The
fixed startup probe then acquired the normal lock, launched its own pinned
rust-dos, ran the installed fingerprint-matching DSUN.EXE and closed the
session successfully. The earlier refusal is retained as runtime history
in docs/RUNTIME.md.

C: held a copy of the verified DSUN.EXE only; the probe stops before main's
first instruction and does not need game assets or saves there. D: mounted
the licensed game.ins media. The target was DSUN.EXE -W0 -L. There was no
input, memory write, image capture, patched save or fixed-time continuation.
The run waited for the documented main address from FND-PARTY-038 and
FND-PARTY-215 using continue_(until=...). A breakpoint on the documented
RNG entry would have stopped the probe before that function executed.

| Stop | CS:IP | DS | ES | SS | SP |
|---|---|---|---|---|---|
| Program entry | 05BA:0000 | 05AA | 05AA | 57E9 | 0080 |
| Game-main entry | 1D35:0024 | 4D9A | not used in this comparison | 4D9A | FFD8 |

At main BP was FFE8; flags were 0246, with IF, ZF and PF set and DF clear.
The completed operation's reason was breakpoint at 1D35:0024, with
instruction count 26,282. Emulated times in the report were 1,500,750 ns
at program entry and 5,369,016 ns at main; these were observations, not
wait targets.

The original load segment was 05BA, so static segments based at 1000 map
by subtracting 0A46: header SS 622F maps to 57E9, data segment 57E0 to
4D9A, and main 277B:0024 to 1D35:0024. The four-byte RNG state at the
mapped 57E0:384A was 1 at both stops. The generator guard at 05BA:0822
was not reached and the recorded draw list was empty.

The local result is startup-segments.json under the licensed installation's
captures/rust-dos-startup-segments/main-segments-20261015-retry/.
The session package also retains its run journal there; neither is committed.

## Interpretation

This run agrees with FND-PARTY-215's explicit startup stack replacement:
SS no longer holds the initial header segment when game main begins;
DS and SS are equal at that stop. It verifies the fixed probe's address
translation, state-based stopping and register/RNG-state reads on this host.

The result removes startup-main equality from Q-PARTY-081's unobserved
obligations. It does not settle equality or preservation at the later
deletion-map call, nor establish a gameplay screen or whole-party cleanup.

## Alternatives

- Only the stated executable arguments and deterministic environment were
  observed. Other startup paths, versions or configurations were not tested.
- This stops before main executes. No inventory, intro, combat, character
  generation, input delivery or asset loading behavior was exercised.
- Equality at main is not proof of the later near-map storage identity;
  Q-PARTY-081 retains that call and intervening preservation question.
- The guard covers the known RNG function from FND-RNG-001. This run does
  not prove an independent complete census of all possible random mechanisms
  and does not validate a rebuild replay fixture.

## How to reproduce

From the commit adding this finding, use the locked evidence Python from
the repository root and run startup_segments.py with a new alphanumeric,
underscore or hyphen run name. The probe resolves the approved installation
from project-config.json or DARK_SUN_WAKE_REDUX_GAME_DIR, verifies DSUN.EXE,
uses the normal machine lock and writes the local report beneath GAME_DIR.
The recorded command was:

`artifacts/evidence-python/Scripts/python.exe -I tools/research/rust-dos-runs/startup_segments.py main-segments-20261015-retry`

The probe's only destination is main 277B:0024, translated from the actual
program-entry segment. Its guard is RNG 1000:0822 and its four-byte read
is RNG state 57E0:384A. Do not continue this executable-only C: drive to
gameplay or treat its emulated timestamps as readiness conditions.
