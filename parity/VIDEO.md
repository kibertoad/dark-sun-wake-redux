# VIDEO

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `FMT-VIDEO-001` | FLI animation | supported | missing | None | None | supported | The source manifest lists the five FLI files as game data; nothing parses them. |
| `RULE-VIDEO-001` | A cinematic plays its FLI from the installation, copying it from the disc first when it can, and falls back to still pictures | supported | missing | None | None | supported | The rebuild plays no cinematic. |
| `RULE-VIDEO-002` | The FLI player shows the first record after a wait and then one record every given number of milliseconds, until the header's frame count or a Shift key | supported | missing | None | None | supported | The rebuild has no FLI player. |
| `RULE-VIDEO-003` | When an FLI cannot play, the cinematic's still pictures are shown for up to 8 seconds each | supported | missing | None | None | supported | The rebuild shows no cinematic pictures. |
| `RULE-VIDEO-004` | While an FLI plays, a timer slot with a period of 1,000 microseconds counts milliseconds | supported | missing | None | None | supported | The rebuild has no FLI player. |
