---
id: FND-RNG-034
title: Overlay 179's nineteen unread roll_sum calls are the dice of nineteen cases for 22 DATA numbers in the hit routine's effect switch, for damage, healing, durations, a dispel chance, a death test, a raising test and a reincarnation
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065653..0x00066B11
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00066B11..0x00066CE5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066CE5..0x00066D26
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066EFA..0x00066F95
tool: tools/research/exec-census/overlay_listing.py and trampoline_target.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 179's code starts at file `0x642E0`
and is `0x2CB5` bytes. Overlay 179 `+1373` is the routine the hit routine `+0D2F` calls at
`+1134` (FND-PARTY-096). It keeps its first argument, the target `t`, in `DI`; its second is the
second slot `c`, its fourth the `DATA` number `n`. Its other arguments, from the pushes at
`+1115..+1131`: `s` at `bp+0x10`, the byte `+0D2F` stores at `bp-1` from its saving throw call
`+2A46` at `+0EDF` (1 for a success, FND-PARTY-099); a flags pointer at `bp+0x12`; the bytes `a`
at `bp+0x14` and `g` at `bp+0x16`, from `+0D2F`'s byte arguments at `bp+0x1C` and `bp+0x1E`; and
the word `u` at `bp+0x18`, `+0D2F`'s local at `bp-0xC`, which holds -9999 (0xD8F1) or the result
of overlay 193 `+1E81` (`+0E32..+0E4D`). `+1373` takes `t`'s combatant record (`DS:19C9`, 49-byte
records) as `R` and its details record (`DS:19C5`, 66 bytes) as `D` (`+138E..+13ED`).

`+1373` compares `n` with the 81 words at `cs:28C1` and jumps through the 81 words after them at
`cs:2963`, going to `+2A32` when none matches (`+13F0..+140C`). Decoding each case to its
`roll_sum` call (`28C9:391D`, whose first pushed-last argument is the count and second the sides,
RULE-RNG-001) shows FND-RNG-028's nineteen overlay 179 sites as instructions. Every jump that lands
between a case label and its site comes from inside that case, except the jump at `+21B0`, the
whole of case 117, which goes to the site `+225D` with the count 1 and sides 8.

`+2A05`, which the cases reach through `push cs` and a near call, takes a slot, `n`, a code and an
amount; it calls overlay 193 `+1AA4` with the slot and 5, then overlay 206 `+0B81` when the word at
`DS:0DAB` is 1 and overlay 206 `+1121` otherwise, and passes the slot and the amount to overlay 173
`+22C4`, the damage routine (FND-PARTY-086) (`+2A05..+2A46`). Overlay 197 `+2D05` (two arguments,
the slot and an amount) and `+2EC0` and `+2E40` (four: `t`, `c`, an amount and `n`) are called
with the totals below; their bodies were not read. "Effect `k`" below is overlay 193 `+166F`,
the effect routine (FND-PARTY-096), called with `t`, `c`, `n`, code `k`, a dword duration and a
last word.

| Site | `DATA` | Count, sides | Condition | Use of the total |
|---|---|---|---|---|
| `+145E` | 227 | 8, 6 | byte `+0x12` of `D` is 9 | `+2A05` with code 0x86: damage |
| `+14C8` | 295 | 2, 4 when `s` is not 0; 2, 10 when it is 0 (`+14DF`) | none | `+2A05` with code 0x20: damage |
| `+14EF` | 298 | 3, 6 | `s` is not 0 | `+2A05` with code 8: damage |
| `+1725` | 89 | 4, 4 when word `+0` of `R` is below 31; 2, 4 below 61; 1, 4 below 91; no roll and a duration of 0 otherwise | none | times 60, kept as a signed byte, as effect 0x45's duration |
| `+18AA` | 312 | 1, 6 | `t` is not `c` | overlay 177 `+0CB3` with `t`, `c` and the total |
| `+1D01` | 60, 165 | 1, 20 | none | the case goes on when the total is at least `11 - a +` byte `+0x1E` of `D`, and then, when byte `+0x12` of `D` is 14, damages `t` by 10,000 through `+22C4` |
| `+1E06` | 26, 150 | 1, 100, once per effect | for each effect `k` from 0 to 112 whose bit is set in `t`'s block, other than 4, 8, 27, 37, 58, 64, 66, 68, 71 and 106 (`cs:2881`) | removes `k` through overlay 193 `+194F` when the total is at most `50 + 5 * a - 5 *` overlay 193 `+1A2D` of `t` and `k` |
| `+1F74` | 23, 245 | 1, 6 | none | effect 98 for 23 and 2 for 245, with duration `u` and the total as its last word |
| `+214E` | 210 | 1, 4 | none, after overlay 197 `+30ED` and removing 19 effects through `+194F` | overlay 197 `+2D05` with `t` and word `+8` of `D` less word `+0` of `R` less the total |
| `+2188` | 211 | 1, 4 | none, after effect 0x1B with duration -9999 | `+2A05` with code -1 and word `+0` of `R` less the total: damage |
| `+21B9` | 170 | 2, 8 | none | plus 1, to overlay 197 `+2D05` |
| `+21D7` | 194 | 3, 8 | none | plus 3, to overlay 197 `+2D05` |
| `+225D` | 166, and 117 | 2, 6 for 166; 1, 8 for 117 | for 166, byte `+0x15` of `R` equals `g` | overlay 197 `+2D05` |
| `+229D` | 127 | 1, 8 | none | overlay 197 `+2E40` |
| `+22D3` | 38 | `a / 2` (rounded down), 6 | `c` is below 9999 | overlay 197 `+2EC0` |
| `+2308` | 243 | 1, 6 | `c` is below 9999 | overlay 197 `+2EC0` |
| `+24E1` | 203 | 1, 100 | `+2C1A` of `t` returns 1 | the raising goes on when the total is below `5 * (`byte `+0x1B` of `R` `- 9) + 70`, unsigned byte read, signed compare |
| `+25DC` | 77 | 1, 8, again while the total is 1 and byte `+0x12` of `D` was not 1 before | `+2C71` of `t` returns 1 | stored in byte `+0x12` of `D` |
| `+25FE` | 77 | 1, 2 | after `+25DC` | stored in byte `+0x13` of `D` |

Further detail for the cases whose totals decide a branch:

- `DATA` 166 with byte `+0x15` of `R` other than `g` makes no roll: when `s` is 0 it applies effect
  0x45 with duration `u`.
- `DATA` 203: `+2C1A` returns 1 when `2D40:3E64` finds `t`, `t` is below 4, byte `+0x14` of `R` is
  7 and byte `+0x12` of `D` is not 3 (`+2C1A..+2C71`). On a success it lowers byte `+0x17` of `D`
  and byte `+0x1B` of `R` by 1, sets word `+0` of `R` to 1 and byte `+0x14` to 1, and applies
  effects 0x42, 0x44, 0x41 and 0x43 with `DATA` number 203 and duration -9999
  (`+2502..+259B`); otherwise it shows the text at `DS:09A4` and sets byte `+0x14` of `R` to 8
  (`+259E..+25B2`).
- `DATA` 77: `+2C71` returns 1 when `2D40:3E64` finds `t`, `t` is below 4 and byte `+0x14` of `R`
  is 7 (`+2C71..+2CB5`). After the two rolls, byte `+0x12` 8 sets byte `+0x13` to 2 and word
  `+0x10` of `R` to 13; 7 sets byte `+0x13` to 1 and word `+0x10` to 12; any other value sets word
  `+0x10` to twice byte `+0x12` plus byte `+0x13` less 3. It then copies word `+8` of `D` to word
  `+0` of `R`, sets byte `+0x14` to 1 and places a new object for `t` through `31E0:0121`
  (`+260D..+26EB`).
- `DATA` 89's byte keeps only the low eight bits of `60 * total`: totals of 4 and 16 give 240 and
  960, stored as -16 and -64.

## Interpretation

The nineteen sites are the dice of the hit routine's per-`DATA` effects. `DATA` 117, 170 and 194
heal 1d8, 2d8+1 and 3d8+3 and `DATA` 166 heals 2d6 when `t`'s byte `+0x15` matches `g`; `DATA`
210 brings word `+0` of `R` to word `+8` of `D` less 1d4, and `DATA` 211 brings it down to 1d4,
which reads as hit points to the maximum less 1d4 and down to 1d4. `DATA` 227, 295, 298, 38 and
243 do dice damage. `DATA` 26 and 150 remove each removable effect on a d100 against a chance
that rises with `a` and falls with the effect's level; `DATA` 60 and 165 kill a target whose `D`
byte `+0x12` is 14 when a d20 reaches `11 - a +` its byte `+0x1E`; `DATA` 203 raises a party
member whose `R` byte `+0x14` is 7 on a d100 below a value from its byte `+0x1B`; and `DATA` 77
gives such a party member a new origin byte from 1 to 8 and gender byte from 1 to 2. No rule
entry describes these effects yet.

## Alternatives

- Which spells, powers or items the `DATA` numbers are, what `a` and `g` hold, what codes 0x45,
  98, 2, 0x1B and the others are, and what overlay 197 `+2D05`, `+2E40`, `+2EC0`, overlay 177
  `+0CB3` and overlay 193 `+1A2D` do, were not read. Reading `+2D05` as healing rests on the
  dice and the subtraction of `DATA` 210 alone.
- Byte `+0x1B` of `R` read as Constitution, byte `+0x14` 7 as dead, and bytes `+0x12` and `+0x13`
  of `D` as origin and gender (FND-PARTY-070) are readings from other findings, not checked here.
- The overlay is outside the resident harness; no emulated call or run was made.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`: `overlay_listing.py <dsun> 179
642E0 66F95` and, for `+2A05`, `overlay_listing.py <dsun> 179 66CE5 66D26`; read the ranges above,
the 81 keys at file `0x66BA1` and the 81 targets at `0x66C43` as little-endian words, and the ten
words at `0x66B61`. For each site, list the jumps whose targets lie between its case label and the
site. Listings are kept under `GAME_DIR/analysis/rng-callers-20261020/`. Nothing was executed.
