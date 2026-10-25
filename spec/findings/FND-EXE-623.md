---
id: FND-EXE-623
title: The 3 routines of overlay 200 that no entry placed link a chain of object slots in after another slot, request the image of every placed entry with bit 3 of byte 5 set, and release the buffers and screen handles of the overlay's initializer
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00089160..0x00089242
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000895CF..0x00089725
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 3 rows of overlay 200 (header segment `5773`, 8
trampolines, code from file offset `0x89160`, 1,550 bytes) that no spec location covered, 568
bytes in all. Each row below is one row of the inventory; "offset" is the overlay code offset, the
file offset is `0x89160` plus it, and `DS` is `57E0`. Every routine has a trampoline in `5773`.
Callers are the hits of `direct_callers.py`, checked against the instructions around them, named
by the row that holds them. A placed entry is one of the 8-byte entries under the far pointer
`DS:67B7`, `DS:264E` of them in use, with its object slot in word `+6`, negative for an object
that has no slot yet (FND-COMBAT-054). A slot's 37-byte record is at `DS:67BB + 37 * slot`, and
its word `+1` numbers its placed entry, -1 for none.

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+0000` | `0x89160..0x891DE` | `5773:0043` | Takes slots `a` and `b` and returns at once unless both are 0 to `0x13F`. From `b` it follows the records' word `+0x21` while it is not -1 and the record's word `+1` is not -1, and returns when the record it stops on, `e`, has word `+1` -1. Otherwise it copies `a`'s word `+0x21` to `e`'s word `+0x21`; when that word is not -1 it stores in word `+0x23` of the slot it names the word `+6` of `e`'s placed entry. Then it stores `b` in `a`'s word `+0x21` and `a` in `b`'s word `+0x23`. Called from one resident far call at `31E0:03A2`, which passes the slot in `4C13:0367` as `a` and the slot `31E0:0121` has just placed as `b`, and from overlay 179 `+1ADF` and overlay 182 `+0A42` and `+1377`. |
| `+007E` | `0x891DE..0x89242` | `5773:003E` | For each placed entry with bit 3 (`0x08`) of byte `+5` set, takes an image number: for a negative word `+6`, what `31E0:426E` returns for its negation, the image number of that `OJFF` (FND-ACTOR-004), stopping when that is -1; otherwise word `+0x19` of the slot's record. Calls `31E0:3568`, the `BMP` and `CBMP` cache request (FND-IMAGE-006), with the number and 0. Called from overlay 182 `+130A` and `+1714`. |
| `+046F` | `0x895CF..0x89725` | `5773:002A` | The cleanup of FND-CONFIG-184, which returns at once while the byte `DS:265C` is 0. For each placed entry with a slot whose record word `+0x16` is above 0, it stores in that word the negation of the word `DS:6578 + 9 * w`, `w` the old value. It frees the far pointers `DS:2652` and `DS:6573` and the 64 far pointers of the 9-byte rows from `DS:657A` through `444C:0092`, storing what it returns in each; clears bit 1 of the byte `52A2:0CB6 + 16 * i` for `i` below the word `DS:265D`; releases each of the words `DS:9BFC`, `DS:9BF8` and `DS:9BFA` that is not -1 through `2D40:3BD1` and sets it to -1; fills the 576 bytes from `DS:6577` with 0 through `1000:3FA2`; and stores 0 in `DS:265C`. Called from the initializer `+0386` (FND-CONFIG-183) at `+045F`, from the resident far call at `31E0:3D0A` in the routine at `31E0:3C02`, which a far call of trampoline `5773:0039` follows, and from overlay 182 `+15C5`, `+1683` and `+194B`. |

## Interpretation

`+0000` keeps a doubly linked chain of object slots through record words `+0x21` (the next slot)
and `+0x23` (the slot before): it walks from `b` to the last slot of `b`'s chain and splices the
whole chain in after `a`. The resident call links a just-placed object after the record in
`4C13:0367`, which holds the record a running script was started for (FND-PARTY-098). `+007E` brings the images of the flagged placed objects into the image cache. `+046F`
undoes `+0386`: it frees the two buffers and the 64 rows it allocated, gives back the three
handles `1BF3:27A8` returned, and leaves the byte `DS:265C` at 0 so that `+0386` sets the area
up again. The names
stay `fn_200_<offset>` until a reading shows what the fields and tables are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list. `31E0:426E`, `31E0:3568` and `2D40:3BD1` are read only as far as
the cited entries go, and overlay 179 `+1ADF` and overlay 182 `+0A42`, `+1377`, `+130A`, `+1714`,
`+15C5`, `+1683` and `+194B` are not read here, so what `a` and `b` stand for on those calls is
open. Word `+0x23` of the slot after `a` gets the slot of `e`'s placed entry, which is `e` only
when that entry still names `e`; the reading of `+0x21` and `+0x23` as the two links of one
chain rests on these stores alone. What bit 3 of byte `+5` of a placed entry means, and what the
rows at `DS:6577` hold, is not settled here.

## How to reproduce

Run `overlay_listing.py` for overlay 200 against the installed `DSUN.EXE` over each file range in
the table and `direct_callers.py` for each offset (`200+<offset>`), and compare the ranges with the
rows of `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`. Run `resident_listing.py` over
`31E0:034C..31E0:03AA` for the resident call of `+0000`.
