---
id: FND-PARTY-188
title: Overlay 186 +0421, the character sheet's experience line, has three callers; two pass 4E4F:0029 right after overlay 184 +1DCE fills it, and the third, overlay 184 +0462, passes the record at DS:1429 and runs only under overlay 183 +0A3E and +10FF
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
    offset: 0x0006D3CE..0x0006D4FA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009463D..0x00094683
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000985D5..0x00098627
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006C0DF..0x0006C17A
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Overlay 183's code starts at file `0x6AFE0`, overlay 184's at `0x6D090`, overlay
209's at `0x93160` and overlay 212's at `0x98200`.

**The callers.** `direct_callers.py` finds three calls of overlay 186 `+0421` (FND-PARTY-058),
all through trampoline `56E9:0048`, and none near:

- Overlay 209 `+151E`. Just before it, `+14DD..+1504` pushes the combatant and details records
  numbered by the routine's argument `[bp + 6]` and calls overlay 184 `+1DCE` (FND-PARTY-080);
  `+1507..+151D` then pushes `0x00660004`, `4E4F:0029` (`push 0x360`, `push 0x29`) and the dword
  at `55BD:0004`, with no call or store between.
- Overlay 212 `+0422`. Just before it, `+03D5..+0408` pushes the records numbered by the word at
  `4E71:0B44` and calls `+1DCE`; `+040B..+0421` pushes `0x00780095`, `4E4F:0029` and the dword at
  `55CA:0000`, with no call or store between.
- Overlay 184 `+0462`, in the routine at `+033E` (trampoline `56DD:005C`). It pushes `0x121C`
  and the word at `DS:121E` less 2, then the dwords at `DS:1429` and `DS:1431` (`+044D..+0461`),
  after the same pattern with the dwords at `DS:1429` and `DS:1431` for the calls of overlay 183
  `+1783`, `+1827`, `+1867` and `+1881` (`+0361..+0431`).

**Callers of overlay 184 `+033E`.** `direct_callers.py` finds three, all far through `56DD:005C`
from overlay 183: `+0C89` in the routine at `+0A3E`, which sets the class bytes of the record at
`DS:1429` to the codes 1 to 8 the generation buttons pass (FND-PARTY-073), and `+1137` and
`+118C` in the routine at `+10FF`, each after a call of `2D40:3BEC` and before overlay 186
`+055E`. The only caller found of `+10FF` is overlay 184 `+0E6D` through `56CC:0089`, in the
routine whose `+0E62` calls `+0A3E` with codes 3, 4, 5 and 8 (FND-PARTY-073).

**Writers of `DS:1429`.** A search of the file for a word or dword store to `DS:1429` or
`DS:142B` with a direct address (`mov` from a register with opcode `89` or `A3`, with or without
the `66` prefix, `mov` of an immediate with `C7 06`, `pop` with `8F 06`) finds these, each beside
a store to `4E71:0B44`:

| Site | Value stored |
| --- | --- |
| overlay 184 `+07E9` (in `+07D8`) | `4E4F:0029` |
| overlay 190 `+062E`, `+0632` | the details record in the table at `DS:19C5` numbered by `4E71:0B44` |
| overlay 190 `+3705`, `+3709` | the same |
| overlay 209 `+023E`, `+0242` | the same |
| overlay 209 `+0AAA`, `+0AAE` | the same |
| overlay 202 `+028F` | the routine's first argument |
| overlay 211 `+0329` | the details record numbered by a register the routine computed |
| overlay 212 `+0631` | the routine's first argument |

## Interpretation

The sheet's next-level figure reads its class bytes from `4E4F:0029` on two of the three routes,
and on those the bytes are the ones `+1DCE` has just written from the 17 codes to codes 1 to 8:
for overlay 212 from the same slot's record it copies, and for overlay 209 from the slot in
`4E71:0B44`, which may differ from the record copied (FND-PARTY-080). The third route prints the
record at `DS:1429` and is reached only from the generation routines of overlay 183, which work
on that record as codes 1 to 8. Six routines outside generation point `DS:1429` at a stored
details record, whose class bytes hold the 17 codes; if one ran after the last call of overlay
184 `+07D8` and before the generation screen drew its sheet, the figure would come from the
wrong table as BUG-PARTY-001 describes.

## Alternatives

- The searches cover direct-address stores. A store through a pointer, a string move or a block
  copy onto `DS:1429` would not be found; none was looked for.
- Whether the generation screen can be reached without `+07D8`, or can call one of the routines
  of overlays 190, 202, 209, 211 and 212 above, was not traced.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, run in `tools/research/exec-census/`: `direct_callers.py <dsun> 186+0421
184+033E 183+10FF 183+0A3E 184+1DCE`; `overlay_listing.py <dsun> 184 6D360 6D500`, `209 94640
94690`, `212 985C0 98630`, `183 6C0DF 6C180`, `190 788F0 78990`, `190 7BA00 7BA60`, `202 8A700
8A750`, `209 93360 933B0`, `209 93BD0 93C20`, `211 96110 96150` and `212 98800 98850`. Search
the file for `89` with a ModRM byte from `06` to `3E` in steps of 8, then `29 14` or `2B 14`; the
same after `66`; `C7 06 29 14`, `C7 06 2B 14`, `8F 06 29 14`, `A3 29 14` and `A3 2B 14`.
