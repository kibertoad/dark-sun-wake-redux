# RNG

Next ID: Q-RNG-005

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
- Q-RNG-003. RULE-RNG-001: What complete input-to-seed contract and admitted
  reseeding paths do the two recovered seed callers use? Settles it: complete
  reading of the admitted caller paths and near-buffer DS/SS
  identity on other admitted paths; callers reaching overlay
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
  Tried: FND-RNG-014 verifies the zero-output startup call's near-buffer
  identity, pushed offsets and unchanged data through conversion, whose
  actual return supplies the seed argument's low word. That startup identity
  obligation is answered; complete conversion, other input/path identities
  and reseeding coverage remain. Tried: FND-RNG-015 separates the wrapper's
  full-width return and optional-copy contract from conversion dependencies.
  Complete conversion and service/environment obligations move to Q-RNG-004;
  this item retains caller reachability, aliases and seed-write coverage. Blocks: reproducible
  startup seeding and Q-PARTY-081's later recorded gameplay run.
- Q-RNG-004. RULE-RNG-001: What complete date/time-to-value conversion does
  the clock source use? Settles it: all branches and operand widths of
  1000:386D, its month tables, environment initializer 1000:3B56 and daylight
  helper 1000:3CD7; globals' initialization and writers, service input domain
  and INT 21 effects, including admission of every indexed date/time value.
  Tried: FND-RNG-009 identifies the conversion chain, FND-RNG-014 measures
  one startup input and return, and FND-RNG-015 records direct buffer access
  widths and the separate wrapper contract. These do not settle the
  environment branches, tables or full admitted domain. Split from
  Q-RNG-003 so conversion and reseeding reachability can be settled separately.
  Blocks: complete reproducible seed-source semantics.

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
