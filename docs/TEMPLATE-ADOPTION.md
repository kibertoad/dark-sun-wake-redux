# Merged evidence workflow adoption

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
