---
id: FND-MAGIC-012
title: For a party slot of 4 or less, every writer of combatant word +0x04 found sets it to the slot, so overlay 177 +0B00's two details records are the same record; a CHAR load sets it through the details chunk's field 15, character generation stores the slot before copying the working record, and an edit keeps it
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062C00..0x00062CC8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006FEAF..0x0006FEFB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000701CA..0x0007042B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:018C..2D40:01FD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2D1C..2D40:2D28
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3E64..2D40:3EBF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006D885..0x0006D891
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E6D8..0x0006E79D
tool: tools/research/exec-census/record_base_accesses.py, block_copies.py, store_values.py, overlay_listing.py, resident_listing.py and direct_callers.py at the commit that adds this finding, with coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv as the inventory; Capstone 5.0.7; Python 3.14.7
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x62100 + x` for overlay 177, `0x6D090 + x` for 184 and
`0x6FE30 + x` for 187. The combatant table is the far pointer `DS:19C9`, 49-byte records
(FMT-COMBAT-001), and the details table `DS:19C5`, 66-byte records (FMT-COMBAT-002). A handle is
an entry of the 3-byte table at `4F49:0C33`: a kind byte and, at `4F49:0C34`, a record number
(FND-PARTY-051).

**What `+0B00` reads.** Overlay 177 `+0B00(n, slot)` takes the combatant record numbered by the
handle word `4F49:0C34 + 3 * slot`, not by the slot itself, reads its word `+0x04`, and reads
byte `+0x12` and the level bytes `+0x1E + i` of the details record of that number (`+0B11..+0B91`).
The class byte `+0x1B + i` comes from the details record numbered by the slot (`+0B93..+0BA0`).

**Direct writes of combatant word `+0x04`.** `record_base_accesses.py <dsun> <inventory> --source
19C9 4` lists 104 accesses; one writes: overlay 187 `+048B`, `mov word es:[bx+4], 0x270F`.
Without `--source`, the other writes at displacement 4 take their base from the tables at `DS:19C1`
(23-byte), `DS:19C5` (66-byte), `DS:40C8`, `DS:1052`, `DS:142D`, a `DS:2A63` word, or a local
far pointer; none of those with a multiplier has 49. Among the 35 with a local or undecided base,
none was read as a combatant record except overlay 184 `+16CC` (below); overlay 190 `+33B9` stores
1 through a pointer normalised by `1000:060E`, and `1BF3:5E0C`, `4842:1616` and `4AE5:04BE` are
undecided.

**Block copies into the combatant table.** `block_copies.py <dsun> <inventory> 10` prints three
copies with `0x31` bytes or the pointer `DS:19C9` in context: overlay 187 `+0478` and overlay 184
`+1708`, both into a combatant record, and overlay 184 `+1DF7`, out of one into `4E4F:006B`.

**The `CHAR` load.** FND-PARTY-049 and FND-PARTY-051 read the load of a `CHAR` resource into
slot `s`; FND-PARTY-052 finds every installed `CHAR` resource to hold a first chunk of byte 0 = 1,
kind 2 and 49 bytes, then a chunk of byte 0 = 3, byte 1 = 0, kind 3, word `+0x06` = 15 and 66
bytes. In the listings:

1. `187 +039A(data, header, handle, slot)` calls `2D40:2C87` with the slot and the handle's kind
   (`+03FC..+0419`). For kind 2 it copies `0x31` bytes into combatant record `r`, the number
   `2D40:2C87` returned, stores 9,999 to its words `+0x04`, `+0x0A`, `+0x08` and `+0x0C`, and `r`
   to `4F49:0C34 + 3 * handle` (`+045E..+04F0`). For kind 3 it copies `0x42` bytes into details
   record `r` (`+054C..+0567`). For any kind it then stores `r` in the header's word `+0x04` and
   as the handle's record number (`+05CE..+05EC`).
2. The first chunk reaches `+039A` through `187 +0000` with the slot as both handle and slot
   (FND-PARTY-049); `2D40:2C87` with kind 2 keeps a slot of 4 or less (FND-PARTY-049).
3. The second chunk reaches `2D40:018C`, which calls `187 +007F` (`2D40:0199`). `+007F` stores
   the header's kind byte as the kind of the handle in `DS:60EB` and, when the header's word
   `+0x02` is 3, calls `+039A` with the handle and the slot (`+0087..+00BD`). For kind 3,
   `2D40:2C87` returns a slot of 4 or less as it is (`2D40:2D1C..+2D25`). `2D40:018C` then calls
   `1AA0:012A` with the handle that the header's byte 1 names (the first chunk's), the header's
   word `+0x06` and the header's word `+0x04` (`2D40:01CA..+01F5`).
4. `1AA0:012A` stores the value into a field of the handle's record; `OBJEX.GFF`'s `FNFO` gives
   kind 2 field 15 the offset 4 and a 2-byte type (FND-PARTY-051, FND-PARTY-052).

**Character generation.** Overlay 184 `+07D8` points `DS:1429` at `4E4F:0029` and `DS:142D` at
`4E4F:006B`, the working details and combatant records (file `0x6D885`, `0x6D88B`, from
`store_values.py <dsun> 142D 142F`). `+1648(si)`, when `DS:112E` is -1, stores kind 2 and `si` in
the handle of `si`, stores `si` at word `+4` of the record `DS:142D` points to (`+16C8..+16CC`),
and copies `4E4F:0029` to details record `si` and `4E4F:006B` to combatant record `si`
(`+16D0..+1708`). When `DS:112E` is not -1 it copies both into the records numbered by
`DS:112E` (`+167B..+16A7`, `+16FA..+1708`), leaving word `+4` as the working copy holds it; the
edit path `+1606` fills the working copy from the slot's own two records through `+1DCE`
(FND-PARTY-063). The other writers of `DS:142D` are overlay 190 (file `0x78990`, `0x7BA67`) and
overlay 209 (`0x933C0`), which point it at combatant record `4E71:0B44`, and overlay 202
`0x8A738`, which stores an argument.

**The status panel's numbering.** `2D40:3E64` gives a handle's record number, when the handle is
below 9,999 and of kind 2 and the number below 42, and that record's word `+0x04`, returning 1
only when the word is below 17 (`2D40:3E6A..+3EB7`).

**`1AA0:012A`'s callers.** `direct_callers.py` finds 14. Two pass a fixed field: `2D40:091F`
field `0x4D`, for a slot above 4 (`2D40:090F..+0919`), and `2D40:2A23` field 2. `2D40:01F5` is the
`CHAR` chunk above. `2D40:25F1`, `2D40:2718` and `2D40:2940` pass a field from a local or an
argument with the handle in `DS:60EB`; the others (`1AA0:0317`, load image `0xEE3F`, overlay 188
`+0764` and `+079D`, overlay 204 `+18B5`) were not read.

## Interpretation

For a party slot `s` of 4 or less, the handle of `s` names combatant record `s` (kind 2 keeps
the slot), and each writer read here leaves combatant word `+0x04` of record `s` equal to `s`:

- a `CHAR` load stores 9,999 when it copies the combatant chunk and then `s`, the number
  `2D40:2C87` gave the details chunk, through field 15;
- a character made in generation gets `s` in its working record before the copy;
- an edited character keeps the word it had;
- a load restores the combatant, details and handle tables together from `SAVE` 5, 6 and 3
  (FND-SAVE-015, FND-SAVE-021), so it brings back what the save held.

So, for a party slot, the details record `+0B00` reads by the slot and the one it reads through
combatant word `+0x04` are the same record, and its class bytes, origin byte and level bytes come
from one character. A party record's word `+0x04` is 9,999 only between the two chunks of a
`CHAR` load, when `+0B00` does not run.

## Alternatives

- A script field store through `1AA0:012A` with field 15 on a party handle would change the word:
  five callers that pass a variable field and five that were not read are open. Whether any `GPL `
  program stores field 15 of a kind-2 object was not searched.
- A `CHAR` resource written by the game (a saved character) could lack the details chunk or name
  another field: only the 19 installed resources were read (FND-PARTY-052); the character writer
  was not read.
- `CHAR` resources whose details chunk comes before the combatant chunk, or whose byte 1 names
  another chunk, would bind another record: none of the installed resources does.
- Writes through the undecided or unread pointers listed above, and copies of whole tables (for
  example a party reorder that moves records without moving handles), were not ruled out beyond
  the `block_copies.py` hits.
- A party slot above 4: `+050A` calls `+0B00` only for a slot below 4 (FND-MAGIC-004), so this
  does not arise there.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python, `<dsun>` the installed `DSUN.EXE` and
`<inv>` `../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`: `record_base_accesses.py <dsun> <inv>
--source 19C9 4` and `record_base_accesses.py <dsun> <inv> 4`, keeping the writes;
`block_copies.py <dsun> <inv> 10`, keeping the sites with `0x31` or `19C9` in context;
`store_values.py <dsun> 142D 142F`; `overlay_listing.py <dsun> 177 62C00 62CC8`, `187 6FEAF
6FF00`, `187 701CA 70440`, `184 6E6D8 6E820` and `190 7B6D0 7B700`; `resident_listing.py <dsun>
2D40:018C..2D40:0200 2D40:2D1C..2D40:2D34 2D40:3E64..2D40:3EE0 2D40:0905..2D40:0924
2D40:25DC..2D40:25F6 2D40:2700..2D40:271D 2D40:2928..2D40:2945 2D40:2A0B..2D40:2A28`;
`direct_callers.py <dsun> 1AA0:012A`.
