---
id: FND-EXE-596
title: The 27 routines of overlay 187 that no entry placed write and read saved-game resources, remap GPLI pairs, keep two map bitmaps, copy and play cinematics, and show copy progress
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007060A..0x00070713
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070BFD..0x00070CEE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070DE7..0x00070F62
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007118B..0x0007129C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071445..0x00071997
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071A87..0x00071C5C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071C73..0x00071E9E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007201A..0x000729C1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000729DC..0x00072B8B
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-595
environment: null
---

## Observation

After the restage of FND-EXE-595, `npm exec -- standard-coverage` reported 274 inventory rows of
the installed `DSUN.EXE` that no spec location covered, all in overlays. Overlay 187 (header
segment `56EF`, 53 trampolines, code from file offset `0x6FE30`) held the most of them: 27 rows,
6,441 bytes. Each row below is one range of the inventory; "offset" is the overlay code offset,
the file offset is `0x6FE30` plus it, and `DS` is `57E0`. Every routine has a trampoline in
`56EF`, so each can be entered from another overlay or the resident code.

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+07DA` | `0x7060A..0x70670` | `56EF:006B` | Takes `n`. Returns 0 when `37ED:0064` fails on the archive handle at `DS:144E`; otherwise removes `ETAB/n` with `37FC:05C5` and writes 12,000 bytes from the far pointer at `DS:67B7` as `ETAB/n` with `37FC:00E8`, returning 1 when the write succeeds. |
| `+0840` | `0x70670..0x706D5` | `56EF:0070` | Takes a slot, `n` and a tag. Same steps, writing the slot's 37-byte record at `57E0:67BB + 37 * slot` under that tag and `n`. |
| `+08A5` | `0x706D5..0x70713` | `56EF:0075` | Takes a slot, `n` and a tag. Reads that resource into the slot's record through `38FF:04AB`; returns 1 on success. |
| `+0DCD` | `0x70BFD..0x70CEE` | `56EF:0084` | The save routine FND-SAVE-008 reads. Before the steps listed there it asks `44DE:04A1` for the free space and shows one of two messages through overlay 172's `+05DB` when the space is low. |
| `+0FB7` | `0x70DE7..0x70F62` | `56EF:008E` | Takes three words. For party slots 0 to 3 whose word at `+1` is not `0xFFFF` it keeps the words at `+0x0A` and `+0x0C`, may set them to `16 * argument + 8`, negates the word at `+0x16` through the table at `DS:6578`, clears bit 6 of byte `+5` of the slot's entry under `DS:67B7`, calls `+0699` and `+018F`, and writes the slot's record with `+0840` under tag 3 and number `slot + 1` (removing it for an empty slot). It then restores the kept words, releases each slot through `31E0:05BB` and sets `DS:2A62` to 1. |
| `+135B` | `0x7118B..0x7129C` | `56EF:0098` | Takes a slot, `n` and two coordinates. Opens the archive, reads a counter from `RGTP/n`, sets the slot's position, writes the slot through `+018F` and `+0840` under number counter + 5, writes the counter plus 1 back to `RGTP/n`, releases the slot and closes the archive. Overlay 188 `+09B2` calls it. |
| `+1615` | `0x71445..0x71659` | `56EF:00A7` | Takes a flag. Loads `GPLI/1` (size from `38FF:05B5`, bytes from `38FF:04AB`) as 6-byte entries, walks the 19-byte records at `4F49:08AB` from the index in `DS:5AF5` along the byte at `+0x8B5` of each, and replaces each word pair equal to an entry's second and third words with its first word; with the flag set it does the same over 200 13-byte records at the far pointer `57E0:40C0`. Frees the table through `444C:0092`. |
| `+1829` | `0x71659..0x71869` | `56EF:00AC` | The reverse of `+1615`: replaces a word equal to an entry's first word with its second and third. |
| `+1A39` | `0x71869..0x718B9` | `56EF:00B1` | Enumerates through `2D40:0D94` until `0x270F` and releases each slot whose type byte has its low three bits below 6. |
| `+1A89` | `0x718B9..0x71997` | `56EF:00B6` | Takes `n`. Walks the 10-byte item list `2D40:2196` returns and, for each kind-1 item whose 23-byte record under the far pointer `DS:19C1` has `0xFF` at `+0x13`, stores the byte `2D40:2B6A` returns. |
| `+1C57` | `0x71A87..0x71B1E` | `56EF:00C0` | Allocates the bitmaps through `+1DD8`, then for each of 128 by 98 cells sets a bit in the bitmap at `DS:55B8` when `25AF:00E1` is true and `25AF:0576` false, and a bit in the one at `DS:55B4` when `25AF:014B` is true. |
| `+1CEE` | `0x71B1E..0x71C08` | `56EF:00C5` | When the word at `DS:0538` is not 0, calls overlay 182's `+0297` with `DS:140C`, loads `GMAP/[DS:140C]` into `DS:0538` again, applies the two bitmaps through `25AF:017F`, `01DD`, `01AE` and `020C`, and closes the handle at `DS:145A` through `38FF:02B5`. |
| `+1DD8` | `0x71C08..0x71C41` | `56EF:00CA` | Allocates 3,136 bytes through `444C:0008`, stores the pointer at `DS:55B8` and the pointer plus `0x620` at `DS:55B4`. |
| `+1E11` | `0x71C41..0x71C5C` | `56EF:00CF` | Takes a destination. Copies the 3 bytes at `4F49:0C33 + 3 * [DS:1A32]` there with `1000:3F5A` and zeroes that entry's first byte. |
| `+1E43` | `0x71C73..0x71C8E` | `56EF:00D4` | Takes a source and copies 3 bytes from it into the same entry. |
| `+1E63` | `0x71C93..0x71E9E` | `56EF:0110` | The region copier of FND-VIDEO-007. |
| `+21EA` | `0x7201A..0x720E3` | `56EF:010B` | Takes a source and a destination path. When `DS:13F7` is set it calls `+2B6C`; it opens both files (`44DE:0014`, `44DE:0086`), copies in 4,096-byte chunks through the buffer at `DS:45AC` (`44DE:01AF` reads, `44DE:022C` writes), calls `+22B3` with each chunk's length and closes both. |
| `+22B3` | `0x720E3..0x72213` | `56EF:00D9` | Takes a length. Adds it to the double word at `DS:4454`, scales the sum by `DS:444E` to 0 to `0x13F`, and when the flags at `4E71:0004`, `DS:13F7`, `DS:14E3`, `DS:14E4` and `DS:1439` allow it draws a bar of that width through `1BF3:282D`, `1BF3:2D44`, overlay 190's `+34E7` and `2D40:3BEC`/`3BD1`. Overlay 182 calls it three times. |
| `+23E3` | `0x72213..0x72254` | `56EF:0115` | Deletes the installed speech files when bit 2 (value 4) of `DS:55BC` is set (FND-SOUND-006). |
| `+2424` | `0x72254..0x722C6` | `56EF:00DE` | Takes a code. Reads `DATA/1002` through `38FF:04AB`, returns the double word at `8 * (code - 0x31)` of it (code 1 counts as `0x31`) and frees the buffer. |
| `+2496` | `0x722C6..0x72376` | `56EF:00E8` | Formats a message from `DS:4454` and `DS:140C`, frees `DS:14DF`, sets `DS:4454` to 0, waits on `4611:0051`, checks with `44DE:0086` that the disc's resource file opens when `DS:14E8` is set, and calls `+2B91` with 1. Overlays 182 and 192 call it. |
| `+2546` | `0x72376..0x72632` | `56EF:00FC` | The cinematic routine of FND-VIDEO-004. |
| `+2802` | `0x72632..0x726E3` | `56EF:00ED` | The length check of FND-VIDEO-004. |
| `+28B3` | `0x726E3..0x72851` | `56EF:011A` | The cinematic fallback of FND-VIDEO-006. |
| `+2A21` | `0x72851..0x7299C` | `56EF:0124` | The cinematic 5 pictures of FND-VIDEO-006. |
| `+2B6C` | `0x7299C..0x729C1` | `56EF:00F7` | Sets `DS:4263` to 0 and, when `DS:13F7` is set, calls `4A32:0185` and `2834:0001` (FND-SOUND-008). Overlays 182, 196 and 203 call it. |
| `+2BAC` | `0x729DC..0x72B8B` | `56EF:011F` | The installed-cinematic count of FND-VIDEO-004 and FND-VIDEO-007. |

FND-INPUT-009 calls the routine at `0x71659` dispatcher-shaped with a fourth case that requests
`GPLI/1`. In this inventory that address starts `+1829`, a single routine with no case table whose
first request is `GPLI/1`; the routines that call it (`+0A61`, `+0CBD`) are outside this batch.
FND-PARTY-240's conversions at `0x7154A`, `0x71738` and `0x7175C` lie in `+1615` and `+1829`.

## Interpretation

These routines hold no single feature. `+07DA`, `+0840`, `+08A5`, `+0FB7` and `+135B` move party
and object state into and out of `DARKRUN.GFF` resources. `+1615` and `+1829` turn the 6-byte
`GPLI/1` entries into one word and back, which fits a pass before and after saving.
`+1C57` to `+1DD8` keep two 128 by 98 bitmaps for the map in `DS:0538`. `+1E11` and `+1E43` move a
3-byte entry selected by `DS:1A32`. The rest copy cinematic and data files from the disc, show the
copy's progress and play cinematics, as FND-VIDEO-004, FND-VIDEO-006 and FND-VIDEO-007 read.
The names stay `fn_187_<offset>` until a reading shows what each piece of state is.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine with no
recorded caller may still run. The fields `+0FB7` touches are named by offset only; their meaning
in the party record is not read here. Whether FND-INPUT-009's "fourth case" was read across a
boundary that has since moved is not settled here.

## How to reproduce

Run `overlay_listing.py` for overlay 187 against the installed `DSUN.EXE` and `direct_callers.py`
for each offset in the table, and compare the ranges with the rows of
`coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`.
