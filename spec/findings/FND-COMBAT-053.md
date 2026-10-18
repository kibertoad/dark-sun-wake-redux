---
id: FND-COMBAT-053
title: Nothing from program start to START GAME's party load writes the effect count or rows, no writer sets the count to a constant, and the new-game routes to the effect writers start at the gate routine's later calls
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: claude
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069B4B..0x00069D75
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000800A0..0x00080140
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FB63..0x0007FBD3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:044C..31E0:049A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0ABC..31E0:0ACC
tool: tools/research/exec-census/immediate_search.py, segment_references.py, reach_config.py, overlay_listing.py and resident_listing.py at fb337fd3; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1; scientific-method-engine 19.9.0 reach
environment: null
---

## Observation

The installed DSUN.EXE is the input (size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b). Overlay offsets are relative to
each overlay's code: overlay 182 starts at file 68850, 187 at
6FE30, 193 at 7E2F0 and 194 at 81130 (hexadecimal throughout).

### Count writers

An encoded-operand search for the word 2100, the effect count
`DS:2100` (FND-COMBAT-031), over the load image and every overlay
returns 50 instructions. Five write the word: `inc` at overlay 193
+1749 and +23C5 and `dec` at +1933, +19DC and +2383, the insertion,
removal and transfer sites FND-COMBAT-031, FND-COMBAT-032 and
FND-COMBAT-033 read. The other 45 compare or load it, apart from two
`shl byte [bx+di+2100]` decodes at 314D:0001 and overlay 193 +1C58,
each starting one byte inside a following `mov ax, [2100]`. No
candidate stores a constant into the count. The insertion's increment
at +1749 is the positive control. The numbered restore's request for
`SAVE/26` (FND-COMBAT-035) writes the count through a far pointer and
is not an operand hit.

### Row-store candidates

The row table is 4E71:024D, ten bytes per row (FND-COMBAT-031). Of the
1036 relocated references to segment 4E71 (descriptor 110), 32
instructions in the decoded context after the reference store through
ES at a displacement from 024D to 09E1, the 192 rows and the boundary
row. Their file offsets are 1CA64, 25DE5, 58E75, 58E87, 58EA0, 58FE6,
59005, 6180C, 61A6D, 61B1F, 61BA6, 61BD8, 65993, 65C5D, 65C70, 65C83,
7FA4A, 7FA59, 7FA6B, 7FA7D, 7FA8F, 7FAC1, 7FDF3, 7FE54, 800FD, 806DC,
81CC8, 8230A, 82319, 8B0DC, 936B6 and 99A57. The insertion's six field
stores (7FA4A to 7FAC1, FND-COMBAT-031) are the positive control.
Several of the others use displacements of other fields indexed by a
register (for example `bx + 9CD` and `bx + 8E2`); this list does not
say which of them address a row.

### Reach from program start to the party load

The 40 starts, two leaves and two controls of FND-PARTY-037 (program
start, the start window, the Start Game branch's calls and the gate
routine overlay 182 +1167's calls up to its +12FF) were walked with 42
targets: the five count writers, the 32 row-store candidates, overlay
193 +166F (the checked insertion wrapper), +16F6 (the insertion),
+192F, and overlay 187 +0A6C and +0EBE (the numbered restore and its
wrapper). Both controls are reached and resolved. The walk reads the
same 701 routines and 43,816 instructions as FND-PARTY-037, with the
same 61 unresolved transfers. No target is reached.

### Reach from the start window's callback

A walk from overlay 194 +0000, the start window's event callback
(FND-UI-035), with the same targets reaches 24 of them: the five count
writers, the wrapper and insertion, the insertion's six field stores,
and 6180C, 65993, 65C5D, 65C70, 65C83, 7FDF3, 7FE54, 800FD, 806DC,
8230A and 82319. Every read route to each of the 24 passes overlay 182
+1167. Overlay 187 +0A6C and +0EBE are not reached.

### Reach from the gate routine's later calls

From +12FF to its return the gate routine calls, in order: overlay 182
+0365 (with flags from the placed-object count `DS:264E`), +08C8,
overlay 200 +007E, 2C5F:018C, overlay 182 +1F1D when the byte
`DS:13FA` is 0, 31E0:0121 when the byte `DS:068E` is nonzero, overlay
200 +0000 when 31E0:0121 returns other than -1, 4611:0051 in a loop
when the bytes `DS:13F7` and `DS:14E3` are nonzero,
1BF3:4FEB, 1BF3:4723 twice, 3D72:0D83, 444C:0008, 3796:0514,
444C:0092, 2C5F:0139, 2C5F:0C8D, 3D72:0D83, 28C9:2A44 and overlay 187
+2496; it then stores 1 to `DS:0DAB` and calls 1695:000B when the
placed-object count was at most 4, 1695:01D1
for each party slot whose state byte at 4F49:0C33 is 2, 3796:0387 five
times, 1000:3FA2 and overlay 187 +2496 again. A walk from these 22
routines, with no leaves, reaches the same 24 targets, each from
overlay 182 +0365, +08C8, +1F1D, 2C5F:018C, 31E0:0121, 2C5F:0139,
1695:000B and 1695:01D1, and leaves 41 transfers unresolved. Overlay
187 +0A6C and +0EBE are not reached.

### Removal of an owner's rows

Overlay 193 +1DB0 takes an owner, finds its first row through
3143:000D and, while the row at the index has that owner and the
index is below the count, clears bit 0 of a byte at `4E71:08E2` plus
34 times the row's second word plus its `DATA` word when that word is
at least 235 and the second word below 4, then calls +1873 with the
owner and the row's key. +1873 finds the owner's row with that key,
calls overlay 195 +1000 with the owner and the row's effect byte, then
removes the row through +18E3 and, when that succeeds, calls overlay
197 +1E7B with the owner. +1DB0 advances its index only when +1873
returns 0, so it removes every row of the owner that +1873 finds.

31E0:044C, the object unload's chunk-deletion routine (FND-PARTY-213),
calls +1DB0 through trampoline 573B:002F with its slot, then 371D:0002,
when the byte `DS:265B` is nonzero, the slot's state byte at
4F49:0C33 is 2 and its second argument is nonzero. The unload
31E0:05BB passes its own second argument there at 31E0:0AC9; the
defeat cleanup loop passes 1 (FND-PARTY-213).

## Interpretation

At program start the effect count is the load image's zero
(FND-COMBAT-031), and no read route from program start through the
start window and START GAME up to the gate routine's party load writes
the count or a row. The count is never reset to a constant: its only
operand writers add or subtract one, and its only bulk writer, the
numbered restore, is reached from neither the start window's callback
nor the gate routine's later calls. On a new game the first possible
changes come through the insertion, removal and transfer routines,
reached only from calls the gate routine makes from +12FF on.

A game started after an earlier one in the same run therefore keeps
the earlier game's rows and count, except for rows those routines take
out. The defeat cleanup's unload calls the owner removal for each
party slot when `DS:265B` is nonzero; rows of owners that are not
party slots are not removed at that call.

## Alternatives

- The walks list routes a decoder can read, not routes that run: which
  of the gate routine's later calls reach an effect writer on a new
  game, and with what arguments, is not read here.
- The negative from program start rests on the 61 unresolved
  transfers FND-PARTY-031 and FND-PARTY-037 list; a target reached
  only through one of them is not excluded. The two leaves rest on
  `DS:0DAB` being 0 (FND-PARTY-124), which holds for the first game.
- Stores through ES set other than from a relocated immediate, string
  and block copies with computed destinations, and stores through a
  far pointer loaded from memory are not candidates of the row search.
  The numbered restore is the one bulk writer of rows and count named
  here (FND-COMBAT-035).
- Whether 31E0:0AC9 is reached on the defeat path, and the value of
  `DS:265B` there, is not read here; FND-PARTY-213 reads the deletion
  call 31E0:054F under the same `DS:265B` gate.
- Whether overlay 195 +1000's handler can insert a row while +1DB0
  removes the owner's rows is not read; a walk reaches the insertion
  wrapper from +1873.

## How to reproduce

In `tools/research/exec-census/`, with the locked evidence Python:
`immediate_search.py <dsun> 2100`, and `segment_references.py <dsun>
4E71`, keeping each reference whose decoded instructions after it
include a store (mov, and, or, xor, add, sub, inc, dec, shifts and
rotates, not, neg, adc, sbb, pop, xchg) whose first operand is
`es:[...]` with a displacement from 024D to 09E1. Write three queries,
each with the 42 targets: file offsets 7FA39, 7FC23, 7FCCC, 80673 and
806B5, overlay sites 193+166F, 193+16F6, 193+192F, 187+0A6C and
187+0EBE, and the 32 row-store offsets above; and controls 0x689D0 and
0x68E70. Query 1 takes the starts and leaves of FND-PARTY-037. Query 2
starts at 194+0000 with the same leaves. Query 3 starts at 182+0365,
182+08C8, 200+007E, 2C5F:018C, 182+1F1D, 31E0:0121, 200+0000,
4611:0051, 1BF3:4FEB, 1BF3:4723, 3D72:0D83, 444C:0008, 3796:0514,
444C:0092, 2C5F:0139, 2C5F:0C8D, 28C9:2A44, 187+2496, 1695:000B,
1695:01D1, 3796:0387 and 1000:3FA2 with no leaves. For each run
`reach_config.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv
<config> <query>` and `python -I -m scientific_method_engine reach
<config>`. List overlay 182 file 699B7..69D75 and overlay 193 file
7FB63..7FBD3 and 800A0..80140 with `overlay_listing.py`, and
31E0:044C..31E0:0570, 31E0:05BB..31E0:0640 and 31E0:0A90..31E0:0AE0
with `resident_listing.py`. The queries, listings and reports are
retained under GAME_DIR/analysis/combat-effect-init-20261018/. No
original function or game process was run.
