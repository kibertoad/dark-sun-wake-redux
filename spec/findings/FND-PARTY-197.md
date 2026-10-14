---
id: FND-PARTY-197
title: Of 42 byte-pattern candidates writing through ES at +0x14, 16 follow a load of the combatant table DS:19C9 and only overlay 173 +22C4 stores a mark above 2 there; a reach from the Create Characters and Load Saved Game starts reaches +22C4 through the window View Character hands events to, and the attack routine through set_leader
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
tool: Python 3.14.7 with Capstone 5.0.7 (tools/research/exec-census/offset14_stores.py, overlay_listing.py, reach_config.py); scientific-method-engine 19.7.0 reach
environment: null
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005CBE4..0x0005CCCA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007998E..0x000799B2
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. The combatant table is the far pointer at `DS:19C9`, 49-byte records whose byte
`+0x14` is `combat_mark` and word `+0x00` the hit points (FMT-COMBAT-001).

**The candidates.** `offset14_stores.py` finds 42 instructions that write through ES at `+0x14`
of a base register with an 8-bit displacement:

| Pointer loaded before the store | Sites | Values written |
| --- | --- | --- |
| `DS:19C9` within the 80 bytes before | overlay 173 `+2349`, `+235B`, `+2385`, `+3D94`; 182 `+11B3`, `+1A40`; 184 `+1391`; 187 `+04E1`; 190 `+0AC9`, `+0B25`, `+1998`; 197 `+2E38`, `+2EB8`, `+2F4A`; 204 `+0D57`, `+103E` | 8, 7, 3 at 173 `+2349`, `+235B`, `+2385`; 0 at 184 `+1391`; 1 at the others |
| `DS:1429` (the details record at `4E4F:0029`) | overlay 183 `+0195`, `+15FC`, `+1607`, `+1617`, `+166D`; 184 `+082E` | 5, `al`, 1, 8, 5, 5 |
| `DS:19C1` (a word) | overlay 173 `+3E8D` | 0x41 |
| a far argument or local | overlay 172 `+01AE` (`[bp+0xA]`), 179 `+2515`, `+25AD`, `+2668` (`[bp-0xA]`), resident `31E0:2350` (`[bp-4]`) | 0, 1, 8, 1, `al` |
| none decoded | overlay 200 `+04DD` (`add [si+0x14], dh`) | |
| resident word stores | `1BF3:614F`, `2D40:1611`, `39D1:09A4`, `0A82`, `0B37`, `3A8E:047E`, `0489`, `0579`, `3BA6:03AD`, `0495`, `3EBE:0093`, `00D4`, `3F96:003A` | |

The three stores above 2 after a `DS:19C9` load are FND-PARTY-086's damage routine overlay 173
`+22C4`: 8 or 7 with hit points 0 when the damage is at least the hit points plus 10, and 3 with
hit points 0 when it is at least the hit points.

**The reach.** `q073_mark_stores_reach.json` takes the 24 starts and 9 leaves of
`q017_create_characters_reach.json` (overlay 212 `+0967`, the Create Characters window) and
`q017_load_window_reach.json` (overlay 192 `+0785`, the Load Saved Game window) together, with
the 16 `DS:19C9` sites and `173+22C4` as targets and `187+04E1` as an instruction control. The
engine reads 1,226 routines and 99,169 instructions, leaves 66 transfers unresolved and reports
`negativeUsable` false. It reaches the control and 13 targets, all from overlay 212 `+0967`; it
does not reach 182 `+11B3` and `+1A40` (behind the leaf 182 `+1167`), 184 `+1391` or 204 `+0D57`
and `+103E`. Overlay 173 `+22C4` and its three stores are reached by two routes:

- A, seven calls: 212 `+09AB` calls overlay 190 `+139B` (View Character's handler), whose
  `+1667` passes the event (24 bytes from `[bp+6]`) to overlay 211 `+081B` when the far pointer
  at `55C9:0000` equals `DS:1431` (`+1649..+1667`); then 211 `+0901` to overlay 193 `+2290`,
  `+22CF` to 193 `+0000`, `+0029` to `+003C`, `+04DE` to overlay 179 `+0D2F`, and `+0D9C` to
  `173+22C4`.
- B, the shortest chain to the target 173 `+3D94` (a store of 1 inside `+3CDA`), six calls:
  212 `+0A03` calls overlay 190 `+0802`, whose `+096F` calls `182+1D3E` (`set_leader`,
  RULE-PARTY-008), whose `+1E48` calls `2D40:10AE`, whose `2D40:13D9` calls the attack routine
  `173+1CC1` (FND-PARTY-091), whose `+2290` calls `+24C7`, whose `+2514` calls `+3CDA`. The report
  gives one shortest chain per target, so whether this route also reaches `+22C4` is not shown.

## Interpretation

Among the stores that go through the combatant table pointer directly, only the damage routine
gives a mark that leaves a member not standing. A static walk from the title screen's windows
reaches it through the overlay 211 window that View Character hands events to, and reaches the
attack routine and `+3CDA` through `set_leader`; whether either route can run before START GAME
depends on the conditions along it, which the walk does not test. FND-PARTY-192's run executed 184 `+1391` from this screen and
the walk does not reach it, so routines reached only through registered handlers are missing and
the unreached targets say nothing.

## Alternatives

- The search covers ES writes with an 8-bit displacement of `0x14`. A write through DS or SS, with
  a 16-bit displacement, by a string store or a block copy, or to the hit-point word `+0x00`, is
  not in it.
- The six sites through a far argument, local or `si` were not followed to their pointer's
  source, and the resident word stores were not checked against the combatant table.
- That overlay 211 is the spell window rests on its use of `DS:43F7` and `ICON` 19101; not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, in `tools/research/exec-census/`: `offset14_stores.py <dsun>`;
`overlay_listing.py <dsun> 173 5CBE4 5CCD0` and `190 79960 799C0`; `reach_config.py <dsun>
../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv OUT q073_mark_stores_reach.json`, then
`python -I -m scientific_method_engine reach OUT` and read `targets`, `counts`, `controls` and
`negativeUsable`.
