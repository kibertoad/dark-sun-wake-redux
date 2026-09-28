---
id: FND-CONFIG-060
title: No literal far call to overlay 180's close-all routine was found in the shipped executable
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56B2:0025
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56B2:0039
tool: Python 3.14.7 bounded byte-pattern and near-call-target searches; Ghidra 12.1.3 ReportReferences.java on the original MZ import
environment: null
---

## Observation

FND-CONFIG-059 maps overlay 180's `56B2:0025` trampoline to its
close-all routine at physical file offset `0x00067942`. In the shipped
`DSUN.EXE`, a raw search finds no four-byte literal far pointer with
offset `0x0025` and any of these segment representations: overlay
descriptor 180's fixup word `0x05A0`, the unrelocated resident segment
`0x46B2`, or the segment `0x56B2` after loading at `0x1000`.

The same search method finds four literal offset-and-segment pairs for
another overlay 180 trampoline, `56B2:0039`, with segment word
`0x05A0`, and two with the unrelocated resident segment `0x46B2`.
The raw pattern search therefore detects known literal references to
this overlay in both code representations. A scan of overlay 180's
declared 2,948 code bytes also finds no near-call displacement targeting
`0x00067942`.

Ghidra's original MZ import reports no recognized reference to
`56B2:0025` through `ReportReferences.java`. That analyzer result is
supplementary; its recognized references have missed other confirmed
code uses in this executable (FND-CONFIG-037).

## Interpretation

The surveyed literal far-call encodings and local near calls do not
explain how the close-all routine is reached. FND-CONFIG-061 identifies
its exit-callback registration as separate word pushes, which this
contiguous-pointer search does not detect. Other indirect or
unexamined routes remain possible; the search does not establish that
the routine is unused.

## Alternatives

Other calls could be formed or reached through data, a register or memory
operand, or code outside the surveyed overlay 180 range. The negative
inventory does not establish archive state at any message call or that
the close-all operation runs only during shutdown.

## How to reproduce

For the approved `DSUN.EXE`, search the complete shipped file for the
little-endian pointer pairs `25 00 A0 05`, `25 00 B2 46` and
`25 00 B2 56`. As a positive control, search for `39 00 A0 05` and
`39 00 B2 46`. In overlay 180's declared code range
`0x000671E0..0x00067D64`, check each possible near-call displacement
against the target `0x00067942`. In the existing original MZ Ghidra
import, run `ReportReferences.java` for `56B2:0025` and retain the
analyzer result as a separate, weaker observation.
