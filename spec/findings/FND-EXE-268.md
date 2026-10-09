---
id: FND-EXE-268
title: Another initial loader-segment pointer targets the filename comparison input rather than the selected root
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004B326..0x0004B32A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00040059..0x0004005E
tool: Python 3 and xxhash 4.0.1
environment: null
---

## Observation

The initial words at shipped offsets `0x0004B326` and `0x0004B328` are
offset `0x000A` and stored segment `0x3AE5`. The latter site is an MZ
relocation; loading at segment `0x1000` gives `4AE5:000A`, shipped offset
`0x0004005A`. This target is not FND-EXE-175's selected root at
`4AE5:0010`. It lies inside the five-byte CS-relative comparison input
`4AE5:0009..4AE5:000E` read by FND-EXE-264's filename helper.

Enumerating the MZ relocation table and selecting stored segment word
`0x3AE5` returns four positive sites: `0x0004AEE4`, `0x00050998`,
`0x000509C2` and `0x0004B328`. Their immediately preceding words are
respectively `0x04F4`, `0x0D27`, `0x0193` and `0x000A`. The first is
FND-EXE-176's independently known initial handler-pointer control. These
are relocated segment operands with adjacent words, not automatically
four admitted call targets or four valid far-pointer records.

## Interpretation

This adds a concrete non-call pointer candidate to Q-EXE-001 and
Q-EXE-010's native-entry search. Its target overlaps a known data consumer's
input, so merely decoding bytes at that target cannot admit it as the
loader root. The use, lifetime and consumers of this initial pointer remain
unread. The other adjacent-word candidates likewise require storage and
consumer readings before admission.

This is a positive enumeration under one stored segment value, not an
absence-of-caller proof. Segment aliases with other stored values, FBOV
fixups, unrelocated pointers, computed/runtime-written targets, direct
relative transfers and targets to interior root instructions are outside
this query. No complete_reading or inventory replacement follows.

## Alternatives

Equating a matching segment with a matching procedure would ignore the
offset and the independently read comparison input. Treating every word
before a relocated segment operand as a far-pointer offset would assume a
layout not yet read. The initial pointer could be consumed as data or
interpreted differently by a later consumer; neither use is established.

## How to reproduce

At revision `4ff32e5`, read the installed source identity in FND-EXE-236
and verify its XXH3-128 before examining the MZ table. Read relocation
count at header offset six, table offset at `0x18`, and header paragraph
count at eight. For each relocation entry's little-endian offset and
segment, compute shipped site as header-size plus sixteen times segment
plus offset. Select entries whose stored word equals `0x3AE5`, and report
only the site and immediately preceding word. Check the four positive
results above, including the independently known handler-pointer control.

For the selected initial pair, add load segment `0x1000` to its stored
segment, retain offset `0x000A`, and map it using MZ header size `0x5200`.
Compare its target with FND-EXE-264's five-byte CS-relative input interval
and FND-EXE-175's selected entry. Do not turn a linear decode of the data
input into callable-entry admission. Original source and local reports
remain in GAME_DIR.
