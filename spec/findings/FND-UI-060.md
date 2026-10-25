---
id: FND-UI-060
title: The Preferences dispatcher takes button numbers through the window's handler and keys through the global hook, and after each button redraws the button and the bar it changed, the music button drawing the music level in the message-delay bar's place
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008B240..0x0008BCC8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007AC83..0x0007ADD7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:071F..39D1:0BD3
tool: Capstone 5.0.9 over overlay and resident code; tools/research/exec-census/overlay_listing.py, resident_listing.py and trampoline_target.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 203's code starts at file `0x8B240`.
Trampoline `5782:0043` enters `+00EE`, `5782:0034` enters `+0605`, `5782:002F` enters `+0706`,
`5782:003E` enters `+0529` and `5782:0020` enters `+09B3`.

**Registrations.** The opener `+0000` opens `WIND/16500` through overlay 182 `+016B` with the
corner (55, 42) and the handler `5782:0043`, stores `+0605` as the record's `+0xF9` callback
through `3A8E:0D0C` and overlay 190 `+363B` as `+0xFD` through `3A8E:0D3C`, sets the `+0x62`
callback of `APFM/10201` to `+0706` through `3F96:02AB` (the double word `0x27D9` pushed there is
the frame number 10201), passes `5782:0043` to overlay 190 `+11A1`, which stores it in the global
hook `DS:A0F1` through `39D1:0418`, and draws the window through `3A8E:060D`.

**The dispatcher `+00EE`** reads the event record's kind and its word at offset 2:

| Kind | Action |
|---|---|
| 1 | calls `+0529` (the close routine) and returns `0xFFFF` |
| 6 | copies the 24-byte record and passes it to overlay 190 `+139B` with `5782:003E` |
| 2 | looks the word up in 13 button numbers at `+04F5` and jumps through the 13 targets at `+050F`; no match does nothing |
| others | nothing |

Every case but kind 1 ends by calling `2C5F:0182` and returning 0.

**The event builder** `39D1:097D..0BD3`: a mouse packet becomes kind 2 with the word
`3D72:0009` returns, the hit button's resource number, and a zero result reads the next packet.
A key packet that no MENU, ACCL or mnemonic of the current window takes becomes kind 6 with
`DS:A0FD` cleared, so `39D1:0752` calls `DS:A0F1`. Kind 1 is built from a class-15 packet, which
`4464:02E4` queues at `39D1:0A1B` when Esc (`0x011B`) arrives while bit 2 of the word `DS:9DEE`
is set; the builder then calls `DS:A0F1` at `39D1:0B55`. `DS:9DEE` is 0 in the file.

**Drawing.** The bar routine overlay 190 `+2943` takes the window, x, y, a limit and a value,
draws `BMP/20106` at (x, y) from the window's corner, and draws `BMP/20107` at (x + 1, y + 1)
clipped to a width of `164 * value / limit / 2` and a height of 9. `+082D` takes the window, a
button and a state, and calls `3EBE:071F` on the button with codes 4 and 5 for a nonzero state
and 1 and 0 for zero. `28C9:2A81` with 1 is called "flush" below, and `45E1:0097` with an id and
`DS:26B5` "sound", which runs only when `DS:13F7` and `DS:1435` are both nonzero.

| Button | After the state change |
|---|---|
| 10308 | `+0529`, overlay 182 `+19F8`, `3D72:0D83`; no redraw |
| 11308 | `+0529`, overlay 190 `+319E`, overlay 181 `+0480`; no redraw |
| 16300 | `+082D` with `DS:1436`; bar (75, 26), limit `DS:26B6`, value `DS:26B4` when music is on and 0 when off |
| 16301 | sound `0xE7` when effects are now on; `+082D` with `DS:1435`; bar (75, 45), limit 127, value `DS:26B5` when effects are on and 0 when off |
| 16302 | `+087D` |
| 16303 | nothing while `DS:1438` is nonzero; otherwise overlay 182 `+056F` and `+082D` with `DS:1437` |
| 16304, 16305 | bar (75, 26), limit 80, value `DS:26B7 - 20`; flush |
| 16306, 16307 | for 16306 with effects off: `+056C` with 1, `+082D` for 16301, `DS:26B5` = 1; then `DS:26B5` plus 7 or minus 7, kept at most 127; at 0, `+056C` with 0 and `+082D` for 16301; sound `0xE7`; bar (75, 45) as for 16301; flush |
| 16308, 16309 | bar (65, 64), limit 3, value `DS:143A`; the difficulty name from the table at `DS:26B9` through `401B:0172` at y 66, centred on x 106; flush; sound with the id from the bytes at `DS:26C9`, 205, 143, 231 and 24 for difficulties 0 to 3 |
| 16310 | `+082D` with `DS:14E4` |

The full redraw `+0605` calls `+082D` for 16310, 16300, 16301 and 16303, draws the message-delay
bar, the effects bar with `DS:26B5` whether effects are on or off, and the difficulty bar and
name, and draws no music bar. `+087D` (About) clears `+0xF9`, calls `3EBE:071F` with code 1 for
the twelve buttons other than 16310, draws the background and nine lines of text at x 42 from
y 19 in steps of 8, sets `APFM/10201`'s callback to `+09B3`, and flushes; `+09B3` undoes these
and redraws the whole window. `+0706` writes the label it picks, or an empty string, into the
control `0x2C06`.

## Interpretation

Button clicks reach the dispatcher as kind 2 through the window's handler and keys as kind 6
through the global hook, where overlay 190 `+139B` handles them as in the game view
(FND-INPUT-012). Esc closes Preferences as kind 1 only when something sets bit 2 of `DS:9DEE`.

A click on Music on or off draws the music level, or an empty bar, in the bar beside the
message-delay arrows, where it stays until a delay arrow or a full redraw draws the message
delay there again; the screen has no other music bar (FND-CONFIG-219). Volume up pressed while
effects are off turns them on at volume 8 whatever volume was stored. The difficulty arrows play
a sound per difficulty. Music, effects, animation and voice clicks do not call the flush, so
their changes appear at the next page flip. While the About text is shown, the voice button
stays enabled.

## Alternatives

What the button codes of `3EBE:071F` show (Q-CONFIG-007), the `%C` words passed to `401B:0172`,
`2C5F:0182`, overlay 182 `+056F` and `+19F8`, and the sound and music calls were not read. That
`45E1:0097` plays a sound rests on FND-SOUND-011. Which mouse actions give the values `+0706`
and `+09B3` test was not read. Whether the keys F4, F5, F6 and `A`, which change settings through
`+139B`, lead to a redraw of Preferences was not read; the dispatcher draws nothing after `+139B`
returns.

## How to reproduce

`overlay_listing.py <dsun> 203 8B240 8BB80` and `... 203 8BB77 8BCC8`; `... 190 7AC83 7ADD7`;
`resident_listing.py <dsun> 39D1:071F..39D1:0BD3`; `trampoline_target.py <dsun> 5782:0020
5782:002F 5782:0034 5782:003E 5782:0043`.
