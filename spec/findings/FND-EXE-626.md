---
id: FND-EXE-626
title: The 17 routines of overlay 204 that no entry placed are script requests and their per-slot callbacks that set door flags for a group, adjust a thief skill by worn items and effect flags, test what a slot holds, do nothing, set or count combatant byte 0x15, reset a combatant's fight words, brighten the palette by channel, back up and restore the script locals, test or remove effects, play a speech sample and test a slot record
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C56C..0x0008C5CB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C802..0x0008C97D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C982..0x0008CA5C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008CA5E..0x0008CA7A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D63C..0x0008D646
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D781..0x0008D828
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D93A..0x0008DAEE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008DD05..0x0008DDF3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008DE45..0x0008DF08
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008E0A0..0x0008E0E7
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 17 rows of overlay 204 (header segment `5787`, 40
trampolines, code from file offset `0x8BDC0`, 10,400 bytes) that no spec location covered, 1,837
bytes in all. Each row below is one row of the inventory; "offset" is the overlay code offset,
the file offset is `0x8BDC0` plus it, and `DS` is `57E0`. Every routine has a trampoline in
`5787`. Callers are the hits of `direct_callers.py`, checked against the instructions around
them, named by the row that holds them. "Request `n`" is entry `n - 1` of the 53-word table at
`+06C7` through which `+0169`, the handler of script opcode `0x22`, dispatches with its second
argument in `SI` and its third and fourth as two dwords (FND-PARTY-156, FND-SCRIPT-026). Requests
18, 22, 24 and 40 call `+1A68` with `SI`, the state 2, a trampoline of overlay 204 and the two
dwords, and request 53 with `SI`, -1, `5778:0048` and the dwords; `+1A68` calls the far routine
with each slot the target `SI` names and the two dwords, and counts the nonzero byte results
(FND-PARTY-156). A slot's state byte is at `4F49:0C33 + 3 * slot` and its record number at
`4F49:0C34 + 3 * slot`, which selects the 49-byte combatant record at the far pointer `DS:19C9`.
"The block" is the 16-byte flag block overlay 193 `+1BA3` fills for a slot (FND-PARTY-112). The
thief skill routine `+0887` takes a slot, a skill number `k` and a near pointer to a modifier
dword (FND-PARTY-169).

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+07AC` | `0x8C56C..0x8C5CB` | `5787:00C5` | Takes a target `t` and a byte `b`. For `t` from 1 to 9998 calls overlay 182 `+0DE6` (FND-PARTY-250) with `t`, `b` and 1; for `t` of 0 or below calls it for each slot `2D40:0D94` returns for `t` until 9999 while the index it keeps is below `0x140`; for `t` of 9999 or more does nothing. Called from `+02C6`, request 2. |
| `+0A42` | `0x8C802..0x8C81B` | `5787:00D4` | Takes a slot and returns `+0BEF` of it with the list `54A8:0000` (the word 6, then 13) and no second list. Called from `+08DA` in `+0887`, which returns 0 for skill 5 when this is nonzero. |
| `+0A5B` | `0x8C81B..0x8C834` | `5787:00D9` | Takes a slot and returns `+0BEF` of it with the list `54A9:0000` (6, 1, 10, 3, then 13) and no second list. Called from `+09D8` in `+0887`, which then subtracts the byte at `549B:00B8 + k` from the skill. |
| `+0A74` | `0x8C834..0x8C97D`, `0x8C982..0x8C9AF` | `5787:00DE` | Takes a slot, a far pointer to its block, `k` and a far pointer to the modifier dword. Adds -1000 to the modifier and returns when block byte 0 has bit `0x80`, byte 2 bit `0x20`, byte 4 bit 8 or byte 8 bit `0x20` set, or when byte `0x0B` has bit 8 and `k` is 0, byte 1 bit 4 and `k` is not 5, byte 5 bit `0x40` and `k` is 4, byte 4 bit `0x40` and `k` is 0 or 4, byte `0x0D` bit 4 and `k` is 0, 1 or 6, or byte 4 bit `0x20` and `k` is 7. Otherwise, when byte 4 has bit 1, with `s` overlay 193 `+1A2D` of the slot and `0x20` (FND-EXE-606), it sets the modifier to `modifier * 100 / (10 * s + 100)` for `k` 4 and to `(10 * s + 100) * modifier / 100` for `k` 6. It then stores `0x7D5` at `54A9:000A` and 6 at `54A9:000E` and calls `+0BEF` with the slot and those two lists; a nonzero result adds 1000 for `k` 3 and -1000 for `k` 5 and returns. Last it adds 1000 when byte 3 has bit 4 and `k` is 2, or byte 6 bit `0x20` and `k` is 4. Called from `+09FF` at the end of `+0887`. The two `jmp` at `+0BBD` follow a `jmp` and nothing reaches them, so the row leaves their five bytes out. |
| `+0BEF` | `0x8C9AF..0x8CA5C`, `0x8CA5E..0x8CA7A` | `5787:002A` | Takes a slot, a far list `A` of words and a far list `B` of words. Fills a stack item list through `2D40:2196` (FND-PARTY-218) and, for each entry of kind 1, compares byte `+0x13` of its 23-byte record at the far pointer `DS:19C1`, the position byte of FND-PARTY-217, with the words of `A` in order while they are below 13. On a match it returns 1 when `B` is null or when the record's word `+0` equals one of the positive words at the start of `B`. Returns 0 otherwise. Called from `+0A53` (in `+0A42`), `+0A6C` (in `+0A5B`) and `+0BA2` (in `+0A74`). The `jmp` at `+0C9C` follows a `jmp` and nothing reaches it, so the row leaves its two bytes out. |
| `+187C` | `0x8D63C..0x8D641` | `5787:0048` | Returns at once. Called from `+03D4`, request 14. |
| `+1881` | `0x8D641..0x8D646` | `5787:004D` | Returns at once. Called by one far call, at `28C9:0FDB`, the F9 case of the key routine `28C9:0CFF` (FND-AI-004). |
| `+19C1` | `0x8D781..0x8D7D5` | `5787:00AC` | Takes a slot and a dword whose low byte is `b`. For a slot above 4 whose block byte 2 has bit `0x10` clear, stores `b` in byte `+0x15` of its combatant record. Returns 0. No direct call was found; request 18 passes its trampoline to `+1A68`. |
| `+1A15` | `0x8D7D5..0x8D828` | `5787:0057` | Takes a mask `m` and counts the slots 5 to `0x13F` in state 2 whose combatant byte `+0x15` has a bit in common with `m`, or every such slot when `m` is 0. Returns the count. Called from `+045F`, request 20, with the low word of the first dword. |
| `+1B7A` | `0x8D93A..0x8D95F` | `5787:00B6` | Takes a slot. When the fight word `4C10:0019` (FND-COMBAT-023) is not 0, calls overlay 173 `+306B` with the slot and the mask 0, which resets the slot's words at `4C4F:00DC` and `4C4F:01D8` (FND-COMBAT-047), and returns 1; otherwise returns 0. No direct call was found; request 22 passes its trampoline to `+1A68`. |
| `+1B9F` | `0x8D95F..0x8DAEE` | `5787:0075` | Takes dwords `a` and `b`. With `n = (b / 10) * 640 / 100`, set to 64 when it is 57, and the step `s = b % 10`: when `n` is 0 it calls `+1D2E` with `s` and returns. Otherwise the decimal digits of `a` (hundreds, tens, units) choose red, green and blue. For `i` from 0 while below `n`, by steps of `s`, it waits `90 - 10 * s` through `1000:12FA`, then for each DAC index 0 to 255 reads the entry through `1BF3:2AA7`, adds `s` to each chosen channel that stays below 64 and writes the entry back through `1BF3:2A8C` when one changed (FND-IMAGE-012). With `s` 0 and `n` above 0 the loop does not end. Called from `+04EA`, request 26. |
| `+1F45` | `0x8DD05..0x8DD77` | `5787:007A` | Copies the four local variable blocks at the far pointers `4C13:033B`, `0337`, `0333` and `032F` (9, `0x40`, `0x28` and `0x1A4` bytes) to `4E71:0244`, `0204`, `01DC` and `0038` through `1000:3F5A` (FND-SCRIPT-026). Called from `+0521`, request 29. |
| `+1FB7` | `0x8DD77..0x8DDE9` | `5787:007F` | Copies the same four ranges back from `4E71` to the blocks (FND-SCRIPT-026). Called from `+0529`, request 30. |
| `+2029` | `0x8DDE9..0x8DDF3` | `5787:009D` | Takes a slot and a dword code. Returns 1 when the code is below `0x71` and `3143:00A8` of the slot and the code's low byte does not return `0xFFFF`, and 0 otherwise. The row covers only `+2029..+2033`, the prologue and the code test; the rest of the routine, `+2033..+2057`, is in no inventory row. No direct call was found; request 24 passes its trampoline to `+1A68`. |
| `+2085` | `0x8DE45..0x8DF08` | `5787:00A7` | Takes a slot and a dword code. For a code below `0x71` calls overlay 193 `+194F` with the slot and the code's low byte, which removes the slot's effect rows of that code (FND-COMBAT-032), and returns 1. For any other code calls it with the slot and each of 1, `0x13`, `0x16`, `0x17`, `0x3B`, `0x3D`, `0x46`, `0x47`, `0x5B`, `0x41`, `0x42`, `0x44`, `0x43` and `0x45`, and returns 0. No direct call was found; request 40 passes its trampoline to `+1A68`. |
| `+22E0` | `0x8E0A0..0x8E0BA` | `5787:0089` | Takes a dword and, when `DS:13F7` is set, calls the speech routine `2C5F:0AFA` (FND-SOUND-008) with its low byte. Called from `+04DA`, request 44. |
| `+22FA` | `0x8E0BA..0x8E0E7` | `5787:008E` | Takes a slot and returns 1 when the word `+0x1B` of its 37-byte record at `DS:67BB + 37 * slot` is not -1 and bit 0 of the record's byte `+0` is set, and 0 otherwise. The slot is not bounded. Called from `+04B9`, request 45, with `SI` sign-extended. |

## Interpretation

Most of these are script requests of opcode `0x22`: request 2 sets a door flag on one object or a
group through overlay 182 `+0DE6`, request 20 counts combatants by the side bits of byte `+0x15`,
request 26 brightens chosen colour channels of the whole palette in steps, requests 29 and 30
save and restore the script's local variables around a call, request 44 plays a speech sample and
request 45 tests a slot's record. Requests 18, 22, 24 and 40 run a callback over a target's slots:
18 sets byte `+0x15`, which FND-PARTY-244 takes to name a side, for slots that are not party members
and lack a block flag; 22 resets a combatant's fight words during a fight; 24 tests for an effect
code; 40 removes one effect code, or a fixed set of fourteen. Requests 14 and the F9 key reach empty
routines. `+0A42`, `+0A5B`, `+0A74` and `+0BEF` refine the thief skill of `+0887`: items whose
position byte is 6, or one of 6, 1, 10 and 3, fail skill 5 or lower a skill by a table byte, and the block's
flags make a skill fail with -1000 or pass with +1000 or scale it by the caster level of effect
code `0x20`. In the call at `+0BA2` the first list starts with `0x7D5`, which is not below 13, so
`+0BEF` never compares a position and returns 0, and the +1000 for skill 3 and -1000 for skill 5
never apply. The names stay `fn_204_<offset>` until a reading shows what the fields and tables
are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list; `+19C1`, `+1B7A`, `+2029` and `+2085` have no direct call and
are reached through the trampolines the requests pass to `+1A68`, and may also be reached by
other computed far calls. `2D40:0D94`, `2D40:2196`, `3143:00A8`, `1000:3F5A`, `1000:12FA` and
overlay 173 `+306B` past FND-COMBAT-047 are not read here; the reading of them as a group walk, an
item list, an effect lookup that returns `0xFFFF` when the slot has no row of the code, a block
copy and a wait rests on how their arguments and results are used. The lists of `+0A74`'s call
may have been meant as a position list `6` and an item list `0x7D5`, with the two pointers
swapped; the code as shipped tests nothing. The rest of `+2029` is described from the listing
although no inventory row holds it.

## How to reproduce

Run `overlay_listing.py` for overlay 204 against the installed `DSUN.EXE` over each file range in
the table and `direct_callers.py` for each offset (`204+<offset>`), read the table at `+06C7` and
the cases it names, read the words at `54A8:0000` (file `0x49C80`), and compare the ranges with the
rows of `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`.
