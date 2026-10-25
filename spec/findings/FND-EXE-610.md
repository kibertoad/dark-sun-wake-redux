---
id: FND-EXE-610
title: The 4 routines of overlay 184 that no entry placed compute a new character's psionic points from constitution, intelligence, wisdom and psionicist level, load and free a run of resources into an array of far pointers, and clear a slot's items or move its worn items that overlay 189 +2B64 refuses
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006D75D..0x0006D868
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E5F2..0x0006E696
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006F197..0x0006F3B0
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 4 rows of overlay 184 (header segment `56DD`, 20
trampolines, code from file offset `0x6D090`, 8,992 bytes) that no spec location covered, 968
bytes in all. Each row below is one row of the inventory; "offset" is the overlay code offset, the
file offset is `0x6D090` plus it, and `DS` is `57E0`. Every routine has a trampoline in `56DD`.
Callers are the hits of `direct_callers.py`, checked against the instructions around them, named
by the row that holds them. Overlay 184 is character generation (FND-PARTY-071). During
generation the far pointers `DS:1429` and `DS:142D` name the 66-byte details record and the
49-byte combatant record (FND-PARTY-059). In the details record the bytes `+0x1B` to `+0x1D` are
the three classes, in generation codes 1 to 8 of `DS:1164`, where 6 is Psionicist
(FND-PARTY-073), the bytes `+0x1E` to `+0x20` their levels, and the word `+0x0C` the greatest
psionic points (FND-PARTY-059). The combatant record keeps the current scores at `+0x19` to
`+0x1E`, so `+0x1B` is constitution, `+0x1C` intelligence and `+0x1D` wisdom (FND-PARTY-161). A
slot's held objects are the 10-byte entries `2D40:2196` lists, with the kind in word `+2` and the
item in word `+4` (fn_2D40_2196), and an item's 23-byte record under `DS:19C1` has its position
byte at `+0x13` (FND-PARTY-217).

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+06CD` | `0x6D75D..0x6D868` | `56DD:006B` | Takes no argument. Sets the details word `+0x0C` to 0 and adds the signed byte at `4E4F:00EF` plus the constitution when that is 15 or more, the byte at `4E4F:00F7` plus the intelligence when that is 15 or more, and the byte at `4E4F:00FF` plus the wisdom when that is 15 or more. Sets `DS:42C4` to 0. When one of the three class bytes is 6 it stores in `DS:42C4` the level of the first such class, adds `(DS:42C4 - 1) * 10`, and when the wisdom is above 15 adds the byte at `4E4F:0107` plus the wisdom times `DS:42C4 - 1`. Called from the roll routine `+033E` at `+05C0` (FND-PARTY-075) and from overlay 183 `+0769`. |
| `+1562` | `0x6E5F2..0x6E63C` | `56DD:0070` | Takes a tag (a double word), a near array of far pointers, a first number and a last number. For each number from the first to the last it loads that resource of the tag through `38FF:04AB` into the next far pointer of the array, stopping at the first nonzero return. Returns 0. Called from `+0AE2` with `BMP `, `DS:11C8`, 20048 and 20058 when the first pointer at `DS:11C8` is 0. |
| `+15AC` | `0x6E63C..0x6E696` | `56DD:0075` | Takes a near array of far pointers, a first number and a last number. Frees each pointer of the run that is not null through `444C:0092` and stores the result in it. Returns 0. Called from `+11F4` with `DS:11C8`, 20048 and 20058. |
| `+2107` | `0x6F197..0x6F3B0` | `56DD:007F` | Takes a slot `s`. When byte `+0x14` of `s`'s 49-byte record is 0, it lists `s`'s objects and removes each entry of kind 1 through `2D40:2452`, listing again after each removal and going on from entry 1. It calls `+1EB9` with `s` when `DS:112E` is -1 or byte `+0x14` is 0. It lists `s`'s objects again and returns when none is of kind 1. Otherwise, for each entry of kind 1 whose record's position byte plus 4 is below 18 (unsigned) and for which overlay 189 `+2B64` with the item, 0 and `s`'s 66-byte record returns 0, it asks overlay 191 `+06B3` for a place for `s`. With a place it moves the entry there within `s`'s list through `2D40:288C` (the list, the entry, the place, the list, 0 and 4). With none, when overlay 191 `+0000` returns nonzero, it calls overlay 191 `+05DB` with the position words `DS:67C5` and `DS:67C7` of the slot in `4C13:0369` and 0 when `28C9:2522` returns nonzero, or removes entry `DS:1A34` of the list at `5072:0040` through `2D40:2452` otherwise; then it fills `5072:0040` for `DS:1A32`, moves the entry into it through `2D40:288C` with -1, 0 and -1, and calls overlay 191 `+074B` with 0. After either move it lists `s`'s objects again and goes on from entry 1. Called from `+125F` and from overlay 209 `+1998`. |

## Interpretation

`+06CD` is the generation form of overlay 210 `+0131` (FND-PARTY-081): the same bonuses for
scores of 15 or more, plus 10 per psionicist level above the first and the wisdom term per level,
with two differences. It sets the greatest psionic points outright, and its wisdom test for the
per-level term is above 15, where `+0131` tests 15 or more. A character with no psionicist class
gets only the score bonuses. `+1562` and `+15AC` load and free the eleven `BMP` resources 20048
to 20058 the generation screen uses. `+2107` is called from overlay 209, whose DUAL path changes a
character's class (FND-PARTY-080), and from generation code at `+125F`: it can clear a slot's
items, and it takes each worn item, a position from -4 to 13, that overlay 189 `+2B64` refuses
for the character's records and moves it to the place overlay 191 `+06B3` gives, or else hands it
to overlay 191's routines. Resuming at entry 1 after each change leaves entry 0 of the new
list untested. The names stay `fn_184_<offset>` until a reading shows what the fields and tables
are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list. Overlay 189 `+2B64`, overlay 191 `+06B3`, `+0000`, `+05DB` and
`+074B`, `28C9:2522`, `2D40:2452`, `2D40:288C` and `+1EB9` are not read here; the reading of them
as a use test, a free place, a drop at the slot's position and a removal rests on their
arguments and on fn_2D40_288C. That positions below 14 are worn places is inferred from the bound
of 18 on the position plus 4. What byte `+0x14` of the combatant record and `DS:112E` select in
`+2107` is not settled.

## How to reproduce

Run `overlay_listing.py` for overlay 184 against the installed `DSUN.EXE` over each file range in
the table and `direct_callers.py` for each offset (`184+<offset>`), read `+0AC8..+0AEA` and
`+11E0..+11FA` for the arguments of `+1562` and `+15AC`, and compare the ranges with the rows of
`coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`.
