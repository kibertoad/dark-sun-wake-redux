---
id: FND-RNG-084
title: From play-ready under rust-dos no music driver is loaded, the timer slots hold only 4654:09A9, 31E0:31A7 and the old timer vector, the far heap starts at DS plus 0x1000 paragraphs, and the driver dispatch is not entered in 3 s
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/indirect_targets.py at the commit that adds this finding; Python 3.14.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; default muted host audio; checkpoint play-ready of run v175-play-ready-b-20261019
---

## Observation

Each run restores `play-ready`. Addresses are given as static ones; the run's `DS` is `4D9A`
(static `57E0`), and values read are as the run holds them.

**`adv97-20261025`** moves the pointer to (160, 199) at 500 ms and to (160, 100) at 2500 ms,
clicks the left button at (120, 118) at 3000 ms and runs to 6000 ms, counting entries of
`4734:0852`, `4842:0CBE`, `4842:0D30` and `4842:0D5A`: none was entered.

**`adv97b-20261025`** runs 3000 ms without input, counting `45CD:0390`, `4842:0498`, `4654:09A9`
and `4842:03F4`. `4842:0498`, the timer's far call to a slot's callback, ran 50 times (the cap) by
234 ms, and `4654:09A9` 50 times by 494 ms, each from `4842:049D`'s return address. `45CD:0390` and
`4842:03F4` were not entered.

**`slots4842-20261025`** has the inputs of the first run and reads the timer slots at the start
and at 6000 ms, with the same values both times: the callbacks at `4842:0006 + 4n` hold `3C0E:09A9`
(slot 0, static `4654:09A9`), `279A:31A7` (slot 1, `31E0:31A7`) and `4D0F:003E` (slot 2) and,
for slot 16, `3DFC:0519` (`4842:0519`, which calls the vector saved at `4842:011A`); the state words
at `4842:006C + 2n` are 2 for slots 0, 1 and 16 and 0 for the others.

**`drv4842-20261025`** reads at the start `4842:0166..0186` (16 words of `FFFF`), `4842:0186..01A6`
(1 then 15 zeros), `4842:011A` (`F000:1100`) and `4842:01B0` (`FFFF`).

**`blocks97-20261025`** reads `DS:343C..344E` as 0, `FFFF`, `FFFF`, `FFFF`, then zeros (so
`DS:343E` is `FFFF` and the far pointers at `DS:3446` and `DS:344A` are null), `DS:3411` as the far
pointer `68D0:0004`, and `4E71:0C10` as 0.

**`heap-20261025`** reads `DS:009E..00AA` as the far pointers `5D9A:0000`, `92C9:0000` and
`9FFF:0000`.

## Interpretation

In these runs no music driver is loaded (`DS:343E` holds `FFFF`), so no sequence block is
allocated and the timer never calls a driver's function `0067`: the only slot callbacks that run
are `4654:09A9`, `31E0:31A7` and the chain to the old timer vector, and slot 2's state is 0. Of the
driver slots of segment `4842` only the first was set up, and no timer slot was taken for it. The
far heap's base is `DS` plus `0x1000` paragraphs, the first paragraph past the data segment, as
FND-RNG-083's startup reading gives when `DS:37EC` is 0 and memory is ample; the `SOUND.CFG`
buffer at `68D0:0004` lies above it.

## Alternatives

The runs show what rust-dos's configuration does, where the music driver is not loaded; with a
music driver loaded the timer would call its function `0067`, which these runs do not exercise
(FND-RNG-082 reads it). Why the driver word stays `FFFF` was not read.

## How to reproduce

With `GAME_DIR` the installed BLD-GOG-EN-1.1 directory, run with the evidence interpreter
`indirect_targets.py v175-play-ready-b-20261019 play-ready <run> <ms>`: `adv97-20261025 6000` with an
inputs file holding the two moves and the left click and `--count` for `4734:0852`, `4842:0CBE`,
`4842:0D30` and `4842:0D5A`; `adv97b-20261025 3000` with `--count` for `45CD:0390`, `4842:0498`,
`4654:09A9` and `4842:03F4`; `slots4842-20261025 6000` with the same inputs and
`--read 4842:0006:44` and `--read 4842:006C:22`; `drv4842-20261025 500` with `--read 4842:0166:20`,
`--read 4842:0186:20`, `--read 4842:011A:4` and `--read 4842:01B0:2`; `blocks97-20261025 300`
with `--read 57E0:343C:12`, `--read 57E0:3411:6` and `--read 4E71:0C10:4`; `heap-20261025 300`
with `--read 57E0:009E:C`.
