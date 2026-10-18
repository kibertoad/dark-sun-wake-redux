---
id: FND-INPUT-012
title: Overlay 190's key routine opens the Game Menu on Tab, and on C, U, E, I and V in either case the spell, effect, inventory and character windows, unless a non-party combatant has the turn
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000796DB..0x00079706
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000798E3..0x000798F5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079DA6..0x0007A20C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007A20C..0x0007A308
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000960C5..0x000961BE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:1139..28C9:1201
tool: tools/research/exec-census/overlay_listing.py, resident_listing.py, direct_callers.py and trampoline_target.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`); overlay 190's code
starts at file `0x78340`. `k` is the key word at `[bp+0x12]` of overlay 190 `+139B`
(FND-COMBAT-025), and the far pointer at `DS:1431` is the window shown, null when none is.

**The entry.** `+139B` compares `k` with the 33 words at `+1F44` (`+13AC..+13BD`) and jumps
through the words 0x42 bytes further on when one matches. `0x0F09` (Tab) is the only one with
the low byte 9. Its handler `+15A3` far-calls the routine at `[bp+0x1E]` when that pointer is
not null and then far-calls trampoline `56B9:0020`, whose target is overlay 181 `+0480`, the
routine that opens `WIND/10500` (FND-UI-046). It tests nothing else.

**Other keys.** A `k` that matches none goes to `+1A66`: it takes `(k | 0x20) & 0xFF`, subtracts
`0x63` and, when the result is 0 to `0x13` (unsigned), jumps through the 20 words at `+1F0C`,
which send `c` (0), `e` (2), `i` (6), `u` (`0x12`) and `v` (`0x13`) to `+1A8E`, `o` and `p` to
`+1A82`, and the rest to the end `+1EC9`. None of the 33 words of `+1F44` has a low byte that
becomes `c`, `e`, `i`, `u` or `v` under that fold, so every key word whose character byte is one
of those letters in either case reaches `+1A8E`, and a key word whose character byte is another
value does not.

**The tests.** `+1A8E` goes to the end when `turn_combatant` is 4 or more (`cmp word [0x426D],
4; jl`, signed). When `DS:1431` is null it goes straight to `+1CBE`. Otherwise it compares
`DS:1431` with four far pointers: `55CA:0000`, `55C9:0000`, `DS:2690` and `5072:003C`, and for the
first that is equal it calls `3EBE:071F` with `DS:1431`, the button number 10300, 11305, 11306
or 11304 and 5; for the first three it also stores 0 in that far pointer when the folded key is
`v`, `c`, `u`, `e` or `m`. It sets the byte `[bp-1]` to 1 (`+1C50`), looks the folded key up
among the six words at `+1EF4` (`c`, `e`, `i`, `o`, `u`, `v`) and calls `3EBE:071F` with
`DS:1431`, the button number 11305, 11306, 11304, 10305, 11305 or 10300 and 4, and then
`28C9:2A81` with 1 (`+1C54..+1CBD`).

**The open.** At `+1CBE` it far-calls the routine at `[bp+0x1E]` when that is not null, and jumps
through the 20 words at `+1ECC` by the same index:

| Key | Target | What it does |
|---|---|---|
| `c`, `u` | `+1D8D` | stores `DS:1431` at `55C9:0000` and far-calls trampoline `57C1:003E`, overlay 211 `+02B5` |
| `e` | `+1E08` | stores `DS:1431` at `DS:2690` and far-calls trampoline `577D:0020`, overlay 202 `+0287` |
| `i` | `+1D60` | near-calls `+36D2` with a slot and 9999 |
| `v` | `+1CE4` | stores `DS:1431` at `55CA:0000` and far-calls trampoline `57C9:0025`, overlay 212 `+061C` |
| `o` | `+1E86` | the overhead map (FND-UI-046) |
| `p` | `+1EC4` | far-calls trampoline `5782:0048`, overlay 203 `+0000` |

The slot passed to the four openers is the word `4E71:0B44` when `[bp-1]` is 1 and the word
`4C13:0369` otherwise, with the 66-byte and 49-byte records at `DS:19C5` and `DS:19C9` that it
selects. Overlay 211 `+02B5` requests window 11500 with the handler at trampoline `57C1:005C`
(overlay 211 `+081B`) and stores it at `55C9:0000` and `DS:1431` (`+0380..+03A9`). Overlay 212
`+061C` opens `WIND/11500` (FND-PARTY-045), `+36D2` opens `WIND/13500` (FND-PARTY-217), and
overlay 202 `+0287` requests window 11500 and keeps it at `DS:2690` (FND-COMBAT-040).

**Callers.** `+139B` is reached only through trampoline `571F:0020`. Its direct callers are
`28C9:11F4`, where the map's key routine `28C9:0CFF` sends a key word that its own table does not
hold when the word at `DS:0DAB` is 1 (FND-AI-004; `28C9:1139..28C9:11F9`), and overlay 175
`+0B87`, 181 `+0034`, 189 `+05C6` and `+0604`, 202 `+05FC`, 203 `+011E`, 211 `+0880` and 212
`+09AB`. Overlay 181 `+0034` is in the Game Menu's handler, which passes every key but `0x0F09`
(FND-UI-046), and overlay 211 `+0880` and 212 `+09AB` are in the handlers `+081B` and `+0967`
that the spell and character windows register. The Game Menu's buttons 10300, 11304, 11305 and
11306 lead to the same four openers: overlay 181 calls overlay 212 `+061C` at `+02FA`, `+36D2`
at `+0314`, overlay 211 `+02B5` at `+0377` and overlay 202 `+0287` at `+03D6`.

## Interpretation

On the map, a key the map routine does not take itself goes to `+139B` while the word at
`DS:0DAB` is 1. Tab opens the Game Menu at any turn. `C`, `U`, `E`, `I` and `V`, upper or lower
case and with or without Shift, open the Cast Spells and Use Psionics, Current Spell Effects,
Inventory and View Character screens, the four the buttons 11305, 11306, 11304 and 10300 of the
navigation row open (SCR-UI-008, FND-UI-030); they do nothing while a combatant outside the party
has the turn. With one of those screens open, the same keys reach `+139B` from its handler, mark
the new screen's button, close the shown screen through the callback and open the new one for
the character the old one showed.

## Alternatives

- The identification of overlay 211 `+02B5` with the spell screen and overlay 202 `+0287` with
  the effect screen rests on the button each key marks and on the Game Menu's button routes,
  whose meanings come from the manual through SCR-UI-008, and on overlay 211 holding the cast
  list (FND-PARTY-109) and overlay 202 the effect frames (FND-COMBAT-040).
- What the frames 4 and 5 that `3EBE:071F` sets look like, what `28C9:2A81` does, and what each
  screen handler does before it calls `+139B`, were not read.
- What the word at `DS:0DAB` holds outside the value 1 is not read here; FND-AI-004 gives the
  branch.

## How to reproduce

In `tools/research/exec-census/`, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: `overlay_listing.py <dsun> 190 796DB 79706`, `190 798E3 798F5`, `190 79DA6 7A20C`
and `211 960C5 962A0`; `resident_listing.py <dsun> 28C9:1139..28C9:1201`; read the words at
overlay 190 `+1F44` (33), `+1F86` (33), `+1F0C` (20), `+1ECC` (20) and `+1EF4` (12); `direct_callers.py
<dsun> 190+139B 181+0480 211+02B5 202+0287 212+061C 190+36D2`; and `trampoline_target.py <dsun>
571F:0020 56B9:0020 57C1:003E 577D:0020 57C9:0025 571F:005C 57C1:005C`. The listings are retained
under `GAME_DIR/analysis/input-hotkeys-20261018/`. No original function or game process was run.
