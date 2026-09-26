# RNG

Next ID: Q-RNG-003

## Static

- Q-RNG-001. RULE-RNG-001: Where is the random number generator seeded, and
  which rules call its modulo, inclusive-range and repeated-roll helpers, in
  what order? Settles it: the callers of the seed setter and of each helper,
  including calls through relocations and far pointers. Tried: a complete
  instruction-text scan for the two state displacements and direct far calls
  to the setter (FND-RNG-001, FND-RNG-002), and the one direct consumer chain
  of the modulo helper (FND-RNG-007, FND-RNG-008), which identify no seed
  source or rule-level consumer; a far-call search found a fourth direct
  caller of the generator, the script instruction `0x52` (FND-SCRIPT-011),
  which draws without a helper. Blocks: slices 2-7.
- Q-RNG-002. RULE-RNG-001: In what widths and signedness do the reducers
  compute? Settles it: the multiply, divide and store instructions of
  `random_mod`, `random_between` and `roll_sum`, read for operand width and
  signed or unsigned forms. Blocks: none.

## Emulated call

None.

## Agent run

None.

## Live session

None.

## Source

None.

## Blocked

None.
