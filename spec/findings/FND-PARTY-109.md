---
id: FND-PARTY-109
title: A spell of target type 4 cast from overlay 211's list goes through overlay 208's target cursor, which takes a combatant of a side the caster's side is hostile to, or of any side while a Shift key is held, and overlay 193 +003C passes a single chosen target to the hit routine with no side test
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000972CA..0x0009755A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0009755A..0x0009756E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000929DA..0x00092A73
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00092A73..0x00092A83
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00092EA2..0x0009304D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00091E42..0x00091F6C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009241C..0x00092539
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00092539..0x00092545
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005E83A..0x0005E999
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44B6:0011..44B6:0018
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E32C..0x0007E7D3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062592..0x0006260A
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, resident_listing.py, direct_callers.py, data_bytes.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. `2C5F:0003` with `n` returns the `DATA` resource numbered `n` (FND-PARTY-096); the
side byte below is the byte at `+0x15` of the FMT-COMBAT-001 record that the word at
`4F49:0C34 + 3 * c` selects for combatant `c`, read through the far pointer at `DS:19C9`.

**The cast in overlay 211.** Overlay 211 `+10F4` runs the events of the window that lists the
current slot's spells or psionic powers: the slot is the word at `4E71:0B44`, the list the words at
`DS:9C20`, and the event codes go through five words at `cs:174A` (2, 4, 0x20, 0x40 and 0x80).
Event 0x40 sets the byte at `DS:9D76`, and event 0x20, when that byte is set, takes the list's
entry `n`, plus 235 when the byte at `DS:43E8 + slot` is 3, and, after its tests for a power that
cannot start, a caster who has to rest, one who was hit and one who cannot cast
(`+14BA..+15B0`), reads byte `+0x0C` of `DATA n` (`+1602..+1627`):

- 0 or 7: it calls overlay 193 `+2290` with the slot, the slot again and `n` (`+1629..+1673`).
- 3: it stores `n` in the word at `DS:43F5` and sets the byte at `DS:43F7` (`+16DB..+16F8`).
- any other value: it calls overlay 208 `+1612` (trampoline `57A6:0066`) with the slot, `n` and
  three further words (`+16F8..+1741`).

**The cursor in overlay 208.** `+1612` stores the slot in the word at `DS:9BE0`, `n` in the word
at `DS:9BE2` and -1 in the word at `DS:9BDE`, and sets the words at `DS:1440` and `DS:4596` to 5
(`+1612..+169D`). `+114A` (trampoline `57A6:006B`, reached far from `28C9:0C85`) takes a point on
the map and switches on byte `+0x0C` of the stored `DATA` record through eight words at
`cs:11E3`; for 4 it calls `+16AB`, which stores the point in the words at `DS:9BDC` and `DS:9BDA`
and calls `+16F1`. When `+16AB` returns nonzero, or else while the value of `44B6:0011` has one
of its low three bits set and the byte at `DS:143C` is not 0, `+114A` calls `+1761`
(`+114A..+11E2`). `+16F1` calls `+05B2` with the words at `DS:9BE0` and `DS:9BE2` and the
addresses of the point, and when the result `c` is not below 0 and byte `+0x0C` of `DATA n` is 3,
4, 5, 7 or 8, it stores `c` in the word at `DS:9BDE` and returns 1 (`+16F1..+1760`). `+1761` calls
overlay 193 `+2290` with the words at `DS:9BE0`, `DS:9BDE` and `DS:9BE2` first (`+1761..+17A4`).

**The pick.** `+05B2` takes the caster and `n`. It calls `+0B8C` with `n` and the caster's side
byte, and, when byte `+0x0C` of `DATA n` is 3, 4, 5, 7 or 8, calls overlay 173 `+3F1A` with the
caster, the point, a range and that result as a mask, and returns what `+3F1A` returns
(`+05B2..+06DC`). Overlay 173 `+3F1A` finds the object at the point and returns -1 when there is
none or it is not of kind 2, -2 when bit `s` of the mask is clear, `s` being the object's side
byte, -5 when it lies outside the bounds in the words at `DS:14D7` to `DS:14DD` or farther than
the range, and -4 when `1B0B:00A4` returns nonzero for the caster's cell and the object's; otherwise it returns
the object (`+3F1A..+4078`).

**The mask.** `+0B8C` takes `n` and a side `s`. It returns 0 for `n` = -1. It forms two masks
from three groups of sides, 0x71, 0x184 and 0x608:

- the first is the group holding `s`, with 0x71 for `s` = 0 and 0x800 for `s` = 11;
- the second is 0x71 when `s` is in 0xF80, 0x184 when `s` is in 0xC58, and 0x608 when `s` is in
  0x964, with 0xF80 for `s` = 0 and 0x800 for any `s` other than 11 (`+0B93..+0C66`).

It then calls `44B6:0011`, which returns the keyboard flags of interrupt `0x16` function 2 with
the high byte cleared; when bit 0 or 1 is set it returns 0xFFFF. Otherwise it switches on byte
`+0x0C` of `DATA n` less 3 through six words at `cs:0CA9`: 3 and 7 return the first mask, 4 the
second, and 5, 6, 8 and any other value 0xFFFF (`+0C69..+0CA8`).

**The hit.** Overlay 193 `+2290` sends a spell number below 235 to overlay 177 `+0492`
(FND-PARTY-108), which calls overlay 193 `+0000` with its first, second and third arguments first
(`+04D8..+0503`). When the second argument, the target, is not -1, overlay 193 `+003C` makes it the
one entry of its target list; when it is -1 it fills the list through overlay 208 `+0000` with the
mask `+0B8C` returns for the spell and the caster's side (`+041B..+045D`). It calls the hit routine
for each entry (FND-PARTY-107). Before the list it leaves the routine only when byte `+0x17` of the
`DATA` record is not -1 and the low four bits of byte `+2` of the record of `51F1:0000` it selects
are not 1 (`+03DD..+0405`).

`data_bytes.py <install>/RESOURCE.GFF` gives, for `DATA` 104 and 225, the byte at `+0x0C` as 4,
at `+0x17` as -1 and at `+0x19` as 59.

## Interpretation

A party member who casts a spell of target type 4 from the spell list, such as `DATA` 104 or
225, picks its target with the map cursor. The cursor takes only a combatant of a side the
caster's side is hostile to, unless a Shift key is held as the target is clicked, when it takes a
combatant of any side, the caster's companions included. The spell then reaches the hit routine
with that combatant as its target, whatever its side.

## Alternatives

- The party members' side byte was not read; without Shift the cursor takes another party member
  only if the caster's side and the target's side fall in the second mask, which depends on it.
- Whether a party member's spell list at `DS:9C20` can hold `DATA` 104 or 225 was not read.
- The input dispatch around `28C9:0C85` was not read, so when `+114A` runs is taken from its
  callers' stores of `DS:4596` and `DS:1440`, which `+1612` sets to 5, only.
- What the hit routine does for a party member after its saving throw is FND-PARTY-108's reading.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 211 96F04 97560`,
`208 929DA 92A80`, `208 92EA2 93060`, `208 91E42 91F6C`, `208 9241C 92548`, `173 5E83A 5E999`,
`193 7E32C 7E800` and `177 62592 6260C`; `resident_listing.py <dsun> 44B6:0011..44B6:0018
28C9:0C81..28C9:0C90`; `direct_callers.py <dsun> 193+2290 208+1612 208+114A 208+16AB 208+16F1
208+1761 208+05B2 208+0B8C`; read the words at `cs:174A` of overlay 211, `cs:11E3` and `cs:0CA9`
of overlay 208; and `data_bytes.py <install>/RESOURCE.GFF C`, `17` and `19`.
