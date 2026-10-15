---
id: FND-COMBAT-034
title: Effect rows, the neighboring three-byte table and effect count have separate numbered-save sources
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070713..0x00070822
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F809..0x0007F855
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F915..0x0007F95F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00080140..0x00080171
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004E5DA..0x0004E5E6
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000445DC..0x000445E8
    kind: file-data
tool: tools/research/exec-census/segment_references.py, direct_callers.py and overlay_listing.py; tools/evidence/report.mjs table; installed-build fingerprint and relocation/fixup checks
environment: null
---

## Observation

The relocation/fixup reference query for mapped resident segment 50B0
finds the independently known classifier segment operand at overlay
193 +1E5D (FND-COMBAT-029), several other overlay-193 operands, and two
resident data relocation sites. The query searches MZ relocation words
and FBOV fixup words and omits direct far-call/jump patterns. Its
bounded context decodes are candidates, not proof that every displayed
instruction is reached. Computed segments, alternate physical aliases,
unrelocated constants and bulk writes remain outside its search model.

The resident data site at file 4E5E0 is the segment half of a far
pointer whose offset word is 000F. It therefore names 50B0:000F after
MZ relocation, not 50B0:0000. Its owner is the source-pointer table
consumed by the numbered-save writer in overlay 187 +08E3, previously
located by FND-SAVE-008.

### Three explicit save sources

On the writer's reached loop, index s selects a far source from
DS:157E + 4*s and a 32-bit byte length from 4F38:0000 + 4*s. A nonzero
source leads to the remove/write requests. The requested resource
number is selector*61 + s + 1 under tag SAVE. Selector zero admits
61 iterations; other selectors use 17. These are requested ranges,
not an assertion that every write succeeds.

Reading source entries 23..25 and their shipped lengths gives:

| Index | SAVE number for selector zero | Relocated source | Shipped length |
|---|---|---|---|
| 23 | 24 | 4E71:024D | 1920 bytes. |
| 24 | 25 | 50B0:000F | 252 bytes. |
| 25 | 26 | 57E0:2100 | 2 bytes. |

The three source records are at file 4E5DA..4E5E6; their segment words
are MZ relocation sites. The three initial lengths are at
445DC..445E8. They are not inferred from divisibility or neighboring
symbols. The writer actually indexes the corresponding source and
length arrays before forwarding them to 37FC:00E8.

The length array is writable. This writer calls preparation routines
before the loop and contains a conditional update for a source equal
to the global far pointer at DS:67B7. This finding records the shipped
values and indexed request route only; it does not admit those values
as unchanged for every runtime save, or prove successful persistence.
The reader/restoration route is not covered by this writer reading.

### The neighboring table starts after the category prefix

For admitted effect IDs 0..112, the classifier reads the fifteen-byte
prefix 50B0:0000..000F. The SAVE/25 source begins at its exclusive end.
With its shipped length it covers 000F..010B, not the prefix. This
particular source entry therefore does not designate the classifier
bytes as its payload. No absence claim about other source entries or
runtime aliases follows from that boundary alone.

Two exported local producers also use the later part of this segment:

- Overlay 193 +1519, trampoline 573B:00CA, calls +15B9 with owner and
  byte argument. Only a returned word FFFF permits its append test.
  It then requires signed DS:2102 < 84, writes an owner word at
  000F + 3*count and the low byte argument at 0011 + 3*count, and
  increments that count. There is no local nonnegative-count check.
  The lookup's return/effects and global count admission remain separate.
- Overlay 193 +1625, trampoline 573B:00E3, walks signed indices from
  zero below DS:2102.
  For a matching owner word at 000F + 3*i, it replaces the byte at
  0011 + 3*i with -(old_byte + 2), modulo 256. Other owners are skipped.
  It does not access the classifier prefix on admitted indices 0..83.

For a valid neighboring-table count 0..84, these row positions lie
within the 252-byte shipped source span. This is a conditional address
separation, not a global count proof. Other segment-reference candidates
include compaction and byte updates and remain to be qualified with
their callers, counts and effects. The data relocation at file 4B430
also remains unclassified by this finding.

## Interpretation

This answers the explicit source identity and shipped length of three
adjacent numbered-save entries relevant to Q-COMBAT-009 and
Q-COMBAT-011. Effect rows, the neighboring three-byte records and the
effect-count word are separate source entries. The three-byte table's
local count is DS:2102, distinct from effect count DS:2100.

It also removes one false equivalence from bitmap-writer research:
using segment 50B0 does not itself mean a write changes category bits.
The qualified append and transform sites address the subsequent table
under the stated count domain. This does not establish the category
prefix as immutable, close all writers, or establish restore-time
lengths and destination backing. In particular, 1920 shipped save bytes
do not by themselves prove the extra boundary-row storage required by
FND-COMBAT-032/033.

## Alternatives

- Treating the source relocation as a pointer to offset zero drops its
  actual 000F offset and conflates the classifier with the later table.
- The matching arithmetic 84*3 = 252 agrees with the append guard, but
  the recorded length comes directly from the indexed length array.
- A save-source pointer is not a restore-destination contract. Neither
  it nor a shipped byte count proves all later buffers or loaded counts.
- The local signed upper-bound append test does not reject negative
  counts. No original run with such counts was attempted or implied.

## How to reproduce

Use the locked evidence Python and fingerprinted installed DSUN.EXE.
Run segment_references.py for 50B0, 4E71 and 57E0 and retain the independently located
classifier operand as a positive overlay-fixup control. The two resident
data sites are not reached code; do not accept their automatic nearby
decodes as instructions. Read overlay_listing.py descriptor 187,
70713..70822, and descriptor 193, 7F809..7F855, 7F915..7F95F and
80140..80171. direct_callers.py for 193+1519 and 193+1625 reports their
trampolines. The resident data sites 4E5DC, 4E5E0 and 4E5E4 verify the
three source pointers' segment relocations.

Use report.mjs table with the build XXH3 to read three records from
4E5DA, stride four, fields offset/segment of width two at offsets zero
and two; separately read three width-four lengths at 445DC, stride
four. Both queries have count and limit three. Derive the first index
as (15DA - 157E)/4 = 23, with MZ header 5200 and mapped DS 57E0.
Interpret the source segment words through their MZ relocations; do
not add the load segment to offsets or lengths.

Configurations remain in GAME_DIR/analysis/combat-effects/ as
save-sources-24-26.json and save-lengths-24-26.json. Reports and
original-derived listings stay there too. No original code was run.
