---
id: FND-PARTY-054
title: The CHAR writer emits the character's chunk first, then walks its fields depth first in the order 15, 16, 17, 4 for a character and 5, 2, 4 for a 23-byte record, so a record the game writes has its details chunk second whenever the character has one
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E5B7..0x0006E5D0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006FFBF..0x000701CA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0E1D..2D40:0E80
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2196..2D40:2410
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0328..2D40:0399
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00072F7B..0x00072FF7
tool: Python 3.14.7 with Capstone 5.0.7 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, resident_listing.py, direct_callers.py, immediate_search.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; handles are those of FND-PARTY-051.

**The caller.** Overlay 184 `+1527..+1538` calls overlay 187 `+018F` (trampoline `56EF:0020`)
with a slot, the far value at `DS:144A`, the byte 0, a resource number and the tag `CHAR`. A
search of `DSUN.EXE` for the immediate `0x4843` finds the tag `CHAR` pushed only there and at
overlay 171 `+02F1` and `+0AEF` and overlay 182 `+0697`, which pass it to the load `2D40:000A`
(FND-PARTY-046, FND-PARTY-013). `direct_callers.py` finds overlay 187 `+018F` called from that
site and from overlay 187 `+1077` and `+13D0`, which pass a tag built by `+0699` from the
characters of `RDOJRGOT` instead.

**The writer, overlay 187 `+018F`.** It allocates 0xC00 bytes through `444C:0008` and returns 0
when that fails (`+019A..+01B7`). It calls `2D40:2196` with the slot and a 0x320-byte array on
the stack, which returns a count of 10-byte entries (`+01C2..+01D3`). For each entry it stores
the entry's byte 2 and word 4 as the kind and record number of the handle in `DS:60EB`, takes
the record's address from `2D40:0E1D`, and calls `+0303`, which copies the 10-byte entry and
then as many bytes of the record as the entry's word at `+0x08` into the buffer and returns the
position after them (`+01E0..+0272`). The test at `+0237..+0246` compares the entry's word at
`+0x02` with 4 and, when it is equal, with 6, so it never skips an entry. After the last entry
it writes the first entry again with byte 0 set to 0xFF and the word at `+0x08` set to 0
(`+0295..+02B5`), and passes the buffer, its length (the sum of 10 plus each entry's word at
`+0x08`, plus 10), the number and the tag to `+034E`, which writes them through `37FC:00E8`
and returns 1 on success (`+02B8..+02D7`, `+034E..+0399`).

**`2D40:2196`**, the routine FND-CONFIG-143 reads, starts from the slot as the current handle
and fills one entry per accepted handle, up to 0x4F entries (`2D40:23CB`):

| Entry offset | Value |
| --- | --- |
| `0x00` | 1 for the first entry; otherwise 4 when the byte at `DS:60ED` plus the current field number is 8, 3 when the handle is the one in `DS:60EB`, and 2 for any other (`2D40:21EB..+221E`) |
| `0x01` | the index of the entry whose children are being visited, 0 for the first (`2D40:2283`, `+23A6..+23B4`) |
| `0x02` | the handle's kind, as a word (`2D40:223A..+2256`) |
| `0x04` | the handle's record number (`2D40:225A..+2274`) |
| `0x06` | the field number through which the handle was reached; the slot itself for the first entry (`2D40:2295`) |
| `0x08` | the word at `DS:6167` plus 2 times the kind: 0x17, 0x31 or 0x42 for kinds 1, 2 and 3 (`2D40:2299..+22BD`) |

For each accepted entry it calls `1AA0:0566` with the handle, which pushes the handle's nonzero
field values onto a stack, and then takes the next handle from `1AA0:051C`, which pops the most
recent one (FND-CONFIG-145). It passes over a handle whose record number is 9,999 or negative or
whose kind is 0, 4, 6 or above 5 (`2D40:2329..+23A0`). For a field whose byte at `DS:60ED` is 8 it
stores the value as the record number of the handle in `DS:60EB`, with the kind of the last entry
before it that was not of type 3 (`2D40:22E7..+231E`). At the end it stores 0xFF at byte 0 of the
entry after the last and the count at byte 1 of the first entry (`2D40:23F1..+2406`).

**The field lists.** `1AA0:0566` reads, for a handle of kind *k*, a starting position from the
word at `DS:5EEB + 34 * k` and visits the field numbers in the words at `DS:5ECB + 34 * k + 2 *
position` from that position down to 0, skipping 0 (FND-CONFIG-145). Overlay 188 `+00DB..+0113`
clears the sixteen words at each of `DS:5EED`, `DS:5F0F`, `DS:5F31`, `DS:5F53` and `DS:5F75`,
and `+0115..+0151` then stores:

| Kind | Starting position | Field numbers from position 0 |
| --- | --- | --- |
| 1 | 2 (`DS:5F0D`) | 5, 2, 4 (`DS:5EED`, `DS:5EEF`, `DS:5EF1`) |
| 2 | 4 (`DS:5F2F`) | 15, 0, 16, 17, 4 (`DS:5F0F` to `DS:5F17`) |
| 5 | 1 (`DS:5F95`) | 0, 2 (`DS:5F75`, `DS:5F77`) |

Kinds 3 and 4 keep the starting position 0 of the load image (FND-CONFIG-145) and the cleared
field 0. `FNFO` 2 gives field 2 the type byte 8, field 5 the type byte 13 and fields 4, 15, 16
and 17 the bytes 7, 12, 7 and 7 (FND-PARTY-052); `1AA0:0009` turns a value of type 10 or more into
the handle in `DS:60EB` with the kind of the type less 9 (FND-CONFIG-140).

**The load's last test.** After the walk of FND-PARTY-051, when the slot's kind is 2,
`2D40:0328..+035C` returns -1 if the word at `+0x04` of the slot's combatant record is 9,999 or
more.

## Interpretation

The game writes a `CHAR` resource as the chain FND-PARTY-051 reads. The first chunk is the
character's combatant record. Because the stack hands back the last field pushed first, a
character's children come out in the order 15, 16, 17, 4: the details record (field 15, type 12,
so kind 3 and chunk type 3) whenever `details_index` is set, then the records in the words at
`0x08`, `0x0A` and `0x0C` as type-2 chunks. A 23-byte record's children come out as field 5
(type 13, kind 4, which the writer passes over), then field 2 (type 8, so a type-4 chunk: the
next record of the chain), then field 4 (a type-2 chunk). Each chain is followed to its end
before the contents met along it. The end header is the first header with byte 0 at 0xFF and a
length of 0, and byte 1 of the first header is the chunk count, as in the stored records
(FND-PARTY-052). A character with no details record would be written without the type-3 chunk,
and the load rejects such a record: in a chain this writer produces only a type-3 chunk names
field 15, so `details_index` keeps the 9,999 the type-1 copy stores (FND-PARTY-049).

## Alternatives

- Another routine writes `CHAR` resources: the search for the tag's immediate found no other
  push, but a tag built at run time, as `+0699` builds others, was not searched for.
- What the value at `DS:144A` names, and when overlay 184 reaches `+1538`, were not read here.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, run in `tools/research/exec-census/`: `immediate_search.py <dsun> 4843`,
`direct_callers.py <dsun> 187+018F`, `overlay_listing.py <dsun> 184 0x6E590 0x6E5D8`, `187
0x6FFBF 0x701CA` and `188 0x72F7B 0x72FF7`, and `resident_listing.py <dsun>
2D40:0E1D..2D40:0E80 2D40:2196..2D40:2410 2D40:0328..2D40:0399`.
