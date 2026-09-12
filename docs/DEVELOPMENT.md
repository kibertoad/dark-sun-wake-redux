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

Extraction will be transactional: a new content pack is staged and fully
verified before it replaces the previous verified pack. The current partial
Slice 2 pack contains the evidenced 320x200 title, four start buttons, twenty
character-generation/main-and-modal images, shared party-window image, indexed font,
and deterministic ID-preserving text catalog; dynamic party fields and the
window image's composition semantics remain.
Extracted
content is ignored by Git and must not be redistributed.

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
dotnet run --project tools/DarkSunWakeRedux.Inspect -- image-catalog "C:\path\to\RESOURCE.GFF"
dotnet run --project tools/DarkSunWakeRedux.Inspect -- ui-catalog "C:\path\to\RESOURCE.GFF"
dotnet run --project tools/DarkSunWakeRedux.Inspect -- font-catalog "C:\path\to\RESOURCE.GFF"
dotnet run --project tools/DarkSunWakeRedux.Inspect -- text-catalog "C:\path\to\RESOURCE.GFF"
```

The first command emits path, size, and SHA-256 inventory. The second emits only
bounded GFF resource descriptors (tag, number, offset, and size). The third
validates all indexed images and palettes in one GFF and emits dimensions and
counts, but no proprietary pixel or palette content. The fourth validates
bounded window, button, application-frame, and edit-box records and their
resource references.
The fifth validates indexed bitmap-font records and emits glyph counts,
dimensions, and resource metadata without emitting proprietary glyph pixels.
The sixth validates TEXT line envelopes and emits only counts and lengths.

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
