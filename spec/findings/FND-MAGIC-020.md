---
id: FND-MAGIC-020
title: A spell turned back by overlay 197 +1760 is queued from the target onto the caster with the hit's number and level, so the call at +180F carries the level as well as 6; script function 46 of opcode 0x22 casts the number arg mod 1000 at the level (arg / 1000) mod 100
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00084B80..0x00084CD6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000654BC..0x000655E4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008BF3B..0x0008BF4B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C01E..0x0008C062
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D68D..0x0008D779
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0008C487..0x0008C4F1
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Capstone 5.0.7; Python 3.14.7; a read of the 53 words at file 0x8C487
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x83400 + x` for overlay 197, `0x642E0 + x` for 179 and
`0x8BDC0 + x` for 204. Overlay 197 `+1760` is called only from the hit routine at overlay 179
`+0E9E` (`direct_callers.py <dsun> 197+1760`), with `bp+6` the target `t`, `bp+8` the caster `c`,
`bp+0xA` `t`'s effect block, `bp+0x12` the number `n` and `bp+0x1C` the level `L`. It ends the hit
when it returns 0.

**The turn-back.** The far call at `+180F` to overlay 179 `+12B1` is reached two ways:

- falling through from `+1808`, which pushes 6, 0x133 (307), `t` and `t`, after `+17E0..+1805`
  (bit 0x40 of block byte 5 and a d100 of 75 or less);
- by the jump at `+18D3`, after `+18CA..+18D0` push `L`, `n`, `t` and `c`. Two paths reach
  `+18CA`: bit 0x10 of block byte 0x0A set, bit 0x80 of word `+0x11` of `DATA n` set, `c` below
  0x30 and overlay 208 `+0885` of `n` and 1 at most 8 (`+181C..+1857`); or bit 0x20 of block byte
  0x0A set, `c` below 0x30 and not `t`, `n` from 0 to 0xEA whose level by overlay 177 `+09C5` is 1
  to 3, and byte `+0x17` of `DATA n` 0xFF, after overlay 193 `+1AA4` of `t` and 0x55
  (`+1859..+18C7`).

After the call `+1760` returns 0 (`+1814..+1819`). Overlay 179 `+12B1(a, b, n, L)` stores `a`, `b`
and `n` at words `+0`, `+2` and `+4` of a queue record and `L` at byte `+0x0C` (`+12EA..+1302`).
The queue runner `+11DC` calls overlay 193 `+003C` with the record's word `+2` as the caster, word
`+0` as the target, word `+4` as the number and byte `+0x0C` as the level byte, at the position
words of slot `+0`'s object record (`+11F9..+125C`).

**Script function 46.** Overlay 204 `+0169` decrements its first argument, returns for a value
above 0x34 and jumps through the 53 words at `cs:06C7` (`+017B..+018B`); word 45 is `0x025E`.
There it sets `DS:45AA` to 1 and, for a slot argument of 9999, calls `+18CD` with -1 and its two
dword arguments (`+025E..+0282`), and otherwise passes trampoline `5787:0093`, which enters
`+18CD`, to `+1A68` (`+0289..+029C`). `+18CD` (`+18CD..+19B9`) stores the first dword divided by
1000, times 16 plus 8, in `DS:43DD` and its remainder, the same way, in `DS:43DB`; from the second
dword `v` it keeps `v mod 1000` as `n`, `(v / 1000) mod 100` as `L`, `v / 100000` in `DS:43D9`
and 1 in `DS:43CE` when `v / 10^7` is even (`+18D6..+195C`). For a slot below 0 or inside `0x800`
by `0x620` it calls overlay 193 `+0000` with -1 as the caster, the slot, `n`, `L` and the four
words `DS:43D5` to `DS:43CF` (`+196F..+19AF`). `+003C` takes the position from `DS:43DD` and
`DS:43DB` and `g` from byte `DS:43D9` while `DS:45AA` is not 0 (`+0055..+0075`).

## Interpretation

A target protected by the effect behind bit 0x10 or 0x20 of block byte 0x0A turns a spell back:
the hit on it ends, and the spell is queued as the target's cast at the original caster, with the
original number and level. So the queue's level byte is the turned spell's level, not 6, on this
route; FND-RNG-051's row for overlay 197 `+180F` lists only the 6 of the route through `+1808`.

A script casts with function 46 of opcode 0x22, packing the spell number in the last three
decimal digits of its argument and the level in the two above them, so a script can cast at any
level from 0 to 99, capped at 10 for the dice.

## Alternatives

- What the block bits 0x10 and 0x20 of byte 0x0A stand for, overlay 208 `+0885`, and effect 0x55
  were not read; Q-MAGIC-008 holds the block bits and `+0885`.
- Which scripts call function 46 and with what values was not tallied.
- That function 46 is opcode 0x22's case rests on FND-PARTY-101's reading of `+0169` as that
  opcode's handler and of index 22 as function 23.
- No emulated call or run was made.

## How to reproduce

From `tools/research/exec-census/`: `overlay_listing.py <dsun> 197 83400 86F67`, `... 179 642E0
66F95`, `... 204 8BF29 8BF4C`, `... 204 8C010 8C068` and `... 204 8D68D 8D780`; read the 53 words
at file `0x8C487`; `direct_callers.py <dsun> 197+1760 179+12B1 179+11DC 204+18CD`.
