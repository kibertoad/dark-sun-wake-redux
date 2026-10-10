---
id: FND-PARTY-123
title: Before a level gain, overlay 182 +19F8 sets the combat mark of each party member whose mark is 0 to 1 while DS:0DAB is 2, and for 2 or 3 runs the main view's routines and sets DS:0DAB to 1
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00088521..0x00088675
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006A248..0x0006A40A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006A22F..0x0006A239
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000729C1..0x000729DC
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/trampoline_target.py, overlay_listing.py, immediate_search.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Overlay 210 `+0B44` and `+0B55` call overlay 199 `+0C21` before the level gain
`+08BC` (FND-PARTY-081).

**Overlay 199 `+0C21`** stores 0 at `DS:0DA4`, calls `+0BC1`, stores 1 at `DS:25BB`, and passes
each nonzero far pointer at `52A1:0008`, `52A1:0000` and `52A1:0004` to overlay 182 `+0128`
(trampoline `56BD:0043`) before clearing it. It passes a nonzero dword at `DS:6558` to
`444C:0092` and stores the result back, and calls overlay 182 `+19F8` (trampoline `56BD:00BB`),
`3D72:0D83` and `2C5F:0182`. When the bytes at `DS:13F7`, `DS:14E3` and `DS:14E8` are not 0 it
opens and closes `RESOURCE.GFF` on the drive letter at `4E71:0033` through `44DE:0086` and
`44DE:003A`. It then calls overlay 187 `+2B91` (trampoline `56EF:00F2`) with 2 when the fight
state word `4C10:0019` is 0 and with 3 otherwise, calls `45B9:0034` until its result has bit 0
clear, and stores 0 at `DS:25BC` (`+0C21..+0D75`). **Overlay 187 `+2B91`** stores 1 at `DS:4263`,
calls `2834:0001` with its argument, stores 0 at `DS:4275` and returns 1 (`+2B91..+2BAC`).

**Overlay 182 `+19F8`** stores 0 at `DS:0DA4` and `DS:0DA3`. While the word at `DS:0DAB` is 2,
for each slot 0 to 3 whose combatant record (FMT-COMBAT-001, at `DS:19C9`) has a word at `+0x06`
that is not 0 and a byte at `+0x14` that is 0, it stores 1 at `+0x14`; it then calls its own
`+0840` and `+0297`, `362C:06B9` and `362C:01FA` with the word at `DS:140C`, and overlay 200 by
trampoline `5773:0025` with 0x92E0 (`+19F8..+1A79`). While the word is 2 or 3 it calls its own
`+01EF`, `39D1:0430`, `2C5F:0139` and `28C9:250A` with the byte at `DS:143F` set to 1 around the
last two, `39D1:0448` with `28C9:152C`, stores 1 at `DS:0D9C`, calls overlay 190 by trampoline
`571F:0034` with `28C9:0CFF`, stores the word at `4C13:0369` in `DS:426D` when the fight state is
0, and stores 1 in `DS:0DAB`; when the word at `DS:445A` is not 0 it then loads resources through
`38FF:04AB` into `DS:9BE8`, `4E71:0A59` and `4E71:0A5D` where those are 0
(`+1A79..+1BBA`). For any other value of `DS:0DAB` it returns after the first part.

**The stores to `DS:0DAB`.** `immediate_search.py <dsun> 0DAB` finds stores of 1 at overlay 182
file `0x00069C6E`, `0x00069FA9` and `0x0006A352`, 0 at `0x00069E1F`, 4 at `0x00069E5C` and 2 at
`0x0006A233`, in the routine before `+19F8`, which stores 2 after its own calls and then calls
overlay 206 by trampoline `5799:0039` and `28C9:2A44` with 0 (`+19DF..+19E9`), and one store of a
register at resident `2B1C:0000`. None stores 3 as an immediate.

## Interpretation

On the way to a level gain, overlay 182 `+19F8` writes the combat mark at `+0x14` of a party
member's combatant record, from 0 to 1, when `DS:0DAB` is 2. Overlay 210 `+08BC` takes a slot
whose mark is 2 or less, so that store does not change which members gain. For 2 or 3 the routine
runs the main view's routines and the key handler setup, which were not read here, and leaves
`DS:0DAB` at 1; for other values it stores nothing in a party member's records itself. The value
of `DS:0DAB` when overlay 210 `+0B44` or `+0B55` runs was not settled.

## Alternatives

- `28C9:250A`, `2C5F:0139`, `2C5F:0182`, `3D72:0D83`, `39D1:0430`, `39D1:0448`, `444C:0092`,
  overlay 182 `+0128`, `+01EF`, `+0297` and `+0840`, overlay 190's trampoline `571F:0034`,
  `2834:0001` and the event loop through `45B9:0034` were not read for stores to the records.
- The register store at `2B1C:0000` could hold 3, or another value, in `DS:0DAB`; its source was
  not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `trampoline_target.py <dsun> 576C:0039
56BD:0043 56BD:00BB 56EF:00F2`; `immediate_search.py <dsun> 0DAB`; and `overlay_listing.py` over
the ranges above.
