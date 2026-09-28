---
id: FND-SAVE-009
title: The later PREF and GREQ save writes select the CHARSAVE.GFF archive
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56E9:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:002F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5736:0000
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV overlay entries and resident archive-open entry; FBOV descriptor and trampoline inspection
environment: null
---

## Observation

The routine at `DSUN.EXE+0x0006FC72` in overlay 186 checks the numeric
archive handle at `DS:144A` (FND-CONFIG-067). If it is zero, the routine
passes `DS:1388`, the literal `CHARSAVE.GFF`, together with `DS:144A` as an
output location and two option values to the overlay 182 trampoline
`56BD:002F`. That trampoline targets
`DSUN.EXE+0x00068850`. The wrapper there appends the supplied filename to
the game's directory at `DS:44F2` and passes the resulting path, output
location and options to resident `38FF:0066`. It returns success when the
resident call does not return `-1`. The companion routine at
`DSUN.EXE+0x0006FCAD` closes a nonzero `DS:144A` handle and clears it.

After the numbered save-file copy succeeds, overlay 192's Save Game routine
passes the numeric handle at `DS:144A` to the resource-handle selection entry
`0x0110:0064`. Only when that entry returns zero does it remove and write
`PREF/100` and `GREQ/n` through the resource entries (FND-SAVE-004). The
physical `DSUN.EXE` bytes contain only ten occurrences of the displacement
`4A 14`; the bounded instruction sites at `0x0006FC75`, `0x0006FC84`,
`0x0006FCB0`, `0x0006FCB8`, `0x0006FCD2` and `0x0007D864` include the
handle's zero test, initialization, close/reset and save selection.

## Interpretation

The later `PREF/100` and `GREQ/n` writes in Save Game select the
`CHARSAVE.GFF` archive by its numeric handle (FND-CONFIG-067). They occur
after the numbered `SAVEnn.SAV` copy from `DARKRUN.GFF` (FND-SAVE-008),
so they are not part of that copy.

## Alternatives

The remaining displacement occurrences in overlay 171 and overlay 184 also
use this handle but are not a complete audit of every indirect alias to it.
The exact option values and error behavior of the resident archive-open
routine have not been fully read. This finding does not establish whether
every remove and write succeeds, or the final contents of either archive
after an interrupted save.

## How to reproduce

Resolve overlay 186 and overlay 182 with
`tools/ghidra/ReportFbovOverlayMap.ps1`. Read the `DS:1388` string at file
offset `0x0004E388`. Disassemble the initializer and closer at
`0x0006FC72..0x0006FCDC`, resolve `56BD:002F` to the wrapper at
`0x00068850`, and follow its call to resident `38FF:0066`. Compare the
`DS:144A` selection at `0x0007D864..0x0007D8D9` with the resource calls
and the earlier file-copy call in FND-SAVE-004 and FND-SAVE-008.
