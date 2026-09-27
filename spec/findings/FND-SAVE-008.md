---
id: FND-SAVE-008
title: Saving updates DARKRUN.GFF resources before copying it to a numbered save file
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5736:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56EF:0084
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4544:0000
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV overlay 187 and resident file-copy entry; FBOV descriptor and trampoline inspection
environment: null
---

## Observation

The Save Game routine in overlay 192 passes the selected 125-byte record's
path field and its description field at offset 80 to overlay 187's
`56EF:0084` trampoline (FND-SAVE-004). The trampoline targets
`DSUN.EXE+0x00070BFD`.

That routine first calls the local archive-open path at `0x00071A17`.
It names the string `darkrun.GFF` at `DS:172D` and places the opened
archive handle at `DS:144E`. On the opened-handle path it then calls `0x00070713` with zero and the
description pointer. In that path, the routine considers resource numbers
1 through 61 under tag `SAVE`; for each nonzero source entry it removes
and writes the numbered resource through the archive entries. When the
description pointer is nonzero it also removes and writes `STXT/1` with a
requested length of 45 bytes. These write paths pass the handle at
`DS:144E`. The outer routine closes that handle through `0x00071A56` on this path.

Next it joins the game's directory at `DS:44F2` with `darkrun.GFF` and
calls resident `4544:0000` with that path as source and the selected
record's path as destination. The resident routine opens the first path
for reading and the second for writing, then loops over file reads and
writes, checks each transferred length, closes both handles and returns
its success flag. The selected record's path is the numbered
`SAVEnn.SAV` path (FND-SAVE-004). After this wrapper returns, the overlay
192 caller separately removes and writes `PREF/100` and `GREQ/n` through
another resource handle (FND-SAVE-004).

## Interpretation

Saving creates a numbered `SAVEnn.SAV` as a copy of the working
`DARKRUN.GFF` archive after the `SAVE` resource group and the player's
`STXT/1` description have been submitted to that archive. This directly
explains why the list reader can request `STXT/1` from a saved-game file
(FND-SAVE-007). The subsequent `PREF` and `GREQ` writes are a distinct
step, not part of the file-copy call.

## Alternatives

The source table for the numbered `SAVE` resources has not been fully read,
so this finding does not assert that all 61 are present or give their
layouts. The remove and write results are not all checked by the caller;
the exact contents of a failed or interrupted save are not established.
The archive targeted by the later `PREF` and `GREQ` calls requires its own
handle trace. Other resources already in `DARKRUN.GFF` remain unidentified.

## How to reproduce

Resolve the far call at `DSUN.EXE+0x0007D84A` through descriptor 187 and
trampoline `56EF:0084`. Disassemble the bounded wrapper at
`0x00070BFD..0x00070CED`, the resource writer at
`0x00070713..0x0007089B`, and the archive open/close entries at
`0x00071A17..0x00071A86`. Read the short `DS:172D` string. Resolve raw
call segment `0200` through descriptor 64 to resident `4544:0000`, then
inspect its open, read, write and close loop. Compare the selected record
path and the subsequent resource writes with FND-SAVE-004.
