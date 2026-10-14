---
id: FND-PARTY-203
title: The game reads an SPST resource with its own length into a 15-byte entry, so a 9-byte one fills only the first 9 bytes, and in the shipped CHARSAVE.GFF the three 9-byte ones belong to characters that neither ADD nor START GAME loads
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
tool: Python 3.14.7 with Capstone 5.0.7 (tools/research/exec-census/overlay_listing.py, direct_callers.py, disassembly of the load image at segment 1000) and a reading of CHARSAVE.GFF through its directory (FMT-GFF-002)
environment: null
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006F982..0x0006F9C2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:07B1..38FF:094A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068E95..0x00068F24
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058CFD..0x00058D6F
---

## Observation

**The read.** Overlay 186 `+0282` (trampoline `56E9:0025`, FND-PARTY-012) takes a slot in `si`
and a number in `di`, and calls `38FF:04AB` with the tag `SPST`, the number and the address of a
far pointer to `4D62:039C + 15 * slot` (`+0290..+02B1`). `38FF:04AB` calls `38FF:07B1` with an
offset of 0 and a length of -1 (`38FF:04B9..38FF:04CD`), the complete transfer FND-CONFIG-151
reads. In `38FF:07B1`, a length of -1 is replaced by the resource's own length from its directory
entry (`38FF:08D7..38FF:08ED`), and when the far pointer it is given is not null the data goes to
that address with no buffer of its own (`38FF:0906..38FF:0946`).

**The callers.** `direct_callers.py` finds two calls of `+0282`:

- overlay 171 `+0B5A`, which passes as the number the word at `+2` of the entry of the 51-byte
  list at `DS:40C8` that the ADD window has selected (`+0B30..+0B5A`); FND-PARTY-012 reads that
  list as built from the `CACT` resources 1 to 39 that read and hold a nonzero word;
- overlay 182 `+06CF`, which passes `di + [bp-1]` with `di` set to `0x28` (40) at `+0645`, the
  load of `CHAR` records 40 to 43 when START GAME is chosen first (FND-PARTY-186).

**The data.** The installed `CHARSAVE.GFF` has `CACT` 29 to 39, each two bytes. Those of 29 to 35
hold 0, and those of 36 to 39 hold `0x8018`, `0x8017`, `0x8016` and `0x8015`. Its `SPST`
resources 30, 31 and 33 are 9 bytes and the other 16 (29, 32, 34 to 43 and 50 to 53) 15 bytes.
The ADD window in FND-PARTY-184's run lists four characters.

## Interpretation

When the game loads a 9-byte `SPST`, it writes the first 9 bytes of the slot's 15-byte entry
(spells 0 to 71, FMT-PARTY-005) and leaves bytes 9 to 14 as they were. With the shipped archive
this never happens: ADD offers only characters 36 to 39 and START GAME loads 40 to 43, all with
15-byte resources. A 9-byte resource reaches the game only when the transfer utility wrote it
(FND-PARTY-011) and gave the character a nonzero `CACT` word.

## Alternatives

- What bytes 9 to 14 of an entry hold before such a load, and so which spells from 72 to 119 a
  transferred character keeps, depends on the earlier writes to `4D62:039C`, which were not read.
- Other readers of `SPST` than `+0282` were not searched for; FND-PARTY-012 names `+0282` as the
  routine that reads the three resources into the table.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, in `tools/research/exec-census/`: `overlay_listing.py <dsun> 186 6F982
6F9F0`, `182 68E80 68F30` and `171 58CF0 58D70`; `direct_callers.py <dsun> 186+0282`. Disassemble
the load image (file offset of `S:O` is `(S - 0x1000) * 16 + O + 0x5200`) at `38FF:04AB` and
`38FF:07B1` to `38FF:0950`. Read the `CACT` and `SPST` tables of `CHARSAVE.GFF` and print each
resource's size and, for `CACT`, its bytes.
