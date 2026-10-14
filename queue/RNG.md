# RNG

Next ID: Q-RNG-004

## Static

- Q-RNG-001. RULE-RNG-001: Which rules call the generator and its modulo,
  inclusive-range and repeated-roll helpers, in what order, and can indirect
  code read or write the state? Settles it: verified callers of the generator
  and each helper, including relocations, far pointers and indirect state
  access; map each to a rule and its draw order. Tried: a complete
  instruction-text scan for the two state displacements and direct far calls
  to the setter (FND-RNG-001, FND-RNG-002), and the one direct consumer chain
  of the modulo helper (FND-RNG-007, FND-RNG-008), which identify no seed
  source, and whose consumer is the music selector of RULE-SOUND-003
  (FND-SOUND-012); a far-call search found a fourth direct
  caller of the generator, the script instruction `0x52` (FND-SCRIPT-011),
  which draws without a helper. Tried: FND-RNG-009 recovers two direct overlay
  seed callers; their complete source/reseeding contract is separated into
  Q-RNG-003. Its generator control query yields further overlay candidates
  requiring instruction and rule verification. Blocks: slices 2-7.
- Q-RNG-002. RULE-RNG-001: In what widths and signedness do the reducers
  compute? Settles it: the multiply, divide and store instructions of
  `random_mod`, `random_between` and `roll_sum`, read for operand width and
  signed or unsigned forms. Blocks: none.
- Q-RNG-003. RULE-RNG-001: What complete input-to-seed contract and admitted
  reseeding paths do the two recovered seed callers use? Settles it: complete
  reading of 1000:0963 and conversion 1000:386D, their near-buffer DS/SS
  identity, globals and INT 21 service inputs/effects; callers reaching overlay
  180 +065E and overlay 204 +001E; other indirect or aliased seed writes and
  calls. Tried: FND-RNG-009 proves both direct call sequences use only AX from
  the helper, whose optional output pointer is zero, and separates the complete
  conversion/reachability obligations from Q-RNG-001. FND-UI-039 observes a
  later state change without identifying its writer. Tried: FND-RNG-010
  observes the admitted startup caller, its argument and setter result, then
  saves and file-verifies the opener checkpoint with no known generator draw.
  Complete conversion, earlier near-buffer identity and other reseeding paths
  remain. FND-UI-040 subsequently verifies native restore of the opener
  checkpoint; the clock conversion and other reseeding obligations remain.
  Blocks: reproducible
  startup seeding and Q-PARTY-081's later recorded gameplay run.

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
