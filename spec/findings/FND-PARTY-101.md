---
id: FND-PARTY-101
title: Besides item 313 on a roll of 20, effect code 59 reaches overlay 193 +166F as the byte at +0x19 of a hitting item's DATA record, which is 59 in DATA 104 and DATA 225, and as a script's argument to function 23 of script opcode 0x22; every other caller passes a fixed code other than 59
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000650AB..0x00065222
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066702..0x000667A6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FB29..0x0007FB60
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008BF29..0x0008BF4E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C23E..0x0008C24F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008DE17..0x0008DE41
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00081B20..0x00081B43
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00082939..0x00082977
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, direct_callers.py, data_bytes.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Overlay 193 `+166F` takes a slot, a second slot, a number `n`, a signed byte code `c`
and further arguments, and passes a positive `c` to overlay 195 `+0AF9`, which runs the level
drain for `c` = 59 on slots 0 to 3 (FND-PARTY-096). Of its 38 far calls and one near call, the
code each pushes is:

- an immediate: 10, 99, 53, 24, 66, 68, 59 (overlay 179 `+1822`, FND-PARTY-096), 92, 69, 65,
  111, 50, 67, 27, 15, 60, 105 in overlay 179; 66, 68, 65, 67, 69 and 34 in overlay 195
  (`+03CE` pushes 69 and `+1202` pushes 34, each with other pushes after the code); and 34 at both
  calls in overlay 197;
- **overlay 179 `+24BB`**, in `+1373`, the local word at `bp-0x2C`. Its only stores in `+1373` are
  the six immediates 0x5F, 0x51, 0x3F, 0x11, 0x5A and 0x27, and no address of it is taken. The
  path to the call comes from the switch at `+2422`, which the outer switch of `+1373` reaches
  for the item numbers 126, 134, 144 and 177 only, and whose own four keys are those numbers, with
  targets that store 0x5A, 0x27, 0x11 and 0x3F (`+2422..+24BB`);
- **overlay 193 `+1861`**, in `+1839`, its own byte argument `[bp+0xE]`, once for each slot of a
  list (`+1839..+186E`). `+1839`'s one caller, overlay 195 `+08F7` (trampoline `573B:0043`),
  pushes 75 for it;
- **overlay 204 `+2077`**, in `+2057` (trampoline `5787:00A2`), its dword argument when it is below
  113 (`+205A..+207C`). `+2057` has no direct caller. Overlay 204 `+0169` (trampoline
  `5787:0066`) switches on its first argument minus 1, bounded by 52, through the 53 words at
  `cs:06C7` (`+017B..+0189`); for 23 the target `+047E` passes `5787:00A2` and its two dword
  arguments on to `+1A68` (`+047E..+048C`, `+055D`, `+019E..+01A1`). `+0169`'s one caller is the
  resident far call at file `0xD46B`, in the handler of script opcode `0x22`, which forwards the
  script's arguments from `4C13:00B4..00C0` (FND-CONFIG-136);
- **overlay 179 `+0F36`**, in `+0D2F`, the local word at `bp-0xA`. `+0D2F` takes a slot `t`, a
  second slot and an item number `i` (FND-PARTY-096). It returns unless the word at `4C10:0019`
  is 2 or more (`+0DCB..+0DE0`). With `i` not -1 it stores the signed byte at `+0x19` of `DATA`
  `i` in that word, and 0 otherwise (`+0E11..+0E2F`, `+0E52`). After three overlay 197 calls that
  can end the routine (`+0E57..+0ECC`), and with `i` not -1, it calls the saving throw overlay
  179 `+2A46` with `t`, the second slot and `i` (FND-PARTY-099); when that returns 0 and the word
  is not 0, it calls `+166F` with the word as the code (`+0ECF..+0F36`).

`data_bytes.py <install>/RESOURCE.GFF 19 59` reads the 323 `DATA` resources: the signed byte at
`+0x19` is 0 in 143 of them, and 59 in two, `DATA` 104 and `DATA` 225, each 73 bytes long.

## Interpretation

Effect code 59, the level drain, has three sources: the item of `DATA` number 313 on an attack
roll of 20 (FND-PARTY-096); a hit in a fight with an item whose `DATA` record holds 59 at `+0x19`,
`DATA` 104 or `DATA` 225, when the target fails its saving throw or the item allows none; and a
script that calls function 23 of opcode `0x22` with the code 59. The byte at `+0x19` is an
item's effect on a hit, which other items set to other codes.

## Alternatives

- What `+1A68` does with the callback and its two arguments, and which slots it applies the code
  to, was not read, nor which scripts call function 23 with which codes.
- Which attacks call `+0D2F` with the attacker's item as `i`: its callers, overlay 179 `+0D1A` and
  `+1299` and overlay 193 `+04DE`, were not read, and neither were the three overlay 197 calls
  before the saving throw.
- What items `DATA` 104 and `DATA` 225 are, and who carries them, was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `direct_callers.py <dsun> 193+166F 193+1839
204+2057 204+0169`, reading the pushes before each call; `overlay_listing.py <dsun> 179 64F00
6600F`, `179 65653 66BA1`, `193 7FB20 7FB60`, `195 81B00 81B48`, `195 82930 8297C`, `204 8BF29
8BF70`, `204 8C200 8C2A0` and `204 8DE00 8DE40`; read the outer switch's 81 keys at file
`0x66BA1` with targets at `0x66C43`, the four keys at `0x66B25` with targets at `0x66B2D`, and the
53 words at `0x8C487`; and `data_bytes.py <install>/RESOURCE.GFF 19 59`.
