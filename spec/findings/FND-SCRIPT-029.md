---
id: FND-SCRIPT-029
title: The game runs cell and box triggers when an object enters a cell, and the one-key and key-pair triggers when the player picks an item up, talks to, attacks, looks at or uses an object, or uses one item on another, and at the end of a fight and a creature's death
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:10AE..2D40:1180
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0260..31E0:0310
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2608..28C9:26E0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00063100..0x00063160
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000633C5..0x000633F0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00063C00..0x00063C50
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000608A0..0x00060970
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000694C0..0x000695A0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069C20..0x00069D00
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00076A80..0x00076B80
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007C1F0..0x0007C300
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007CDD6..0x0007CEC0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00099860..0x00099980
tool: Capstone 5.0.9 over resident and overlay code; tools/research/exec-census/direct_callers.py, resident_listing.py and overlay_listing.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Addresses are static, load image at segment 1000, `DS` = `57E0`. The walkers are those of
FND-SCRIPT-028. `direct_callers.py` finds these far calls and no others:

| Walker | Caller | Arguments and context |
| --- | --- | --- |
| `1695:01D1` | `2D40:1179`, in the walk planner `2D40:10AE` | the slot, its cell, and kind 0 for the leader `4C13:0369`, 1 for another slot below 4, 2 otherwise, on each plan from the current cell (FND-EXPLORE-014) |
| | `31E0:02F6`, in `31E0:0121` | after `2D40:32C5` places an object, while `DS:0DAB` is not 0 |
| | overlay 182 `+1497` | each party slot 0 to 3 whose type byte is 2, after the region's script |
| | overlay 188 `+0974` | instruction `0x5E` moving an object to a cell of the current region (FND-EXE-614) |
| `1695:0290` | overlay 178 `+0B74`, overlay 191 `+0C94`, overlay 175 `+0CAE` | `(DS:426D, DS:1A32)`: the object window's GET button, taking an item off the ground, closing a container window with an item held |
| `1695:0218` | overlay 178 `+032F`, `+0B93` | `(DS:426D, slot)` after `2C5F:018C(slot, 1)`: a click whose action is talk, the TALK button |
| `1695:02AD` | overlay 173 `+2286` | `(attacker, target)` at the end of the attack routine `+1CC1`, on each path |
| | overlay 193 `+0543` | `(si, target)` in its loop over targets when `si` is not negative (FND-PARTY-201) |
| `1695:02CA` | overlay 178 `+009E` | `(DS:426D, slot)` at the start of `+0040`, before the action window opens; `+0040` returns when the script changed the slot's word `+0x1B` |
| `1695:02E7` | overlay 182 `+0D36` | `(actor, slot)` in `+0C23`, using a map object, when its third argument is 0 (FND-PARTY-250) |
| | overlay 213 `+0703` | `(DS:426D, 4F49:000B)` for an item from 30000 to 31000 used from the item menu, after `1695:0834` has bit `0x10` |
| | overlay 173 `+3E1E` | `(DS:426D, -2975)` when `4C10:0019` is not 0 (FND-PARTY-087, FND-PARTY-185) |
| | overlay 206 `+0E9D`, overlay 201 `+098F`, overlay 197 `+2CD0` | `(slot, -2976)` for a creature that dies (FND-RNG-050, FND-RNG-060) |
| `1695:0304` | overlay 175 `+0D66`, overlay 189 `+21B6` | `(4E71:0B44, DS:1A32, 4F49:000B)`: the held item dropped on an inventory item, after `1695:0834` has bit `0x20`; overlay 175 shows `NOTHING HAPPENS` (`DS:05E3`) when it returns 0 |
| | overlay 191 `+00FC` | `(4C13:0369, DS:1A32, object)`: the held item dropped on a map object; on a 0 return it drops the item |
| `1695:0834` | `28C9:2692`, in the pointer image routine `28C9:2608` | keeps the interact image when the mask has a bit of `0x1D` |
| | overlay 178 `+0059` | masks with `0x1D` to choose the action window's buttons |
| | overlays 175 `+0D20`, `+0D35`, 189 `+2116`, `+212B`, 213 `+06A3` | before the `1695:0304` and `1695:02E7` calls above |
| `1695:000B` | overlay 182 `+142E` with `DS:140C`, overlay 187 `+1306` | the region gate and the region change after a region load |

Overlay 197 `+2CBE` stores `DS:444B` in `4C13:0363` just before its call of `1695:02E7`, which
stores its own second argument there.

`script_decode.py --all` finds the key -2975 in 50 shipped `0x6D` instructions and -2976 in 14.

## Interpretation

Cell and box triggers fire when any object, the party included, enters a cell: on each step the
planner takes, when an object is put on the map, when the party is placed after a region's
script, and when a script moves an object. The one-key lists answer the player's actions: head
`0x0D` picking an item up, `0x0F` talking, `0x11` attacking, `0x13` looking at an object, `0x15`
using an object or an item, and `0x17` using one item on another in either order. Two fixed keys on
the use list mark the end of a fight and a creature's death. Overlay 197's store of the attacker in
`4C13:0363` has no effect on the script.

## Alternatives

The event names come from what the calling windows do (FND-EXE-598, FND-EXE-604, FND-EXE-622) and
from the call sites; `DS:426D` as the acting character, `DS:1A32` as the item in the hand,
`4E71:0B44` as the character whose inventory is shown and `4F49:000B` as a scratch slot for an
item rest on other findings. Overlay 193 `+0543`'s `si` and target, and overlay 182 `+142E`'s
condition (FND-COMBAT-053), were taken from those findings. What the callers of `1695:01D1` other
than the planner do with its result was not read.

## How to reproduce

`direct_callers.py <dsun> 1695:01D1 1695:0218 1695:0290 1695:02AD 1695:02CA 1695:02E7 1695:0304
1695:0834 1695:000B 1695:0235`; `resident_listing.py <dsun>` and `overlay_listing.py <dsun>` over
the locations above; `script_decode.py --all`, counting the keys of `0x6D`.
