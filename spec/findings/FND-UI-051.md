---
id: FND-UI-051
title: The region word changes after startup only through a saved-game load from window 0x4844's handler or through script opcode 0x5E with a first operand of 32766, and no script on MAS/99's or MAS/50's route before 0x05DC holds that instruction
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:1A43..172C:1B30
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073890..0x000738B4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007D5B2..0x0007D5D3
tool: tools/research/exec-census/reach_config.py (with q007_dsun_apfm_census.json and newgame_declarations.json), direct_callers.py, callback_args.py, resident_listing.py, overlay_listing.py, trampoline_target.py and script_decode.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, pypcode 4.0.1, xxhash 4.0.1; scientific-method-engine 19.9.0 reach
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`).

**Callers.** `direct_callers.py` finds, as far and near calls and through each overlay's
trampolines:

- overlay 182 `+15D7`, which holds the two stores to `DS:140C` (`current_region`) at files
  `0x69E4C` and `0x69F05`: four far calls, overlay 187 `+0F21` and `+0F9C` in `+0EBE`, and
  `+1169` and `+12D0` in `+1132`;
- overlay 187 `+0EBE`: one, overlay 192 `+0654`, in `+05ED`; `+05ED`: one near call, `+0BDF`,
  in overlay 192 `+0785`;
- overlay 187 `+1132`, which holds the store at file `0x71046`: one, overlay 188 `+0A05`, in
  `+09F0`; overlay 188 `+09F0`: one, file `0xDFD6`, which is `172C:1B16`.

**The opcode 0x5E handler.** The script instruction table at `DS:030A` sends opcode `0x5E` to
`172C:1A43` (FND-SCRIPT-005), the only entry of the 129 holding `1A43`. It reads five operands
into the doublewords at `4C13:00B4`, `00B8`, `00BC` and `00C0` and the byte at `00C4`. When the
first equals `0x7FFE` (32766) it calls overlay 188 `+09F0` at `172C:1B16` with the second, third
and fourth; otherwise it places objects through overlay 188 `+0864` and `2D40:07DE` and does not
call `+09F0`. Overlay 188 `+09F0` compares its first argument with `DS:140C` and calls overlay 187
`+1132` with its three arguments only when they differ (FND-COMBAT-044); when they are equal it
calls `+0864` instead, for those of four slots whose byte at `4F49:0C33` plus three times the slot
is 2.

**Window 0x4844's handler.** Overlay 192 `+0785` is passed as a far pointer (through overlay 192's
trampoline `5736:002F`) only at overlay 192 `+02B2` (to overlay 182 `+016B`) and `+02CE` (to
overlay 190 `+11A1`), among the 31 calls of `+016B` and 23 of `+11A1` that `callback_args.py`
reads (the others pass other relocated handlers, 0, or at overlay 172 `+0479`, `+0775` and overlay
209 `+178E` the handler registered before, read back from overlay 190 `+11C1`, FND-EXE-587); both
lie in `+024B`, which opens window `0x4844` with that handler (FND-PARTY-044).
`+024B`'s only callers are the near calls at overlay 192 `+0010` and `+0025`, in `+0000` and
`+0015`, and its trampoline has no far caller.

**The walks with cuts.** FND-EXE-592's three walks, run again with routines cut as stops:

| Cut | Startup | Callback | Gate routine's later calls | Stores reached |
|---|---|---|---|---|
| `172C:1A43` | 2,081 routines | 1,901 | 1,902 | `0x69E4C`, `0x69F05`; every route passes overlay 192 `+0785`, `+05ED` and overlay 187 `+0EBE` |
| overlay 192 `+0785` | 2,086 | 1,906 | 1,907 | all three; every route passes `172C:1A43`, overlay 188 `+09F0` and overlay 187 `+1132` |
| both | 2,059 | 1,879 | 1,881 | none |

Every run leaves no transfer unresolved, reaches its controls (files `0x689D0` and `0x68E70`)
and reports its negative as usable. The stores `277B:0182` and `277B:03C1` are reached by none.

**Scripts.** In the decode of FND-SCRIPT-024, 218 `0x5E` instructions have the first operand
32766. `MAS/99`, which overlay 188 `+046C` runs on the startup route (FND-PARTY-040), holds only
opcodes `0x0A`, `0x16`, `0x31`, `0x6D` and `0x70`. `MAS/50` holds four `0x5E` instructions before
`0x05DC`, at `0x0223`, `0x022F`, `0x028D` and `0x03A1`, with first operands -163, -141, -133 and
-141; `GPL/142`'s two, at `0x0446` and `0x046D`, have -60; `GPL/27`'s one, at `0x0022`, lies before
its entries 228 and 491 and no jump from them goes below 228.

## Interpretation

After the program starts, the region word changes only when a saved game is loaded from window
`0x4844`'s handler (the Load and Save window overlay 192 `+0000` and `+0015` open), when a script
runs `0x5E` with a first operand of 32766 and a region other than the current one, or through the
`-R` switch or after a defeat in the main routine (FND-UI-050). The script route FND-UI-050 reads
for a new game, `MAS/99` at startup and `MAS/50` up to `0x05DC` with the scripts it calls, holds no
such instruction. So on that route the gate routine's `MAS/50` runs, and `0x05DC` is reached, with
region 50 loaded. This no longer rests on unresolved transfers.

## Alternatives

- Another script that runs between program start and `MAS/50`'s `0x05DC` could hold `0x5E` with
  32766 and another region: one registered with `0x1B` by `MAS/50`, or one run through a window
  the player opens before START GAME. Which scripts run in that span is not read here.
- A `0x5E` whose first operand is a variable holding 32766 at run time counts as well; none of the
  instructions read on the route has a variable first operand.
- Overlay 172 `+0479` and `+0775` and overlay 209 `+178E` register again a handler read back from
  overlay 190 `+11C1`; after the Load and Save window, that can be `+0785`, so the handler can run
  again while it stays registered. Which window those routines restore it under is not read here.
- The cuts show only which routines the read routes pass; whether a route runs depends on which
  handler is registered and which instruction runs (FND-EXE-592).

## How to reproduce

In `tools/research/exec-census/`, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: run `direct_callers.py <dsun> 182+15D7 187+0EBE 192+05ED 187+1132 188+09F0 192+024B`;
`direct_callers.py <dsun> 190+11A1 182+016B` and `callback_args.py <dsun> 190+11A1 6 <sites>` and
`callback_args.py <dsun> 182+016B C <sites>` with the call sites it lists;
`resident_listing.py <dsun> 172C:1A43..172C:1B30`; `overlay_listing.py <dsun> 188 73890 738E0`;
`trampoline_target.py <dsun> 56EF:0093 5702:00AC 5702:00A7`; and read the 129 words at file
`0x4D30A`. Run FND-EXE-592's three queries with `stops` of `{routine, reason}` for `172C:1A43`,
for `192+0785`, and for both. Run `script_decode.py GPLDATA.GFF OBJEX.GFF --all` and read
`MAS/99`, `MAS/50`, `GPL/142` and `GPL/27`. The queries, listings and reports are retained under
`GAME_DIR/analysis/reach-transfers-20261018/` and the decode under
`GAME_DIR/analysis/script-instructions-20261018/`. No original function or game process was run.
