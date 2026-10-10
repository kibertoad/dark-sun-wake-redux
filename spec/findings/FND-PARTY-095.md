---
id: FND-PARTY-095
title: The routine 28C9:2E0E that changes a combatant's kind treats the kinds as three sides, 0x71, 0x184 and 0x608, each with a set of kinds hostile to it, 0xF80, 0xC58 and 0x964, so the award and kill masks are the party's side and the kinds hostile to it
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2E0E..28C9:305F
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/field_stores.py, resident_listing.py)
environment: null
---

## Observation

The combatant records and slots are as FND-PARTY-081 gives; the kind is the combatant record's
byte at `+0x15`. `field_stores.py <dsun> <inventory> --es 15`, keeping the stores within six
instructions after a text naming `19c9`, lists seven in `28C9:2E0E`, one at overlay 195 `+00A7`
and one at overlay 204 `+1A0B`.

**`28C9:2E0E`** takes a slot and a kind `T`. It returns unless the slot is 4 or above and its state
byte is 2 (`2E13..2E34`). With `K` the current kind of the slot's combatant, and the masks
`A = 0x71`, `B = 0x184`, `C = 0x608` and `HA = 0xF80`, `HB = 0xC58`, `HC = 0x964`, tested with
`1 << kind`:

1. When `T` is not `K`, it returns when `T` is in `A` and `K` in `HA`, when `T` is in `B` and `K`
   in `HB`, or when `T` is in `C` and `K` in `HC` (`2E4F..2EA4`); and it returns when `K` is in
   `A` and `T` in `HA`, when `K` is in `B` and `T` in `HB`, or when `K` is in `C` and `T` in `HC`
   (`2EA4..2EF3`).
2. When `T` and `K` are both in `A`, both in `B` or both in `C`, it stores 11 (`2EF3..2F56`).
3. Otherwise, when `K` is in `B` it stores 8, in `C` 10, and in `A` 6 (`2F56..2FDC`).
4. Otherwise, when `K` is 1, it stores 7 when `T` is in `A`, 3 when `T` is in `B` and 5 when `T`
   is in `C` (`2FDC..305B`); for any other `K` it stores nothing.

As sets: `A` is 0, 4, 5 and 6; `B` is 2, 7 and 8; `C` is 3, 9 and 10; `HA` is 7 to 11; `HB` is 3,
4, 6, 10 and 11; `HC` is 2, 5, 6, 8 and 11. Kind 1 is in none of them, and kind 11 in all three
hostile sets and no side.

## Interpretation

The kinds form three sides. The mask `0x71` that overlay 188 `+1604` requires of a member given
experience is side `A`, the party's side (kind 0), and the mask `0xF80` that overlay 173 `+30C7`
requires of a killed combatant is `HA`, the kinds hostile to that side (FND-PARTY-086). Kind 11
is hostile to all sides and kind 1 to none. When a combatant is provoked by a side `T`, it turns
hostile to all when it is on `T`'s side, hostile to both other sides while staying on its own when it
was on another side (8, 10 or 6), and, when it was neutral (kind 1), takes a kind of another side
that is hostile to `T`'s side alone (7, 3 or 5); one already hostile to `T`'s side, or of a side
`T` is hostile to, keeps its kind.

## Alternatives

- What overlay 195 `+00A7` and overlay 204 `+1A0B` store, and the callers of `28C9:2E0E` and what
  they pass as `T`, were not read; the reading of `T` as the provoking side rests on the
  structure of the tests.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `field_stores.py <dsun>
../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv --es 15`, keeping the hits whose printed
instructions name `19c9`, and `resident_listing.py <dsun> 28C9:2E0E..28C9:3070`.
