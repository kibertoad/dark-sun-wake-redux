---
id: FND-SAVE-024
title: Only the stored-character list of overlay 171 writes save_list_top, leaving its page start there when it closes, so the Save and Load screens open at that page
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058210..0x00058AF4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079100..0x00079260
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00088BC2..0x00088C80
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000416C0..0x000416C8
tool: tools/research/exec-census/segment_offset_refs.py, segment_references.py, immediate_search.py, direct_callers.py and overlay_listing.py at the commit that adds this finding; Capstone 5.0.9; Python 3.14.7
environment: null
---

## Observation

Overlay 171 offsets `+x` are at file `0x58210 + x`, overlay 190 at `0x78340 + x`, overlay 199 at
`0x87900 + x`.

Segment `4C4C` is descriptor 95, 8 bytes: the selected line (word 0), `save_list_top` (word 2) and
a far window pointer. All 8 bytes are 0 in the file (file `0x416C0`). `segment_references.py <dsun>
4C4C` finds the segment word in the segment table and at 127 code sites, 86 in overlay 171 and 41
in overlay 192; each loads it into `ES` and uses a fixed operand `es:[0]` to `es:[6]`. None pushes
the segment, builds a far pointer from it or passes it to a copy or a read.
`immediate_search.py <dsun> 3C4C` finds no resident load of it.

`segment_offset_refs.py <dsun> 4C4C 2` finds six stores, all in overlay 171's list `+0126` and its
window handler `+03C5`:

| Site | Value |
|---|---|
| file `0x58303` (`+00F3`), in `+0020` | 0, when no entry outside the party is found from the current top and line |
| file `0x5870F` (`+04FF`), Down key at line 9 | `DS:044C` after adding 10, kept from 0 to the count less 10 |
| file `0x58750` (`+0540`), Down on the last entry or Up on entry 0 | 0 |
| file `0x5884F` (`+063F`), Up key at line 0 | `DS:044C` after taking 10, kept from 0 |
| file `0x588B9` (`+06A9`), button `0x082D` or key word `0x284A` | `DS:044C` after taking 10, kept from 0 |
| file `0x58904` (`+06F4`), button `0x082E` or key word `0x284B` | `DS:044C` after adding 10, kept from 0 to the count less 10 |

The count is `DS:40C6`, the number of stored characters, at most 39. After each step `+0020` keeps
the top when an entry outside the party lies at or after it and stores 0 otherwise. Overlay 192
only reads the word (five sites). Nothing in overlay 171 resets it when the list closes.

`direct_callers.py` finds one call of `171 +0126`: overlay 190 `+0ED4`, which the ADD choice
(`+0F00`) and the full-store DELETE CHARS choice (`+0EB8`) of an empty character box reach.
`immediate_search.py <dsun> 044C` finds `DS:044C` used in overlay 171 and in overlay 199
(`0x88BC2..0x88C80`), which steps it by 5 and keeps it from 0 to the byte `DS:6322` less 5.

## Interpretation

The Load Game and Save Game screens open at the page the stored-character list last showed. With
11 or more stored characters, paging down in the ADD list and then opening Save or Load shows
saved games 11 to 20 or later in the ten rows; a fresh install has four stored characters and
opens at 0. Since overlay 199 shares `DS:044C`, a page up in the list after a scrolled dialogue
may start from the dialogue's value.

## Alternatives

Which on-screen buttons give `0x082D` and `0x082E` and which keys give `0x284A` and `0x284B` was
not established. A computed copy or read that runs past the end of segment `4C49` into `4C4C` was
not excluded; no far pointer to `4C4C` exists. What `DS:6322` holds, and so whether a dialogue can
leave `DS:044C` at 11 or more, was not read. The routines of overlay 171 after `+08E4` were not
read; the operand census shows they do not store the word.

## How to reproduce

`segment_offset_refs.py <dsun> 4C4C 2`; `segment_references.py <dsun> 4C4C`;
`immediate_search.py <dsun> 3C4C 044C`; `direct_callers.py <dsun> 171+0126 171+0020`;
`overlay_listing.py <dsun> 171 58210 58AF4`, `... 190 79100 79260`, `... 199 88BC2 88C80`; read 8
bytes at file `0x416C0`.
