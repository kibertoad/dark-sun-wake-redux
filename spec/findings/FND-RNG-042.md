---
id: FND-RNG-042
title: The fight set-up words add a dexterity table term and six effect terms to a d10, the next combatant to act is the one with the greatest first word, and only the fight start and script request 25 write the byte that spares a group the draws
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 27E5:02D9..27E5:02F2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 27E5:04A0..27E5:04F1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3143:000D..3143:0061
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D84C..0x0004D866
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008594B..0x0008599E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FE93..0x0007FF07
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B585..0x0005B5D1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B9B8..0x0005BA75
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008BF29..0x0008BF4E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C284..0x0008C294
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0008C487..0x0008C4F1
tool: tools/research/exec-census/resident_listing.py, overlay_listing.py, trampoline_target.py, segment_displacement_writes.py, field_reads.py and script_decode.py at commit 5017c4d1; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`. FND-RNG-041 has overlay 173 `+0C65` store, for
each slot it sets up, `r + f + g + 20` or `f + g + 30` in the word at `4C4F:00DC + 4 * c` and a
draw of 0 to 199, or 200, in the word after it, `c` being the slot's combatant.

**`f`, `27E5:02D9`.** It calls `27E5:04A0` with its slot and 1 and returns the signed byte at
`DS:084C` indexed by the result. `27E5:04A0` returns 9 when the slot is 9999 or more (unsigned),
when the slot's kind byte at `4F49:0C33 + 3 * slot` is not 2, or when the combatant word at
`4F49:0C34 + 3 * slot` is 9999 or more; otherwise it returns the byte at `+0x19` plus its second
argument of that combatant's 49-byte record, zero-extended. Neither tests the index against the
table's size. The 26 bytes at `DS:084C` (file `0x4D84C`) are 0, -6, -4, -3, -2, -1, 0 (6 to 15),
1, 2, 2, 3, 3, 4, 4, 4, 5, 5; the bytes after them are FND-PARTY-166's table at `DS:0866`.

**`g`, overlay 197 `+254B`** (trampoline `575A:0061`). It has overlay 193 `+1BA3` (trampoline
`573B:0025`) fill a 16-byte block on its stack for the slot and starts from 0. It subtracts 4
for bit 0 of block byte 14, subtracts 2 for bit 2 of byte 1, adds 2 for bit 2 of byte 5, adds 40
for bit 3 of byte 6, subtracts 2 for bit 3 of byte 11 and adds 50 for bit 7 of byte 12, and
returns the sum (`+2552..+2599`). `+1BA3` clears the block's first 15 bytes through `1000:3FA2`
and, for a slot below 48, sets bit `t & 7` of byte `t / 8` for the type byte `t` at
`4E71:0253 + 10 * e` of each effect entry `e` from `3143:000D` of the slot while the entry's word
at `4E71:024D + 10 * e` is the slot and `e` is below the word at `DS:2100` (`+1BAB..+1C11`), as
FND-COMBAT-036 also reads. The six bits are those of types 112, 10, 42, 51, 91 and 103.
`+254B` makes no draw and calls nothing else; `27E5:04A0`, `3143:000D`, a binary search over the
effect entries by their word `+0x24D`, and `1000:3FA2` call nothing, and `27E5:02D9` calls nothing
else, so none of them
draws.

**The reader of the words.** `field_reads.py <dsun> <inventory> --es --table 308 --table 3c4f dc
de` lists eleven reads of the two words through ES, all in overlay 173 and overlay 197. Six test
the first word against -1, 10 or 1 (`+06D5`, `+1227`, `+2F00`, `+3027`, `+3049` and overlay 197
`+0FA0`), among them FND-COMBAT-047's action two at `+3027` and `+3049`. The other five are in
overlay 173 `+1098`, which, after `+3304` returns a nonzero byte, goes through the slots 0 to 47
whose kind byte is 2 and keeps the slot whose combatant's first word is greater than the greatest
so far, or equal to it with a second word greater than the one kept, both compared as signed words
and starting from 0 and 0; it writes the kept combatant to its output word each time it keeps one
(`+10BC..+1152`). It returns 9999 when no slot is kept; what it does with a kept slot from
`+1155` on was not read.

**The byte at `4C4F:0006`.** `segment_displacement_writes.py <dsun> 4C4F 6` lists two writes. At
the start of `+0C65`, when the byte at `DS:054E` is 0, overlay 173 stores 0xFF in it, adds 60 to
the dword at `4C13:035B` and makes three calls; when that byte is not 0 it only clears it
(`+0C72..+0CA6`). Overlay 204 `+04C4` stores the low byte of its third parameter in it
(`+04C4..+04D1`). Overlay 204 `+0169`, the dispatcher of FND-RNG-027, sends request `q` from 1 to
53 through word `q - 1` of the table at its `+06C7` (file `0x8C487`); word 24 is `+04C4`, so
request 25 makes this store, and word 26 is `+04EF`, FND-RNG-022's request 27.

In `script_decode.py`'s full listing, 36 instructions `0x22` have the literal 25 as their first
parameter, all at the top level of `GPL` resources: `GPL/5` (5), 39, 66, 89, 91 (2), 97 (2), 108,
111, 119, 122, 126 (3), 139, 187, 234, 236, 270, 287, 291, 307, 311 (3), 313 (3), 318, 319 and
322 (1 each unless given). Their third parameters are the literals 0 (6 instructions), 1 (16), 2
(7), 7 (4) and 11 (3); FND-RNG-027's one instruction `0x22` with a variable request takes 10 to
13 only.

## Interpretation

The first word is the order in which combatants act: the greatest goes first, and the second word
breaks a tie. `f` is a dexterity term (the table runs from -6 at a score of 1 to 5 at 24 and 25,
with 0 for 6 to 15), and `g` adds 2, 40 or 50 for effects 42 (`Hasted`), 51 (`Hasted`) and 103
(`Time Stopped`) and takes 2, 2 or 4 off for effects 10 (`Blind`), 91 (`Slowed`) and 112
(`Hesitating`), reading the block's bits as FND-COMBAT-029's status panel reads them, by effect
number, and the names from FND-COMBAT-027. A script sets the byte at `4C4F:0006` through request
25 before a fight it starts; with 0, 1 or 2, the combatants of the groups the mask 0x71, 0x184 or
0x608 names take one above the greatest a roll gives and the greatest second word, so they act before the
others of the same `f` and `g`. The fight start resets the byte to 0xFF, which matches none.

## Alternatives

- Reading the first word as action order rests on `+1098`'s comparison; what `+1098`'s caller
  does with its result, and whether a combatant that has acted gets a first word of -1 there, was
  not read. FND-COMBAT-047's action two also changes the first word.
- `27E5:04A0` with 1 reads byte `+0x1A` of the record; that this is the dexterity score rests on
  FND-PARTY-166's reading of the tables beside it.
- Which groups the masks name, what `DS:054E` stands for and who sets it, what the values 7 and 11
  of request 25 do (no mask of `+0C65` matches them, so they act as 0xFF does), and why the
  shipped scripts pass them, were not read.
- The write search lists ES stores with an immediate displacement only; stores through a base or
  index register, string instructions and block copies are excluded. The read search keeps hits
  with the segment word within six instructions.
- The overlays are outside the resident harness; no emulated call or run was made.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`: `resident_listing.py <dsun>
27E5:02A7..27E5:030B`, `27E5:04A0..27E5:0560` and `3143:000D..3143:00A8`; print the 26 bytes at file `0x4D84C`;
`trampoline_target.py <dsun> 575A:0061 575A:0089 573B:0025 56A7:0075`; `overlay_listing.py <dsun>`
for 197 `83400 86F67`, 193 `7E2F0 80ECE`, 173 `5A920 5EA7C` and 204 `8BDC0 8E800`, reading the
ranges above; print the 53 words at file `0x8C487`; `segment_displacement_writes.py <dsun> 4C4F 6`;
`field_reads.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv --es --table 308 --table 3c4f dc de`;
and `script_decode.py <install>/GPLDATA.GFF <install>/OBJEX.GFF --all`, keeping the lines with
`22 25,`. Output is kept under `GAME_DIR/analysis/rng-callers-20261020/`. Nothing was executed.
