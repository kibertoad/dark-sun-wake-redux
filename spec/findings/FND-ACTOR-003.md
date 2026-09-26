---
id: FND-ACTOR-003
title: The routine at 31E0:0EFF requests an OJFF and 31E0:0E1B places it from an 8-byte entry
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0E1B..31E0:0EFF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0EFF..31E0:128C
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

The far routine at `31E0:0EFF` takes five word arguments: a signed object number, an entry index,
a slot number, a byte, and a word of flags. It returns at once when the byte at `DS:265B` is 0. A
slot number of -1 becomes 319. Then:

1. At `31E0:0F35` it calls the resource request `38FF:04AB` with the tag `OJFF` (`0x46464A4F`),
   the object number sign-extended to 32 bits, and a far pointer to a local buffer as the
   destination. When the request fails it calls `2D40:03C1` with the slot and leaves.
2. Unless the slot is 9,999, it compares the object number with seven values. For 430, 5,879,
   415 or 561, 541, 547 and 1,339 it stores 11,001, 11,002, 11,005, 11,007, 11,006 and 11,008 in
   the word at `0x19` of the slot's record and in the word at `0xC` of the local buffer.
3. When the slot is below 4 and the byte at `DS:068E` is not 0, it stores 11,001 at `0xC` of the
   buffer; when the slot is below 4 and the byte at `DS:6281` is not 0, it stores 13,009 there.
   It then copies the word at `0xC` of the buffer to `0xE`, except when the second condition
   holds, or when neither holds and the object number is 1,339.
4. At `31E0:0FFA` it calls `31E0:0E1B` with the buffer, the slot's record and the entry index.
   The slot's record is the 37-byte record at `DS:67BB + slot * 0x25`.
5. When bit 2 of the flags is set and the object number is below 9,000 or above 13,998, at
   `31E0:1028` it calls `2D40:000A` with the slot, the object number sign-extended to 32 bits,
   the tag `RDFF` (`0x46464452`) and the byte argument. A return value of -2 takes the failure
   path of step 1.

The rest of the routine, up to its return at `31E0:128B`, calls `2D40:0461` when the slot is 319,
and makes a near call to `31E0:2670` at `31E0:115B`.

The routine at `31E0:0E1B` takes the buffer, the record and the entry index. It returns at once
when the byte at `DS:265B` is 0. It takes the far pointer at `DS:67B7`, adds the entry index times
8, and calls the 8 bytes there the entry. It then writes these fields of the record:

| Record offset | Value |
|---|---|
| `0x0` | byte `0x0` of the buffer |
| `0x1` | the entry index |
| `0x3` | the entry's word at `0x0`, less the buffer's word at `0x2` |
| `0x5` | the entry's word at `0x2`, less the buffer's word at `0x4`, less the buffer's byte at `0xA` sign-extended |
| `0x7` | byte `0x2` of the buffer |
| `0x8` | the buffer's word at `0x4` |
| `0xA` | the entry's word at `0x0` |
| `0xC` | the entry's word at `0x2` |
| `0xE` | the entry's byte at `0x4` |
| `0xF` | byte `0xB` of the buffer |
| `0x12` | 0 |
| `0x15` | 0 |
| `0x19` | the buffer's word at `0xC` |
| `0x1D` | 0, as a 32-bit value |

It reads no other byte of the buffer. `31E0:0FFA` is the only call to `31E0:0E1B` in the resident
load image.

## Interpretation

`31E0:0EFF` loads an object's `OJFF` definition into a slot of a table of 37-byte records, and
`31E0:0E1B` computes where the object's image goes: the entry's position less the definition's
offsets at `0x2` and `0x4` and its vertical offset at `0xA`, which is the placement FND-IMAGE-010
observes. The word at `0xC` is the image number, replaced for seven object numbers by numbers from
11,001 to 11,008. The 8-byte entry has the layout of an `ETAB` record: a position at `0x0` and
`0x2` and the vertical offset byte at `0x4`. An object whose number lies outside 9,000 to 13,998
also has its `RDFF` resource of the same number requested.

## Alternatives

Nothing here shows that the table at `DS:67B7` is filled from a region's `ETAB`; that tag occurs
only in overlay code (FND-REGION-007). The code after `31E0:1040` was read only for its calls, so
what it does with the slot is not known. What `2D40:000A` does with the `RDFF` request and
the byte, and what the special image numbers show, are not known.

## How to reproduce

Disassemble `31E0:0E1B` to `31E0:128C` with the relocations applied, and search the resident image
for near and far calls to `31E0:0E1B`.
