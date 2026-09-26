---
id: FND-CONFIG-001
title: The installed CHARSAVE.GFF holds one 9-byte PREF resource, number 100
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1AED..0x1AF6
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

Read through the directory of the installed `CHARSAVE.GFF` (FMT-GFF-001, FND-PARTY-005), the
archive holds one `PREF` resource, number 100, of 9 bytes at file offset `0x1AED`:
`00 00 FF 3F FF 01 01 00 01`. The disc's copy has no `PREF` resource.

## Interpretation

The resource holds the settings the game saves with every saved game (FND-SAVE-004). Read in the
order the save routine writes it, it is the word 0, then the bytes 255, 63, 255, 1, 1, 0 and 1.

## Alternatives

A single resource shows no range for any byte; which setting each byte is comes only from the
globals the save and load routines copy it from and to.

## How to reproduce

Read the directory of `CHARSAVE.GFF` and of the disc's copy, and list the `PREF` resources with
their offsets and bytes.
