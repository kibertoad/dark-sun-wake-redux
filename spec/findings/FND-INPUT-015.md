---
id: FND-INPUT-015
title: The routine at 0x71659 that FND-INPUT-009 took for a dispatcher is overlay 187's GPLI reverse remap, which no keyboard caller reaches
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071659..0x00071869
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56EF:00AC
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay 187's code starts at file offset `0x6FE30`; its header segment is `56EF`. The routine at
`0x71659` is its `+1829`, entered from other overlays and the resident code through the
trampoline `56EF:00AC`. Decoded from `+1829` to its `retf` at `+1A38`, it has no indirect jump
and no table of cases. It asks `38FF:05B5` for the size of `GPLI/1` (it pushes `1` and the tag
`0x494C5047`, "GPLI") and reads the resource through `38FF:04AB`, calling overlay 180's `+0867`
through trampoline `56B2:0034` when either fails. It divides the size by 6 and walks the same
records as `+1615` (FND-EXE-596), replacing each word equal to an entry's first word with the
entry's second and third words, and frees the table through `444C:0092`.

`direct_callers.py` finds two calls to `+1829`, both `push cs` and a near call inside overlay
187: at `+0A61` in the numbered-save writer `+08E3` (FND-COMBAT-034) and at `+0CBD` in the
numbered save reader `+0A6C` (FND-COMBAT-035). The far callers of the shift-flag routine
`44B6:0011` in overlay 187, `+28DB` and `+298B`, lie in `+28B3`, the cinematic fallback of
FND-VIDEO-006 (FND-INPUT-014).

## Interpretation

The routine at `0x71659` converts the selector words of the party and map records back from
`GPLI/1`'s numbering when a game is saved and loaded. It handles no key and is not reached from
the keyboard routines; FND-INPUT-009's dispatcher reading does not hold.

## Alternatives

This replaces FND-INPUT-009, which read `0x71659` as a dispatcher whose fourth case requests
`GPLI/1`, and said only one of the five `CD 16` encodings decodes as an instruction. The
routine has no cases, and four of the encodings are instructions (FND-INPUT-014). FND-INPUT-009
took the routine from Ghidra's references one layer above the callers of `44B6:0011`, which
missed the overlay far callers. Whether any routine above those callers maps keys to actions is
not read here.

## How to reproduce

In `tools/research/exec-census/`, run `python -I overlay_listing.py <DSUN.EXE> 187 71659 71869`
and `python -I direct_callers.py <DSUN.EXE> 187+1829 44B6:0011`. Nothing is executed.
