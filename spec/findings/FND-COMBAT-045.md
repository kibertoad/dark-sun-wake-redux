---
id: FND-COMBAT-045
title: The nominal effect-table boundary lies inside resident storage and begins at the owner-flag base
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004B3F0..0x0004B3F8
tool: tools/evidence/report.mjs table; FMT-EXE-002 segment-span mapping and existing effect-row findings
environment: null
---

## Observation

The bounded, hash-checked segment-table read identifies descriptor 110
as segment 3E71 relative to the load image, flags zero, start offset
zero and exclusive end offset 0C6D. FMT-EXE-002 and FND-EXE-571
describe those fields and their resident-load-image mapping. At the
project's modeled load segment 1000, this is 4E71:0000..0C6D.
With executable header size 5200, its file span is 43910..4457D.
The descriptor itself is file 4B3F0..4B3F8. These are hexadecimal,
half-open bounds; the resident span contains 3181 bytes.

FND-COMBAT-029/031/032 identify the effect table's base 4E71:024D
and ten-byte stride, the pre-count boundary owner reads, and removal's
copy through the row at old count n. FND-COMBAT-034 independently
identifies the shipped save source as that same base with length 1920.
The following address arithmetic compares those existing contracts
with the descriptor; it does not infer a runtime count invariant from
the file's length.

| Conditional span | Resident offsets | Relation to descriptor |
|---|---|---|
| 192 nominal ten-byte rows | 024D..09CD | Entirely inside 0000..0C6D. |
| Owner word read at row index 192 | 09CD..09CF | Entirely inside the same span. |
| Complete boundary row copied at index 192 | 09CD..09D7 | Entirely inside the same span. |

For admitted 0 <= n <= 192, the complete boundary-row exclusive end
024D + 10*(n+1) is at most 09D7. None of those offset calculations
wraps a word or crosses the resident descriptor's end. The full
ten-byte case also covers the weaker two-byte owner-read backing
obligation. At n=192 that ten-byte file range is 442DD..442E7.

Under the stable resident mapping, this supplies nominal byte backing
for the collector/key-search boundary reads and the removal/owner-
transfer copy source described in FND-COMBAT-032/033. It does not
establish the stored count is always in that domain, the supplied
indices are admitted, all segment/pointer state remains unchanged,
or arbitrary restored lengths fit the destination.

### Backing does not reserve a sentinel record

The nominal 192-row span ends exactly at offset 09CD. The separate
flag addressing in FND-COMBAT-039 uses 4E71:09CD + 34*owner + offset;
in particular, admitted owner zero and byte offset zero address that
same first boundary byte. The resident descriptor therefore does not
identify these ten bytes as a dedicated extra effect row, prescribe
their contents or establish a sentinel owner value.

The source span requested by the save writer's shipped 1920-byte
length also ends at 09CD and excludes the following boundary bytes.
FND-COMBAT-035 records that restoration instead requests a complete
selected resource into the supplied pointer, without using that
shipped writer length as a destination-capacity argument. Neither the
descriptor nor this comparison closes restoration admission.

## Interpretation

This answers Q-COMBAT-009's nominal resident backing question for
effect counts 0..192. The previously identified extra reads are
within the installed resident span in that bounded case; they do
not require a hypothetical unmapped row or a special sentinel value.
The logical row capacity and all runtime count/ID/order/lifetime
conditions remain separate questions.

The finding introduces no general allowance for larger counts or
insertion beyond the wrapper's nominal limit. It does not establish
boundary contents, runtime immutability, successful restoration or
native presentation. No original function was executed.

## Alternatives

- Treating the 1920-byte save source's end as the resident segment's
  end confuses one requested source span with descriptor 110's extent.
- Treating resident bytes as an extra reserved effect row ignores the
  separate flag addressing beginning at the same offset.
- Treating descriptor backing as proof of a global count bound ignores
  unchecked mutation entries and the restore-reader contract.

## How to reproduce

Use the supported DSUN.EXE, size 634416 and XXH3
e296af55ba2ecde7e77f555c90f33d0b, with the locked evidence tooling.
Run report.mjs table for the FMT-EXE-002 table at hexadecimal 4B080,
count/limit 229 and stride eight, with width-two fields segment at
zero, end_offset at two, flags at four and start_offset at six.
FMT-EXE-002/FND-EXE-571 supply the count and field evidence. Select
the row whose segment is 3E71 and verify it is index 110. Alternatively
query just its eight-byte descriptor with count/limit one.

Apply the load/header mapping above and compare 024D + 192*10 and
024D + 193*10 with the exclusive descriptor end. Follow the existing
row read/move contracts and flag expression rather than inferring
sentinel contents from bytes. Local effect-storage-descriptors
configuration/report files remain in GAME_DIR/analysis/combat-effects/.
No original bytes, listing or extracted flag data is committed.
