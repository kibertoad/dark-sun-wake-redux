---
id: RULE-RNG-001
title: The game's random number generator and its reductions
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-001, FND-RNG-002, FND-RNG-011, FND-RNG-012, FND-RNG-013, FND-RNG-006, FND-SCRIPT-011, FND-RNG-009, FND-RNG-027, FND-RNG-028, FND-RNG-029, FND-RNG-032, FND-RNG-044, FND-RNG-035, FND-RNG-039, FND-RNG-040, FND-RNG-041, FND-RNG-043, FND-RNG-045, FND-RNG-046, FND-RNG-047, FND-RNG-048, FND-RNG-049, FND-RNG-050, FND-RNG-052, FND-RNG-053, FND-RNG-054, FND-RNG-055, FND-RNG-056, FND-RNG-057, FND-RNG-058, FND-RNG-059, FND-RNG-060, FND-RNG-061, FND-RNG-062, FND-RNG-063]
conflicting: []
split_with: []
related: [RULE-VIDEO-005, RULE-MAGIC-005, RULE-MAGIC-006, RULE-AI-003, RULE-COMBAT-012, RULE-COMBAT-013, RULE-ITEM-007, RULE-ITEM-008, RULE-ITEM-009, RULE-MAGIC-007, RULE-MAGIC-008, RULE-MAGIC-009, RULE-MAGIC-010, RULE-MAGIC-011, RULE-MAGIC-012, RULE-MAGIC-013, RULE-MAGIC-014, RULE-MAGIC-015, RULE-MAGIC-016, RULE-MAGIC-017, RULE-MAGIC-018, RULE-MAGIC-019, RULE-MAGIC-020, RULE-MAGIC-021, RULE-MAGIC-022, RULE-SCRIPT-012, RULE-COMBAT-017, RULE-MAGIC-023]
---

## Summary

The game has one generator, `rng`, a linear congruential generator whose 32-bit state is
`rng_state`. A draw gives 0 to 32767. `random_mod(n)` uses a signed word divisor;
for positive n it gives 0 to n - 1. `random_between` scales one draw using a
signed word span, whose wrap can produce values outside the endpoints.
`roll_sum` scales one draw per positive-count roll and accumulates only the
low 16 bits. For positive sides each term is 1 to sides. `chance_in_ten(n)`
uses the existing threshold rule: n from 1 to 9 succeeds on n + 1 of ten
remainders, 10 always succeeds without drawing, and other values fail.

## When it runs

Wherever a rule draws. Recovered resident callers of the generator are `random_mod`,
`random_between`, `roll_sum` and the script instruction of RULE-SCRIPT-006 (FND-SCRIPT-011), and
`chance_in_ten` draws through `random_mod`. Recovered direct seed callers
occur in startup's overlay-180 routine and overlay 204's separate entry; FND-RNG-053 finds the
first called only once, from startup, and the second only from its four known calls.
Both call an OS-service-backed helper with no output destination and seed
from only its low 16-bit return word (FND-RNG-009). The helper's date and time buffers are in
its stack frame, which its wrappers address through `DS`; on every path after startup `DS` equals
`SS` at the entry of overlay 193 `+003C`, overlay 201 `+05D5` and overlay 204 `+1F06`, the
reseeding callers FND-RNG-053 names (FND-RNG-056, FND-RNG-061). The helper's complete
conversion and admitted reseeding paths remain unknown (Q-RNG-003).
The conversion reads one environment string, the value of the first `TZ=` entry of the
environment the operating system passes at startup, which the game copies once and does not
change; the game does not decide that environment (FND-RNG-054).
The direct-call census (FND-RNG-028) finds 29 direct calls of the generator, 60 of `roll_sum`,
11 of `random_between`, 11 of `random_mod` and 4 of `chance_in_ten`; `roll_sum` and
`random_between` are called only from overlays, and `random_mod` and `chance_in_ten` only from
the music selector and `chance_in_ten`. Recorded findings describe 52 of those sites, and
FND-RNG-029 five more: the generation screen's opening picture and RULE-PARTY-016's spell picks.
Overlay 176's five `random_between` sites (FND-RNG-025) are the psionic power check, which
RULE-MAGIC-014 takes, and the psionic defence of a target, which RULE-MAGIC-015 takes
(FND-RNG-046). Overlay 179's six (FND-RNG-026) sample a cast's area points for overlay 193
`+0617`, by the mode of the cast `DATA` record's effect row, which RULE-MAGIC-023 takes with the
row's amount dice (FND-RNG-052).
FND-RNG-043 reads overlay 206's six: two draws per pixel of a removed combatant's dissolve and a
`roll_sum` draw choosing a combatant's cry, which RULE-SOUND-004 and RULE-VIDEO-006 take; the events
that reach them remain Q-RNG-014, Q-RNG-026 and Q-RNG-027; overlay 173's removal list is
emptied after the routines that apply hits or damage (FND-RNG-063).
FND-RNG-050 reads the other calls of the removal routine overlay 206 `+0E1E`: six make no draw,
five reach draws only through effect row removals, an area-effect routine or the script
interpreter (FND-RNG-060), and overlay 201 `+05D5`, run in
place of the cry for a combatant whose word `+0x1B` is 430, calls RULE-VIDEO-005's offset loop,
which reseeds from the clock and draws once, while sound 230 plays and at one picture frame.
FND-RNG-041 reads overlay 173's seven: two fight set-up words per combatant, a retried pick
from a distance-sorted list of slots, a removed combatant's drop placement and an item-break test
after every damaging armed swing, which RULE-COMBAT-014, RULE-COMBAT-015, RULE-COMBAT-016 and
RULE-COMBAT-011 take.
FND-RNG-032 reads the last, overlay 197 `+1082`: a
d20 to-hit roll that five `DATA` numbers cast by a combatant numbered 48 or more must pass before
the hit routine applies them, which RULE-COMBAT-010 takes. FND-RNG-044 reads overlay 179's
nineteen `roll_sum` sites, the dice of the hit routine's per-`DATA` effects, whose rule entries
RULE-MAGIC-016 to RULE-MAGIC-022 are, from FND-RNG-047's reading of every path through them. FND-RNG-035 reads seven more, in overlays 191, 193, 198, 204 and 211: three
d20 item tests, a `DATA` record's dice formula by level, a one-in-four spell-use test, a copy of
the saving throw roll and a d20 turning test, and FND-RNG-039 overlay 195's ten: a d10 confusion
table, a side roll, a d100 test and the dice of per-round effect damage. RULE-MAGIC-005 takes the
dice formula, RULE-MAGIC-006 the d20 test of `DATA` 306 and RULE-AI-003 the spell-use test,
RULE-COMBAT-012 the d10 table, RULE-COMBAT-013 the d100 test and RULE-ITEM-007, RULE-ITEM-008
and RULE-ITEM-009 the three item tests; RULE-SCRIPT-012 takes the saving throw copy, which FND-RNG-048 finds reached
only from function 41 of script opcode `0x22`, and RULE-COMBAT-017 the per-round effect damage, from FND-RNG-049's reading of every step. FND-RNG-040 reads overlay 197's ten `roll_sum` sites: a
d6, a d100, a 2d6 and a d20 in the hit routine's gates, which RULE-MAGIC-007 to RULE-MAGIC-010
take, a d100 under the preserver level in a check routine, which RULE-MAGIC-011 takes, d4 and d12
picks of a summoned creature, which RULE-MAGIC-012 takes, and the 4d20 pool of `DATA` 71, which
RULE-MAGIC-013 takes. That reads
every site of the direct-call census. No stored far pointer, segment alias or declared computed
transfer names the generator, the seed setter or a reducer (FND-RNG-045), so those sites are all
of their calls.

Overlay 204's entry, RULE-VIDEO-005's display offset loop, runs, and reseeds, each time a script
issues request 27 of instruction `0x22`; 44 shipped instructions do, in 21 `GPL` resources (FND-RNG-027).
It also runs from overlay 201's removal effect (FND-RNG-050) and for a cast of `DATA` 293 whose
cast routine byte `bp+0x1A` is not 0, as every queued cast's is (FND-RNG-053). In the shipped
data that cast is the special attack of a combatant whose special attack byte is 6, made once
before the byte is cleared, or the same cast sent back, one time in six, by a target whose byte
is 13 (FND-RNG-055). A monster's byte comes unchanged from its `RDFF` object's combatant chunk, and
only `RDFF` 428 (`Grt Earth Elem`) holds 6 and only `RDFF` 430 (`Tarrasque`) 13 (FND-RNG-062). No shipped script uses
instruction `0x59`, so FND-RNG-024's d20 threshold test does not draw in the shipped game
(FND-RNG-027).

## Parameters

`seed_random(value)`, `random_mod(n)`, `random_between(lower, upper)`, `roll_sum(count, sides)`
and `chance_in_ten(n)`.

## Inputs

`rng_state`.

## Procedure

Integer division below truncates toward zero; a % b is a minus that
truncated quotient times b, so the remainder has the dividend's sign.
Word casts retain low 16 bits;
INT16 interprets that word as signed, and UINT16 as its unsigned bit value.
The roll total is a returned word; each caller's width and signedness are
recorded with its site (FND-RNG-043, FND-RNG-044, FND-RNG-035, FND-RNG-039, FND-RNG-040), rather than an unbounded default integer.

```text
# draw() is the generator: it sets
#     rng_state = UINT32(rng_state * 0x015A4E35 + 1)
# and gives (rng_state >> 16) & 0x7FFF, a value from 0 to 32767.

define seed_random(value: UINT16):
    rng_state = value

define random_mod(n: UINT16) -> UINT16:
    if n == 0:
        return 0
    return UINT16(INT16(draw()) % INT16(n))

define random_between(lower: INT16, upper: INT16) -> INT16:
    if lower >= upper:
        return lower
    let span = INT16(upper - lower + 1)
    let scaled = (INT32(draw()) * INT32(span)) / 32768
    return INT16(lower + INT16(scaled))

define roll_sum(count: INT16, sides: INT16) -> UINT16:
    let total: UINT16 = 0
    let left = count
    while left > 0:
        let term = UINT16((INT32(draw()) * INT32(sides)) / 32768 + 1)
        total = UINT16(total + term)
        left = left - 1
    return total

define chance_in_ten(n):
    if n < 1 or n > 10:
        return false
    if n == 10:
        return true
    return random_mod(10) <= n
```

## Outputs

`random_mod` and `random_between` return the values above and `roll_sum` returns
the accumulated low word. `chance_in_ten` returns true or false. Each helper makes the draws its procedure
shows and changes no state other than `rng_state`.

## Edge cases

`random_mod(0)`, `random_between` with `lower` at or above `upper`, `roll_sum` with a `count` of 0
or less, and `chance_in_ten` with an argument outside 1 to 9 make no draw. `chance_in_ten(n)` for
`n` from 1 to 9 succeeds when the draw modulo 10 is at most `n`, which is `n + 1` values out of
ten, so `chance_in_ten(9)` always succeeds as well, at the cost of a draw. `random_mod` keeps the
bias of a plain remainder: for a divisor that does not divide 32768, the low results come up more
often for a positive divisor. An argument word above 32767 uses its signed
negative interpretation: the remainder is nonnegative and less than its
mathematical absolute magnitude. For example, word 65535 always yields zero
but still draws (FND-RNG-011).

A positive range length of 32768 becomes span -32768; with lower 0, upper
32767 and draw 32767, the returned signed value is -32767. The full signed
endpoint interval wraps its span to zero, so lower -32768 and upper 32767
return lower while still drawing (FND-RNG-012).

Positive roll counts with sides zero still consume count draws and add one
per draw. Negative sides use signed scaling. The total wraps modulo 65536;
callers determine its signed or unsigned interpretation (FND-RNG-013).
`seed_random` leaves the high word of the state 0.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Which date and time the DOS services return to the clock helper, the conversion's only caller,
  which passes them from its own frame (FND-RNG-009, FND-RNG-014..015, FND-RNG-057). FND-RNG-023 settles local
  date-before-time sampling without a coherence retry; external service
  domain and rollover guarantees remain Q-RNG-006 and Q-RNG-028, with conditional pinned
  handler-source leads in SRC-RUSTDOS-172. The conversion's tables, label pointers and name are
  only read on a walk from every established entry, its offset record is written only by the
  timezone initializer and its environment root only by the startup constructor (FND-RNG-019..020,
  FND-RNG-058); writes through pointers formed elsewhere remain Q-RNG-005, Q-RNG-029 and
  Q-RNG-030; the copies of segment `2707` write only video memory (FND-RNG-079, FND-RNG-080).
  One such write is found: with `DS:426D` holding 9999, a view click sends that slot to
  `2D40:2F34`, whose link store puts a record number over the July and August lengths at
  `DS:3956`, so a later reseed converts dates from August on with those lengths (FND-RNG-067,
  FND-RNG-068); the end of combat leaves that 9999 until a leader key (FND-RNG-069, FND-RNG-070),
  the routines the path goes on to call write none of the windows (FND-RNG-071), and
  `28C9:1EDB` stores nothing with 9999 until the record is filled and then calls overlay 173
  `+059A` with combat word 0 on every pass (FND-RNG-074, FND-RNG-075), which indexes nothing with
  9999 and runs a combat round step each time (FND-RNG-076, FND-RNG-077, BUG-COMBAT-001); overlay 173
  `+059A` stores 9999 only when it ends combat (FND-RNG-078), and block copies or file reads that
  could pass 9999 and the route through `31E0:2C86` remain Q-RNG-031 and Q-RNG-034. Whether the game
  creates objects 428 and 430, and whether the party's
  target cursor can pass `DATA` 293, remain open (FND-RNG-055, FND-RNG-062, FND-RNG-064, Q-RNG-003, Q-RNG-025). The second root's local reseeding and
  conditional draw loop, and its script wrapper widths, are recorded in
  FND-RNG-021..022, and the shipped scripts that issue it in FND-RNG-027.
- The events that reach overlay 206's
  dissolve and cry draws (FND-RNG-043, FND-RNG-063, Q-RNG-014, Q-RNG-026, Q-RNG-027), and whether the five callees of the removal
  routine that reach draws only through long routes draw when it calls them: four only through
  overlay 195 `+1000` when an effect row is removed, or overlay 193 `+0DC7` (FND-RNG-050,
  FND-RNG-060, Q-RNG-022), and `1695:02E7` through the scripts it runs (Q-RNG-024). Recovered `random_mod`
  uses include `chance_in_ten`, the table selection in FND-RNG-007 and the music selector of
  RULE-SOUND-003 (FND-SOUND-012).
  FND-PARTY-099 recovers an effect-saving caller of `roll_sum(1, 20)`; the earlier statement
  that `roll_sum` had no recovered caller was stale.
- What the readers of a cast's sampled area points do with them and with the amount, and what
  shapes the area (FND-RNG-052, Q-MAGIC-017).
- Whether computed, aliased or bulk writes change `rng_state`. On a walk from every established
  entry only the generator and the setter name it, and the handle flag words below it reach it
  only for handles 67 and 68, which two file routines do not bound (FND-RNG-001, FND-RNG-059,
  Q-RNG-007). It holds 1 at load (FND-RNG-059).
