---
id: FND-PARTY-210
title: Bit 0 of a slot's PSST byte is set when the slot uses that psionic power and it succeeds, unless the power's byte at 51F1:07E2 is 99, and cleared by overlay 176 +050E, which ends the power, and by three other routines
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/immediate_search.py, overlay_listing.py, resident_listing.py, direct_callers.py)
environment: null
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00061780..0x00061832
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00061A2B..0x00061A73
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005E750..0x0005E78B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 30B0:00C1..30B0:00F3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008B10C..0x0008B128
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058FEC..0x0005900B
---

## Observation

Overlay offsets are written `descriptor +offset`. The `PSST` table is at `4E71:09CD`, 34 bytes per
slot, one byte per power (FND-PARTY-012, FND-PARTY-104); code reaches segment `4E71` through the
segment word `0x0370` (descriptor 110).

**The sites.** `immediate_search.py <dsun> 09CD` gives, leaving out hits that start inside another
instruction, `593C:000D` (`cmp ax, 0x9CD`), overlay 179 `+14DC` (`jmp 0x9CD`), the two
`add ax, 0x9CD` of overlay 186 (the write and read of FND-PARTY-012), and 22 instructions on a
`PSST` byte through `ES`: reads (`mov al`) in overlay 173 `+3E45`, overlay 176 `+0018`, `+004F`,
`+00B2`, `+0152`, `+04EB`, `+05AA`, `+05C4` and `+09D1`, overlay 202 `+0A24` and `+0B4A`, and
overlay 209 `+0CD3`, and these writes:

| Site | Instruction |
|---|---|
| overlay 176 `+02CC` | `or ..., 1` |
| overlay 176 `+052D`, overlay 202 `+0C82`, `30B0:00E5` | `and ..., 0xFE` |
| overlay 171 `+0DF5`, overlay 176 `+05DF`, `+0666`, `+0698` | `and ..., 1` |
| overlay 176 `+05E7`, `+066C` | `or ..., al` and `or ..., 2`, each right after the `and` before it |

**Setting.** Overlay 176 `+0196` (trampoline `5691:0043`, called from overlay 193 `+2317`) takes a
slot in `SI` and a power number at `[bp + 0xA]`. From `+0240` it takes the byte at
`51F1:07E1 + 3 * power` and compares it with the word at `+2` of the slot's 49-byte record in the
table at `DS:19C9`; when that word is below it, it shows `Insufficient psps` for a slot below 4
(`+0254..+0286`). Otherwise it calls `+0D2E` with the slot, the power and 0, stores the result at
`DS:4224` (`+028C..+029A`), and when the result is above 0, the byte at `51F1:07E2 + 3 * power` is
not `0x63` (99) and the slot is below 4, sets bit 0 of the `PSST` byte at `4E71:09CD + 34 * slot +
power` (`+029D..+02D2`). A result of `0xFF` shows `psionics disrupted by static` and another result
of 0 or less `psionic power check fails` for a slot below 4 (`+02D4..+02EA`).

**Clearing.** Overlay 176 `+050E` (trampoline `5691:006B`) takes a slot and a power; for a slot
below 4 it clears bit 0 of that byte (`+051C..+052D`) and goes on with the power plus `0xEB`, which
was not read. Its callers are the near call at `+04FB`, inside a loop over powers 0 to 33 that calls
it for each byte whose bit 0 is set (`+04EB..+0505`), and overlay 173 `+3E53`. Overlay 173 `+3E30`
does the same for slots 0 to 3 and powers 0 to 33 (`+3E35..+3E6A`); its callers are the near call at
overlay 173 `+0792` and overlay 204 `+1291` through trampoline `5671:00E8`.

`30B0:00C1` (also reached as `2D40:37C1`, near-called from `2D40:1FA4`) takes a slot and for each
power 0 to 33 whose byte at `51F1:07E2 + 3 * power` is above 0 clears bit 0 of the slot's byte
(`30B0:00C4..30B0:00F3`). Overlay 202 `+0C82` clears bit 0 of the byte of the slot in
`4E71:0B44` and the power in `AX` (`+0C6C..+0C82`). Overlay 171 `+0DF5` keeps only bit 0 of each of
the slot's 34 bytes, after storing 0 in its `PSIN` byte at `4E71:0A55 + slot` (`+0DDC..+0DFB`).

## Interpretation

Bit 0 of a `PSST` byte marks a psionic power that the character used successfully and that stays in
effect: using a power sets it unless the power's second table byte is 99, and ending the power
(`+050E`) clears it. The rank in bits 1 to 7 (FND-PARTY-104) is written around it and never touches
it. Overlay 173 `+3E30` ends every power still in effect for the whole party.

## Alternatives

- What the byte at `51F1:07E2 + 3 * power` holds was not read; 99 there keeps a power from being
  marked, which fits a power with no lasting effect, but no reading shows it.
- When `+3E30`, `30B0:00C1` and overlay 202 `+0C82` run (at the end of a fight, at rest, or on some
  other event) was not followed past their callers.
- What `+050E` does with the power plus `0xEB` was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `immediate_search.py <dsun> 09CD`;
`overlay_listing.py <dsun> 176 616D6 61840`, `176 61A20 61A80`, `173 5E730 5E790`,
`202 8B0F0 8B128` and `171 58FD0 59010`; `resident_listing.py <dsun> 30B0:00B0..30B0:0110`;
`direct_callers.py <dsun> 173+3E30 176+0196 176+050E 2D40:37C1`. The text at `DS:0600`, `DS:0614`
and `DS:0633` is in the data segment.
