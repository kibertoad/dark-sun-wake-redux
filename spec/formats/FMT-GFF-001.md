---
id: FMT-GFF-001
title: GFF resource container
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["*.GFF"]
byte_order: little
size: null
text: false
definition: fmt_gff_001.ksy
evidence: [FND-GFF-001, FND-GFF-002, FND-GFF-004, SRC-DSUN-MUSIC-79B6927]
conflicting: []
split_with: []
related: []
---

## Layout

A `.GFF` file holds numbered resources, each filed under a four-character tag. Its directory
(FMT-GFF-002) gives, for each tag, the numbers the file holds and the offset and size of each
resource's bytes, which lie in `data`. The spec refers to a resource as `FILE.GFF#TAG/number`: the
file's path as the build entry writes it, the tag with its trailing spaces removed, and the
resource number in decimal. `RESOURCE.GFF#BMP/20029` is the resource numbered 20029 under the tag
`BMP ` in `RESOURCE.GFF`, and `RESOURCE.GFF#WIND/12500` the one numbered 12500 under `WIND`. A tag
has a space only at its end [FND-GFF-005], so the reference gives the tag back unchanged once the
spaces are added to four characters.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `char[4]` | `signature` | `GFFI`, ASCII, with no NUL. | supported | FND-GFF-001 |
| `0x04` | 4 | `UINT32LE` | `version` | `0x00030000` in every shipped file. | supported | FND-GFF-001 |
| `0x08` | 4 | `UINT32LE` | `header_size` | 28 in every shipped file: this header's size and the offset of `data`. | supported | FND-GFF-001 |
| `0x0C` | 4 | `UINT32LE` | `directory_offset` | File offset of `directory`. Every resource lies before it. | supported | FND-GFF-001, FND-GFF-002 |
| `0x10` | 4 | `UINT32LE` | `directory_size` | Bytes in `directory`, up to the end of its gap list. | supported | FND-GFF-001, FND-GFF-004 |
| `0x14` | 4 | `UINT32LE` | `unk_14` | Purpose unknown. 8 in the region files `RGN*.GFF` and the installed `CHARSAVE.GFF`, 0 in the others. | supported | FND-GFF-001 |
| `0x18` | 4 | `UINT32LE` | `unk_18` | Purpose unknown. 1 in most files; 117 in the installed `CHARSAVE.GFF`; 3 to 22 in the region files, rising by one in file-name order. | supported | FND-GFF-001 |
| `0x1C` | `directory_offset - 28` | `BYTE[directory_offset - 28]` | `data` | The resources' bytes, at the offsets the directory gives, and the unused ranges its gap list gives. | supported | FND-GFF-002, FND-GFF-004 |
| | `directory_size` | `FMT-GFF-002` | `directory` | The tag tables and the gap list. | supported | FND-GFF-002, FND-GFF-004 |
| | `file_size - directory_offset - directory_size` | `BYTE[file_size - directory_offset - directory_size]` | `trailing` | Bytes that no resource and no gap covers: none in most files, thousands in `GPLDATA.GFF`, `OBJEX.GFF` and `RESOURCE.GFF`. | supported | FND-GFF-004 |
| | | | | Total size `file_size` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 26 `.GFF` files of the installation and all 25 in the root of the disc's ISO 9660 volume, in
BLD-GOG-EN-1.1, read in full: in each, the header, the directory, every resource, the gap list and
the trailing bytes account for every byte of the file [FND-GFF-001, FND-GFF-002, FND-GFF-004,
FND-GFF-005]. SRC-DSUN-MUSIC-79B6927 reads the same header and directory layout.

## Open questions

- What `unk_14` and `unk_18` hold, whether the game writes them when it saves, and whether it reads
  `version`, `header_size` or `directory_size` at all (Q-GFF-002).
- Whether the game ever reads `trailing`, or whether the bytes are left over from an earlier, longer
  file, as their position suggests (FND-GFF-004, Q-GFF-002).
- Which file the game looks in for a given tag (FND-GFF-005, Q-GFF-002).
