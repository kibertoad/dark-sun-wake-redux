---
id: FND-MAGIC-005
title: The rest routine, overlay 204 +0CBA, refuses during combat, casts Cure Disease, Remove Curse and Remove Paralysis from the party's best cleric and best druid of level 5 or more and Neutralize Poison from a druid of 7 or more on every member, ends effects, sets each standing member's hit points, PSPs and spell pools to full, and adds 60 times its hour argument to the clock; a map object numbered 0x525 starts it with 8 hours and script requests 1 and 4 with 0
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008CA7A..0x0008D074
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D074..0x0008D0E2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069473..0x0006952B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C071..0x0008C07F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C0A0..0x0008C0A8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0008C487..0x0008C4F1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000864ED..0x0008654E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:06E1..2D40:06E6
tool: tools/research/exec-census/overlay_listing.py, resident_listing.py, direct_callers.py and script_decode.py at the commit that adds this finding; Capstone 5.0.9; Python 3.14.7; a short read of the 53 words at file 0x8C487 and a raw byte search of DSUN.EXE for the far-call words 0x0660:0x0020 and 0x4787:0x0020
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x8BDC0 + x` for overlay 204, `0x68850 + x` for overlay 182 and
`0x83400 + x` for overlay 197. "Member" below is a party slot that `2D40:0776` returns: started
from 9999, it returns the next of slots 0 to 3 whose combatant word `+0x06` is negative, and 9999
after the last (FND-CONFIG-133). A member's combatant record is the 49-byte record under `DS:19C9`
and its details record the 66-byte record under `DS:19C5` that the combatant word `+0x04` numbers
(FMT-COMBAT-001, FMT-COMBAT-002). Class codes are FND-PARTY-057's: 1 to 4 Cleric, 5 to 8 Druid,
11 Preserver.

**Arguments.** Overlay 204 `+0CBA` takes a flag `f` (`bp+6`), a count `h` (`bp+8`) and a slot
filter `w` (`bp+0xA`), where 0x7FFE means every member.

**The start.** When `combat_state` (`4C10:0019`) is not 0 it shows `NO RESTING DURING COMBAT` and
returns (`+0CD1..+0CEA`). Otherwise, when `28C9:2522` returns 1 (the word at `DS:0DAB`,
FND-CONFIG-117) it shows `THE PARTY RESTS` (`+0CED..+0D00`); either way it calls `28C9:2A25` and
`1BF3:4FEB` with a zero dword (`+0D03..+0D10`) and starts the first pass (`+0D13..+0D16`).
Nothing in the routine looks at other combatants, the map or a random draw before it goes on.

**The first pass** (`+0D19..+0F5F`) looks only at members equal to `w`, or at all when `w` is
0x7FFE. For such a member it sets `combat_mark` (combatant byte `+0x14`) to 1 when `f` is 1 or the
mark is 2 or 3 (`+0D3D..+0D5C`). When the mark is then 1 it asks overlay 176 `+076C` (the level
byte of the member's class position holding that code, 0 when none does, FND-CONFIG-132) for
codes 1 to 4, 5 to 8 and 11, and keeps, over all such members, the greatest of codes 1 to 4 at
`bp-6` with its slot at `bp-0xC`, the greatest of 5 to 8 at `bp-8` with its slot in `di`, and the
value for 11 at `bp-0xA` with its slot at `bp-0xE` (`+0D69..+0F4A`). A tie keeps the first slot.

**The second pass** (`+0F65..+128E`) runs over every member, whatever `w`, in the order
`2D40:0776` gives. For each member `s`, in this order:

1. Overlay 193 `+1BA3` fills the 16-byte flag block of `s` at `bp-0x1E` (`+0F65..+0F6F`).
2. When `bp-0xA` is 10 or more it calls `2D40:06E1` with the Preserver's slot (`+0F72..+0F80`);
   `2D40:06E1` sets up a frame and returns at once, changing nothing.
3. When `bp-6` is 5 or more it calls `+12B4` with the spell numbers 148, 157 and 159, target `s`,
   caster `bp-0xC` and level `bp-6`; when `bp-8` is 5 or more it calls `+12B4` with 148, 157 and
   159 and the druid's slot and level; when `bp-8` is 7 or more it calls `+12B4` with 178
   (`+0F81..+100D`, the seven calls of FND-CONFIG-119). `+12B4` saves `DS:0DAB`, sets it to 5,
   calls the hit routine overlay 179 `+0C7F` with the target, the caster, the spell, the level
   as `a` and the caster's combatant byte `+0x15` as `g` (FND-RNG-051), and puts `DS:0DAB` back
   (`+12B4..+1322`, FND-CONFIG-117). By FND-MAGIC-004's table the four numbers are `CURE DISEASE`
   (148, level 3, byte 0x30), `REMOVE CURSE` (157, level 3, 0x30), `REMOVE PARALYSIS` (159,
   level 3, 0x30) and `NEUTRALIZE POISON` (178, level 4, 0x20).
4. When `f` is 1 and the mark of `s` is not 8, it removes effect 0x47 (overlay 193 `+194F`), sets
   the mark to 1 and raises hit points (combatant word `+0x00`) below 1 to 1 (`+1010..+1064`).
5. It calls overlay 197 `+30ED` with 0xD5 and `s` (`+1064..+106D`). `+30ED` walks the 10-byte
   rows of `4E71:024D` that belong to `s`, from the index `3143:000D` gives, and ends through
   overlay 193 `+1873` each row whose word at `4E71:0251` equals its first argument
   (`+30ED..+314E`). 0xD5 is 213, `SUNSTROKE` in FND-MAGIC-004's table.
6. It removes effect codes 0x01, 0x13, 0x6C, 0x69, 0x65, 0x64, 0x52, 0x2C, 0x23, 0x21, 0x15,
   0x14, 0x0A, 0x41, 0x42, 0x44, 0x43, 0x45 and 0x46 through overlay 193 `+194F`, 19 calls
   (`+1070..+113E`), then calls overlay 197 `+29E0` with `s` and the block (`+1141..+114C`), the
   per-object effect pass of the end of combat (FND-COMBAT-060).
7. When the mark of `s` is 1 (`+114F..+116E`): it calls overlay 182 `+0B52` with `s`, which
   recounts both spell pools (FND-PARTY-111) (`+1171..+1177`); stores details word `+0x08` in
   combatant word `+0x00`, hit points (`+1178..+11B9`); stores details word `+0x0C` in combatant
   word `+0x02`, the PSI pair FND-PARTY-059 reads (`+11BC..+11FD`); and stores details byte `+0x15`
   in combatant byte `+0x19` unless the combatant byte is already below it and block byte 3 has
   bit 0x08 (`+1201..+1269`). A member with another mark skips step 7.
8. When block byte 8 has bit 0x80 it calls overlay 173 `+22C4` with `s` and 1000, the damage
   routine (FND-PARTY-086) (`+126D..+127C`). This step does not depend on the mark.

**The end** (`+1291..+12B4`, FND-PARTY-211): overlay 173 `+3E30` ends every psionic power in
effect, overlay 195 `+1515` adds `60 * h` to the double word `4C13:035B` (FND-EXE-619),
`371D:0334` runs the timed queues, queue 0 against `4C13:035B` (FND-COMBAT-060), and overlay
204 `+1D2E` with 1 fades the palette back in steps of 1 (FND-IMAGE-012).

No instruction of `+0CBA` or `+12B4` writes `DS:4469` to `DS:4505` other than through `+0B52`,
and neither calls overlay 197 `+3060`, the pool spend, whose only direct callers are overlay 174
`+0D2E` and overlay 190 `+2632` (`direct_callers.py <dsun> 197+3060`).

**The routes.** `direct_callers.py <dsun> 204+0CBA` gives the near call at `+02B9` and one far
call through trampoline `5787:0020`, at overlay 182 `+0CCC`. A raw search of the whole file for
the far-call words `0x0020, 0x0660` finds only that call, preceded by `9A`, and none for the
resident form `0x0020, 0x4787`.

- Overlay 182 `+0C23` takes an actor, a placed slot and a byte. When `28C9:2839` of the slot is
  below 9999 and the slot record's word `+0x16` is not 0x8000 it reads `object_number`, word
  `+0x1B` of the slot's record at `DS:67BB` (FND-PARTY-250). A number from 2400 to 2949 takes the
  door path; otherwise, for 0x525 (1317), it calls the rest routine with `f` 0, `h` 8 and `w`
  0x7FFE (`+0CBD..+0CD1`) and then `31E0:1808` with the slot. Overlay 182 `+0C23` is reached
  from overlay 178 `+0040`, the click on a map object, when the pointer's value `b` is 0x10 and
  `a` is 0, and from the `0x3BC6` and `0x3BCB` buttons of overlay 178's object window
  (FND-EXE-604, FND-CONFIG-077).
- Overlay 204 `+0165` dispatches the requests of script opcode 0x22 through the 53 words at
  `cs:06C7` (file `0x8C487`) on the request less 1. Request 1 goes to `+02B1`, which calls the
  rest routine with `f` 1, `h` the low word of the first dword operand and `w` the slot operand;
  request 4 goes to `+02E0`, the same with `f` 0 (`+02B1..+02BF`, `+02E0..+02E8`).
  `script_decode.py <GPLDATA.GFF> <OBJEX.GFF> --all` finds twelve such instructions, all with
  both dwords 0:

| Request | Script and offset | Slot operand |
| --- | --- | --- |
| 1 | GPL 75 `103C`, GPL 84 `0631` | `var9[39]` |
| 1 | GPL 126 `00A0`, GPL 182 `01AB`, GPL 329 `0350`, `0ABB`, `0ECA` | 32766 (0x7FFE) |
| 1 | GPL 241 `0A36`, `155F` | `var9[38]` |
| 4 | GPL 184 `17E5` | 32766 |
| 4 | GPL 218 `028A`, `02C6` | `var9[39]` |

## Interpretation

The rest the manual ties to the fire ring is overlay 204 `+0CBA`, and the map object the game
treats as the fire ring is the one whose `object_number` is 1317: clicking it, or choosing the
object window's button for it, rests with 8 as the hour count and every member as a possible
healer. Scripts rest the party the same way with no time passing; request 1 also stands every
member up, at 1 hit point at least before the heal, unless its mark is 8.

Resting:

- is refused only during combat; the routine tests no enemy distance, draws no number and has no
  exit between its message and its end, so nothing in it stops a rest that has started;
- heals by setting hit points to the details maximum for every member whose `combat_mark` is 1
  after the first pass, whether or not anyone casts; members marked 2 or 3 are first set to 1;
- casts no healing spell. The cure spells it casts are the condition cures: from the party's
  highest-level cleric of level 5 or more, `CURE DISEASE`, `REMOVE CURSE` and `REMOVE PARALYSIS`;
  then the same three from the highest-level druid of level 5 or more; then `NEUTRALIZE POISON`
  from that druid at 7 or more. Each is cast on every member in member order, through the hit
  routine with the class level as the caster level, so the spell's own effect decides what it
  changes. The levels 5 and 7 are those at which RULE-MAGIC-001's cast window first reaches
  spell levels 3 and 4. A cleric gets no `NEUTRALIZE POISON`, which fits its level 4 Cosmos byte
  0x20 that a cleric's mask lacks (FND-MAGIC-004). These casts spend no pool byte;
- refills PSPs to the details maximum and both spell pools to the recount of overlay 177 `+05B7`,
  for members marked 1;
- puts byte `+0x19` back to its stored value unless block byte 3 bit 0x08 keeps it lower;
- ends the members' `SUNSTROKE` effects, 19 effect codes, the end-of-combat effect pass, every
  psionic power in effect, and then whatever timed effects the clock's advance makes due;
- deals 1000 damage to a member whose block byte 8 has bit 0x80.

A Preserver of level 10 or more makes one more call, to a routine that returns at once, which
looks like a cut feature.

The game keeps no memorized spells. A wizard knows the spells set in its `SPST` bits
(`4D62:039C + 15 * slot`, FND-PARTY-104, FND-PARTY-110) and a priest every spell of its spheres
(FND-MAGIC-004); a rest sets each level's pool to a count and a cast takes one from the pool of
its level, so any known spell of a level can be cast while that level's pool is above 0.

## Alternatives

- That object number 1317 is the fire ring rests on the manual's description and on it being the
  only object number that starts a rest; which art it is and which pointer mode gives `b` 0x10
  were not read. A run that clicks the Look pointer on a fire ring settles it.
- The unit of `4C13:035B` is open. A combat round adds 60 to it (FND-RNG-076), and the fire ring
  adds 480. Read as hours, the 8 makes the clock count minutes and a round an hour; read as
  rounds, the rest lasts 8 rounds. A run that reads `4C13:035B` before and after a rest, or a
  reading of what prints the time, settles it.
- What `combat_mark` 2, 3 and 8 stand for, what effect codes 0x47 and the 19 others are, what
  block byte 3 bit 0x08 and byte 8 bit 0x80 mean, and which ability byte `+0x19` is (FND-PARTY-070
  copies the six bytes at `+0x19` to details `+0x15` at generation) were not read.
- Whether the hit routine, given these four spells with `DS:0DAB` at 5, reaches the pool spend or
  changes the target only when it has the condition, was not traced (Q-MAGIC-006).
- `28C9:2A25` and `1BF3:4FEB` at the start, overlay 197 `+29E0`'s full effect on a party member,
  and the transitive effects of `371D:0334`'s queue run (kind 3 unloads an object) were not read;
  a timed script started by the queue could still follow a rest.
- `var9[38]` and `var9[39]` in the scripts, and what each of the twelve scripts is in play, were
  not read.
- Overlay 176 `+076C` also returns a held-back former class's level (FND-PARTY-165), so a
  dual-classed human's old cleric or druid level counts as a healer here.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python and `<dsun>` the installed
`DSUN.EXE`: `overlay_listing.py <dsun> 204 8BDC0 8E660`, `182 68850 6AD08` and `197 83400 86F67`,
keeping `+0165..+0300`, `+0CBA..+1322` of 204, `+0C23..+0CDB` of 182 and `+30ED..+314E` of 197;
`resident_listing.py <dsun> 2D40:06E1..2D40:0776`; `direct_callers.py <dsun> 204+0CBA 204+12B4
182+0C23 197+3060`. Read the 53 words at file `0x8C487` and keep the entries `0x02B1` (index 0)
and `0x02E0` (index 3). Search the file for the bytes `20 00 60 06` and `20 00 87 47` and check
that each hit follows a `9A`. Run `script_decode.py <GPLDATA.GFF> <OBJEX.GFF> --all` and keep the
lines whose instruction is `22 1,` or `22 4,`, nested or not. Look up 148, 157, 159, 178 and 213
in FND-MAGIC-004's table.
