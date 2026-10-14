# RNG

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `RULE-RNG-001` | The game's random number generator and its reductions | supported | partial | None | None | supported | Defect (FND-RNG-009): recovered direct seed callers are not represented; complete source/reseeding inputs remain Q-RNG-003 and rule/draw callers remain Q-RNG-001. `NativeRandom` implements the generator, `random_mod`, `random_between` and `roll_sum`, with golden vectors worked out from the spec. It lacks `chance_in_ten`; no game system draws from it or seeds it. |
