---
id: FND-RNG-030
title: Overlay 206 draws twice per pixel of a random-order dissolve of a removed combatant's picture, and once with roll_sum to choose a combatant's cry
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008F577..0x0008F58C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008F76E..0x0008F9C3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008FBA7..0x0008FD43
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008FE6E..0x0009004C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004F90A..0x0004F91E
tool: tools/research/exec-census/direct_callers.py, trampoline_target.py, overlay_listing.py and immediate_search.py at commit c40653e3; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 206's code starts at file `0x8E950`
and is `0x16FC` bytes. Its sixteen trampolines (header segment `5799`) enter `+0000`, `+0240`,
`+0308`, `+093A`, `+0986`, `+0A4E`, `+0A7A`, `+0AEF`, `+0B49`, `+0B81`, `+0E1E`, `+1073`,
`+1121`, `+1257`, `+13F3` and `+151E`. Decoding `+1257` and `+151E` from their entries to their
far returns shows FND-RNG-028's six overlay-206 sites as instructions: the generator calls
`+1360` and `+1382` in `+1257`, and the `roll_sum` calls `+15B8`, `+1600`, `+16A6` and `+16E8`
in `+151E`. The direct-call query finds one near caller of `+1257`, at `+103E`, and two of
`+151E`, at `+0C2C` and `+0E77`, and no far call through their trampolines.

### The dissolve, `+1257`

`+1257` takes `x0`, `y0`, `x1`, `y1` (signed words), a `mode` byte and a `delay` byte. It raises
`x0` and `y0` to 0 when negative and lowers `x1` to 320 and `y1` to 200 when above them. Mode 0
copies the rectangle row by row and mode 1 column by column, each pixel through `1BF3:2C18` with
buffer 1 and `1BF3:2AF4` with buffer 0, waiting `delay` milliseconds through `1000:12FA` after
each row or column; neither draws. Other modes do nothing.

Mode 2 sets `w = x1 - x0` and `h = y1 - y0` as words and `N` to the low word of their signed
product. For `k` from 0 while `k < N` (unsigned) it draws twice: `x = x0 + (draw * w) / 32768`
at `+1360` and then `y = y0 + (draw * h) / 32768` at `+1382`, each with the draw sign-extended,
the size zero-extended and a signed 32-bit division. It copies pixel `(x, y)` from buffer 1 to
buffer 0 as above and, when `k` is a multiple of 10, waits `delay` milliseconds. It then calls
`28C9:2A81` with 1. So mode 2 makes `2 * N` draws, in x-then-y pairs; the pixels are chosen with
repetition, so some are copied more than once and others not at all.

The one caller, in `+0E1E` (`+0FAA..+1044`), runs only for a slot of 4 or more whose state byte
at `4F49:0C33 + 3 * slot` is 2. From the slot's record at `DS:67BB + 37 * slot` it takes the
position words at `+3` and `+5` and the size bytes at `+0x13` and `+0x14`, and goes on only when
`x < DS:1408 + 320`, `x + byte(+0x13) > DS:1408`, `y < DS:140A + 200` and
`y + byte(+0x13) > DS:140A`: the vertical test adds the `+0x13` byte, the one the horizontal test
uses, not `+0x14`. It then calls `+1257` with `x - DS:1408`, `y - DS:140A`, those plus the
`+0x13` and `+0x14` bytes, mode 2 and delay 1, and afterwards `31E0:05BB` (FND-PARTY-140).

### The cry, `+151E`

`+151E` takes a slot `s` and a flag and returns a byte, which both callers pass to `2C5F:0A22`,
the sound-effect player (FND-SOUND-007). `+0C2C` passes flag 0; `+0E77`, in `+0E1E`, passes flag
1. With `d` the details record of the slot's combatant (the combatant record at `DS:19C9 + 49 *`
the word at `4F49:0C34 + 3 * s`, whose word `+4` indexes `DS:19C5` in 66-byte records):

| Case | Flag 1 | Flag 0 |
|---|---|---|
| state byte `4F49:0C33 + 3 * s` is not 2 | 48 | 98 |
| byte `+0x3C` of `d` is not 0 and `s` is 4 or more | that byte | that byte |
| gender byte `+0x13` is 1 | `DS:2911 + roll_sum(1, 12)` | 153 + `roll_sum(1, 5)` |
| gender 2 and origin byte `+0x12` is 8 | 228 | 228 |
| gender 2, other origin | `DS:2909 + roll_sum(1, 8)` | 140 + `roll_sum(1, 4)` |
| other gender | 61 | 98 |

The cases are tested in that order. Each `roll_sum` call is one draw. The returned total is used
unsigned, as a byte offset for flag 1 and added in a byte for flag 0. The file holds, at
`DS:290A..2912`, the bytes 19, 137, 138, 139, 140, 145, 147 and 148 (female, rolls 1 to 8) and,
at `DS:2912..291E`, 18, 133, 177, 178, 179, 180, 182, 183, 190, 195, 211 and 172 (male, rolls
1 to 12). The immediate search for `2909`, `290A`, `2911` and `2912` finds no instruction
other than the two table reads; its control `2904`, the far pointer just before the tables,
returns the overlay's known uses of it.

In `+0E1E` the cry call comes first, when the state byte is 2 and the absolute value of the word
at `DS:67D6 + 37 * slot` is not 430; the dissolve, when reached, follows.

## Interpretation

These six sites serve presentation. A non-party combatant removed by `+0E1E` while its picture
is in view dissolves over `N` pixel copies with two draws each, so a 40 by 40 picture makes 3,200
draws, and the number of draws depends on the picture's size and place. The cry routine makes
at most one draw, through `roll_sum`, for a party member or a combatant without its own sound.
No rule entry describes either yet; RULE-SOUND-001 describes only how the chosen number plays.

## Alternatives

- With a height byte below the width byte, an object whose bottom edge is above the view can
  pass the vertical test. `h` is then 0 or negative after clipping; 0 makes no draw, and a
  negative `h` makes `N` the low word of a negative product, up to 65,535 iterations, with `y`
  computed from the zero-extended `h`. This follows from the code; no run has shown it.
- Which game events call `+0B81` (the flag 0 cry) and `+0E1E` is not read here, nor whether
  `+0E1E`'s earlier calls, among them overlay 173 `+3136` and overlay 197 `+30ED`, draw.
- The voice tables have no direct writer in the search; a computed or bulk write is not ruled
  out. `1BF3:2C18` and `1BF3:2AF4` are taken as the pixel read and write from their arguments
  only; their bodies were not read.
- The overlay is outside the resident harness; no emulated call or run was made.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`: resolve the sixteen
trampolines `5799:0020` to `5799:0095` with `trampoline_target.py`; `direct_callers.py <dsun>
206+1257 206+151E`; `overlay_listing.py <dsun> 206 8E950 9004C` and read the ranges above;
`immediate_search.py <dsun> 2909 2911 290A 2912 2908 2904`. The table bytes are at file
`0x4F90A..0x4F91E` (MZ header 0x5200 bytes, `DS` segment `57E0`). Output is kept under
`GAME_DIR/analysis/rng-callers-20261020/`. Nothing was executed.
