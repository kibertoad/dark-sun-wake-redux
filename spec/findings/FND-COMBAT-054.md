---
id: FND-COMBAT-054
title: With the effect count at zero the removal and transfer entries change nothing, and every route from the gate routine's later calls to an effect insertion passes the area-effect routine overlay 193 +12A8, which runs only for an object slot whose byte 0xF is not 0
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: claude
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007ED9D..0x0007F409
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F809..0x0007F8A9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FBD3..0x0007FC3F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00080DE3..0x00080DFA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3143:000D..3143:0061
  - build: BLD-GOG-EN-1.1
    file: RGN03B.GFF
    offset: 0x1392F..0x13937
tool: tools/research/exec-census/immediate_search.py, direct_callers.py, reach_config.py, overlay_listing.py, resident_listing.py and etab_objects.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1; scientific-method-engine 19.9.0 reach
environment: null
---

## Observation

The installed DSUN.EXE is the input (size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b). Overlay 193's code starts at file
7E2F0; offsets are hexadecimal.

### The removal and transfer entries at count 0

3143:000D, the lower-bound search FND-COMBAT-031 reads, returns the
count `DS:2100` at once when it is 0. With the count at 0:

- the key removal +1873 calls the lookup +1C17, which returns the
  count, and its own test of that index below the count fails, so it
  calls neither overlay 195 +1000 nor +18E3 nor overlay 197 +1E7B;
- the owner removal +1DB0 calls 3143:000D and leaves its loop at the
  first test, index below count;
- the code removal +194F calls 3143:0061, which returns the count, and
  leaves its loop at the same test;
- the transfer +2327 skips every row mutation (FND-COMBAT-033).

None of them writes the count or a row then. The index removal +18E3
is different: it skips the move when the index is not below the count
but decrements the count at +1933 on every path, so it is not inert
at count 0.

### Routes to the effect writers with those four as leaves

FND-COMBAT-053's third walk (the 22 routines the gate routine overlay
182 +1167 calls from +12FF on, its 42 targets and two controls) was run
again with +1873, +1DB0, +194F and +2327 as leaves. It reads 953
routines and leaves 41 transfers unresolved. The index removal's
decrement (+1933), the code removal's decrement (+19DC), both transfer
writers and the numbered restore are no longer reached. 18 targets are
reached, each from the same eight calls as before (overlay 182 +0365,
+08C8, +1F1D, 2C5F:018C, 31E0:0121, 2C5F:0139, 1695:000B and
1695:01D1): the insertion wrapper +166F, the insertion +16F6, its
increment +1749 and six field stores, and the row-store candidates
6180C, 65993, 65C5D, 65C70, 65C83, 7FDF3, 7FE54, 8230A and 82319. For
every one of the 18 the walk lists overlay 193 +12A8 among the
routines every read route passes; for the wrapper and insertion it
also lists overlay 179 +0D2F, the hit routine (FND-PARTY-202).

### Who calls +12A8, and when

A second walk with the same starts and leaves, targeting +12A8 and its
callers, reaches it from the same eight calls. The raw search finds
four near calls of +12A8, all in overlay 193, and no far call of its
trampoline 573B:0066:

- +0D9B in +0AAD. +0AAD takes an index into the 8-byte entries at the
  far pointer `DS:67B7`, reads the entry's slot word at +6, and returns
  0 with no call when the slot is negative or the slot's byte at
  `DS:67CA + 37 * slot`, byte 0xF of FMT-ACTOR-004, is 0. Otherwise it
  passes that byte less one to +12A8 for each object it finds near the
  slot. Its route from 31E0:0121 runs through 31E0:0D09, 31E0:0B4A,
  371D:0002, 371D:044D, 371D:04B3 and 371D:050F.
- +0FBE and +102F in +0DC7, which 31E0:0121 calls at 31E0:0303.
  For each entry index from the word `DS:6571` up to the word
  `DS:656F` it reads the entry's slot, skips it when the slot is
  negative or its byte 0xF is 0, and otherwise may pass that byte less
  one to +12A8.
- +10CA in +1054, which +0DC7 calls at its end. +1054 walks the
  three-byte records at 50B0:000F while the index is below the word
  `DS:2102`, and calls +12A8 only for a record whose owner is its
  argument and whose byte is negative.

The word `DS:2102` is 0 in the load image (file 4F102). Its operand
writers are the decrement +1105 in +1054's loop, the increment +155E in
the append +1519 and the decrement +15B3 in +1565, which returns before
it when the lookup +15B9 finds no record, as it does at count 0. The
append +1519 has two direct callers, +0DA8 and +0FCB, each right after
a +12A8 call on the paths above.

### What sets an object slot's byte 0xF

An encoded-operand search for 67CA returns two writes: overlay 193
+2B06 (file 80DF6), which stores an argument byte plus one, in the
routine at +29EB, and overlay 187 file 70ABB, inside the numbered
restore +0A6C. The slot fill 31E0:0E1B writes the field from byte 0xB
of the object's `OJFF` definition (FND-ACTOR-003). In the second walk
+29EB is reached only through +12A8. Of the 4,479 `OJFF` resources of
OBJEX.GFF only object 147 has byte 0xB other than 0, and it is 5
(FND-ACTOR-001). Of the 13,559 records of the 20 `ETAB` resources of
the 26 installed archives, one names object 147: record 52 of
`RGN03B.GFF#ETAB/59`, at file 1392F of that archive. The control object 430 is found in
`RGN0FF.GFF#ETAB/255`.

## Interpretation

While the effect count is 0, the removal and transfer entries the gate
routine's later calls can reach leave the table as it is, so the count
can first change only through an insertion. Every route the walk reads
from those calls to an insertion passes overlay 193 +12A8, which runs
only for an object slot whose byte 0xF is not 0, or for a record of the
area list at `DS:2102`, which only those same +12A8 paths add to. A
slot gets a nonzero byte 0xF from the definition of object 147, from
+29EB after +12A8 has run, or from the numbered restore, which is not
reached on the new-game route (FND-COMBAT-053).

So on the first game of a run, if the gate routine's later calls place
no object 147, the count stays 0 and no row is written up to the gate
routine's return: the game begins with no effect rows. The only
shipped region that places object 147 is region 59 (RGN03B); the
opening region's table is `RGN032.GFF#ETAB/50` (FND-ACTOR-012), which
does not name it.

## Alternatives

- The 41 unresolved transfers leave the walks' negatives unusable: a
  route to an insertion or to the index removal +18E3 through one of
  them is not excluded.
- Which region the gate routine loads on a new game, and whether any
  route other than the region's `ETAB` (a script, the party's own
  objects) places object 147 there, is not read here.
- The search for writers of byte 0xF covers the displacement 67CA and
  the slot fill. Block copies into the slot table and stores through
  other bases are not covered; a copy from another slot keeps a value
  some writer above put there.
- A game started after an earlier one in the same run begins with what
  the earlier game left (FND-COMBAT-053); which rows those are is not
  read here.

## How to reproduce

In `tools/research/exec-census/`, with the locked evidence Python and
`<dsun>` the installed DSUN.EXE: take FND-COMBAT-053's query 3 (its 22
starts, 42 targets and controls 0x689D0 and 0x68E70) and add the leaves
193+1873, 193+1DB0, 193+194F and 193+2327; run `reach_config.py <dsun>
../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> <query>` and
`python -I -m scientific_method_engine reach <config>`. Run it again
with the targets 0x7F08B, 0x7F2AE, 0x7F31F, 0x7F3BA, 193+29EB,
193+12A8, 193+0AAD, 193+1054, 193+1119 and 193+0DC7. Then
`immediate_search.py <dsun> 67CA`, `immediate_search.py <dsun> 2102`,
`direct_callers.py <dsun> 193+12A8 193+1519 193+1565 193+1054`,
`overlay_listing.py <dsun> 193` over 7ED9D..7F409, 7F809..7F8A9,
7FB63..7FD05, 7FF07..7FFA6 and 80D80..80E10, `resident_listing.py <dsun>
3143:000D..3143:00A0`, and `etab_objects.py <install> 147 430`. The
queries, listings and reports are retained under
GAME_DIR/analysis/combat-effect-newgame-20261018/. No original function
or game process was run.
