---
id: FND-PARTY-055
title: The character sheet prints origin, gender and alignment from bytes 0x12, 0x13 and 0x14 of the combatant details record as 1-based indexes into the name tables at DS:113C, and the levels from bytes 0x1E to 0x20 for each nonzero class byte at 0x1B to 0x1D
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006C17A..0x0006C303
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006C763..0x0006C847
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006D3CE..0x0006D485
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007BA25..0x0007BA4D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00078958..0x00078976
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006D870..0x0006D885
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004E13C..0x0004E1C0
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004E244..0x0004E363
    kind: file-data
tool: Python 3.14.7 with Capstone 5.0.7 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, immediate_search.py, direct_callers.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`, whose data segment `57E0` starts at file `0x4D000`.

**The name tables.** From `DS:113C` the data segment holds 33 far pointers to strings in the
same segment: 2 genders (`MALE`, `FEMALE`), 8 origins from `DS:1144` (`HUMAN`, `DWARF`, `ELF`,
`HALF-ELF`, `HALF-GIANT`, `HALFLING`, `MUL`, `THRI-KREEN`), 8 classes from `DS:1164` (`CLERIC`,
`DRUID`, `FIGHTER`, `GLADIATOR`, `PRESERVER`, `PSIONICIST`, `RANGER`, `THIEF`), 6 score labels
from `DS:1184` and 9 alignments from `DS:119C` (`LAWFUL GOOD`, `LAWFUL NEUTRAL`, `LAWFUL EVIL`,
`NEUTRAL GOOD`, `TRUE NEUTRAL`, `NEUTRAL EVIL`, `CHAOTIC GOOD`, `CHAOTIC NEUTRAL`, `CHAOTIC
EVIL`). The strings are at file `0x4E244..0x4E363`.

**Overlay 183 `+1783`** (trampoline `56CC:002A`) takes a far pointer, a far pointer to a record and
two coordinates. It reads the record's byte at `+0x13`, multiplies it by 4, and prints the string
whose far pointer is at `DS:1138` plus that (`+1791..+17DF`), so 1 prints `MALE` and 2 `FEMALE`.
It then does the same with the byte at `+0x12` and `DS:1140`, so 1 prints `HUMAN` and 8
`THRI-KREEN` (`+17E7..+181B`), placing it after the first string.

**Overlay 183 `+1827`** (trampoline `56CC:002F`) prints the string at `DS:1198` plus 4 times the
record's byte at `+0x14` (`+182A..+185D`), so 1 prints `LAWFUL GOOD` and 9 `CHAOTIC EVIL`.

**Overlay 183 `+11BC`** returns without printing when the record's byte at `+0x1B` is 0. Otherwise
it prints the byte at `+0x1E`; when the byte at `+0x1C` is nonzero it adds `/` and the byte at
`+0x1F`; and when the byte at `+0x1D` is also nonzero it adds `/` and the byte at `+0x20`
(formats at `DS:10DF`, `DS:10CF` and `DS:10B8`). While the far value at `55CA:0000` is nonzero, the
colour of each number comes from overlay 190 through trampoline `571F:008E` with the matching class
byte. Overlay 183 `+119A` passes the class table at `DS:1164` to overlay 190 through trampoline
`571F:0089`; that routine was not read.

**The record.** Overlay 184 `+0361..+03BC` calls `+1783` and `+1827` with the far pointer at
`DS:1431` and the record at `DS:1429`. Overlay 190 `+36E5..+3709` stores its argument as the slot
in `4E71:0B44` and sets `DS:1429` to the record of that slot in the table at the far pointer
`DS:19C5`, 66 bytes per record: the FMT-COMBAT-002 record. Overlay 190 `+0618..+0632` sets it the
same way. Overlay 184 `+07E9..+07EF` sets it to `4E4F:0029` instead, and overlays 202, 209, 211 and
212 also store to it.

## Interpretation

On the character sheet, for the selected slot, the combatant details record's byte at `0x12` is
the origin, `0x13` the gender and `0x14` the alignment, each counted from 1 in the order of the
name tables. The bytes at `0x1B` to `0x1D` hold up to three classes, 0 meaning none, and `0x1E` to
`0x20` the level in each. A `CHAR` record's type-3 chunk carries these bytes at `0x57`, `0x58`,
`0x59`, `0x60` to `0x62` and `0x63` to `0x65` (FND-PARTY-051).

## Alternatives

- The class bytes are indexes into the class table at `DS:1164`: the stored values (FND-PARTY-056)
  run above 8, so the routine behind `571F:0089` maps them some other way; it was not read.
- The routines print another record than the slot's on this screen: the other stores to
  `DS:1429` (overlays 184 `+07EF`, 202, 209, 211 and 212) were not traced to this screen's calls.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 183 0x6C17A
0x6C303` and `183 0x6C763 0x6C847`, `184 0x6D3CE 0x6D485` and `184 0x6D870 0x6D885`, `190 0x78958
0x78976` and `190 0x7BA25 0x7BA4D`, `immediate_search.py <dsun> 1429 142B`, and `direct_callers.py
<dsun> 183+1783 183+1827`. Read the 33 far pointers at file `0x4E13C` and the strings they name.
