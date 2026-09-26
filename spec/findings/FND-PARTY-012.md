---
id: FND-PARTY-012
title: Overlays 171, 184 and 186 keep stored characters under CACT numbers 1 to 39 with their CHAR, SPST, PSST and PSIN
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5664:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56DD:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56E9:0000
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

The code is in the FBOV overlays of `DSUN.EXE` (FND-EXE-003), numbered by segment-table
descriptor. Overlay code has no `segment:offset` address, so the locations are the resident
headers of the overlays: 171 at `5664`, 184 at `56DD` and 186 at `56E9`. Offsets below are file
offsets. Each tag is pushed as a 32-bit immediate (`66 68` and the four tag bytes; the tag bytes
are the occurrences FND-EXE-006 lists). The far calls go through three entry points, at offsets
`0x04AB`, `0x05C5` and `0x00E8` of their segments, with the same arguments as the transfer
utility's read, remove and write routines (FND-PARTY-011); their segments are fixed up when the
overlay loads.

Overlay 186 (code at `DSUN.EXE+0x0006F700`, 1,709 bytes):

- The routine at `DSUN.EXE+0x0006F8B6` takes a slot index and a number. For each of `SPST`,
  `PSST` and `PSIN` in turn it calls the remove entry and then the write entry with that tag and
  number, writing 15 bytes from offset `0x039C + slot * 15`, 34 bytes from offset
  `0x09CD + slot * 34` and 1 byte from offset `0x0A55 + slot` of two data segments. It returns 0
  as soon as a write returns `0xFFFF`, and 1 otherwise. `0x09CD + 4 * 34` is `0x0A55`.
- The routine at `DSUN.EXE+0x0006F982` takes a slot index and a number and reads the same three
  resources into the same places through the read entry, returning 0 on the first failure and 1
  otherwise.
- The routine at `DSUN.EXE+0x0006FAC1` reads `CACT` 1 to 39 and returns 1 when all 39 read and
  hold a nonzero word, as the transfer utility's `13D8:040E` does.

Overlay 184 (code at `DSUN.EXE+0x0006D090`, 8,992 bytes):

- The routine at `DSUN.EXE+0x0006E4D5` takes a slot index. It has the shape of the transfer
  utility's `13D8:046C`, with two differences: it scans `CACT` 1 to 39 rather than 0 to 39, and it
  takes the identifier from offset 6 of a 49-byte record in a table the far pointer at `DS:19C9`
  points to. It removes and writes the 2-byte `CACT`, then calls another overlay's routine with
  `CHAR`, the number and the slot, and on success a routine with the slot and the number.
- At `DSUN.EXE+0x0006EDE4` a loop reads `CACT` 1 to 39 looking for a given word.

Overlay 171 (code at `DSUN.EXE+0x00058210`, 4,170 bytes):

- From `DSUN.EXE+0x000584C7`, a loop over the numbers 1 to 39 reads each `CACT`, and for each that
  reads and is nonzero, calls a far routine with a working slot, the number, `CHAR` and 7, then
  appends a 51-byte entry to the list the far pointer at `DS:40C8` points to: the identifier from
  the loaded character, the number, and the character's name. It stores the entry count at
  `DS:40C6`.
- At `DSUN.EXE+0x00058CFD` it calls the same far routine with `CHAR` and the number of one list
  entry, chosen by two words at the start of another data segment.
- From `DSUN.EXE+0x000591A2` it looks for the `CACT` among 1 to 39 whose word equals the
  identifier of one list entry, and removes it and writes it again holding 0.

## Interpretation

The game keeps a store of up to 39 characters in its character archive, one per number from 1 to
39, with the character's identifier in `CACT` and its records in `CHAR`, `SPST`, `PSST` and
`PSIN` under the same number, the same scheme the transfer utility writes to. Overlay 184 saves a
party member into the store, replacing the entry with the same identifier or taking a free
number. Overlay 171 builds the list of stored characters from every nonzero `CACT`, loads the one
the player picks, and deletes one by setting its `CACT` to 0, which frees the number while its
other resources stay in the file until they are replaced. `DSUN.EXE+0x0006FAC1` tests whether the
store is full. The party is four slots, one `PSIN` byte, one 34-byte `PSST` and one 15-byte
`SPST` each.

The installed `CHARSAVE.GFF` holds `CACT` 29 to 39, of which 36 to 39 are nonzero, so its store
lists four characters (FND-PARTY-005).

## Alternatives

Which archive the entry points act on is not shown here; `CHARSAVE.GFF` is the only file that
holds these tags. What the far routine called with 7 does with its fourth argument, which screens
call these routines, and what the 51-byte list entry holds beyond the three fields are not read.
That overlay 171 serves the ADD choice of the View Character screen (SCR-UI-002) and its
stored-character list (SCR-UI-003) is inferred from the manual and has not been traced.

## How to reproduce

Map the overlays with `tools/ghidra/ReportFbovOverlayMap.ps1 -SourcePath <DSUN.EXE>`, search the
code block of each overlay for `66 68` followed by each tag, and disassemble each routine from
its start.
