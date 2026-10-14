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
CD image `game.ins`. rust-dos has the card the installed `SOUND.CFG` names (a
Sound Blaster 16 at 0x220, IRQ 5, DMA 1) and no Ultrasound. Runs made before
dinorefurb-dosbox-session 0.10.0 had rust-dos's default card at IRQ 7 and a copy
of `SOUND.CFG` rewritten to IRQ 7, and their checkpoints are not used.

Fresh C: preparation uses `probe_drive.py` and the supported installed-build
fingerprint manifest. It validates all selected files before copying verified
bytes into an empty destination. It excludes `game.ins`, `game.gog` and
storefront `MUSIC/` from C:; D: remains the mounted CD image. Unlisted captures,
analysis and user saves are never traversed or copied. Changed manifest files
produce a named error; the helper does not rewrite SOUND.CFG. Synthetic checks:
`artifacts/evidence-python/Scripts/python.exe -I tools/research/rust-dos-runs/check_probe_drive.py`.
This preparation change does not make the historical time-driven launcher a
state-driven gameplay probe.

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
range. At each write that changes a watched value it prints the old and new
bytes, the static address of the writing instruction
and the BP chain: each frame's far return, with the 24 bytes before it so that
overlay code can be found in the file. A run that saves no checkpoint deletes
its copy of C: when it ends.

A step with `breaks`, a list of run addresses, sets those breakpoints while the
step runs and removes them after it. At each stop it prints, as at a watchpoint
stop, the stack, the 12 bytes at `BP + 6` (the arguments of the routine whose
frame BP holds) and the BP chain.

`start.json` (run directory `artifacts/runs/2026-10-14-start6`) makes the
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

FND-PARTY-178's runs: `two-play.json` repeats `exp-add-two-start.json` and
saves the checkpoint `play` 90,000 ms after START GAME (run directory
`artifacts/runs/2026-10-14-twoplay2`); `leader-keys.json`
presses 4, 1, 2, 3 and 4 from it with a watchpoint on `leader`.

FND-PARTY-198's runs: `cast-button-create.json` clicks the cast button at
`(99, 171)` on Create Characters from `added`, `cast-button-play.json` clicks it
in play from `talk` after opening View Character, and `cast-key-create.json`
presses `c` on Create Characters from `added`; each breaks on `3EBE:071F` and
overlay 211's trampolines.

FND-PARTY-199's run is `fight-state-create.json`: reads of the fight state on
Create Characters before and after `c`.

FND-PARTY-195's run is `leader-page-flip.json`: key 4 from the same checkpoint,
with breakpoints inside `4328:0B4C` past its gates and reads of the display
page words.

FND-PARTY-184's run is `marks-two.json`: `exp-add-two-start.json` with
watchpoints on the four combatant records' `combat_mark` bytes.

`start-talk.json` repeats `exp-start-control.json`, picks the first answer of
the opening conversation and saves the checkpoint `talk` (run directory
`artifacts/runs/2026-10-14-talk2`).

`generation-new.json` (from `start`) and `generation-new-in-play.json` (from
`talk`) open the generation screen with NEW, from Create Characters and from
View Character in play. In play, a box is emptied first: right-click a filled
box, DROP at `(165, 129)`, then YES at `(166, 117)`; the empty box's menu has
NEW at its first line, `(115, 117)` for the third box and `(166, 117)` for the
fourth. `generation-edit.json` and `dual.json` make a character with DONE at
`(270, 183)`, right-click its box, and click EDIT at `(115, 117)` or DUAL at
`(115, 141)`.

The other plans (`exp-add-start.json`, `add.json`, `drop.json`, `after-drop.json`,
`start-game-after-drop.json`, `start-game-control.json`) are the exploratory
runs that found the screens' positions, each from the checkpoint the one before
it saved (`add.json` in `2026-10-14-cc7`, `drop.json` in `2026-10-14-cc8`,
`after-drop.json` in `2026-10-14-cc9`).

`generator_callers.py <run-dir> <earlier-run-dir>:<state> <until_ms> [start]`
counts the return addresses of the generator's calls until the emulated time
given, after a click on START GAME with `start`.

## State-stop startup probe

`startup_segments.py <new-run-name>` is a fixed probe of program entry to the
documented game-main entry (FND-PARTY-031). It verifies installed DSUN.EXE,
copies only that executable to its private C: drive, uses the pinned muted
RustDosSession, and waits for the main address rather than an emulated time.
It records entry/main registers and RNG state under the licensed installation's
`captures/rust-dos-startup-segments/<new-run-name>/`. An RNG-entry breakpoint
refuses further execution before a draw; it sends no input or memory write.
It establishes no later segment-preservation or party-cleanup result.

Use the locked evidence Python from the repository root:

```sh
artifacts/evidence-python/Scripts/python.exe -I tools/research/rust-dos-runs/startup_segments.py <new-run-name>
```

Live startup execution is verified by FND-PARTY-219: after the earlier
2026-10-15 lock refusal, the normal lock was available and a fresh run stopped
at main with DS equal to SS and no RNG draw. The probe does not prepare a
complete gameplay drive or prove later segment preservation. Every run still
uses normal machine coordination; never substitute a lock path or modify an
unrecognized lock to make a probe run.

`startup_window.py <new-run-name>` uses the verified private-drive helper
and waits for main's start-window opener call, before the call executes.
It has the same known-RNG guard and no inputs or writes. FND-UI-039 records
the successful call-site stop, argument read and DS/SS comparison; it does
not establish a drawn window. Reports stay under the licensed installation's
`captures/rust-dos-startup-window/`. Wall-clock observation exhaustion pauses
the owned session diagnostically; it never counts as arrival. Synthetic
continuation/pause checks are in check_startup_window.py.

The extended probe first stops at seed_random and admits only the startup
overlay-180 caller from FND-RNG-009, verified against its loaded trampoline
and far-call target. It reads the argument and checks the setter result before
the opener continuation. A second seed call or known RNG draw stops the run.
At the verified opener it saves the strict, file-checked state `opener`.
FND-RNG-010 records the actual seed call and checked save; a fresh native
restore of this checkpoint still needs its own attempt. The historical first
extended report mislabeled its checkpoint boolean as a path; current reports
use `checked` and `directory`, backed by the package journal's path and hashes.
