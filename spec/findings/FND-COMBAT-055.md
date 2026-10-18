---
id: FND-COMBAT-055
title: No placement on MAS/50's script route names object 147, its variable placements name 675 and 110 to 113, and START GAME's party objects are 300 to 313
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x14DD0F..0x14DD19
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x1FF028..0x1FF053
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x83984..0x83A09
tool: tools/research/exec-census/script_decode.py and script_reach.py at 3473ee39; Python 3.14.7
environment: null
---

## Observation

Script offsets are given within each resource. In the installed `GPLDATA.GFF`, `MAS/50` starts
at file `0x14DB75` (1,510 bytes), `GPL/27` at `0x1FEE3D` (714 bytes) and `GPL/142` at `0x83444`
(1,567 bytes).

**Every placement the script route reaches.** `script_reach.py` over the decode of
FND-SCRIPT-024, from `MAS/50` offset 0 and following each `0x14` call into the script it names,
taking both ways at every condition, reaches 23 instructions of opcode `0x25` or `0x5E` (counting
those nested in a condition) and reports no transfer it could not follow:

- with a literal first operand: `MAS/50` `0x25` at `0x01D2`, `0x01E0`, `0x0253`, `0x0261`,
  `0x026F`, `0x029C`, `0x0338` (-200, -204, -160, -6540 twice, -133, -142) and `0x5E` at
  `0x0223`, `0x022F`, `0x028D`, `0x03A1` (-163, -141, -133, -141); `GPL/142` `0x25` at `0x03B7`,
  `0x03EE`, `0x03FC`, `0x040A`, `0x04B9`, `0x04CA`, `0x0505` (-60, -200, -204, -204, -60, -299,
  -204) and `0x5E` at `0x0446` and `0x046D` (-60 both);
- with a variable first operand, three:
  - `GPL/27` `0x0205` places the object global 2 names. Its only route from `MAS/50` is the call
    `0x14` 491, 27 at `MAS/50` `0x019F`, and the instruction before that call, at `0x019A`,
    stores 675 in global 2. `GPL/27` from 491 to the placement does not write global 2;
  - `GPL/142` `0x0574` and `0x05A5`, inside conditions, place the object local 4 names. `GPL/142`
    reaches them only through its routine at 1344 (`0x0540`), which stores in local 4 the random
    number from 0 to 1 that instruction `0x52` returns (RULE-SCRIPT-006) plus 110 before the
    first and plus 112 before the second.

So the first operands that route can pass are -60, -133, -141, -142, -160, -163, -200, -204,
-299, -6540, 110, 111, 112, 113 and 675.

**The party's objects.** START GAME's party load places for each of characters 40 to 43 the object
300 plus the `CHAR` word at `0x1A` (FND-PARTY-013, FND-PARTY-048), and that number is 300 to 313
for every stored character (FND-PARTY-049).

## Interpretation

On a new game no script instruction `MAS/50` runs, or any script it calls with `0x14`, places
object 147, whichever way its conditions go, and the party's own objects are not object 147.
With FND-COMBAT-054, an effect row on the way to `MAS/50` `0x05DC` can then come only from a
script that a trigger instruction registered and that runs in that span, or from a window or
frame callback live in it.

## Alternatives

- The walk does not read scripts that `MAS/50` registers with `0x1B` or the other trigger
  instructions (`0x6D`, `0x70`, `0x6E`, `0x65`, `0x66`, `0x6A`, `0x6C`); which of them run before
  `0x05DC` is not read here.
- Whether `0x25`'s first operand is the object it places, and what its sign means, is
  Q-SCRIPT-004; the reading above holds whatever the sign means, since neither 147 nor -147 is
  among the operands.
- Code outside the scripts that places an object by number (spell effects, for example) is not
  covered here.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`: run `script_decode.py
GPLDATA.GFF OBJEX.GFF --all` into a listing, then `script_reach.py <listing> 25,5E MAS/50@0`.
Read `MAS/50` at `0x0186` to `0x01A4`, `GPL/27` from 491 (`0x01EB`) to `0x0216` and `GPL/142`
from 1344 (`0x0540`) to `0x05C5` in the listing. The resource offsets come from the archive's
directory as `script_decode.py` reads it. The listing is retained under
`GAME_DIR/analysis/script-instructions-20261018/` and the walk's report under
`GAME_DIR/analysis/newgame-placements-20261018/`. No original function or game process was run.
