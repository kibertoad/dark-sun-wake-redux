# Fidelity ledger

Status values: `unknown`, `documented`, `observed`, `implemented`, and
`validated`. Implementation without comparison is not validation.

| Area | Status | Evidence | Current statement |
|---|---|---|---|
| Legal-source recognition | implemented | `GOG-1432903719`, synthetic tests | One English GOG build has six exact fingerprint anchors |
| Asset-pack contract | implemented | synthetic tests | Version, game/source identity, exact inventory, hashes, provenance, media type, conversion, and unexpected files are checked |
| Asset extraction | implemented | `DATA-GOG-TITLE-001`, synthetic extraction test, owned-build verification | The title image is converted to DSIX, staged, fully pack-verified, and promoted transactionally; remaining Slice 2 assets are not mapped |
| GFF container directories | implemented | `DATA-GOG-GFF-001`, `DSUN-MUSIC`, synthetic tests | Bounded metadata parsing succeeds for all 26 GFF files in the owned build; payload semantics are not implied |
| Indexed images and palettes | implemented | `DATA-GOG-IMAGE-001`, `DSUN-MUSIC`, synthetic tests | All 4,510 matching images (9,279 frames) and 40 palettes decode within bounds; semantic mapping and visual comparison remain open |
| UI resource layouts | implemented | `DATA-GOG-UI-001`, synthetic tests | All 28 WIND and 139 BUTN resources in RESOURCE.GFF parse and resolve; the start-menu layout is mapped, while runtime state/frame behavior remains unobserved |
| Rules | implemented | `MANUAL-1994`, `FAQ-81038`, Core tests | Initial party-creation invariants are implemented; disputed eligibility and all later gameplay rules remain unresolved |
| AI | unknown | none | Not researched |
| Controls | documented | `MANUAL-1994` | Semantic actions known; coordinates and runtime behavior unobserved |
| Persistence | unknown | none | Native saves/replays and legacy import are not implemented |
| Layout/graphics | implemented | `DATA-GOG-IMAGE-001`, `DATA-GOG-TITLE-001`, `DATA-GOG-UI-001` | The evidenced 320x200 title resource/palette is displayed with nearest-neighbor scaling; start-menu resources and coordinates are mapped but not yet rendered; pixel aspect and timing remain unvalidated |
| Animation timing | unknown | none | FLI and gameplay cadence are unverified |
| Text | unknown | none | Storage, encoding, layout, and route mapping are unverified |
| Sound/speech | unknown | file inventory only | VOC files observed; mappings/codecs/timing unverified |
| Music | unknown | file inventory only | GOG Ogg files observed; mapping and original playback behavior unverified |
| Video | unknown | file inventory only | Four FLI files observed; format variant and playback unverified |
| Error behavior | implemented | synthetic tests and smoke path | Missing/invalid pack and source mismatches return actionable diagnostics |
| Packaging | documented | configured scripts | Identity is specialized; packages are not release-ready while decoders are absent |
