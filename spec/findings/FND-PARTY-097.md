---
id: FND-PARTY-097
title: The fight routine runs object trigger scripts, and script opcode 0x24 reaches overlay 190 +36D2, which stores the word at 4C13:0369 in 4E71:0B44 and makes that slot's records current at DS:1429 and DS:142D
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B03B..0x0005B040
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B709..0x0005B70F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:207E..2D40:2130
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0F79..172C:0F84
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073910..0x00073948
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007BA12..0x0007BA6F
tool: Python 3.14.7 with Capstone 5.0.9, xxhash 4.0.1 and scientific-method-engine 18.4.0 (tools/research/exec-census/reach_config.py, overlay_listing.py, resident_listing.py, direct_callers.py, trampoline_target.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Overlay 173 `+059A` is the fight routine, called from `2A00:0DE3` and `2B00:0DD1`
(FND-PARTY-089). FND-PARTY-084 lists the stores to `DS:142D`; they lie in overlay 184 `+07D8`,
overlay 190 `+05A9` and `+36D2`, overlay 202 `+0287`, overlay 209, overlay 211 `+02B5` and
overlay 212 `+061C`.

**The reach runs.** `reach_config.py` builds each configuration as in FND-PARTY-040 (205 regions,
7,562 far transfers, 157 jump tables). Starts: the routines `2A00:0B6B` and `2B00:0CEF` that hold
the two calls of `+059A`. Targets: overlay 190 `+05A9` and `+36D2`, overlay 202 `+0287`, overlay
211 `+02B5` and overlay 212 `+061C`. Leaf: the exit worker `1000:0388`. Controls: the calls at
`2A00:0DE3` and `2B00:0DD1`.

1. `q043_loop_reach.json`, with no declared calls: 1,038 routines and 88,116 instructions, both
   controls reached, no target reached, 36 computed transfers unresolved, among them the script
   dispatcher's two calls at `0xC667` and `0xC675`.
2. `q043_loop_declared_reach.json` adds these `indirectCalls`, each declared exhaustive: `0xC667`
   with the target `2D40:37F3` and `0xC675` with the 129 words at file `0x4D30A` (`DS:030A`), as
   FND-CONFIG-135 and FND-CONFIG-136 give the script dispatcher; the six calls through `DS:3F42`
   and `DS:3F46` (`0x9A3C`, `0x9B83`, `0xA04A`, `0xA09A`, `0xAEF1`, `0xB0A8`) with the four offsets
   in segment `1425` that FND-EXE-391, FND-EXE-393, FND-EXE-396 and FND-EXE-398 give for each
   field; and the formatter calls `1BF3:6199` and `1BF3:61A0` as FND-PARTY-075 gives them. It
   reads 1,484 routines and 111,425 instructions, reaches both controls, and reaches overlay 190
   `+36D2` and no other target, leaving 45 computed transfers unresolved. The one route uses one
   declared call, `0xC675`:

   `+059A` calls `+0C65` (near, at `+071D`); `+0C65` calls `2D40:207E` with a combatant number
   (`+0DEA`); `2D40:207E` reaches the script interpreter `172C:000C`, which reaches the dispatcher
   `172C:018F`; the dispatcher's table call at `0xC675` takes the word for opcode `0x24`, `0x0F79`,
   to `172C:0F79`; that handler calls trampoline `5702:0098`, overlay 188 `+0A70`, which calls
   overlay 190 `+36D2` (trampoline `571F:005C`).

**`2D40:207E`** takes an object number `s`. When the word at `4F49:08A9 + 0x13 * s` is 0 it
returns. Otherwise it takes the positions of `s` and of the combatant numbered by the word at
`4C13:0369`, from the 37-byte records at `DS:67C5`, passes them to `1B0B:00A4` and `1B0B:000C`,
and goes on to the object's script.

**`172C:0F79`**, the handler of opcode `0x24`, reads a parameter through `172C:3278` and passes
it to overlay 188 `+0A70`. **Overlay 188 `+0A70`** passes that word to `2D40:0D94`, and when the
result is not 9,999 calls overlay 190 `+36D2` with the word at `4C13:0369` and the result, then
calls overlay 199 through `576C:002F` (`+0A70..+0AA7`). **Overlay 190 `+36D2`** stores its first
argument in `4E71:0B44`, points `DS:1429` at the details record and `DS:142D` at the combatant
record numbered by it, and stores its second argument at `DS:1A30`, then calls on into other
screens (`+36E5..+3734`).

## Interpretation

During a fight, the fight routine runs the trigger script of an object, and opcode `0x24` in
that script makes the slot in `4C13:0369` the current record, the one whose wisdom a level gain
after the fight reads as `screen_wisdom` (RULE-PARTY-013). The opcode opens a screen for that
slot with a second number, which `+36D2` checks against 9,999.
The other store routines are not reached from the two fight loops under these declarations.

## Alternatives

- No script that can run during a fight holds opcode `0x24`: the route is in the code, and which
  scripts the objects of each region carry was not read.
- `2D40:207E` runs the script only under its distance test: the test and the further conditions
  after `2D40:2130` were not read, so the route rests on the reach walk, which takes every branch.
- A store reached only through one of the 45 transfers left unresolved, or through a value of the
  `DS:3F42` and `DS:3F46` fields beyond the four producers read (FND-EXE-391 leaves their writer
  coverage open): not ruled out.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `reach_config.py <dsun>
../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <out> q043_loop_reach.json` and the same for
`q043_loop_declared_reach.json`, then `python -I -m scientific_method_engine reach <out>` for
each; `overlay_listing.py <dsun> 173 5B030 5B045`, `173 5B6E0 5B712`, `188 73910 73948` and
`190 7BA12 7BA80`; `resident_listing.py <dsun> 2D40:207E..2D40:2130` and
`172C:0F79..172C:0FA0`; `trampoline_target.py <dsun> 5702:0098`; and read the word at file
`0x4D352` (`DS:030A` plus twice `0x24`).
