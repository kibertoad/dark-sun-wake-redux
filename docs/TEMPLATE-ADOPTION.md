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
