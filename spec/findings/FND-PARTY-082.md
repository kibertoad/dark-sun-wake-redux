---
id: FND-PARTY-082
title: Overlay 210 +0B66 lowers each class level by one, sets the experience to half the sum of DATA 1000 words at the old level and the one below, and then raises again every level that experience reaches, taking a hit die roll off the greatest hit points for each class it leaves lowered; with the generation screen, the class change and the level gain it is the only code that stores a details record's level bytes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00095AC6..0x00095D9A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00082330..0x00082342
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000789F4..0x00078A16
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, resident_listing.py, direct_callers.py, trampoline_target.py, field_stores.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Records, slots, `DATA` 1000 and its rows are as FND-PARTY-081 gives; "column `c`"
means the word overlay 185 `+0000` returns for 1000, the class's row and `c`, without the factor
of 100 the comparisons use.

**Overlay 210 `+0B66`** (trampoline `57B9:0070`) takes a slot. It reads the combatant record
numbered by the slot's combatant word and the details record numbered by that combatant record's
word at `+0x04` (`+0B90..+0BCC`).

1. For each class position 0 to 2 of that record, only position 0 when the origin byte is 1, with
   a nonzero class byte and level `L` (`+0BCC..+0BF4`): it keeps the greatest column `L` over the
   positions, starting from 0 (`+0BF4..+0C38`). When `L` is not 1 it takes column `L - 1`,
   subtracts 1 from the level byte at that position of the details record numbered by the slot,
   and sets a result flag to 1 (`+0C38..+0C91`); when `L` is 1 it takes 0 (`+0C91..+0C99`). It
   keeps the least value taken over the positions, starting from 2,000,000,000 (`+0C99..+0CAB`).
2. It stores the sum of the greatest and the least, times 50, in the experience dword of the
   details record numbered by the slot (`+0CB4..+0CCF`).
3. For each position 0 to 2 of that record with a nonzero class byte, with no test of the origin
   (`+0CCF..+0CEE`): when column `L` for the position's level, times 100, is not above the
   experience it adds 1 to the level byte (`+0CEE..+0D4D`). Otherwise it calls `28C9:391D`
   (`roll_sum`) with 1 and the hit die byte of the class's row of the table in segment `55BE`,
   adds 1 to a count, raises the roll to the byte at `55BE:0038` plus the details record's byte
   at `+0x17` when that is greater, doubles it when the origin byte is 5, and adds it to a total
   (`+0D4D..+0DD8`), the steps FND-PARTY-074 gives for overlay 210 `+0000`.
4. It subtracts the total divided by the count (`idiv`, signed) from the details record's word
   at `+0x08` (`+0DE1..+0DF7`), and when the word at `+0x00` of the combatant record numbered by
   the slot is above that (unsigned) stores the word at `+0x08` there (`+0DF7..+0E33`). It returns
   the flag (`+0E33..+0E3A`).

It writes nothing at `+0x0A` or `+0x39..+0x3B`. `direct_callers.py` finds one call, at overlay 195
`+0BC9`, which pushes `SI` after testing that it is below 4 (`+0BC0..+0BCF`).

**Stores to the level bytes.** `field_stores.py` with `--es` and the displacements `1E`, `1F` and
`20` finds these byte stores:

- overlay 183 `+136A`, `+136F`, `+1374`, `+13C4`, `+13D2` and `+14BB`, in `+1323`, through the
  generation details record at `DS:1429` (FND-PARTY-065);
- overlay 209 `+1831` and `+1890`, in `+17DC`, the class change (FND-PARTY-083);
- overlay 210 `+07CB` in `+0740` (FND-PARTY-081), and `+0C86` and `+0D46` in `+0B66`;
- overlay 190 `+06D1`, in `+06B4`, which stores 0 at `+0x1E` of its far pointer argument when
  `1000:40D7` returns more than 30 for it (`+06BB..+06D6`);
- `28C9:18AC`, overlay 173 `+1FF4`, overlay 177 `+04D4` and overlay 204 `+26A2` and `+26D5`,
  each at `+0x1F` or `+0x20` of a combatant record in the table at `DS:19C9`, and overlay 177
  `+0043` at `+0x20` of a 33-byte record in segment `4D62`.

The other hits are dword additions at `+0x20` through the far pointer `DS:9D9F` in segment
`37FC` (`+0264`, `+04A4`, `+0740`, `+0A89`, `+0CAD`), the same bytes decoded one byte later
(`2AA0:0003`, `3822:0005`, `3846:0005`, `3870:0001`, `38A4:000A`, `38C6:000E`), and `48B0:000F`,
decoded from inside the instruction at `48B0:000E`.

## Interpretation

`+0B66` is a level drain. For a character with one class at level `L` above 1 it sets the experience
to halfway between the start of level `L` and the start of level `L + 1`, lowers the level to `L -
1`, and then raises it back to `L`, since the experience is above the start of level `L`; no roll is
taken, the count stays 0 and the `idiv` divides by 0. At level 1 it halves the experience at which
level 2 starts, keeps the level and takes one roll off the greatest hit points. With several classes
the experience is set from the greatest of the next-level starts and the least of the current-level
starts, and a class stays lowered only when that figure is below the start of its old level; for a
human it lowers only the current class but the second pass can raise a former class when the new
experience reaches the start of its next level. Halfway through the level below would need columns
`L - 2` and `L - 1`.

Since the drain leaves the word at `+0x0A` and the greatest levels at `+0x39` as they were,
regaining a lowered level at which the total of the levels equals the total of the greatest
levels makes overlay 210 `+0000` roll again (FND-PARTY-081), adding a die to `+0x0A` that the
drain did not take away.

In play the level bytes of a party member's details record change only in `+0740` (one level up),
`+0B66` (the drain) and the class change of overlay 209 (FND-PARTY-083); overlay 183 writes the
generation record, and the other hits write other records.

## Alternatives

- What the run-time does on a divide by 0 was not read; the executable holds the run-time's
  `Divide error` and `Abnormal program termination` texts at file `0x4D048`.
- When overlay 195 `+0BC9` runs, and with which slot, was not read; it may be unreachable, or
  reachable only for characters with several classes.
- Stores through an index register only, or block copies into a details record such as the load
  of FND-PARTY-051 and the copies of overlay 184 `+1648` and `+1DCE`, are not in this search.
- Overlay 190 `+06B4`'s argument was taken to be a string because of the length test; its callers
  were not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 210 95AC6 95D9A`,
`195 82310 82350`, `190 789E0 78A20`, `173 5C8E0 5C918`, `177 62110 62147`, `177 625A0 625D8` and
`204 8E440 8E4A0`; `resident_listing.py <dsun> 28C9:1890..28C9:18B0 37FC:0250..37FC:026A
48B0:0000..48B0:0014`; `direct_callers.py <dsun> 210+0B66`; `trampoline_target.py <dsun>
57B9:0070`; and `field_stores.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv --es 1E 1F
20`.
