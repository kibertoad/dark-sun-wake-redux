---
id: FND-INPUT-016
title: The input packet queue is a ring buffer with its size at 57E0:33C0, free bytes at 33C2, read and write offsets at 33C4 and 33C6 and buffer at 33C8; a packet that does not fit is dropped whole, and 2C5F:0182 empties the queue
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4464:014E..4464:01F3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4464:0230..4464:02DA
tool: Capstone 5.0.9 over resident code; tools/research/exec-census/resident_listing.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Addresses are static, load image at segment 1000, `DS` = `57E0`.

**`4464:014E(buffer, size)`** returns -1 with `DS:33BE` at `0x802` when `DS:33CC` is already 1, and
with `0x0D` when `size` is 0. A null `buffer` is replaced by an allocation of `size` bytes through
`1000:15A5`, with `DS:33CE` set to 1, or -1 and `DS:33BE` at 8 when that fails. It then stores
`size` in `DS:33C0` and `DS:33C2`, the buffer's far pointer in `DS:33C8`, 1 in `DS:33CC`, installs
the keyboard hook (`44B6:0018`) and registers the mouse handler (`44D0:0006`, FND-INPUT-005).

**`4464:0230(packet)`**, with interrupts off, returns -1 with `DS:33BE` at `0x0D` when `DS:33CC` is
not 1. It normalizes the packet's far pointer through `45CD:0000` and takes its length from the
packet's word `+2`. When `DS:33C2` is below the length (signed) it returns -1 with `DS:33BE` at
`0x801` and copies nothing. Otherwise it copies the packet into the buffer from offset `DS:33C6`,
up to `DS:33C0` when the read offset `DS:33C4` is not above the write offset, then the rest from
offset 0; it adds each part to `DS:33C6` and takes it from `DS:33C2`.

`2C5F:0182` calls `4464:0083`, which with interrupts off copies `DS:33C0` to `DS:33C2` and clears
`DS:33C4` and `DS:33C6` (FND-CONFIG-171).

## Interpretation

Key and mouse packets go into a ring buffer of `DS:33C0` bytes. When it is full a new packet is
lost. `2C5F:0182` drops every packet still waiting, so keys pressed while a step or menu action ran
are not acted on afterwards.

## Alternatives

The reader of the queue was not read here; the reading of `DS:33C4` as the read offset rests on
the writer's wrap test and the flush. Where `2C5F:0182` is called from (36 lines of
`direct_callers.py` output) was not sorted.

## How to reproduce

`resident_listing.py <dsun> 4464:014E..4464:02DA`; `direct_callers.py <dsun> 2C5F:0182`.
