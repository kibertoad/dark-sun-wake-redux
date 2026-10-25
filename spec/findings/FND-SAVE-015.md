---
id: FND-SAVE-015
title: Overlay 187 saves and loads SAVE resources 1 to 61 through a table of far pointers at DS:157E and double-word lengths at 4F38:0000; entry 46 is the current-combatant word 4C13:0369 and no entry covers DS:426D
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070713..0x0007089C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007089C..0x000709A8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004E57E..0x0004E672
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00044580..0x00044674
tool: tools/research/exec-census/overlay_listing.py and gff_tag_numbers.py at the commit that adds this finding; Capstone 5.0.9; Python 3.14.7
environment: null
---

## Observation

Addresses are static; segment words in the file's table are relative to 0, so `0x1000` is added.

**The table.** The 61 far pointers at `DS:157E` (file `0x4E57E`) and the 61 double words at
`4F38:0000` (file `0x44580`) give, for `SAVE` resource `i`:

| `i` | Address | Length |
|---|---|---|
| 1 | `DS:67BB` | `0x2E40` |
| 2 | `4F49:08A3` | `0x390` |
| 3 | `4F49:0C33` | `0x624` |
| 4 | `4F49:0363` | `0x540` |
| 5 to 10 | null | `0x80A`, `0x462`, `0x23F0`, `0x2EE0`, `0xC40`, `0x1CC` |
| 11 to 18 | `DS:56ED`, `DS:5AF5`, `DS:56F1`, `DS:5AF3`, `DS:56EF`, `DS:55C8`, `DS:55C6`, `DS:140C` | 2 each |
| 19 | `4F49:0330` | `0x33` |
| 20 | `4D62:039C` | `0x3C` |
| 21 | `4E71:09CD` | `0x88` |
| 22 | `4E71:0A55` | 4 |
| 23 | `DS:445E` | `0x88` |
| 24 | `4E71:024D` | `0x780` |
| 25 | `50B0:000F` | `0xFC` |
| 26 | `DS:2100` | 2 |
| 27 | `551D:0352` | `0x6A4` |
| 28 | `551D:0000` | `0x352` |
| 29 | `DS:2A6B` | 4 |
| 30 to 38 | null | `0x65`, `0x190`, `0x50`, `0x1A4`, 9, `0x40`, `0x28`, `0x1A4`, `0xA28` |
| 39, 40 | `4C13:0357`, `4C13:035B` | 4 each |
| 41 to 45 | `4C10:001B`, `4C10:001D`, `4C10:001F`, `4C10:0019`, `4C10:0021` | 2 each |
| 46 to 51 | `4C13:0369`, `4C13:0367`, `4C13:0365`, `4C13:0363`, `4C13:035F`, `4C13:032D` | 2 each |
| 52 to 61 | `4C10:0005` to `4C10:0017`, every 2 bytes | 2 each |

No entry covers `DS:426D`.

**Saving.** Overlay 187 `+08E3(n)` (file `0x70713`) goes through the entries `i` below `0x11`, or
below `0x3D` when `n` is 0 (`+08F6`, `+091C`). For a non-null entry it writes `SAVE` resource
`0x3D × n + i + 1` from the entry's pointer with the entry's length; for the entry whose pointer
equals the far pointer at `DS:67B7` it first stores 8 times the word `DS:264E` as that length
(`+0939..+0961`).

**Loading.** `+0A6C(n)` (file `0x7089C`) goes through the same entries with the same counts; it
fills the entry at `DS:67B7` with `0x2EE0` bytes through `1000:3FA2` (`+0ABB..+0ADC`) and reads
`SAVE` resource `0x3D × n + i + 1` into each non-null entry's pointer through `38FF:04AB`
(`+0AEE..+0B1D`). After the loop it copies the double word `4C13:035B` to `DS:19B8` (`+0B6A..+0B74`).

**The install.** `gff_tag_numbers.py` finds no `SAVE` table in any of the 26 installed `.GFF`
archives, and the install directory holds no `SAVEnn.SAV` file.

## Interpretation

A load restores the current-combatant word `4C13:0369` (entry 46) and the combat word
`4C10:0019` (entry 44) from what an earlier save took from them, and leaves `DS:426D` to whatever
code later sets it from `4C13:0369`. Nothing in the shipped files supplies `SAVE` resources, so a
loaded `4C13:0369` is always a value the game itself held when it saved.

## Alternatives

The null entries may be filled by code not read here; they hold null in the file. What reads
`4C13:0369` into `DS:426D` after a load was not read here.

## How to reproduce

Read 244 bytes at file offsets `0x4E57E` and `0x44580`; `overlay_listing.py <dsun> 187 70713
7089C` and `... 187 7089C 709A8`; `python -I gff_tag_numbers.py <install> SAVE`.
