---
id: FND-EXPLORE-019
title: A walker moves by its animation frames, at most one frame per map pass and per 1/96 s tick, each frame adding its signed pixel steps to the position; reaching a cell centre clears the between-cells bit and the same pass pops the next step
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:19A1..28C9:1EDB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2322..28C9:24EA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0271..2C5F:0334
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:1808..31E0:2670
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:271E..31E0:2C86
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:31A7..31E0:31C8
tool: Capstone 5.0.9 over resident code; tools/research/exec-census/resident_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Addresses are static, load image at segment 1000, `DS` = `57E0`. A slot record is the 37-byte
record at `DS:67BB + 37 × slot`, 320 of them. A frame is 8 bytes: `+0` loop target, `0xFF` for an
empty frame; `+1` delay; `+2` flags (1 jump to `+0` after this frame, 2 hold, `0x10` mark between
cells without moving, `0x20` lift the footprint); `+3`, `+4` signed pixel steps. A record's
animation set (`+0x16`) is an index into the 64 nine-byte cache records at `DS:6577`, whose word
`+7` is the frame count, or a negated `SCMD` number not yet loaded; `+0x18` is the current frame,
`+0x15` the delay, and the double word `+0x1D` the value of `DS:145E` when the last frame was
applied.

**The tick.** `31E0:31A7` stores 1 in `DS:265A` and adds 1 to the double word `DS:145E`
(`31E0:31AA..31E0:31AF`). FND-TIME-007 and FND-RNG-081 place it as the 96 Hz timer callback.

**The pass.** The map pass `28C9:2322` calls `28C9:1EDB`, which runs `28C9:19A1` for every slot
(FND-RNG-074), and when the byte that returns is 0 calls `2C5F:0271` with 1 when `DS:1440` is 1
and with 0 otherwise (`28C9:24B1..28C9:24CE`). `2C5F:0271(flag)` calls `31E0:2BB9(DS:1408,
DS:140A, flag, &changed, 0)` (`2C5F:02B4..2C5F:02C6`) and, when that returns a count above 0,
`31E0:2C86(count)`.

**`31E0:2BB9`** returns 0 unless the bytes `DS:265B`, `DS:265A` and `DS:265C` are all set. For each
slot record whose word `+0x01` is 0 or above it calls `31E0:2837`, skipping a record with bit 5
of byte `+0` set when `flag` is 0 (its last argument is 0 here). It clears `DS:265A` at the end
(`31E0:2C7B`).

**`31E0:2837`**, for one record: returns 0 when the dword `+0x1D` plus the delay is above
`DS:145E` (unsigned), the delay being byte `+0x15`, times 10 when above 99 (`31E0:2888..28BE`). It
returns 0 unless bit 0 of byte `+0` is set or the placed entry has bit `0x40`. It calls
`31E0:1808`; when the frame index has reached the set's frame count it clears bit 0 and uses frame
0. With the byte `DS:1437` at 0, the slot's type byte `4F49:0C33 + 3 × slot` at 2 and a frame that
moves, it calls `31E0:2158`; otherwise `31E0:1B60(record, frame)`. Unless the frame has flag 2, it
adds 1 to `+0x18`, and when the frame now there has flag 1 sets `+0x18` to that frame's `+0`.

**`31E0:1B60`** adds the frame's sign-extended byte `+3` to word `+0x0A` and byte `+4` to word
`+0x0C` (`31E0:1D99..31E0:1DA9`), and the same to each record down the `+0x21` chain. It sets bit
5 when the frame moves or has flag `0x10`. It sets `+0x15` to the frame's `+1`, or to 0 when the
frame moves and `4C10:0019` is not 0, and stores `DS:145E` in `+0x1D`. A hold frame snaps a
walker off a cell centre to `(word & 0xFFF0) + 8` on both axes and clears bit 0. When `+0x0A + 8`
and `+0x0C + 8` are both multiples of 16 it clears bit 5 (`31E0:2091..31E0:209C`), and when the
frame moved sets bit `slot` of the bitmap at `52A2:0000`.

**`31E0:2C86`** calls `31E0:2D6E` for each slot whose bit is set there; `31E0:2D6E` calls
`2D40:10AE(slot, 0)` (FND-RNG-072), which pops the next direction (FND-EXPLORE-014).

**`28C9:19A1`** (adding to FND-EXPLORE-014) calls `31E0:1808` and keeps the running cycle going
when bit 0 is set, the type byte is 2, the combatant index is not 9999, the combatant's facing
`+0x1F` equals the step's direction `d` and the current frame moves: it then starts from the
current frame. Otherwise it starts from the frame that word `b` of the set's 36-word table at
`DS:6573 + 0x48 × set` names, with `b` byte `d` of `DS:1413`, which holds 1, 7, 9, 15, 17, 23,
25 and 31. It calls `31E0:271E(slot, frame)`, which stores the frame in `+0x18`, sets bit 0 and
stores `DS:145E` in `+0x1D`.

**`31E0:2158(slot, dir, n)`**, with animations off, switches to the animation that word `b` of
the table names, with `b` byte `dir` of `DS:141C`, which holds 0, 8, 8, 8, 16, 24, 24 and 24. It
sets the position to `((pos >> 4) + step[dir]) × 16 + 8` on both axes, with the signed steps at
`DS:29F7` and `DS:2A00`, clears bits 5 and 0, sets the slot's bit at `52A2:0000`, and for a slot
above 4 whose walk mode is not 11 outside a fight sets `+0x15` to 16 and `+0x1D` to `DS:145E`.

## Interpretation

A walker moves one frame of its walk animation per pass, and no faster than one frame per tick of
`DS:145E`, since `31E0:2BB9` runs only after a tick has set `DS:265A`; ticks missed by a slow pass
are not made up. A cell step therefore takes as many passes as the frames whose steps add up to
16 pixels, and at least that many ticks of 1/96 s. Arrival at the centre and the pop of the next
step happen in the same pass, and the next pass starts the next step. While `DS:1440` is not 1
outside a fight, `2C5F:0271` passes 0, so walkers between cells stand still. With animations off
a kind-2 walker jumps a whole cell per pass, and a slot above 4 outside a fight waits 16 ticks
after each cell.

## Alternatives

That `31E0:31A7` runs at 96 Hz rests on FND-TIME-007 and FND-RNG-081. How fast the map pass runs,
and so whether a step is bound by ticks or passes, was not measured. What `28C9:1EDB` returns, the
other far callers of `31E0:2BB9` and `31E0:2C86` (file offsets `0x2C080`, `0x2BEFE`, `0x2C17F`),
and `31E0:3388`, `31E0:31F7`, `31E0:4408`, `31E0:4323` and `2C5F:0A22` were not read.

## How to reproduce

`resident_listing.py <dsun>` over `28C9:19A1..28C9:1EDB`, `28C9:2322..28C9:24EA`,
`2C5F:0271..2C5F:0334`, `31E0:1808..31E0:2670`, `31E0:271E..31E0:2C86` and
`31E0:31A7..31E0:31C8`; `direct_callers.py <dsun> 31E0:2BB9 31E0:2C86 31E0:2837 31E0:2158
31E0:1B60`. The data bytes `DS:1413`, `DS:141C`, `DS:29F7` and `DS:2A00` are at file offsets
`0x4D000` plus the offset.
