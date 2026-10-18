---
id: FND-PARTY-247
title: Attached-record word 0x08 heads the items stored inside a Chest, reached only through the generic object-graph walk, which keeps them out of the inventory map
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0001FF50..0x0001FF51
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0002031E..0x00020320
tool: tools/research/exec-census/record_base_accesses.py, field_reads.py, direct_callers.py and char_item_records.py at commit 5151d3a2; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

A kind-1 record is the 23-byte record at the far pointer `DS:19C1` plus 23 times its number
(FND-PARTY-051). The shipped `FNFO` 1 word for kind-1 field 4 (`OBJEX.GFF` `0x1FF50`) is 8, and
the `FNFO` 2 type bytes, which the game reads to `DS:60ED` (FND-CONFIG-153), are 7 for field 4
and 8 for field 2 (`0x20320`, `0x2031E`), so field 4 is the word at `0x08` and field 2 the chain
link at `0x04` (FND-PARTY-052, FND-PARTY-216).

**Shipped records.** FND-PARTY-196 finds the word `0x08` not 9999 in three records of
`CHARSAVE.GFF`, of objects 2212, 2211 and 2212, each with the item type 19 at `0x0A` and the name
number 102 at `0x14`. Entry 102 of the item-name table `DS:56F3` is line 101 of `TEXT/1000`
(FND-TEXT-008), `Chest`. In each case a type-2 chunk names field 4 and the chest's chunk as its
target: `CHAR/30` chunk 23 (object 31972), followed by the type-4 chunks 24 to 28 chained through
field 2, and `CHAR/33` chunk 20 (object 761) and chunk 21 (object 30042), followed by the type-4
chunks 22 to 25 (`char_item_records.py`). The chests hold 12 records in all.

**Direct accesses.** `record_base_accesses.py` lists no ES access at displacement 8 or 9 whose
base was loaded from `DS:19C1`, `DS:19C3`, `DS:6153` or `DS:6155`; for displacement 6 it lists 19
accesses through `DS:19C1`, the positive control. `field_reads.py --es --table 19c1 8` finds no
read in the inventory or at any other byte of the load image and overlays (161 output lines for
displacement 6). The field accessors `1AA0:0009` and `1AA0:012A` have 14 direct call sites each
(`direct_callers.py`); none pushes the literal field number 4 as the field argument, the word
at `[bp+8]` whose low byte `1AA0:012A` passes to `2D40:06C4` and with which it indexes
`DS:60ED`. Those with a literal field pass 0, 1, 2, 9, 10 or 77; the others pass a field number
known only at run time: a graph entry's word `+6`, the walk's field list, a caller's argument or
a register. Overlay 204's script field getter and setter accept the kind-1 keys 0, 1, 3, 7, 8
and 10 only (FND-PARTY-245).

**The walk.** `1AA0:0566` visits, for kind 1, the fields 4, 2 and 5 (FND-PARTY-054), so
`2D40:2196` makes an entry for an item held at `0x08`, with word `+6` 4 and the chest's entry as
its parent (FND-PARTY-218), and then entries for the records chained after it with word `+6` 2.

**The inventory map.** Overlay 189 `+1688` places each kind-1 entry of a character's graph at
its position byte `0x13` plus 4 and, for a result from 1 to 29, calls `27E5:01F3`, which follows
parents while the entry's field has the type byte 8 and returns the parent of the entry where it
stops; when that is not 0 it writes 0xFF to the record's byte `0x13` and leaves the map word
alone (FND-PARTY-217). An item reached through a character's field 16, 17 or 4 stops at a head
whose parent is the root entry 0. An item inside a chest stops at the head the chest's `0x08`
names, whose field 4 has the type byte 7, and its parent is the chest's entry, which is not 0.
In the shipped chests, `CHAR/33`'s six content records hold 0xFF at `0x13`, and `CHAR/30`'s six
hold 38 to 43, which the redraw turns into 42 to 47, the six map words of its third 0xFFFF run
and the range of its separate branch (FND-PARTY-217).

## Interpretation

The word at `0x08` of a 23-byte record is the head of the chain of items inside that item, used
by the shipped Chest items. No instruction found names the field directly: the game reaches the
contents through the generic object graph, so they travel with the chest in every graph walk (the
`CHAR` writer, placement and the inventory redraw). The inventory screen does not show them in
the character's grid: for a position from 1 to 29 the redraw sets their position byte to -1 and
leaves the slot to items carried directly.

## Alternatives

- Which action opens a chest, shows its contents or moves an item into or out of it was not
  found. The setter calls that take a field from a graph entry (`2D40:288C` and its neighbours)
  can write field 4 of a kind-1 record when an entry's word `+6` is 4; their callers were not
  read.
- The accessor calls whose field is a caller's argument or a register (overlay 188 `+0764` and
  `+079D` among them) were not bounded; one could name field 4 of a kind-1 record.
- `CHAR/30`'s contents, at positions 42 to 47, take the redraw's separate branch, which
  FND-PARTY-217 reads only in part; that those six map words show an open container's contents
  fits their count but was not read.
- A far pointer to a record kept in a local or passed as an argument is not attributed to
  `DS:19C1` by the census.

## How to reproduce

With the locked evidence Python and `<dsun>` the installed `DSUN.EXE`, in
`tools/research/exec-census/`: `record_base_accesses.py <dsun>
coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv --source 19c1 --source 19c3 --source 6153 --source 6155 8 9`
and the control `--source 19c1 6`; `field_reads.py <dsun> <inventory> --es --table 19c1 8` and
the same for 6; `direct_callers.py <dsun> 1AA0:0009 1AA0:012A`, then the instructions before
each call; `char_item_records.py <install>`. Read `OBJEX.GFF` at `0x1FF50` and `0x2031C + n`, and
line 101 of `RESOURCE.GFF` `TEXT/1000` through its `GFFI` entry (FMT-GFF-003, FMT-GFF-007). The
outputs are retained under `GAME_DIR/analysis/party-record-fields-20261018/` (`base-08-09-kind1.txt`,
`reads-08-19c1.txt`, `accessor-callers.txt`, `accessor-args.txt`, `char-items.txt`,
`text1000-102.txt`). No original function or game process was run.
