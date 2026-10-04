# Handover

Current work outside a goal. Active research goals retain their own handovers in
docs/goals/; queue items and findings remain in their respective durable files.

## State

- Stage: Slices; slices 2 and 3 remain in progress. Survey exit still needs Q-EXE-003.
- Last gate: 2026-10-04, tools/Invoke-Validation.ps1 passed (715 .NET tests, 75 upstream node tests), including locked restore, Release build and assetless publish/smoke. Evidence: artifacts/template-migration/template-39d31fd-validation.log. Run it with NoDefaultCurrentDirectoryInExePath unset: when set, cmd.exe cannot find the launcher test's invoke.cmd.
- Template 39d31fd is adopted: standard c1758fd, checker 0.2.0 (action a260e39), reader 1.0.0, engine 1.0.1, RefurbishedDinosaurs 2.0.0 .NET packages and the toolkit's software OpenGL action. Dispositions: docs/TEMPLATE-ACCEPTANCE.md.
- Required asset pack revision is 36. The owner's revision-35 pack under LocalAppData is rejected until play.bat re-extracts it.
- Established latest-original-version readiness is recorded in project-config and SOURCE-EDITIONS. Native runtime remains owner-only; no original run or executable analysis occurred in the migration.
- Process audit found no migration-owned orphan requiring termination; reusable MSBuild nodes remain untouched.

## Unfinished

No unfinished migration work. Continuing shared-tooling work and its external-delivery approval are recorded in docs/goals/upstream-gap-resolution.md; the scoped-memory acceptance plan is committed.

## Blockers

PR 4 (shared settings storage, RefurbishedDinosaurs.Core 1.3.0) overlaps this migration in Directory.Build.props, the Game csproj and package locks; it needs a rebase onto 2.0.0. Live signing, repository signing-environment branch protection and high/mixed-DPI original-window capture remain separate acceptance checks. No release was performed.

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
