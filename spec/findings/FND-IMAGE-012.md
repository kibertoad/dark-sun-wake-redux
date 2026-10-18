---
id: FND-IMAGE-012
title: During play the DAC holds the region's palette PAL/current_region, and five cycling ranges rotate indexes 1 to 31, so the overhead-map marks 1, 11 and 16 cycle while mark 100 stays one colour
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3796:0387..3796:0570
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:2A8C..1BF3:2AF4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:5877..1BF3:5931
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:055E..277B:056F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:31A7..31E0:31F7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 362C:029D..362C:02D4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068A93..0x00068AE7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068BE8..0x00068C3E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069C0C..0x00069C44
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069CF5..0x00069D59
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071F81..0x00071FBF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000814E2..0x000814EA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008E60D..0x0008E660
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008DAEE..0x0008DCC6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007DF21..0x0007DF68
tool: tools/research/exec-census/resident_listing.py, overlay_listing.py, direct_callers.py, far_pointer_words.py, immediate_search.py, gff_tag_numbers.py, reach_config.py and palette_entries.py at the commit that adds this finding, with ui011_map_dac_reach.json and ui011_dac_controls_reach.json; scientific-method-engine reach; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`. Overlay 182's code
starts at file 0x68850, overlay 192's at 0x7D300 and overlay 204's at
0x8BDC0.

### The DAC writers

FND-IMAGE-011's port census places every DAC write in four routines and
the FLI player (overlay 215, file 0x9AD38). Their callers, from
`direct_callers.py` and from `far_pointer_words.py` (no stored far
pointer to any of them was found):

| Routine | What it does | Callers |
|---|---|---|
| 3796:0514 | Far pointer at [BP+6], first index at [BP+0A], count at [BP+0C]: writes `count` colours of three bytes each to the DAC from that index, unchanged | overlay 182 +0281 and +13E3, overlay 187 +217E, overlay 194 +0547, overlay 204 +288A, and the cycler 3796:047C |
| 1BF3:2A8C | Writes one entry: index [BP+6], red, green and blue at [BP+8], [BP+0A], [BP+0C], with interrupts off | overlay 180 +05B8, overlay 204 +1CE7 and +1ED6, and 1BF3:4EFF, 1BF3:4FDE and 1BF3:52E7 |
| 1BF3:58AD | Finds the chunk named `CMAP` (the eight bytes at CS:586F are `CMAPBODY`) in an IFF-style block and writes 256 colours from it, each byte shifted right by 2 | none; no word of the file equals 0x58AD |
| 3796:047C | The cycler, below | 277B:0565 only |

1BF3:2AA7 and 1BF3:2ACF read one entry back. The five callers of
3796:0514 all pass first index 0 and count 256 (the dword 0x01000000):

- overlay 182 +0243 returns at once when the byte at DS:14E5 is
  nonzero; otherwise it requests `PAL ` number [BP+6] through
  38FF:04AB and, when that succeeds, writes it to the DAC and releases
  it. Its callers pass 1000 (overlay 194 +03B5, in the routine that
  builds the start window), 11164 (overlay 187 +2A54) and the numbers
  from the word table at DS:1687 (overlay 187 +2920), in routines that
  show one `BMP ` after another.
- overlay 187 +2164 requests `PAL ` 1002 and writes it after drawing
  `BMP ` 11004 or 11015 through 2D40:3BEC (overlay 187 +206E); the request's
  result is not tested.
- overlay 182 +13E3 (in the gate routine +1167, after its gate),
  overlay 194 +0547 and overlay 204 +288A write a 768-byte block that
  444C:0008 returns for the arguments 256 and 3 (FND-CONFIG-209)
  and then release it. The block's contents are not established here.
  Overlay 204 +284D first stores 1 in DS:14E5.

Overlay 204 +1D2E stores 0 in DS:14E5, takes its byte argument as a
step (0 becomes 1), and requests `PAL ` number current_region (the word
at DS:140C) through 38FF:04AB; the result of the request is not tested.
It then runs passes while a counter, starting at 0 and raised by the
step after each pass, is below 64. Each pass waits through 1000:12FA
with `90 - 10 * step` and, for each index 0 to 255, reads the DAC entry
and moves each component toward the palette's: up or down by the step
when the difference is larger than the step, otherwise to the palette's
value; it writes the entry back through 1BF3:2A8C. After the passes it
releases the palette. Since a component is at most 63, after its passes
every entry equals the palette's. Overlay 204 +1B9F, called from
overlay 204 +04EA, first raises every component by a computed amount
for a computed number of passes, kept below 64, and then calls +1D2E.
Overlay 204 +1D2E's callers are overlay 192 +0C62 (with 5; it follows
the call of the gate routine overlay 182 +1167 at +0C32 on one branch,
and on the other the call of overlay 204 +284D at +0C47 when the byte
at 50AF:0004 is nonzero), overlay 199 +02BE,
overlay 201 +08EA and +0B92, and overlay 204 +069A, +12AC and +1D26.

The region loader 362C:01FA also requests `PAL ` number [BP+6] when
DS:14E5 is 0 (362C:02A5) and then releases it at once through
444C:0092 without writing the DAC.

The resident calls of 1BF3:2A8C lie in the inventory rows 1BF3:4C91
and 1BF3:4FEB. 1BF3:4FEB marks which colour indexes the pixels of
graphics slot [BP+6] use, moves only those entries step by step to the
colour of the index [BP+8] and leaves them there. 1BF3:4C91 marks the
used indexes the same way; it is not followed further here.

`immediate_search.py` finds the tag `PAL ` (0x204C4150, or its low word
0x4150) only at 362C:02B0 (in 362C:01FA) and in overlays 182 (+0265),
187 (+2164) and 204 (+1D64).

### The cycler

3796:0387 fills slot [BP+6] of sixteen eight-byte records at CS:0006:
the state word at +0 is set to 0, the byte [BP+8] goes to +6 (the first
index), the byte [BP+0A] to +7 (the count), the word [BP+0C] to +2 and
+4 (the period and the counter), and the state word is then set to 3.
3796:03BF sets a slot's state to 0. 3796:03DE, 3796:0402, 3796:0426,
3796:0447 set or clear bit 0 of one slot or of every slot, and
3796:0462 stores four zero words at ES:0006; none has a direct caller.

3796:047C runs over the sixteen slots in order. For a slot whose state
has bits 0 and 1 both set, it subtracts 1 from the counter, and when
that gives 0 it waits for the vertical retrace through 1BF3:2A61 when
the byte at CS:0386 is nonzero, reloads the counter from the period,
reads the `count` entries from the first index through ports 0x3C7 and
0x3C9, and writes them back through 3796:0514 with the last one first,
so that after the step entry `first + k` holds what entry
`first + (k - 1) mod count` held.

3796:047C's only caller is 277B:0565, in the loop of 277B:0024, which
calls it when the byte at DS:1424 is nonzero and then stores 0 there.
The only store of 1 to DS:1424 is at 31E0:31BF, in the timer callback
31E0:31A7 (FND-PARTY-047, FND-EXE-585), which adds 1 to the dword at
DS:145E each time it runs and stores the 1 when the result's low three
bits are 0.

3796:0387's callers are all in overlay 182. In the gate routine +1167,
after its gate, +14A5..+1501 fill five slots, each with period 1:

| Slot | Indexes |
|---|---|
| 0 | 1 to 5 |
| 1 | 6 to 10 |
| 2 | 11 to 15 |
| 3 | 16 to 23 |
| 4 | 24 to 31 |

In +0365, which +1300 (in +1167) and +16C7 call with the region number,
+039B looks the region up among the seven words at overlay 182 +0553
and jumps through the seven words after them. Regions 55, 58, 60, 61,
62 and 63 fill slot 5 with period 2 and indexes 48 to 52, 56 to 63, 32
to 36, 40 to 47, 40 to 47 and 40 to 47; region 1 leaves slot 5 as it
is; any other region sets its state to 0 through 3796:03BF.

### The palettes

`palette_entries.py` lists the colours of the 40 `PAL ` resources. The
20 region palettes, `RGN*.GFF#PAL/n` with n the region number, all give
index 100 the colour (63, 51, 0), and all give indexes 1 to 5 the
colours (0, 1, 63), (15, 34, 63), (32, 56, 63), (15, 34, 63), (0, 1, 63)
and indexes 11 to 15 the colours (12, 34, 12), (12, 48, 13),
(11, 62, 12), (13, 48, 13), (12, 34, 12), except that `RGN042.GFF#PAL/66`
gives index 12 (13, 48, 13). Indexes 16 to 23 take one of three lists:

| Palettes | Indexes 16 to 23 |
|---|---|
| 50, 52, 54, 56, 57, 58, 59, 60, 65, 67 | (63, 63, 0), (63, 51, 2), (63, 40, 5), (63, 31, 8), (56, 21, 5), (58, 26, 5), (60, 33, 5), (63, 40, 5) |
| 1, 51, 55, 61, 62, 63, 68, 69, 255 | (63, 63, 0), (63, 51, 2), (63, 40, 5), (63, 31, 8), (56, 21, 5), (49, 12, 2), (42, 5, 1), (36, 0, 0) |
| 66 | (63, 63, 0), (60, 47, 1), (58, 33, 3), (56, 21, 5), (57, 25, 5), (59, 29, 5), (61, 35, 5), (63, 40, 5) |

`RESOURCE.GFF#PAL/1000` gives index 100 the same (63, 51, 0), and at
indexes 1 to 5, 11 to 15 and 16 to 23 it holds the colours of the
region palettes' 1 to 5, 11 to 15 and first list in other orders.

### The overhead map

A `reach` walk from the overhead map's routine overlay 201 +0073 and its
window handler overlay 201 +0000 (FND-UI-046), with the call to
1BF3:2AF4 at overlay 201 +02D9 as its positive control, has as targets
the DAC writes at 3796:0530, 1BF3:2A96 and 1BF3:58DF and in overlay 215
at file 0x9AD38, the cycler 3796:047C and the seven slot routines of
segment 3796. It reaches 939 routines and leaves 41 computed transfers
unresolved, so its negative results are not usable as proof. It reaches
one target, 1BF3:2A96, by routes that all pass the view draw 2C5F:0139
(called from overlay 182 +1856 at the start of +0073), 31BA:000E,
31E0:0EFF, overlay 173 +24C7, overlay 206 +0E1E and overlay 201 +05D5,
which calls 1BF3:4FEB at +08D6. The control walk from overlay 204 +1D2E,
overlay 182 +0243 and overlay 182 +1167 reaches 3796:0530, 1BF3:2A96 and
3796:0387, with both of its controls.

## Interpretation

During play the DAC holds the palette of the current region,
`PAL/current_region` from that region's `RGN*.GFF`, written in full by
overlay 204 +1D2E's fade, as overlay 192 +0C62 calls it. FND-IMAGE-010's
capture of the opening region, whose colours match `RGN032.GFF#PAL/50`,
agrees. `RESOURCE.GFF#PAL/1000` is the start window's palette and
`PAL/1002` that of the full-screen picture overlay 187 +206E shows.

From the gate routine on, the first main-loop pass after each eighth run
of the timer callback rotates the five ranges 1 to 5, 6 to 10, 11 to 15,
16 to 23 and 24 to 31 by one entry, each colour moving one index up and
the last one wrapping to the first. Slot 5 rotates one more range every
second step in the regions listed above.

The overhead-map marks are pixels of indexes 1, 16, 100 and 11
(FND-UI-047). Index 100 lies in no range, so a kind-1 mark is
(63, 51, 0) in every region. Indexes 1, 11 and 16 are each the first of
a cycling range, so their colours step through the range's colours in
reverse index order, one per step, from wherever the rotation stands:

- index 1, the leader in party slots 0 to 3: the colours of indexes 1,
  5, 4, 3, 2, that is (0, 1, 63), (0, 1, 63), (15, 34, 63),
  (32, 56, 63), (15, 34, 63), and again;
- index 11, kind-5 objects: the colours of indexes 11, 15, 14, 13, 12,
  that is (12, 34, 12), (12, 34, 12), (13, 48, 13), (11, 62, 12),
  (12, 48, 13), and again;
- index 16, kind-2 objects and the leader in slot 4: the colours of
  indexes 16, 23, 22, ..., 17 of the region's list.

The map routine and its handler write no DAC entry and change no cycling
slot by any route this walk reads, other than through the view draw,
whose route continues into overlay 201 +05D5's picture sequence.

## Alternatives

- Reading the region loader's `PAL ` request at 362C:02A5 as the load of
  the play palette fails: it releases the palette without writing it.
- Reading 1BF3:58AD as the play palette's loader fails: nothing calls it,
  and its source is an IFF `CMAP` chunk, which no `PAL ` resource is.
- That every route into play ends with overlay 204 +1D2E, and that no
  other route writes the DAC while play runs, is not shown: the census
  of callers above lists who can write it, not that the new-game route
  reaches +1D2E. FND-IMAGE-010's match of the opening region with
  `PAL/50` supports it for one frame.
- Whether the route from the map through 2C5F:0139 to overlay 201 +05D5
  can run when the map opens, and what the 41 unresolved transfers
  reach, is not read here.
- The 768-byte block the gate routine writes before the region fade may
  be all zero; this reading does not show what 444C:0008 fills it with.
- The rotation's pace is that of the timer callback and the main loop;
  the timer's period is FND-TIME-007's, and a loop pass's length is not
  read here.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and the locked evidence Python. In
`tools/research/exec-census/`: list 3796:0387..3796:0570,
1BF3:2A4A..1BF3:2AF4, 1BF3:4FEB..1BF3:52F4, 1BF3:5877..1BF3:5931,
277B:0540..277B:05A1, 31E0:31A7..31E0:31F7 and 362C:01FA..362C:035C with
resident_listing.py, and overlay 182 file 68A93..68BB5, 68BB5..68DA3 and
699B7..69D75, overlay 187 file 71E9E..7201A and 726E3..7299C, overlay
192 file 7DEF0..7DF80, overlay 194 file 812A0..81320 and 814AB..816C9,
overlay 201 file 897C0..89D95 and overlay 204 file 8D95F..8DCC6 and
8E60D..8E660 with overlay_listing.py. Read the seven words at overlay 182
+0553 (file 0x68DA3) and the seven after them, and the eight bytes at
1BF3:586F. Run `direct_callers.py <dsun> 3796:0514 3796:0544 3796:047C
1BF3:58AD 1BF3:58FB 1BF3:2A8C 1BF3:2AA7 1BF3:2ACF 3796:0387 3796:03BF
3796:03DE 3796:0402 3796:0426 3796:0447 3796:0462 182+0243 204+1D2E
204+1B9F 204+284D 182+1167 182+0365`, `far_pointer_words.py <dsun>
1BF3:58AD 1BF3:58FB 3796:0514 3796:0387 1BF3:2A8C`,
`immediate_search.py <dsun> 204c 4150` and `immediate_search.py <dsun>
1424`. For each of `ui011_map_dac_reach.json` and
`ui011_dac_controls_reach.json` run `reach_config.py <dsun>
../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> <query>`, then
`python -I -m scientific_method_engine reach <config> > <report>`. Run
`palette_entries.py <install dir> 0..32 100`. The listings and reports
are retained under GAME_DIR/analysis/ui-map-palette-20261018/. No
original function or game process was run and no image was viewed.
