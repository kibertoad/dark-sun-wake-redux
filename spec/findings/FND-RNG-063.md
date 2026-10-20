---
id: FND-RNG-063
title: The removal list is emptied into overlay 206's dissolve at nine calls; seven follow hits or damage, in the fight, attack and per-round effect routines, two damage loops of overlay 188 and two hit routines, and two are in routines no inventory row holds
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B141..0x0005B171
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005CBA4..0x0005CBB3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000742E5..0x0007432C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000743B7..0x00074407
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F756..0x0007F77A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00081EB1..0x00081EF5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C02D..0x0008C069
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D2DD..0x0008D2F9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:0C8D..28C9:0CE8
tool: tools/research/exec-census/direct_callers.py, trampoline_target.py, overlay_listing.py and resident_listing.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`. FND-RNG-043 finds that overlay 173 `+22C4`
adds a combatant whose hit points it takes to 0 to the removal list at `4C4F:000A`, and that
overlay 173 `+24C7` empties the list, last added first, into overlay 206 `+0E1E`, which plays a
cry with flag 1 and dissolves a removed combatant in view. `direct_callers.py <dsun> 173+24C7`
finds nine calls of `+24C7`: two near calls in overlay 173 and seven far calls through its
trampoline `5671:00B1`, one of them in the load image at file offset `0x1EB73` (`28C9:0CE3`, which
`direct_callers.py` prints as `2997:0003`). Each is read here from the code before it:

| Call | Routine | What runs before the call |
|---|---|---|
| overlay 173 `+084C` | the fight routine `+059A` (FND-PARTY-089) | when its local byte at `bp-1` is not 0: overlay 173 `+1238` with the slot, and overlay 195 `+0789` with the slot when that returned 2; with the byte 0 it returns 2 without the call (`+0821..+0851`) |
| overlay 173 `+2290` | the attack routine `+1CC1` (FND-PARTY-091) | `1695:02AD` with the slot and `DI`, on every path through `+2284` (`+2284..+2293`) |
| overlay 188 `+1487` | `+1445` | for each slot `2D40:0D94` returns for its first argument until the cursor reaches 0x140, unless 9,999: overlay 179 `+2A05` with the slot and its second argument as the damage (`+1445..+148C`) |
| overlay 188 `+1562` | `+14DE` | for each slot it finds: overlay 179 `+2A05` with a damage of the combatant's word `+0` times its second argument divided by 100, as signed 32-bit values (`+1517..+1567`) |
| overlay 193 `+1485` | `+13EC`, which passes `DATA` 104 or 225 (FND-PARTY-102) | overlay 179 `+0C7F`, the hit routine's entry, for its slot; the call is skipped when the slot's combatant byte `+0x14` is 1 (`+1466..+148A`) |
| overlay 195 `+0780` | the per-round effect routine `+00D1` (RULE-COMBAT-013) | a loop of overlay 179 `+0C7F` calls, then overlay 193 `+194F` when the combatant byte `+0x14` is 3 or more, and `+0A1F` when the byte `50C2:0000` is 0; reached on every path through `+0741` (`+0741..+0785`) |
| overlay 204 `+02A4` | a routine no inventory row holds | `+18CD` when `SI` is 9,999, otherwise `+1A68` with `SI`, 2, `0x93` and segment `5787` (`+026D..+02A9`) |
| overlay 204 `+1534` | `+1322` (FND-RNG-051) | a loop of overlay 179 `+0C7F` calls over a local list, then the unload `31E0:05BB` (`+151D..+1539`) |
| `28C9:0CE3` | a resident routine no inventory row holds | when the byte `DS:43FA`, sign-extended, plus 5 is above 4 unsigned and the fight state word `4C10:0019` is not 0: overlay 173 `+306B` (trampoline `5671:0098`) with `DS:426D` and 0 (`28C9:0C8D..28C9:0CE8`) |

`+1238` is the near target at `+0838`, `+18CD` and `+1A68` those at `+027F` and `+029C`.

## Interpretation

The list fills only at `+22C4` (FND-RNG-043), and seven drains follow code that applies hits or
damage: the fight routine's step, the attack routine, the hits of overlay 193 `+13EC` and overlay
204 `+1322`, the per-round effects, and two damage loops of overlay 188 (a fixed amount, or a
percentage of each target's hit points). So a combatant those remove cries and dissolves at the
end of the routine whose hit or damage removed it, unless a path returns before its drain, as the
fight routine's does when its byte at `bp-1` is 0. What the routines holding overlay 204 `+02A4`
and `28C9:0CE3` do before their drains was read only as far as the calls above.

## Alternatives

- Which events call overlay 188 `+1445` and `+14DE`, overlay 204's routine holding `+02A4`, the
  resident routine holding `28C9:0CE3`, and overlay 204 `+1322` was not read.
- A list left filled by a path that returns before its drain is emptied by the next drain; when
  that is was not read.
- Indirect calls of `+24C7` other than through its trampoline were not searched.
- The routines holding `+02A4` and `28C9:0CE3` have no inventory row (FND-EXE-593); their starts
  were not established.
- No emulated call or run was made; the overlays are outside the resident harness.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`, `<dsun>` the installed
`DSUN.EXE`: `direct_callers.py <dsun> 173+24C7 173+22C4 179+2A05`; `trampoline_target.py <dsun>
5671:0098 5671:00B1`; `overlay_listing.py <dsun>` for 173 `5B11C 5B174` and `5CB60 5CBB8`, 188
`742D7 7432F` and `743B2 7440A`, 193 `7F725 7F77D`, 195 `81EA0 81EF8` and 204 `8C014 8C06C` and
`8D0E2 8D300`; `resident_listing.py <dsun> 28C9:0C30..28C9:0CEB`. Outputs are kept under
`GAME_DIR/analysis/rng-014-20261021/`. Nothing was executed.
