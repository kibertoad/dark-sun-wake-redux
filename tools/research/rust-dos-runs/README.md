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

The plans in `plans/` run in this order, each from the run directory the
previous one names:

| Plan | Run directory | From |
| --- | --- | --- |
| `start.json` | `artifacts/runs/2026-10-14-start5` | a new start; saves `start` at the start window |
| `add.json` | `artifacts/runs/2026-10-14-cc4` | `start`; ADD to box 2, saves `added` |
| `drop.json` | `artifacts/runs/2026-10-14-cc5` | `added`; DROP, saves `dropped` at the confirmation |
| `after-drop.json` | `artifacts/runs/2026-10-14-cc6` | `dropped`; YES, saves `afterdrop` |
| `start-game-after-drop.json` | `artifacts/runs/2026-10-14-cc7` | `afterdrop`; Esc, Esc, START GAME |
| `start-game-control.json` | `artifacts/runs/2026-10-14-ctl1` | `start`; START GAME |

`generator_callers.py <run-dir> <earlier-run-dir>:<state> <until_ms> [start]`
stops at each call of the generator `1000:0822` until the emulated time given,
after a click on START GAME with `start`, and counts the return addresses.
