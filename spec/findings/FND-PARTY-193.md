---
id: FND-PARTY-193
title: View Character's Esc branch (overlay 190 +1953) and the gate routine overlay 182 +1167, which START GAME calls, each give combat_mark 1 to every slot whose combatant word +0x06 is nonzero and whose mark is 0
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
tool: Python 3.14.7 with Capstone 5.0.7 (tools/research/exec-census/overlay_listing.py, direct_callers.py) and a byte search of the file
environment: null
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079C93..0x00079D0A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000699B7..0x00069A0E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000812E3..0x000812EA
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`, where overlay 182's code starts at `0x68850`, overlay 190's at `0x78340` and overlay
194's at `0x81130`. The combatant table is the far pointer at `DS:19C9`, 49-byte records
(FMT-COMBAT-001), whose byte `+0x14` is `combat_mark`.

**Overlay 190 `+1953..+19C7`.** When the dword at `[bp + 0x1E]` is nonzero and the word at
`[bp + 0x12]` is `0x11B` (the Esc key's code, as FND-PARTY-070 reads it for generation), it calls
the far routine at `[bp + 0x1E]` (`+1961`). Then for `si` from 0 to 3 (`+1964`, `+199D..+19A1`),
when the word at `+0x06` of record `si` is not 0 and its byte `+0x14` is 0, it stores 1 to that
byte (`+1966..+199D`). It then calls `28C9:2522` (`+19A3`); when that returns 0 it calls overlay
194 `+037B` with 0 (`+19AC..+19AE`), and otherwise overlay 182 `+19F8`, `3D72:0D83` and
`28C9:2A81` with 1 (`+19B5..+19C1`). Both ways it jumps to `+1EC9` (`+19C7`).

**Overlay 182 `+1167`.** After calling overlay 187 `+2B6C` and its own `+236F` when the byte at
`DS:13F7` is not 0 (`+116E..+117C`), it runs the same loop over `si` from 0 to 3 with the same
tests and store (`+117F..+11BC`). The test of `DS:0DAB` that FND-PARTY-126 reads at `+11BE..+11C5`
comes after it.

**The two copies.** A search of the file for the 25 bytes `8A 47 14 B4 00 0B C0 75 10 8B C6 6B C0
31 8B 1E C9 19 03 D8 26 C6 47 14 01` (load the byte, test it for 0, store 1) finds them at file
`0x699EF` (overlay 182 `+119F`) and `0x79CC4` (overlay 190 `+1984`), and nowhere else.

**START GAME.** Overlay 194's Start Game branch (`+013E`, FND-PARTY-026) calls `+1167` with 0 at
`+01B5`, after calls of overlay 187 `+2424` and `+206E` (`+019F..+01B0`).
`direct_callers.py` finds the other call of `+1167` at overlay 192 `+0C32`, in the confirm branch
of the Load Saved Game window (FND-PARTY-045).

## Interpretation

Leaving View Character with Esc, starting the game and confirming a saved game each turn a
`combat_mark` of 0 into 1 for every slot that holds a character, if the word at `+0x06` is what
marks a filled record. So a character made with `NEW`, which `DONE` stores with mark 0
(FND-PARTY-189, `+1391`), has mark 1 before overlay 173 `+3CDA` reads it at the start of play
(FND-PARTY-185), as stored characters loaded by ADD do (FND-PARTY-183). A party reaches
`+3CDA` with no standing member only if every filled slot has a mark of 3 or more, or a mark of
1 or 2 and hit points below 1.

## Alternatives

- What the word at `+0x06` holds was not read here. FND-PARTY-050 lists the loaded fields as hit
  points, identifiers, image offsets and marks without naming `+0x06`.
- The search covers this exact instruction sequence only. A store of 1 to the byte written
  another way, or a loop over a different register, would not be found.
- Whether the Esc branch is the only way out of View Character to the title screen was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 190 79C60
79D20`, `182 699A0 69A10` and `194 81280 812F0`; `direct_callers.py <dsun> 182+1167`. Search the
file for the 25 bytes given above.
