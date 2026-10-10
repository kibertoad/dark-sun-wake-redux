---
id: FND-PARTY-051
title: The CHAR load reads a resource as a chain of chunks, each a 10-byte header and the number of bytes its word at 8 gives, until a chunk whose first byte is 0xFF, and handles each chunk by its first byte
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:000A..2D40:03B9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000229B9..0x000229C1
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006FE30..0x0006FFBF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000701CA..0x0007042B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007042B..0x00070435
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070435..0x000704C9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1AA0:012A..1AA0:0209
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:06C4..2D40:06E1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0D59..2D40:0D94
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2C87..2D40:2E0A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0002540A..0x00025414
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00072EA0..0x00073098
tool: Python 3.14.7 with Capstone 5.0.7 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, resident_listing.py, trampoline_target.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. A handle is an index into the table of 3-byte entries at `4F49:0C33`, whose byte
`0x0C33` holds a kind and whose word `0x0C34` holds a record number.

**The walk.** After `2D40:000A` has the resource's bytes (FND-PARTY-049), it keeps a pointer to
the current chunk at `[bp-0x0C]` and one to that chunk's data, 0x0A bytes further on, at
`[bp-4]` (`2D40:00C9..+00D1`). It reads the chunk's byte 0, less 1, as an index from 0 to 3
into the four words at `2D40:03B9` (`2D40:00D8..+00E9`): `2D40:00EE`, `+012F`, `+018C` and
`+0200`. Any other byte 0 jumps to `2D40:02AF`, which clears `[bp-4]`. Each handler stores a
far pointer to `[bp-4]`, and a null one goes to `2D40:02C4`, which returns -2. Otherwise
`2D40:02BE` returns -2 once the count of handled chunks at `[bp-0x12]` reaches 0x50; below that,
`2D40:02E4` adds the chunk's word at `+0x08` to `[bp-4]`, and while the byte there is not 0xFF
it jumps back to `2D40:00C9` with that address as the next chunk (`2D40:02F1..+02F7`). At 0xFF
the walk ends. The header's byte 1 is read only by the handlers of bytes 2, 3 and 4.

Every handler ends by calling overlay 187 `+0605` (trampoline `56EF:0052`), which stores a
handle, that handle's kind and its record number as the next 6-byte entry of a list on the
stack at `[bp-0x1F8]` and raises the count (`+0605..+063F`), so entry *n* belongs to the *n*-th
chunk. The handlers of bytes 2, 3 and 4 then call overlay 187 `+0641` (trampoline `56EF:0057`)
with the header's byte 1, which returns the handle in that entry of the list (`+0641..+0697`).

**Byte 1: `2D40:00EE`** calls overlay 187 `+0000` (FND-PARTY-049), which copies the chunk's
data into the 49-byte record of the slot in the table at `DS:19C9` and makes the slot its own
handle, with kind 2 and record number the slot (overlay 187 `+045E`, `+04E6..+04F0`).

**Byte 2: `2D40:012F`** calls overlay 187 `+0040` (trampoline `56EF:0034`). That takes a free
handle through `+014F`, which calls `2D40:0D59` with the header's words at `+0x02` and `+0x04`;
`2D40:0D59` stores them as the handle's kind and record number. While `2D40:0D59` returns
9,999, `+014F` calls `+1B67` and tries again, up to eight attempts; a handle of 9,999 makes
`+0040` return a null pointer. Otherwise `+0040` calls `+039A` with the data, the header, the
handle and the slot. Back in `2D40:012F`, the code calls `1AA0:012A` with the handle that
`+0641` returned, the header's word at `+0x06` and the new handle (`2D40:0185..+01F5`).

**Byte 3: `2D40:018C`** calls overlay 187 `+007F` (trampoline `56EF:002A`). That uses the
handle in the word at `DS:60EB`, stores the header's byte 2 as its kind, and, when the header's
word at `+0x02` is 3, calls `+039A` with the data, the header, that handle and the slot
(`+0087..+00C9`); for another word it stores the header's word at `+0x04` as the handle's record
number. Back in `2D40:018C`, the code calls `1AA0:012A` with the handle that `+0641` returned,
the header's word at `+0x06` and the header's word at `+0x04` (`2D40:01E2..+01F5`).

**Byte 4: `2D40:0200`** calls overlay 187 `+00ED` (trampoline `56EF:002F`), which does what
`+007F` does without the test on the word at `+0x02` (`+00F2..+0124`). Back in `2D40:0200`, the
code takes the record number of the handle that `+0641` returned, keeps the word at `+0x04` of
that record in the table of 23-byte records at `DS:19C1`, stores the header's word at `+0x04`
there instead, and stores the kept word at `+0x04` of the record whose number is the header's
word at `+0x04` (`2D40:0256..+02A9`).

**Overlay 187 `+039A`**, after the call to `2D40:2C87` (FND-PARTY-049), returns a null pointer
when that returns -1. Otherwise it dispatches on the kind: kind 1 (`+04F8`) copies 0x17 bytes
of data to the record of the number `2D40:2C87` returned in the table at `DS:19C1` and sets
that record's words at `+0x04` and `+0x08` to 9,999 and at `+0x0C` to 0; kind 3 (`+054C`)
copies 0x42 bytes to the record of that number in the table at `DS:19C5` (FMT-COMBAT-002), and
for a number above 4 multiplies its word at `+0x08` by `100 + 50 * (word at DS:143A - 1)` and
divides by 100. Then `+05CE..+05EC` stores that number in the header's word at `+0x04`, in the
resource buffer, and as the handle's record number, and returns the data's address.

**`2D40:2C87`** for kind 3 (`2D40:2D1C`) returns a slot of 4 or less as it is; for a larger
slot it looks through records 5 to 16 of the table at `DS:19C5` for one whose word at `+0x0E`
equals the data's word at `+0x0E`, or else one where that word is 0. For kind 1 (`2D40:2D78`)
it takes a record number from `2D40:0E80` and sets that record's words at `+0x04` and `+0x08`
to 9,999.

**`1AA0:012A`** takes a handle, a field number and a 32-bit value. With a handle of 9,999,
below 0, or of kind 0, it stores -2 to `DS:40C4` and returns. Otherwise it finds the record
from the far pointer at `DS:614F` plus 4 times the kind, plus the word at `DS:6167` plus 2 times
the kind times the handle's record number, and adds what `2D40:06C4` returns for the kind and
field: the word at `DS:5A33` plus 0xC4 times the kind plus 2 times the field. An offset of -1
stores -1 to `DS:40C4`. It then reads the field's type, the byte at `DS:60ED` plus the field:
type 6 copies 16 bytes from the far pointer the value holds, a type of 0x80 or more copies that
type less 0x80 bytes from it, and any other type copies the byte at `DS:040C` plus the type
bytes of the value itself (`1AA0:01A1..+01E9`).

**Overlay 188 `+0000`** reads `FNFO` 1 into `DS:5AF7` (`DS:5A33` plus 0xC4) and `FNFO` 2 into
`DS:60ED` (`+0010..+00A6`, FND-CONFIG-150), stores 0x17, 0x31 and 0x42 to the words at
`DS:6169`, `DS:616B` and `DS:616D` (`+00B0..+00C2`), and copies the far pointers at `DS:19C1`,
`DS:19C5` and `DS:19C9`, once allocated, to `DS:6153`, `DS:615B` and `DS:6157` (`+01A2`,
`+01C9`, `+01F0`). So kind 1 finds its records in the table at `DS:19C1` with 23 bytes each,
kind 2 in `DS:19C9` with 49, and kind 3 in `DS:19C5` with 66.

## Interpretation

A `CHAR` resource is a chain of chunks, each a 10-byte header followed by the number of data
bytes the header's word at `+0x08` gives, and the chain ends at a chunk whose byte 0 is 0xFF.
Byte 0 of a header picks how the chunk is handled:

- 1: the character's 49-byte record (FMT-COMBAT-001), in the slot's own record.
- 2: a 23-byte record copied into a new record of the table at `DS:19C1`. Its handle is stored
  in the field that the header's word at `+0x06` numbers, of the object loaded by the earlier
  chunk whose index is the header's byte 1.
- 3: a 66-byte record copied into the table at `DS:19C5` (FMT-COMBAT-002), which for a slot of 4
  or less is the slot's own record. Its record number is stored in the field that the header's
  word at `+0x06` numbers, of the object of the chunk that byte 1 indexes.
- 4: a 23-byte record placed after the record of the chunk that byte 1 indexes, in a chain
  through the records' words at `+0x04`.

Which byte of the record a field number reaches is set by the `FNFO` 1 resource the game reads at
start-up.

## Alternatives

- FND-PARTY-004 reads a `CHAR` resource as a 79-byte header and 33-byte records. The code
  here reads no 79-byte header. In the stored records those 79 bytes are a 59-byte chunk at
  `0x00` and the first 20 bytes of a 76-byte chunk, and the sizes come out as `79 + 33 *
  byte1` because the two first chunks, the 10-byte end header and the 33-byte later chunks add
  up to that (FND-PARTY-052).
- The walk visits chunks in another order than the resource holds them: ruled out by the single
  pointer that moves forward by each chunk's size.
- Overlay 187 `+1B67` (also reached through trampoline `56EF:00BB`) and `2D40:0E80`, which make
  room for handles and records, were not read; they decide only whether a chunk finds space, not where its
  bytes go.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, run in `tools/research/exec-census/`: `resident_listing.py <dsun>
2D40:000A..2D40:03B9 2D40:06C4..2D40:06E1 2D40:0D59..2D40:0D94 2D40:2C87..2D40:2E0A
1AA0:012A..1AA0:0209`, `overlay_listing.py <dsun> 187 0x6FE30 0x6FFBF`, `187 0x701CA
0x704C9` and `188 0x72EA0 0x73098`, and `trampoline_target.py <dsun>` for `56EF:0025`,
`56EF:002A`, `56EF:002F`, `56EF:0034`, `56EF:0052` and `56EF:0057`. Read the four words at file
`0x229B9`, the five at `0x7042B` and the five at `0x2540A`.
