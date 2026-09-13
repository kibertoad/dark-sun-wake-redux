# Original file formats

Confidence values are `unknown`, `low`, `medium`, `high`, and `verified`.
Observed filenames and sizes establish only inventory facts, not internal
semantics.

| Format/family | Observed role | Current knowledge | Confidence | Required failure behavior |
|---|---|---|---|---|
| `*.GFF` | Shared resources, numbered regions, and legacy saves | Little-endian version `0x00030000` container directory is decoded; indexed-image and palette payloads are decoded, while other tag semantics remain mostly unknown | verified for the directory layout and recorded image/palette families in the owned build | Bound file size and every count/offset/length; reject overflow, truncation, invalid secondary references, duplicate keys, and partial overlaps with source context |
| GFF `BMP `, `CBMP`, `ICON` | Indexed image frames | Shared size/frame envelope plus sparse row, `PLAN`, and `PLNR` encodings are decoded to palette indices and alpha | verified for all matching resources in the owned build | Bound payload, frame count, offsets, dimensions, total pixels, compressed runs, dictionaries, and bitstreams; reject malformed input with resource identity |
| GFF `PAL ` | 256-color palettes | Exactly 256 RGB triples with 6-bit VGA components, scaled by four to 8-bit channels | verified for all matching resources in the owned build | Require exactly 768 bytes and reject components outside `0..63` |
| GFF `FONT` | Indexed bitmap glyphs | A shared-height 256-glyph table with character map, absolute offsets, widths, and palette-index pixels is decoded | verified for `FONT` #100 in `RESOURCE.GFF` | Bound payload, count, height, offsets, widths, total pixels, and exact record ends |
| GFF `TEXT` | Short text tables | Printable 7-bit ASCII lines with mandatory CRLF delimiters and final terminator | verified for all 62 records in `RESOURCE.GFF` | Bound payload, line count, and line length; reject unsupported bytes and bare/missing terminators |
| GFF `CHAR` envelope, identity, and abilities | Original character records | Version 1 has a 79-byte fixed header followed by a byte-counted sequence of 33-byte uninterpreted tail records; six one-byte ability scores at offsets 35..40 follow manual Strength, Dexterity, Constitution, Intelligence, Wisdom, Charisma order; the name is printable ASCII, NUL-terminated within a 16-byte slot at offset 43; remaining fields are uninterpreted | high across all 19 records in the owned `CHARSAVE.GFF`; not runtime-validated | Require version 1 and exact `79 + count * 33` size; bound score range and name slot; stop at the first NUL because later slot bytes may be stale; reject trailing data, out-of-range scores, and empty, unterminated, or non-printable names |
| GFF `PSIN` | Character companion mask | Same-number companion for every owned `CHAR`; exactly one byte using a nonzero subset of the low three bits | high for record correlation and structural mask envelope; individual and combined bit meanings remain unknown | Require exactly one byte, reject zero and bits outside `0..2`, and require one-to-one correlation in catalog inspection |
| GFF `WIND`, `BUTN`, `APFM`, `EBOX` | UI layout and controls | Bounded common fields and references are decoded; unknown fixed fields and variable button tails remain uninterpreted | verified for all matching resources in `RESOURCE.GFF` | Require signatures and exact declared sizes; bound fixed records, complete child records, dimensions, tags, repeated identities, and references |
| GFF `RNME`, `PAL `, `MAP `, `GMAP`, `TILE`, `ETAB` | Region identity, palette, terrain grid, geometry plane, tile images, and placed-object references | Same-number region records, exact 128x98 byte planes, single-frame 16x16 tiles, and eight-byte entity records are decoded; geometry and entity-flag meanings remain unknown | verified structurally across all 20 owned region files; field roles corroborated by `DSUN-MUSIC` | Require exactly one matching region record per tag; bound names, planes, tile images, entity counts, and every local `TILE`/external `OJFF` reference |
| Pack `*.dsix` | Derived indexed-image asset | `DSIX` v1 stores one decoded 256-color palette plus bounded indexed frames and binary alpha | implemented; title asset round-trip and extraction validated | Bound file size, version, frame count, dimensions, pixels, alpha, and trailing data; reject non-binary alpha |
| Pack `*.dsft` | Derived indexed-font asset | `DSFT` v1 stores a character map, shared height, widths, and bounded indexed glyph pixels | implemented; interface-font round-trip and extraction validated | Bound file size, version, glyph count, height, widths, pixel counts, and trailing data |
| Pack `*.dstx` | Derived text catalog | `DSTX` v1 deterministically stores original resource IDs and printable ASCII lines | implemented; full TEXT catalog round-trip and extraction validated | Bound file size, version, resource/line counts and lengths; reject duplicates, unsupported bytes, and trailing data |
| Pack `*.dsui` | Derived UI catalog | `DSUI` v1 deterministically stores the six start-flow windows and their resolved BUTN/APFM/EBOX graph | implemented; synthetic round-trip and owned extraction validated | Bound file size, per-type counts, child counts, tags, dimensions, identities, references, and trailing data |
| Pack `*.dsch` | Derived character metadata catalog | `DSCH` v1 deterministically stores only the bounded `CHAR`/`PSIN` subset: resource ID, neutral tail-record count, name, six ordered abilities, and raw mask | implemented; synthetic round-trip and owned extraction validated | Bound file size, version, record/name counts, ability range, raw mask, canonical order, and trailing data; reject duplicates and do not carry uninterpreted source bytes |
| `*.FLI` | Four observed cinematics | Likely animation resources by extension only; dimensions, palette changes, frame timing, and exact variant are unverified | low | Bound frames, chunks, dimensions, decoded bytes, and timing; unsupported chunk types fail explicitly |
| `*.VOC` | Speech and sound-effect files | Creative Labs VOC is suggested by extension; headers, codecs, sample rates, and block use must be verified per file | low | Bound blocks and decoded samples; reject unsupported codecs and malformed terminators |
| `MUSIC/*.ogg` | 39 GOG-supplied music files | Ogg container presence is observed; mapping, loop points, provenance, and relationship to original media are unknown | low | Validate stream metadata and decode limits; unknown track mapping remains data, not a guessed rule |
| `ITEMS.BIN` | Small game data file | Size is 936 bytes in the supported build; record layout and meaning are unknown | unknown | Require exact supported source fingerprint before any reader; bound all table dimensions |
| `game.gog` / `game.ins` | GOG disc-image payload and descriptor | Bounded Mode 2/2352 ISO 9660 inspection located the disc character archive for `DATA-GOG-CHAR-006`; broader extractor relevance is unknown | medium for that bounded observation | Never mount/execute automatically; parse only when a documented evidence question requires it, bound sectors/directories/extents, and keep temporary original bytes outside Git |
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

## Region maps

**Evidence:** `DATA-GOG-REGION-001`, corroborated by `DSUN-MUSIC`.

Each of the 20 owned `RGN*.GFF` containers has a single region number shared by
`RNME`, `PAL `, `MAP `, `GMAP`, and `ETAB`. `RNME` is a printable ASCII name
terminated by NUL. `MAP ` and `GMAP` are each exactly 12,544 bytes, indexed in
row-major order over 128 columns and 98 rows. Every `MAP ` value is a byte-sized
resource number naming a local `TILE`; all 3,043 owned tiles decode as one 16x16
indexed-image frame. `GMAP` values are preserved verbatim because their
collision and wall-bit meanings have not yet been established for this build.

`ETAB` contains consecutive eight-byte records. The independently decoded
structural fields are signed 16-bit X and Y, a signed vertical-offset byte, an
uninterpreted flags byte, and a signed 16-bit object number. The absolute object
number of all 13,559 owned records resolves to an `OJFF` resource in the
fingerprinted `OBJEX.GFF`. The sign and flags are retained; no mirroring,
interaction, ordering, or collision behavior is inferred from them yet.

The reader caps the region name at 64 bytes and the entity table at 16,384
records, requires the exact map dimensions and 16x16 tile frames, and rejects
missing local tiles or external objects with region/resource context.

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

## Indexed bitmap fonts

**Evidence:** `DATA-GOG-FONT-001`.

`FONT` #100 in the fingerprinted `RESOURCE.GFF` is 8,299 bytes. Its little-endian
header starts with a 16-bit glyph count of 256 and a shared 16-bit height of 9;
four bytes remain uninterpreted. A 256-byte character map begins at offset 8,
followed at offset 264 by 256 absolute 16-bit glyph offsets. The first glyph
record begins at offset 776. Each strictly increasing record contains a 16-bit
width and exactly `width * height` palette-index bytes. Zero-width glyphs are
valid and contain no pixels. The final record must end exactly at payload end.
In the supported owned build, all 256 character-map entries are identity values
(`00` through `FF`) with SHA-256
`40aff2e9d2d8922e47afd4648e6967497158785fbd1da870e7110266bf944880`.
Consequently, direct byte indexing and lookup through this particular map select
the same glyph. The glyph pixels use three distinct palette indices with a
range of 0 through 254; their exact values and visual roles are not inferred by
the metadata-only catalog.

The reader caps payloads at 1 MiB, height at 64, width at 256, and total decoded
pixels at 4 MiB. The map field's generalized semantics, the four unknown header
bytes, palette pairing, advance/spacing rules, strings, and presentation remain
open.

## TEXT resources

**Evidence:** `DATA-GOG-TEXT-001`.

All 62 `TEXT` records in `RESOURCE.GFF` satisfy a narrow printable-ASCII and
CRLF line contract. They total 2,994 bytes and 316 lines; the maximum observed
line is 18 bytes. The reader caps payloads at 1 MiB, lines at 65,536, and each
line at 4,096 bytes. Empty lines are preserved. Resource-number semantics and
relationships to UI controls remain unassigned until separately evidenced.

## UI windows and buttons

**Evidence:** `DATA-GOG-UI-001`.

A `WIND` payload begins with its ASCII tag, exact 32-bit byte size, and embedded
32-bit resource number. The fixed record occupies 261 bytes; the remainder is a
sequence of complete 30-byte child references. Each interpreted child record
contains a printable four-byte tag at relative offset 4, its resource number at
8, and signed 16-bit logical x/y coordinates at 12/14. Window width and height
occur at fixed offsets 190/192. Offset 58 is a possibly-zero `BMP` resource
reference; every nonzero value in the owned archive resolves. Targeted
executable analysis (`EXE-GOG-UI-002`) found no read of this field in the
complete generic WIND lookup, registration, redraw, or activation paths, so it
must not be interpreted as an automatic background draw command. Other fixed
fields and any app-specific consumer remain unknown.

A `BUTN` payload has the same tag/size/resource prefix and a fixed 110-byte
known portion. Width and height occur at offsets 40/42, the resource number is
followed by the shared 16-bit event mask at 88, the resource number is repeated
at 90, and a possibly-zero `ICON` resource reference occurs at 100.
Observed payloads range from 110 to 163 bytes; bytes after the fixed portion are
preserved as uninterpreted rather than assigned guessed semantics. The reader
caps a standalone payload at 1 MiB and validates nonzero dimensions. The
inspection command additionally verifies embedded/directory identities and all
nonzero child and image references.

An `APFM` application-frame payload is exactly 116 bytes and stores its embedded
resource number at offset 8, dimensions at 40/42, and a 16-bit event mask at
offset 88. The executable tests this word against a caller-supplied mask before
building a dispatch record; individual bit meanings remain unknown. An `EBOX`
edit-box payload is exactly 168 bytes, repeats its offset-8 resource number at
offset 24, and stores dimensions at 34/36 plus its event mask at 150. All other
fields remain deliberately uninterpreted.
Every one of the 97 `APFM` and 7 `EBOX` resources in the owned `RESOURCE.GFF`
matches these bounded contracts. All four start-window `BUTN` records store a
zero event mask, so bit semantics or runtime initialization cannot be inferred
from their serialized values alone.

## Derived DSIX indexed-image asset

DSIX is an original, versioned pack format; it is not an original-game format.
It keeps the decoded indexed pixels and palette explicit so the runtime need not
parse GFF data. Version 1 is little-endian:

| Offset/sequence | Field |
|---|---|
| `0x00` | ASCII `DSIX` signature |
| `0x04` | 16-bit format version (`1`) |
| `0x06` | 16-bit frame count |
| `0x08` | 256 RGB triples using decoded 8-bit components |
| per frame | 16-bit width, 16-bit height, 32-bit pixel count, then all palette-index bytes followed by all binary-alpha bytes |

The pack reader applies the same frame/dimension/pixel limits as the original
image decoder, caps the file at 160 MiB, requires the declared pixel count to
equal width times height, accepts alpha values only at 0 or 255, and rejects
trailing bytes.

## Derived DSUI start-flow UI catalog

DSUI is an original, versioned pack format that removes uninterpreted UI bytes
while preserving the verified runtime-facing graph. Version 1 begins with ASCII
`DSUI`, a 16-bit version, then 16-bit counts for windows, buttons, application
frames, and edit boxes. Window records store resource/image IDs, dimensions,
and ordered children as a four-byte tag, resource ID, and signed coordinates.
Control records store their resource ID, dimensions, and event mask; buttons
also store their image resource ID. Records of each type are written in numeric
resource order for deterministic output.

The reader caps the file at 4 MiB and each record family and window child list
at 4,096 entries. It rejects non-printable or unsupported child tags, duplicate
same-type identities, invalid dimensions, unresolved children, truncation, and
trailing bytes. The Slice 2 pack contains `WIND` #18501 and #19500 through #19505
plus all 56 of their referenced `BUTN`, `APFM`, and `EBOX` records. Unknown source fields,
button tails, palette assumptions, and shell draw semantics are not carried.

## Derived DSCH character-metadata catalog

DSCH is an original deterministic pack format that carries only fields with a
recorded bounded interpretation. Version 1 starts with ASCII `DSCH`, a 16-bit
version, and a 32-bit character count. Records are written in ascending numeric
resource-ID order and contain the 32-bit source resource ID, one-byte neutral
tail-record count, one-byte ASCII name length plus the name bytes, the six
ability bytes in Strength, Dexterity, Constitution, Intelligence, Wisdom, and
Charisma order, and the unchanged one-byte raw PSIN mask.

The reader caps files at 64 KiB and catalogs at 256 records. It requires names
of 1 through 15 printable ASCII bytes, abilities from 9 through 24, and a
nonzero raw mask limited to the low three bits. It rejects duplicate or
noncanonical resource ordering, truncation, and trailing data. No
uninterpreted `CHAR` header or tail bytes enter the pack, and PSIN bit meanings
remain deliberately unassigned.

## Derived DSFT indexed-font asset

DSFT is an original, versioned pack format and contains no uninterpreted FONT
header fields or offsets. Version 1 starts with ASCII `DSFT`, a 16-bit version,
a 16-bit glyph count, a 16-bit shared height, and the 256-byte character map.
Each of the 256 glyphs then stores a 16-bit width, a 32-bit pixel count, and its
palette-index bytes. The reader requires each pixel count to equal width times
height, applies the original FONT dimension and aggregate-pixel limits, caps the
file at 8 MiB, and rejects truncation and trailing bytes. Palette selection and
text layout deliberately remain outside this asset until separately evidenced.

`IndexedGlyphRunRasterizer` and `IndexedGlyphBlockRasterizer` are
presentation-independent derived operations over DSFT. They accept explicit
glyph indices and caller-selected nonnegative glyph/line spacing, copy the
original palette-index pixels into a single row or ordered multiline block, and
cap output dimensions and area. Zero-width glyphs and blank lines remain valid.
They deliberately do not consult the uninterpreted character map or assign
transparent/color semantics, so they introduce no claim about source string
encoding or authentic appearance.

## Derived DSTX text-catalog asset

DSTX is an original deterministic pack format. Version 1 starts with ASCII
`DSTX`, a 16-bit version, and a 32-bit resource count. Resources are written
in ascending numeric-ID order; each stores its 32-bit original resource ID, a
32-bit line count, then lines as 16-bit byte lengths and printable ASCII bytes.
CRLF delimiters are removed because line boundaries are explicit. The reader
caps files at 8 MiB, resource/line counts at 65,536, and lines at 4,096 bytes,
and rejects duplicate IDs, unsupported bytes, truncation, and trailing data.
