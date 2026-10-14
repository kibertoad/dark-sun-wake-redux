---
id: FND-PARTY-211
title: The party's powers in effect all end at the end of a fight and at the end of a rest, through overlay 173 +3E30, called from the fight end at overlay 173 +0792 and from the rest routine overlay 204 +0CBA
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, resident_listing.py, direct_callers.py)
environment: null
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B0AD..0x0005B0B5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008CA7A..0x0008CAC8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D051..0x0008D074
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006950D..0x00069524
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:1F91..2D40:1FA8
---

## Observation

Overlay offsets are written `descriptor +offset`. Overlay 173 `+3E30` ends, for slots 0 to 3, each
power whose `PSST` bit 0 is set (FND-PARTY-210). `direct_callers.py <dsun> 173+3E30` gives two
calls: the near call at overlay 173 `+0792` and overlay 204 `+1291` through trampoline
`5671:00E8`.

**The fight end.** Overlay 173 `+0792` is the first of the calls the end of a fight makes before
the level gain (`+078D..+07A4`, FND-PARTY-088).

**The rest.** Overlay 204 `+1291` is in the routine at overlay 204 `+0CBA`. When the word at
`4C10:0019` is not 0 it shows `NO RESTING DURING COMBAT` and returns (`+0CD1..+0CEA`); otherwise,
when `28C9:2522` returns 1 it shows `THE PARTY RESTS` (`+0CED..+0D00`), and goes on. After a loop
that ends when `2D40:0776` returns 9999 (`+1280..+128C`), it calls overlay 173 `+3E30`
(`+1291`), passes its argument `[bp + 8]` times 60 to overlay 195 through `574E:0057`
(`+1296..+12A2`), calls `371D:0334` and overlay 204 `+1D2E` with 1, and returns (`+12A3..+12B4`). `direct_callers.py <dsun> 204+0CBA` gives the near
call at overlay 204 `+02B9`, which passes 1, the word at `[bp + 0xA]` and `SI`, and overlay 182
`+0CCC` through trampoline `5787:0020`, which, when the word at `[bp - 4]` is `0x525`, passes 0, 8
and `0x7FFE` (`+0CBD..+0CD1`).

**The third clearer.** `30B0:00C1`, which clears bit 0 of a slot's powers whose byte at
`51F1:07E2 + 3 * power` is above 0 (FND-PARTY-210), is called as `2D40:37C1` from `2D40:1FA4`, in
a loop over slots 0 to 3 of the routine at `2D40:197A`, after `2D40:10AE` with the slot and 1 and
only when the byte at `[bp - 0x18]` is not 0 (`2D40:1F91..2D40:1FA8`). `direct_callers.py <dsun>
2D40:197A` gives one call, from `2AAD:000C`, which passes 9999.

## Interpretation

Every psionic power the party has in effect ends when a fight ends and when the party rests: the
rest routine, after its checks and before time passes, calls the same routine the fight end does.
The rest that overlay 182 starts for button `0x525` passes 8 as the argument the routine
multiplies by 60.

## Alternatives

- What overlay 195's `574E:0057` does with the minutes, and when `28C9:2522` returns 1, were not
  read.
- What `2D40:197A` does, and when `[bp - 0x18]` is set, were not read; nor were the callers of
  overlay 202 `+0C82`, the third routine that clears bit 0.
- Which button `0x525` is on screen was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `direct_callers.py <dsun> 173+3E30 204+0CBA
2D40:197A`; `overlay_listing.py <dsun> 173 5B090 5B0D0`, `204 8CA7A 8CAD0`, `204 8D020 8D074`,
`204 8C050 8C080` and `182 694D0 69530`; `resident_listing.py <dsun> 2D40:1F60..2D40:1FB0` and
`2AAD:0000..2AAD:0020`.
