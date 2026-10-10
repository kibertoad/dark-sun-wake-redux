---
id: FND-PARTY-084
title: The far pointer DS:142D holds the combatant record of the slot last stored in 4E71:0B44 by overlays 190 and 209, a record given to overlays 202, 211 or 212, or the generation screen's working record, and the level gain sets it only through a new Psionicist level
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006D86F..0x0006D891
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00078958..0x00078994
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007BA25..0x0007BA6B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008A727..0x0008A73B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00093160..0x00093174
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00093378..0x000933C4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00093B62..0x00093BE4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00093BE4..0x00093C30
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00096107..0x00096145
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009881C..0x0009883D
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, store_values.py, immediate_search.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Records and slots are as FND-PARTY-081 gives. `store_values.py` for `142D` and `142F`
and `immediate_search.py` for `142D` find these stores to the far pointer at `DS:142D`, each made
with a store to the far pointer at `DS:1429` just before it:

- overlay 184 `+07E9..+0801`, in `+07D8`: `4E4F:006B`, the generation screen's working combatant
  record, after storing the routine's argument in the word at `4E71:0B44` (FND-PARTY-076);
- overlay 190 `+0618..+0654` and `+36E5..+372B`, and overlay 209 `+0218..+0264` and
  `+0A8A..+0AD0`: the combatant record in the table at `DS:19C9` numbered by the word at
  `4E71:0B44`, with the details record numbered by it in `DS:1429`; overlay 190 `+36ED` and
  overlay 209 `+021E` and `+0A92` store the routine's argument in that word first;
- overlay 202 `+0287..+029B`: the routine's second far pointer argument;
- overlay 211 `+02F7..+0335`: the combatant record numbered by the word at `DS:426D`;
- overlay 212 `+061C..+063D`, in `+061C`: the routine's second far pointer argument, after storing
  its fifth argument word in `4E71:0B44`.

Overlay 210 `+0131` is the only reader in overlay 210 (`+01B0`), and overlay 210 stores nothing
there.

**The Psionicist level.** Overlay 209 `+0A02` (trampoline `57B0:0061`), which overlay 210 `+0740`
calls for a new Psionicist level (FND-PARTY-081), takes a slot, calls `27E5:00C1` with it and 12,
sets the byte at `DS:43EC` from the result, and when the result is not 0 calls `+0A84` with the
slot, then runs events through `39D1:097D` and `39D1:071F` while the dword at `55BD:0000` is not 0
(`+0A09..+0A83`). Overlay 209 `+0000` (trampoline `57B0:0066`), called for a new Preserver level,
stores its argument in `4E71:0B44` (`+0007..+0014`) and does not store to `DS:142D` there.

## Interpretation

When overlay 210 `+0131` runs after a level, `DS:142D` points to the combatant record of the slot
that overlay 190 or 209 last made the current one through `4E71:0B44`, to a record overlays 202,
211 or 212 were given, or, if the generation screen set it last, to the screen's working record.
After a new Psionicist level overlay 209 `+0A84` has just pointed it at the levelling character, so
the wisdom term of the greatest psionic points uses that character's own wisdom. After any other
level it uses the wisdom of whichever record the pointer was left at, which is the levelling
character's only if that character was the one made current last.

## Alternatives

- Which slot is current when the level gain runs after a fight was not read: the paths through
  overlays 173 and 188 that lead to the gain may set `4E71:0B44` and `DS:142D` first.
- A store to `DS:142D` through a register holding the address was not searched for; the searches
  cover direct-address stores and immediates.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `store_values.py <dsun> 142D 142F`;
`immediate_search.py <dsun> 142D`; and `overlay_listing.py <dsun> 184 6D868 6D894`, `190 78950
78994`, `190 7BA20 7BA6B`, `202 8A700 8A73C`, `209 93160 93190`, `209 93378 933C4`, `209 93B62
93C34`, `211 960E0 96146` and `212 98800 9883E`.
