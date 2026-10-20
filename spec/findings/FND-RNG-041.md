---
id: FND-RNG-041
title: Overlay 173's seven unread generator calls are two fight set-up words per combatant, a retried random pick from a distance-sorted list of slots, a removed combatant's drop placement and an item-break test after every damaging armed swing
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005A920..0x0005AC5D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B635..0x0005B83D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B8D9..0x0005B8EA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005BC46..0x0005BC77
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005BF7C..0x0005BFA7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005C11B..0x0005C145
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005C326..0x0005C378
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005C3ED..0x0005C5E1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005C6DD..0x0005C71C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005CA58..0x0005CA70
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005DA56..0x0005DC24
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005E3BF..0x0005E4D7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0005E4D7..0x0005E4DF
tool: tools/research/exec-census/direct_callers.py, trampoline_target.py and overlay_listing.py at commit 5017c4d1; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 173's code starts at file `0x5A920`
and is `0x415C` bytes, with 45 trampolines (header segment `5671`). Decoding from the entries
`+0000`, `+0159`, `+02AA`, `+0C65`, `+1ACD`, `+3136` and `+3A9F`, each a trampoline target or a
near call target of one, to their far returns shows FND-RNG-028's seven unread overlay-173 sites
as instructions: `+0E99` and `+0EDB` in `+0C65`, `+1BE2` in `+1ACD`, `+31DF` and `+3208` in
`+3136`, and `+3B21` and `+3B3D` in `+3A9F`. Each draw is sign-extended, multiplied by a constant
or a size and divided by 32768 as signed 32-bit values, so a factor `m` gives 0 to `m - 1`. A
slot's cell is its words at `DS:67C5` and `DS:67C7 + 37 * slot` shifted right by 4, its
combatant the word at `4F49:0C34 + 3 * slot`, and `b` a combatant's byte `+0x15`.

### Fight set-up, `+0C65`: `+0E99` and `+0EDB`

`+0C65`, which the fight routine calls (FND-PARTY-097), takes each slot in turn and runs the
slot's trigger through `2D40:207E` under the conditions FND-PARTY-244 lists. In that same path,
after the trigger call, with `c` the slot's combatant, it sets two words of the combatant's
four-byte entry at `4C4F:00DC + 4 * c`:

- When the byte at `4C4F:0006` is 0, 1 or 2 and bit `b` of 0x71, 0x184 or 0x608 respectively is
  set, it makes no draw: the word at `+0` is `f + g + 30` and the word at `+2` is 200
  (`+0DF0..+0E97`).
- Otherwise it draws at `+0E99` with factor 10, then calls the two helpers, and stores
  `r + f + g + 20` at `+0`; it then draws at `+0EDB` with factor 200 and stores that result at
  `+2` (`+0E99..+0F05`).

`f` is `27E5:02D9` of the slot and `g` overlay 197 `+254B` of the slot. The routine then stores
ten times overlay 197 `+24FB` of the slot as the combatant's movement (FND-PARTY-146). A slot
that fails one of FND-PARTY-244's conditions gets -1 in the word at `+0` and keeps its word at
`+2` (`+0FB9..+0FCA`). FND-COMBAT-047's action two later lowers the word at `+0`.

### Slot lists, `+0000`, `+0159` and `+02AA`

`+0000` takes a cell `x, y`, a mask, a limit, a list and a byte flag. For each slot `s` from 0 to
47 it adds an entry when all of these hold: the slot's kind byte at `4F49:0C33 + 3 * s` is 2; bit
`b` of `s`'s combatant is set in the mask; that combatant's byte `+0x14` is 2 or less; `s`'s
cell lies within the words at `DS:14D7`, `DS:14DB` (column) and `DS:14D9`, `DS:14DD` (row), each
divided by 16 as a signed value, both ends included, compared without sign; the distance
`1B0B:000C` gives for `x, y` and the cell is not above the limit (signed); and, when the flag is
not 0, `1B0B:00A4` of `x, y, 0` and the cell and 0 returns 0 (`+0008..+0149`). It returns the
number of entries added. `+0159` does the same around the cell of a slot argument `t`, with the
distance from `2D40:35EF` of `t` and `s`, and always makes the `1B0B:00A4` test (`+0161..+029A`).
Neither leaves out `t` or any slot because it is the centre.

`+02AA` takes the count `n` so far, the list, the slot and its distance `d`. It finds by a binary
search over the `n` four-byte entries, sorted by their word `+0`, the first entry whose word `+0`
is `d` or more, moves that entry and those after it up four bytes through `289B:01DC`, and writes
`d` in word `+0` and the slot in word `+2` of the entry at that place (`+02B2..+0339`). The list
is so kept in order of rising distance, a new slot going before slots of the same distance.

### Target pick, `+1ACD`: `+1BE2`

`+1ACD` takes a slot `a`, its combatant, an output word pointer, a word it does not read, a
limit `L` and a byte flag, and returns at once when the output word is not -1. Otherwise it
builds a mask from `b` of the combatant by OR-ing 0x71 when bit `b` of 0xF80 is set, 0x184 when
bit `b` of 0xC58 is, 0x608 when bit `b` of 0x964 is, 0xF80 when `b` is 0 and 0x800 when `b` is
not 11 (`+1AEC..+1B8C`). `+0C65` builds the same mask for its test of slots 5 and up
(`+0D15..+0DC0`). With the flag 0 it fills a local list through `+0000` with `a`'s cell, the
mask, `L`, the list and 0; otherwise through `+0159` with `a`, the mask, `L` and the list. These
return a count `n`. When `n` is 0 or less it stores -1 in the output and `0xFF` in the byte at
`4C4F:0088 + combatant`. Otherwise, for attempt `k` from 0:

1. It draws at `+1BE2` with factor `n`, giving `p`, and starts with `ok` true.
2. When `3143:00A8` of `a` and 0x21 gives an entry `e` other than -1 and the word at
   `4E71:024F + 10 * e` equals `p`, `ok` becomes false.
3. When `3143:00A8` of `p` and 0x57 gives an entry other than -1 and overlay 179 `+2A46`, the
   saving throw of FND-PARTY-099, of `a`, `p`, 0x7D and byte `+0x1E` of `a`'s 66-byte record at
   `DS:19C5` returns zero in AL, `ok` becomes false.
4. With `ok` it stores word `+2` of local entry `p`, the slot that entry holds, in the output,
   sets the byte at `4C4F:0088 + combatant` to 1 and returns (`+1C89..+1CA7`). Without it, when
   `n` is 1 or less, or `k` has reached `n`, it stores -1 and `0xFF` as above; otherwise it adds 1
   to `k` and goes to 1.

So a call makes from 1 to `n + 1` draws at `+1BE2` when `n` is above 1, and one when `n` is 1,
each followed, in step 3, by the saving throw's own draw when that test is reached. A count of 1
still draws, though the pick is always 0. The value `p` is the place in the list, the `p`-th
nearest slot from 0, and steps 2 and 3 use that same number as a slot. Its direct callers are
`+1354` (flag 0, limit 24), `+1684` (flag 1, limit a local word), `+1822` (flag 1, limit 1) and
`+1A55` (flag 1, limit byte `+6` of an item type's 15-byte record at `DS:19CD`).

### Removal drop, `+3136`: `+31DF` and `+3208`

`+3136` takes a slot and does nothing for a slot below 4. Otherwise it lists the slot's held
objects through `2D40:2196`, whose ten-byte entries have the kind in word `+2` and the item in
word `+4`, and goes through the entries whose kind is 1, keeping the greatest byte `+0x12` of the
item record at `DS:19C1 + 23 * item`. At the first such entry it draws twice with factor 15:
`+31DF` gives `ry` and `+3208` gives `rx`, and it calls the object placement `31E0:0121` with
object 2200, -1, `x - 7 + rx`, `y - 7 + ry`, 0, the byte kept so far and 7, `x` and `y` being the
words at `DS:67C5` and `DS:67C7 + 37 * slot`. The flag that marks the first entry stays set for
the rest of the call, so a call makes 0 or 2 draws.

When the placement returns -1 the call returns. When the placed slot's kind byte is 0, it calls
`31E0:05BB` with the placed slot's word at `DS:67BC + 37 * slot`, 1, 0 and 0 and returns
(`+323F..+3277`). Otherwise it moves the entry to the placed slot through `2D40:288C`, returns
when that gives 0, and otherwise lists the slot's held objects again and starts over from the
first entry (`+327A..+32C7`). When a pass finds no entry of kind 1 and an object was placed, it
clears the low three bits of byte `+5` of the eight-byte entry at the far pointer `DS:67B7` that
the placed slot's word at `DS:67BC + 37 * slot` numbers and sets the greatest byte into them
(`+32CA..+32FC`). Its only direct caller is overlay 206 `+0E1E` at `+0FA4`.

### Item break, `+3A9F`: `+3B21` and `+3B3D`

`+3A9F` takes an item number and the item's type. It sets `e` to 1 when bit 0x80 of byte `+4` of
the type's 15-byte record at `DS:19CD` is clear, byte `+0x16` and word `+0x10` of the item's
record at `DS:19C1` are 0, and the low four bits of that byte `+4` are 0 to 3 (a four-word jump
table at `+3BB7..+3BBF` sends all four to the same store); otherwise `e` is 0. It draws at
`+3B21` with factor 8 and returns when the result is at least `e`. Otherwise it draws at `+3B3D`
with factor 20 and returns when that result is at least `e`. Otherwise it formats
`%s is broken !` with the item's name, calls `+3E6B` with the item and shows the text through
overlay 172 `+05DB`.

So every call draws once; an item with `e` 0 never breaks; one with `e` 1 draws a second time on
4096 of the 32768 values of the first draw and breaks on 1639 of the 32768 values of the second.
The attack routine `+1CC1` calls it at `+214A`, its only direct caller, with the number and type
of the item in the attacker's entry `bp-0x18`, when that entry is 0 or more and the swing
`+2560` (FND-PARTY-091) returned a value above 0 (`+2138..+2150`).

## Interpretation

The seven sites serve four things in combat: two words per combatant at fight set-up, a pick
that retries up to `n` times among the slots of the groups a combatant's mask names within a
limit of it, the place of the object a removed non-party combatant leaves, and a test, after
every damaging swing with an item, that breaks an eligible item on one pair of draws in about
160. Using `p`, a place in the list, as a slot in steps 2 and 3 tests a slot that is in general
not the one picked.

## Alternatives

- The meaning of the words at `4C4F:00DC + 4 * c`, of the byte at `4C4F:0006`, of the group masks,
  and of the helpers `f` and `g` is not read here; FND-RNG-042 reads `f`, `g`, the byte's writers
  and the routine that reads the words.
- What `1B0B:00A4`, `2D40:35EF` and `3143:00A8` compute, and what the four callers of `+1ACD` use
  the pick for, were not read. Steps 2 and 3 may be meant to test the picked slot; the code tests
  the slot numbered `p`.
- Object 2200 is taken from the immediate passed to the placement; what it is, what the arguments
  0, the greatest byte and 7 mean, and what `2D40:288C` and `31E0:05BB` do, were not read.
- The break test's item eligibility is stated in record bytes only. `+3E6B`'s effect on the item
  was not read.
- This finding replaces FND-RNG-031, which wrote the target pick as storing the first word of
  local entry `p`. The store at `+1C89..+1C9A` reads word `+2` of the four-byte entry, which
  `+02AA` fills with the slot after the distance in word `+0`. FND-RNG-031 also wrote the removal
  drop as testing the first word of each held-object entry; the test reads word `+2`, the kind.
  This finding adds the readings of `+0000`, `+0159` and `+02AA`, the store of -1 for a slot that
  fails the set-up conditions, the mask `+1ACD` builds, its callers' flags and limits, and the
  removal drop's returns after a failed placement, a placed slot of kind 0 and a failed move.
  Everything else FND-RNG-031 records is unchanged here.
- The overlay is outside the resident harness; no emulated call or run was made.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`: resolve trampolines `5671:0020`
to `5671:00FC` with `trampoline_target.py`; `direct_callers.py <dsun> 173+0C65 173+1ACD
173+3136 173+3A9F 173+3E6B`; `overlay_listing.py <dsun> 173 5A920 5EA7C` and read the ranges
above, the jump table as four little-endian words at file `0x5E4D7`. Output is kept under
`GAME_DIR/analysis/rng-callers-20261020/`. Nothing was executed.
