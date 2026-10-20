---
id: FND-RNG-047
title: The paths of overlay 179 +1373's rolled cases around FND-RNG-044's sites, with the steps before and after each roll, the flags each sets and the messages of the raising and reincarnation
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065653..0x000656F1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065703..0x0006581D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000659D4..0x00065A22
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065AFB..0x00065B13
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065B7F..0x00065BCC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065EB2..0x00065ECE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065FC5..0x00066115
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066179..0x0006618F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006623B..0x00066272
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006634B..0x0006660A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000667AD..0x000669DB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066A2C..0x00066A36
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066CE5..0x00066D26
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066EFA..0x00066F95
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00066B61..0x00066B89
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D9A4..0x0004D9E5
tool: tools/research/exec-census/overlay_listing.py at commit f644c2c1; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 179's code starts at file `0x642E0`.
Names are FND-RNG-044's: `t` (in `DI`), `c` at `bp+8`, `n` at `bp+0xE`, `s` at `bp+0x10`, the
flags pointer at `bp+0x12`, `a` at `bp+0x14`, `g` at `bp+0x16` and `u` at `bp+0x18`; `R` and `D`
are `t`'s combatant and details records. `+1373` also keeps `c`'s combatant record as a far
pointer at `bp-0xE`, which it sets to 0:0 first and fills only when `c` is below 9999, and a
local byte `L` at `bp-0x13`, 0 on entry (`+137E..+13D3`). "Effect" is overlay 193 `+166F`, called
with `t`, `c`, a `DATA` number, a code, a dword duration and a last word; "remove" is overlay 193
`+194F` with a slot and a code.

Shared steps: `+1423` sets bit 0x01 of the flags byte and `+274C` bit 0x02, and both go on to
`+2752`, the path after the switch that FND-PARTY-166 reads; `+1531` calls overlay 197 `+2D05`
with `t` and the word pushed before it, then goes to `+274C`; `+1506` calls `28C9:2C9E` with `c`
and the far pointer `DS:67BB + 37 * t`, then overlay 173 `+08EE` (trampoline `5671:00F2`) with
`c`, then stores 0 in the byte `+0x0E` of `c`'s record through `bp-0xE`, and goes to `+2752`;
`+2321` sets bits 0x03 of the flags byte. `+2A05` takes a slot, a `DATA` number, a code and an
amount, calls overlay 193 `+1AA4` with the slot and 5, overlay 206 `+0B81` with the amount, the
slot and the code when the word at `DS:0DAB` is 1 and overlay 206 `+1121` with the slot otherwise,
and overlay 173 `+22C4` with the slot and the amount.

The ten words at `cs:2881` (file `0x66B61`) that `DATA` 26 and 150 skip are followed by ten jump
targets, every one `+1E26`, the loop's step.

| `DATA` | Path |
|---|---|
| 227 | When byte `+0x12` of `D` is 9: `+2A05` with `t`, `n`, 0x86 and the 8d6 total, then bit 0x01. Then, when `s` is not 0: effect code 0x0A with duration `u / 2` (signed, toward zero) and last word 0, then bit 0x02. Then `+1506` (`+142C..+14AE`). |
| 295 | `+2A05` with `t`, `n`, 0x20 and the total, then `+2752` (`+14BE..+14E3`, `+1EA9`). |
| 298 | When `s` is not 0, `+2A05` with `t`, `n`, 8 and the 3d6 total. Then `+1506` (`+14E3..+1506`). |
| 89 | Effect code 0x45 with the signed duration byte and last word 0, through `+1BDA`, then `+2752` (`+16F4..+1742`). |
| 312 | When `t` is `c`: effect `DATA` 312, code 0x5C, duration -9999, last word 0; bit 0x02; `L` bit 0x02. Otherwise overlay 177 `+0CB3` with `t`, `c` and the 1d6 total, bit 0x02 and `L` bit 0x01 (`+189F..+18EC`, `+181B..+1833`, `+1F8B`). |
| 60, 165 | When `11 - a +` byte `+0x1E` of `D` (unsigned bytes, word sum) is above the d20, `+2752`. Otherwise, when byte `+0x12` of `D` is 14, overlay 173 `+22C4` with `t` and 10,000 and then bit 0x01; when it is not, `+2752` (`+1CE5..+1D2D`). |
| 26, 150 | First, when byte `+0x12` of `D` is 11, byte `+0x0E` of `R` is 10 and `c` is below 9999: effect code 0x41 with duration 240 and last word 0. Then for each code `k` from 0 to 112 whose bit `k & 7` of byte `k >> 3` of the far pointer at `bp+0xA` is set and that is not one of the ten: a d100, and when `50 + 5 * a - 5 *` overlay 193 `+1A2D` of `t` and `k` (signed words) is at least the total, remove `k` from `t` and bit 0x02 (`+1D2D..+1E35`). |
| 23, 245 | Effect code 98 for 23 and 2 for 245, duration `u` (sign-extended), last word the 1d6 total; bit 0x02; `L` bit 0x01 (`+1F5B..+1F8B`, `+181B..+1833`). |
| 210 | Overlay 197 `+30ED` with 213 and `t`; remove codes 108, 105, 101, 100, 82, 44, 37, 35, 33, 27, 21, 20, 10, 65, 66, 68, 67, 69 and 70 from `t`, in that order; `+1531` with word `+8` of `D` less word `+0` of `R` less the 1d4 total (`+206B..+2169`). |
| 211 | Effect code 0x1B, duration -9999, last word -1; then `+2A05` with `t`, `n`, -1 and word `+0` of `R`, read after the effect call, less the 1d4 total; then `+2321` and `+2752` (`+2169..+21AA`). |
| 117 | `+1531` with the 1d8 total (`+21AA`, `+225D..+2269`). |
| 170 | `+1531` with the 2d8 total plus 1 (`+21B3..+21C5`). |
| 194 | Overlay 197 `+30ED` with 213 and `t`, then `+1531` with the 3d8 total plus 3 (`+21C5..+21E5`). |
| 166 | When byte `+0x15` of `R` equals `g`: `+1531` with the 2d6 total. Otherwise, when `s` is 0, effect code 0x45 with duration `u` and last word 0; then, either way, `+1423` (`+224B..+2294`). |
| 127 | Overlay 197 `+2E40` with `t`, `c`, the 1d8 total and `n`; `L` bit 0x08; `+274C` (`+2294..+22B9`). |
| 38 | When `c` is 9999 or more, `+2752`. Otherwise overlay 197 `+2EC0` with `t`, `c`, the total of `a / 2` (rounded down) d6 and `n`; bits 0x03; `L` bit 0x04 (`+22B9..+22F5`). |
| 243 | When `c` is 9999 or more, `+2752`. Otherwise overlay 197 `+2EC0` with `t`, `c`, the 1d6 total and `n`; `L` bit 0x04; bits 0x03 (`+22F5..+232A`). |
| 203 | When `+2C1A` of `t` returns 0, the message `Cannot raise` (`DS:09C5`). Otherwise the d100 of FND-RNG-044; on a success its changes, effects 0x42, 0x44, 0x41 and 0x43 (each `DATA` 203, duration -9999, last word 0), bit 0x02, and bit 0x10 of byte 5 of the eight-byte record that the far pointer at `DS:67B7` and the word at `DS:67BC + 37 * t` select is cleared; on a failure the message `Blew Con roll, permanantly dead` (`DS:09A4`) and byte `+0x14` of `R` set to 8 (`+24CD..+25BC`). |
| 77 | When `+2C71` of `t` returns 0, the message `CANNOT REINCARNATE` (`DS:09D2`). Otherwise FND-RNG-044's rolls and stores; then bit 0x02, `L` bit 0x01, the same bit 0x10 cleared, the words at `DS:67C5 + 37 * t` and `DS:67C7 + 37 * t` kept, the 37 bytes at `DS:67BB + 37 * t` filled with 0xFF through `1000:3FA2`, and `31E0:0121` called with word `+0x10` of the combatant record numbered `t` plus 300, `t`, the two kept words, 0, 7 and 0; its result is not used (`+25BC..+26FB`). |

`+2C1A` returns 1 only when `2D40:3E64` finds `t`, `t` is below 4, byte `+0x14` of its combatant
record is 7 and byte `+0x12` of its details record is not 3; `+2C71` the same without the details
test (`+2C1A..+2CB5`).

## Interpretation

These are the steps each rolled case takes besides its roll. `DATA` 203's failure message names
byte `+0x1B` of `R` as Constitution and byte `+0x14` 8 as permanent death; byte `+0x14` 7, which
both gates require, is then death that a raising can undo.

## Alternatives

- FND-RNG-044 leaves out the preliminary effect of `DATA` 26 and 150, the call before `DATA` 194's
  roll, `DATA` 211's bits 0x03, `DATA` 227's effect and tail, and the messages and tails of `DATA`
  203 and 77. Nothing it records is contradicted, so it stands.
- The far pointer at `bp+0xA` is the target's effect bit set: not read here; FND-RNG-044 calls it
  `t`'s block.
- What `L` is used for after `+2752` was not read.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`: `overlay_listing.py <dsun> 179
642E0 66F95` and read the ranges above; read the twenty words at file `0x66B61` and the strings at
`DS:09A4`, `09C5` and `09D2` (file `0x4D9A4..0x4D9E5`). The listing is kept as `ovl179-all.txt`
under `GAME_DIR/analysis/rng-callers-20261020/`. Nothing was executed.
