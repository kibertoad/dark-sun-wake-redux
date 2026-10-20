---
id: FND-RNG-049
title: Overlay 195 +00D1 runs a chain of four effect steps and five more by the target's effect block, with seven roll_sum sites, the damage of +0C7F, two 1000-point damages of +22C4, and texts the name can replace with a short one
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00081841..0x00081EF9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008218F..0x00082196
tool: tools/research/exec-census/overlay_listing.py and trampoline_target.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset` and are in overlay 195, whose code starts at file
`0x81770`, unless another overlay is named. FND-RNG-039 gives the routine's caller and its steps
in outline; this reads each step to its last instruction (`+00D1..+0788`). `s` is the slot
argument. `B` is the 16-byte block overlay 193 `+1BA3` (`573B:0025`) fills for `s` at `bp-0x1A`.
`R(x)` is the `combatants` record numbered by the word at `4F49:0C34 + 3x`, and `D` the
`combatant_details` record `R(s)`'s word `+0x04` numbers. "Entry `k` of `x`" is the result of
`3143:00A8` with `x` and `k`. For an entry `e`, the words `+2` and `+4` and bytes `+8` and `+9`
are those at `4E71:024F`, `4E71:0251`, `4E71:0255` and `4E71:0256` plus `10e`; no step but the
first, fourth and ninth tests `e` for -1 before reading them. "Damage of `x` by `y` with code `c`
and amount `a`" is a far call of overlay 179 `+0C7F` (`56A7:0066`) with the 13 words `x`, `y`,
-1, `c`, a far pointer to a local word holding `a`, `a`, 0, 0, 1, 0xFF, 1 and 0.

**Set-up.** It fills `B`, calls overlay 193 `+0DC7` (`573B:005C`) with `s` and 1, and for each
slot `c` from 0 to 47 whose entry 0x1E is not -1 and whose `R(c)` byte `+0x14` is 3 or more
(unsigned) calls overlay 193 `+194F` (`573B:004D`) with `c` and 0x1E (`+00D1..+0141`).

**The chain.** Exactly one of these runs, the first whose bit is set; each goes on to step 5.

1. Bit 0x40 of `B` byte 3. Nothing more for `s` of 4 or more (signed). Otherwise it formats the
   string at `DS:2128` with `combatants` record `s`'s name (`DS:19C9 + 49s + 0x21`) into
   `DS:43FB` through `3150:000E`, and when `1000:37D8` gives that text a length above 29 it
   formats `DS:213D` there instead. It calls `+152F`, the d100 test of FND-RNG-039, with `s`. On
   0 it passes `DS:43FB` to `566A:002A`. Otherwise it calls `+194F` with `s` and 0x1E, formats
   `DS:2143` with the name, and passes the text to `566A:002A` unless its length is above 29.
   Then, when entry 0x1E of `s` is not -1, it calls `28C9:391D` with entry byte `+9`
   sign-extended as the count and 20 as the sides at `+020D`, and damages `s` by the entry's word
   `+2` with code 0x40 and the total (`+0141..+024A`).
2. Bit 0x10 of `B` byte 12. It lowers `R(s)` byte `+0x1B` by 1, and when the byte is then 0 calls
   overlay 173 `+22C4` (`5671:005C`) with `s` and 1000. It takes entry 0x64 of `s` without a -1
   test, and ends unless its word `+4` is 0x122 and `s` is below 4. It formats the `DS:2128` text
   as step 1 does and calls `+152F` with `s`. On a nonzero result it calls overlay 197 `+30ED`
   (`575A:008E`) with 0x122 and `s` and formats the `DS:2143` text. Either way it passes the text
   to `566A:002A` unless its length is above 29 (`+024D..+035D`).
3. Bit 0x01 of `B` byte 2. With `e` entry 0x10 of `s`, it calls `+194F` with `s` and 0x45, and
   overlay 179 `+2A46` (`56A7:0075`) with `s`, `e`'s word `+2`, 188 and the result of overlay 193
   `+1A2D` (`573B:0052`) with `s` and 0x10. When that byte is 0 it calls overlay 193 `+166F`
   (`573B:0039`) with `s`, `e`'s word `+2`, 188, 0x45, the doubleword -9999 and 0; otherwise it
   calls `+194F` with `s` and 0x10. It then calls `28C9:391D` with 1 and 4 at `+03E9`, adds the
   result of overlay 173 `+2C00` (`5671:007A`) with `s`, `s` and 0, and when the sum is above 0
   (signed) damages `s` by `e`'s word `+2` with code 0x20 and the sum (`+0360..+0421`).
4. Bit 0x04 of `B` byte 6. When `+152F` of `s` is not 0 it calls overlay 193 `+1AA4`
   (`573B:0057`) with `s` and 0x32, and then, when entry 0x32 of `s` is not -1 and its byte `+8`
   is below 2, `+194F` with `s` and 0x45 (`+0424..+046C`).

**The other steps.** Each runs when its condition holds, in this order.

5. `D` byte `+0x12` is 0x10 and bit 0x01 of `B` byte 3 is set. With `e` entry 0x18 of `s`,
   taken without a -1 test, and `v` its word `+2`: when `R(v)` byte `+0x14` is 3 or more, or when
   bit 0x04 or 0x20 of byte 8 of `v`'s own block (filled through `+1BA3`) is set and `e`'s byte
   `+8` is already 4 or more, it passes the string at `DS:2150` to `566A:002A`, stores `s` in the
   word at `DS:444B`, calls `+30ED` with 0x12D and `s`, and calls `+22C4` with `v` and 1000. When
   either bit is set and byte `+8` is below 4 it adds 1 to that byte. With neither bit it calls
   `+30ED` with 0x12D and `s` (`+046F..+0568`).
6. Bit 0x20 of `B` byte 12. Entry 0x65 of `s`, no -1 test; 1 and 4 at `+0585`; damage of `s` by
   the entry's word `+2` with code 2 (`+056B..+05C1`).
7. Bit 0x02 of `B` byte 0. Entry 1; 2 and 4 at `+05DE`; code 0x40 (`+05C4..+061A`).
8. Bit 0x08 of `B` byte 2. Entry 0x13; 2 and 4 at `+0637`; code 2 (`+061D..+0673`).
9. Bit 0x80 of `B` byte 13. With `v` the word `+2` of entry 0x6F of `s`, taken without a -1
   test: nothing when `R(v)` byte `+0x14` is 3 or more. Otherwise, when entry 0x6E of `v` is -1,
   it calls `+194F` with `s` and 0x6F. When it is not, it rolls 2 and 4 at `+06E5` as `m`, and
   for `i` from 0 to `m` damages `v` by `s` with code 0x10 and an amount of 8 for `i` of 0 and
   the total of 2 and 4 at `+0705` for each later `i`; afterwards it calls `+194F` with `s` and
   0x6F when `R(v)` byte `+0x14` has become 3 or more (`+0676..+076B`).

**The end.** When the byte at `50C2:0000` is 0 it near-calls `+0A1F` with `s`; `+0A1F` is
`inc bp`, `push bp`, `mov bp, sp`, `pop bp`, `dec bp` and `ret`, and does nothing. It then calls
overlay 173 `+24C7` (`5671:00B1`) with no arguments and returns (`+076E..+0788`).

**The strings.** `DS:2128` is `%Fs TRIES TO ESCAPE!` (20 bytes), `DS:213D` `HELP!`, `DS:2143`
`%Fs ESCAPES!` (12 bytes) and `DS:2150` `FLAYER HASSSS EATEN BRAIN!`. A name of 13 to 16
characters makes the first text longer than 29; the second, with a name of at most 16, never is.

## Interpretation

The routine is the start-of-round handling of a combatant's effects. Steps 1 and 2 are two holds a
target tries to escape on the d100 check of RULE-COMBAT-013, a party member only, with damage each
round from step 1's hold: as many d20 as the entry's byte `+9`, from the entry's word `+2`, which
other steps also treat as the effect's source. Step 3 is an effect that offers a saving throw each
round against `DATA` 188 and deals 1d4 plus a term of overlay 173. Steps 6 to 8 deal 1d4 or 2d4 a
round. Step 9 reverses the direction: the source of effect 0x6F on `s` takes 8 and then `m` more
hits of 2d4 while it holds effect 0x6E. Step 5 counts rounds of a hold by a creature of details
kind 16 and ends with the brain text and 1000 damage to the held target.

## Alternatives

- What overlay 193 `+0DC7`, overlay 173 `+2C00` and `+24C7`, and overlay 197 `+30ED` with 0x122
  and 0x12D do, and what the twelve words after the code tell `+0C7F`, were not read.
- Reading word `+2` of an entry as the effect's source rests on its use as the damage source and as
  the target of step 9; reading step 5 as a mind flayer's hold rests on the text alone.
- An entry taken without a -1 test reads the bytes before the table, at `4E71:0245` to
  `4E71:024C`; what is there was not read.
- The overlay is outside the resident harness; no emulated call or run was made.

## How to reproduce

Use the installed `DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`) with
the locked evidence Python. In `tools/research/exec-census/`, run `overlay_listing.py <dsun> 195
81770 82DAA` and read `+00D1..+0788` and `+0A1F..+0A25`; run `trampoline_target.py <dsun>
573B:005C 5671:007A 5671:00B1 56A7:0066`; and print the strings at file `0x4F128`, `0x4F13D`,
`0x4F143` and `0x4F150`. The listing is kept as `ovl195-all.txt` under
`GAME_DIR/analysis/rng-callers-20261020/`. Nothing was executed.
