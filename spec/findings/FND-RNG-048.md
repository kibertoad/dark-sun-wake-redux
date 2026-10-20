---
id: FND-RNG-048
title: Overlay 204 +214E, the second copy of the saving throw roll, is reached only as the routine function 41 of script opcode 0x22 hands to the slot walk +1A68, and no shipped script calls function 41
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C30F..0x0008C322
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008BF5E..0x0008BF6C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D828..0x0008D93A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008DF0E..0x0008E0A0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C4D7..0x0008C4D9
tool: tools/research/exec-census/alias_far_words.py, overlay_listing.py, trampoline_target.py and script_decode.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset` and are in overlay 204 unless another overlay is
named; file offsets are those of the installed `DSUN.EXE`. FND-RNG-035 reads `+214E`, entered
through trampoline 29, `5787:00B1`, and finds no direct caller of either.

**The one reference.** `alias_far_words.py <dsun> 5787:00B1` finds two segment words that can
form the trampoline's address with an offset word nearby. One is canonical: at `+0557..+055C`
the request dispatcher `+0169` pushes the segment word `0x0660` (fixup to descriptor 204,
segment `5787`) and then the word `0x00B1`. The other, at file `0x2B704`, is the segment word of
a resident far call to `56B2:0034`; the word `0x0E01` five bytes on is the tail of a following
near jump and a `push cs`, not a stored offset.

**The case.** `+0169` switches on its first argument less 1 through the 53 words at `+06C7`
(FND-PARTY-108). Word 40, at file `0x8C4D7`, is `0x054F`; no other word is. The case at `+054F`
pushes the doublewords at `bp+0xE` and `bp+0xA` (the dispatcher's third and fourth arguments),
the far pointer `5787:00B1` and the word 2, and joins `+019E`, which pushes `SI` (the second
argument) and calls `+1A68`, keeps the result in `DI` and returns it at `+06C1`
(`+054F..+055F`, `+019E..+01A9`). `alias_far_words.py <dsun> 5787:0066` finds one canonical
reference to the dispatcher's trampoline, the resident far call at file `0xD46B` that
FND-PARTY-108 places in the handler of script opcode `0x22`, and one alias at file `0xBF00`,
the operand of a `mov ax, 0x3C0D` with the bytes of `mov dx, [bx+6]` before it.

**The walk.** `+1A68` takes a target word `t`, a kind word `k`, a far routine and two
doublewords `p` and `q`, and calls the routine far with a slot, `p` and `q`, adding 1 to a count
for each call that returns a nonzero `AL`; it returns the count (`+1A68..+1B79`):

- For `t` from 1 to 9998, one call with `t` when the byte at `4F49:0C33 + 3t` equals `k` or `k` is
  -1.
- For `t` of 0x7FFE, when the byte at `DS:13FA` is not 0, one call for each slot 0 to 3 whose
  `combatants` record of that number has a nonzero word at `+0x06`; otherwise one call with the
  word at `4C13:0369`. Neither path tests the kind byte.
- For any other `t`, a cursor word starts at 0. While the cursor is below 320, `2D40:0D94` is
  called with `t` and the cursor's address; a result of 9999 ends the walk, and any other result
  is a slot called under the same kind test as the first case.

**The routine.** `+214E` takes the slot `t`, a word `s` at `bp+8` and a byte `x` at `bp+0xC`;
called from the walk, `s` is the low word of `p` and `x` the low byte of `q`. It fills a 16-byte
block for `t` through overlay 193 `+1BA3` (trampoline `573B:0025`). With `R` the `combatants`
record whose number is the word at `4F49:0C34 + 3t` and `D` the `combatant_details` record that
`R`'s word `+0x04` numbers, it returns 0 when bit 0x20 of block byte 8 is set or `R`'s byte
`+0x14` is 3. It keeps `T`, `D`'s byte `+0x30 + s`, and calls `28C9:391D` with 1 and 20 at
`+21B7`, keeping the low byte `r`. `r` of 1 returns 0 and `r` of 20 returns 1. Otherwise bit
0x80 of block byte 2 returns 0; else `r` plus the low byte of `+2204` of `t`, the block and `s`,
plus `x`, all in a byte, returns 1 when at least `T` and 0 below it, comparing unsigned
(`+214E..+2203`).

**The modifier.** `+2204` starts at 0 and subtracts 2 when bit 0x10 of block byte 5 is set,
subtracts 1 for bit 0x01 of byte 11, adds 3 for bit 0x40 of byte 11 when `s` is not 1, and adds 1
each for bit 0x40 of byte 0, bit 0x02 of byte 1 and bit 0x01 of byte 9. When `s` is not 1 it
subtracts the result of `27E5:02F2` with `t`. When `s` is 1 it adds, when `D`'s byte `+0x12` is
2 or 6, `R`'s byte `+0x1B` times 2 divided by 7 (signed), and then adds the result of
`27E5:030B` with `t` (`+2204..+22DF`). FND-PARTY-166 reads those two routines as returning the
signed bytes of the tables at `DS:0866` and `DS:0880` indexed by `27E5:04A0` of the slot with 1
and 2.

**The scripts.** `script_decode.py GPLDATA.GFF OBJEX.GFF --all` lists 3,577 instructions of
opcode `0x22`, top level or nested. None has the literal 41 as its function. The one whose
function is not a literal, `GPL/27` `0x01C8`, takes it from global number 8, which every write
in the listing sets to 10, 11, 12 or 13 (FND-PARTY-237).

## Interpretation

`+214E` runs only when a script calls function 41 of opcode `0x22`. The function rolls a saving
throw for each slot the target word names, with parameter 2 as the save number and parameter 3
as a bonus, and returns how many saved. It differs from overlay 179 `+2A46` (FND-PARTY-099) in
having no `DATA` record: no tests of byte `+0x1F`, no doubling, no record bonus, and its own
modifier in place of overlay 197 `+0206`'s. Save number `s` reads `D`'s byte `+0x30 + s`, so 1
to 5 are the five saves of RULE-PARTY-014, as with `+2A46`. With no shipped script calling
function 41, the roll is never made in the shipped game unless a script builds the function
number at run time in a way the listing does not show.

## Alternatives

- `2D40:0D94` was read only as far as FND-CONFIG-141 takes it; which slots it gives for a
  negative target word, such as a group, was not read.
- What the block bits, `D`'s byte `+0x12` values 2 and 6, and `R`'s byte `+0x14` value 3 stand for
  was not read; reading `+1BA3`'s block as the effect block of FND-PARTY-112 rests on the same
  routine filling it.
- A script could reach function 41 through global number 8 only if a write of that global outside
  the shipped `GPL ` and `MAS ` resources set it to 41.
- The overlay is outside the resident harness; no emulated call or run was made.

## How to reproduce

Use the installed `DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`),
`GPLDATA.GFF` and `OBJEX.GFF` with the locked evidence Python. In
`tools/research/exec-census/`, run `alias_far_words.py <dsun> 5787:00B1 5787:0066`,
`trampoline_target.py <dsun> 5787:00B1 573B:0025` and `overlay_listing.py <dsun> 204 8BDC0
8E660`, and read `+019E..+01A9`, `+054F..+055F`, `+06C1`, `+1A68..+1B79` and `+214E..+22DF` and
the 53 words at file `0x8C487`. Run `script_decode.py GPLDATA.GFF OBJEX.GFF --all` and count the
function numbers of the instructions `: 22` and `{....: 22`. The listings are kept as
`ovl204-all.txt` and `scripts-all.txt` under `GAME_DIR/analysis/rng-callers-20261020/`. Nothing
was executed.
