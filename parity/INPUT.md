# INPUT

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `RULE-INPUT-001` | The right mouse button steps the pointer through the Walk, Attack and Look modes | sourced | complete | None | None | implemented | `ExplorationSession` steps `ExplorationCursorMode` from Walk to Attack to Look and back on `CycleCursorMode`. `ExplorationRightMouseInput` sends that command when a right-button press is released without dragging, which is the modern drag gesture COMPAT-INPUT-001 in `docs/RULES-AND-EVIDENCE.md` records. |
| `RULE-INPUT-002` | Which image the pointer shows for each mode, and its hotspot | supported | partial | None | None | supported | `ExplorationCursorFeedback` picks the Walk, hand-to-hand and Look pairs and draws the image at its top-left corner, but never the ranged pair, so it carries `PLACEHOLDER: RULE-INPUT-002`. `OriginalContent.ExplorationCursorAssets` names all ten images. |
| `RULE-INPUT-003` | The keys that open the character option screens and the Game Menu | sourced | complete | None | None | implemented | `ExplorationHotkeys` maps `C`, `U`, `E`, `I`, `V` and `Tab` to the screens on a key's rising edge, and also `O`, `5`, `6` and `Escape`. |
