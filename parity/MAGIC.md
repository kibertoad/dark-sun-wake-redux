# MAGIC

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `RULE-MAGIC-001` | How many spells of each level a caster can cast before resting | sourced | missing | None | None | sourced | No code counts spells. `ExplorationSession` opens the Cast Spells / Use PSI view with no spells in it. |
| `RULE-MAGIC-002` | Which spheres of cleric spells a priest may cast | sourced | missing | None | None | sourced | No code holds spells or their spheres. A created character carries a `ClericalSphere` (RULE-PARTY-003). |
| `RULE-MAGIC-003` | Activating and maintaining a psionic power | sourced | missing | None | None | sourced | No code holds psionic powers or PSPs. |
| `RULE-MAGIC-004` | Camping restores spells and psionic strength points | sourced | missing | None | None | sourced | No code camps or rests. |
| `RULE-MAGIC-005` | A DATA record's dice are a count growing with the level, of sides the record gives, plus a bonus per level step | supported | missing | None | None | supported | No code rolls a `DATA` record's dice. |
| `RULE-MAGIC-006` | DATA 306 destroys a target of details kind 9 on a d20 at least three times its adjusted level byte less the adjusted caster level, plus 10 | supported | missing | None | None | supported | No code applies `DATA` 306. |
