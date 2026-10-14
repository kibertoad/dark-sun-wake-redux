---
id: FND-PARTY-199
title: The movement routine 2D40:10AE reaches its call of the attack routine at 2D40:13D9 only when the fight state at 4C10:0019 is not 0, and that word reads 0 on Create Characters
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
tool: Python 3.14.7 with Capstone 5.0.7 (disassembly of the load image at segment 1000, MZ relocation table read); dinorefurb-dosbox-session 0.10.0 RustDosSession, rust-dos v1.7.2 debug server (memory reads, key input), tools/research/rust-dos-runs/run_plan.py with plans/fight-state-create.json
environment: Windows 11 Pro 10.0.26200; rust-dos v1.7.2 (executable SHA-256 310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4), deterministic at 15000 instructions a millisecond from 1995-04-11 12:34:56, host sound muted, Sound Blaster 16 at 220, IRQ 5, DMA 1, HDMA 5, no Ultrasound
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:1197..2D40:13DE
---

## Observation

Addresses are in the load image based at segment 1000.

`2D40:1197` loads ES with `0x3C10`, a word the MZ relocation table names at `2D40:1198`, so ES
is segment `4C10` once loaded, and compares the word at `ES:0019` with 0; when it is 0 it jumps
to `2D40:1452` (`2D40:1197..2D40:11A4`). Otherwise it calls `2D40:3E64` for the slot in `si`,
needs it to return nonzero, and for each slot `di` from 0 to 47 (`2D40:11C1`, `2D40:1449..144F`)
calls `2D40:3E64` again, needs it to return nonzero, needs the `combat_mark` of the combatant it
gave for `di` to be 1 and the byte at `3E71:0B1A` plus that combatant's number to be nonzero, and
then compares the two combatants' bytes at `+0x15` against the bit masks `0x71`,
`0xF80`, `0x184`, `0xC58`, `0x608` and `0x964` (`2D40:11A7..2D40:1394`). The passing branches go
to `2D40:1397`, which calls `2D40:35EF` and goes on only when it returns 1 or less; it then calls
`2D40:2196`, `4671:0098`, `1C5F:018C` and, at `2D40:13D9`, `4671:0057` with the two slots, `0xE`,
segment `0x3F49` and 1. `4671:0057` is the trampoline that FND-PARTY-197's reach followed into the
attack routine overlay 173 `+1CC1`, and the only route between `2D40:10AE` and `2D40:13D9` in that
reach's chain.

FND-PARTY-089 reads the word at `4C10:0019` as the fight state, 0 outside a fight.

**Run.** `4C10` is `41CA` in the run. `fight-state-create.json` starts from the checkpoint `added`
of `add.json` (Create Characters with one stored character) and reads `41CA:0019` and
`DS:0DAB` before and after pressing `c`, which opens the USE window (FND-PARTY-198). Both words
read 0 both times.

## Interpretation

`set_leader`'s call of `2D40:10AE` (overlay 182 `+1E48`) reaches the attack routine only during a
fight. On Create Characters the fight state is 0, so the route FND-PARTY-197 found from the leader
button to the attack routine and `+3CDA` does not run there unless something sets the fight
state first.

## Alternatives

- Whether anything before START GAME sets `4C10:0019` to a value other than 0 is not settled
  here. FND-PARTY-089 lists the stores to it; overlay 193 `+0533`, which stores 1, is in the
  overlay that FND-PARTY-197's other route passes through.
- The run reads the word at two moments; a change between them that is undone would not show.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, disassemble the load
image (file offset of `S:O` is `(S - 0x1000) * 16 + O + 0x5200`) from `2D40:1197` to `2D40:13DE`
and look up `2D40:1198` in the MZ relocation table. With `GAME_DIR` set to the installed build and
the checkpoint `added` in `artifacts/runs/2026-10-14-cc7`, run
`tools/research/rust-dos-runs/run_plan.py artifacts/runs/<dir>
tools/research/rust-dos-runs/plans/fight-state-create.json` and read the `mem` lines.
