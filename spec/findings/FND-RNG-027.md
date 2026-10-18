---
id: FND-RNG-027
title: No shipped script uses instruction 0x59, the d20 threshold test, and 44 shipped script instructions issue request 27, which reseeds the generator
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008BF29..0x0008BF4E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C2AF..0x0008C2C2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008DCC6..0x0008DD05
tool: tools/research/exec-census/script_decode.py and overlay_listing.py at commit 60d3df6a; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

FND-SCRIPT-005's dispatch table sends instruction `0x59` to `172C:1804`, the caller of
FND-RNG-024's d20 threshold test. FND-SCRIPT-024 gives every instruction's operands, with
`0x59` reading three parameters, and its decoder lists all 350 `GPL ` and `MAS ` resources of
`GPLDATA.GFF`; 349 decode to their last byte, and `GPL/37` leaves `0x0D38..0x0D58` undecoded.

**Instruction 0x59.** In the full listing (`script_decode.py --all`) no instruction begins with
`0x59`, at the top level or nested inside an expression. Nested instructions are listed: `0x52`
(FND-SCRIPT-011) occurs 796 times inside expressions and `0x5A`, whose handler `172C:18A5`
follows `0x59`'s in the table, 60 times.

**Request 27.** FND-RNG-022 shows that request 27 of overlay 204 `+0169`, the dispatcher that
instruction `0x22` reaches with its four parameters (FND-PARTY-156, FND-VIDEO-010), is overlay
204 `+04EF`. The dispatcher rejects a request outside 1 to 53 (`+017B..+0189`); `+04EF` passes
the third and fourth parameters to `+1F06` and returns with no test; `+1F06` divides the third
by ten and calls overlay 204 `+0000` with the quotient's low byte, the remainder's low byte and
the fourth parameter's low byte, each sign-extended, with no test. FND-RNG-021 shows `+0000`
reseeds the generator from the clock (`+001E`) before it tests its arguments. The dispatcher
loads the second parameter into SI (`+0171`), and this route does not use it.

The listing has 44 instructions `0x22` whose first parameter is the literal 27, all at the top
level of `GPL` resources: `GPL/24` (1), 82 (4), 86 (1), 87 (5), 89 (1), 92 (2), 93 (1), 126 (1),
188 (6), 191 (1), 197 (1), 199 (5), 200 (5), 203 (1), 220 (1), 226 (1), 230 (1), 291 (1), 326
(3), 327 (1) and 330 (1). 42 of them pass literal third and fourth parameters: the third
from 12 to 44, which gives a quotient of 1 (2 instructions), 2 (19), 3 (13) or 4 (8) and a
remainder of 1 (15), 2 (4), 3 (7) or 4 (16), and the fourth 10 (1), 20 (9), 25 (1), 30 (15), 35
(1), 40 (9), 50 (5) or 60 (1). `GPL/197` `0x0076` passes 14 and the value of `{0x52 20} + 10`
computed at `0x006A`. `GPL/24` `0x0939`, inside a loop, passes `var7[1] * 10 + var7[2]` and
`var7[15] / var7[1]`.

One instruction `0x22` takes its request from a variable: `GPL/27` `0x01C8`, with `var7[8]`.
The listing writes `var7[8]` only by assigning the literals 10, 11, 12 or 13 (16 instructions,
in `GPL/1`, `GPL/163`, `GPL/164` and `GPL/185`), and has no `var7` index that is not a literal.

## Interpretation

The d20 threshold test of FND-RNG-024 is never run by the shipped scripts, so its draw never
happens in the shipped game unless a script reaches `0x59` through bytes the decode does not
read. Request 27 reseeds the generator from the clock each time one of these 44 instructions
runs, and its draw loop then runs with the magnitude, mode and count listed; `GPL/197` makes a
draw of its own just before the reseed. `GPL/27`'s variable request is 10 to 13 whenever the
scripts set it, so it does not issue request 27.

## Alternatives

- `GPL/37`'s undecoded 32 bytes, a script changed at run time, or a native write to `var7[8]`,
  are outside the listing; a native write could make `GPL/27` issue request 27.
- What `var7[8]` holds before any script sets it was not read.
- Native code also calls overlay 204 `+0000` directly (FND-RNG-022: overlay 193 `+0413`, overlay
  201 `+075F` and `+0810`); those reseeds are not script requests.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`:
`script_decode.py GPLDATA.GFF OBJEX.GFF --all` (FND-SCRIPT-024), then collect the lines with an
instruction `59` at the top level (`: 59 `) or nested (`{OFFSET: 59 `), the nested `52` and `5A`
controls, every `22 27, ` instruction with its parameters, every `22` instruction whose first
parameter is not a literal, and every assignment `-> var7[8]`. `overlay_listing.py <dsun> 204`
for `8BF29 8BF60`, `8C2AF 8C2C2` and `8DCC6 8DD05`. The listing is retained as `all-scripts.txt`
under `GAME_DIR/analysis/script-instructions-20261018/`. No original function or game process was
run.
