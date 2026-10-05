# Latest release gap audit, 2026-10-06

The [five-gap contract audit](FIVE-GAP-CONTRACT-AUDIT.md) distinguishes delivered
toolkit work, pending connected consumer acceptance and original-game unknowns.
The active connected-evidence goal remains open. An open consumer exit is not
by itself an undelivered library feature or a demonstrated migration blocker.

The PR 293 follow-up adopts scientific-method-engine 10.1.0, released at tag
2007f0f0be3c3919cf36b467ae519b75da996f55, matching the merge commit.
Wheel SHA-256 is d458056ba12d677b50f36fa8fa4215570fdf0ceafe679f9df0bb25916541b35c;
sdist SHA-256 is 8921997a75323d55a3aefa53079b7aca250918ce3dfc941f5dee0fcd890d6a92.
Downloaded hashes match published release digests. Wheel production files,
sdist production files and the installed package match. The published test
suite passes; logs and integrity driver are under artifacts/engine101.
The exact wheel pin in tools/evidence/requirements.txt is updated. This focused
adoption leaves runtime, reader, checker and the local rules snapshot unchanged.

Issue 290's constant-mask bound reproducer now holds on a complete returning
synthetic query. The actual Gap 37 mask-bound occurrence also holds, while its
whole verdict stays undecided at the unresolved later copy count. Unmasked,
omitted table-load and one-step controls retain negative outcomes. This removes
the demonstrated mask-bound reporting limitation, without closing Gap 37 or
any other full connected exit. See TRANSFER-CALLER-PRODUCER-AUDIT.md.

The preceding release baseline selected scientific-method-engine 10.0.0 and
RefurbishedDinosaurs 7.0.0; reader 2.1.0 remains current and checker 1.1.0
is adopted at tag c79d7fe3addb3fb14bc5f118f19f47571aa4c3f0.
Engine tag scientific-method-engine@10.0.0 resolves to
eaeaad180f36cd6ccab1198410e40ca9421cd4e5. Wheel SHA-256 is
f31e59b5eb0c3bae9a3b9b60063b5dcbe7c144cb181e6a33c327312af19f2bba;
sdist SHA-256 is 391b52d3905e1ec0e55cc4d324866b7571dc13cfbdec6278857d5f7eeba9ca20.
Wheel, sdist, tagged production files, installed files and released tests agree.
The full published suite passes with its test-only Unicorn oracle and the
installed-engine Node bridge. Logs are artifacts/engine100/release-tests.log.

Compared with engine 8.1.1, PR 236 changes Ghidra instruction-start diagnostics
and flow-export coverage; PR 244 adds checkpoint memory last-writer probes.
Compared with runtime 6.2.0, PRs 238â€“241 add opt-in PCX short-stream filling,
static random transitions and two InstallShield content-source formats.
None supplies the missing original caller, data or edition evidence.
The local rules snapshot is unchanged; no standards refresh was requested.

| Remaining gap | Release effect and completion evidence still needed |
| --- | --- |
| 5 | No portable inventory change. Distinct disc executable/inventory acceptance is still required. |
| 9 | PCX filling does not change the UI window catalog. True window image versus copied control fields remains unaddressed. |
| 27 | No effect-order traversal change. Complete connected fill/bracket coverage remains required. |
| 29 | No loop/input change. Admitted cache, age and capacity producers and connected restarted-search controls remain required. |
| 31 | New memory probe removes the missing-read anchor limitation. Actual DX last-writer and BX-origin occurrences hold; wrong-writer, unread, unscoped and cap controls retain their expected outcomes. Whole verdicts remain undecided because caller-frame formation and repeating routes are incomplete. |
| 32 | PR 281 delivers toolkit #274: a produced ptr16:16 target now enters exact declared code. Installed positive and negative controls pass. Actual callback reloads still have unknown pointer words and storage/caller gaps; whole controls remain undecided. |
| 33 | No recursive provenance change. Replayed compare/input and re-encoding controls retain local witnesses and whole undecided verdicts; real finite child/selector/leaf and caller coverage remains required. |
| 34 | Actual caller prologues now establish pre-guard frames. Connected fill/setup/copy reaches the count-one store and retains it at normalization; local origin/last-writer/order controls hold. DOS, path and step stops still prevent whole normalization and known append-cardinality acceptance. Pair/split/copy/terminator exits remain required. |
| 36 | No width/provenance change. Connected low/high-byte and segment producers, normalized service coverage and fixtures remain required. |
| 37 | Ghidra diagnostics do not supply the startup-to-transfer route. Actual later writers/external producers and known-answer placement controls remain required. |
| 40 | Checkpoint probes do not establish missing cleanup predecessors. Complete assignment and caller-frame coverage remains required. |
| 41 | Checkpoint probes can inspect output bytes but do not establish formatter output or native capacity. Complete count/terminator/caller controls remain required. |
| 42 | No allocator or clearing change. Request arithmetic, extent, header and complete clearing/callee controls remain required. |
| 43 | No arithmetic admission change. Complete restricted caller-range and counterexample acceptance remains required. |
| 44 | No capture-helper or fixture change. Repeated controlled capture diagnostics and cause evidence remain required. |

No remaining full gap meets its closure exit solely through these releases.
This audit covers the release changes against each request; it is not a new
complete reading of the original. Preserve every request in gaps.md.

The actual FND-CONFIG-164 checkpoint at the caller predicate inspects the
two-byte SS:BP scratch slot without adding an original read. Both reached
occurrences name the later DX store, and both predicates retain unknown
interrupt BX provenance. Claiming the earlier CX store as the last writer is
rejected. Unread interrupt, missing frame scope and one-step cases remove
probe witnesses. Bounds and unknown input memory remain unchanged. Whole
controls are undecided, with the exit route lacking its root frame and the
repeating route stopped. Source configs/reports are local at
GAME_DIR/analysis/reporter-audit/issue5-poll91; driver/assertions are
artifacts/engine91/issue5-poll.mjs and poll-controls.log.

The follow-up FND-CONFIG-161 root-frame query includes the documented
FND-CONFIG-162/163/165/167 callee bodies. It now reaches an unmodeled DOS
interrupt and bounded poll/setter routes; whole frame coverage remains
undecided. Existing unresolved-query bounds were preserved. The actual
caller's callback literal reaches both the overlay and resident registration
stores. Both local origin controls hold; a poll-derived origin is rejected,
and omitted-setter and one-step controls remove the witnesses. Reports are
local under GAME_DIR/analysis/reporter-audit/issue5-root91 and
issue5-producers91; drivers are artifacts/engine91/root-dependencies.mjs and
callback-producers.mjs.

For Gap37, the actual FND-CONFIG-193 call-free initializer returns completely
within its conditional model. Known-answer root and pool words agree, and
last-writer controls hold for those words and for the first and last free
flag words: the low byte is written and the high byte retains entry-state
provenance. Wrong-root-writer, one-step and four-visit controls retain their
expected rejection or undecided outcomes. Its separate finite visit bound
comes from the documented fixed loop, not an increased bound on unresolved
transfer queries. The initial region accidentally excluded the documented
return instruction; including that endpoint corrected the query. Reports
are local under GAME_DIR/analysis/reporter-audit/issue5-initializer91, with
driver artifacts/engine91/initializer-producers.mjs. This does not connect
startup state to a later transfer or establish native admission.

Producing the remaining caller, producer and connected-route evidence is
consumer research work here. It requires no extra upstream input unless a
specific missing tool capability is demonstrated. No original run, original
emulation, invented memory, stitched state or spec/parity change was used.

## Latest library and requester disposition revision

NuGet/npm/PyPI registry checks select runtime 6.8.0, checker 0.6.0, reader
2.1.0 and engine 9.1.0. Runtime 6.7/6.8 improve InstallShield archive handling;
checker 0.3 through 0.6 add typed field checks, explicit skipped compilation,
fixture-hash diagnostics, retirement of layout-free unknown format listings,
and duplicate build-manifest detection. These do not change engine traversal.
The exact checker tag rebuild matches all installed dist files and its registry
integrity matches the lock. Its released synthetic suite passes, with platform
skips explicitly retained. Ordinary and all packaging profiles adopt runtime
6.8.0. Official NuGet signatures and canonical content hashes verify; all
package contents match the installed cache. Signature-bearing archive bytes
are not substituted for the canonical content hash. Verification logs are
artifacts/engine91/runtime680-integrity.json and runtime680-content-match.log.
Canonical validation includes Release build and assetless smoke.

The five old Dark Sun toolkit trackers carry delivered shared-tool requests
and consumer prerequisites. Their delivered requests are accounted for; source
coverage stays in project issue 5. Toolkit #274 is a new, duplicate-checked
capability request: a wholly synthetic instruction-produced far pointer is
known at its indirect call, but the declared target is not followed. Immediate
far-call positive returns and unknown-pointer negative stops. No proprietary
bytes or invented original state were used. No whole game gap is closed merely
by separating delivered requests from consumer evidence.

Logs: artifacts/engine91/latest-canonical-validation.log,
checker060-integrity.log, checker060-release-tests.log and indirect-far-repro.log.

## Released migration and confirmation

Engine 10.0.0 includes PR 281 for toolkit #274. The original synthetic
instruction-produced pointer now enters its declared target and returns, with
both pointer writers retained. Immediate far-call control returns; unknown and
undeclared targets stop with their distinct reasons; a one-step control never
reaches the target. Actual bracket/writer controls run on the installed release
at unchanged bounds. Their local origins remain held, while whole verdicts stay
undecided. Actual callback words are unknown; declared code cannot substitute
for a pointer producer. Source reports: GAME_DIR/analysis/reporter-audit/
engine100-brackets and engine100-writer-inputs.

Runtime 7.0.0 rejects control and Windows-reserved characters in portable paths.
Checker 1.0/1.1 add F# citations and explicit skipped base comparisons; the CI
action requires its comparison base. Exact CI/tag pins are updated; the local
rule snapshot is unchanged. Tagged checker build matches installed distribution
bytes and registry integrity. Official NuGet signatures, canonical content
hashes and all cached package contents match. Ordinary and every packaging
profile are refreshed. Full canonical validation passes, including Test.ps1,
locked restore, Release build and assetless smoke. Tagged engine/checker suites
pass with their explicit platform skips.

Verification: artifacts/engine100/engine-integrity.log, release-tests.log,
package-integrity.log, checker-tests.log, indirect-far-controls.log,
actual-brackets.log, actual-writer-inputs.log and canonical-validation.log.
Toolkit #274 is closed after the released confirmation; project issue 5 is
revised to the current versions and consumer exits.
The library update and its compatibility validation are complete. The five
connected-evidence exits in project issue 5 remain consumer work; no new
undelivered toolkit requirement was demonstrated by these reruns.
