# Parity matrix

States are `unknown`, `researched`, `implemented`, `partially validated`,
`validated`, `intentional deviation`, and `not applicable`.

| Feature | Evidence | Core/rules | Presentation/input | Audio/video | Persistence | Automated validation | Manual validation | Status |
|---|---|---|---|---|---|---|---|---|
| GOG source recognition | exact local hashes | not applicable | Extractor diagnostics | not applicable | source manifest | synthetic mismatch tests | owned build verifies | partially validated |
| Extracted asset pack | approved contract, `DATA-GOG-TITLE-001`, `DATA-GOG-UI-001/002/003`, `DATA-GOG-FONT-001`, `DATA-GOG-TEXT-001` | not applicable | missing-pack startup guidance | title, fourteen controls, shared window image, font, and text catalog | versioned manifest v6 | exact inventory/hash and transactional tests | owned 18-asset pack verifies; remaining Slice 2 inventory unknown | partially validated |
| Start and party flow | `MANUAL-1994`, `CONFLICT-PARTY-001`, `DATA-GOG-UI-001` | party bounds, sex/alignment/ability/class invariants and deterministic semantic menu transitions implemented; conflicting eligibility unresolved | start-window/button resources and logical coordinates mapped; runtime input/navigation not implemented | unknown | none | 25 focused Core tests plus bounded UI-resource tests | shipped state transitions not yet observed | partially implemented |
| Indexed graphics resources | `DATA-GOG-IMAGE-001`, `DSUN-MUSIC` | not applicable | bounded indexed-image and palette readers; title and start-window mappings/rendering implemented | not applicable | not applicable | synthetic decoding and malformed-input tests | all owned matching resources decode | partially validated |
| Indexed bitmap fonts | `DATA-GOG-FONT-001` | not applicable | bounded 256-glyph FONT reader and DSFT pack asset; text rendering not implemented | not applicable | derived DSFT v1 | synthetic decoding, round-trip, extraction, and malformed-input tests | owned `FONT` #100 decodes, extracts, and balances exactly | partially validated |
| Text resources | `DATA-GOG-TEXT-001` | not applicable | bounded ASCII/CRLF TEXT reader and deterministic DSTX catalog; resource routing and rendering not implemented | not applicable | derived DSTX v1 | synthetic line, round-trip, deterministic-order, extraction, and malformed-input tests | all 62 owned records extract | partially validated |
| Static title/start window | `DATA-GOG-TITLE-001`, `DATA-GOG-UI-001` | not applicable | 320x200 title and four first-frame button images rendered with point sampling at recorded coordinates | preceding/following timing unknown | five verified DSIX assets within the 18-asset pack | synthetic mapping/pack tests and content smoke | owned resources/palette inspected; runtime visual comparison and frame states pending | partially validated |
| Character-generation controls | `DATA-GOG-UI-002` | class choices represented in existing Core rules | ten image-backed controls mapped and extracted; shell/dynamic fields not rendered | unknown | ten DSIX assets | mapping, geometry, frame, extraction, and content-smoke tests | local decoded labels verified; interaction comparison pending | partially implemented |
| Tyr exploration/dialogue | manual + FAQ route | not implemented | not implemented | unknown | none | planned | not started | researched |
| Opening combat | manual + FAQ route | not implemented | not implemented | unknown | none | planned | not started | researched |
| Character rules | manual + FAQ conflicts | not implemented | not implemented | unknown | none | planned | not started | researched |
| Campaign/quests | FAQ route index | not implemented | not implemented | unknown | none | planned | not started | researched |
| Saves/replays | manual intent | start-flow commands/events, explicit seed, versioned snapshots, and verified replay | not implemented | not applicable | in-memory start-flow replay only | deterministic hash, restore, rejection, and divergence tests | native format and whole-game coverage not started | partially implemented |
| Full audiovisual parity | inventory only | not applicable | not implemented | not implemented | not applicable | planned | not started | unknown |
| Packaging | configured scripts | not applicable | project identity configured | not applicable | app identity configured | build/smoke planned | not started | researched |

## Blockers

- The extracted startup pack and renderer do not yet include party-screen text or interaction.
- The exact underlying DOS revision in the GOG build is unknown.
- Native resolution, palettes, timing, audio mapping, and screen geometry are
  unobserved.
- No gameplay rule has yet passed original-reference validation.

No broad parity claim is permitted while these rows remain unvalidated.
