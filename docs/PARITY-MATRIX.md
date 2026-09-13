# Parity matrix

States are `unknown`, `researched`, `implemented`, `partially validated`,
`validated`, `intentional deviation`, and `not applicable`.

| Feature | Evidence | Core/rules | Presentation/input | Audio/video | Persistence | Automated validation | Manual validation | Status |
|---|---|---|---|---|---|---|---|---|
| GOG source recognition | exact local hashes | not applicable | Extractor diagnostics | not applicable | source manifest | synthetic mismatch tests | owned build verifies | partially validated |
| Extracted asset pack | approved contract, `DATA-GOG-TITLE-001`, `DATA-GOG-UI-001`-`004`, `DATA-GOG-UI-006`-`007`, `DATA-GOG-FONT-001`, `DATA-GOG-TEXT-001`, `DATA-GOG-CHAR-005` | not applicable | missing-pack startup guidance | title, two start-shell layers, two party-overview layers, twenty-four controls, shared window image, font, text catalog, resolved six-window UI graph, and bounded character metadata | versioned manifest v11 | exact inventory/hash, DSUI/DSCH catalogs, and transactional tests | owned 34-asset pack extracts and verifies; remaining Slice 2 inventory unknown | partially validated |
| Start and party flow | `MANUAL-1994`, `CONFLICT-PARTY-001`, `RULE-PARTY-003`-`005`, `DATA-GOG-UI-001`, `DATA-GOG-UI-006`-`007` | party invariants, exact origin modifier facts, discipline/sphere and human dual-class progression/selection rules, occupied-slot edit/drop/add, and deterministic commands/events/snapshots/replay implemented; modifier application and conflicting eligibility remain unresolved | the measured start shell/controls and party-overview shell render through one logical transform; click routing resolves DSUI coordinates/dimensions/image and semantic resource IDs; a shared resolver materializes typed control graphs for all six extracted windows; party fields, DUAL, and other destination screens are not rendered | unknown | schema-4 in-memory snapshots and hashes include ordered class/level progression, the active DUAL/edit target, and dropped-character storage | Core origin-table, dual-class command/restore/hash boundaries, atomic edit/storage, DSUI mixed-control resolution, shell mapping, transform, rectangle-edge, and routing tests | shipped defaults/edges/transitions not yet observed | partially implemented |
| Indexed graphics resources | `DATA-GOG-IMAGE-001`, `DSUN-MUSIC` | not applicable | bounded indexed-image and palette readers; title and start-window mappings/rendering implemented | not applicable | not applicable | synthetic decoding and malformed-input tests | all owned matching resources decode | partially validated |
| Indexed bitmap fonts | `DATA-GOG-FONT-001` | not applicable | bounded 256-glyph FONT reader, DSFT pack asset, and encoding-neutral run/multiline-block rasterizers with explicit spacing; screen text rendering not implemented | not applicable | derived DSFT v1 | synthetic decoding, round-trip, extraction, run/block composition and bounds, and malformed-input tests | owned `FONT` #100 decodes, extracts, balances exactly, and has a 256-entry identity map | partially validated |
| Text resources | `DATA-GOG-TEXT-001` | not applicable | bounded ASCII/CRLF TEXT reader and deterministic DSTX catalog; resource routing and rendering not implemented | not applicable | derived DSTX v1 | synthetic line, round-trip, deterministic-order, extraction, and malformed-input tests | all 62 owned records extract | partially validated |
| Static title/start window | `DATA-GOG-TITLE-001`, `DATA-GOG-UI-001`, `DATA-GOG-UI-006` | not applicable | title extracted but runtime sequencing pending; start window renders two measured shell layers and four first-frame controls over black with point sampling and palette #1000 | title transition, animation, and following timing unknown | seven verified DSIX assets plus DSUI layout within the 34-asset pack | synthetic shell/order/palette, graph/image/rectangle tests, and owned content smoke | owned resources/palettes inspected and public frame correlated; controlled runtime comparison and frame states pending | partially validated |
| Party overview shell | `DATA-GOG-UI-007` | party aggregate and transitions implemented | the complete original base and VIEW CHARACTER title render after CREATE CHARACTERS | unknown | two verified DSIX assets within the 34-asset pack | exact mapping/order/geometry, extraction, and content smoke | owned resources correlated with public DOS capture; controlled comparison pending | partially implemented |
| Character-generation controls | `DATA-GOG-UI-002` | class choices represented in existing Core rules | ten image-backed controls mapped and extracted; shell/dynamic fields not rendered | unknown | ten DSIX assets | mapping, geometry, frame, extraction, and content-smoke tests | local decoded labels verified; interaction comparison pending | partially implemented |
| Character-generation modals | `DATA-GOG-UI-004` | effects not assigned | ten three-frame labels mapped and extracted; modals not rendered | unknown | ten DSIX assets | mapping, geometry, frame, extraction, and content-smoke tests | local decoded labels verified; transitions pending | researched |
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
