# Validation

CI and routine repository checks require no proprietary content. Synthetic GFF,
indexed-image, palette, indexed-font, text, DSIX, DSFT, DSTX, DSUI, DSRG, and
DSOB fixtures exercise successful decoding plus
truncation, bounds, invalid-component, unsafe-path, inventory, and transactional
replacement failures. Core rule tests use explicit inputs and no ambient state.
Start-flow tests also prove identical seeds and commands yield identical events
and state hashes, snapshots restore exactly, rejected commands are sequenced,
and replay stops at the first divergent hash.
Party-rule coverage includes the manual-documented all-three Psionicist rule,
the exactly-one discipline rule for other characters, Cleric-only elemental
spheres, canonical hash sensitivity to the selected sphere, the exact
six-ability modifier values for all eight origins, and immutable
dual-class progression across the human-only, level-three, repeated-career,
three-career, monotonic-advancement, and former-benefit boundaries. It also
proves ordered class/progression consistency, atomic DUAL selection and
cancellation, exact command payloads, progression-sensitive hashes, and
snapshot restore both during and after selection.
Start-flow coverage also proves occupied-slot edits replace rather than append,
invalid replacements are atomic, DROP moves a member to character storage, ADD
restores it, and an active edit target survives snapshot restore.
Presentation-independent input tests prove viewport letterboxing cannot change
the logical start-button choice, exercise DSUI-derived exclusive rectangle edges,
and reject incomplete, unexpected, or image-mismatched start-window graphs.
UI-resource tests also cover the executable-evidenced nonzero-intersection rule
for serialized/runtime event masks without assigning speculative names to bits.
Font tests compose variable-width indexed glyph runs and multiline blocks,
preserve palette-index bytes, zero-fill explicit glyph/line spacing, retain
blank lines, accept zero-width glyphs, and reject malformed fonts, negative
spacing, and oversized output.
Region-scene tests prove row-major tile placement, clipped cross-tile viewports,
the corroborated OJFF/ETAB offset transform and mirror bit, first-frame selection,
transparent-pixel preservation, ordered object overdraw, and rejection of
invalid viewport, tile, frame, and cross-catalog references.
Terrain-grid tests prove the executable-evidenced `GMAP` `0x40` mask, preserve
the independent `0x80` flag, close out-of-bounds cells, validate pixel/cell
edges and centers, and reject malformed planes. Planner tests prove stable
camera-to-cell Walk routes, obstacle detours, unreachable destinations, and
inactive mode/view/outside-canvas rejection without assigning route cadence.
Actor-controller tests compose those boundaries through logical Walk clicks,
prove fixed-step accumulation is independent of frame chunking, cap catch-up
work without discarding backlog, reset partial cadence on replanning, require
a placed actor, interrupt on a new blocker in shared occupancy, and drive the
camera-relative sprite from the same anchor. Presentation tests cover exact
fixed-point forward, reverse, and diagonal interpolation plus malformed progress
and interval rejection; controller tests prove continuity across a semantic
step boundary.
Viewport-layout tests prove that wide and tall displays expose additional
bounded map area, preserve the base camera center, clamp at world edges, map
physical input back into the expanded slice, and retain fixed 320x200
letterboxing when expansion is disabled. Dialogue-layout tests prove the exact
two window rectangles, portrait anchor, four scrollbar controls, five response
strip placements, F9 and F12 rising-edge input contracts, bounded greedy wrapping,
five-label preview selection, projected response output with retained blank
lines, malformed/empty-output rejection, overflow rejection, and unsupported-character
handling. Dialogue-session tests prove stable page identities, atomic one-shot
row selection, rejection without mutation, original branch retention, matching
branch enforcement, atomic local-flag application, deterministic menu
reselection after a returning branch, and caller-page retention for a target
shared by both menus. Unknown local-number increments are
rejected before mutation. Conditional local-flag tests prove both outcomes and
unknown-input rejection without mutation. Synthetic projection tests reject drift in choice
0's prints, newlines, assignment, offsets, and local return; choices 2/3's
prints, matching flag clears, local-number-0 increments, offsets, and returns;
and choice 7's print, assignments, offsets, and local return.
Choice 1 projection tests cover all three print boundaries, its local flag clear,
the extended global-357 condition, conditional local flag 6/7 assignments,
global assignment, and return. Core tests exercise both known condition outcomes
and reject an unknown global flag before any mutation.
Owned content smoke additionally proves the actual projected first page fits
the measured text widths with the extracted font, selects its fifth row as
original choice 7 with the projected branch target intact, and reaches the
completed state with local flags 14 and 4 set. A separate owned-pack path selects
choice 0, clears local flag 0, returns to `AwaitingChoice`, advances through the
opening continuation, and verifies second-menu source order 0, 5, and 6. It
then reidentifies the shared target-2905 completion for source 6 and completes.
A counter-progression path selects source choices 2 and 3, verifies both flag
clears and increments, and proves that local number 0 reaches 2 while source
choice 4 becomes visible in the resulting 0, 1, 4, 6, and 7 order. Selecting it
sets local flag 9, resets local number 0, and enters second-menu order 0, 5, 6.
Another owned-pack path selects choice 1 and verifies local flag 1 clears, local
flags 6/7 become true from the fresh-opening condition, global flag 357 becomes
true, and the second menu resolves as 1, 3, 5, and 6.
That path then selects target 1825, verifies local flag 6 clears, local flag 10
is enabled from fresh-opening local flag 16, the page becomes 2, 3, 5, 6, and
the extracted transcript fits the speech area.
The same owned path selects newly enabled target 3479, verifies local flag 16
sets and local flag 10 clears, resolves page order 3, 5, 6, and rasterizes its
three-part transcript. Synthetic tests reject either assignment or return drift.
The path then selects target 1996 under captured global number 22 equal to one,
verifies local flag 11 sets and local flag 7 clears, resolves page order 4, 5,
6, and rasterizes its two-part transcript. Synthetic tests reject condition,
control-flow, or return drift.
Separate owned paths set global number 22 away from one and validate both
global-number-84 bit-2 transcript variants, the shared prints, `GNUM84 |= 1`,
local-flag-7 clear, deterministic page reselection, and bounded rasterization.
Core tests prove inactive alternatives do not require unknown target state.
It next selects target 2352, verifies local flag 11 clears, resolves page order
5, 6, and rasterizes the single-part transcript; synthetic tests reject its
assignment or return drift.
It then selects target 2415, verifies local flags 12/13 set while local flag 10
remains clear because the owned path already set flag 16, advances to third-menu
source order 0, 1, 6, resolves its GSTRING #6 exit label, and rasterizes the
three-part transcript. Synthetic tests cover the opposite conditional outcome,
the exact menu identities, transition legality, and control-flow/truncation drift.
The owned path next selects target 2921, verifies local flag 12 clears, resolves
third-page source order 1, 6, and rasterizes its three-part transcript;
synthetic tests reject assignment, return, and truncation drift.
It then selects target 3089, verifies flag 15 sets and flag 13 clears, resolves
source order 2, 6, and rasterizes three output parts. Target 3257 follows,
clears flag 15, leaves only source choice 6, and rasterizes four output parts.
Synthetic tests reject assignment, return, and truncation drift for both.
The owned script also validates alternate targets 3686/3786 and rasterizes
their two- and three-part outputs. Synthetic tests prove the local-flag-17/18
transition shapes and reject assignment, return, and truncation drift.
The primary owned path finally selects target 3976, verifies flag 8 clears
before the six-flag helper derives flag 14, enters `Completed`, and rasterizes
the two-part completion output. Synthetic tests validate both output branches,
subroutine targets, post-assignment ordering, unknown-input atomic rejection,
and helper/control-flow/truncation drift.
The owned smoke also rasterizes all five implemented returned transcripts with
the extracted font, including choice 0's explicit blank line, within the bounded
speech area.
Exploration command tests also center on arbitrary world points with both-edge
clamping, reject partial/out-of-world targets, suspend centering outside world
and Game Menu views, and select expanded display through Collapse Party.
Game Menu routing tests supply the moving actor's visual center only to the
context-dependent Center action, route Exit through a distinct request, and
keep the remaining Load/Save action inert.
Combat-input tests map each documented Guard, Wait, target-cycle, end-turn, and
disable-control hotkey to a validated Core command on its rising edge, without
assigning the unresolved combat-resolution behavior.
Destination-page tests resolve both the character and inventory WIND graphs
through one semantic page object, prove exact shared-button placement and image
identity, exclusive hit rectangles, cross-page navigation/return commands, and
malformed-page rejection.
Movement-session tests prove deterministic command/event traces, exactly one
semantic step per advance, atomic replanning failure, cancellation/completion,
snapshot isolation, and interruption when a step or diagonal side becomes
blocked after planning.
Occupancy-session tests prove canonical immutable multi-cell footprints,
deterministic occupant ordering, atomic place/move/remove and explicit rejected
transitions, terrain/bounds/overlap exclusion, own-cell movement overlap, live
whole-footprint passability, and composition with route planning.
Actor-movement tests prove route/occupancy lockstep, whole-footprint detours,
atomic step commits, rejection and dynamic-blocker interruption without partial
movement, repeatable traces, immutable snapshots, and external drift detection.

The smoke modes have distinct purposes:

- `--smoke-test` exits before content or graphics initialization and is safe on
  a content-free CI worker.
- `--content-smoke-test --asset-pack <path>` verifies the exact pack inventory,
  opens all seventy-eight DSIX images, the DSFT interface font, the DSTX text
  catalog, the resolved start-flow, Game Menu/Preferences, and
  character/inventory/Cast/Effects and hostile-interaction DSUI graphs, and DSCH character metadata
  catalog, opens the DSRG Tyr region, every source-derived structural-region DSRG
  catalog, and DSOB object-frame graph, and checks their frame, geometry, glyph,
  reference, and inventory contracts without a window.
- `--platform-smoke-test` creates the MonoGame platform surface and exits; it is
  reserved for installed-package environments with a display server.
- Normal startup verifies the pack before opening a window and renders the
  evidenced start shell/controls, party-overview shell, ADD-list shell, Tyr,
  Game Menu, Preferences, character, inventory, Cast/Use, and Current Effects
  shells; title sequencing, ADD-list content, dynamic destination content, and
  remaining destinations remain pending.

Owned-build validation currently targets GOG product `1432903719`, installed
build `52095422060333615`. Metadata-only FONT inspection verifies that all 256
character-map entries are identity values and summarizes its pixel-index range
without emitting glyph pixels. The retained ignored owned-source pack has been
transactionally refreshed and verifies as the exact required-revision-34 16,524-asset manifest: 16,401 source-mapped DSOP
assets preserving every 233-file source input and all 16,168 GFF descriptors, plus 123 specialized
derivatives including start/party/ADD assets, all seven start-flow windows and 56 controls, the 210x116 Game Menu/Preferences base,
the 14-button/30-control Game Menu and 13-button/15-control Preferences graphs,
the 320x200 inventory base, the observed USE/EFFECTS title images, the
86-control character/Cast/Effects and 89-control inventory graphs, and 19 bounded character metadata entries,
plus 20 source-derived, structurally validated region DSRG catalogs and the bounded Tyr region with 94 tiles
and 867 entity records, its 287 definitions, 246 images, and 477 frames, and
the exact 13-frame opening-leader image and all ten `ICON` #19101-#19110 cursor
images with their exact one-frame geometry, plus the three-window interaction/dialogue graph,
thirteen action/dialogue control images, `PORT` #18 portrait, GPL #135/MAS #99
provenance-checked DSGP v2 scripts, and the four-label/ten-description/nine-line Preferences DSTX
catalog. The additional evidence-only derivative is the 98x32 combat status-panel
DSIX from RESOURCE.GFF BMP #19003 with PAL #1000 at its observed (215,4)
placement; it is validated as an extracted asset and is not rendered.
The runtime content-smoke path verifies
all 13 frame dimensions plus frame 0's 367-pixel alpha coverage and rasterizes 320x200 viewports at
both opposite region corners successfully. It also verifies Tyr's exact four
`GMAP` values and 8,169 terrain-open cells through the bounded navigation
contract; these diagnostic checks are not a claim about party spawn.
The no-window smoke also plans and advances one real Tyr step from the evidenced
opening anchor through the runtime actor controller. Its provisional
single-cell footprint and 125 ms step are implementation policies, not native
timing or footprint evidence.
The owner-requested right-button grab-drag path is covered independently of
MonoGame: stationary press/release retains mode cycling, held logical motion
emits incremental inverse camera deltas at the documented 13:10 pan multiplier
with deterministic fractional carry,
canvas re-entry cannot jump, and a
completed drag suppresses cycling. Core separately bounds and clamps pan
commands. Either Alt key completing an Enter chord toggles once, and a held or
partial chord does not repeat. These are `DEV-INPUT-001`, not
original-parity claims.
Exact comparison of the retained ignored native opening frame against the
compositor at `(1024,1368)` isolates the visible leader to a 367-differing-pixel
component bounded by logical `(160,91)`-`(176,125)`. Exhaustive same-geometry
resource comparison identifies OJFF #305/BMP #599 frame 0 as an exact match;
bounded executable analysis ties its world top-left `(1184,1459)` to collision
anchor cell `(74,91)`. The multi-cell footprint remains unknown.
Six controlled local cursor captures at the unchanged opening camera establish the
Walk/Can't Walk, melee/invalid melee, and Look/invalid Look pairs and the
manual-defined upper-left hotspot. The valid melee hotspot resolves through the
static compositor transform to OJFF #9258 -> BMP #346. Three additional owned
captures establish hostile Look, award-notification, and two-window dialogue
layouts; GPL disassembly independently anchors the captured exchange in chunk
#135. Synthetic tests exercise the bounded first-conversation projection,
including exact observed offsets, literal and variable text sources, compound
condition parsing, menu bounds, and fail-closed identity/opcode/truncation
handling. Core tests distinguish true, false, and unknown constant/local flag/
local-number conditions; adapter tests preserve their parsed identity. Owned
content smoke confirms two speech variants, eight initial menu entries, and the
ordered condition shapes without checking proprietary text into Git. Local-only decoded previews established the title
and start-window mappings recorded as `FND-IMAGE-007`,
`SCR-UI-001` to `SCR-UI-003` and `SCR-UI-008`; screenshots and decoded outputs stay
under ignored `analysis/original/` and never become golden files. Presentation
goldens in Git must use synthetic stand-ins. Visual comparison, input traces,
animation timing, and audiovisual synchronization remain open and will be
recorded per parity row rather than inferred from passing parsers.

## Spec checks

The `Documentation standard` job in `.github/workflows/ci.yml` runs the
`check-documentation` action from
[refurbished-dinosaurs-toolkit](https://github.com/kibertoad/refurbished-dinosaurs-toolkit),
pinned to a full commit SHA, on every pull request. It checks `spec/`, `parity/`
and `deviations/` against the standard's list of
[checks](../vendor/upstream/documentation-standard.md#checks) (lines 787-838), compiles each
`.ksy` file with the Kaitai Struct compiler, checks that every spec and
deviation ID cited in `src/`, `tests/`, `tools/` and `docs/` exists and is
not superseded, fails when `spec/index/` or `PARITY.md` is stale, and fails a `validated`
row whose marked tests are not in `VALIDATION.md` as they are now (see
[Tests against the original](#tests-against-the-original)). It fetches
the full history so it can fail a pull request that deletes a spec ID, area or
deviation that exists on `main`. The toolkit's
[setup guide](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/blob/main/docs/documentation-standard-check.md)
lists its inputs.

The check also fails when a code comment gives an address that no entry the
comment cites records, in its locations or text or in the evidence of an entry
it cites. A neutral name (`fn_…`, `g_…`) is always an address. A plain `0x…`
value is one only inside an image the job gives with the action's `images`
input, so colours, masks and offsets are left alone. DSUN.EXE is an MZ
executable, whose addresses are written `segment:offset`, so `ci.yml` gives no
`images` input and only explains how to add one. A range larger than `max-range`
(64 KiB by default), such as a whole section, records only its two ends, nothing
inside it. When a
comment fails, cite the finding that records the address, or write one.

`./tools/Test.ps1` runs the hash-verified offline checker in `--check` mode
through `tools/Invoke-NodeChecks.mjs`, with the CI step's `references: docs`, from the exact toolkit commit the workflow pins.
It also runs the synthetic evidence and snapshot suites. Node.js 22+ and
PowerShell 7+ (`pwsh`) are required for these suites. See
[UPSTREAM-RULES](UPSTREAM-RULES.md) for explicit upstream checks and refreshes.
With `kaitai-struct-compiler` on `PATH` it also compiles the `.ksy` files;
without it the documentation checker warns and skips them.

The check writes `spec/index/` and `PARITY.md`; nobody edits them by hand. After
changing the spec, `parity/` or `deviations/`, run the script without `--check`
and commit what it writes:

```sh
node tools/upstream.mjs docs --references docs
git add spec/index PARITY.md
```

`tools/upstream.mjs docs` passes the checker the inputs the CI step gives
under `with:` (`code`, `references`, `images`, `max-range` and `data-dirs`), so
a local run checks what CI checks; an option given on its command line wins.
`--check` reports problems without writing anything. `tools/Test-TemplateInfrastructure.ps1`
fails if the workflow stops running the check or pins it to anything but a full
commit SHA. The script does not check some items on the standard's list, such as
the fixture schema and the hashes of saves and recordings; its guide lists them,
and reviewers check those by hand. A save-patch write is given as a byte offset
and value in the experiment's Setup section.

`.githooks/pre-commit` runs the gate's node checks, listed once in
`tools/Invoke-NodeChecks.mjs`, before each commit once a clone enables it with
`git config core.hooksPath .githooks`. It copies the index to a temporary
directory and checks that, so it judges what is being committed: unstaged edits
neither hide a problem nor block a clean commit. The documentation check runs
there with `--no-ksy`, so `.ksy` compile errors still surface only in the gate
and CI. Without `node` the hook prints a warning and lets the commit through.


## Tests against the original

A parity row's Tests column lists only tests that compare the rebuild with
evidence from the original. Those that need the original's files find them
under the directory named by the `GAME_DIR` environment variable, which holds
one directory per build, named by its build ID, with the files laid out as the
build manifest's paths give them (`GAME_DIR/BLD-GOG-EN-1.1/DSUN.EXE`, and
`GAME_DIR/BLD-GOG-EN-1.1/CD/...` for a file read from the disc).
`GAME_DIR/captures/` holds the captures and saves that cannot be committed, each
named by its XXH3-128 hash. A test checks each file's hash against the build
manifest before it reads it, and skips when the file is absent.

A test file that reads the original through `GAME_DIR` carries the comment
`// needs: GAME_DIR`, and only such a file does; the documentation check fails a
listed test file that mentions `GAME_DIR` without it. No test does so yet.

CI never has a copy of the original, so the marked tests skip there. They run on
the maintainer's machine with `GAME_DIR` set to the owned copy. After a run of
`./tools/Test.ps1` in which every test in every marked test file of a
`validated` row passed and none was skipped, record the run and commit the
`VALIDATION.md` it writes at the repository root:

```sh
node tools/upstream.mjs docs --references docs --record-validation BLD-GOG-EN-1.1
git add VALIDATION.md
```

`VALIDATION.md` holds the commit, the date, the builds, and the hash of each
marked test file of a `validated` row. The check fails a `validated` row whose
marked test file is missing from it or has changed since it was recorded, so a
change to such a test needs a new local run before it merges.


## Bounded reporter case verification

2026-09-30: static checks against BLD-GOG-EN-1.1, located through the stable
GAME_DIR baseline, compared the ten selected reporter requests and related
conversion/segment/cleanup cases with their existing findings. See
[REPORTER-CASE-AUDIT.md](REPORTER-CASE-AUDIT.md). Reports/configurations remain
under GAME_DIR. No original program or emulated function was run; no evidence
status changed. Gap 18 passed the actual adopted reporter. Candidate fixes for
gaps 14 and 19 are proposed upstream and are not adopted locally yet.


### Revised upstream adoption, 2026-09-30

Adopted latest website, toolkit and template main revisions recorded in TEMPLATE-ADOPTION.md. tools/Test.ps1 passed: 92 Python tests (including PE32), 41 Node tests and 700 .NET tests; repository/configuration checks, exact source pins, local section ranges and documentation check (615 entries, 158 parity rows, 5 deviations) passed. Recorded Dark Sun static cases passed known-read controls, incorrect-control rejection, dispatch normalization and all three effective-width conversion controls against the actual adopted reporter. Conditional continuations remain conditional. No original game or emulated function ran; raw reports/configurations remain in GAME_DIR.


Goal acceptance batch: verified FND-CONFIG-096 two-entry table and third-entry rejection, plus FND-CONFIG-144 pushed-pointer relocation controls against the pinned reporter. Source baseline unchanged; local queries/reports remain GAME_DIR-only. Gap 12 closed; gap 16 partial.

Bounded-map/diagnostic tooling batch: root gate passed 92 Python, 43 Node and 700 .NET tests. New scratch-Git diagnostic test and synthetic Java map acceptance both passed, without skips. Actual read-only Ghidra 12.1.3 map controls passed (page/exact name and negative controls); logs remain GAME_DIR. Template canonical gate passed 92 Python, 41 Node and 56 .NET tests; PR 32 tracks upstream delivery.

Inventory acceptance batch: read-only Ghidra shared export matches all 2,723 retained mapped-view rows; shared view join reproduces the committed installed inventory exactly (1,284 resident + 869 overlay = 2,153). New identity/schema/destination checks pass actual installed inventory and corruption controls. Root gate passes 92 Python, 44 Node, 700 .NET; template gate passes 92 Python, 40 Node, 56 .NET and clean build. Disc-source validation remains pending; no substitute-source claim.

Repository policy checks all Git-visible files, including untracked files not ignored. deniedFileNamePatterns rejects JVM fatal-error/replay logs and heap dumps at any depth even if force-staged. The canonical Test.ps1 gate runs the synthetic diagnostic and bounded-map suites; current test totals belong in its generated log.

## Migrated capture and research tracking

The fast gate checks area queues, question IDs and reverse entry links with Check-ResearchTracking.mjs. Capture-OriginalWindow.ps1 isolates PrintWindow in a bounded worker, selects only client pixels and avoids desktop fallback. Its tests create synthetic windows. Original-game captures remain owner-only under AGENTS.md; adding this helper grants agents no original runtime access. Mixed-DPI native captures require owner validation.

Release safeguards: `node --test tests/upstream/release-signing.test.mjs` uses synthetic GitHub and Authenticode doubles. Live SSL.com signing, timestamp servers and repository environment policies require a separately authorized release. Configure `ES_CERTIFICATE_THUMBPRINT` in the protected `release-signing` environment and restrict its deployment branches to main.

Signing workflow acceptance uses `tests/upstream/release-signing.test.mjs` synthetic tag API and certificate tests. Signed release setup requires `ES_CERTIFICATE_THUMBPRINT` and main-only deployment branches in the protected `release-signing` environment. Live signing is not exercised by synthetic acceptance.

## Explicit rerun without restore

After a normal successful validation has restored this checkout, run
`./tools/Invoke-Validation.ps1 -NoRestore` to rerun using those existing dependencies
when NuGet is unavailable. `./tools/Test.ps1 -NoRestore` also supports a direct
rerun after its normal run has populated the same test artifact paths. Restoring
only the solution does not populate a different test artifacts path.

The switch skips restore, not policy, configuration, Python/Node checks, build,
tests or assetless publish/smoke. Normal validation and CI still restore. It never
falls back to network restore on failure. Missing assets/packages fail through
the .NET diagnostics. This option does not refresh or independently prove the
freshness of cached restore state: run normal validation again after changing
dependency inputs, lockfiles, SDK or build paths. Existing filters/count controls
retain their normal meanings; a filtered run remains partial acceptance.


Offline option acceptance (2026-10-01): the full configured gate passes with
-NoRestore and unreachable HTTP proxies; log artifacts/offline-root-full-validation.log.
The normal restoring gate is recorded in artifacts/offline-root-normal-validation.log.
Shared template normal and sequential offline gates pass, recorded in
artifacts/offline-template-normal-validation.log and
offline-template-offline-sequential.log. Synthetic option controls retain all
checks, filters/counts and failure propagation. The restricted full rerun still
fails native capture and Java filesystem controls; its failure is retained in
artifacts/offline-validation-restricted-rerun.log and is not counted as acceptance.
Shared delivery is proposed in template PR 41; gap 4 remains open pending review.


Final template PR 41 adoption: merged revision
bd3d9a381cae7d3f015e9c089f2fe134e8a0a857. The option tests use scratch-local
lock files and require a build plus a test route, avoiding vacuous assertions.
The configured gate's route is Test.ps1; its forwarding control replaces the
template's direct dotnet-test assertion. Test.ps1 supplies its running portable
PowerShell for these controls when PWSH is unset and restores the prior value.
Normal and offline full gates are rerun for this final adaptation.

Final normal and offline gates pass: artifacts/offline-pr41-final-normal.log and
artifacts/offline-pr41-final-offline.log. The production portable-host block also
passes unset-PWSH fallback and preservation of a supplied caller value; local
probe artifacts/pr41-portable-host-control.ps1. Gap 4 is closed after this final
merged adoption; the earlier pending-PR notes above describe prior acceptance.

Merged toolkit PRs 35?37 / template PR 42 adoption (2026-10-01): exact
reporter integrity, adopted owner/caller/pointer source controls and canonical
normal validation pass; log artifacts/merged-pr42-validation.log. Offline
NoRestore gate uses unavailable proxy endpoints and retains all required checks;
log artifacts/merged-pr42-offline-validation.log. Source controls and reports
remain in GAME_DIR/analysis/reporter-audit. No original runtime was launched.

Gap 17 upstream candidate PR 38 (2026-10-01): source acceptance plus
Python, Node bridge/documentation, policy, .NET restore/build/tests and packages
pass after integration with c133cd4; logs artifacts/overlap-candidates-final-*.
Candidate is not yet adopted; licensed-source artifacts stay outside Git.

Gap 20 upstream candidate PR 39 (2026-10-01): hash-guarded source controls,
Python and full Node suites, repository policy, .NET restore/build/tests and both
packages pass; logs artifacts/callee-graph-final-*. Candidate is not adopted.

Gap 17 final merged adoption (2026-10-01): toolkit 1ef21ef exact pin,
complete source overlap/width/rejected/cap/partial controls and owner/pointer
regressions pass. Canonical validation passes, including Test.ps1 and Release
build/publish/smoke; log artifacts/gap17-merged-adoption-validation.log.

Gap 21 candidate PR 40 (2026-10-01): bounded hash-guarded source controls,
complete Python/Node suites, policy, .NET restore/build/tests and packages pass;
logs artifacts/near-pointer-final-*. Stopped source paths remain incomplete and
no native DS/SS relationship is claimed. Candidate is not adopted.

Root canonical Invoke-Validation.ps1 -NoRestore passes after the Gap 21
candidate acceptance record, including Test.ps1 and Release publish/smoke;
log artifacts/near-pointer-pr40-root-validation.log.

Final merged PRs 39/40 adoption (2026-10-01): exact toolkit pin
67340fcb975449600c160ef5a4995119d4e8f127 passes graph and near-pointer
whole-contract source controls, nonvacuous formation cap, operand overlap and
owner regressions. Canonical NoRestore gate passes, including Test.ps1 and
Release build/publish/smoke; log artifacts/merged-graph-provenance-validation.log.
Template PR 43 already contains this pin at 5ee4f81; exact integrity verified
and all applicable CI passes. No duplicate update was pushed.

Gap 15 candidate PR 41 (2026-10-01): whole source controls, synthetic
Python/Node bridge/documentation, repository policy, .NET restore/build/tests and
packages pass; logs artifacts/call-order-final-*. Candidate is not adopted.

Template PR 43 final merge adoption (2026-10-01): merge 79d18a20 has the
identical tree to reviewed 5ee4f81; exact toolkit pin remains 67340fc.
Root canonical NoRestore validation passes after the Gap 15 candidate records,
including Test.ps1 and Release build/publish/smoke; log
artifacts/call-order-pr41-root-validation.log.

Gap 26 candidate toolkit PR 43 at f0d05f4 (2026-10-01): hash-guarded source cases and
nonvacuous cap/rejected-encoding controls, synthetic Python/Node suites,
repository policy, .NET restore/build/tests and both packages pass; logs
artifacts/return-flow-final-* and return-flow-packages-*. Conditional child models, unknown effects and
stopped source paths remain explicit. Exact adopted toolkit pin is unchanged.

Current package-layout candidate rerun (2026-10-01): engine/source controls,
workspace lint/format/type checks, bridge/standard-checker/release-planner tests,
npm build/tarballs, Python wheel and .NET tests/NuGet packages all pass. Toolkit
PR 43 is updated by normal push with published history preserved. Root canonical
NoRestore validation passes after acceptance records; log
artifacts/return-flow-pr43-root-validation.log.

Gap 15 current package-layout candidate f8c756f (2026-10-01): all source
controls and package/workspace gates pass; logs artifacts/call-order-packages-*.
Upstream PR 41 is updated by normal push; the restoration pin is unchanged.


Published-package migration (2026-10-02): configured normal and NoRestore canonical
validation, adapter/protocol/CI-input controls and existing adopted Dark Sun
reporter drivers pass. Source configs and reports stay under GAME_DIR. Exact
engine 0.1.0, reader/checker 0.0.0 and Capstone 5.0.7 locks replace copied tooling;
rule snapshot bytes are unchanged. Logs: artifacts/package-delivery/root-normal-validation.log
and root-offline-validation.log. Template normal and unreachable-proxy NoRestore
gates pass in the isolated migration worktree.
