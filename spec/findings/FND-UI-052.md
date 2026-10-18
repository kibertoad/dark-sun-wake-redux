---
id: FND-UI-052
title: On a first new game the span from the gate routine's +12FF to MAS/50 0x05DC runs no window or frame callback and no trigger script, writes no effect row, and writes the DAC only at MAS/50 0x0000 (black) and 0x05DC (the region fade)
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069B4F..0x00069C7E
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x14DB75..0x14E15A
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x837ED..0x8387B
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x1FEF21..0x1FEF9E
tool: tools/research/exec-census/newgame_span_queries.py, reach_config.py and script_decode.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, pypcode 4.0.1, xxhash 4.0.1; scientific-method-engine 19.9.0 reach
environment: null
---

## Observation

Script offsets are within each resource (`MAS/50` at `GPLDATA.GFF` `0x14DB75`, `GPL/142` at
`0x83444`, `GPL/27` at `0x1FEE3D`). Overlay addresses are `descriptor +offset`.

**The script path on a first new game.** Overlay 169 clears the blocks of global numbers and
global flags before the first script runs (FND-PARTY-241), and `MAS/99`, which runs at startup
(FND-UI-051), stores 1 in global 22 at `0x008A`. Read with global 22 at 1 and global 82 and
global flag 188 at 0 as `MAS/50` starts:

- `MAS/50` `0x013F` tests global 22 for 1. It holds, so the instructions from `0x0145` to
  `0x01A4` run and the else part from `0x01A8` to `0x036F` is skipped. That part holds every
  `0x5E` of `MAS/50` but the one at `0x03A1`, its `0x08`, `0x37` and `0x40`, and its call of
  `GPL/142` at 1080.
- `0x0179` sets bit 2 of global 82 and `0x0181` calls `GPL/142` at 937 (`0x03A9`). There
  `0x03A9` (bit 2 set) holds, so `0x03B7` places object -60 and `0x03CF` clears the bit;
  `0x03D8` needs bit 7 of global 82 and does not hold. The routine calls its own routine at 1344
  (`0x0540` to `0x05C5`) and returns at `0x0437`.
- `0x019F` calls `GPL/27` at 491, which returns at `0x021C`; `0x0009` calls it at 228, which
  returns at `0x0161`.
- `0x038B` tests global flag 188, which is 0, so the `0x5E` at `0x03A1` is skipped.

The other conditions on the way choose between instructions of the same opcodes. So the opcodes
that run from `MAS/50` `0x0000` to `0x05DC` are 29: `09 13 14 15 16 18 19 1B 1C 1F 22 25 31 33
39 3E 3F 52 65 66 67 6A 6C 6D 6E 6F 70 7A 80`. Opcode 0x22 runs only functions 33, 34, 37, 38
and 52. Its function 37 runs at `0x0000` with the last two arguments 0, 0 and at `0x05DC` with
0, 1. The only `0x31` on the path is at `0x05E5`, after `0x05DC`. No `0x08` and no `0x5E` runs.

**The walks.** `newgame_span_queries.py` builds two queries from FND-EXE-592's effect and palette
queries. Their starts are the gate routine overlay 182 +1167's calls from +12FF to its call of the
script runner `1695:000B` (effects), or its calls after the DAC write at +13E3 (palette), plus
segment 2707's 34 relocated rows. Both narrow the opcode dispatch at `172C:01B5` to the handlers
of the 29 opcodes and cut the 46 cases of overlay 204 +0169 that serve other opcode 0x22
functions. They also cut:



- the region change of opcode 0x5E (FND-UI-051);
- the interpreter's error routine 188+1901, the runtime exit and the stack guard;
- overlay 193 +12A8 (FND-COMBAT-054, FND-COMBAT-055);
- the opcode hook's call of overlay 199 +0C21 at `2D40:380A`.

Each query adds as targets the nine window hook, record callback and frame callback call sites,
the three far calls by which `2D40` runs a trigger's script through `172C:000C`, and the seven
stores to the fight state (FND-PARTY-200). The effect query also adds overlay 193 +18E3.

- Effects: 598 routines and 36,661 instructions, none unresolved. None of the 62 targets is
  reached: no effect-row writer, callback call, trigger run or fight-state store.
- Palette: 529 routines. 6 of the 36 targets are reached, and every route passes the opcode 0x22
  handler `172C:0F84`, overlay 204 +0169 and function 37's case +0657. Cutting that case leaves
  none reached (519 routines). Function 37 with 0, 0 goes through +284D, which writes the block
  that `444C:0008` returns zeroed; with 0, 1 it goes through +1D2E, the fade to the region's
  palette (FND-UI-048).

The same queries with the 34 opcodes the script walk reaches when it takes both ways at every
condition (FND-COMBAT-055) reach 47 effect targets and 26 palette targets. Every one of those
routes passes opcode 0x5E's handler, its call of 188+0864 and that routine's call of 199+0C21,
which the path above never runs.

## Interpretation

On a first new game, with the globals as overlay 169 and `MAS/99` leave them, nothing between the
gate routine's +12FF and `MAS/50` `0x05DC` writes an effect row, runs a window handler, record
callback or frame callback, or runs a trigger's script. The scripts that run are `MAS/50`, `GPL/27`
at 228 and 491, and `GPL/142` at 937 with its routine at 1344. The DAC is written at `MAS/50`
`0x0000` with black and next at `0x05DC` by the fade, so the region palette first reaches the DAC
through that fade.

## Alternatives

- The reading assumes `MAS/50` starts with global 22 at 1 and global 82 and flag 188 at 0. A
  script run between program start and the gate's call of `MAS/50` could change them. The walk
  from program start and the start window's callback to +12FF reaches `1695:000B` and
  `1695:01D1` only through declared hook targets, and 53 script instructions write one of the
  three across the archive, so this is not settled here.
- The cuts assume the script loads, frames stay within bounds and the program does not exit,
  and that the block `444C:0008` returns is allocated.
- The +12A8 cut rests on FND-COMBAT-054 and FND-COMBAT-055, which find that only a slot made from
  object 147 runs it and that the route places no object 147.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`: run `script_decode.py
GPLDATA.GFF OBJEX.GFF --all` into a listing and read `MAS/50` `0x0000` to `0x05E5`, `GPL/142`
`0x03A9` to `0x0437` and `0x0540` to `0x05C5`, and `GPL/27` `0x00E4` to `0x0161` and `0x01EB` to
`0x021C`. Take FND-EXE-592's effect and palette queries (retained under
`GAME_DIR/analysis/reach-transfers-20261018/effects/` and `palette/`) and run
`newgame_span_queries.py <dsun> <effect query> <palette query> <out> executed`, then
`reached --fight-cut`. For each query run `reach_config.py <dsun>
../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> <query>` and `python -I -m
scientific_method_engine reach <config>`. Queries, configs and reports are retained under
`GAME_DIR/analysis/newgame-span-20261018/`. No original function or game process was run.
