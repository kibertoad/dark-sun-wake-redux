---
id: FND-CONFIG-009
title: The Preferences renderer indexes its four difficulty labels with the first PREF word
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5782:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5000:A4B9..5000:A4C8
tool: Ghidra 12.1.3 mapped-overlay MZ import, JDK 21.0.12.1, ReportReferences and ReportInstructionContext; ReportFbovOverlayMap.ps1
environment: null
---

## Observation

In overlay 203, the routine at `DSUN.EXE+0x0008B845` reads the word at
`DS:143A` while drawing the Preferences screen. It uses that word twice as a
four-byte stride into the far-pointer array at `DS:26B9`. The four entries of
that array point to the difficulty labels Easy, Balanced, Hard and Hideous in
that order (FND-TEXT-005). The routine also passes the word to another drawing
call. The save routine copies `DS:143A` into the first two bytes of `PREF/100`,
and the load routine copies those bytes back (FND-SAVE-004, FND-SAVE-005).

Ghidra's references to `DS:143A` in the mapped image also include a write of
3 in overlay 171, a read in overlay 173, and the save/load copies in overlay
192. The mapped-image reference list does not by itself show a Preferences
button handler writing the word.

## Interpretation

`PREF/100` offset `0x00` is the saved difficulty-label index. Values 0 to 3
select the four labels in order. The installed resource's value 0 selects Easy
when that saved value is loaded; it does not establish the default for a new
game.

## Alternatives

The write of 3 in overlay 171 has not been traced through its condition or
callers, so it does not establish the starting difficulty. The drawing call's
other arguments have not been read, so this finding does not establish the
label's exact screen position or how button clicks change the index.

## How to reproduce

In the mapped-overlay import of the approved `DSUN.EXE`, report references to
`5000:923A` (`DS:143A`) and bounded instruction context at `9616:0592` and
`9616:05B6`. Convert those mapped addresses to the shipped file offsets with
`ReportFbovOverlayMap.ps1`. Read the four relocated far pointers at
`5000:A4B9` as FND-TEXT-005 describes, and compare FND-SAVE-004 and
FND-SAVE-005 for the resource offset.
