---
id: FND-UI-044
title: Every literal window number 19501 and 19502 is an opener argument; the game-entry routine opens WIND 19501 and two overlay 201 routines open WIND 19502
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: claude
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069A0E..0x00069A83
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00089833..0x00089912
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000897C0..0x00089827
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00089827..0x00089833
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00089BE8..0x00089BF6
tool: tools/research/exec-census/immediate_search.py, far_pointer_words.py, direct_callers.py, callback_args.py, trampoline_target.py, overlay_listing.py and resident_listing.py; Python 3.14.7, Capstone module version 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Overlay 182's code starts at file `0x68850` and overlay 201's at `0x897C0`.
FND-UI-025 records that `RESOURCE.GFF#WIND/19501` holds `APFM/19200` (320 x 200) and
`APFM/19201` (28 x 16 at (10, 10)), and `WIND/19502` one full-window `BUTN/2099`.
FMT-UI-001 and SCR-UI-002 left their use open.

**Literal census.** The immediate and displacement decode at every byte of the load image and of
every flags-3 overlay code span finds these instructions with an operand word `0x4C2C` (19500),
`0x4C2D` (19501) or `0x4C2E` (19502):

| Raw site | Classification |
|---|---|
| overlay 194 `+049B` | `push 0x4C2C`, the start window's opener call (FND-UI-035); positive control |
| overlay 182 `+11D1` | `push 0x4C2D` before the near call of `+016B` at `+11D5` |
| overlay 201 `+0098` | `push 0x4C2E` before the far call of `+016B` at `+009B` |
| overlay 201 `+042E` | `push 0x4C2E` before the far call at `+0431`, FND-PARTY-194's routine; positive control |
| overlay 171 `+0695`, `+0696` | Inside the `jmp` through `cs:[bx+22h]` at `+0693` and the `sub` at `+0697`, whose memory operand is `DS:044C` |
| overlay 199 `+12C1` | Inside the `sub word [044C], 5` at `+12C2`, which follows the 3-byte `jmp` at `+12BF` |

A second pass over every 16-bit word of the file equal to one of the three numbers gives the same
four instruction immediates and, besides them, only the interior bytes above, one more interior
pair at overlay 171 `+062E` (inside the `sub word [044C], 0Ah` at `+062D`) and one word at file
`0x3429`, inside the MZ relocation table (`0x3E` to `0x49BA`). No word in the load image's data
holds 19501 or 19502.

**Opener census.** `direct_callers.py` finds 31 calls of overlay 182 `+016B`: the near call at
`+11D5` and 30 far calls through trampoline `56BD:0048`. Reading the pushes that fill the
callee's first argument word at each one, every call passes a literal window number: 19501 at
182 `+11D5` only, 19502 at 201 `+009B` and `+0431` only, and 19500 at 194 `+049E` only. The
`WIND` load helper that `+016B` calls, 182 `+00C4`, has one other direct caller, overlay 172
`+0078` through trampoline `56BD:0039`, which passes 14000 or 14001. The only other `WIND` tag
immediates are inside `+00C4`, in `3A8E:0C2B` and at `39D1:04BC`. `3A8E:0C2B` looks a
registered window up by the number it is given and returns 1 when none is found, 16 when it is
the one at `DS:A11D`, and otherwise its `0x9E` word masked to the low five bits; its one
direct caller, overlay 183 `+0122`, passes 19505 (`0x4C31`, FND-PARTY-070). `39D1:04BC` compares
a supplied tag with `WIND`.

**WIND/19501.** Overlay 182 `+1167` is the routine that the far calls at overlay 194 `+01B5`, in the Start Game
branch, and at overlay 192 `+0C32` reach through trampoline `56BD:00AC` (FND-PARTY-021). Its first loop sets
byte `+14` of each of the four 49-byte records at the far pointer `DS:19C9` to 1 where the
record's word `+06` is nonzero and that byte is 0. When the word at `DS:0DAB` is nonzero it
jumps to its exit at `+1522` (`+11BE`). Otherwise it calls `+016B` with 19501, a zero dword and the far
pointer `28C9:0CFF` (`+11C8..+11D5`) and stores the returned far pointer at `4E28:00D7`. It
then passes `28C9:0CFF` to overlay 190 `+11A1`, and calls `+01EF` for `APFM/19200` with
`28C9:1261` and `0x01EE`, and for `APFM/19201` with `28C9:0061` and `0x0166` (`+11E9..+1230`,
as FND-PARTY-044 reads), before it stores 1 at `DS:0D9C` (`+1233`).

**WIND/19502 with a handler.** Overlay 201 `+0073` is trampoline 0 (`5778:0020`) of overlay
201; direct far calls reach it from overlay 181 `+028E` and overlay 190 `+1EBD`. It calls
overlay 182 `+1856` and `28C9:2ABD` as `+0410` does, then calls `+016B` with 19502, a zero dword
and the far pointer `5778:002A` (`+0098..+009B`), stores the result at `5499:0000` and at
`DS:1431`, and passes `5778:002A` to overlay 190 `+11A1`. It requests `BMP ` 20120
(`0x4E98`) through `38FF:04AB` and, when that returns 0 with a nonzero pointer, passes it to
`2D40:3BEC` with the words 1, 27 and 10 before it. It then requests `BMP ` number
`20100 + 25 * word DS:140C + byte DS:4459`, after clearing the byte `DS:4459` when it is 25 or
more (signed compare). On a 0 return it reads frame 0's two dimension words through `1BF3:76C2`
and `1BF3:771C` (FND-CONFIG-190), stores `(186 - first) / 2` and `(139 - second) / 2` (unsigned
shift) at `DS:9B06` and `DS:9B04`, and passes the picture to `2D40:3BEC` with 1, `DS:9B06 + 67`
and `DS:9B04 + 30` before it. A nonzero return of the second request jumps to `+02EB`. A loop
over party slots follows from `+01DE`; it and `+02EB` were not read.

`5778:002A` enters overlay 201 `+0000`. That handler subtracts 1 from its first word argument
and returns through `+004F` when the result is above 5. Otherwise it jumps through the six words
at `+0067`, which are `+0058`, `+002E`, `+004F`, `+004F`, `+004F` and `+0013`:

- event 1 calls `+03C9`, stores 0 at `DS:1462` and returns `0xFFFF`;
- event 2 calls `+0317` with a 24-byte copy of its arguments when the word at `[bp+8]` is
  2099 (`0x0833`), the number of the window's one button;
- event 6 calls `+03C9` when the word at `[bp+12h]` is `0x011B`, `0x1C0D` or `0x3920`;
- events 2 and 6 after those calls, and events 3 to 5 and the other cases directly, call
  `2C5F:0182` and return 0.

`+03C9` and `+0317` were not read.

**WIND/19502 with no handler** is FND-PARTY-194's overlay 201 `+0410`, reached from overlay 173
`+3CDA` when no member is standing.

## Interpretation

`WIND/19501` is the window the game-entry routine opens on the Start Game and Load paths, after
the start window and before the party load gate. Its handler is `28C9:0CFF`, the key routine of
FND-AI-004 and FND-EXPLORE-004, and its full-screen frame `APFM/19200` gets the callback
`28C9:1261` (FND-CONFIG-113). That makes it the window that takes exploration input when play
begins; how long it stays open and what draws the region view into it are not read here.

`WIND/19502` is a full-screen picture window used by two routines of overlay 201: with no
handler for the defeat message (FND-PARTY-194), and with handler `+0000` by `+0073`, which draws
BMP 20120 and a picture chosen by `DS:140C` and `DS:4459`. Clicking button 2099, or the keys
`0x011B`, `0x1C0D` and `0x3920` (which by their low bytes are Escape, Enter and Space), reach
that handler's two unread routines. Which screen `+0073` shows is not established.

## Alternatives

- `WIND/19501` and `WIND/19502` are the windows of the View Character screen reached from the
  start window, as earlier notes thought (FND-UI-025): the openers above are the only literal
  uses of either number, and neither is on that path.
- A window number computed at run time, or a window found by a loaded table, opens either
  window elsewhere: every direct `+016B` and `+00C4` caller passes a literal, and no data word
  holds either number. Computed or stored far calls into `+016B`'s trampoline, and numbers
  read from resource files, were not searched.

## How to reproduce

Use installed `DSUN.EXE`, size 634416, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`, and the
locked evidence Python, from the commit that adds this finding, in
`tools/research/exec-census/`:

1. `immediate_search.py <dsun> 4c2c 4c2d 4c2e` and `far_pointer_words.py <dsun> 1000:4C2D
   1000:4C2E 1000:4C2C`; classify each hit against the instruction starts of the listings below.
2. `direct_callers.py <dsun> 182+016B 182+00C4 182+1167 201+0073 201+0410 201+0000
   3A8E:0C2B`; then `callback_args.py <dsun> open 6 <site ...>` at the 31 `+016B` call sites
   and at overlay 172 file `0x594A8`, reading the `[bp+6]` word.
3. `trampoline_target.py <dsun> 5778:0020 5778:002A 5778:003E 56BD:0048`.
4. `immediate_search.py <dsun> 4957 444e` for the `WIND` tag; `resident_listing.py <dsun>
   3A8E:0C2B..3A8E:0C7B`.
5. `overlay_listing.py <dsun> 182 699B7 69AB0`, `201 897C0 89A20`, `201 89BD0 89C30`,
   `171 58820 588B0`, `199 88BA0 88BD0` and `172 59430 594B0`.

The reports are under `GAME_DIR/analysis/ui-window-19501-20261018/`. No original game or
function was executed and no image was viewed.
