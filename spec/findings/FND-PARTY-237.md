---
id: FND-PARTY-237
title: No shipped script sends effect code 59 through function 23 of opcode 0x22, and the one call of function 46 that could cast spell 104 or 225 is GPL/255 0x0327, which casts only when global number 129, written by no script, holds -9999
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008DE17..0x0008DE3C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D863..0x0008D90D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:1331..172C:1347
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x001110F4..0x00111114
tool: tools/research/exec-census/script_decode.py and overlay_listing.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. The script opcode `0x22` reads four parameters (FND-SCRIPT-024) and passes them to
overlay 204 `+0169` as the function number, `SI` and the dwords at `bp+0xA` and `bp+0xE`
(FND-PARTY-108, FND-PARTY-156). Below, the parameters are numbered 0 to 3 in script order.

**Function 23.** Its case at `+047E` passes the routine `+2057` and parameters 2 and 3 to
`+1A68`, which calls the routine with a slot, parameter 2 and parameter 3 in that order
(`+1AA3..+1AAC`, `+1AE2..+1AED`, `+1B03..+1B15`, `+1B44..+1B4D`). `+2057` compares the dword at
its `bp+8`, parameter 2, with 0x71 and, when it is less, calls overlay 193 `+166F` with the
slot, the low word of parameter 3, -1 and the low byte of parameter 2 as the code
(`+205A..+2077`).

**Function 46.** Its case at `+025E` calls `+18CD` with -1 and parameters 2 and 3 when `SI`,
parameter 1, is 9999 (`+026D..+027F`), and `+18CD` casts parameter 3 modulo 1000
(FND-PARTY-122).

**The shipped scripts.** `script_decode.py --all` lists every `GPL ` and `MAS ` resource with
FND-SCRIPT-024's layouts. Opcode `0x22` begins 3,203 instructions and occurs 374 times nested
in an expression; parameter 0 is a literal in all but one, and functions 23 and 46 occur only at
top level.

- Function 23 occurs 16 times, each with a literal parameter 2: 1, 2, 11, 18, 19, 35, 37, 47,
  69, 70, 71, 91 and 98. None is 59.
- The one call whose function is not a literal, `GPL/27` `0x01C8`, takes it from global number
  8. Every write of global number 8 in the listings is an assignment of 10, 11, 12 or 13
  (`GPL/1`, `GPL/163`, `GPL/164`, `GPL/185`), and no in-place instruction of RULE-SCRIPT-011
  names it.
- Function 46 occurs 83 times. In 58 parameter 1 is the literal 9999; 56 of those have a
  literal parameter 3, whose remainders by 1000 are 8, 22, 26, 27, 32, 43, 54, 58, 69, 70, 83,
  90, 93, 207 and 236. The other two take parameter 3 from local number 0 of the big-number
  kind: in `GPL/176` `0x06B9` the routine at `0x0687` holding the call is entered only from
  the two local calls at `0x0668` and `0x0681`, inside the routine at `0x0655`, which is
  entered only from the local calls at `0x0570` and `0x05A0`. Each of those follows at once an
  assignment of 412027 or 412043 to that local; both routines begin after a return, no literal
  jump from outside lands in them, and they hold no `0x14` call, so `GPL/164`, which assigns
  the local a computed value and which `GPL/176` calls at `0x05DD`, cannot change it on the
  way. In `GPL/312` `0x0679` the local is assigned only the literals 1112028,
  1112053, 1112083, 1114008 and 1114093, and `GPL/312` calls no other script.
- In the other 25 calls of function 46 parameter 1 is -190, -192, a variable of kind 9, or
  computed. Their parameter 3 is a literal with remainder 3, 8, 10, 26, 42, 170, 174, 194,
  203, 210, 236 or 240, or a local of the big-number kind: in `GPL/125` `0x03C5` it is assigned
  1115198, 1115143 or 1115171 right before each of the four local calls of the routine at
  `0x03B5`, the only way into it (the instruction before it returns, and no literal jump lands
  in it); in `GPL/137` `0x01D9` it is 1115169, 1115198, 1115172 or 1115143, or a value of
  local number 0 left from earlier in the run, which among the 11 scripts `GPL/137` reaches is
  written only by `GPL/134` `0x091A` as a 16-bit number times 60; in `GPL/312` it holds the
  literals above.
- `GPL/255` `0x0327` passes as parameter 1 the result of opcode `0x1F` on global number 129,
  and as parameter 3 `var2[1] * 1000 + var2[0]`, which `0x031A` and `0x031F` set from the kind-9
  variables `0x22` and `0x21`. The handler of `0x1F`, `172C:1331`, reads one expression,
  multiplies it by -1 as a 32-bit number and stores it in the accumulator. Global number 129
  occurs nowhere else in the listings, so no script instruction assigns it or changes it in
  place.

## Interpretation

No shipped script sends the level drain's effect code 59 through function 23. A literal or
script-assigned fourth argument of function 46 never has the remainder 104 or 225, and a
product by 60 cannot have either, since both are not multiples of 20. The one exception is
`GPL/255` `0x0327`: it casts only when global number 129 holds -9999, and then casts the
remainder by 1000 of the kind-9 variable `0x22`, which the script also uses with `0x21` as the
two halves of a number of the form `a * 1000 + b`. Whether that cast can happen depends on the
value global number 129 starts with or a saved game gives it, and on what the kind-9 variables
hold, neither of which the scripts decide.

## Alternatives

- Instructions whose handlers write through a parameter's address other than those of
  RULE-SCRIPT-011 were not looked for; one such write to a local of the big-number kind or to
  global number 8 or 129 would add values to the lists above.
- Where a local is read before the run assigns it, this finding takes it to hold no value a
  script set earlier; how the game clears local variables between runs is Q-SCRIPT-005.
- Whether a script run can begin inside the routines named above at an offset a trigger record
  holds was not checked; the trigger offsets are `GPL ` entry points of FND-SCRIPT-013.

## How to reproduce

Use the installed `DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`),
`GPLDATA.GFF` (2,191,945 bytes, `2bd1d2b989eef2dcb0545feb95975fca`) and `OBJEX.GFF`
(6,816,516 bytes, `e08d3cac153c547f6804ca27b155915d`) with the locked evidence Python. In
`tools/research/exec-census/`, run `overlay_listing.py <dsun> 204 8DE17 8DE40`, `204 8C01E
8C050`, `204 8C23E 8C250` and `204 8D828 8D93A`, and `resident_listing.py` for
`172C:1331..172C:1347`. Run `script_decode.py GPLDATA.GFF OBJEX.GFF --all` and search its
output for `: 22 23,`, `: 22 46,`, `22 var`, `var7[8]`, `var7[129]` and the writes of
`var5[0]`, following the `0x13` and `0x14` targets by hand. The listing is retained as
`all-scripts.txt` under `GAME_DIR/analysis/script-instructions-20261018/`. No original function
or game process was run.
