# Fidelity ledger

Status values: `unknown`, `documented`, `observed`, `implemented`, and
`validated`. Implementation without comparison is not validation.

| Area | Status | Evidence | Current statement |
|---|---|---|---|
| Legal-source recognition | implemented | `GOG-1432903719`, synthetic tests | One English GOG build has six exact fingerprint anchors |
| Asset-pack contract | implemented | synthetic tests | Version, game/source identity, exact inventory, hashes, provenance, media type, conversion, and unexpected files are checked |
| Asset extraction | implemented | `DATA-GOG-TITLE-001`, `DATA-GOG-UI-001`-`004`, `DATA-GOG-FONT-001`, `DATA-GOG-TEXT-001`, tests, owned build | Title, twenty-four controls, and window image become DSIX, font DSFT, TEXT DSTX, and the six-window resolved graph DSUI; all 29 assets are transactionally verified |
| GFF container directories | implemented | `DATA-GOG-GFF-001`, `DSUN-MUSIC`, synthetic tests | Bounded metadata parsing succeeds for all 26 GFF files in the owned build; payload semantics are not implied |
| Indexed images and palettes | implemented | `DATA-GOG-IMAGE-001`, `DSUN-MUSIC`, synthetic tests | All 4,510 matching images (9,279 frames) and 40 palettes decode within bounds; semantic mapping and visual comparison remain open |
| Indexed bitmap fonts | implemented | `DATA-GOG-FONT-001`, synthetic tests | `FONT` #100 decodes as 256 bounded indexed glyphs, has a verified 256-entry identity map, round-trips through DSFT, and supports encoding-neutral run and multiline-block rasterization with explicit spacing; generalized map semantics, authentic spacing/alignment, palette, and runtime presentation remain open |
| Text resources | implemented | `DATA-GOG-TEXT-001`, synthetic tests | All 62 RESOURCE.GFF TEXT records decode and round-trip in deterministic DSTX with IDs preserved; ID meanings and UI routing remain open |
| UI resource layouts | implemented | `DATA-GOG-UI-001`-`005`, `EXE-GOG-UI-001`-`002`, synthetic tests, owned build | All UI records parse and resolve; the six start-flow windows and 39 referenced controls extract as bounded DSUI, and a shared runtime resolver preserves ordered typed controls, geometry, image references, and event masks; the original generic WIND path does not consume the image field, while its app-specific role, event-bit meanings, shell draw, and runtime state behavior remain unobserved |
| Rules | implemented | `MANUAL-1994`, `FAQ-81038`, `DATA-GOG-UI-001`, Core tests | Initial party-creation invariants, psionic disciplines, clerical spheres, occupied-slot edit/drop/add, semantic start/party routing, and immutable human dual-class progression boundaries are implemented; DUAL integration, disputed eligibility, and later gameplay rules remain unresolved |
| AI | unknown | none | Not researched |
| Controls | implemented | `MANUAL-1994`, `DATA-GOG-UI-001`, synthetic tests, owned content smoke | Start-window mouse clicks use DSUI-derived BUTN rectangles and resource-ID semantics under scale-independent hit testing; original edge behavior, focus, keyboard access, and later controls remain unobserved |
| Persistence | implemented | Core tests | Start-flow snapshot schema 3 covers discipline/sphere choices, active edit target, and dropped-character storage; hash-verified replay is implemented, while native files, migration, and whole-game coverage remain open |
| Layout/graphics | implemented | `DATA-GOG-IMAGE-001`, `DATA-GOG-TITLE-001`, `DATA-GOG-UI-001`, owned content smoke | The evidenced 320x200 title and first frames of all four start controls are displayed through one nearest-neighbor transform at DSUI WIND child coordinates; frame-state semantics, pixel aspect, and timing remain unvalidated |
| Animation timing | unknown | none | FLI and gameplay cadence are unverified |
| Text | researched | `DATA-GOG-FONT-001`, `DATA-GOG-TEXT-001` | Glyph and printable-ASCII short-text envelopes plus indexed run/block composition are implemented; the owned font map is identity, while palette, authentic glyph/line spacing, alignment, resource meanings, and route mapping are unverified |
| Sound/speech | unknown | file inventory only | VOC files observed; mappings/codecs/timing unverified |
| Music | unknown | file inventory only | GOG Ogg files observed; mapping and original playback behavior unverified |
| Video | unknown | file inventory only | Four FLI files observed; format variant and playback unverified |
| Error behavior | implemented | synthetic tests and smoke path | Missing/invalid pack and source mismatches return actionable diagnostics |
| Packaging | documented | configured scripts | Identity is specialized; packages are not release-ready while decoders are absent |
