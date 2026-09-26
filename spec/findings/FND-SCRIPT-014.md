---
id: FND-SCRIPT-014
title: A table of 200 linked 13-byte records at the far pointer 57E0:40C0 is set up at start and walked to run GPL scripts
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:40C0..57E0:40C4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:0024
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0158
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1695:010F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1695:0170
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1695:07DD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1695:08DB
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

Ghidra's `ReportReferences` finds 34 direct references to the far pointer at `57E0:40C0` (Ghidra's
`5B7C:0700`), all reads, in nine functions, and no direct write. The pointer's 16 bytes are 0 in
the file.

The routine `277B:0024`, whose only direct caller is the program entry at `1000:0158`, rejects a
first argument below 1. It then fills `0x0A28` bytes (200 times 13) at the pointer with `0xFF`,
writes, for each index i from 0 to 199, the word i + 1 at offset 11 of record i (the record at
the pointer plus `13 * i`), stores 0 in the word `4C13:032D`, and stores `0xFFFF` in five
adjacent 16-bit words. No record gets a link of `0xFFFF`: record 199 links to 200.

The four routines of segment `1695` that call `172C:000C` through a record (FND-SCRIPT-013) walk
a list from a 16-bit index, record by record through the link at offset 11, and stop at
`0xFFFF`. Each reads the words at offsets 0, 2, 4 and 6 and, where it needs them, the bytes at
8, 9 and 10, and on a match runs script word 2 from start word 0 with selector 1. Their tests:

- words 4 and 6 equal the words `4C0D:0005` and `4C0D:0003`, and byte 8 is a lower threshold
  that the word `4C0D:0001` is checked against;
- `4C0D:0005` and `4C0D:0003` lie in the inclusive ranges that bytes 8 and 9 give from words 4
  and 6, and byte 10 is the same threshold for `4C0D:0001`;
- word 4 equals `4C0D:0009`;
- words 4 and 6 equal `4C0D:0009` and `4C0D:0007` in either order.

The direction of the threshold tests was not recorded.

`1695:010F` walks a list and calls `1695:07DD` for each record whose byte at offset 6 is 0, and
`1695:0170` does the same for records whose byte 8 is 0. `1695:07DD` unlinks the record from the
list, links it in front of the list whose head is `4C13:032D`, and makes it that head. The
references to `4C13:032D` are the start-up write, and a read and a write in `1695:07DD`.
`1695:08DB` walks a list and reports success when `4C0D:0009` equals word 4 of a record; for the
list whose head is `4C10:0017` it also accepts word 6.

`ReportFunctionScalarIntersection` finds no function holding both `0x5B7C` and `0x0700`.

## Interpretation

The table is a pool of 200 script trigger records. At start every record is on the free list
whose head is `4C13:032D` (FND-SCRIPT-015), in index order, and the trigger lists are empty.
Records on a trigger list name a script entry point (word 0 the offset, word 2 the `GPL `
number) and a condition on game values in words 4 and 6 and bytes 8 to 10; the four walkers
test the condition and run the script. `1695:07DD` returns a record to the free list. The values
at `4C0D:0001` to `4C0D:0009` were not identified; tile coordinates and an object are plausible
for the tests above.

## Alternatives

Ghidra's decompilation of `277B:0024` reports unresolved far control flow, so the reading above
rests on the instructions around the fill and the link loop. What the pointer at `57E0:40C0`
points to, and who writes it, was not found; a write through a computed address would not show
as a direct reference. Whether record 200 can be reached depends on whether all 200 records can
be taken off the free list.

## How to reproduce

Run `ReportReferences` on `5B7C:0700` in a Ghidra project of `DSUN.EXE`, and
`ReportInstructionContext` on the fill and link loop in `277B:0024` and on `1695:010F`,
`1695:0170`, `1695:07DD` and `1695:08DB`.
