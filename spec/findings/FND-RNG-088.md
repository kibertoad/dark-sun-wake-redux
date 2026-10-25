---
id: FND-RNG-088
title: DSUN.EXE reads files at five DOS read requests; of the 19 calls of the read routine 44DE:01AF, eight read into frame locals or fixed DS buffers that miss DS:426D, two into blocks just allocated on the far heap, and nine through far pointers held in frame locals, arguments or DS variables
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:01AF..44DE:0209
tool: Capstone 5.0.9 over the load image and overlay code; tools/research/exec-census/reach_config.py with q007_dsun_census.json, the scientific_method_engine reach command, dsun_interrupt_census.py, direct_callers.py, resident_listing.py and overlay_listing.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Addresses are static; segment immediates in load-image code are relative to 0, so `0x1000` is
added. The interrupt census of FND-EXE-574, run again, lists five `int 0x21` sites with `AH` 0x3F,
the DOS read request: `44DE:01E7` and `44DE:0208` in the routine `44DE:01AF`, `4AE5:0293` and
`4AE5:040C` in the overlay manager, and the candidate `15F3:0183`, which FND-EXE-584 finds reached
through the record table at `DS:3F42`. `1000:2F0F` loads `AH` from its first argument
(FND-EXE-574).

`44DE:01AF` takes a handle, a far buffer and a doubleword count. `direct_callers.py` finds 19 far
calls of it, 15 in the load image and four in overlay code. The buffer and count each passes:

| Buffer | Count | Calls |
|---|---|---|
| `DS:4252` | 3 | `2834:046A` (file `0x1D9AA`) |
| `DS:A440` | 6 | file `0x3CE6F` |
| `DS:A43E` | 2 | file `0x3CEEC` |
| `DS:45AC` | `0x1000` | overlay 187 `+224D` |
| a frame local through `SS` | `0x1C`, 8, 1, `0x10` | file `0x2E2AA`, `0x2E931`, `0x3B5AE`; overlay 196 `+018E` |
| the block `444C:00FA` has just returned, allocated with the same count or, at overlay 196, with its low word | `6 × [DS:4252]`; the doubleword read at overlay 196 `+018E` less `0x10` | file `0x1D9F0` (block kept at `DS:424E`); overlay 196 `+01F7` |
| a far pointer in a frame local plus 0, 2 or `0x31` | a frame local, or `[DS:A43E] - 2` | file `0x2DA1A`, `0x2E40B`, `0x2EB92`, `0x3A733`, `0x3CF58`, `0x3D017` |
| the far pointer at `DS:342C` or `DS:3430` | `[DS:A42C]` | file `0x3B65C`, `0x3B68E` |
| the far pointer argument `[bp + 0xA]` | `0x80` | overlay 196 `+014B` |

The file offsets are those of the far call's offset word, as `direct_callers.py` prints them.

## Interpretation

The eight reads into fixed buffers and frame locals miss `DS:426D` and `4C13:0369`: the one
below `DS:426D` is the 3-byte read at `DS:4252`, which ends at `DS:4255`, and the others lie at
`DS:45AC` or above. The two reads into blocks the far heap has just allocated land above `DS`
(FND-RNG-083), overlay 196's only while the count is below `0x10000`. The nine reads through stored far pointers, the overlay manager's two reads,
`15F3:0183` and the callers of `1000:2F0F` are not shown to miss either word.

## Alternatives

Where the nine stored far pointers point was not read, nor the destinations of the overlay
manager's reads, of `15F3:0170`'s caller or of `1000:2F0F`'s callers.

## How to reproduce

From `tools/research/exec-census/`, as FND-EXE-574's How to reproduce says: `reach_config.py
<dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> q007_dsun_census.json`,
`python -I -m scientific_method_engine reach <config> > <report>`, and `dsun_interrupt_census.py
<dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <report> <config>`, keeping the `AH 3F`
lines. Then `direct_callers.py <dsun> 44DE:01AF 15F3:0170 1000:2F0F` and the 48 bytes before each
call; `overlay_listing.py <dsun> 187 72040 72084` and `... 196 83080 8316C`.
