---
id: FND-PARTY-052
title: Every stored CHAR record is a 49-byte type-1 chunk, a 66-byte type-3 chunk, 0 to 27 chunks of 23 bytes of types 2 and 4, and a 10-byte header with byte 0 at 0xFF, and FNFO 1 maps the fields their headers name to combatant words 4, 8, 10 and 12
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1C..0x44F
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x517..0xCA4
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0xD6E..0xE83
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0xEB7..0xFED
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1052..0x1125
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1128..0x11FB
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1395..0x1468
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1496..0x1527
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x16AB..0x17E1
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1964..0x1A9A
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1B48..0x1EF1
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x21B3..0x28C3
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x1FF48..0x2037E
tool: a Python 3.14.7 reading of CHARSAVE.GFF and OBJEX.GFF through their directories (FMT-GFF-001, FMT-GFF-002)
environment: null
---

## Observation

Each of the 19 `CHAR` resources of the installed `CHARSAVE.GFF` (29 to 43 and 50 to 53) was
read as FND-PARTY-051 says the load reads one: a 10-byte header, then as many data bytes as
the header's word at `0x08`, repeated until a header whose byte 0 is 0xFF. In every resource:

- The walk ends on a byte 0xFF exactly 10 bytes before the end of the resource, so no byte is
  left over.
- The first chunk, at `0x00`, has byte 0 = 1, byte 2 = 2, byte 3 = 0 and a length of 49; its data
  is `0x0A..0x3B`.
- The second, at `0x3B`, has byte 0 = 3, byte 1 = 0, byte 2 = 3, byte 3 = 0, the word 15 at
  `0x06` and a length of 66; its data is `0x45..0x87`.
- Every later chunk, from `0x87`, has byte 2 = 1, byte 3 = 0 and a length of 23. 42 have byte 0
  = 2 and 109 have byte 0 = 4.
- The type-2 chunks have the word 16 (14 chunks), 17 (18) or 4 (10) at `0x06`, and byte 1 = 0
  except in three chunks of `CHAR/30` and `CHAR/33` where byte 1 is 15, 18 or 22 and the word is 4.
  Each of those three indexes an earlier chunk of byte 2 = 1.
- Every type-4 chunk has the word 2 at `0x06` and a byte 1 below its own index.
- Byte 1 of the first chunk equals the number of chunks before the 0xFF.
- The end header is the first header with byte 0 = 0xFF and a length of 0.

| Chunks | Size | Resources |
| --- | --- | --- |
| 2 | 145 | 32 |
| 4 | 211 | 34, 35, 36, 42 |
| 5 | 244 | 51 |
| 6 | 277 | 39, 40, 43 |
| 7 | 310 | 29, 37, 38, 41 |
| 12 | 475 | 53 |
| 13 | 508 | 50 |
| 19 | 706 | 52 |
| 21 | 772 | 31 |
| 26 | 937 | 33 |
| 29 | 1,036 | 30 |

So the size is `145 + 33 * (chunks - 2)`, which is `79 + 33 * byte1`.

`OBJEX.GFF`'s `FNFO` 1 (980 bytes at file `0x1FF48`) and `FNFO` 2 (98 bytes at `0x2031C`,
FND-CONFIG-153) hold, for the fields the chunk headers name:

| Kind | Field | Word at `FNFO` 1 offset | Value | Type byte in `FNFO` 2 |
| --- | --- | --- | --- | --- |
| 2 | 15 | `0xE2` | 4 | 12 |
| 2 | 16 | `0xE4` | 8 | 7 |
| 2 | 17 | `0xE6` | 10 | 7 |
| 2 | 4 | `0xCC` | 12 | 7 |
| 1 | 4 | `0x08` | 8 | 7 |

The bytes at `DS:040C` plus 7 and plus 12 are 2 in the load image of `DSUN.EXE` (file `0x4D413`
and `0x4D418`).

## Interpretation

Through `1AA0:012A` (FND-PARTY-051), a type-3 chunk of a stored record stores the number of the
combatant details record it filled in the word at `+0x04` of the character's combatant record,
the `details_index` of FMT-COMBAT-001, since its byte 1 is 0 and indexes the type-1 chunk. Each
type-2 chunk with byte 1 = 0 stores the handle of its 23-byte record in the character's word at
`+0x08`, `+0x0A` or `+0x0C`, the three words the type-1 copy sets to 9,999 (FND-PARTY-049); the
three with another byte 1 store it at `+0x08` of an earlier 23-byte record. The type-4 chunks
add records to the chains those start. The 23-byte records go into the table at `DS:19C1`.

## Alternatives

- FND-PARTY-004 interprets the same sizes as a 79-byte header and `byte1` records of 33 bytes.
  The load code reads the chunks of this finding instead (FND-PARTY-051), and the 33-byte
  records it describes from `0x4F` do not start where the chunks do; this was found while
  locating the code that reads bytes after `0x3A`. This finding repeats its sizes.
- The `FNFO` buffers hold other values at the time of a load: they are read once at start-up by
  overlay 188 `+0000` (FND-CONFIG-150); writes to `DS:5AF7..DS:5ECB`, `DS:60ED..DS:614F` and
  `DS:040C` after that were not searched for.

## How to reproduce

From the commit that adds this finding, read each `CHAR` resource of the installed
`CHARSAVE.GFF` through the archive's directory (FMT-GFF-002) and walk its chunks: from offset
0, read the 10-byte header (byte 0, byte 1, byte 2, byte 3, the words at `0x04`, `0x06` and
`0x08`) and step on by 10 plus the word at `0x08` until byte 0 is 0xFF; print each header and
the offset of the 0xFF. Read `FNFO` 1 and `FNFO` 2 of `OBJEX.GFF` the same way and print the
words at `0xCC`, `0xE2`, `0xE4`, `0xE6` and `0x08` of the first and bytes 4, 15, 16 and 17 of
the second. Read the bytes at file `0x4D413` and `0x4D418` of `DSUN.EXE`.
