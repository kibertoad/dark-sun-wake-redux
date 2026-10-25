---
id: FND-UI-062
title: The inventory's position map is a 96-byte request, and Split with a full backpack has overlay 189 +1AD7 read the word 518 bytes into it and, unless that word is 0xFFFF, draw from the graph entry it names
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007BB65..0x0007BB77
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00076487..0x0007650E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00076578..0x0007663D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00077A92..0x00077AE9
tool: Capstone 5.0.9 over overlay code; tools/research/exec-census/overlay_listing.py and immediate_search.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 189's code starts at file `0x749B0`
and overlay 190's at `0x78340`.

The inventory opener in overlay 190 calls `444C:00FA` with the double word `0x60` at `+3825` and
stores the far pointer it returns in `DS:617D` at `+3830..+3837`. `immediate_search.py` with
`617D` and `617F` finds no other store of either word in the load image or the overlays besides
overlay 189's release at `+0490..+04A4`, which stores the result of `444C:0092`.

Overlay 189 `+1AD7` takes a logical position `p`. It first calls `+30E2` with `p` and four local
addresses; `+30E2` asks `3F96:01A6` for frame `0x2BCC + p` of the window at `5072:003C` and copies
the four words it gets into the locals. Then:

- below 4 it returns;
- from 4 to 29 it reads the word `2p` bytes into the block at `DS:617D`;
- from 30 to 35 it reads the word at `DS:9B3E + 2p` and uses the list at `DS:619D`;
- from 36 up it reads the word `2p` bytes into the block at `DS:617D`.

On 0xFFFF it returns. Otherwise it multiplies the word by 10, adds it to the current member's
graph buffer at `DS:6189 + 4 * [4E71:0B44]`, takes the entry's word `+4` as a 23-byte record
number under `DS:19C1`, reads that record's word `+0x0C` and goes on to draw.

## Interpretation

`444C:00FA` asks the allocator for one byte more than its argument (FND-EXE-293), so the map is
a 97-byte request holding the 48 words that FND-PARTY-217's fills cover. The Split branch of the
inventory handler passes 259 when the backpack has no free position (FND-UI-063). `+1AD7` then
reads the word 518 bytes from the map's start, 421 bytes past the end of the request, in heap
memory nothing in the map's code writes. When that word is not 0xFFFF, the routine draws the
item of whatever graph entry it names, at the place `3F96:01A6` gives for frame 11471, a number
no inventory frame has (FND-UI-030). Before it, `+3139` changed nothing, so the held stack keeps
its full quantity.

## Alternatives

What the heap holds at that offset, what `3F96:01A6` returns for a frame the window lacks, and
so what appears on screen, were not read; a run with a full backpack and a held stack would show
it.

## How to reproduce

`overlay_listing.py <dsun> 190 7BB40 7BC40`, `... 189 76487 7650E`, `... 189 76578 7663D` and
`... 189 77A92 77AE9`; `immediate_search.py <dsun> 617D 617F`.
