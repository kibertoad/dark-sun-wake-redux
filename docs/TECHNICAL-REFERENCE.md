# Technical reference

This is the concise technical map of the restoration as it stands. It is an
index to verified facts and explicit boundaries, not a replacement for the
detailed evidence records. For a rule or format to become implementation
behavior, its detailed record must be consulted first.

## Evidence and clean-room status

- Supported source: English GOG product `1432903719`, build
  `52095422060333615`. The exact fingerprint and edition facts are in
  [SOURCE-EDITIONS.md](SOURCE-EDITIONS.md).
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
descriptors. The current v31 pack contains 16,401 lossless DSOP corpus assets
plus 102 specialized DSIX, DSGP, DSTX, DSUI, DSCH, DSRG, and DSOB derivatives:
16,503 assets in total. Opaque preservation means the resource is retained and
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
  selected character envelopes have bounded readers.
- Twenty regions expose bounded identity, palette, terrain, geometry, tile,
  and placed-object records. Tyr has an extracted DSRG/DSOB graph used for the
  opening scene.
- GPL and MAS carry their exact source family in DSGP v2. Only recorded,
  fail-closed projections are interpreted; there is no generic bytecode
  interpreter.
- FLI, VOC, OGG, configuration, and item-table files are fully inventoried and
  preserved. Their unresolved decoder, routing, and rule semantics remain
  opaque.

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

## Determinism and time

Core transitions happen only from explicit commands. Rendering interpolates
presentation but cannot advance game rules. Snapshots and replay hashes make
the start flow and current exploration state reproducible.

- Route advancement is semantic and clock-free in Core. The runtime uses a
  bounded fixed-step accumulator and fixed-point visual interpolation.
- The opening actor's current single-cell footprint and 125 ms semantic step
  are explicit modern policies, not claims about the native implementation.
- `EXE-GOG-TIMING-001` establishes BIOS tick use only for startup/mixing paths,
  not actor or animation cadence. `EXE-GOG-MEDIA-001` finds no literal FLI
  header-validation lead in `DSUN.EXE`; raw cinematic speed fields are data,
  not assumed milliseconds.
- `EXE-GOG-RNG-001` establishes a 16-bit-seeded native LCG and bounded result
  transforms. Native seed ownership and call ordering are still open, so new
  rules do not silently consume that stream.

## Static-analysis boundaries worth preserving

Static analysis has produced reusable structural facts while deliberately
rejecting unsupported semantics:

- GPL/MAS use distinct selected source families, a bounded native cache, and a
  shared processing path. Static evidence does not license general GPL opcode
  execution.
- `EXE-GOG-EVENT-001` establishes a mutable, linked runtime 13-byte selector
  record with multiple predicate traversals and a secondary relinked chain. It
  does not identify the table's on-disk source or connect it to dialogue,
  quests, combat, or map triggers.
- `GPLI` #1 is an exact 329-by-24-byte opaque data envelope. No literal GPLI
  tag exists in the analyzed executable, so no lookup role is assumed.
- `ITEMS.BIN` is a verified 234-pair envelope. `DSUN.EXE` contains no literal
  filename lead, so no item/equipment mapping is inferred.

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
