---
id: FND-MAGIC-006
title: Overlay 182 +0B52 fills the spell pools only at a rest, at Start Game, in the new-game party loader, when ADD or the end of character generation places a character, and on the M key while DS:143C is set; the level gain does not call it, and a load restores the pools of slots 0 to 3 from SAVE 23
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000693A2..0x000693FC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068E8D..0x0006900C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058990..0x000589D4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058CB5..0x00058E50
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E0B9..0x0006E0D6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E2E3..0x0006E387
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079794..0x000797B0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0007A284..0x0007A308
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008126E..0x00081309
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008CF2E..0x0008CF38
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Capstone 5.0.9; Python 3.14.7; a short read of the 66 words at file 0x7A284 and a raw byte search of DSUN.EXE for far and near calls to overlay 182 +0B52
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x58210 + x` for overlay 171, `0x68850 + x` for 182, `0x6D090 +
x` for 184, `0x78340 + x` for 190, `0x81130 + x` for 194 and `0x8BDC0 + x` for 204 (the code bases
the overlay headers give, FND-EXE-003).

Overlay 182 `+0B52` takes a slot, does nothing when it is above 4 (signed), and otherwise stores,
for `L` from 1 to 9, the low byte of overlay 177 `+05B7` of the slot, kind 1 and `L` at
`DS:4469 + 34 * slot + L` and of kind 2 at `DS:4474 + 34 * slot + L` (`+0B52..+0BAC`,
FND-PARTY-111).

`direct_callers.py <dsun> 182+0B52` gives one near call and five far calls through trampoline
`56BD:0098`. A raw search of the whole file for the far-call words `0x0098, 0x05B0` finds the
same five, each after a `9A`, and none for the resident form `0x0098, 0x46BD`; a search of
overlay 182's code at every byte for `E8` or `E9` with a target of `+0B52` finds only `+0712`.

| Call | In routine | Slot | Condition |
| --- | --- | --- | --- |
| overlay 182 `+0712` (near) | `+063D`, the party loader (FND-PARTY-013, FND-PARTY-021) | each of 0 to 3 | every pass of its loop over slots 0 to 3, after the slot's `CHAR` 40 + slot is loaded and placed; the place call's result is not tested (`+0690..+07BC`) |
| overlay 194 `+01BE` | `+013E`, the Start Game branch of the start window (FND-PARTY-044) | each of 0 to 3 | after overlay 182 `+1167` with 0, the gate that can reach the loader (FND-PARTY-021); right after, `4C13:0357`, the party's money (FND-EXE-607), is set to 5000 (`+013E..+01D9`) |
| overlay 171 `+0C3A` | `+0AA5`, called from `+07C1` in the ADD window's handler (FND-PARTY-046), on the branch taken when the word at `4C4C:0000` is not -1; when it is -1 the branch shows `NO CHARACTERS AVAILABLE` instead (`+0780..+07C4`) | the word at `4E71:0B44`, the box number | after a `CHAR` resource is loaded into that slot and placed through `31E0:0121` (`+0AA5..+0C40`) |
| overlay 184 `+12F1` | `+1029`, the end of character generation (FND-PARTY-070) | its slot argument | only with its flag argument at `bp+8` not 0 and `DS:112E` equal to -1, after the place call `31E0:0121` (`+1029..+1046`, `+1253..+12F7`) |
| overlay 190 `+1461` | `+139B`, the map's key handler (FND-INPUT-012) | each of 0 to 3 | for the key words 0x324D and 0x326D (`M`, `m`) of the 33-word table at `cs:1F44`, whose targets follow at `cs:1F86`, only when the byte at `DS:143C` (`g_57E0_143C`) is not 0 (`+1454..+1470`) |
| overlay 204 `+1172` | `+0CBA`, the rest routine (FND-MAGIC-005) | each member | when the member's `combat_mark` is 1 (`+116E..+1178`) |

No overlay 209 or 210 routine, where the level gain and its spell and power windows run
(FND-PARTY-081, FND-PARTY-104), calls `+0B52`, and `immediate_search.py` finds no other writer of
the pool bytes than `+0B52`, overlay 177 `+0CB3` (the move) and overlay 197 `+3060` (the spend)
(FND-PARTY-111). `SAVE` resource 23 is the `0x88` bytes at `DS:445E` (FND-SAVE-015), which hold
`DS:4469 + 34 * slot + L` and `DS:4474 + 34 * slot + L` for slots 0 to 3 and `L` from 1 to 9
(`DS:446A` to `DS:44E3`).

## Interpretation

The pools are filled for slots 0 to 3 at Start Game and by the new-game party loader, for one
slot when a character joins through ADD or comes out of character generation, for each member
that stands at a rest, and by the `M` key when the byte at `DS:143C` is set, which reads as a
debug key. A level gain changes the class level that `+05B7` counts but does not recount, so
a character who gains a level casts from the old pools until the next rest. Saving writes the
pools of slots 0 to 3 as they are and loading reads them back, so a load restores the spells
left at the save rather than recounting.

## Alternatives

- A level gain could reach one of the six routines through calls that were not followed; only
  direct calls of `+0B52` were searched, and indirect calls are not covered by either search.
- That the load path reads `SAVE` 23 rests on FND-SAVE-015 and FND-SAVE-017 and FND-SAVE-022; whether the load's
  call of overlay 182 `+1167` can reach the party loader, which would recount slots 0 to 3, depends
  on `DS:264E` after a load, which FND-PARTY-021 leaves open.
- The pools of slot 4 are filled by `+0B52` but not saved; what slot 4 is was not read.
- What `DS:112E` equal to -1 and the flag of overlay 184 `+1029` mean (a new character against an
  edited one) was not read beyond FND-PARTY-070.
- What sets `DS:143C` was not read.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python and `<dsun>` the installed
`DSUN.EXE`: `direct_callers.py <dsun> 182+0B52 171+0AA5 197+3060`; `overlay_listing.py <dsun> 171
58210 5925A`, `182 68850 6AD08`, `184 6D090 6F3B0`, `190 78340 7BE16`, `194 81130 816C9` and
`204 8BDC0 8E660`, keeping the routines in the table. Read the 33 words at file `0x7A284` and
the 33 after them and keep the pair `0x324D` / `0x326D` with target `0x1454`. Search the file for
the bytes `98 00 B0 05` and `98 00 BD 46`, and overlay 182's code `0x68850..0x6AD08` at every
byte for `E8` or `E9` whose target is `0x693A2`. Compare `DS:445E + 0x88` with the pool addresses.
