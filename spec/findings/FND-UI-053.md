---
id: FND-UI-053
title: The Game Menu's Exit to DOS and Load or Save buttons open three-item choices at (92, 71) whose first and second items lead to Save Game, quitting, Load Game and Save Game
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067F89..0x0006810E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000682F8..0x00068330
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079C93..0x00079DA6
tool: tools/research/exec-census/overlay_listing.py and trampoline_target.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`); overlay 181's code
starts at file `0x67EB0`. "The choice" is a far call of trampoline `566A:0025`, overlay 172
`+018F` (FND-COMBAT-051), with a left edge, a top edge, a title, a flag and three item labels, a
null label leaving its slot empty. Its result is the slot number 1, 2 or 3 of the item chosen,
by a click or by the item's first letter, and 0 for `Esc` when the flag is 1 (FND-COMBAT-051).
`combat_state` is the word `4C10:0019`.

**The table.** The Game Menu handler's event-2 table at overlay 181 `+0448` holds 14 button
numbers and the 14 branch offsets after them (FND-UI-046); button 10301 (Exit to DOS,
FND-UI-028) goes to `+00D9` and 10302 (Load or save) to `+0199`.

**Exit to DOS** (`+00D9..+0199`). The choice at (`0x5C`, `0x47`), that is (92, 71), with the
title `EXIT: SAVE GAME?` when `combat_state` is 0 and `EXIT GAME?` otherwise, the flag 1, and
the items `SAVE` (only when `combat_state` is 0; null otherwise), `QUIT` and `CANCEL`, the
strings at `DS:0EEF`, `DS:0EE4`, `DS:0F00`, `DS:0F05` and `DS:0F0A`. On 1 it near-calls `+0612`,
stores 1 in the byte `DS:627C` and far-calls trampoline `5736:0025`, overlay 192 `+0015`, the
Save Game entry (FND-SAVE-010). On 2 it near-calls `+0612` and stores 0 in `exit_control_byte`.
On any other result it calls `3A8E:060D` with the window at `DS:1431` and sets button 10301's
frame to 5 through `3EBE:071F`.

**Load or save** (`+0199..+025E`). The choice at the same corner with the title `LOAD/SAVE GAME`
(`DS:0E72`), the flag 1, and the items `LOAD` (`DS:0F11`), `SAVE` (only when `combat_state` is
0; null otherwise) and null. On 1 it near-calls `+0612` and far-calls trampoline `5736:0020`,
overlay 192 `+0000`, the Load Game entry; on 2 it near-calls `+0612` and far-calls `5736:0025`,
Save Game. On any other result it does as the Exit branch with button 10302.

**F3.** Overlay 190's `F3` handler `+1953` (FND-SAVE-010) makes the same choice with the same
corner, titles, flag and items, from its own copies of the strings at `DS:1F4B`, `DS:1F40`,
`DS:1F5C`, `DS:1F61` and `DS:1DC2`. On 1 it calls the caller's callback, stores 1 in `DS:627C`
and opens Save Game; on 2 it calls the callback and stores 0 in `exit_control_byte`. The same
handler takes `Esc`, `Alt-X` and `Ctrl-C` (FND-COMBAT-025): for `Esc` (`0x011B`) while a
callback is set, that is while a screen is shown, it calls the callback and returns without the
choice (`+1953..+19C7`); every other case makes the choice.

**After the save.** When the byte `DS:627C` is not 0, the confirm branch of the load and save
window stores 0 in `exit_control_byte` after its save or load calls (FND-PARTY-045).

## Interpretation

Exit to DOS, `F3`, `Alt-X`, `Ctrl-C`, and `Esc` while no screen callback is set ask `EXIT: SAVE GAME?` with Save,
Quit and Cancel, or `EXIT GAME?` with only Quit and Cancel during combat. Save opens Save Game
and quits once the game is saved; Quit leaves the main loop; Cancel, `Esc` or `C` returns to the
game. The Load or Save button asks `LOAD/SAVE GAME` with Load and Save, Save left out during
combat, and opens Load Game or Save Game.

## Alternatives

- Where the choice's box, title and items are drawn from the corner (92, 71), and which images
  it uses, belong to overlay 172 `+018F` and were not read here.
- What `+0612` does before the screens open, and what `3A8E:060D` does on the cancel path, were
  not read.
- Whether a click on an empty slot can return its number was not read; FND-COMBAT-051 reads
  the key handler's label tests.

## How to reproduce

In `tools/research/exec-census/`, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: `overlay_listing.py <dsun> 181 67F89 6810E` and `190 79C93 79DA6`;
`trampoline_target.py <dsun> 566A:0025 5736:0020 5736:0025`; read the 28 words at file
`0x682F8` (overlay 181 `+0448`) and the strings at `DS:0E72`, `DS:0EE4`, `DS:0EEF`, `DS:0F00`,
`DS:0F05` and `DS:0F11` (`DS` is file `0x4D000`). The listings are retained under
`GAME_DIR/analysis/ui-exit-choice-20261018/`. No original function or game process was run.
