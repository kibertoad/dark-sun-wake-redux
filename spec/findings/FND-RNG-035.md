---
id: FND-RNG-035
title: The roll_sum calls of overlays 191, 193, 198, 204 and 211 are three d20 item tests, the DATA dice formula, a one-in-four spell-use test, a d20 saving throw and a d20 turning test
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007CEC0..0x0007D0F5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00080278..0x000803B6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008772D..0x00087831
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00087831..0x00087859
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008DF0E..0x0008DFC4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00097C50..0x00097D1F
tool: tools/research/exec-census/overlay_listing.py, direct_callers.py and trampoline_target.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`. The code of overlays 191, 193, 198, 204 and 211
starts at file `0x7C1F0`, `0x7E2F0`, `0x87310`, `0x8BDC0` and `0x95E10`. Decoding each routine
below from its entry, a trampoline target, to its far return shows seven of FND-RNG-028's
`roll_sum` sites as instructions: overlay 191 `+0D2A`, `+0DE8` and `+0EC6`, overlay 193 `+20B1`,
overlay 198 `+0451`, overlay 204 `+21B7` and overlay 211 `+1EDF`. `roll_sum` is `28C9:391D` with
the count as its first argument and the sides as its second (RULE-RNG-001). `R` is a combatant
record (`DS:19C9`, 49 bytes), `D` the details record its word `+4` numbers (`DS:19C5`, 66 bytes),
and an item record is the 23-byte record at `DS:19C1`. The held-object lists come from
`2D40:2196`, in ten-byte entries whose word `+2` is 1 for the entries these routines take and whose
word `+4` is the item.

### Overlay 191: three item tests

The direct-call query finds the callers in overlay 179 `+1373`'s effect switch (FND-RNG-034):
`+0CD0` from `+271B` (`DATA` 103) and `+2735` (`DATA` 284), `+0E3A` from `+2728` (`DATA` 103,
when `+0CD0` returns 0) and `+2742` (`DATA` 275 and 283), and `+0D60` from `+270E` (`DATA` 235).
Each takes a slot.

- `+0CD0` has overlay 173 `+2E1B` pick an entry of the slot's list; with none it returns 0. With
  `b` the signed byte `+0x16` of the entry's item record, it rolls 1 and 20 at `+0D2A`; when the
  total is below `8 - b` it shows the text at `DS:205C`, removes the entry through `2D40:2452`
  and returns 1, and otherwise returns 0 (`+0CD0..+0D60`).
- `+0D60` takes the first entry whose item's byte `+0x12` is the lowest below 6; with none it
  returns 1. It rolls 1 and 20 at `+0DE8`; when the total is below `15 - b`, `b` that item's
  signed byte `+0x16`, it calls overlay 173 `+2B88` with the list, the item, the entry and 1. It
  returns 1 either way (`+0D60..+0E3A`).
- `+0E3A` goes through the entries in order. For one whose item's signed byte `+0x13` is 0, 6
  or 9, with `w` the item's word `+0x10`: when `w` is 0 it makes no roll; otherwise it rolls 1
  and 20 at `+0EC6` and goes on to the next entry when the total is at least `10 - w`. Not going
  on, it shows the text at `DS:2070`, removes the entry and returns 1. It returns 0 after the
  last entry (`+0E3A..+0F05`).

### Overlay 193 `+1F88`: the dice of a `DATA` record

`+1F88` takes a `DATA` number `n`, a level byte `L` and a word pointer `p`; its direct callers are
overlay 193 `+04B0`, in `+003C` (FND-PARTY-107), and `+1431`, both near calls after `push cs`.
For `n` of -1 it returns 0. It copies 73 bytes of `DATA n` through `2C5F:0003` to a local and
lowers `L` to 10 when above. With `A` bits 5 to 7 and `B` bits 0 to 4 of byte `+0x1C`, `C` bits
3 to 7 and `E` bits 0 to 2 of byte `+0x1D`, `S` bits 0 to 3 and `F` the signed bits 4 to 7 of byte
`+0x1E`, it returns 0 when `A`, `B`, `C` and `S` are all 0. It sets `E` to 1 when 0, in the copy,
and takes `k = (L + F) / E`, a signed division, raised to 1 when below. Then, with the count
`m = C + A * k`, it stores `B * k + m * S` at `p` when `p` is not 0, and returns
`roll_sum(m, S) + B * k`, the call at `+20B1` (`+1F88..+20C6`).

### Overlay 198 `+041D`: a spell-use test

`+041D` takes a combatant number; its one direct caller is overlay 173 `+15F0`, after overlay
198 `+0149` of the combatant's byte `+0x0E` and 0 has returned other than -1, and it goes on when
the result is not 0. With `K` byte `+0x0E` of `R`, it rolls 1 and 4 at `+0451` and keeps 1 when
the total is 4 and 0 otherwise. It then indexes 20 words at `cs:0521` (file `0x87831`) by
`K - 1`, unsigned, and returns 0 for `K` of 0 or above 20 (`+0467..+0475`):

| `K` | Result |
|---|---|
| 1 to 4, 6 and 17 | 1 when word `+0` of `R` is below word `+8` of `D` halved (unsigned), else 0 |
| 5, 7, 8, 9, 14, 16, 18, 19 and 20 | the kept 1-in-4 result |
| 10 | 1 when overlay 195 `+0000` returns 4 in AL, else 0 |
| 11 and 12 | 1 when overlay 195 `+0000` returns 0, else the kept result |
| 13 | 0 |
| 15 | 1 |

### Overlay 204 `+214E`: a saving throw

`+214E` takes a slot `t`, a save number `s` and further arguments, among them a byte `x` at
`bp+0xC`; the direct-call query finds no caller of it or of its trampoline `5787:00B1`. It fills
`t`'s 16-byte block through overlay 193 `+1BA3` and returns 0 when bit 0x20 of the block's byte 8
is set or byte `+0x14` of `R` is 3. With `T` the byte `+0x30 + s` of `D`, it rolls 1 and 20 at
`+21B7`, keeping the low byte: 1 returns 0 and 20 returns 1. Otherwise it returns 0 when bit 0x80
of the block's byte 2 is set; it adds the byte result of `+2204` of `t`, the block and `s`, and
`x`, in a byte, and returns 1 when the sum is at least `T` (unsigned bytes) and 0 otherwise
(`+214E..+2204`). These are the steps of overlay 179 `+2A46` (FND-PARTY-099) from its roll on,
without its `DATA` record tests.

### Overlay 211 `+1E40`: a turning test

`+1E40` takes a slot, a second slot `c`, a flags pointer and a level byte `L`; its one direct
caller is overlay 179 `+1EA4`, the case of `DATA` 306 in `+1373`, which passes `t`, `c`, its flags
pointer and `a` (FND-RNG-034).
It goes on only when `2D40:3E64` finds the slot and byte `+0x12` of `D` is 9. With `h` byte
`+0x1E` of `D`: above 4 it becomes `h + 1`, and above 13 it becomes 13. `L` above 14 becomes 14;
then above 10 it is lowered by 1, and then above 12 by 1 again. The target is `3 * (h - L) + 10`,
and 20 when that is 22. It rolls 1 and 20 at `+1EDF`; when the total is at least the target it
calls overlay 179 `+2A05` with the slot, -1, -1 and 1000, damage of 1000 (FND-RNG-034), and sets
bit 0 at the flags pointer; when the total is below the target it calls `2D40:06E1` with its
second argument, `c`. Every other path returns without a call (`+1E40..+1F0F`).

## Interpretation

Overlay 191's three routines are item tests that `DATA` 103, 235, 275, 283 and 284 run: a picked
item is lost on a d20 below `8 -` its bonus byte `+0x16`; the lowest-ranked item is passed to
overlay 173 `+2B88` on a d20 below `15 -` that byte; and the first item of type 0, 6 or 9 is lost
at once when its word `+0x10` is 0 and on a d20 below `10 -` that word otherwise. Overlay 193
`+1F88` is the dice formula a `DATA` record gives by level: `(C + A * k)` dice of `S` sides plus
`B * k`, with `k` the level, offset by `F`, per `E` levels, and the level capped at 10. Overlay
198 `+041D` decides, by a creature kind byte, whether a non-party combatant uses a spell from its
list: at half hit points for some kinds, one time in four for others, always or never for two.
Overlay 204 `+214E` is a second copy of the saving throw roll. Overlay 211 `+1E40` turns a target
whose `D` byte `+0x12` is 9 on a d20 at least `3 * (h - L) + 10`, adjusted, by 1000 damage. No
rule entry describes these yet.

## Alternatives

- What overlay 173 `+2E1B` and `+2B88`, overlay 195 `+0000` and `2D40:06E1` do, what the text at
  `DS:205C` and `DS:2070` says, and which items the types 0, 6 and 9 are, were not read.
- The `DATA` dice formula's callers' use of the total and of the value stored at `p` was not
  read beyond the calls; reading `L` as the caster's level rests on the callers' arguments only.
- `+214E` with no direct caller may be reached through a computed call; no search for one was
  made.
- Reading `D` byte `+0x12` 9 as undead and `+1E40` as turning rests on the target table's shape
  and the 1000 damage alone.
- The overlays are outside the resident harness; no emulated call or run was made.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`: `overlay_listing.py <dsun> 191
7C1F0 7D212`, `193 7E2F0 80ECE`, `198 87310 878CC`, `204 8BDC0 8E660` and `211 95E10 97F00`, and
read the ranges above and the 20 words at file `0x87831`; `direct_callers.py <dsun> 191+0CD0
191+0D60 191+0E3A 193+1F88 198+041D 204+214E 211+1E40`. Output is kept under
`GAME_DIR/analysis/rng-callers-20261020/`. Nothing was executed.
