---
id: FND-MAGIC-010
title: Only generation, overlay 184 +06CD, and the level-gain pass, overlay 210 +0131, store the greatest psionic points, details word +0x0C; the pass runs for every standing party member each time overlay 210 +08BC runs, and generation gives a character without a Psionicist class the score bonuses alone
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006D75D..0x0006D868
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00095091..0x00095193
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009581C..0x00095872
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00095A91..0x00095A97
tool: tools/research/exec-census/overlay_listing.py, direct_callers.py and field_stores.py at the commit that adds this finding with coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x6D090 + x` for overlay 184 and `0x94F60 + x` for 210. The
tables are the signed bytes at `4E4F:00EF`, `00F7`, `00FF` and `0107` plus a score, whose values
for scores 15 to 25 FND-PARTY-081 gives: call them `C`, `I`, `W` and `V`.

| Score | 15 | 16 | 17 | 18 | 19 | 20 | 21 | 22 | 23 | 24 | 25 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `C` (constitution) | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 0 | 1 | 2 |
| `I` (intelligence) | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 0 | 20 | 22 | 24 |
| `W` (wisdom) | 20 | 22 | 24 | 26 | 28 | 30 | 32 | 34 | 0 | 1 | 2 |
| `V` (wisdom, per level) | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 8 | 10 |

**The stores.** `field_stores.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv --es C` lists 53
writes with displacement 0x0C through `ES` and 5 more outside the inventoried ranges or inside other instructions. Of
the 53, those whose base is a 66-byte details record are overlay 184 `+06D4`, `+06FC`, `+0722`,
`+0748`, `+07A5` and `+07D2`, through `DS:1429`, and overlay 210 `+022B`, through the details
record `+0131` loads; the others store into the 49-byte combatant records (`DS:19C9`,
`DS:142D`), the 23-byte item records (`DS:19C1`) or records of other routines.

**Generation, overlay 184 `+06CD`** (`+06CD..+07D8`): it stores 0 in the details word `+0x0C` at
`DS:1429`, adds `C`, `I` and `W` of the constitution, intelligence and wisdom bytes `+0x1B` to
`+0x1D` of the combatant record at `DS:142D` for each of 15 or more, and, when a class byte of the
details record is 6 (generation code for Psionicist, FND-PARTY-073), keeps the first such class's
level `L` in `DS:42C4`, adds `(L - 1) * 10` and, when the wisdom is above 15, `V(wisdom) *
(L - 1)` (`+06D0..+07D2`). Its callers are `+05C0` in the roll routine and overlay 183 `+0769`
(`direct_callers.py <dsun> 184+06CD`).

**Level gain, overlay 210 `+0131`** (`+0131..+0233`): as FND-PARTY-081 reads it, `C`, `I` and
`W` for scores of 15 or more, then `(p - 1) * 10` plus, for a wisdom of 15 or more,
`V(wisdom) * (p - 1)` when `p`, the result of `27E5:00C1` with code 12, is above 0, and otherwise
4 times `27E5:0154`; when the details word `+0x0C` is below the total it adds the difference to
the combatant word `+0x02` and stores the total (`+0212..+022F`). `+08BC` (`+08BC..+0B44`) runs,
for each slot 0 to 3 whose state byte is 2, whose `combat_mark` is at most 2 and whose details
key `+0x0E` is not 0, the class positions and then `+0131` with the slot at `+0B31..+0B37`, whether
or not a level was gained. `+08BC` runs at the end of a fight and after each experience award
outside one (FND-PARTY-081).

## Interpretation

A character's greatest PSPs are, with `B` the sum of the bonuses `C`, `I` and `W` for each score
of 15 or more:

- for a psionicist of Psionicist level `L`: `B + 10 * (L - 1) + V(wisdom) * (L - 1)`, the wisdom
  term only for a wisdom of 15 or more, and `V(15)` is 0, so generation's "above 15" and the level
  gain's "15 or more" agree;
- for any other character in play: `B + 4 * level`, the level being `27E5:0154`'s;
- for any other character at generation: `B` alone, until the first run of `+08BC` raises it to
  `B + 4 * level`.

A new single-class psionicist starts at level 7 (FND-PARTY-065), so with every score below 15 it
has 60 PSPs; at levels 1 to 15 the formula gives 0, 10, 20 and so on to 140 plus `B` and the
wisdom term. The level gain never lowers the points, so a score that falls or a class that is
held back leaves the earlier greatest in place; and since `+08BC` runs after every fight, a change
in score or class shows in the greatest points at the next fight's end whether or not a level
came with it. The bonus tables, read as indexed by the score, give 0 for constitution 23 and
intelligence 22, which looks like a table read from the wrong base; the rule takes the bytes as
the game reads them.

## Alternatives

- Stores into a details record through a register other than a displacement of 0x0C, block
  copies (a `CHAR` load, FND-PARTY-052) and stores through `DS` were not searched; the premade
  and saved characters carry their own word.
- The wisdom bonus `W` reads the record at `DS:142D`, which FND-PARTY-084 finds is the slot's own
  record in play only after a new Psionicist level; on other runs of `+0131` it may be another
  character's wisdom.
- The current points start at the greatest only where overlay 184 `+05C3..+05DC` copies them
  (FND-PARTY-059) or a rest sets them (FND-MAGIC-005); generation's own copy condition was not
  traced further.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python: `field_stores.py <dsun>
../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv --es C` and keep the hits whose preceding
instructions load `DS:19C5` with a multiplication by 0x42, `DS:1429`, or are overlay 210 `+022B`;
`overlay_listing.py <dsun> 184 6D75D 6D868`, `210 95074 95174` and `210 957FF 95A9F`;
`direct_callers.py <dsun> 184+06CD 210+0131`.
