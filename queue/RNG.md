# RNG

Next ID: Q-RNG-002

## Static

- Q-RNG-001. (entries pending migration): Where is the random number generator
  seeded, and which rules call its modulo, inclusive-range and repeated-roll
  helpers, in what order? Settles it: the callers of the seed setter and of
  each helper, including calls through relocations and far pointers. Tried: a
  complete instruction-text scan for the two state displacements and direct
  far calls to the setter, and the one direct consumer chain of the modulo
  helper, which identify no seed source or rule-level consumer. Blocks: slices
  2-7.

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
