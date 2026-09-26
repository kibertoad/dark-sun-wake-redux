---
id: FND-UI-008
title: The generic window lookup, registration, redraw and activation code does not read 0x3A of a WIND record
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:04BC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:02B3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0003
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:060D
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

In the tag-aware resource lookup, the branch for the tag `WIND` at `39D1:04BC` searches a linked
list of registered windows, comparing the number at `0x8` of each window record with the one
asked for and following the far pointer at `0xEE` to the next.

The registration routine `3A8E:02B3` resolves each child record of a window by its tag and number.
The redraw routine `3A8E:0003` restores and clips the rectangles of registered windows. The
activation routine `3A8E:060D` calls an optional handler and then dispatches the window's
resolved children. A reading of all three found no read of offset `0x3A` of the window record.

## Interpretation

The generic window code keeps windows in a list linked through `0xEE`, finds them by the number at
`0x8`, and resolves children by tag and number (FMT-UI-001, FMT-UI-002). It does not draw an image
named at `0x3A`, so that value, which is copied from an edit box (FND-UI-003), is not a
background the window code draws.

## Alternatives

Code specific to one screen could still read `0x3A`; no such reader is known. The value at `0xC2`,
which names an image of the window's size (FND-UI-002), was not looked for in this reading.

## How to reproduce

Follow the `WIND` tag comparison in the lookup to `39D1:04BC`, then read `3A8E:02B3`, `3A8E:0003`
and `3A8E:060D` for accesses to the window record, listing each offset read.
