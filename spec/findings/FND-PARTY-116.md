---
id: FND-PARTY-116
title: Only the -k911 debug switch opens the two paths that set DS:13F8, the game's own stores keep class levels at 15 or below, and the OBJEX actors Nortemus, Fori, Zydeko and Matthias hold Preserver levels of 18 and 20
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:0024..277B:006E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:00EB..277B:010F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:0188..277B:019A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0001CF51..0x0001CF85
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:0CFF..28C9:0D75
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:102B..28C9:10B6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0001F091..0x0001F0F1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D6BD..0x0004D6C1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004E3F8..0x0004E3F9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004E43C..0x0004E43D
  - build: BLD-GOG-EN-1.1
    file: RAVAGER.BAT
    offset: 0x00..0x14
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:1890..28C9:18B0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000625BC..0x000625D8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008E454..0x0008E499
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000789FB..0x00078A16
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00094961..0x000949F4
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x577..0x57D
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x40D250..0x40D256
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x43EF47..0x43EF4D
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x43F069..0x43F06F
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x65F6E6..0x65F6EC
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/immediate_search.py, resident_listing.py, overlay_listing.py, field_stores.py, details_chunks.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed files.
The data segment `DS` is `57E0`, at file `0x0004D000` of `DSUN.EXE`.

**`DS:13F8`.** `immediate_search.py <dsun> 13F8` finds two stores, both of 1, at `277B:0192` and
`28C9:10AE`, and three reads, overlay 177 `+0519` and `+05C7` and overlay 211 `+1F52`. The byte
is 0 in the file.

**The command line.** `277B:0024` takes the argument count and vector. For each argument after the
first it lowers the argument's second character, subtracts `a` and, for a value up to 25, jumps
through the 26 words at `277B:05A1` (`+0043..+006E`). The word for `k` enters `+00EB`, which
compares the text after the argument's first two characters with the string `911` at `DS:06BD`
through `1000:3787` and, when that returns 0, stores 1 in the byte at `DS:143C`. The word for `s`
enters `+0188`, which stores 1 in `DS:13F8` when `DS:143C` is not 0. `immediate_search.py <dsun>
143C` finds no other store to `DS:143C`, which is 0 in the file. `RAVAGER.BAT`, which the GOG
DOSBox configuration runs, holds the one command `DSUN -W0 -L`.

**The key handler.** `28C9:0CFF` returns at once unless its first argument is 6. Otherwise it
compares the word at `[bp + 0x12]` with 24 words at `28C9:1201` and jumps through the 24 words
after them (`+0D55..+0D71`). The word for `0x6B00`, the code the BIOS gives Alt+F4, enters
`+102B`, which does nothing while `DS:143C` is 0. With it set, for each slot from 0 to 3 whose
byte at `4F49:0C33 + 3 * slot` is 2, it sets the 0x73 bits of the slot's spell bits at
`4D62:039C + 15 * slot`, and when `27E5:00C1` of the slot and 12 returns more than 0, calls
`5691:0039` with the slot and each of 0 to 0x21; then it stores 1 in `DS:13F8` (`+102B..+10B3`).
Overlay 182 pushes `0x0CFF` four times and overlay 199 once.

**The level bytes.** `field_stores.py <dsun> <inventory> --es 1E 1F 20` lists 20 stores in
inventory rows and 10 outside them. Those that write bytes `+0x1E` to `+0x20` of a 0x42-byte
details record through `DS:19C5` or `DS:1429` are:

- overlay 183 `+136A`, `+136F`, `+1374`, `+13C4`, `+13D2` and `+14BB`, generation's level setting
  (FND-PARTY-065);
- overlay 209 `+1831` and `+1890`, the class change, which moves each class's level byte up one
  position and stores 1 for the new class (`+1801..+1894`);
- overlay 210 `+07CB`, `+0C86` and `+0D46`, the level gain and the level drain
  (FND-PARTY-081, FND-PARTY-082).

The other stores write other records. `28C9:18AC`, `28C9:1D73` (listed as `2AA0:0003`), overlay
177 `+04D4` and overlay 204 `+26A2` and `+26D5` form their pointer from `DS:19C9` and 0x31, the
combatant records; overlay 173 `+1FF4` does the same. Overlay 190 `+06D1` stores 0 at `+0x1E` of
the string at `[bp + 0x0C]` when `1000:40D7` returns more than 0x1E for it. Overlay 177 `+0043`
writes 0x21-byte records in segment `0x0320`, and the five stores in `37FC` add to and subtract
from a dword at `+0x20`; the second pass lists them again out of alignment. `48B0:000F` is decoded
from inside the instruction `mov di, 0x126` at `48B0:000E`.

**The archives.** `details_chunks.py <install>/CHARSAVE.GFF --levels` lists 22 details chunks.
Their highest level for each class code is 15 or less; Preserver (code 11) appears in four
characters, highest in `CHAR` 50 (`Ar'Anda`, classes 11, 12 and 17 at levels 15, 14 and 15).
`details_chunks.py <install>/OBJEX.GFF --levels` lists 375. 22 of them hold a level above 15.
Four of those are of class code 11: `RDFF` 64 (`Nortemus`), 65 (`Fori`) and 67 (`Zydeko`) at level
20, and `RDFF` 172 (`Matthias`) at level 18. The others are class code 9 at 16 (six, among them
`Helmine` and `Melantha`), code 4 at 20 (`Xarax`), codes 19 and 18 at 15 and 18 (`Lord
Warrior`), and ten with class code 0 (drakes, feyr, `Aravinda`, `Kartang`, `Traxiss`).

## Interpretation

`DS:13F8` is a debug byte. Only a command line holding `-k911` before `-s` sets it at start, and
only `-k911` with Alt+F4 in play sets it with all wizard spells for the party. The shipped launch
passes neither, so in a normal game the byte stays 0 and the cast list keeps the class tests of
FND-PARTY-111.

Within `DSUN.EXE`, the stores found at the level bytes' fixed displacements are generation, the
class change and the gain and drain, none of which goes above 15. The shipped party list holds no
level above 15. So a party member reaches a Preserver level above 15 only from a record those do
not cover: a character `CHARTRAN.EXE` brings from a Dark Sun 1 saved game (its copy at
`13D8:0F73..13D8:0F92` takes the three level bytes as they are, FND-PARTY-114), or an `OBJEX`
actor, four of which are Preservers at 18 or 20, if such an actor can become a party member.
Those four also bear on Q-PARTY-052: a Preserver at level 17 or more has a level 9 count, so as
an enemy each could cast spell `DATA` 104 if the enemy cast takes the same counts.

## Alternatives

- `1000:3787` is read as a string compare from the zero test of its result; its body was not read.
- A store to a level byte through an address computed into a register, with no displacement of
  0x1E to 0x20, is outside the search, as are block copies other than those FND-PARTY-115 lists
  and file reads such as a saved game's load, which puts back what a save wrote.
- Script instructions that change a character's fields were not read (Q-SCRIPT-004).
- The levels a Dark Sun 1 saved game can hold are not in this installation, and the `CHARTRAN.EXE`
  routine that fills its 0x47-byte slot records from the save was not read.
- The `OBJEX` chunks are found by their header bytes; one of the 375 lies outside any resource, and
  a name is printed only where a combatant chunk header precedes the details chunk.
- Whether the five pushes of `0x0CFF` register `28C9:0CFF` for the play screen was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `immediate_search.py <dsun> 13F8 143C 0CFF`;
`resident_listing.py <dsun>` over the resident ranges above; read the 26 words at file
`0x0001CF51`, the 48 words at file `0x0001F091`, and the bytes at file `0x0004D6BD`,
`0x0004E3F8` and `0x0004E43C`; `field_stores.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv
--es 1E 1F 20` and `overlay_listing.py` over the overlay ranges above; and `details_chunks.py
<install>/CHARSAVE.GFF --levels` and `details_chunks.py <install>/OBJEX.GFF --levels`. Read
`<install>/RAVAGER.BAT`. For `CHARTRAN.EXE`, unpack it as FND-PARTY-114 does and read
`13D8:0F73..13D8:0F92`.
