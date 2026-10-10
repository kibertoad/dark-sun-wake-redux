---
id: FND-PARTY-112
title: For the spells DATA 104 and 225, the three overlay 197 calls before the hit routine's saving throw end the hit only for a target with certain effect flags or effects, a failed resistance test, or a creature type, record byte or details byte that the tables or tests name
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065137..0x000651AF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008428F..0x00084502
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00084502..0x0008451A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008451A..0x00084AB5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00084B60..0x00084E94
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00085052..0x000851C6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004F588..0x0004F5BA
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, trampoline_target.py, data_bytes.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. In the hit routine overlay 179 `+0D2F`, with target `t`, caster `c` and `DATA` number
`i` (FND-PARTY-108, FND-PARTY-109), `+0DF5..+0E0E` has overlay 193 `+1BA3` fill a 16-byte block
for `c` at `bp-0x30` and one for `t` at `bp-0x20`; below, "byte `k` of the block" is a byte of
`t`'s block unless `c`'s is named. Then (`+0E57..+0ECC`):

- it calls overlay 197 `+0E8F` (trampoline `575A:0093`) with `t`, `c`, `t`'s block, `i` and two
  further arguments, and ends when the result is not 0;
- it calls overlay 197 `+1760` (`575A:0098`) with `t`, `c`, both blocks, `i`, `m` (below)
  and further arguments, and ends when the result is 0;
- it calls overlay 197 `+111A` (`575A:009D`) with `t`, `t`'s block, `i`, its fourth argument `m`
  and the address of its local dword at `bp-6`, and ends when the result is not 0.

From overlay 193 `+003C`, `m` is the word at `+0x1A` of `DATA i` (FND-PARTY-107's call at
`+04DE`). `data_bytes.py` gives, for both `DATA` 104 and `DATA` 225, the word at `+0x1A` as 0x100,
the word at `+0x11` as 0x4000, the byte at `+0x17` as -1, and the word at `+1` as -2; their
levels by overlay 177 `+09C5` are 9 and 7 (FND-PARTY-110). Below, "the type" is byte `+0x0F` of
`t`'s FMT-COMBAT-001 record and "the type word" the word at `DS:2588 + 2 * type`.

**Overlay 197 `+0E8F`** returns 1 when `2D40:3E64` does not find `t`; when the type word is
0xFFFF; when overlay 208 `+0F07` of `t`, `c` and `i` returns 0; when bit 0x10 of byte 0 of the
block is set and `i` is below 235; when bit 0x01 of byte 7 is set and `t` is not `c`, for every `i`
other than 26, 150, 72 and 236; when bit 0x10 of byte 0x0B is set and `i` is below 235, the
value of overlay 197 `+3693` of `t` and `i`; when its fifth argument, 0 from overlay 193 `+003C`, is not 0
and bit 0x80 of byte 5 is set or overlay 197 `+314E` of `t` returns nonzero; when bit 0x08 of byte 1 is set and the word at
`4C4F:00DC + 4 * index` is -1, `index` being one of `2D40:3E64`'s results, and overlay 208 `+0885`
of `i` and 1 is not above 8; when `i` is 115 or below and the word at `+6` of `t`'s record is
0x21C; and when bit 0x80 of byte 6 is set and the spell's level is below the byte at
`4E71:0256 + 10 * e`, `e` being `3143:00A8` of `t` and 0x37. A test on bits 5 to 7 of byte
`+0x1F` of `DATA i`, which are 0 for both spells, does not apply, and both numbers are missing
from the six words at `cs:1102`, so it returns 0 otherwise (`+0E8F..+1101`).

**Overlay 197 `+1760`** returns 1 unless one of these returns 0 (`+1760..+1A93`):

- when overlay 208 `+0885` of `i` and 1 is not above 8: bit 0x40 of byte 5 is set and
  `28C9:391D` of 1 and 100 returns 75 or less; or `c` is below 48, bit 0x10 of byte 0x0A is set
  and bit 0x80 of the word at `+0x11` of `DATA i` is set, which is clear for both spells; or `c`
  is below 48 and not `t`, bit 0x20 of byte 0x0A is set and the spell's level is 1 to 3;
- bit 0x80 of byte 9 is set, bit 0x100 of `m` is set, byte `+0x12` of `c`'s FMT-COMBAT-002
  record is 9 and overlay 179 `+2A46` of `t`, `c`, 38 and 6 returns nonzero.

**Overlay 197 `+111A`** returns 1 when the type word is 0xFFFF; when overlay 197 `+1C52` of `t`,
the block and `m` returns 0, the value it stores in the dword; when `i` is below 235 and
`3143:00A8` of `t` and 0x5C is not -1; when overlay 179 `+2BD7` of `t`, `i` and its last argument
returns nonzero; when `3143:00A8` of `t` and 0x37 is not -1 and the spell's level is below the
byte at `4E71:0256 + 10 * e`; when byte `+0x12` of `t`'s FMT-COMBAT-002 record is 9, for `m` of
0x100; when byte `+0x0E` of `t`'s record is 9 and `i` is below 115, or 115 or more and overlay
211 `+1FB5` of the slot at `DS:426D` and `i` is below 7; and when byte `+0x0E` is not 0 and
overlay 198 `+0149` of that byte and 0, 1 or 4 equals `i`. Its other tests depend on bits of the
word at `+0x11` other than 0x4000, on byte `+0x19` being 69 or 91, on `+0x17` not being -1, on `m` bits other than 0x100, on a
byte `+0x0E` of 0x0D or 0x0A, or on `i` of 252 or 288, and do not end the hit for these spells
(`+111A..+16B4`).

**Overlay 197 `+1C52`** starts from -1. When `2D40:3E64` does not find `t` it returns 100. For a
type below 18 it returns 0 when the type word shares a bit with `m` (for `m` of 0x100); otherwise
100, halved when the type is below 7 and the word at `DS:25AC + 2 * type` shares a bit with `m`,
which none of the seven words does for 0x100.
The resistance bits it forms from bytes 4 and 9 of the block are 2, 4, 6 and 0x80, which do not
include 0x100, and the tests for `m` of 0x400 or with no bit of 0xFD79 do not apply
(`+1C52..+1DC5`). The 18 type words are 0, 1, 0x7FF, 0xD3F, 0xA, 0x3A, 0, 0x93D, 0x38, 0x1038,
0x3038, 0xD39, 0x303A, 0x9BD, 0x4000, 2, 0xFFFF and 0x400, and the seven words at `DS:25AC` are 0,
0xA, 0x14, 0x80, 0x30, 0 and 0x86.

## Interpretation

For the spells `DATA` 104 and 225, the three calls let the hit go on to the saving throw unless
the target has one of the effect flags or effects named above, fails the resistance test of
overlay 179 `+2BD7`, has the type 2, 3, 7, 11, 13 or 16, has the record byte `+0x0E` or the
details byte `+0x12` the tests name, or wins the 75 in 100 chance of byte 5's bit 0x40. A party
member with none of these takes the hit to its saving throw.

## Alternatives

- What the bits of the block and the effects 0x37 and 0x5C stand for, what overlay 179 `+2BD7`,
  overlay 197 `+3693`, `+314E` and overlay 208 `+0F07` test, and which values party members have
  in the record bytes `+0x0E` and `+0x0F`, were not read; a party member with the type 2, 3, 7,
  11 or 13 would never be drained by these spells.
- What overlay 208 `+0885` returns for these spells was not read; it only gates branches that
  need flags as well.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 179 6503F 65220`,
`197 8428F 8451A`, `197 8451A 84B60`, `197 84B60 85052` and `197 85052 85200`;
`trampoline_target.py <dsun> 575A:0093`, `575A:0098` and `575A:009D`; read the six words at file
`0x00084502`, the 18 words at file `0x0004F588` and the seven at `0x0004F5AC`; and
`data_bytes.py <install>/RESOURCE.GFF 1`, `2`, `11`, `12`, `17`, `1A` and `1B`.
