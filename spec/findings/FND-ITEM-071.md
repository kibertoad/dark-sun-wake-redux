---
id: FND-ITEM-071
title: The equipment flag is zero at main entry after the observed startup initialization
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/startup_segments.py at 13944e3b; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

FND-ITEM-070 identifies an explicit startup zero fill before initialization
callbacks but does not establish the flag after those callbacks. On
2026-11-04 the fixed startup probe ran the approved installed DSUN.EXE
through an owned muted session, with arguments -W0 -L, and stopped before
main's first instruction. Only the verified executable was copied to the
isolated C: drive; the probe must not be continued into gameplay.

The program entered at 05BA:0000, with DS 05AA and SS 57E9. The main
breakpoint was reached at 1D35:0024, the relocation of 277B:0024. At that
stop DS and SS both equaled 4D9A and SP was FFD8. The independently checked
data-segment relocation also gave 4D9A. The named equipment flag read at
4D9A:6174 returned exactly one byte, value zero.

The probe admitted this read only after checking the main breakpoint,
data-segment identity and nonwrapping real-mode extent. Synthetic controls
for the observation contract are specified in the runtime tooling plan.
The guarded generator entry was not reached; initial and main RNG states
were both one, and the draw list was empty. The probe supplied no input,
made no guest memory writes or frame captures, and never resumed past main.
The owned session closed normally.

## Interpretation

Q-PARTY-118's initial endpoint now has a measured value on this fixed
startup path: zero after initialization and before game main executes.
This agrees with the earlier clear but does not show that the flag stayed
zero continuously, or that every initializer lacks a writer. It supplies
neither a restored-map value nor any value at an equipment effect consumer.
Other startup paths, computed/aliased/bulk writers, immediate and queued
children and the outer caller's eventual clear remain separate obligations.
There is no parity or complete-reading promotion.

## Alternatives

Inferring a nonzero value at this exact endpoint contradicts its guarded
byte observation. Inferring continuous preservation from the two endpoints
would omit intervening callbacks. Inferring later consumer values or the
same result under different command-line/environment state would exceed
the attempted path. No callback instruction trace was reconstructed.

## How to reproduce

Use the pinned runtime and project-local evidence Python with installed
DSUN.EXE length 634416, XXH3-128 e296af55ba2ecde7e77f555c90f33d0b:

```text
python -I tools/research/rust-dos-runs/startup_segments.py <fresh-run-name>
```

The accepted run is
GAME_DIR/captures/rust-dos-startup-segments/v181-q118-main-flag-20261104/.
Its startup-segments.json has SHA-256
432999ead51b9d93501fb7ca4e8169d397cf0ae0e1fe80dd8dace6840d1bc99c.
Check the recorded entry/main registers, named flag observation, empty draws
and unchanged RNG state. The session retains its configuration and output
privately. No original executable or captured memory is committed.
