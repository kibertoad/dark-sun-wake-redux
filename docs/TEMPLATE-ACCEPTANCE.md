# Template and toolkit acceptance audit

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
