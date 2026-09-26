---
id: FND-UI-011
title: The child dispatcher at 3D72:0EB8 reads a window's count at 0xF3, flags at 0x9E and offset at 0x96
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0EB8..3D72:1157
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4237:0BF8..4237:0C42
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4400:0062..4400:007F
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

The far routine at `3D72:0EB8` takes a point and two result pointers. It visits as many windows
as the word at `DS:A103` says, starting from the far pointer at `DS:A105` and following the far
pointer at `0xEE` of each window record. It stops after a window whose word at `0x9E` has bit
`0x100` set, and it skips a window whose word at `0x9E` has bit 4 set.

For a window it passes the point and the address `0xC` of the window record to `4237:0BF8`, and
the point and `DS:9F06` to the same routine. `4237:0BF8` returns 1 when the point lies in one of a
list of 8-byte rectangles, the list's count being the word at the address passed. When neither
call finds the point, the window is skipped.

It then visits child `i` for `i` from 0 while `i` is below the word at `0xF3` of the window,
taking the child's record at `0x105 + i * 30` of the window record plus the byte at `0xF2`. It
skips a child whose 32-bit value at `0x0` is 0, whose word at `0x1C` has bit `0x8000` set, or
whose tag at `0x4` is `ACCL` (`0x4C434341`), and applies the `BUTN` tests of FND-UI-006. When
the point lies in the region at `DS:9F06` and the child's tag is `MENU` (`0x554E454D`), it calls
`3BA6:08BF` with the far pointer at `0x0`. For other children in a window whose region holds the
point it computes the child's rectangle from the record the pointer at `0x0`
points at, by a three-way branch on its tag at `3D72:1157`, and passes that rectangle with the
window's words at `0x96` and `0x98` to `4400:0062`, which adds the first word to the rectangle's
two x values and the second to its two y values. A child whose moved rectangle holds the point,
and that passes the `EBOX` test of FND-UI-006 and two further region tests, is stored through the
result pointers; the search goes on, and ends when the child stored is an `APFM`.

## Interpretation

Once the game has loaded a window, the record in memory holds more than the file's bytes: a
region list at `0xC`, flags at `0x9E`, a screen offset at `0x96` and `0x98`, the list link at
`0xEE`, and in each child a far pointer to the resolved control at `0x0` and flags at `0x1C`. In
the file these places hold zeros or copied bytes (FND-UI-002, FND-UI-003), apart from byte `0x9F`,
the high byte of the flags, which differs from the copied edit box in two windows and is 1 in
three others. The child count is read as a 16-bit word at `0xF3`. A window's screen position is
the pair at `0x96` and `0x98`, and a child's hit rectangle is its own rectangle moved by it.

## Alternatives

Whether the game overwrites the file's bytes at `0xC`, `0x96` and `0x98` when it loads a window,
or reads the copied values, is not shown here; the Look panel's origin of (67, 44) (FND-UI-018)
does not match the file's 0 and 0 at `0x96` and `0x98` of `WIND/3020`, so something sets them.
The windows at `DS:A105` and the region at `DS:9F06` were not traced to their writers. The tags
`ACCL` and `MENU` do not occur in any shipped window's children.

## How to reproduce

Disassemble `3D72:0EB8` to its return at `3D72:1156`, `4237:0BF8` to `4237:0C41` and `4400:0062`
to `4400:007E` with the relocations applied, and note each displacement read from the window and
child records.
