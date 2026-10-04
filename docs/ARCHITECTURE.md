# Architecture

The product has a separate Asset Extractor and reimplemented runtime, following
the established product boundary used by `C:\sources\rechaos-overlords`.

```text
licensed GOG copy -> Extractor -> verified local asset pack -> Game
                          |                 ^                  |
                          v                 |                  v
                     Resources ------------+                Core
                          ^
                          |
                       Inspect (read-only)
```

- `DarkSunWakeRedux.Core`: deterministic commands, events, explicit RNG, quest
  and combat state, native saves, and replays. No MonoGame, format parsing, I/O,
  wall clock, or ambient randomness.
- `DarkSunWakeRedux.Resources`: bounded original-format readers, the asset
  records the pack holds, and this game's original-content rules in
  `OriginalContent`: its game id, required pack revision, pack location and
  pack checks. No MonoGame. Edition manifests, pack manifests, source and pack
  verification, and pack staging come from the exactly pinned
  [`RefurbishedDinosaurs.Core` and `RefurbishedDinosaurs.LegacyFormats`](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/blob/main/docs/runtime-libraries.md)
  NuGet packages, whose version `Directory.Build.props` sets once
  (`RefurbishedDinosaursVersion`).
- `DarkSunWakeRedux.Extractor`: separately runnable legal-copy verification and
  transactional extraction. It depends on Resources, reads the original source,
  and writes a staged/versioned pack. It never modifies or executes the source.
- `DarkSunWakeRedux.Inspect`: read-only research inventory over owned files.
- `DarkSunWakeRedux.Game`: MonoGame presentation. It may depend on Core and
  Resources but consumes only a completely verified asset pack, never the GOG
  installation, original executable, or DOSBox.
- `DarkSunWakeRedux.Tests`: original-free architecture, safety, parsing,
  behavior, replay, and presentation tests using synthetic fixtures.

## Asset-pack transaction

Edition manifests are the package's `AssetManifest`. The Extractor matches the
owner's directory against every one with `AssetVerifier.IdentifyAsync` and refuses
a copy that no manifest, or more than one, describes. After that it decodes into
a unique sibling staging directory (the package's `StagedAssetPack`), writes an
`InstalledAssetManifest` holding the required revision as its format version, the
source edition and fingerprint, the extractor version and the exact output
inventory, re-opens and hashes every output through `InstalledAssetVerifier`, and
rejects unlisted files. Every file records its source path, media type and
conversion. Only then is the staged pack committed, which swaps it in and
restores the previous pack if the swap fails. A pack that fails verification is
never committed, so the last verified pack stays in place. The full corpus
manifest is about 7.8 MB, so `OriginalContent` reads it with a 16 MiB bound
instead of the package's 4 MiB default.
Current code exercises this transaction for every byte of the 233-file source
baseline and every one of its 16,168 GFF records, retained as source-mapped DSOP
assets where no semantic contract is established. The same required revision 36 pack also carries
the 123 evidenced specialized derivatives for startup, party, ADD-list, Tyr,
the source-derived structural catalogs for every owned region,
Game Menu/Preferences, character, inventory, Cast, Effects, and first dialogue.
Later work adds semantic readers and behavior only after their mappings are
recorded; it does not extend corpus coverage by silently omitting unknown data.

A revision bump means the required derived-asset inventory or contract changed.
`play.bat` then refreshes an older pack transactionally from the owner's licensed
installation instead of running it.

## Runtime startup

`--smoke-test` is assetless for CI. `--content-smoke-test` verifies and opens the
123 derived startup/Tyr/menu/cursor/dialogue/combat-evidence/structural-region assets without creating a window. Normal startup verifies
the default or explicit `--asset-pack` directory before creating the game window
and displays the measured start shell plus controls, the party-overview shell,
and the ADD-list shell reached through their Core states. Title sequencing and
ADD-list content/interaction remain pending.
Failure is reported through the package's `StartupFailure` with the
verifier's problem names, the asset pack path, `startup-error.log` under the
per-user state directory, and the Extractor command to run. The Windows error
dialog appears only when a person launched the game; a smoke test or a run with
`CI` set never blocks on it.

## Determinism

Rules accept explicit commands and seed state and emit sequenced events.
Rendering and audio consume events but cannot mutate rules according to frame
rate. The start/party flow now has versioned snapshots, canonical state hashes,
and replay divergence checks. Exploration camera/view, route, and occupancy
sessions likewise transition only from explicit commands; route advancement is
one semantic cell per command, configurable multi-cell placement is atomic, and
the actor aggregate commits both in lockstep or interrupts without partial
advancement.
Later systems must extend this contract without introducing ambient randomness,
wall-clock decisions, or presentation state.

## ADR-001 - Separate Extractor and verified pack

**Decision:** Replace the generic Import shell with
`DarkSunWakeRedux.Extractor` under `src/`; the runtime reads only its verified
pack.

**Reason:** This keeps proprietary content and risky parsers outside the game
loop, makes licensed ownership explicit, supports transactional upgrades, and
matches the successful Rechaos restoration pattern requested by the owner.

**Consequence:** The game cannot run normally until extraction succeeds. CI and
the assetless smoke path remain independent of all proprietary data.
