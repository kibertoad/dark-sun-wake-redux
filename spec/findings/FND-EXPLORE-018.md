---
id: FND-EXPLORE-018
title: The byte at 4E71:0000 is stored only with 1, by the direction key's step, the Walk click and the step pop, and no instruction found reads it
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:033F..28C9:034A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:04FB..28C9:0506
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:2D71..31E0:2D7C
tool: Capstone 5.0.9 over resident and overlay code; tools/research/exec-census/segment_offset_refs.py and immediate_search.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Addresses are static, load image at segment 1000. Segment `4E71` is `0x3E71` in load-image code
and the overlay segment word `0x370` in overlay code.

`segment_offset_refs.py <dsun> 4E71 0` lists three sites, each `mov ax, 0x3e71; mov es, ax;
mov byte ptr es:[0], 1`:

| Site | Routine |
| --- | --- |
| `28C9:033F` | the direction key's step `28C9:01CB`, after it plans a move (FND-EXPLORE-004) |
| `28C9:04FB` | the Walk click, after `2D40:10AE` and the no-path message test (FND-EXPLORE-014) |
| `31E0:2D71` | `31E0:2D6E`, the pop of the next step at a cell (FND-RNG-072) |

The two sites FND-EXPLORE-004 and FND-RNG-072 located are the positive control. With `4C10 19`
the same tool lists 106 sites, overlay sites through segment word `0x2E0` among them.

`immediate_search.py <dsun> 3E71 370` lists every instruction with either word as an immediate or
displacement. Of the pushes of either word, none is followed by a push of the constant 0; the 16
followed by a pushed register take offsets `di × 0x22 + 0x9CD`, `di + 0xA55`, `10 × i + 0x24D`,
`10 × i + 0x257`, or the constant 7 (overlays 186, 193 and 209).

## Interpretation

The byte is set to 1 whenever a move is planned or a step popped, and nothing found reads it, so
it has no effect the code shows.

## Alternatives

The tool finds only reads that load the segment word into a register and address offset 0 within
six instructions. A read through a far pointer held in memory (`les`), an offset computed into a
register, a block copy or save of segment `4E71` from offset 0, or a segment register kept longer
than six instructions is not excluded. FND-COMBAT-045 and FND-COMBAT-056 dump the segment from
offset 0 in runs; the save entries of FND-SAVE-015 start at `4E71:024D` and above.

## How to reproduce

`segment_offset_refs.py <dsun> 4E71 0` and `... 4C10 19`; `immediate_search.py <dsun> 3E71 370`,
then a disassembly of the instructions after each push of `0x3E71` or `0x370`.
