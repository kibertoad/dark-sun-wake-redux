---
id: FND-MAGIC-009
title: Generation gives a psionicist 18 fixed powers at rank 1 and any other character the five powers of its one chosen discipline, an item whose DATA number is a power teaches it or raises its rank for any member, and no list or use of a power tests a class
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E906..0x0006EA10
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000436F6..0x00043719
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000999A9..0x00099B77
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 29C8:0045..29C8:00C3
tool: tools/research/exec-census/overlay_listing.py, resident_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1; a short read of the 35 bytes at file 0x436F6
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x6D090 + x` for overlay 184 and `0x99260 + x` for 213. Segment
`4E4F` starts at file `0x436F0` (FND-PARTY-081 gives `4E4F:00FE` at `0x437EE`). Overlay 176
`+064E` sets a power's rank to 1, `+0586` raises a nonzero rank below 30 by 1, `+0674` clears the
rank, all keeping bit 0 (FND-PARTY-104, FND-EXE-611). `direct_callers.py <dsun> 176+064E 176+0586
176+0674` finds `+064E` called from overlay 184 `+1940` and `+196E`, overlay 209 `+111B`, overlay
213 `+08BC` and the resident `29C8:00A6`; `+0586` from overlay 209 `+10FB` and overlay 213 `+0865`;
`+0674` from overlay 184 `+1913`.

**Generation, overlay 184 `+1876`** (`+1876..+1980`), takes a slot `s`. It sets a flag when any of
the three class bytes `+0x1B` to `+0x1D` of `s`'s details record is 12, Psionicist
(`+1881..+18BD`). For `k` from 0 to 2 it sets or clears bit `k` of the `PSIN` byte `4E71:0A55 + s`
from bit `0x80 >> k` of `DS:42C0` and, for a set bit, stores `k` in a local `d` that starts at 0
(`+18BF..+1908`, FND-PARTY-077). It clears the rank of every power 0 to 33 through `+0674`
(`+190A..+1922`). Then, with the flag, it calls `+064E` for each of the 20 signed bytes
`4E4F:0015..0028` that is not -1 (`+1924..+194E`); without it, for each of the 5 bytes at
`4E4F:0006 + 5 * d` that is not -1 (`+1950..+197A`). The bytes:

| Bytes | Powers |
| --- | --- |
| `4E4F:0006..000A` (`d` 0) | 0 Detonate, 1 Disintegrate, 3 Ballistic Attack, 5 Inertial Barrier, 30 Mind Blank |
| `4E4F:000B..000F` (`d` 1) | 8 Life Draining, 10 Adrenalin Control, 11 Biofeedback, 18 Lend Health, 0 Detonate |
| `4E4F:0010..0014` (`d` 2) | 23 Superior Invisibility, 22 Psychic Crush, 25 Ego Whip, 31 Psionic Blast, 30 Mind Blank |
| `4E4F:0015..0028` (psionicist) | 0, 1, 4, 5, 7, 8, 11, 13, 14, 16, 23, 24, 25, 27, 31, 30, 32, 33, 0, 0 |

No byte is -1. The psionicist list names 18 powers: Detonate, Disintegrate, Control Body, Inertial
Barrier, Energy Containment, Life Draining, Biofeedback, Cell Adjustment, Displacement, Flesh
Armor, Superior Invisibility, Tower of Iron Will, Ego Whip, Intellect Fortress, Psionic Blast,
Mind Blank, Synaptic Static and Thought Shield.

**Items, overlay 213 `+0749..+0917`.** For the item record at `DS:19C1 + 0x17 * i`, a word `+0x10`
of 0 shows `NOTHING HAPPENS`; otherwise `n` is that word less 1 (`+0749..+076D`). For an item
number from 0x7918 to 0x79DF or 0x7AA8 to 0x7B0B and `n` from 235 to 268, it calls overlay 176
`+0000` for the member in `4E71:0B44` and power `n - 235`: when the member knows it, `+0586` and
`%Fs ENHANCED TO %d` with the new rank; otherwise `+064E` and `%Fs LEARNED`; then it removes the
item through overlay 191 `+0B8C` (`+082D..+0912`). `n` of 269 or more shows `CANNOT LEARN FROM
THIS ITEM`. No class byte or class level is read in `+0749..+0917`.

**The debug grant** (`29C8:0045..29C8:00C3`): for each party slot whose state byte is 2 it sets
the `SPST` bits of spells 0 to 114, and when `27E5:00C1` of the slot and 12 is above 0 calls
`+064E` for every power 0 to 33; then it sets `DS:13F8` to 1. `DS:13F8` is set only on the debug
paths (FND-PARTY-116).

**Lists and use.** The cast list keeps a power when its rank is above 0 (overlay 211 `+1DBC`),
and `+0196`'s gate `+005B` tests the details key, `combat_mark`, effect 0x43, the points, the
in-effect bit and `g_4C4F_00B2` (FND-RNG-046, FND-PARTY-169); neither reads a class byte. The
level a use passes on is `+076C(s, 12)`, the psionicist level or 1 when there is none
(FND-PARTY-169). Ranks rise in play otherwise only through overlay 209 `+0A02` on a new greatest
Psionicist level (FND-PARTY-104).

## Interpretation

Every character can use psionic powers: the game uses the rank, not the class, to decide which.
A new psionicist knows 18 powers at rank 1, whatever disciplines its `PSIN` byte holds; it does
not get Project Force, Ballistic Attack, Animal Affinity, Absorb Disease, Adrenalin Control, Body
Weaponry, Enhanced Strength, Graft Weapon, Lend Health, Share Strength, Domination, Mass
Domination, Psychic Crush, Id Insinuation, Mental Barrier or Mind Bar, which it can learn on later
levels. A new character of any other class knows the five powers of its one discipline, the wild
talents: two lists hold a power of another discipline (Mind Blank in the psychokinesis list,
Detonate in the psychometabolism list). Its powers never rise with its levels, and it uses them
at level 1. Any member can learn a power, or raise one by a rank, from an item that holds it.

## Alternatives

- What `DS:42C0` holds when `+1876` runs for a character made outside the generation screen was
  not read; with none of its bits set, `d` is 0 and a non-psionicist gets the psychokinesis list.
- The premade characters' `PSST` resources (FND-PARTY-012) and a load restore ranks without
  `+1876`; their values were not compared with these lists.
- Overlay 209 `+111B` was not read here; FND-PARTY-104 reads the window it belongs to.
- That the items of 0x7918 to 0x79DF and 0x7AA8 to 0x7B0B are scrolls, and which of them hold
  power numbers, was not read; the code before `+0749` that reaches this path was read only for
  class tests.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python: `direct_callers.py <dsun> 176+064E
176+0586 176+0674 176+0000`; `overlay_listing.py <dsun> 184 6D090 6F3B0` (keep `+1876..+1980`),
`213 99260 99A40` and `213 99A40 99B80` (keep `+0749..+0917`); `resident_listing.py <dsun>
29C8:0040..29C8:00C8`; read the 35 signed bytes at file `0x436F6`.
