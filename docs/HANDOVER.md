# Handover

Current work outside a goal. Active research goals retain their own handovers in
docs/goals/; queue items and findings remain in their respective durable files.

## State

- Stage: Slices; slices 2 and 3 remain in progress. Survey exit still needs Q-EXE-003.
- Last gate: 2026-10-02, tools/Invoke-Validation.ps1 passed, including tools/Test.ps1, locked restore, Release build and assetless publish/smoke. Evidence: artifacts/template-migration/final-validation.log.
- Latest upstream migration is complete. Template 79d18a20, toolkit 0b4694df, reader 0.2.0, checker 0.1.0 and engine 0.4.0 are current. Standard/methodology/protocol ca39d075 freshness and offline digests pass. Capability dispositions: docs/TEMPLATE-ACCEPTANCE.md.
- Established latest-original-version readiness is recorded in project-config and SOURCE-EDITIONS. Native runtime remains owner-only; no original run or executable analysis occurred in the migration.
- Process audit found no migration-owned orphan requiring termination; reusable MSBuild nodes remain untouched.

## Unfinished

The earlier template migration is complete. Shared runtime migration uses published toolkit 1.3.0 packages. Continuing shared-tooling work and its external-delivery approval are recorded in docs/goals/upstream-gap-resolution.md; the scoped-memory acceptance plan is committed.

## Blockers

Live signing, repository signing-environment branch protection and high/mixed-DPI original-window capture remain separate acceptance checks. No release was performed.

## Next

1. Continue existing goals from their handovers; migration does not close research requests.
2. Q-EXE-003 Survey exit; then Q-CONFIG-008, Q-CONFIG-007 and Q-CONFIG-002 for RULE-CONFIG-005 and FMT-CONFIG-003.
3. Q-UI-005 and Q-SAVE-001 for SCR-UI-013 and SCR-UI-014; Q-UI-002 for SCR-UI-007.
4. Q-CONFIG-001 owner live session; Q-PARTY-001, Q-PARTY-005 and Q-PARTY-009.
5. Configure ES_CERTIFICATE_THUMBPRINT and main-only release-signing deployment branches before a signed release.

Shared FLI readiness tooling: Inspect `fli-check` now uses RefurbishedDinosaurs.Media.Fli 1.0.0.
Synthetic COPY, FLC rejection and chunk-overrun tests pass. It does not implement cinematic
playback or raise spec status. Original-media validation remains local and was not run.

The toolkit documentation action version comment now matches its pinned commit.
Zizmor passes locally with the CI default persona.

## Shared runtime migration

Branch: `feat/shared-runtime-primitives`. See [migration status](SHARED-RUNTIME-MIGRATION.md).
The solution build, .NET settings suite and full canonical gate passed with a workspace-local JDK.
The migration uses public 1.3.0 packages and refreshed NuGet locks. The full canonical gate
passes, including Java export-boundary and .NET settings controls.
The validation-lease fixture now uses production path initialization, so Windows short
and long TEMP paths select the same lock. Synthetic failure and exclusion controls pass locally.
Original-game parity and live-device behavior were not assessed.
