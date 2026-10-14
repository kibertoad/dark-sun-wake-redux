# rust-dos runs

Agent runs of the installed `DSUN.EXE` through `dinorefurb-dosbox-session`
(`RustDosSession`, the pinned rust-dos, host sound muted), as `docs/RUNTIME.md`
describes. Run them from the repository root with the evidence Python and
`GAME_DIR` set to the installed BLD-GOG-EN-1.1:

```sh
artifacts/evidence-python/Scripts/python.exe -I tools/research/rust-dos-runs/run_plan.py <run-dir> <plan.json>
```

Every run is deterministic at 15000 instructions a millisecond (`cycles fixed
15000` in GOG's configuration), with C: a copy of the game's files and D: the
CD image `game.ins`. The copy of `SOUND.CFG` names IRQ 7 in place of IRQ 5,
since the session's rust-dos takes no configuration and has its Sound Blaster
at IRQ 7.

`run_plan.py` follows a plan: each step queues its keys, typed text or mouse
event, runs to the emulated time `until_ms`, checks that the input was
delivered, then reads memory, takes a screenshot or saves a checkpoint. A plan
with `start` starts from a checkpoint of an earlier run, named
`<run-dir>:<state>`. Mouse positions are given in the mouse driver's
coordinates (`"coords": "virtual"`), which are the game's 320 x 200 pixels.
Memory addresses are those of the run: the program is loaded with its first
segment at `05BA`, so a segment `S` of the repository's load image (based at
`1000`) is `S - 0A46` here.

A plan with `watch_draws` stops at each call of the generator `rng`
(`1000:0822`) and records the draw as `{rule, bound, result}`: script
instruction `0x52` under RULE-SCRIPT-006, and `random_mod` under the rule the
plan's `rules` gives for the routine that called it (or that called
`chance_in_ten`), found in `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`. It checks
each result against the value the original computed, and stops the run at a
draw it cannot name. A plan with `rng_state` writes that seed through a guarded
write before the first step.

A plan with `watch`, a list of `[address, length]`, sets a watchpoint on each
range. At each write it prints the static address of the writing instruction
and the BP chain: each frame's far return, with the 24 bytes before it so that
overlay code can be found in the file. A run that saves no checkpoint deletes
its copy of C: when it ends.

`start.json` (run directory `artifacts/runs/2026-10-14-start5`) makes the
start-window checkpoint `start` from a new start. EXP-PARTY-001's runs start
from it:

| Plan | Run |
| --- | --- |
| `exp-add-drop-start.json` | ADD, DROP, START GAME |
| `exp-start-control.json` | START GAME |
| `exp-add-start-long.json` | ADD, START GAME |
| `exp-add-start-long-seed.json` | the same with `rng_state` 12345 |

EXP-PARTY-002's runs start from it too:

| Plan | Run |
| --- | --- |
| `exp-add-two-start.json` | ADD to box 1, ADD to box 3, START GAME |
| `exp-add-two-start-seed.json` | the same with `rng_state` 12345 |

FND-PARTY-174's run is `leader-writes-two.json`: `exp-add-two-start.json` with
a watchpoint on `leader`.

The other plans (`exp-add-start.json`, `add.json`, `drop.json`, `after-drop.json`,
`start-game-after-drop.json`, `start-game-control.json`) are the exploratory
runs that found the screens' positions, each from the checkpoint the one before
it saved.

`generator_callers.py <run-dir> <earlier-run-dir>:<state> <until_ms> [start]`
counts the return addresses of the generator's calls until the emulated time
given, after a click on START GAME with `start`.
