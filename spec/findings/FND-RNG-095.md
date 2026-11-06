---
id: FND-RNG-095
title: Known cursor selector publications and six caller bindings use a 73-byte prefix copy
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00092EA2..0x00092F2E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005A26C..0x0005A27A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000754D6..0x000754F1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00096E93..0x00096EBE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009753B..0x00097551
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000976C6..0x000976DF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00099DC4..0x00099DE7
tool: overlay_listing.py and resident_listing.py at b3d9442f, Python 3.12.10, Capstone 5.0.9 and xxhash 4.0.1 for the copy-count correction; retained FND-RNG-064 caller and operand observations
environment: null
---

## Observation

This supersedes FND-RNG-064's copy-count wording. At 208+1631, CX
receives 0049 before the call of 1000:0452. That primitive counts
bytes, copying 36 words and one trailing byte here, not 73 words.
Thus the own destination is DS:9B86..9BCF, half-open, under admitted
DS/storage and normal copy continuation. It ends before the subsequent
local pointer fields at DS:9BCF/9BD1 and selector fields. FND-PARTY-299
independently verifies the primitive's odd-count branch. All other
observations below are retained with their original bounded exclusions.

The fresh setup report is word-selector208-setup.txt under
GAME_DIR/analysis/handler-census-20261106/. Reproduce with
overlay_listing.py descriptor 208 file 92EA2..92F2E and
resident_listing.py 1000:0452..1000:046E. No original game ran.

Overlay 208 `+1612` (trampoline `57A6:0066`, code from file offset `0x91890`) takes five
arguments. It stores the first (`bp+6`) at `DS:9BE0`, the second (`bp+8`) at `DS:9BD4`, the third
(`bp+0xA`) at `DS:9BE2`, the low byte of the fourth (`bp+0xC`) at `DS:9BD3` and the low byte of
the fifth (`bp+0xE`) at `DS:9BE5` (`+164F..+166F`), and before that copies 0x49 bytes to
`DS:9B86` from the far pointer that resident `2C5F:0003` returns for the third.

A direct-operand search of every inventory row finds one store to `DS:9BD3` (`+1662`) and one to
`DS:9BE2` (`+1655`), both in `+1612`; the other accesses read them. A byte search of the whole
file for the far call `9A 66 00 80 06` finds six sites, and for the load-image form with segment
word `0x47A6` none:

| Call | Number (third) | Byte (fourth) | Fifth |
|---|---|---|---|
| overlay 172 `+0E45` | its `SI` | 0 | overlay 211 `+1FB5`'s result |
| overlay 189 `+0B3C` | `DS:43F5`, when below `0x148` (`+0B26`) | 1 | 6 |
| overlay 211 `+10A9` | `DS:43F5`, when below `0x148` (`+1083`) | 0 | `+1FB5`'s result |
| overlay 211 `+173C` | its `SI` | 0 | `+1FB5`'s result |
| overlay 211 `+18CA` | 306 | 0 | `+1FB5`'s result |
| overlay 213 `+0B82` | `[bp-4]`, when below `0x148` signed (`+0B64`) | 1 | 6 |

The search for stores to `DS:43F5` finds two: overlay 211 `+16ED` from its `SI` and overlay 213
`+0AE2` from its `[bp-4]`, the item's word `0x10` less 1 that FND-PARTY-243 reads.

## Interpretation

The byte that overlay 208 `+179F` passes to the spell dispatcher as `bp+0x1A` (FND-RNG-055) is 1
only when the cast was set up by overlay 189 `+0B3C` or overlay 213 `+0B82`, and then the number
is `DS:43F5` or an item's word `0x10` less 1. Either is 293 only if `DS:43F5` gets 293, which
FND-PARTY-243 rules out for modes 1 to 3 and for overlay 213's store unless an item's word
`0x10` is 294, or if an item holds 294 in word `0x10`, which no shipped item does (FND-RNG-055).
The callers that pass 0 can pass any number without setting the byte.

## Alternatives

A store to `DS:9BD3` or `DS:9BE2` through a register or a computed address is outside the
direct-operand search; the routine's own stores use direct operands, and no other direct one
exists. A far call built at run time to `57A6:0066` is outside the byte search. Words written
into an item's word `0x10` at run time were not followed here, so the item route rests on the
shipped files only.

## How to reproduce

`overlay_listing.py <dsun> 208 92EA2 92F2E`, then the six call sites: `172 5A26C 5A27A`,
`189 754D6 754F1`, `211 96E93 96EBE`, `211 9753B 97551`, `211 976C6 976DF` and
`213 99DC4 99DE7`. Search the file for the bytes `9A 66 00 80 06` and `9A 66 00 A6 47`, and
disassemble each inventory row for operands `[0x9bd3]`, `[0x9be2]` and `[0x43f5]`.
