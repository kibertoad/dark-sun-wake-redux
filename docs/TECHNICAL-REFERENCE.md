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
- The [spec](../spec/README.md) holds every fact about the original, with its
  evidence. [`PARITY.md`](../PARITY.md) states what the rebuild implements of it,
  and [`deviations/`](../deviations/) where it departs on purpose.
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
descriptors. The current required revision 36 pack contains 16,401 lossless DSOP corpus assets
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
  preserved. Cinematics are the VIDEO area of the spec: the FLI layout is
  `FMT-VIDEO-001`, and `RULE-VIDEO-001` to `RULE-VIDEO-004` give how a
  cinematic is copied from the disc, played frame by frame, and replaced by
  still pictures when it cannot play. Chunk decoding and when cinematics 2 to 5
  play are open (`Q-VIDEO-001`, `Q-VIDEO-002`).
- Sound is the SOUND area of the spec. Voice files and `BVOC` resources are
  `FMT-SOUND-001`; the effect and speech players are `RULE-SOUND-001` and
  `RULE-SOUND-002`, which build their file names from the `.VOC` patterns in
  the executable (`FND-SOUND-006`). Music is chosen from `DJ.DAT`
  (`FMT-SOUND-002`) by `RULE-SOUND-003` and plays as a disc audio track, which
  GOG supplies as `MUSIC/TrackNN.ogg`. How the sound library plays a sample or
  a track is not read (`Q-SOUND-002`, `Q-SOUND-004`), so the rebuild plays no
  sound yet.
- `SOUND.CFG` is `FMT-CONFIG-001`: the sound helper `SOUND_DS.EXE` reads
  `SOUND.INI` (`FMT-CONFIG-002`) and writes it (`FND-CONFIG-004`), and the main
  executable reads it through its sound library (`FND-CONFIG-005`). The main
  executable never names or starts the helper (`FND-SOUND-005`), and the
  helper holds no VOC header, `.VOC` name or BIOS wait (`FND-SOUND-003`); its
  port output sets the timer chip and a card with a variable base
  (`FND-SOUND-004`), which the modern runtime does not emulate.

- The opening actor's current single-cell footprint and 125 ms semantic step
  are explicit modern policies, not claims about the native implementation.
- The original's clocks known so far are `RULE-TIME-001` (a millisecond
  wait that watches the timer chip, used for fixed pauses) and
  `RULE-TIME-002` (a timer interrupt at the shortest period any timer slot
  asks for). Neither is tied to actor movement or animation yet
  (`Q-TIME-001`), and the BIOS time-of-day reads, `INT 15h` sites and
  display-status copy are not clocks (`FND-TIME-001` to `FND-TIME-003`).
  `FND-SOUND-003` separately finds no `INT 15h` opcode in the sound helper.
  None must be recreated as an interrupt-disabled render
  loop, hardware timer, or actor scheduler.
  The FLI player times frames with a 1 ms timer slot of that library
  (`RULE-VIDEO-004`) and ignores the header's speed field (`RULE-VIDEO-002`).
  The title picture `BMP ` #11011 is the last still picture the opening's
  fallback shows (`FND-VIDEO-006`), which is why no instruction holds its
  number (`FND-IMAGE-008`).
- The random number generator and its reductions are `RULE-RNG-001`. Its seed
  and the rules that draw from it are not known, so new rules do not consume
  it yet.

## Static-analysis boundaries worth preserving

Static analysis has produced reusable structural facts while deliberately
rejecting unsupported semantics:

- The script interpreter is `RULE-SCRIPT-010` to `RULE-SCRIPT-008`: it loads
  `GPL` and `MAS` scripts into a cache, runs their instructions through a
  129-entry dispatch table, and registers attack and move-tile triggers in a
  pool of 13-byte records (`FMT-SCRIPT-004`). A separate list of 19-byte
  records (`FMT-SCRIPT-005`) and a 64-slot `SCMD` cache (`FND-SCRIPT-018`)
  also exist; what fills the trigger lists and when the game tests them is
  `Q-SCRIPT-002`.
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
- The six `PLYL` resources are playlists of song and sound-effect pairs that
  match an uncalled playlist routine (`FND-SOUND-014`), and `CSEQ` #1000 is an
  XMIDI sequence that plays no note (`FND-SOUND-015`). `DSUN.EXE` names
  neither tag, and no code that reads either family is located. The `GREQ` and `CACT` resources are
  `FMT-SAVE-002` and `FMT-SAVE-001`: overlay 192 writes and reads `GREQ` when
  it saves and loads a game (`FND-SAVE-004`, `FND-SAVE-005`), and `CACT` holds
  the identifiers of stored characters (`FND-PARTY-011`, `FND-PARTY-012`).
- The `PREF` tag is pushed only by the save and load routines of overlay 192
  (`FND-CONFIG-002`); it supplies no settings names, defaults, or control
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
- `FND-VIDEO-003` finds that the game sets BIOS mode `0x13` unchained into
  four planes at startup, plain mode `0x13` for cinematics and text mode 3 on
  exit, and that the `INT 10h` wrapper at `1000:1136` serves the C runtime's
  text output. The routines that draw into the planes were not read.
- The mouse and keyboard reach the game through the wrappers and hooks of
  `FND-INPUT-004` to `FND-INPUT-006`, which queue key words and mouse events as
  packets. They do not identify axes, a global transform, control hit testing,
  gestures, or a key-to-action map, so the measured canvas, the DSUI contracts
  and `RULE-INPUT-001` to `RULE-INPUT-003` remain authoritative.
  `FND-INPUT-007` to `FND-INPUT-009` exclude direct keyboard-port reads, a
  unified combat-key switch, and the one overlay route above the keyboard
  routine as a key dispatcher. A key routine in overlay 190 compares BIOS key
  words with a table of 33 and handles the combat keys `G`, `W` and `Q`
  (`FND-COMBAT-025`); its other keys are not read.
- `FND-IMAGE-009` finds no `PORT` tag in the resident image; its one
  occurrence is in overlay 199. It supplies no portrait loader, palette,
  drawing, dialogue, or timing rule.
- The character archive's tags occur only in overlay code (`FND-PARTY-009`):
  overlays 171, 184 and 186 keep the stored characters and their records
  (`FND-PARTY-012`), and overlay 182 loads characters 40 to 43 into the party
  (`FND-PARTY-013`, `RULE-PARTY-006`). `SVIEW.EXE` is a text viewer
  (`FND-PARTY-014`). `CHARTRAN.EXE`, once unpacked, transfers Dark Sun 1
  characters into the archive (`FND-PARTY-010`, `FND-PARTY-011`).
- `GPLI` #1 lists script entry points (`FMT-SCRIPT-003`); overlay 187
  converts the entry points of trigger records to and from its entries
  (`FND-SCRIPT-017`).
- `ITEMS.BIN` is `FMT-ITEM-001`, read only by the character transfer utility
  (`FND-ITEM-006`, `RULE-ITEM-006`); `DSUN.EXE` holds no name for it
  (`FND-ITEM-004`).

Addresses, methods and competing interpretations are kept in the spec's
findings, and [GHIDRA.md](GHIDRA.md) describes the tools. Open questions are
items in `queue/` rather than encoded as APIs.

## Next evidence gates

The active gates are intentionally concrete:

1. Find what fills the script trigger lists and when the game tests them (`Q-SCRIPT-002`).
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
