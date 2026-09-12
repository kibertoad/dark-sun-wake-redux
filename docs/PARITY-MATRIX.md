# Parity matrix

States are `unknown`, `researched`, `implemented`, `partially validated`,
`validated`, `intentional deviation`, and `not applicable`.

| Feature | Evidence | Core/rules | Presentation/input | Audio/video | Persistence | Automated validation | Manual validation | Status |
|---|---|---|---|---|---|---|---|---|
| GOG source recognition | exact local hashes | not applicable | Extractor diagnostics | not applicable | source manifest | synthetic mismatch tests | owned build verifies | partially validated |
| Extracted asset pack | approved contract, `DATA-GOG-TITLE-001` | not applicable | missing-pack startup guidance | title asset only | versioned manifest | exact inventory/hash and transactional tests | owned title pack verifies; remaining Slice 2 inventory unknown | partially validated |
| Start and party flow | `MANUAL-1994`, `CONFLICT-PARTY-001` | party bounds, sex/alignment/ability/class invariants implemented; conflicting eligibility unresolved | not implemented | unknown | none | 13 focused Core tests | shipped menus not yet observed | partially implemented |
| Indexed graphics resources | `DATA-GOG-IMAGE-001`, `DSUN-MUSIC` | not applicable | bounded indexed-image and palette readers; only the title mapping/rendering is implemented | not applicable | not applicable | synthetic decoding and malformed-input tests | all owned matching resources decode | partially validated |
| Static title image | `DATA-GOG-TITLE-001` | not applicable | 320x200 title asset extracted and rendered with point sampling | preceding/following timing unknown | verified DSIX asset | synthetic mapping/pack tests and content smoke | owned resource/palette inspected; runtime visual comparison pending | partially validated |
| Tyr exploration/dialogue | manual + FAQ route | not implemented | not implemented | unknown | none | planned | not started | researched |
| Opening combat | manual + FAQ route | not implemented | not implemented | unknown | none | planned | not started | researched |
| Character rules | manual + FAQ conflicts | not implemented | not implemented | unknown | none | planned | not started | researched |
| Campaign/quests | FAQ route index | not implemented | not implemented | unknown | none | planned | not started | researched |
| Saves/replays | manual intent | deterministic seed scaffold only | not implemented | not applicable | not implemented | scaffold test | not started | unknown |
| Full audiovisual parity | inventory only | not applicable | not implemented | not implemented | not applicable | planned | not started | unknown |
| Packaging | configured scripts | not applicable | project identity configured | not applicable | app identity configured | build/smoke planned | not started | researched |

## Blockers

- No extracted asset pack or player-visible renderer exists.
- The exact underlying DOS revision in the GOG build is unknown.
- Native resolution, palettes, timing, audio mapping, and screen geometry are
  unobserved.
- No gameplay rule has yet passed original-reference validation.

No broad parity claim is permitted while these rows remain unvalidated.
