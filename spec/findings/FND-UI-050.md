---
id: FND-UI-050
title: A new game loads region 50 and runs MAS/50 first, whose instruction 0x05DC is the first fade-in request a script on the new-game route makes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:0024..277B:0531
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069C7E..0x00069C82
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069E4C..0x00069E4F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069F05..0x00069F08
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071046..0x00071049
tool: tools/research/exec-census/script_decode.py, immediate_search.py, resident_listing.py, overlay_listing.py and reach_config.py at 32b49898; Python 3.14.7, Capstone 5.0.9, pypcode 4.0.1, xxhash 4.0.1; scientific-method-engine reach
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`.

**The region word.** `immediate_search.py` for `140C` finds five stores to `DS:140C`
(`current_region`): `277B:0182`, which stores the `-R` switch's number; `277B:03C1`, which
stores 50; overlay 182 `+15D7` at files `0x69E4C` and `0x69F05` (`mov [140c], si`); and the
region transition overlay 187 `+1132` at file `0x71046`. The other operands read the word. The
load image holds 50 at `DS:140C` (file `0x4E40C`); FND-SOUND-010 finds that `-R` needs `-K 911`,
and `RAVAGER.BAT` runs `DSUN -W0 -L`.

In the main routine `277B:0024` (file `0x1C9D4`), the block `277B:0353..277B:0531`, which
stores 50 at `277B:03C1`, runs MAS/99 and jumps back to `277B:02C9`, runs only when the byte
`DS:13FB` is nonzero at `277B:0330`; script function 19 sets that byte on a defeat.

Three reach walks look for the five stores. The first starts at the 40 starts FND-PARTY-031
lists for the startup route, with overlay 182 `+1856` and `+19F8` as leaves (FND-PARTY-124):
701 routines, 43,816 instructions, 61 unresolved transfers. The second starts at the start
window's callback overlay 194 `+0000` with the same leaves: 980 routines, 80,106 instructions,
42 unresolved. The third starts at the 18 calls the gate routine overlay 182 `+1167` makes from
`+1300` on other than `1695:000B`, `1695:01D1`, `3796:0387` and `1000:3FA2` (FND-COMBAT-053):
953 routines, 77,674 instructions, 41 unresolved. Each reaches its controls (files `0x689D0`
and `0x68E70`, and in the second walk also the gate's call of `1695:000B` at `0x69C7E`), and
none reaches any of the five stores. All three report their negative result as not usable
because of the unresolved transfers.

**The first script.** The gate routine calls `1695:000B` at `+142E` (file `0x69C7E`) with the
word at `DS:140C`, so it runs `MAS/50` from offset 0. In the decode of FND-SCRIPT-024,
`MAS/50` begins with `0x22` function 37 with arguments 0 and 0, which calls `204 +284D` and
does not fade, and calls `GPL/27` at offset 228, which only zeroes variables and returns. Its
other calls into other scripts before its end are `GPL/142` at 937 and 1080 and `GPL/27` at
491. None of those routines, followed through their local calls (`GPL/142` jumps to 1344 from
both), has an `0x22` instruction with function 1, 26 or 37; `GPL/142`'s only one is function 15
at `0x0613`. `MAS/50` ends with function 34 (map page 0), 38, 33 (the whole region, size
`128098`), 34 (page 0), 52 and, at `0x05DC`, function 37 with a fourth argument of 1, then
`0x31`. It has no other `0x22` call with function 1, 26 or 37 and a fourth argument of 1.

**Object 147.** The literal object numbers of `MAS/50`'s seven `0x25` instructions are -200,
-204, -160, -6540 (twice), -133 and -142. `GPL/142`'s routines at 937 and 1080 name -60, -200,
-204 and -299, and `GPL/27` at 491 takes the number from global 2. The literal -147 appears in
the shipped scripts only in `MAS/59`.

## Interpretation

A new game started as the shipped batch file starts it loads region 50: the word holds 50 from
the load image, the `-R` store needs a switch the batch file does not pass, the other constant
store runs only after a defeat, and the walks from program start through the gate routine's
later calls reach neither the region transition nor `+15D7`'s stores. The first script the gate
runs is `MAS/50`, and the first instruction on its route that asks for overlay 204's fade to the
region palette is `MAS/50` `0x05DC`, after the subregion and map page are set. This matches
FND-IMAGE-010's capture of the opening region with `PAL/50`. `RGN032.GFF#ETAB/50` does not
place object 147 (FND-COMBAT-054), and no literal placement on `MAS/50`'s route names it.

## Alternatives

- The walks leave 61, 42 and 41 transfers unresolved; one of them could reach a store to
  `DS:140C` or a DAC writer, and the script interpreter's opcode dispatch is among them.
- `MAS/50` registers nine `0x1B` entries with other scripts (`GPL/142`, 138, 135, 147, 149,
  139, 23, 136 and 141); if one runs before `0x05DC`, its instructions come first. The decode
  does not show when they run.
- Whether `0x25`'s first operand is the object it places is Q-SCRIPT-004; `GPL/27` at 491
  places the object whose number global 2 holds.
- A run with `-R` and `-K 911` starts in the named region.

## How to reproduce

In `tools/research/exec-census/`, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`): run
`immediate_search.py <dsun> 140c`, `resident_listing.py <dsun> 277B:0024..277B:0531`,
`overlay_listing.py <dsun> 182 69C0C 69F10` and read the word at file `0x4E40C`. For each walk
write a query with the starts named above, the five stores as targets and the controls, and run
`reach_config.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> <query>` and
`python -I -m scientific_method_engine reach <config>`. Run `script_decode.py GPLDATA.GFF
OBJEX.GFF --all` and read `MAS/50`, `GPL/142` from offset 937 and `GPL/27` from offsets 228 and
491. The queries, configurations and reports are retained under
`GAME_DIR/analysis/new-game-region-20261018/` and the listing under
`GAME_DIR/analysis/script-instructions-20261018/`. No original function or game process was
run.
