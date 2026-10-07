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

## Follow-up synchronization: 25c5808 (2026-10-07)

Target only this configured repository. Compare committed 99c3e11 with
25c5808bb497d863aded815e50838ee1e6d94256 from the canonical clone.
The pinned rule digests/checker, runtime 10.0.0 and Python engine 12.0.0
already match; retain them without a rules refresh. Adopt executable-reader
2.3.0 with its npm lock and the additional applicable evidence-review guidance.
Retain existing stronger local session, clean-room, no-push and validation
contracts; reconcile overlapping guidance rather than overwrite them.
Acceptance: exact dependencies verify, offline rules and link checks pass,
documentation and the assetless full Test.ps1 gate pass. Review every changed
upstream capability for adoption or explicit retention. Risks: generic runtime
permissions and obsolete rule paths must not replace configured constraints.
No owner questions block this maintenance. Commit migration and handover
separately; leave the interrupted research report local for later continuation.

Reconciliation: architecture/media/extraction/configuration, release/signing,
CI checker pin, Python locks and runtime packages already match or retain
stronger configured behavior. Existing goal discovery, separate handover commits,
Queue trailer restrictions, supersession/query-value guidance and implementation
case links already cover the template additions. Adopt the expanded evidence
review, mandatory-deviation test clarification and concrete implementation test
summary. Retain the evidence-backed parameter placeholder and unknown-signature
warning rather than replace it with a no-argument assertion. All other changed
files contain generic profile placeholders, changelog history, lock equivalents
or line-range refreshes already represented by this configured project.

## Follow-up synchronization: 403a749 (2026-10-07)

Target only this configured repository. The canonical template's origin/main
adds command-scoped Git ownership guidance since 25c5808. Adopt that section
in AGENTS.md, using the resolved trusted checkout or worktree path and keeping
the exception local to each command. It grants no push authorization.
Acceptance: the guidance matches the canonical section, avoids wildcard and
global exceptions, and the documentation checks and assetless Test.ps1 gate
pass. No runtime, package, evidence, or game behavior changes are required.
Risk: a broader exception would trust unrelated repositories; retain the exact
checkout scope. Exit: validated guidance and a separate handover are committed
locally without pushing. No owner questions block this maintenance.
## Follow-up synchronization: 0b9ab9c (2026-10-07)

Target only this configured repository. Adopt canonical template 0b9ab9c,
including its pinned Standard/Protocol revision e84495f and checker 2.5.0.
The owner's latest-template request authorizes this bundled rules refresh.
Preserve vendor/upstream paths, npm locks, locked Python tooling, configured
identity, game evidence, owner-only original runtime and mandatory full gate.

Adopt scheduled main-only spec index/parity generation, commit-message address
checks, stricter code/spec separation, coverage reporting guidance and bounded
PE32/MZ/FBOV inventory normalization. Keep generated files out of this batch.
Update applicable skills and docs semantically; retain project-specific source
mappings, stronger hooks and tests. Do not claim provenance/region TSV support
that checker 2.5.0 does not provide. No source analysis or gameplay changes.

Acceptance: snapshot digests, exact checker/action/package pins and section
links agree; synthetic malformed/format/ownership inventory controls pass;
commit-message checks work; documentation and assetless Test.ps1 pass. Audit
processes, commit the migration and a separate handover locally, without push.
Risks: new citation/separation checks may expose existing documentation gaps;
repair their wording without changing original-game claims. Scheduled pushes
may require existing repository permissions; no settings or workflow runs are
changed here. Exit: reconciled migration, passing gates and local commits.
No owner questions block this maintenance.

Reconciliation: adapt the scheduled job to npm ci and retain the local staged-tree
hook, exact Python environment and mandatory full Test.ps1. Local checker runs
use local main as their default base; CI retains its PR base. Keep existing
Inspect package-based tools: the template's AddressCitationTests exercise local
PortableExecutableImage/AddressCitations types that this project superseded,
so their unrelated test file is not imported. Existing adopted tooling controls
remain the acceptance for that retained implementation. Clarify two external
libgff citations as explicit external links to avoid treating its source tree
as this rebuild; original claims and statuses remain unchanged.

Observed exit: focused controls and the full assetless gate passed, including
local/CI/explicit-base and forbidden branch-edit controls. Documentation passes
with existing argument-count skips and the scheduled-generation comparison skip.
The external-source diagnostic was reported after duplicate checks as toolkit
issue 353; the local citation clarification keeps the check enabled. Configured
runtime/media/release/extraction contracts and generated files remain unchanged.
