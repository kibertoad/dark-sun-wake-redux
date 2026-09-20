# Supported source editions

Support is determined by fingerprints, not storefront branding. Source paths
below refer to the repository owner's legal local installation and are never
runtime defaults or committed content.

## GOG English build 52095422060333615

| Field | Value |
|---|---|
| Status | Exact 233-file baseline recognition and complete 16,523-asset local-corpus extraction implemented, including 20 source-derived structural region catalogs |
| Acquisition | Legally owned GOG release, *Dungeons & Dragons: Dark Sun Series* |
| Local validation path | `C:\GOG Games\Dark Sun 2` |
| GOG product ID | `1432903719` |
| Installed build ID | `52095422060333615` |
| Language | English (`en-US`) |
| Underlying DOS revision | Version 1.1, documented by the owned package README and dated 1994-12-14; physical retail-media provenance remains unknown |
| Manifest | `src/DarkSunWakeRedux.Extractor/source-manifests/gog-en-52095422060333615.json` |

The recognition manifest records exact paths, sizes, and SHA-256 values for all
233 immutable baseline inputs: 26 GFF containers, 147 VOC files, five FLI
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

`DATA-GOG-README-001` records the source-revision evidence without retaining
the README: its package heading identifies Version 1.1 and distinguishes the
earlier 1.0 and 1.01 save formats. A later historical fixes section is labelled
1.02, but does not replace the package's Version 1.1 heading. This identifies
the supported game-data revision, not the provenance of any original physical
CD or retail release.

Bounded inspection of the fingerprinted `game.gog` image found a 3,864-byte
disc `CHARSAVE.GFF` containing eight paired character resources (#40-#43 and
#50-#53). The installed 11,735-byte character storage contains 19 pairs
(#29-#43 and #50-#53). The difference is an evidence boundary about character
provenance, not an extraction exception: both the disc image and the installed
archive are preserved as fingerprinted baseline inputs. See
`DATA-GOG-CHAR-006`; neither block is yet designated as the complete
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
