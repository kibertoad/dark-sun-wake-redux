---
id: RULE-RNG-001
title: The game's random number generator and its reductions
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-001, FND-RNG-002, FND-RNG-011, FND-RNG-012, FND-RNG-013, FND-RNG-006, FND-SCRIPT-011, FND-RNG-009]
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
Additional overlay caller candidates still require verification and rule
mapping (Q-RNG-001); the resident list is not a complete caller census.

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
part of Q-RNG-001, rather than an unbounded default integer.

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

- The complete seed-source conversion, environmental inputs, near-buffer
  identity and admitted reseeding paths beyond the recovered direct call
  sequences (FND-RNG-009, Q-RNG-003).
- Which game rules call the reducers. `random_between` and `roll_sum` have no recovered callers,
  and `random_mod` is called only from `chance_in_ten` and the table selection in FND-RNG-007,
  which is the music selector of RULE-SOUND-003 (FND-SOUND-012, Q-RNG-001).
- Whether indirect code reads or writes `rng_state` (FND-RNG-001, Q-RNG-001).
