# Toolkit response acceptance

## 2026-10-04: issues 108, 109 and 110

The owner responses on toolkit issues
[108](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/108),
[109](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/109) and
[110](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/110)
were checked against the consumer, not treated as closure from toolkit tests.
This is a tooling acceptance batch; no game behavior or spec status changes.

### Shared runtime: issue 108

Dark Sun pins Core and LegacyFormats exactly at 2.0.0 in Directory.Build.props.
The committed project and packaging lock files record the NuGet content hashes.
The migration commits are cc48ccf (content verification) and 2496721 (settings,
consumer PR 4). The packages use XXH3-128 asset identities; source verification
uses LegacyFormats. No vendored implementation replaces the package imports.

| Slice | Consumer disposition and positive control |
|---|---|
| R1 portable paths | Adopted. Extractor inventory, region catalog, startup extraction and Resources asset references call PortableAssetPath.Relative. SourceCorpusInventoryTests and StartupAssetExtractorTests exercise known source paths and source-mapped extracted output. |
| R2 persistence/settings | Adopted. LaunchSettingsStore uses RecoverableFile.Write, ReadBounded and SettingsRecovery.Select, retaining the game's serializer and field admission as the runtime guide prescribes. LaunchSettingsStoreTests cover round trips, backup retention, corrupt-primary recovery and rejected oversized/version-invalid input. |
| R3 PCM/audio/WAVE | No PCM/WAVE conversion or playback consumer was found in src or the Inspect tooling. Not claimed as adopted or tested. |
| R4 input | Not migrated: Game retains previous KeyboardState and local chord/edge helpers. A separate implementation batch must preserve existing input semantics while adopting InputState/InputBindings and run the game's binding controls. The current research-side goal does not authorize changes to src. |

The full consumer Test.ps1 -NoRestore gate passed. Its log is
artifacts/issue108-root-gate.log. Toolkit release status is resolved, but the
consumer-wide issue is not closed while R4 remains outstanding. No new consumer
PR or push was made in this batch.

### Transfer inputs and segment reporting: issue 109

Upstream accepts Gap 39 as closed. Its existing installed-wheel regression
controls and the complete reporting-contract audit remain authoritative.
Gap 37 remains open. Adopted engine is 4.0.0, reader 2.0.0; an earlier issue
comment saying consumer engine 2.0.0 is historical, not current.

A separate candidate environment installed the published engine 6.1.1 wheel,
SHA-256 1b88ff60e41f926f96cc9d557577e25ee1b018d47b50bb6d1e08194a0b3c5e1d,
and published reader 2.1.0. These versions were not substituted into the
project's pins. Both use prepared protocol 3; runtime dependencies remain
Capstone 5.0.7, pypcode 4.0.0 and xxhash 4.0.1. The source was checked by
Bootstrap-Project.ps1 -ValidateFactsOnly and each query's XXH3 identity guard.
No original execution or emulation occurred.

The supported producer recipe is now the next step: one connected trace before
the producers; failing that an evidenced register-only entry; or returned
registers and bounded preserved frame bytes for unread calls. A control's
assume input does not steer paths. Entry-memory inputs would need a separate
ADR and protocol migration; they are not required or promised by this response.

Candidate controls used effects, explicit DS/SS/SP, unknown initial memory,
200 steps per path, 16 paths, 5000 total steps and visit limit 4:

- The complete FND-CONFIG-192 body again reaches four hardware sites only on
  stopped paths. Placement remains unresolved; step/visit stops and dropped
  paths remain explicit. Its serialized report is 7778355 bytes.
- An entry at the complete FND-CONFIG-193 initializer, declaring both producer
  and consumer regions, stops at the initializer's loop visit limit after 44
  steps, before any transfer event. Its serialized report is 54798 bytes.
  A one-step negative control stops without reaching the consumer.
- This is not a connected caller-tree acceptance: declaring two regions does
  not create an edge between them. The recorded startup call precedes later
  requests, but a complete route from that caller to the selected transfer and
  its later slot/count/mask writers is not established by FND-CONFIG-193.
  No number of intervening calls or external-input admission is invented.

Do not raise the old budgets or stitch these reports. The next acceptance
needs a supported connected caller entry and writer/input coverage; the new
producer diagnostic alone does not prove the supported recipe impossible.

### Argument frames: issue 110

The same candidate pair ran arguments on the saved FND-CONFIG-179 caller and
FND-CONFIG-180 consumer configs, with sourceKind mz, returnBytes 4, entry and
evidence-labelled regions. The complete caller retains its existing bounds:
1000 steps, 5000 total steps, 64 paths. The consumer-only config retains
256 steps, 1500 total steps, 16 paths and two explicit balanced-return,
register-preservation call models; unknown memory is never silently preserved.

The known registration call has argumentFrames and an argumentFrameSites row.
The report recognizes the far window (offset 0, width 4), identifier (offset 4,
width 2), and far callback (offset 6, width 4) on the paths that consume them.
The site does not agree: bypass paths leave callback/mask slots unread, and
other paths stop in unread callees. Forwarded window/identifier reads into a
deeper frame are retained. The full caller also retains path-limit gaps.
The consumer-only query has no argument frame for its modeled setters, so it
is not passed off as a caller-to-callee positive control.

Assertions require an actual known call frame and a consumed four-byte callback
grouping, and require the unresolved site to remain disagreed. Gap 35 stays
open; no hand-reading workaround was deleted. Closure requires settled widths
and forwarding controls against adopted packages, not this qualified candidate.

Reproduction: set EVIDENCE_PYTHON to the isolated candidate Python, then run
node artifacts/issue-response-review/run-cases.mjs. Configs and original-derived
reports remain only in GAME_DIR/analysis/reporter-audit/issue-response-review;
the driver and log are ignored local artifacts. The command invokes the
published reader's arguments/effects commands. No proprietary report is posted
upstream or committed.

## 2026-10-04: issue 113 and connected cleanup frames

The new [issue 113 response](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/113)
accepts the per-read predecessor results but identifies the missing suffix
return frame as a toolkit limitation. Its proposed entryFrame field is in
[PR 140](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/pull/140),
head ff38793c09b9957b9ebe068c7aa021a5a6034b65, still open when checked.
PyPI still publishes engine 6.1.1. No candidate code was adopted.

### Adopted connected-entry controls

Reader 2.0.0 and engine 4.0.0 traced the complete FND-CONFIG-199 caller from
its actual prologue, independently for first-request failure, second-request
failure and two acquisitions. The original bounds remain 1000 steps per path,
5000 total steps and 64 paths. Explicit DS/SS/SP inputs and balanced returning
service/register hypotheses remain conditional, not native facts.

The bounded preservation hypothesis covers thirty SS/BP-relative bytes:
twelve local bytes, saved SI/DI and BP, the far return frame and eight argument
bytes. It retains existing bytes only; unknown inputs and unwritten locals
stay unknown, and flags and memory outside the scope remain unknown. The
prologue runs in the same query; no suffix receives invented saved contents.

Every retained path now returns without the former missing-root-frame stop.
Observed lastWriter occurrences satisfy the declared writers. First-request
failure leaves both bytes of the second cleanup word without a local writer;
later acquisition cases retain the second assignment. Nonetheless, both whole
controls remain undecided: path-limit gaps remain, and returned paths which
bypass the anchor crossed modeled callees. A held observed occurrence does not
prove the whole control. Gap 40 remains open.

The one-step and unread-service controls remove the cleanup witness. Removing
memory scopes loses the earlier assignment's value. A contradictory second-slot
writer declaration fails the report. These assertions run in the ignored local
connected-entry driver; results are in
artifacts/continuation-budget-delivery/connected-cleanup.log.

### Unreleased entryFrame composition control

The exact PR 140 source was exported into an isolated candidate directory,
without altering a checkout or production locks. Published reader 2.1.0 ran
that source with the verified published engine dependencies. The candidate's
complete EntryFrameTests pass; a separate synthetic control verifies its import
comes from that candidate, rather than claiming a published-wheel result.

The actual candidate query starts at the first request and names the complete
caller as entryFrame.from. Its region declares both entries; SP/BP inputs are
omitted as ADR 0012 requires. DS/SS/SI/DI, models, scopes and the original
bounds are explicit. The prefix crosses frame setup, the stack-limit service,
optional image/frame-reader work and coordinate gates before acquisition.

All three scoped cases leave entryFrame unestablished. The prefix stops at
the stack-limit or frame-reader service; the suffix stops at its modeled
request. The diagnostic is "preservesMemory address unresolved: segment and
base must be concrete before the call". ADR 0012 observes a frame relative to
unknown entry SP and rejects supplied SP/BP; the scope resolver requires a
concrete base. Removing scopes instead leaves prefix path-limit gaps and loses
saved-slot provenance, so it is not accepted as a workaround.

A synthetic prologue, modeled service, narrower assignment/read and epilogue
isolates the interaction. Without a memory scope, the frame is established,
all paths return and the writer control holds. Adding only a bounded scope for
four locals and saved BP makes the frame unestablished and the control
undecided. Supplying concrete SP with entryFrame is rejected. The three
interaction controls pass against the exact candidate source; no original
bytes or instructions appear in that synthetic case.

The sanitized source case and reproducible synthetic control were added to
[issue 113](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/113#issuecomment-5982560966),
rather than opening a duplicate. Logs are entry-frame-cleanup.log,
entry-frame-suite.log and entry-frame-scope.log under
artifacts/continuation-budget-delivery. Original-derived configs/reports stay
only in GAME_DIR/analysis/reporter-audit/cleanup-predecessor-controls/connected-entry.
No original execution/emulation, spec claim or parity status changed.

Next acceptance needs a delivered supported way to compose observed frames
with bounded saved-memory scopes, then the remaining prefix coverage. Do not
retry the same capped connected body with larger bounds, silently drop scopes,
or count an unreleased synthetic pass as consumer closure.

## 2026-10-04: downstream-request tracker cleanup

The owner explicitly requested reviewing the open downstream-request issues
from Dark Sun, closing resolved or mostly resolved trackers and replacing the
latter with focused remaining upstream asks. The label inventory contained
108, 109, 110, 111, 113 and 114. Issue 111's requests are from sub-culture-max
and enemy-reinfestation, so it was left open and unchanged.

| Original | Disposition | Remaining work |
|---|---|---|
| 108 shared runtime | Closed completed for delivered upstream feature work. | Dark Sun input migration remains consumer work; no toolkit defect or new implementation ask was found. R1/R2 accepted, R3 not applicable. |
| 109 hardware/segments | Closed as superseded, not as full consumer acceptance. | [143](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/143): transfer starting-state support/decision and placement acceptance; Gap 39 remains accepted closed. |
| 110 argument frames | Closed as superseded. | [144](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/144): release PR 142's consistent observed-width fields and verify the Dark Sun case. |
| 113 relational controls | Closed as superseded. | [145](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/145): observed frame/saved-memory scope composition, with synthetic reproduction and consumer acceptance. |
| 114 loop reporting | Closed completed for delivered upstream feature work. | Local Gap 29 source-case validation remains; no demonstrated outstanding upstream implementation ask. |

Before creating replacements, all open issues and focused all-state searches
were checked. The overlapping reports were the three original trackers and
closed issue 73; there was no separate focused issue for the remaining asks.
PRs 140 and 142 were still open. Follow-ups link their existing implementation
work and request delivery/acceptance, rather than duplicate implementation.

Issue 143 explicitly preserves the consumer provenance needed before any new
entry-memory design decision; it does not call unresolved placement a bug or
demand that input unconditionally. Issue 144 retains open bypass frames and
the distinction between agreed frames and consistent observed reads. Issue 145
includes the candidate scope interaction, not just a request to rerun tests.

The resulting open labelled set was verified as 111, 143, 144 and 145. Closure
comments explain the handoffs and retained local work. Administrative closure
does not change gaps.md, any goal acceptance-ledger status, a spec claim or
parity. Local gaps 29, 35, 37 and 40, and the other consumer-only relational
cases, remain open until their entire contracts pass. No pushes occurred.

## 2026-10-04: compact published bridge removes wrapper transport failure

Reader 2.1.0 / published engine 6.1.1 was tested in the isolated, verified
installation, with no candidate source override. The source identity and
Bootstrap-Project.ps1 -ValidateFactsOnly gate pass. The FND-CONFIG-191 wrapper
query declares both actual validator/getter traversals, no call models, and
retains the existing 200-step, 64-path, 20000-total-step and visit-limit-4 bounds.
The same config was sent through adopted reader 2.0.0 / engine 4.0.0 and the
isolated published pair. The reader's spawn call was observed without changing
its arguments, output cap, timeout or returned data.

The adopted bridge reproduces ENOBUFS at its unchanged 33554432-byte limit,
producing no complete report. The published compact bridge carries 15673737
bytes and parses a report; formatting that returned object with two-space
indentation would take 36502840 bytes. This demonstrates the transport benefit
without claiming the old aborted report was semantically compared in full.

The produced report retains a step-limit stop and completeWithinModel=false,
despite no dropped-path gaps. Native reachability stays unconfirmed. Producing
a report is not complete wrapper/primitive coverage or closure of Gap 27.
This supersedes only the historical inability to obtain that bounded broad
report; original-case control coverage and adoption remain separate work.

Local reproduction: prepare-compact-wrapper.py and compact-wrapper.log under
artifacts/issue-response-review; configs/reports and the driver stay in
GAME_DIR/analysis/reporter-audit/compact-wrapper-controls. Production pins,
original content, game spec/parity and runtime remain untouched.

## Issue 144: published argument-width consistency adoption, 2026-10-04

Engine 6.2.0 is adopted with existing reader 2.0.0 and prepared protocol 3.
The wheel hash is pinned in tools/evidence/requirements.txt. Published wheel,
sdist, installed package and release bf46e8aa2a2d4dbe35a65fe8bee86a7d279323c6
source agree. The published source suite passes, including the installed reader
PE bridge, nonvacuous width/grouping conflicts, bypasses, no-read and window-limit
controls. Unicorn is isolated as a synthetic test oracle, not a runtime dependency.
Integrity and validation logs are under artifacts/issue144/.

The FND-CONFIG-179 caller plus FND-CONFIG-180 callee query uses the unchanged
bounded config from issue 144, without entry-memory values or modeled calls.
The registration site has six traced frames. readWidths contains offset 0 width
4 on six paths, offset 4 width 2 on six paths, and offset 6 width 4 on four
paths; each grouping is consumed width only. widthsConsistent is true and
conflictingWidths is empty. agreed stays false: every frame remains unsettled,
including bypasses with unread slots. The report remains incomplete with
path-limit gaps and unread callees. A one-step control removes the call witness.
The conditional-setter-only query is not the positive case. Proprietary configs
and reports stay under GAME_DIR/analysis/reporter-audit/issue-response-review/;
consumer assertions and a compact summary are in artifacts/issue144/consumer.log.

Engine upgrade migration: repository wrappers do not consume removed inline
preservedMemoryScopes or Ghidra counts. Connected cleanup predecessor controls
pass on the installed release, retaining their undecided whole-query controls.
A separate migration check resolves call-return conditionalModel indices into
the same path's conditionalModels and verifies scoped entries are nonempty;
inline copies are absent. Ghidra fall-through disagreement controls pass in the
published suite; no false agreement was accepted or original export rewritten.
The bridge's compact transport retains its existing output limit.

Issue 144's released-feature and bounded consumer exit is satisfied. This does
not establish unread arguments, complete native behavior, or the full Gap 35
contract; gaps.md and the goal ledger retain Gap 35 pending that broader exit.
No game spec, parity status, gameplay or original runtime changes.

Canonical Test.ps1 -NoRestore passes: artifacts/issue144/root-gate.log. Issue 144
is closed as completed after the verified response; the broader ledger is unchanged.

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

Canonical Test.ps1 -NoRestore passes (artifacts/engine73/root-gate.log).
Retained runtime-mode and loop controls pass again on the new installed engine;
logs are runtime-regression.log and loop-regression.log. Issue 145 is closed
with [verified consumer feedback](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/145#issuecomment-5983684370);
issue 144 has [full Gap 35 feedback](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/144#issuecomment-5983684860).

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

## Gap 32 guard and reload acceptance, 2026-10-04

Reader 2.1.0/engine 7.3.0 now passes stronger FND-CONFIG-165/171 source controls.
The complete finding spans, hash guard and post-stack-guard entry boundary are
verified. No game spec or parity changes follow from these tooling checks.

| Contract | Verified consumer result | Qualification |
|---|---|---|
| Metadata access precedes validation | Every metadata-wrapper branch retains the two-byte read before the null guard, with no protecting guard attached. An order assertion naming the later guard fails. | This describes local ordering, not a reachable native invalid-pointer access. |
| Failure does not suppress the runtime request | Null and marker-failure paths retain the flag-one write before the same runtime call; the matching branch retains its byte clear. All three conditional paths return without dropped paths. | Runtime return and saved-frame scopes are explicit hypotheses. Unknown marker storage can alias the saved frame; returned paths alone do not prove its preservation. |
| Cleared return versus release success | A supplied rejected AX/DX result is overwritten by the outer zero result on each path. | The modeled result is not a native error origin or successful release. FND-CONFIG-167's recorded runtime result analysis remains the read-only input, not a new claim. |
| Checked snapshot versus reload | Both callback sites retain the preceding nonnull gate, bracket call and fresh four-byte target read. Frame-only scopes leave identity unknown; separate target-field plus DS preservation yields sameTargetValue and held order occurrences. | Target preservation is an explicit four-byte hypothesis, not actual callee preservation. Intervening calls remain in relational occurrences. |
| Callback result branch coverage | Zero modeled callback results cover all four gate combinations; nonzero cases retain zero bypass and FFFF local returns. No path gaps at the original limits. | The narrower query establishes only observed SP/BP offsets. Prefix memory is not copied. Scope hypotheses and supplied returns remain visible. |
| Nonvacuous controls | Unread runtime call stops before return; one-step caps remove read/frame/call witnesses. Unscoped metadata cases lose saved-BP identity. Removing DS loses target identity, and DS scopes without DS preservation are rejected. | A root return or conditional completeWithinModel is not native state preservation or successful callback execution. |

**Gap 32 remains open.** Stable-target occurrences hold, but whole relational
controls remain undecided: unread modeled callees on anchor-bypass paths could
contain the anchor. The published tracker requires controlled consumer exits;
per-occurrence success is not a whole-control closure. The next acceptance
needs actual bracket/callee coverage and field/segment producers, without
larger budgets or stitched windows.

The callback query uses entryFrame.from with a verified post-stack-guard entry,
explicit DS/SS/DI hypotheses, unknown SP/BP and bounded frame scopes. The stable
variant additionally scopes only the four-byte callback field. Existing limits
are unchanged: 256 steps/path, 16 paths and 1500 total steps. Model effects not
listed remain unknown. Earlier whole-entry path gaps are retained, not treated
as full acceptance.

Local reports/configs: GAME_DIR/analysis/reporter-audit/guard-order730/.
Assertion drivers/logs: artifacts/engine73/verify-guard-contract.mjs,
verify-callback-results.mjs, verify-target-relations.mjs and their logs.
After checking existing issues, extra consumer details were added to
[existing tracker 113](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/113#issuecomment-5983866493).
Its administrative closure is unchanged; no duplicate issue was created.
No original execution, emulation, native output claim or source implementation occurred.

### Open follow-up for Gap 32

At the owner's request, the current open toolkit issues were checked for the
remaining bracket/callee coverage and target-field/segment producer work.
Tracker 113 is closed; issues 143 and 190 concern different contracts.
[Issue 198](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/198)
now tracks the actual connected evidence, both whole guard controls and
nonvacuous rejected/stopped cases. It does not request relaxed conservative
verdicts. Gap 32 remains open; neither existing occurrence holds nor creating
this tracker proves full consumer acceptance.

## Gap 33 recursive result-origin controls, 2026-10-04

The installed reader 2.1.0/engine 7.3.0 runs the complete FND-CONFIG-172 helper
region with a guarded source identity and explicit bounded return/frame
hypotheses. Original graph fields remain unknown. No spec/parity claim changes
and no original execution or emulation follow from these tooling controls.

Returned no-recursion leaf paths supply zero. A supplied recursive FFFF
retains its compare and branch dependencies as a conditional modeled result.
The helper then writes a fresh FFFF immediate; its own return records that
separate local value producer. Both result flows keep successEstablished false.
A new immediate producer is not proof of an originating semantic/native error.
Supplying recursive zero instead yields only zero on returned paths, including
continuations that discard the modeled pointer-wrapper result.

These are bounded conditional paths, not complete finite-tree acceptance.
Both queries retain loop stops and path-limit gaps, completeWithinModel false
and nativeReachability unconfirmed. A one-step cap removes recursive-call and
normal-return witnesses. No limits were raised: 100 steps/path, 16 paths,
1500 total steps and visit limit 2. The query uses sourceKind=mz, XXH3 guard,
returnBytes=4, unknown SP/BP and labelled saved-frame scopes. The helper's
return contract records an encoding, not its native reachability or origin.
The selector/recursive/guard/pointer-return models do not supply graph memory
or establish valid records and eventually terminating traversal.

**Gap 33 remains open.** Real recursive producer/selector evidence, finite
valid-state conditions and complete caller/leaf controls remain required;
FND-CONFIG-174/175 normalized/discarded-result acceptance is not yet tested in
this batch. The handbook's direct base-producer origin recipe needs a caveat
for a tested recursive error re-encoded as a new immediate. After checking
current issues for duplicates, [issue 200](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/200)
requests the supported recipe/guidance and tracks the remaining source exit.
The engine's local value provenance is not reported as wrong.

Local reports/configs: GAME_DIR/analysis/reporter-audit/result-origin730/.
Ignored assertion drivers/logs: artifacts/engine73/explore-result-origin.mjs,
verify-result-origin.mjs, result-origin-explore.log and result-origin-controls.log.
Instruction-boundary metadata stays local; no original bytes, code or dumps
are committed or attached upstream.

### Normalized result consumer controls, 2026-10-04

FND-CONFIG-174's complete normalizer region now has source controls on the
installed reader 2.1.0/engine 7.3.0. Source identity, full instruction span and
call selectors are verified. Under explicit balanced return/frame hypotheses,
both zero and FFFF supplied callee results yield zero on every retained normal
return. Reached optional external calls make the comparison nonvacuous.
Earlier gated calls are not all covered: both queries retain six path-limit
gaps, even though every retained path returns. completeWithinModel remains
false and nativeReachability unconfirmed. No full path coverage is accepted.

The one-step negative removes all return witnesses. Bounds remain 256 steps
per path, 16 paths and 1500 total steps; initial memory and SP/BP are unknown,
DS/SS are explicit hypotheses, and only stated frame scopes survive calls.
This is discarded-result tooling evidence, not successful external operation,
valid caller state, native producer admission or transitive preservation.
Gap 33 remains open for graph producers and complete caller/leaf controls.
FND-CONFIG-175 preservation acceptance remains separate and unfinished.

Local configs/reports are result-origin730/normalizer-0 and normalizer-65535
under GAME_DIR/analysis/reporter-audit/; ignored driver/log are
artifacts/engine73/verify-normalizer.mjs and normalizer.log. Additional results
were added to [existing issue 200](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/200#issuecomment-5984025397);
no duplicate issue or closure. No original execution, spec/parity change,
window stitching or larger traversal budget occurred.

### Copy-bypass and callee-cleanup control, 2026-10-04

The installed FND-CONFIG-175 copy-wrapper source control distinguishes normal
zero-return bypass paths from the route requesting the copy primitive. Explicit
balanced return models preserve stated DS/SI and frame bytes conditionally;
zero at the local continuation does not identify whether copying occurred.
The copy-request route stops at the wrapper return with a stack-balance
mismatch. The recorded primitive removes eight argument bytes, which the
balanced call model does not express. This is not a native failure or a
reporter instruction bug. No unsupported cleanup field or preserved stack
adjustment is invented to turn the case green.

A one-step cap removes all return witnesses. Without service models, bypasses
still return zero and unread service routes stop. These controls do not accept
performed-copy, native preservation, complete bracket effects or Gap 33 closure.
The complete source span/call selectors and hash guard are verified. Configs
and reports remain local in result-origin730/copy-wrapper under
GAME_DIR/analysis/reporter-audit/; ignored driver/log are
artifacts/engine73/verify-copy-wrapper.mjs and copy-wrapper.log.
The next acceptance should trace the actual primitive with verified argument
producers, rather than use the inadequate balanced model or stitch windows.
No original execution, spec/parity change or raised traversal bounds occurred.

### Actual primitive replaces the balanced-copy model, 2026-10-04

The FND-CONFIG-175 wrapper now declares and traces the actual bounded copy
primitive. Its two reached string-operation routes reach the encoded return
with eight-byte argument cleanup. The prior balanced-model stack mismatch is
removed without an invented cleanup parameter or stack write.

Unknown source/destination pointer storage still prevents full preservation:
copy routes stop with unknown/overwritten return-target provenance, and saved
DS/SI values become unknown. Four bypass routes return zero and retain the
stated DS/SI input values. No path gaps are present, but stopped copy routes
keep completeWithinModel false and nativeReachability unconfirmed. A one-step
cap removes string operations and return witnesses. Ordinary memory copying
is not native successful-copy, capacity, alias safety or rendered output.

The next acceptance requires actual pointer/argument producers and distinct
admitted storage; the frame/segment register hypotheses alone cannot prove
those conditions. Gap 33 remains open. Local configs/reports are
result-origin730/copy-actual under GAME_DIR/analysis/reporter-audit/;
ignored assertions are artifacts/engine73/verify-actual-copy.mjs and
actual-copy-controls.log. No original execution, spec/parity changes, enlarged
bounds or stitched state were used.

## Gap 34 append-capacity source controls (2026-10-04)

Published reader 2.1.0 / engine 7.3.0 trace the complete append body from FND-CONFIG-176 with the supported MZ identity check, unknown pointer/count inputs and explicit balanced guard/copy models. The frame scope is a conditional preservation hypothesis; it does not preserve destination contents or establish native input admission. No original run, game spec or parity status changed.

The retained rejection paths return FFFF before the copy request and before the destination count write. Accepted paths retain the copy request, a subsequent count reload and an increment whose provenance comes from that reload. The report does not reuse the admitted pre-call count as the increment's input: the post-call count remains unknown. Thus the local gate alone cannot establish a post-call count invariant or destination safety under unknown copy effects/aliases. All retained paths return with no gaps and completeWithinModel is true, while nativeReachability remains unconfirmed.

Nonvacuous controls remove the copy model and retain rejection while stopping accepted continuations; a one-step cap removes returns and count-update witnesses. The local source identity, selectors, configs and full reports remain in GAME_DIR/analysis/reporter-audit/result-origin730/append-*; ignored drivers and assertions are artifacts/engine73/explore-append.mjs and verify-append.mjs, with append-controls.log. No original-derived instruction exports or reports are committed.

This verifies the local pre-write capacity/result ordering only. FND-CONFIG-177's pairwise expansion and FND-CONFIG-178's syntactic split bound still need actual connected producer/callee acceptance, finite admitted inputs and alias/guard qualifications. FND-CONFIG-182's copy-versus-terminator base and wrapped-length controls also remain required. Gap 34 stays open: complete conditional append paths do not establish feasible geometry, native capacity, whole-transform safety or rollback.

## Gap 34 connected pair/fill coverage and remaining producer request (2026-10-04)

The published reader 2.1.0 / engine 7.3.0 source query now includes the complete local pair wrapper, initializer, fill wrapper/primitive, intersection, append, eight-byte/full-region copy and normalizer spans described by FND-CONFIG-175/176/177. Only stack-limit guards are modeled, with explicit register and symbolic SS/BP frame-preservation hypotheses. Source identity is MZ/XXH3 checked; inputs/counts remain unknown, SP/BP are not supplied, and bounds are maxSteps=256, maxPaths=16, totalSteps=1500 and visitLimit=4. No original execution, invented entry memory, selected branch outcomes or report stitching occurred.

The connected report reaches actual initialization and string fills, including 69 word iterations for the private 138-byte region, and the pair-selection call. It retains path-limit gaps and loop/step stops. No retained path reaches the append call; its returned zero path contains neither pair selection nor append. That return is not an overflow witness or whole-transform acceptance. The included append region does not itself prove connected capacity coverage when traversal never reaches it.

Nonvacuous controls omit the actual fill region, exhaust the string budget, and cap the query at one step. Each removes the connected pair-selection witness and returned continuation. Full configs/reports remain local under GAME_DIR/analysis/reporter-audit/result-origin730/pair-*; ignored hash-guarded selectors and source assertions are artifacts/engine73/pair-sites.py, explore-pair.mjs and verify-pair.mjs, with pair-controls.log. The accepted symbolic frame scopes remain guard hypotheses, not native input/alias preservation evidence.

An all-state duplicate check found no cardinality/pairwise/split/Gap 34 tracker. Toolkit [issue 213](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/213) now records the full remaining consumer contract and asks for a supported bounded recipe or an accurately documented expressiveness boundary after producer evidence. It does not report conservative limits as a defect. Gap 34 remains open for actual count/record producers and duplicate/overlap invariants, feasible split paths versus FND-CONFIG-178's syntactic bound, alias/callee effects, and FND-CONFIG-182's copy/terminator-base controls. Do not repeat this unknown-input query with larger caps; retry needs new producers, a justified connected route or supported input tooling.

## Gap 34 path-copy and terminator-base controls (2026-10-04)

Published reader 2.1.0 / engine 7.3.0 now trace FND-CONFIG-182's actual caller instructions from the verified boundary immediately after the limit load. entryFrame from the helper root establishes SP=-8/BP=-2 relative to unknown root SP. SI=80 and destination/source length returns are explicit conditional inputs, with register and symbolic frame preservation on modeled calls. The MZ/XXH3 source guard remains required. Bounds are maxSteps=100, maxPaths=16 and totalSteps=1000; no entry memory, branch choices, original execution or stitched reports are supplied.

| Conditional prefix/source length | Counted copy request | Explicit zero location |
| --- | --- | --- |
| 0 / 80 | 79 bytes at original destination | Original destination +78 |
| 3 / 77 | 76 bytes at original destination +3 | Original destination +75 |
| 40 / 40 | 39 bytes at original destination +40 | Original destination +38 |
| 80 / 0 | Source work skipped | No explicit zero |
| 8000 / 8000 hexadecimal | Signed first gate admits negative length; word sum wraps to zero and selects concatenation | No truncation zero |

Source assertions compare the actual pushed destination-offset expression with the explicit zero write's interval base, rather than assuming the zero follows the advanced copy destination. Count zero displacement is normalized to the base expression by the engine. The conditional paths return with completeWithinModel=true and no gaps, while nativeReachability remains unconfirmed. Alias invalidations and unknown final SI/DI/BP remain visible: entryFrame does not carry saved prefix bytes, and a returned report is not a native register-preservation or pointer-validity proof.

Replacing the truncated-copy model in the 40/40 case with the real bounded primitive reaches 19 word copies and one byte copy. It then stops on unknown return-target provenance before the caller's explicit zero write. The gap-free stopped report is incomplete; the modeled copy continuation does not establish that real terminator execution. No native failure, corruption or successful content is claimed.

A one-step cap removes observed frame establishment; removing the copy model removes the explicit-zero witness; removing SI preservation removes the fixed-limit relation. All source controls pass. Original-derived selectors/configs/reports stay local under GAME_DIR/analysis/reporter-audit/result-origin730/path-*; ignored drivers/assertions are artifacts/engine73/path-append-sites.py, explore-path-append.mjs, explore-path-copy.mjs and verify-path-append.mjs, with path-append-controls.log.

Relevant new details were added to existing [toolkit issue 213](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/213#issuecomment-5984294234). Gap 34 remains open for real count/string/pointer producers, admitted storage and aliases, actual scans/concatenation and connected return coverage, plus the pairwise/split contract. Conditional lengths and capacity-like branch limits do not prove native input admission, safe storage, rollback or full generated-cardinality acceptance. Retry this actual-copy case only with new pointer/alias evidence or supported input tooling.

## Gap 36 normalized-wrapper word provenance controls (2026-10-05)

Published reader 2.1.0 / engine 7.3.0 now trace FND-CONFIG-188's complete replacement wrapper together with both complete local services read in FND-CONFIG-189. The MZ/XXH3 guard and evidence-labelled region mappings are retained. Only child/guard calls outside those bodies are modeled, including callback requests with unknown target identity. Models preserve stated registers and an explicit symbolic SS/BP frame interval; AX zero or FFFF are separate conditional result cases. DS data memory is not implicitly preserved or seeded. Bounds remain maxSteps=256, maxPaths=16 and totalSteps=1500.

The retained normalized returning path includes both real service word reads. Each has width two and its complete addressed interval, with low and high byte producer slots separately marked missing. The earlier service's high byte has no write on this path; the later read records modeled-call invalidation rather than inventing a stable neighbor. Conditional active child requests remain in the same timeline before the actual wrapper clears AX to zero. Thus normalization does not erase guard-width dependencies or establish active service admission, accepted pointer state or callback success.

Zero and FFFF child-result hypotheses both retain normalized zero returns. The full query retains path-limit gaps and completeWithinModel=false, with nativeReachability unconfirmed. The returned path is not complete caller/service coverage. Unscoped frame models remove the returning witness; an unread first service removes the joined later guard; a one-step cap removes returns and later guard witnesses. These source assertions pass. The earlier byte-clear/word-read case remains a separate producer control, not a stitched predecessor of this wrapper query.

A rejected attempted fixed-offset memory-scope shape was corrected before acceptance: the supported API requires an address-width general-register base. No artificial register-zero hypothesis was introduced to preserve global flags. Only the stated frame scopes appear in the accepted configs. The source controls instead retain unknown/invalidation provenance for both addressed bytes.

Full selectors/configs/reports remain local under GAME_DIR/analysis/reporter-audit/result-origin730/width-*; ignored drivers/assertions are artifacts/engine73/width-wrapper-sites.py, explore-width-wrapper.mjs and verify-width-wrapper.mjs, with width-wrapper-controls.log. An all-state upstream duplicate review found no specific Gap 36 tracker, but these results expose no new shared-tool defect or design request: the widths and conservative missing-producer qualifications work. No issue was created solely for outstanding consumer research.

Gap 36 stays open for connected real low/high-byte and segment producers, complete normalized caller/service coverage and the full fixture contract. No game spec/parity/queue status, native execution or emulation changed. Retry producer-sensitive paths only with new evidence, a justified connected route or supported input tooling; do not seed neighboring bytes, omit model effects or increase caps to manufacture admission.

## Gap 29 complete eviction-helper source controls (2026-10-05)

Published reader 2.1.0 / engine 7.3.0 now trace the complete call-free FND-SCRIPT-021 helper with MZ/XXH3 source identity, unknown age/cache inputs, unknown root SP/BP and the actual near-return contract. Its fixed 16-slot scan justifies visitLimit=17 for this helper (iterations plus the final condition), with maxSteps=512, maxPaths=16 and totalSteps=5000. This changes neither defaults nor the old restarted room-search query; it is not a larger-budget repeat of that unresolved whole search.

Retained paths cover selected indices zero through 15 and return the selected index. Each retains its same selected-frame-byte producer across the tail rereads, then writes the selected start, end and resource-number words to FFFF, followed by the selected age byte to FF. Assertions check the ordered two/two/two/one-byte widths and each selected-slot address interval. The helper does not read the three previous marker words on these paths. Their new values cannot establish that those stores were no-ops; previous markers and native admission remain unproved.

All retained helper paths return, while path-limit gaps keep completeWithinModel=false and nativeReachability unconfirmed. Covering each selected index does not cover every signed-age ordering, tie or reachable caller state. A four-visit control removes the completed invalidation witness, a one-step cap removes it, and a wrong far-return contract rejects the actual near return. All source assertions pass. No calls are modeled and no cache/age memory is seeded.

A post-selection register-only tail was rejected as a recipe before query acceptance: the actual tail rereads the selected byte from its frame before each marker write. The connected helper creates that local producer itself; unrelated tail-register values or fabricated saved memory are not substitutes. This is tooling acceptance against the existing finding, with no game spec, parity, queue, original execution or emulation change.

Configs/reports remain local under GAME_DIR/analysis/reporter-audit/result-origin730/eviction-*; original instruction context remains local only. Ignored hash/selectors and source assertions are artifacts/engine73/eviction-selectors.py, explore-eviction.mjs and verify-eviction.mjs, with eviction-controls.log. Relevant new consumer details were added to existing [toolkit issue 114](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/114#issuecomment-5984446761), whose delivered feature remains closed. No newly demonstrated shared-tool defect or duplicate feature issue is claimed.

Gap 29 remains open for the complete connected repeated no-op eviction/restarted-search case and real cache/age/capacity producers. FND-SCRIPT-022 explicitly distinguishes the hypothetical high-bit-capacity example from FND-CONFIG-156's ordinary capacity producer. This helper's completed writes and selected-index outcomes do not establish ordinary termination, repeated whole state or native high-bit capacity. Keep old unresolved search bounds unchanged; retry needs new producers, a justified connected route or supported input tooling.

## Published runtime 6.2.0 / engine 8.1.1 acceptance, 2026-10-05

Reader 2.1.0/checker 0.2.0 remain current; prepared protocol remains 3. Engine
wheel SHA-256 is pinned in tools/evidence/requirements.txt. Registry hashes,
wheel/sdist/release-tag/installed bytes and the complete released suite pass,
including the actual installed engine Node bridge. Canonical Test.ps1, locked
restore, Release build and assetless publish/smoke pass. All packaging profiles
are regenerated for runtime 6.2.0. Logs are in artifacts/engine811/.

FND-CONFIG-172 returns queries implement the issue 200 recipe at unchanged
limits, unknown graph memory and explicit conditional models. Old broad frame
scopes now correctly stop when they overlap the processor-written return frame.
Revised hypotheses preserve existing caller stack upward from pre-call SP and
exclude that frame. The recursive compare-input control holds at every reached
occurrence; matching-branch and local-output producer hold at reached checkpoints.
Wrong recursive output origin is rejected; the one-step cap removes checkpoints.
Whole verdicts remain undecided with stopped/dropped routes and unread modeled
callees on bypasses. No native leaf error or finite traversal is established.

FND-CONFIG-164 connected caller/wrapper queries consume the released interrupt
model. Hardware vector and conditional return are retained; CX then DX write
through the same pointer, and returned AX follows supplied BX. Zero and one
select opposite caller branches. Default/unscoped/capped controls stop as expected.
The zero case retains an unmatched caller root frame; the one case retains the
repeat/step stop. The actual caller has no post-store scratch read for the
proposed lastWriter anchor; this remains a supported-query/consumer question,
not permission to add a synthetic read. Issue 190 stays open.

FND-CONFIG-176 append controls with corrected conditional caller-stack scopes
retain accepted zero and rejected FFFF returns, the pre-copy gate and fresh
unknown count reload. FND-CONFIG-199 registration argument controls retain the
consistent consumed widths under explicit scopes. The new three-state width
contract and conservative occurrence lower bounds are retained. No whole Gap 34
cardinality positive, capacity safety, rollback or feasible geometry is inferred.

Posted verified responses on toolkit issues 143, 190, 198, 200 and 213. None meets
its full closure exit. Project follow-up:
https://github.com/kibertoad/dark-sun-wake-redux/issues/5.
Issue 111 is shared with other restorations; its current awaiting controls do
not name this project, so no unrelated consumer request was closed. Original-derived
configs/reports are local at GAME_DIR/analysis/reporter-audit/engine811; no original
execution, emulation, game spec or parity change occurred.

## Issue 5 poll provenance and caller-frame acceptance, 2026-10-05

Reader 2.1.0 / engine 8.1.1 now apply the interrupt BX origin control to
FND-CONFIG-164's actual caller predicate, with BX left unknown in the model.
Both reached predicate occurrences retain that input and hold locally. Whole
verdicts remain undecided because the repeat route stops at the existing step
bound and the exit route lacks an established caller root frame. Supplied BX
zero/one select the previous branch outcomes but are constants rather than
origin inputs; their origin occurrences correctly remain undecided.

The unread interrupt, omitted memory scope and one-step controls have no
predicate occurrence. A wrong CX input has no held occurrence and reports
that CX is absent from the value's inputs. It remains undecided, rather than
violated, behind the modeled interrupt; do not claim a rejected whole control.

Removing the supplied SP/BP and asking `entryFrame` to trace from the real
FND-CONFIG-161 entry yields no arrival at the late poll. Adding the complete
FND-CONFIG-162 first local callee replaces the initial unresolved local call
with two stopped routes: the optional pointer helper and the fallback setter
lie outside the declared regions. Thus a late-entry frame cannot yet be
established by supplying the root name alone. These are specific missing
callee inputs for the next connected acceptance step, not an engine defect or
an upstream-input dependency. No larger bounds or preserved prefix memory
were introduced.

Configs and full reports remain in GAME_DIR/analysis/reporter-audit/issue5-poll;
the ignored driver and assertion log are artifacts/engine811/issue5-poll.mjs
and issue5-poll.log. Bounds remain the prior 64 steps per path, four paths,
256 total steps and visit limit four. Source identity is checked by the reader.
The driver checks the local BX witness and disappearance controls and keeps
the failed frame explicit. These are tooling controls against read-only
FND-CONFIG-161/162/164, with no game spec, parity, native run or emulation change.
Gap 31 and issue 5 remain open for connected callee/frame coverage, a supported
real store anchor and complete positive/negative controls. Gaps 32/33/34/37
remain in the issue's scope.

## Latest engine 9.1.0 checkpoint acceptance, 2026-10-05

The new lastWriter address probe resolves the missing scratch-read anchor
above without adding a read to the original path. Actual FND-CONFIG-164
DX-writer and BX-input occurrences hold; wrong CX writer is rejected and
unread/unscoped/cap controls lose witnesses. Whole verdicts remain undecided
because the caller frame and repeat route are incomplete. Runtime 6.6.0,
engine 9.1.0, exact-source checks and canonical validation are adopted.
See LATEST-RELEASE-GAP-AUDIT.md for every remaining request and retained exit.
No complete gap, game spec or parity status changed.

## Gap 32 actual bracket traversal, 2026-10-05

Engine 9.1.0 source traversal now includes FND-CONFIG-171's real before/after
brackets and gated middle helper, FND-CONFIG-099/175's buffer and coordinate
helpers, actual forward copy, and shared-CS pointer setter/getter. The former
models for those calls are removed. Unknown SP/BP, the existing frame-prefix
query and unresolved-route bounds remain unchanged. Findings stay read-only.

The retained path reaches the second fresh callback target after the actual
before bracket and reaches the actual after bracket and getter. The actual
DS:A057/A059 input read is retained as a producer of both shared-CS word
stores. Both reached store-origin controls hold locally; whole verdicts are
undecided. The later getter does not retain that setter origin across the
remaining callback model, whose unscoped effects remain unknown. Its reached
origin control is undecided. No implicit shared-CS preservation was added.

Both whole target-order controls remain undecided. The first callback anchor
has no witness in the retained bounded routes; second-anchor occurrences are
undecided, rather than inherited holds from the older bracket hypotheses.
Path gaps, unknown target/segment/alias producers and root return-frame stops
remain. The old root guard model's frame scope is rejected as overlapping the
processor-written return frame. Replacing it with FND-CONFIG-163's actual
body exposes the unmodeled DOS interrupt on guarded prefix and helper routes;
it does not establish the whole root frame. The older 7.3.0 acceptance is
historical conditional-model evidence, not current actual-route coverage.

The wrong null-gate control rejects a reached callback witness. Omitting the
before bracket stops at its call sites, and one-step controls remove frame,
callback and relational witnesses. All experiment assertions pass. Original
configs/reports are local under GAME_DIR/analysis/reporter-audit/issue5-brackets91;
ignored driver/log: artifacts/engine91/actual-brackets.mjs and actual-brackets.log.

Gap32 remains open for the actual callback-field producer, admitted storage,
remaining middle-helper callees, guard outcomes, both callback routes and
whole controls. These are consumer research steps; no new upstream capability
limitation has been demonstrated. No original runtime/emulation, entry-memory
fabrication, stitched state, larger unresolved bounds, spec or parity change.

## Gap 33 actual selector and recursive leaf producers, 2026-10-05

Engine 9.1.0 now traverses FND-CONFIG-172's actual selector and recursive call
instead of supplying a selector pointer and child FFFF result. Staged controls
then replace the root guard with FND-CONFIG-163's actual body and the pointer
wrapper/runtime models with FND-CONFIG-165/167's documented source regions.
The final query has no call models. Graph/count/record-length memory and SP/BP
remain unknown; the original step/path/visit limits are unchanged.

The actual recursive child reaches its local zero return on a retained route,
and that zero producer reaches the parent's comparison. Actual child-zero,
local-zero-return and selector-offset producer controls hold at every reached
occurrence. Their whole verdicts remain undecided. The final query's returned
root paths are zero; this describes retained conditional paths, not an
unconditional finite traversal or absence of a native error. Dropped runtime
routes, repeat/step stops and the unmodeled DOS guard remain explicit.

Claiming the local FFFF producer as the source of the actual child zero is
rejected. Omitting the selector removes the child-comparison producer witness,
and one-step controls remove every origin witness. All driver assertions pass.
The intermediate selector-only query still supplies a recursive FFFF and
retains conditional error returns; it is not used as actual leaf evidence.
The source-derived reports/configs are local under
GAME_DIR/analysis/reporter-audit/issue5-recursion91; ignored driver/log:
artifacts/engine91/actual-recursion.mjs and actual-recursion.log.

Gap33 remains open for admitted finite graph/count/length producers, full
recursive and caller coverage, and normalized/copy dependencies. Actual leaf
provenance replaces a modeled result at reached comparisons; it does not
close the whole native error-origin contract. No source findings or parity
statuses changed, and no original runtime/emulation, fabricated entry memory,
stitched state or larger unresolved-query bounds were used. No extra upstream
input was needed for this source traversal.

## Gap 34 actual caller count provenance, 2026-10-05

Engine 9.1.0 now traces FND-CONFIG-173's actual list caller before the pair
operation, with FND-CONFIG-176's initializer/fill and one-record setup,
FND-CONFIG-177/178's pair/split/sentinel/copy regions and the documented
coordinate, selector and runtime dependencies. Historical guard models are
removed; FND-CONFIG-163's actual guard exposes DOS stops. No call models or
entry memory are supplied. The old pair-query bounds remain unchanged.

The actual fixed-region initialization's zero count reaches a concrete pair
input read. Its origin names the actual fill instruction, and its last-writer
control names that same instruction. Both hold at every reached occurrence;
whole verdicts remain undecided. Claiming the fill preparation instruction as
the last writer is rejected. Omitting the fill or limiting the query to one
step removes both witnesses. All driver assertions pass. Read anchors use
full trace events; the initial effects-only inspection omitted reads and was
not evidence that the count lacked a producer.

This is a real zero-count producer case, not the requested known nonzero
append-count positive. The actual caller reaches pair, normalizer and later
copy/sentinel dependencies, while path/step gaps and DOS stops retain incomplete
coverage. The separate actual one-record root stops at unknown/overwritten
return provenance after filling its unknown supplied destination, or at the
DOS guard; it does not reach its count-one store. That is a storage/alias and
caller-input dependency, not proof the native setup fails. No initialized
count or memory from one query is imported into another.

Reports/configs remain local under
GAME_DIR/analysis/reporter-audit/issue5-count91; ignored driver/log:
artifacts/engine91/actual-count-caller.mjs and actual-count-caller.log.
Gap34 remains open for real nonzero count/record/pointer and alias admission,
known append cardinality, complete pair/split/copy/terminator routes and whole
controls. The adopted guidance explicitly has no entry-memory input; this
batch used actual producer tracing instead. No new upstream tool defect or
input requirement was demonstrated. Findings/parity remain unchanged, and no
original runtime/emulation, fabricated memory, stitched state or larger
unresolved-query bound was used.

## Gap 37 actual initializer caller continuation, 2026-10-05

Engine 9.1.0 now traverses FND-CONFIG-193's actual two-zero push/call/cleanup
window into the initializer and out to FND-CONFIG-149's following mode test.
This justified narrow entry assumes earlier startup helpers returned; it does
not import their state or prove their archive/I/O outcomes. The independently
source-bounded initializer loop limits remain unchanged. No calls are modeled.

The actual nested initializer return retains all previous root/pool/flag-byte
last-writer controls, and the caller restores its pre-window SP before the
mode test. The same field controls also hold at the actual caller checkpoint.
The latter query declares FS=1BF3 solely to address the resident fields:
this window and initializer do not use FS for their work, and the probe
supplies no data. It is an explicit inspection-address hypothesis, not native
segment admission. The resident initializer CS and caller overlay CS are
distinct; claiming the overlay CS addresses those fields is rejected.

All reached occurrences hold; whole controls stay undecided because each
following mode route stops at its unread helper. Wrong-root-writer and
wrong-segment controls reject; missing-initializer and one-step controls
remove the witnesses. All driver assertions pass. An initially incorrect
expectation that the wrong-segment case would be undecided was corrected to
the actual rejection after checking the propagated selectors and intervals.

Source-derived configs/reports remain under
GAME_DIR/analysis/reporter-audit/issue5-startup91; ignored driver/log:
artifacts/engine91/actual-startup-initializer.mjs and actual-startup-initializer.log.
Gap37 remains open for the full startup-to-transfer route, later writers,
external/state producers, admission and known-answer hardware placement.
No initializer memory is stitched into a disconnected transfer, and no
original runtime/emulation, spec/parity change, invented memory or increased
unresolved-transfer bound was used. Following helper/caller work remains
consumer research; no additional upstream input was needed for this case.

## Gap 37 actual mode helpers and setup clear, 2026-10-05

Engine 9.1.0 now includes FND-CONFIG-155's actual zero-mode reset and its
explicit overlapping internal IRET entry, FND-CONFIG-159's actual nonzero-mode
body, FND-CONFIG-144's setup and actual fill callees, and FND-CONFIG-149's
following byte-gate body. No call models or memory inputs are supplied, and
the previous caller query limits are unchanged. The real internal IRET is a
local stack continuation, not an external interrupt observation.

The retained zero-mode path restores its saved flags through that IRET and
reaches setup. Resident root/pool/flag-byte writers hold at setup entry and
again after the actual table fill, before the next setup initializer call.
A probe of slot523's three cleared bytes names the two-byte repeated store
for its first two bytes and the trailing byte store for its last byte. Its
GS=4F49 input is an explicit unused inspection-address hypothesis, like the
resident FS probe; it supplies no contents or native storage admission.
All reached writer occurrences hold; whole verdicts remain undecided.

The setup-return checkpoint still has no witnesses. An initial expectation
that it would be reached was disproved and corrected: unknown direction-flag
forks, path gaps and unread helpers remain. FND-CONFIG-154 explicitly leaves
that incoming flag unresolved; no clear flag or gate value was supplied to
manufacture the return. After including the real fill, the retained setup
path reaches the next initializer call rather than stopping at the fill.
The separate nonzero helper query stops at its SOUND.CFG acquisition or
returns on its local gate bypass; it supplies no state to the caller query.

Wrong root writers reject at reached setup-entry anchors. Omitting the zero
helper removes the internal IRET witness; omitting the fill removes the
after-clear field/table witnesses; one-step controls remove the anchors.
All experiment assertions pass. Local source reports/configs:
GAME_DIR/analysis/reporter-audit/issue5-modes91; ignored driver/log:
artifacts/engine91/actual-mode-helpers.mjs and actual-mode-helpers.log.
Gap37 remains open for incoming flag/data producers, full setup/caller and
startup-to-transfer coverage, later writers/external inputs and hardware
placement. No native run/emulation, spec/parity change, stitched state,
seeded memory or larger unresolved-query bound was used. No additional
upstream input was required for this source extension.

## Gap 37 actual setup list and counter producers, 2026-10-05

Engine 9.1.0 now includes FND-CONFIG-147's real range initializers, list
inserter and overlay record-reset scan in the connected startup/setup query.
The startup path reaches the inserter and scan, then stops at its unchanged
step bound; it has no initializer-return checkpoint witness. The earlier
incoming direction-flag/path gaps and nonzero-mode external call remain.

A separate query starts at the actual setup entry and creates its own clear
and list state. It reaches the first range initializer's return. Both retained
direction cases preserve slot523's original clear writers there. The first
inserted slot's byte and paired word instead name the actual inserter writes,
and the DS head word names its actual new-head store. The inserted index
retains the real setup counter-seed producer. All reached occurrences hold;
whole verdicts remain undecided because later scans hit the original bounds
and paths are omitted. No starting table/head contents or gate/flag values
are supplied, and no state from this query is imported into startup.

Claiming the earlier fill as the inserted slot's writer is rejected. Removing
the range initializer removes every insertion/return witness, and one-step
controls remove them too. All driver assertions pass. These local producer
controls do not prove the whole finite construction, native admission, setup
completion or the later graphics-transfer route.

Source configs/reports are local under
GAME_DIR/analysis/reporter-audit/issue5-setup91; ignored driver/log:
artifacts/engine91/actual-setup-lists.mjs and actual-setup-lists.log.
The FS/GS inputs remain inspection-address hypotheses. Findings/parity are
unchanged, no call model was introduced, and no original runtime/emulation,
fabricated memory, stitched state or larger unresolved-query bound was used.
Gap37 remains open for full startup/setup and transfer coverage, incoming
flag/data and external producers, later writers and hardware placement.

## Gap 31 actual state-clear and status gates, 2026-10-05

Engine 9.1.0 now includes FND-CONFIG-166's actual state-clear, status and
forwarder bodies in the root dependency set. The root formation remains
unestablished behind the previous poll/guard/step/path stops. A justified
narrow caller window starts at FND-CONFIG-162's actual status-clear call,
assuming the preceding driver poll returned; it supplies none of that poll's
memory or caller-frame state. No calls are modeled in this window.

The window reaches the real status entry but drops its zero-return routes at
the original path bound. The status helper's separate actual entry retains
two zero-returning gate paths. Their local zero producer and unchanged
DS gate-word byte controls hold at every reached occurrence. The caller
window also retains entry-state provenance for that gate word at its reached
status read. This proves modeled byte preservation there, not a zero input,
valid pointed storage, native immediate return or complete active processing.
All whole controls remain undecided behind unread device calls and stopped
or dropped routes. The independent status query supplies no state to the
caller window or the root-frame query.

A wrong caller-call-site zero origin is rejected. Omitting state clear or
status removes its downstream witnesses, and one-step caps remove them too.
All driver assertions pass. Initial assumptions that the caller window would
retain a zero-return witness were corrected after inspecting its path gaps;
no bounds or starting memory were changed to obtain one.

Source-derived selectors/configs/reports remain under
GAME_DIR/analysis/reporter-audit/issue5-status91 and issue5-root91;
ignored driver/log: artifacts/engine91/actual-status-window.mjs and
actual-status-window.log. The bounded selector records only the actual call
boundary, not original instructions. Gap31 remains open for true root/frame
coverage, interrupt/result sequences, active device/callee and input producers,
whole controls and their negative cases. No original runtime/emulation,
spec/parity change, stitched state, fabricated memory or larger unresolved
query bound was used. This source work required no extra upstream input.
