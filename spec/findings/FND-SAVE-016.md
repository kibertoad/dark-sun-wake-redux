---
id: FND-SAVE-016
title: A saved game's file holds SAVE resources 1 to 61, STXT 1, ETME 0 and the per-region SAVE resources 61r+1 to 61r+17 that each region change writes into DARKRUN.GFF; the null table entries are filled from run-time pointers before each save and load
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070713..0x0007089C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070B4A..0x00070BFD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070BFD..0x00070CEE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070F62..0x00071060
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071445..0x000716FE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071A17..0x00071C41
tool: tools/research/exec-census/overlay_listing.py and gff_tag_numbers.py at the commit that adds this finding; Capstone 5.0.9; Python 3.14.7
environment: null
---

## Observation

Overlay 187 offsets are given as `+x`, file offset `0x6FE30 + x`. The table is FND-SAVE-015's.

**Filling the null entries.** `+0D1A` (file `0x70B4A`) copies the far pointers at `DS:19C9`,
`DS:19C5`, `DS:19C1`, `DS:67B7`, `DS:55B8` and `DS:19D1` into the table slots of entries 5 to 10
(`DS:158E` to `DS:15A2`), and those at `4C13:034B`, `:0347`, `:0343`, `:033F`, `:033B`, `:0337`,
`:0333`, `:032F` and `57E0:40C0` into the slots of entries 30 to 38 (`DS:15F2` to `DS:1612`). The
writer `+08E3` calls it at `+0915` and the reader `+0A6C` before its loop.

**The writer's other steps.** Before the loop `+08E3` calls `+1C57` (`+0905`), which allocates
3,136 bytes at `DS:55B8` and fills two planes of 1,568 bytes, one bit per cell of a 128 by 98 grid,
from two map-cell tests; when the allocation fails the writer returns 0. It then calls
`+1615(flag)` (`+092B`) and, after the loop, `+1829(flag)` (`+0A61`). `+1615` loads `GPLI/1` as
6-byte entries and, in the 19-byte records chained from `DS:5AF5` and, when the flag is set, in the
200 trigger records, replaces each pair of words that equals an entry's second and third words with
its first word; `+1829` makes the reverse replacement. Before each remove and write the writer
selects the archive handle `DS:144E` through `37ED:0064` (`+097B`, `+0A18`). A failed selection or
write ends the loop, skips `STXT/1` and returns 0.

**The wrapper.** `+0DCD` (file `0x70BFD`) asks DOS for the free space and shows one of two
warnings when the free clusters are fewer than twice, or than once, the clusters 1,330,000 bytes
need, and saves anyway. It opens `DARKRUN.GFF` (`+1BE7`) only when `DS:167A` shows it closed and
closes it only if it opened it; a failed close skips the copy to the numbered file
(FND-SAVE-008).

**The region change.** `+1132` (file `0x70F62`) opens the archive (`+11C0`), calls the writer
with the old region `DS:140C` (`+11E0`, `+11E5`), switches region (`+11FC`, storing the new one in
`DS:140C` at `+1216`) and calls the reader with the new region (`+121B`). With `n` not 0 the writer
and the reader cover entries 1 to 17 as `SAVE` resources `61n + 1` to `61n + 17` (FND-SAVE-015).

**A saved game.** `C:\GOG Games\Dark Sun 2\cloud_saves` holds `SAVE01.SAV`, `DARKRUN.GFF` and
`CHARSAVE.GFF`. `SAVE01.SAV` and `DARKRUN.GFF` there are 49,763 bytes each and byte-identical.
`gff_tag_numbers.py` finds in them `SAVE` 1 to 61, `STXT` 1 and `ETME` 0, and no other table; the
`CHARSAVE.GFF` beside them holds `PREF` 100 and `GREQ` 1 to 10. The lengths of the `SAVE` resources
equal the table's lengths, except `SAVE/8`, which is `0x1B60` bytes (8 times 876 entries).

## Interpretation

A `SAVEnn.SAV` is the whole working archive at the moment of the save: the 61 `SAVE` resources of
the current state, the description in `STXT/1`, the `ETME/0` copied from `DARKSAVE.GFF` when the
game started (FND-SAVE-006), and, for every region the party has left, that region's 17 object
resources. Settings and the subregion go to `CHARSAVE.GFF` instead (FND-SAVE-009). The shipped
save holds no per-region resources, so it was made before the party left its first region. The
written resources hold the translated form of the words `+1615` replaces.

## Alternatives

What `GPLI/1`'s entries mean, and whether the in-memory form after a load is the translated one,
was not read: the reader does not call `+1829`. Which archive the reader reads from rests on the
open before it, since it does not call `37ED:0064`. The roles of entries 10, 19, 23, 25, 27 to 29,
41 to 43, 45 and 48 to 50 were not traced.

## How to reproduce

`overlay_listing.py <dsun> 187 70B4A 70BFD`, `... 70713 7089C`, `... 70BFD 70CEE`, `... 70F62
71060`, `... 71445 716FE`, `... 71A17 71C41`; `gff_tag_numbers.py <install>\cloud_saves TAG` for
`SAVE`, `STXT`, `ETME`, `PREF` and `GREQ`; a byte comparison of the two 49,763-byte files.
