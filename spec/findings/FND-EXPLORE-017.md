---
id: FND-EXPLORE-017
title: 28C9:305F, the direction key's blocked step in a fight, checks the walker's effect and action bits, marks the target hostile to the walker's side, runs overlay 173's attack routine, and runs the fight check of overlay 173 +059A, which can end the fight and reset the pointer to Walk
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:305F..28C9:31D2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000851C6..0x0008527B
tool: Capstone 5.0.9 over resident and overlay code; tools/research/exec-census/resident_listing.py, overlay_listing.py and trampoline_target.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Addresses are static, load image at segment 1000, `DS` = `57E0`. The combat word is `4C10:0019`.
Overlay 197's `+1DC6` is at file `0x851C6`.

**`28C9:305F(c, t, n)`** returns a word, 0 or 1. FND-EXPLORE-004 reads the direction key's call
with the walker `c`, the object `t` whose area holds the blocked cell, and `n` = 1.

1. When the combat word is not 0, it calls overlay 197 `+1DC6(c)` (trampoline `575A:002F`). It then
   returns 0 when `n` is above 1 (signed) and bit `0x20` of word `c` of `4C4F:01D8` is clear, and
   returns 0 when bit 2 of that word is clear. With the combat word at 0 it skips all of step 1.
2. It calls `28C9:2E0E(t, k)`, with `k` byte `+0x15` of the 49-byte record at `DS:19C9` that word
   `4F49:0C34 + 3 × c` names.
3. It calls `2D40:2196(c, buffer)` with an 800-byte stack buffer, then overlay 173 `+1CC1(c, t,
   buffer, n)` (trampoline `5671:0057`), and sets its result to 1.
4. It returns 1 when overlay 173 `+2EC7(c)` (trampoline `5671:0089`) returns 0.
5. Otherwise it calls overlay 173 `+059A(f, &c)` (trampoline `5671:0039`), `f` 0 when the combat
   word is 0 and 3 when it is not, and stores the result in the combat word. When that is not 0 it
   returns 1.
6. When it is 0, the result becomes 0. When `DS:0DA5` is not -1: when byte `+0x14` of record
   `DS:0DA5` of `DS:19C9` is 1 it stores `DS:0DA5` in `DS:426D` and calls `2D40:3D4C(4C13:0369,
   DS:0DA5)`, and otherwise stores `4C13:0369` in `DS:426D`; it calls `56BD:0025(1)` when the byte
   `DS:425E` is 0, and stores -1 in `DS:0DA5`. Then, whether or not `DS:0DA5` was -1, it calls
   `5699:0043` and `2C5F:097E`, stores 1 in `DS:1440` and calls `2C5F:0C8D(1)`, and returns 0.

**Overlay 197 `+1DC6(s)`** does nothing unless the type byte `4F49:0C33 + 3 × s` is 2. With `m`
the word after that byte, it has overlay 193 `+1BA3` fill a 16-byte flag block for `s`
(trampoline `573B:0025`, FND-EXE-619). With bit 5 of the block's byte 8 set, or with bit 3 of byte
11 or bit 2 of byte 6 set and overlay 195 `+0000` (trampoline `574E:0020`) returning an odd byte,
it calls overlay 173 `+306B(s, 0)` and returns. Otherwise it clears bits from word `m` of
`4C4F:01D8`: `0x22` when bit 1 of byte 8 or bit 3 of byte 4 is set; `0x04` when bit 4 of byte 8,
bit 3 of byte 4 or bit 7 of byte 0 is set; `0x01` when bit 2 of byte 8 is set (its test of bit 5 cannot pass, since that bit took the early
return).

## Interpretation

In a fight, a direction key toward another object's area is an attack: the walker needs bit 2 of
its action word, which overlay 197 `+1DC6` clears for some effects, and the target is turned
hostile to the walker's side (FND-PARTY-095) before overlay 173 `+1CC1` runs the attack
(FND-PARTY-199). Outside a fight the same call starts one through overlay 173 `+059A` with 0
(FND-RNG-074). When that call returns 0, the fight is over or did not start, and the routine
gives the turn back to `DS:0DA5` or `4C13:0369` and resets the pointer to Walk, as `28C9:1EDB`
does (FND-RNG-073, FND-RNG-074). The direction key ignores the result (FND-EXPLORE-004).

## Alternatives

Step 1 indexes `4C4F:01D8` by the slot, while overlay 197 `+1DC6` and overlay 173 `+2EC7`
(FND-EXE-600) index it by the record index `m`; the two agree only when the slot equals its
record index, which was not checked for party slots. What the bits of `4C4F:01D8` and the flag
block stand for, and what overlay 173 `+1CC1`, `+059A` and `+306B`, `2D40:3D4C`, `5699:0043`,
`2C5F:097E` and `56BD:0025` do in full, were not read here; FND-EXE-600, FND-EXE-624, FND-EXE-605,
FND-COMBAT-047 and FND-RNG-074 describe parts of them.

## How to reproduce

`resident_listing.py <dsun> 28C9:305F..28C9:31D2`; `overlay_listing.py <dsun> 197 851C6 8527B`;
`trampoline_target.py <dsun>` with `575A:002F`, `5671:0039`, `5671:0057` and `5671:0089`.
