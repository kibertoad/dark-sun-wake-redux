# Fidelity ledger

Status values: `unknown`, `documented`, `observed`, `implemented`, and
`validated`. Implementation without comparison is not validation.

| Area | Status | Evidence | Current statement |
|---|---|---|---|
| Legal-source recognition | implemented | `GOG-1432903719`, synthetic tests | One English GOG build has six exact fingerprint anchors |
| Asset-pack contract | implemented | synthetic tests | Version, game/source identity, exact inventory, hashes, provenance, media type, conversion, and unexpected files are checked |
| Asset extraction | unknown | plan only | No Dark Sun decoder or output pack exists yet |
| GFF container directories | implemented | `DATA-GOG-GFF-001`, `DSUN-MUSIC`, synthetic tests | Bounded metadata parsing succeeds for all 26 GFF files in the owned build; payload semantics are not implied |
| Rules | documented | `MANUAL-1994`, `FAQ-81038` | Initial input/party/combat intent recorded; no gameplay implemented |
| AI | unknown | none | Not researched |
| Controls | documented | `MANUAL-1994` | Semantic actions known; coordinates and runtime behavior unobserved |
| Persistence | unknown | none | Native saves/replays and legacy import are not implemented |
| Layout/graphics | unknown | none | Logical dimensions, palette, mapping, and rendering are unobserved |
| Animation timing | unknown | none | FLI and gameplay cadence are unverified |
| Text | unknown | none | Storage, encoding, layout, and route mapping are unverified |
| Sound/speech | unknown | file inventory only | VOC files observed; mappings/codecs/timing unverified |
| Music | unknown | file inventory only | GOG Ogg files observed; mapping and original playback behavior unverified |
| Video | unknown | file inventory only | Four FLI files observed; format variant and playback unverified |
| Error behavior | implemented | synthetic tests and smoke path | Missing/invalid pack and source mismatches return actionable diagnostics |
| Packaging | documented | configured scripts | Identity is specialized; packages are not release-ready while decoders are absent |
