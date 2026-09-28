---
id: FND-CONFIG-040
title: Startup selects wraparound traversal of the open resource archives
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:000E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 37ED:00B8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39A9:0157
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56B2:0000
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded resident and FBOV windows; shipped-byte search of direct far-call operands
environment: null
---

## Observation

The resident archive initializer at `38FF:000E` clears the archive-list
head and active pointer, and sets the word at `DS:9D9B` to 1 when its
`GFFI` signature has not yet been initialized. The setter at
`37ED:00B8` accepts values 0, 1 or 2 for that word after checking the
signature; other values return an error. Overlay 180's startup path calls
the initializer, then immediately calls the setter with 2 at
`DSUN.EXE+0x00067489..0x00067495`, before opening `GPLDATA.GFF` and
the chosen resource archive (FND-SCRIPT-003, FND-CONFIG-039).

The archive-open success path inserts a new record at the head and links
its `+0x0C` pointer to the previous head (FND-CONFIG-037). The traversal
helper at `39A9:0157`, called by the resource reader on a missing type or
number (FND-CONFIG-038), handles the three mode values:

| `DS:9D9B` | Next archive after a miss |
|---:|---|
| 0 | None. |
| 1 | The current record's `+0x0C` pointer, including zero at the end. |
| 2 | The `+0x0C` pointer if nonzero and not the active record; otherwise the list head if the current record has no successor and the head is not active. It returns zero before revisiting the active record. |

In the mode 2 startup path, a lookup can therefore walk every still-open
record once, beginning at the selected archive, wrapping to the head
when needed. A raw search for direct far calls with offset `0x00B8`
finds the startup call at `0x00067490`; this is not an inventory of
indirect calls or later writes through an alias.

## Interpretation

Selecting another archive after startup does not by itself make the
startup resource archive unreachable to the reader while mode 2 holds
and that archive remains in the linked list. Successful resource
acquisition still depends on the archive being open, the requested
entry and the reader's later allocation and I/O branches.

## Alternatives

A later indirect call or state write could change the traversal mode,
and a close could remove the resource archive. This finding does not
prove the mode or archive list contents at every message call, nor
that the resource request succeeds in a live state.

## How to reproduce

Disassemble the approved `DSUN.EXE` at physical offsets
`0x0002E1F0..0x0002E256` (archive initializer),
`0x0002D188..0x0002D1C2` (mode setter),
`0x0002EDE7..0x0002EE4C` (traversal),
`0x0002E446..0x0002E4A5` (list insertion), and
`0x00067489..0x00067496` (startup call). Resolve the startup call's
`0x0110` FBOV fixup to the resident segment of `37ED:00B8`.
