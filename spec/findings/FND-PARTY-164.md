---
id: FND-PARTY-164
title: The saving throw modifier overlay 197 +0206 subtracts 100 against CHAOS for a target with no fighter, gladiator or ranger level from overlay 176 +076C, and the saving throw's byte sum and unsigned comparison make a modifier near -100 pass the save
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00083A89..0x0008428F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00083650..0x0008365C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066EA6..0x00066ED1
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py; a scratch script that prints the byte at +0x1F, the word at +0x1A and the name at +0x20 of DATA resources read through the GFF directory as details_chunks.py reads a directory)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Overlay 197 `+0206` is the saving throw modifier that overlay 179 `+2A46` adds to its
roll (FND-PARTY-099, FND-PARTY-163). It keeps its first argument, the target's slot, in `si`, and
a sum in `di`; `bp+0x14` is the `DATA` number `n` of the effect. At `+0250..+0259` it stores in
`bp-6` the result of `27E5:0154` for `si`, which is the target's level (FND-PARTY-081). Overlay
176 `+076C` (trampoline `5691:0061`) returns the level of the first class position holding a
class code, with no test of the origin byte (FND-PARTY-162). Class codes are as
`character_class` gives: 1 to 4 cleric, 5 to 8 druid, 9 fighter, 10 gladiator, 13 to 16 ranger.

After the general terms, the routine tests `n` against four numbers:

1. **56** (`+0689..+0A83`). It takes the greatest of the results of `+076C` for `si` and the codes
   13, 14, 15, 16 and 9, then compares it with the result for code 10, counted as 0 unless it is
   above 0, and keeps the greater. When the value kept is not above 0 it subtracts 100 (0x64)
   from `di` (`+0A7F..+0A83`).
2. **60** (`+0A86..+0A96`). It adds `bp-6` less the byte argument at `bp+0x16` to `di`.
3. **57** (`+0A98..+0AA4`). It subtracts 100 from `di` when `bp-6` is not above 6.
4. **21** (`+0AA7..+0E87`). It returns 100 when `bp-6` is at least 6. Otherwise it takes the
   greatest result of `+076C` over the codes 1 to 8 (`+0ABC..+0E25`). When that is not above 0
   it returns -100 (`0xFF9C`, `+0E84`). When it is above 0 it tests the origin byte at `+0x12` of
   the target's details record: not 3 returns -100 (`+0E51..+0E56`); 3 is then compared with 4
   (`+0E7D..+0E82`), which fails, and returns -100. No path of this case returns `di`.

Otherwise the routine returns `di` (`+0E89`).

**The saving throw.** At overlay 179 `+2BA6..+2BAB`, `+2A46` adds the low byte of the modifier to
the roll byte at `bp-2`, then adds the effect's signed 4-bit bonus (`+2BAE..+2BC3`). At
`+2BC6..+2BCC` it compares that byte with the save byte at `bp-1` by `cmp al, [bp-1]` and `jb`:
below goes to `+2BD1`, which returns 0, and otherwise to the return of 1 at `+2AA7`. Before the
modifier, a roll of 1 has returned 0 and a roll of 20 has returned 1 (`+2B64..+2B70`), and a roll
is doubled when the word at `+0x1A` of `DATA` `n` has a bit of 0x86 (`+2B79..+2B85`), so the byte
holds 2 to 19 or 4 to 38.

**The records.** In `RESOURCE.GFF`, `DATA` 21 is `SCARE`, with 0xA0 at `+0x1F`; 56 is `CHAOS`,
with 0xA1; 57 is `CLOUD KILL`, with 0x3C; and 60 is `DISMISSAL`, with 0xA1. `+2A46` returns 0
before the roll when bit 0 of that byte is clear (FND-PARTY-099, step 1).

## Interpretation

Only `CHAOS` and `DISMISSAL` of the four have bit 0 set, so the cases for `SCARE` and
`CLOUD KILL` do not run for the shipped records: `+2A46` returns 0, no save, before it calls the
modifier.

Against `CHAOS`, a target with no fighter, gladiator or ranger level gets 100 subtracted. The
roll byte then holds the roll plus about 0x9C, from 0x9E for a roll of 2 with no other term,
and the terms read change it by tens at most, so it stays far above any save byte
the classes give (FND-PARTY-085) and the unsigned comparison finds it not below: the save
succeeds on every roll but a 1. A target with such a level rolls normally. The level comes
from overlay 176 `+076C`, so a dual-classed human whose former class is a fighter, gladiator or
ranger counts as one even while the other routines hold that class back (FND-PARTY-162).

The subtraction reads as a way to deny the save, which the byte arithmetic turns into a near
certain success. A return of 100, as `SCARE` gives a target of level 6 or more, passes the same
way.

## Alternatives

- Whether a save that succeeds spares the target from `CHAOS` rests on FND-PARTY-099's reading of
  `+2A46`'s result; the spell's own code was not read.
- The general terms of `+0206` before `+0689` add or subtract constants from 1 to 20, a multiple
  of 2 and a share of the constitution score, but also the results of `27E5:02F2`, `27E5:0324`
  and `27E5:030B`, which were not read. A sum of terms beyond about 90 in either direction would
  change the result above.
- The other calls of overlay 176 `+076C`, in overlay 197 `+1270` and overlays 174, 204 and 208,
  were not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: in `tools/research/exec-census/`, run `overlay_listing.py <dsun> 197 83606 8428F` and
`overlay_listing.py <dsun> 179 66E40 66EC0`. For the `DATA` resources 21, 56, 57 and 60 of
`RESOURCE.GFF`, print the byte at `+0x1F` and the 32 bytes at `+0x20`.
