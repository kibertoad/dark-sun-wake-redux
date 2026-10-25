---
id: FND-COMBAT-074
title: FNFO 1 gives every byte of the combatant and details records a field number, and the CHAR copy names all but four; overlay 173 +2DD0 returns the combatant field an item was reached through, so weapons come from field 17 and ability items from field 16; no engine code reads combatant byte +0x18 bits 0 to 4 and 7 or byte +0x20, which scripts set and test bit by bit
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x2000C..0x200D0
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x200D0..0x20194
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x2031C..0x2037E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D6F0..0x0005D73B
tool: tools/research/exec-census/overlay_listing.py, record_base_accesses.py, field_reads.py, script_decode.py and details_chunks.py at the commit that adds this finding, with coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv as the inventory; a short Python 3.14.7 read of FNFO 1 and 2 and tally of the field numbers in script_decode.py's output; Capstone 5.0.7; Python 3.14.7
environment: null
---

## Observation

`FNFO` 1 of `OBJEX.GFF` (980 bytes at `0x1FF48`) holds 98 signed words per object kind from kind
1, and `FNFO` 2 (98 bytes at `0x2031C`) a type byte per field number (FND-PARTY-052,
FND-CONFIG-153). The kind-2 row is at `0x2000C` and the kind-3 row at `0x200D0`. The words other
than -1 give:

| Combatant offset | Field | Type | | Details offset | Field | Type |
|---|---|---|---|---|---|---|
| `0x00` | 13 | 2 | | `0x00` | 35 | 5 |
| `0x02` | 14 | 3 | | `0x04` | 36 | 5 |
| `0x04` | 15 | 12 | | `0x08`, `0x0A`, `0x0C` | 37, 38, 39 | 3 |
| `0x06` | 0 | 9 | | `0x0E` | 0 | 9 |
| `0x08`, `0x0A`, `0x0C` | 16, 17, 4 | 7 | | `0x10` | 40 | 3 |
| `0x0E`, `0x0F` | 18, 19 | 1 | | `0x12` to `0x20` | 41 to 55 | 1 |
| `0x10` | 6 | 2 | | `0x21` | 56 | 0 |
| `0x12` | 20 | 0 | | `0x22` | 57 | 1 |
| `0x13`, `0x14`, `0x15` | 21, 22, 23 | 1 | | `0x23` | 58 | 0 |
| `0x16` | 24 | 0 | | `0x24` to `0x2D` | 59 to 68 | 1 |
| `0x17` | 9 | 1 | | `0x2E`, `0x2F`, `0x30` | 69, 70, 71 | 0 |
| `0x18` to `0x20` | 25 to 33 | 1 | | `0x31` to `0x3B` | 72 to 82 | 1 |
| `0x21` | 34 | 6 | | `0x3C`, `0x3E` | 83, 84 | 3 |
| | | | | `0x40`, `0x41` | 85, 86 | 1 |

Field 0 is also offset 0 of kinds 1 and 5; field 9 is also byte `+0x12` of kind 1 and offset 4 of
kind 5; field 6 is also word `+0x0C` of kind 1.

**Field of an item.** Overlay 173 `+2DD0(list, i)` (`0x5D6F0..0x5D73B`) climbs from entry `i` of a
graph list of `2D40:2196` through each entry's parent byte `+1` while the parent's kind word `+2`
is not 2, and returns word `+6` of the entry whose parent is of kind 2. FND-PARTY-054 gives word
`+6` as the field number through which `2D40:2196` reached the entry.

**Byte `+0x18` and byte `+0x20`.** `record_base_accesses.py <dsun> <inv> --source 19C9 2 8 A C 12
17 18 20` and `field_reads.py <dsun> <inv> --es --table 19c9 18 12 17 20` list every access at
those displacements whose pointer is `DS:19C9`. At `+0x18` the tests are of 0x20 (`25AF:0349`,
`28C9:0605`, overlay 189 `+0192`, 190 `+0566`, `+0A1E`, 202 `+0026`, 211 `+0026`, 212 `+002B`)
and 0x40 (overlay 190 `+0541`, `+0A02`); the whole-byte loads at `28C9:2020`, overlay 173
`+06B0`, `+0703`, `+11DC`, `+40FA` and overlay 188 `+0E8C` are followed by `and al, 0x20` or
`test ax, 0x20` or `0x40`; the stores are `and 0xDF`, `or 0x20`, `or 0x60` and `and 0x9F`
(overlays 188, 190, 195) and overlay 204 `+268D`, the script setter. At `+0x20` there are only
overlay 204 `+2434` and `+26D5`, the script getter and setter (FND-PARTY-152). At `+0x12` and
`+0x17` there is none; at `+0x02` there are 21 (FND-COMBAT-073). The `field_reads.py` pass over
all code bytes adds no hit outside the inventory. Details offsets `0x36` and `0x41` have no access
through `DS:19C5` in `record_base_accesses.py <dsun> <inv> --source 19C5 0 2 4 6 A C E 36 41`.

**Scripts.** `script_decode.py <GPLDATA.GFF> <OBJEX.GFF> --all` gives 104,921 lines. Tallied by
field number, the field paths name field 25 383 times and field 33 68 times; script function 48
reads field 25 85 times and field 33 22 times, and function 49 writes them 81 and 36 times. The
paths combine field 25 with `&` of each of 1, 2, 4, 8, 16, 32, 64 and 128 and with `|` of each
of them, and field 33 with `&` of each of them but 4 and `|` of each but 64. Function 49 stores the
literals 0, 1, 64, 192, 193 and 255 in field 25 and 1, 2 and 3 in field 33; several resources
(`GPL/11`, 12, 14) read both into variables and later store them back. No path or function call
names field 9, 20, 21 or 86, and the search form of instruction `0x33` names only fields 0, 1
and 3. Field 77 is named 60 times, compared most often with 7 (40 times) and
also with 0, 1, 4, 8 and 9.

**Shipped values.** `details_chunks.py <gff> --byte 36` over `OBJEX.GFF` gives byte `0x36` of the
375 details chunks as 0 (33), 1 (3), 2 (5), 4 (236), 5 (4) and 7 (94; among them the chunks named
`Mindflayer`, `Yuan-ti`, `Slave` and `Guard`); over `CHARSAVE.GFF`, 0 (5), 1 (13) and 4 (4).
`--byte 41` over `OBJEX.GFF` gives 0 in all 375.

## Interpretation

Every byte of both records belongs to a field the generic accessor `1AA0:012A` can reach, and the
offsets agree with the `CHAR` copy of FND-PARTY-049 and FND-PARTY-051: combatant offset `n` is
`CHAR` offset `0x0A + n` and details offset `n` is `CHAR` offset `0x45 + n`, so FMT-PARTY-001
names them. What remains without a meaning is combatant `+0x12` (field 20, signed, set from
`base_armor_class` at the end of generation), combatant `+0x17` (field 9, the field number the
item record uses for its byte `+0x12`), details `+0x36` (field 77) and details `+0x41` (field 86).

A combatant's words `+0x08`, `+0x0A` and `+0x0C` head three lists of 23-byte records. `+2DD0` tells
them apart by field: the attack routine draws weapons only from items whose `+2DD0` is 17, the
word at `+0x0A` (FND-COMBAT-065's "location 0x11"); the ability rebuild and the routines of
FND-MAGIC-015 and FND-MAGIC-024 count items whose `+2DD0` is 16, the word at `+0x08`; the armour
class counts every item whose `+2DD0` is not 4 (FND-COMBAT-068), so the word at `+0x0C` heads the
items that count for neither, the list the recipient transfer fills (FND-PARTY-221). Read as
equipment: `+0x0A` is the items in hand, `+0x08` the items worn, `+0x0C` the items carried. Which
screen shows which list is Q-PARTY-082.

Byte `+0x18` holds the engine's two control bits 0x20 and 0x40, and the other six bits belong to
the scripts alone, which set and test each of them; byte `+0x20` is wholly the scripts'. Byte
`+0x41` is 0 in every shipped record and has no reader. Byte `+0x36` varies among creatures, and
scripts compare it with small numbers, 7 most often; nothing in the executable reads it.

## Alternatives

- The searches see fixed displacements from a `DS:19C9` or `DS:19C5` base and the scripts'
  literal field numbers; a read through a copy of a record, a computed field number in a script, or
  `1AA0:012A` with a field from a variable would be missed.
- That a script path naming field 77 or 41 on objects `0x8025` to `0x802C` reaches the details
  record was not read: the kind-2 row of `FNFO` 1 has no offset for them, so the path resolver
  must follow `details_index` or the objects are not of kind 2.
- The labels worn, in hand and carried are interpretation; the code shows only which routine uses
  which list.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python, `<dsun>` the installed `DSUN.EXE`,
`<inv>` `../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv` and the installation's `GPLDATA.GFF`,
`OBJEX.GFF` and `CHARSAVE.GFF`: read the signed words of `OBJEX.GFF` at `0x2000C + 2 * f` and
`0x200D0 + 2 * f` and the bytes at `0x2031C + f` for `f` from 0 to 97;
`overlay_listing.py <dsun> 173 5D6F0 5D73B`; `record_base_accesses.py <dsun> <inv> --source 19C9
2 8 A C 12 17 18 20` and `--source 19C5 0 2 4 6 A C E 36 41`; `field_reads.py <dsun> <inv> --es
--table 19c9 18 12 17 20`; `script_decode.py <GPLDATA.GFF> <OBJEX.GFF> --all`, then count the
numbers in each `path(...; ...)` and each `22 48,` and `22 49,` call by field;
`details_chunks.py <OBJEX.GFF> --byte 36`, `--byte 41` and `details_chunks.py <CHARSAVE.GFF>
--byte 36`.
