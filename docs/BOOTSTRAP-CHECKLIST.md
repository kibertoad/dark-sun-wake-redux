# Bootstrap checklist

Work top to bottom. `AGENTS.md` describes the same sequence for coding agents,
and `docs/CUSTOMIZATION.md` documents every configuration knob.

## Plan

- [x] Record the original title, developer, release year, genre, and the
      editions available for validation.
- [x] Fill in `docs/IMPLEMENTATION-PLAN.md` and have it approved before writing
      implementation code.

## Configure

- [x] Fill in `tools/project-config.json` and run `./tools/Configure-Project.ps1`.
- [x] Run `./tools/Verify-Configuration.ps1` and resolve every finding.
- [x] Customize the player-facing README, acknowledgements, NOTICE description,
      and the supported/limited feature table.

## Original content

- [x] Replace the sample manifest and add one fingerprint manifest per supported
      edition.
- [x] Extend `tools/repository-policy.json` with the extensions the original
      game actually uses.
- [x] Decide what may be clean-room/open data and what must remain user-imported.
- [x] Add explicit/manual source selection first; add storefront, registry,
      media, or archive discovery as optional adapters.
- [x] Implement read-only inventory in `Inspect` before extraction.
- [x] Implement bounded format readers in `Resources` with synthetic fixtures.
- [x] Transform rather than copy original executables whenever decoded data is
      sufficient.
- [x] Verify generated files before committing the staged `UserContent` directory.
- [x] Make missing content produce an actionable GUI error and local diagnostic log.

## Implement

- [ ] Add deterministic commands, events, seed control, snapshots, and replay to
      Core. The start/party flow now has all five; keep this open until gameplay
      commands and persistence use the same contract.
- [ ] Fill in architecture, format, analysis, fidelity, and validation docs as
      the answers arrive.

## Package and verify

- [ ] Customize Inno Setup source discovery and validation; keep the generated
      AppId, shortcut smoke tests, and uninstall checks.
- [ ] Customize Debian package name/dependencies and macOS bundle
      identifier/minimum version if shipping them.
- [ ] Run assetless package inspection and installed executable tests on CI.
- [ ] Remove this checklist when all project-specific decisions are captured
      elsewhere.
