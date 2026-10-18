# RNG

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `RULE-RNG-001` | The game's random number generator and its reductions | supported | partial | None | None | supported | Defect (FND-RNG-011, FND-RNG-012, FND-RNG-013): reconcile the signed divisor, wrapped signed span and word total with the corrected arithmetic contract. Defect (FND-RNG-009): recovered direct seed callers are not represented; reseeding paths remain Q-RNG-003 and Q-RNG-011, seed inputs Q-RNG-004 and Q-RNG-012, and rule/draw callers Q-RNG-001, Q-RNG-008, Q-RNG-009 and Q-RNG-010. `NativeRandom` implements the generator, `random_mod`, `random_between` and `roll_sum`, with golden vectors worked out from the earlier spec. It lacks `chance_in_ten`; no game system draws from it or seeds it. |
