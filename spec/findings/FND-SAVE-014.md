---
id: FND-SAVE-014
title: The shipped scripts set 137 different subregions through function 33 of opcode 0x22, and map pages 0 to 9, 255 or the value of the global that chose the subregion through function 34
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x001FC47F..0x001FC5DE
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x00044185..0x0004431B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000898DA..0x000898EF
tool: tools/research/exec-census/script_decode.py and overlay_listing.py at commit 33d4aa51; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Script offsets are offsets in the resource named. The script opcode `0x22` reads four
parameters, numbered 0 to 3 in script order (FND-SCRIPT-024); FND-SAVE-013 reads its
functions 33, 34, 35 and 19. Function 33 takes parameter 2 as the corner and parameter 3 as
the size, each packed as `x * 1000 + y` in 16-pixel cells, and keeps the edges in pixels;
function 34 stores the low byte of parameter 2 as the map page; functions 35 and 19 store
it as the defeat picture byte. Variables are named by kind and number as RULE-SCRIPT-004
reads them: "local" a local number (kind 2), "local big" a local big number (kind 5),
"global" a global number (kind 7) and "global big" a global big number (kind `0x0A`).

**The calls.** The decode of every `GPL ` and `MAS ` resource with FND-SCRIPT-024's layouts
finds 92 calls of function 33, 39 of function 34, one of function 35 and five of function
19, all at the top level of an instruction, each with a literal function number. The one
call whose function number is a variable, `GPL/27` `0x01C8`, takes it from global 8, which
holds 10 to 13 (FND-PARTY-237). The value file
[FND-SAVE-014.calls.csv](FND-SAVE-014.calls.csv) lists every call with its parameters 2 and
3 and, for function 33, the four edges in pixels that FND-SAVE-013's procedure gives; where
the parameters are variables it has one row for each value the script assigns them, with
the case that selects it.

**Literal subregions.** 79 of the 92 calls of function 33 pass two literals.

**Subregions from variables.** In the other 13 the script assigns the variables a few
instructions before the call, from literals:

- `GPL/4` `0x02D5` and `0x03C3`, and `GPL/119` `0x064A`, compute the corner and size from
  the cell bounds a `compare` block on global 12 sets (`GPL/4` `0x03CD`, `GPL/119`
  `0x0654`): nine cases, 0 to 8, and a default that covers the whole region. `GPL/119`
  first finds the case: from 0 it adds 1 to global 12 until the party's position lies
  strictly inside the case's bounds (`0x05D2..0x0618`).
- `GPL/21` `0x08CF` takes the corner from a `compare` block on the low four bits of global
  13, cases 1 to 7, 9 and 11, and the size from the same block or, for an odd case, 20 by
  18 cells.
- `GPL/74` `0x0376` and `GPL/100` `0x0814` take them from `compare` blocks on global 24
  (cases 0 to 7) and global 26 (cases 1 to 5); these two count the size without adding 1 to
  the difference of the bounds, as the others do.
- `GPL/30` `0x0168`, `GPL/244` `0x0923` and `GPL/287` `0x0645` run after one of several
  groups of assignments: ten groups of four cell bounds in `GPL/30`, six in `GPL/244`, and
  eleven pairs of packed numbers in global bigs 14 and 15, written only in `GPL/287`.
- `GPL/38` `0x0B03`, `GPL/241` `0x034C` and the second group of `GPL/244` start from the
  cells (63, 0) to (125, 26) and move the right or left edge, or the top and right edges,
  when the party's position is above row 10 or right of column 96 and story flags allow.
- `GPL/313` `0x0642` passes one of two pairs, (0, 98098) or (102000, 30049).
- `GPL/268` `0x0B59` passes a size of 20 by 13 cells and a corner 10 cells left of and 7
  cells above the party's position, or 0 where that is negative.

Over all rows the calls give 137 different sets of edges. Four rows cover the whole region,
(0, 0) to (2048, 1568): the defaults of the two tables on global 12 and `MAS/50` `0x05BE`.

**Map pages.** 29 calls of function 34 pass a literal: 0 (6 calls), 1 (10), 2 (4), 3 (2), 4
(2), and 5, 6, 7, 8 and 9 once each. The other 10 pass a variable:

- `GPL/4` `0x02CC` passes local 10, which the same `compare` block on global 12 sets to 0
  to 5; `GPL/30` `0x015F` passes local 4, which each of its ten groups sets, to 0 to 9;
  `GPL/21` `0x08C6` passes local 3, which the block on global 13 sets to 1, 3 or 4, or to
  255 in cases 1, 3, 5, 7, 9 and 11; `GPL/313` `0x0674` passes local 2, 0 or 1 with its two
  pairs.
- Six pass the global that the `compare` block just before chose the subregion by, after
  the block ends: `GPL/81` `0x1802` global 60 (cases 1 to 24, all literal calls),
  `GPL/96` `0x1732` global 70 (cases 1 to 12), `GPL/327` `0x04CA` global 113 (cases 1 to 3),
  `GPL/100` `0x080B` global 26 (cases 1 to 5), and `GPL/119` `0x057C` and `0x05C7` global 12.
  The page is the global's value also when no case matched, which leaves the subregion as
  it was. Global 60 is assigned 1 to 24 in `GPL/81` and 24 in `GPL/225`, global 70 1 to 12
  in `GPL/96` and 24 in `GPL/221`. `GPL/197` takes 1 from each and from global 113 when it is
  above 0 and a story flag or global 112 allows, and `GPL/218` under the same conditions
  takes 8, or sets 0 below 8. Global 113 is also assigned 1 to 3 (`GPL/327`), 100
  (`GPL/200`, which also takes 1 from it) and 0 or 24 times a local (`GPL/229`). `GPL/100`
  `0x0429` sets global 26 to 1 to 6 from the party's position; case 6 has no subregion.

The overhead map compares the page byte with 25 as a signed byte and sign-extends it before
adding it to the picture number (overlay 201 `+011A`, `+011F`, `+012B` and `+012E`, file
`0x898DA..0x898EF`), so the 255 of `GPL/21` stays and is read as -1.

**Defeat pictures.** Function 35 is called once, `GPL/88` `0x0176`, with 4; function 19
with 0 (`GPL/88`, `GPL/126`, `GPL/331`) and 3 (`GPL/91`, `GPL/200`).

## Interpretation

A subregion is a part of a region map that a script restricts the view to; the shipped
scripts define 137 of them, either outright or as a table indexed by a global, and one that
follows the party, 20 by 13 cells around it. The map page goes with the subregion: where a
script picks the subregion from a table it passes the table's index, 0 to 24, or a page
number of 0 to 9 that it sets beside the bounds, so `BMP 20100 + 25 * region + page` is one
picture per subregion of a region. The one other value is 255, from `GPL/21`, with which the
overhead map asks for picture `20099 + 25 * region`, the last page number of the region
before. The globals 60, 70 and 113 also act as counters that
`GPL/197` and `GPL/218` lower; when one reaches 0 the next call of the subregion routine
sets page 0 and keeps the old subregion. Which value a variable holds when a call runs is
run-time state the scripts do not decide alone: the party's position, the story flags, and
the values the globals, or locals not assigned in the run, hold from earlier.

## Alternatives

- Instructions whose handlers write through a parameter's address other than those of
  RULE-SCRIPT-011 and `0x06` were not looked for; such a write to one of the variables above
  would add values to the rows.
- Where a call follows a group of assignments only on some paths (`GPL/30` has nine callers
  of the routine at `0x00F1` that assign no group just before), the rows list every group of
  the script; a path that reaches the call with no group assigned in the run passes what the
  locals held before, which depends on how locals are cleared between runs (Q-SCRIPT-005).
- Whether every listed case is reached in play, which needs the triggers that start each
  routine, was not read.

## How to reproduce

Use the installed `GPLDATA.GFF` (2,191,945 bytes, XXH3-128
`2bd1d2b989eef2dcb0545feb95975fca`) and `OBJEX.GFF` (6,816,516 bytes,
`e08d3cac153c547f6804ca27b155915d`) with the locked evidence Python. In
`tools/research/exec-census/`, run `script_decode.py GPLDATA.GFF OBJEX.GFF --all`, search its
output for `: 22 33,`, `: 22 34,`, `: 22 35,`, `: 22 19,` and for `: 22 ` followed by a
variable, and for each call with a variable parameter read the instructions before it in
the same resource and every write of the variable (`-> var`, and the opcodes `0x01` to
`0x07`, `0x11` and `0x76` to `0x7F` naming it) in that resource, or in all resources for a
global. Convert each pair with FND-SAVE-013's division by 1000, sums, products by 16 and
limits of 2048 and 1568. With the installed `DSUN.EXE` (634,416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`), run `overlay_listing.py <dsun> 201 898C0 89900`; in this
16-bit code the byte `0x98` at `+012E` is `cbw`, which Capstone labels `cwde`. The listing is retained as `all-scripts.txt` under
`GAME_DIR/analysis/script-instructions-20261018/`. No original function or game process was
run.
