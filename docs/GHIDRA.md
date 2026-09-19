# Ghidra setup for clean-room analysis

Ghidra is a recommended research tool for questions that the manual,
walkthrough, controlled play, and bounded data inspection cannot answer exactly.
Use it only against the repository owner's legally owned local executable.
Ghidra projects, binaries, byte dumps, screenshots, and full disassembly or
decompiler output must never be added to Git.

This workflow follows the established practice in
`C:\sources\rechaos-overlords`: fingerprint the executable first, keep a
disposable local analysis project, answer narrow questions with bounded scripts,
record address-level factual findings and confidence, then implement the behavior
independently with synthetic tests.

## Installed toolchain

The current research machine has:

| Tool | Version | Location |
|---|---|---|
| Ghidra | 12.1.3 | `C:\Users\kiber\AppData\Local\Programs\Ghidra\ghidra_12.1.3_PUBLIC` |
| Eclipse Temurin JDK | 21.0.12.1 | `C:\Users\kiber\AppData\Local\Programs\Java\jdk-21.0.12.1+1` |

Check `support\analyzeHeadless.bat` and `bin\java.exe` at these paths before
searching the machine or installing anything. A sandboxed process may need
permission to persist Ghidra preferences under the user profile; request that
narrow permission instead of reinstalling the toolchain.

## Reference executable

```text
C:\GOG Games\Dark Sun 2\DSUN.EXE
```

- Edition evidence: GOG product `1432903719`, installed build
  `52095422060333615`, English.
- Length: 634,416 bytes.
- SHA-256: `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`.
- Evidence ID: `GOG-1432903719`.

Always verify length and SHA-256 before interpreting an address. Findings from a
different executable belong to a separate edition record and address map.

## When to use Ghidra

Use Ghidra after forming a narrow, player-visible question, for example:

- the exact order or boundary of a combat calculation;
- RNG algorithm, seeding, range conversion, or consumption order;
- the producer/consumer relationship for an evidenced GFF field;
- a quest flag transition or timer whose behavior conflicts across sources;
- lookup-table dimensions, sentinel values, or resource identifiers;
- the cause of a reproducible original defect that needs a fidelity decision.

Do not begin with unrestricted decompilation or a speculative attempt to recreate
the original source tree. Prefer runtime observation for presentation and data
inspection for self-describing resources. Static evidence is strongest when a
focused finding and a controlled observation agree.

## Headless workflow

Create a uniquely named disposable project below `%TEMP%` and import the
fingerprinted executable directly:

```powershell
$wakeGhidraHome = 'C:\Users\kiber\AppData\Local\Programs\Ghidra\ghidra_12.1.3_PUBLIC'
$wakeJavaHome = 'C:\Users\kiber\AppData\Local\Programs\Java\jdk-21.0.12.1+1'
$env:GHIDRA_HOME = $wakeGhidraHome
$env:JAVA_HOME = $wakeJavaHome
$env:Path = "$wakeJavaHome\bin;$wakeGhidraHome;$wakeGhidraHome\support;$env:Path"
$wakeProjectRoot = Join-Path $env:TEMP ('dark-sun-wake-ghidra-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $wakeProjectRoot | Out-Null
& "$wakeGhidraHome\support\analyzeHeadless.bat" `
  $wakeProjectRoot DarkSunWakeAnalysis `
  -import 'C:\GOG Games\Dark Sun 2\DSUN.EXE' `
  -overwrite
```

Retain that project only locally and reuse it for focused scripts:

```powershell
& "$wakeGhidraHome\support\analyzeHeadless.bat" `
  $wakeProjectRoot DarkSunWakeAnalysis `
  -process 'DSUN.EXE' `
  -noanalysis `
  -scriptPath "$PWD\tools\ghidra" `
  -postScript ReportFunctionSummary.java 0x00000000
```

The address above is intentionally a placeholder example, not a finding. Replace
it only with an address selected through an evidence-led query.

## Bounded script pattern

Adapt the reusable headless scripts and methodology from
`C:\sources\rechaos-overlords\tools\ghidra` or their shared upstream source;
do not copy game-specific findings. Preferred scripts are deliberately bounded:

- explicit byte-pattern searches capped to a reviewable result count;
- function summaries for a small explicit address list;
- references to explicit addresses or symbols;
- no more than 256 data bytes from one explicit address;
- scalar searches capped to a reviewable result count;
- string-reference searches using a specific known UI label or error message;
- decompiler literal matches within one selected function;
- a single basic-block-sized decompile window, never adjacent windows stitched
  into a retained function;
- short instruction context that cannot cross the containing function;
- call-site filtering using an exact callee and known scalar arguments.

Script output is temporary navigation evidence, not production input and not
proof by itself. Never redirect broad output into the repository.

## Evidence record

### EXE-GOG-UI-001 - APFM offset 88 is an event mask

- **Question:** Does the varying 16-bit `APFM` field at payload offset 88
  describe appearance, a resource, or dispatch behavior?
- **Target:** GOG-1432903719 `DSUN.EXE`, 634,416 bytes, SHA-256
  `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Bounded finding:** Function `3d72:0515` compares the input tag with the
  little-endian `APFM` scalar at `3d72:0529`. At `3d72:054d` it bit-tests the
  word at structure offset `0x58` against a caller-supplied word, branches when
  no requested bit is present, and otherwise begins assembling a dispatch
  record containing the resource identity. This rejects an appearance/resource
  interpretation and supports an event-mask role.
- **Corroboration:** all 97 owned `APFM` records vary only at identity,
  dimensions, and offset 88; #19200/#19201 store masks 494/486. Those values do
  not resolve as resource numbers in `RESOURCE.GFF`.
- **Confidence:** high for event-mask role; unknown for individual bits.
- **Implementation:** `UiApplicationFrameResource.EventMask`, synthetic
  decoding test, and UI catalog output; no bit-level behavior is assigned.
- **Shared-layout corroboration:** the sibling dispatcher at `3d72:0fd4`
  verifies a `BUTN` tag and tests the same structure offset `0x58` against bit 4
  at `3d72:0fea`; its next `BUTN` branch tests bit 2 at `3d72:100e`.
  `UiButtonResource.EventMask` therefore preserves the same field.
- **Edit-box corroboration:** after verifying `EBOX` at `3d72:10b3`, the same
  dispatcher tests bit 2 at edit-box structure offset `0x96` at `3d72:10c1`.
  This is preserved as `UiEditBoxResource.EventMask`.
- **Mutation corroboration:** APFM helper `3f96:02f8` resolves the record by
  tag/identity, then mutates offset `0x58`: operation 1 ORs in the supplied
  bits, operation 2 ANDs with their complement, and operation 3 clears the
  word. This independently confirms mutable event registration rather than
  appearance data.
- **Matching rule:** handlers use nonzero bit intersection, implemented
  independently as `UiEventMasks.Matches`. Numeric bit meanings remain unnamed.

### EXE-GOG-UI-002 - WIND image field is not a generic background command

- **Question:** Does the nonzero resource number at WIND payload offset 58
  (`0x3a`) tell the generic window engine to tile or stretch that image as the
  window background?
- **Target:** GOG-1432903719 `DSUN.EXE`, 634,416 bytes, SHA-256
  `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Bounded finding:** the resource resolver's exact `WIND` branch at
  `39d1:04bc` searches a dedicated linked registry by identity at structure
  offset 8 and link at `0xee`. Complete bounded inspection of registration
  `3a8e:02b3`, redraw `3a8e:0003`, and activation `3a8e:060d` found no read of
  offset `0x3a`. Registration resolves each child by its tag and identity;
  redraw restores/clips registered rectangles; activation invokes an optional
  callback and then dispatches the resolved children.
- **Interpretation:** the generic WIND path does not establish `BMP` #19004 as
  an automatic tiled, stretched, or full-window background. The field may be
  consumed by screen-specific code or may serve another role. Both remain open.
- **Corroboration:** all six start-flow WIND records carry #19004 although the
  observed start window instead composes `BMP` #20029, #20028, and four controls
  over black; #19004 itself is only 96x9.
- **Confidence:** high for absence from the inspected generic path; unknown for
  the field's actual presentation role.
- **Implementation consequence:** preserve the resource identity in DSUI and
  the extracted DSIX asset, but do not tile, stretch, or draw it until an
  app-specific consumer or controlled observation establishes the operation.

### EXE-GOG-UI-003 - Party surface delegates slot meaning to application logic

- **Question:** Does the single full-canvas control under `WIND` #19502 expose
  the character-slot hit regions or semantic actions used by the party screen?
- **Target:** GOG-1432903719 `DSUN.EXE`, 634,416 bytes, SHA-256
  `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Bounded finding:** owned data inspection establishes that `WIND` #19502
  contains only image-less, event-mask-zero `BUTN` #2099 at (0,0), sized
  319x199. Exact scalar searches for 19502 and 2099 found no executable use.
  The generic window registry at `3a8e:02b3` clears optional callback fields at
  runtime offsets `0xf9` and `0xfd` (`3a8e:05d5`/`05df`). Activation
  `3a8e:060d` tests/calls `0xfd` at `06fa`/`0707`, dispatches resolved children,
  then tests/calls `0xf9` at `08aa`/`08b7`. A whole-program structure-offset
  query found only those reads, the registry clears, and generic setters at
  `3a8e:0d0c`/`0d3c`; the sole direct activation caller is the registered-window
  scan at `3a8e:1048` (`107a`).
- **Interpretation:** the serialized graph establishes one application surface,
  not the internal party-slot rectangles or their actions. Those semantics are
  installed or dispatched indirectly at runtime and remain unknown; panel art
  alone is insufficient evidence for hit boundaries.
- **Confidence:** high for the serialized surface, generic callback lifecycle,
  and absence of the two direct scalar constants; unknown for the application
  callback and slot partition.
- **Implementation consequence:** validate the exact 319x199 exclusive surface
  from DSUI and keep it semantically inert until controlled observation or a
  bounded indirect-call finding establishes the slot partition and actions.

### EXE-GOG-UI-004 - Preferences difficulty and About text tables

- **Question:** Which finite difficulty labels and About payload belong to the
  supported Preferences screen?
- **Target:** GOG-1432903719 `DSUN.EXE`, reverified at 634,416 bytes with
  SHA-256 `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Bounded finding:** the data block at `5000:a4b9` contains four adjacent far
  pointers to the null-terminated difficulty strings at `5000:a4f5` through
  `5000:a508`. The values are Easy, Balanced, Hard, and Hideous in that order.
  A `%C%C%C%s` formatter at `5000:a523` is followed at `5000:a52b` by exactly
  ten contiguous null-terminated help strings spanning the Preferences roles;
  the tenth terminator ends exactly where the About table begins. The same
  block contains nine adjacent far pointers to centered About format
  strings at `5000:a5cc` through `5000:a6c9`; each begins with three `%C`
  controls. The payload covers the title/copyright, publisher/address, support,
  and hint-line categories described by MANUAL-1994 page 15.
- **Conflict:** the manual calls the four choices Easy, Balanced, Hard, and
  Hideous, then calls the default “Average.” No Average label exists in the
  executable table. Treat Average as unresolved terminology, not a fifth
  setting or proof of the default index.
- **Confidence:** high for table shape, order, text boundaries, and format
  controls; high for the ten-description table boundary but only medium for its
  role ordering until runtime hover behavior is observed; unknown for the
  selected default, mutation handler, and exact About presentation geometry.
- **Bounded negative probes:** direct-reference reports for the table address
  `5000:a4b9` and its first label at `5000:a4f5` produced no references. Scalar
  reports for their 16-bit offsets (`0xa4b9`, `0xa4f5`, and `0xa523`) and for
  the two extracted difficulty-button identities (16308 and 16309) likewise
  produced no matches. These bounded results do not prove that the tables or
  controls are unused: segmented pointers and resource-derived values can be
  calculated at runtime. They do rule out treating a direct literal reference
  as evidence for a particular button handler, selected default, or settings
  storage location.
- **Implementation consequence:** a fixed-edition bounded reader validates the
  executable offsets, printable bytes, terminators, exact description span,
  counts, and About control prefixes. The Extractor strips only the three
  centering controls and writes the four labels, ten descriptions, and nine
  About lines to a local DSTX catalog. Original text is not committed and the
  Game never reads the executable.

### EXE-GOG-CHAR-001 - No adjacent default-party identity table established

- **Question:** Does the supported executable contain either disc character
  block #40-#43 or #50-#53 as an adjacent default-party lookup table?
- **Target:** GOG-1432903719 `DSUN.EXE`, reverified at 634,416 bytes with SHA-256
  `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.
- **Bounded finding:** exact four-byte and eight-byte little-endian searches
  found no #40-#43 sequence and no 16-bit representation of either block. Two
  four-byte #50-#53 matches at `5000:a368` and `5000:a379` lie inside the
  executable's upper- and lower-case hexadecimal digit strings, not a character
  table. The apparent matches were therefore rejected.
- **Corroboration:** `DATA-GOG-CHAR-006` establishes that both blocks occur on
  the disc and that one independently reported default member maps to #43, but
  does not establish the remaining selection.
- **Confidence:** high for rejecting these exact adjacent-table encodings;
  unknown for an indirect, computed, flagged, or non-adjacent selection.
- **Implementation consequence:** no default-party mapping is derived from this
  query. `ReportBytePattern`, `ReportDataBytes`, `ReportReferences`, and
  `ReportScalarConstants` retain bounded, reusable navigation methods for later
  evidence questions.

### EXE-GOG-REGION-001 - GMAP bit 0x40 blocks traversable cells

- **Question:** Which `GMAP` bit is consulted when the supported executable
  decides whether an in-bounds map cell blocks movement?
- **Target:** GOG-1432903719 `DSUN.EXE`, reverified at 634,416 bytes with SHA-256
  `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Bounded finding:** region loader `362c:01fa` requests tag scalar
  `0x50414d47` (`GMAP`) at `362c:0322`. The dedicated predicate at
  `25af:00e1` treats X outside 0..127 or Y outside 0..97 as blocked. After an
  optional actor-specific test of the low three bits, its in-bounds result is
  the cell byte masked with `0x40`. Seven bounded call sites use the predicate.
  The placement helper at `25af:05aa` refuses an already-`0x40` cell and sets
  `0x60`; removal helper `25af:05fb` requires `0x20` and clears `0x60`. This
  independently identifies `0x40` as the shared blocked/occupied bit and
  `0x20` as paired dynamic occupancy metadata. Coordinator `25af:0969`
  selects those removal/placement helpers, and callers `2d40:0f89`,
  `2d40:1045`, `2d40:32c5`, and `2d40:3388` iterate coordinate cells while
  invoking it. The footprint enumerator at `2d40:0f89` uses a 0x25-byte actor
  record and is called at `2d40:1143` and `2d40:118e`. Their enclosing path at
  `2d40:10ae` reads the actor X/Y fields, shifts each right by four, and passes
  the resulting cell coordinates to that enumerator. The rendering path
  `31e0:2bb9` -> `31e0:2837` reads the same actor X/Y fields as the sprite
  rectangle's world top-left and uses the stored width/height. Combined with
  `DATA-GOG-ACTOR-001`'s exact `(1184,1459)` top-left, this establishes opening
  anchor cell `(74,91)`. This corroborates per-cell footprint mutation without
  establishing the footprint shape, actor category, or update timing. A separate
  `0x80` test at `2778:0006` does not participate in this movement predicate.
- **Footprint-enumerator boundary:** a focused reinspection of `2d40:0f89`
  establishes that it reads the first signed word of the indexed 0x25-byte
  actor record and takes a separate branch when its absolute value is `430`.
  That branch calls `25af:0adf` with the requested cell coordinates and returns
  without the normal coordinate-enumeration loop or the `25af:0969`
  coordinator. The ordinary branch obtains candidate cells from its helper,
  filters them through `25af:00e1`, invokes the coordinator once for every
  accepted candidate, and records those accepted coordinates for its caller.
  Direct callers at `2d40:1143` and `2d40:118e` both use this routine.
- **Sentinel-footprint refinement:** `25af:0adf` invokes `25af:09cb` in its
  clear mode. That callee clears a centered 5x5 cell area through the existing
  removal helper and stores an out-of-map marker. Its paired placement mode,
  reached from the magnitude-430 branches in `2d40:32c5`, clears the previous
  area and then places every cell in the same 5x5 area except its four corners:
  21 cells. `2d40:3388` selects the paired clear mode. These are guarded
  dynamic paths, not a mapping from an OJFF or ETAB entry to that actor record.
  The initial enumerator's sentinel branch clears rather than enumerates cells.
  This establishes the bounded dynamic 21-cell pattern but does not establish
  what `430` denotes, its actor-data mapping, update timing, or the concrete
  footprint of the opening actor. In particular, it rules out treating the
  ordinary enumerator as proof that every actor has the same shape.
- **Corroboration:** fingerprinted Tyr `GMAP` contains only `00`, `40`, `80`,
  and `c0`, with respective counts 8,131, 2,044, 38, and 2,331. Its low five
  bits are therefore always zero; 8,169 cells are terrain-open when only the
  evidenced `0x40` block is applied.
- **Confidence:** high for bounds, `0x40` terrain/occupancy blocking, the
  `0x20` dynamic pairing, the guarded 21-cell sentinel pattern, the
  sentinel/ordinary occupancy-path split, and the opening anchor relationship
  in this executable; unknown for the sentinel's meaning, actor-data mapping,
  `0x80`, actor-specific low-bit policy in other regions, moving blockers, and
  the opening actor footprint.
- **Implementation consequence:** `RegionTerrainGrid` interprets only `0x40`,
  preserves and exposes every raw flag, treats out-of-bounds as closed, and is
  combined with the independent deterministic pathfinder by
  `ExplorationTerrainRoutePlanner`. The Core route session rechecks its supplied
  passability predicate immediately before each semantic step, including both
  diagonal side cells. `ExplorationOccupancySession` supplies bounded atomic
  per-cell placement for caller-provided immutable footprints and a live
  whole-footprint path predicate. `ExplorationActorMovementSession` commits that
  occupancy atomically with each accepted semantic route step, but does not
  infer a concrete actor footprint, movement cadence, or animation. The
  resource-exact opening actor uses the evidenced anchor `(74,91)` independently
  of those still-open behaviors.

### EXE-GOG-ACTOR-002 - The observed hostile object ID is not a direct combat entry point

- **Question:** Does the first observed hostile Tyr object, OJFF #9258, occur
  in the supported executable as a direct reference that can identify its
  encounter or combat handler?
- **Target:** GOG-1432903719 `DSUN.EXE`, reverified at 634,416 bytes with
  SHA-256 `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportBytePattern` searched all loaded blocks for OJFF #9258's
  explicit 16-bit little-endian encoding (`2a 24`). `ReportReferences` then
  queried the single hit, and `ReportDataBytes` classified a bounded 96-byte
  neighborhood.
- **Bounded finding:** exactly one raw match occurs at `1000:9754`. Ghidra
  has no reference to that address, and the surrounding bytes are an
  unreferenced data run rather than a decoded instruction or a resource-loading
  operand. The match therefore does not identify a code consumer.
- **Interpretation:** a 16-bit resource number is too short to be a reliable
  executable lead by itself. This result does not establish that #9258 is
  unused, nor does it identify its interaction, hostility, encounter,
  placement, combat state, or handler. It rules out treating this lone raw
  occurrence as a direct combat entry point.
- **Confidence:** high for the one raw occurrence and the absence of a direct
  reference in this analysis; unknown for all object and combat semantics.
- **Implementation consequence:** the runtime continues to use #9258 only for
  the separately observed cursor-eligibility boundary. A combat transition
  requires an independent state or call-path lead, or a controlled observation.
  `DATA-GOG-ACTOR-002` independently rejects its four neutral OJFF words as
  direct `SCMD` identifiers and records why matching `RDFF`/`OJFF`/`BMP `
  numbers cannot be treated as a semantic substitute.

### EXE-GOG-UI-005 - Hostile Look panel IDs do not identify an activation path

- **Question:** Do the observed hostile Look window, application frame, or
  button IDs occur as executable scalar constants that identify the code which
  opens or renders the panel?
- **Target:** GOG-1432903719 `DSUN.EXE`, reverified at 634,416 bytes with
  SHA-256 `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** After the panel had been independently measured in
  `DATA-GOG-INTERACTION-001`, `ReportScalarConstants` scanned every decoded
  instruction operand for the five exact decimal resource identities: `3020`
  (`WIND`), `15200` (`APFM`), and `15306`/`15307`/`15308`/`15309` (`BUTN`).
  The script has a 256-match output cap; this query produced no candidates.
- **Bounded finding:** none of the five requested scalar values occurs in a
  decoded instruction operand. The generic window dispatcher documented by
  `EXE-GOG-UI-001` and `EXE-GOG-UI-002` can process registered controls, but
  this query supplies no application-specific registration, activation, draw,
  target-name, level, hostility, or dismissal call path for this panel.
- **Interpretation:** the owned window graph and capture prove the panel's
  appearance for that one observed case, not how play reaches it. The absence
  of immediate literals does not prove the panel unused: IDs can be loaded or
  passed indirectly. It does rule out treating the resource numbers themselves
  as a direct executable activation lead.
- **Confidence:** high for the absence of these exact immediate scalar uses;
  unknown for all application-specific panel activation and rendering behavior.
- **Implementation consequence:** retain the measured DSUI graph and decoded
  assets, but do not wire an interaction panel into exploration or synthesize a
  target-capability mapping. A future runtime slice needs a controlled
  activation trace plus an independent data or executable call-path finding.

### EXE-GOG-UI-006 - Character-generation IDs do not identify control behavior

- **Question:** Do the character-generation window, class-label, EXIT, or DONE
  resource identities occur as immediate executable operands that identify their
  application-specific handlers?
- **Target:** GOG-1432903719 `DSUN.EXE`, reverified at 634,416 bytes with
  SHA-256 `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportScalarConstants` scanned every decoded instruction operand
  for the exact decimal identities `19503` (`WIND`), `18302` (EXIT), `19304`
  (DONE), and `2002` through `2009` (the eight class-label `BUTN` records). The
  query is capped at 256 matches and produced none.
- **Bounded finding:** none of the twelve requested values occurs as a decoded
  instruction scalar. The generic resource/control dispatch paths documented by
  `EXE-GOG-UI-001` and `EXE-GOG-UI-002` remain compatible with indirect or
  table-driven registration, but this query identifies no create-screen
  activation, class selection, validation, focus, DONE, or modal call path.
- **Interpretation:** the #19503 graph proves control identity and layout, not
  player-visible semantics. Absence of immediate literals does not prove a
  control unused, but rules out treating its resource number as a direct handler
  lead.
- **Confidence:** high for the absence of the exact immediate scalar operands;
  unknown for all application-specific character-generation transitions.
- **Implementation consequence:** preserve and render the measured graph, keep
  EXIT as the separately evidenced cancellation route, and leave class/DONE
  interactions inert. A future implementation needs a controlled screen trace
  plus an independent static/data call-path or state-transition finding.

### EXE-GOG-TIMING-001 - BIOS clock reads do not establish actor cadence

- **Question:** Does the supported executable contain a BIOS-tick timing path
  that can establish the native cadence for exploration movement or animation?
- **Target:** GOG-1432903719 `DSUN.EXE`, reverified at 634,416 bytes with
  SHA-256 `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportBytePattern` completed a whole-loaded-block search for
  the two-byte `INT 1Ah` encoding (`cd 1a`), yielding three matches without
  reaching its 100-match cap. `ReportInstructionContext` inspected all three;
  `ReportReferences` then followed the one decoded helper entry's direct
  caller.
- **Bounded finding:** at startup, `1000:0120` sets `AH` to zero, calls
  `INT 1Ah` at `1000:0122`, and stores returned `DX`/`CX` words. The sole
  decoded helper, `15f3:010c`, sets `AH` to zero at `15f3:011f`, calls the
  same interrupt at `15f3:0121`, rotates a caller-supplied word three times,
  and XORs it with returned `DX` before storing it back. Its one direct call
  is from `1425:111f`, which passes a word pointer and then stores the result
  in a 14-byte indexed record. The final byte-pattern match, `5000:6764`, is
  inside a non-coherent stream of invalid-looking decoded instructions and
  does not supply a trustworthy code path.
- **Interpretation:** the two coherent reads establish BIOS tick-of-day use at
  startup and for a caller-supplied word mixer. They do not establish an actor
  update interval, a frame scheduler, a delay loop, or a link to Tyr movement.
  Other timing mechanisms remain possible, so this is a boundary rather than
  evidence that native behavior is clock-free.
- **Confidence:** high for the three raw opcode matches and the two bounded
  coherent instruction paths; high that neither inspected path is an actor
  cadence; unknown for all native movement/animation timing.
- **Implementation consequence:** do not translate the BIOS tick frequency
  into a movement step. The runtime's fixed-step accumulator, bounded catch-up,
  and presentation interpolation remain explicit CPU-speed-independent policy
  until a movement call path and controlled cadence observation are available.

### EXE-GOG-ITEMS-001 - no literal ITEMS.BIN loader lead in DSUN.EXE

- **Question:** Does the supported main executable identify `ITEMS.BIN` by its
  literal filename, providing a direct static loader path for the known
  fixed-width pair table?
- **Target:** GOG-1432903719 `DSUN.EXE`, reverified at 634,416 bytes with
  SHA-256 `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportBytePattern` searched every mapped block for exact ASCII
  bytes `49 54 45 4d 53 2e 42 49 4e` (`ITEMS.BIN`).
- **Bounded finding:** no match was found.
- **Interpretation:** this rejects only a direct literal filename lead in
  `DSUN.EXE`. The file may be opened through a constructed or relocated name,
  another supplied executable, an external loader, or no runtime path at all.
  It does not identify either pair column, object-frame relation, equipment,
  combat, or inventory behavior.
- **Confidence:** high for the absent literal in this executable; unknown for
  file ownership and every table-field role.
- **Implementation consequence:** preserve `DATA-GOG-ITEMS-001` as a bounded
  opaque source fact. Do not add an equipment reader or item mapping until an
  independent materialization/consumer path or controlled observation exists.

### EXE-GOG-MEDIA-001 - no literal FLI header validation lead in DSUN.EXE

- **Question:** Does the supported executable contain the literal FLI header
  magic needed to identify an in-process cinematic decoder or its timing path?
- **Target:** GOG-1432903719 `DSUN.EXE`, reverified at 634,416 bytes with
  SHA-256 `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportScalarConstants` searched decoded instructions for the
  unsigned scalar `0xAF11`; `ReportBytePattern` separately searched every
  mapped block for its little-endian on-disk representation `11 af`.
- **Bounded finding:** neither query produced a match.
- **Interpretation:** this rejects only a literal FLI-header comparison or
  embedded header value in this executable. It does not establish that the
  cinematics are unused or unsupported: validation may be bytewise or
  constructed, live in another supplied helper executable, or be omitted while
  a stream is passed to another component. No decoder, raw-speed unit, frame
  cadence, or audiovisual sequencing follows from this negative result.
- **Confidence:** high for the absent scalar and raw two-byte pattern in the
  analyzed executable; unknown for every media implementation and timing role.
- **Implementation consequence:** retain `DATA-GOG-MEDIA-001`'s raw FLI speed
  values as data. Do not select a playback clock or attach a decoder based on
  the generic header alone; a future reader needs an independently corroborated
  source/implementation path and a monotonic, CPU-independent timing policy.

### EXE-GOG-RNG-001 - Native 32-bit linear-congruential random primitive

- **Question:** Does the supported executable contain a bounded random-number
  primitive whose state transition and output range can be reproduced without
  assigning it to an unevidenced gameplay rule?
- **Target:** GOG-1432903719 `DSUN.EXE`, reverified at 634,416 bytes with
  SHA-256 `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportFunctionScalarIntersection` searched for the two 16-bit
  halves (`0x015a`, `0x4e35`) of a common 32-bit LCG multiplier and returned
  one candidate. `ReportDecompileWindow` and `ReportInstructionContext`
  bounded its multiply, carry, state-write, mask, and return instructions.
  `ReportReferences` then enumerated direct callers of the generator and its
  modulo wrapper.
- **Bounded finding:** far helper `1000:0822` reads a 32-bit state, multiplies
  it by `0x015a4e35`, adds one with carry, and writes the low 32 bits back. It
  returns the new state's upper word masked to 15 bits, yielding `0..32767`.
  Its adjacent seed setter `1000:0811` clears the upper state word and stores
  its one 16-bit argument as the lower word; Ghidra finds no direct reference
  to that setter. The separate far wrapper `2834:061c` returns zero without
  calling the generator when its divisor is zero; otherwise it consumes one
  generator result and applies remainder reduction by the supplied divisor.
  Ghidra finds three direct calls to the generator and multiple direct calls to
  the wrapper, but those callers do not establish seed ownership, stream
  partitioning, consumption order, or any particular game mechanic.
- **Inclusive-range refinement:** direct caller `2d40:3a03` returns its first
  signed argument unchanged without a generator call when it is greater than
  or equal to its second. When lower is less than upper, it consumes one
  15-bit result and returns `lower + result * (upper - lower + 1) / 32768`.
  Its direct-reference query is empty, so this proves a generic range-helper
  contract but not a particular call-site meaning or input domain.
- **Repeated-roll refinement:** direct caller `28c9:391d` returns zero without
  a generator call for a non-positive first signed argument. Otherwise it
  iterates exactly that many times, consuming one result per iteration and
  summing `result * secondArgument / 32768 + 1`. The argument roles and every
  semantic use remain unknown; this is recorded as a scaled repeated-roll
  helper rather than assigned to combat or character generation.
- **Interpretation:** this is a native shared pseudo-random stream primitive,
  not evidence that every random-looking game outcome uses it. The modulo
  wrapper has ordinary modulo bias, which is a native implementation detail;
  it must not be silently replaced with rejection sampling where source parity
  matters.
- **Confidence:** high for recurrence, 15-bit result range, zero-divisor
  non-consumption, the inclusive-range and repeated-roll branch/formulae, and
  the 16-bit seed-setter boundary; unknown for seed source, callers' semantic
  purposes, consumption order, and the full set of indirect callers.
- **Implementation consequence:** `NativeRandom` preserves the bounded state
  transition, modulo, inclusive-range, and repeated-roll behavior with golden
  vectors. It is not wired to start flow, dialogue, combat, or other mechanics
  until each caller's seed and consumption contract is independently evidenced.

### EXE-GOG-COMBAT-001 - Combat hotkey dispatch is not a direct shared literal table

- **Question:** Does the supported executable contain an obvious single function
  or compact literal table that directly dispatches all six manual combat keys?
- **Target:** GOG-1432903719 `DSUN.EXE`, reverified at 634,416 bytes with
  SHA-256 `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Bounded finding:** the ordered six-byte PC-scancode pattern for Guard,
  next/previous target, end turn, Wait, and Space has no match. The reusable
  `ReportFunctionScalarIntersection` query also found no function containing
  all six expected scancodes (`0x22`, `0x31`, `0x19`, `0x10`, `0x11`, `0x39`) or
  all six uppercase-ASCII values (`G`, `N`, `P`, `Q`, `W`, Space).
- **Interpretation:** this rules out the specific direct-literal forms queried,
  not a shared dispatch mechanism. Input may be translated before dispatch,
  stored in a data table, or handled by multiple functions. The result does not
  identify a command handler, key state, target transition, or turn effect.
- **Confidence:** high for the absence of these two direct representations in
  the analyzed function and memory scans; unknown for the original dispatch
  architecture and every command's runtime semantics.
- **Implementation consequence:** `CombatHotkeys` remains a manual-evidenced
  input adapter only. Its binding order and rising-edge policy are independent
  design choices, not an original-dispatch parity claim.

### EXE-GOG-COMBAT-002 - The manual's -10 threshold is not a unique executable lead

- **Question:** Does the manual's -10 death threshold identify a focused
  executable routine suitable for resolving original incapacity semantics?
- **Target:** GOG-1432903719 `DSUN.EXE`, reverified at 634,416 bytes with
  SHA-256 `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Bounded finding:** `ReportScalarConstants` queried the unsigned 16-bit
  representation of -10 (`0xfff6`) in instruction operands. It reached its
  256-result cap across many functions before exhausting the program.
- **Interpretation:** the literal by itself is non-discriminating. This query
  neither identifies a hit-point field nor establishes signed comparison
  direction, status effects, target removal, or a combat call path.
- **Confidence:** high that this single-scalar query cannot isolate the
  documented threshold; unknown for the original incapacity implementation.
- **Implementation consequence:** `CombatHitPointRules` remains bounded by the
  manual threshold. Any original combat-state behavior requires a narrower
  multi-signal static-analysis question or a controlled runtime observation.

### EXE-GOG-COMBAT-003 - Combat action labels do not identify command handling

- **Question:** Do the executable's literal `COMBAT` or `GUARD` text occurrences
  identify a code reference that can be used as a focused lead for combat input
  or action resolution?
- **Target:** GOG-1432903719 `DSUN.EXE`, reverified at 634,416 bytes with
  SHA-256 `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportBytePattern` searched all loaded blocks for the explicit
  ASCII byte encodings of `COMBAT` and `GUARD`, reporting no more than 100
  matches and 20 inbound references per match. The single inbound `GUARD`
  reference was then bounded with `ReportInstructionContext` to eight
  instructions on each side.
- **Bounded finding:** `COMBAT` has four raw matches in `CODE_208` at
  `5000:9b4f`, `5000:9bb3`, `5000:9cd1`, and `5000:a6ec`; none has an inbound
  Ghidra reference. `GUARD` has two raw matches in the same block at
  `5000:8e52` and `5000:9cae`; only the first has one inbound READ reference,
  from `1bf3:66eb` in `28c9:19a1 FUN_28c9_19a1`. Its sixteen-instruction
  context compares `AL` with `0x20`, loads an indirect far pointer, adjusts a
  word through that pointer, and loops; it contains no documented combat
  scancode, direct combat-state reference, or action-resolution call.
- **Interpretation:** the queried literal labels are not a reliable combat
  command path. The sole direct reference is compatible with generic
  label/data processing, but the bounded context cannot establish its owner.
  This result does not show that the labels are unused and does not establish
  any input, target, turn, Guard, or rendering behavior.
- **Confidence:** high for the capped raw-match/reference results and the
  documented instruction context; unknown for label ownership and all combat
  semantics.
- **Implementation consequence:** retain the manual-evidenced `CombatHotkeys`
  adapter only. Do not connect its commands, infer a UI graph, or name a combat
  routine from these text literals; seek a multi-signal data/call-path lead or
  controlled observation first.

### EXE-GOG-MONR-001 - MONR has no raw executable tag literal

- **Question:** Does the sole `MONR` resource in `RESOURCE.GFF` identify an
  executable tag consumer that could constrain its role in monster, encounter,
  or combat processing?
- **Target:** GOG-1432903719 `DSUN.EXE`, reverified at 634,416 bytes with
  SHA-256 `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportBytePattern` searched every loaded memory block for the
  explicit ASCII encoding `4d 4f 4e 52`, capped by the reusable script's
  100-match limit.
- **Bounded finding:** no raw byte-pattern match exists in the loaded program.
- **Interpretation:** this rules out only an embedded literal-tag representation.
  It does not establish that the resource is unused, nor does it identify a
  loader, field layout, record boundary, monster mapping, encounter, or combat
  behavior. `DATA-GOG-MONR-001` independently rejects its arithmetic 81-byte
  candidate stride as a format claim.
- **Confidence:** high for the absent raw-literal query; unknown for the
  resource's loading and semantics.
- **Implementation consequence:** no `MONR` parser, extractor contract, or
  combat behavior is introduced from this query.

### EXE-GOG-ITEMS-001 - The executable does not embed the owned ITEMS.BIN filename

- **Question:** Does the supported executable embed the exact null-terminated
  `ITEMS.BIN` filename, providing a direct starting point for item-data loading
  or layout analysis?
- **Target:** GOG-1432903719 `DSUN.EXE`, reverified at 634,416 bytes with
  SHA-256 `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportBytePattern` searched every loaded memory block for the
  explicit null-terminated ASCII bytes `49 54 45 4d 53 2e 42 49 4e 00`.
- **Bounded finding:** no loaded-memory byte-pattern match exists.
- **Interpretation:** this rules out only the queried literal representation in
  this executable. The filename may be absent from the code path, constructed
  at runtime, owned by another executable or data layer, or supplied by a
  launcher. The result establishes neither an item-file reader nor any record
  structure, item behavior, or combat rule.
- **Confidence:** high for the absent raw-byte representation; unknown for
  item-data loading and all item semantics.
- **Implementation consequence:** `ITEMS.BIN` remains an unparsed source whose
  layout needs an independent bounded lead. Do not use a failed Ghidra
  defined-data/string classification as an absence claim; raw-byte search is
  the prerequisite negative check for this kind of question.

### EXE-GOG-CHAR-002 - CHARSAVE filename strings do not identify a party loader

- **Question:** Does an embedded `CHARSAVE.GFF` filename identify an executable
  code reference that can constrain the archive loader or START GAME's supplied
  party selection?
- **Target:** GOG-1432903719 `DSUN.EXE`, reverified at 634,416 bytes with
  SHA-256 `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** a full Ghidra auto-analysis pass completed first (including ASCII
  strings, data/reference analysis, disassembly, function discovery, and x86
  constant/reference analyzers). `ReportBytePattern` then searched every loaded
  block for the explicit ASCII bytes for `CHARSAVE.GFF`, capped at 100 matches;
  `ReportDataBytes` classified the shared 48-byte neighborhood; and
  `ReportReferences` queried each reported virtual address, capped at 200
  inbound references. A later `ReportFunctionScalarIntersection` query for the
  contiguous installed-only candidate IDs #29, #30, #31, and #32 found no
  function containing all four; the individual-ID query produced numerous
  unrelated scalar uses and was not treated as a resource-request trace. A
  separate raw byte-pattern search of fingerprinted `CHARTRAN.EXE` (24,761
  bytes, SHA-256
  `e99572016901c67135b9d1b14b6db3936078749779b89e5c62f7044fe722cf1d`) found
  no `CHARSAVE.GFF` token.
- **Bounded finding:** the raw filename bytes occur twice in `CODE_208`, at
  `5000:9188` and `5000:9197`. The first is a null-terminated filename; the
  second is that filename within a separate missing-file diagnostic, not a
  second filename entry. Even after the full analyzer pass, Ghidra reports no
  inbound reference to either byte address. No loader, archive open,
  character-record selection, or START GAME transition is identified by this
  query. The #29-#32 grouping is therefore not corroborated by a shared
  executable function, and the transfer utility's raw strings do not identify
  a second filename-based lead.
- **Interpretation:** the strings may be used through an indirect pointer,
  constructed/relocated data, another executable, or an unrecognized code path.
  The result rules out only treating either raw byte address as a direct static
  loader lead; it does not prove the archive unused. Likewise, a contiguous
  installed resource-ID run is not evidence that those records form the
  supplied party, and the negative string query does not prove `CHARTRAN.EXE`
  cannot access the archive indirectly.
- **Confidence:** high for the two raw matches and absent direct references;
  unknown for loader ownership and party-selection behavior.
- **Implementation consequence:** retain `ShippedPartyUnresolved` and do not
  assign the catalog's records to START GAME. A controlled startup observation
  or an independently corroborated loader/state trace is required before party
  logic is introduced.

### EXE-GOG-GPLI-001 - no native literal tag establishes GPLI ownership

- **Question:** Does the supplied executable identify `GPLI` as a resource tag,
  so that the 329 fixed-width records in `GPLDATA.GFF` can safely be assigned a
  native lookup or script role?
- **Target:** GOG-1432903719 `DSUN.EXE`, reverified at 634,416 bytes with
  SHA-256 `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1. The full analyzer
  pass completed before this focused query.
- **Method:** `ReportBytePattern` searched all mapped blocks for bytes
  `47 50 4c 49`, the exact ASCII spelling of `GPLI`.
- **Bounded finding:** the query found no matching byte sequence. Independently,
  `DATA-GOG-GPLI-001` establishes that `GPLDATA.GFF` `GPLI` #1 is 7,896 bytes,
  exactly 329 records of 24 bytes, each containing four consecutive six-byte
  lanes. The archive fact does not compensate for the absent executable lead.
- **Interpretation:** this rejects only a direct, literal-tag loader or lookup
  lead in this executable. The index may still be accessed through relocated
  data, a constructed tag, another binary, or an unrecognized code path. It
  does not establish that any lane identifies a GPL resource, a condition, an
  encounter, or a dialogue entry.
- **Confidence:** high for the absent literal and exact data envelope; unknown
  for runtime ownership and every field meaning.
- **Implementation consequence:** retain the resource losslessly as DSOP. Do
  not add a GPLI reader or connect it to dialogue until a constrained call path
  or controlled observation independently corroborates an interpretation.

### EXE-GOG-EVENT-001 - shared processing entry consumes linked runtime selectors

- **Question:** Do the coherent callers of the shared `172c:000c` processing
  entry establish a bounded record envelope or a feature-specific rule?
- **Target:** GOG-1432903719 `DSUN.EXE`, reverified at 634,416 bytes with
  SHA-256 `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1. The full analyzer
  pass completed before these focused queries.
- **Method:** `ReportReferences` enumerated the entry's eight direct far
  callers. Bounded instruction contexts classified their immediate argument
  setup. Four bounded decompilations of the `1695` callers compared every
  dereferenced offset and all chain termination and match predicates.
- **Bounded finding:** four callers traverse a resident table pointer at
  `5b7c:0700` from a supplied 16-bit index. Index `-1` terminates an
  unsuccessful traversal. Each record is addressed as `index * 13`; all four
  read words at offsets 0, 2, 4, and 6, bytes at 8, 9, and 10 where applicable,
  and the next index at offset 11. On a match, each supplies the two leading
  words to `172c:000c` with a literal third call argument of one. One selector
  compares the two words at 4/6 exactly to resident words `4c0d:0005` and
  `4c0d:0003` and applies byte 8 as a lower-threshold check against
  `4c0d:0001`; another treats bytes 8 and 9 as inclusive extents from words
  4 and 6 and applies byte 10 as that threshold; a third exactly compares word
  4 to `4c0d:0009`; the fourth accepts either order of resident words
  `4c0d:0009` and `4c0d:0007` against record words 4/6. No conclusion about
  the meanings of the resident values or fields follows from their arithmetic.
  Two recursive readers prove that offset 11 is a mutable list link rather
  than only a traversal field: `1695:010f` scans a supplied chain and delegates
  to `1695:07dd` when the low byte at offset 6 is zero, while `1695:0170` does
  the same when byte 8 is zero. That helper replaces the caller's index with
  the old offset-11 link, overwrites that link with the resident head at
  `4c13:032d`, and makes the removed index the new head. This establishes two
  data-driven node removals and relinking to a second resident chain, but not
  the chains' gameplay roles. A further
  reader, `1695:08db`, traverses the same offset-11 links and reports a
  low-byte success when resident word `4c0d:0009` equals record word 4. For the
  one currently designated resident index at `4c10:0017`, it also accepts a
  match against record word 6. The special index's identity and both fields'
  behavior remain unknown.
- **Interpretation:** this is a verified linked runtime-selector shape feeding
  the shared processing path, not evidence that it is a map trigger, dialogue
  option, combat event, or an on-disc file format. The apparent 13-byte stride
  is proven for these callers only; table allocation/population, index-source
  validity, execution outcome, and relation to GPL/MAS remain unknown.
- **Confidence:** high for the four routine-local record accesses, mutable
  chain shape, and call relationship; unknown for table ownership, field
  semantics, and player-visible behavior.
- **Implementation consequence:** do not add a resource parser, persist this
  data, or infer interaction triggers. Preserve the evidence as `Q18` until a
  source container, a static materialization path, or a controlled observation
  independently connects this runtime table to game content.

### EXE-GOG-GPL-001 - GPL and MAS are selected script-resource families

- **Question:** Does the original executable distinguish `GPL ` from `MAS `
  when loading script resources, and what bounded ownership/caching behavior
  can be established without assigning instruction semantics?
- **Target:** GOG-1432903719 `DSUN.EXE`, reverified at 634,416 bytes with
  SHA-256 `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1. The full analyzer
  pass completed before this focused query.
- **Method:** `ReportBytePattern` found the explicit four-byte `GPL ` and
  `MAS ` tags. `ReportInstructionContext` inspected their two assignments in
  one function. A single 78-line `ReportDecompileWindow` and its bounded caller
  list then established the surrounding control/data flow.
- **Bounded finding:** function `172c:04cf` rejects a sentinel first argument
  and every selector other than 1 or 2. It assigns `GPL ` for selector 1 and
  `MAS ` for selector 2, searches a fixed 16-entry cache, and either reuses a
  matching entry or invokes two common tag-plus-resource helpers. In the miss
  path, the first helper supplies the allocation extent, the code allocates one
  additional byte, the second helper fills the allocation, and the code writes
  byte `0x31` immediately after the copied range. Its only direct static caller
  is `172c:0388`. That wrapper avoids repeating an already-current
  identity/selector pair, tries a separate source before this cache, and records
  a successful pair. Its coherent caller `172c:0299` stores the requested
  identity and selector, invokes the wrapper, and reports a failed wrapper
  result through a low-disk-space diagnostic. Separately, when a distinct state
  byte is zero, it invokes `172c:01c1` with its second argument; this call is
  not statically conditional on the wrapper result. The latter is not an opcode
  dispatcher: it increments a byte counter, resets it to zero at 50, makes an
  opaque far transfer on that rollover, then copies the two resident bytes at
  `4c0e:0003` and `4c0e:0004` into parallel arrays at `4c13:013f + index` and
  `4c13:010d + index`. A nearby pop routine, `172c:0241`, reads the entries at
  the current index back into those two resident bytes and then decrements the
  counter, structurally providing a last-in-first-out operation. Its sole
  direct caller lies in non-coherent decoded instructions, however, so the pop
  cannot establish a live scheduler, script execution, or call order. This
  establishes 50-slot storage, rollover, and a LIFO pop shape, but not a live
  consumer; in particular, it does not establish that the call's second
  argument is recorded. A separate coherent chain begins at `172c:00a1`: after
  its nonzero first argument and resident-state guards, it calls `172c:0299`,
  then repeatedly processes work while its fourth argument is no greater than
  the counter and the same state byte remains zero. Each iteration calls
  `172c:20f5`, which obtains a counter-derived byte through `172c:2805` and
  `172c:281b`; the latter reads a word from a counter-indexed table at offset
  `0x0295`, adds the byte at resident offset `0x0193`, and uses the result to
  index a resident byte array beginning at `0x0255`. `172c:20f5` also calls
  `172c:20b3` and stores that derived byte at resident offset `0x032a` before
  passing it to `172c:018f`. Instructions at `172c:0193..01b5` prove that
  values through `0x80` optionally call a far callback at `57e0:02f6` and then
  make an indexed indirect call through the word table at offset `0x030a`;
  larger values take a separate `172c:20a2` path. This is a dispatch boundary,
  not proof that values are GPL opcodes or that any handler executes GPL/MAS
  bytes. The initial executable image contains zero bytes throughout
  `4c13:030a..0409`, covering the first 128 word slots, and the table base has
  no direct static reference beyond this indexed use. Static analysis therefore
  cannot enumerate registrations or assert a handler for any value; at least
  the examined slots require later runtime initialization before a meaningful
  indirect call. The guarded far entry itself has eight direct calls from three
  modules: `277b` has one call whose immediately preceding setup pushes two
  literal words, `1695` has four calls that source a pair of words from
  external records indexed with a 13-byte stride, and `2d40` has three calls
  that source pairs from records indexed with a 19-byte stride. The caller
  contexts establish shared use across those record families only. They do not
  identify either record layout, the words' roles, or a dialogue, combat, map,
  or other feature ownership. The other apparent wrapper call lies in
  non-coherent decoded instructions and is not accepted as evidence. No
  GPL/MAS opcode dispatch, script meaning, cache eviction policy, or
  caller-level feature meaning is established.
- **Interpretation:** `GPL ` and `MAS ` are distinct native input families,
  not interchangeable labels for the same extracted payload. The helpers and
  cache establish a native loading boundary, but do not license execution of
  source bytes or a general interpreter.
- **Confidence:** high for selector validation, tag choice, fixed cache bound,
  bounded allocation/copy sequence, 50-slot storage/rollover/LIFO-pop shape,
  and the separately verified range-gated indirect dispatcher; unknown for
  opcode semantics, script side effects, cache replacement/ordering, live
  recorder consumption, handler meanings, and higher-level caller intent.
- **Implementation consequence:** DSGP v2 records the exact source tag with
  resource identity and bytes. The first-Tyr dialogue projections require
  `GPL ` and the global-string projections require `MAS `; both remain
  fail-closed, bounded projections rather than a GPL interpreter.

### EXE-GOG-SOUND-001 - No literal SOUND.CFG loader path

- **Question:** Does the supported executable contain a direct textual link
  from `SOUND.CFG` to a Preferences setting or configuration-loading path?
- **Target:** GOG-1432903719 `DSUN.EXE`, reverified at 634,416 bytes with
  SHA-256 `ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** after the normal analyzer pass, `ReportBytePattern` searched all
  loaded blocks for the nine-byte ASCII sequence `SOUND.CFG` and would have
  reported each matching address and its direct references. The query produced
  no matches.
- **Bounded finding:** no literal `SOUND.CFG` pathname is present in this
  executable image. Therefore this query supplies no direct executable call,
  path reference, or control binding that can connect the 59-byte file to the
  Preferences controls.
- **Interpretation:** this rules out only a static literal-name lead in this
  binary. It does not rule out a dynamically assembled path, a configuration
  reader in another executable/module, a differently named source, or runtime
  propagation through resident state.
- **Confidence:** high for the bounded literal-name absence; unknown for every
  configuration field and setting behavior.
- **Implementation consequence:** retain inert Preferences mutations. Do not
  map `SOUND.CFG` fields, setting ranges, defaults, or audio timing from this
  absence; seek a corroborating controlled observation or a bounded finding in
  the actual configuration-owning path.

For each useful finding, add a concise entry here or in the relevant
`docs/RULES-AND-EVIDENCE.md` / `docs/ORIGINAL-FORMATS.md` section with:

- finding ID and the exact question asked;
- executable edition, length, and SHA-256;
- Ghidra and JDK versions plus load settings;
- virtual/file address or bounded address range and call relationship;
- observed constants, comparisons, reads/writes, and operation order described
  independently in our own words;
- competing interpretations and rejected hypotheses;
- confidence: `unknown`, `low`, `medium`, `high`, or `verified`;
- the controlled runtime observation or data fact used for corroboration;
- the production entry point and synthetic automated test, once implemented.

An interpretation remains provisional until independent evidence supports its
semantics. Ghidra's pseudocode can misidentify types, reuse variables, and fold
control flow; check instruction context when a conclusion depends on those
details.

## Clean-room boundary

- Never copy decompiled implementation, original names inferred only from debug
  remnants, or original control structure into production code.
- Implement factual behavior independently using repository-owned names and
  architecture.
- Keep `DSUN.EXE`, temporary projects, dumps, listings, and screenshots under
  ignored `analysis/original/`, `%TEMP%\dark-sun-wake-ghidra-*`, or another
  local-only path.
- Commit only independently written findings, bounded reusable scripts, and
  synthetic tests.
- A static finding may guide a parser or rule, but production code must never
  depend on an address or execute/load the original binary.
