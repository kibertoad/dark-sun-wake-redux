---
id: FND-EXE-550
title: Game startup can replace its upper segment before priority dispatch while shipped pair words begin zero
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:00D2..1000:014C
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:00D2..1000:014C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D090..0x0004D0AA
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x0004D000..0x0004D01A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00050908..0x0005090A
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x000507EC..0x000507EE
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-509 records startup's earlier stores of incoming ES at
DS:0090, an incoming DS word at DS:00A8 and a selected
sum at DS:00A0/00A4. After its zero fill, startup tests
current DS word 37C2 installed or 3736 on disc unsigned
against 0014. At most that value skips the following native-request chain.
Larger values additionally require DS byte 0092 at least three and,
when exactly three, DS byte 0093 at least 001E.

The admitted branch requests interrupt 21 with AX 5801 and BX
two. Carry set transfers through 011D to resident 02AD. Carry
clear requests AH 67 with BX freshly loaded from the edition's tested
word. Its carry set takes the same transfer. Otherwise it requests AH
48 with BX one. Carry set again transfers; carry clear increments
returned AX at word width and stores that incremented value at DS:00A8.
This write precedes the remaining calls and is not locally rolled back.

It decrements AX again, assigns ES from AX and requests AH 49.
Carry set transfers to 02AD with the upper-word store already made.
Otherwise it requests AX 5801 with BX zero, continuing only on
carry clear; carry set takes that failure transfer. Thus the local order
does not imply earlier native effects are reversed when a later request fails.
Native outcomes, register/segment preservation and the meaning of the returned
storage remain unestablished here. Every store uses the current DS at its
point of execution, not a proved stable segment across interrupts.

All skip or continuing paths next request interrupt 1A with AH zero.
They store returned DX at DS:0096 and CX at DS:0098.
Returned AL zero skips another write; nonzero sets ES to 0040,
BX to 0070 and stores byte one through ES:BX. No original
clock or hardware behavior is inferred from the request and conditional store.
It then clears BP, loads ES from saved CS:02C4, selects the
edition's table bounds and calls near 0220 as recorded by FND-EXE-510.
The dispatcher therefore follows this upper-word writer and native chain.

In the shipped images, the following words are zero in both editions:

| Relative data word | Installed shipped offset | Disc shipped offset |
| --- | --- | --- |
| 0090 | 4D090 | 4D000 |
| 0092 | 4D092 | 4D002 |
| 009E | 4D09E | 4D00E |
| 00A0 | 4D0A0 | 4D010 |
| 00A2 | 4D0A2 | 4D012 |
| 00A4 | 4D0A4 | 4D014 |
| 00A6 | 4D0A6 | 4D016 |
| 00A8 | 4D0A8 | 4D018 |
| 3908 installed / 387C disc | 50908 | 507EC |

These offsets use the shipped data bases 4D000 installed and 4CF70
on disc. With the modeled startup relocation from FND-EXE-509, these
correspond to data segments 57E0/57D7. The native preservation needed
to admit actual accesses to that data is not proved by the static mapping.
The zero-fill interval recorded there starts at 39C4/3938, above
all these words; that fill is not their zero producer. Shipped zero values
are not a census of later direct, indirect, aliased or native writers.

FND-EXE-548 reads the lower/current/upper pair words, and
FND-EXE-549 reads and updates the cache/current/upper words. This
startup chain supplies a concrete earlier upper-word writer, while leaving
actual segment equality and complete writer coverage open. Both editions share
local control flow with the listed table-word differences.

## Interpretation

This advances the shared-pair producer obligation before first-priority startup,
and separates shipped initialization from conditional native writes. Q-EXE-007
retains native request and preservation contracts, complete shared-word writer
coverage, actual segments, aliases and storage admission, the 02AD target,
other callers and remaining startup dependencies. No complete initialization
or launch exclusion is claimed.

## Alternatives

Treating the shipped zeros as runtime constants ignores earlier and later
writers. Treating zero fill as their producer contradicts its interval.
Assuming upper-word replacement always runs ignores its word and byte gates.
Treating failure as rollback ignores the store before the last two requests.
Treating the static segment mapping as native preservation ignores interrupt
boundaries and current-segment accesses.

## How to reproduce

At revision bab1bd98 require both DSUN.EXE identities from FND-EXE-350.
With header size 5200 and modeled load segment 1000, decode shipped
52D2..534C in sixteen-bit mode. Follow every unsigned word/byte gate,
native carry branch, upper-word write, clock-result stores and table dispatch.
Read the little-endian words at the exact shipped offsets in the table.
Use FND-EXE-509/510 for the preceding stores, zero-fill boundaries and
dispatch; use FND-EXE-548/549 for pair/cache consumers. Keep native
and writer admission explicit. Licensed bytes remain outside Git; no original
process, DOSBox or emulated call runs.
