---
id: FND-PARTY-165
title: Overlay 176 +076C, which counts a human's held-back former class, gives the psionicist level for psionic points and power use, the thief level for the thief skills, the best cleric, druid and preserver levels when the party rests, and a cleric code 3 level that ends a hit
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00061BE2..0x00061C82
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000618E6..0x00061905
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000619A9..0x000619D2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005F559..0x0005F5CA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00091E92..0x00091EAD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000923D5..0x000923F0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C647..0x0008C795
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008CAE8..0x0008CD80
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00084666..0x0008467F
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/direct_callers.py, overlay_listing.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Overlay 176 `+076C` (trampoline `5691:0061`) takes a party slot and a class code and
returns the level of the first class position holding the code, 1 for code 12 when none does and
0 otherwise, with no test of the origin byte (FND-PARTY-162). Class codes are as
`character_class` gives: 1 to 4 cleric, 5 to 8 druid, 11 preserver, 12 psionicist, 17 thief.
`direct_callers.py <dsun> 176+076C` finds, besides overlay 197 `+0206` (FND-PARTY-163,
FND-PARTY-164), these calls:

1. **Overlay 176 `+06A2`** takes a slot `s`. It calls `27E5:04A0` with `s` and 3, 2 and 4, and
   keeps the bytes as `a3`, `a2` and `a4` (`+06AC..+06D3`). It calls `+076C` with `s` and 12,
   multiplies the result by `a4 - 5`, and adds `2 * a4`, `a3 - 15` when `a3` is above 15,
   `a2 - 15` when `a2` is above 15, and -10. It stores the sum in the word at `+2` of `s`'s
   combatant record (`+06D6..+073B`). `direct_callers.py <dsun> 176+06A2` finds its trampoline
   `5691:0052` and no call of it.
2. **Overlay 176 `+03AB` and `+0479`** call `+076C` with code 12 and pass the result, with a
   number above 0xEB, to overlay 193 `+0000` (`+03A6..+03C2`, `+0474..+048F`).
3. **Overlay 174 `+0793`** follows a test of overlay 176 `+0742` for a number `p` against the word
   at `+2` of a combatant record (`+0759..+0773`). It calls `+076C` with code 12 and passes the
   result, with `p + 0xEB`, to overlay 193 `+2290`, then subtracts `+0742`'s result for `p` from
   that word (`+0790..+07C6`).
4. **Overlay 208 `+0611` and `+0B54`**: for a number from 0 to 0xEA, each calls overlay 177
   `+07AF` with the slot and the number; for one from 0xEB to 0x10C, `+076C` with the slot and
   code 12 (`+05EC..+0619`, `+0B2F..+0B5C`). The result goes on as the routine's level.
5. **Overlay 204 `+0887`** takes a slot and a number `k`. It keeps the result of `27E5:04A0`
   with the slot and 1, calls `+076C` with code 17, and when the result is not above 0 returns 0
   for some `k` (`+08AD..+0903`). Otherwise it adds a byte from the table at `549B:k`, one from
   the same table at `8 * origin + k` and four times the `+076C` result, then adds or subtracts
   5 or 3 for each step of the kept value against the table's bytes at `+0xA0`, `+0xA8` and
   `+0xB0` (`+0906..+09D3`).
6. **Overlay 204 `+0D28..+0F4A`** runs after the string `THE PARTY RESTS` is shown
   (`+0CF8`). For each party slot it takes the greatest result of `+076C` over codes 1 to 4,
   over codes 5 to 8, and for code 11, and keeps the greatest of each over the slots with the
   slot that gave it (`bp-6`, `bp-8`, `bp-0xA`). After the loop, a preserver level of 10 or more
   calls `2D40:06E1` with its slot, and a cleric level of 5 or more calls `+12B4` with 0x94, its
   slot and the level (`+0F72..+0F94`).
7. **Overlay 197 `+1266..+127C`**, in the routine `+111A` that returns 1 to end a hit
   (FND-PARTY-112): when its argument `m` has bit 2 set, it calls `+076C` with code 3 and returns
   1 when the result is 5 or more.

## Interpretation

The word at `+2` of a combatant record that `+06A2` sets and overlay 174 spends is the psionic
points, and `a2` to `a4` read as constitution, intelligence and wisdom, the scores at `+0x1B` to
`+0x1D` of the combatant record when `27E5:04A0` reads the byte at `+0x19` plus its argument. Numbers from 0xEB are psionic powers, so their level is the psionicist
level from `+076C`, while a spell's level goes through overlay 177 `+07AF`, which holds a former
class back (FND-PARTY-162). Overlay 204 `+0887` reads as a thief skill: a base, an origin
adjustment and 4 per thief level. When the party rests, its best cleric, druid and preserver act
by level.

In each of these, a dual-classed human whose former class is the one asked for counts with that
class's level even while the other routines hold it back. A former psionicist keeps its points
and power level, a former thief its skills, a former cleric, druid or preserver acts at rest, and
a former cleric of code 3 at level 5 or more is spared by hits whose `m` has bit 2 set.

## Alternatives

- What `27E5:04A0` returns for 2, 3 and 4 was not read; reading them as the scores rests on the
  formula's shape.
- What overlay 193 `+0000` and `+2290`, `2D40:06E1`, overlay 204 `+12B4` and the routines the two
  overlay 208 sites lie in do with the level was not read, nor what bit 2 of `m` selects.
- Whether a human can use a power or a thief skill while the class is held back depends on the
  menus and lists that offer them, which were not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: in `tools/research/exec-census/`, run `direct_callers.py <dsun> 176+076C 176+06A2` and
`overlay_listing.py <dsun>` with 176 `61B60 61CAC`, 176 `618C0 61910`, 176 `61990 619E0`, 174
`5F530 5F5D0`, 208 `91E40 91EE0`, 208 `92390 92420`, 204 `8C600 8C7D0`, 204 `8CAA0 8CD60` and 197
`84290 84700`.
