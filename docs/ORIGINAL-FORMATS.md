# Original file formats

Confidence values are `unknown`, `low`, `medium`, `high`, and `verified`.
Observed filenames and sizes establish only inventory facts, not internal
semantics.

| Format/family | Observed role | Current knowledge | Confidence | Required failure behavior |
|---|---|---|---|---|
| `*.GFF` | Shared resources, numbered regions, and legacy saves | Little-endian version `0x00030000` container directory is decoded; indexed-image and palette payloads are decoded, while other tag semantics remain mostly unknown | verified for the directory layout and recorded image/palette families in the owned build | Bound file size and every count/offset/length; reject overflow, truncation, invalid secondary references, duplicate keys, and partial overlaps with source context |
| GFF `BMP `, `CBMP`, `ICON` | Indexed image frames | Shared size/frame envelope plus sparse row, `PLAN`, and `PLNR` encodings are decoded to palette indices and alpha | verified for all matching resources in the owned build | Bound payload, frame count, offsets, dimensions, total pixels, compressed runs, dictionaries, and bitstreams; reject malformed input with resource identity |
| GFF `PAL ` | 256-color palettes | Exactly 256 RGB triples with 6-bit VGA components, scaled by four to 8-bit channels | verified for all matching resources in the owned build | Require exactly 768 bytes and reject components outside `0..63` |
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
rejected. Other payload structures and cross-tag relationships remain open and
require separate evidence entries and tests.

## Indexed images and palettes

**Evidence:** `DATA-GOG-IMAGE-001`, corroborated by `DSUN-MUSIC`.

Bounded inspection decoded every `BMP `, `CBMP`, `ICON`, and `PAL ` resource in
all 26 installed GFF files. `RESOURCE.GFF` contains 653 image resources with
1,109 frames and 20 palettes; `OBJEX.GFF` contains 3,857 image resources with
8,170 frames; 20 region GFFs each contain one palette. The observed total is
4,510 images, 9,279 frames, and 40 palettes. These counts validate the payload
readers, not the semantic purpose or correct palette pairing of each resource.

An image payload begins with its exact 32-bit byte size, a 16-bit frame count,
and one strictly increasing 32-bit frame offset per frame. Each frame begins
with a 16-bit width and height. Frames then use one of these observed forms:

- Sparse rows name a row, then one or more bounded runs. Each run records its
  horizontal start, final-run/high-X flags, decoded length, encoded length, and
  PackBits-like literal or repeated palette-index data. Pixels not covered by a
  run remain transparent.
- `0xff` plus `PLAN` selects an MSB-first packed-symbol stream over a local
  palette-index dictionary.
- `0xff` plus `PLNR` uses the same dictionary and bit order with repeated-symbol
  commands. A decoded dictionary value of zero is transparent in both planar
  variants.

The reader caps a payload at 64 MiB, a frame count and either dimension at
4,096, one frame at 16 Mi pixels, and all frames in an image at 64 Mi pixels.
Palette payloads contain exactly 256 RGB triples. Each stored component is in
the VGA range `0..63`; multiplying by four produces the decoded channel range
`0..252`. Resource-to-palette mapping, display composition, frame origins, and
animation timing remain open.
