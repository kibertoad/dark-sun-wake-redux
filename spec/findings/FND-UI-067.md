---
id: FND-UI-067
title: Start Game opens the character window only when box 0 holds a stored character whose identifier is 0x8300, which no code makes and which the installed CHARSAVE.GFF's listable characters do not carry
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00081309..0x00081322
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006EE3B..0x0006EE4D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007028E..0x000702AD
tool: Capstone 5.0.9 over overlay and resident code; tools/research/exec-census/overlay_listing.py, resident_listing.py, immediate_search.py and field_stores.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`. The Start Game branch of overlay 194's callback
tests word `+6` of the record at the far pointer `DS:19C9` for `0x8300` and `DS:140C` for 50 at
`+01D9..+01F2`, after it has called the gate routine overlay 182 `+1167` and overlay 182 `+0B52`
(FND-UI-059). The record is box 0's 49-byte combatant record.

**Writers of word `+6` of that record:**

| Writer | What it stores | Reached from |
|---|---|---|
| overlay 187 `+045E`, copy at `+0478` | `CHAR` word `0x10` of the character loaded into the box | `2D40:000A`, whose four `CHAR`-tag callers are the Create Characters list build and `ADD` in overlay 171, the party loader overlay 182 `+063D` (`CHAR 40` to 43) and the store in overlay 184 |
| `2D40:2533` | 0 | `2D40:2410` on a kind-2 slot: the `ADD` list build after each listed character, `DROP`, and the cleanup after a defeat |
| overlay 184 `+1DAB` | an identifier counted up from `0x8015` in a loop that leaves at `0x8300` (`+1DB2`) | finishing a generated character |
| overlay 184 `+16F0..+170D` | the working record's identifier | finishing a generated character |
| overlay 187 `+0A6C` | the whole table from a `SAVE` resource | a saved-game load, after which play begins without the start window |

`field_stores.py --es 6` finds no other overlay store at displacement 6 into this table.
`immediate_search.py 8300` finds `0x8300` compared with a record's `+6` only in this test and in
the generation bound.

**Writers of `DS:140C`** are the load image (50), the `-R` switch (only with `-K911`), the store of
50 at `277B:03C1` after a defeat, the saved-game load in overlay 182 `+15D7` and script opcode
`0x5E` in overlay 187 `+1132`; FND-UI-051 finds none of them between the click and the test.

When the placed-object count is 0 at the gate, the gate's loader puts `CHAR 40`, identifier
`0x8019`, in box 0 (FND-PARTY-013, FND-PARTY-021).

## Interpretation

The test holds only when the player adds, into box 0 through Create Characters, a stored
character whose `CHAR` word `0x10` is `0x8300`, returns to the start window by Esc or the return
button while `DS:0DAB` is 0, and clicks Start Game; the placed-object count is then not 0, so the
gate keeps box 0. The opening then runs as usual and the jump calls overlay 212 `+061C` with
slot 0's two records and 0, which opens the character window `WIND/11500` on that character
after the gate has stored 1 in `DS:0DAB` (FND-COMBAT-053), so its Esc stays in play
(FND-PARTY-046, FND-PARTY-170). No code makes the identifier `0x8300`. In the installed
`CHARSAVE.GFF` the listable characters are `CHAR 36` to 39, with identifiers `0x8018` to `0x8015`;
`CHAR/33` holds `0x8300` with `CACT` 0, and `CHAR/53` lies outside the list's range of 1 to 39
(FND-PARTY-012, FND-PARTY-050, FND-PARTY-053). With that file the test is false on every route
from the start window.

## Alternatives

When the gate returns early, whether its other callees, `+0B52` or `MAS/50`'s opcodes write box
0's `+6`, and the byte offsets of overlay 204's field setter `+259D` were not read. The `CACT` and
`CHAR` values are cited, not read again.

## How to reproduce

`overlay_listing.py <dsun> 194 81309 81322`, `... 184 6EE3B 6EE4D` and `... 187 7028E 702AD`;
`field_stores.py <dsun> <inventory> --es 6`; `immediate_search.py <dsun> 8300 140C`.
