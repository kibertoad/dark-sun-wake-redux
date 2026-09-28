---
id: FND-CONFIG-067
title: Archive open returns a numeric handle distinct from its internal record pointer
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:0066
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39A9:0001
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:02B5
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded resident windows
environment: null
---

## Observation

The resident archive-open entry at `38FF:0066` stores the opened record's
far pointer in its internal list and active-pointer words `DS:9DA3` and
`DS:9D9F` (FND-CONFIG-037). Separately, it copies the word at `DS:9D8E`
into the record at offset `0x28`. If the caller supplied a nonzero output
location, it writes that word and a zero high word there, then increments
`DS:9D8E`. Overlay 180's startup passes `DS:1442` as that output location
(FND-CONFIG-039). Thus `DS:1442` receives a 32-bit numeric archive handle,
not the record's far pointer.

The helper at `39A9:0001` walks the linked records from `DS:9DA3`, compares
each record's 32-bit field at offset `0x28` with a supplied handle, and
returns the matching far pointer or zero. The close entry at `38FF:02B5`
uses this resolver for ordinary handles; only the all-ones argument takes
its close-all path (FND-CONFIG-041). The graphics initializer receives the
numeric value at `DS:1442` and stores it at `DS:9E70`; the shipped file has
one literal reference to `DS:9E70`, that store.

## Interpretation

The startup handle and the reader's active record pointer are separate
state. A call that passes `DS:1442` passes an archive number, and a close
of that number resolves the current record before unlinking it. The
absence of a later literal `DS:1442` use (FND-CONFIG-066) does not prove
that the record cannot be closed through a copied number or close-all.

## Alternatives

Reading `DS:1442` as a far pointer is ruled out by the open entry's output
write and the resolver's comparison. The reason for storing the handle at
`DS:9E70`, and any access through a computed address, remain unread.

## How to reproduce

Disassemble the approved `DSUN.EXE` at physical offsets
`0x0002E446..0x0002E4A5` (open success), `0x0002EC91..0x0002ECD5`
(handle resolver), and `0x0002E4C5..0x0002E50B` (close dispatch).
Compare overlay 180's output-address push at `0x00067606..0x00067622`
with the graphics handoff at `0x000676D9..0x000676E9` and the initializer
store at `0x0002EF4F..0x0002EF57`. Search the shipped bytes for the
address word `70 9E` to check the direct-reference bound.
