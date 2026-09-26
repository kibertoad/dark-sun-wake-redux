# RNG

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `RULE-RNG-001` | The game's random number generator and its reductions | supported | partial | None | None | supported | `NativeRandom` implements the generator, `random_mod`, `random_between` and `roll_sum`, with golden vectors worked out from the spec. It lacks `chance_in_ten`, and no game system draws from it or seeds it, because the original's seed and callers are not known. |
