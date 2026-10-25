---
id: FND-MAGIC-019
title: Overlay 193 +13EC, the second caller of the DATA dice, rolls them for an object's DATA number at the level the object holds and hits the slot that set the object off as both target and caster; the level is the low five bits of the object slot's byte +0x15, which +29EB stores as the placing cast's level capped at 31, and the same level times a field of the object record sets how long the object lasts
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F6DC..0x0007F798
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F598..0x0007F6DC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00064F5F..0x0006500F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E64A..0x0007E6F5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E907..0x0007EAE1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007EAE1..0x0007EB77
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00080CDB..0x00080ECE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F064..0x0007F08E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F26F..0x0007F325
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F344..0x0007F409
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070A00..0x00070ADD
tool: tools/research/exec-census/overlay_listing.py, direct_callers.py and immediate_search.py at the commit that adds this finding; Capstone 5.0.7; Python 3.14.7
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x7E2F0 + x` for overlay 193, `0x642E0 + x` for 179 and
`0x6FE30 + x` for 187. `R` is the 18-byte record of the table at `51F1:0000` (FND-EXE-606), whose
word `+0` is a `DATA` number; an object slot is FMT-ACTOR-004's 37-byte record at `DS:67BB`, whose
byte `+0x0F` is one more than the number of its `R` and byte `+0x15` is `DS:67D0` with the slot's
offset.

**`+13EC(s, n, L, g)`** (`+13EC..+148E`) copies `DATA n` and calls `+1F88` with `n`, `L` and the
address of its word `bp-8`, set to 0 at `+13FA`, and keeps the total in `bp-6`
(`+1426..+1437`). It calls overlay 179 `+0C7F` with `s` as the target, `s` again as the second
slot, `n`, word `+0x1A` of the copy, the address of `bp-6`, the word `bp-8`, 0, the dword
0x80000 (words 0 and 8), 0x63, `L` and `g` (`+143A..+145E`), and then overlay 173 `+24C7` when
the combatant byte `+0x14` of `s` is not 1 (`+1466..+1485`). `+148E` passes its four arguments to
`+13EC` (`+148E..+14A8`). `direct_callers.py <dsun> 193+13EC 193+148E` finds the near calls at
`+1359` and `+14A0`, and at `+13B8` for `+148E`, and no far call through their trampolines.

**Overlay 179 `+0C7F`** (`+0C7F..+0D2F`) calls overlay 193 `+1AA4` with its second slot and 0x35,
then 0x36, unless the `DATA` number's byte `+0` low two bits are neither 0 nor 2 and the word at
`bp+0xE` is 0 (`+0C8D..+0CF0`); `bp+0xE` is the offset of the total's address, which is not 0 for
a stack local, so the two calls run. It then calls `+0D2F` with its own thirteen words
(`+0CF3..+0D1A`) and the queue runner `+11DC` (`+0D25`). FND-MAGIC-018 reads `+0D2F`'s use of the
total and maximum.

**`+12A8(s, i, L, g, mode, f)`** (`+12A8..+13EC`) returns for `i` of 0xFF or -1 or for an `R` `i`
whose byte 2 low nibble is 0; it takes `n` from word `+0` of `R` `i` and switches on `mode`
(`+12B2..+1300`). Mode 0 sends `n` 0x64, 0x5A and 0x6D to `+1119`, 0xDD to `+1249`, skips 0xDF and
0x6D for `s` below 4, and sends the rest to `+13EC` with `s`, `n`, `L` and `g` (`+1303..+135F`).
Mode 1 calls `+148E` with the same unless `R`'s byte 2 low nibble is 1, or 2 with `f` set, or 3
with `f` clear (`+1362..+13BB`). Mode 2 calls `+14A8` with `s` and `n` only (`+13DF..+13E6`).

`direct_callers.py <dsun> 193+12A8` finds four near calls:

- `+0D9B` in `+0AAD` and `+0FBE`, `+102F` in `+0DC7` pass `L` = the low five bits and `g` = bits 5
  to 7 of the object slot's byte `+0x15`, with mode 0, 0 and 1 (`+0D74..+0D9B`, `+0F93..+0FBE`,
  `+1004..+102F`);
- `+10CA` in `+1054` passes `L` 6, `g` 1 and mode 2 (`+10AC..+10CA`); mode 2 does not pass `L` on.

**The writers of byte `+0x15`.** `immediate_search.py <dsun> 67D0` lists the reads above, the
copy at overlay 187 `+0C12` and `+0C97`, and four stores in overlay 193 `+29EB`. Overlay 187 reads
the byte at `+0C12`, refills the slot through `31E0:0EFF` (`+0C3A`) and stores the byte back
(`+0C97`). `+29EB` (`+29EB..+2BDE`) stores into the low five bits its byte argument at
`bp+0x12`, 31 when that is 31 or more, and into bits 5 to 7 the low three bits of its byte at
`bp+0x14` (`+2ADE..+2B1D`), and one more than its `R` number in byte `+0x0F` (`+2AFC..+2B06`).
`direct_callers.py <dsun> 193+29EB 193+07F1 193+0617` gives one caller each: `+0884` in `+07F1`,
`+07E8` in `+0617`, and `+03B5` in `+003C`. Through the pushes, `+29EB`'s `bp+0x12` and `bp+0x14`
are `+07F1`'s `bp+0x18` and `bp+0x1A`, which are `+0617`'s `bp+8` and `bp+0xA`, which `+003C`
fills with its level byte `bp+0xC` and its kind byte `bp-0x1D` (`+039B..+03B5`).

**`+003C`'s object branch.** When byte `+0x17` of `DATA n` is not 0xFF, `+003C` calls `+0617` with
`n`, `L`, `g`, the position and the four position words (`+035A..+03B5`); afterwards it goes on to
its target loop only when the low nibble of byte 2 of `R` `+0x17` is 1 (`+03DD..+0405`).

**The object's lifetime.** In `+0617` (`+0617..+07F1`), with `R` the record `+0x17` names, the
dword `bp-4` is 30000 when `R`'s word `+0xC` is 30000, and otherwise `(high nibble of R+0xB * L
+ overlay 173 +2A5D(bits 2 to 7 of R+0xA, low nibble of R+0xB, 0)) * word R+0xC`
(`+0716..+0775`). For a low nibble of `R` byte 2 other than 1 it calls `+07F1` with that dword
(`+07BB..+07E8`); for 1 it calls `+20DA`, which receives neither `L` nor the dword
(`+079A..+07B3`). `+07F1` adds to the dword the absolute values of `R`'s words `+0xE` and `+0x10`
and 1 (`+0819..+085D`) and passes the sum to `+29EB` as its dword `bp+8`, with a dword it computes
from the far pointer at `DS:9C0C` and the byte `DS:9C0A` as `bp+0x18` (`+0806..+0825`,
`+0866..+0884`). When the sum is not negative, `+29EB` calls `371D:01F9` with byte `bp+0x16`, the
sum of the two dwords, 3 and a local pair holding the object slot and 2 (`+2BA8..+2BCB`).

## Interpretation

The second call of `data_dice` belongs to a placed spell object, such as an area a spell leaves on
the ground. When something sets the object off, `+12A8` casts the object's `DATA` number on the
slot that did so, which is both the target and the caster, at the level the object holds. That
level is the placing cast's level, capped at 31 when stored and at 10 inside `data_dice`, so an
object's dice and the duration of the effect it gives grow with the caster's level as a direct
cast's do. The object's total and maximum go through the hit routine as FND-MAGIC-018 reads.

The placing cast's level also scales the object's lifetime: the high nibble of `R` byte `0x0B`
gives time per level, on top of a roll. The sum is scheduled through `371D:01F9` as an event of
kind 3 for the object; reading `DS:9C0C` as a clock is a guess.

An object filled through `31E0:0EFF` alone holds level 0 in byte `+0x15` (FMT-ACTOR-004 has the
fill clear the byte), which `data_dice` treats as `k` of at least 1 and `+1E81` raises to 1.

## Alternatives

- `immediate_search.py` finds stores through the displacement `0x67D0` only. A store to byte
  `+0x15` through a pointer to the slot, such as `31E0:0EFF`'s fill or a block copy of the table
  on a load, is outside it; FMT-ACTOR-004 records the fill's clear from FND-ACTOR-003.
- What `+0AAD`, `+0DC7` and `+1054` test before their calls (stepping on, entering or leaving the
  object), what `+1119`, `+1249`, `+20DA` and `371D:01F9` do, and what `R`'s fields `+0xA` to
  `+0x10` mean beyond this arithmetic, were not read.
- Effects 0x35 and 0x36 that `+0C7F` removes from the slot, and overlay 173 `+24C7`, were not read.
- No emulated call or run was made.

## How to reproduce

From `tools/research/exec-census/`: `overlay_listing.py <dsun> 193 7E2F0 80ECE`, `... 179 642E0
66F95` and `... 187 70A00 70AE0`, reading the ranges above; `direct_callers.py <dsun> 193+13EC
193+148E 193+12A8 193+29EB 193+07F1 193+0617 179+0C7F`; `immediate_search.py <dsun> 67D0`.
