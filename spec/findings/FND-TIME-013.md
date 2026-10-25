---
id: FND-TIME-013
title: The music driver install of 4734:0133 runs only while DS:3444 is not 0, and both the -L launch (through overlay 180) and bit 3 of SOUND.CFG word 0x14 store 0 there through 4734:0128, the word's one writer
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:0128..4734:0133
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:014F..4734:0162
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:0293..4734:03B3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067263..0x000672A9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 49E9:018F..49E9:022E
tool: tools/research/exec-census/resident_listing.py, overlay_listing.py, direct_callers.py and immediate_search.py at the commit that adds this finding; Capstone 5.0.7; Python 3.14.7
environment: null
---

## Observation

Addresses are static, load image at segment 1000, `DS` = `57E0`; overlay 180's `+x` is at file
`0x671E0 + x`. `DS:3444` holds 1 in the file.

**The writer.** `4734:0128(v)` stores `v` in `DS:3444`. `immediate_search.py <dsun> 3444` finds that
store (`4734:012E`) and five compares, at `4734:0293`, `02C7`, `03C6`, `0541` and `05D2`.
`direct_callers.py` finds three calls of `4734:0128`: overlay 180 `+0091` and `+00A4`, and the near
call at `4734:015E`.

**Overlay 180** (`+0083..+00A9`) tests `digital_sound_on` `DS:14E3`. When it is not 0 it calls
`2660:01C4` and then `4734:0128(0)`; otherwise `4734:0128(1)`. FND-SOUND-010 reads the `-L`
argument setting `DS:14E3` to 1.

**`4734:0133`**, called from `4734:00DE` in `4734:00B0` after the `SOUND.CFG` buffer's address is
stored at `DS:3411`:

- `4734:014F..0162` calls `4734:0128(0)` when bit 3 (`8`) of word `0x14` of the buffer is set;
- `4734:02C7..02D1` jumps to `4734:039B` when `DS:3444` is 0, past the music block
  `4734:02D1..039B`, which calls `4842:0A23`, `4842:0B4D`, `4842:0B73` and, at `4734:038D`,
  `4842:0B79`;
- `4734:039B..03A0` calls `4734:07FD`, which allocates the 10,000 µs slot of `4654:09A9`;
- `4734:03A6..03B3` calls `49E9:018F` when bit 0 of word `0x14` is set.

`49E9:018F` loads the digital driver and calls `4842:0B79` at `49E9:0229`. `direct_callers.py
<dsun> 4842:0B79` finds only the calls at `4734:038D` and `49E9:0229`.

The installed `SOUND.CFG` word `0x14` is 11 (`0x000B`), with bits 0, 1 and 3 set.

## Interpretation

With `-L`, or with bit 3 of word `0x14` set, `DS:3444` is 0 when `4734:0133` reaches the music
block, and nothing stores another value, so the FM music driver is never installed and
`4842:0B79` runs only for the digital driver. The installed files have both, so the music driver's
timer slot (FND-TIME-012) never exists.

## Alternatives

Whether the setup program means word `0x14` as flags, or wrote the driver chunk number 11 there
(FND-CONFIG-003), was not read. A store to `DS:3444` through a computed address is outside
`immediate_search.py`'s search; FND-TIME-014's runs see none. What `2660:01C4` does beyond
FND-SOUND-013's reading was not read.

## How to reproduce

From `tools/research/exec-census/` with `<dsun>` the installed `DSUN.EXE`: `resident_listing.py
<dsun> 4734:0000..4734:0560 49E9:018F..49E9:0250`; `overlay_listing.py <dsun> 180 671E0 67300`;
`immediate_search.py <dsun> 3444`; `direct_callers.py <dsun> 4734:0128 4842:0B79`. Read word `0x14`
of the installed `SOUND.CFG`.
