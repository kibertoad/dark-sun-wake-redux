---
id: FND-PARTY-091
title: The attack routine overlay 173 +1CC1 reads the combatant byte at 0x16 as the attacker's base, lowers it by bonuses and passes it to +2560, which hits on a roll of 20, or on a roll other than 1 that is at least that value less a target value
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005C6DD..0x0005C6F1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005C758..0x0005C798
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005C93A..0x0005CA08
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D052..0x0005D0E9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005CED8..0x0005CEEA
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/field_reads.py, overlay_listing.py, trampoline_target.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. The combatant records are the 49-byte records through the far pointer `DS:19C9` and
the details records the 66-byte records through `DS:19C5` (FND-PARTY-081); overlay 210 `+0740`
stores 20 less the best class group term in the combatant byte at `+0x16` (FND-PARTY-085).

**The search.** `field_reads.py <dsun> <inventory> --es --table 19c9 16` lists the instructions
that read a displacement `0x16` through ES within six instructions after a text naming `19c9`. It
lists one: overlay 173 `+1DC9`. As a control, the same search for `14` lists 75, among them overlay
173 `+23EF` and `+3367`, whose reads of the `combat_mark` FND-PARTY-086 and FND-PARTY-089 give.

**Overlay 173 `+1CC1`** (trampoline `5671:0057`). It loads the combatant record numbered by its
local at `bp-0xE`, the attacker, and stores the signed byte at `+0x16` in a local word, the base
(`+1DBD..+1DD1`). On the path that reads an item record through `DS:19C1`, it takes that record's
signed byte at `+0x16` plus the result of `27E5:03B1` as a bonus (`+1E38..+1E5D`), and when
its word argument at `bp+0xE` is at most 1 adds the result of `+3514` (`+1E5D..+1E75`). Later it
subtracts 2 from the base in two cases (`+202E`, `+205F`), subtracts the result of overlay 197
through `575A:0066` (`+2099..+20A4`), adds the word at `DS:143A` less 1 to the bonus when the
attacker is not a party slot (`+20A4..+20B0`), and calls `+2560` with the base less the bonus as
its third word argument and the result of `+2C00` as its fourth (`+20B4..+20E5`).

**Overlay 173 `+2560`.** It draws `1000:0822`, multiplies the signed result by 20, divides it by
32,768 (signed), adds 1 and keeps that as the roll, also storing it at `DS:420E` (`+2737..+2756`).
After a call to `+2A9A` it goes on to the hit path when the roll is 20; it skips it when the roll
is 1; otherwise it goes on when the third argument less the fourth is at most the roll, signed
(`+2768..+277F`). The hit path calls `+2A5D` with three item values, takes at least 1 from it,
passes it through overlay 197 `575A:0057` for a single attack, adds it to a local total and
stores 1 at `DS:054F` (`+277F..+27C9`). Earlier, `+2560` reads the details record's byte at
`+0x24`, or an item record's byte at `+7` when its word argument at `bp+0x16` is above 1, into a local
(`+25B8..+25CA`).

## Interpretation

The combatant byte at `+0x16` is the attacker's THAC0: the attack hits when a roll of 1 to 20 is
at least the THAC0, lowered by the attack's bonuses, less the value `+2C00` gives for the target,
which reads as its Armor Class. A roll of 20 always hits and a roll of 1 always misses. This is
the comparison RULE-COMBAT-002 gives from the manual and the FAQ, with a roll equal to the needed
value hitting, as the manual says.

## Alternatives

- `+2C00` giving something other than the target's Armor Class: it was not read.
- The draw `1000:0822` and its reduction are RULE-RNG-001's; which reduction the multiply and
  divide match was not compared here.
- A reader of the byte at `+0x16` through a pointer loaded more than six instructions before it,
  or through another register than ES: the search would not list it.
- What the details byte at `+0x24` read at `+25C4` counts: it was not followed.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `field_reads.py <dsun>
../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv --es --table 19c9 16`, and the same with `14` as
the control; `overlay_listing.py <dsun> 173 5C5E1 5CBE4` and `173 5CE80 5D3BA`; and
`trampoline_target.py <dsun> 5671:0057`.
