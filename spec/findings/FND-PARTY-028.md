---
id: FND-PARTY-028
title: Of the other reservations before the party-loader gate, the scroll routine releases all it takes, and two routines each keep one entry
status: superseded
builds: [BLD-GOG-EN-1.1]
superseded_by: [FND-PARTY-033]
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:53B4..1BF3:5813
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3E06:0002..3E06:021E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 41E1:000B..41E1:0214
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000; the call graph of FND-PARTY-026
environment: null
---

## Observation

FND-PARTY-027 lists the calls to the reservation routine `1BF3:27A8` (FND-PARTY-025) that the
routines reachable by direct calls before the gate make, other than the startup one. They are in
three routines. "Sub-entry" below is an entry made by `1BF3:282D`, which uses a table entry but no
pool space (flag `0x40`, FND-PARTY-025).

**`1BF3:53B4`** (file `0x000164E4`) moves a rectangle of a reservation by the two offsets it is
given, using the pool for temporary copies. It returns at once when both offsets are 0. Otherwise
it always makes two sub-entries (kept in the words at `1BF3:10CC` and `1BF3:10CE`), one
reservation (`1BF3:10DC`) with two sub-entries (`10D0`, `10D6`), and, when the word read from its
table at `[si+8]` is nonzero, two more reservations (`10DE`, `10E0`) with two sub-entries each
(`10D2`, `10D8`, `10D4`, `10DA`); otherwise it sets `10DE` to `0xFFFF`. Before its one return it
releases, when `10DE` is not `0xFFFF`, `10DA`, `10D4`, `10E0`, `10D8`, `10D2` and `10DE`, and
then always `10D6`, `10D0`, `10DC`, `10CE` and `10CC`. Every entry it makes is released before it
returns.

**`3E06:0002`** (file `0x00033262`) saves what is under the mouse pointer. It returns at once
when the word at `DS:332C` is nonzero. Otherwise it releases the entries in the words at
`DS:A167` and `DS:A169` when each is above 1, sets both to `0xFFFF`, and, on the path that saves
again, makes a sub-entry for the pointer's screen rectangle in `DS:A167` and a reservation of
(0, 0, width, height) in `DS:A169`. The width and height come from `1BF3:76C2` and `1BF3:771C` for
the image at `DS:A145`, each clipped so that the rectangle ends inside the bounds at `DS:A039`
and `DS:A03B`, and the routine stops when either is below 1. So it holds at most one reservation
after it returns.

**`41E1:000B`** (file `0x0003701B`) returns 0 at once unless the word at `DS:A179` and the far
pointers at `DS:A17F` and `DS:A183` are nonzero and its argument equals the one at `DS:A17F`.
Otherwise, after calling `41E1:0215` when the word at `DS:A18F` is not `0xFFFF`, it makes a
sub-entry kept at `DS:A18F` and a reservation of (0, 0, 1, rows − 1) kept at `DS:A191`, where
rows is the word at `DS:A189`: one byte per row, `(rows + 15) >> 4` paragraphs. It returns
`0xFFFF` when either fails. A further sub-entry of the whole screen, made and released in the
same call, leaves `DS:A352` as it was. The reservation at `DS:A191` is still held when it returns.

## Interpretation

On the paths of FND-PARTY-026, the reservations that can be held when the gate routine makes its
own two, other than the startup one, are at most one for the mouse pointer and at most one for
the routine at `41E1:000B`, which reads as an edit field's caret. Whether `41E1:0215` releases
the earlier caret entry before the new one is made was not read, nor what sets `DS:A189` or the
pointer image's size. A pointer save of at most 1,000 paragraphs and a caret save of at most 6
paragraphs, `DS:A189` up to 96 rows, leave room for the gate's 61.

## Alternatives

- The caret routine accumulates entries: not ruled out until `41E1:0215` is read.
- Reservations are made through one of the unresolved indirect calls of FND-PARTY-026: not ruled
  out.

## How to reproduce

Disassemble `DSUN.EXE` from file offset `0x000164E4` to `0x00016943` (segment `1BF3`, offsets
`0x53B4` to `0x5813`), from `0x00033262` to `0x0003347E` (`3E06:0002` to `3E06:021E`) and from
`0x0003701B` to `0x00037224` (`41E1:000B` to `41E1:0214`) as 16-bit code, and resolve the calls
to `1BF3:27A8`, `1BF3:282D` and `1BF3:28C5`.
