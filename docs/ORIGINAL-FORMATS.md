# Original file formats

Confidence values are `unknown`, `low`, `medium`, `high`, and `verified`.
Observed filenames and sizes establish only inventory facts, not internal
semantics.

| Format/family | Observed role | Current knowledge | Confidence | Required failure behavior |
|---|---|---|---|---|
| `*.GFF` | Shared resources, numbered regions, and legacy saves | The container, its directory, tag tables and `GFFI` index are described by `FMT-GFF-001` to `FMT-GFF-007`, which also define the `FILE.GFF#TAG/number` resource reference; tag payloads are described below where known | see the format entries for the container; per-tag confidence below | Bound file size (512 MiB), tag tables (4,096) and resources (1,000,000), and every count/offset/length; reject overflow, truncation, invalid secondary references, duplicate tags or numbers, and partial overlaps with source context |
| GFF `BMP `, `CBMP`, `ICON`, `PORT`, `TILE` | Indexed image frames | Described by `FMT-IMAGE-001` and `FMT-IMAGE-002`, decoded by `RULE-IMAGE-001` and `RULE-IMAGE-002` | see the spec entries | Bound payload, frame count, offsets, dimensions, total pixels, compressed runs, dictionaries, and bitstreams; reject malformed input with resource identity; do not infer frame scheduling, cache policy, composition, or actor semantics from decoder dispatch |
| GFF `PAL ` | 256-color palettes | Described by `FMT-IMAGE-003` and `FMT-IMAGE-004` | see the spec entries | Require exactly 768 bytes and reject components outside `0..63` |
| GFF `FONT` | Indexed bitmap glyphs | Described by `FMT-TEXT-001` and `FMT-TEXT-002` | see the spec entries | Bound payload, count, height, offsets, widths, total pixels, and exact record ends |
| GFF `TEXT` | Short text tables | Described by `FMT-TEXT-003` | see the spec entry | Bound payload, line count, and line length; reject unsupported bytes and bare/missing terminators |
| GFF `CHAR`, `PSIN`, `PSST`, `SPST` | Character records | Described by `FMT-PARTY-001` to `FMT-PARTY-005`; the game keeps stored characters under `CACT` numbers 1 to 39 (`FND-PARTY-012`) | see the spec entries | Require version 1 and exact `79 + count * 33` size; bound score range and name slot; stop at the first NUL because later slot bytes may be stale; reject trailing data, out-of-range scores, and empty, unterminated, or non-printable names; require exactly one `PSIN` byte, reject zero and bits outside `0..2`, and require one-to-one correlation in catalog inspection |
| GFF `PREF` | Saved settings | Described by `FMT-CONFIG-003`; the game writes it with every saved game (`FND-SAVE-004`) | see the spec entries | Preserve byte-for-byte as DSOP; do not add a reader, schema, settings mutation, or UI binding without an independent source path or controlled observation |
| GFF `GREQ`, `CACT` | Saved game state and stored character identifiers | Described by `FMT-SAVE-002` and `FMT-SAVE-001` | see the spec entries | Preserve byte-for-byte as DSOP until a reader is specified |
| GFF `PLYL`, `CSEQ` | Opaque small-resource families | `RESOURCE.GFF` has six `PLYL` resources (#0, #10, #50-#53) with lengths from 3, 5, and 7 bytes, plus one 78-byte `CSEQ` #1000. `CSEQ` divides into six 13-byte units, but its leading words have only one GPL-number-set member and do not establish the separate runtime 13-byte selector source. No unaligned little-endian 16-bit window in the six `PLYL` payloads matches an installed `CHAR` resource number (FND-PARTY-019). These facts identify no field layout, feature, or relationship to another family | high for owned inventory/lengths and bounded word-reference results; unknown for every semantic role | Preserve byte-for-byte as DSOP; do not add readers or infer sequence or combat behavior without an independent source path or controlled observation |
| GFF `WIND`, `BUTN`, `APFM`, `EBOX` | UI layout and controls | Described by `FMT-UI-001` to `FMT-UI-005`; the mask tests are `RULE-UI-001` | see the spec entries | Require signatures and exact declared sizes; bound fixed records, complete child records, dimensions, tags, repeated identities, and references |
| GFF `GPL ` / `MAS ` | Script resources | Described by `FMT-SCRIPT-001` and, for strings in script code, `FMT-SCRIPT-002`; the interpreter that loads and runs them is `RULE-SCRIPT-001` to `RULE-SCRIPT-008` | see the spec entries | Preserve the four-byte source tag with resource identity and bytes; bound script/string lengths, reject unsupported tags/markers, truncation, and unterminated strings; never execute source bytes during extraction |
| GFF `GPLI` #1 | Script entry point index | Described by `FMT-SCRIPT-003`; overlay 187 converts entry points to and from its entries (`FND-SCRIPT-017`) | see the spec entry | Preserve losslessly as DSOP until a reader is specified |
| GFF `MONR` #1 | Unclassified payload | `FMT-ACTOR-003`, status unknown; `FND-ACTOR-008` profiles its 27 repeating 42-byte units | see the spec entry | Preserve losslessly as DSOP; do not parse or assign monster, encounter or combat fields until the spec entry has a layout |
| GFF `RNME`, `PAL `, `MAP `, `GMAP`, `TILE`, `ETAB` | Region identity, palette, terrain grid, cell flags, tile images, and placed-object references | Described by `FMT-REGION-001` to `FMT-REGION-006`, `FMT-IMAGE-003` and `FMT-IMAGE-001`; terrain drawing is `RULE-REGION-001` | see the spec entries | Require exactly one matching region record per tag; bound names, planes, tile images, entity counts, and every local `TILE`/external `OJFF` reference |
| GFF `OJFF` | Object definitions | Described by `FMT-ACTOR-001`; placement is `RULE-ACTOR-001` | see the spec entries | Require exact record size, a zero final word, a present nonempty referenced image, bounded requested IDs, and contextual errors |
| Pack `*.dsix` | Derived indexed-image asset | `DSIX` v1 stores one decoded 256-color palette plus bounded indexed frames and binary alpha | implemented; title asset round-trip and extraction validated | Bound file size, version, frame count, dimensions, pixels, alpha, and trailing data; reject non-binary alpha |
| Pack `*.dsft` | Derived indexed-font asset | `DSFT` v1 stores a character map, shared height, widths, and bounded indexed glyph pixels | implemented; interface-font round-trip and extraction validated | Bound file size, version, glyph count, height, widths, pixel counts, and trailing data |
| Pack `*.dstx` | Derived text catalog | `DSTX` v1 deterministically stores original resource IDs and printable ASCII lines | implemented; full TEXT catalog round-trip and extraction validated | Bound file size, version, resource/line counts and lengths; reject duplicates, unsupported bytes, and trailing data |
| Pack `*.dsui` | Derived UI catalog | `DSUI` v1 deterministically stores selected window families and their resolved BUTN/APFM/EBOX graphs | implemented; start-flow, Game Menu, and character/inventory destination synthetic round-trip and owned extraction validated | Bound file size, per-type counts, child counts, tags, dimensions, identities, references, and trailing data |
| Pack `*.dsch` | Derived character metadata catalog | `DSCH` v1 deterministically stores only the bounded `CHAR`/`PSIN` subset: resource ID, neutral tail-record count, name, six ordered abilities, and raw mask | implemented; synthetic round-trip and owned extraction validated | Bound file size, version, record/name counts, ability range, raw mask, canonical order, and trailing data; reject duplicates and do not carry uninterpreted source bytes |
| Pack `*.dsrg` | Derived region catalog | `DSRG` v1 canonically stores the region identity, decoded palette and tiles, exact terrain/geometry planes, and raw ordered entity records | implemented; synthetic round-trip and owned Tyr extraction validated | Bound file size, version, fixed dimensions, counts, names, binary alpha, canonical tile order, map references, and exact content length |
| Pack `*.dsob` | Derived object-frame catalog | `DSOB` v1 canonically stores bounded OJFF fields plus deduplicated decoded indexed frames; the region supplies the palette | implemented; synthetic round-trip and owned Tyr extraction validated | Bound file size, version, definition/image/frame/pixel counts, dimensions, binary alpha, canonical ordering, complete references, and exact content length |
| Pack `*.dsgp` | Derived opaque script resource | `DSGP` v2 stores one verified `GPL ` or `MAS ` source tag, resource number, and byte-identical bounded bytecode | implemented; synthetic round-trip and owned GPL #135/MAS #99 extraction validated | Bound tag, file and bytecode sizes; require a nonzero identity and nonempty payload; reject unsupported tags/versions, truncation, and trailing data |
| Pack `*.dsop` | Derived opaque payload | `DSOP` v1 preserves one source file or GFF resource payload exactly without assigning it a meaning; source identity is held by the verified pack manifest | implemented; synthetic round-trip and owned 16,401-asset corpus extraction/read-back validated | Bound payload to 128 MiB; reject invalid magic/version, oversized length, truncation, and trailing data |
| `*.FLI` | Five numbered cinematics | Every file has a 128-byte little-endian `0xAF11` header, total-size equality, 320x200 8-bit canvas, flags `0x0003`, and raw frame/speed fields. From byte 128 to EOF, declared `0xF1FA` records cover each file exactly and number one more than its header frame count. FLI 1–4 also validate one nested declared-count/chunk-length envelope (with two total trailing bytes in FLI 1/2). FLI 5 rejects both that model and a one-byte physical-alignment alternative at its first/next-record boundary. `EXE-GOG-MEDIA-002` finds raw numbered-name/CINE-template strings but no direct reference, so file order, loader/fallback behavior, chunk semantics, palette changes, speed units, and playback timing remain unverified | high for the header/record envelope and FLI 1–4 nested bounds; high for the bounded string/reference result; unknown for FLI 5's inner variant and every playback role | Bound headers, frame records, chunks, decoded bytes, and timing; preserve the header/physical-record count distinction, validate variants independently, and reject unsupported chunk types explicitly; never treat filename order as a schedule |
| `*.VOC` | 147 speech and sound-effect files | Every file has the 26-byte `Creative Voice File` header with data offset 26, version `0x010a`, checksum `0x1129`, and first block type 1 whose declared size fits the file; raw first-block controls are time constants 165/210/131 and codec byte 0. `EXE-GOG-SOUND-002`/`006` find no complete header signature in either the loaded or complete physical sound-helper file, and `EXE-GOG-SOUND-008` finds it absent in the complete physical main executable. `EXE-GOG-SOUND-009` finds five raw main-executable `.VOC` fragments, but all are unreferenced non-instruction data. `EXE-GOG-SOUND-003` finds no `INT 15h` opcode; `EXE-GOG-SOUND-007` finds low-level port output but no file, decoder, device, rate, or timing owner. None establishes a native decoder or timing contract. Codec/sample-rate semantics, later blocks, and playback remain unverified | high for the common header/first-block envelope in the supported build; high for the bounded static results | Bound blocks and decoded samples; reject unsupported codecs and malformed terminators; do not infer standard-codec behavior, port emulation, filename mapping, or timing from the header alone |
| `MUSIC/*.ogg` | 40 GOG-supplied music files (`Track02` through `Track41`) | Ogg container presence and installed inventory are observed; mapping, loop points, provenance, and relationship to original media are unknown | high for the inventory; unknown for playback semantics | Validate stream metadata and decode limits; unknown track mapping remains data, not a guessed rule |
| `SOUND.CFG`, `SOUND.INI` | Sound card configuration and card list | Described by `FMT-CONFIG-001` and `FMT-CONFIG-002`; `SOUND_DS.EXE` reads `SOUND.INI` and writes `SOUND.CFG`, and the game reads `SOUND.CFG` (`FND-CONFIG-004`, `FND-CONFIG-005`) | see the spec entries | Require exact supported source fingerprint before a reader; bound every fixed field, identifier terminator/padding, and trailing size; do not infer Preferences mappings or audio behavior |
| `ITEMS.BIN` | Item translation table | Described by `FMT-ITEM-001`; the character transfer utility reads it (`RULE-ITEM-006`) | see the spec entries | Require exact supported source fingerprint before any reader; require exact length, parse only complete pairs, validate the documented unique-left/order envelope, and reject unsupported count/order forms without assigning item behavior |
| `game.gog` / `game.ins` | GOG disc-image payload and descriptor | Bounded Mode 2/2352 ISO 9660 inspection located the disc character archive for `FND-PARTY-005`; broader extractor relevance is unknown | medium for that bounded observation | Preserve as opaque DSOP; never mount or execute automatically. Parse only when a documented evidence question requires it, bound sectors/directories/extents, and keep original bytes outside Git |
| `DSUN.EXE` | Original DOS executable | Fingerprinted research oracle and bounded source for the Preferences text block, `FMT-TEXT-004`. Its `FBOV` overlay pack is described by `FMT-EXE-001` to `FMT-EXE-005`; the rebuild only checks its envelope and otherwise keeps the file as local opaque evidence | implemented for the three fixed-edition tables; unknown for executable semantics | Bound file size, MZ and `FBOV` envelope/table/payload ranges, fixed string-table offsets/counts/terminators/printable bytes, exact description span, and About control prefixes; never load or execute the opaque pack copy, or put it in Git |

The GFF container (`FMT-GFF-001`) is the first implemented format layer. Slice 2A now preserves
every fingerprinted source payload whether or not it has a dedicated reader;
unknown meaning is represented by DSOP rather than omitted. Every semantic
payload reader still requires synthetic fixtures and its own evidence before
production behavior uses it.

## Region maps

The region resources are `FMT-REGION-001` to `FMT-REGION-006`, and the terrain
drawing is `RULE-REGION-001`. The rebuild's reader caps the region name at 64
bytes and the entity table at 16,384 records, requires the exact map dimensions
and 16x16 tile frames, and rejects missing local tiles or external objects with
region/resource context. Required pack revision 35 serializes each
manifest-selected region independently as a source-derived
`regions/structural/rgnxxx.dsrg` DSRG v1 catalog, reads it back before
promotion, and assigns no selection, travel, camera, entity, or gameplay meaning
to that catalog.

## Object-frame definitions

The `OJFF` record is `FMT-ACTOR-001`, and `RDFF` is `FMT-ACTOR-002`, whose
layout is unknown. `GffObjectFrameCatalog` validates the complete catalog or a
bounded requested subset, caches shared images, keeps the words the spec leaves
unnamed as raw 16-bit values, and assigns no animation, draw-order, anchor,
collision, or interaction meaning.

## Static region composition

`RULE-REGION-001` draws the terrain and `RULE-ACTOR-001` the placed objects.
`RegionSceneRasterizer` overlays ETAB objects in stored order, each with the
first frame of its DSOB image at `entityX - objectXOffset` and
`entityY - objectYOffset - entityVerticalOffset`, mirrored when entity flag bit
`0x80` is set; the mirror reading is an open question of `RULE-ACTOR-001` and
`FMT-REGION-006`. It requires an explicit viewport origin within the 2048x1568
region and caps output at 4096 pixels per dimension and 16,777,216 pixels. It
does not select the opening camera, advance multi-frame images, interpret other
entity flags, or assign collision, depth, or interaction semantics.

## Indexed images and palettes

The image and palette layouts are `FMT-IMAGE-001` to `FMT-IMAGE-004`, and the
frame encodings `RULE-IMAGE-001` and `RULE-IMAGE-002`. The rebuild's reader caps
a payload at 64 MiB, a frame count and either dimension at 4,096, one frame at
16 Mi pixels, and all frames in an image at 64 Mi pixels. It widens each 6-bit
palette component as `FMT-IMAGE-004` describes.

`GPLDATA.GFF` contains no palette, so the derived dialogue portrait uses the
interface palette `PAL ` #1000. Which palette the original pairs with each image
is an open question of `FMT-IMAGE-001`.

## Indexed bitmap fonts and TEXT resources

The font layout is `FMT-TEXT-001` and `FMT-TEXT-002`, and the `TEXT` layout is
`FMT-TEXT-003`. The rebuild's font reader caps payloads at 1 MiB, height at 64,
width at 256, and total decoded pixels at 4 MiB. Its `TEXT` reader caps payloads
at 1 MiB, lines at 65,536, and each line at 4,096 bytes, and preserves empty
lines.

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

## Derived DSUI UI catalogs

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
trailing bytes. The pack contains a start-flow catalog with `WIND` #18501 and
#19500 through #19505 plus all 56 referenced controls, a separate Game Menu
graph for `WIND` #10500, and a destination graph containing `WIND` #11500 and
#13500 with all 86 and 89 referenced controls respectively. The latter two
share five evidenced navigation buttons while retaining their otherwise
uninterpreted controls. The Game Menu catalog retains all 30 ordered referenced
`BUTN`/`APFM` records.
Unknown source fields, button tails, palette assumptions, and unmeasured shell
placement semantics are not carried.

FND-PARTY-018 rejects the narrow direct byte and aligned little-endian word
ordinal mapping from the FND-PARTY-016/017 label-table order across the two
capture-confirmed records #40 and #42. It does not locate a field; all
uninterpreted fixed-header and tail bytes therefore remain outside DSCH.

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

## Derived DSRG region catalog

DSRG is an original deterministic pack format containing only the bounded
region subset. Version 1 begins with ASCII `DSRG`, a 16-bit version, the 32-bit
region number, fixed 16-bit map width/height/tile size values, 16-bit name and
tile counts, and a 32-bit entity count. The printable ASCII name is followed by
256 decoded RGB triples, the exact terrain and geometry planes, then tiles in
ascending byte resource-number order. Each tile stores its number, 256 palette
indices, and 256 binary-alpha bytes. Ordered entity records retain the original
two signed coordinates, signed vertical offset, flags byte, and signed object
resource number.

The reader caps files at 2 MiB, names at 63 bytes, tiles at 256, and entities at
16,384. It requires the 128x98 planes, 16x16 tile geometry, canonical unique tile
order, binary alpha, complete map-to-tile references, and an exact computed file
length. DSRG carries no GFF offsets or uninterpreted container bytes. Version 1
does not assign collision meaning to geometry values or behavior to entity
flags and object references.

## Derived DSOB object-frame catalog

DSOB is an original deterministic pack format containing the object definitions
referenced by one extracted region and their deduplicated decoded images. Version
1 begins with ASCII `DSOB`, a 16-bit version, and 32-bit definition/image counts.
Definitions follow in ascending resource-number order as the resource number,
four preserved raw 16-bit words, signed X/Y offsets, and a 32-bit image resource
number. Images follow in ascending resource-number order with a 16-bit frame
count; each frame stores 16-bit dimensions, a 32-bit pixel count, palette-index
bytes, and equally sized binary-alpha bytes. The companion DSRG palette supplies
colors, so DSOB does not duplicate it.

The reader caps files at 64 MiB, definitions/images at 65,536 each, total frames
at 65,536, and total decoded pixels at 32 MiB. It requires canonical unique
ordering, valid nonempty frames, exact dimensions and lengths, binary alpha,
every definition-to-image reference to resolve, every stored image to be used,
and no trailing data. Version 1 deliberately assigns no meaning to the four raw
words or to animation, draw order, anchoring, collision, and interaction.

## GPL scripts and derived DSGP assets

**Evidence:** `OBS-GOG-DIALOGUE-001`.

`GPLDATA.GFF` holds the `GPL ` and `MAS ` scripts (`FMT-SCRIPT-001`). `GPL` #135
is 4,124 bytes and is independently tied to the first captured Tyr conversation.
MAS #99 initializes global string #5 for the first two menus' exit label and
global string #6 for the third menu's exit label.
The extractor preserves both complete bytecode chunks as opaque; runtime readers now
project only the bounded dialogue structures described below, beginning with
the evidenced opening at offsets 16, 118, 199, and 253:
portrait selection, two conditional speech sources, and the initial menu, plus
MAS #99's string-copy assignments at offsets 20 and 66. A second GPL #135 projection
validates opening choice 7's target at 2905: print GSTRING #5, assign immediate
one to local flags 14 and 4, and return locally at 2920.
Another bounded projection validates choice 0 at offsets 1017-1147: three
compressed-literal prints with two intervening newline opcodes, immediate zero
to local flag 0, and a local return. This establishes the response output and
menu-return effects without embedding its text in Git.
The same projection reader validates choice 2 at offsets 1148-1182 and choice
3 at 1183-1231. Each branch contains one compressed-literal print, an immediate
zero assignment to its matching local flag, a word increment of local number 0,
and a local return.
Choice 4 is bounded at offsets 1232-1394: three compressed-literal prints,
immediate one to local flag 9, immediate zero to local number 0, and a local
return. The post-menu block at offsets 547-740 reads local flags 1, 2, 3, and 9,
derives local flags 4 and 5, decides whether to repeat the opening menu, and,
when advancing, derives local
flags 1, 6, 7, and 8 from global flag 357. The following seven-entry menu starts
at offset 750 and uses local flags 1, 6, 10, 7, 11, and 8 plus constant exit
choice 6 targeting the same completion branch at 2905.
Choice 1 is bounded separately at offsets 1597-1824: three compressed-literal
prints, immediate zero to local flag 1, an extended global flag 357 equals-zero
condition guarding immediate-one assignments to local flags 6 and 7, immediate
one to global flag 357, and a local return.
Second-menu target 1825 is bounded through its return at 1995: three
compressed-literal prints, immediate zero to local flag 6, a local flag 16
equals-zero condition guarding immediate one to local flag 10, and a local
return.
The newly enabled target 3479 is bounded through its return at 3685: three
compressed-literal prints, immediate one to local flag 16, immediate zero to
local flag 10, and a local return.
Target 1996 is bounded across both global-number-22 paths. When it equals one,
two compressed-literal prints precede immediate one to local flag 11. Otherwise
global number 84 bit 2 selects one of two lead-in prints, two common prints
follow, and global number 84 is updated with bit 1. Both paths join at immediate
zero to local flag 7 and local return at 2351.
Target 2352 is bounded through local return at 2414: one compressed-literal
print followed by immediate zero to local flag 11.
Target 2415 is bounded through the third-menu entry at 2616: three
compressed-literal prints, immediate one to local flags 12 and 13, a local flag
16 equals-zero condition guarding immediate one to local flag 10, and the
local-flag-8 loop header. The following seven-entry menu uses local flags 12,
13, 15, 10, 17, and 18 plus a constant source choice 6 targeting offset 3976.
Its source-choice-0 target at 2921 is bounded through local return at 3088:
three compressed-literal prints followed by immediate zero to local flag 12.
Target 3089 is bounded through local return at 3256: three compressed-literal
prints, immediate one to local flag 15, and immediate zero to local flag 13.
The newly enabled target 3257 is bounded through local return at 3478: four
compressed-literal prints followed by immediate zero to local flag 15.
Third-menu target 3686 is bounded through local return at 3785: two
compressed-literal prints, immediate zero to local flag 17, and immediate one
to local flag 18. Its newly enabled target 3786 is bounded through local return
at 3975: three compressed-literal prints and immediate zero to local flag 18.
The constant third-menu target 3976 is bounded through local return at 4048.
It clears local flag 8, calls a helper at 4049 that sets local flag 14 only when
local flags 1/6/7/11/10/8 are all zero after that clear, and selects either a
two-part completion output (including helper 4082) or a one-part early output.
Both helpers, both branch targets, and their local returns through offset 4123
are structurally validated.

`GplPackedString` reads the strings of `FMT-SCRIPT-002`: marker `0x01`
stands for the active character's name, and marker `0x05` begins a string of
7-bit characters that `0x03` ends. It stops at 1,024 characters and rejects
marker `0x02`, other markers, truncation and a missing terminator. The menu projection bounds choices to 24, accepts only the expression
forms needed by this observed menu, captures labels and branch offsets, and
projects its three observed condition shapes: constant, local flag, and local
number equality. It fails closed on any other condition shape, drift, or
truncation. The MAS projections additionally require the assignments to end at
offsets 33 and 94 and target type-6 string slots 5 and 6. The completion projection accepts
only its exact immediate/short-variable forms and rejects opcode, operand,
offset, or truncation drift. The choice 0 response projection likewise requires
the exact print destinations, instruction boundaries, flag target/value,
choice 2/3 counter target, choice 4's flag/number assignments, the exact
post-menu control flow and second-menu conditions, choice 1's exact extended
global reference and branch target, target 1825's local condition and assignment,
target 3479's two flag assignments, target 1996's equality/bitmask paths, target
2352's flag clear, target 2415's assignments/condition/loop bridge, target
2921's flag clear, targets 3089/3257 and 3686/3786's flag transitions, and each
applicable local return. Target 3976 additionally requires its subroutine calls,
post-assignment six-flag condition, conditional output branches, and completion
return boundaries. Decoding these structures does not
authorize executing other
instructions or assigning condition and state-mutation semantics.

DSGP is an original deterministic envelope that preserves a selected script
without teaching the runtime to parse GFF. Version 1 consists of ASCII `DSGP`,
a 16-bit version, a 32-bit source resource number, a 32-bit payload length, and
the byte-identical source payload. The reader caps bytecode at 1 MiB, requires a
nonzero resource number and nonempty payload, and rejects truncation or trailing
data. No original dialogue text or executable interpretation is added to Git.

## Derived DSOP opaque-payload asset

DSOP is an original deterministic local-pack envelope used by the full-corpus
extraction gate. Version 1 consists of ASCII `DSOP`, a 16-bit version, a 32-bit
payload length, and byte-identical source bytes. Resource identity, source file,
XXH3-128, size, and conversion description belong to the pack manifest, so DSOP
does not infer a tag, format, or runtime role. The reader accepts at most 128
MiB and rejects invalid magic/version, oversized declarations, truncation, and
trailing data. It preserves content for a licensed local pack only; it does not
authorize the Game to load or execute opaque source bytes.

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

Pack format 26 also uses DSTX for `text/preferences.dstx`, taken from the
executable block `FMT-TEXT-004`. Resource 0 contains
the four executable-backed difficulty labels in table order; resource 1 contains
the nine About lines after the reader validates and removes each leading
three-control centering prefix; resource 2 contains the ten contiguous
Preferences descriptions. The source offsets are edition-specific and are
accepted only after the executable fingerprint matches the supported manifest;
the reader also requires the description table to end exactly at the About
table boundary.
