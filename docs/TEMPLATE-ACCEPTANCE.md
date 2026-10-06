# Template and toolkit acceptance audit

## Rules efa138b, checker 2.2.0 and template ebdd5c3, 2026-10-06

The vendored Standard, Methodology and Protocol move from `c1758fd` to
`efa138ba212260b23bb4eb599e61336a7126c973`, still Standard v1. The checker
moves from 1.1.0 to 2.2.0, the release at toolkit
`92a5592055559013373ea243a458f4ffc4fa05c4`; the CI action pin, the lock and
`package.json` agree. Template main `ebdd5c3` (four commits after `7a79818`)
is the template side of the same change.

| Change | Disposition |
| --- | --- |
| `deviated` parity status, deviation Replaces and Tests items (checker 2.1.0) | `PARITY.md` regenerated with the new Status row; deviation template in SPEC-ENTRY-TEMPLATES and the plan-work skill updated as the template did. No current deviation replaces an entry entirely, so no row changes status. |
| Argument counts (checker 2.2.0) | RULE-IMAGE-001 and RULE-COMBAT-005 Parameters are in the countable list form. The other uncounted sections stay in prose: SCRIPT rules belong to the config-static goal, RULE-IMAGE-002 has a flag with no notation type, and the emitting rules would fail falsely until toolkit issue 317 is decided. |
| Findings edited in place only without changing what they record (IDENTIFIERS-8) | No finding changed. |
| Queue trailer lists only closed items; goal-file commits are not batches | research-item and end-session skills say so. |
| New test sections (calls that combine results, rules behind an adapter, widths, allocation and cleanup, continuation cases) | implement-rows points to them with line ranges. |
| Template Test SDK 18.10.1 | Adopted with a refreshed test lock. |
| Checker 2.0.0 `starting_state` forms, 1.0.0 `.fs` citations | Existing experiments and code pass; no change needed. |

Section line ranges were rewritten with `node tools/upstream.mjs links --write`.
No spec claim or parity status changed.

## Published engine 4 adoption, 2026-10-04

Engine 4.0.0 is adopted with reader 2.0.0 for prepared protocol 3. The exact
wheel/source-release verification, published source suite, installed PE bridge,
retained inventory and explicit hardware/continuation migrations are recorded
in REPORTER-CASE-AUDIT.md. Checker and Capstone/pypcode/xxhash stay pinned.
Full adoption validation: artifacts/continuation-budget-delivery/adoption-validation.log.
Game-specific unresolved producer, alias, native-device and whole-gap contracts
remain open; capped queries are not accepted as complete reports.

## Published scoped-memory pair adoption, 2026-10-04

Reader and engine 2.0.0 are adopted together for prepared protocol 3, ahead of
template 049f300's older tooling pins. Exact archive/source integrity, installed
synthetic and retained original-source controls are recorded in
REPORTER-CASE-AUDIT.md. The checker stays 0.2.0; Capstone/pypcode/xxhash and
vendored rule pins remain unchanged. Normal full validation passes; evidence:
artifacts/protocol3-delivery/adoption-validation-final.log. This adoption does
not establish native service preservation or complete any broader gap contract.

## Latest template dependency maintenance sync, 2026-10-04

Template main `7a798184faddb271da2ec52745887972fdcdeb72` was fetched after the
initial 192208c maintenance sync; the additional xUnit 4.0.1 patch and
refreshed test package lock are adopted. The complete
changed-file delta from `049f300` is accounted for here.

| Capability | Disposition |
| --- | --- |
| Dependabot | Adopted weekly NuGet groups and reviewed SabreTools exclusion; adapted npm/Python guidance to exact npm and hash-locked Python dependencies. |
| Interpreter helper, report wrapper and memory-block tests | Retained stronger project-local interpreter selection, verified installed pins, withEngine and ghidraScriptPath routing. No competing PATH-probing helper added. |
| Evidence test and validation comments | Existing verified interpreter routing already covers their purpose. |
| Development and evidence setup guidance | Updated maintenance policy; retained documented locked setup instead of template global pip installation. |
| Template changelog | Provenance recorded here; template-specific historical state remains upstream. |

Validation evidence: `artifacts/template-dependency-sync-validation.log`.
No original game files were read or run; no spec or parity status changed.

## Current template sync, 2026-10-04

Template main `049f30036e612e3680838a59d4995ad8f0c85e00` was fetched and
rechecked after validation. The complete delta from accepted `39d31fd` is
accounted for below; earlier acceptance records remain historical.

| Changed capability | Disposition |
| --- | --- |
| Pre-commit dependency link | Adopted Node-created junction with explicit unlink before snapshot cleanup; retained configured exact-lock verification and EVIDENCE_PYTHON routing. |
| Release workflow | Adopted explicit off-main failure, existing-Release rerun rejection and no-mac-x64 preset. Retained configured packaging locks, signing default none, artifact contracts and bounded job durations. No release performed. |
| Signature helper | Already identical to latest template; retained. |
| Tag helper | Adopted exact latest source, including newline-preserving error messages; tag identity/create/rejection controls pass. |
| Release tests and gate routing | Adopted exact latest synthetic release tests; Test.ps1 runs them inside its selected PWSH scope alongside offline controls. Invoke-Validation retains configured locking, restore, build and publish orchestration. |
| Releasing and validation guidance | Signing, rerun and branch prerequisites reconciled; configured multi-platform CI retained. Existing VALIDATION signing prerequisites already cover the template change. |
| Plan, handover and template changelog | Current plan target updated, handover rewritten separately, and provenance recorded here rather than copying template-specific state or its outside-project plan reference. |
| Rule snapshots and package pins | No changes in this template delta; installed exact locks and pinned offline rule verification pass. |

`tools/Invoke-Validation.ps1` passed with locked restore, canonical checks,
Release build and assetless publish/smoke. Evidence:
`artifacts/template-latest-sync-validation.log`. The original game was not run
or read; no spec or parity claim changed. Local synthetic capture diagnostics
are reported separately in template issue 58; Gap 44's historical cause remains
unconfirmed.

## Windows gate environment follow-up, 2026-10-04

The canonical `tools/Test.ps1 -NoRestore` passes with normal user access and
process-scoped `PSExecutionPolicyPreference=Bypass`; evidence is
`artifacts/goal-continuation-authorized-test.log`. Restricted child PowerShell
policy stops the synthetic capture tests before their assertions. Template
[issue 55](https://github.com/kibertoad/refurbished-dinosaurs-template/issues/55)
records that setup concern after duplicate checking; no capture guard was changed.

Both synthetic child launches now declare `-ExecutionPolicy Bypass` for that
process only. The direct Node test run under a Restricted parent policy passes
the stalled-worker recovery, physical-edge, invalid-window and blank-frame
controls; evidence is `artifacts/capture-restricted-parent.log`. This corrects
test setup without changing persistent policy or the capture implementation.

Sandbox Java realpath access to the inherited temporary directory also failed.
Putting temporary fixtures inside this checkout clears that access failure but
invalidates the configuration-copy test's non-checkout control. Use an accessible
temporary directory outside any checkout. These environment failures do not
establish a capture defect or close a reporter gap.

## Current migration acceptance, 2026-10-04

The owner asked for the latest template. Template main was
`39d31fdef9d7420e8571ab6d09e3b3026be05010` (template PR 46), one commit after the
previously accepted `79d18a2`. Its file changes were mapped onto this configured
project and each was adopted or adapted as below. The toolkit already publishes
newer reader and engine releases (2.0); this migration takes the versions the
template pins.

| Component | Target | Disposition |
| --- | --- | --- |
| Template | `39d31fdef9d7420e8571ab6d09e3b3026be05010` | Adopted with the configured adaptations below. |
| Standard v1, methodology and protocol | `c1758fd9c2fd253e28fb4328412061d7e92ce14a` | Standard text refreshed (rules numbered, still v1); methodology, protocol and license bytes unchanged. Section link ranges rewritten. |
| Standard checker | `0.2.0`, action at toolkit `a260e391fe2dea67f0034d5480e2bb439b0663ac` | The lock records the action commit and the version it carries; `upstream.mjs` requires the CI pin, the lock and `package.json` to agree and refuses another installed version. Refresh accepts only the toolkit commit tagged for the release. |
| Executable reader | `1.0.0` | Exact npm lock. Prepared-config protocol 2: sources are named by xxh3, and a config naming sha256 is refused. |
| Evidence engine | `1.0.1` | Exact wheel hash, plus hash-pinned xxhash 4.0.1 (new engine dependency); Capstone 5.0.7 and pypcode 4.0.0 unchanged. |
| RefurbishedDinosaurs .NET packages | `2.0.0` | Core and LegacyFormats replace the local edition manifest, source and pack verification, staging, path and startup-failure code. Set once in `Directory.Build.props`. |
| Software OpenGL action | toolkit `6ab530df7851354c98e833a2beb76dbe0f71e421` | Replaces `Install-MesaSoftwareGL.ps1` in CI and release; the step fails when the driver cannot be installed. |

| Capability | Disposition and reason |
| --- | --- |
| Package manager | npm with `package-lock.json` and `Restore-ToolDependencies.ps1` stay; the template's pnpm switch is not adopted. The project's restore already verifies exact versions and archive integrity, and the checker pin checks apply to `package.json` the same way. |
| Python requirements | Remain in `tools/evidence/requirements.txt` with `--require-hashes`, installed into `artifacts/evidence-python`; the template's unhashed `requirements-evidence.txt` is not adopted. |
| xxh3 naming | Evidence configs, captured frames (checkpoint schema 3) and the analysis gate use xxh3. The gate now requires `analysisExecutable.xxh3_128`; the SHA-256 stays in SOURCE-EDITIONS as a historical record only. |
| Asset pack | Required revision 36: the manifest is the toolkit's installed-asset layout. Retained project contracts: unlisted files rejected, a 16 MiB manifest cap (the corpus-scale manifest exceeds the toolkit's 4 MiB default), and required media type and conversion on every file. Revision 35 packs are rejected and `play.bat` re-extracts. |
| Edition identification | `AssetVerifier.IdentifyAsync`; a copy two manifests match is refused. Only directory sources are supported, so the template's ISO and CUE/BIN tests are not copied. |
| Startup failures | `StartupFailure.Report` from Core; a small local wrapper supplies the stale-pack recovery text and suppresses the dialog for smoke tests and CI. |
| Inspect | Uses `FileFingerprint`; it never had the template's SpecHash or `--sha256` citation option. |

Verified against the owned GOG copy without running the original: `verify-source`
gives fingerprint `5dfea1d78b28656cb1ce976f81dd77d4`, matching the existing pack; a
scratch extraction produced 16,524 files that `verify-pack` accepts, and the
Game's content smoke test exits 0. Gate log:
`artifacts/template-migration/template-39d31fd-validation.log`. No spec claim,
parity status or research request changes.

## Earlier migration acceptance, 2026-10-02

Toolkit main and the public package registries were rechecked for the owner's
latest-toolkit request. Toolkit targets below are current at that check; template
and rule snapshots retain their earlier accepted revisions.

| Component | Verified current target | Disposition |
| --- | --- | --- |
| Template | `79d18a20cb4d97c7153e74e5695cbb80e7ebf73e` | Previously accepted template capabilities retained; configured adaptations preserved. |
| Toolkit | `592088dbdb1cc8d804ba853eb39ae6ca1215577f` | Latest reviewed main/release source; CI action advanced, with unchanged action/checker source. |
| Executable reader | `0.2.0` | Exact npm archive lock and installed version already current. |
| Standard checker | `0.1.0` | Exact npm archive lock and installed version already current. |
| Evidence engine | `0.8.0` | Exact PyPI wheel hash; every shipped source/test and installed engine file matches the release archive/tag. |
| pypcode runtime | `4.0.0` | Latest engine-required version, with published cross-platform wheel hashes and installed-version rejection controls; pypcode is the sole instruction backend. |
| Standard v1, methodology and protocol | `ca39d0750e67c8c3900e8554e66a84083fe67452` | Retained accepted snapshot; offline digests match. No rule freshness check or refresh in this toolkit update. |

Comparison uses a clean, commit-selected template under ignored artifacts,
rather than the older sibling checkout. The file comparison is generated in
`artifacts/template-migration/file-comparison.json`. Current acceptance supersedes
the historical version and prerequisite statements below.

| Capability | Current disposition and reason |
| --- | --- |
| Project boundaries and extraction | Retain configured Core/Resources/Game/Extractor/Inspect boundaries, exact GOG manifests, XXH3 fingerprints, complete opaque corpus extraction, transaction rollback and required pack revision rejection. Generic sample state and manifests cannot replace these contracts. |
| Source media and cabinet expansion | Retain directory-only supported GOG edition. ISO/CUE and InstallShield adapters are outside its evidenced supported input contract; adding them would imply new media support. No supported edition needs cabinet expansion. |
| Bootstrap and configuration | Adopted one-time latest-version gate and configured regression controls remain; project-config records provenance, length and SHA-256. Earlier unresolved-prerequisite prose is historical. No re-investigation of established game facts. |
| Parsing and original-content policy | Retain bounded DOS MZ/FBOV tooling, configured restricted extensions and local-content/diagnostic protections, all exercised by synthetic controls. |
| Launchers and validation | Retain play.bat identity, argument/exit/smoke contracts, serialized validation, Test.ps1 routing, strict NoRestore and locked dependencies. Template process stopping is not copied: this task does not own pre-existing processes. No long-running test category currently requires separate selection. |
| CI, packaging and signing | Retain configured platform/installer jobs, pinned signing adapters, main-only signed-release safeguards and signature rejection controls; advance the unchanged latest toolkit checker action pin. No release or live signing is part of migration. |
| Shared Ghidra and instruction reporters | Use published engine script directory and reader entry points instead of reintroducing template-vendored implementations. Installed source matches its release tag; local tests cover routing, versions, pypcode and diagnostics. Retained broad exporters guard local-only output. |
| Inspect, citations and inventory | Retain DOS MZ/FBOV mapped-location checks, spec hash/source identity verification and established CD inventory paths. Generic PE-specific citation and image helpers do not replace DOS contracts. |
| Rules, skills and documentation | Pinned rules remain under vendor/upstream with checked section ranges. Retain origin terminology, owner-only DOSBox policy, no automatic push, extraction gate and stronger evidence requirements. Existing research goals and candidate work retain their own scope. |

Earlier full migration validation: `tools/Invoke-Validation.ps1` runs `tools/Test.ps1`,
configuration and repository checks, synthetic infrastructure/reporting tests,
locked restore, Release build and assetless publish/smoke. Generated evidence:
`artifacts/template-migration/canonical-validation.log` and
`artifacts/template-migration/final-validation.log`. No game behavior, spec claim,
parity status or research-request closure follows from this migration. Latest
toolkit-only source and configured gate evidence is recorded in VALIDATION.md
and artifacts/package-delivery/engine080-root-test.log.

## Historical acceptance audit


Audit date: 2026-10-01. Audited checkout: `fb481a0`.

## Verdict

The relevant infrastructure gaps found by the initial audit are now addressed.
`tools/Invoke-Validation.ps1` passes locked restore, the canonical policy and test
suite, Release build, assetless publish and smoke. Exporters compile against
Ghidra 12.1.3 public APIs without opening an original program. Local-only export
boundary controls and release/signing failure paths pass synthetic tests.

Latest-original-version readiness remains explicitly unresolved: version 1.1 is
recorded, but conclusive patch provenance and the approved baseline SHA-256 are
not yet recorded. The executable-analysis gate rejects that state. This is a
research prerequisite, not a build failure. The owner authorized Patches Scrolls
as a patch authority on 2026-10-01; attempts to read its current site returned
HTTP 403. No missing patch facts were inferred from that failure.

## Capability disposition after migration

| Capability | Accepted disposition |
| --- | --- |
| Bootstrap/version gate | Defensive one-time facts gate, strict boolean, version agreement and recorded SHA-256/length controls; configuration preserves nested facts. Missing provenance fails closed. |
| Signing | Golden pinned CodeSignTool, eSigner and OpenPGP adapters; opt-in Windows/Linux workflow. Timestamp, exact output names, expected certificate and expected GPG fingerprint required. Synthetic negative checks pass; no live signing performed. |
| Release workflow | Compared New Chrome main `88f4112791db0caaf488b5bcd906cabd842bd4e1`. Main-only preparation, bounded jobs and same-commit tag reruns adopted. Golden downloader/error-handling improvements retained. |
| Dependency locks | Every .NET project has a lock; locked restore passes. |
| Validation | Checkout lock prevents concurrent runs; errors release it. Canonical gate and CI check configured infrastructure. Filtered tests report partial acceptance. |
| Broad exports | Four golden exporters adopted with real-path local-output guards and bounded edition labels; public API compilation and synthetic boundary checks pass. Version Tracking tools remain dormant for the single supported edition. |
| Launcher | Existing play.bat identity preserved; SDK diagnostic, argument forwarding, smoke bypass and process exit propagation pass synthetic controls. |
| PE-oriented Inspect/citation helpers | Not applicable to DOS MZ/FBOV. Existing Inspect, ReportFbovOverlayMap, physical-pattern reporter, mapped-location checker and inventory join retain the corresponding configured capabilities, exercised by the canonical synthetic gate. No PE schema or new edition introduced. |
| Other broad Ghidra helpers | Existing bounded pinned reporters cover the project research workflow. No broad analysis database or proprietary export enters Git. Missing optional exporter filenames are not acceptance gaps for the single-edition DOS workflow. |

Evidence: `artifacts/migration-acceptance.log`, public exporter compilation under
`artifacts/java-controls/`, and the canonical `tests/upstream/` controls. The
following sections preserve the initial audit and its historical hosted results;
that hosted run predates these changes. New workflow changes have local synthetic
acceptance, not a new hosted installer run or signed release.

## Source and integrity acceptance

GitHub's main branch endpoints independently confirmed:

| Source | Current revision | Result |
| --- | --- | --- |
| Template | `7b3bbe46b251b163ee02a6539ac0d81559dbe921` | Matches recorded adoption and local golden checkout |
| Toolkit | `7da1b93cdd9ac0d59dbaf82b66b4db95d578ab9d` | Matches checker/CI and reporter pins |

Every file in tools/upstream-lock.json and tools/evidence/x86-lock.json matches
both its digest and the corresponding committed source object. Offline snapshot
verification, reporter verification, local section links and documentation checks
pass. The source comparison used committed objects rather than trusting sibling
working trees. A sibling checkout has an unrelated untracked .worktrees/
directory; neither sibling repository was modified.

## Executed acceptance

| Check | Result | Evidence |
| --- | --- | --- |
| Canonical tools/Test.ps1 | Pass, including Python, Node, .NET, repository/configuration, documentation, tracking, capture and pin checks | artifacts/migration-reaudit-test.log |
| Full solution build | Pass, zero warnings/errors | artifacts/migration-reaudit-build.log |
| Release publish and assetless smoke | Pass; published output has no UserContent directory | artifacts/migration-reaudit-publish.log and artifacts/migration-reaudit-assetless/ |
| Golden Test-TemplateInfrastructure.ps1 against this checkout | **Fail** | artifacts/migration-reaudit-infrastructure.log |
| Hosted platform and installer acceptance | All jobs pass at `8e6be85537bb480d84aa4fac049efc4efaf24639` | [CI run 36799584294](https://github.com/kibertoad/dark-sun-wake-redux/actions/runs/36799584294) |

Hosted results were re-read from GitHub, including job steps. Between that
tested revision and the audited checkout only HANDOVER.md and
TEMPLATE-ADOPTION.md changed. Hosted jobs cover Windows, Linux and both macOS
architectures, documentation, Windows install/shortcut/platform smoke/uninstall,
Debian layout/smoke and macOS package builds. These are existing successful jobs,
not a newly dispatched workflow or a signed-release test.

The sandbox's initial canonical attempt failed at NuGet access. Re-running with
authorized development-tool/network access passed. PowerShell used a process-only
execution-policy override; no global Git or execution-policy setting changed.
The successful gate's generated log is the authority for current test totals.

## Initial audit gaps (resolved or dispositioned above)

| Capability | Observed gap | Acceptance needed |
| --- | --- | --- |
| One-time bootstrap/version gate | Bootstrap-Project.ps1 is absent. Configuration lacks original.latestOfficialVersion, analysisVersion, patchStatusEvidence and patchStatusEstablished. SOURCE-EDITIONS records version 1.1, but that alone is not the template's enforced latest-version record. | Adapt the bootstrap gate and regression tests; populate only established provenance, version and fingerprint facts. Reuse an existing conclusive decision if present rather than repeating historical research. |
| Signing infrastructure | Install-CodeSignTool.ps1, Invoke-ESigner.ps1 and Invoke-GpgSigner.ps1 are absent. Release workflow lacks signed_release, release-signing and Authenticode verification. | Adopt and synthetically validate signing adapters and workflow contracts, or record an owner-supported scope exception. Actual release/signing remains a separate owner action. |
| Dependency reproducibility | Extractor packages.lock.json is absent. | Adopt the appropriate dependency-lock contract and prove locked restore on a content-free worker. |
| Complete validation entry point | Invoke-Validation.ps1 and Test-TemplateInfrastructure.ps1 are absent. Test.ps1 passes but does not check the missing infrastructure. | Adapt golden infrastructure acceptance into the canonical gate and CI, and preserve applicable serialized validation, Release build and test-count controls. |
| Guarded analysis exports | ExportEditionAnalysis.java, ExportFunctionAddressCorrelations.java, ExportVersionTrackingAddressContexts.java and ExportVersionTrackingMatches.java are absent. | Adopt guarded local-only exporters with public/synthetic API acceptance, or document capability-equivalent replacements or justified edition-specific exclusions. Never commit broad original-derived exports. |
| Root launcher contract | Golden check expects Start *.bat; project uses play.bat. The difference is also functional: play.bat lacks where dotnet, smoke-mode bypasses and argument forwarding via %*. | Preserve the project launcher identity while proving dependency diagnostics, argument forwarding and content-free smoke paths; avoid source extraction for assetless smoke. |

The raw golden check also reports required workflow markers and each missing
configuration field separately. Its log is retained without converting every
filename mismatch into an independent gameplay defect.

The wider file comparison found additional Ghidra helpers and PE-oriented
Inspect/citation helpers absent. Their mere absence does not prove a required
Dark Sun capability is broken: the owned executable is DOS MZ/FBOV, and local
Inspect, overlay mapping, inventory joins and bounded reporters are specialized.
Full adoption still needs an explicit capability disposition for those helpers;
this audit does not claim their replacement coverage has passed. No original
was opened, run or emulated to perform this check.

## Retained adaptations and remaining validation limits

Keep configured identity, XXH3 source contracts, licensed-source manifests,
transactional extraction, required-pack revision enforcement, local overlay
inventory paths, vendor/upstream snapshot placement and owner-only DOSBox policy.
The solution build and synthetic architecture/content tests pass for these
boundaries; template placeholders and PE-specific scaffolding must not overwrite
the configured project.

Mixed/high-DPI original-window capture remains owner validation. Synthetic
capture acceptance does not prove original-screen pixel parity. Survey,
game-specific research requests and unfinished gameplay parity are separate
from this migration verdict. No original-game session, capture, signed release,
release publication or installer execution on this local machine was performed.

## Exit for full acceptance

Disposition every missing capability above in the implementation plan, migrate
authorized generic infrastructure with synthetic tests, and have the adapted
golden infrastructure gate pass alongside Test.ps1, full build, assetless
publish/smoke and applicable hosted installer checks. A new pin alone cannot
close this audit. Preserve historical adoption results and record the broader
acceptance result explicitly.

## Engine 6.2.0 adoption, 2026-10-04

Engine 6.2.0 is adopted with reader 2.0.0/protocol 3. Exact source integrity,
published-suite and Dark Sun argument-width acceptance plus Engine upgrades
migration checks are recorded in TOOLKIT-RESPONSE-ACCEPTANCE.md. Other pins
remain unchanged. Full Gap 35 acceptance remains separate.

## Reader 2.1.0 / engine 7.3.0 adoption, 2026-10-04

Published releases are adopted with exact registry locks and installed-file
integrity verification. The published engine suite and Dark Sun frame/scope,
full consumed-argument and retained runtime-mode controls pass. Detailed source,
migration and acceptance qualifications are in TOOLKIT-RESPONSE-ACCEPTANCE.md.
Gap 35 closes; issue 145's composition exit passes; Gap 40 and issue 143 remain
open for their separate whole-query contracts. Prepared protocol 3, checker,
.NET packages, configured template and vendored rules stay unchanged.
