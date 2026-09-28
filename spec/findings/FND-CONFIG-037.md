---
id: FND-CONFIG-037
title: Opening and closing resource archives changes the pointer tested by message acquisition
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:0066
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:02B5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:07B1
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded resident windows; Ghidra 12.1.3 ReportReferences.java and ReportInstructionContext.java
environment: null
---

## Observation

The resident resource reader at `38FF:07B1` checks the far pointer at
`DS:9D9F` before searching records (FND-CONFIG-034). The resident archive
open entry at `38FF:0066` checks an opened file's initial header, allocates
an archive record and checks its reads. On its success path it links the
new record with the record at `DS:9DA3`, writes the new far pointer to
`DS:9DA3` and `DS:9D9F`, increments the count at `DS:9D9D` and returns
zero. Its failure paths return `0xFFFF` before those final assignments.

The close entry at `38FF:02B5` can clear `DS:9D9F` and `DS:9DA3` when the
count is one, or replace `DS:9D9F` with a linked record pointer when
closing the selected record. A separate resident helper at `37ED:0064`
also assigns `DS:9D9F` only after obtaining a nonzero archive pointer;
another list-update path at `37FC:077C` can replace it. FND-SAVE-007
identifies a Save/Load path that uses the same open and close entries for a
saved-game archive.

Ghidra's direct-reference list for `DS:9D9F` finds the close and two other
writers but misses the archive-open assignment at `DSUN.EXE+0x0002E46F`;
bounded physical disassembly confirms that assignment. A raw search of the
loaded bytes finds seven null-terminated `RESOURCE.GFF` strings, none with
a recognized direct reference in the existing Ghidra import. That result
does not identify which startup path opens the file.

## Interpretation

The pointer required by the message resource reader is stateful. An
archive-open success can provide it, while close and list-update paths can
replace or clear it. The present `WIND/10501` record in the installed
`RESOURCE.GFF` therefore does not, by itself, prove that every message
call acquires that record.

## Alternatives

Normal game states may keep `RESOURCE.GFF` open and reachable through the
archive list even while another archive is selected. FND-CONFIG-039
identifies the startup open, and FND-CONFIG-038 follows the reader's list
traversal; the later state at each message call remains unread, so this
finding does not establish a failed acquisition in any live state. The raw
filename hits could be reached indirectly or copied at runtime; absence
of recognized direct references is not evidence that they are unused.

## How to reproduce

Disassemble the approved `DSUN.EXE` at physical offsets
`0x0002E256..0x0002E4A5` (`38FF:0066`), `0x0002E4A5..0x0002E617`
(`38FF:02B5`), and `0x0002D134..0x0002D188` (`37ED:0064`). Follow
the assignments to `DS:9D9F`, their branch conditions and the record
links at offsets `0x08` and `0x0C`. Compare with the reader's pointer
check at `0x0002E9A1..0x0002E9F3`. Search raw loaded bytes for
null-terminated ASCII `RESOURCE.GFF` with ReportBytePattern.java and
inspect references to each hit before assigning any of them a caller.
