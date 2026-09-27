---
id: FND-CONFIG-025
title: The resident PREF bytes are part of a Preferences label
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:0E81..57E0:0E90
tool: bounded physical-byte inspection and Ghidra 12.1.3 ReportReferences.java mapped-image query
environment: null
---

## Observation

The `PREF` hit at `DSUN.EXE+0x0004DE85` (`DS:0E85`) begins four bytes
into the null-terminated label `SET PREFERENCES` at `DS:0E81`. Its NUL is
at `DS:0E90`. A resident pointer table at file offset `0x0004DE46`
holds the far pointer to the label's start, `57E0:0E81`, between pointers
to other menu labels. Thus those four bytes are not a standalone `PREF`
resource tag.

The other three physical `PREF` hits remain the instruction-embedded tags
in overlay 192's Save Game and Load Game paths (FND-SAVE-004,
FND-SAVE-005). Ghidra found no recognized references to either the label
start or the interior `PREF` bytes in its mapped image. A raw search for
the little-endian interior offset `0E85` found no such offset: its one
byte-pair match in overlay 172 is part of a different instruction.

## Interpretation

The resident `PREF` byte match is a label substring, not evidence of a
fourth resource-tag use. The direct instruction-embedded tag uses found by
the physical search remain confined to the save/load paths examined so
far. This removes the resident-label occurrence as a lead for new-game
settings initialization.

## Alternatives

The pointer table's exact rendering caller was not read. An indirect,
computed or copied `PREF` tag, or a path that initializes settings without
reading `PREF`, remains possible. Ghidra's missing recognized reference
does not rule out the label's use, and the raw search does not establish
the absence of all other resource operations.

## How to reproduce

In the approved `DSUN.EXE`, inspect only file offsets
`0x0004DE81..0x0004DE91` for the string boundary and
`0x0004DE3E..0x0004DE56` for the pointer table. Search for physical
`PREF` bytes as in FND-CONFIG-016, then query mapped addresses
`5000:8C81` and `5000:8C85` with `ReportReferences.java`. Check the
single raw `85 0E` hit at `0x0005A3F7` in bounded instruction context
before interpreting it as a displacement.
