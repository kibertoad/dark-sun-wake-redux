---
id: RULE-RNG-001
title: The game's random number generator and its reductions
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-001, FND-RNG-002, FND-RNG-011, FND-RNG-012, FND-RNG-013, FND-RNG-006, FND-SCRIPT-011, FND-RNG-009, FND-RNG-027, FND-RNG-028, FND-RNG-029, FND-RNG-030, FND-RNG-031, FND-RNG-032, FND-RNG-034, FND-RNG-035]
conflicting: []
split_with: []
related: []
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
occur in startup's overlay-180 routine and overlay 204's separate entry.
Both call an OS-service-backed helper with no output destination and seed
from only its low 16-bit return word (FND-RNG-009). The helper's complete
conversion and admitted reseeding paths remain unknown (Q-RNG-003).
The direct-call census (FND-RNG-028) finds 29 direct calls of the generator, 60 of `roll_sum`,
11 of `random_between`, 11 of `random_mod` and 4 of `chance_in_ten`; `roll_sum` and
`random_between` are called only from overlays, and `random_mod` and `chance_in_ten` only from
the music selector and `chance_in_ten`. Recorded findings describe 52 of those sites, and
FND-RNG-029 five more: the generation screen's opening picture and RULE-PARTY-016's spell picks.
FND-RNG-030 reads overlay 206's six: two draws per pixel of a removed combatant's dissolve and a
`roll_sum` draw choosing a combatant's cry, whose triggers and rule entries remain Q-RNG-014.
FND-RNG-031 reads overlay 173's seven: two fight set-up words per combatant, a retried target
pick, a removed combatant's drop placement and an item-break test after every damaging armed
swing, whose rule entries remain Q-RNG-015. FND-RNG-032 reads the last, overlay 197 `+1082`: a
d20 to-hit roll that five `DATA` numbers cast by a combatant numbered 48 or more must pass before
the hit routine applies them, which RULE-COMBAT-010 takes. FND-RNG-034 reads overlay 179's
nineteen `roll_sum` sites, the dice of the hit routine's per-`DATA` effects, whose rule entries
remain Q-RNG-017. FND-RNG-035 reads seven more, in overlays 191, 193, 198, 204 and 211: three
d20 item tests, a `DATA` record's dice formula by level, a one-in-four spell-use test, a copy of
the saving throw roll and a d20 turning test, whose rule entries also remain Q-RNG-017. The other
20 `roll_sum` sites (Q-RNG-013) still need qualification and rule mapping, and calls not made directly remain
Q-RNG-008.

Overlay 204's entry runs, and reseeds, each time a script issues request 27 of instruction
`0x22`; 44 shipped instructions do, in 21 `GPL` resources (FND-RNG-027). No shipped script uses
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
The roll total is a returned word; its interpretation by callers remains
part of Q-RNG-013, rather than an unbounded default integer.

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

- The environment strings that reach the clock conversion's initializer (FND-RNG-016..018,
  Q-RNG-004), and the date and time values each caller passes (FND-RNG-009, FND-RNG-014..015,
  Q-RNG-012). FND-RNG-023 settles local
  date-before-time sampling without a coherence retry; external service
  domain and rollover guarantees remain Q-RNG-006, with conditional pinned
  handler-source leads in SRC-RUSTDOS-172. Global/table storage and
  reader/writer identity remain separate (FND-RNG-019..020, Q-RNG-005). Near-buffer identity
  and admitted reseeding paths beyond the recovered direct call sequences
  remain separate (Q-RNG-003). The second root's local reseeding and
  conditional draw loop, and its script wrapper widths, are recorded in
  FND-RNG-021..022, and the shipped scripts that issue it in FND-RNG-027.
- The rule entry for overlay 204 +00E1's draw (FND-RNG-021, Q-RNG-016), the rule and draw order of the 20 `roll_sum` sites (Q-RNG-013) that FND-RNG-028..035 leave undescribed, the rule entries for overlay 179's effect dice and FND-RNG-035's routines (FND-RNG-034, FND-RNG-035, Q-RNG-017), the events that reach overlay 206's
  dissolve and cry draws (FND-RNG-030, Q-RNG-014), and the rules of overlay 173's draws
  (FND-RNG-031, Q-RNG-015). Recovered `random_mod`
  uses include `chance_in_ten`, the table selection in FND-RNG-007 and the music selector of
  RULE-SOUND-003 (FND-SOUND-012).
  FND-PARTY-099 recovers an effect-saving caller of `roll_sum(1, 20)`; the earlier statement
  that `roll_sum` had no recovered caller was stale.
- Calls of the generator, a helper or the seed setter through a stored far pointer, a computed
  call or a segment alias (FND-RNG-001, FND-RNG-002, Q-RNG-008).
- The inputs and rules of the eleven `random_between` sites: FND-RNG-025 reads five in overlay
  176's psionic action and resistance checks, including conditional and equal-bound draw paths,
  and FND-RNG-026 six in overlay 179's coordinate samplers (Q-RNG-009).
- Whether computed, aliased or bulk accesses read or write `rng_state`
  beyond the known generator/setter (FND-RNG-001, Q-RNG-007).
