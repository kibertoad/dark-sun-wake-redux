# Merged evidence workflow adoption

Current full-template acceptance is recorded in [TEMPLATE-ACCEPTANCE.md](TEMPLATE-ACCEPTANCE.md).
The 2026-10-01 audit found older infrastructure gaps, now addressed with
configured adaptations and local acceptance. The current migration record in
TEMPLATE-ACCEPTANCE.md supersedes historical version and prerequisite statements
below; project-config records established latest-patch provenance and SHA-256.
Historical completion statements describe their incremental adoption scope.

Source: refurbished-dinosaurs-template `e698e5b`, incorporating PRs 19, 21 and 22.
Checker: refurbished-dinosaurs-toolkit `1560349d2e7430fd6b1b229b0b52f3782d08d3b0`
(PRs 10 and 11). Standard remains v1. Upstream licenses accompany the exact copies.

| Capability | Adoption |
| --- | --- |
| Relocations, fixups, incoming calls and bounded tables | Shared `tools/evidence/`, synthetic tests and usage guide |
| Flow boundaries and inventories | Shared flow exporter and transactional inventory exporter |
| Memory, effects, arithmetic, progress and hardware | Conditional research review and worked synthetic examples |
| Offline authority and checker | Verified `vendor/` snapshots, explicit refresh, matching CI pin |
| Efficient configuration tests | Git-visible scratch copies, mutation-sensitive snapshots and configured-project support |
| Branch logic in Core | Agent and implementation procedures; no gameplay refactoring in this tooling batch |

Local adaptations: immutable docs live outside the game-doc citation scan; the
existing Test.ps1 gate, XXH3 source identities, owner-only native runtime rules,
FBOV mapped-image tooling and committed inventory paths remain in force. The
specialized join is retained because it converts the established overlay import,
which the generic join requires to be canonicalized first. No original is run or
read to validate this migration. No evidence status is promoted.

Review guidance requires researchers to record claim-dependent contracts; it does
not claim to infer every effect, pointer identity or loop proof automatically.
Individual gap closure requires checking the whole request against delivered
behavior. A passing tool test or added review guidance does not establish that
every related request is complete. The original requests remain in `gaps.md`.

## Verification and gap disposition

The adopted gate passes: 31 synthetic Node tests, 700 .NET tests, repository and
configuration checks, and 615 spec entries / 158 parity rows / 5 deviations.
The full solution builds without warnings. Both adopted Ghidra scripts compile
against the installed public Ghidra 12.1.3 API; no original executable was opened. The new checker required correcting
20 EOF range endpoints in 12 findings to the final valid byte from the existing
manifest; no descriptions, evidence or statuses changed.

The original cleanup overstated completion: it removed 39 entries by mapping
them to ten priority groups. That was not an acceptance audit of each request.
The original requests have been restored. The delivered tools and documentation
remain useful, but an entry is removed only after its full requested behavior is
verified. In particular, review guidance does not implement an automated variable
use, argument, effect, guard, or capacity reporter.

## Selected top ten: delivered scope

| Priority | Delivered | Remaining limit |
| --- | --- | --- |
| 1. Canonical locations and relocation provenance | MZ/FBOV operand mapping and the v1 executable location rules | The checker validates file bounds, not every documented mapped-code range; no general banked-format support |
| 2. Function boundaries and complete readings | Explicit-edge flow review, ownership warnings and overlapping starts | Exported overlay ownership, operand decoding and complete readings still require researcher verification |
| 3. Memory identity and arguments | Conditional checks and synthetic worked examples | No automatic effective-segment or caller/callee argument reconstruction |
| 4. Path effects and failures | Review requirements for ordering, guards, cleanup and failure paths | No automatic guarded effect or alias-aware call summaries |
| 5. Trustworthy negative findings | Controlled relocated far-call search, exclusions and queue decomposition guidance | No entry-based variable-use inventory; near-call search is excluded |
| 6. Capacities, arithmetic and caller ranges | Bounded numeric table reader and review examples | Input transformations, producer capacities and caller ranges are not inferred by the reporter |
| 7. Static and emulated limits | Hardware/indirect-flow warnings and progress guidance | No automatic termination proof or recursive error-origin tracing |
| 8. Portable honest coverage | Transactional exporter, explicit view joins and portable destination generation | Existing inventories are not automatically audited against manifest identity; raw exports allow only start/size |
| 9. Superseded ownership | Merged checker excludes superseded rules from active procedure ownership | This fixes the ownership bug; it does not close other research-tool requests |
| 10. Offline authority | Exact licensed snapshots, digests and explicit freshness/refresh commands | Offline integrity does not prove upstream freshness or enable offline .NET restore |

The task addressed these ten priorities at the documented scope. It did not
fully resolve 39 detailed tool requests. Further reporter work must be scoped
and planned separately; the restored entries preserve its acceptance criteria.

## PR 17: local rules and bounded section reading

Adopted `18a67f38dc48476d0af1e5fedfc6c592cb6fdfef`: the methodology joins the
exact Standard v1 and Protocol snapshots. Local pages are assumed current for
tasks; update checks and refreshes happen only at the owner's request. Section
links state checked line ranges, and the link checker detects external rule-page
links, misplaced paths, missing headings and missing/stale ranges. Skills open
only needed sections and reuse context. Canonical summaries now link to the
local definitions; blank entry forms and local runtime exceptions remain.

This workflow adoption does not close additional gap entries.

Verification: all six immutable snapshot digests match their pinned source;
local link checks and 34 Node tests pass, including missing/stale ranges,
misplaced paths, absent headings, published-page links and range rewriting.
The configured project?s `tools/Test.ps1` passes all 700 .NET tests and the
615-entry documentation check. No rules-page network read is part of the gate.


## PR 24: restored rules and refreshed local authority

Adopted `9349a89280de5f5f86a8f1b803d43cb1ad7575d0` from template PR 24.
Detailed planning, evidence/status, fidelity, citation and entry-template
instructions are restored. The exact local rules are pinned to
`595fdbe8410a3f3bf76f6387d9d16c0e6fb3cb8f`; toolkit pin remains unchanged.
Snapshots retain their upstream bytes and licenses under `vendor/upstream/`,
and all section ranges are regenerated. Owner-only DOSBox access, the absent
emulator harness and existing coverage paths remain local adaptations.

Gap 6 is closed: the complete authoritative Protocol and Standard are locally
readable, their source revision and digests are recorded in the lock, and
`check-upstream` / explicit revision refresh are documented in
`docs/UPSTREAM-RULES.md`. Freshness checks remain owner-initiated. PR 24 supplies
no new reporters: all other requests remain in `gaps.md`, including those for
automatic effect summaries and mapped-range validation. Restored guidance is
not treated as implementation of those requests.

Verification: `tools/Test.ps1` passes with the repository-local PowerShell 7
runtime on PATH: 34 Node tests, 700 .NET tests, repository/configuration checks
and the 615-entry / 158-row / 5-deviation documentation check. The full solution
build passes with zero warnings and errors. No original content was read.


## Latest template: PRs 25 and 26

Adopted template main `8ed674008cd9ce2860b19cd6ac6a569d70d2bc03`.
The exact rules are pinned to `b923e85ae0319bf9e79781c7e09d46544ad9daea`
(as recorded by the lock), and the checker to
`c36182046f3eb71e6485163b502bc4950880aa24`, matching CI.
PR 26's recorded-run and draw-fixture requirements are adopted as procedures;
Probe remains none under the local DOSBox policy. No RNG hook or native probe
was implemented or claimed.

Gap 7 is closed for the supported MZ overlay edition: canonical offsets are
accepted and checked against an evidenced Code ranges row, not merely file
length. The build now lists all 98 installed/disc overlay ranges from the
existing FND-EXE-003 header procedure. A synthetic checker regression accepts
half-open endpoints and rejects holes and ranges crossing overlay boundaries.
Other executable formats retain their specified address conventions.

The other selected gaps (18, 21, 26, 33, 34, 36, 37, 40 and 42) gained explicit
review/harness requirements but still request reporter implementation. They
remain open. Gap 28 is already described as fixed in the earlier adoption
record, but this update does not supply its requested replacement-rule fixture
in this repository, so it is retained pending the full acceptance audit.

The refreshed Survey requirement exposes incomplete explicit listing/exclusion
evidence in the existing build. Q-EXE-003 tracks that re-audit; the checklist
no longer claims the revised Survey denominator has passed. Corpus extraction
and existing gameplay statuses are unchanged.

Verification: `tools/Test.ps1` passes with 35 Node tests and 700 .NET tests,
including configuration, snapshot/CI integrity, local links and the 615-entry /
158-row / 5-deviation documentation check. The full solution builds with zero
warnings or errors. Only bounded overlay metadata was reread by the existing
header procedure; no original process was launched.


## Merged bounded reporters: standards 26, toolkit 14 and template 27

Adopted template merge `3e8805ea474c60e7c3234213a108cb85a9e86265` and
toolkit merge `c2b21ee62fc404391e8dcfafd7029185f81241a9`. Exact rules now
name standards merge `94f8f678afb05171567f48d9fb19488e48309f12`. The
checker and CI action pin name the toolkit merge; its checker bytes are
unchanged from the previous pin. Standard remains v1.

The adopted source includes the final merge's alias invalidation, return-frame,
branch-assumption, canonical-target and shared-step-budget fixes, rather than
only the earlier PR branch versions. One pinned MZ/FBOV parser now supplies both
the lightweight commands and the instruction-derived commands. The existing
configured-project inventory joins and committed CD directory paths remain.

`tools/evidence/x86-lock.json` records exact source, license, documentation and
test digests. The canonical gate verifies the pin, runs the Python acceptance
suite and Node bridge/integrity suites, and proves configuration preserves the
reporter and guide even when their examples contain substitution tokens. CI and
release validation install the pinned Capstone dependency and exercise these
checks. Reports and configurations belong in GAME_DIR. No original program was
run or read for this adoption and no evidence status or gameplay changed.

### Individual gap audit

The revised standard explicitly keeps a game's reporter request open until its
own case passes. These upstream synthetic tests demonstrate capability; they
are not reports on the cited Dark Sun cases. The selected ten therefore remain
open with concrete reporters available for their next bounded research queries.

| Gap | Adopted capability | Remaining acceptance work |
| --- | --- | --- |
| 14 | Entry-based uses, matching controls, raw candidates and undecoded ranges | Run the cited two-field inventory with its established instruction hits |
| 21 | Effective segments, offset provenance and conditional alias identity | Check the cited caller/helper and mixed-segment filename paths; string operations are unsupported |
| 35 | Consumed near/far stack widths and LDS/LES pointer grouping | Verify the cited mask/callback/identifier grouping and forwarding |
| 27 | Ordered path effects, early returns, predicates and explicit external assumptions | Check the cited nested loader/cache/cleanup paths and all continuations; unknown callees stop or invalidate state |
| 26 | Full/partial returns, producer-scoped failure contracts and caller predicates | Verify the cited initializer, truncation and raw-field consumer cases |
| 36 | Complete byte intervals and distinct neighboring-byte producers | Run the cited byte stores followed by wider services and wrapper normalization |
| 13 | Full named-region scans, later callers, relative calls and canonical far aliases | Query the cited complete declared caller domains and known controls; computed/unrelocated targets remain excluded |
| 32 | Guard order, branch polarity and invalidation of reloaded targets | Check the cited metadata access, ignored rejection and intervening-callee reload paths |
| 42 | Request width/modulus, units, observed extent/pointer and later writes | Read and query the cited lower allocator/header contracts; repeated clearing is unsupported |
| 18 | Executed normalization/gates and declared dispatch-layout validation | Run the cited extended-bit decoder case with its evidenced table mapping/count |

Gap 28 is closed. `tests/upstream/superseded-ownership.test.mjs` uses a scratch
copy of this repository and restores the historical rule's original Procedure
fence to the ordinary declaration format while retaining its supersession link
and replacement. The adopted checker accepts the actual retained declarations
without duplicate active ownership, and leaves historical text intact. This
fulfills the requested replacement-rule fixture; no real spec entry is edited.
All other gaps are retained, including secondary requests for unsupported string
effects, recursive origins, progress proofs and general cardinality analysis.

Verification: 45 Python tests and 41 Node tests, 700 .NET tests, configuration,
repository policy, offline pins, section links and the 615-entry / 158-row /
5-deviation documentation check. Full solution build: zero warnings/errors.


## Follow-up: recorded game-case verification

The ten selected reporter requests were checked against their recorded cases,
with the adopted toolkit revision and a separately tested refinement candidate.
See [the full case audit](REPORTER-CASE-AUDIT.md) for inputs, expected controls,
observations, conditional models and remaining limits. Gap 18 passes against
the actual adopted source and is closed. Gaps 14 and 19 pass with the candidate
fixes but remain open pending adoption. Other requests remain partially verified
or unsupported; no group closure is claimed.

Upstream review: toolkit PR 16, template PR 28 and standards/protocol PR 27.
This session does not change the game's reporter/rule pins or any evidence
status. Original-derived reports and configurations stay in GAME_DIR.


## Revised main adoption, 2026-09-30

Fully adopted website `3b4e6fcfca887620cdf13c8a8e62f9ca53133d60`, toolkit `926e287a4134512d59fe021efe6507c933da03f1` and template `b9f542549840cf7ce2d254a8f9f7bf0f502daaa7`. Reviewed the complete main-to-previous-adoption delta, including revised PRs 27/16/28, PE32 loading/reporting, raw-backed executable extent checks and separate conditionalAccesses with named dependencies. All changed reporter source, guide, license and tests are pinned exactly. The template sync mapping and Python test discovery include PE files. Local configured paths and owner-only original runtime constraints are retained; no gameplay or spec/parity statuses change.

Rules/checker lock and CI action pin now name current main, with unchanged checker bytes and Standard v1. Local section ranges verify. The recorded cases were rerun against the exact adopted source: gaps 14 and 19 now close, gap 18 still passes, and 37 requests remain. See REPORTER-CASE-AUDIT.md for conditional observations and remaining limits.

Validation: 92 Python, 41 Node and 700 .NET tests pass, together with repository/configuration policy, pin/section verification and the 615-entry / 158-row / 5-deviation documentation gate.


## Latest template PRs 31/32 adoption

Adopted template main c048c63523a1b061b5325f6b05055d98819780d7 after reviewing the full delta from b9f542549840cf7ce2d254a8f9f7bf0f502daaa7. PR 31 clarifies that plans document authorized work and places changing narrative totals in generated reports, preserving quantities that support evidence or constrain behavior. PR 32's revised source adds requested page counts, capped ambiguous-name indices, policy-configured denied diagnostic names and heap-dump exclusion/redirection. Upstream regression files are copied exactly; the Ghidra category and repository checker retain configured-project adaptations. No methodology/checker/reporter pin changes are present in this template delta; inventory PR 33 work remains separate.

Canonical Test.ps1 and full solution build pass; generated output is in artifacts/latest-template-validation.log and artifacts/latest-template-build.log. The merged map script compiled against Ghidra 12.1.3 and again passed the actual large-map page/name checks in read-only mode; gap2-merged-adoption.log remains GAME_DIR-only. Current policy regressions prove force-staged heap-dump and diagnostic rejection plus ordinary-log acceptance. Merged gaps 2 and 38 are closed; no game spec/parity claims changed.

## Latest merged inventory refinements

Adopted template e0325e0b063735e94b7e3ac94b0b8b89d0a38a79 (PR 33), including canonical start formatting, rejection of analyzer default names, and input-path agreement. Shared source and acceptance tests match the merged revision. Configured identity and the historical disc inventory path remain intact; gap 5 still requires its distinct disc source.

## Complete upstream migration, 2026-10-01

Adopted website 82deb767ab64ca9922bb6347d66d9856b7640e91, toolkit 7da1b93cdd9ac0d59dbaf82b66b4db95d578ab9d and template 7b3bbe46b251b163ee02a6539ac0d81559dbe921. Reviewed the full template delta from e0325e0: capture worker and synthetic tests, research tracking and question links, file-data/unpacked locations and checker/CI pins, instruction-owned operands, strings/saved flags/local IRET, and revised contested overlap traversal. Reporter files/tests/guide are exact pinned upstream bytes. Skills, entry templates and validation guidance retain configured paths and owner-only runtime; capture defaults use this game title. Existing queue IDs and research conclusions are preserved, with missing structural links and one existing AI question newly tracked. Generic documentation example IDs use placeholders. Empty upstream RNG/SAVE queues are already present locally; template handover and plan histories are not substituted for game records. No architecture, packaging or gameplay delta exists in these revisions.

Actual width, helper effects and instruction operand controls pass against the exact adopted source. Validation output is in artifacts/full-upstream-migration-validation.log and artifacts/full-upstream-migration-build.log. Mixed-DPI original capture remains owner validation, not claimed by synthetic acceptance.

## Migration acceptance audit, 2026-10-01

The requirement-by-requirement audit found CI still selected only test_x86.py and omitted adopted tracking/capture regressions. The audit also found CI's old remote policy action lacked the adopted diagnostic-filename protections; CI now invokes the local repository checker. CI now runs all reporter suites and the same synthetic evidence/upstream tests as the canonical local gate, and requires research tracking. The canonical gate passes again. Assetless Release publish and smoke-test exit successfully with no UserContent directory. Exact pins and source bytes were audited against the adopted revisions; configured owner-only runtime remains intact. High/mixed-DPI original-window capture is an explicit upstream limitation, not covered by this machine's synthetic acceptance. Final hosted acceptance passes at code revision `8e6be85537bb480d84aa4fac049efc4efaf24639`: [CI run 36799584294](https://github.com/kibertoad/dark-sun-wake-redux/actions/runs/36799584294), including documentation, Windows/Linux/Intel macOS/Apple Silicon verification, assetless publish/smoke, and all installer jobs. The final local canonical gate also passes (`artifacts/migration-final-acceptance.log`). No unfinished migration changes remain; remaining game-research requests stay tracked separately in `gaps.md` and `queue/`.

## Call-target, bounds and owner reporters and code-comment addresses, 2026-10-01

Adopted template `8d0eef35ec8f3b1053ba1dd129a7bd75b044cf27` after reviewing the full delta from `7b3bbe4`: template PRs 39 (addresses in code comments, pre-commit hook, `tools/Invoke-NodeChecks.mjs`) and 40 (reporter and rules adoption). Rules name website `ca39d0750e67c8c3900e8554e66a84083fe67452` (website PR 31: call-target, boundary and ownership contracts, format-table controls, incoming-call coverage). The checker and CI action pin name toolkit `f7da1328b2adc0a45865de45b35d42c88afdbc6b`; the reporter pin names toolkit `313bb7d8baa238b5934e5ddf3f60cb2d5f4887ed`, which holds toolkit PRs 28 to 32. Both locks match the template's exactly after path mapping. Standard remains v1.

Configured adaptations: the vendored rules stay under `vendor/upstream/` and section links were rewritten to the new line ranges. `tools/Test.ps1` runs `tools/Invoke-NodeChecks.mjs` in place of its separate documentation, research-tracking and reporter-pin steps; `tools/Invoke-Validation.ps1` keeps this repository's structure and runs Test.ps1. Two template assertions in `tests/upstream/upstream.test.mjs` were adapted: this repository's CI step passes `references: docs`, and the shared node checks are run from Test.ps1. The CI step keeps `references: docs` and the working-file size check. DSUN.EXE is MZ, so no `images` input is set and plain `0x` comment values are not checked; no code comment uses an `fn_`/`g_` neutral name.

Gate: `tools/Invoke-Validation.ps1` passes with repository-local PowerShell 7 (137 Python, 77 Node and 700 .NET tests, documentation check of 615 entries / 158 parity rows / 5 deviations, Release build, assetless publish and smoke). Log: `artifacts/reporter-provenance-adoption.log`. The play-launcher test needs `NoDefaultCurrentDirectoryInExePath` unset for its `cmd.exe` child; with it set it fails identically on the previous commit.

No gaps.md request closes. The new `target`, `bounds`, `owner`, incoming-coverage and path-model capabilities have synthetic acceptance only. Their Dark Sun cases (gaps 8, 10, 11, 13, 21, 22, 23, 26, 27, 35, 39 and 42) need executable analysis, which the facts gate in docs/SOURCE-EDITIONS.md blocks until latest-patch provenance and the baseline SHA-256 are recorded. No original program was read or run.


## Merged dispatch and pointer inventory adoption

The exact reporter pin is toolkit b87064216317eeee0ac991a8b154d5b58a46534c,
merged PRs 33/34. New dispatch and pointer modules and tests are included in the
hashed mapping. Full gap 13 source acceptance, capped/partial negative controls
and canonical validation pass; REPORTER-CASE-AUDIT records the scope. This closes
the bounded inventory request, not player reachability or universal absence.


## Final explicit offline rerun adoption

Template PR 41 merged as bd3d9a381cae7d3f015e9c089f2fe134e8a0a857.
The configured Test.ps1 and Invoke-Validation.ps1 expose -NoRestore; default
validation and CI retain restore. All checks, filters/count controls and
serialization remain intact. The final portability, scratch cleanup and
nonvacuous test refinements are adopted with this project's Test.ps1 route.
The production host block passes fallback and caller-environment restoration
controls. Final normal and offline gates pass; logs
artifacts/offline-pr41-final-normal.log and artifacts/offline-pr41-final-offline.log.
The latter uses unreachable HTTP proxies and no restore fallback. Documentation
states existing restore state is a prerequisite and does not promise cached
freshness. The whole gap 4 request passes and is removed. No gameplay changes.

## Merged reporter adoption, 2026-10-01

Template PR 42 merged as beaade054a3c125206166771b8a632ea745dc33e,
pinning toolkit c133cd48bfe6cc3cb7126616996e7d548982a068 (PRs 35?37).
Adopted exact reporter source, tests and guide; retained configured validation
routing and removed the obsolete PYTHONPATH mutation. The merged test suite
selects the vendored import path itself. Offline validation from PR 41 remains
explicit and preserves every required check. No standard snapshot refresh.

## Reviewed operand candidate adoption, 2026-10-01

Toolkit PR 38 merged as 1ef21ef46567dd108ea082a0a493f7024ba79a07.
Exact source, tests and guide adopted, including review refinements for encoded
literals, repeated prefixes and distinct overlapping starts. Configured source
controls and clipped/rejected controls pass. Template pin update is proposed
separately; game behavior and local standard snapshots are unchanged.

## Reviewed callee graph adoption, 2026-10-01

Toolkit PR 39 merged as 8853eb0e9a3542ef3a5c2864b25bd95b5201ae75.
Adopt exact source/tests/guide, including breadth-first shortest-depth traversal,
shared per-node summaries, cycle-path validation, shared-tail overlap handling
and tightened positive controls. Template PR 43 already carries the subsequent combined final pin.

Toolkit PR 40 also merged: final exact pin
67340fcb975449600c160ef5a4995119d4e8f127 includes reviewed near-pointer
provenance, dereference registers on string/XCHG/far-load accesses and bounded
newest-formation retention. Adopted exact source/tests/guide; template PR 43
already carries both merged graph and provenance revisions at commit
5ee4f81e7296bdc27a6425056600f1bc53acbe7f. Its applicable CI is green;
the PR merged as 79d18a20cb4d97c7153e74e5695cbb80e7ebf73e. The merged tree
is identical to that reviewed update (b2ef1cbe9c292997c2fd20b3460c54f9568d7a6a),
so the already adopted configured changes need no further code delta. Local
duplicate update discarded after verification.

## Published toolkit package adoption, 2026-10-02

Template PR 43 is already adopted at merged tree 79d18a20; no duplicate template
update is needed. Toolkit PR 42's registry delivery is now available: engine
0.1.0 and npm executable-reader/standard-checker 0.1.0. Exact archive locks replace
copied shared implementations and their sync locks. CI, hooks, configured wrappers
and Ghidra paths use package entry points; project-specific tools remain local.
The methodology, Standard v1 and protocol snapshot bytes remain unchanged.

Normal and NoRestore configured canonical gates pass, including Release build and
assetless publish/smoke. Existing local-only source controls pass through the
installed engine, preserving hash checks, rejected controls and unresolved paths.
Logs are under artifacts/package-delivery/. Toolkit PRs 41 and 43 remain open;
their candidate features are not included in these adopted package releases.

## Latest reviewed toolkit adoption, 2026-10-02

The owner's latest-toolkit request advances the CI action to reviewed main
98395df03fab3990bca3a3de5cb1dcd1ae0df3a3 and the engine to published 0.7.0.
Reader 0.2.0/checker 0.1.0 remain latest; their exact npm locks are unchanged.
Engine/Capstone/pypcode are hash-locked runtime dependencies. Every wheel source,
sdist source/test and installed engine file matches the release archive/tag.
Public-index uncached hash-locked wheel download and dependency rejection tests
pass. Source regressions and limits are recorded in REPORTER-CASE-AUDIT.md.

PR 60 is reviewed and delivered; the installed conditional-target API passes a
synthetic prefix-write/return and capped-route check. Its former original MENU
candidate positive does not pass the reviewed shared budgets: capped ordinary
paths and broad output limits remain explicit. Gap 27 stays open. PR 63 merged
only its plan/tests, not the local scoped-memory implementation. Engine 0.7.0
contains the reviewed backend seam/pypcode phases; its default remains handwritten.
No native behavior, hardware validation or full gap closure follows from adoption.
Template and pinned local rules are retained; no snapshot refresh or release was
performed. Canonical configured validation is recorded in VALIDATION.md.

## Latest toolkit pypcode cutover, 2026-10-02

During the final latest-version check, reviewed PR 66 delivered engine 0.8.0 at
592088dbdb1cc8d804ba853eb39ae6ca1215577f. This supersedes the current version and
backend disposition in the preceding adoption record: handwritten semantics are
removed and pypcode is the sole instruction backend. CI action/engine pins are
advanced; reader/checker and exact Capstone/pypcode locks stay current. Every
shipped source, archive source/test, installed source and source inventory matches
the release tag. Installed synthetic dependency/conditional-target controls and
retained original static controls, including the conservative linked-child limit
case, pass. Qualifications and full gap contracts stay open; no native outcome
or parity claim follows from the backend cutover.
