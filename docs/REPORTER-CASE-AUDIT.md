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
