---
id: FND-CONFIG-219
title: None of the 13 Preferences buttons writes the music level, which the screen only reads from the sound library on entry and draws as a bar
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008B240..0x0008B2A2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008B32E..0x0008B735
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008B735..0x0008B769
    kind: file-data
tool: tools/research/exec-census/overlay_listing.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`); overlay 203's code
starts at file `0x8B240`. The music level is the byte `DS:26B4` (FND-CONFIG-012).

**The entry.** Overlay 203 `+0000`, which opens the Preferences window with the handler at
`5782:0043`, does, before it opens the window: when the byte `DS:13F7` is not 0, it calls
`49E9:00FD` and keeps the result at `DS:9B79`, and when `DS:1436` is also not 0 it calls
`4A6E:011F` and stores its return in `DS:26B4` (`+0010..+0041`). This is the only instruction of
the overlay that writes `DS:26B4`, and nothing in the overlay writes `DS:26B6`.

**The buttons.** The handler `+00EE` passes event 6 to overlay 190 `+139B` (`+0105..+0126`)
and, for event 2, compares the event's word with the 13 words at `+04F5` and jumps through the 13
words that follow (`+0132..+014E`):

| Button | Branch | Button | Branch |
|---|---|---|---|
| 10308 | `+0152` | 16305 | `+0176` |
| 11308 | `+0164` | 16306 | `+029E` |
| 16300 | `+01CC` | 16307 | `+0280` |
| 16301 | `+035F` | 16308 | `+03D0` |
| 16302 | `+048A` | 16309 | `+03D5` |
| 16303 | `+0491` | 16310 | `+01B9` |
| 16304 | `+018E` | | |

FND-CONFIG-010 reads the branches of 16300, 16301 and 16303 to 16310, and FND-UI-034 the About
button 16302. `+0152` and `+0164` near-call `+0529` (trampoline `5782:003E`) and then call
`56BD:00BB` and `3D72:0D83`, or overlay 190 `+319E` and the Game Menu's opener overlay 181
`+0480`. The music
button's branch, after `+056C` (FND-CONFIG-218), sets the button from `DS:1436` and draws the
music bar from `DS:26B4`, or 0 when `DS:1436` is 0, over `DS:26B6` (`+0250..+027D`). None of the
13 branches writes `DS:26B4`.

## Interpretation

The Preferences screen has no control that changes the music level. On entry it takes the
level from the sound library and shows it as the music bar, which the music button redraws;
the two arrow pairs change the message delay and the sound-effects volume. Together with
FND-CONFIG-029, which finds only this write and the Load Game copy of `DS:26B4` in the
executable, the music level a player can set in this build comes only from a saved game.

## Alternatives

- The manual's reading, that the top bar is the music volume and its arrows change it
  (SRC-MANUAL-1994, page 15), is ruled out for this build: the arrows beside the top bar are
  16304 and 16305, whose branches change the word `DS:26B7` and whose hover label is
  `MESSAGE DELAY` (FND-CONFIG-010, FND-UI-034), and no button writes `DS:26B4`.
- What `4A6E:011F` returns, and whether the sound library changes the audible level by another
  route, was not read.

## How to reproduce

In `tools/research/exec-census/`, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: `overlay_listing.py <dsun> 203 8B240 8B735`, and read the 26 words at file
`0x8B735`. Search the listing for `0x26b4` and `0x26b6`. The listing is retained as
`ovl203-0000-04F5.txt` under `GAME_DIR/analysis/config-hotkeys-20261018/`. No original function
or game process was run.
