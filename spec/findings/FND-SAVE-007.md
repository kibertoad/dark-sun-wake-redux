---
id: FND-SAVE-007
title: Save-list rows read STXT 1 from each available save file
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
    address: 38FF:0066
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV overlay 192 and resident archive entries; FBOV descriptor inspection
environment: null
---

## Observation

The shared Save/Load window routine allocates ten records of 125 bytes each
and asks the list builder at `DSUN.EXE+0x0007D3F2` to fill them (FND-UI-036).
For a listed slot, the helper at `0x0007D32A` passes the beginning of its
record to resident `38FF:0066` with another argument of 2. That resident
entry opens the passed path through a file routine, reads and checks an
initial header, and returns `0xFFFF` on failure.

On a successful open, the helper passes a pointer to record offset `0x50`
(80), resource number 1 and the four-byte tag `STXT` to resident
`38FF:04AB`, then closes the opened archive through `38FF:02B5`. The row
population routine at `0x0007E058` passes the same record offset `0x50` to
the row control. FND-SAVE-004 independently identifies the beginning of
the 125-byte record as the path of a `SAVEnn.SAV` file and the field from
offset 80 as its player description.

If the open or resource request yields `0xFFFF`, the helper clears the
record's first byte. In Load mode it also clears the first byte at offset
`0x50`; in Save mode it copies an empty source string there. The callback
uses the record's first byte as one guard while moving among rows in Load
mode (FND-UI-037).

## Interpretation

An available saved-game file is read as a resource archive, and its
`STXT/1` resource supplies the saved-game description to the list record.
The path marker distinguishes a row with a usable saved-game record from
one whose archive or description request failed. The visible Save/Load
list has ten row controls, but the number of filled rows can be smaller.

## Alternatives

This reading does not establish the full `SAVEnn.SAV` archive contents,
the byte length or encoding of `STXT/1`, or whether a missing description
alone makes an otherwise present game unusable. The header checks in
`38FF:0066` and the resource-read error convention need a complete reading
before specifying the file format. The exact row text painting and the
`<available>` wording from the manual remain open.

## How to reproduce

Map overlay 192 and disassemble `0x0007D32A..0x0007D3F1`, the bounded
list-builder call at `0x0007D65B..0x0007D670`, and row population at
`0x0007E058..0x0007E0F3`. Resolve the `0128` FBOV segment encoding
through descriptor 37 to resident `28FF`, then inspect the file-open entry
at `38FF:0066` and the resource request and close entries at `38FF:04AB`
and `38FF:02B5`. Compare record offset `0x50` with FND-SAVE-004.
