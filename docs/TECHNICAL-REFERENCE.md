# Technical reference

This is the concise technical map of the restoration as it stands. It is an
index to verified facts and explicit boundaries, not a replacement for the
detailed evidence records. For a rule or format to become implementation
behavior, its detailed record must be consulted first.

## Evidence and clean-room status

- Supported source: English GOG product `1432903719`, build
  `52095422060333615`, documenting game-data Version 1.1. The exact fingerprint
  and edition facts are in [SOURCE-EDITIONS.md](SOURCE-EDITIONS.md).
- No original executable, data, save, screenshot, or decompiler output belongs
  in this repository. The game consumes a local verified pack; it never starts
  the original executable or DOSBox.
- [RULES-AND-EVIDENCE.md](RULES-AND-EVIDENCE.md) is authoritative for data,
  manual, and observation facts. [GHIDRA.md](GHIDRA.md) is authoritative for
  bounded static-analysis findings. [FIDELITY.md](FIDELITY.md) and
  [PARITY-MATRIX.md](PARITY-MATRIX.md) state what is actually implemented.
- An unobserved or untraced behavior remains unknown. It is not approximated as
  a native-parity rule merely because an implementation would be convenient.

## Data pipeline

```text
licensed GOG installation
  -> source fingerprint verification
  -> bounded format readers and lossless opaque preservation
  -> staged pack validation and atomic promotion
  -> verified local asset pack
  -> MonoGame presentation plus deterministic Core
```

The Extractor verifies all 233 immutable baseline files and all 16,168 GFF
descriptors. The current required revision 35 pack contains 16,401 lossless DSOP corpus assets
plus 123 specialized DSIX, DSGP, DSTX, DSUI, DSCH, DSRG, and DSOB derivatives:
16,524 assets in total. The added combat-status-panel DSIX is a bounded source-backed
image at its observed geometry; it assigns no combat semantics. Twenty source-derived DSRG files independently retain
the verified structural envelope of every owned region without assigning any
travel or presentation behavior. Opaque preservation means the resource is retained and
hash-verified; it does not mean the runtime understands or executes it.

The transaction stages output beside the installed pack, writes an exact
manifest, re-opens and hashes every output, rejects unexpected files, and only
then atomically replaces the old verified pack. CI needs neither the original
game nor an installed asset pack.

## Architecture

| Component | Owns | Must not own |
|---|---|---|
| `Core` | deterministic commands, state, events, snapshots, replay hashes | MonoGame, original-format parsing, I/O, wall-clock decisions |
| `Resources` | bounded source and pack contracts | MonoGame or gameplay state |
| `Extractor` | source verification and transactional pack creation | game-window behavior |
| `Inspect` | read-only owned-content research | extraction or game-state mutation |
| `Game` | MonoGame input, rendering, fixed presentation clock | direct original-content access or rule mutation outside Core |
| `Tests` | original-free synthetic proof of contracts and behavior | licensed content dependencies |

The fuller dependency and transaction rationale is in
[ARCHITECTURE.md](ARCHITECTURE.md).

## Established source contracts

The complete structural descriptions, bounds, and malformed-input behavior are
in [ORIGINAL-FORMATS.md](ORIGINAL-FORMATS.md). The principal current contracts
are:

- GFF directory parsing for all owned containers; unclassified payloads are
  preserved as DSOP.
- Indexed bitmaps, palettes, FONT, TEXT, UI `WIND`/`BUTN`/`APFM`/`EBOX`, and
  selected character envelopes have bounded readers. Native code distinguishes
  `PLAN` and `PLNR` on bounded tile/object image paths; this corroborates the
  reader's supported image-family boundary, not a frame or actor behavior.
- Twenty regions expose bounded identity, palette, terrain, geometry, tile,
  and placed-object records. Tyr has an extracted DSRG/DSOB graph used for the
  opening scene.
- GPL and MAS carry their exact source family in DSGP v2. Only recorded,
  fail-closed projections are interpreted; there is no generic bytecode
  interpreter.
- FLI, VOC, OGG, configuration, and item-table files are fully inventoried and
  preserved. The executable contains raw numbered FLI names and CINE-directory
  path templates, but their inspected locations have no direct references, so
  names do not establish a loader, fallback, sequence, or timing policy. Those
  media semantics remain opaque. The loaded sound-helper image has no complete
  VOC header signature or `.VOC` filename-extension literal. The main
  executable carries five `.VOC` fragments, but every one is unreferenced data
  rather than an instruction (`EXE-GOG-SOUND-009`). These facts establish no
  decoder, codec, filename mapping, or timing contract.
- `SOUND.CFG` has a bounded 59-byte envelope, but neither the main executable
  nor the separately shipped sound helper contains its literal pathname; the
  main executable also has no literal `SOUND.INI` pathname. This is not
  evidence of configuration ownership or Preferences behavior. Neither
  case-variant of the configuration file's `.adv` module suffix occurs anywhere
  in the helper's complete physical file, including its overlay, which likewise
  does not establish module ownership or driver-selection behavior. The
  complete main executable also lacks the literal `SOUND_DS.EXE` helper
  filename (`EXE-GOG-SOUND-010`), and its decoded `INT 21h` instructions have
  no nearby literal DOS EXEC setup (`EXE-GOG-SOUND-011`), so no main-to-helper
  launch path is claimed.
- The sound helper's complete physical file, including that overlay, also has
  no whole `Creative Voice File` signature. This strengthens the loaded-image
  absence but still does not identify or exclude partial validation, a decoder,
  device routing, or CPU-independent playback timing.
- Its loaded code does contain fixed-port and runtime-base-plus-offset output
  sequences, but their direct callers do not identify a VOC consumer, device,
  Preferences control, rate, duration, or playback clock. Those low-level
  operations are evidence against assuming a simple standard decoder, not a
  contract to emulate in the modern runtime.

## Current runtime boundary

The current Slice 3 build can verify and open the local pack, render bounded
startup and interface shells, show the observed Tyr opening viewport, route
movement through deterministic A*, and render the known cursor family.
Dialogue preview uses the original fixed 320x200 canvas and measured dialogue
chrome while the exploration view may expand to the physical display aspect.
The bounded first dialogue projection owns only the validated opening paths;
unknown visible targets stay inert.

The screens' layers, geometry and image mapping are the `SCR-UI` entries in
[spec/screens](../spec/screens/). The plan and current
slice acceptance criteria are in [IMPLEMENTATION-PLAN.md](IMPLEMENTATION-PLAN.md).

The start window is `SCR-UI-001`. No code that handles its buttons is known
(`FND-UI-012`), so deterministic routing is an independently designed boundary,
and START GAME continues with an explicit unresolved shipped-party origin.

## Destination-screen and combat evidence boundary

The View Character, Inventory, Cast and Effects screens are `SCR-UI-002` and
`SCR-UI-008` to `SCR-UI-010`. Five navigation controls route today; all interior
controls stay inert. The label table of those screens (`FND-UI-023`) has no
known reader. Read those entries, FND-PARTY-020 and FND-MAGIC-001 before
extending these screens.

Two item names visible in the owner-confirmed inventory capture are lines of
`RESOURCE.GFF#TEXT/1000` (`FND-ITEM-008`). No evidence ties a line to an item
record, slot or selection path, so the runtime keeps inventory interiors inert.
The manual's inventory, store and item-spell behaviour is `RULE-ITEM-001` to
`RULE-ITEM-005`.

Combat remains evidence acquisition only. The owner reports that entry leaves
the map and character presentation substantially unchanged except for the
absence of the compact status panel; the first stable combat state shows the
currently active character's panel. The owner identifies dsun_009 as an enemy
movement frame; it visibly retains dialogue chrome and has no compact panel,
so it is not evidence for the stable combat layout. dsun_011 is enemy striking,
and dsun_012 is a player turn labelled Thy'rokh, with visible panel strings
`90/85` and `Moves 15`; that identifies neither their value semantics nor a
turn transition. The owner reports no visible turn-transition treatment and
that a direct enemy click causes the active character to approach and strike,
without a separate target-switching or confirmation presentation. The static
panel is the 98x32 BMP #19003 at (215,4), with a
dynamically overlaid region bounded to (243,8) through (284,31). Its visible
dynamic text changes from `Draxan`/`Moves 20` in the enemy-striking frame to
`Thy'rokh`/`90/85`/`Moves 15` in the player-turn frame, supporting only the
owner-confirmed active-combatant presentation. dsun_011 shows a red 11
feedback glyph whose visible glyph-area pure-red components occupy (151,95)
through (178,113) on the 320x200 canvas. The required-revision-34 pack preserves the static artwork as
`images/combat/status-panel.dsix` with source provenance and the interface palette,
but no Game code consumes it. These facts establish
neither movement cost, action ordering, attacker/target identity, panel-value
meaning, damage resolution, turn progression, timing, nor exit. No combat session, encounter,
or rules pipeline is implemented until the controlled C0-C6 observation gate
and a traceable data or executable path establish them. `DATA-GOG-COMBAT-001`
finds no baked `Moves` caption in the static panel and no exact printable ASCII
source match in the 26 owned GFF archives; `EXE-GOG-COMBAT-005` likewise finds
no null-terminated executable literal. Those bounded negative results leave
the visible caption dynamically sourced but otherwise opaque; they do not
identify a renderer, value field, turn transition, or combat rule.
`EXE-GOG-COMBAT-006` additionally classifies the static panel request's direct
eight-call native fan-in as a shared, feature-neutral boundary: only small
`0`/`1` argument pairs are visible, with no recovered encounter, actor,
command, damage, turn, or timing identity. It is not a combat-only route.
`FND-INPUT-004` follows the native mouse-coordinate guard to the entry code
without identifying a combat screen, click action, target, approach, attack,
or confirmation handler.
`EXE-GOG-COMBAT-013` follows its only coordinate-consuming far-thunk boundary,
but the destination is outside the mapped image and its sole other recovered
relation is non-coherent decompiler output; it supplies no combat semantics.
`EXE-GOG-COMBAT-014` binds the coordinate branch's resident word to a broader
combat-adjacent switch through its leader-change diagnostic, but no mapped
caller or action meaning is recovered for value five or any other value.
`EXE-GOG-COMBAT-015` finds the mode setter's only recovered direct call supplies
literal two rather than five, leaving the coordinate branch without a traceable
native producer.
`EXE-GOG-COMBAT-016` then proves a separate direct writer can assign decimal 19
to the same word, so its local one-through-five switch is not a finite combat
mode model.
`EXE-GOG-COMBAT-017` exhausts the recovered direct-write graph without a
literal-five producer; indirect or computed writers remain open.
`FND-INPUT-003` finds the routine that picks the pointer image (`RULE-INPUT-002`);
its overlay caller is opaque, so it is a presentation boundary, not an action,
target, movement, or strike path. `FND-INPUT-004` finds no direct caller of the
native button-press wrapper, and `FND-INPUT-005` shows the mouse handler and the
keyboard hook queueing packets through one buffer; the packets' consumer is
unknown, so this is not evidence for a combat click dispatcher or action rule.
A third producer sends packets of another kind, which supports shared
transport rather than a combat-specific input path.
`EXE-GOG-COMBAT-011` shows that the mapped six-selector dispatcher containing
the panel case has no recovered direct caller. Its neighboring selectors are
therefore not identified as combat commands, transitions, or rule paths.
`EXE-GOG-COMBAT-012` finds that the shared state word used by the two panel
routes has multiple direct comparison values and only one direct write, which
sets a value used by just one route. No value is thereby identified as combat
or a turn phase. `EXE-GOG-COMBAT-018` finds no decoded function that directly
co-locates `BMP ` #19003 with `FONT` #100; this leaves the panel's dynamic
renderer and fields opaque rather than proving either resource unused.
`FND-ACTOR-008` finds that the one `MONR` payload repeats as 27 units of 42
bytes with a constant zero tail; `FMT-ACTOR-003` stays unknown, and the tag
occurs only in overlay 204 (`FND-ACTOR-006`).

`EXE-GOG-AI-001` is the current aggregate enemy-decision audit, not a claim
that the native executable lacks AI. It covers every current static candidate:
the observed hostile OJFF #9258, raw `MONR` and `ETAB` leads, bounded `RDFF`
paths, coordinate/input branches, the four `COMPUTER CONTROL` label matches,
and the RNG/panel route. None connects an actor or hostile record to a native
target-selection, movement, action, or outcome consumer. The mapped coordinate
destination is only a five-guard validation sequence; the computer-control
labels are unreferenced data; and the RNG/panel routine has no recovered caller.
Indirect or runtime-built paths remain possible. Consequently, no enemy-AI
model is present in Core or Game. C0-C6 must first establish combat entry,
active-state changes, targeting, an enemy action, and turn progression before a
new focused static query can assign behavior.

### Combat evidence ledger

| Surface | Established evidence | What is deliberately not inferred | Next evidence needed |
|---|---|---|---|
| Entry and exit presentation | Owner reports exploration stays visually continuous at entry apart from the compact panel; combat exit immediately resumes single-leader exploration. | Encounter trigger, state transition, victory/defeat conditions, or any invisible setup/teardown. | C0, C1, and C6 controlled captures plus a traceable owner path. |
| Active-combatant display | Owner-confirmed dsun_011/dsun_012 frames show the static #19003 panel at (215,4) with different dynamic strings; dsun_012 is Thy'rokh's labelled turn. | The panel values' field ownership, meaning, update cadence, or turn algorithm. | A native text/value producer or a controlled frame sequence correlated to source data. |
| Hostile motion and strike feedback | Owner confirms dsun_009 as enemy movement, while that frame visibly retains dialogue chrome and lacks the compact panel; dsun_011 is enemy striking and visibly includes the red `11` glyph. | Pathfinding, movement cost, actor/target identity, hit, damage, or timing rules, or a stable-combat layout inferred from dsun_009. | C3/C5 captures and a bounded native/data route for the action result. |
| Direct enemy click | Owner reports that clicking an enemy makes the active character approach and strike, with no separate visible target switch or confirmation. | Click hit testing, target legality, approach path, range, attack resolution, or an implicit selection state. | A recovered consumer beyond the coordinate/callback boundaries and C3 capture notes. |
| Cursor presentation | `ICON` #19101–#19108 are selected by one native routine behind a flag test in its overlay caller (`FND-INPUT-003`). | That #19103 executes an attack, that #19104 states why an attack fails, or any combat-specific owner. | Recoverable indirect owner plus branch-input meaning. |
| Native input boundaries | Mouse-coordinate and button probes establish generic wrappers; the recovered callback conditionally emits a 14-byte packet through guarded resident buffering. BIOS-keyboard probes remain generic. | A combat input loop, packet fields/consumer, the manual-key dispatch, repeat policy, or a command-to-action mapping. | Screen-specific consumer path and controlled command traces C2/C4. |
| Resident state candidates | One coordinate branch reads value five; a broader combat-adjacent switch and direct writes of two and 19 are recovered. | A finite mode enum, combat phase, target state, or attack mode. | A mapped producer and action body, including indirect/computed writes. |
| Enemy decision route | `EXE-GOG-AI-001` audits OJFF #9258, `MONR`, `ETAB`, `RDFF`, coordinate/input, computer-control labels, and RNG/panel leads. None reaches an attributable decision/action consumer. | AI absence, target choice, movement, action selection, turn ownership, damage/outcome rules, or automation behavior. | C0-C6 state/action captures followed by a focused static query anchored to that observed path. |
| Rule data | Manual arithmetic and hotkeys remain isolated research helpers; `MONR` is only an opaque 27-by-42 envelope. | Attack, movement, damage, turn, encounter, difficulty, AI, or outcome implementation. | Controlled C0–C6 observations paired with a traceable executable or data finding for each rule. |

## Object and static-scene route

The following is the current bounded path from original static region data to
the rendered opening scene. It documents data and presentation connections; it
is deliberately not an actor-behavior model.

```text
RGN#50: RNME / PAL / MAP / GMAP / TILE / ETAB
  -> DSRG region contract + DSOB referenced object-frame catalog
  -> clipped static terrain and first-frame object compositor
  -> verified opening viewport and independent leader overlay
```

- Every `ETAB` record supplies bounded position, vertical-offset, flags, and
  an absolute `OJFF` reference. The record's flags and all non-evidenced
  geometry meanings remain raw data.
- An `OJFF` is `FMT-ACTOR-001`. `31E0:0EFF` requests it into a 37-byte slot
  record and `31E0:0E1B` computes the image position from it (`FND-ACTOR-003`);
  `RULE-ACTOR-001` draws the placed objects. Actor ownership, collision,
  interaction, and animation are not established.
- A bounded native caller chain reaches distinct `PLAN` and `PLNR` image
  dispatch after tile/object image resolution. It confirms the static image
  route but does not establish how a frame is selected or scheduled.
- The one observed opening leader remains separately evidenced: OJFF #305,
  bitmap #599 frame 0, world top-left `(1184,1459)`, and collision anchor
  `(74,91)`. Its footprint, all later frames, cadence, party formation, and
  other actor behavior are unknown.

See `FMT-REGION-001` to `FMT-REGION-006`, `RULE-REGION-001`, `FND-IMAGE-010`, `FMT-ACTOR-001`,
`RULE-ACTOR-001`, `FND-ACTOR-002`, `FND-ACTOR-003`, and `FND-IMAGE-005` before
expanding this route.

## Preferences evidence boundary

`WIND` #16500 is a 210x116 resource graph with thirteen buttons and two
application frames. Its authentic base and first-frame controls render, and the
Game Menu and Return actions are implemented. The following table separates
the documented control role from the state that has not yet been measured.

| Control family | Established contract | Deliberately not implemented as native behavior |
|---|---|---|
| Music, sound effects, animations, voice effects | The manual defines each as an on/off toggle; voice applies to CD-capable installs | Initial on/off state, state storage, visual frame mapping, and audio routing |
| Music and sound-effects volume | Each is a slider adjusted through buttons at its two ends | Numerical range, increment, initial value, displayed fill, and mixer mapping |
| Difficulty | The executable table orders the four labels Easy, Balanced, Hard, Hideous; the manual assigns it to combat difficulty. SRC-GAMEFAQS-81038 reports a hostile-HP-at-spawn hypothesis (`FAQ-81038-COMBAT-001`) | Selected default, its relationship to the manual's conflicting “Average” wording, mutations, hostile-HP multipliers/rounding/timing, and rule consumers |
| About | The manual specifies version, copyright, support, hint-line, and address information; the executable supplies nine centered lines | Modal geometry, backdrop, input dismissal, and native transition |

Direct-reference and scalar probes find no literal binding from the executable
text table, including all nine centered About strings, or two tested control
pairs to a handler. This does not prove that the controls are inactive: their
dispatch may be resource-driven or calculated. It does prevent treating those
literal values as evidence for an implementation or an About modal route.
Follow-up reference queries also find no recovered direct caller of the generic
window-registration entry or its two generic callback setters. That prevents
using the generic UI framework as an identified Preferences registration path;
it does not prove that native callbacks or settings behavior are absent.
`PREF` #100 is a single nine-byte envelope in `CHARSAVE.GFF`; it provides no
field layout or connection to the in-game screen. The sole raw `PREF` tag also
has no direct executable reference, so neither source establishes settings
behavior.
`SOUND.CFG` and `SOUND.INI` are absent as literal names in the separately
shipped sound helper, and `SOUND.INI` is also absent from the main executable,
so neither is mapped to Preferences. See
`SCR-UI-007`, `DATA-GOG-PREF-001`, `FMT-TEXT-004`, `FND-TEXT-006`,
`EXE-GOG-PREF-001`, and `DATA-GOG-SOUND-002` in the detailed evidence records
before changing this boundary.

## Determinism and time

Core transitions happen only from explicit commands. Rendering interpolates
presentation but cannot advance game rules. Snapshots and replay hashes make
the start flow and current exploration state reproducible.

- Route advancement is semantic and clock-free in Core. The runtime uses a
  bounded fixed-step accumulator and fixed-point visual interpolation. Positive
  host elapsed time accumulates saturating ticks, while camera clamping widens
  delta arithmetic and the modern drag adapter widens then saturates pointer
  deltas. Actor interpolation uses checked widened arithmetic, and
  sprite-visibility rectangle edges also widen before clipping, so
  clock discontinuities or extreme input/coordinates cannot overflow into
  gameplay or presentation state.
- The opening actor's current single-cell footprint and 125 ms semantic step
  are explicit modern policies, not claims about the native implementation.
- `EXE-GOG-TIMING-001` establishes BIOS tick use only for startup/mixing paths,
  not actor or animation cadence. `EXE-GOG-TIMING-002` finds that all four
  literal `INT 15h` sites select extended-memory services (`AH=87h`/`88h`),
  rather than the BIOS wait service (`AH=86h`). `EXE-GOG-TIMING-003` adds a
  CPU-busy VGA-status transition poll around a generic word-copy path, not a
  semantic clock. `EXE-GOG-TIMING-004` additionally bounds direct PIT latch/
  read and programming routines, but finds no recovered feature owner or
  duration contract. `EXE-GOG-SOUND-003` separately finds no `INT 15h` opcode
  in the loaded sound-helper image. None must be recreated as an
  interrupt-disabled render loop, hardware timer, or actor scheduler.
  `EXE-GOG-MEDIA-001` finds no literal FLI
  header-validation lead, and `EXE-GOG-MEDIA-002` finds no direct reference to
  the embedded filename/path entries. `FND-IMAGE-008` likewise finds no
  operand for the static-title `BMP ` #11011. Raw cinematic speed fields, filename order, and the title asset are
  data, not assumed milliseconds or a schedule.
- The random number generator and its reductions are `RULE-RNG-001`. Its seed
  and the rules that draw from it are not known, so new rules do not consume
  it yet.

## Static-analysis boundaries worth preserving

Static analysis has produced reusable structural facts while deliberately
rejecting unsupported semantics:

- GPL/MAS use distinct selected source families, a bounded native cache, and a
  shared processing path. No function co-locates the known first-dialogue GPL
  #135 identity with the literal GPL tag construction, so that script has no
  direct static loader lead. The guarded processing helper's sole decoded
  direct caller forwards parameters and resident state, not a literal resource
  identity. Static evidence does not license general GPL opcode execution.
- `EXE-GOG-EVENT-001` establishes a mutable, linked runtime 13-byte selector
  record with a 2,600-byte entry-flow clear and deterministic link setup,
  multiple predicate traversals, a guarded `GPL ` resource-request path, and a
  secondary relinked chain. Its sole recovered initializer caller is the
  executable entry, which passes three resident words and meets an immediate
  lower-bound guard. Instruction context also confirms the shared clear call,
  13-byte successor-link loop, and secondary-head clear. These facts identify
  neither values' meanings nor table ownership. It does not identify the
  table's source/population path or connect a record to dialogue, quests,
  combat, or map triggers. Its pointer's initialized image is zero, so the
  later runtime population path is still required evidence.
- `EXE-GOG-RECORD19-001` separates a second linked 19-byte resident family
  that uses the same request entry. Shared processing does not establish a
  common source, a record meaning, or a player-visible feature for either
  table.
- `EXE-GOG-SCMD-001` establishes a separate 64-slot `SCMD` loader/cache. Its
  direct callers do not overlap the selector path, so `SCMD` remains opaque
  rather than a substitute event or combat implementation.
- `31E0:0EFF` requests an object's `OJFF` and then, for an object numbered
  outside 9,000 to 13,998, its `RDFF` of the same number, into the same 37-byte
  slot records (`FND-ACTOR-003`, `FND-ACTOR-005`). Its three resident callers
  pass values from resident state, arguments or registers (`FND-ACTOR-004`).
  `FND-IMAGE-005` continues that route
  into the PLAN/PLNR image dispatch. None of this identifies a runtime actor,
  animation, collision, target, or interaction behavior.
- The first hostile's object, 9,258, lies inside the range that takes no `RDFF`
  request and has no `RDFF` resource; no resident constant names it, and none
  of its `OJFF` words names a script or one of the 23 `RDFF` resources that hold
  its Look-panel label at offset 43 (`FND-ACTOR-007`, `FND-ACTOR-009`). The
  `RDFF` layout is unknown (`FMT-ACTOR-002`), and the label's bytes do not occur
  in `DSUN.EXE`. No source for the dynamic interaction text is identified.
- `RULE-UI-001` and `FND-UI-011` give how the window code picks the control
  under the pointer. No specific control handler or widget drawing path is known,
  so image-less controls remain geometry and event contracts rather than
  invented pixels.
- The `FBOV` overlay pack of `DSUN.EXE` is `FMT-EXE-001` to `FMT-EXE-005`.
  The code that loads overlays is not located (`FND-EXE-007`), so the pack
  describes where overlay code sits, not how or when it runs. Static queries
  into overlay code use the local-only mapped image described in
  `docs/GHIDRA.md`.
- `EXE-GOG-COMBAT-008` recovers a distinct panel route in that mapped image: a
  dispatcher branch reaches the #19003 cache initializer through a guarded
  four-record, 49-byte-stride operation. Its immediate post-panel callee is a
  two-caller `stdpatch`-related initialization path, and its unresolved
  `0x92e0` operand has no matching `RESOURCE.GFF` resource number. This route
  is therefore structural only; its state codes, record ownership/fields,
  event input, and every combat rule remain unknown.
- `EXE-GOG-SMALLTAG-001` finds no literal loader lead for `PLYL` or `CSEQ` in
  the resident image. `DATA-GOG-SMALLTAG-001` bounds their owned inventories
  and short envelopes, and `FND-PARTY-019` rejects a direct installed-`CHAR`
  resource number in every `PLYL` byte and unaligned 16-bit window; neither
  family has a known runtime role. The `GREQ` and `CACT` resources are
  `FMT-SAVE-002` and `FMT-SAVE-001`: overlay 192 writes and reads `GREQ` when
  it saves and loads a game (`FND-SAVE-004`, `FND-SAVE-005`), and `CACT` holds
  the identifiers of stored characters (`FND-PARTY-011`, `FND-PARTY-012`).
- `EXE-GOG-PREF-001` finds one raw `PREF` tag literal but no direct reference;
  it supplies no Preferences loader, settings schema, default, or control
  behavior.
- `FND-IMAGE-008` finds no operand for the static-title `BMP ` #11011, so it
  supplies no title loader or sequencing rule.
- `FND-TEXT-002` finds no recovered function combining `FONT` #100 with the
  tag words, and no recorded reference to the two `FONT` data occurrences. It
  supplies no font-selection, spacing, palette, or screen-layout rule.
- `FND-TEXT-004` finds two resident `TEXT` data occurrences with no recorded
  reference, and three more in the code of overlays 186 and 188. It assigns no
  text resource to a screen.
- `FND-IMAGE-006` records a 300-entry `BMP `/`CBMP` cache, a wrapper taking
  indices 0 to 320 that eight functions call, and a separate window-image
  request path. The title resource #11011 has no observed connection to either,
  so neither title sequencing nor image composition is inferred.
- `EXE-GOG-VIDEO-001` identifies nine coherent BIOS-video calls in one shared
  wrapper with fourteen direct callers, plus two raw matches outside decoded
  instruction boundaries. The
  wrapper accepts generic service/register values and does not connect any
  call to a resource, screen, palette, resolution, or layout. It is not a
  native rendering contract.
- The mouse and keyboard reach the game through the wrappers and hooks of
  `FND-INPUT-004` to `FND-INPUT-006`, which queue key words and mouse events as
  packets. They do not identify axes, a global transform, control hit testing,
  gestures, or a key-to-action map, so the measured canvas, the DSUI contracts
  and `RULE-INPUT-001` to `RULE-INPUT-003` remain authoritative.
  `FND-INPUT-007` to `FND-INPUT-009` exclude direct keyboard-port reads, a
  unified combat-key switch, and the one overlay route above the keyboard
  routine as a key dispatcher.
- `FND-IMAGE-009` finds no `PORT` tag in the resident image; its one
  occurrence is in overlay 199. It supplies no portrait loader, palette,
  drawing, dialogue, or timing rule.
- The character archive's tags occur only in overlay code (`FND-PARTY-009`):
  overlays 171, 184 and 186 keep the stored characters and their records
  (`FND-PARTY-012`), and overlay 182 loads characters 40 to 43 into the party
  (`FND-PARTY-013`, `RULE-PARTY-006`). `SVIEW.EXE` is a text viewer
  (`FND-PARTY-014`). `CHARTRAN.EXE`, once unpacked, transfers Dark Sun 1
  characters into the archive (`FND-PARTY-010`, `FND-PARTY-011`).
- `GPLI` #1 is an exact 329-by-24-byte opaque data envelope. Its four aligned
  third lane words are a strong GPL-number-set correlation (1,315 of 1,316
  occurrences are members, collectively covering every GPL ID), but repeats
  and one non-member reject a one-to-one map. No literal GPLI tag exists in the
  analyzed executable, and no decoded function directly combines the known GPL
  resource-135 ID with the literal GPL tag; no lookup role is assumed.
- `ITEMS.BIN` is `FMT-ITEM-001`, read only by the character transfer utility
  (`FND-ITEM-006`, `RULE-ITEM-006`); `DSUN.EXE` holds no name for it
  (`FND-ITEM-004`).

Addresses, methods, competing interpretations, and confidence are retained in
[GHIDRA.md](GHIDRA.md); open questions are kept in the plan rather than encoded
as APIs.

## Next evidence gates

The active gates are intentionally concrete:

1. Establish source ownership and game roles for the runtime selector records.
2. Obtain controlled observations for shipped-party membership, remaining UI
   transitions, native cadence, and visibly dynamic fields.
3. Trace or observe item, equipment, combat, quest, and persistence semantics
   before adding those systems.
4. Derive media decoding and timing from a corroborated source path; use a
   monotonic, CPU-independent playback policy rather than DOS-era delay loops.

Consult [HANDOVER.md](HANDOVER.md) for the immediate operational priorities and
[VALIDATION.md](VALIDATION.md) for the full verification procedure.

## Verification

```powershell
./tools/Verify-Configuration.ps1
./tools/Verify-Repository.ps1
./tools/Test.ps1
dotnet build DarkSunWakeRedux.slnx
dotnet run --project src/DarkSunWakeRedux.Game -- --smoke-test
```

The content smoke path additionally requires a previously verified local pack.
Any claimed parity change must update the relevant evidence, fidelity, and
parity records in the same cohesive batch.
