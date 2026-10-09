---
id: FND-VIDEO-001
title: The five numbered FLI files are 320x200 8-bit animations whose frame records cover each file and number one more than the header's frame count
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: 1.FLI
    offset: 0x00..0x7F
  - build: BLD-GOG-EN-1.1
    file: 2.FLI
    offset: 0x00..0x7F
  - build: BLD-GOG-EN-1.1
    file: 3.FLI
    offset: 0x00..0x7F
  - build: BLD-GOG-EN-1.1
    file: 4.FLI
    offset: 0x00..0x7F
  - build: BLD-GOG-EN-1.1
    file: 5.FLI
    offset: 0x00..0x7F
tool: file listing and hex inspection with Python 3.14.7
environment: null
---

## Observation

The installation's root holds five files named `1.FLI` to `5.FLI`, 19,498,251 bytes in all, and
no other `.FLI` file. Read as little-endian values, the first 128 bytes of each hold at offset 0
a double word equal to the file's length, at 4 the word `0xAF11`, at 8 and 10 the words 320 and
200, at 12 the word 8, and at 14 the word 3, and bytes 18 to 127 are 0. The word at 6 and the
word at 16 differ:

| File | Size | Word at 6 | Word at 16 | Records | Records with no chunk |
|---|---|---|---|---|---|
| `1.FLI` | 5,710,926 | 1175 | 7 | 1176 | 593 |
| `2.FLI` | 1,293,526 | 373 | 7 | 374 | 105 |
| `3.FLI` | 6,414,620 | 575 | 7 | 576 | 146 |
| `4.FLI` | 323,812 | 284 | 7 | 285 | 14 |
| `5.FLI` | 5,755,367 | 1398 | 5 | 1399 | 843 |

From byte 128, reading a double word size and a word type at each position and stepping by the
size lands exactly on the end of every file, and every type is `0xF1FA`. Each record's 16 bytes
hold the size, the type, at offset 6 a word count of the chunks that follow, and 8 bytes that are
0 in every record of the five files. Every record with
no chunk is 16 bytes long. Each chunk starts with a double word size, which counts its own 6-byte
head, and a word type. Stepping through the stated number of chunks by their sizes gives these
types, as counts of `0x0B`/`0x0C`/`0x0F`/`0x10`:

| File | Chunk types | Bytes left over |
|---|---|---|
| `1.FLI` | 17/578/1/1 | 2 |
| `2.FLI` | 3/266/1/1 | 2 |
| `3.FLI` | 4/429/1/0 | 0 |
| `4.FLI` | 1/270/1/0 | 0 |
| `5.FLI` | 6/554/1/0 | 0 |

The left-over bytes are record bytes after the last chunk. In the first record of `5.FLI`, 5,279
bytes long with two chunks, the second chunk, of type `0x0F`, states 4,486 bytes where 4,485 remain
in the record; every other chunk of the five files ends inside its record. The first record of
each file starts with a chunk of type `0x0B`. The last record of `3.FLI` and of `4.FLI` has no
chunk.

An earlier inspection treated `5.FLI`'s overlong chunk as a record boundary: extending the first
record by one byte makes both chunks fit, but the next record then reads as type `0x01F1` with a
size of 4,194,304,014 bytes. That reading is wrong, since the records step by their own sizes.

## Interpretation

The files are Autodesk FLI animations, whose published layout names these fields: file size,
magic, frame count, width, height, depth, flags and speed, then frame records of type `0xF1FA`
holding colour (`0x0B`), line-delta (`0x0C`), run-length (`0x0F`) and raw copy (`0x10`) chunks.
The one record past the frame count is the published format's loop frame. The one-byte overrun in
`5.FLI` is in a last chunk, whose stated size nothing after it depends on.

## Alternatives

The chunk names come from the published format; FND-VIDEO-008 shows which types the game's
player treats how. Whether the game shows the record past the frame count is shown there as well.

## How to reproduce

List the installation's `.FLI` files with their sizes, read the first 128 bytes of each, then
step through the records from byte 128 and through each record's chunks by their stated sizes.
