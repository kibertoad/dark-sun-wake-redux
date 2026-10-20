---
id: FND-RNG-051
title: The hit routine's byte g is the acting combatant's byte 0x15, 11 with no actor, 0 for per-round effects or bits 5 to 7 of an object's byte; a is 1 at six direct calls, 6 in eight queued casts and otherwise a value the caller computes or is given
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00064F5F..0x0006500F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000654BC..0x00065653
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D170..0x0005D1CE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E32C..0x0007E7D3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F05C..0x0007F7A6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00081988..0x00082C3D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D074..0x0008D2D4
tool: tools/research/exec-census/direct_callers.py and overlay_listing.py at commit e1047cfa; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`. The hit routine overlay 179 `+0D2F` takes its
byte arguments `a` at `bp+0x1C` and `g` at `bp+0x1E` (FND-RNG-044); a caller pushes `g` first and
`a` second of its thirteen argument words. `direct_callers.py <dsun> 179+0D2F 179+0C7F` gives
`+0D2F` the near calls at `+0D1A` and `+1299` and the far call at overlay 193 `+04DE`, and
`+0C7F`, which passes its own thirteen arguments to `+0D2F` in the same places at `+0D1A`
(`+0CF3..+0D1A`), sixteen far calls. The pushes before each call give:

| Call | In routine | `a` | `g` |
|---|---|---|---|
| overlay 173 `+28A9` | `+2560` | byte `+0x1E` of the details record (`DS:19C5`, 66 bytes) numbered by `+2560`'s word argument at `bp+0x10` | byte `+0x15` of the combatant record (`DS:19C9`, 49 bytes) numbered by its word argument at `bp+0xE`, the attacker (FND-PARTY-091) |
| overlay 179 `+1299` | `+11DC` | byte `+0xC` of the queue record | byte `+0xD` of the queue record |
| overlay 193 `+04DE` | `+003C` | its byte argument at `bp+0xC` | byte `+0x15` of the combatant record of its first argument, the slot it passes as the hit's second slot (`+00C2..+00DE`), or 11 when that slot is negative (`+0374..+0398`) |
| overlay 193 `+1170`, `+11A0`, `+11D0`, `+1200`, `+123D` | `+1119` | its byte argument at `bp+8` | its byte argument at `bp+0xA` |
| overlay 193 `+129E` | `+1249` | its byte argument at `bp+8` | its byte argument at `bp+0xA` |
| overlay 193 `+145E` | `+13EC` | its byte argument at `bp+0xA` | its byte argument at `bp+0xC` |
| overlay 195 `+0242`, `+05BC`, `+0615`, `+066E`, `+0733` | `+00D1` | 1 | 0 |
| overlay 195 `+14C8` | `+147C` | the result of overlay 193 `+1A2D` with the slot and `+147C`'s byte argument at `bp+8` | 11, pushed before the call of `+1A2D` and left on the stack by its cleanup of 4 bytes (`+149F..+14C8`) |
| overlay 204 `+130E` | `+12B4` | its byte argument at `bp+0xC` | byte `+0x15` of the combatant record of its third argument, the slot it passes as the hit's second slot (`+12BB..+12EF`) |
| overlay 204 `+150F` | `+1322` | 1 | 11 |

**One step back.** `+1119`, `+1249` and `+13EC` have the near callers `+132F`, `+1348` and
`+1359`, all in overlay 193 `+12A8`, which pass its byte arguments at `bp+0xA` and `bp+0xC` as
`a` and `g`, and `+13EC` also `+14A0` in `+148E`, which passes its own, from its one direct caller
`+13B8` in `+12A8` with the same two bytes. `+12A8` has the near callers `+0D9B` (in `+0AAD`),
`+0FBE` and `+102F` (in `+0DC7`), each passing as `a` the low five bits and as `g` bits 5 to 7 of
the byte at `DS:67D0 + 37 * k`, byte `+0x15` of the object slot record of a slot `k`, and `+10CA`
(in `+1054`), passing `a` 6 and `g` 1. `+12B4` has seven near callers in overlay 204 `+0CBA`, at
`+0F94` to `+100A`, which pass as `a` its local byte at `bp-6` or `bp-8`.

**The cast queue.** `+11DC` runs each queue record of `4DA1:0000` (FND-PARTY-107) through overlay
193 `+003C` with the record's byte `+0xC` as `+003C`'s byte argument at `bp+0xC`; its call of
`+0D2F` at `+1299` is never taken (FND-PARTY-107). `+12B1` stores its byte argument at `bp+0xC`
there (`+12FB..+1302`) and `+1304` its byte at `bp+0x12` (`+1363..+136A`). The callers give:

| Call | Routine | `DATA` | Byte stored at `+0xC` |
|---|---|---|---|
| overlay 179 `+1028` (in `+0D2F`), `+12C9` (in `+12B1`, when the queue is full) | `+12B1` | 305, 307 | 6 |
| overlay 197 `+0F7C`, `+10F1`, `+180F`, `+17A3`, `+1A3C` | `+12B1` | 307, 307, 307, 124, 308 | 6 |
| overlay 197 `+1A2B` | `+1304` | -1 | 6 |
| overlay 179 `+2707` (in `+1373`) | `+12B1` | 311 | `+1373`'s byte `a` at `bp+0x14` |
| overlay 197 `+1202` (in `+111A`) | `+12B1` | its `SI` | `+111A`'s byte argument at `bp+0x14` |
| overlay 198 `+00EF`, `+013D` (in `+0000`) | `+12B1` | its `SI` | `+0000`'s byte argument at `bp+0xE` |

Overlay 193 `+0000` passes its byte argument at `bp+0xC` on to `+003C` in the same place
(`+001A..+0029`). Its callers give: overlay 193 `+22CF` (in `+2290`) its byte argument at
`bp+0xC`; overlay 176 `+03BD` and `+048A` (in `+0196`) the result of overlay 176 `+076C`; overlay
177 `+04FE` (in `+0492`) the result of overlay 177 `+07AF`; overlay 204 `+19AF` (in `+18CD`) its
local byte at `bp-6`.

## Interpretation

`g` is, wherever a combatant acts, the acting combatant's byte `+0x15`, the byte a load copies
from the character record's `combatant_kind` (FMT-PARTY-001): the attacker in the attack routine,
the caster in the cast routine `+003C`, the slot overlay 204 `+12B4` passes as the hit's second slot. With no acting
combatant it is 11 (`+003C` with a negative slot, overlay 195 `+147C`, overlay 204 `+1322`), for
overlay 195's per-round effect steps 0, and for hits that come from an object slot through
overlay 193 `+12A8` it is packed with `a` in the object's byte `+0x15`. So `DATA` 166's test of
the target's byte `+0x15` against `g` (RULE-MAGIC-016) compares the target's kind with the actor's.

`a` is 1 for the per-round effect steps and overlay 204 `+1322`, 6 for every queued cast of
`DATA` 124, 305, 307 and 308 and for overlay 193 `+1054`'s hits, the actor's details byte `+0x1E`
in the attack routine, the low five bits of an object's byte `+0x15` for object hits, and
elsewhere a value the caller computed or was given.

## Alternatives

- What the details byte `+0x1E`, the object byte `+0x15`, overlay 176 `+076C`, overlay 177
  `+07AF` and overlay 193 `+1A2D` hold, and the values of the arguments and locals named above,
  are not read here; `a` read as a level rests on its uses in FND-RNG-044 alone.
- Each table row was read from the pushes before one call; a byte stored into a word read whole
  is read here as the low byte the hit routine uses, since `+0D2F` and `+0C7F` read only `al`.
- Calls through a stored pointer to `+0D2F`, `+0C7F`, `+12B1` or `+0000` were not searched;
  `direct_callers.py` finds direct calls only.
- No emulated call or run was made; the overlays are outside the resident harness.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`, `<dsun>` the installed
`DSUN.EXE`: `direct_callers.py <dsun> 179+0D2F 179+0C7F 179+12B1 179+1304 193+0000 193+003C
193+1119 193+1249 193+13EC 193+12A8 193+148E 204+12B4 173+2560`; `overlay_listing.py <dsun> 173
5A920 5EA7C`, `176 61540 62067`, `177 62100 6300E`, `179 642E0 66F95`, `193 7E2F0 80ECE`, `195 81770
82DAA`, `197 83400 86F67`, `198 87310 878CC` and `204 8BDC0 8E660`, reading the pushes before
each call above. Outputs are kept under `GAME_DIR/analysis/rng-callers-20261020/`. Nothing was
executed.
