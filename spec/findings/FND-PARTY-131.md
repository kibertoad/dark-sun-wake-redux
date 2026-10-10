---
id: FND-PARTY-131
title: Overlay 184 +1648 stores 2 in the object slot state byte at 4F49:0C33 for the slot of a character it adds to the party, the state that sends 31E0:0B4A to overlay 193 +1DB0 and 371D:0002
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E704..0x0006E760
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0B4A..31E0:0B7B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0DD7..31E0:0DE9
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/store_values.py, overlay_listing.py, resident_listing.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. FND-ACTOR-005 reads the byte at `4F49:0C33 + slot * 3` with the 37-byte slot record at
`DS:67BB + slot * 0x25`.

`store_values.py <dsun> --indexed C33` lists 20 stores through `ES` to that byte. Those of an
immediate are at overlay 175 file `0x000608CA`, overlay 189 `0x00076A71` and overlay 213
`0x000998DC` (1), overlay 184 `0x0006E743` (2), and overlay 184 `0x0006E192`, overlay 187
`0x00070262` and `0x00071C6B` and resident `2E0B:000A` and `2E13:0007` (0). The other eleven
store a register: resident `1ADA:000C`, `1AED:000F`, `2E17:0009`, `2F71:000E`, `2FAE:0007` and
`2FC0:0000`, and overlay 187 file `0x0006FE4E`, `0x0006FECF`, `0x0006FF3D`, `0x00070029` and
`0x000704A2`.

**Overlay 184 `+1648`** (FND-PARTY-063) tests the word at `DS:112E`. When it is -1, it stores 2 in
the byte at `4F49:0C33 + 3 * SI`, `SI` in the word at `4F49:0C34 + 3 * SI` and in the word at
`+0x04` of the record the far pointer at `DS:142D` names, and then copies the character's details
and combatant records into slot `SI` of the tables at `DS:19C5` and `DS:19C9` (`+1674..+16F0`).
When it is not -1, it copies them into the slot `DS:112E` names and stores none of the three.

**`31E0:0B4A`** takes a slot number: it returns 0 when the byte at `DS:265B` is 0, and otherwise
reads the slot record at `DS:67BB + slot * 0x25` and the 8-byte entry of the table at `DS:67B7`
that the record's word at `+1` numbers (`31E0:0B4A..31E0:0B7B`). `31E0:0D09` calls it with the
word at `+6` of an entry of that table (`31E0:0DD7..31E0:0DE9`). FND-PARTY-130 shows that it calls
overlay 193 `+1DB0` and `371D:0002` when the slot's byte at `4F49:0C33` is 2.

## Interpretation

State 2 at `4F49:0C33` marks the object slot of a party member, numbered as the party slot, so
the branch of `31E0:0B4A` that FND-PARTY-130 follows to the routines that write the records is
the one for a party member's object. That branch runs from the view draw `31BA:000E` only for a
table entry whose word at `+6` is negative, so Q-PARTY-054 turns on whether a party member's entry
can be in that state when overlay 182 `+19F8` redraws the view before a level gain.

## Alternatives

- The eleven register stores may also store 2; their sources were not read. Overlay 187's
  copy a byte at `+2` of a record its caller passes, which may be a saved slot state.
- That the entry's word at `+6` holds the slot number once loaded is read from `31BA:000E`, which
  stores `31E0:0EFF`'s result there, and from `31E0:0D09`; `31E0:0EFF`'s result was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `store_values.py <dsun> --indexed C33`;
`overlay_listing.py <dsun> 184 6E704 6E790`; and `resident_listing.py <dsun>
31E0:0B4A..31E0:0B80 31E0:0DB0..31E0:0DEC`.
