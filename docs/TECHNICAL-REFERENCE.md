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
descriptors. The current required revision 34 pack contains 16,401 lossless DSOP corpus assets
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
  media semantics remain opaque. The loaded sound-helper image likewise has no
  complete VOC header signature or `.VOC` filename-extension literal, so it
  establishes no decoder, codec, filename mapping, or timing contract.
- `SOUND.CFG` has a bounded 59-byte envelope, but neither the main executable
  nor the separately shipped sound helper contains its literal pathname; the
  main executable also has no literal `SOUND.INI` pathname. This is not
  evidence of configuration ownership or Preferences behavior. Neither
  case-variant of the configuration file's `.adv` module suffix occurs anywhere
  in the helper's complete physical file, including its overlay, which likewise
  does not establish module ownership or driver-selection behavior.

## Current runtime boundary

The current Slice 3 build can verify and open the local pack, render bounded
startup and interface shells, show the observed Tyr opening viewport, route
movement through deterministic A*, and render the known cursor family.
Dialogue preview uses the original fixed 320x200 canvas and measured dialogue
chrome while the exploration view may expand to the physical display aspect.
The bounded first dialogue projection owns only the validated opening paths;
unknown visible targets stay inert.

The precise screen layers, logical geometry, image mapping, and observation
confidence are maintained in [UI-ATLAS.md](UI-ATLAS.md). The plan and current
slice acceptance criteria are in [IMPLEMENTATION-PLAN.md](IMPLEMENTATION-PLAN.md).

## Destination-screen and combat evidence boundary

The Character, Inventory, Cast/Use, and Effects destinations share the
resource-backed character-screen family. WIND #11500 has 86 ordered children:
64 APFM, one EBOX, and 21 BUTN records. Five independently evidenced
navigation controls route today; all interior controls stay inert. The graph
preserves distinct event-mask classes and exact geometry, but those values do
not name a field, widget appearance, focus state, or action. In particular,
the character-view #11308 button reuses the Preferences Game Menu icon but has
no established route. No immediate native handler operand was found for that
button, Return #10308, or #11318/#11319/#11320, the only character-view
buttons with nonzero masks.

The executable has an adjacent eight-entry UI text vocabulary beginning with
View Character, View Inventory, Cast Spells/Use Psionic, and Current Spell
Effects. The table base and first text entry have no direct references, and no
decoded instruction directly names the table's `0xab20` base or the adjoining
character-vocabulary table bases. That excludes only simple absolute access;
the vocabulary and ordering still do not establish a screen, field projection,
text renderer, or activation path. Owner-confirmed captures establish visible
party/destination-shell composition and captions, not character-record field
semantics or item/spell behavior. See DATA-GOG-UI-010,
OBS-GOG-PARTY-001, EXE-GOG-UI-009, and EXE-GOG-UI-010 before extending these
screens.

Combat remains evidence acquisition only. The owner reports that entry leaves
the map and character presentation substantially unchanged except for the
absence of the compact status panel; the first stable combat state shows the
currently active character's panel. The owner identifies dsun_009 as an enemy
movement frame, dsun_011 as enemy striking, and dsun_012 as a player turn
labelled Thy'rokh, with visible panel strings
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
`EXE-GOG-COMBAT-007` likewise follows the bounded native mouse-coordinate
guard one direct caller layer without identifying a combat screen, click
action, target, approach, attack, or confirmation handler.
`DATA-GOG-MONR-002` corrects the opaque `MONR` structural lead from a rejected
14-by-81 arithmetic split to a stronger 27-by-42 aligned-word envelope with a
constant tail. No executable loader or combat meaning is established.

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
- An `OJFF` has bounded X/Y offsets and a bitmap reference. Two native OJFF
  tag-selection paths exist. The sole observed successful-result consumer
  transfers portions beginning at offsets `0x00`, `0x02`, `0x04`, `0x0a`,
  `0x0b`, and `0x0c` into a 37-byte resident record family. This corroborates
  structural consumption only, not OJFF field names, actor ownership,
  collision, interaction, or animation.
- A bounded native caller chain reaches distinct `PLAN` and `PLNR` image
  dispatch after tile/object image resolution. It confirms the static image
  route but does not establish how a frame is selected or scheduled.
- The one observed opening leader remains separately evidenced: OJFF #305,
  bitmap #599 frame 0, world top-left `(1184,1459)`, and collision anchor
  `(74,91)`. Its footprint, all later frames, cadence, party formation, and
  other actor behavior are unknown.

See `DATA-GOG-REGION-001`, `DATA-GOG-OBJECT-001`, `DATA-GOG-SCENE-001`,
`DATA-GOG-ACTOR-001`, `EXE-GOG-OJFF-001`, and `EXE-GOG-IMAGE-001` before
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
| Difficulty | The executable table orders the four labels Easy, Balanced, Hard, Hideous; the manual assigns it to combat difficulty. FAQ-81038 reports a hostile-HP-at-spawn hypothesis (`FAQ-81038-COMBAT-001`) | Selected default, its relationship to the manual's conflicting “Average” wording, mutations, hostile-HP multipliers/rounding/timing, and rule consumers |
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
`DATA-GOG-UI-011`, `DATA-GOG-PREF-001`, `EXE-GOG-UI-004`,
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
  the embedded filename/path entries. `EXE-GOG-TITLE-002` likewise finds no
  recovered function that co-locates static-title `BMP ` #11011 with its tag
  words. Raw cinematic speed fields, filename order, and the title asset are
  data, not assumed milliseconds or a schedule.
- `EXE-GOG-RNG-001` establishes a 16-bit-seeded native LCG and bounded result
  transforms. Its direct static callers are only the generic modulo,
  inclusive-range, and repeated-roll helpers. The modulo wrapper also reaches
  two bounded selection sites and a generic threshold selector over opaque
  six-byte entries. Nine threshold draws use the fixed divisor 10; the two
  selection draws use guarded local counts. The table and feature ownership
  are unknown. Native seed ownership and rule-level call ordering are still
  open, so new rules do not silently consume that stream.

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
- `EXE-GOG-RDFF-001` places `RDFF` beside that same separate 37-byte record
  path. It likewise has no direct selector connection and remains opaque.
- `EXE-GOG-OJFF-001` establishes two native OJFF resource lookup paths and
  a sole observed post-lookup transfer into the 37-byte resident path. The
  selected source offsets are structural data accesses, not assigned object
  field meanings. `EXE-GOG-IMAGE-001` continues that bounded route into
  distinct PLAN/PLNR image dispatch. Its 320-entry coordinate-based candidate
  scan has no recovered direct caller, so it does not identify a target action.
  Neither finding identifies a runtime actor, animation, collision, target, or
  interaction behavior.
- `DATA-GOG-RDFF-001` finds the `Draxan` pattern in 23 distinct `RDFF`
  resources (and an aggregate resource), all at relative offset 43. The
  bounded `OJFF` #9258 overlap check finds none of its four observed raw
  words in that label-bearing subset. `DATA-GOG-RDFF-002` further shows that
  1,216 `RDFF` resources are 68 bytes while the remaining 427 share a
  `43 + 33*n` size residue; the 23 label-bearing resources are only a
  high-length subset of that residue. `EXE-GOG-RDFF-002` finds no direct
  displacement 43 or 76 in either known RDFF-tag path. Together with the
  direct lookup callers, which pass local, argument, register, or
  resident-state values rather than a recovered fixed resource number, this
  rejects a direct field-to-label mapping for the first hostile Look panel. It
  does not identify an alternative source or permit dynamic interaction text
  to be rendered. The main executable's 276,672-byte physical MZ overlay also
  contains neither the exact six-byte label nor its NUL-terminated form, so
  that exact spelling has no loaded-image or overlay literal path; other
  encodings and runtime/resource paths remain open.
- `EXE-GOG-UI-007` establishes a generic resource-derived UI input boundary:
  current pointer state is resolved against `APFM`, `BUTN`, or `EBOX` children,
  then event-bit guards select indirect handlers. It identifies neither a
  specific control handler nor any widget drawing/chrome path, so serialized
  image-less controls remain geometry/event contracts rather than invented
  pixels.
- `EXE-GOG-OVERLAY-001` now has a reproducible `fbov-profile` envelope check:
  DSUN's physical overlay is `FBOV`, declares 276,656 payload bytes, and its
  229 opaque descriptors occupy MZ-file offsets `[307328,309160)`. Their raw
  endpoint totals cannot directly describe that payload, so this validates a
  container boundary only—not an overlay address map, loader call, resource
  reader, combat rule, or executable behavior.
- `EXE-GOG-OVERLAY-002` rules out the first concrete file-I/O candidate: the
  only loaded-image routine that directly uses both the bounded DOS seek and
  read wrappers scans caller-supplied six-byte signatures and lengths. It has
  no recovered MZ-end, `FBOV`, or descriptor-table input, so it does not map
  the physical overlay or establish a combat/resource code path.
- `EXE-GOG-SMALLTAG-001` finds no literal executable loader lead for `GREQ`,
  `CACT`, `PLYL`, or `CSEQ`. `DATA-GOG-SMALLTAG-001` bounds their owned
  inventories and short envelopes; `DATA-GOG-PLYL-002` additionally rejects a
  direct installed-`CHAR` resource number in every unaligned 16-bit `PLYL`
  window, while `DATA-GOG-SMALLTAG-002` rejects it in every `GREQ` and `CACT`
  window. None of these results assigns the opaque families a runtime role or
  identifies the supplied party.
- `EXE-GOG-PREF-001` finds one raw `PREF` tag literal but no direct reference;
  it supplies no Preferences loader, settings schema, default, or control
  behavior.
- `EXE-GOG-TITLE-002` finds no decoded instruction operand for static-title
  `BMP ` #11011 and no recovered function that combines that identity with
  both correctly ordered tag words. These bounded negative results supply no
  title loader or sequencing rule.
- `EXE-GOG-FONT-002` finds no recovered function combining interface `FONT`
  #100 with both literal tag words. Its two raw `FONT` data occurrences have
  no recorded direct references. These facts supply no font-selection, glyph,
  spacing, palette, or screen-layout rule.
- `EXE-GOG-TEXT-001` finds two raw `TEXT` data occurrences, neither with a
  recorded direct reference or containing instruction. This excludes only a
  direct literal-tag loader lead; it neither assigns one of the 62 bounded text
  resources to a screen nor establishes native typography or timing.
- `EXE-GOG-IMAGE-002` establishes a bounded 300-entry native `BMP `/`CBMP`
  selector/cache, a shared 321-index wrapper used from eight recovered
  functions (including the OJFF route), and a separate generic window-image
  request path. The title resource #11011 has no observed connection to either,
  so neither title sequencing nor image composition is inferred.
- `EXE-GOG-VIDEO-001` identifies nine coherent BIOS-video calls in one shared
  wrapper with fourteen direct callers, plus two raw matches outside decoded
  instruction boundaries. The
  wrapper accepts generic service/register values and does not connect any
  call to a resource, screen, palette, resolution, or layout. It is not a
  native rendering contract.
- `EXE-GOG-MOUSE-001` finds generic mouse-service wrappers, two direct
  coordinate-query callers, and one caller-specific interior guard accepting
  only a `1..317`/`1..198` returned pair. It does not identify axes, a global
  transform, control hit testing, gestures, or pointer behavior, so the
  measured canvas and DSUI contracts remain authoritative.
- `EXE-GOG-KEYBOARD-001` finds one decoded BIOS modifier-status wrapper. Its
  external caller masks two returned bits while processing an opaque 37-byte
  resident record; no recovered keyboard site reads a key code or maps a
  player action. Manual and observed keyboard routes therefore remain separate
  evidence, not a consequence of this generic path. `EXE-GOG-KEYBOARD-002`
  additionally excludes only direct `IN AL,60h` and bounded immediate-DX
  keyboard-controller port forms; it does not identify a replacement
  key-acquisition or combat-command route.
- `EXE-GOG-PORT-001` finds no literal `PORT` tag in the executable. It does
  not contradict the observed first-Tyr portrait, but supplies no general
  portrait loader, palette, drawing, dialogue, or timing rule.
- `EXE-GOG-CHAR-003` finds no direct literal-tag lead for the character
  archive: raw `CHAR` bytes have no direct references and `PSIN` is absent.
  The separately fingerprinted `SVIEW.EXE` has neither a literal
  `CHARSAVE.GFF` pathname nor a `CHAR` tag (`EXE-GOG-CHAR-004`), while
  `CHARTRAN.EXE` has one unreferenced raw `CHAR` occurrence and no `PSIN`
  pattern (`EXE-GOG-CHAR-005`). Neither utility result can select a shipped
  party.
- `GPLI` #1 is an exact 329-by-24-byte opaque data envelope. Its four aligned
  third lane words are a strong GPL-number-set correlation (1,315 of 1,316
  occurrences are members, collectively covering every GPL ID), but repeats
  and one non-member reject a one-to-one map. No literal GPLI tag exists in the
  analyzed executable, and no decoded function directly combines the known GPL
  resource-135 ID with the literal GPL tag; no lookup role is assumed.
- `ITEMS.BIN` is a verified 234-pair envelope. `DSUN.EXE`, `CHARTRAN.EXE`, and
  `SVIEW.EXE` each lack the queried literal filename/stem forms, so no
  item/equipment mapping or loader is inferred.

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
