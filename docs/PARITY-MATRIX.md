# Parity matrix

States are `unknown`, `researched`, `implemented`, `partially validated`,
`validated`, `intentional deviation`, and `not applicable`.

| Feature | Evidence | Core/rules | Presentation/input | Audio/video | Persistence | Automated validation | Manual validation | Status |
|---|---|---|---|---|---|---|---|---|
| GOG source recognition | exact local hashes | not applicable | Extractor diagnostics | not applicable | source manifest | synthetic mismatch tests | owned build verifies | partially validated |
| Extracted asset pack | approved contract, `DATA-GOG-TITLE-001`, `DATA-GOG-UI-001`, `DATA-GOG-FONT-001` | not applicable | missing-pack startup guidance | title, four start-button assets, and interface font | versioned manifest v3 | exact inventory/hash and transactional tests | owned six-asset startup pack verifies; remaining Slice 2 inventory unknown | partially validated |
| Start and party flow | `MANUAL-1994`, `CONFLICT-PARTY-001`, `DATA-GOG-UI-001` | party bounds, sex/alignment/ability/class invariants and deterministic semantic menu transitions implemented; conflicting eligibility unresolved | start-window/button resources and logical coordinates mapped; runtime input/navigation not implemented | unknown | none | 25 focused Core tests plus bounded UI-resource tests | shipped state transitions not yet observed | partially implemented |
| Indexed graphics resources | `DATA-GOG-IMAGE-001`, `DSUN-MUSIC` | not applicable | bounded indexed-image and palette readers; title and start-window mappings/rendering implemented | not applicable | not applicable | synthetic decoding and malformed-input tests | all owned matching resources decode | partially validated |
| Indexed bitmap fonts | `DATA-GOG-FONT-001` | not applicable | bounded 256-glyph FONT reader and DSFT pack asset; text rendering not implemented | not applicable | derived DSFT v1 | synthetic decoding, round-trip, extraction, and malformed-input tests | owned `FONT` #100 decodes, extracts, and balances exactly | partially validated |
| Static title/start window | `DATA-GOG-TITLE-001`, `DATA-GOG-UI-001` | not applicable | 320x200 title and four first-frame button images rendered with point sampling at recorded coordinates | preceding/following timing unknown | five verified DSIX assets within the six-asset startup pack | synthetic mapping/pack tests and content smoke | owned resources/palette inspected; runtime visual comparison and frame states pending | partially validated |
| Tyr exploration/dialogue | manual + FAQ route | not implemented | not implemented | unknown | none | planned | not started | researched |
| Opening combat | manual + FAQ route | not implemented | not implemented | unknown | none | planned | not started | researched |
| Character rules | manual + FAQ conflicts | not implemented | not implemented | unknown | none | planned | not started | researched |
| Campaign/quests | FAQ route index | not implemented | not implemented | unknown | none | planned | not started | researched |
| Saves/replays | manual intent | deterministic seed scaffold only | not implemented | not applicable | not implemented | scaffold test | not started | unknown |
| Full audiovisual parity | inventory only | not applicable | not implemented | not implemented | not applicable | planned | not started | unknown |
| Packaging | configured scripts | not applicable | project identity configured | not applicable | app identity configured | build/smoke planned | not started | researched |

## Blockers

- The extracted startup pack and renderer do not yet include party-screen text or interaction.
- The exact underlying DOS revision in the GOG build is unknown.
- Native resolution, palettes, timing, audio mapping, and screen geometry are
  unobserved.
- No gameplay rule has yet passed original-reference validation.

No broad parity claim is permitted while these rows remain unvalidated.
