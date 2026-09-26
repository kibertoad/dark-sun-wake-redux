---
id: FND-TEXT-005
title: DSUN.EXE holds the Preferences difficulty labels, descriptions and About lines as one data block
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5000:A4B9..5000:A6CA
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

The 529 bytes from `5000:A4B9` (file offset `0x4F6B9`) to `5000:A6CA` in the resident load image
of `DSUN.EXE` read as follows:

- At `5000:A4B9`, four far pointers, each a 16-bit offset followed by a 16-bit segment. They are
  stored as `47E0:26F5`, `47E0:26FA`, `47E0:2703` and `47E0:2708`, and the MZ relocation table
  has an entry for each segment word, so with the load image at segment `0x1000` they point at
  `57E0:26F5` and so on: `5000:A4F5`, `5000:A4FA`, `5000:A503` and `5000:A508`.
- At `5000:A4C9`, four 16-bit values: 205, 143, 231 and 24.
- At `5000:A4D1`, nine far pointers stored and relocated the same way, to `5000:A5CC`,
  `5000:A5EA`, `5000:A605`, `5000:A620`, `5000:A642`, `5000:A65F`, `5000:A67D`, `5000:A696` and
  `5000:A6AE`.
- From `5000:A4F5`, 25 strings of printable ASCII, each ended by a NUL and each starting right
  after the NUL of the one before: the four the first pointers name, 4, 8, 4 and 7 characters
  long; at `5000:A510` the 16-character pattern `%c:\RESOURCE.GFF`; at `5000:A521` the pattern
  `%C%C%C%s`; from `5000:A52A` ten strings of 19, 13, 20, 19, 15, 17, 5, 21, 9 and 14 characters;
  and from `5000:A5CC` the nine strings the second pointers name, of 29, 26, 26, 33, 28, 29, 24,
  23 and 27 characters, each starting with `%C%C%C`. The ninth ends at `5000:A6CA`.

Read as text, the first four strings are the four difficulty choices that SRC-MANUAL-1994 (page
15) gives for the Preferences screen, in the manual's order. The ten strings from `5000:A52A`
describe the settings of that screen one by one, and the nine from `5000:A5CC` hold the title,
copyright, publisher, support and hint-line information that the manual says the About button
shows.

## Interpretation

The block is the executable's data for the Preferences screen: a table of the difficulty labels,
a table of the About lines, and the strings they point at, with the ten descriptions stored
between them without a pointer table of their own. The pattern `%C%C%C%s` and the `%C%C%C` that
starts each About line suggest the three `%C` are formatting controls the game's own text
routine interprets.

## Alternatives

Nothing in the block shows what reads it, what the four values at `5000:A4C9` are, which
description belongs to which control, or what `%C` does. The descriptions could be reached
through a table elsewhere or by walking from one NUL to the next.

## How to reproduce

Read the bytes at file offset `0x4F6B9`, `0x5200 + (0x5000 - 0x1000) * 16 + 0xA4B9`, decode the
pointers and walk the strings from `5000:A4F5`. Check the MZ relocation table (its count at
header offset `0x06`, its offset at `0x18`) for entries at the load-image offsets of the pointers'
segment words.
