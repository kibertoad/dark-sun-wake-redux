# AI

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `RULE-AI-001` | The computer-control button toggles computer control of a party member unless it is locked, and Space turns it off for every unlocked member | supported | partial | None | None | supported | `CombatHotkeys` maps Space to a `DisableComputerControl` request, which carries `PLACEHOLDER: RULE-AI-001`. Nothing resolves the request, the rebuild keeps no computer-control setting, and the small buttons beside the character boxes do nothing. |
| `RULE-AI-002` | What a computer-controlled combatant does in its turn | unknown | missing | None | None | unknown | The rebuild has no combat turns and no decisions for hostile creatures or party members. |
