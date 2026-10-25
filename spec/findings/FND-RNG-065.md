---
id: FND-RNG-065
title: In rust-dos runs from the two-character play checkpoint, the timer interrupt's callback call takes 4654:09A9, 31E0:31A7 and 4842:0519, its chain call takes the BIOS, the segment 2707 table jumps take entry 4, and five other unresolved transfers do not run
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/indirect_targets.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; default muted host audio; checkpoint play-ready of run v175-play-ready-b-20261019 (FND-UI-054)
---

## Observation

`indirect_targets.py` restored the checkpoint `play-ready` (FND-UI-054), with run segments equal
to static ones less `0x0A46`, and kept a breakpoint on each of the nine transfers FND-RNG-058
leaves unresolved. At each stop it formed the memory operand's address from the stop's registers
and read the word or double word the transfer takes, removing a site's breakpoint after 200 hits.

`indirect-idle-20261025` ran 10 s with no input and all nine sites:

- `4842:0498` (`call far cs:[si+6]`) stopped 200 times: 94 times to `4654:09A9` through the slot
  at `4842:0006`, 89 to `31E0:31A7` through `4842:000A` and 17 to `4842:0519` through
  `4842:0046`.
- `4842:051A` (`call far cs:[011A]`) stopped 182 times, each to `F000:1100`.
- The other seven did not stop.

`indirect-walk-20261025` ran 40 s on the seven silent sites with a pointer move to (160, 199) at
0.5 s and (160, 100) at 2.5 s, left clicks at (120, 118), (120, 75), (40, 160) and (125, 180) at
3, 9, 15 and 21 s, a right click at (160, 100) at 27 s and a left click at (24, 18) at 33 s.
`2707:0188` stopped 16 times, each to `2707:0218` (entry 4 of its table), and `2707:058C` 16
times, each to `2707:062C` (entry 4 of its table). `1000:1D8D`, `1000:2A24`, `1000:298A`,
`44B6:00B8` and `55E3:003F` did not stop.

`indirect-read-20261025` read the pointers behind the five sites that did not stop, after the
restore and again 1 s later, with the same values both times: `DS:A4D0` 0000:0000, `DS:A4DE`
F000:FF53, `DS:3916` `0x0260`, `44B6:000C` F000:1004, `55E3:002F` 0000:0000, and `4842:011A`
F000:1100.

## Interpretation

In this state the timer interrupt runs three callbacks, two in the game's code and one the
handler's own chain entry `4842:0519`, and chains to the BIOS timer handler; the two table jumps
of segment `2707` run their fifth case. The INT 19h hook of `55E3` is not installed (its saved
vector is null), and the critical-error handler's user call through `DS:A4D0` has nothing to call.
FND-RNG-066 reads what each transfer can take in every state.

## Alternatives

A run reaches only the states it visits: these runs did not leave play, so a target taken only in
another mode, at startup or at exit is not seen. The breakpoint cap stops the count at 200 at the
two timer sites, so a fourth callback first called after the 200th stop would be missed.

## How to reproduce

With `GAME_DIR` the installed BLD-GOG-EN-1.1 directory, run
`indirect_targets.py v175-play-ready-b-20261019 play-ready <run> <ms> <site> ...` with the
evidence interpreter: `indirect-idle-20261025` with 10000 and the nine sites `1000:1D8D`,
`1000:2A24`, `1000:298A`, `2707:0188`, `2707:058C`, `44B6:00B8`, `4842:051A`, `4842:0498` and
`55E3:003F`; `indirect-walk-20261025` with 40000, the seven others and `--clicks` holding the
inputs above; `indirect-read-20261025` with 1000, no site and `--read` for `57E0:A4D0:4`,
`57E0:A4DE:4`, `57E0:3916:2`, `44B6:000C:4`, `55E3:002F:4` and `4842:011A:4`. Each run writes
`targets.json` under `GAME_DIR/captures/rust-dos-indirect/<run>`.
