---
id: FND-PARTY-257
title: Overlay 173 reads word 0x10 of a 23-byte item record as one more than the DATA number of an effect cast at the target after a damaging swing, ammunition's first and then the weapon's, and also as a nonzero test that stops item breakage and adds 10 to an item's score
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005AE1E..0x0005AE51
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D1F7..0x0005D2F5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D3BA..0x0005D4A8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005E3F8..0x0005E41A
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding, and a direct-operand search over the inventory rows; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

Overlay 173's code starts at file offset `0x5A920`; offsets are overlay offsets, and an item's
23-byte record is entry `i` under the far pointer `DS:19C1` (FMT-PARTY-006). FND-PARTY-248 lists
five reads of its word `0x10` in overlay 173, at `+0511`, `+28ED`, `+2924`, `+2B45` and `+3AF5`.

**`+0511`**, in the item score `+0444` of FND-EXE-600 (`+04FE..+0531`). When the item's signed
byte `0x16` is 0 or less and its word `0x10` is 0, the routine adds 10 to its sum only when bit 2
of byte `+4` of the item's 15-byte type record is set. When byte `0x16` is above 0 or word `0x10`
is not 0, it adds 10 without that test. The word is used only as a zero test here.

**`+3AF5`**, in the item break `+3A9F` of FND-RNG-041 (`+3AD8..+3AFA`). A nonzero word `0x10`
jumps to the first draw with the break chance at 0, as a set bit 7 of the type byte `+4` or a
nonzero byte `0x16` does, so an item whose word `0x10` is not 0 never breaks.

**`+2B45`**, in the ammunition lookup `+2A9A` of FND-EXE-600 (`+2B3A..+2B52`). For each kind-1
entry of the item list whose 23-byte record has 1 in byte `0x13`, it stores that record's byte
`0x16` at `4C4F:0009` and its word `0x10` at `4C4F:0007`, and returns the record's word `+2`,
which it alone tests. `+2A9A` writes `4C4F:0009` with 0 on entry and never writes `4C4F:0007`
except at `+2B4E`.

A search of every inventory row for an instruction with the direct operand `[0x0007]` or
`[0x0009]` and, within the eight instructions before it, an immediate naming segment `4C4F`
(`0x308` in overlay code, `0x3C4F` in the load image) finds, for `0x0007`, the store at
`+2B4E`, three reads and a store of 0 in `+2560` and nothing else; for `0x0009`, the two stores
of `+2A9A` (the positive control) and reads in `+1CC1` and `+2560`.

**`+28ED` and `+2924`**, in the swing `+2560` of FND-PARTY-091. `+2560` calls `+2A9A` with the
weapon, its type and flag 0 at `+26F3` and with flag 1 at `+2762`. When the swing's damage word
`bp-4` is above 0 and the weapon number `w` is 0 or more, it runs (`+28DE..+29D2`):

1. Clears the byte `bp-0x19`. If word `0x10` of `w` is 0 and `4C4F:0007` is 0, it ends here.
2. Takes `v` as `4C4F:0007` when that is not 0, and as word `0x10` of `w` otherwise, and `d = v -
   1`. When `d` is `0x148` (328) or more, signed, it goes to step 5.
3. Reads the `DATA` record `d` through `2C5F:0003`. When the low two bits of its byte `+0` are 1
   and byte `0x16` of `w` is 0 or more, signed, it goes to step 5.
4. Calls overlay 190 `+2448` with the attacker, 3 and `w`; overlay 193 `+2290` with the attacker,
   the target slot `DS:4206`, `d`, 6, the target's position words `+0x0A` and `+0x0C` of its
   37-byte record at `DS:67BB` twice, the far address of `bp-0x19`, 1 and 1; then overlay 190
   `+2503` with 2.
5. When `4C4F:0007` is not 0, stores 0 there and goes back to step 1.

Overlay 193 `+2290` takes `d` as a `DATA` number: it returns 0 for `d` of 328 or more and
otherwise reaches the spell and power routines (FND-PARTY-108). Overlay 190 `+2448` records an
item action of kind 3 with the member and the item, and `+2503` with 2 ends it, decrementing byte
`0x0E` of the item unless bit 1 of byte `0x0B` of its type record is set (FND-PARTY-243,
FND-PARTY-248). FND-PARTY-196 gives the shipped values of the word as 0 to 327.

## Interpretation

Word `0x10` holds 0 or one more than the number of a `DATA` record, a spell or power the item
casts on the target of every swing that does damage, as overlay 193 `+2290` would cast it. A hit
with ammunition casts the ammunition's effect first and then the weapon's own. An item with such
an effect never breaks and counts as worth 10 more in `+0444`'s choice of item. A `DATA` record
whose byte `+0` has 1 in its low two bits is cast only from an item whose byte `0x16` is negative.
Each cast is wrapped in an item action, so it uses up a charge of the item unless its type has
bit 1 of byte `0x0B` set.

`4C4F:0007` is written only by `+2A9A` and cleared only after its effect is cast, and `+2A9A` is
also called by `+0444`, `+1CC1` and resident `28C9:254C`. A value it stored on a path that did not
reach step 5 would stay for the next damaging swing, which would then cast it even with a
weapon that uses no ammunition (Q-PARTY-114).

## Alternatives

The search for `4C4F:0007` accepts only a segment named within eight instructions; an access
whose segment was loaded further back, or code outside the inventory rows, would be missed. The
reading of byte `+0` of a `DATA` record as a kind with value 1 rests on this test alone. That the
cast reaches the target as a spell rests on FND-PARTY-108's reading of `+2290`, not on a reading
of every routine it reaches.

## How to reproduce

Run `overlay_listing.py <dsun> 173` over `0x5AE1E..0x5AE51`, `0x5CE80..0x5D37D`,
`0x5D3BA..0x5D4A8` and `0x5E3BF..0x5E4D7`, and `direct_callers.py <dsun> 173+2A9A 173+2560
173+3A9F 173+0444`. For the operand search, decode each range of each row of
`coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv` and keep each instruction with a memory operand without
base or index register whose displacement is 7 (then 9), with `mov ax, 0x308`, `mov dx, 0x308`
or an immediate `0x3C4F` among the eight instructions before it.
