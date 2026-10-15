---
id: FND-COMBAT-041
title: The effect-slot background requests cache index eight sourced from BMP/13007
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007B4F8..0x0007B585
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007B827..0x0007B8AD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008A7C5..0x0008A7DB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004EC98..0x0004ECC2
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:1954..1425:1A03
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:1B0B..1425:1BBA
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00046822..0x000473DE
tool: tools/research/exec-census/immediate_search.py, direct_callers.py, overlay_listing.py and resident_listing.py; tools/evidence/report.mjs table; DarkSunWakeRedux.Inspect gff and image-catalog; installed-build fingerprints and relocation/fixup checks
environment: null
---

## Observation

The effect-window setup pushes word eight to 571F:00A2 at overlay
202 +0327, then stores returned DX:AX at 4E71:002F. This is the
image pointer used by the background requests in FND-COMBAT-038/039.
The exported trampoline enters overlay 190 +34E7, a cache retrieval
path, rather than a direct ICON-resource request.

The immediate/displacement candidate query for 459E and 61AB recovers
that known retrieval as a control and finds the fill producer in
overlay 190 +31B8. Direct-call/trampoline checks identify +31B8 as
571F:00AC and +34E7 as 571F:00A2. The candidate query alone does not
cover computed/aliased references or prove all cache writers.

### The first source group supplies item eight

The fill entry initializes a 32-bit accumulated offset and a local
resource pointer to zero. Its first loop runs indices 0..20. It reads
a word resource number from DS:1C98 + 2*i, zero-extends that number,
and requests tag BMP with the address of its local pointer through
38FF:04AB.

Only a zero returned AX enters that loop's write/offset branch. It
reads the resource's leading double-word size, supplies its low word
as transfer count to 1425:1B0B, and passes the resource pointer,
current accumulated offset and two-word cache handle at DS:459E.
The caller does not test the transfer's returned AX before recording
the current offset in DS:61AB + 4*i and advancing its accumulator by
the full 32-bit resource size. It then requests release through
444C:0092 and stores returned DX:AX into its local resource pointer.

A failed resource request skips that write/offset/release branch.
The local pointer is initialized once before the loop, not reset at
the top of every iteration. The reader's assigned-before-error path
from FND-CONFIG-151 and the release dependencies from FND-CONFIG-165
therefore remain relevant; success and complete buffer state are not
inferred from the loop merely advancing.

The 21 word numbers are shipped at executable file 4EC98..4ECC2,
mapped DS:1C98 with MZ header 5200. The bounded read gives resource
13007 at index eight and 13008 at the following index. Thus the
qualified source producer for the setup's item eight requests
BMP/13007. Under admitted successful adjacent source acquisitions,
stable sizes and transfer/release effects, the next offset minus the
item-eight offset describes that resource's byte span. This is not
proof of a populated runtime cache after an earlier failure.

The remaining fill phases are outside this finding's claim. This
reading does not establish all source groups, all offset entries,
cache-handle creation or a persisted cache format.

### Retrieval has a separate count and failure contract

Entry +34E7 rejects an unsigned index at least 49 and otherwise
computes a 32-bit byte count as offsets[index+1] - offsets[index].
It passes count one and that computed byte count as the two
double-word arguments to 444C:0008. A null returned pointer exits
with DX:AX zero.

For a nonnull allocation, it supplies the low word of the computed
count, that destination pointer, the full current offset and DS:459E
to 1425:1954. It tests full returned AX. Nonzero requests release and
returns a null far pointer; zero returns the allocated pointer.
The index guard does not establish initialized/ordered offsets or
the relation between the full allocation count and low-word transfer
count. Those require admitted cache state.

The two resident transfer prefixes consume a word count at BP+12
hexadecimal, a two-word handle whose low word is sent to the local
selector and whose high word is checked against the selected record,
and a full double-word offset. The source/destination far pointer and
offset occupy opposite argument positions in the two calls. These
prefixes confirm the caller's widths and distinguish the cache handle
from the returned ordinary image pointer. They are not a complete
cache-backend or transfer-success contract; their dispatch, handle,
buffer and range dependencies remain open.

### Installed background frames match the requested indices

The supported RESOURCE.GFF directory locates BMP/13007 at decimal
288802 with 3004 bytes. A hash-checked fixed-header read gives size
3004 and frame_count 23. The approved image-catalog reader reports
all 23 frames as 18 by 18. RESOURCE.GFF's complete identity is
XXH3 92fe031616f36062f99484b0f728210e, size 5724669.

The caller-side background frame selections already read in the
effect paths are therefore inside this installed resource's frame
table:

| Local path | Background frame index |
|---|---|
| Effect-icon construction, FND-COMBAT-038. | 2. |
| Frame handler event value 2, FND-COMBAT-039. | 8. |
| Frame handler event values 4 and 32, FND-COMBAT-039. | 5. |

Event values are decimal; frame indices are zero-based. These local
arguments do not identify physical input meanings or prove what the
frame pixels show. FND-COMBAT-040 records the corresponding 18-by-18
APFM slots, while the referenced effect ICON frames are 16 by 16.
The metadata is consistent with the separate background and +1-offset
ICON draw requests. Native archive/cache identity, palette, geometry,
backend results and visible states remain separate dependencies.

For this installed source, 3004 fits the transfer word. If the
runtime adjacent offset difference and bytes match that resource,
the full allocation size and low transfer word agree. No general
truncation policy, failed-fill state or arbitrary index input is
admitted from this one bounded resource case.

## Interpretation

This answers the concrete background-image producer and installed
frame-metadata part of Q-COMBAT-010. The backdrop is sourced through
cache item eight's BMP/13007 request, whereas the effect's ICON number
has the separate reader path in FND-COMBAT-038. The stored window
resource's image field zero does not describe this manually supplied
background-image producer.

The finding does not establish actual successful copying into and
out of the runtime cache. Q-COMBAT-010 retains cache handle/offset
writers and initialization, transfer and pointer validity, active
resource selection, release effects and native backend/palette outcomes.
No reader, cache schema or new draw behavior was implemented.

## Alternatives

- Treating the setup's word eight as an ICON number skips the actual
  cache-index lookup and its source-number table.
- Resource presence and valid frame indices do not prove that a runtime
  fill succeeded. The offset store follows an unchecked cache-write
  request and is skipped on a resource-read error.
- The retrieval guard's 49 entries are not the first source group's
  21-entry bound, nor do they prove all 50 offset boundaries initialized.
- The draw arguments and metadata do not establish a pressed/hover
  appearance or animation. Q-UI-001 retains native event meanings.

## How to reproduce

Use the locked evidence environment and supported DSUN.EXE/RESOURCE.GFF.
Run immediate_search.py for 459E and 61AB with the known retrieval
as control, and direct_callers.py for 190+31B8 and 190+34E7.
Read overlay_listing.py descriptor 190, 7B4F8..7B585 and
7B827..7B8AD, and descriptor 202, 8A7C5..8A7DB. Read
resident_listing.py ranges 1425:1954..1425:1A03 and
1425:1B0B..1425:1BBA. Bounds are hexadecimal and half-open.

Use report.mjs table with the executable XXH3
e296af55ba2ecde7e77f555c90f33d0b, start 4EC98 hexadecimal,
count/limit 21, stride two and a width-two resource_number field.
The loop's explicit 21 iterations supply count evidence. Inspect
gff/image-catalog on the owned archive supplies BMP/13007's directory
and frame records. Independently read its six-byte header with the
archive XXH3 above, decimal start 288802, count/limit one, stride six,
and width-four size at zero and width-two frame_count at four.

Check the error branches before offset stores, full/low-word count
uses, following-offset dependency and caller frame arguments. Local
configurations slot-background-source-ids and slot-background-header,
and original-derived catalog/reports, stay in
GAME_DIR/analysis/combat-effects/. No original function was executed.
