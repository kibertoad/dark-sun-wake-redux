---
id: FND-PARTY-198
title: On Create Characters the cast button 0x2C29 does nothing, but the key c opens overlay 211's USE window through overlay 190 +1D8D, so View Character can pass events to overlay 211 +081B before START GAME
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
tool: dinorefurb-dosbox-session 0.10.0 RustDosSession, rust-dos v1.7.2 debug server (breakpoints, memory reads, screenshots, mouse and key input), tools/research/rust-dos-runs/run_plan.py with plans/cast-button-create.json, cast-button-play.json and cast-key-create.json; Python 3.14.7 with Capstone 5.0.7 (tools/research/exec-census/overlay_listing.py)
environment: Windows 11 Pro 10.0.26200; rust-dos v1.7.2 (executable SHA-256 310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4), deterministic at 15000 instructions a millisecond from 1995-04-11 12:34:56, host sound muted, Sound Blaster 16 at 220, IRQ 5, DMA 1, HDMA 5, no Ultrasound
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007BBCE..0x0007BC02
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079F94..0x0007A024
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007A0CD..0x0007A0DD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079023..0x0007904C
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 190's code starts at file `0x78340`.

**Static.** Overlay 190 `+388E` calls `28C9:2522`, which returns `DS:0DAB` (FND-PARTY-193), and
when the result is not 2 calls `3EBE:071F` with the window `DS:1431`, the buttons `0x2C2C`,
`0x2C29` (`BUTN` 11305, Cast spells or use psionics, SCR-UI-002) and `0x33F4`, and 1 each
(`+388E..+38C2`). The button case `+0CE3` (button `0x2C29` in the table at `cs:0D3D`) stores
`DS:1431` at `55C9:0000` and calls overlay 211 `+02B5` through trampoline `57C1:003E`
(`+0CE3..+0D0C`). The key path at `+1C54` takes the key code at `[bp+0x12]` in lower case, sets
the button of `c`, `e`, `i`, `o`, `u` or `v` to frame 4 through `3EBE:071F`, calls `28C9:2A81`
with 1 and the callback at `[bp+0x1E]` when it is set, and jumps through the table at `cs:1ECC`
by the key less `c` (`+1CC8..+1CDF`); `c` and `u` go to `+1D8D`, which also stores `DS:1431` at
`55C9:0000` (`+1D8D..+1D9D`). The far pointer at `55C9:0000` is what `+1649` compares with
`DS:1431` before passing an event to overlay 211 `+081B` (FND-PARTY-197).

**Runs.** A segment `S` of the load image is `S - 0A46` in the run: `55C9` is `4B83`, `3EBE` is
`3478`, the trampolines of overlay 211 are at `4D7B:003E` (`+02B5`) and `4D7B:005C` (`+081B`).

1. `cast-button-create.json` starts from the checkpoint `added` of `add.json` (Create
   Characters, one stored character in the third box) and clicks the third button of the row
   under the boxes at `(99, 171)` with breakpoints on `3478:071F`, `4D7B:003E` and `4D7B:005C`.
   No breakpoint stops; `DS:0DAB` reads 0 and `55C9:0000` reads 0 before and after.
2. `cast-button-play.json` starts from the checkpoint `talk` of `start-talk.json`, opens the USE
   screen and View Character in play, where `DS:0DAB` reads 2, and clicks the same position with
   the same breakpoints. `3478:071F` stops for button `0x283C` with 5 and `0x2C29` with 4, then
   `4D7B:003E` stops, and `55C9:0000` then holds the value of `DS:1431`.
3. `cast-key-create.json` starts from `added` and presses `c`. `3478:071F` stops for `0x283C`
   with 5 and `0x2C29` with 4, `4D7B:003E` stops twice, and 16 more `3478:071F` stops follow.
   `DS:0DAB` stays 0 and `55C9:0000` then holds the value of `DS:1431`. The screenshot at
   222,000 ms shows the USE window over the boxes, with the character's spell icons and
   `CLERIC` and `LEVEL 1` under them.

## Interpretation

Outside play the screen sets the cast button to frame 1 and a click on it does not reach the
button code, but the key `c` opens the USE window anyway, so on Create Characters View Character
hands its events to overlay 211's handler, the first step of the route FND-PARTY-197 found to the
damage routine.

## Alternatives

- That frame 1 is what keeps the click from reaching the button code is not read; the run shows
  only that nothing the breakpoints cover ran.
- The key `u` goes to the same case and was not run.
- Whether casting from this window can damage a party member before START GAME is not tested.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `GAME_DIR` set to the
installed build and the checkpoints `added` (`artifacts/runs/2026-10-14-cc7`) and `talk`
(`artifacts/runs/2026-10-14-talk2`): run `tools/research/rust-dos-runs/run_plan.py
artifacts/runs/<dir> tools/research/rust-dos-runs/plans/<plan>` for each of the three plans and
read the `mem` and `break` lines and the screenshots. List overlay 190 with
`tools/research/exec-census/overlay_listing.py <dsun> 190 7BB90 7BC10`, `79F80 7A040`,
`7A0C0 7A0E0` and `78F90 79050`.
