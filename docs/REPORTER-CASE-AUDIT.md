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

