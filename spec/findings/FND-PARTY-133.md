---
id: FND-PARTY-133
title: Handlers that windows register in display mode 2 reach the experience award's level gain through a spell cast in overlay 211's window, the debug keys through overlay 175's and 211's, and the script interpreter through overlay 171's character placement
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009662B..0x00096726
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00060741..0x0006076C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005899E..0x000589D6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058CB5..0x00058E3A
tool: Python 3.14.7 with Capstone 5.0.9, pypcode 4.0.1 and xxhash 4.0.1 (scientific_method_engine reach; tools/research/exec-census/reach_config.py, direct_callers.py, trampoline_target.py, overlay_listing.py, q065_mode2_handlers_reach.json)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Reading each routine from its call of overlay 182 `+1856` (FND-PARTY-129) to its
first `retf`, 18 of 20 read return to their caller without calling overlay 182 `+19F8`; the
others are overlay 201 `+06A0` and overlay 204 `+02F9`. The calls in overlay 184 `+089C`,
overlay 190 `+3734`, overlay 199 `+0587` and overlay 201 `+0A8B` were not read this way. While mode 2 holds,
the game runs the handlers those windows register.

`direct_callers.py <dsun> 190+11A1 39D1:0418 39D1:0430 39D1:0448` lists the registrations of a
key handler (overlay 190 `+11A1`, FND-PARTY-127) and of the three handler slots `DS:A0F1`,
`DS:A0F5` and `DS:A0F9` (FND-PARTY-130). The two words pushed before each call name 18 distinct
handlers other than the main view's `28C9:0CFF` and `28C9:152C`; `trampoline_target.py` resolves
them to overlay 171 `+03C5`, 172 `+04C5`, 175 `+0B61`, 181 `+0000`, 184 `+0DA7`, 189 `+0559`,
190 `+0092`, 192 `+0785`, 194 `+0000`, 199 `+0053`, `+0D75` and `+102C`, 201 `+0000`, 202
`+05CA`, 203 `+00EE`, 209 `+169C`, 211 `+081B` and 212 `+0967`.

The query `q065_mode2_handlers_reach.json` starts at those 18 handlers, with the script
interpreter `172C:000C` and the four calls of the level gain (overlay 173 `+07AD`, overlay 188
`+16E9`, overlay 190 `+1444` and `+144C`) as targets, the message window's call of overlay 182
`+19F8` at file `0x00059BFB` as control, and the exit worker `1000:0388` and overlay 182 `+19F8`
as leaves, `+19F8` also as a routine that does not return. The walk reads 1,262 routines and
107,770 instructions, reaches the control, and leaves 52 computed transfers unresolved.

- **Overlay 188 `+16E9`** is reached from overlay 211 `+081B`, and every route passes overlay 173
  `+22C4`, `+30C7` and overlay 188 `+1604`. The shortest calls overlay 193 `+2290` at overlay 211
  `+0901`, then overlay 193 `+0000` and `+003C`, overlay 179 `+0D2F` and overlay 173 `+22C4`.
  Overlay 211 `+081B` reaches `+0901` when the byte at `DS:43F7` is not 0 and its word at
  `bp+0x18` is below 8 (`+08A1..+08B1`), with calls of overlay 182 `+059C` and its own `+1FB5`
  and no call of `+19F8` between (`+081B..+0916`).
- **Overlay 190 `+1444` and `+144C`** are reached from overlay 175 `+0B61`, which calls overlay
  190 `+139B` (trampoline `571F:0020`) at file `0x00060767`. Overlay 211 `+081B` calls the same
  routine at `+0880`.
- **The interpreter** is reached first through overlay 194 `+0000` and overlay 182 `+1167`. With
  `+1167` added as a leaf, since it returns at once unless `DS:0DAB` is 0 (FND-PARTY-126), it is
  still reached: overlay 171 `+03C5` calls its own `+0AA5` at `+07C1`, which calls `31E0:0121` at
  `+0C25`, which reaches `1695:01D1` (FND-PARTY-129) and the interpreter. Overlay 171 `+0AA5`
  calls overlay 186 `+0572`, `2D40:000A`, overlay 186 `+0282` and overlay 182 `+0A7C` before
  that call (`+0AA5..+0C2A`); overlay 171 shows `NO CHARACTERS AVAILABLE` through the message
  window at `+0791`.
- **Overlay 173 `+07AD`** is not reached.

## Interpretation

While a window that overlay 182 `+1856` opened is up, its handlers can reach a level gain in
three ways the walk reads: a spell cast from overlay 211's window that kills a hostile combatant
while the fight state word is 0, through the kill award; the debug keys through overlay 190
`+139B`; and a script that overlay 171's window starts by placing a character's object, which
may award experience through instruction `0x21`. So `DS:0DAB` can be 2 when overlay 210 `+0B44`
runs from overlay 188 `+16E9` or from overlay 190, and overlay 182 `+19F8` then sets the combat
marks of 0 to 1 and sets `DS:0DAB` to 1 while the window is still open (FND-PARTY-123). The fight
end's gain at overlay 173 `+07AD` is not reached from these handlers.

## Alternatives

- The 52 unresolved transfers leave the negative result for overlay 173 `+07AD` unusable.
- Each route is a route through the code; whether the game reaches it depends on the data it
  tests, such as `DS:43F7`, the event word, the fight state and the shipped scripts.
- A window that registers a handler through a pointer it reads from memory, rather than through
  two pushed words, is not among the 18 starts.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: in `tools/research/exec-census/`, run `direct_callers.py <dsun> 182+1856 190+11A1
39D1:0418 39D1:0430 39D1:0448`, read the two words pushed before each call, and resolve them with
`trampoline_target.py`, the segment being the descriptor's segment-table segment plus 0x1000;
then `overlay_listing.py` over the ranges above. In the repository root run `python -I
tools/research/exec-census/reach_config.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config>
tools/research/exec-census/q065_mode2_handlers_reach.json` and `python -I -m
scientific_method_engine reach <config>`; for the second walk, add `182+1167` to `leaves` first.
