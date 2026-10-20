---
id: FND-RNG-031
title: Overlay 173's seven unread generator calls are two fight set-up words per combatant, a retried random target pick, a removed combatant's drop placement and an item-break test after every damaging armed swing
status: superseded
builds: [BLD-GOG-EN-1.1]
superseded_by: [FND-RNG-041]
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B709..0x0005B83D
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
tool: tools/research/exec-census/direct_callers.py, trampoline_target.py and overlay_listing.py at commit c40653e3; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 173's code starts at file `0x5A920`
and is `0x415C` bytes, with 45 trampolines (header segment `5671`). Decoding from the entries
`+0C65`, `+1ACD`, `+3136` and `+3A9F`, each a trampoline target, to their far returns shows
FND-RNG-028's seven unread overlay-173 sites as instructions: `+0E99` and `+0EDB` in `+0C65`,
`+1BE2` in `+1ACD`, `+31DF` and `+3208` in `+3136`, and `+3B21` and `+3B3D` in `+3A9F`. Each
draw is sign-extended, multiplied by a constant or a size and divided by 32768 as signed 32-bit
values, so a factor `m` gives 0 to `m - 1`.

### Fight set-up, `+0C65`: `+0E99` and `+0EDB`

`+0C65`, which the fight routine calls (FND-PARTY-097), takes each slot in turn and runs the
slot's trigger through `2D40:207E` under the conditions FND-PARTY-244 lists. In that same path,
after the trigger call, with `c` the slot's combatant number, it sets two words of the
combatant's four-byte entry at `4C4F:00DC + 4 * c`:

- When the byte at `4C4F:0006` is 0, 1 or 2 and bit `b` of 0x71, 0x184 or 0x608 respectively is
  set, `b` being the combatant's byte `+0x15`, it makes no draw: the word at `+0` is `f + g + 30`
  and the word at `+2` is 200 (`+0E62..+0E97`).
- Otherwise it draws at `+0E99` with factor 10, then calls the two helpers, and stores
  `r + f + g + 20` at `+0`; it then draws at `+0EDB` with factor 200 and stores that result at
  `+2` (`+0E99..+0F05`).

`f` is `27E5:02D9` of the slot and `g` overlay 197 `+254B` of the slot; their bodies were not
read. The routine then stores ten times overlay 197 `+24FB` of the slot as the combatant's
movement (FND-PARTY-146). FND-COMBAT-047's action two later lowers the word at `+0`.

### Target pick, `+1ACD`: `+1BE2`

`+1ACD` takes a slot, its combatant, an output pointer and further arguments, and returns at once
when the output word is not -1. Otherwise it builds a group mask from the combatant's byte
`+0x15` and fills a local list through `+0000` or `+0159`, according to its byte argument at
`bp+0x10`, which return a count `n`. When `n` is 0 or less it stores -1 in the output and `0xFF`
in the byte at `4C4F:0088 + combatant`. Otherwise, for attempt `k` from 0:

1. It draws at `+1BE2` with factor `n`, giving `p`, and starts with `ok` true.
2. When `3143:00A8` of the slot and 0x21 gives an entry `e` other than -1 and the word at
   `4E71:024F + 10 * e` equals `p`, `ok` becomes false.
3. When `3143:00A8` of `p` and 0x57 gives an entry other than -1 and overlay 179 `+2A46`, the
   saving throw of FND-PARTY-099, of the slot, `p`, 0x7D and the slot's level byte `+0x1E`
   returns zero in AL, `ok` becomes false.
4. With `ok` it stores the first word of local entry `p` in the output, sets the byte at
   `4C4F:0088 + combatant` to 1 and returns. Without it, when `n` is 1 or less, or `k` has
   reached `n`, it stores -1 and `0xFF` as above; otherwise it adds 1 to `k` and goes to 1.

So a call makes from 1 to `n + 1` draws at `+1BE2` when `n` is above 1, and one when `n` is 1,
each followed, in step 3, by the saving throw's own draw when that test is reached. A count of 1
still draws, though the pick is always 0. The value `p` is used both as an index into the local
list and as a slot number in steps 2 and 3. Its direct callers are `+1354`, `+1684`, `+1822` and
`+1A55`.

### Removal drop, `+3136`: `+31DF` and `+3208`

`+3136` takes a slot and does nothing for a slot below 4. Otherwise it lists the slot's held
objects through `2D40:2196` and, for each list entry whose first word is 1, keeps the greatest
byte `+0x12` of the item record at `DS:19C1 + 23 * item`. At the first such entry it draws twice
with factor 15: `+31DF` gives `ry` and `+3208` gives `rx`, and it calls the object placement
`31E0:0121` with object 2200, -1, `x - 7 + rx`, `y - 7 + ry`, 0, the byte kept so far and 7, `x`
and `y` being the words at `DS:67C5` and `DS:67C7 + 37 * slot`. The flag that marks the first
entry stays set for the rest of the call, so a call makes 0 or 2 draws. The rest of the call
moves the listed objects to the placed one through `2D40:288C`, and at the end puts the greatest
byte into byte `+5` of the eight-byte entry at the far pointer `DS:67B7` that the placed slot's
word at `DS:67BC + 37 * slot` numbers, after clearing its low three bits. Its only direct caller is
overlay 206 `+0E1E` at `+0FA4`, after the cry and before the dissolve of FND-RNG-030.

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

The seven sites serve four things in combat: two words per combatant at fight set-up, a
target pick that retries up to `n` times, the place of the object a removed non-party combatant
leaves, and a test, after every damaging swing with an item, that breaks an eligible item on one
pair of draws in about 160. No rule entry describes any of them yet.

## Alternatives

- The meaning of the words at `4C4F:00DC + 4 * c`, of the byte at `4C4F:0006`, of the group masks,
  and of the helpers `f` and `g` was not read; whether `f` or `g` draws is not known.
- In `+1ACD`, which slot list `+0000` and `+0159` build, and whether using `p` as a slot is the
  intended check, were not read. Effect codes 0x21 and 0x57 are not named here.
- Object 2200 is taken from the immediate passed to the placement; what it is and what the
  arguments 0, the greatest byte and 7 mean were not read.
- The break test's item eligibility is stated in record bytes only. `+3E6B`'s effect on the item
  was not read.
- The overlay is outside the resident harness; no emulated call or run was made.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`: resolve trampolines `5671:0020`
to `5671:00FC` with `trampoline_target.py`; `direct_callers.py <dsun> 173+0C65 173+1ACD
173+3136 173+3A9F 173+3E6B`; `overlay_listing.py <dsun> 173 5A920 5EA7C` and read the ranges
above, the jump table as four little-endian words at file `0x5E4D7`. Output is kept under
`GAME_DIR/analysis/rng-callers-20261020/`. Nothing was executed.
