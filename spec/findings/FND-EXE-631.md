---
id: FND-EXE-631
title: Of ten overlay routines that no direct call reaches, only overlay 181 +0648 has its trampoline's far address in the code, pushed by overlay 181 +0480 as the callback of eighteen Game Menu controls
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000683B7..0x0006840D
tool: tools/research/exec-census/far_pointer_words.py and overlay_listing.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9
environment: null
---

## Observation

The placement findings FND-EXE-606 to FND-EXE-630 leave ten routines with no direct call and no
registration they found: overlay 191 `+0A94` (trampoline `572F:005C`), overlay 179 `+0B93`
(`56A7:0057`), overlay 170 `+03B3` (`5660:0025`), overlay 193 `+273B` (`573B:00BB`), overlay 188
`+1745` (`5702:00ED`) and `+17C7` (`5702:00F7`), overlay 181 `+0648` (`56B9:002F`), overlay 176
`+05F6` (`5691:007A`) and `+0757` (`5691:005C`), and overlay 210 `+06CD` (`57B9:0052`). A far
call or a stored far pointer to any of them has to hold the trampoline's offset next to the
trampoline's segment: in the load image a relocated word equal to the header segment less
`0x1000`, in overlay code a fixed-up word equal to the overlay's descriptor times 8.

`far_pointer_words.py` lists every word of `DSUN.EXE` equal to each trampoline offset. None is a
far call or relocated pointer to the trampoline's own segment. The ones it reports as immediates
near a segment word were checked for the segment word of the trampoline's overlay within 12 bytes
either side: only two are, in overlay 181 (code from file offset `0x67EB0`):

- `+0507..+0532` pushes `0x5A8` (descriptor 181) and `0x2F`, the control number `si` widened to
  a double word, and the double word at `4E46:0000`, and calls `3F96:02AB`, for `si` from
  `0x27D9` to `0x27E3`;
- `+0532..+055D` does the same for `si` from `0x27E5` to `0x27EB`.

Both loops are in the routine at `+0480`, after it stores the window that overlay 182 `+016B`
returns at `4E46:0000` (the failure message at `DS:0F16` names it `WIND_GAMEMENU`) and passes
`56B9:0034` to overlay 190 `+11A1`.
`3F96:02AB` is the routine that FND-CONFIG-074 and FND-CONFIG-092 find window code passing a
frame callback to.

## Interpretation

Overlay 181 `+0648`, the hover-label routine FND-EXE-617 reads, is the callback of the Game
Menu's controls `0x27D9` to `0x27E3` and `0x27E5` to `0x27EB`, which matches the control numbers
it tests. The other nine routines have no far address anywhere in the file that this search
recognises; if they run, it is through a far pointer built at run time from separate words or
taken from a file, or they are dead code.

## Alternatives

A pointer whose offset and segment words are written by separate instructions more than 12 bytes
apart, or computed, or read from a data file, is not found by this search. A routine with no
caller may still run in a state no static search reaches; a breakpoint on its trampoline during
play would show it.

## How to reproduce

`far_pointer_words.py <dsun> 572F:005C 56A7:0057 5660:0025 573B:00BB 5702:00ED 5702:00F7 56B9:002F
5691:007A 5691:005C 57B9:0052`, then for each "immediate near a segment word" hit look for the
little-endian word of the trampoline's descriptor times 8 (`0x5F8`, `0x598`, `0x530`, `0x608`,
`0x5E0`, `0x5A8`, `0x580` or `0x690`) within 12 bytes of it. `overlay_listing.py <dsun> 181
683B7 6840D` shows the two loops.
