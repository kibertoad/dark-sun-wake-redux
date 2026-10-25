---
id: FND-COMBAT-056
title: Three seconds after the view click that fills record 9999, the memory outside the stack differs from a run without the click in the copied record, the month lengths, 21 slot-record owners, three pairs of record bytes and words of 4C13 near 4C13:0369
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

Both runs restore `play-ready`, move the pointer to (160, 199) at 500 ms and to (160, 100) at
2500 ms, run to 6000 ms and read, at the start and the end, `DS:0000..FFFF`, `4F49:0000..FFFF`,
`4C13:0000..1000`, `4C4F:0000..1000`, `4C10:0000..0400`, `4E71:0000..2000` and `52A2:0000..0800`.
`diff-click-20261025` also clicks the left button at (120, 118) at 3000 ms; `diff-noclick-20261025`
does not.

Counting only bytes that read the same at the start and at the end of the run without the click
and differ at the end of the run with it, and leaving out `DS:F78A` and above (the stack) and the
overlapping parts of the reads (`4F49` from `0x8970` on is the data segment; `4C10` overlaps
`4C13`; `4E71` overlaps `4F49`), the differences are:

- `DS:0CE6..0D0B` from the help text to `0xFF`, and record 14 at `DS:69C1..69E6` from `0xFF` to
  that text, with further bytes of record 14 to `DS:6A2A` changed;
- `DS:3956..3958` from `1F 1F` to `0E 00`, and `DS:819A..819E` from `FF FF FF` to `FE 0E 00`;
- word `+0` of the 19-byte slot records 21, 23, 26, 27, 30 and 32 to 47 at `4F49:08A3`, from 1 to 0
  (the bytes `4F49:0A32`, `0A58`, `0A91`, `0AA4`, `0ADD` and every `0x13` from `0B03` to `0C20`);
- the type entry of record 14 at `4F49:0C5D..0C60` from `00 0D 00` to `2E 00 CD`;
- byte `+1` of records 30, 31 and 32 (`DS:6C12`, `6C37`, `6C5C`) and 275, 276 and 277 (`DS:8F7B`,
  `8FA0`, `8FC5`) each lower by 1;
- `4C13:00B8`, `4C13:0101` and `4C13:0367` from `0x0F` to `0x10`; `4C13:035B` (FND-RNG-077);
  `4C13:036B..036F` from `94 0F 00 00` to `A1 0F BD 0F`; `4C13:0373..0375` from `01 00` to
  `FF FF`; `4C13:0377..037A` from `6B 00 FF` to `5E 00 FE`; `4C13:0381..0383` from `00 00` to
  `C2 0F`;
- `4F49:0000` from `0x10` to 0, `4F49:04FC` from 0 to `0xFF`, `4F49:0523..053C` from 0 to
  `04` fourteen times then `08 FF 09 04 3F 00 4C 00 48 00 3C`, `4F49:09D3..09D8` from `01 00 00 00 00` to
  `3F 00 4C 00 0F`, `4F49:1790..1792` from 0 to `08 03`, and 261 more bytes from `4F49:35C8` to
  `4F49:8712`;
- in the data segment, `DS:08BC` (3 to 2), `DS:1424` and `DS:265A` (1 to 0), `DS:19B8..19BA`,
  `DS:40FE..4100`, `DS:417A..4185`, `DS:562B..5630`, `DS:56ED` (`0x0E` to `0x0D`),
  `DS:6550..6553` and `DS:A12F..A13B`.

`4C4F:0000..1000` and `52A2:0000..0800` show no such difference.

## Interpretation

The differences mix the one-time work of `2D40:2F34` on the click (the copy to record 14, its type
entry, the free-list head `DS:56ED` and the link stores of FND-RNG-068) with the work of the
repeated passes (FND-RNG-076, FND-RNG-077). The 21 slot records whose owner word went from 1 to 0
and the records whose byte `+1` went down by 1 are candidates for state the repeated round step
or end of combat changes.

## Alternatives

One pair of runs; the no-click run also moves state that depends on time and the pointer, and
bytes it changed are left out even where the click run changed them differently. Which code made
each change was not observed.

## How to reproduce

With `GAME_DIR` the installed BLD-GOG-EN-1.1 directory, run with the evidence interpreter
`indirect_targets.py v175-play-ready-b-20261019 play-ready <run> 6000` with `--read` for
`57E0:0000:FFFF`, `4F49:0000:FFFF`, `4C13:0000:1000`, `4C4F:0000:1000`, `4C10:0000:400`,
`4E71:0000:2000` and `52A2:0000:800`: for `diff-noclick-20261025` with an inputs file holding the
two moves, and for `diff-click-20261025` with the moves and the left click. Compare the end reads
byte by byte, keeping bytes whose start and end values agree in the run without the click and
whose end value differs in the run with it.
