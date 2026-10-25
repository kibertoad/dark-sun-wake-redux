---
id: FND-SAVE-018
title: The key dispatcher's callback is a release routine its caller passes, called before F1 and F2 open a screen, on Esc and on the exit choice's Save and Quit; Save from the exit choice sets DS:627C, which makes the Save screen end the game and which nothing clears
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079AFD..0x00079B7A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079C93..0x00079D85
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067F90..0x00067FF0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007DEB0..0x0007E014
tool: tools/research/exec-census/overlay_listing.py, direct_callers.py, callback_args.py and immediate_search.py at the commit that adds this finding; Capstone 5.0.9; Python 3.14.7
environment: null
---

## Observation

Overlay 190 offsets `+x` are at file `0x78340 + x`; the key dispatcher is `190 +139B`
(FND-COMBAT-025). Its far pointer argument at `[bp+0x1E]` comes from its caller.
`direct_callers.py` and `callback_args.py` find eight callers: the resident call at file
`0x1F084` passes null, and overlay callers pass `175 +0C05`, `181 +0612`, `189 +042F`,
`202 +0547`, `203 +0529`, `211 +073D` and `212 +08E3`.

The dispatcher calls the pointer when it is not null:

- on `F1` (`+17BD`) and `F2` (`+1807`), after `3EBE:071F(DS:1431, 10302, 4)` when the double word
  `4E46:0000` is not 0, and before it opens Save Game or Load Game; `F1` refuses during combat
  before this;
- on Esc (key word `0x011B`, `+195A..+1961`); it then stores 1 in byte `+0x14` of each of the
  49-byte combatant records 0 to 3 at `DS:19C9` whose word `+6` is not 0 and whose byte `+0x14` is
  0, and calls `194 +037B(0)` when `28C9:2522` returns 0, or `182 +19F8`, `3D72:0D83` and
  `28C9:2A81(1)` otherwise, without showing the exit choice;
- on the exit choice's answer 1, Save (`+1A1B..+1A2A`), before it stores 1 in `DS:627C` and opens
  Save Game, and on answer 2, Quit (`+1A32`), before it stores 0 in `DS:1462`.

When the pointer is null, Esc reaches the exit choice like `Alt-X`, `Ctrl-C` and `F3`.

The seven routines, read from their first instructions: `181 +0612` and `203 +0529` release a
window through `182 +0128` and clear `DS:1431` and their own window pointer; `189 +042F`,
`202 +0547`, `211 +073D` and `212 +08E3` store `4E71:0B44` in `DS:4264`, store 2, 4, 3 or 1 in the
word at `DS:4265` plus twice that value, call `39D1:0430(0)` and free buffers; `175 +0C05` and the
routines of overlays 189, 211 and 212 call `213 +02D5` when `55CB:0000` is not 0 and free buffers.
None of them names a save file, `DS:1462` or `DS:627C`.

`immediate_search.py <dsun> 627C` finds three operands: the store of 1 above (file `0x79D65`),
a store of 1 in overlay 181's copy of the exit choice (file `0x67FDF`, after its own callback
`181 +0612`), and `cmp byte ptr [0x627C], 0` in overlay 192 (file `0x7DF0F`). The byte is 0 in the
file. In the Save and Load window callback of overlay 192, after either action has run and the
record buffer is freed, a nonzero `DS:627C` stores 0 in `DS:1462` and returns -1; otherwise the
callback rebuilds the play view (`182 +19F8`, `28C9:2522`, and on).

## Interpretation

The callback lets the screen that runs the dispatcher close itself before the dispatcher opens
another screen or the game ends. Its effects are on windows and buffers, not on saved data.
Answering Save to the exit choice marks the save as the last action: once the Save Game screen
saves, the game ends. Nothing found clears the mark, so after a Save answer followed by EXIT on the
Save screen, the next save or load from any screen would also end the game.

## Alternatives

Only direct operands were searched; a block clear of the data segment that reaches `DS:627C` was
not excluded. The seven routines were read at their heads and the buffers they free were not
identified. The callback after EXIT on the Save screen was not traced to show that it skips the
test.

## How to reproduce

`overlay_listing.py <dsun> 190 79AFD 79B7A`, `... 190 79C93 79D85`, `... 181 67F90 67FF0`, `...
192 7DEB0 7E014`, and each callback over 0x50 bytes from its start; `direct_callers.py <dsun>
190+139B`; `callback_args.py <dsun> keycb 1E 1F084 60767 67EE4 74F76 74FB4 8AA9C 8B35E 96690
98BAB`; `immediate_search.py <dsun> 627C`.
