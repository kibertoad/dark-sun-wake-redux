---
id: FND-EXE-616
title: The 6 routines of overlay 174 that no entry placed return a spell's weight, sum the hit points of the marked combatants of one group, step the N and P target through the hostile slots near the leader, equip a combatant's carried items into its empty positions, find a carried item at position 2, and route a spell count decrement by slot
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005EF67..0x0005EF81
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005F3BD..0x0005F404
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005F724..0x0005FB41
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 6 rows of overlay 174 (header segment `5682`, 13
trampolines, code from file offset `0x5EE00`, 3,393 bytes) that no spec location covered, 1,150
bytes in all. Each row below is one row of the inventory; "offset" is the overlay code offset, the
file offset is `0x5EE00` plus it, and `DS` is `57E0`. Every routine has a trampoline in `5682`.
Callers are the hits of `direct_callers.py`, checked against the instructions around them, named
by the row that holds them. The 49-byte records under the far pointer `DS:19C9` and the 66-byte
records under `DS:19C5` are those of FMT-COMBAT-001 and FMT-COMBAT-002, reached for a slot through
the word at `4F49:0C34 + 3 * slot` and the 49-byte record's word `+4` (FND-EXPLORE-002). An item
list is the 10-byte entry list of `2D40:2196`, its kind-1 entries naming 23-byte records under
`DS:19C1` whose word `+0x0A` is a type `t` among the 15-byte records under `DS:19CD`
(FND-EXE-600) and whose byte `+0x13` is the position the item is worn in (FND-PARTY-217).

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+0167` | `0x5EF67..0x5EF81` | `5682:002F` | Takes a spell number and returns the absolute value of the signed byte at `4C78:0000` plus it, the weight table of FND-PARTY-121. Called from `+054E`. |
| `+05BD` | `0x5F3BD..0x5F404` | `5682:0039` | Takes `n` and returns the sum of `hit_points` (word `+0`) over the 49-byte records 0 to 41 whose `combat_mark` (byte `+0x14`) is 1 and whose byte `+0x15` is `n`. Called from `+034E` and `+035A`. |
| `+0924` | `0x5F724..0x5F84E` | `5682:004D` | Takes a direction byte. Calls overlay 173 `+0000` (trampoline `5671:0020`) with the leader's cell (the position words `+0x0A` and `+0x0C` of slot `4C13:0369`'s 37-byte record shifted right by 4), `0x0F80`, `0x18`, a stack list of 4-byte entries and 0, and does nothing when it returns 0 or less. With the target word `4C4F:022C` at `0x270F` it takes the first entry's slot (word `+2`) for a direction of 1 and the last for 0. Otherwise it walks the entries forward (1) or backward (0) and takes the slot after the one that equals the target, and when the walk ends without one it sets the target to `0x270F` and starts again from the first or last entry. It then calls `2C5F:018C` with the slot and 1 and `28C9:31D2` with the slot, and stores the slot in `4C4F:022C`. Called from the key routine `28C9:0CFF` at `28C9:1022`, which passes 1 for N and 0 for P (FND-AI-004). |
| `+0A4E` | `0x5F84E..0x5FADB` | `5682:0052` | Takes a slot, an entry `k`, a count `n` and a far item list. With `p` the item number (word `+0`) of entry `k` when `k` is above 0, and 0 otherwise, it clears a 14-byte table of used positions and sets 14 candidate words to -1. For each kind-1 entry among the `n` with `t` other than `0x270F`: when its position byte is below 14 it marks that position used and adds `27E5:033D` of `t` (1 for a type whose byte `+5` is 5, 2 when its byte `+0x0B` also has bit `0x40`) to a hand count and `27E5:0379` of `t` (byte `+2` of `t` when bit 0 of its byte `+0` is set, else 0) to a second sum. Otherwise, when the slot's 66-byte word `+0x10` shares a bit with `t`'s word `+0x0C`, and, for a 66-byte record whose byte `+0x12` is 8, `t`'s byte `+5` is `0x0C`, it makes the entry the candidate of each position whose byte in the 14-byte table at `506F:0000` (3, 11, 12, 5, 9, 2, 10, 6, 7, 1, 5, 9, 8, 4) equals `t`'s byte `+5`, when that position has none yet or the item's number is `p`. Then for each position 0 to 13 not used and with a candidate, unless the hand count plus `27E5:033D` of the candidate is above 2 or both the second sum and `27E5:0379` of the candidate are above 30, it calls `2D40:288C` with the list, the candidate, the position, the list, 0 and `0x11` for positions 2, 3 and 10 or `0x10` for the others, refills the list for the slot through `2D40:2196` when that returns 1, and starts again from the beginning. Called from overlay 173 `+13B1`. |
| `+0CDB` | `0x5FADB..0x5FB1E` | `5682:0057` | Takes a count `n` and a near item list and returns the index of the first kind-1 entry among the `n` whose position byte `+0x13` is 2, or -1. Called from overlay 173 `+19F7`. |
| `+0D1E` | `0x5FB1E..0x5FB41` | `5682:005C` | Takes a slot and a spell and returns overlay 197 `+3060` of them for a slot below 4, which lowers the count of the spell's kind and level (FND-PARTY-121), and overlay 177 `+0C36` of them otherwise. Called from `+00A9`. |

## Interpretation

`+0167` reads the weight byte that `+012F` multiplies by a draw in the enemy spell choice of
FND-PARTY-121, without the draw and without its sign. `+0924` is the N and P target selection: it
gathers the hostile slots (the kind mask `0x0F80` of FND-PARTY-095) within `0x18` of the leader and
moves the target to the next or previous one, centring on it. `+0A4E` fills a combatant's empty
worn positions from its carried items that its class mask allows, keeping to two hands and to a
limit of 30 on the second sum, and preferring the item `p`. `+0CDB` finds the item worn at
position 2, and `+0D1E` sends a used spell to the party's or the enemy's count routine. The names
stay `fn_174_<offset>` until a reading shows what the fields and tables are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list. Overlay 173 `+0000`, overlay 177 `+0C36`, `2C5F:018C`,
`28C9:31D2` and `2D40:288C` are not read here; the reading of them as a slot gather sorted by
distance, an enemy spell count decrement, a view centring, a target mark and an item move rests on
their arguments and on FND-EXE-600 and FND-PARTY-121. What `27E5:0379` measures (the byte `+2` of
a type with bit 0 of byte `+0` set) and what byte `+0x15` of the 49-byte record groups are not
settled. The last two arguments of `2D40:288C` are read here as a flag and a mode from their use
with 4 in FND-EXE-598.

## How to reproduce

Run `overlay_listing.py` for overlay 174 against the installed `DSUN.EXE` over each file range in
the table and `direct_callers.py` for each offset (`174+<offset>`), disassemble `27E5:033D` and
`27E5:0379` with `resident_listing.py`, read the 14 bytes at `506F:0000` (file offset `0x458F0`),
and compare the ranges with the rows of `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`.
