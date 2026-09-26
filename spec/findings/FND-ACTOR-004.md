---
id: FND-ACTOR-004
title: Three resident call sites pass object numbers to 31E0:0EFF, and 31E0:426E returns an OJFF's image number
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0121..31E0:0173
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0589..2D40:06A2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31BA:0131..31BA:013C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:426E..31E0:42A5
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

A search of the resident load image `1000:0000..6237:0000` for far calls, and for near calls
within segment `31E0` preceded by `push cs`, finds three calls to `31E0:0EFF` (FND-ACTOR-003):

- `31E0:0170`, in the routine at `31E0:0121`. When the word at `DS:264E` is 1,500 or more, that
  routine calls `31E0:0D09` and, if the word is still 1,500 or more, returns -1. Otherwise it
  passes its own first argument as the object number, the word at `DS:264E` as the entry index,
  its second argument as the slot, bits 0 to 2 of its byte argument, and another byte argument as
  the flags.
- `2D40:069A`, in the routine at `2D40:0589`. That routine first calls `2D40:0D94` in a loop that
  runs while a counter it passes is below 320, and for each slot number it returns compares the
  distance, from `1B0B:000C`, between that slot's record and the record of the slot given as its
  first argument, each position taken as the record's words at `0xA` and `0xC` shifted right by 4.
  Then, when its second argument is negative, it walks the first `DS:264E` 8-byte entries of the
  table at `DS:67B7` and, for each entry whose word at `0x6` equals that argument and that lies
  nearer than any before, calls `31E0:0EFF` with the negated argument as the object number, the
  entry's index, slot -1, bits 0 to 2 of the entry's byte at `0x5`, and flags 4.
- `31BA:0137`, which passes two local words as its first two arguments.

The routine at `2D40:0589` has two far callers, `172C:0D50` and `172C:31A1`. The routine at
`31BA:000E` that holds `31BA:0137` has six far callers.

The routine at `31E0:426E` calls `38FF:04AB` with the tag `OJFF`, its word argument
zero-extended to 32 bits, and a local buffer. It returns -1 when the request fails and the
buffer's word at `0xC` otherwise. The search finds no near or far call to it.

## Interpretation

The table of 8-byte entries at `DS:67B7`, of which `DS:264E` counts the entries in use and which
holds at most 1,500, is the list of placed objects: `31E0:0121` appends an object to it, and
`2D40:0589` looks up the nearest entry with a given object number. The entries' word at `0x6` is
the object number, as in an `ETAB` record, and the game can store negative numbers there.
`31E0:426E` gives an object's image number.

## Alternatives

The search finds only direct calls, so a routine reached through a pointer, such as
`31E0:426E`, can still be used. The distance routine `1B0B:000C` was not read, and the roles of
the entry index passed by `31BA:0137` and of the routine's six callers are not known.

## How to reproduce

Search the resident image for the far call bytes `9A` with each target, relocations applied, and
for `0E E8` near calls within segment `31E0`. Disassemble the listed ranges.
