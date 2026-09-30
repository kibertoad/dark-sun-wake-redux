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
[checks](../vendor/upstream/documentation-standard.md#checks) (lines 780-831), compiles each
`.ksy` file with the Kaitai Struct compiler, checks that every spec and
deviation ID cited in `src/`, `tests/`, `tools/` and `docs/` exists and is
not superseded, fails when `spec/index/` or `PARITY.md` is stale, and fails a `validated`
row whose marked tests are not in `VALIDATION.md` as they are now (see
[Tests against the original](#tests-against-the-original)). It fetches
the full history so it can fail a pull request that deletes a spec ID, area or
deviation that exists on `main`. The toolkit's
[setup guide](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/blob/main/docs/documentation-standard-check.md)
lists its inputs.

`./tools/Test.ps1` runs the hash-verified offline checker with
`--check --references docs`, from the exact toolkit commit the workflow pins.
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

The script does not check some items on the standard's list, such as the
fixture schema and the hashes of saves and recordings; its guide lists them,
and reviewers check those by hand. A save-patch write is given as a byte offset
and value in the experiment's Setup section.

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
