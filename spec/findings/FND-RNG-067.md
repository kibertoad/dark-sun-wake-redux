---
id: FND-RNG-067
title: From the two-character play checkpoint, a left click on the view sends slot 9999 to 2D40:2F34, which fills record 14 of DS:67BB with text from DS:0CE6 and through its link writes 14 over the July and August day counts at DS:3956
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/indirect_targets.py at the commit that adds this finding and a local probe that stops on a watchpoint and walks the BP chain; Python 3.14.7, Capstone 5.0.9
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; default muted host audio; checkpoint play-ready of run v175-play-ready-b-20261019 (FND-UI-054)
---

## Observation

Addresses are static (run segment plus `0x0A46`); times are from the restore.

**The watch.** `watch-clock-20261025` restored `play-ready` (FND-UI-054) and set watchpoints, which
stop after an instruction changes the bytes, over the windows of Q-RNG-005 (`DS:3920..3924`,
`DS:34F5..3675`, `DS:3950..3976`, `DS:3976..397A`, `DS:397A..3980`, `DS:3980..3983`,
`DS:A4E4..A4EC`) and, as a control, `rng_state` at `DS:384A`. It ran 40 s with the input of
`indirect-walk-20261025` (FND-RNG-065). The control changed 12 times, at `1000:083D` and
`1000:0840`. One change fell in a window: at 3056 ms, after the left click at (120, 118) at 3 s,
the instruction before `2D40:310B` changed `DS:3956..3958` from `1F 1F` to `0E 00`. The bytes at
`DS:3950..395C` are the twelve month lengths 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31
(FND-RNG-016), so July and August became 14 and 0.

**Record 14.** `rec14-click-20261025` repeated the pointer moves and the click and ran to 3.4 s.
The 37 bytes `DS:69C1..69E6`, record 14 of the 37-byte records at `DS:67BB`, were all `0xFF` after
the restore; at 3056 ms the instructions before `1000:3F6F` and `1000:3F72` wrote them with the 37
bytes found at `DS:0CE6` in the file, the start of a help message, and then `DS:3956` changed as
before. `rec14-span-20261025` compared `DS:6600..6B00` before and after: the bytes that changed
around the record array were `DS:683A..683F`, record 14 and scattered bytes after it in
`DS:69E7..6A2A`.

**The calls.** At the stop for the first change of `DS:69C1`, the stack held, from the innermost
frame, the copy routine `1000:3F5A` called from `2D40:2FB3` with destination `DS:69C1`, source
`DS:0CE6` and 37; `2D40:2F34` called from `2D40:10D9` with 9999; `2D40:10AE` called from
`28C9:04DA` with 9999 and 1; `28C9:0353` called from `28C9:06C5` with 9999, 120 and 118; then
`28C9:14BF`, `3D72:0594`, `3D72:0361`, `39D1:0BB0`, `277B:0546` and `1000:015D`.

**The checkpoint.** `read-426d-20261025` read, after the restore, `DS:426D` = 9999, the combat
word `4C10:0019` = 0 and the words `4C13:0361..0369` = 0, `0xF461`, `0xF461` and 15.

## Interpretation

At the checkpoint combat is off (`4C10:0019` is 0, FND-CONFIG-058) and `DS:426D` holds 9999, no
slot. A left click on the view passes that 9999 as a slot to `28C9:0353` and on to `2D40:10AE` and
`2D40:2F34`, which take it for a real slot. FND-RNG-068 reads the code: record 9999 lies, by a
16-bit product, at `DS:0CE6`, so the new record 14 gets text, and the text's link word sends a
store of 14 into the month-length table that the clock conversion sums.

## Alternatives

The watchpoints report changes, so a store of the value a byte already held is not seen. The run
covers one state and one input; whether other states reach `2D40:2F34` with 9999 is read in
FND-RNG-068.

## How to reproduce

With `GAME_DIR` the installed BLD-GOG-EN-1.1 directory, run with the evidence interpreter
`indirect_targets.py v175-play-ready-b-20261019 play-ready <run> <ms>` with: for
`watch-clock-20261025`, 40000, the clicks of FND-RNG-065's walk and `--watch` for `57E0:3920:4`,
`57E0:34F5:180`, `57E0:3950:26`, `57E0:3976:4`, `57E0:397A:6`, `57E0:3980:3`, `57E0:A4E4:8` and
`57E0:384A:4`; for `rec14-click-20261025`, 3400, `--read 57E0:69C1:25`, `--watch 57E0:69C1:25`,
`--watch 57E0:3950:C` and clicks holding the moves to (160, 199) at 500 ms and (160, 100) at
2500 ms and a left click at (120, 118) at 3000 ms; for `rec14-span-20261025`, 3400, the same
clicks and `--read 57E0:6600:500`; for `read-426d-20261025`, 200, `--read 57E0:426D:2` and
`--read 4C10:0019:2` and `--read 4C13:0361:8`. The stack was read at the watchpoint stop on `DS:69C1` from `SS:SP` and by
following saved `BP` values, each frame's return offset and segment at `BP+2` and `BP+4`.
