---
id: FND-PARTY-148
title: The words at 0x04, 0x06 and 0x0E of a CHAR record hold the party slot the character was written from, and the words at 0x12 to 0x16 are 9,999 exactly where the record has no chunk for that field
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x51B..0x531
tool: Python 3.14.7 (a scratch script reading the CHAR resources through the GFF directory as tools/research/exec-census/details_chunks.py does and walking each record's chunk chain)
environment: null
---

## Observation

The writer, overlay 187 `+018F`, writes a `CHAR` resource as a chain of chunks whose 10-byte
headers are the entries of `2D40:2196`. The first entry starts from the slot as the current
handle: its word at `+0x04` is the handle's record number and its word at `+0x06` the slot
itself, and the record copied after it is the slot's FMT-COMBAT-001 record (FND-PARTY-054). For a
slot of 4 or less, `2D40:2C87` keeps the slot as the number of the details record a type-3 chunk
fills, and a load stores that number in the combatant record's `details_index`, the word that
CHAR word `0x0E` loads (FND-PARTY-049, FND-PARTY-051). A load replaces the words that CHAR words
`0x12`, `0x14` and `0x16` load with 9,999, and the type-2 chunks of `field` 16, 17 and 4 store
their records' numbers there (FND-PARTY-050, FND-PARTY-051).

Walking the chunk chain of each of the 19 `CHAR` resources of the installed `CHARSAVE.GFF`:

- The words at `0x04`, `0x06` and `0x0E` are equal in every record, 0 to 3, and equal to the
  `record_ref` of the type-3 chunk at `0x3B` and to the `record_ref` and `field` of the end header. In resource 50
  (at file `0x517`) they are 3 (file `0x51B`, `0x51D` and `0x525`).
- The word at `0x12` is 9,999 in the five records with no type-2 chunk whose `target_chunk` is 0
  and `field` is 16, and 476 to 518 in the other 14. The word at `0x14` is 9,999 only in resource
  32, the one record with no such chunk of `field` 17. The word at `0x16` is 9,999 in the twelve
  records with no such chunk of `field` 4. No word of the three equals the `record_ref` of the
  chunk of its field: in resource 29 the words are 516 and 517 and the chunks' `record_ref`s
  173 and 50.
- The bytes at `0x18` and `0x19` are 0 in all 19 (FND-PARTY-113).

## Interpretation

`unk_04` holds two words, the chunk header's `record_ref` and `field` of the writer's first entry,
and both are the party slot, 0 to 3, the character was in when it was written. `unk_0e` is the
character's `details_index` at that time, which for a party slot is the slot again. The three
words at `0x12` are the values the combatant record held at `+0x08`, `+0x0A` and `+0x0C` when it
was written: 9,999 where the character had no record in that field, and otherwise a number the
load discards for the number of the record it makes from the chunk.

## Alternatives

- What the numbers 456 to 519 at `0x12` to `0x16` numbered when the shipped records were written,
  and why the chunks' `record_ref`s differ from them, was not read; Q-PARTY-020 asks what those
  records hold.
- Whether the game, in play, writes a record whose slot is above 3 was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python: read the directory of
the installed `CHARSAVE.GFF` as `tools/research/exec-census/details_chunks.py` does; for each
`CHAR` resource, follow the chunk chain from offset 0 by the `len_data` of each header to the
header of type 0xFF, and compare the words at `0x04`, `0x06`, `0x0E`, `0x12`, `0x14` and `0x16`
with the headers' `record_ref` and `field`.
