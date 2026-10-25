---
id: FND-SAVE-017
title: Loading copies SAVEnn.SAV over DARKRUN.GFF, reads SAVE 1 to 61 and the region's GMAP flags from it, resets the party's slots, then reads PREF and GREQ from CHARSAVE.GFF whatever the copy returned
status: superseded
builds: [BLD-GOG-EN-1.1]
superseded_by: [FND-SAVE-022]
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007D8ED..0x0007DA85
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070CEE..0x00070DE7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007089C..0x000709A8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071A17..0x00071C41
tool: tools/research/exec-census/overlay_listing.py at the commit that adds this finding; Capstone 5.0.9; Python 3.14.7
environment: null
---

## Observation

Overlay 192 offsets `+x` are at file `0x7D300 + x`, overlay 187 offsets at `0x6FE30 + x`.

The Load routine `192 +05ED(slot)` (FND-SAVE-005):

1. stores 0 in `DS:459B` when it holds 9 and shows the loading message;
2. calls `204 +284D`, `187 +2424(DS:140C)` and `187 +206E` with that call's double word;
3. calls `187 +0EBE` with the record's path. `+0EBE` copies the file over `DARKRUN.GFF` through
   `4544:0000` and returns 0 at once when that fails. Otherwise it calls `182 +1856`, opens
   `DARKRUN.GFF`, calls `182 +15D7(DS:140C, 0, 1, 0)` and the reader `187 +0A6C(0)`. The reader
   allocates the bit buffer (`+1DD8`), refills the table (`+0D1A`), reads `SAVE/1` to `SAVE/61` in
   order, stopping at the first failed read (FND-SAVE-015), and, when every read succeeded, calls
   `+1CEE`, which reads `GMAP/DS:140C` and sets or clears two flags of each map cell from the two
   bit planes; it then frees the buffer and copies `4C13:035B` to `DS:19B8`. Back in `+0EBE`, for
   each object entry whose byte 5 has bit 3, it stores -1 in word `+0x10` of its 37-byte slot,
   clears double word `+0x1D`, and clears `DS:13FA` when the slot's first byte has bit 7; it calls
   `182 +15D7(DS:140C, 0, 0, 0)` and closes `DARKRUN.GFF`;
4. ignores the result of step 3 and opens `CHARSAVE.GFF` (`186 +0572`);
5. reads `PREF/100` and `GREQ/(save_list_top + slot + 1)` (FND-SAVE-005, FND-SAVE-009);
6. closes `CHARSAVE.GFF` (`186 +05AD`) and calls `187 +2496`.

The per-region `SAVE/(61r + i)` resources are not read here; the region change reads them from
`DARKRUN.GFF` (FND-SAVE-016).

## Interpretation

A load restores the state of the saved game in the order: archive copy, the 61 resources, the
map's cell flags, the clock copy, the party's slots, then settings and subregion. A failed copy
still applies the settings and subregion of the chosen slot to the game in memory.

## Alternatives

`204 +284D`, `187 +2424`, `187 +206E`, `182 +1856`, `182 +15D7` and `187 +2496` were read only at
their heads; their role in preparing the display and reloading the region is a reading of their
arguments.

## How to reproduce

`overlay_listing.py <dsun> 192 7D8ED 7D96A`, `... 192 7D95C 7DA85`, `... 187 70CEE 70DE7`, `...
187 7089C 709A8`, `... 187 71A17 71C41`.
