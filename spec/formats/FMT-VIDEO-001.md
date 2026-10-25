---
id: FMT-VIDEO-001
title: FLI animation
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["*.FLI", "CD:CINE/*.FLI"]
byte_order: little
size: null
text: false
definition: fmt_video_001.ksy
evidence: [FND-VIDEO-001, FND-VIDEO-008, FND-VIDEO-004, FND-EXE-613, FND-VIDEO-011]
conflicting: []
split_with: []
related: []
---

## Layout

The five cinematics `1.FLI` to `5.FLI` of the installation [FND-VIDEO-001], which the game plays
through its FLI player [FND-VIDEO-008], and the copies it takes from the disc's `CINE` directory
[FND-VIDEO-004].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `UINT32LE` | `file_size` | The file's length. Not read by the player. | supported | FND-VIDEO-001, FND-VIDEO-008 |
| `0x04` | 2 | `UINT16LE` | `magic` | `0xAF11`; the player refuses a file without it. | supported | FND-VIDEO-001, FND-VIDEO-008 |
| `0x06` | 2 | `UINT16LE` | `frame_count` | The number of records the player shows. | supported | FND-VIDEO-001, FND-VIDEO-008 |
| `0x08` | 2 | `UINT16LE` | `width` | 320. Not read by the player, which draws 320x200. | supported | FND-VIDEO-001, FND-VIDEO-008 |
| `0x0A` | 2 | `UINT16LE` | `height` | 200. Not read by the player. | supported | FND-VIDEO-001, FND-VIDEO-008 |
| `0x0C` | 2 | `UINT16LE` | `depth` | 8. Not read by the player. | supported | FND-VIDEO-001, FND-VIDEO-008 |
| `0x0E` | 2 | `UINT16LE` | `flags` | 3. Not read by the player. | supported | FND-VIDEO-001, FND-VIDEO-008 |
| `0x10` | 2 | `UINT16LE` | `speed` | 7, or 5 in `5.FLI`. Not read by the player, which takes the time between frames from its caller. | supported | FND-VIDEO-001, FND-VIDEO-008 |
| `0x12` | 110 | `BYTE[110]` | `padding` | 0. | supported | FND-VIDEO-001, FND-VIDEO-008 |
| `0x80` | to the end | `frame_record[]` | `records` | `frame_count + 1` records, each starting where the last one's `record_size` ends. | supported | FND-VIDEO-001, FND-VIDEO-008 |

`frame_record`, `record_size` bytes:

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `UINT32LE` | `record_size` | The record's length, these 16 bytes included. | supported | FND-VIDEO-001, FND-VIDEO-008 |
| `0x04` | 2 | `UINT16LE` | `record_type` | `0xF1FA`; the player stops with an error on any other value. | supported | FND-VIDEO-001, FND-VIDEO-008 |
| `0x06` | 2 | `UINT16LE` | `chunk_count` | The number of chunks; 0 in a record that changes nothing. | supported | FND-VIDEO-001, FND-VIDEO-008 |
| `0x08` | 8 | `BYTE[8]` | `padding` | 0. | supported | FND-VIDEO-001, FND-VIDEO-008 |
| `0x10` | `record_size - 16` | `chunk[chunk_count]` | `chunks` | The chunks, each starting where the last one's `chunk_size` ends; any bytes after the last are not read. | supported | FND-VIDEO-001, FND-VIDEO-008 |

`chunk`, `chunk_size` bytes:

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `UINT32LE` | `chunk_size` | The chunk's length, these 6 bytes included. | supported | FND-VIDEO-001, FND-VIDEO-008 |
| `0x04` | 2 | `UINT16LE` | `chunk_type` | What the player does with the data; see below. | supported | FND-VIDEO-001, FND-VIDEO-008 |
| `0x06` | `chunk_size - 6` | `BYTE[chunk_size - 6]` | `data` | Passed to the routine for the type. | supported | FND-VIDEO-008 |

## Enumerations and flags

### `chunk_type`

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| `0x0B` | `COLOUR` | Colour packets, kept in the palette buffer and, when the player asks, written to the VGA palette; layout below. | supported | FND-VIDEO-008, FND-EXE-613 |
| `0x0C` | `LINE_DELTA` | Changed lines of the screen; layout below. | supported | FND-VIDEO-008, FND-EXE-613 |
| `0x0D` | `BLACK` | The player fills the 64,000 bytes of the screen with 0. | supported | FND-VIDEO-008 |
| `0x0E` | `IGNORED` | The player does nothing. | supported | FND-VIDEO-008 |
| `0x0F` | `RUN_LENGTH` | A whole 320 by 200 screen in byte runs, 200 lines; layout below. | supported | FND-VIDEO-008, FND-EXE-613 |
| `0x10` | `COPY` | The player copies 64,000 bytes of data to the screen. | supported | FND-VIDEO-008 |

Any other type is skipped. The files use `0x0B`, `0x0C`, `0x0F` and `0x10` [FND-VIDEO-001].

### Chunk data

`COLOUR` data is a `UINT16LE` packet count, then for each packet a skip byte and a count byte (0
meaning 256) followed by three bytes for each of the count colours. The skip moves the colour
index forward by its value before the packet. The routine that fills the palette buffer adds
skip and count as whole words whose high byte is the packet count's high byte, or 1 after a count
of 0, so in a chunk where a 256-colour packet is followed by another its skip and count are each
256 larger; the routine that writes the VGA palette uses the bytes alone [FND-EXE-613].

`RUN_LENGTH` data is, for each of the 200 lines, a packet count byte and then that many packets:
a signed count byte, then for a count of 0 or more one byte stored that many times, and for a
negative count the negation of the count bytes copied. Each line starts 320 bytes after the one
before [FND-EXE-613].

`LINE_DELTA` data is a `UINT16LE` first line and a `UINT16LE` line count, then for each line a
packet count byte and that many packets: a skip byte added to the position, then a signed count
byte, and for a count of 0 or more that many bytes copied, for a negative count one byte stored
the negation of the count times. Each line starts 320 bytes after the one before. The run signs
are the opposite of `RUN_LENGTH`'s [FND-EXE-613].

Every chunk of the five installed files decodes within its stated size and its record, no colour
chunk has a 256-colour packet followed by another, and no line count is 0. The run-length chunk
of `5.FLI`'s first record states one byte more than its record holds; its 200 lines end at the
record's last byte, so that byte is never read [FND-VIDEO-011].

## Differences between builds

None known.

## Coverage

All five installed files [FND-VIDEO-001]. The copies on the disc were not compared with them.

## Open questions

None.
