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
evidence: [FND-VIDEO-001, FND-VIDEO-002, FND-VIDEO-004]
conflicting: []
split_with: []
related: []
---

## Layout

The five cinematics `1.FLI` to `5.FLI` of the installation [FND-VIDEO-001], which the game plays
through its FLI player [FND-VIDEO-002], and the copies it takes from the disc's `CINE` directory
[FND-VIDEO-004].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `UINT32LE` | `file_size` | The file's length. Not read by the player. | supported | FND-VIDEO-001, FND-VIDEO-002 |
| `0x04` | 2 | `UINT16LE` | `magic` | `0xAF11`; the player refuses a file without it. | supported | FND-VIDEO-001, FND-VIDEO-002 |
| `0x06` | 2 | `UINT16LE` | `frame_count` | The number of records the player shows. | supported | FND-VIDEO-001, FND-VIDEO-002 |
| `0x08` | 2 | `UINT16LE` | `width` | 320. Not read by the player, which draws 320x200. | supported | FND-VIDEO-001, FND-VIDEO-002 |
| `0x0A` | 2 | `UINT16LE` | `height` | 200. Not read by the player. | supported | FND-VIDEO-001, FND-VIDEO-002 |
| `0x0C` | 2 | `UINT16LE` | `depth` | 8. Not read by the player. | supported | FND-VIDEO-001, FND-VIDEO-002 |
| `0x0E` | 2 | `UINT16LE` | `flags` | 3. Not read by the player. | supported | FND-VIDEO-001, FND-VIDEO-002 |
| `0x10` | 2 | `UINT16LE` | `speed` | 7, or 5 in `5.FLI`. Not read by the player, which takes the time between frames from its caller. | supported | FND-VIDEO-001, FND-VIDEO-002 |
| `0x12` | 110 | `BYTE[110]` | `padding` | 0. | supported | FND-VIDEO-001, FND-VIDEO-002 |
| `0x80` | to the end | `frame_record[]` | `records` | `frame_count + 1` records, each starting where the last one's `record_size` ends. | supported | FND-VIDEO-001, FND-VIDEO-002 |

`frame_record`, `record_size` bytes:

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `UINT32LE` | `record_size` | The record's length, these 16 bytes included. | supported | FND-VIDEO-001, FND-VIDEO-002 |
| `0x04` | 2 | `UINT16LE` | `record_type` | `0xF1FA`; the player stops with an error on any other value. | supported | FND-VIDEO-001, FND-VIDEO-002 |
| `0x06` | 2 | `UINT16LE` | `chunk_count` | The number of chunks; 0 in a record that changes nothing. | supported | FND-VIDEO-001, FND-VIDEO-002 |
| `0x08` | 8 | `BYTE[8]` | `padding` | 0. | supported | FND-VIDEO-001, FND-VIDEO-002 |
| `0x10` | `record_size - 16` | `chunk[chunk_count]` | `chunks` | The chunks, each starting where the last one's `chunk_size` ends; any bytes after the last are not read. | supported | FND-VIDEO-001, FND-VIDEO-002 |

`chunk`, `chunk_size` bytes:

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `UINT32LE` | `chunk_size` | The chunk's length, these 6 bytes included. | supported | FND-VIDEO-001, FND-VIDEO-002 |
| `0x04` | 2 | `UINT16LE` | `chunk_type` | What the player does with the data; see below. | supported | FND-VIDEO-001, FND-VIDEO-002 |
| `0x06` | `chunk_size - 6` | `BYTE[chunk_size - 6]` | `data` | Passed to the routine for the type. | supported | FND-VIDEO-002 |

## Enumerations and flags

### `chunk_type`

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| `0x0B` | `COLOUR` | The player passes the data to two routines that read as setting the palette. | supported | FND-VIDEO-002 |
| `0x0C` | `LINE_DELTA` | The player passes the data and the screen to a decoder. | supported | FND-VIDEO-002 |
| `0x0D` | `BLACK` | The player fills the 64,000 bytes of the screen with 0. | supported | FND-VIDEO-002 |
| `0x0E` | `IGNORED` | The player does nothing. | supported | FND-VIDEO-002 |
| `0x0F` | `RUN_LENGTH` | The player passes the data, the screen and the height 200 to a decoder. | supported | FND-VIDEO-002 |
| `0x10` | `COPY` | The player copies 64,000 bytes of data to the screen. | supported | FND-VIDEO-002 |

Any other type is skipped. The files use `0x0B`, `0x0C`, `0x0F` and `0x10` [FND-VIDEO-001].

## Differences between builds

None known.

## Coverage

All five installed files [FND-VIDEO-001]. The copies on the disc were not compared with them.

## Open questions

- How the data of the `0x0B`, `0x0C` and `0x0F` chunks is laid out: the routines at `57D4`,
  `57D7`, `57DA` and `57DD` that the player passes it to were not read, and the published FLI
  layout is the only account (FND-VIDEO-002, Q-VIDEO-001).
- The last chunk of the first record of `5.FLI` states one byte more than its record holds; what
  the run-length routine reads there was not checked (FND-VIDEO-001, Q-VIDEO-001).
