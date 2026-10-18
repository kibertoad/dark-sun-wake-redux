---
id: FND-UI-049
title: The gate routine's 768-byte palette block is all zeros, so START GAME leaves the DAC black, and the region fade is reached on a new game from script opcode 0x22's functions 1, 26 and 37, not from the start window or the gate routine
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: claude
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069C0C..0x00069C47
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 444C:0008..444C:008E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:18BC..1000:1959
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0F84..172C:0FB0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008BF29..0x0008BF4E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C071..0x0008C07F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C1F8..0x0008C21A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C2A0..0x0008C2AF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C417..0x0008C460
tool: tools/research/exec-census/reach_config.py, overlay_listing.py, resident_listing.py and direct_callers.py at bf20053f; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1; scientific-method-engine 19.9.0 reach
environment: null
---

## Observation

The installed DSUN.EXE is the input (size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b). Overlay 182's code starts at file
68850 and overlay 204's at 8BDC0 (hexadecimal throughout).

### The block written at overlay 182 +13E3

The gate routine overlay 182 +1167 calls 444C:0008 at +13CD with the
dwords 256 and 3, and passes the far pointer it returns, with first
index 0 and count 256, to 3796:0514 at +13E3, which writes 768 bytes to
the DAC (FND-IMAGE-012); it then releases the block through 444C:0092.
444C:0008 multiplies its two dwords through 1000:041A, adds one, and
calls 1000:18D0 with that sum, 769, and 1 (FND-CONFIG-209). 1000:18D0
multiplies its two dwords through 1000:07D9, requests that many bytes
through 1000:15A5, and when the pointer is not null fills the block in
chunks of at most 0xFA00 bytes through 1000:18BC, a `rep stosb` of the
byte argument, which it passes as 0, and after each chunk calls
1000:0583 with the chunk's size and the address of its pointer. It returns the pointer from 1000:15A5. For
769 bytes there is one chunk, so all 768 bytes the gate routine writes
are 0 when the request succeeds. When it fails, 444C:0008 returns a
null pointer and the gate routine still passes it to 3796:0514.

### Walks after +13E3

All walks use the regions, far transfers and jump tables that
`reach_config.py` builds, with the targets 3796:0514, 1BF3:2A8C,
1BF3:58AD, file 9AD38, 3796:047C, overlay 204 +1D2E, the 3796:0514
calls overlay 182 +0281, overlay 187 +217E, overlay 194 +0547 and
overlay 204 +288A, the 1BF3:2A8C callers overlay 180 +05B8, overlay 204
+1CE7 and +1ED6, 1BF3:4EFF, 1BF3:4FDE and 1BF3:52E7, and overlay 201
+05D5, and the controls 0x689D0 and 0x68E70.

1. From the nine calls the gate routine makes after +13E3 (2C5F:0139,
   2C5F:0C8D, 3D72:0D83, 28C9:2A44, overlay 187 +2496, 1695:000B,
   1695:01D1, 3796:0387 and 1000:3FA2) the walk reaches 1BF3:2A8C,
   overlay 204 +1D2E and +1ED6, 1BF3:52E7 and overlay 201 +05D5, each
   from 2C5F:0139, 1695:000B and 1695:01D1, and every read route passes
   +05D5. None of the 3796:0514 calls is reached.
2. With the effect entries overlay 193 +1873, +1DB0, +194F and +2327
   as leaves, which change nothing while the effect count is 0
   (FND-COMBAT-054), the same five are reached, and every read route
   to each passes overlay 193 +12A8 as well as +05D5.
3. From the program's main routine (file 1C9D4) with those leaves and
   overlay 193 +12A8 as a leaf, +1D2E is reached only through +05D5,
   by 28C9:2322, 28C9:1EDB, overlay 173 +059A, +24C7 and overlay 206
   +0E1E. The other writers reached are the cycler and those of the
   startup and the start window: overlay 180 +0141, overlay 187 +2546
   (the FLI player), overlay 182 +0243 from overlay 194 +037B, overlay
   204 +284D from overlay 187 +28B3, and 1BF3:4C09 and 1BF3:4FEB from
   overlay 194. The walk leaves 63 transfers unresolved, among them the
   script instruction table's calls.
4. From the start window's callback overlay 194 +0000 with the same
   leaves, the writers reached are overlay 194 +0504's 1BF3:4FEB fade
   and block write at +0547, and overlay 187 +206E, which writes
   `PAL/1002` at +217E and is called at +01AB, just before the gate
   routine's call at +01B5. From overlay 192 +0015 with those leaves
   no writer is reached.

### Script opcode 0x22

The script instruction table sends opcode 0x22 to 172C:0F84
(FND-VIDEO-005), which reads four arguments and passes them to overlay
204 +0169. A raw search finds no other direct call of +0169 or of its
trampoline 5787:0066. +0169 switches on its first argument less 1,
bounded by 52, through the 53 words at CS:06C7 (FND-PARTY-156):

- function 1 (case +02B1) calls +0CBA with 1, the third argument and
  the second; +0CBA holds the call of +1D2E at +12AC (FND-IMAGE-012);
- function 19 (case +0438) stores 1 in `DS:13FB` and the third
  argument's low byte in `DS:4458`, and stores 1 in `DS:0690` when the
  fourth argument is 1 and 0 otherwise;
- function 26 (case +04E0) calls +1B9F, which calls +1D2E
  (FND-IMAGE-012);
- function 37 (case +0657) calls +284D when the third and fourth
  arguments are both 0; when they are 1 and 0, stores 1 in `DS:14E5` and
  calls 1BF3:4FEB with 0 and 0xF0; and when the fourth is 1 otherwise,
  calls +1D2E with step 1.

## Interpretation

START GAME sets every DAC entry to 0 in the gate routine, unless the
block request fails, and nothing the start window or the gate routine
calls afterwards brings the region palette in, apart from routes that
pass the area-effect routine overlay 193 +12A8, the effect removals, or
overlay 201 +05D5. The region palette's first appearance on a new game
is therefore a fade through overlay 204 +1D2E that a script asks for
through opcode 0x22, with function 37 and a fourth argument of 1, or
function 26, or function 1 on +0CBA's route to its fade, unless one of
those other routes runs first. Function 19 is the switch into play
(`DS:13FB`) that lets the main loop call overlay 192 +0015
(FND-PARTY-031).

## Alternatives

- That the region palette comes from the gate routine's write at +13E3
  fails: the block is zeros when the request succeeds.
- Which script instruction asks for the fade first on a new game, with
  which function and arguments, is not read here; that depends on the
  scripts the new game runs.
- The walks list routes a decoder can read. The 63 unresolved transfers
  of the third walk include the script dispatch, so the negative
  results do not exclude another route to a DAC writer through them.
- Whether +0CBA reaches its +1D2E call for the arguments function 1
  passes, and what +1B9F's raise of the components shows before its
  fade, is not read here.
- What a failed block request writes to the DAC (the bytes at 0000:0000)
  is not read here.

## How to reproduce

In `tools/research/exec-census/`, with the locked evidence Python and
`<dsun>` the installed DSUN.EXE: `overlay_listing.py <dsun> 182 69C0C
69C47` and `overlay_listing.py <dsun> 204 8BF29 8C4F1`;
`resident_listing.py <dsun> 444C:0008..444C:0092 1000:18BC..1000:1959
1814:0104..1814:0140` (1814:0104 is 172C:0F84); `direct_callers.py
<dsun> 182+1167 204+0169`; and read the 53 words at file 8C487. For each
walk write a query with the starts above, the 17 targets and two
controls listed, and the leaves named (193+1873, 193+1DB0, 193+194F,
193+2327, and for walks 3 and 4 also 193+12A8), and run
`reach_config.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv
<config> <query>` and `python -I -m scientific_method_engine reach
<config>`. Walk 3 starts at 0x1C9D4, walk 4 at 194+0000 and 192+0015.
The queries, listings and reports are retained under
GAME_DIR/analysis/ui-newgame-palette-20261018/. No original function or
game process was run.
