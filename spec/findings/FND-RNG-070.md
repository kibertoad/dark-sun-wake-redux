---
id: FND-RNG-070
title: When overlay 173 +059A finds no next combatant it ends combat and stores 9999 through its pointer argument, which 28C9:2146 sets to DS:426D
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2146..28C9:2164
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005AEBA..0x0005AEE3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B032..0x0005B0F5
tool: tools/research/exec-census/overlay_listing.py at the commit that adds this finding; Capstone 5.0.9; Python 3.14.7
environment: null
---

## Observation

`28C9:2146..2153` calls overlay 173 `+059A` (trampoline `5671:0039`, code from file offset
`0x5A920`) with the combat word `4C10:0019` and the near address `0x426D`, and `28C9:2160` stores
what it returns in `4C10:0019`.

Overlay 173 `+059A` loads its second argument into `DI` (`+05A2`) and the word it points to into
`SI` (`+05A9`). At `+0712`, when `SI` is 9999, it goes on to `+071B`; otherwise it jumps to
`+07D5`. From `+071B`, unless the near routine at `+0C65` returns a nonzero byte (then it jumps
to `+07F1`), it does the following when `DS:13F7` is not 0: calls `4A6E:0064` with the byte
`4E71:0C14`, opens a `RESOURCE.GFF` path through `44DE:0086` and closes it when `DS:14E8` is not
0, and calls overlay 187 `+2B91` with 2. Then it sets `SI` to 9999 at `+078D`, calls `+3E30`, overlay 195 `+14D5`, overlay 206 `+0B49` and
`+3CDA`, then either overlay 210 `+0B44` and overlay 188 `+1820` (when `DS:13FB` is 0) or
`1BF3:4FEB` with 0 and `1BF3:4723` with 1, and stores `SI` through `DI` (`+07CF`) and returns 0
(`+07D1..+07D5`).

## Interpretation

The routine that advances combat ends it when no combatant is next: it returns 0, which
`28C9:2160` stores as the combat word, and leaves 9999 in `DS:426D`. Nothing on this path picks
a leader, so `DS:426D` holds 9999 after such an end until another store sets it; FND-RNG-069
sees a key press do that, and FND-RNG-068 reads what a view click does in between.

## Alternatives

The path through `+07F1`, the other values of the first argument and the jump table at `+0855`
were not read here; they may also end combat or store 9999 through `DI`.

## How to reproduce

`overlay_listing.py <dsun> 173 5AEBA 5AEE3` and `... 173 5B032 5B0F5`; disassemble `28C9:2146`
to `28C9:2164` from the load image.
