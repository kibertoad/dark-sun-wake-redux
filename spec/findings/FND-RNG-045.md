---
id: FND-RNG-045
title: No stored far pointer, segment alias or declared computed transfer of DSUN.EXE names the generator, the seed setter or the four reducers, so they are reached only by direct calls
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0811
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0822
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2834:0519
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2834:061C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:391D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3A03
tool: tools/research/exec-census/far_pointer_words.py at commit 5c7baa33 and alias_far_words.py, with q007_dsun_apfm_census.json and newgame_declarations.json; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

The input is the installed `DSUN.EXE` (634,416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`). The six targets are FND-RNG-028's: the seed setter
`1000:0811`, the generator `1000:0822`, `chance_in_ten` `2834:0519`, `random_mod` `2834:061C`,
`roll_sum` `28C9:391D` and `random_between` `2D40:3A03`. A call that is not direct has three ways
to reach one: a far pointer stored or built from a segment word the loader fixes, a computed
call or jump whose target set includes it, or another segment that forms the same address.

**Words equal to each offset.** Every 16-bit word in the file equal to a target's offset, with
the word after it checked against the MZ relocations and every overlay's fixup list:

| Target | Far calls | Relocated or fixed-up pointer | Immediate next to a segment word | Other words |
|---|---|---|---|---|
| `1000:0811` | 2 | 0 | 0 | 3 |
| `1000:0822` | 29 | 0 | 0 | 1 |
| `2834:0519` | 0 | 0 | 0 | 5 |
| `2834:061C` | 0 | 0 | 0 | 8 |
| `28C9:391D` | 60 | 0 | 0 | 3 |
| `2D40:3A03` | 11 | 0 | 0 | 2 |

The far calls are FND-RNG-028's 102 far sites, the same counts per target. The 22 other words
have no relocated or fixed-up segment word after them: eleven lie in the header, which holds the
MZ relocation table, or in an overlay's fixup list; five are displacements or immediates of
unrelated `cmp`, `test` and `add` instructions; and six decode as no instruction holding the
word: two in the load image at linear `0x54A67` and `0x57CB7`, one at overlay 199 `+026C` and
three adjacent words at overlay 201 `+09C3..+09C9`. None of the 22 lies in its target's own
segment, where a near word would name the target; the four in the load image are at linear
`0x2EC7C` (for `1000:0811`), `0x4895F`, `0x54A67` and `0x57CB7` (for the two `2834` targets).

**Segment words that can form each address.** The file has 12,965 relocated or fixed-up segment
words, every one naming a descriptor's segment. For each target, every such word whose segment S
can form the target's address S * 16 + O with an offset O from 0 to 0xFFFF was taken, and the word
O searched for in the 12 bytes either side of it. No hit has a segment other than the target's own.
With the target's own segment the hits are the 102 far calls and three more pairs, whose offset
word, in each case, is that of the next far call already counted (overlay 180 `+065E` and 204
`+001E` for the seed setter, 179 `+0389` for `random_between`). The positive control for a far
pointer formed by immediates is `28C9:0CFF`, which callers of overlay 190 `+11A1` push as a
fixed-up segment and an offset (FND-EXE-587): the search finds it five times, at overlay 182
`+11C9`, `+11EA`, `+174C`, `+1ADC` and overlay 199 `+0BE9`, each a segment push followed by the
offset push.

**Computed transfers.** FND-EXE-589's walk from the established entries reaches 2,195 routines;
78 of its computed transfers are declared in `q007_dsun_apfm_census.json` with the target sets
FND-EXE-579 to FND-EXE-588 read from their writers, and FND-EXE-589 classifies the other 50 by
where they go; `newgame_declarations.json` declares those FND-EXE-592's walks meet. The 113
declarations of the two files name 181 distinct resident targets and 77 overlay targets; none of
the six is among them, and no declared site lies in segment `2834`, `28C9` or `2D40`. Of the
classified transfers that stay in the file, the five video table jumps go to the 34 relocated
rows of segment `2707`'s tables (in the segment-word search above, none forms a target's
address), `jmp [398C]` to `1000:129F`, `jmp [3916]` to the return address `1000:0260`, and
`4842:0498` to `4654:09A9`, overlay 196 `+040B` or `31E0:31A7`. The script opcode table's targets include the handler `172C:170B`, which calls
the generator directly (FND-SCRIPT-011). The transfers that leave the file go to the XMS driver,
the interrupt handlers installed before the program, and the sound driver code of `ADV`
resources, whose only calls into the file are `4602:0040`, `4842:105F` and `4842:107E`
(FND-EXE-589, FND-EXE-590, FND-EXE-591).

## Interpretation

The generator, the seed setter and the four reducers are entered only through the direct calls
FND-RNG-028 counts, and, for `random_mod` and `chance_in_ten`, the near calls inside segment
`2834`: no constant far pointer names them in any form, no reached computed transfer's target set
holds them, and no other segment's words form their addresses. So the draws and seeds that
FND-RNG-028's sites make are all the game's calls of these routines on the walk's assumptions.

## Alternatives

- A computed transfer goes to one of these routines. Ruled out for every transfer the walk
  reaches, as far as each declaration's finding holds; code the walk does not reach is not
  covered.
- A far pointer to one of them is built at run time: a segment read from memory that no
  relocation or fixup marks, segment arithmetic, or an offset computed rather than stored. Not
  searched; the walk's declarations would then have to be wrong for such a pointer to be called.
- A near pointer inside segment `1000`, `2834`, `28C9` or `2D40` names one. None of the other
  words above lies in its target's segment, and no declared computed transfer in `2834`, `28C9`
  or `2D40` exists; those in `1000` have declared targets that are not these routines.
- Each hit is a byte pattern; only the far calls are decoded as instructions (FND-RNG-028's
  limits apply).

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`, with `<dsun>` the installed
`DSUN.EXE`: `far_pointer_words.py <dsun> 1000:0811 1000:0822 2834:0519 2834:061C 28C9:391D
2D40:3A03` gives the word table, and `alias_far_words.py <dsun> 1000:0811 1000:0822 2834:0519
2834:061C 28C9:391D 2D40:3A03 28C9:0CFF`, from the commit that adds this finding, the segment-word
search with its control. The declared targets are the `targets` of every `indirectCalls` and
`indirectJumps` entry in the two JSON files. Outputs are kept as `fpw.txt` and `alias.txt` under
`GAME_DIR/analysis/rng-callers-20261020/`. Nothing was executed.
