---
id: FND-RNG-043
title: Overlay 206 draws twice per pixel of a random-order dissolve of a removed combatant's picture after unloading it, and once with roll_sum to choose a combatant's cry; the removal list of overlay 173 and overlay 179 +2A05 reach them
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
    offset: 0x0005CBE4..0x0005CE1B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066CE5..0x00066D26
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004F90A..0x0004F91E
tool: tools/research/exec-census/direct_callers.py, trampoline_target.py, overlay_listing.py, segment_displacement_writes.py and immediate_search.py at commit 8e5130b3; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
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
`+0x13` and `+0x14` bytes, mode 2 and delay 1 (`+0FAA..+1041`). Before that call it calls the
unload `31E0:05BB` (FND-PARTY-140) at `+1023` with the record's word `+1`, 1, 0 and 1, and then
`2C5F:0139` (`+1019..+102B`). A slot of 4 or more that fails the view test, or whose state byte
is 1, gets only the unload, at `+1067`, with the word `+1`, 1, 1 and 1 (`+1048..+106C`); another
state byte gets neither.

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
at `DS:67D6 + 37 * slot` is not 430; for a slot of 4 or more, overlay 173 `+3136` (FND-RNG-041)
follows at `+0FA4` when its state byte is 2, then the unload and the dissolve when the dissolve is reached.

### What reaches them

`direct_callers.py` gives `+0E1E` one caller, overlay 173 `+24E8`, and `+0B81` one, overlay 179
`+2A27`, each through its trampoline (`5799:003E`, `5799:0048`). Overlay 173 `+24C7` takes slots
from the list of up to 42 words at `4C4F:000A`, whose count is the word at `4C4F:0004`, last
added first, and calls `+0E1E` for each until the count is 0 (`+24CB..+24F9`). Overlay 173
`+2495` adds a slot to that list when the count is below 42 (`+2498..+24C5`);
`segment_displacement_writes.py <dsun> 4C4F 4` lists only its increment and `+24C7`'s decrement
as writes of the count. `+2495`'s two callers are in overlay 173 `+22C4`, which takes a slot and a
damage `n`, returns 0 for a slot of state byte 0 or an `n` of 0 or less, and otherwise:

- for state byte 2, with `h` the combatant's word `+0`: when `h + 10` is not above `n` it sets
  byte `+0x14` to 8 for an `n` of 10,000 or more and to 7 otherwise and `h` to 0; else when `n`
  is `h` or more it sets byte `+0x14` to 3 and `h` to 0; else it takes `n` from `h`. When byte
  `+0x14` is then not 1 it calls overlay 173 `+306B` with the slot and 0, and when it is above 2
  it calls `+30C7` with the combatant, clears the slot from the 42 words at `4E71:0AC6`, sets the
  word at `DS:55CF + 6 * slot` to -1 and adds the slot through `+2495` at `+244D`; it then sets
  the byte at `4C4F:00B2 + c` to 1 (`+2321..+245C`);
- for state byte 1, it adds the slot at `+2486` when byte `+3` of the type record at
  `DS:19CD + 15 * t` is below `n`, `t` being word `+0xA` of the item record at
  `DS:19C1 + 23 * c` and `c` the slot's combatant word (`+245E..+248A`).

Overlay 179 `+2A05` takes a target and three more words and does not read its second; it calls
overlay 193 `+1AA4` with the target and 5, then `+0B81` with its fourth argument, the target and
its third argument when the word at `DS:0DAB` is 1, or overlay 206 `+1121` with the target
otherwise, and then `+22C4` with the target and its fourth argument (`+2A0A..+2A3F`). `direct_callers.py` lists `+24C7`'s callers
as overlay 173 `+084C` and `+2290`, a far word at load image `2997:0003`, overlay 188 `+1487` and
`+1562`, overlay 193 `+1485`, overlay 195 `+0780` and overlay 204 `+02A4` and `+1534`; `+22C4`'s
as overlay 179 `+0D9C`, `+1D22`, `+27BF`, `+27D3` and `+2A3A`, overlay 195 `+0295`, `+050E`,
`+0825`, `+0845`, `+0885` and `+0A5A` and overlay 204 `+1277`; and `+2A05`'s as overlay 179
`+1078`, `+141D`, `+1470`, `+14D9`, `+1500` and `+21A1`, overlay 188 `+1478` and `+1553`, overlay
193 `+1215`, overlay 197 `+1A85`, `+2ED3` and `+3ADE` and overlay 211 `+1EF5`.

## Interpretation

These six sites serve presentation. A non-party combatant removed by `+0E1E` while its picture
is in view is unloaded and then dissolves over `N` pixel copies with two draws each, so a 40 by
40 picture makes 3,200 draws, and the number of draws depends on the picture's size and place.
The cry routine makes at most one draw, through `roll_sum`, for a party member or a combatant
without its own sound. `+22C4` applies damage: a combatant whose hit points fall to 0 or below
is marked 3, 7 or 8 and queued, and the queue is emptied, last in first out, into `+0E1E`. The
flag 0 cry comes from `+2A05` before the damage is applied when the word at `DS:0DAB` is 1.

## Alternatives

- With a height byte below the width byte, an object whose bottom edge is above the view can
  pass the vertical test. `h` is then 0 or negative after clipping; 0 makes no draw, and a
  negative `h` makes `N` the low word of a negative product, up to 65,535 iterations, with `y`
  computed from the zero-extended `h`. This follows from the code; no run has shown it.
- Which game events reach `+2A05`, `+22C4` and `+24C7` through their callers listed above, and
  whether `+0E1E`'s calls other than `+151E` and overlay 173 `+3136` draw (among them overlay 197
  `+30ED`, `31E0:05BB` and `2C5F:0139`), are not read here.
- The voice tables have no direct writer in the search; a computed or bulk write is not ruled
  out. `1BF3:2C18` and `1BF3:2AF4` are taken as the pixel read and write from their arguments
  only; their bodies were not read.
- This finding replaces FND-RNG-030, which placed the call of `31E0:05BB` after the dissolve. The
  listing has it at `+1023`, before the dissolve's call at `+103E`. This finding also adds the
  unload's arguments, the branch for a slot outside the view or of state byte 1, and the callers
  of `+0E1E` and `+0B81` with the removal list and `+22C4`. Everything else FND-RNG-030 records is
  unchanged here.
- The overlay is outside the resident harness; no emulated call or run was made.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`: resolve the sixteen
trampolines `5799:0020` to `5799:0095` and `5671:005C`, `5671:00AC`, `5671:00B1` and `573B:0057`
with `trampoline_target.py`; `direct_callers.py <dsun> 206+1257 206+151E 206+0B81 206+0E1E
206+1121 173+22C4 173+2495 173+24C7 179+2A05`; `overlay_listing.py <dsun> 206 8E950 9004C`,
`173 5A920 5EA7C` and `179 642E0 66F95` and read the ranges above; `segment_displacement_writes.py
<dsun> 4C4F 4`; `immediate_search.py <dsun> 2909 2911 290A 2912 2908 2904`. The table bytes are at
file `0x4F90A..0x4F91E` (MZ header 0x5200 bytes, `DS` segment `57E0`). Output is kept under
`GAME_DIR/analysis/rng-callers-20261020/`. Nothing was executed.
