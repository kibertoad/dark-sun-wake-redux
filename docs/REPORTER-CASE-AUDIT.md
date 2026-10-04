# Dark Sun reporter acceptance audit

## Source and scope

2026-09-30. BLD-GOG-EN-1.1, the stable owned DSUN.EXE baseline in
`docs/GHIDRA.md`: 634,416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. The recorded path and current size/metadata
were checked before querying. Each reporter also checks its SHA-256 input guard.
No original process, DOSBox, emulated function or native observation was used.
No spec or parity status changed.

Initial adopted reporter/checker (historical; current pin is recorded in the latest batch below): toolkit `926e287a4134512d59fe021efe6507c933da03f1`. Website rules: `3b4e6fcfca887620cdf13c8a8e62f9ca53133d60`. Template: `b9f542549840cf7ce2d254a8f9f7bf0f502daaa7`. The initial candidate was `2b688e66ba34ee25d9882ea4938f95b5461b0ba4`; revised upstream changes are now fully adopted. Python 3.14 and Capstone 5.0.7; Standard remains v1.

Configurations and raw JSON reports live under `GAME_DIR/analysis/reporter-audit/`
and are not committed. `run.mjs` records the initial cases; `verify.mjs` checks
controls and conditional continuations; `verify-adopted.mjs` repeats the dispatch
check with the actual adopted source. The case names below identify their JSON
configurations and corresponding `.report.json` files in that local directory.
The summary here records tool acceptance, not new claims about the game.

## Ten selected requests

| Gap | Case and expected control | Observed result and disposition |
| --- | --- | --- |
| 14 | `gap14-gate` / `gap14-code`, established entry and the two reads in FND-CONFIG-108 | Adopted effect traces stop before the known reads. The current adopted source finds both as conditionalAccesses with named stops/untraced callees, correct widths and unresolved effects; it retains undecoded ranges and rejects an offset-one bad control. Close the bounded discovery request, without claiming values or unconditional execution. |
| 13 | `gap13-selector` / `gap13-setup`, eleven selector calls in FND-CONFIG-101 and six setup calls in FND-CONFIG-111 | Named overlay-domain scans recover all eleven and six encodings. Entry CFG confirms seven and two respectively; the remaining four in each inventory stay raw candidates because of unresolved dispatch routes. The FND-CONFIG-128 same-segment query recovers its later relative-call candidate, also unverified. Positive controls cannot be accepted for these missing paths. Keep open. |
| 18 | `gap18-normalization`, FND-CONFIG-137's verified selection continuation after the byte reader returns | Both adopted and candidate dispatch reports map the recorded normalized/extended pair to position 12 and raw target 3351, with decoded transformation, range gates and the evidenced 99-word layout. Out-of-domain examples remain unresolved without an invented selection. Close this reporter request; no general expression-reader or native-input claim. |
| 21 | `gap21-caller` / `gap21-helper`, FND-CONFIG-143 and FND-CONFIG-145 | Candidate preserves SS-based formation versus DS-based consumption and explicit unresolved aliases. The helper can finish only under a declared returning-copy model; caller loops and mixed-segment filename dependencies in FND-CONFIG-181 remain unverified. String/repeat operations are unsupported. Keep open. |
| 35 | `gap35-register` / `gap35-consumer`, FND-CONFIG-179 and FND-CONFIG-180 | Actual callee reads retain a far window, word identifier, far callback and word mask at distinct consumed offsets/widths. Setter continuations require explicit return/preservation assumptions; the full caller reaches a path limit. Candidate fixes far-frame handling, but the full caller-to-forwarding acceptance remains incomplete. Keep open. |
| 27 | `gap27-loader`, FND-SCRIPT-019's bypasses, loader writes and transfer failure | Early exits stay distinct. With the near-entry return frame correctly configured, unread calls and bounded age loops still prevent complete ordered transfer/rollback summaries. No unchanged-state or transactionality conclusion is accepted. Keep open. |
| 26 | `gap26-initializer` / `gap26-frame`, FND-CONFIG-156 and FND-CONFIG-190 | Widths and producer-specific contracts remain visible, but initializer loops/alias-sensitive return provenance and frame-pointer carry rotation stop the required caller chains. No success conclusion is inferred from a nonzero byte. Keep open. |
| 36 | `gap36-bracket`, FND-CONFIG-187 and FND-CONFIG-188 | Byte and word widths remain distinct, with missing producers explicit. Callee boundaries and alias-sensitive returns prevent full propagation into normalized wrapper results; modeled calls invalidate memory rather than preserve an unwritten neighbor. Keep open. |
| 32 | `gap32-metadata` / `gap32-reload`, FND-CONFIG-165 and FND-CONFIG-171 | The metadata read appears before the null guard; all wrapper paths reach the same runtime request and conditional zero result. Candidate also reports the full far-indirect reload and a changed guard identity after an unread bracket. Modeled continuation/path limits leave the wider layered contract partial. Keep open. |
| 42 | `gap42-wrapper` / `gap42-runtime`, FND-CONFIG-209 through FND-CONFIG-211 | Candidate gets past XCHG; multiplication, carry/counter operations, split-word request representation, repeated clearing and lower heap dependencies remain unsupported/unresolved. Requested quantity, header extent and native capacity are not equated. Keep open. |

Conditional queries explicitly assume a returning call with balanced encoded
frame and the named preserved registers. They invalidate all memory and flags;
AX/DX results remain unknown. These are conditional local continuations, not
evidence of callee preservation, successful release or actual failure. The
audit never uses a modeled return to remove a missing-producer condition.

## Related requests

Gap 19: `gap19-reader` and `gap19-advance` check the three conversion sites
in FND-CONFIG-138. The candidate reports AL-to-AX, 16-bit effective size and
explicit decoder-mnemonic mismatch. Prefixed controls use entirely synthetic
instructions. The current adopted source passes all three recorded conversion controls. Close this conversion-reporting request; the advancement body still stops at an out-of-region call.

Gap 39: `gap39-frame` reaches the DS-based indexed stores in FND-CONFIG-198
under explicitly conditional guard/request returns, retaining BP-derived offset
provenance. The first loop stops at the repeated-instruction bound before the
second loop's reads. Stack reservation is not accepted as DS buffer capacity.
Keep open pending that full case.

Gap 40: `gap40-cleanup` exposes path-specific writes and unknown frame bytes,
but prior image/handle calls and the path cap prevent exhausting all cleanup
predecessors in FND-CONFIG-199. Keep open; no native defect is inferred.

## Upstream refinements and validation

- [Toolkit PR 16](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/pull/16)
  preserves entry-CFG operand observations after unresolved callees, independent
  unknown flag producers, push-CS/near-call frames and explicit modeled widths,
  far-indirect guard provenance, effective-width conversions, XCHG and low-result
  IMUL. Output exhaustion names its 32 MiB bound. Tests contain constructed bytes.
- [Template PR 28](https://github.com/kibertoad/refurbished-dinosaurs-template/pull/28)
  pins that exact source, guide, license and acceptance suite.
- [Standards/protocol PR 27](https://github.com/kibertoad/refurbished-dinosaurs/pull/27)
  clarifies operand-versus-effect reports, independent unresolved flag producers,
  encoded far frames and the contents of a durable case-verification record.

Toolkit: 55 Python tests; 121 Node tests (120 passed, one case-sensitive-file-system
skip on Windows); 17 .NET tests and repository policy pass; build has no warnings
or errors. Template canonical gate: 55 Python, 39 Node and 56 .NET tests pass.
Website: pnpm check passes. Actual source cases above were separate local static
checks; upstream synthetic success did not determine gap closure.

All three PRs' applicable CI checks pass. Dark Sun's existing pinned gate also
passes: 45 Python, 41 Node and 700 .NET tests, with repository, documentation,
configuration and upstream-pin checks passing.

## Revised main adoption rerun

The local verification script now imports this checkout's pinned reporter and checks the reviewed conditionalAccesses schema: the two known reads name their stop dependencies rather than masquerading as traced effects. Incorrect controls still fail. Dispatch and all three effective-width conversions pass again. Conditional continuations for the other cases were rerun with unchanged unresolved/cap limits; they do not close those requests. No original process or emulated call ran. Earlier validation counts above describe the initial PR batch.

## Goal batch: numeric table and pointer controls

2026-09-30, current pinned tooling. Local verify-simple.mjs and gap12-tags/targets/extra configurations and reports are in GAME_DIR/analysis/reporter-audit. FND-CONFIG-096's evidenced two four-byte tags and separate two-word target table match the known tags and canonical targets. An attempted third entry is rejected with limit 2 derived from the loop, and no ASCII fallback is used. The tool requires the researcher to supply the evidenced count/limit; it does not infer the loop. Gap 12 is closed.

FND-CONFIG-144's two pushed far-pointer segment operands retain raw tokens, declared fixup membership, decoded descriptors 113/116 and mapped segments/offsets. These controls pass; gap 16 remains open pending the segment-load and stored-pointer cases. No new game claims or runtime observations.

## Goal batch: bounded maps and JVM diagnostics

Template PR [32](https://github.com/kibertoad/refurbished-dinosaurs-template/pull/32), commit 686f643, supplies shared memory-block selection and diagnostic policy. Local tools use the same implementation, with the configured script category retained. Gap 2's actual retained Ghidra 12.1.3 mapped project reports total 3,546, page 3538/count 16 emits eight blocks with partial scope, exact HEADER emits one block, missing name and oversized default queries fail. Logs gap2-page.log and gap2-controls.log remain GAME_DIR/analysis/reporter-audit. The project was opened read-only with no analysis or original execution; no bytes/code were exported.

Gap 38's scratch-Git test proves ignored root/nested JVM basenames, force-staged diagnostic rejection (including case variants), and ordinary-log acceptance. Java acceptance uses an entirely constructed 3,546-block map and verifies empty/clipped/capped pages, exact/ambiguous/missing names, invalid ranges and arguments. Both behaviors are implemented and verified locally; the upstream requests stay open pending PR merge. Root gate passes 92 Python, 43 Node and 700 .NET tests; template passes 92 Python, 41 Node and 56 .NET tests and a zero-warning/error build.

## Goal batch: actual inventories and committed identity

verify-inventory.mjs under GAME_DIR/analysis/reporter-audit checks the retained original/mapped Ghidra exports against the adopted shared join. Canonicalization uses the existing documented mapped-import formula, retaining explicit exclusions: 97 rows outside source ranges are excluded before the join. The shared exporter was compiled/executed on the read-only retained mapped project, without new analysis, and its 2,723-row output exactly matches the historical export. The join accepts 1,284 original resident and 869 mapped overlay starts, producing the committed 2,153-row TSV exactly. Conflicting view ownership, duplicate/invalid starts, invalid sizes and unmapped starts are rejected. Gap 3's view-specific coverage request passes and closes. Counts remain analyzer-discovered starts, not whole-program coverage.

Template PR [33](https://github.com/kibertoad/refurbished-dinosaurs-template/pull/33), commit 85175d9, adds committed inventory-check identity/destination verification. The actual installed 2,153-row TSV passes; synthetic CLI and schema checks reject wrong prefixes/destinations, numeric aliases, malformed columns/counts and unevidenced legacy paths, while optional empty text cells remain valid. Gap 1 is delivered/verified locally pending upstream merge. Gap 5's portable destination and legacy disc prefix checks pass, but validation against the separately owned disc executable is still pending; no installed-source result is substituted for that case. Raw exports/canonicalization summaries/configurations remain GAME_DIR-only.

Root gate: 92 Python, 44 Node and 700 .NET tests pass. Template canonical gate: 92 Python, 40 Node and 56 .NET tests pass, build zero warnings/errors.

## Merged template PR 32 acceptance

Reviewed and adopted the final merge c048c63523a1b061b5325f6b05055d98819780d7, including review-time requested-count headers, bounded ambiguity indices and policy-driven heap-dump protections. The revised synthetic tests pass without skips. The actual retained mapped project again reports the expected tail page and exact-name result, with requested count explicit; its read-only log is gap2-merged-adoption.log under GAME_DIR. Both entire requests 2 and 38 now have upstream delivery and local acceptance evidence and are removed from gaps.md. Earlier pending-merge notes above are historical.

## Merged template PR 33 acceptance

Adopted final merge e0325e0b063735e94b7e3ac94b0b8b89d0a38a79. Exact merged source and synthetic tests pass, including canonical address formatting, analyzer-name rejection and input-path agreement. The retained installed-source inventory export/join and committed checker pass again. Canonical validation and full build pass; logs are artifacts/template-e0325e0-validation.log and artifacts/template-e0325e0-build.log. Gap 1 is removed. Gap 5 remains open for distinct disc-source verification; earlier pending-merge notes are historical.

## Bounded string and saved-flags candidate

Toolkit PR [19](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/pull/19), commit 247e30c8cbdf9448901d39891512fb9c62364ae5, and template PR [34](https://github.com/kibertoad/refurbished-dinosaurs-template/pull/34) provide sequential bounded MOVS/STOS/LODS, conditional direction provenance and intact saved-flags restoration. Synthetic segmented and flat controls pass; toolkit policy, bridge/documentation checks, full build and .NET tests pass. The one skipped toolkit test requires a Linux case-sensitive filesystem. Template canonical validation and full build pass.

The actual FND-CONFIG-154 bounded prefix passes its forward/backward/unknown-direction controls in GAME_DIR/analysis/reporter-audit/verify-string.mjs. The explicit forward starting hypothesis produces 23 width bytes from DS:040C, while the unspecified input produces two direction cases. Every prefix stops at its deliberately selected boundary and makes no full-function completeness claim. Reports/configurations remain local only. Root retains its merged upstream pin pending review of these candidates. Gap 25 remains partial: the FND-CONFIG-155 overlapping IRET helper and restoration through it have not passed. Gap 24 also remains open.

## Explicit overlapping helper and local IRET acceptance

Toolkit PR 19 follow-up e08397283b9cc8271dba01019803d4571430f618 now retains overlapping target proof from a verified direct edge and models a strictly local segmented16 IRET frame. Synthetic cases retain operand-byte false-boundary rejection and verify saved direction restoration, missing/root/prefixed/flat frames, corrupted saved FLAGS and overwritten return targets. Toolkit gates and full build pass.

GAME_DIR/analysis/reporter-audit/verify-iret.mjs verifies the complete FND-CONFIG-155 helper, including its explicit interior target and continuation. Under explicit SS:SP = 9000:8000 nonaliasing stack hypotheses, the three CS word spans 0126..0166 (zero), 0166..0186 (FFFF), 0186..01A6 (zero) match the recorded control. The path completes its far return; starting DF zero, one or unknown is restored respectively through the saved FLAGS frame. The actual incoming control at 0x0003DD33 targets 0x0003DD30 without an unresolved overlap gap. The query initially supplied an incorrect near-return width; correcting it to the finding's far-return contract fixes the root frame check. This was a query error, not an implementation relaxation.

No original is executed, no interrupt or native reachability claim is made, and raw reports remain local. Gaps 24/25 now have passing source controls in the candidates but remain pending reviewed merge and local exact-pin adoption. Root merged reporter pin is unchanged.

## Instruction-owned operand acceptance

2026-10-01: toolkit PR 19 adds an instruction-owned operand query. GAME_DIR/analysis/reporter-audit/verify-operands.mjs checks the remaining resident/overlay/stored word mappings and rejects undeclared neighboring words. verify-instruction-operands.mjs then confirms verified instruction ownership for resident load, overlay load, stored pointer segment and pushed segment argument, with raw representation and declared source-derived mapping. The query rejects partial/wrong-width immediates and unverified boundaries in synthetic controls, and its hash-guarded CLI is tested. Gap 16 now has passing controls in the candidate but remains pending reviewed merge/adoption.

A review of command filtering found effects omitted flag/string events despite trace retaining them. The candidate fixes this and verify-iret-effects.mjs passes the actual complete helper's span/restoration controls through effects. Toolkit canonical checks and full build pass; no source-derived report or config is committed. Root merged reporter pin stays unchanged pending reviewed adoption.

## Reviewed main migration acceptance, 2026-10-01

Merged toolkit 7da1b93cdd9ac0d59dbaf82b66b4db95d578ab9d and template 7b3bbe46b251b163ee02a6539ac0d81559dbe921 are adopted exactly. The instruction-owned resident/overlay/stored/pushed controls and actual width/helper-effects controls pass again against the pinned root source (adopted-verify scripts under GAME_DIR). Revised overlap traversal retains contested-reachability boundaries with synthetic regressions. Gaps 16, 24 and 25 are removed; older candidate/pending notes above are historical. The canonical gate, synthetic capture/tracking checks and full build pass; logs stay under artifacts. No original runtime was used.

## Adopted instruction-model rerun, 2026-10-01

Reporter `313bb7d8baa238b5934e5ddf3f60cb2d5f4887ed`, website
`ca39d0750e67c8c3900e8554e66a84083fe67452`, template
`8d0eef35ec8f3b1053ba1dd129a7bd75b044cf27`. The installed executable length
and SHA-256 match SOURCE-EDITIONS before querying; XXH3-128 was also reverified. Configurations and raw
reports are local under GAME_DIR/analysis/reporter-audit/adopted-313bb7d-rerun;
run.mjs replays unchanged baseline queries, conditional.mjs replays existing
conditional configurations with visitLimit 64 and bounded step/path limits.
The latter retains the earlier explicitly assumed returning-call models;
completion under those models never proves an unread callee's effects.

| Gap | Rerun disposition |
| --- | --- |
| 26 | Frame reader completes both modeled paths, without the previous carry/rotate instruction stop. Initializer still stops at repeated-instruction bound, unknown/overwritten return provenance and an undeclared callee. Raising visitLimit to 64 under its existing conditional model retains the bound and return-provenance stops. Full request stays open. |
| 27 | Baseline loader retains completed exits and an unresolved outside-mapped-code call. The enlarged conditional query exceeds the explicit 32 MiB report bound and produces no complete report; narrow it before another attempt. Full request stays open. |
| 35 | Registration query reaches the path cap and undeclared callees. Consumer baseline retains an undeclared callee; its existing two returning-call models complete the conditional paths. This does not verify actual preservation or end-to-end forwarding. Full request stays open. |
| 39 | Baseline stops at undeclared callees. Enlarged conditional query exhausts the modeled-call path limit and emits no terminal paths. Neither buffer capacity nor whole-loop completion is accepted. Full request stays open. |
| 42 | Wrapper retains undeclared callees; runtime retains an outside-mapped-code call. No unsupported-instruction stop is reported before these boundaries. This does not establish lower-heap effects or allocation units. Full request stays open. |
| 21 | Caller retains visit-limit, unresolved/undeclared-call and return-provenance stops. Helper baseline retains an undeclared callee; its existing returning-copy model completes conditional paths. Mixed-segment filename/caller dependencies remain unverified. Full request stays open. |

These are static reporter acceptance results. No original process, emulated
call, native observation, spec claim or parity status changed. No gap closes
from this rerun. Empty gaps arrays do not override stopped paths or an explicit
completeWithinModel false result.


Canonical Invoke-Validation.ps1 passes, including Test.ps1, locked restore,
Release build and assetless publish/smoke; log: artifacts/reporter-rerun-validation.log.


## Target, format and boundary controls, 2026-10-01

The adopted reporter remains toolkit 313bb7d (website ca39d07, template
8d0eef3; exact revisions in TEMPLATE-ADOPTION). Installed DSUN.EXE length,
SHA-256 and XXH3-128 match SOURCE-EDITIONS. All queries use the Node loader's
source-derived mappings. Known controls are independently recorded in
FND-EXE-002 through FND-EXE-005: 229 descriptors, 49 overlays, 8,262 fixups
and 854 trampolines. They are asserted before the instruction query runs.
Local configurations, reports and replay/assertion drivers live under
GAME_DIR/analysis/reporter-audit/target-controls and boundary-controls.
No original runtime, emulated call, game claim or parity status changed.

| Gap | Cases and acceptance disposition |
| --- | --- |
| 8 | target-controls/run.mjs and verify.mjs check the Start Game helper's 0160 and 01D0 FBOV encodings and its overlay-182 common-exit call from FND-CONFIG-028. Reports preserve the raw word, descriptor 44/58/182, source fixup membership, load segment, resolved address and canonical destination; the overlay call retains its resident trampoline and overlay file citation. Every positive call is an entry-path instruction. The actual no-fixup raw candidate produces relocated false and no target; its unverified boundary stays explicit. Adopted synthetic tests also cover an instruction-boundary no-relocation control. Entire bounded mapping request passes and closes. |
| 10 | The two child-registration operands in FND-CONFIG-031 report raw 2EBE:0008 and 2F96:000B, source MZ relocation membership, load segment 1000, mapped 3EBE:0008 and 3F96:000B, and the correct shipped-file destinations/citations from FND-CONFIG-032. The APFM branch is entry-path verified after declaring the table-derived branch entries; the BUTN site remains explicitly a raw candidate under this walk. The requested relocation mapping passes for both; no whole registration-flow claim is made. Entire normalization request closes. |
| 22 | FND-CONFIG-179/183's call retains raw 0640:0025, descriptor 200, loaded 5773:0025, resident trampoline and canonical overlay file target. FND-COMBAT-009's analyzer address is retained alongside it and flagged as disagreeing; the two game readings are not merged or modified. Wrong range/fixup counts, resident-descriptor selection, invalid trampoline selection and escaping regions fail before querying. verify-malformed.mjs mutates copies in memory only and verifies header trap, resident-as-overlay, payload/code bounds, odd fixup size, fixup operand extent and trampoline table/target bounds are rejected. Entire format/mapping-validation request passes and closes. |
| 23 | boundary-controls/run.mjs and verify.mjs reproduce FND-CONFIG-151's body-byte count, three-byte hole and later far return; the analyzer comparison lists the return beyond start plus body size. FND-CONFIG-158's mode setter reports both far-return exits, retaining explicit port/interrupt continuation assumptions. Truncating the resource reader at start plus size or the setter at its first return produces complete false and named gaps. This proves local bounds, not callee effects or native hardware behavior. Entire boundary-report request passes and closes. |
| 11 | The setup control is owned by FND-CONFIG-092's setup entry. The handler call is rejected as setup-owned and retains the intervening setup return as a warning; however, the handler's indirect dispatch stops its body traversal and leaves ownership unresolved. This correctly avoids the earlier false adjacency claim but does not prove the requested full handler ownership. Keep open. |

The missing-fixup control is a byte-candidate test, not a new instruction
claim. Known format counts are acceptance constants, not coverage percentages.
Negative range controls never convert an incomplete local body into a complete
Standard reading. No proprietary source or raw report is committed.

Canonical Invoke-Validation.ps1 passes after these closures, including Test.ps1,
locked restore, Release build and assetless publish/smoke.
Validation log: artifacts/target-boundary-validation.log.


## Incoming coverage rerun, 2026-10-01

GAME_DIR/analysis/reporter-audit/incoming-controls/verify.mjs reruns the
selector, setup and later-relative configurations with the declared resident
segment from FND-CONFIG-114/128, descriptor/trampoline target selection and
independent format-table controls. It asserts all eleven selector encodings,
all six setup encodings and the later relative-call candidate against the
recorded inventories. Every byte of the declared search domains is scanned;
coverage reports no unsearched ranges. This says nothing about regions outside
those declared domains or routes the instruction walk cannot resolve.

The accepted-entry walk still confirms only seven selector sites and two
setup sites. Four in each inventory remain candidates with explicit position
metadata. Full known-inventory positive controls fail at an unverified site
instead of treating raw discovery as instruction proof. The later relative
call also remains an unverified candidate and fails its positive control.
Computed-transfer inventories remain explicit. A prefix ending at the caller
return is correctly reported as partialSearch with unsearched segment bytes
and negativeUsable false. Thus the new coverage controls pass but gap 13 stays
open. FND-CONFIG-114's separate relocated-pointer exact-pair/aliased-target
inventory has not been supplied by these call-only queries and remains part
of the unfulfilled request. No spec claims or parity statuses change.

## Indirect dispatch and pointer inventory candidate

Toolkit PR [33](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/pull/33),
commit 45db236, adds bounded source-table indirect-jump declarations for CFG
ownership/incoming discovery and an exact-pair/aliased-target relocated-pointer
inventory. GAME_DIR/analysis/reporter-audit/dispatch-pr-controls/verify.mjs
asserts the four FND-CONFIG-092 table targets before using their declared
consumer. The candidate assigns the handler call to the handler entry, rejects
the mistaken setup owner, and retains incompleteness under a nonexhaustive
declaration. The table has separate discriminator and target arrays; the first
query's interleaved-layout hypothesis failed its independently recorded target
control and was corrected locally, with no change to the finding or tool.

FND-CONFIG-114's pointer query produces separate zero exact-pair/aliased-target
counts. Unresolved adjacent pairs remain explicit and negativeUsable is false;
no pointer-absence claim is accepted. The tool does not establish that adjacent
relocated words are used as runtime pointers. No original bytes or source-derived
configurations/reports enter the PR. Upstream Python, bridge/documentation, policy,
build and .NET gates pass. The current root pin remains 313bb7d pending reviewed
adoption. Gaps 11 and 13 stay open, including the remaining incoming controls.

## Merged toolkit 33 adoption (2026-10-01)

Adopted exact toolkit revision f8c51bfc5d167c25a04f2b6ddb3f7390714dc45f, including dispatch and pointer modules, guide and synthetic tests. The adopted source driver in GAME_DIR/analysis/reporter-audit/dispatch-pr-controls/verify-adopted.mjs passes: the independently checked four-target declaration identifies the handler owner; a partial declaration remains incomplete. The relocated pointer inventory retains unresolved candidates and reports negativeUsable false. Gap 13 remains open; these controls do not satisfy its entire caller/pointer contract.


## Gap 13 complete caller controls and pointer qualification follow-up

The adopted f8c51bf reporter passes all eleven selector, six setup and the
later resident relative-call positive controls. The licensed-source identity is
checked before queries. The replay driver is
GAME_DIR/analysis/reporter-audit/gap13-dispatch-complete/verify.mjs; every
positive control is a confirmed entry-path instruction, not a raw candidate.
Searches retain complete declared-domain coverage and explicit unresolved
computed transfers elsewhere. Source-table declarations use the independently
recorded discriminator arrays in FND-CONFIG-102/128, and bounded reads of the
exported caller dispatch consumers identified in FND-CONFIG-106/111/126. Nested
event dispatches are included; no call-site start is added to bypass them.
The resident declaration is used only under its actual segment mapping.
These controls establish the tooling inventory, not player reachability or
callee effects, and do not change spec claims or parity statuses.

Toolkit [PR 34](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/pull/34),
commit 4ad9179, proposes explicit pointer-domain exclusions. The candidate's
source driver verify-pointers-candidate.mjs in the same local directory passes
FND-CONFIG-127's independently known resident relocated-call control, first
as an exact loaded pair and then with an equivalent alias query. The handler
query has no exact or aliased target matches. Every formerly unresolved row
is accounted for: a pair crosses its declared source boundary or its checked
nonwrapping segmented arithmetic lies outside the resident load image. The
candidate retains these rows, arithmetic and bounds as exclusions; it does
not call them resolved pointers. Overflow stays unresolved. All exclusions
count toward the cap, and the capped source control refuses negativeUsable.
The uncapped controlled zero result qualifies only the declared adjacent-pair
representation; computed, unrelocated or runtime-used pointers remain outside
its scope. No universal absence or game-behavior claim follows.

Candidate gates pass: synthetic Python and Node suites, repository policy,
.NET build/tests and both package checks. Logs are artifacts/pointer-exclusions-*.
The root pin remains f8c51bf. Gap 13 remains open until PR 34 is reviewed,
merged, adopted exactly and the whole source-control suite passes again.


## Gap 13 merged adoption and closure

Toolkit PR 34 merged as b87064216317eeee0ac991a8b154d5b58a46534c and is
adopted exactly, including source, guide and tests. The configured-source replay
in GAME_DIR/analysis/reporter-audit/gap13-dispatch-complete passes verify.mjs,
verify-pointers-adopted.mjs and verify-negative-controls.mjs. All known selector,
setup and later-relative caller controls are confirmed instructions. Reports
retain separate overlay/resident/relative sections, canonical sites, encoding,
full declared coverage, candidate/contested classification and exclusions.
The selector controls assert both the stored shifted word and decoded descriptor.
Incoming result caps and narrowed relative ranges reject usable negatives.
The independent resident pointer control passes exact and equivalent alias
queries. The handler inventory retains every checked exclusion and has no
unresolved mapping under the declared representation; capped output fails its
negative control. Computed/unrelocated routes and runtime pointer use remain
explicitly excluded. These qualified inventories never prove universal absence.
The entire gap 13 tooling contract passes and its request is removed. No game
claim, research queue or parity status changed.

Canonical Invoke-Validation.ps1 passes after exact adoption; log
artifacts/gap13-merged-adoption-validation.log. The final shared template adoption
is being proposed separately and does not change the root's accepted reporter.


## Gap 11 ownership range/export follow-up

The adopted owner traversal settles the handler's entry but did not expose the
analyzer hypothesis's actual ranges or source-derived export provenance on its
owner row. Toolkit PR 35, commit 2b74d8b, proposes checked-entry ranges,
FBOV descriptor/trampoline/container provenance and explicit boundary results.
The loader derives exports from hash-guarded source tables and rejects supplied
copies. Incomplete/contested traversal cannot produce a usable join, and all
continuation assumptions stay explicit. No body-byte count becomes an end.

GAME_DIR/analysis/reporter-audit/owner-provenance-controls/verify-candidate.mjs
passes the independent setup and dispatched-handler controls: both have their
own source export and bounded reached ranges; the neighboring analyzer body
ends before the handler site and disagrees. The partial dispatch control refuses
joining the handler. These are conditional tooling results, not player
reachability or callee-effect evidence. Candidate reporter, policy, build,
.NET and packaging gates pass; logs artifacts/owner-provenance-*.

Template PR 42 and toolkit PRs 35?37 are merged. The exact root pin is
c133cd48bfe6cc3cb7126616996e7d548982a068. The verify-adopted.mjs sibling
driver passes the entire setup/handler contract with source-derived exports,
reached ranges, analyzer disagreement, partial dispatch rejection and entry-limit
join refusal. Gap 11 is closed after this exact-adoption rerun. No spec claim, parity status or research
queue changes, and no original bytes/configurations/reports enter Git.

## Gap 17 overlapping operand candidate follow-up

Toolkit PR 38, commit 0da750d on c133cd4, adds operand-candidates with literal
memory/immediate candidates, prefixes, widths, byte spans and overlap groups.
Entry-path memory uses alone count; other operands, rejected overlaps and
unresolved boundaries remain distinct. Caps report omitted members and
unsearched ranges, and raw/contested positive controls fail.

GAME_DIR/analysis/reporter-audit/operand-overlap-controls/verify-candidate.mjs
passes the recorded FND-CONFIG-136 prefixed comparison and stripped-width
rejection; FND-CONFIG-142 preceding-byte candidates and verified reads;
FND-CONFIG-148 candidates spanning an addition/jump and its immediate case.
Unresolved source candidates remain explicit. Reports and configs stay local.
Synthetic source-bridge, contested-boundary, scan-limit, omitted-group and
failed-control tests pass, together with full toolkit policy/build/test/package
gates (artifacts/overlap-candidates-final-*). This is conditional instruction
ownership tooling, never runtime use or original behavior proof.

Gap 17 stays open pending reviewed merge, exact adoption and whole-contract
rerun. Root reporter pin remains c133cd4; no game spec, parity status or research
queue changed.

## Gap 20 shared-callee and recursion follow-up

Toolkit PR 39, commit c33f1b6 on c133cd4, adds a bounded callees graph.
An edge into the active traversal path is recursivePath; a previously read node
outside that path is sharedNodeReuse. Incomplete or contested back edges remain
unresolved. Each caller's summary retains reachable explicit memory observations,
assumptions and unread dependencies. These are conditional CFG observations,
never runtime recursion or a read-only/effect-completeness guarantee. Unchecked
declared entries block boundary claims, and omitted/capped work stays explicit.

GAME_DIR/analysis/reporter-audit/callee-graph-controls/verify-candidate.mjs uses
FND-CONFIG-138/139/140/141's recorded entries and the hash-guarded installed
source. The parser seed/search path and lookup wrapper converge on the same
lookup; neither is labeled a cycle. Both callers retain the child helper's byte
and word write observations. Unknown root routes remain dependencies. Source
node/depth/edge caps and a false cycle control fail safely. Synthetic true
recursion, diamond reuse, conditional writes, unresolved calls, all caps,
unread entries, overlap rejection and source-bridge controls pass. Full toolkit
policy, build/tests and packages pass; logs artifacts/callee-graph-final-*.

Gap 20 stays open pending reviewed merge, exact adoption and whole-contract
rerun. Root pin remains c133cd4. No original configs, reports or bytes enter Git,
and no game spec, parity status or research queue changes.

Gap 17 final delivery: merged toolkit PR 38, exact pin
1ef21ef46567dd108ea082a0a493f7024ba79a07, passes the verify-adopted.mjs
sibling driver's complete source cases, rejected stripped-start positive control,
result cap and partial scan controls. Prefix order/repeats and literal-only
matching use the final reviewed implementation. No unresolved source candidate is
promoted. The entire Gap 17 tooling contract passes after exact adoption; remove
its request. This does not establish runtime pointer use or original behavior.

## Gap 21 near-pointer segment provenance follow-up

Toolkit PR 40, commit 76a8ec2 on 1ef21ef, retains address formation in
argument/effect reports and links consumed pointer parameters and dereferences
with formation/dereference segment registers, values/producers and offset
relations. Matching offsets alone never bind DS to SS. Propagated equal segment
expressions and matching/affine symbolic offsets permit merging within the model;
producer-only ancestry, unknown/rebound segments and capped formations refuse it.
Effect reports retain the pointer-related reads. No machine alias semantics or
runtime segment relationship is changed or inferred.

GAME_DIR/analysis/reporter-audit/near-pointer-controls/verify-candidate.mjs starts
at FND-CONFIG-143's recorded caller entry and follows FND-CONFIG-145's helpers
under the hash-guarded installed source. Both argument and effect reports retain
the caller's SS-based local address formation, the helper's consumed near-pointer
argument and its DS count dereference. Their segment relationship is unresolved
and storage merging is refused. Loop/unsupported-instruction stops and incomplete
path coverage stay explicit. The first broader query exceeded the 32 MiB output
cap and produced no complete report; narrowed bounded controls pass without an
absence or runtime claim. A formation-limit source control refuses merging.

Synthetic caller passing, DS=SS propagation, differing/rebound segments, field
offsets, erased-value ancestry, caps and source bridge pass, as do full toolkit
policy/build/test/package gates; logs artifacts/near-pointer-final-*. Source
configs/reports stay local. Gap 21 remains open pending reviewed merge, exact
adoption and whole-contract rerun. Root pin remains 1ef21ef; no game spec,
parity status or research queue changed.

Gap 20 final delivery: merged toolkit PR 39, exact pin
8853eb0e9a3542ef3a5c2864b25bd95b5201ae75, passes the verify-adopted.mjs
sibling source driver's entire contract. Both callers refer to the same shared
summary, whose reachable nodes retain the byte and word write observations and
continuation assumptions. Root unread routes remain dependencies; false-cycle,
node/depth/edge cap controls pass. Final review uses shortest-depth breadth-first
reading, explicit cycle-closing paths and referenced per-node summaries; no
shared-node shortcut drops effects. Operand overlap controls also still pass.
The entire Gap 20 tooling contract is accepted; remove its request. Runtime
recursion, execution and effect completeness remain unproven.

Gap 21 final delivery: toolkit PR 40 merged as
67340fcb975449600c160ef5a4995119d4e8f127 and is adopted exactly. The
verify-adopted.mjs sibling source driver passes the entire caller-formation,
consumed-argument and callee-dereference contract in both reports. DS/SS equality
remains unproven and merging refused; source stops remain incomplete. The
formation-cap control retains an actual link and still refuses merging. Final
review retains newest formations and effective segment registers for all modeled
access types; synthetic controls cover those cases. Gap 20 graph controls still
pass on this final pin. Gap 21's tooling request is accepted and removed without
claiming native segment state or runtime aliasing.

## Gap 15 guarded caller-local order follow-up

Toolkit PR 41, commit bab129c on 67340fc, retains the flat incoming inventory
and groups confirmed calls by verified containing entry and necessary CFG guard
edges. Sequence order comes from continuation reachability, not file-address
sorting. Branch alternatives and incomplete/shared/contested ordering stay
explicit. Guarded loops are ordered per shared-guard visit, with recurrence
retained. Adjacent CMP/TEST context requires a sole predecessor. Cleanup is
observed after an assumed return and never proves return success, preserved state
or callee effects; those remain a gap.

GAME_DIR/analysis/reporter-audit/call-order-controls/verify-candidate.mjs uses
all source-derived overlay exports and FND-CONFIG-119's recorded controls.
It retains all seven confirmed incoming sites and proves two three-call groups
and the conditional seventh call within their guard visits. The signed local
word comparisons, independent guard thresholds and eight-byte cleanup remain
explicit. Outer-loop recurrence is preserved rather than flattened into a
single execution. False-alternative and entry/result/instruction/analysis cap
controls reject positive ordering claims. Configs/reports stay local.

Synthetic reversed-address sequence, alternatives, ambiguous comparison
predecessors, shared guards, cleanup, guarded recurrence, shared/overlapping
ownership, caps and source bridge pass, along with full toolkit policy/build/
test/package gates; logs artifacts/call-order-final-*. Gap 15 remains open until
reviewed merge, exact adoption and whole-contract rerun. Root pin remains
67340fc; no game spec, parity status or research queue changed.

## Gap 26 width-preserving result flow follow-up

Toolkit PR 43, candidate f0d05f4 on the current packaged toolkit base cebd5a7 extends return reports with declared
width-bounded failure/result encodings and roles, caller entry/call site,
partial and sibling-byte register writes, stored widths, explicit and implicit
extensions, both predicate operands and signed/unsigned predicate domains.
Producer ancestry is only dependency evidence; encoding lists are nonexhaustive
and no nonzero gate establishes successful initialization or resource validity.
Declarations are validated before tracing, including unreachable ones.

GAME_DIR/analysis/reporter-audit/return-flow-controls/verify-candidate.mjs
hash-guards the established official source. FND-CONFIG-156's actual failure
return and caller's low-byte store pass without child call models; other paths
remain stopped/unread. FND-CONFIG-190's actual unsigned reader-rejection
returns also pass. Conditional balanced-return/register hypotheses retain
FND-CONFIG-157's full-word comparison and boolean normalization,
FND-CONFIG-149's sibling-byte zeroing and OR/JNE nonzero gate, and
FND-CONFIG-189/190's signed dimension consumers. The latter retain index
failure separately from raw word roles. Child effects, high register halves,
actual pointer/resource contents and live outcomes remain unconfirmed. The
consumer cases explicitly model the readers' retained SI/DI; those models
are never complete native-call evidence. No spec claims change.

Nonvacuous result/consumer/analysis and trace step/path caps remove the full
positive initializer control. A mismatched encoding preserves the actual result
but fails the expected failure-role control; an out-of-width encoding is rejected.
All synthetic Python/Node, policy, .NET and package gates pass; logs
artifacts/return-flow-final-* and return-flow-packages-*. Candidate is not adopted. Gap 26 stays open
until reviewed merge, exact adoption and final whole-contract rerun.

Gap 15 packaged-layout follow-up (2026-10-01): toolkit PR 41 at f8c756f
preserves its published history and merges current toolkit packaging. The CLI
usage names call-order, and the typed bridge retains its source-coverage/order
control. Full engine/source controls, workspace lint/format/type/tests, release
planner, repository policy, npm build/tarballs, Python wheel and .NET/NuGet
gates pass; logs artifacts/call-order-packages-*. The source driver now uses
the packaged bridge/engine. Candidate remains unadopted and Gap 15 remains open.

Published package adoption, 2026-10-02: existing adopted source drivers for
callee graph, near-pointer provenance, dispatch normalization, instruction
operand ownership, overlapping owners, strings, local IRET and Gap 13 pointer
inventories pass using the registry engine 0.1.0 and reader 0.1.0. Each retained
source hash and rejected/capped control remains active. Driver configurations and
reports remain under GAME_DIR/analysis/reporter-audit/. This delivery change
neither adds native evidence nor closes the open candidate gaps 15 and 26.


Gap 27 candidate tooling, 2026-10-02: toolkit PR 45 at 4ed65cd extends
existing effects reports with ordered path timelines, write prefixes, nested
unknown effects and exact-width/storage/value restoration witnesses. The
FND-SCRIPT-019 wrapper control confirms the early returns have only prologue
stack writes and no age/service path. Its other routes remain unread at services.
The wider fill query exceeded the existing output limit; narrowing it preserves
explicit path/model caps, not a complete transfer-order answer. Source driver:
GAME_DIR/analysis/reporter-audit/effect-order-controls/verify-candidate.mjs.

Complete engine, prepared-reader integration, workspace lint/format/type/tests,
release planner, policy, .NET and distributable package gates pass. Logs are
artifacts/effect-path-*. No original fixtures or claims changed. This is the
shared summary slice only: all cited Gap 27 child, transfer, cleanup, predicate
and restoration source cases and reviewed registry adoption are still required.
Gap 27 stays open with its full original contract unchanged.

## Merged engine 0.4.0 adoption, 2026-10-02

Toolkit PRs 41, 43 and 45 are merged. The published engine 0.4.0 wheel is
installed by its exact SHA-256 requirements lock; npm reader/checker remain
0.1.0. Installed-package source drivers have no PYTHONPATH override and use the
configured withEngine routing. All retained adopted drivers pass, including
ownership, graph, dispatch/pointer inventories, operand overlap, strings and
IRET boundaries. Local configs/reports remain in GAME_DIR.

Gap 15's complete FND-CONFIG-119 contract passes using
GAME_DIR/analysis/reporter-audit/call-order-controls/verify-adopted.mjs:
flat coverage, verified containing entry, sequence groups/shared guards,
recurrence, cleanup and explicit unresolved callee effects. False-alternative
and nonvacuous entry/result/instruction/analysis caps reject positive claims.
Gap 15 is closed after merged delivery and canonical gates.

Gap 26's complete declared width/encoding contract passes using
GAME_DIR/analysis/reporter-audit/return-flow-controls/verify-adopted.mjs.
Actual initializer failure/low-byte store and unsigned-reader rejection are
retained. Wrapper normalization, sibling-byte extension/nonzero gate and signed
raw-dimension consumers pass under explicitly conditional callee models.
Failure roles remain distinct from raw field roles; no successful initialization,
resource acceptance or live failure occurrence is inferred. Wrong encoding,
out-of-width declarations and nonvacuous result/consumer/analysis/step/path caps
reject the positive control. These qualifications remain part of the tooling
contract. Gap 26 is closed; no native evidence or spec status changed.

Gap 27's installed effect-order driver passes the SCRIPT wrapper early returns
and retains stopped service routes and incomplete fill paths. This is only a
verified slice: the complete fill, child, cleanup, last-predicate and snapshot
contracts cited in gaps.md remain outstanding. Gap 27 stays open unchanged.
Source logs: each adopted driver has a sibling *-engine040.log; no source report
or configuration is committed. Normal and unavailable-proxy NoRestore configured
validation pass, including Test.ps1, Release build and assetless publish/smoke;
template normal and offline canonical gates also pass. Logs:
artifacts/package-delivery/{root,template}-engine040-{normal,offline}.log.

## Gap 27 resource-suffix predicate control, 2026-10-02

The installed engine 0.4.0 passes FND-CONFIG-179's bounded resource-suffix
control. Source identity and descriptor ownership are checked before tracing.
The driver starts at a recorded body instruction under explicit synthetic
DS/SS/SP/BP assumptions; the parent body and epilogue are outside this slice.
This is not a verified whole-function entry or native-reachability claim.

For all three resource requests, both declared AX zero and FFFF hypotheses
retain the subsequent AX OR, then a branch whose producer is the later field
comparison. A false reading that attributes the branch to the AX OR is rejected.
The nonzero third-pointer route bypasses the mode comparison and final clear;
a mode-five route with a returning primitive hypothesis retains the later
local byte clear. The local sentinel write precedes the first service request.
All modeled calls preserve only declared DS/SS/BP hypotheses, invalidate memory
and flags, and remain unknown-effect calls. Output assignment by an unread
resource callee and accepted content are not inferred.

The suffix exits stop at the excluded epilogue, so complete-function and native
coverage remain false. Step/path caps and removing service models destroy the
positive predicate contract. Every path retains transactionality as unestablished.
Local driver: GAME_DIR/analysis/reporter-audit/effect-resource-controls/
verify-adopted.mjs; sibling verify-adopted-engine040.log and JSON reports retain
controls. The full Gap 27 request remains open: complete fill, linked child,
cleanup/hardware, layered cache and snapshot paths still require acceptance.

## Gap 27 conditional table-continuation candidate, 2026-10-02

Published engine 0.4.0 and current upstream main stop the FND-CONFIG-168
resident consumer at its computed child jump, even with a supplied source table.
The adopted-limit control reproduces that stop. Toolkit PR 60 adds separate
conditional declaredContinuationPaths while retaining original stopped paths.
The evidence extension assigns no new instruction semantics or runtime selector;
concrete operands/field addresses and repeated choices reject contradictions,
overlapping targets cannot establish boundaries, and shared budgets remain gaps.
Candidate summaries always leave effect completeness false.

Local driver GAME_DIR/analysis/reporter-audit/child-effect-controls/
verify-candidate.mjs uses the current reader archive and candidate engine source.
It starts at the complete recorded resident entry. The broad four-target query
remains path-capped and does not establish the MENU exit. Splitting at the checked
MENU source word with an explicitly partial table permits both conditional
balanced-return hypotheses. An AX FFFF hypothesis retains the preceding word
count decrement and bypasses later local clears; AX zero retains the unconditional
later pointer clears. Models preserve only declared DS/SS/BP/SI assumptions and
invalidate memory/flags; native child effects and error origin stay unknown.
FND-CONFIG-172's valid finite traversal reading is not contradicted by a modeled
failure case. Other table rows, head/list mutations and outer caller controls
remain outstanding; neither the original whole function nor the full request is
proved complete. Wrong-field and nonvacuous path/step/boundary controls remove
the positive contract. Configs, reports and verify-candidate.log stay local.

Full candidate engine/workspace, prepared-reader integration, lint/format/types,
release planner, .NET build/test/pack and repository policy pass. Wheel/sdist and
npm archives build; isolated installed-wheel/extracted-reader smoke passes with
no PYTHONPATH. Logs artifacts/package-delivery/indirect-effect-*. The candidate
is not an adopted registry release. Gap 27 stays open with its entire contract.

## Published reader 0.2.0 adoption, 2026-10-02

The registry reader 0.2.0 from toolkit release d938689 is installed with its exact
npm archive integrity lock in configured and template checkouts. Engine 0.4.0,
checker 0.1.0 and Capstone 5.0.7 retain their existing locks. All retained adopted
source drivers pass without PYTHONPATH, including guarded call order, return
widths, pointer/dispatch inventories, ownership, graph, string/IRET and effect
controls. Their sibling *-reader020-engine040.log files remain under GAME_DIR.
The child-effect published-limit control still stops at its computed jump;
PR 60's candidate continuations are not accidentally adopted.

Configured and template normal and unavailable-proxy NoRestore canonical gates
pass. Configured validation includes Test.ps1, Release build and assetless
publish/smoke. Logs artifacts/package-delivery/reader020-{root,template}-
{normal,offline}.log. No source claims, parity statuses, rule snapshot bytes or
original-content contracts changed. Gap 27 remains open with its full request.

## Gap 27 returning pointer-consumer caller control, 2026-10-02

Installed reader 0.2.0 and engine 0.4.0 pass FND-CONFIG-161/168's three
conditional returning-call/clear contracts. Each observed selector push is a
zero word; each clear is a zero dword through its independently reloaded fixed
segment and the corresponding field offset. A returned AX word zero or FFFF
reaches that clear without an intervening conditional branch. The reporter
retains each modeled result before the caller replaces AX with its field segment.
No success, release or resource-validity conclusion is inferred from the clear.

The full entry/body query is capped in later services and polling, so it does
not establish complete function coverage. The bounded acceptance split retains
the actual entry and ends after the three local clears. Its stopped paths and
unknown service effects remain explicit. Balanced returns and DS/SS/BP are
hypotheses; models invalidate memory/flags, so later field gates do not inherit
unproved callee preservation. Step/path caps and removing the models destroy
the positive three-clear contract for both result hypotheses. Native cleanup,
error origins and later progress remain unconfirmed.

Driver: GAME_DIR/analysis/reporter-audit/pointer-caller-effects/verify-adopted.mjs.
Its configs/reports and verify-adopted-reader020-engine040.log remain local.
Gap 27 stays open for head/list mutations and its other full cited contracts;
no spec status, parity row, gameplay or native evidence changed.

## Gap 27 head and predecessor mutation controls, 2026-10-02

Installed reader 0.2.0 and engine 0.4.0 pass the FND-CONFIG-168 actual-entry
prefix controls through the local service following the count mutation. Separate
guarded paths retain the head-link copy, count-one search bypass and found-
predecessor link copy. Each copied dword retains its source value expression;
this proves reported value provenance, not shared storage. Source/predecessor
segments remain unknown where the report cannot resolve them. The word count
write retains modulo-word decrement provenance and precedes the modeled service.

Balanced returns and DS/SS/BP preservation are explicit hypotheses. Models
invalidate memory and flags and retain unknown callee effects. Removing them
retains earlier writes but prevents the returning-service positive contract.
Step/path controls likewise reject the complete three-phase positive contract.
A repeated-search witness reaches the copy and service under the default visit
budget; a one-visit control rejects that witness and retains a named repeat stop.
The reporter limit is not evidence of a native membership or iteration bound.

The prefix excludes later child dispatch and epilogue. Its paths remain stopped,
effect completeness false and transactionality unestablished. Full head/list-to-
child coverage, actual record membership, callee effects and native outcomes
remain unconfirmed. Driver and reports stay in GAME_DIR/analysis/reporter-audit/
list-mutation-effects/; verify-adopted-reader020-engine040.log records passing
controls without candidate imports. Gap 27 remains open for its entire contract.

## Gap 27 cleanup and hardware boundary controls, 2026-10-02

Installed reader 0.2.0 and engine 0.4.0 retain FND-CONFIG-183's cleanup
before the later error request for each of five declared failing-request
hypotheses. The query begins at the actual initializer entry and retains the
earlier state-byte write. Cleanup and error are modeled balanced returns, not
successful cleanup or evidence that a termination request leaves the process
alive. Models preserve only declared DS/SS/BP hypotheses and invalidate memory
and flags. Step/path caps and unread services reject the returning-failure order.

FND-CONFIG-184's handle suffix retains all three word sentinel writes after
their unchecked services for returned AX zero and FFFF, without an intervening
result branch. Each sentinel bypass skips its own service and write. A returning
fill hypothesis precedes the final byte clear. The suffix begins at a boundary
checked from the complete local entry; earlier record/release loops and the
epilogue are excluded, and its stopped paths cannot prove whole cleanup.
Step/path and unread-service controls reject the complete returning-clear
contract. All summaries leave transactionality unestablished.

The complete resident hardware-body bounds report lists seven port boundaries,
each with an explicit continuation assumption. A boundary cap rejects that
inventory. The effect tracer stops at the first out instruction. The complete
signed wrapper gate retains a returning at-most-one bypass and a service path
that stops at that boundary. A conditional slot suffix, with declared DS=CS,
retains both slot-handling skips reaching the final port boundary: one skips
both local mutations and the other retains the flag write but skips the count
write. Step/path controls reject the two-skip contract. This does not join the
initial port phase to the suffix or simulate hardware, presentation or timing.
The unsupported-port stop is a retained dependency, not evidence of no effect.

Driver/configs/reports and verify-adopted-reader020-engine040.log remain in
GAME_DIR/analysis/reporter-audit/cleanup-hardware-effects/. No candidate import,
game claim, native run or parity status changed. Gap 27 remains open for its
full fill, joined child/cleanup/hardware, layered-cache and snapshot contracts.

## Gap 27 layered cache and nested-frame controls, 2026-10-02

Installed reader 0.2.0 and engine 0.4.0 pass FND-CONFIG-187's complete local
replacement-helper returning paths under separately declared query/transfer AX
zero and FFFF hypotheses. A successful query retains the signed dword length
comparison and its greater-than-2,048 rejection; both failed-query byte branches
reach the common transfer continuation without that comparison. Transfer results
do not gate the following pointer service or cache writes. The word cache store
retains the final modeled call's post-call SI expression, which remains unknown:
SI preservation and equality with the original argument are not assumed. Both
cache comparisons also retain their no-request bypass. Returning modeled-service
paths keep unknown effects and incomplete effect semantics; complete local path
enumeration is not accepted replacement content or native reachability.

FND-CONFIG-186's complete mode writer retains its word assignment before the
unchecked child request for zero and FFFF return hypotheses, and its equality
bypass skips the assignment and request. Step/path caps and unread services
reject these positive continuation contracts and the replacement-helper contract.
Models preserve only declared DS/SS/BP; memory, flags and other registers remain
unknown. Transactionality remains unestablished throughout.

The conditional parent byte bracket traces the first callee's own clear, but
stops at that callee's return because nested unknown-memory service models have
invalidated the ancestor return frame. The later parent call is not reached.
Step/path/unread controls remove the inner-clear witness. Balanced stack height
and register preservation do not prove unchanged return or saved-register bytes.
This is a correctly conservative stop, not a reason to silently preserve memory.
Current upstream main reproduces the class with synthetic near/far child frames,
a fully traced-service positive control, an explicit overwrite rejection and caps;
a real synthetic MZ prepared-reader control also passes. A separate upstream plan
requests bounded, explicit memory hypotheses without new instruction semantics.

Driver/configs/reports and verify-adopted-reader020-engine040.log stay in
GAME_DIR/analysis/reporter-audit/layered-cache-effects/. The joined parent bracket,
selected-number computed dispatch, full callers/children and accepted content
remain open. No game claim or parity status changed; Gap 27 retains its full
contract. Upstream request gates are logged at artifacts/package-delivery/
nested-frame-*. No proposed preservation behavior is adopted.

The reviewable plan/test request is [toolkit PR 63](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/pull/63),
head 6ef1d25, based on main 0b4694d. Its release:skip label reflects unchanged
published behavior. R1 closes only after the full scoped-memory implementation,
reviewed package delivery and the original joined acceptance pass.

## Gap 27 replacement-pointer wrapper controls, 2026-10-02

Installed reader 0.2.0 and engine 0.4.0 pass FND-CONFIG-188's complete
local returning wrapper paths under explicit zero/FFFF local-service hypotheses.
The null branch retains both later old-pointer release gates and their separate
bypasses, then the default-pointer copy and two dword clears. The nonnull branch
retains its post-service stacked-pointer copy, separate dword clear and word-to-
dword zero extension; it makes no own old-pointer release. Full argument widths,
SS attribution and copied/extended value expressions are checked independently.

The post-call pointer expression differs from the pre-call branch input after
unknown-memory service models; the report does not freeze the original argument
or infer accepted content. The modeled release's zero DX:AX follows the separately
recorded wrapper return contract, without a successful release claim. Local
service zero and FFFF hypotheses both reach the wrapper's own AX-zero producer.
Normalization does not validate child work, a presentation or callback state.

Models preserve only declared DS/SS/BP and balanced returns. All other registers,
memory and flags remain unknown. Complete local conditional path enumeration is
distinct from effect completeness: every path retains unknown service effects
and unestablished transactionality. Nonvacuous step/path caps and unread-service
controls reject the full positive branch/width/normalization contract.

Driver/configs/reports and verify-adopted-reader020-engine040.log remain in
GAME_DIR/analysis/reporter-audit/replacement-pointer-effects/. Active service
children, neighboring-byte/word gates, snapshots, full callers and native outcomes
remain open. No spec claim or parity status changes; Gap 27 keeps its full scope.

## Gap 27 snapshot bypass and local restoration paths, 2026-10-02

Installed reader 0.2.0 and engine 0.4.0 start at both complete resident
FND-CONFIG-189 service entries. The bounds queries retain complete local body
coverage under their stated continuation assumptions; effect queries remain
incomplete because active children and callbacks are unread. Neither query lifts
a path limit or substitutes a suffix for the entry.

Each service retains separate returned word-gate bypasses without a snapshot,
snapshot-bearing early exits without the restoration store, and reachable local
restoration stores. On the latter witnesses, the DS word read, SS saved-word
write and later DS word assignment retain identical value expressions and order,
with no intervening call after the snapshot. This proves the reported value
assignment, not transactionality or unchanged external state. The before-service
FFFF mode exit skips restoration. The after-service early exits retain their
preceding handle-sentinel writes; a reachable local mode route retains all common
commit stores before the restoration assignment.

Positive contracts require an explicitly modeled returning stack guard with
declared DS/SS/BP preservation only; memory/flags and other registers remain
unknown. Step/path caps and removing that guard model reject these qualified
return contracts. Other guard-bypass paths do not rescue the rejected hypothesis.
Unread active child calls remain separate stopped paths. Every summary leaves
transactionality unestablished. Stored handle-request failure followed by commit,
callback routes, post-call snapshot preservation, all active children and native
outcomes remain outstanding; these witnesses do not close the full request.

Driver/configs/reports and verify-adopted-reader020-engine040.log stay in
GAME_DIR/analysis/reporter-audit/snapshot-path-effects/. No candidate imports,
package changes, game claims or parity statuses changed. Gap 27 remains open.

## Gap 27 scoped-memory candidate archive controls, 2026-10-02

The scoped-memory candidate's locally built wheel and npm archive pass the
FND-CONFIG-186 caller-bracket controls through the actual MZ prepared-reader
bridge, without PYTHONPATH or adopted-package replacement. Archive source and
compiled-file integrity is recorded in artifacts/package-delivery/
scoped-memory-archive-integrity.log. Reader and engine both speak candidate
prepared protocol 2; protocol 1 is rejected by the engine's synthetic CLI controls.

Each returning service explicitly hypothesizes preservation of the pre-call
SS:BP six-byte saved-BP/far-return interval, under the existing initial segment,
stack and balanced-return hypotheses. The report retains each resolved interval,
register value/producers and evidence in the conditional models, return events
and effect summaries. It joins the parent's initial byte assignment, the child's
own clear, the next parent request and the final parent clear in that order.
Both later mode-gate routes reach the bracket's end; the query then stops at the
bounded region boundary rather than establishing a whole-function return.

Default models retain their conservative nested-return stop. Partial return
scope, wrong segment, overlapping interval, unread services and nonvacuous
step/path controls reject the joined bracket; scope/byte budget excesses fail
with diagnostics. Synthetic near/far/PE32 cases additionally cover saved
registers independently of return control, pre-call register changes, pushed-CS
consumption, explicit later overwrite, unknown/wrapping addresses, distinct and
possible segment aliases, exact budgets and stable uncached-byte terms.

Modeled services and their traced ancestors retain unknown effects. The mode
writer's no-service bypass retains its local traced classification; that does
not erase preceding unknown services from the enclosing path. All source paths
remain effect-incomplete with transactionality unestablished. No native service
preservation, resource acceptance, hardware or whole-call behavior is confirmed.

Driver, query configs, source reports and verify-candidate-archives.log stay in
GAME_DIR/analysis/reporter-audit/scoped-memory-effects/. Root installed reader
0.2.0 and engine 0.4.0 remain unchanged. Candidate packages use local development
version metadata and are not published releases. Toolkit PR 63 still needs the
reviewed implementation and major release classification delivered to its branch,
merge, paired registry delivery and adopted-package controls. Gap 27 remains
open for its full contract; no game spec or parity status changed.

## Gap 27 signed-handle, AL and getter-coordinate controls, 2026-10-02

Installed reader 0.2.0 and engine 0.4.0 pass FND-CONFIG-191's complete
local graphics-wrapper gates under explicit returning-validator and primitive
result hypotheses. Each signed-negative word-input bypass occurs before any
validator or primitive request. Nonzero AH with zero AL rejects the primitive,
including the first-byte bypass that skips the second validator. Two nonzero
AL results reach the primitive; its modeled zero or FFFF word is forwarded
without overall result normalization. Model memory/flags and unpreserved
registers remain unknown, so original argument identity after those calls and
successful graphics are not inferred.

The coordinate validator also completes its conditional instruction paths with
all four actual getter bodies traced, without service models. Each signed
comparison retains its flag producer and word width, first/last ordering,
non-strict equality admission and separate 320/200 limits. Every described
branch has both conditional directions represented; failure paths return AL
zero and the admitted path returns AL one, with AX still unnormalized. Incoming
handle/slot words remain symbolic, and these paths are not proof of feasible
native states or slot capacity.

A paired full trace verifies each getter's consumed SS word argument and its
DS=CS word-field read, keeping the getter's own field offset distinct from the
handle-derived index. Distinct declared initial SI/DI values and DS are locally
restored on each validator return. This is instruction-model restoration under
those query hypotheses, not a general native caller guarantee. The effects
command intentionally omits ordinary reads; the paired trace supplies their
provenance rather than treating omitted effect-summary reads as absent accesses.

Nonvacuous step/path caps, unread getter/service regions and an overflowing
byte-result declaration reject the positive contracts. An unmodeled wrapper
query retaining both validator/getter traversals exceeds the reader's 32 MiB
output limit; no complete report was produced. The unread-service rejection
control therefore declares only the wrapper body. That bounded rejection does
not establish the omitted child effects. No output or traversal limit was raised.

Driver, configs, reports and verify-adopted-reader020-engine040.log remain in
GAME_DIR/analysis/reporter-audit/coordinate-gates/. No candidate imports,
original runtime, game spec or parity status changed. The request's slot
acquisition/reference chain and partial metadata failure cases, active primitive
and whole-caller/native behavior remain open. Gap 27 keeps its full contract.

## Gap 27 slot/reference and partial-metadata controls, 2026-10-02

Installed reader 0.2.0 and engine 0.4.0 pass bounded FND-CONFIG-191
handle-request ordering controls from its complete resident entry. Source-derived
bounds identify the local scanner described by FND-CONFIG-183. A returning
scanner model declares selected SI offsets at the first, next and last scanner
positions separately; it leaves carry, memory and flags unknown. Its carry-set
branch retains FFFF before any own slot-metadata assignment. These register and
balanced-return hypotheses do not establish a native free slot or unchanged
caller arguments after the service.

For each declared slot, the carry-clear/reference-exit paths retain the reference
word and zero-count assignments before coordinate checks. Each of the four signed
comparison failures returns FFFF with exactly its preceding own coordinate-store
prefix and no own success-marker assignment. Admitted paths store every coordinate
before the next comparison, then the success marker and the divided slot index.
A paired trace connects each stored coordinate and comparison-left value to its
current SS word argument, and the comparison-right value to the corresponding
CS-relative reference field. Equal word widths do not merge the selected slot and
reference namespaces; a neighboring selected-slot control rejects that join.

The actual scanner is also traced without a service model. First-slot conditional
metadata witnesses pass, and returned paths restore distinct declared SI/DI values
and DS. This removes modeled memory effects only from those locally traced paths;
it establishes no native slot availability or capacity. The query retains scanner
and reference-loop visit stops and path-cap gaps. Neither that query nor the
modeled scanner query is accepted as complete entry coverage.

Step/path caps, unread scanner regions, call-depth exhaustion, wrong field
namespace and an overflowing selected-word declaration reject the relevant
positive contracts. A one-visit reference limit still permits the first reference
exit and metadata prefix, while rejecting progressed-link metadata. That distinction
is retained rather than treating a cap as absence of all effects. No limit was
raised. Model-based returned paths retain unknown service effects and unestablished
transactionality; local FFFF results do not imply external rollback or unchanged
state. Reference-chain termination, full scan/exhaustion state and native free-flag
preservation remain open.

Driver, configs, reports and verify-adopted-reader020-engine040.log remain in
GAME_DIR/analysis/reporter-audit/slot-metadata-effects/. The driver carries
`// needs: GAME_DIR`, resolves the licensed source through that variable and
reports a skip when the variable, source or retained local profile is unavailable.
Missing-variable and missing-source controls pass; their logs are in
artifacts/package-delivery/slot-metadata-{no-game-dir,missing-source}.log.
No candidate import, original runtime, game spec or parity status changed. Active
primitives, whole callers and the full Gap 27 contract remain open.

## Gap 36 neighboring-byte/word controls, 2026-10-02

Installed reader 0.2.0 and engine 0.4.0 pass the bounded joined resident
byte-helper/service case from FND-CONFIG-187 and FND-CONFIG-188. The local
`GAME_DIR/analysis/reporter-audit/neighbor-word-effects/verify-adopted.mjs`
uses the locked dependency bridge, verifies the licensed source hash and retains
its profiles/reports and `verify-adopted-reader020-engine040.log` locally.
Absent GAME_DIR or licensed source produces an explicit skip; no candidate
imports or PYTHONPATH are used.

On paths bypassing the service's conditional stack guard, the later word read
retains the helper's low-byte zero producer, a missing high-byte producer, a
two-byte addressed interval and the actual word comparison as flag producer.
Both admitted and bypassed word-gate routes remain conditional. On paths using
a declared returning guard model, memory invalidation removes both earlier byte
producers, despite preserved DS/SS/BP and a balanced return. Own byte stores
remain in the timeline; they do not become evidence of post-call contents.

Wrong-neighbor and wrong-consumer-width controls reject the profile. Step/path
caps, an unread service and an unread guard reject the full bounded distinction.
The unread-guard control still retains direct guard-bypass witnesses, so it is
not evidence of universal absence. Active child dependencies, default path gaps
and ancestor return-frame stops remain explicit. No limits were increased.
Illustrative zero-low-byte/high-byte arithmetic cases are explicitly synthetic;
no source memory was seeded or native byte reachability inferred.

This is tooling acceptance against existing findings, not a new game finding or
parity validation. Gap 36 remains open: complete producers, normalized outer
wrapper propagation and its complete fixture contract are not established by
this local case. No original runtime, emulated call, gameplay, spec status or
parity status changed.

## Gap 27 fill-boundary controls, 2026-10-02

Installed reader 0.2.0 and engine 0.4.0 pass source-bounded local windows
from FND-SCRIPT-019's complete fill body. The local driver is
`GAME_DIR/analysis/reporter-audit/fill-boundary-effects/verify-adopted.mjs`;
its profiles/reports and `verify-adopted-reader020-engine040.log` remain beside
it. The dependency bridge uses exact installed pins with PYTHONPATH absent,
source hashing and source-derived whole-body call/boundary controls. GAME_DIR
or licensed-source absence produces an explicit skip.

The allocation window supplies explicit zero, ordinary-word and FFFF return
hypotheses. It retains the byte-stop bypass separately from two ordered
word-sized slot-bound stores and reached byte-age increments. The allocation
input reads the current low length word and increments at word width. Unknown
memory after the modeled allocator prevents the later local-length read from
being equated to that earlier producer. The segment and selected-slot register
are declared hypotheses, not native slot admission or capacity. Age comparisons
and writes retain byte widths and signed guards. Default repeat/path limits stop
this window before transfer; no default was increased.

The separate transfer window checks zero, nonzero-AH/zero-AL and FFFF returned
words. Only the zero word reaches the append, slot identity/reset and current
identity/start writes, in their source order; it then reads its own local success
byte. Other words request the error service and, under returning zero/FFFF error
hypotheses, reach the excluded epilogue without own rollback or identity stores
in the read suffix. The failure result read remains unknown: the earlier local
zero is outside the window, and modeled services invalidate memory. Post-call
input/field provenance is current storage, not frozen original caller arguments.
Unknown destination pointers do not establish a valid append or buffer capacity.

Wrong slot/width expectations and nonvacuous allocation step/path/unread controls
reject the positive profile. Transfer step/unread-transfer/unread-error controls
reject returning failure acceptance. Excluded selection, prior age passes, saved
frames and epilogue prevent either window from proving a complete function. The
pre-transfer writes and failed-transfer continuation are **not** one joined
report path. Full fill acceptance requires bounded input/path tooling or fresh
producer evidence; repeatedly raising caps or combining fragments is not proof.
Gap 27 remains open. No new game claim, spec/parity status, original run or
emulation is recorded.

## Latest engine 0.7.0 acceptance and linked-child limits, 2026-10-02

Reviewed toolkit main/release tag 98395df03fab3990bca3a3de5cb1dcd1ae0df3a3
is adopted with registry engine 0.7.0, reader 0.2.0, checker 0.1.0, Capstone
5.0.7 and pypcode 4.0.0. Registry hashes and every wheel source, sdist source/test
and installed engine file match. Integrity evidence:
artifacts/package-delivery/engine070-registry/integrity.json. The checker/action
source is unchanged across the pin update. Installed dependency tests reject
missing/wrong pypcode and preserve prepared-protocol mismatch diagnostics.

All retained adopted source drivers pass with PYTHONPATH absent; generated
summary/logs are under GAME_DIR/analysis/reporter-audit/registry-engine070-regression/.
Previous profiles/reports were retained there before regeneration. Earlier scope
qualifications, limits and gap dispositions remain binding. No source input or
report is committed; no native run, emulation, spec or parity status changes.

PR 60's reviewed conditional-target API passes the installed synthetic MZ test
in tests/upstream/tool-dependencies.test.mjs: conditional return retains its prefix
write beside the ordinary unresolved jump, while a shared path cap rejects it.
The original FND-CONFIG-168 MENU candidate control was rechecked using installed
packages. At its smaller path budget, ordinary paths exhaust the shared budget
and no MENU return witness is available. Restoring the broad query's default path
budget hits the reader's output cap; it was not raised or repeatedly retried.
The conservative installed source control is
GAME_DIR/analysis/reporter-audit/child-effect-controls/verify-delivery-limits.mjs,
with verify-delivery-limits-reader020-engine070.log beside it. These positive
limit-reporting checks do not substitute for the unmet conditional-return control.

Reviewed delivery supersedes the earlier unmerged-candidate acceptance, not its
full request contract. No conditional-path absence or native error origin is
inferred. Complete linked-child acceptance needs bounded explicit input/path
work or fresh evidence; Gap 27 stays open. PR 63 merged its planning/test delta
only, so scoped-memory candidate acceptance remains unadopted. The published
engine's default backend remains handwritten; importing pypcode does not grant
hardware or whole-program fidelity.

## Published engine 0.8.0 pypcode cutover, 2026-10-02

Reviewed PR 66/release source 592088dbdb1cc8d804ba853eb39ae6ca1215577f
removes the handwritten backend. Registry engine 0.8.0 uses pypcode alone;
reader/checker and Capstone/pypcode versions remain as above. Archive hashes,
complete shipped source inventory, wheel/sdist source/test and installed source
match the release tag. Integrity evidence:
artifacts/package-delivery/engine080-registry/integrity.json.

Retained installed source controls and the conservative linked-child limit
control pass with PYTHONPATH absent. Their local logs/summary and preserved
previous-version reports are under
GAME_DIR/analysis/reporter-audit/registry-engine080-regression/. Installed
synthetic MZ routing, dependency rejection, prepared mismatch and conditional
target prefix/cap controls also pass. The existing source-case limits and full
gap contracts remain binding. No unresolved MENU return, complete fill, native
hardware or game parity is inferred from the backend change. No original was
run or emulated; no game spec/parity status changed.

## Scoped-memory mainline review, 2026-10-04

Toolkit PR 74 merged as 9b75c0e5b30a7e82792ef953e8d90530ea461aed.
Reviewed main 164ef6673c077022b729a9532d7ace8358241d4b contains explicit
callModels[].preservesMemory hypotheses and prepared protocol 3. The source
contract rejects malformed/unreachable declarations and unresolved, overlapping
or wrapping scopes, retains only declared bytes, keeps uncached bytes unknown,
and carries conditional provenance without establishing external-call effects.

The merged source's scoped-memory unittest suite passes using its isolated
PYTHONPATH with the existing locked Capstone/pypcode dependencies. Evidence:
artifacts/scoped-memory-main-review-controls.log. These are synthetic source
controls, not published-package or original-game acceptance. GitHub lists paired
reader/engine 2.0.0 releases and later engine releases; archive integrity, installed
source equivalence and complete requesting cases have not been checked here.

The earlier conflicting forward-port is preserved for historical comparison but
need not be resumed as missing implementation. Issue 73 and Gap 27 remain open
pending delivery acceptance and the complete Dark Sun nested caller-bracket case.
No adopted dependency, original report, spec claim or parity status changed.

## Isolated published protocol-3 delivery, 2026-10-04

Registry reader 2.0.0 and engine 2.0.0 were verified in ignored isolated storage;
project dependency locks remain unchanged. Engine wheel and sdist SHA-256 match
both PyPI metadata and the GitHub release digests. The complete shipped Python
and Java source inventory matches release commit
9b75c0e5b30a7e82792ef953e8d90530ea461aed byte for byte; sdist source matches the
wheel. Reader's resolved version and archive integrity match registry metadata.
Integrity evidence: artifacts/protocol3-delivery/integrity.json and its audit
scripts; npm package-lock.json records the reader archive and dependencies.

Published-engine scoped-memory controls pass. Release bridge tests were routed
through the installed reader's exported report API and installed engine, with
monorepo source fallback forbidden. Explicit near/far frame hypotheses, stopped
partial/default models, rejected declarations and budgets pass. Logs:
artifacts/protocol3-delivery/installed-memory-controls.log and
artifacts/protocol3-delivery/installed-bridge-controls.log. The engine import
resolves inside installed-engine and reports prepared protocol 3.

This verifies the bounded published delivery controls only. It does not establish
external service preservation, complete original-case acceptance, all toolkit
capabilities, or adoption into the restoration's locks. Gap 27 and issue 73 stay
open for the complete requesting Dark Sun caller-bracket case. The older
conflicting implementation candidates remain untouched.

## Published scoped-memory Dark Sun bracket control, 2026-10-04

Bootstrap facts validation passed before the bounded static rerun. The retained
caller-bracket control from the earlier candidate archive audit was routed
through isolated published reader/engine 2.0.0, with no PYTHONPATH source fallback.
The runtime import resolves to the isolated installed wheel. The reader config
now uses the documented executable xxh3 identity; no input range or analysis
budget was enlarged. Declared overlap is rejected before tracing by the published
validator. Its exact byte-bound error wording replaces the old generic limit
expectation; the initial assertion failure is retained separately.

The conditional joined bracket passes, together with default stopped behavior,
partial/wrong-segment/overlap rejection, step/path/unread controls and scope/byte
limits. Ordered writes before, within and after the child calls are checked.
Unknown external-service effects and conditional scope provenance remain in the
reports; no whole-function or native preservation claim follows.

Driver, configs and reports remain local-only under
GAME_DIR/analysis/reporter-audit/protocol3-scoped-memory-controls/.
Logs: artifacts/protocol3-delivery/original-bracket-controls.log and
original-bracket-controls-final.log. The retained source case starts at the
bracket and stops after its final clear; earlier mode/input gates and later
parent work remain unread. This verifies that bounded requesting control on
published tooling, not Gap 27's entire effect-ordering contract. Adopted project
dependency locks, spec entries and parity statuses remain unchanged.

## Adopted published protocol-3 pair, 2026-10-04

The prior retained regression inventory was replayed against isolated published
reader/engine 2.0.0. Every listed group passes; the generating driver resumes
only unfinished groups. New configs/reports/logs and the compared inventory:
GAME_DIR/analysis/reporter-audit/protocol3-retained-regression/summary.json.
Historical reports are preserved. Drivers adapt installed-package routing and
hash naming only; source assertions, hypotheses and analysis budgets remain.
Some console version labels are inherited literals and are not import evidence.

The child-effect control still lacks the MENU return witness under the shared
path budget. Conditional partial scans, separated fill windows and native
limits retain their earlier qualifications. Regression success closes no gap.

Matching reader and engine 2.0.0 are now adopted through exact npm integrity and
hash-locked Python wheel requirements. Capstone, pypcode and xxhash pins stay
unchanged. Prepared protocol 2 and other mismatches are rejected before source
access. Normal canonical validation, including locked restore, Release build
and assetless publish/smoke, passes; evidence:
artifacts/protocol3-delivery/adoption-validation-final.log. Candidate source is
not used by the adopted gate, and no game spec/parity status changed.

## Published continuation-budget MENU control, 2026-10-04

Toolkit PR 76 merged as 897f640dfa8d144caef23082be1d1ff3d78aba23 and is
published in engine 4.0.0. Registry wheel/sdist hashes match GitHub release
metadata; the complete shipped Python/Java source matches that commit. Integrity:
artifacts/continuation-budget-delivery/integrity.json. The isolated installed
runtime uses prepared protocol 3 with published reader 2.0.0. Adopted engine
2.0.0 remains unchanged.

Published synthetic continuation-budget controls pass; evidence:
artifacts/continuation-budget-delivery/synthetic-controls.log. The retained
conditional MENU input was rerun with an explicitly reported separate budget.
The ordinary path/step/visit inputs were unchanged. Ordinary paths and ordinary
gaps compare equal with continuations disabled and enabled. The declared MENU
child-return witness is reached conditionally; paths disabled and a continuation
step cap remove it, with limits reported. The unresolved ordinary computed jump
is retained and completeWithinModel remains false.

Driver, configs and reports:
GAME_DIR/analysis/reporter-audit/continuation-budget-controls/.
Result log: artifacts/continuation-budget-delivery/menu-controls.log.
This recovers the earlier missing conditional MENU witness using new delivered
tooling, not increased ordinary budgets or filtered paths. Runtime table inputs
and external-service effects remain hypotheses, and no native or whole-Gap 27
claim follows. The full fill case remains separate under toolkit ADR 0008.

## Published engine 4 broader release acceptance, 2026-10-04

The integrity-verified published sdist source suite runs 426 tests successfully
with one monorepo-only reader case skipped. Unicorn 2.1.4 is a test-only oracle
in the isolated delivery runtime, not an adopted runtime dependency. The suite
imports its sdist source; the earlier shipped-source inventory comparison proves
that source matches the wheel and release commit. Log:
artifacts/continuation-budget-delivery/full-source-tests-final.log.
The skipped PE32 bridge assertions were then executed through published reader
2.0.0 and installed engine 4.0.0, with checkout PYTHONPATH removed. Valid mapping
passes and a mismatched mapping rejects. Log: pe-bridge.log in the same directory.

The complete prior retained control inventory was attempted. Nineteen groups
pass with installed engine 4.0.0 and published reader 2.0.0. Two original drivers
remain failing and are not counted as passing or silently weakened:

- cleanup-hardware-effects expects a first-OUT stop. Delivered hardware-boundary
  events intentionally continue segmented instruction analysis, while the report
  retains completeWithinModel=false and effectOrdering.allPathsRead=false.
  New hardware-boundary/placement acceptance must replace the historical stop
  contract explicitly, including FND-CONFIG-192/198 case requirements in issue 109.
- child-effect-controls exceeds the reader's 32 MiB output cap with inherited
  default continuation settings. No complete report is produced. A separate copy
  with only continuationBudget.paths=0 passes every historical assertion, including
  unresolved ordinary dispatch and absent conditional child returns. This isolates
  the changed continuation workload; it does not validate the capped broad query.
  The independently passing bounded MENU positive remains the new conditional
  witness; no larger ordinary budgets or stitched reports were used.

Source configs/reports and inventory:
GAME_DIR/analysis/reporter-audit/engine4-retained-regression/summary.json.
Logs: artifacts/continuation-budget-delivery/retained-regression.log,
retained-regression-remainder.log and child-disabled.log. Historical reports are
preserved. Drivers adapt published-package routing and guarded hash naming;
the disabled child copy additionally declares its explicit zero-path budget.
Inherited console version labels describe old drivers, not loaded packages.

Adopted reader/engine 2.0.0 remains unchanged pending hardware and bounded
continuation migration acceptance. No gap, game claim or native behavior was
promoted. The goal remains active.

## Published hardware-boundary migration controls, 2026-10-04

The historical first-OUT-stop driver is preserved. A separate migrated driver
retains every initializer/handle assertion and verifies the delivered engine 4
hardware contract. In the FND-CONFIG-184 service, returning modeled paths reach
all seven explicit port events. Initial boundaries occur on every traced path;
later boundaries remain unresolved because stopped paths lack them. The signed
wrapper has a returning bypass, so its hardware placement is conditional.
Port event orders are disjoint from ordinary RAM write orders, and any path
containing them remains effect-incomplete with native reachability unconfirmed.
Unknown inputs stay unknown; supplied values are listed query assumptions.
Wrong input site/width rejects, and instruction/path/step caps remove positive
coverage. The mid-function suffix reaches its final ports on both retained skip
arms, then explicitly rejects its missing return frame at the epilogue. No
frame was fabricated and no local-return claim follows from that suffix.
Driver and reports: GAME_DIR/analysis/reporter-audit/engine4-retained-regression/
cleanup-hardware-effects/hardware-migration/. Result:
artifacts/continuation-budget-delivery/hardware-migration.log.

FND-CONFIG-192's complete local static port inventory passes, and a one-
instruction bound rejects complete inventory coverage. This is not dynamic
common/overlap transfer acceptance: slots, chains, masks, counts, scratch/native
mapping and aliases still need bounded explicit inputs. Gap 37 remains open.

A first-store prefix of FND-CONFIG-198 passes with different DS/SS, explicitly
equal DS/SS, and unknown DS. The final BX-addressed word store is DS-relative
in every case; its BP-derived offset expression is unchanged, while its physical
interval differs with the selected segment. Unknown DS stays unresolved.
A one-step cap removes the store witness. Balanced service-return and named
register-preservation hypotheses remain explicit; reserving the SS frame proves
no DS storage capacity or complete alias/callee safety. An initial longer
prefix exhausted the modeled-call path cap and produced no paths; the bounded
first-store query uses 32 steps, not enlarged limits or a stitched loop result.
Gap 39 remains partial, not closed.
Driver and reports: GAME_DIR/analysis/reporter-audit/engine4-case-controls/.
Result: artifacts/continuation-budget-delivery/case-controls.log.

These are installed published-engine controls, with no original execution or
emulation. Engine 2.0.0 stays adopted pending the outstanding full acceptance
contracts. No game spec/parity status changed.

## Bounded symbolic graphics primitive, 2026-10-04

An installed-engine 4 query starts FND-CONFIG-192's full local body with explicit
DS/SS/SP values and otherwise unknown slot/argument/scratch memory. It uses the
existing primitive limits (200 steps, 16 paths, 5,000 total steps, visit limit 4).
Four hardware sites are reached across stopped paths. All hardware placements
remain unresolved because stopped/dropped paths leave coverage open; there is
no returning path. Step/visit stops and path-limit gaps are explicit. Every
reached hardware site belongs to the complete static boundary inventory; input
values remain unknown, output interpretation disclaims rendered/device results,
and hardware orders stay separate from RAM writes. A one-step control removes
all hardware witnesses. No dropped path is accepted as a negative observation.

Controls: artifacts/continuation-budget-delivery/primitive-symbolic-controls.log.
Driver and reports: GAME_DIR/analysis/reporter-audit/engine4-case-controls/
verify-symbolic.mjs and primitive-symbolic*.json. These are conditional symbolic
instruction reports, not execution, emulation, accepted buffer state or pixels.

FND-CONFIG-193 supplies root-field producer contracts, but the released tracer
has register/flag/call/port hypotheses and no entry-memory value input. A producer
reading alone therefore does not make those memory values available to this
query. ADR 0008's decision against branch/path hypotheses is respected. The
supported way to express producer-established slot/stack/scratch values across
this entry remains a question on toolkit issue 109, rather than an invented
memory configuration or patched original. Do not repeat this capped full-body
query without new usable input tooling, an evidence-backed narrower entry or a
supported producer trace. Gap 37 and engine adoption remain open.

## Callee segment controls and published engine adoption, 2026-10-04

FND-CONFIG-198's bounded first-store prefix passes with DS preserved, explicitly
replaced, explicitly equal to SS, and unpreserved through returning call models.
The BX-addressed store uses the post-call DS, its offset expression is unchanged,
and its physical interval changes accordingly. An unpreserved DS stays unknown;
removing the unread-child model removes the store witness. Every returning
model retains unknown memory/flag effects, and all reports stay incomplete with
native reachability unconfirmed. Controls: callee-segments.log in
artifacts/continuation-budget-delivery; source driver/configs/reports:
GAME_DIR/analysis/reporter-audit/engine4-case-controls/verify-callee-segments.mjs.
No SS reservation, DS equality or register preservation establishes complete
storage capacity, alias safety or native callee behavior. Gap 39 stays open.

Published engine 4.0.0 is adopted by exact verified wheel hash with reader 2.0.0;
prepared protocol remains 3. Release adoption is supported by the complete
source suite/inventory, installed bridge, nineteen unchanged retained groups,
explicit cleanup hardware migration, disabled-continuation historical controls
and the bounded MENU positive. Game-specific gap contracts remain separate.
The earlier handover's requirement to finish every dynamic game contract before
package adoption was too broad; it did not identify a package regression.
Full canonical adoption validation:
artifacts/continuation-budget-delivery/adoption-validation.log.
No whole gap, game spec or parity status changes follow from adoption.

The canonical installed synthetic continuation control was migrated from its
historical shared-cap expectation. It now verifies a continuation can return
under the ordinary one-path cap, disabling continuations removes the witness,
its own one-step cap removes the witness, and ordinary paths/gaps compare equal
with continuations disabled. Full validation passes after this explicit contract
migration; no assertion was simply deleted. Test-only Unicorn is not installed
by the project's runtime requirements.

## Gap 39 full tooling-contract acceptance, 2026-10-04

Gap 39 is closed against adopted reader 2.0.0/engine 4.0.0. Its request is
correct effective-segment classification, distinct offset/segment provenance,
and explicit equality, alias and preservation assumptions. It does not require
native frame safety, full original execution or a complete 256-iteration loop.
The historical capped first-loop query and earlier partial dispositions remain
historical; closure uses the following additional evidence.

| Request | Authoritative acceptance |
| --- | --- |
| Every recorded frame-indexed access uses its final addressing register's default segment | A bounded hash-verified pass over FND-CONFIG-198's complete local range identifies all four BX word-access sites without overrides. Adopted source controls cover the first store and both second-loop loads and post-release store; all report effectiveSegmentRegister=ds. |
| BP-derived offset is separate from segment identity | Whole-entry first-store controls retain BP/entry-SP provenance. Narrow second-loop controls declare BP, BX, index and incoming AX base independently; unchanged numeric offsets yield different physical intervals with different DS. Missing AX leaves the second load's offset unresolved. |
| Reservation does not prove DS storage identity/capacity | Different and unknown DS cases remain distinct or unresolved despite the same SS/frame values. Queries remain incomplete and make no capacity claim. Synthetic BP/SS override cases retain SS separately. |
| DS/SS equality and aliases remain explicit | Adopted synthetic tests cover unknown, different and explicitly equal segments, equality created by instructions, and a DS write invalidating potentially aliased frame bytes instead of merging them. Source equality is a stated register hypothesis, not inferred from BP. |
| Callee preservation remains explicit | First-store and second-loop source controls preserve, replace or lose DS through labelled balanced return models. The post-call store follows returned DS; unread child and step controls remove the store witness. Models retain unknown memory/flags, never native service safety. |
| Fixtures say what they test | Six complete published EffectiveSegmentTests run against the installed adopted wheel (implementation origin asserted), with synthetic instructions/state only. Original cases are static reports, not execution or emulator fixtures. |

Source controls: GAME_DIR/analysis/reporter-audit/engine4-case-controls/
verify-second-loop.mjs and its second-loop*.json reports. BX boundary inventory
contains only site, width, access role, base and override fields, not original
bytes or disassembly, and remains local-only. Results:
artifacts/continuation-budget-delivery/second-loop.log and
installed-segment-suite.log; earlier case-controls.log and callee-segments.log
retain the first-store and callee controls.

The second-loop entry is a verified access boundary inside the existing finding's
range. Its BP/index/BX/AX and segment values are explicit suffix hypotheses, not
state inherited from a separately run window. Both release/bypass arms reach the
local epilogue, whose missing root frame is rejected; no returning-original
claim is made. Dynamic trace reports are used because effects presentation
intentionally filters ordinary reads. No original run, emulation, game finding
or parity status changed. Gap 37 and native alias/capacity research remain open.

## Cleanup assignment predecessor controls, 2026-10-04

Gap 40 remains open. Adopted reader 2.0.0/engine 4.0.0 now passes bounded
FND-CONFIG-199 controls starting at the first request's verified call boundary.
The complete local bounds map confirms both requests, transfer and cleanup calls.
First failure, second failure and two-acquisition cases are independent queries,
with explicit BP/SP/segment/register and balanced-service hypotheses. Six SS/BP
bytes covering the flag and handle slots may be preserved across services;
this preserves existing producers only and never initializes unread bytes.

First-request FFFF skips the second request and its own slot store. Cleanup's
first slot reads FFFF; the second reads unknown with two missing byte producers
and no preceding own slot write. Both unknown-sentinel release/bypass outcomes
are reported. Second-request FFFF and two acquisitions retain their own slot
assignments under the declared scopes and follow the recorded release/transfer
sequence. A zero first-handle hypothesis also reaches release: the encoded
cleanup predicates compare with FFFF, not a general admitted-index check.
This does not establish a native invalid release or a reproduced defect.

Unscoped returning services invalidate the earlier slot producer. Unread-service
and one-step controls remove the cleanup witness. All primary suffix cases have
no dropped-path gaps but stop at the epilogue's missing root frame; no frame was
fabricated and no original return is claimed.

The shared per-byte lastWriter controls report the appropriate writer on each
assigned predecessor and unwritten entry state on first failure. Every observed
read occurrence satisfies its declared writer alternatives, but every overall
control stays undecided because the suffix paths stop before their ends.
Wrong writer declarations reject; the step cap stays undecided with no accepted
complete assertion. These occurrence facts are not a held whole-query result.

Complete published LastWriterTests pass against the adopted wheel (implementation
origin asserted). Synthetic instructions only; no original execution/emulation.
Logs: artifacts/continuation-budget-delivery/cleanup-predecessors.log,
cleanup-relational.log and installed-writer-suite.log. Source drivers/configs/
reports: GAME_DIR/analysis/reporter-audit/cleanup-predecessor-controls/.
The closure condition still requires supported connected-entry/control evidence;
do not invent a root frame, inherit successful-path assignments on bypasses,
filter stopped paths or label the overall controls held.

## Gap 29 source-case progress controls, 2026-10-04

The adopted engine 6.2.0 / reader 2.0.0 runs the retained FND-SCRIPT-022
collision-increment windows and FND-CONFIG-161 final-poll windows. Source guards
are XXH3; the legacy local configs are normalized before the published reader.
No original execution, emulation or game evidence promotion occurred.

| Full request | Passing evidence | Remaining qualification |
|---|---|---|
| Wrapped arithmetic before a progress claim | A supplied word end of 65535 stores candidate zero after word increment and zero extension; 65534 stores 65535. Every retained path has that first four-byte store. A one-step cap removes the store witness. | Entry registers are explicit conditional hypotheses. The later cache fields and found byte remain unknown; no native collision or complete search is established. |
| Restart edges and state changes | The original scan backedge and nested replacement-age backedge are retained. The final external-poll model with result bit set reaches its original polling backedge. | Visit/path stops remain explicit. Slot count does not prove a bounded restarted search. |
| Comparison signedness | Slot interval gates are explicitly unsigned and the nested age gate signed in iteration rows. The common room branch retains its signed jge predicate with two 32-bit operands. | The capacity operand is unresolved; this is not a supplied high-bit-capacity example or a claim that the ordinary producer reaches it. |
| External calls and repeated state | Poll result zero reaches the final clearing store without a restart; result one restarts and never reaches that store. The iteration reports memoryForgotten with invalidated stack bytes and no stateRepeatsArrival. Step caps remove the clearing/backedge witnesses. | Both narrow windows remain incomplete: the zero case lacks a proven root return frame; the one case stops at the repeat bound. The unknown-memory model forbids inferring a repeated whole state from unchanged registers. |
| Repeated no-op invalidation | Published synthetic no-op/repeated-state and possibly-aliasing-write controls pass in the verified release suite. | The Dark Sun example requires known slot starts, signed ages and capacity across the connected scan/eviction route. Current unknown-memory queries do not supply them. A generic synthetic success does not satisfy this source case. |

Gap 29 remains open for its complete source contract, particularly the bounded
no-op eviction/restarted-search case and its input provenance. FND-SCRIPT-022
already distinguishes the conditional high-bit-capacity example from the known
ordinary initializer; no finding or queue item is changed here. Do not repeat
the old unknown-input query with larger limits or infer cache contents from a
register-only window. Retry needs an evidence-bounded connected route or supported
inputs. The existing issue 143 input-design discussion is conditional on that
evidence; these windows alone do not establish an upstream input bug.

Drivers, configs and reports stay in
GAME_DIR/analysis/reporter-audit/loop-progress-controls/. Compact current logs:
artifacts/issue-response-review/loop-acceptance.log and loop-final-controls.log.
The earlier installed-loop-suite.log tested engine 4.0.0; current engine 6.2.0
loop controls are included in the verified published release suite recorded in
artifacts/issue144/release-suite.log. Tracker 114 receives the qualified consumer
result rather than a duplicate feature request.

Canonical Test.ps1 -NoRestore passes: artifacts/issue-response-review/loop-root-gate.log.
The qualified consumer result was posted to toolkit issue 114; no duplicate issue was opened.

## Gap 30: runtime-mode and pre-store guard acceptance, 2026-10-04

The complete requesting tooling contract passes on adopted engine 6.2.0 /
reader 2.0.0, using FND-CONFIG-163 and FND-CONFIG-061 as read-only inputs.
Gap 30 is closed; no original-game claim or parity status changes. The exit is
retention of conditional guards, runtime mode, ordered dependencies and unresolved
external outcomes, not proof that cleanup or operating-system termination succeeds.

| Requirement | Authoritative source-case control |
|---|---|
| Keep the pre-store guard | The complete setter query splits at its unsigned-below stack guard. The bypass path stores the four-byte supplied pointer and returns; the other calls the guard before any pointer store and stops at its diagnostic interrupt. The ordered effect timeline and path guard retain both alternatives. |
| Carry mode through cleanup | The actual far wrapper pushes literal mode one and zero middle argument, then enters the actual near runtime body. Its reads and path guards retain those values before and after two declared balanced service returns. Each service preserves only the saved SI/BP, near return and three argument words; outside memory and flags remain unknown. The service summaries explicitly retain modeled-return status, unknownEffects and same-path conditionalModel scope references. |
| Identify reached/bypassed callbacks and indirect calls | Mode one bypasses the recorded exit-callback table and the indirect calls through the three fields named in FND-CONFIG-163. The distinct original mode-zero wrapper provides a nonvacuous contrast: with unknown callback count it reaches either the callback-table dependency or the subsequent local cleanup call, then stops at those unread dependencies. No actual callback target/count or native table traversal is invented. Complete-body boundary metadata verifies the selected call sites. |
| Separate requests, return and success | Under the stated service hypotheses, mode one reaches the recorded interrupt with AH selecting DOS termination and unknown AL. The report stops at the unmodeled handler, has no returned path and stays completeWithinModel=false/nativeReachability=unconfirmed. The setter's diagnostic interrupt is conditional and separate. No wrapper return instruction or hardware placement is interpreted as successful termination, complete cleanup or fallback behavior. |
| Reject missing provenance and insufficient coverage | A one-step query and the unread-service query remove the termination witness. Removing scopes loses the return-frame/argument provenance: a path stops at the unknown return target and termination placement becomes unresolved. All controls retain their gaps/stops; no larger budget or state stitching is used. |

The positive is a connected wrapper-to-runtime-to-termination-prefix query,
not a stitched guard-interrupt continuation. The setter query correctly stops
at its earlier diagnostic interrupt; the encoded route recorded in the existing
finding does not license assuming that handler returns. Only the termination
prefix through the first interrupt is declared; its uncharacterized fallback
is excluded and never reported read. The unknown service effects remain part
of the accepted report, as the request requires.

Local source configs/reports and verified neutral boundary metadata:
GAME_DIR/analysis/reporter-audit/runtime-mode-controls/. Assertions and concise
result: artifacts/issue-response-review/verify-runtime-mode.mjs and
runtime-mode-acceptance.log. Exact installed dependency/source provenance is the
engine 6.2.0 adoption recorded in TOOLKIT-RESPONSE-ACCEPTANCE.md. No original
runtime/emulation, game spec or other goal's queue work occurred.

Canonical Test.ps1 -NoRestore passes: artifacts/issue-response-review/runtime-mode-root-gate.log.
The additional scoped-memory consumer result was posted to existing toolkit issue 73 after duplicate review.

## Published reader 2.1.0 / engine 7.3.0 adoption, 2026-10-04

Exact npm integrity and hashed Python requirements now pin reader 2.1.0 and
engine 7.3.0. The wheel, sdist, installed production/test packages and release
cfe7c4e8d97c521620059f2a35d4d25c5dc4db09 agree byte for byte for engine code;
the locked npm tarball agrees with installed reader files. The published engine
suite passes with the installed package, including the Node PE bridge test,
without PYTHONPATH source fallback. Verification logs are under
artifacts/engine73/. Checker 0.2.0, runtime dependency pins, template, local
upstream rules and shared .NET packages remain unchanged.

The intervening releases add observed entry frames, caller-byte read provenance,
caller-return inventory continuation, Ghidra flow refinements and symbolic-base
memory scopes. Prepared protocol remains 3. Scope reports now include interval;
symbolic offset/linear bounds are null. Consumer assertions use interval rather
than treating those null fields as concrete addresses. Engine 7's Ghidra report
changes include ghidraContinues and falls-through/basis fields; local wrappers
do not consume the changed count or assume unconditional jumps fall through.
The published tests exercise the revised agreement contract. Unknown segments,
possibly overlapping scope bases, partial preservation, alias writes, wrong
writers and entry caps remain conservative failures or undecided results.

### Gap 35: entire tooling request accepted

FND-CONFIG-179/180 remain read-only acceptance inputs. Their existing supersession
and active citations already replace the incorrect historical grouping.
The actual registration caller now exposes every consumed offset/width:
window 0/4, identifier 4/2, callback 6/4 and mask 10/2. Each has positive
fromCallerOnPaths. Two consuming frames settle with twelve mapped argument
bytes; two null-window bypass frames stay open, so site agreed remains false
while widthsConsistent is true. Callback grouping uses two adjacent words at
6 and 8; mask is its own word at 10. The former grouping starting at 8 is absent.

This case conditionally models the two setter calls as balanced far returns,
preserving the explicitly scoped saved frame/arguments and stated registers.
DS and SS are concrete query hypotheses; SP and BP stay unknown. These models
are not native setter-preservation evidence. Removing scopes loses the mask's
caller-byte provenance and settles no registration frame; a one-step cap
removes the known call witness. The separate unmodeled registration case retains
window and identifier derived reads in the real resident setter, two frames
down. It is not stitched to the modeled continuation. Published near/far,
address-wrap, widening, forwarding, overlapping-width, slot-reuse and unread
controls pass. Together these satisfy the shared reporting request; Gap 35 is
removed. Whole caller coverage, native execution and successful registration
are not established: path gaps and bypasses remain explicit.

Local configurations/reports stay under
GAME_DIR/analysis/reporter-audit/issue-response-review/; drivers and logs are
artifacts/engine73/verify-gap35-exit.mjs, gap35-exit.log and arguments.log.

### Issue 145 delivered; Gap 40 remains open

The exact issue reproducer now establishes its observed frame and holds its
control, with no concrete root SP/BP. The original late-entry cleanup cases
no longer stop at unresolved scope bases. Their prefix still exceeds the
recorded path budget, so entryFrame is not established for that entry.
A verified post-prologue instruction boundary instead gives one prefix path,
SP -18 and BP -2 relative to unknown root SP, established in all three cases.
Scoped suffixes retain assignment provenance and return on every retained path.
First-failure, second-failure and two-acquisition whole controls are still
undecided: path drops and modeled bypasses prevent whole-query acceptance.
Neither the limits nor original call hypotheses were broadened.

The capped prefix fails frame establishment. Removing scopes rejects whole
acceptance; unknown SS retains the unresolved-scope stop. Published partial,
wrong-writer, alias and overlap controls pass. This satisfies issue 145's
composition exit, separately from Gap 40's complete predecessor contract.
No frame memory is inferred from entryFrame, no native run is claimed, and
no dropped path is silently accepted. Local reports are under
GAME_DIR/analysis/reporter-audit/cleanup-predecessor-controls/connected-entry/engine730/;
source-composition.log and composition-negatives.log record the assertions.
Issue 143 remains open: observed frames do not supply producer memory or settle
hardware placement along the incomplete connected route.

## Issue 143 response and Gap 31 boundary audit, 2026-10-04

Issue 143's requested consumer evidence remains the actual connected
initializer-to-transfer route, call depth, later slot/count/mask writers and
external inputs. A disconnected initializer does not prove the continuous
recipe impossible. The published migration and observed-frame helper are
adopted; no entry-memory design is demanded from incomplete route evidence.

FND-CONFIG-192's verified saved-register boundary now establishes SP -8 and
BP -2 relative to unknown root SP, with unknown prefix memory. The original
200-step/16-path/5000-total-step/visit-limit-4 bounds remain unchanged. The
whole query still stops at step/visit limits, drops paths and leaves reached
hardware placement unresolved. A one-step control prevents frame establishment
and reaches no hardware boundary. This is helper acceptance, not Gap 37 closure.
The command is effects with entryFrame.from, a complete declared region,
XXH3 source guard and explicit DS/SS hypotheses, without SP/BP or memory rows.
Local reports are transfer-frame730 and transfer-frame-cap730 under
GAME_DIR/analysis/reporter-audit/issue-response-review/; the assertion summary
is artifacts/engine73/transfer-frame.log. The response is recorded on
[issue 143](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/143#issuecomment-5983733344).

Gap 31's connected source prefix on reader 2.1.0/engine 7.3.0 retains the same
far scratch pointer twice, then stops at INT 33h before output writes and
BX-to-AX copy. Independent BX 0/1 controls preserve the register origin,
while missing BX leaves AX unknown; they are not connected to the prefix.
The witness-removing prefix cap and unused callModel-at-INT diagnostic pass.
Original call storage/driver effects and result sequences remain conditions.
No ordered-overwrite or caller-predicate whole control is accepted.

The exact-release handbook maps Gap 31 to lastWriter after both stores and
origin on the loop predicate using a modeled service register, but a CALL
model cannot continue the interrupt; modeling the whole wrapper skips the
writes. Post-interrupt entryFrame cannot carry caller-built pointers or saved
memory. The all-state duplicate check found adjacent issues 7, 73, 109, 136
and 143, without this connected effect-contract ask. Focused
[issue 190](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/190)
asks for a supported bounded recipe or accurate unsupported-case guidance;
any conditional hardware-return input needs its own design. The documented
hard stop is not reported as a bug. Gap 31 remains open.

Source configs/reports stay under GAME_DIR/analysis/reporter-audit/poll-alias-controls/.
The installed rerun summaries are artifacts/engine73/poll-prefix.log and
poll-controls.log. No original execution, frame-memory invention, window
stitching, spec/parity change or raised budget occurred.
