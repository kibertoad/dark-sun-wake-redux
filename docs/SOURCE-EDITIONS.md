# Supported source editions

Support is determined by fingerprints, not storefront branding. Source paths
below refer to the repository owner's legal local installation and are never
runtime defaults or committed content.

## GOG English build 52095422060333615

| Field | Value |
|---|---|
| Status | Source recognition implemented; extraction not yet implemented |
| Acquisition | Legally owned GOG release, *Dungeons & Dragons: Dark Sun Series* |
| Local validation path | `C:\GOG Games\Dark Sun 2` |
| GOG product ID | `1432903719` |
| Installed build ID | `52095422060333615` |
| Language | English (`en-US`) |
| Underlying DOS revision | Unknown; investigation required |
| Manifest | `src/DarkSunWakeRedux.Extractor/source-manifests/gog-en-52095422060333615.json` |

The recognition manifest currently uses six independent anchors:
`DSUN.EXE`, `game.ins`, `GPLDATA.GFF`, `ITEMS.BIN`, `OBJEX.GFF`, and
`RESOURCE.GFF`. It records exact sizes and SHA-256 values. These anchors identify
the owned build but are not a claim that they are the complete extraction input
set. Each decoder slice must add every source file it consumes to the supported
edition contract before extraction can succeed.

The GOG installation also contains DOSBox integration, manuals, a clue book,
region GFF files, FLI cinematics, VOC speech/effects, and Ogg music. Presence is
observed; format semantics and required/optional status remain unknown.

## Unsupported sources

No other GOG build, language, retail CD, floppy edition, compilation, Steam
distribution, or modified executable is supported. A future edition requires a
separate manifest plus evidence that its extracted output is semantically
equivalent or intentionally versioned. Manual source selection must remain
available even if storefront discovery is added.
