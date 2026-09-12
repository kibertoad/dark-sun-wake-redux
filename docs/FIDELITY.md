# Fidelity ledger

Status values: `unknown`, `documented`, `observed`, `implemented`, and
`validated`. Implementation without comparison is not validation.

| Area | Status | Evidence | Current statement |
|---|---|---|---|
| Legal-source recognition | implemented | `GOG-1432903719`, synthetic tests | One English GOG build has six exact fingerprint anchors |
| Asset-pack contract | implemented | synthetic tests | Version, game/source identity, exact inventory, hashes, provenance, media type, conversion, and unexpected files are checked |
| Asset extraction | implemented | `DATA-GOG-TITLE-001`, `DATA-GOG-UI-001`, `DATA-GOG-FONT-001`, synthetic extraction test, owned-build verification | The title and four start-window icon sets are converted to DSIX and the interface font to DSFT, staged, fully pack-verified, and promoted transactionally; remaining Slice 2 assets are not mapped |
| GFF container directories | implemented | `DATA-GOG-GFF-001`, `DSUN-MUSIC`, synthetic tests | Bounded metadata parsing succeeds for all 26 GFF files in the owned build; payload semantics are not implied |
| Indexed images and palettes | implemented | `DATA-GOG-IMAGE-001`, `DSUN-MUSIC`, synthetic tests | All 4,510 matching images (9,279 frames) and 40 palettes decode within bounds; semantic mapping and visual comparison remain open |
| Indexed bitmap fonts | implemented | `DATA-GOG-FONT-001`, synthetic tests | `FONT` #100 decodes as 256 bounded indexed glyphs and round-trips through derived DSFT; encoding, palette, spacing, strings, and presentation remain open |
| Text resources | implemented | `DATA-GOG-TEXT-001`, synthetic tests | All 62 RESOURCE.GFF TEXT records decode as bounded ASCII/CRLF lines; ID meanings and UI routing remain open |
| UI resource layouts | implemented | `DATA-GOG-UI-001`, synthetic tests | All 28 WIND, 139 BUTN, 97 APFM, and 7 EBOX resources in RESOURCE.GFF parse and resolve; start and character-generation shells are mapped, while runtime state/frame behavior remains unobserved |
| Rules | implemented | `MANUAL-1994`, `FAQ-81038`, `DATA-GOG-UI-001`, Core tests | Initial party-creation invariants and semantic start/party routing are implemented; disputed eligibility and all later gameplay rules remain unresolved |
| AI | unknown | none | Not researched |
| Controls | documented | `MANUAL-1994` | Semantic actions known; coordinates and runtime behavior unobserved |
| Persistence | implemented | Core tests | Start-flow snapshots and hash-verified replay are implemented; native files, whole-game coverage, migration, and legacy import remain open |
| Layout/graphics | implemented | `DATA-GOG-IMAGE-001`, `DATA-GOG-TITLE-001`, `DATA-GOG-UI-001` | The evidenced 320x200 title and first frames of all four start controls are displayed with one nearest-neighbor canvas transform; frame-state semantics, pixel aspect, and timing remain unvalidated |
| Animation timing | unknown | none | FLI and gameplay cadence are unverified |
| Text | researched | `DATA-GOG-FONT-001`, `DATA-GOG-TEXT-001` | Glyph and short-text envelopes are verified; palette, spacing, resource meanings, layout, and route mapping are unverified |
| Sound/speech | unknown | file inventory only | VOC files observed; mappings/codecs/timing unverified |
| Music | unknown | file inventory only | GOG Ogg files observed; mapping and original playback behavior unverified |
| Video | unknown | file inventory only | Four FLI files observed; format variant and playback unverified |
| Error behavior | implemented | synthetic tests and smoke path | Missing/invalid pack and source mismatches return actionable diagnostics |
| Packaging | documented | configured scripts | Identity is specialized; packages are not release-ready while decoders are absent |
