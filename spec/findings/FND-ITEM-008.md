---
id: FND-ITEM-008
title: Two item names seen on the inventory screen occur once each, in TEXT 1000 of RESOURCE.GFF
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x17D10..0x17D19
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x17D2A..0x17D30
tool: DarkSunWakeRedux.Inspect resource-pattern, reproduced with Python 3.14.7
environment: null
---

## Observation

The inventory capture `dsun_017.png` (FND-UI-021) shows, among others, two item names: one of 9
letters and one of 6. A case-sensitive search of `RESOURCE.GFF` finds each name once, both inside
`RESOURCE.GFF#TEXT/1000`, which is 2,403 bytes and starts at file offset `0x17C28`. Counting lines
from 0 and splitting at CR LF (FMT-TEXT-003), the 9-letter name is the whole of line 25, starting
at byte 232 of the resource, and the 6-letter name is the whole of line 28, starting at byte 258.

## Interpretation

`TEXT/1000` holds item names, one per line, and the inventory screen shows names from it. The
line numbers suggest that an item's name is chosen by a line index.

## Alternatives

The screen may take the names from a copy of the text elsewhere, or the same lines may serve
another screen. What selects a line is not known.

## How to reproduce

Read the two names from the capture, search `RESOURCE.GFF` for each, and locate each match
inside its resource through the directory (FMT-GFF-001) and its line by counting CR LF pairs.
