---
id: FND-CONFIG-218
title: F4 and F5 change the music and sound-effects bytes as the Preferences buttons do, A and F6 both toggle the animation byte unless it is locked, and no key changes the voice bytes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079B7C..0x00079C34
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079894..0x0007989C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008B7AC..0x0008B845
tool: tools/research/exec-census/overlay_listing.py and trampoline_target.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`). The key table of
overlay 190 `+139B` (FND-COMBAT-025) sends `3E00` (F4) to `+183C`, `3F00` (F5) to `+1868`, and
`4000` (F6), `1E41` (A) and `1E61` (a) to `+1894`. None of these handlers tests
`turn_combatant` or `combat_state`. Messages go through trampoline `566A:002A`, overlay 172
`+05DB`, from the shared tail at `+1554`.

**F4** (`+183C..+1868`) far-calls trampoline `5782:0039`, overlay 203 `+056C`, with the first
argument 1 when the byte `DS:1436` is 0 and 0 otherwise, and the second argument the byte
`DS:1435`. It then shows `MUSIC ON` when `DS:1436` is not 0 and `MUSIC OFF` otherwise.

**F5** (`+1868..+1894`) calls the same routine with `DS:1436` and with 1 when `DS:1435` is 0 and
0 otherwise. It then shows `SOUND EFFECTS ON` when the byte `DS:13F7` is not 0 and `SOUND EFFECTS
OFF` otherwise; it does not test `DS:1435` for the message.

**A and F6** (`+1894..+18F4`): when the byte `DS:1438` is not 0, it shows `ANIMATIONS LOCKED
ON` and changes nothing. Otherwise it stores 1 in `DS:1437` when that byte is 0 and 0 when it is
not, shows `ANIMATIONS ON` when the new byte is not 0 and `ANIMATIONS OFF` otherwise, and
far-calls trampoline `56BD:0066`, overlay 182 `+056F`, with the words 4, 4, 16 and 16 after
turning it on and 16, 16, 16 and 16 after turning it off.

**Overlay 203 `+056C`(music, effects)** (`+056C..+0605`): when the byte `DS:13F7` is 0 it sets
both arguments to 0. It picks 4 for both on, 2 for music only, 3 for effects only and 5 for
neither, and passes it to `45E1:0021` when `DS:13F7` is not 0. When `DS:13F7` is not 0 and the
music argument differs from `DS:1436`, it far-calls overlay 187 `+2B91` with 1 when
`combat_state` is 0 and 3 otherwise, and stores the byte `DS:26B4` in `DS:26B6` when the value
`4A6E:0064` returns for `DS:26B4` is below `DS:26B4`. Last it stores the music argument in `DS:1436`
and the effects argument in `DS:1435`. It is the two-byte routine that the Preferences buttons
16300 and 16301 call (FND-CONFIG-010).

None of the 33 key words of `+139B` leads to a write of `DS:14E4` or `DS:1439`, the voice bytes
of FND-CONFIG-014: the handlers above write only `DS:1437` and, through `+056C`, `DS:1435` and
`DS:1436`. In the load image the bytes `DS:1435`, `DS:1436`, `DS:1437` and `DS:13F7` are 1 and
`DS:1438` is 0.

## Interpretation

`DS:1436` is the music setting and `DS:1435` the sound-effects setting, each on when not 0; F4
and F5 toggle them as the Preferences buttons do, and both are forced off when the master sound
byte `DS:13F7` is 0. `DS:1437` is the animation setting, on when not 0, and `DS:1438` locks it:
`A` and `F6` do the same thing, and there is no voice key. The F5 message reports the master
sound byte, so with sound present it reads `SOUND EFFECTS ON` after F5 turns the effects off.

## Alternatives

- What overlay 182 `+056F` does with its four words, what `45E1:0021` and overlay 187 `+2B91` do
  with the values they get, and which code sets `DS:1438`, were not read.
- The handlers reach `+139B` only when the map routine passes a key on (FND-INPUT-012); which
  other screens pass these keys was not read.
- New-game values of the bytes are not given by their load-image values when a saved game's
  `PREF/100` or another path writes them first (FND-SAVE-005).

## How to reproduce

In `tools/research/exec-census/`, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: `overlay_listing.py <dsun> 190 79B7C 79C34`, `190 79894 7989C` and `203 8B7AC
8B845`; `trampoline_target.py <dsun> 5782:0039 566A:002A 56BD:0066`; and read the bytes at file
`0x4E435`, `0x4E436`, `0x4E437`, `0x4E438` and `0x4E3F7` (`DS` is file `0x4D000`). The key table is
read as in FND-INPUT-012. The listings are retained under
`GAME_DIR/analysis/config-hotkeys-20261018/`. No original function or game process was run.
