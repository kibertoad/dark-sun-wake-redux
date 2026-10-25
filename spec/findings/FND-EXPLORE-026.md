---
id: FND-EXPLORE-026
title: Outside a fight, over a Walk click and walk, two key steps and a whole save, and over the first 6.9 s of a load, no instruction reads 4E71:0000; it is only written with 1 at 28C9:0500, 28C9:0344 and 31E0:2D76
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/walk_drive.py with the configs counts/walk2-tw.json, walk2-tc.json, walk2-sl0.json, walk2-tsl.json, walk2-slprep.json and walk2-tload.json, at the commit that adds this finding; Python 3.14.7; Capstone 5.0.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; SB16 at 0x220, IRQ 5, DMA 1, no Ultrasound; default muted host audio; checkpoint explore of run exprep and checkpoint preload of run slprep (D:/rust-dos-games/work/walk2)
---

## Observation

Method as FND-EXPLORE-023: the driver records the instruction trace in slices and checks every
traced instruction's memory operands, string spans, `XLAT` and `INT 21h` `3Fh`/`40h` buffers
against the run's linear address of `4E71:0000`. Times are emulated ms after the restore. In every
window below the instruction counts are contiguous and no entry was dropped.

**`tw`**, from `explore` (FND-EXPLORE-024): a Walk click (216, 112) at 500 ms and the leader's
three-cell walk, traced 450 to 1,200 ms (9,719,481 instructions); Up at 2,500 ms, traced 2,400 to
2,800 ms (5,531,445); Right at 3,500 ms, traced 3,400 to 4,000 ms (7,795,213). The only accesses
are writes of 1 by `mov byte ptr es:[0], 1`: once at `28C9:0500` in the first window, once at
`28C9:0344` in each of the others, and 17, 14 and 17 times at `31E0:2D76`, one at each step pop of
every walker (slots 15 and 16 walk the street throughout). The byte reads 1 at 0, 1,200, 2,800
and 4,000 ms.

**`tc`**, the control: the same scan over the two bytes of `DS:426D`, 2,450 to 2,650 ms around Up
at 2,500 ms (2,520,084 instructions), finds 678 reads and one write, among them reads through a
direct offset (`28C9:1E69`, `28C9:10EF`) and through `ES:BX` (`31E0:2D81`).

**`sl0`**, untraced: F1 at 500 ms opens Save Game; row 0, the name `WALK` and SAVE at 4,200 ms
give GAME SAVED at 5,000 ms and the play view by 6,000 ms. F2 at 9,500 ms, row 0 and LOAD at
12,200 ms show a full-screen picture from about 19,000 to 64,000 ms, and the play view returns by
67,000 ms with the same screen as after the save. The byte reads 1 at every read from 0 to
79,000 ms. **`tsl`** repeats the save traced from 450 to 7,000 ms (93,466,799 instructions): 45
accesses, all writes of 1 at `31E0:2D76`, all after `DS:1440` returns to 1 at 5,189 ms. Its load
window failed on a lost connection to the emulator and recorded nothing.

**`tload2`** restores `preload` (Load Game open with `WALK` selected, the saved game in the run's
drive), clicks LOAD at 200 ms and traces 100 to 7,000 ms (57,934,337 instructions): no access.
`DS:1440` goes 0 to 1 at 5,875 ms, the load picture is up at 7,000 ms, and the byte reads 1 at 0
and 7,000 ms. The run was stopped during its second window; an earlier attempt ended at 3,782 ms
when the emulator failed to allocate memory.

## Interpretation

Outside a fight, as in the fight (FND-EXPLORE-023), `4E71:0000` is written with 1 at the three
sites FND-EXPLORE-018 lists and read by no instruction over a walk, key steps and a save; the
control shows the scan reports reads in the same state. Over a load only the first 6.9 s were
scanned, before the load picture's phase.

## Alternatives

- From about 7 s after the LOAD click to the play view's return, about 55 s, was not scanned.
- Whether a saved game holds the byte was not seen: it was 1 before the save and after the load,
  and a load from a game where it is 0 was not run.
- Stack pushes and pops, DMA and the emulator's own services other than `INT 21h` functions `3Fh`
  and `40h` were not scanned, and the scan does not count the `INT 21h` calls it saw.

## How to reproduce

From the repository root, with `GAME_DIR` the installed BLD-GOG-EN-1.1 and the checkpoints above:
`artifacts/evidence-python/Scripts/python.exe -I tools/research/rust-dos-runs/walk_drive.py
tools/research/rust-dos-runs/counts/walk2-tw.json`, and the same for each config named in `tool`
(`walk2-slprep.json` writes `preload`). Each run writes `log.json` under
`D:/rust-dos-games/work/walk2/<name>`. A full load window takes about 2.5 to 3 hours of wall time.
