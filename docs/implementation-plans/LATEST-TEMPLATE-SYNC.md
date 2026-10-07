# Latest template synchronization

## Outcome and evidence

Synchronize this configured repository with the committed golden template
`kibertoad/refurbished-dinosaurs-template` at `99c3e11` (2026-10-07), as the
owner requested. Source: an isolated archive of origin/main, fetched from the
canonical template clone. Only this repository is a target. Preserve its
licensed-edition evidence, extraction revision, game rules, owner-only DOSBox
policy, no-push goal discovery and dedicated locked dependency environment.
This is tooling maintenance, with no game implementation or spec promotion.

## Semantic delta and retained contracts

| Capability | Migration or retention |
|---|---|
| Project architecture, extraction and source media | Retain configured Core/Resources/Game/Extractor/Inspect boundaries and game-specific extraction; shared runtime 10.0.0 exceeds template 2.0.0. |
| Rules and checker | Retain vendor/upstream and checker 2.2.0; template uses older docs/upstream and 2.1.0. No rules refresh or downgrade. |
| Dependency automation and test packages | Retain weekly NuGet groups, exact locks and current Test SDK/xunit versions, already matching template. |
| Evidence interpreter and Ghidra | Retain dedicated hash-locked virtual environment, explicit override and installed shared reporters; retain the verified Windows combined-script-path quoting. Template system-python fallback is superseded by this stricter setup. |
| Release and signing | Retain existing off-main/rerun rejection, exact signing thumbprint check, platform smoke tests, locked packaging and configured identity. |
| Pre-commit | Keep staged-tree checks, junction unlink cleanup, exact dependency verification and executing-account TEMP setup; use template's Git-root discovery instead of assuming current directory is the checkout root. |
| Build exclusion | Add template's conventional bin/obj exclusions so redirected intermediate output cannot ingest stale generated source. |
| Goal lifecycle | Add worktree-local goal-run tool, Claude Stop hook and synthetic tests; adapt lifecycle docs to local claims and explicit no-push authorization. Missing or empty session IDs must pass without binding or consuming another session's marker. |
| Runtime capabilities | Carry per-part admission guidance into runtime-access, planning, research and live-session procedures without probing or changing established owner/runtime answers. |
| Continuation cases | Carry template's concrete composed-state summary into AGENTS; retain the newer pinned protocol and existing implementation-skill links. |
| Validation | Keep tools/Test.ps1 and full assetless pre-commit batch gate required by this project's owner instructions; template's relevant-tests-only default does not override them. |

## Acceptance and tests

- Goal stops are held only for the identified session and worktree; deletion,
  explicit stop, another session/worktree, invalid/missing identity and stalled
  HEAD behave as specified. Run imported synthetic goal-run tests plus missing
  identity controls. No actual goal stop or original-game run is required.
- Runtime procedures route by each required capability part, never by a broad
  answer that hides an unavailable part; owner-only original access remains.
- Existing identities, rule/checker/package pins, source manifests and parity
  remain intact. Configuration and infrastructure checks prove retained safety.
- Run documentation generation/check, targeted goal tests and tools/Test.ps1
  without GAME_DIR. Audit processes and commit locally, with a separate handover.

## Risks and exit

The template contains older packages, rule paths and generic runtime permission;
blind replacement would regress this project. Review and adapt by capability.
A Claude hook consumes stdin and writes only a worktree Git-local marker; its
synthetic tests must reject cross-session binding and accidental marker changes.
Report reproducible template defects after checking duplicate issues.
Exit: the matrix is reconciled with actual changes, checks pass, the validated
migration and handover are committed locally, and no push occurs. No owner
questions block this authorized maintenance.
