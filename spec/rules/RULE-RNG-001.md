---
id: RULE-RNG-001
title: The game's random number generator and its reductions
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-001, FND-RNG-002, FND-RNG-003, FND-RNG-004, FND-RNG-005, FND-RNG-006, FND-SCRIPT-011]
conflicting: []
split_with: []
related: []
---

## Summary

The game has one generator, `rng`, a linear congruential generator whose 32-bit state is
`rng_state`. A draw gives 0 to 32767. Four helpers reduce draws to game values: `random_mod(n)`
gives 0 to `n - 1`, `random_between(lower, upper)` gives `lower` to `upper`, `roll_sum(count,
sides)` adds `count` rolls of 1 to `sides`, and `chance_in_ten(n)` succeeds with a chance of
`n + 1` in ten.

## When it runs

Wherever a rule draws. The generator's direct static callers are `random_mod`,
`random_between`, `roll_sum` and the script instruction of RULE-SCRIPT-006 (FND-SCRIPT-011), and
`chance_in_ten` draws through `random_mod`. When
`seed_random` runs, and with what value, is not known.

## Parameters

`seed_random(value)`, `random_mod(n)`, `random_between(lower, upper)`, `roll_sum(count, sides)`
and `chance_in_ten(n)`.

## Inputs

`rng_state`.

## Procedure

```text
# draw() is the generator: it sets
#     rng_state = UINT32(rng_state * 0x015A4E35 + 1)
# and gives (rng_state >> 16) & 0x7FFF, a value from 0 to 32767.

define seed_random(value: UINT16):
    rng_state = value

define random_mod(n: UINT16) -> UINT16:
    if n == 0:
        return 0
    return draw() % n

define random_between(lower: INT16, upper: INT16) -> INT16:
    if lower >= upper:
        return lower
    return lower + INT16((INT32(draw()) * (upper - lower + 1)) / 32768)

define roll_sum(count: INT16, sides: INT16):
    let total = 0
    let left = count
    while left > 0:
        total = total + INT16((INT32(draw()) * sides) / 32768) + 1
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

`random_mod` and `random_between` return the values above and `roll_sum` returns the total as the
default integer. `chance_in_ten` returns true or false. Each helper makes the draws its procedure
shows and changes no state other than `rng_state`.

## Edge cases

`random_mod(0)`, `random_between` with `lower` at or above `upper`, `roll_sum` with a `count` of 0
or less, and `chance_in_ten` with an argument outside 1 to 9 make no draw. `chance_in_ten(n)` for
`n` from 1 to 9 succeeds when the draw modulo 10 is at most `n`, which is `n + 1` values out of
ten, so `chance_in_ten(9)` always succeeds as well, at the cost of a draw. `random_mod` keeps the
bias of a plain remainder: for a divisor that does not divide 32768, the low results come up more
often. `seed_random` leaves the high word of the state 0.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- When the game seeds the generator, and from what. The seed setter has no recovered caller
  (FND-RNG-002, Q-RNG-001).
- Which game rules call the reducers. `random_between` and `roll_sum` have no recovered callers,
  and `random_mod` is called only from `chance_in_ten` and the table selection in FND-RNG-007,
  whose mechanic is not known (Q-RNG-001).
- The widths the original computes in: whether the products in `random_between` and `roll_sum`
  are 32-bit as written, whether `roll_sum` keeps its total in 16 bits, whether the divisions are
  signed, and whether `random_mod` divides signed, which matters only for a divisor above 32767
  (Q-RNG-002).
- What `random_between` does when `upper - lower + 1` passes 32767 (Q-RNG-002).
- Whether indirect code reads or writes `rng_state` (FND-RNG-001, Q-RNG-001).
