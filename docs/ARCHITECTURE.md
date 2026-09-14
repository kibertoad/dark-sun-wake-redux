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
- `DarkSunWakeRedux.Resources`: bounded original-format readers, source
  manifests, asset-pack schemas, verification, and provenance contracts. No
  MonoGame.
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

The Extractor verifies the full source fingerprint before decoding,
write into a unique sibling staging directory, generate a manifest containing
format/extractor versions and exact output inventory, re-open and hash every
output, reject unexpected files, then atomically replace the installed pack.
If any step fails, staging is removed and the last verified pack is restored.
Current code exercises this transaction for the exact 72-asset startup, party,
ADD-list, Tyr, Game Menu/Preferences, character, inventory, Cast, and Effects pack: indexed UI images and font, text and
character catalogs, separate resolved UI graphs, the canonical Tyr region, and
its object-frame graph, and the independently identified opening-leader image.
Later resources extend this inventory only after their
mappings are recorded.

## Runtime startup

`--smoke-test` is assetless for CI. `--content-smoke-test` verifies and opens the
72 derived startup/Tyr/menu assets without creating a window. Normal startup verifies
the default or explicit `--asset-pack` directory before creating the game window
and displays the measured start shell plus controls, the party-overview shell,
and the ADD-list shell reached through their Core states. Title sequencing and
ADD-list content/interaction remain pending.
Failure is reported with stable diagnostic codes,
local technical details, and a command to run the Extractor.

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
