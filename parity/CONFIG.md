# CONFIG

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `FMT-CONFIG-001` | Sound configuration SOUND.CFG | supported | missing | None | None | supported | The start-up extractor copies `SOUND.CFG` into the asset pack as an opaque payload; nothing parses it. |
| `FMT-CONFIG-002` | Sound card list SOUND.INI | supported | missing | None | None | supported | The start-up extractor copies `SOUND.INI` into the asset pack as an opaque payload; nothing parses it. The rebuild has no sound setup program. |
| `FMT-CONFIG-003` | Saved settings in the PREF resource | supported | missing | None | None | supported | The start-up extractor copies `PREF/100` into the asset pack as an opaque payload; nothing parses it. |
| `FMT-CONFIG-004` | game.ins disc-track mapping file | supported | missing | None | None | supported | The extractor preserves the file as an opaque payload; it has no format-specific reader. |
| `FMT-CONFIG-005` | PATCH.RTP file | unknown | missing | None | None | unknown | The source manifest lists the file; no format-specific reader or verified layout exists. |
| `RULE-CONFIG-001` | The on-off settings and their keys | sourced | missing | None | None | sourced | `PreferencesInput` maps the four on-off buttons, which do nothing; the rebuild keeps no settings state. `ExplorationHotkeys` binds none of `A`, `F4`, `F5` and `F6`. |
| `RULE-CONFIG-002` | The difficulty setting | supported | missing | None | None | supported | `PreferencesInput` maps the two difficulty buttons, which do nothing. `ExecutablePreferencesText` reads the four labels. |
| `RULE-CONFIG-003` | Setting the music and sound effects volumes | disputed | missing | None | None | disputed | The manual's upper music-volume arrows conflict with the shipped hover and click paths (FND-UI-034, FND-CONFIG-010). `PreferencesInput` maps the four arrows, which do nothing. |
| `RULE-CONFIG-004` | The Preferences sound-effects volume adjustment | supported | missing | None | None | supported | The rebuild maps the arrows but does not change the volume byte or sound state. |
| `RULE-CONFIG-005` | The Preferences message-delay adjustment | supported | missing | None | None | supported | The rebuild maps the arrows but does not change the delay word or text pacing. |
