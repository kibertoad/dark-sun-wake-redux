# Development guide

## Run from source

Install the .NET 10 SDK and obtain a supported legal copy of the original game,
then verify it with the separate Asset Extractor:

```powershell
dotnet run --project src/DarkSunWakeRedux.Extractor -- verify-source --source "C:\path\to\gog-installation"
dotnet run --project src/DarkSunWakeRedux.Extractor -- extract --source "C:\path\to\gog-installation"
dotnet run --project src/DarkSunWakeRedux.Extractor -- verify-pack
dotnet run --project src/DarkSunWakeRedux.Game -- --smoke-test
```

On Windows, `play.bat` performs a full solution build, verifies or creates the
default local asset pack, and launches the current game build. Override its
documented source default with `DARK_SUN_WAKE_PATH` when necessary.

Extraction is transactional: a new content pack is staged and fully verified
before it replaces the previous verified pack. The ignored default
`UserContent` pack is persistent; keep and reuse it unless extractor or pack
contract changes require replacement. The current required revision 33 pack contains 16,401
lossless DSOP corpus assets (233 source files and 16,168 GFF records) plus 122
specialized derivatives for the evidenced startup, party, ADD-list, Tyr, all-region structural catalogs, menus,
dialogue, and character metadata. The runtime never loads opaque payloads.
Extracted content is ignored by Git and must not be redistributed.

During the active Tyr slice, Alt+Enter toggles native-resolution fullscreen and
F9 toggles a development preview of the measured dialogue portrait, chrome,
and resource-derived first speech/response page over the live map. Its greedy
wrapping is a validation aid. Response rows are condition-filtered in source
order through deterministic Core evaluation; unknown variables fail closed and
the preview uses the bounded observed opening-state snapshot. Implemented
choices and response targets execute atomically; visible targets whose effects
remain unprojected are inert.

F12 captures the final physical backbuffer once on its rising edge. Screenshots
default to `%LOCALAPPDATA%\DarkSunWakeRedux\Screenshots`; pass
`--screenshot-folder <path>` to the Game executable to direct them elsewhere.
Each filename includes a millisecond local timestamp, is created without
overwriting an existing file, and is reported in the window title.

Build and test the complete solution with:

```powershell
./tools/Test.ps1
```

Every compiled C# source file is limited to 1,000 lines by default. The limit can
be lowered with the `MaximumSourceFileLines` MSBuild property for validation.
Exceptional builds can disable it explicitly with
`DisableSourceFileLineLimit=true`; routine development should split oversized
responsibilities instead.

Read source metadata without extracting proprietary payloads using Inspect:

```powershell
dotnet run --project tools/DarkSunWakeRedux.Inspect -- "C:\path\to\gog-installation"
dotnet run --project tools/DarkSunWakeRedux.Inspect -- gff "C:\path\to\RESOURCE.GFF"
dotnet run --project tools/DarkSunWakeRedux.Inspect -- image-preview "C:\path\to\RESOURCE.GFF" ICON 17102 1000 0 "C:\outside-repository\preview.bmp"
dotnet run --project tools/DarkSunWakeRedux.Inspect -- resource-inventory "C:\path\to\RESOURCE.GFF"
dotnet run --project tools/DarkSunWakeRedux.Inspect -- resource-pattern "C:\path\to\RESOURCE.GFF" "printable ASCII"
dotnet run --project tools/DarkSunWakeRedux.Inspect -- object-pattern-overlap "C:\path\to\OBJEX.GFF" 9258 RDFF "printable ASCII"
dotnet run --project tools/DarkSunWakeRedux.Inspect -- record-profile "C:\path\to\RESOURCE.GFF" MONR 1 81
dotnet run --project tools/DarkSunWakeRedux.Inspect -- pair-resource-overlap "C:\path\to\ITEMS.BIN" "C:\path\to\OBJEX.GFF" OJFF
dotnet run --project tools/DarkSunWakeRedux.Inspect -- object-word-overlap "C:\path\to\OBJEX.GFF" OJFF "C:\path\to\ITEMS.BIN"
dotnet run --project tools/DarkSunWakeRedux.Inspect -- resource-word-overlap "C:\path\to\RESOURCE.GFF" PLYL 50 "C:\path\to\CHARSAVE.GFF" CHAR
dotnet run --project tools/DarkSunWakeRedux.Inspect -- object-record-overlap "C:\path\to\OBJEX.GFF" 9258 SCMD
dotnet run --project tools/DarkSunWakeRedux.Inspect -- image-catalog "C:\path\to\RESOURCE.GFF"
dotnet run --project tools/DarkSunWakeRedux.Inspect -- ui-catalog "C:\path\to\RESOURCE.GFF"
dotnet run --project tools/DarkSunWakeRedux.Inspect -- font-catalog "C:\path\to\RESOURCE.GFF"
dotnet run --project tools/DarkSunWakeRedux.Inspect -- text-catalog "C:\path\to\RESOURCE.GFF"
dotnet run --project tools/DarkSunWakeRedux.Inspect -- character-catalog "C:\path\to\CHARSAVE.GFF"
dotnet run --project tools/DarkSunWakeRedux.Inspect -- region-catalog "C:\path\to\RGN032.GFF" "C:\path\to\OBJEX.GFF"
dotnet run --project tools/DarkSunWakeRedux.Inspect -- object-catalog "C:\path\to\OBJEX.GFF" "C:\path\to\RGN032.GFF"
```

The first command emits path, size, and SHA-256 inventory. The second emits only
bounded GFF resource descriptors (tag, number, offset, and size). The third
validates all indexed images and palettes in one GFF and emits dimensions and
counts, but no proprietary pixel or palette content. The fourth validates
bounded window, button, application-frame, and edit-box records and their
resource references.
The fifth validates indexed bitmap-font records and emits glyph counts,
dimensions, character-map identity/hash, and pixel-index range metadata without
emitting proprietary glyph pixels.
The sixth validates TEXT line envelopes and emits only counts and lengths. The
seventh validates bounded character metadata. The eighth validates a region's
identity, planes, decoded tiles, entity records, and external object references,
then emits only structural counts. The ninth validates all object-frame records
needed by that region and their decoded images, again emitting only counts and
dimensions.

Verify the runtime-side derived asset without opening a window:

```powershell
dotnet run --project src/DarkSunWakeRedux.Game -- --content-smoke-test --asset-pack "C:\path\to\UserContent"
```

## Repository projects

- `DarkSunWakeRedux.Core`: deterministic rules and serializable state.
- `DarkSunWakeRedux.Resources`: bounded binary parsing and original-content contracts.
- `DarkSunWakeRedux.Game`: MonoGame DesktopGL presentation with assetless smoke modes.
- `DarkSunWakeRedux.Extractor`: separate legal-copy verification and transactional extraction executable.
- `DarkSunWakeRedux.Inspect`: read-only inventory and research output.
- `DarkSunWakeRedux.Tests`: architecture, safety, and behavioral tests.

New repositories start with `tools/project-config.json` and
`./tools/Configure-Project.ps1`; `docs/CUSTOMIZATION.md` documents every field,
and `./tools/Verify-Configuration.ps1` reports whatever is still left over from
the template. `AGENTS.md` is the working agreement for the repository, including
the rule that `docs/IMPLEMENTATION-PLAN.md` is written and approved before
implementation starts.

Detailed architecture, validation, reverse-engineering, format, and parity notes
live in the other files in this directory. Shared guidance and libraries live in
[Toad Discovery Center](https://github.com/kibertoad/toad-discovery-center).
