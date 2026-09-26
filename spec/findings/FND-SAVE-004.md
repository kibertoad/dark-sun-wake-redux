---
id: FND-SAVE-004
title: Overlay 192 saves a game as SAVEnn.SAV and writes PREF 100 and GREQ nn with nine bytes each
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
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

The code is in overlay 192 of the `FBOV` pack of `DSUN.EXE` (FMT-EXE-001), whose resident header
is at `5736:0000` and whose 3,680 bytes of code start at file offset `0x7D300`. Offsets below are
file offsets. A segment in overlay code is a fixup word holding a segment-table index times 8
(FMT-EXE-005); a fixup that names a resident segment's descriptor (FMT-EXE-002) is given here as
that segment with the load image at `1000`. `DS` is the data segment `57E0`. The resource entry
points at offsets `0x04AB`, `0x05C5` and `0x00E8` are the read, remove and write entries of
FND-PARTY-012.

The routine from `DSUN.EXE+0x0007D6FB` to `DSUN.EXE+0x0007D8EC` takes one 16-bit argument, a
slot. It:

1. copies nine bytes of globals into a local buffer in this order: the word at `DS:143A`, then
   the bytes at `DS:26B4`, `DS:26B5`, `DS:26B6`, `DS:1435`, `DS:1436`, `DS:1437` and `DS:1439`;
2. copies ten bytes into a second buffer: the words at `DS:14D7`, `DS:14D9`, `DS:14DB` and
   `DS:14DD`, then the byte at `DS:4459`, then the byte at `DS:4458`;
3. adds the word at `4C4C:0002` to the slot, giving a number n;
4. formats `SAVE%.2d.SAV` (`DS:20B6`) with n + 1 into a local string;
5. copies the string at `DS:44F2` into the 125-byte record at the far pointer held at `DS:6278`
   plus n * 125, and appends the local string to it, at most 80 characters;
6. shows `SAVING GAME` (`DS:20C3`);
7. calls a far routine with the record and the record plus 80. When it returns 0 in `AL`, the
   routine shows `Failed to save the game!` (`DS:20CF`) and returns;
8. otherwise calls a far routine with the double word at `DS:144A`, and when that returns 0,
   removes `PREF` resource 100 and writes it with the nine bytes of the first buffer, then removes
   `GREQ` resource n + 1 and writes it with the first nine bytes of the second buffer;
9. shows `GAME SAVED` (`DS:20E8`).

The results of the remove and write calls are not checked. The pushes of `PREF` are at
`DSUN.EXE+0x0007D878` and `DSUN.EXE+0x0007D88B`, and of `GREQ` at `DSUN.EXE+0x0007D8AA` and
`DSUN.EXE+0x0007D8C3`. The segment `4C4C` is the fixup word `0x02F8` at overlay offsets `0x045A`,
`0x063C` and `0x072C`: descriptor 95, whose `segment` is `0x3C4C`.

Between steps 5 and 6 the code tests the far pointer to the record plus 80 twice, once for zero
and once for nonzero, before a second append of 80 characters to it. No value passes both tests,
so the second append never runs.

## Interpretation

This is the Save Game routine. `DS:44F2` holds the game's directory, and the record is one entry
of a list of saved games: the path of the file `SAVE01.SAV` to `SAVE10.SAV` in its first 80 bytes
and the player's description from byte 80. The word at `4C4C:0002` is the number of the first
saved game the list shows, so n is the saved game's index from 0 and n + 1 its number. The game
keeps nine bytes of settings in `PREF/100`, one copy for every saved game, and nine bytes of
game state per saved game in `GREQ`. The tenth byte of the second buffer, from `DS:4458`, is not
written.

## Alternatives

Which archive the resource entries write to is not shown here; the shipped `CHARSAVE.GFF` holds
`PREF/100` and `GREQ/1` to `/10` (FND-SAVE-001). The far routine of step 7 is taken to write the
saved game's file, and that of step 8 to report whether the archive can be written; neither was
read. That the loader puts the descriptor's segment in place of a fixup word is an assumption
(FMT-EXE-005); a far call of overlay 180 resolved the same way lands on the routine that loads
the sound configuration, which fits it.

## How to reproduce

Place overlay 192 with `tools/ghidra/ReportFbovOverlayMap.ps1`, resolve each fixup word through
the segment table at file offset `0x4B080`, and disassemble from file offset `0x7D6FB` to
`0x7D8ED`.
