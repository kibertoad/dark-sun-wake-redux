---
id: FND-INPUT-014
title: The BIOS shift-flag routine 44B6:0011 has eleven resident and ten overlay far callers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44B6:0011..44B6:0018
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:2E3B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:0623
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066D62
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007270B
tool: tools/research/exec-census/direct_callers.py and resident_listing.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

The shipped `DSUN.EXE` holds the bytes `CD 16` (`INT 16h`) at five file offsets. Four are
instructions of the resident image: `44B6:0013` in the shift-flag routine `44B6:0011` of
FND-INPUT-005, and `44B6:00EF`, `44B6:0110` and `44B6:011C` in the keyboard interrupt hook
`44B6:00B5`. The fifth, `0x000966F6` in overlay 211's code (`+08E6`), is the displacement of the
near call at `+08E5`, which follows a `push cs`.

`direct_callers.py` finds 21 far calls to `44B6:0011`:

| Where | Call sites |
| --- | --- |
| Resident `28C9` | ten, at file offsets `0x1E4B3`, `0x1E4E5`, `0x1E7D1`, `0x1E7E2`, `0x1E855`, `0x1E866`, `0x1E94C`, `0x1E95D`, `0x1E9BF` and `0x1E9D0` (from `28C9:0623` to `28C9:0B40`) |
| Resident `31E0` | one, at `31E0:2E3B` (file offset `0x29E3B`) |
| Overlay 179 | `+2A82`, `+2A96` (`0x66D62`, `0x66D76`) |
| Overlay 183 | `+161C` (`0x6C5FC`) |
| Overlay 187 | `+28DB`, `+298B` (`0x7270B`, `0x727BB`), both in `+28B3` |
| Overlay 190 | `+1E86` (`0x7A1C6`) |
| Overlay 196 | `+03E7` (`0x83357`) |
| Overlay 208 | `+0C69`, `+11CB` (`0x924F9`, `0x92A5B`) |

The overlay calls use the segment word of the routine's descriptor from the overlay's fixup
list. The hook (at `44B6:0115`) and `44B6:0063` call it with `push cs` and a near call. At
`31E0:2E3B` the caller has found a 37-byte record whose first byte has `0x10` set and masks the
result with 3; at `28C9:0623` the caller tests the result against 3.

## Interpretation

Several resident and overlay routines test whether a shift key is held.

## Alternatives

This replaces FND-INPUT-011, which counted the eleven resident far callers only and titled them
all of the routine's far callers; its search covered the load image's relocations and not the
overlays' fixup lists. Its other observations are unchanged. What holding shift does at each
caller is not recovered.

## How to reproduce

In `tools/research/exec-census/`, run `python -I direct_callers.py <DSUN.EXE> 44B6:0011` and
`python -I resident_listing.py <DSUN.EXE> 44B6:0000..44B6:0130`, and search the file for the bytes
`CD 16`. `overlay_listing.py <DSUN.EXE> 211 966E8 96700` shows the call at overlay 211 `+08E5`.
Nothing is executed.
