# Original file formats

Confidence values are `unknown`, `low`, `medium`, `high`, and `verified`.
Observed filenames and sizes establish only inventory facts, not internal
semantics.

| Format/family | Observed role | Current knowledge | Confidence | Required failure behavior |
|---|---|---|---|---|
| `*.GFF` | Shared resources, numbered regions, and legacy saves | Little-endian version `0x00030000` container directory is decoded; resource-payload semantics remain tag-specific and mostly unknown | verified for the directory layout in the owned build | Bound file size and every count/offset/length; reject overflow, truncation, invalid secondary references, duplicate keys, and partial overlaps with source context |
| `*.FLI` | Four observed cinematics | Likely animation resources by extension only; dimensions, palette changes, frame timing, and exact variant are unverified | low | Bound frames, chunks, dimensions, decoded bytes, and timing; unsupported chunk types fail explicitly |
| `*.VOC` | Speech and sound-effect files | Creative Labs VOC is suggested by extension; headers, codecs, sample rates, and block use must be verified per file | low | Bound blocks and decoded samples; reject unsupported codecs and malformed terminators |
| `MUSIC/*.ogg` | 39 GOG-supplied music files | Ogg container presence is observed; mapping, loop points, provenance, and relationship to original media are unknown | low | Validate stream metadata and decode limits; unknown track mapping remains data, not a guessed rule |
| `ITEMS.BIN` | Small game data file | Size is 936 bytes in the supported build; record layout and meaning are unknown | unknown | Require exact supported source fingerprint before any reader; bound all table dimensions |
| `game.gog` / `game.ins` | GOG disc-image payload and descriptor | Presence and exact sizes observed; extractor relevance is unknown | low | Never mount/execute automatically; parse only if a later slice documents a bounded need |
| `DSUN.EXE` | Original DOS executable | Fingerprinted research oracle; never an extraction output or runtime dependency | verified inventory only | Never execute, load, copy into the pack, or commit; Ghidra findings remain independent evidence |

The GFF directory is the first implemented format layer. Which payload tags and
source files form the minimum complete Slice 2 pack remains open under `Q4` in
the implementation plan. Every payload reader requires synthetic fixtures and
its own evidence before production extraction uses it.

## GFF container directory

**Evidence:** `DATA-GOG-GFF-001`, corroborated by `DSUN-MUSIC`.

All 26 `.GFF` files in GOG-1432903719 parse with the bounded reader. Together
they expose 16,168 resource descriptors. The three fingerprint anchors contain
1,858 descriptors in `RESOURCE.GFF`, 535 in `GPLDATA.GFF`, and 10,532 in
`OBJEX.GFF`. These counts describe directory entries only; they do not prove a
payload tag's meaning.

Observed multibyte integers are little-endian. The 28-byte header contains:

| Offset | Size | Meaning | Confidence |
|---:|---:|---|---|
| `0x00` | 4 | ASCII `GFFI` signature | verified |
| `0x04` | 4 | version `0x00030000` | verified for supported files |
| `0x08` | 4 | header size, `28` | verified |
| `0x0c` | 4 | absolute directory offset | verified |
| `0x10` | 12 | header fields not yet interpreted | unknown |

At the directory offset, two currently uninterpreted 32-bit values precede a
16-bit tag-table count. Each table begins with a four-byte printable ASCII tag
and a 32-bit count marker:

- A positive signed marker is an inline primary table. It is followed by that
  many 12-byte entries: resource number, absolute offset, and byte length.
- A marker with its high bit set identifies a secondary table. Its low 31 bits
  match a repeated resource count, followed by a zero-based entry in the
  primary `GFFI` table and a segmented numbering map. Each numbering segment is
  a first resource number and consecutive count. The referenced `GFFI` payload
  starts with the entry count and then eight-byte absolute-offset/length pairs.
- Primary-only region and save containers legitimately omit a `GFFI` table.

The reader caps files at 512 MiB, tag tables at 4,096, and total resources at
1,000,000. It validates all arithmetic and referenced table bounds before
exposing payload bytes. Exact aliases are accepted because the owned files may
address one byte range more than once; non-identical overlapping ranges are
rejected. Payload structures, compression, palette semantics, and cross-tag
relationships remain open and require separate evidence entries and tests.
