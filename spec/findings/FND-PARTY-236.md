---
id: FND-PARTY-236
title: A numbered save writes the whole 400-record kind-1 table as one SAVE block, word +0C included, and the numbered restore reads it back and then sets +0C to zero in every record
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00072FF7..0x00073042
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000732E9..0x00073305
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070B4A..0x00070BFD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070764..0x00070822
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000708A4..0x00070964
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000709A8..0x000709B1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070AE7..0x00070B0B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00044598..0x0004459C
    kind: file-data
tool: tools/research/exec-census/overlay_listing.py, segment_references.py and immediate_search.py at commit 77c23c19; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 187's code
starts at file 0x6FE30 and overlay 188's at 0x72EA0.

### The table

Overlay 188 +0157 requests 0x23F0 (9,200) bytes through 444C:0008
with the double words 1 and 9,200 and stores the far pointer at
DS:19C1 (+0168, +016C); a null pointer leaves through +03B1. For
records 0 to 398 it stores record + 1 in the word at +04 of each
23-byte record (+0176..+018F), and 9999 (0x270F) in the word at
+04 of record 399, at byte 0x23DD (+0195). It writes no other
record word. It then copies the pointer to DS:6153 (+01A2), where
kind 1 finds its records (FND-PARTY-051). Overlay 188 +0449
releases it through 444C:0092 when nonzero and stores the result
there. An operand search for 0x19C1 and 0x19C3 finds no other store
to the pointer: every other hit reads it.

### The save

Overlay 187 +0D1A copies the far pointers at DS:19C9, DS:19C5,
DS:19C1, DS:67B7, DS:55B8 and DS:19D1 to entries 4 to 9 of the
source table at DS:157E, so entry 6 (DS:1596) holds the kind-1
table's pointer; the shipped entry 6 is zero. It is the only store to
DS:1596 an operand search for 0x1596 finds. The numbered-save writer
+08E3 calls +0D1A at +0915 before its loop. Its loop (+0934..+09E7)
runs index s from 0 below 17, or 61 for selector 0 (FND-COMBAT-034),
and for a nonnull entry s it removes and then writes resource
selector * 61 + s + 1 under the tag `SAVE` through 37FC:00E8 with
the source pointer and the double-word length at 4F38:0000 + 4s.
For entry 6 that is resource selector * 61 + 7.

The shipped length at 4F38:0018 (file 0x44598) is 0x000023F0, the
9,200 bytes of the table. A census of segment words naming segment
4F38 (MZ relocations and FBOV fixups) finds three: the segment
table's own paragraph word at 561F:0008 and the two in the writer,
+095C, which stores `word DS:264E * 8` only into the entry whose
source pointer equals DS:67B7, and +09BE, which reads the length.
So the writer saves the 400 records whole, the word at +0C of each
included, unless the kind-1 pointer equals the one at DS:67B7.

### The restore

The numbered restore +0A6C also calls +0D1A (+0AA2) before its loop,
which runs the same indexes (+0AB2..+0B32). For a nonnull entry it
passes the address of a local copy of the pointer to 38FF:04AB with
`SAVE` and selector * 61 + s + 1, so the complete resource is read
into the existing table (FND-COMBAT-035); any nonzero result clears
the local success byte and ends the loop. After the loop, on both
the success path (+0B7C, through the per-object loop) and the failure
path (+0B7E), it reaches +0CB7, calls +1829, and at +0CC1..+0CD9 stores
0 in the word at +0C of each of the 400 records, index 0 to 399 times
23 from the pointer at DS:19C1, before its call of 2D40:2196.

## Interpretation

A numbered save keeps the whole kind-1 record table, the 400 23-byte
attached records with their free chain, as `SAVE` resource selector
* 61 + 7, and a numbered restore reads it back over the table. The
cached bitmap number at +0C travels in that block but never survives
the restore: the restore sets it to zero in every record, used or
free, whether the block was read or the loop stopped early. After a
numbered load every record's +0C is zero, as after the CHAR load's
kind-1 copy (FND-PARTY-051, FND-PARTY-235), and the drawing path
resolves the picture again (FND-PARTY-224).

## Alternatives

- Reading the SAVE blocks as never covering the kind-1 table fails:
  entry 6 holds its pointer when the writer runs, and its shipped
  length equals the table's allocation.
- Reading a restored +0C as reaching the drawing path fails: the
  clear at +0CC1 follows the restore loop on both of its exits, with
  no branch past it.
- The length of entry 6 is a fixed 9,200 only while the kind-1 pointer
  differs from DS:67B7's; the two come from separate allocations, but
  their inequality is not shown here. A write to segment 4F38 through
  a computed segment is outside the census.
- Whether the allocation in overlay 188 leaves +0C zero before the
  first use depends on what 1000:18D0, which 444C:0008 calls,
  initializes (FND-CONFIG-209); it is not read here. Q-PARTY-091 keeps
  the allocation and copy paths.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and the locked evidence Python. List
overlay 187 file 70713..70B4A and 70B4A..70D60, and overlay 188 file
72F40..730B0 and 732C0..73310, with overlay_listing.py. Read the
double words of the load image at 4F38:0000 (file 0x44580, 61 entries)
and 57E0:157E (file 0x4E57E). Run segment_references.py for 4F38 and
immediate_search.py for 1596, 19C1 and 19C3, keeping the hits whose
destination operand names the word; the stores at overlay 188 +0168
and overlay 187 +0D31 are the positive controls. Retained listings are
under GAME_DIR/analysis/party-save-blocks-20261018/. No original
function or game process was run.
