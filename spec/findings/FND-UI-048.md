---
id: FND-UI-048
title: Overlay 201 +05D5, the one DAC writer the overhead map's walk reaches, ends every path with the region-palette fade, and the new-game route reaches that fade only through +05D5
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: claude
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00089D95..0x0008A181
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00089833..0x0008984A
tool: tools/research/exec-census/reach_config.py, overlay_listing.py at fb337fd3; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1; scientific-method-engine 19.9.0 reach
environment: null
---

## Observation

The installed DSUN.EXE is the input (size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b). Overlay 201's code starts at file
897C0, overlay 192's at 7D300, overlay 194's at 81130 (hexadecimal
throughout).

### Overlay 201 +05D5

The routine takes a combatant slot. It calls 2C5F:018C, then dispatches
on the combatant's byte `+0x1F` through eight words at CS:09C1 (bound
by `cmp bx, 7` and `ja`). The words are +062E, +061C, +061C, +061C,
+0652, +0640, +0640 and +0640, each a call of overlay 206 +0000 that
falls through to the next and on to +0664; kinds above 7 jump to
+0652. It then sets bit 7 of the slot's byte at `DS:67BB + 37 * slot`,
unloads an object through 31E0:05BB, calls the view draw 2C5F:0139 and
overlay 182 +1856, and shows three runs of `BMP` pictures, each loaded
through 38FF:04AB and skipped when the request fails, with waits
through 1000:12FA between frames. It then calls 1BF3:348C, 1BF3:4723
with 1, 1BF3:4FEB with 0 at +08D6, overlay 182 +19F8, 3D72:0D83, and
overlay 204 +1D2E with 5 at +08EA, after which it clears bit 7 of the
same byte and continues with effect-row and object work until its
`retf` at +09C0. Every jump before +08EA is a forward jump, a
loop-back inside one of the picture runs, or one of the table's
targets, so every path through the routine on which its calls return
reaches +08D6 and then +08EA.

### Walks

Each walk uses the declared regions, far transfers and jump tables
that `reach_config.py` builds from the build and its inventory.

1. From overlay 201 +05D5, with the DAC targets of FND-IMAGE-012's map
   walk (3796:0530, 1BF3:2A96, 1BF3:58DF, file 9AD38, 3796:047C and
   the seven slot routines of segment 3796) and overlay 204 +1D2E:
   1BF3:2A96 and +1D2E are reached, +1D2E by its call at +08EA.
2. The same walk with overlay 204 +1D2E and 1BF3:4FEB declared as
   leaves, their DAC writes being those FND-IMAGE-012 reads: no DAC
   target is reached. The control, the call to 1BF3:4FEB at +08D6,
   is reached.
3. From the overhead map's routines overlay 201 +0073 and +0000, with
   targets overlay 201 +05D5, overlay 206 +0E1E and overlay 173 +24C7:
   all three are reached; every read route to +05D5 passes 2C5F:0139,
   the routines at files 26DAE and 27EFF, overlay 173 +24C7 and overlay
   206 +0E1E. The map
   routine's first call, at +0085, is overlay 182 +1856, before it
   builds its window and requests its picture.
4. With targets overlay 204 +1D2E, +1B9F and +284D and the call at
   overlay 192 +0C62, from the start window's callback overlay 194
   +0000, from the 22 calls the gate routine overlay 182 +1167 makes
   from +12FF on (FND-COMBAT-053), and from overlay 192 +0015 (the
   main-loop call FND-PARTY-031 reads): each walk reaches +1D2E, and
   every read route to it in each walk passes overlay 201 +05D5. None
   reaches +1B9F, +284D or the instruction at overlay 192 +0C62.

Every walk leaves 41 transfers unresolved, the same 41 in each: the
far and near calls through `es:[bx+2]` at 5496 and 549D, `[A4C6]` at
5546, `[bx-5BBA]`, `[3676]`, `[367A]` and `[367E]` at 55A1 to 55CD,
`[bp+0A]` at 5C37, the jump through `[398C]` at 772C, the tables at
`DS:3F42` and `DS:3F46` at 9A3C, 9B83, A04A, A09A, AEF1 and B0A8,
`[2F6]` and `[bx+30A]` at C667 and C675, `es:[si+0C]` at 16E3E,
`[di+61B8]` and `[1154]` at 172C9 and 172D0, `call bx` at 1DE0E and
1DE19, `[A0F5]`, `[A0F1]` and `es:[bx+0F5]` at 2F696 to 2FA65,
`es:[bx+0FD]` and `es:[bx+0F9]` at 301E7 and 30397, `es:[bx+70]`,
`+62`, `+5E` and `+5A` at 32A4A, 32EB0, 332E7 and 334F7, `[A119]` at
336C1 and 336F0, and `[348A]`, `[3475]`, `[3479]`, `[3471]` and
`[346D]` at 3CB07, 3F007, 3F071, 3FDBB and 3FDE2.

## Interpretation

Whether or not the view draw that opens the map runs overlay 201
+05D5, the map's picture is drawn with the DAC as the fade overlay 204
+1D2E leaves it: the walk from the map reaches no other DAC writer,
+05D5 writes the DAC only through 1BF3:4FEB and +1D2E, and on every
path it calls +1D2E after 1BF3:4FEB, which brings every entry to
`PAL/current_region` (FND-IMAGE-012). +05D5 runs inside the map
routine's first call, before the map builds its window. This rests on
the 41 unresolved transfers and on the palette request in +1D2E
succeeding, which it does not test.

On a new game, the only read routes to the region-palette fade pass
+05D5, a sequence for one combatant chosen by its kind; overlay 192
+0C62's call of the fade is not reached from the start window's
callback, the gate routine's later calls or the main loop's overlay
192 +0015. What first puts the region palette in the DAC on a new game
is therefore not this fade, unless +05D5 runs then.

## Alternatives

- Reading the map route through +05D5 as leaving the DAC faded toward
  one colour fails: the fade to `PAL/current_region` at +08EA follows
  1BF3:4FEB on every path.
- That +05D5 runs when the map opens is not shown: the walks list
  routes a decoder can read.
- The window-system callbacks through `es:[bx+0F5]`, `+0F9` and
  `+0FD` and the pointers at `DS:A0F1` and `DS:A0F5` are among the
  unresolved transfers; a route from them to the main loop's call of
  the cycler 3796:047C would decide whether the colours keep rotating
  while the map is shown.
- The 768-byte block the gate routine writes to the DAC at overlay 182
  +13E3 (FND-IMAGE-012) is a candidate for the new-game palette; its
  contents are not read here.

## How to reproduce

In `tools/research/exec-census/`, with the locked evidence Python, list
overlay 201 file 89D95..8A181 and 89833..89900 with `overlay_listing.py`
and read the eight words at file 8A181. Write the queries: (1) start
201+05D5, targets 3796:0530, 1BF3:2A96, 1BF3:58DF, 0x9AD38, 3796:047C,
3796:0387, 3796:03BF, 3796:03DE, 3796:0402, 3796:0426, 3796:0447,
3796:0462 and 204+1D2E, control 201+08D6; (2) the same with leaves
204+1D2E and 1BF3:4FEB; (3) starts 201+0073 and 201+0000, targets
201+05D5, 206+0E1E and 173+24C7, control 201+02D9; (4) targets
204+1D2E, 192+0C62, 204+1B9F and 204+284D, controls 0x689D0 and
0x68E70, with the starts 194+0000, then the 22 starts of
FND-COMBAT-053's third query, then 192+0015. For each run
`reach_config.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv
<config> <query>` and `python -I -m scientific_method_engine reach
<config>`. The queries, listings and reports are retained under
GAME_DIR/analysis/ui-map-palette-route-20261018/. No original function
or game process was run.
