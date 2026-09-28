---
id: FND-CONFIG-041
title: Direct archive-close calls target region and save handles or close all
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:02B5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56B2:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:0000
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded resident and FBOV windows; shipped-byte direct far-call inventory; ReportFbovOverlayMap.ps1
environment: null
---

## Observation

The resident close entry at `38FF:02B5` treats the double-word argument
`0xFFFFFFFF` specially: it repeatedly closes the currently selected
archive, by its record number, until the active pointer is zero. A
failed recursive close returns an error. For a different argument it
resolves that archive record, closes its file, unlinks the record and
updates the active pointer if it was the selected record (FND-CONFIG-037).

A shipped-byte search for direct far-call instructions to offset
`0x02B5` finds the following overlay call sites. Each overlay call's
`0x0128` fixup selects resident segment `38FF`:

| Caller overlay | Call file offset(s) | Passed argument |
|---:|---|---|
| 180 | `0x00067A09` | `0xFFFFFFFF` |
| 182 | `0x00068B18`, `0x00069DA9`, `0x00069E8B`, `0x0006A1C9` | Numeric handle at `DS:145A` |
| 186 | `0x0006FCBD` | Numeric handle at `DS:144A` |
| 187 | `0x00071A65` | Numeric handle at `DS:144E` |
| 187 | `0x00071BF0` | Numeric handle at `DS:145A` |
| 192 | `0x0007D394` | Local numeric handle at `[BP-4]` |

There is also a resident wrapper at `47B9:0310`
(`DSUN.EXE+0x0003D0A0`) that calls close with `0xFFFFFFFF` and handles
its error. None of these decoded direct call sites passes the startup
resource-archive handle at `DS:1442` (FND-CONFIG-039,
FND-CONFIG-067). The overlay 180
close-all call follows cleanup calls in a separate routine at
`DSUN.EXE+0x000679D9..0x00067A27`; this reading does not establish its
callers or when it runs.

## Interpretation

The direct close sites do not show an individual close of the startup
resource archive. The close-all path can remove it, while ordinary
selection of another still-open archive leaves it searchable under the
startup wraparound mode (FND-CONFIG-040).

## Alternatives

Indirect close calls, copied aliases of `DS:1442`, or code that changes
the resource record could still remove the archive. The direct-call
inventory does not prove its presence at every message call, and the
close-all routine's placement does not alone prove it runs only on exit.

## How to reproduce

Disassemble the approved `DSUN.EXE` at physical
`0x0002E4A5..0x0002E617` for the close entry and
`0x000679D9..0x00067A27` for the close-all caller. Search the resident
image for `9A B5 02 FF 28` and FBOV overlay code for
`9A B5 02 28 01`; use overlay fixup lists and
`ReportFbovOverlayMap.ps1` to assign each bounded hit, then read its
immediately preceding argument pushes. Compare the Save/Load local
pointer call with FND-SAVE-007 and the `DS:144A` call with FND-SAVE-009.
