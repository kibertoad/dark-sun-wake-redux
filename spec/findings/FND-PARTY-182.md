---
id: FND-PARTY-182
title: Overlay 188 +066E places the object whose number is the negated word 0 of a record of the table at DS:19C1, with the record's byte 0x12 as the placed entry's low bits
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007350E..0x000735A7
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py (Capstone 5.0.9)
environment: null
---

## Observation

Overlay 188's code starts at file offset `0x72EA0`; `+x` is file offset `0x72EA0 + x`. Segment
word `0x00F0` resolves through the overlay's fixups to `31E0`, and `0x0388` to `4F49`.

`+066E` takes a far pointer `[bp + 6]` to an array of 10-byte entries and an index `di`
(`[bp + 0x0A]`). It reads the word at `+4` of entry `di` and takes it as the number of a 23-byte
record in the table at the far pointer `DS:19C1` (`imul ax, ax, 0x17`, `les bx, [19C1]`), twice
(`+0687..+0699`, `+06AD..+06BF`). It then calls `31E0:0121` (`+06C5`) with, first to last:

1. the record's word at `+0x00`, negated (`mov ax, es:[bx]`, `neg ax` at `+06BF..+06C2`);
2. -1;
3. its own arguments `[bp + 0x10]` and `[bp + 0x12]`;
4. 0;
5. the record's byte at `+0x12`;
6. `0x0F`, from its local byte at `[bp - 1]`.

It keeps the result in `si`. When that is -1 it returns 0. When it is not -1 and the
`object_slot_kinds` byte of slot `si` is 0, it calls `31E0:05BB` with the word at `+1` of
`object_slots` entry `si`, 1, 0 and 0, and returns 0. Otherwise it goes on at `+0707`
(`+06CD..+0704`).

FND-ACTOR-004 reads `31E0:0121`: it appends an object to the placed-object list through
`31E0:0EFF` with its first argument as the object number, its second as the slot, bits 0 to 2 of
one byte argument and the other as the flags. FND-ACTOR-003 reads `31E0:0EFF`: it requests the
`OJFF` resource of that object number, and with bit 2 of the flags set, as in `0x0F`, an `RDFF`
of it through `2D40:000A`.

`direct_callers.py <dsun> 188+066E` finds a near call at `+0616`, in `+04F8`, and the trampoline
`5702:00CA`; `+04F8` is called from `1835:0005` and `183B:0004` through `5702:0066`.

## Interpretation

The word at `+0x00` of a 23-byte record at `DS:19C1` is an object number, stored negated: placing
the record's object uses its negation as the number of the `OJFF` and `RDFF` resources to load.
The record's byte `+0x12` supplies the low three bits of the placed entry's byte `+5`.

## Alternatives

- What the array of 10-byte entries is, and so when the game places a record's object this way,
  was not read; nor the other callers of `31E0:0121` that read the table.
- This shows how one routine reads word 0; FND-PARTY-181's data agree with it for every shipped
  record.

## How to reproduce

From the commit that adds this finding, run
`tools/research/exec-census/overlay_listing.py <dsun> 188 72EA0 73700` on the installed
`DSUN.EXE` and read `+066E..+0704`, then
`tools/research/exec-census/direct_callers.py <dsun> 188+066E 188+04F8`.
