# SAVE

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `FMT-SAVE-001` | Stored character identifier in a CACT resource | supported | missing | None | None | supported | The start-up extractor copies each `CACT` resource into the asset pack as an opaque payload; nothing parses it. |
| `FMT-SAVE-002` | Saved game state in a GREQ resource | supported | missing | None | None | supported | The start-up extractor copies each `GREQ` resource into the asset pack as an opaque payload; nothing parses it. |
| `RULE-SAVE-001` | The keys that save, load and quit | sourced | missing | None | None | sourced | `ExplorationHotkeys` binds none of `F1`, `F2` and `F3`. |
| `RULE-SAVE-002` | The number and file name of a saved game | supported | missing | None | None | supported | The rebuild does not read or write the original's saved games. Its own snapshot and replay format (`docs/NATIVE-SAVE-AND-REPLAY.md`) is kept in memory and not written to files. |
