---
id: FND-SAVE-020
title: The Save and Load list search finds SAVE??.SAV files and puts each in the record its two digits minus 1 name, with no upper bound; the window fills its ten rows from save_list_top, which overlay 192 never writes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007D300..0x0007D6FB
tool: tools/research/exec-census/overlay_listing.py and immediate_search.py at the commit that adds this finding; Capstone 5.0.9; Python 3.14.7
environment: null
---

## Observation

Overlay 192 offsets `+x` are at file `0x7D300 + x`.

`+00F2(count)` is called with 10 by the shared window routine `+024B`. It clears every record's
path byte and sets its description to the empty string at `DS:2084`, then joins the game directory
`DS:44F2` and `SAVE??.SAV` (`DS:2085`) and runs DOS find-first and find-next (`44DE:0340`,
`44DE:03AC`) with attribute 0. For each file found it cuts the pattern after the last backslash
(`0x5C`), appends the found name, converts the two characters after `SAVE` with `atoi` and
subtracts 1. When the result is not negative it stores the path in that record and calls `+002A`,
which opens the file as an archive, reads `STXT/1` into the record at offset 80 and closes it; a
failed open or read clears the path byte (FND-SAVE-007). The index is not compared with the count.

`+024B` then fills the ten rows from records `save_list_top + row` (`4C4C:0002`) and marks row
`4C4C:0000`. Neither routine writes `4C4C:0002`.

## Interpretation

Record `k` holds `SAVE(k+1).SAV` whatever the search order. A file numbered 11 or more in the game
directory is stored past the ten records the buffer holds, and a name whose two characters do not
start with a digit gives -1 and is skipped. A nonzero `save_list_top` would show records past the
tenth.

## Alternatives

What fills `DS:44F2` at startup was not read; without a backslash the cut leaves the pattern in the
path. Whether any path leaves `save_list_top` nonzero when Save or Load opens was not established
(FND-UI-038).

## How to reproduce

`overlay_listing.py <dsun> 192 7D300 7D54B` and `... 192 7D54B 7D6FB`.
