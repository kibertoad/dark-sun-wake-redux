---
id: FND-UI-007
title: The routine at 3F96:02F8 sets, clears or zeroes the event mask of an APFM record
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:02F8..3F96:0371
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

The far routine at `3F96:02F8` takes a far pointer, a resource number, a word of bits and an
operation code. At `3F96:0326` it calls the tag-aware resource lookup `39D1:049C` with the tag
`APFM` (`0x4D465041`) and the number, and returns -1 when the lookup gives a null pointer. Then,
on the record found:

| Operation | Effect on the word at `0x58` |
|---|---|
| 1 | ORs in the bits |
| 2 | ANDs with the complement of the bits |
| 3 | sets it to 0 |

Any other operation changes nothing. The routine returns 0 when the record was found.

## Interpretation

The game changes a frame's event mask while it runs, so which events a frame takes depends on
state as well as on the file (RULE-UI-001).

## Alternatives

Who calls this routine, and with which frames and bits, was not recorded.

## How to reproduce

Read `3F96:02F8` to its return at `3F96:0370`, following the pushed tag `0x4D465041` into the call
at `3F96:0326`.
