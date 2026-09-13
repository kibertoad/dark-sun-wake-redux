# Supported source editions

Support is determined by fingerprints, not storefront branding. Source paths
below refer to the repository owner's legal local installation and are never
runtime defaults or committed content.

## GOG English build 52095422060333615

| Field | Value |
|---|---|
| Status | Source recognition and bounded 62-asset startup/Tyr/menu extraction implemented |
| Acquisition | Legally owned GOG release, *Dungeons & Dragons: Dark Sun Series* |
| Local validation path | `C:\GOG Games\Dark Sun 2` |
| GOG product ID | `1432903719` |
| Installed build ID | `52095422060333615` |
| Language | English (`en-US`) |
| Underlying DOS revision | Unknown; investigation required |
| Manifest | `src/DarkSunWakeRedux.Extractor/source-manifests/gog-en-52095422060333615.json` |

The recognition manifest currently uses seven independent immutable anchors:
`DSUN.EXE`, `game.ins`, `GPLDATA.GFF`, `ITEMS.BIN`, `OBJEX.GFF`, and
`RESOURCE.GFF`, plus the first required region `RGN032.GFF`. It records exact
sizes and SHA-256 values. These anchors identify
the owned build but are not a claim that they are the complete extraction input
set. Each decoder slice must add every source file it consumes to the supported
edition contract before extraction can succeed.

`CHARSAVE.GFF` is required supplemental extraction input, but it is character
storage and therefore is not an immutable edition fingerprint. The Extractor
requires the file, validates its complete bounded GFF/`CHAR`/`PSIN` structure,
and records it as DSCH provenance only after the seven immutable anchors identify
the supported installation.

Bounded inspection of the fingerprinted `game.gog` image found a 3,864-byte
disc `CHARSAVE.GFF` containing eight paired character resources (#40-#43 and
#50-#53). The installed 11,735-byte character storage contains 19 pairs
(#29-#43 and #50-#53). This difference is further evidence that the installed
file must remain supplemental mutable input rather than an eighth exact anchor.
See `DATA-GOG-CHAR-006`; neither block is yet designated as the complete
pregenerated party.

The GOG installation also contains DOSBox integration, manuals, a clue book,
the remaining region GFF files, FLI cinematics, VOC speech/effects, and Ogg
music. Presence is observed; format semantics and required/optional status
remain unknown beyond the separately recorded Tyr structural subset.

## Unsupported sources

No other GOG build, language, retail CD, floppy edition, compilation, Steam
distribution, or modified executable is supported. A future edition requires a
separate manifest plus evidence that its extracted output is semantically
equivalent or intentionally versioned. Manual source selection must remain
available even if storefront discovery is added.
