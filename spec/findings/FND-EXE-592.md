---
id: FND-EXE-592
title: With the census's declarations and the classified rest declared, the walks from program start, the start window's callback and the gate routine's later calls leave no transfer unresolved, and they reach the region, effect and palette writers through script opcodes, window and frame callbacks and the exit table
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:029D..1000:02A1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:252C..1000:2530
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0F49..4AE5:0F53
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:00D8..15F3:00E2
tool: tools/research/exec-census/reach_config.py (with q007_dsun_apfm_census.json and newgame_declarations.json) and resident_listing.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, pypcode 4.0.1, xxhash 4.0.1; scientific-method-engine 19.9.0 reach
environment: null
---

## Observation

The installed `DSUN.EXE` is the input (634,416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`). Overlay offsets are written `descriptor +offset`.

**The transfers.** FND-UI-050's three walks (from the 40 startup starts, from the start window's
callback overlay 194 `+0000`, and from the 18 calls the gate routine overlay 182 `+1167` makes from
`+1300` on) leave 63 distinct computed transfers unresolved. 39 of them are call sites that
`q007_dsun_apfm_census.json` already declares, with the target sets FND-EXE-579 to FND-EXE-588
read from each pointer's or slot's writers: among them the window hooks at `DS:A0F1` (`39D1:07CC`,
`083B`, `0B55`, 18 handlers) and `DS:A0F5` (`39D1:0786`, `0899`, overlay 190 `+0092`), the window
record fields `+F5`, `+F9` and `+FD` (`39D1:0802`, `3A8E:08B7`, `3A8E:0707`), the application frame
field `+62` (`3D72:0590`), the record table at `DS:3F42`, the guarded pointers at `DS:346D` to
`3492` and `DS:02F6`, the script opcode table at `DS:030A` (`172C:01B5`), and `289B:025E`, `0269`.
The other 24 are classified by FND-EXE-589 and are declared here, in
`newgame_declarations.json`:

| Transfers | Declared as | Finding |
|---|---|---|
| `1000:029D`, `1BF3:5D0E`, `3D72:012A`, `09C7`, `0BD7`, `0DA1`, `0DD0`, `4AB9:002B`, `0052` | never run (exhaustive, no targets) | FND-EXE-580, FND-PARTY-043, FND-EXE-588, FND-EXE-583 |
| `1000:1F10`, `1000:1FBC` | leave the file for the interrupt instruction built on the stack | FND-EXE-578 |
| `4AE5:0F55`, `0F60`, `0F6D`, `0F7B`, `0FA5`, `0FAF`, `10CB`, `10D9`, `1220` | leave the file for the pointer `int 2Fh` at `4AE5:0F49` returns for AX 4310 | FND-EXE-581 |
| `15F3:05D4`, `066C`, `0699` | leave the file for the pointer `int 2Fh` at `15F3:00D8` returns for AX 4310 | FND-EXE-581 |
| `jmp [398C]` at `1000:252C` | a one-row table at `DS:398C`, which holds `129F` and has no store | FND-EXE-579 |

The same file declares the other XMS calls FND-EXE-582 and FND-EXE-584 list, which these walks
reach once the rest is declared. Three more reports of the walk are not transfers into code the
file holds, and are given as the census gives them: the call at `1000:02C1` does not return, so
the run-time error text after it, which the walk decoded as code and reported as five gap bytes
and four contested starts at `1000:02E9..1000:02F8`, is not followed (FND-EXE-578); `jmp [3916]`
at `1000:298A` returns to `1000:0260`, after the near call the walk already continues at
(FND-EXE-580), so the walk stops there; and the five far jumps through the tables at `2707:018D`,
`02FF`, `03DA`, `04D5` and `0591` stop there, with their 34 relocated table rows given as starts
(FND-EXE-586).

**The three walks again.** With the 78 census declarations and these, and the same starts,
leaves, targets (the five stores to `DS:140C`: `277B:0182`, `277B:03C1`, file `0x69E4C` and
`0x69F05` in overlay 182 `+15D7`, and `0x71046` in overlay 187 `+1132`) and controls:

| Walk | Routines | Instructions | Unresolved, gaps, contested | Stores reached |
|---|---|---|---|---|
| startup | 2,092 | 160,882 | 0, 0, 0 | `0x69E4C`, `0x69F05`, `0x71046` |
| callback | 1,912 | 151,425 | 0, 0, 0 | the same three |
| gate routine's later calls | 1,913 | 151,685 | 0, 0, 0 | the same three |

Each reaches its controls (files `0x689D0` and `0x68E70`, and in the callback walk `0x69C7E`),
and reports its negative as usable. Neither `277B` store is reached. The chains to the three
stores pass declared calls, which is the control that the walk follows a declaration: the first
reaches `+15D7` through `39D1:0B55` into overlay 192 `+0785`, and the others reach overlay 187
`+1132` through the script opcode dispatch `172C:01B5` into `172C:1A43`. FND-UI-051 reads which
routines every route passes.

**The effect and palette walks again.** FND-COMBAT-054's first query and FND-UI-049's second
(same starts, leaves, targets and controls), with the same declarations added, also leave no
transfer unresolved:

- the effect walk reads 1,910 routines and reaches 33 of its 42 targets, among them the insertion
  wrapper overlay 193 `+166F`, the insertion `+16F6` and its increment `+1749`, with no routine
  common to every route to the wrapper; the routes pass the script opcode dispatch, the window
  hook `39D1:0B55` (to overlay 171 `+03C5`, 184 `+0DA7`, 189 `+0559`, 192 `+0785` or `28C9:0CFF`)
  and the application frame call `3D72:0590` (to overlay 202 `+0673`, 209 `+03DC`, `+0EF0` or
  213 `+034F`). The numbered restore overlay 187 `+0A6C` is reached through the script dispatch
  into `172C:1A43`, and the saved-game load `187+0EBE` through `39D1:0B55` into overlay 192
  `+0785`. Nine targets are not reached: overlay 193 `+1933` (the index removal's decrement),
  `+19DC` (the code removal's), `+192F`, `+1E0D`, `+2383`, `+23C5`, `+23EC`, `277B:00B4` and
  `2D40:37E5`;
- the palette walk reads 1,910 routines and reaches 14 of its 17 targets: `3796:0514`, overlay 204
  `+1D2E` and its fade callers through script opcode `0x22` (`172C:01B5` into `172C:0F84`), overlay
  187 `+217E` and overlay 194 `+0547` through the start window's handler at `39D1:0B55`, and
  `1BF3:2A8C` and `1BF3:52E7` through the exit table's call `1000:03A1` into overlay 180 `+0762`.
  `1BF3:58AD`, `3796:047C` and overlay 180 `+05B8` are not reached.

FND-UI-048's walk from the map's routines overlay 201 `+0073` and `+0000`, with the same
declarations, reaches all three of its targets (`201+05D5`, `206+0E1E`, `173+24C7`), with no
unresolved transfer, through `3A8E:08B7` into overlay 189 `+001E` and `39D1:0B55` into `28C9:0CFF`.

## Interpretation

Every computed transfer these walks reach now has a target set from its writers, a reading that
it never runs, or a destination outside the file that returns, so their negative results are
usable on the walk's assumptions. The walks read every declared target as possible at every
declared call: a window hook may call any of the 18 handlers some caller registers, and the script
dispatch any opcode handler. So a target they reach is reached on some route through a callback or
an opcode, and whether that route runs on a new game depends on which handler is registered and
which script instruction runs at the time, which a walk does not decide.

## Alternatives

- The declarations rest on the findings cited; each names its own limits, and the walk adds no
  evidence for them.
- A route the walks report may not run. Narrowing one needs the handlers registered on the route
  and the scripts it runs, read in order (FND-UI-051 for the region stores).
- The destinations outside the file (the XMS driver, the interrupt the stack code makes) are
  assumed to return and to call nothing in the file.

## How to reproduce

In `tools/research/exec-census/`, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: take FND-UI-050's three queries and add to each the `indirectCalls` of
`q007_dsun_apfm_census.json` and the `indirectCalls`, `indirectJumps`, `starts`, `noReturn` and
`stops` of `newgame_declarations.json`; do the same for FND-COMBAT-054's first query, FND-UI-049's
second and FND-UI-048's fifth (starts `201+0073` and `201+0000`, targets `201+05D5`, `206+0E1E`
and `173+24C7`, control `201+02D9`). Run `reach_config.py <dsun>
../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> <query>` and `python -I -m
scientific_method_engine reach <config>`. `resident_listing.py <dsun> 4AE5:0F40..4AE5:0F55
15F3:00D0..15F3:00E2` shows the two `int 2Fh` sites. The queries and reports are retained under
`GAME_DIR/analysis/reach-transfers-20261018/`. No original function or game process was run.
