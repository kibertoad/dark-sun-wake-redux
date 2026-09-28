---
id: FND-CONFIG-064
title: Literal archive traversal-mode references are two writes and two reads
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 37ED:00B8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:000E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39A9:0157
tool: Python 3.14.7 bounded raw-word search of the shipped executable; Capstone 5.0.7 16-bit disassembly of resident windows
environment: null
---

## Observation

The archive traversal mode is the word at `DS:9D9B` (FND-CONFIG-040).
A raw search of the complete shipped `DSUN.EXE` for its little-endian
address word finds four occurrences, all in the resident MZ image:

| Address-word file offset | Containing instruction and effect |
|---|---|
| `0x0002D1B0` | The setter's instruction at `0x0002D1AE` writes its argument to `DS:9D9B` only after the archive signature and value-range checks. |
| `0x0002E246` | The archive initializer's instruction at `0x0002E244` writes 1 to `DS:9D9B`. |
| `0x0002EDFE` | The traversal helper's instruction at `0x0002EDFC` compares `DS:9D9B` with zero. |
| `0x0002EE05` | The next traversal instruction at `0x0002EE03` compares `DS:9D9B` with one. |

FND-CONFIG-040 identifies the startup call that sets mode 2 through
the guarded setter before opening the resource archives. This raw
inventory finds no other literal-address write to the mode word in
the shipped executable, including its declared FBOV code bytes.

## Interpretation

The two direct writers and two direct readers of the traversal mode
are now bounded. If no later indirect call to the setter or initializer
and no indirect or block write changes this word, the startup mode 2
continues to let resource lookup traverse the still-open archive list
(FND-CONFIG-040).

## Alternatives

The raw address-word search does not detect a write through a computed
pointer, an alias or a copied block. It also does not prove that the
setter or initializer cannot be called indirectly after startup, nor
that a resource archive remains open or an I/O request succeeds.

## How to reproduce

Search the approved `DSUN.EXE` for the two-byte pattern `9B 9D` and
classify each of the four hits by bounded disassembly at
`0x0002D188..0x0002D1C2`, `0x0002E22F..0x0002E256`, and
`0x0002EDE7..0x0002EE17`. Compare the startup setter call and the
mode-dependent traversal with FND-CONFIG-040.
