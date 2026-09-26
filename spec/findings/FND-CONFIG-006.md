---
id: FND-CONFIG-006
title: SOUND_DS.EXE does not hold .adv in either case
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0000..2D1E:0000
tool: Ghidra 12.1.3 ReportBytePattern, then a byte search of the whole file with PowerShell 5.1.26100.9444
environment: null
---

## Observation

A search of the loaded image of `SOUND_DS.EXE` in Ghidra, after a full analysis, and a byte search
of the whole 204,593-byte file, including the 80,209 bytes after the load image, find neither
`.adv` (`2E 61 64 76`) nor `.ADV` (`2E 41 44 56`).

## Interpretation

The setup program does not add the extension to the driver names it writes to `SOUND.CFG`
(FND-CONFIG-003); the names come whole from `SOUND.INI`, whose driver-name values include the
extension (FND-CONFIG-007).

## Alternatives

A name built from shorter pieces would not show in the search.

## How to reproduce

Search the whole file for both byte strings.
