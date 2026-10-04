# Supported source editions

Support is determined by fingerprints, not storefront branding. Source paths
below refer to the repository owner's legal local installation and are never
runtime defaults or committed content.

## GOG English build 52095422060333615

| Field | Value |
|---|---|
| Status | Exact 233-file baseline recognition and complete 16,524-asset local-corpus extraction implemented, including the evidence-only combat-status panel and 20 source-derived structural region catalogs |
| Acquisition | Legally owned GOG release, *Dungeons & Dragons: Dark Sun Series* |
| Local validation path | `C:\GOG Games\Dark Sun 2` |
| GOG product ID | `1432903719` |
| Installed build ID | `52095422060333615` |
| Language | English (`en-US`) |
| Underlying DOS revision | Version 1.1, dated 1994-12-14 (`BLD-GOG-EN-1.1`); physical retail-media provenance remains unknown |
| Manifest | `src/DarkSunWakeRedux.Extractor/source-manifests/gog-en-52095422060333615.json` |

The recognition manifest is a `RefurbishedDinosaurs.Core` `AssetManifest` with
`sourceKind` `directory`. Its `xxh3` values are the ones the build manifest gives,
and `DarkSunWakeRedux.Inspect <directory>` prints the size and `xxh3` of every
file. The Extractor tries every manifest it embeds, reports why each failed, and
refuses a copy that more than one manifest matches, because those manifests do
not tell the editions apart. It records exact paths, sizes, and XXH3-128 values
for all 233 immutable baseline inputs: 26 GFF containers, 147 VOC files, five FLI
files, 40 music tracks, the disc image/descriptor, original helper executables,
and static configuration/data files. `inventory-source` additionally assigns
each of the 279 currently installed files an explicit disposition: game data,
mutable capture/save, DOSBox wrapper/configuration, storefront wrapper, or
documentation. No input is silently ignored.

`CHARSAVE.GFF` is included as the fingerprinted baseline character archive so
the complete owned source corpus can be reproduced. It may be user-modifiable
under the original game, so a changed original installation is intentionally
reported as a source mismatch; extract from a pristine supported copy rather
than treating a changed archive as the same edition.

The build itself, its files and their hashes, the disc image, the files that
differ between the disc and the installation, and how `DSUN.EXE` is laid out
are described by the spec's build entry,
[`BLD-GOG-EN-1.1`](../spec/builds/BLD-GOG-EN-1.1.md), and its manifest. The
package's `README.TXT` (`SRC-README-1.1`) heads the game data as Version 1.1;
that identifies the game-data revision, not the provenance of any retail CD.

Bounded inspection of the fingerprinted `game.gog` image found a 3,864-byte
disc `CHARSAVE.GFF` containing eight paired character resources (#40-#43 and
#50-#53). The installed 11,735-byte character storage contains 19 pairs
(#29-#43 and #50-#53). The difference is an evidence boundary about character
provenance, not an extraction exception: both the disc image and the installed
archive are preserved as fingerprinted baseline inputs. See
`FND-PARTY-005`; `RULE-PARTY-006` gives the first disc block, #40-#43, as the
party START GAME supplies.

The GOG installation also contains DOSBox integration, manuals, a clue book,
the remaining region GFF files, FLI cinematics, VOC speech/effects, and Ogg
music. Presence is observed; format semantics and required/optional status
remain unknown beyond the separately recorded Tyr structural subset.

## Latest official version readiness

Version 1.1 is the latest official version, and the analysis executable is that
version. The owner accepts https://www.patches-scrolls.de/ as authoritative patch
information. On 2026-10-01 the owner checked its game card for *Dark Sun 2: Wake of
the Ravager*: the Patches tab lists one patch, "patch 1.10" (posted 16.08.13), and
nothing later. The installation's `README.TXT` heads the game data as Version 1.1,
dated 12/14/94 (SRC-README-1.1).

The analysis executable is `C:\GOG Games\Dark Sun 2\DSUN.EXE`: 634,416 bytes,
XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`, SHA-256
`ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`. The SHA-256
was computed from the installed file on 2026-10-01. `tools/project-config.json`
records the path, length and XXH3-128, the hash the standard names every file by,
together with the other facts with `original.patchStatusEstablished` set to true, and
`Bootstrap-Project.ps1 -ValidateFactsOnly` accepts them. The patch archive itself
was not downloaded or compared, so whether its `DSUN.EXE` is byte-identical to the
GOG one is not recorded. Once these facts are recorded they are reused rather than
investigated each session.

## Unsupported sources

No other GOG build, language, retail CD, floppy edition, compilation, Steam
distribution, or modified executable is supported. A future edition requires a
separate manifest plus evidence that its extracted output is semantically
equivalent or intentionally versioned. Manual source selection must remain
available even if storefront discovery is added.
