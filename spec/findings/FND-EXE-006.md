---
id: FND-EXE-006
title: Several four-letter GFF tags occur in DSUN.EXE only inside overlay code
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..6237:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5664:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56DD:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56E9:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56EF:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5702:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 576C:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5773:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5787:0000
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

A search of the whole file for the four ASCII bytes of each tag counts these occurrences in the
resident load image (`1000:0000..6237:0000`, file `0x5200..0x57570`) and in the FBOV pack (file
`0x57570..0x9AE30`); the MZ header has none:

| Tag | Resident image | Pack | Overlay code that holds each pack occurrence |
|---|---|---|---|
| `MONR` | 0 | 2 | overlay 204, `DSUN.EXE+0x0008D330`, `DSUN.EXE+0x0008D359` |
| `ETAB` | 0 | 3 | overlay 187, `DSUN.EXE+0x00070635`, `DSUN.EXE+0x0007064B`; overlay 200, `DSUN.EXE+0x00089284` |
| `GPLI` | 0 | 4 | overlay 187, `DSUN.EXE+0x0007145F`, `DSUN.EXE+0x0007148C`, `DSUN.EXE+0x00071673`, `DSUN.EXE+0x000716A0` |
| `PORT` | 0 | 1 | overlay 199, `DSUN.EXE+0x00088E2B` |
| `PSIN` | 0 | 3 | overlay 186, `DSUN.EXE+0x0006F949`, `DSUN.EXE+0x0006F95F`, `DSUN.EXE+0x0006FA0A` |
| `TEXT` | 2 | 3 | overlay 186, `DSUN.EXE+0x0006FD40`; overlay 188, `DSUN.EXE+0x00073103`, `DSUN.EXE+0x00073173` |
| `CHAR` | 14 | 4 | overlay 171, `DSUN.EXE+0x00058503`, `DSUN.EXE+0x00058D01`; overlay 182, `DSUN.EXE+0x00068EE9`; overlay 184, `DSUN.EXE+0x0006E5B9` |

Every pack occurrence lies inside the code block of an overlay (FND-EXE-003), numbered by its
segment-table descriptor. Overlay code has no `segment:offset` address, so this finding is located
at the resident header of each overlay named in the table: 171 at `5664`, 182 at `56BD`, 184 at
`56DD`, 186 at `56E9`, 187 at `56EF`, 188 at `5702`, 199 at `576C`, 200 at `5773` and 204 at
`5787`.

## Interpretation

A search of the load image alone, which is all a disassembler shows of the file unless the
overlays are mapped in, misses these tags. A negative result from such a search says nothing about
the overlay code.

## Alternatives

A four-byte match inside code need not be a tag the code uses: it can be part of an instruction or
of other data. Nothing here shows which, if any, of these bytes a routine passes to a resource
lookup.

## How to reproduce

Search the whole of `DSUN.EXE` for each tag's four bytes and split the matches at the image end,
`0x57570`. Find the overlay whose block holds each pack match from the `CodeFileOffset` and
`CodeBytes` that `tools/ghidra/ReportFbovOverlayMap.ps1 -SourcePath <DSUN.EXE>` prints.
