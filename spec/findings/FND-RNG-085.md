---
id: FND-RNG-085
title: Three instructions store the slot word +6 of the placed-object entries through the far pointer DS:67B7 directly, none of them 9999 except 31E0:1127 with a slot argument of 9999, and only four calls of 31E0:0121 pass a slot not shown to be below 9999
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2F34..2D40:2FF4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0EFF..31E0:112B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070586..0x000705E7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000709D7..0x00070AE7
tool: Capstone 5.0.9 over the load image and overlay code; tools/research/exec-census/immediate_search.py, direct_callers.py and overlay_listing.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Addresses are static; segment immediates in load-image code are relative to 0, so `0x1000` is
added. The far pointer `DS:67B7` names the 8-byte entries of the placed objects; an entry's word
`+6` holds a slot, or a negated object number (FND-ACTOR-004). `31E0:2C86` reads it as the slot
`s` (FND-RNG-072).

**The direct stores.** Among the hits of `immediate_search.py <dsun> 67B7`, the 32 decodings
`les bx, [0x67b7]` and `les di, [0x67b7]` in the load image and overlay code were each followed for
25 instructions, up to a reload of `ES`, a transfer or a return. Three of them store a word at
`ES:[bx + 6]`, each after adding 8 times a record's word `+0x01` to `BX`:

| Site | Value stored |
|---|---|
| `2D40:2FF0`, in `2D40:2F34` | the slot `[bp - 2]` that `2D40:2F34` took from its call at `2D40:2F87`; the routine leaves at `2D40:2F93` when that slot is 9999 |
| `31E0:1127`, in `31E0:0EFF` | the routine's slot argument `SI`, which is 319 when it was -1; on the path for 319 (`31E0:1065..10E2`) `SI` is the result of `2D40:0461`, and the routine leaves when that is 9999 three times |
| overlay 187 `+07B3` (file `0x705E3`) | the negated absolute value of the record's word `+0x1B`, a negative number or `0x8000` |

The other hits store only bits of the byte `+5`.

**The slot argument of `31E0:0EFF`.** At `31E0:0F51` the routine skips the code for seven object
numbers when the slot is 9999 and goes on to `31E0:0FFA`, so a slot of 9999 reaches the store at
`31E0:1127` when the record's word `+0x01` at `DS:67BC + 37 × 9999` is not -1 (`31E0:1045`).
`direct_callers.py` finds five calls of `31E0:0EFF` and 21 of `31E0:0121`, which passes its second
argument as the slot (FND-ACTOR-004). The slot argument is:

| Call | Slot |
|---|---|
| `2D40:069A`, `31BA:0137`, overlay 182 `+094B` | -1 |
| `31E0:0170` (in `31E0:0121`) | `31E0:0121`'s second argument |
| overlay 187 `+0C3A` | the loop counter `SI`, from 0 below `0x140` (`+0BA7`, `+0CAE`) |
| `31E0:038A`; overlay 173 `+3237`; overlay 182 `+0A31` and `+1363`; overlay 188 `+06C5` and `+0CAB`; overlay 191 `+066B`; overlay 193 `+022D`, `+0302`, `+2132` and `+2A92`; overlay 204 `+138C` and `+185F`; overlay 206 `+01C1` and `+10C3` | -1 |
| overlay 197 `+34CB` | 4 |
| overlay 182 `+06FF` | the byte `[bp - 1]`, zero-extended |
| overlay 171 `+0C25` | the word at offset `0x0B44` of the segment named by the fixup word `0x0370` |
| overlay 179 `+26DE` | `DI` |
| overlay 182 `+04E0` | `SI`, a loop counter set to 0 at `+045C` |
| overlay 184 `+12E5` | `SI` |

In the last four rows the same register or word also picks the 49-byte record at the far pointer
`DS:19C9` whose word `+0x10` plus 300 is passed as the object number.

## Interpretation

A slot of 9999 can reach an entry's word `+6` directly only through `31E0:0EFF` called with slot
9999, and of its callers only the last four rows of the table can pass a value that is not shown
to be below 9999. In the game those four pick a party member's record, which suggests a small
index, but their bounds were not read.

## Alternatives

The search covers only stores right after a load of the pointer from `DS:67B7`. Stores through
a copy of the pointer (the hits that move or push `DS:67B7`'s words), block copies and file reads
into the entries (`SAVE` resource 1's neighbour, FND-SAVE-015, and the clear at
`1000:3FA2`), and stores more than 25 instructions after the load are not covered.

## How to reproduce

`immediate_search.py <dsun> 67B7`; disassemble 25 instructions after each `les` hit;
`direct_callers.py <dsun> 31E0:0EFF 31E0:0121` and the 48 bytes before each call; disassemble
`2D40:2F34..2FF4` and `31E0:0EFF..112B` from the load image; `overlay_listing.py <dsun> 187 70586
705E7` and `... 187 709D7 70AE7`.
