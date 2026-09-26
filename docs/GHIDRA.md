# Ghidra setup for clean-room analysis

Ghidra is a recommended research tool for questions that the manual,
walkthrough, controlled play, and bounded data inspection cannot answer exactly.
Use it only against the repository owner's legally owned local executable.
Ghidra projects, binaries, byte dumps, screenshots, and full disassembly or
decompiler output must never be added to Git.

This workflow follows the established practice in
`C:\sources\rechaos-overlords`: establish and document the executable
fingerprint once for its stable approved path, keep a
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
- XXH3-128: `e296af55ba2ecde7e77f555c90f33d0b`.
- Evidence ID: `BLD-GOG-EN-1.1`.

The documented path/fingerprint pair is the baseline for all focused queries in
this research environment; do not rehash it before every query. Revalidate the
size and XXH3-128 only if the path, file metadata, source package, or documented
edition changes, a fresh environment lacks the baseline, or there is a concrete
replacement concern. Findings from a different executable belong to a separate
edition record and address map.

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

For exact byte patterns that may be in the physical file tail rather than the
loaded MZ image, use `tools/ghidra/ReportPhysicalBytePattern.ps1` first. It
accepts a source path and one 1-to-64-byte hexadecimal pattern, bounds source
input to 128 MiB, and emits only the canonical path, file/pattern lengths,
match count, and file offsets. It does not print, retain, or create a copy of
source bytes. Follow a physical hit with `ReportBytePattern` in the applicable
loaded or mapped Ghidra view before assigning it a code owner.

## FBOV mapped image

Ghidra's MZ loader shows only the resident load image of `DSUN.EXE`; the code
of its 49 overlays sits in the `FBOV` pack after the image (FMT-EXE-001), and
resident code reaches it only through trampolines (FMT-EXE-004).
`tools/ghidra/New-FbovMappedImage.ps1` writes a local-only copy of `DSUN.EXE`
whose header loads the `FBOV` overlay code as ordinary segments, so Ghidra can
follow calls into it. It refuses an output path inside the repository or one
that already exists. It builds a new MZ relocation table from the original one,
rewrites each of the 854 trampolines as a far jump into its overlay's code with
a relocation for the jump's segment, replaces each overlay fixup word with the
segment its descriptor names and adds a relocation for it, and reports how many
fixup words had bit 0 set (0 for the GOG `DSUN.EXE`), since it does not
transform those. For the GOG `DSUN.EXE` the output is 668,768 bytes, and Ghidra
12.1.3 imports it with the MZ loader. Import it into its own disposable
project. A successful import does not validate decompiler output: indirect
calls and generic MZ-analysis warnings remain, so each query still records a
bounded result.

That copy is not a shipped file, and the spec never cites
its addresses. `tools/ghidra/ReportFbovOverlayMap.ps1` prints, for each overlay,
the segment of its resident header, the file offset and length of its code, and
the segment its code has in the mapped image. Given `-MappedAddress` values, it
converts each to the file offset of the same byte in `DSUN.EXE` and says whether
it lies in overlay code, in the resident load image, or elsewhere in the `FBOV`
pack. The mapped copy keeps every byte after the MZ header in place, so a
resident address such as `5000:A4B9` is the same in both images and is cited as
it is, while overlay code is cited as `DSUN.EXE+0x...` from the reporter's
`FileOffset`.

## Packed helper executables

`CHARTRAN.EXE` is compressed by LZEXE 0.91 (`LZ91` at offset `0x1C`), so its
strings and code are not visible in the shipped file. Unpack it first with
`dotnet run --project tools/DarkSunWakeRedux.Inspect -- unlzexe <packed.exe>
<output.exe>`, writing the output outside the repository, and import the
unpacked file into Ghidra. The command prints the size and XXH3-128 of both
files. A search of the packed file is not evidence of absence. `DSUN.EXE`,
`SOUND_DS.EXE`, `SVIEW.EXE` and `PATCH.EXE` carry no packer signature.

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
- immediate-port setup followed by bounded same-function `DX` I/O.
- explicit DOS `INT 21h` instructions with a literal `AH` setup found no more
  than twelve preceding instructions earlier in the same function.

Script output is temporary navigation evidence, not production input and not
proof by itself. Never redirect broad output into the repository.

## Evidence record

### EXE-GOG-UI-001 - APFM offset 88 is an event mask

- **Question:** Does the varying 16-bit `APFM` field at payload offset 88
  describe appearance, a resource, or dispatch behavior?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, 634,416 bytes, XXH3-128
  `e296af55ba2ecde7e77f555c90f33d0b`;
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
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, 634,416 bytes, XXH3-128
  `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Bounded finding:** the resource resolver's exact `WIND` branch at
  `39d1:04bc` searches a dedicated linked registry by identity at structure
  offset 8 and link at `0xee`. Complete bounded inspection of registration
  `3a8e:02b3`, redraw `3a8e:0003`, and activation `3a8e:060d` found no read of
  offset `0x3a`. Registration resolves each child by its tag and identity;
  redraw restores/clips registered rectangles; activation invokes an optional
  callback and then dispatches the resolved children.
- **Follow-up query:** ReportScalarConstants scanned every decoded instruction
  operand for unsigned scalar 19004 and found no match. This excludes only a
  directly encoded immediate resource-ID request; it does not exclude an
  argument, resident value, indirect table, constructed identifier, or
  app-specific path.
- **Interpretation:** the generic WIND path does not establish `BMP` #19004 as
  an automatic tiled, stretched, or full-window background. The field may be
  consumed by screen-specific code or may serve another role. Both remain open.
- **Corroboration:** all six start-flow WIND records carry #19004 although the
  observed start window instead composes `BMP` #20029, #20028, and four controls
  over black; #19004 itself is only 96x9.
- **Confidence:** high for absence from the inspected generic path and the
  bounded immediate-ID query; unknown for the field's actual presentation role.
- **Implementation consequence:** preserve the resource identity in DSUI and
  the extracted DSIX asset, but do not tile, stretch, or draw it until an
  app-specific consumer or controlled observation establishes the operation.

### EXE-GOG-UI-003 - Party surface delegates slot meaning to application logic

- **Question:** Does the single full-canvas control under `WIND` #19502 expose
  the character-slot hit regions or semantic actions used by the party screen?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, 634,416 bytes, XXH3-128
  `e296af55ba2ecde7e77f555c90f33d0b`;
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
  scan at `3a8e:1048` (`107a`). Follow-up direct-reference queries find no
  recovered caller of the generic registration entry `3a8e:02b3` or either
  generic callback setter. Those routines therefore cannot supply a bounded
  application registration path for this surface or for Preferences controls.
- **Interpretation:** the serialized graph establishes one application surface,
  not the internal party-slot rectangles or their actions. Those semantics are
  installed or dispatched indirectly at runtime and remain unknown; panel art
  alone is insufficient evidence for hit boundaries.
- **Confidence:** high for the serialized surface, generic callback lifecycle,
  absent direct callers of the three registration/setter entry points, and
  absence of the two direct scalar constants; unknown for the application
  callback and slot partition.
- **Implementation consequence:** validate the exact 319x199 exclusive surface
  from DSUI and keep it semantically inert until controlled observation or a
  bounded indirect-call finding establishes the slot partition and actions.

### EXE-GOG-UI-007 - EBOX tag paths do not establish native chrome rendering

- **Question:** Do the dialogue's image-less edit-box controls yield a bounded
  native rendering path that establishes missing corner or bevel treatment?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportBytePattern` located `EBOX`, `WIND`, `BUTN`, and `APFM`
  tag uses. Bounded contexts classified the three direct EBOX tag-request
  wrappers and the generic child dispatcher; `ReportReferences` enumerated
  their direct callers. A follow-up direct-reference query and bounded
  decompilations of `3d72:0009` and its sole recovered caller classified the
  immediate generic input path.
- **Bounded finding:** wrappers `409b:189c`, `4228:002b`, and `4228:00a9`
  each pass a caller-supplied identity and `EBOX` tag to the same tag-aware
  resolver used by the known WIND path, then test its returned pointer/result.
  Ghidra finds no direct callers of any of those wrappers. The generic child
  dispatcher `3d72:0eb8` distinguishes `APFM`, `BUTN`, and `EBOX` records and
  has only two direct callers, both inside `3d72:0009`. Its bounded context
  reads resident fields and event bits. `3d72:0009` has one recovered direct
  caller, `39d1:097d`, which reaches it for one decoded input-event class.
  The container supplies current pointer state to the child dispatcher twice,
  retains the resolved child/event result, and conditionally enters indirect
  handler tables under event-bit guards. This establishes resource-derived
  child hit/activation processing. Neither bounded routine directly identifies
  an image draw, fill, bevel, corner, clipping, or palette operation.
- **Interpretation:** the observed EBOX controls participate in a native
  tag-aware resource and child-dispatch framework, but this query does not
  establish an EBOX visual primitive or connect any wrapper to dialogue
  rendering. It therefore cannot justify synthesizing missing dialogue chrome
  from image-less control geometry.
- **Confidence:** high for the explicit tag requests, all-read direct-caller
  results, sole recovered generic caller, repeated child resolution, and
  event-bit-guarded indirect dispatch; unknown for specific handler targets,
  callback registration, control ownership, and native visual treatment.
- **Implementation consequence:** keep the captured panel artwork as the only
  dialogue chrome layer and retain image-less controls as geometry/event
  contracts. A renderer change requires a measured capture or a separately
  traceable drawing path.

### EXE-GOG-PREF-001 - No direct literal `PREF` resource route

- **Question:** Does the sole `PREF` resource-family tag yield a direct
  executable path that can define Preferences settings behavior?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportBytePattern` searched all loaded executable blocks for the
  exact four-byte ASCII encoding `PREF`. Its single result at `5000:8c85` in
  `CODE_208` was then passed to `ReportReferences`.
- **Bounded finding:** the raw literal occurs once and has no direct Ghidra
  reference. The query supplies no tag lookup, resource manager caller,
  control binding, stored field, default, range, increment, or audio/rules
  consumer.
- **Interpretation:** this excludes only a direct reference to that literal in
  this executable. It does not show that the resource is unused, nor rule out
  a constructed tag, indirect lookup, a different module, or runtime-propagated
  state.
- **Confidence:** high for the one raw occurrence and absent direct-reference
  result; unknown for loader ownership, resource layout, and every
  player-visible Preferences behavior.
- **Implementation consequence:** retain the `PREF` payload as DSOP and keep
  Preferences mutations inert. Do not infer a settings schema or a behavior
  from this negative lead.

### EXE-GOG-CHAR-001 - No adjacent default-party identity table established

- **Question:** Does the supported executable contain either disc character
  block #40-#43 or #50-#53 as an adjacent default-party lookup table?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with XXH3-128
  `e296af55ba2ecde7e77f555c90f33d0b`;
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
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with XXH3-128
  `e296af55ba2ecde7e77f555c90f33d0b`;
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
  `FND-ACTOR-002`'s exact `(1184,1459)` top-left, this establishes opening
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

### EXE-GOG-UI-005 - Hostile Look panel IDs do not identify an activation path

- **Question:** Do the observed hostile Look window, application frame, or
  button IDs occur as executable scalar constants that identify the code which
  opens or renders the panel?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
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

### EXE-GOG-UI-008 - Observed hostile label has no exact executable literal

- **Question:** Does the supported executable contain the exact observed
  hostile Look-panel label `Draxan`, either as a null-terminated or an
  unterminated ASCII byte sequence, giving the dynamic text field an
  executable-side source lead?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** after the label was independently measured in
  `DATA-GOG-INTERACTION-001`, `ReportBytePattern` searched every loaded memory
  block first for `44 72 61 78 61 6e 00` and then for its exact six-byte prefix
  `44 72 61 78 61 6e`. Each query has bounded occurrence and reference output.
- **Bounded finding:** neither exact byte pattern occurs in loaded executable
  memory. The result identifies no literal text, source-data mapping, target
  name, level, hostility, panel activation, or rendering path.
- **Physical-file follow-up:** the MZ header reports a 20,992-byte header and
  357,744-byte load image in the 634,416-byte approved file, leaving a
  276,672-byte physical overlay outside Ghidra's loaded program-memory image.
  A bounded PowerShell 5.1.26100.9444 scan of the complete physical file found
  zero occurrences of both the six-byte `Draxan` prefix and its seven-byte
  NUL-terminated form. It retained only the computed MZ sizes and match counts,
  not source bytes. This closes the exact-ASCII overlay variant of this query.
  A NUL-terminated search in the [FBOV mapped image](#fbov-mapped-image)
  agrees: no match.
- **Interpretation:** this excludes only the observed-case ASCII spelling in
  this complete physical executable. It may be encoded with another case or
  character set, held in a GFF/resource or a separate module, assembled at
  runtime, or supplied by an unobserved data path. It does not turn the
  captured name into a safe production literal.
- **Confidence:** high for the two exact absent loaded-memory and complete-file
  representations; unknown for the dynamic text source and all general target
  presentation.
- **Implementation consequence:** keep the hostile panel's dynamic name and
  level unrendered rather than hard-coding captured original text. A source
  projection needs a bounded data-format or native call-path finding.

### EXE-GOG-COMBAT-008 - Mapped overlay reaches the observed status-panel initializer

- **Question:** Does the recovered FBOV view expose an executable path from a
  bounded dispatcher to the source-backed combat status-panel bitmap, and does
  that path establish any panel or combat rules?
- **Target/method:** Use the [FBOV mapped image](#fbov-mapped-image).
  `ReportScalarConstants` searched every decoded operand for panel ID 19003;
  `ReportInstructionContext`, `ReportReferences`, and 160-line bounded
  decompilations then followed its containing function, its two direct callers,
  and one direct caller layer above the only reachable caller. Mapped-image
  addresses below are reproducible transform addresses, not claimed original
  load addresses.
- **Bounded finding:** the sole decoded 19003 operand is in mapped function
  `7393:0560`; its guarded image request has the `BMP ` tag and shares an
  adjacent request for #19000. It has exactly two direct callers. One,
  `74bb:0498`, first checks one shared state word for value two, iterates
  exactly four records at a 49-byte stride, conditionally changes one byte at
  offset `0x14` in each qualifying record, and invokes `7393:0560`. It then
  continues only while the same state word is two or three. `74bb:0498` has
  one direct caller: mapped dispatcher `76da:007d` reaches it from its
  parameter-one-equals-two branch when its second parameter is `0x7fc`, after
  two preparatory calls and one helper whose result is compared with one, two,
  and three. The second panel caller has no recovered direct caller. All stated
  field offsets and values are data-access facts only. A follow-up 47-line
  decompilation shows that this routine calls `4758:01d5` immediately after
  the panel request with an unresolved `0x92e0` operand beside a recovered
  `RESOURCE.GFF` literal. Aggregate inventory confirms no `RESOURCE.GFF`
  resource number 37600 exists. `4758:01d5` has exactly two direct callers
  (this routine and `74bb:0077`). The latter is the separate shared
  RNG/redraw route recorded by `FND-RNG-008`; it directly calls
  `7393:0560` but has no recovered direct caller. `4758:01d5` contains a
  `stdpatch` literal plus generic initialization/state handling, not a decoded
  resource lookup. Thus neither the operand nor the literal identifies a
  resource request.
- **Interpretation:** this is a concrete native code connection from a
  dispatcher branch to static artwork observed only in combat captures. The
  follow-up makes the four-record operation less, not more, attributable to
  combat: it may be a shared setup or patch-related operation. It does not
  assign the shared state values to combat, establish that the four records are
  party members or active combatants, identify their fields, decode the event
  IDs, establish panel text/value meanings, or identify entry, turn,
  targeting, movement, attack, damage, outcome, or timing behavior.
- **Confidence:** high for the mapped scalar, direct-call counts, exact state
  comparisons, four-iteration/49-byte-stride operation, and immediate call
  order; unknown for every semantic interpretation.
- **Implementation consequence:** retain the status bitmap as evidence-only.
  Do not use the four-record operation, `RESOURCE.GFF` literal, or unresolved
  operand as a status model, resource contract, or playable combat lead.

### EXE-GOG-UI-006 - Character-generation IDs do not identify control behavior

- **Question:** Do the character-generation window, class-label, EXIT, or DONE
  resource identities occur as immediate executable operands that identify their
  application-specific handlers?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
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
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
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

### EXE-GOG-TIMING-002 - BIOS extended-memory services are not wait loops

- **Question:** Do any literal `INT 15h` instructions in the supported
  executable select the BIOS wait service that could provide a native delay
  boundary for movement, animation, or media playback?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportBytePattern` searched all loaded blocks for the exact
  two-byte `INT 15h` encoding (`cd 15`) and found four matches. Bounded
  `ReportInstructionContext` windows inspected each match's service selector;
  no decompilation or runtime execution was used.
- **Bounded finding:** the four raw matches occur at loaded addresses
  `1000:5fa3`, `1000:62db`, `4000:be0e`, and `4000:c098`. Their bounded
  instruction contexts resolve to `15f3:0073`, `15f3:03ab`, `4ae5:0fbe`, and
  `4ae5:1248`, respectively. The first and last select `AH=87h`; the second
  and third select `AH=88h`. None selects `AH=86h`, the BIOS wait service.
- **Interpretation:** this establishes only that every literal `INT 15h`
  site in the analyzed loaded image is an extended-memory service boundary,
  not a BIOS wait call. It does not exclude a timing mechanism using another
  interrupt, port, busy loop, driver, overlay, or runtime-generated code, and
  it assigns no movement, animation, or media cadence.
- **Confidence:** high for the complete four-match raw opcode result and the
  four immediately preceding service selectors; unknown for all native timing
  policies outside those bounded paths.
- **Implementation consequence:** do not recreate a DOS BIOS wait loop or
  derive a delay from these sites. Retain the explicit monotonic,
  CPU-speed-independent runtime clock until a movement/media consumer path and
  controlled cadence observation establish a native contract.

### EXE-GOG-TIMING-003 - VGA-status busy wait gates a generic copy path

- **Question:** Does direct polling of the VGA input-status port establish a
  native presentation-synchronization or gameplay-timing rule?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportScalarConstants` searched decoded instruction operands
  for `0x3da`, then `ReportInstructionContext` inspected both candidates.
  `ReportReferences` and short caller contexts bounded the direct callers of
  the one port-I/O helper. No runtime observation or decompilation was used.
- **Bounded finding:** one scalar occurrence at `1000:0f01` loads `DX=03dah`.
  When two segment values differ, `1000:0ecc` disables interrupts, repeatedly
  reads `IN AL,DX`, rotates the low status bit into carry, waits for one carry
  state and then its opposite, moves one word, reenables interrupts, and loops.
  The other scalar occurrence, `2707:03d5`, is an indexed far-jump-table
  displacement rather than a port load. The busy-loop helper has exactly two
  direct callers: `1000:0fb5`, after a separate local preparation and before
  calls to the shared BIOS-video wrapper, and `1000:265c`, which chooses it or
  a separate copy helper according to a resident word.
- **Interpretation:** the supported binary contains a CPU-busy display-status
  transition poll surrounding a generic word-copy path. It is evidence of a
  presentation synchronization technique, not of a duration, frame rate,
  animation cadence, actor update, input cadence, or resource-specific draw
  order. The service path cannot identify which player-visible screen, if any,
  reaches either caller.
- **Confidence:** high for the two scalar contexts, the port-I/O loop, and its
  two direct callers; unknown for status-bit meaning, display ownership, call
  frequency, visual effect, and every gameplay timing policy.
- **Implementation consequence:** do not reproduce the interrupt-disabled
  busy wait or couple Core advancement to display status. Presentation may use
  a modern non-blocking renderer; Core remains monotonic and
  CPU-speed-independent until a measured native behavior establishes a
  semantic cadence.

### EXE-GOG-TIMING-004 - Direct PIT access has no recovered feature owner

- **Question:** Do direct immediate or immediate-`DX` accesses to the standard
  PIT control/data ports establish a movement, animation, or media cadence that
  the restoration must reproduce?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1. The full analyzer
  pass completed before these focused queries.
- **Method:** `ReportBytePattern` searched every loaded block for the four
  immediate-port encodings `e4 40`, `e6 40`, `e4 43`, and `e6 43`.
  `ReportInstructionContext` classified each coherent instruction sequence,
  while `ReportReferences` enumerated direct callers of both containing
  functions and of the local read-loop wrapper. `ReportImmediatePortIo` then
  searched every recovered function for `MOV DX,40h` or `MOV DX,43h` followed
  within sixteen instructions by `IN` or `OUT` through `DX`.
- **Bounded finding:** the four raw searches find two coherent `IN 40h`, two
  coherent `OUT 40h`, no `IN 43h`, and two coherent `OUT 43h` instructions;
  three other short byte matches are inside unrelated decoded instructions.
  The bounded immediate-`DX` query finds no matching sequence. Function
  `1000:12bf` saves flags, disables interrupts, writes zero to port
  `43h`, performs two local calls separated by reads from `40h`, inverts the
  combined two bytes, restores flags, and returns. It has exactly two direct
  callers, both inside `1000:12fa`; that wrapper has no Ghidra-recorded direct
  caller. Separately, the previously recorded source-mapping label
  `4842:059c` saves flags, disables interrupts, writes literal `0x36` to
  `43h`, obtains a caller-supplied word, and emits its two bytes to `40h`.
  A follow-up `ReportReferences` query resolves its mapped call target as
  `4000:89bc`, with exactly three direct callers at `4868:061d`, `4868:0384`,
  and `4926:0095`. Their bounded contexts respectively pass zero after a
  local decrement, scale a stack word by observed literals `0x2710` and
  `0x20bc`, and pass the result of an opaque local helper before another
  opaque call. The enclosing first caller has four direct Ghidra callers, the
  scaling caller has one, and the third caller has one; none supplies a Tyr
  coordinate, actor, image, FLI/VOC resource, UI control, or dialogue
  identity. The differing segment labels are an overlay-address presentation
  issue, not evidence that any of the routines belongs to the named segment
  or a gameplay subsystem.
- **Interpretation:** the executable contains two real direct PIT-access
  boundaries, including one latch/read sequence and one caller-word programming
  sequence. Their direct caller shapes do not establish a feature owner,
  duration unit, delay contract, playback clock, frame cadence, or Core update
  schedule. The negative `DX` result excludes only immediate setup followed by
  nearby same-function I/O; computed, indirect, loaded, or runtime-generated
  port access remains possible. A no-reference result for the local wrapper
  likewise excludes only a direct Ghidra caller.
- **Confidence:** high for the raw-match counts, decoded port accesses, local
  operation order, direct-reference counts, and bounded call contexts;
  unknown for hardware purpose, caller ownership, duration units, and every
  player-visible timing behavior.
- **Implementation consequence:** do not program or poll PIT hardware, infer a
  duration from the observed literals, or couple rules to a DOS-era timer
  routine. Retain the explicit monotonic, CPU-independent runtime clock until a
  resource/feature-specific path and controlled cadence observation establish a
  player-visible contract.

### EXE-GOG-VIDEO-001 - Shared BIOS-video wrapper is not a screen contract

- **Question:** Do literal BIOS video calls establish a screen-specific native
  display mode, palette, renderer, or layout boundary for the restoration?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportBytePattern` searched all loaded blocks for the exact
  `INT 10h` encoding (`cd 10`) and found eleven matches. Bounded instruction
  contexts inspected the nine decoded sites in their shared recovered function;
  `ReportReferences` enumerated its direct callers, and three representative
  caller contexts established that the helper receives both fixed and
  caller-supplied service/register values.
- **Bounded finding:** nine matches at `1000:1149` through `1000:11a2` are in
  `1000:1136`. Its entry dispatches on the caller's `AH` value and issues
  `INT 10h`; nearby branches make additional service calls only for selected
  return values. `ReportReferences` finds fourteen direct calls from seven
  recovered functions. Representative call contexts pass service values `02h`
  and `09h`, an all-caller-supplied register set, and `0fh`/`00h` in one
  mode-query/set sequence. The remaining raw matches at `1000:e907` and
  `1000:e989` lie outside a containing decoded instruction or function in this
  analysis.
- **Interpretation:** the bounded evidence establishes a shared BIOS-video
  handoff with generic adapter/mode handling. It does not associate a call with
  a title, dialogue, Preferences, region, resource, palette, framebuffer
  layout, logical resolution, or screen transition. In particular, a service
  value passed through the wrapper is not evidence of a specific player-visible
  screen mode.
- **Confidence:** high for the eleven raw matches, the nine-site shared helper,
  its fourteen direct-call references, and the three bounded caller shapes;
  unknown for all native screen ownership, display-mode policy, palette use,
  and rendering behavior.
- **Implementation consequence:** retain the independently measured 320x200
  logical canvas and resource-backed palette contracts. Do not emulate BIOS
  adapter probing, infer a screen mode, or add a rendering/timing policy from
  this generic wrapper; a screen-specific consumer path plus controlled visual
  observation is required.

### EXE-GOG-MOUSE-001 - Native mouse wrappers do not establish UI semantics

- **Question:** Do literal mouse-service calls establish a native coordinate
  transform, hit-testing policy, or screen-specific control contract?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportBytePattern` searched all loaded blocks for the exact
  `INT 33h` encoding (`cd 33`). Bounded instruction contexts inspected the
  nine decoded sites in the recovered mouse-wrapper area at `45b9`; two raw
  references to its returned-coordinate wrapper were followed with
  `ReportReferences` and their short caller contexts were inspected. Direct
  references to the two range-setting wrappers were also queried.
- **Bounded finding:** the decoded wrappers select service values `00h`,
  `03h`, `05h`, `06h`, `07h`, `08h`, `04h`, `09h`, and `0ch`; their arguments
  and returned registers are passed through caller-owned storage. The
  returned-coordinate wrapper at `45b9:0034` has exactly two direct callers:
  `28c9:23a0` and `39d1:068d`. The first rejects a returned coordinate pair
  when either value is zero, when the first is at least 318, or when the second
  is at least 199, accepting only the interior pair ranges `1..317` and
  `1..198`. The second computes two caller-local values, invokes the separate
  position-setting wrapper, queries the coordinates, and copies them into
  resident words. The `45b9:00bb` and `45b9:00d3` range-setting wrappers have
  no Ghidra-recorded direct references. Other raw `cd 33` matches were not
  treated as code without a containing decoded instruction.
- **Interpretation:** the executable has a generic mouse-service boundary and
  one bounded caller-specific interior guard. It does not establish which
  coordinate represents an application X/Y axis, a global cursor range, a
  resource-derived control rectangle, a click/drag gesture, button semantics,
  pointer hotspot, or the owner of either caller. In particular, the guard is
  not sufficient to replace the measured 320x200 layout or the existing
  resource-derived hit contracts.
- **Confidence:** high for the decoded service values, wrapper/caller counts,
  and the two numerical guard bounds; unknown for mouse scaling, coordinate
  ownership, screen routing, gesture semantics, and all general UI behavior.
- **Implementation consequence:** retain logical-to-physical coordinate
  transforms and semantic DSUI hit testing as explicit, independently tested
  contracts. Do not emulate INT 33h calls, adopt an unproven interior margin,
  or change click/drag behavior from this generic boundary; controlled input
  traces and a screen-specific consumer path are required.

### EXE-GOG-KEYBOARD-001 - BIOS modifier query does not map player keys

- **Question:** Does the supported executable's literal BIOS keyboard path
  establish a player key, modifier, repeat, or control-routing contract?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportBytePattern` searched all loaded blocks for the exact
  `INT 16h` encoding (`cd 16`), then `ReportInstructionContext` inspected the
  one decoded site. `ReportReferences` enumerated the direct callers of its
  wrapper and bounded contexts inspected both callers. Other raw byte matches
  were not treated as code without a containing decoded instruction.
- **Bounded finding:** four raw matches exist, but only `4000:4b73` is decoded
  as an instruction, resolving to `44b6:0013`. Its wrapper sets `AH=02h`,
  executes `INT 16h`, clears `AH`, and returns. It has two direct callers: an
  internal helper at `44b6:0063`, which stores the returned low byte in a
  resident byte, and `31e0:2e3b`. The latter reaches the call after locating a
  37-byte-stride resident record and checking its leading-byte bit `0x10`; it
  then masks the returned value with `0x03` before a guard. No decoded site
  reads a key code or directly dispatches a player action.
- **Interpretation:** this proves one native modifier-status boundary and a
  guarded two-bit use in an opaque record path. It does not establish the
  record's ownership, the meanings of either bit, any specific modifier,
  keystroke, hotkey, key-repeat rule, focus rule, or screen transition. The
  manual and controlled input observations remain separate evidence sources.
- **Confidence:** high for the four raw matches, the one decoded wrapper, two
  direct callers, and its bounded masks/record stride; unknown for all
  player-visible keyboard behavior.
- **Implementation consequence:** retain semantic, edge-triggered modern
  keyboard input behind the documented manual/observation contracts. Do not
  derive a native modifier map, repeat policy, or control action from this
  opaque two-bit guard; a screen-specific consumer path and controlled trace
  are required.

### EXE-GOG-KEYBOARD-002 - No direct keyboard-controller port lead

- **Question:** Does the supported executable directly read the conventional
  keyboard-controller data port, or set `DX` to conventional keyboard-controller
  ports before `IN`/`OUT`, providing a focused combat-hotkey dispatch lead?
- **Target:** the documented stable BLD-GOG-EN-1.1 `DSUN.EXE` target,
  634,416 bytes, XXH3-128
  `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3 and JDK 21.0.12.1. The documented path, package, size, and
  baseline metadata remain unchanged, so this focused query reuses the
  approved target without rehashing.
- **Method:** `ReportBytePattern` searched all loaded program blocks for the
  exact immediate-port input encoding `e4 60` (`IN AL,60h`).
  `ReportImmediatePortIo` separately scanned recovered functions for
  `MOV DX,60h` or `MOV DX,64h` followed within 16 instructions by a DX-addressed
  `IN` or `OUT` operation.
- **Bounded finding:** neither query produced a match.
- **Interpretation:** this excludes only the exact immediate byte-read and the
  bounded immediate-DX controller-port forms. Keyboard input may arrive through
  BIOS/DOS services, an installed driver, a dynamically supplied port, another
  executable, or an indirect/native handler. It does not identify or exclude
  any player key, hotkey dispatch, repeat policy, combat command, or turn
  behavior.
- **Confidence:** high for the two queried raw/recovered instruction forms;
  unknown for every other keyboard acquisition and dispatch path.
- **Implementation consequence:** retain the manual-evidenced combat bindings
  as unconnected semantic input requests. Do not emulate controller I/O or
  infer combat hotkey behavior from this negative probe.

### EXE-GOG-KEYBOARD-003 - Manual combat keys do not co-locate in one decoded function

- **Question:** Does the mapped FBOV view contain one decoded keyboard command
  function with every manual combat key—Space and upper-case `G`, `N`, `P`,
  `Q`, and `W`—as immediate operands?
- **Target/method:** Use
  the [FBOV mapped image](#fbov-mapped-image). `ReportFunctionScalarIntersection` scanned every
  decoded function for the six unsigned values 32, 71, 78, 80, 81, and 87,
  then separately for the five upper-case letter values 71, 78, 80, 81, and
  87 and their lower-case forms 103, 110, 112, 113, and 119. The queries are
  co-location probes only; they do not search strings, key maps, tables,
  scan-code representations, or indirect values.
- **Bounded finding:** no decoded function contains all six requested scalar
  values, the five upper-case letter values, or the five lower-case letter
  values.
- **Interpretation:** this excludes only a simple direct implementation that
  compares all documented combat bindings in one decoded function. It does not
  identify a key-acquisition path, show that any command is absent, or exclude
  dispatch spread across helpers, tables, scans, indirect values, or another
  executable.
- **Confidence:** high for the six-value decoded scalar-intersection result;
  unknown for every native keyboard and combat-command behavior.
- **Implementation consequence:** retain manual bindings as unconnected
  semantic requests. Do not create a command dispatcher or repeat policy from
  this negative co-location result.

### EXE-GOG-KEYBOARD-004 - Mapped BIOS keyboard wrapper remains feature-neutral

- **Question:** Does the mapped FBOV image expose an overlay-side BIOS keyboard
  reader with a traceable combat-command consumer?
- **Target/method:** `ReportBytePattern` searched the local-only mapped image
  for `INT 16h` (`cd 16`). `ReportReferences`, bounded decompilations, and
  16-instruction caller contexts inspected its sole decoded wrapper and three
  direct callers, then one dispatch-shaped caller layer above.
- **Bounded finding:** five raw patterns occur; only `44b6:0011` is a decoded
  `INT 16h` wrapper. It has three direct callers. None supplies an explicit
  `AH` service value in the 16-instruction context before the call; the two
  recoverable callers test opaque return bits or cache one returned byte. The
  only dispatch-shaped upstream route is `7c0e:0379` case four, which requests
  `GPLI` #1 and updates selector tables; it establishes no keyboard command.
- **Interpretation:** the mapped image adds a generic BIOS-input boundary, not
  a combat key-acquisition or command route. The raw matches and unreliable
  caller decompilations do not establish which BIOS service runs, keyboard
  repeat behavior, key values, command mapping, or any player-visible effect.
- **Confidence:** high for the raw/decoded count, direct-call count, absent
  immediate `AH` setup in the bounded contexts, and the GPLI branch facts;
  unknown for every keyboard and combat behavior.
- **Implementation consequence:** retain manual commands as unconnected input
  requests. Do not treat this wrapper, its callers, or the GPLI branch as a
  combat dispatcher.

### EXE-GOG-MEDIA-001 - no literal FLI header validation lead in DSUN.EXE

- **Question:** Does the supported executable contain the literal FLI header
  magic needed to identify an in-process cinematic decoder or its timing path?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
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

### EXE-GOG-MEDIA-002 - Embedded FLI names do not establish cinematic order

- **Question:** Do the five fingerprinted numbered FLI files have a direct
  executable filename table that establishes their playback order or a native
  cinematic-loading call path?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1. A full analyzer
  pass completed before this focused query.
- **Method:** `ReportBytePattern` searched all loaded blocks for the exact
  NUL-terminated ASCII names of the five source-manifest FLI files. The two
  resulting bounded data neighborhoods were inspected with `ReportDataBytes`,
  and `ReportReferences` checked each block and every individual name entry.
- **Bounded finding:** the executable contains two raw `1.FLI` occurrences and
  one raw root-level occurrence each of `2.FLI` through `5.FLI`. One block also
  contains four drive-prefixed `CINE`-directory path templates for numbered
  files 2 through 5, while the other contains a standalone `1.FLI` name and
  otherwise zero-filled bytes within the inspected range. Ghidra reports no
  direct reference to either data block or to any of the seven individual
  filename/path entries. The query did not find a decoded call, comparison,
  filename construction, file-open operation, sequence table, or timing value.
- **Interpretation:** the filenames are present in the fingerprinted executable,
  but this bounded static result cannot identify which names are live, whether
  the CINE templates are fallback paths, how paths are constructed, the order
  of playback, a decoder, input-skipping behavior, or a transition to any game
  screen. Missing direct references do not prove the strings are unused; they
  may be accessed indirectly, copied, or reached through another module.
- **Confidence:** high for the enumerated raw filename/path occurrences and
  absence of direct Ghidra references at the inspected addresses; unknown for
  every loader, sequence, timing, presentation, and gameplay relationship.
- **Implementation consequence:** retain every FLI as a complete opaque source
  asset and keep all cinematic sequencing and clock behavior unimplemented.
  A decoder or playback implementation requires a bounded consumer path and a
  controlled native observation; raw header speed and filename order remain
  data, not a scheduling contract.

### EXE-GOG-COMBAT-001 - Combat hotkey dispatch is not a direct shared literal table

- **Question:** Does the supported executable contain an obvious single function
  or compact literal table that directly dispatches all six manual combat keys?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
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
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
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

- **Question:** Do the executable's literal `COMBAT`, `GUARD`, or `ATTACK`
  text occurrences identify a code reference that can be used as a focused
  lead for combat input or action resolution?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportBytePattern` searched all loaded blocks for the explicit
  ASCII byte encodings of `COMBAT` and `GUARD`, reporting no more than 100
  matches and 20 inbound references per match. The single inbound `GUARD`
  reference was then bounded with `ReportInstructionContext` to eight
  instructions on each side. A follow-up raw-byte query searched every loaded
  block for the exact null-terminated `ATTACK` encoding (`41 54 54 41 43 4b
  00`).
- **Bounded finding:** `COMBAT` has four raw matches in `CODE_208` at
  `5000:9b4f`, `5000:9bb3`, `5000:9cd1`, and `5000:a6ec`; none has an inbound
  Ghidra reference. `GUARD` has two raw matches in the same block at
  `5000:8e52` and `5000:9cae`; only the first has one inbound READ reference,
  from `1bf3:66eb` in `28c9:19a1 FUN_28c9_19a1`. Its sixteen-instruction
  context compares `AL` with `0x20`, loads an indirect far pointer, adjusts a
  word through that pointer, and loops; it contains no documented combat
  scancode, direct combat-state reference, or action-resolution call. The
  exact null-terminated `ATTACK` query has no raw matches, and therefore no
  executable address or reference to inspect.
- **Interpretation:** the queried literal labels are not a reliable combat
  command path. The sole direct reference is compatible with generic
  label/data processing, but the bounded context cannot establish its owner.
  The absent `ATTACK` encoding excludes only this exact null-terminated
  executable literal; it does not exclude resource-driven, constructed,
  relocated, or runtime text. Neither result establishes any input, target,
  turn, Guard, attack, or rendering behavior.
- **Confidence:** high for the capped raw-match/reference results, exact
  `ATTACK` no-match result, and documented instruction context; unknown for
  label ownership and all combat semantics.
- **Implementation consequence:** retain the manual-evidenced `CombatHotkeys`
  adapter only. Do not connect its commands, infer a UI graph, or name a combat
  routine from these text literals; seek a multi-signal data/call-path lead or
  controlled observation first.

### EXE-GOG-COMBAT-004 - Observed status panel has a bounded native cache path

- **Question:** Does the static panel matched to the owner-confirmed combat
  captures have a direct executable resource request, and does that request
  identify combat behavior?
- **Target:** BLD-GOG-EN-1.1 DSUN.EXE, reverified at 634,416 bytes with
  XXH3-128 e296af55ba2ecde7e77f555c90f33d0b;
  Ghidra 12.1.3, JDK 21.0.12.1, and the previously completed default
  auto-analysis.
- **Method:** After the source-panel template match identified BMP #19003,
  ReportScalarConstants scanned all decoded instruction operands for decimal
  19003. ReportInstructionContext and a bounded 55-line containing-function
  decompilation classified the sole result. ReportReferences then followed the
  function and its one direct caller by address only.
- **Bounded finding:** one decoded instruction at 2c5f:041e supplies #19003
  together with the BMP tag to the common native image request entry, guarded
  by resident state and a zero/nonzero image-cache field. One success path
  reads an opaque indexed resident family with a 49-byte stride before issuing
  further generic calls. That loader function has one direct caller at
  2c5f:0705; its immediate caller has eight direct recovered callers across
  four code segments. No caller supplies a recovered encounter, actor,
  command, damage, turn, or timing identity.
- **Interpretation:** this establishes a real native request/cache path for
  the panel asset, corroborating its visual match. The source does not attach
  the panel to one combat screen, assign the overlay, name the 49-byte record
  fields, or identify an update rule. The generic fan-in remains compatible
  with indirect or shared UI use.
- **Confidence:** high for the one scalar operand, its direct BMP request,
  resident cache guard, bounded stride observation, and recovered caller
  counts; unknown for feature ownership, text/value rendering, and every
  combat rule.
- **Implementation consequence:** preserve BMP #19003 as an observed
  source-backed panel candidate, but do not create a combat renderer, panel
  value model, record reader, or update loop from this shared native path.

### EXE-GOG-COMBAT-005 - Panel caption has no direct executable literal path

- **Question:** Does the visible `Moves` caption in the owner-confirmed
  Thy'rokh combat panel occur as a null-terminated ASCII literal in the
  approved executable, with a direct reference that can identify its renderer
  or update path?
- **Target:** the already documented stable BLD-GOG-EN-1.1 `DSUN.EXE` target,
  634,416 bytes, XXH3-128
  `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1. Its path, package,
  size, and baseline metadata remain unchanged, so this focused query reuses
  the approved target without rehashing.
- **Method:** `ReportBytePattern` searched every loaded memory block for the
  explicit null-terminated ASCII sequence `4d 6f 76 65 73 00`. Its normal
  bounded output includes references to each raw match; only matched addresses
  would proceed to reference and instruction-context inspection.
- **Bounded finding:** no raw byte-pattern match exists, so there is no matched
  executable address or direct reference to inspect.
- **Mapped-image follow-up:** the same NUL-terminated search in the
  [FBOV mapped image](#fbov-mapped-image) also finds no match, so the literal
  is absent from the overlay code as well. Four NUL-terminated strings ending
  in `COMBAT` sit in the resident image at file offsets `0x4ED4F`, `0x4EDB3`,
  `0x4EED1` and `0x4F8EC`; `ReportReferences` in the mapped image finds no
  direct reference to any of them, so they identify no combat dispatcher.
- **Interpretation:** this excludes only the exact null-terminated ASCII
  literal in this executable. It does not show that the observed caption is
  absent at runtime or identify it as a field: it may use another encoding,
  split or constructed characters, relocated data, a resource, or a different
  executable/runtime path. `DATA-GOG-COMBAT-001` separately excludes only an
  exact printable-ASCII GFF source match and the known static panel bitmap.
- **Confidence:** high for the exact raw-byte negative query; unknown for text
  construction, drawing, update ownership, panel values, turns, and combat
  behavior.
- **Implementation consequence:** do not use the caption as a combat-renderer
  or game-rule lead. Keep the observed panel's dynamic region and text/value
  source opaque pending an independent native path or controlled observation.

### EXE-GOG-COMBAT-006 - Panel loader's immediate fan-in remains feature-neutral

- **Question:** Do the direct recovered callers of the native boundary that
  immediately contains the BMP #19003 panel request identify a combat-specific
  owner or pass a recoverable encounter, actor, command, damage, turn, or
  timing identity?
- **Target:** the documented stable BLD-GOG-EN-1.1 `DSUN.EXE` target,
  634,416 bytes, XXH3-128
  `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1. The approved path,
  package, size, and baseline metadata remain unchanged, so this focused query
  reuses the target without rehashing.
- **Method:** `ReportInstructionContext` first classified the immediate panel
  caller's call at `2c5f:0705` as lying in recovered function `2c5f:06cb`.
  `ReportReferences` then enumerated its eight direct recovered calls:
  `28c9:33ce`, `28c9:342e`, `28c9:348d`, `2c5f:0175`, `2c5f:031e`,
  `31e0:13e2`, `31e0:1759`, and `362c:0c08`. One bounded eight-instruction
  context was inspected at each call site; no decompilation window or broader
  call-graph traversal was retained.
- **Bounded finding:** the three `28c9` calls occur in one recovered function;
  the remaining five calls occur in five other recovered functions across
  three code segments. Each prepares only `0` or `1` immediate arguments at
  the visible call boundary: six sites use `(1,1)`, while `2c5f:0175` and
  `362c:0c08` use `(1,0)`. The contexts contain no recovered panel resource
  identity, encounter, actor, command, damage amount, turn identifier, clock,
  or other combat-specific scalar. This is the eight-call fan-in previously
  bounded by EXE-GOG-COMBAT-004, now classified at the direct-call boundary.
- **Interpretation:** the two small arguments may be generic mode or success
  flags, but their role is not established. The shared fan-in remains
  compatible with indirect feature ownership and does not make BMP #19003 a
  combat-only UI route. It also does not exclude a combat caller that reaches
  this helper indirectly.
- **Confidence:** high for the recovered function/call-site list, code-segment
  distribution, and visible `0`/`1` argument pairs; unknown for argument
  meaning, feature ownership, text/value rendering, and all combat behavior.
- **Implementation consequence:** preserve the panel only as source-backed
  artwork. Do not add a combat presenter, panel model, loader simulation, or
  gameplay rule from this shared helper boundary; the controlled C0-C6 capture
  gate and an independently traceable behavior path remain required.

### EXE-GOG-COMBAT-007 - Mouse-coordinate dispatch remains screen-neutral

- **Question:** Does the native coordinate-query caller with the established
  320x200 interior guard lead directly to a combat-specific click, target, or
  attack handler?
- **Target:** the documented stable BLD-GOG-EN-1.1 `DSUN.EXE` target,
  634,416 bytes, XXH3-128
  `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, JDK 21.0.12.1. The documented path, package, size, and
  baseline metadata remain unchanged, so this focused query reuses the
  approved target without rehashing.
- **Method:** `ReportInstructionContext` inspected the existing coordinate
  call at `28c9:23a0`. `ReportReferences` then queried the containing function
  `28c9:2322`, and one complete 91-line `ReportDecompileWindow` classified
  only that immediate function. A fresh, local-only Ghidra 12.1.3 import then
  revalidated the direct call at `277b:056f` in `277b:0024`, inspected its
  sixteen-instruction context, and followed only the direct reference to that
  containing routine. No indirect handler target or data structure was
  decompiled.
- **Bounded finding:** the coordinate call belongs to `28c9:2322`, which has
  one recovered direct caller at `277b:056f`. The function obtains two local
  coordinate words, rejects zero and values outside its previously recorded
  `1..317`/`1..198` interior ranges, and branches through several resident
  state guards. Its remaining branches call opaque helpers, including a
  three-argument helper reached only under one state value, and retain no
  recovered encounter, actor, target, action, damage, turn, clock, or combat
  resource identity. The revalidated call occurs among generic calls and byte
  state guards in `277b:0024`; its sole direct reference is the program entry
  at `1000:0158`. The source-level entry-to-caller-to-coordinate-function
  relation is real, but all intermediate helper roles remain unrecovered.
- **Interpretation:** this extends the generic input boundary to an
  entry-rooted caller chain, but does not connect a canvas click to combat. In
  particular, it cannot distinguish direct enemy selection, target cycling,
  approach movement, attack resolution, or a confirmation step from other
  input paths.
- **Confidence:** high for the one direct recovered caller, local coordinate
  guard, and bounded branch/call observations; unknown for every state value,
  helper role, screen owner, click action, and combat behavior.
- **Implementation consequence:** do not create a native-style combat click
  dispatcher from this route. Preserve direct click-to-approach-and-strike only
  as an owner-observed interaction contract, pending an independently
  traceable action path and rule-resolution evidence.

### EXE-GOG-COMBAT-009 - Native mouse button-press wrapper has no direct caller

- **Question:** Does the decoded native mouse button-press service provide a
  direct caller that can identify the owner-confirmed enemy-click combat path?
- **Target/method:** Use
  the [FBOV mapped image](#fbov-mapped-image). `ReportInstructionContext` confirmed `MOV AX,5` in
  `45b9:0059` immediately before its `INT 33h`; `ReportReferences` enumerated
  the wrapper's direct callers, and a bounded decompilation classified only its
  register-to-output forwarding.
- **Bounded finding:** `45b9:0059` invokes `INT 33h` with service five and
  copies the returned button/count and coordinate registers to caller-supplied
  output words. It has no recovered direct reference in the mapped image.
- **Interpretation:** this excludes only the simplest static form in which a
  direct caller of the native button-press wrapper owns the combat click. It
  does not identify a different mouse service, indirect call, callback, event
  queue, driver path, or any click/target/attack behavior.
- **Confidence:** high for the decoded service value, forwarding shape, and
  zero recovered direct references; unknown for all mouse and combat semantics.
- **Implementation consequence:** do not treat service five as evidence for a
  combat click dispatcher, target model, approach, or automatic attack rule.

### EXE-GOG-COMBAT-010 - Mouse callback feeds a shared packet boundary, not a combat route

- **Question:** Does registration of the native mouse callback reveal the
  owner-confirmed enemy-click combat path or its action handler?
- **Target/method:** Use
  the [FBOV mapped image](#fbov-mapped-image). `ReportInstructionContext` confirmed the existing
  native callback-registration wrapper at `45b9:0122`; `ReportReferences`
  enumerated its direct caller. Bounded raw-byte windows then decoded the
  caller's stack construction and the recovered callback entry. A reference
  query and bounded context classified the shared far-dispatch target.
- **Bounded finding:** the wrapper has one recovered direct caller,
  `44d0:002f`, reached from `44d0:0006`. That routine first probes native mouse
  availability, then registers native service `0Ch` with mask `0x007f` and
  marks mouse support as available. Its raw stack construction passes its own
  code segment with offset `0x0053`, recovering the native callback entry as
  `44d0:0053`; the mapped decompiler's `45b9:0053` target was an aliasing
  artifact. The callback suppresses forwarding under one resident byte guard.
  Otherwise it constructs a 14-byte stack packet and sends a far pointer to
  shared target `4464:0230`. Its raw local writes are, in word order, literal
  `2`, literal `14`, literal `0`, entry `CX`, entry `DX`, resident segment
  `57e0`, and entry `AX` preserved through `DI`; `BX`, `SI`, and entry `DI`
  are not copied into this packet. The mapped reference view
  has one separately recovered caller of that target, `4201:012c`, which also
  dispatches a 14-byte packet. Its complete bounded function explicitly writes
  packet words `4`, `14`, and `0x00cc`, followed by a guarded `0` or `1`; it
  makes no local write to the remaining three packet words before dispatch.
  This demonstrates differently initialized producer inputs, not a field or
  event-class meaning. Mapping does not recognize the raw callback call as a
  reference. The raw target forwards
  its packet pointer through an opaque far helper, reads the packet's second
  word as a byte count, and copies that many bytes to guarded resident storage
  while updating separate offset/capacity words and failure indicators.
  Its initializer has one recovered direct caller at `39d1:0173`, which passes
  an opaque non-null far-storage pointer and literal capacity `0x410` (1040).
  The initializer stores those values into its resident fields and sets the
  guard; its one further recovered caller provides no feature identity in the
  bounded instruction context.
- **Interpretation:** this establishes a concrete native callback-to-generic
  packet-buffer boundary, rather than a combat callback. The packet field
  meanings, resident guard, buffer consumer behavior, callback event semantics,
  and feature owner remain unknown. It neither connects registration to combat
  nor identifies target selection, approach, striking, or any action rule.
- **Confidence:** high for the single registration caller, service/mask,
  raw callback pointer, guard, literal/register packet layout, dispatch call,
  and separately recovered packet caller/buffer copy/initializer capacity;
  unknown for packet field semantics, dispatch owner, event routing, and all
  combat behavior.
- **Implementation consequence:** do not derive a combat event loop, click
  handler, target model, approach rule, or automatic-attack rule from this
  global registration path. A combat-specific path still needs independent
  executable or controlled-observation evidence.

### EXE-GOG-COMBAT-011 - Status-panel dispatcher is reached indirectly

- **Question:** Can the mapped dispatcher branch that invokes the observed
  status-panel initializer identify a direct application caller or combat-owned
  selector semantics?
- **Target/method:** Use
  the [FBOV mapped image](#fbov-mapped-image). `ReportReferences` queried the entry point
  `76da:007d`; `ReportDecompileWindow` then examined its complete 135-line
  recovered function, bounded to the six-value selector range established by
  `EXE-GOG-COMBAT-008`. `ReportFunctionScalarIntersection` also searched all
  decoded functions for the dispatcher segment `0x76da` and offset `0x7d`.
- **Bounded finding:** no recovered direct reference targets `76da:007d`. Its
  decoded `param_1 == 2` branch accepts selector values `0x7fa` through
  `0x7ff`; selector `0x7fc` performs the panel-adjacent calls and then invokes
  `74bb:0498`. The remaining selector cases contain distinct calls or opaque
  instructions, but no static caller supplies a selector value or a screen,
  actor, encounter, command, or action identity. No decoded function contains
  both segment and offset as immediate operands, excluding the simplest
  statically constructed far-pointer registration form.
- **Interpretation:** the function is an indirect dispatch boundary. Adjacent
  selector cases are not established combat commands, transitions, or rules,
  and the panel case has no recovered application owner.
- **Confidence:** high for the direct-reference result, bounded selector range,
  panel-case call sequence, and absent immediate far-pointer pair; unknown for
  the indirect caller, selector meanings, screen ownership, and all combat
  behavior.
- **Implementation consequence:** do not model the selector range as combat
  state or command IDs. Continue to require an independently traceable indirect
  caller or controlled observation before deriving any combat behavior.

### EXE-GOG-COMBAT-012 - Shared panel state word remains feature-neutral

- **Question:** Do direct accesses to the mapped state word that guards the
  two panel-initializer routes identify combat state or a combat transition?
- **Target/method:** The decompiler-generated label in the panel paths encodes
  flat mapped address `ram:00058bab`. `ReportReferences` enumerated every
  direct reference to that address. The existing complete bounded
  decompilation of `74bb:0077` classified its sole direct write, while
  `EXE-GOG-COMBAT-008` supplies the other panel path's value-two/value-three
  guard.
- **Bounded finding:** Ghidra records 22 direct references from 14 recovered
  functions: 21 reads comparing or loading values zero, one, two, three, four,
  five, or seventeen, and one write. The sole direct write is
  `74bb:00ac`, which assigns four inside `74bb:0077`'s already-indirect route.
  That route calls the panel initializer only under a value-four guard; the
  distinct `74bb:0498` route reaches the same initializer under its recorded
  value-two/value-three condition. No direct reference identifies a screen,
  actor, encounter, command, attack, result, or transition.
- **Interpretation:** the word is a shared mutable state boundary with multiple
  observed numeric values. The two panel paths do not establish one state value
  as combat or a turn phase, and the single direct write does not exclude
  indirect or computed writes.
- **Confidence:** high for the address, direct-reference count, read/write
  classification, visible compared values, and the two different panel-route
  guards; unknown for state ownership, state meanings, initialization, indirect
  writes, and all combat behavior.
- **Implementation consequence:** do not encode these native values as combat
  modes, turn states, panel phases, or transitions. Keep both panel routes and
  all state semantics evidence-only pending an owner or rule-level path.

### EXE-GOG-COMBAT-013 - Coordinate consumer reaches a bounded gate, not an AI route

- **Question:** Does the opaque helper that receives the bounded mouse
  coordinate pair in the recovered input loop identify the owner-confirmed
  enemy-click action or any combat rule consumer?
- **Target/method:** Reuse
  the [FBOV mapped image](#fbov-mapped-image), rebuilt from the documented 49 overlay headers and
  854 trampoline targets in a disposable Ghidra 12.1.3 project. The prior
  bounded decompilation found that one opaque resident-value branch forwards
  two local coordinate words and literal `1` through `thunk_FUN_8d83_0053`.
  `ReportReferences`, `ReportDataBytes` (96 bytes), and
  `ReportInstructionContext` then queried its `8000:d883` transfer target.
- **Bounded finding:** the earlier entry query still has two unconditional
  jumps: the expected `57a6:0070` jump to the thunk entry and `8d83:004f` in
  recovered function `8539:0017`. In the rebuilt mapped image,
  `ReportReferences` finds the latter as the only direct reference to
  `8000:d883`. The 96 target bytes begin four repeated guards: each tests one
  bit in a stack-local byte and, when that bit is present, compares one
  stack-supplied word against a distinct immediate (`0x14`, `0x66`, `0x07`, or
  `0x15`), accumulating a nonzero result on mismatch. The preceding entry
  path has the same form for bit `0x08` and immediate `0x23`. Ghidra aliases
  the target's instruction context into `8539:0017`, a 5,612-line recovered
  function with overlapping instructions and unresolved cross-segment
  constructors; that decompilation remains incoherent outside these directly
  inspected instruction facts.
- **Interpretation:** mapping the destination corrects the earlier
  out-of-image boundary, but only exposes an opaque validation gate. The
  compared values, stack flags, caller contract, and result owner are not
  identified. This does not connect the coordinate branch to enemy targeting,
  path selection, approach movement, striking, damage, turn order, or any AI
  decision.
- **Confidence:** high for the rebuilt-map coverage, direct-reference count,
  raw bytes, and five bounded bit/compare forms; unknown for their semantics,
  the wider malformed function, and all gameplay behavior.
- **Implementation consequence:** do not model the helper as an attack,
  movement, or confirmation handler. A mapped implementation body or an
  independent measured action trace is still required before deriving a combat
  action path.

### EXE-GOG-COMBAT-014 - Combat-adjacent mode switch does not name the coordinate action

- **Question:** Does the resident word whose value five reaches the native
  coordinate-consuming branch identify a combat attack mode, target action, or
  confirmation route?
- **Target/method:** Reuse
  the [FBOV mapped image](#fbov-mapped-image). `ReportReferences` queried the flat mapped address
  `ram:00059240`, which corresponds to the word read as value five by the
  coordinate branch. A complete bounded decompilation of `74bb:0223` inspected
  its direct reads and writes of that word, its one-through-five switch, and
  the adjacent native diagnostic literal. `ReportReferences` then queried the
  function entry.
- **Bounded finding:** Ghidra records 27 direct references across 13 recovered
  functions, including nine direct writes. The visible read comparisons use
  zero, one, four, and five; direct immediate writes establish zero and one,
  while the remaining writes copy register values. In `74bb:0223`, one guarded
  path accepts word values one through five. Its case two enters a sequence
  that passes the native `CAN'T CHANGE LEADER IN COMBAT` diagnostic, establishing
  a combat-conditioned leader-change boundary for that case only. Cases one
  through four have distinct operations; value five falls through to opaque
  shared processing. The coordinate loop separately tests this same word for
  five before forwarding local coordinates through the unmapped thunk from
  `EXE-GOG-COMBAT-013`. No recovered direct reference targets `74bb:0223`.
- **Interpretation:** the word participates in a native multi-mode control
  boundary that is combat-adjacent, but this does not equate any numerical value
  with combat globally. In particular, neither the leader-change diagnostic nor
  the value-five coordinate branch establishes attack mode, enemy targeting,
  approach, striking, confirmation, damage, turn order, or an action outcome.
- **Confidence:** high for the reference count, visible read/write forms,
  one-through-five branch shape, case-two diagnostic path, value-five coordinate
  test, and lack of recovered direct callers; unknown for state ownership,
  indirect callers, all value meanings, and gameplay semantics.
- **Implementation consequence:** do not expose or persist these values as
  combat modes, target states, or commands. Preserve the owner-observed direct
  click behavior separately while a mapped action body or controlled trace is
  obtained.

### EXE-GOG-COMBAT-015 - Direct mode-setter caller supplies value two, not five

- **Question:** Does a direct caller of the resident-mode setter provide the
  value-five coordinate-branch input and thereby identify a combat action?
- **Target/method:** Reuse
  the [FBOV mapped image](#fbov-mapped-image). `ReportReferences` queried the small setter at
  `2b10:007a`, which writes the word examined in `EXE-GOG-COMBAT-014`.
  A complete bounded decompilation of its one recovered caller and a
  16-instruction context at the call site classified the visible argument form.
- **Bounded finding:** the setter has exactly one recovered direct caller, at
  `297f:0023` in `2974:0035`. The immediate instruction context pushes literal
  two directly before the call; it does not supply literal five. The containing
  routine has opaque status and BIOS-keyboard guards, routes either through the
  setter or other helpers, and contains no recovered encounter, actor, target,
  attack, damage, turn, or action-result identity. Its sole recovered entry
  transfer is an unconditional jump at `2834:00a4` from the already-bounded
  opaque selection routine. The 16-instruction context there contains resident
  word initialization/comparisons and a generic far helper, but no combat
  action identity, literal-five assignment, or target/result data.
- **Interpretation:** the simplest recovered setter call supports a value-two
  transfer only. It does not establish what value two means, exclude indirect or
  register-mediated writes of five, or identify the value-five coordinate branch
  as an attack, target selection, movement, or confirmation operation.
- **Confidence:** high for the one direct caller, literal-two instruction
  context, one upstream transfer, and bounded caller shapes; unknown for
  calling convention details, indirect writers/callers, all value meanings,
  and gameplay semantics.
- **Implementation consequence:** do not infer a combat command or action from
  the resident-mode setter. Keep value five and its coordinate consumer
  evidence-only pending a traceable producer and mapped action body.

### EXE-GOG-COMBAT-016 - Other direct writer proves the resident word is not bounded to five values

- **Question:** Does the other direct writer of the coordinate-branch resident
  word receive value five from a traceable native action source?
- **Target/method:** Reuse
  the [FBOV mapped image](#fbov-mapped-image). `ReportReferences` queried direct writer
  `2b10:00c5`, while instruction contexts inspected its sole recovered call site
  at `297f:000b` and its entry sequence.
- **Bounded finding:** the writer has one recovered direct call site. That
  site's far-call shim pushes `0x13` immediately before control transfer. At
  the writer entry, native instructions load the word at `[BP+6]` and copy it
  into both resident words, including the coordinate-branch word at
  `ram:00059240`. Thus the only recovered direct call writes decimal 19, not
  five. The remainder of the 129-line recovered routine uses opaque tables and
  helpers with no recovered encounter, actor, target, attack, damage, turn, or
  action-result identity.
- **Interpretation:** the resident word demonstrably takes a value outside the
  one-through-five switch bounded by `EXE-GOG-COMBAT-014`; that switch is not a
  complete enumeration of its values. This rules out only the direct-writer
  hypothesis for producing value five and does not establish what 19, five, or
  any other value means.
- **Confidence:** high for the one recovered call site, its stack literal,
  parameter load, and two native stores; unknown for indirect callers/writers,
  all state meanings, and gameplay semantics.
- **Implementation consequence:** do not model the resident word as a finite
  combat-mode enum. No combat input, action, target, movement, strike, damage,
  turn, or outcome rule is introduced from this route.

### EXE-GOG-COMBAT-017 - Recovered direct writes do not produce coordinate value five

- **Question:** Does the complete recovered direct-write graph for the resident
  word identify a literal-five producer that can serve as a native combat input
  lead?
- **Target/method:** Reuse the direct-reference inventory for
  `ram:00059240` from `EXE-GOG-COMBAT-014`. Follow the remaining three
  register-mediated writer entries with `ReportReferences`; inspect the one
  helper that has recovered callers using a bounded 16-instruction call context.
- **Bounded finding:** the nine direct writes consist of immediate zero/one
  stores and register-mediated writes. `EXE-GOG-COMBAT-015` and
  `EXE-GOG-COMBAT-016` independently establish direct values two and 19 for
  two writer routes. The writer entries at `2b10:009a`, `74bb:065a`, and the
  switch routine `74bb:0223` have no recovered direct reference. The remaining
  multi-write helper, `2bd8:00e2`, has exactly three direct calls, all from the
  shared input loop `2ae8:0132`; a bounded call context carries opaque literal
  `0x270f`, not five, with no encounter, actor, target, attack, damage, turn,
  or result identity. No recovered direct write supplies literal five.
- **Interpretation:** this exhausts only the Ghidra-recorded direct-write forms
  for this address. It does not exclude indirect, computed, overlay-runtime, or
  external writes, nor establish the meaning of any observed value. In
  particular, it does not make the coordinate value five an attack mode or
  disprove another native action route.
- **Confidence:** high for the direct-write inventory, two/19 direct-value
  results, no-reference results, three-call shared-loop fan-in, and bounded
  opaque-token context; unknown for every indirect/computed writer and all
  gameplay semantics.
- **Implementation consequence:** no resident-word value is exposed as a combat
  command or target state. Continue to require a mapped action body or a
  controlled action trace before deriving combat behavior.

### EXE-GOG-COMBAT-018 - Panel and font requests have no shared decoded owner

- **Question:** Does one decoded function directly co-locate the observed
  combat status-panel request, `BMP ` #19003, with the bounded interface-font
  request, `FONT` #100, providing a native dynamic-panel rendering lead?
- **Target/method:** Reuse
  the [FBOV mapped image](#fbov-mapped-image). `ReportFunctionScalarIntersection` scanned every
  decoded function for all six unsigned instruction scalars: #19003, the two
  `BMP ` tag halves (`0x4d42`, `0x2050`), #100, and the two `FONT` tag halves
  (`0x4f46`, `0x544e`). The query reports only functions containing every
  scalar as decoded instruction operands.
- **Bounded finding:** no decoded function contains all six requested scalar
  operands.
- **Interpretation:** this excludes only a direct literal co-location of the
  two resource requests in one decoded function. It does not exclude separate
  request and renderer functions, arguments, resident handles, pointer tables,
  relocated/overlay code, or runtime-built resource IDs. It identifies no
  text renderer, panel field, active combatant, update cadence, or combat rule.
- **Confidence:** high for the complete decoded-function scalar-intersection
  no-match; unknown for every indirect, relocated, or runtime rendering path.
- **Implementation consequence:** retain the panel's dynamic area as opaque;
  do not derive a text renderer, resource routing model, or combat presenter
  from the static panel and font resources alone.

### EXE-GOG-COMBAT-019 - Computer-control labels are unlinked overlay data

- **Question:** Does the manual's `COMPUTER CONTROL` term identify a native
  handler, default, toggle, or automation path for Space during combat?
- **Target/method:** `ReportPhysicalBytePattern.ps1` ran a bounded
  complete-physical-file scan of the documented
  634,416-byte BLD-GOG-EN-1.1 `DSUN.EXE` searched the exact uppercase ASCII
  term and reported only counts/file offsets. The [FBOV mapped image](#fbov-mapped-image)
  then ran `ReportBytePattern` for the same 16-byte
  sequence under Ghidra 12.1.3/JDK 21.0.12.1, which reports every match, direct
  inbound reference, and decoded-instruction containment.
- **Bounded finding:** the physical file contains four exact occurrences, at
  file offsets 322839, 322902, 322929, and 322953, all in the physical overlay.
  The mapped image contains the same four raw occurrences at `5000:9b17`,
  `5000:9b56`, `5000:9b71`, and `5000:9b89`. Every occurrence is data in a
  mapped block; none has a Ghidra-recorded direct inbound reference or belongs
  to a decoded instruction.
- **Interpretation:** the exact label is not a direct static handler or
  automation lead. It may be reached indirectly, copied at runtime, or be one
  of several diagnostics, but it establishes neither whether computer control
  starts enabled, what Space changes, how it is restored, nor any enemy or
  party decision behavior.
- **Confidence:** high for the exact physical/mapped counts, locations,
  no-reference result, and non-instruction classification; unknown for all
  label ownership and automation semantics.
- **Implementation consequence:** keep the manual's Space binding as an
  unconnected evidence lead. Do not model a computer-control toggle or infer
  AI behavior from the label.

### EXE-GOG-AI-001 - Current static paths do not identify an enemy-decision owner

- **Question:** Do the currently evidenced executable and data entry points
  connect the observed hostile to a native routine that selects an enemy target,
  movement, action, or turn outcome?
- **Target/method:** Review the fingerprinted BLD-GOG-EN-1.1 `DSUN.EXE`
  baseline and the local-only FBOV mapped view under Ghidra 12.1.3/JDK
  21.0.12.1. The focused audit joins the independent actor-object probe
  (`FND-ACTOR-009`), `MONR` tag query (`FND-ACTOR-006`), region entity
  tag query (`FND-REGION-007`), hostile-display path
  (`FND-ACTOR-003`, `FND-ACTOR-005` and `FND-ACTOR-009`), coordinate/input chain
  (`EXE-GOG-COMBAT-007`, `009`, `010`, and `013` through `019`), and native
  RNG selector chain (`FND-RNG-001` to `FND-RNG-008`). The final two focused
  queries rechecked the mapped coordinate destination and the RNG/panel
  routine `74bb:0077`: the former exposes only the bounded opaque validation
  gate in `EXE-GOG-COMBAT-013`, while the latter still has no recovered direct
  reference.
- **Bounded finding:** none of the reviewed paths supplies both an
  actor/hostile identity or record and a recovered decision/action consumer.
  The observed OJFF #9258 has no direct executable consumer; mapped `MONR`
  bytes are unreferenced non-instruction data; and `ETAB` has no loaded tag
  literal. The RDFF route is an indexed-record path with no payload-field
  role. The coordinate path reaches an opaque validation gate, not an actor
  operation; the four literal computer-control labels are likewise unlinked
  data. The RNG chain reaches a shared indirect routine and panel
  initialization, but has no recovered caller or rule-level owner.
- **Interpretation:** this is a coverage statement about the enumerated,
  reproducible paths, not proof that the original has no enemy logic. Indirect
  overlay dispatch, runtime-populated state, constructed tags, and unobserved
  code remain possible. It establishes no target preference, route planner,
  range test, action weighting, turn order, randomness use, or outcome rule.
- **Confidence:** high for the cited narrow findings and their stated limits;
  unknown for all native enemy-AI semantics.
- **Implementation consequence:** no AI model or automated enemy turn may be
  introduced. Resume static work only from a new actor/action anchor supplied
  by a controlled C0-C6 observation, a constrained data relationship, or a
  traceable executable consumer.

### EXE-GOG-CURSOR-001 - Shared cursor selector includes observed interaction resources

- **Question:** Do the observed Walk, Attack, and Look cursor resources have a
  bounded executable selection path that can distinguish presentation evidence
  from an inferred combat command?
- **Target/method:** Reuse the approved BLD-GOG-EN-1.1 `DSUN.EXE` target and
  [FBOV mapped image](#fbov-mapped-image) (Ghidra 12.1.3, JDK
  21.0.12.1). `ReportScalarConstants` searched the eight resource IDs 19101
  through 19108. `ReportInstructionContext` inspected each observed melee
  branch and the routine return; `ReportReferences` then bounded direct callers
  of the recovered routine and its only caller.
- **Bounded finding:** resource IDs 19101 through 19108 all occur in the one
  recovered routine at `2b10:00db`. Its distinct branches load observed melee
  Attack #19103 and invalid melee Attack #19104, as well as the Walk, ranged
  Attack, and Look pairs. One inspected terminal sequence moves its selected
  ID to the return register. The routine has one recovered direct caller,
  `7316:0618`; that caller supplies an opaque four-byte input and has no
  recovered direct caller itself. The two remaining cursor IDs
  (invalid spell target #19109 and processing #19110) have no literal in this
  bounded routine query.
- **Interpretation:** the executable has a shared cursor-resource selection
  boundary consistent with the observed presentation family. The branch inputs
  and the opaque caller are not a recovered actor, map object, action,
  reachability, target, click, or strike path. In particular, selecting #19103
  does not establish an attack command, and selecting #19104 does not identify
  the native reason an attack is disallowed.
- **Confidence:** high for the literal locations, inspected return sequence,
  and direct-reference counts; unknown for caller ownership, branch predicates,
  #19109/#19110 handling, and every gameplay meaning.
- **Implementation consequence:** cursor assets may retain their existing
  presentation provenance only. No combat eligibility, movement, target, or
  damage rule follows from this selector.

### EXE-GOG-CURSOR-002 - Cursor selection is an optional shared-handler branch

- **Question:** Is the recovered cursor selector the universal native handling
  route for an interaction input, or a bounded optional branch whose caller
  shape excludes a direct combat interpretation?
- **Target/method:** Reuse the local mapped GOG executable and the selector
  caller `7316:0618` from `EXE-GOG-CURSOR-001`. Inspect its 96-byte raw window
  and entry context. Query the alternate far-call target's references with
  `ReportReferences`; a scalar-intersection query and an exact encoded
  far-pointer byte-pattern query searched for a recoverable caller owner.
- **Bounded finding:** after rejecting a zero four-byte input, `7316:0618`
  checks the following byte. A nonzero value invokes the cursor selector;
  zero invokes a separate shared helper instead. The selector has this one
  recovered direct caller. The alternate helper has nine recovered direct
  callers across six recovered functions. Neither a function containing both
  source segment/offset scalars nor an exact four-byte encoded far-pointer
  pattern was found, so this bounded query does not recover the outer indirect
  owner.
- **Interpretation:** the cursor-resource selector is a conditional service
  inside a wider handler, rather than evidence for a universal click action.
  The opaque input, flag meaning, alternate helper, indirect owner, and all
  mouse/combat semantics remain unknown. This excludes neither an indirect
  combat route nor another native cursor owner.
- **Confidence:** high for the raw null/byte guard, the two direct call sites,
  selector/alternate direct-reference counts, and both bounded no-match
  results; unknown for input layout semantics, feature ownership, and action
  behavior.
- **Implementation consequence:** retain the cursor selector solely as
  presentation provenance. Do not derive a combat input dispatcher or
  eligibility state from its conditional call.

### EXE-GOG-ITEMS-001 - No raw ITEMS.BIN filename or stem representation in DSUN.EXE

- **Question:** Does the supported executable embed either raw representation
  of `ITEMS.BIN`, or a null-terminated `ITEMS` stem that could provide a
  direct starting point for a constructed item-data filename or layout analysis?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportBytePattern` searched every loaded memory block for both
  exact ASCII bytes `49 54 45 4d 53 2e 42 49 4e`, the same explicit
  null-terminated representation ending in `00`, and the null-terminated
  stem `49 54 45 4d 53 00`.
- **Bounded finding:** none of the three loaded-memory byte patterns exists.
- **Physical-file follow-up:** a byte search of the whole 634,416-byte file,
  including the `FBOV` pack after the load image (FMT-EXE-001), finds no
  `ITEMS.BIN` either.
- **Interpretation:** this rules out only the two queried literal
  representations in this executable. The filename or stem may be absent from
  the code path, constructed or relocated at runtime, owned by another
  executable or data layer, supplied by a launcher, or absent from a runtime
  path. The result establishes neither an item-file reader nor any record
  structure, pair-column role, object-frame relation, equipment, inventory, or
  combat behavior.
- **Confidence:** high for the three absent raw-byte representations; unknown
  for item-data loading, file ownership, table-field roles, and all item
  semantics.
- **Implementation consequence:** `ITEMS.BIN` remains an unparsed source whose
  layout needs an independent bounded lead. Do not use a failed Ghidra
  defined-data/string classification as an absence claim; raw-byte search is
  the prerequisite negative check for this kind of question.

### EXE-GOG-ITEMS-002 - CHARTRAN has no direct item-table filename lead

- **Question:** Does the supported character-creation executable embed either
  the exact `ITEMS.BIN` filename or a null-terminated `ITEMS` stem that could
  constrain a character-creation item-data loader?
- **Target:** BLD-GOG-EN-1.1 `CHARTRAN.EXE`, reverified at 24,761 bytes with
  XXH3-128 `f7466f2ac604dc7c353358bc80bd0131`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportBytePattern` searched every loaded memory block first for
  exact ASCII bytes `49 54 45 4d 53 2e 42 49 4e` and then for the explicit
  null-terminated stem `49 54 45 4d 53 00`.
- **Bounded finding:** neither pattern exists in loaded memory.
- **Interpretation:** this rules out only the two queried literal forms in
  this executable. A loader may construct or receive a name at runtime, live
  in another executable or data layer, or not participate in character
  creation. The result establishes neither a file reader nor any pair-field
  role, equipment path, inventory behavior, or combat behavior.
- **Confidence:** high for the two absent raw-byte representations; unknown
  for item-data loading, file ownership, table-field roles, and all item
  semantics.
- **Implementation consequence:** retain `ITEMS.BIN` as an unparsed source.
  The raw-byte result is a bounded negative lead, not a generic string-analysis
  absence claim and not permission to infer item behavior.

### EXE-GOG-ITEMS-003 - SVIEW has no direct item-table filename lead

- **Question:** Does the separately shipped viewer utility embed either the
  exact `ITEMS.BIN` filename or a null-terminated `ITEMS` stem that could
  identify an item-data loader?
- **Target:** BLD-GOG-EN-1.1 `SVIEW.EXE`, reverified at 89,061 bytes with
  XXH3-128 `2b5581f61c843f6e8f27c8487dd852a9`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1. A full analyzer
  pass completed before the focused queries.
- **Method:** `ReportBytePattern` searched every loaded memory block first for
  exact ASCII bytes `49 54 45 4d 53 2e 42 49 4e` and then for the explicit
  null-terminated stem `49 54 45 4d 53 00`.
- **Bounded finding:** neither pattern exists in loaded memory.
- **Interpretation:** this rules out only the two queried literal forms in the
  viewer utility. It does not establish whether another executable, a
  constructed/indirect name, or a data-layer path reads the table, and does not
  assign a pair-field role or item, equipment, inventory, or combat behavior.
- **Confidence:** high for the two absent raw-byte representations; unknown
  for item-data loading, utility ownership, table-field roles, and all item
  semantics.
- **Implementation consequence:** retain `ITEMS.BIN` as an unparsed source.
  This is a bounded negative lead, not a statement that the utility or game
  cannot access item data indirectly.

### EXE-GOG-CHAR-002 - CHARSAVE filename strings do not identify a party loader

- **Question:** Does an embedded `CHARSAVE.GFF` filename identify an executable
  code reference that can constrain the archive loader or START GAME's supplied
  party selection?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
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
  matching intersection query for the disc candidate IDs #40, #41, #42, and
  #43 likewise found no function containing all four. A
  separate raw byte-pattern search of fingerprinted `CHARTRAN.EXE` (24,761
  bytes, XXH3-128
  `f7466f2ac604dc7c353358bc80bd0131`) found
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
  installed or disc resource-ID run is not evidence that those records form
  the supplied party, and the negative string query does not prove `CHARTRAN.EXE`
  cannot access the archive indirectly.
- **Confidence:** high for the two raw matches and absent direct references;
  unknown for loader ownership and party-selection behavior.
- **Implementation consequence:** retain `ShippedPartyUnresolved` and do not
  assign the catalog's records to START GAME. A controlled startup observation
  or an independently corroborated loader/state trace is required before party
  logic is introduced.

### EXE-GOG-CHAR-003 - Character resource tags do not supply a direct loader

- **Question:** Do raw `CHAR` or `PSIN` resource-tag literals expose a direct
  executable path for the character archive or supplied-party selection?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportBytePattern` searched every loaded block for the exact
  four-byte ASCII encodings `CHAR` and `PSIN`. The reusable report also emits
  up to twenty direct references for every raw match.
- **Bounded finding:** `CHAR` has fourteen raw matches, all in `CODE_208`, and
  none has a direct Ghidra reference. `PSIN` has no raw byte-pattern match.
  The `CHAR` bytes include data/text occurrences and do not by themselves
  classify any match as a resource-tag use, loader, archive open, record lookup,
  or START GAME selection.
- **Interpretation:** the query rejects only a direct literal-tag path in this
  executable. It does not rule out a constructed tag, indirect resource
  manager call, another module, or runtime-propagated resource identity.
- **Confidence:** high for the capped raw-match and direct-reference results;
  unknown for archive ownership, character loading, and party selection.
- **Implementation consequence:** preserve `ShippedPartyUnresolved`. Do not
  bind `CHAR`/`PSIN` catalog records to START GAME from these tag queries.

### EXE-GOG-CHAR-004 - SVIEW has no literal character-archive lead

- **Question:** Does the separately shipped `SVIEW.EXE` utility provide a
  direct static character-archive or `CHAR` tag path that could constrain the
  supplied START GAME party?
- **Target:** BLD-GOG-EN-1.1 `SVIEW.EXE`, reverified at 89,061 bytes with
  XXH3-128 `2b5581f61c843f6e8f27c8487dd852a9`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1. A full analyzer
  pass completed before the focused queries.
- **Method:** `ReportBytePattern` searched every loaded block for the exact
  null-terminated ASCII encoding of `CHARSAVE.GFF` and, separately, the exact
  four-byte ASCII encoding `CHAR`.
- **Bounded finding:** neither byte pattern occurs in the loaded `SVIEW.EXE`
  image. This produces no archive pathname, direct tag request, record lookup,
  or supplied-party-selection lead in the utility.
- **Interpretation:** this excludes only these literal representations in this
  executable. It does not show that `SVIEW.EXE` cannot consume character data
  through a constructed/indirect route, and it says nothing about `DSUN.EXE`'s
  runtime party selection.
- **Confidence:** high for the two absent raw-pattern results in the
  fingerprinted utility; unknown for utility ownership and all character or
  START GAME behavior.
- **Implementation consequence:** retain `ShippedPartyUnresolved`. Do not use
  `SVIEW.EXE` as evidence to bind any `CHAR` catalog record to START GAME; the
  controlled S0-S2 startup observation or an independently corroborated main
  executable state trace remains required.

### EXE-GOG-CHAR-005 - CHARTRAN has no direct character resource-tag path

- **Question:** Does the separately shipped character-transfer utility expose
  a direct `CHAR` or `PSIN` resource-tag path that could constrain character
  archive loading or the supplied START GAME party?
- **Target:** BLD-GOG-EN-1.1 `CHARTRAN.EXE`, reverified at 24,761 bytes with
  XXH3-128 `f7466f2ac604dc7c353358bc80bd0131`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1. A full analyzer
  pass completed before the focused queries.
- **Method:** `ReportBytePattern` searched every loaded program-memory block
  for the exact four-byte ASCII encodings `CHAR` and `PSIN`. For each raw match,
  the bounded report enumerates direct inbound references.
- **Bounded finding:** `CHAR` occurs once at `1000:58ad`, but Ghidra reports no
  direct reference to that address. `PSIN` has no raw byte-pattern match in the
  loaded utility image. The queries identify no tag assignment, resource
  request, archive open, character-record lookup, or party-selection route.
- **Interpretation:** this excludes only direct literal-tag representations in
  this transfer utility. It does not prove that `CHARTRAN.EXE` cannot access
  character data through constructed tags or indirect state, and it does not
  establish any `DSUN.EXE` START GAME behavior.
- **Confidence:** high for the one raw `CHAR` occurrence, its absent direct
  reference, and the absent `PSIN` pattern; unknown for character-transfer
  behavior, archive ownership, and supplied-party selection.
- **Implementation consequence:** retain `ShippedPartyUnresolved`. Do not bind
  any `CHAR` catalog record to START GAME from this utility; the controlled
  S0-S2 startup observation or an independently corroborated main-executable
  state trace remains required.

### EXE-GOG-CHAR-006 - Observed party names are not hardcoded native selector strings

- **Question:** Does the supported executable embed one of the exact
  NUL-terminated names visible in the owner-confirmed supplied-party captures,
  providing a direct static selector lead?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1. A fresh disposable
  project completed Ghidra's default auto-analysis before the focused queries.
- **Method:** `ReportBytePattern` searched every loaded memory block,
  individually and with its 100-match cap, for the exact ASCII-plus-NUL
  encodings of `Ar'Anda`, `Terrannus`, `Thy'rokh`, and `Gerakis`.
  The query would enumerate direct references for each raw match.
- **Bounded finding:** none of the four patterns occurs in the loaded program
  image. Consequently this query yields no raw string address, direct
  reference, loader, resource lookup, or START GAME selection path.
- **Interpretation:** this excludes only an exact NUL-terminated hardcoded-name
  representation in this executable. It does not exclude name data in
  `CHARSAVE.GFF`, a differently encoded or non-NUL representation, constructed
  text, indirect/resource-derived selection, another executable, or a runtime
  table. It does not identify a default-party member record.
- **Confidence:** high for the four absent raw patterns in this fingerprinted
  loaded image; unknown for archive loading and supplied-party selection.
- **Implementation consequence:** retain `ShippedPartyUnresolved`; do not use
  the observed names, their displayed order, or this negative query to create a
  four-member default table.

### EXE-GOG-CHAR-007 - Contiguous class-label table has no direct role-selector lead

- **Question:** Do the class labels visible in the confirmed character views
  identify a direct native table or code reference that can map a `CHAR`
  record's unknown fields to an on-screen role?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, 634,416 bytes, XXH3-128
  `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3 with the previously completed default auto-analysis.
- **Method:** `ReportBytePattern` located the exact NUL-terminated
  PRESERVER, CLERIC, FIGHTER, and GLADIATOR label encodings from the four
  owner-confirmed character views. `ReportDataBytes` inspected the bounded
  96-byte span beginning at the first located label, and
  `ReportReferences` inspected each of the four raw-label addresses. After
  the absent direct references, a second raw-byte query searched every loaded
  block for the exact little-endian segmented far-pointer encoding
  `8c 90 00 50` of the table base `5000:908c`.
- **Bounded finding:** the 64-byte span at `5000:908c` through
  `5000:90cb` holds exactly eight adjacent NUL-terminated labels in this
  order: Cleric, Druid, Fighter, Gladiator, Preserver, Psionicist, Ranger, and
  Thief. The following bytes begin a separate adjacent stat-label family
  (STR, DEX, CON, INT, WIS, CHR). Ghidra reports no inbound direct reference
  to the four queried labels at `5000:908c`, `5000:9099`, `5000:90a1`,
  and `5000:90ab`; the exact four-byte far-pointer encoding is also absent.
- **Interpretation:** the span establishes the executable's finite class-label
  vocabulary and its local serialized order. It does not establish that these
  ordinal positions are `CHAR` field values, that the text is rendered on
  the observed screen, or which code displays, chooses, or mutates a class.
  The absent direct reference and one exact stored-pointer form still leave a
  constructed/relocated pointer, another executable, resource data, or runtime
  state possible.
- **Confidence:** high for the bounded labels, their order, the next-table
  boundary, and the four absent direct-reference results; unknown for record
  layout, UI ownership, and all class behavior.
- **Implementation consequence:** retain the existing independently designed
  class enum and evidence conflicts, but do not add a `CHAR` class-field
  reader, default-party selector, or screen-data binding from this table.

### EXE-GOG-CHAR-008 - Character-view vocabulary families are bounded but unlinked

- **Question:** Do the long origin and alignment labels visible in the
  owner-confirmed character views bound the native character-view vocabulary,
  and do they directly identify a record-field or rendering path?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, 634,416 bytes, XXH3-128
  `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3 with the previously completed default auto-analysis.
- **Method:** `ReportBytePattern` located exact NUL-terminated THRI-KREEN,
  HALF-GIANT, CHAOTIC NEUTRAL, and LAWFUL GOOD encodings. Bounded data windows
  of 80 bytes at `5000:9040` and 128 bytes at `5000:90e0` established the
  adjacent printable-table boundaries. `ReportReferences` queried the two
  long-origin and two alignment addresses. A follow-up
  `ReportInstructionText` scan searched every decoded instruction rendering
  for the four explicit table-base operands `0x9040`, `0x908c`, `0x90e0`, and
  `0xab20`; the first three are the bounded gender/origin/class/stat/alignment
  vocabulary area, and the last is the separately bounded destination-label
  pointer table.
- **Bounded finding:** a local text family holds:

  - gender labels Male and Female;
  - eight origins in order: Human, Dwarf, Elf, Half-Elf, Half-Giant, Halfling,
    Mul, Thri-Kreen;
  - the eight class labels documented by `EXE-GOG-CHAR-007`;
  - the six stat labels STR, DEX, CON, INT, WIS, and CHR;
  - nine alignments in order: Lawful Good, Lawful Neutral, Lawful Evil, Neutral
    Good, True Neutral, Neutral Evil, Chaotic Good, Chaotic Neutral, and
    Chaotic Evil.

  The bounded origin and alignment spans meet the adjacent class/stat spans;
  their queried raw addresses are `5000:9069`, `5000:9081`, `5000:90eb`,
  and `5000:9146`. Ghidra reports no inbound direct reference to any of
  those four addresses. The complete decoded-instruction scan also has no
  rendered operand naming any of the four supplied table bases.
- **Interpretation:** this is a finite native vocabulary and local table
  ordering, not a serialized `CHAR` schema. It does not establish gender,
  origin, alignment, or class ordinal field offsets; which screen consumes
  these labels; or any creation, eligibility, combat, or spell behavior. The
  base-operand absence excludes only simple decoded absolute addressing;
  relocated, register-constructed, resource-derived, and indirect paths
  remain possible.
- **Confidence:** high for the exact bounded label families, ordering, and
  four absent direct-reference results; unknown for every field and runtime
  consumer.
- **Implementation consequence:** use the documented modern `origin`
  terminology in new code. Do not extract this text as a character-field map
  or bind it to the observed party until an independent data or native path
  establishes that connection.

### EXE-GOG-UI-009 - Character-view button IDs are not direct handler operands

- **Question:** Do selected character-view controls with a visible navigation
  identity or nonzero event mask occur as direct native control-handler
  operands?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, JDK 21.0.12.1, and the previously completed default
  auto-analysis.
- **Method:** `ReportScalarConstants` scanned every decoded instruction
  operand for exact unsigned decimal values 11308 and 10308, then separately
  for 11318, 11319, and 11320. The first is the 28x16 character-view `BUTN`
  that reuses the Preferences Game Menu icon; the second is the independently
  evidenced Return control; the last three are the only character-view BUTN
  records with nonzero event masks. Each query reports at most 256 matches,
  and neither query produced a match.
- **Bounded finding:** none of the five requested control identities occurs as
  a decoded instruction scalar.
- **Interpretation:** this excludes only direct immediate-ID dispatch in the
  analyzed executable. It does not negate #10308's independently established
  Return behavior, nor does it prove that #11308 opens Game Menu on the
  character view. Resource-derived registration, callbacks, indirect lookup,
  another module, and runtime state remain possible.
- **Confidence:** high for all five absent immediate scalar operands; unknown
  for #11308's character-view activation route and all other
  application-specific control behavior.
- **Implementation consequence:** keep the five independently evidenced
  character/inventory navigation controls routable, retain #11308 visually and
  structurally only where its source asset is required, and do not add a
  character-view Game Menu route without a controlled native trace or an
  independent bounded activation path.

### EXE-GOG-UI-010 - Character/destination label table is bounded but unlinked

- **Question:** Does the exact native `VIEW CHARACTER` title text identify a
  direct screen-construction or field-rendering path?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, JDK 21.0.12.1, and the previously completed default
  auto-analysis.
- **Method:** `ReportBytePattern` searched all loaded memory for the exact
  NUL-terminated ASCII pattern for `VIEW CHARACTER`, finding one raw match.
  `ReportDataBytes` then inspected the bounded 256-byte neighborhood at
  `5000:ab20`; `ReportReferences` queried both that apparent table base and
  the found text address. `ReportInstructionText` later included `0xab20` in
  its complete decoded-instruction operand scan alongside the adjacent
  character-vocabulary table bases.
- **Bounded finding:** `VIEW CHARACTER` occurs once at `5000:ab59`. Eight
  adjacent far pointers at `5000:ab20` target, in order, `VIEW CHARACTER`,
  `VIEW INVENTORY`, `CAST SPELL/USE PSIONIC`, `CURRENT SPELL EFFECTS`,
  `MEMORIZE SPELLS`, `HIT POINTS: CURRENT/MAX`, `PSIONIC POINTS:
  CURRENT/MAX`, and `CURRENT STATUS`. The two preceding standalone strings
  are `GAME MENU` and `RETURN TO GAME`. Ghidra reports no inbound direct
  reference to either `5000:ab20` or `5000:ab59`, and no decoded instruction
  rendering contains the explicit `0xab20` operand.
- **Interpretation:** the bounded data establishes a finite native UI
  vocabulary and local pointer order. It does not show which window consumes
  it, whether these labels ever render as text rather than resource artwork,
  a field layout, a value source, a button route, or an activation path.
  The operand absence excludes only a simple decoded absolute base reference;
  indirect tables, relocated pointers, another module, and runtime state
  remain possible.
- **Confidence:** high for the one raw match, the eight pointers and their
  printable targets, the adjacent standalone labels, and the two absent
  direct-reference results; unknown for all screen ownership and behavior.
- **Implementation consequence:** retain the independently measured source
  windows, icon mappings, and title artwork. Do not turn this vocabulary into
  destination-field rendering, a text asset contract, or control semantics
  until an independent data/native path or controlled observation establishes
  the consuming screen.

### EXE-GOG-GPLDATA-001 - literal GPL archive name has no direct reference

- **Question:** Does the supported executable expose a direct static loader
  path for `GPLDATA.GFF` that can constrain GPLI or dialogue ownership?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1. The full analyzer
  pass completed before these focused queries.
- **Method:** `ReportBytePattern` searched all mapped blocks for the eleven
  ASCII bytes spelling `GPLDATA.GFF`, then `ReportReferences` queried every
  direct inbound reference to the sole matched virtual address.
- **Bounded finding:** the pathname occurs once at `5000:88f1` in `CODE_208`.
  Ghidra reports no inbound direct reference to that address after the full
  analyzer pass. A raw search also finds no exact in-image little-endian far
  pointer encoding (`f1 88 00 50`) for that virtual address. This establishes a
  single literal archive-name occurrence, not a file open, a resource lookup,
  a GPLI consumer, or a dialogue trigger.
- **Interpretation:** the string may be used through an indirect pointer,
  relocated/constructed data, another executable, or an unrecognized code
  path. The absent reference and exact far-pointer encoding reject only those
  direct loader leads; they do not show that the archive is unused.
- **Confidence:** high for the exact occurrence and lack of direct references;
  unknown for archive ownership, load timing, resource-tag selection, and all
  player-visible behavior.
- **Implementation consequence:** do not add a GPLI reader or connect an
  archive literal to dialogue. Continue to require a constrained call path or
  controlled observation.

### EXE-GOG-GPLI-001 - no native literal tag establishes GPLI ownership

- **Question:** Does the supplied executable identify `GPLI` as a resource tag,
  so that the 329 fixed-width records in `GPLDATA.GFF` can safely be assigned a
  native lookup or script role?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1. The full analyzer
  pass completed before this focused query.
- **Method:** `ReportBytePattern` searched all mapped blocks for bytes
  `47 50 4c 49`, the exact ASCII spelling of `GPLI`.
- **Bounded finding:** the query found no matching byte sequence. Independently,
  `DATA-GOG-GPLI-001` establishes that `GPLDATA.GFF` `GPLI` #1 is 7,896 bytes,
  exactly 329 records of 24 bytes, each containing four consecutive six-byte
  lanes. Its only two aligned-lane occurrences of the known GPL numeric ID
  135 are in distinct records and lane positions, so even that candidate does
  not supply a unique fixed-lane mapping. The archive facts do not compensate
  for the absent executable lead.
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

### EXE-GOG-GPLI-002 - GPL resource 135 has no direct hardcoded request lead

- **Question:** Does the known first-Tyr dialogue resource ID 135 co-occur with
  the decoded little-endian GPL tag scalar in a single native function,
  identifying a direct hardcoded request path that could corroborate a GPLI
  lane?
- **Target:** BLD-GOG-EN-1.1 DSUN.EXE at the documented stable approved path,
  634,416 bytes and XXH3-128 e296af55ba2ecde7e77f555c90f33d0b;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1, and the existing
  completed default analysis.
- **Method:** ReportFunctionScalarIntersection scanned decoded instruction
  operands in every discovered function for both unsigned scalar 135 and
  scalar 0x204c5047, the little-endian machine-value representation of the
  four bytes GPL followed by a space.
- **Bounded finding:** no discovered function contains both requested scalar
  operands. The query therefore finds no direct hardcoded GPL resource-135
  request site from which to derive a GPLI field interpretation.
- **Interpretation:** this excludes only a function whose decoded operands
  directly contain both the known ID and tag. The resource can still be reached
  through arguments, resident memory, a constructed tag, a relocated pointer,
  another executable, or an unrecognized path. It neither proves that GPLI is
  unused nor links any GPLI lane to a GPL resource, dialogue, condition,
  encounter, or quest.
- **Confidence:** high for the bounded decoded-operand intersection; unknown
  for runtime selection and every GPLI field role.
- **Implementation consequence:** retain GPLI as DSOP and keep the bounded
  dialogue projection tied only to independently captured GPL resource-135 evidence.

### EXE-GOG-EVENT-001 - shared processing entry consumes linked runtime selectors

- **Question:** Do the coherent callers of the shared `172c:000c` processing
  entry establish a bounded record envelope or a feature-specific rule?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1. The full analyzer
  pass completed before these focused queries.
- **Method:** `ReportReferences` enumerated the entry's eight direct far
  callers, then enumerated all direct references to the resident table pointer
  at `5b7c:0700`. Bounded instruction contexts classified immediate argument
  setup and the one direct caller of the observed initializer, including the
  entry-flow's three resident-word pushes and the initializer's first-argument
  guard. A second pair of bounded instruction contexts classified the
  initializer's only two direct selector-pointer accesses. A bounded
  decompilation of its shared callee identified the byte-fill helper used
  immediately before the link loop. Four bounded decompilations of the `1695`
  callers compared every dereferenced offset and all chain termination and
  match predicates. Bounded decompilations of `172c:000c`, its `00a1` helper,
  and the already documented `0299` GPL/MAS request entry followed the three
  supplied arguments without assigning a record field name. A later
  160-line decompilation attempt for initializer `277b:0024` was excluded: its
  far-control-flow recovery reports unresolved destinations outside loaded
  memory, so it supplies no reliable allocation or population inference. A
  `ReportFunctionScalarIntersection` probe for the table's segment `0x5b7c`
  and offset `0x0700` found no function containing both scalar operands.
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
  The common `172c:000c` entry acts only when resident byte `4c0e:000b` is 2.
  It resets several resident values and forwards its three supplied words to
  `172c:00a1`. Subject to a nonzero first word and another resident guard,
  that helper forwards the first word and the literal third word to
  `172c:0299`; the latter records them as a requested identity/selector pair
  and invokes the bounded loader path. The selectors established in
  `EXE-GOG-GPL-001` map literal selector 1 to the `GPL ` source family.
  Therefore these traversals can request a `GPL ` resource identified by their
  leading record word under the observed guards. The second supplied record
  word also reaches `172c:01c1` only under a separate resident-state condition;
  its known bounded bookkeeping behavior does not establish a record-field or
  feature meaning.
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
  behavior remain unknown. `ReportReferences` found 34 direct references to
  `5b7c:0700`, all reads, across nine functions; it found no direct pointer
  write. One of those functions, `277b:0024`, has exactly one direct caller:
  the executable entry at `1000:0158`, which pushes resident words at offsets
  `0x88`, `0x86`, and `0x84` before the far call. The initializer reads the
  most recently pushed value through its first stack argument and immediately
  rejects values below one. These instructions establish only entry-flow
  argument provenance and a lower-bound guard, not a field meaning or table
  owner. Immediately before the link loop, it calls a verified generic
  byte-fill helper at that same table pointer with an observed length of
  `0x0a28` and fill byte `0xff`; `0x0a28` equals 200 13-byte strides. A
  direct instruction context independently corroborates this sequence without
  relying on the initializer's unreliable decompilation: it observes the far
  table pointer pushed with the composite immediate `0x0a28ffff` before the
  shared call and an eight-byte stack cleanup after it. The sole later pointer
  load follows an explicit multiply by 13, writes the successor at offset 11,
  compares the source index to `0xc8`, and then clears the secondary-chain
  head. These observations corroborate the clear/link setup mechanics only;
  they do not identify the pointer's allocation, its source data, or any
  record field meaning. A
  direct-reference query of the secondary-chain head at `4c13:032d` found
  exactly three direct uses: the initializer's zero write, plus one read and
  one write in the proven node-removal helper `1695:07dd`. It then
  writes the offset-11 link for source indices 0 through 199 to each next
  numeric index, so the final written value is 200. It clears the
  secondary-chain head at `4c13:032d` and resets five adjacent resident
  16-bit fields to `-1`. This establishes deterministic native link setup on
  an entry flow, including a 2,600-byte cleared span, but not a heap allocation
  or valid-record bound: the observed loop writes no terminal link for index
  200. The scalar-intersection absence closes only the narrow hypothesis of
  one function materializing this pointer through both literal operands; it
  does not rule out split, computed, indirect, or dynamically supplied table
  ownership. `ReportDataBytes` also found sixteen zero bytes at `5b7c:0700`
  in the executable's initialized image. Alongside the all-read
  direct-reference result, this excludes treating the pointer as a statically
  initialized source mapping but does not identify its runtime write or
  population path.
- **Interpretation:** this is a verified linked runtime-selector shape that can
  request the `GPL ` source family through the shared processing path. It is
  not evidence that a particular record is a map trigger, dialogue option,
  combat event, or an on-disc file format, nor that a requested GPL resource
  executes or has any particular outcome. The apparent 13-byte stride is
  proven for these callers and entry-flow link setup only; table allocation and
  population, index-source validity (including index 200), record-to-resource
  mapping, execution outcome, and relation to MAS remain unknown.
- **Confidence:** high for the four routine-local record accesses, mutable
  chain shape, deterministic entry-flow clear/link writes, and guarded GPL
  request relationship; unknown for table ownership, allocation/valid record
  bounds, field semantics, record-to-resource mapping, and player-visible
  behavior.
- **Implementation consequence:** do not add a resource parser, persist this
  data, or infer interaction triggers. Preserve the evidence as `Q-SCRIPT-002` until a
  source container, record population path, or controlled observation
  independently connects this runtime table to game content.

### EXE-GOG-RECORD19-001 - A separate linked 19-byte resident record family

- **Question:** Do the other direct callers of the shared GPL request entry
  identify the source or ownership of the linked 13-byte selector records?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportReferences` enumerated the eight direct calls to
  `172c:000c`; three occur in the `2d40` module. `ReportInstructionContext`
  inspected those calls, `ReportDecompileWindow` bounded the traversal and
  its two list-maintenance helpers, and a direct-reference query of the
  traversal head located its reads and writes.
- **Bounded finding:** all three `2d40` calls index a distinct resident record
  family with `index * 19`. Two calls supply the words at offsets 8 and 12 to
  `172c:000c` with literal selector 1; the third supplies offsets 10 and 14.
  The bounded traversal at `2d40:0ef1` starts at resident head
  `5b7c:2135`, follows the signed byte at offset 18, and invokes the same
  common entry only when its local guards permit it. The maintenance pair uses
  signed predecessor/successor bytes at offsets 17/18: one prepends an
  unlinked record to the head, and the other reconnects neighbors, updates the
  head when necessary, clears the 19-byte record, and restores both link bytes
  to `-1`. A separate direct updater scans exactly indices 0 through 47,
  subject to local guards replaces a word at offset 0, and has two direct
  callers in `28c9`; those callers supply resident words but do not identify
  the record family or any player-visible action.
- **Interpretation:** the `2d40` callers establish a second mutable linked
  resident table which can request `GPL ` resources through the same common
  entry. It is structurally distinct from the 13-byte selector table; shared
  processing does not make either table the other's source or establish a
  dialogue, quest, map, object, combat, or script-execution role.
- **Confidence:** high for the three call sites, 19-byte stride, pair offsets,
  selector literal, head/list-link mechanics, bounded clear, and the updater's
  48-entry scan; unknown for allocation, table population, record-field
  meanings, resource mapping, caller ownership, and player-visible behavior.
- **Implementation consequence:** retain the records and any associated source
  payloads as opaque. Do not merge this family with the 13-byte selectors or
  create a GPL interpreter, interaction trigger, or gameplay system from the
  common entry.

### EXE-GOG-GPL-001 - GPL and MAS are selected script-resource families

- **Question:** Does the original executable distinguish `GPL ` from `MAS `
  when loading script resources, and what bounded ownership/caching behavior
  can be established without assigning instruction semantics?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1. The full analyzer
  pass completed before this focused query.
- **Method:** `ReportBytePattern` found the explicit four-byte `GPL ` and
  `MAS ` tags. `ReportInstructionContext` inspected their two assignments in
  one function. A single 78-line `ReportDecompileWindow` and its bounded caller
  list then established the surrounding control/data flow. A follow-up
  `ReportFunctionScalarIntersection` queried GPL #135 together with the two
  little-endian 16-bit halves of the literal GPL tag (`0x4c50`, `0x2047`) to
  test the narrow direct-loader hypothesis for the measured first dialogue.
  A final `ReportReferences`/`ReportInstructionContext` pair enumerated the
  decoded direct callers of the guarded processing helper at `172c:00a1` and
  classified its one recovered call-site argument setup.
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
  argument is recorded. A separate coherent chain begins at `172c:00a1`: its
  sole decoded direct caller is `172c:000c`, which forwards three caller
  parameters and a resident byte into the helper rather than a literal resource
  identity. After its nonzero first-argument and resident-state guards, it
  calls `172c:0299`,
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
  caller-level feature meaning is established. No function contains all three
  scalar values GPL #135, `0x4c50`, and `0x2047`; this excludes a direct
  co-located literal-tag request for that known script, not an indirect or
  runtime-propagated request.
- **Interpretation:** `GPL ` and `MAS ` are distinct native input families,
  not interchangeable labels for the same extracted payload. The helpers and
  cache establish a native loading boundary, but do not license execution of
  source bytes or a general interpreter.
- **Confidence:** high for selector validation, tag choice, fixed cache bound,
  bounded allocation/copy sequence, 50-slot storage/rollover/LIFO-pop shape,
  separately verified range-gated indirect dispatcher, and the bounded
  no-co-location result for GPL #135; unknown for opcode semantics, script side
  effects, cache replacement/ordering, live recorder consumption, handler
  meanings, and higher-level caller intent.
- **Implementation consequence:** DSGP v2 records the exact source tag with
  resource identity and bytes. The first-Tyr dialogue projections require
  `GPL ` and the global-string projections require `MAS `; both remain
  fail-closed, bounded projections rather than a GPL interpreter.

### EXE-GOG-SCMD-001 - SCMD uses a separate bounded cache

- **Question:** Does the opaque `SCMD` family provide the source table or a
  direct execution path for the linked 13-byte selectors?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportBytePattern` located both in-image `SCMD` tag
  assignments. A bounded decompilation of their shared function
  `31e0:1893` classified its resource lookup and cache shape;
  `ReportReferences` enumerated its direct callers, and one caller was bounded
  to classify its immediate request/result flow.
- **Bounded finding:** `31e0:1893` queries the `SCMD` tag by a caller-supplied
  identity and maintains 64 resident cache slots. It reuses a matching entry
  when available; otherwise it obtains the source extent, allocates/copies the
  payload through shared helpers, and records the new entry. The direct caller
  list contains only `31e0:1808` and `31e0:2670`. The bounded first caller
  sources a signed identity from a 37-byte indexed resident record, requests
  its negated non-sentinel value, and stores the loader result back into that
  record. Neither direct caller is one of the `1695` selector traversals nor
  the `172c` GPL request path documented in `EXE-GOG-EVENT-001`.
- **Interpretation:** `SCMD` is a separately selected native resource family
  with a bounded cache. This excludes only the narrow direct-call hypothesis
  that its known loader itself materializes or receives the 13-byte selector
  records. It does not identify SCMD record semantics, cache lifetime, indirect
  callers, object behavior, combat behavior, or any player-visible effect.
- **Confidence:** high for the literal tag, 64-slot cache bound, and two direct
  callers; unknown for resource payload semantics, higher-level ownership, and
  every relationship not represented by a direct call.
- **Implementation consequence:** retain `SCMD` as DSOP. Do not add an SCMD
  reader, connect it to selectors, or infer object/combat behavior without a
  constrained call path or controlled observation.

### EXE-GOG-SMALLTAG-001 - No direct literal loaders for four small families

- **Question:** Do the small opaque `GREQ`, `CACT`, `PLYL`, or `CSEQ` GFF
  families supply a direct static source lead for selectors or gameplay rules?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportBytePattern` searched all loaded executable blocks for
  the exact four-byte ASCII encodings of `GREQ`, `CACT`, `PLYL`, and `CSEQ`.
  The bounded resource inventory identifies ten, eleven, six, and one owned
  records respectively; no payload bytes were retained.
- **Bounded finding:** none of the four patterns occurs in the executable
  image. This query therefore yields no direct tag assignment, resource lookup,
  cache, caller, or selector-table connection for any of these families.
- **Interpretation:** the result excludes only a literal-tag source path in
  this executable. It does not rule out constructed tags, indirect/resource
  manager lookups, another module, or runtime-propagated data, and it assigns
  no meaning to any payload.
- **Confidence:** high for the exact four literal absences; unknown for every
  family’s loader, format, ownership, and player-visible behavior.
- **Implementation consequence:** retain all four families as DSOP. Do not add
  readers or infer quest, interaction, character, or combat behavior from this
  negative result.

### EXE-GOG-SOUND-001 - No literal sound-configuration loader path

- **Question:** Does the supported executable contain a direct textual link
  from either examined sound-configuration pathname (`SOUND.CFG` or
  `SOUND.INI`) to a Preferences setting or configuration-loading path?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** after the normal analyzer pass, `ReportBytePattern` searched all
  loaded blocks for each nine-byte ASCII sequence `SOUND.CFG` and `SOUND.INI`
  and would have reported every matching address and direct reference. Neither
  query produced a match.
- **Bounded finding:** neither literal pathname is present in this executable
  image. Therefore these queries supply no direct executable call, path
  reference, or control binding that can connect either candidate configuration
  source to the Preferences controls.
- **Interpretation:** this rules out only a static literal-name lead in this
  binary. It does not rule out a dynamically assembled path, a configuration
  reader in another executable/module, a differently named source, or runtime
  propagation through resident state.
- **Confidence:** high for the two bounded literal-name absences; unknown for
  every configuration field and setting behavior.
- **Implementation consequence:** retain inert Preferences mutations. Do not
  map `SOUND.CFG`/`SOUND.INI` fields, setting ranges, defaults, or audio timing
  from these absences; seek a corroborating controlled observation or a bounded
  finding in the actual configuration-owning path.

### EXE-GOG-SOUND-002 - Sound helper has no loaded VOC header signature

- **Question:** Does the separately shipped sound helper expose a direct native
  VOC decoder boundary by embedding the fixed `Creative Voice File` header
  signature present in every owned voice/sound-effect file?
- **Target:** BLD-GOG-EN-1.1 `SOUND_DS.EXE`, reverified at 204,593 bytes with
  XXH3-128 `236c2dc23c071eca421eb5b427caee57`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1. A full analyzer
  pass completed before this focused query.
- **Method:** `ReportBytePattern` searched every loaded program-memory block
  for the explicit 19-byte ASCII `Creative Voice File` signature. The search
  did not reach its 100-match cap.
- **Bounded finding:** the helper's loaded executable image has no occurrence
  of the complete VOC signature. This query consequently found no direct
  header comparison, decoder routine, codec selection, sample-rate transform,
  timing conversion, or file-to-playback call path.
- **Interpretation:** this excludes only a literal complete-header validation
  lead in the loaded helper image. It does not prove that the helper cannot
  play VOC data: validation may be partial, bytewise, constructed, delegated,
  present only in its physical overlay, or absent. It assigns no meaning to a
  VOC time constant or codec byte.
- **Confidence:** high for the exact loaded-image signature absence; unknown
  for every decoder, mixer, device, codec, sample-rate, routing, and timing
  behavior.
- **Implementation consequence:** do not adopt a standard VOC decoder or
  derive sample rates/playback timing from the header envelope alone. Preserve
  the files as opaque local assets until a bounded consumer path and controlled
  observation establish the supported subset and its CPU-independent schedule.

### EXE-GOG-SOUND-003 - Sound helper has no loaded BIOS-wait instruction

- **Question:** Does the separately shipped sound helper contain a literal
  `INT 15h` instruction that could select the BIOS wait service and provide a
  direct audio-delay or playback-clock lead?
- **Target:** BLD-GOG-EN-1.1 `SOUND_DS.EXE`, reverified at 204,593 bytes with
  XXH3-128 `236c2dc23c071eca421eb5b427caee57`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportBytePattern` searched every loaded program-memory block
  for the exact two-byte `INT 15h` encoding `cd 15`. The query intentionally
  used a no-analysis import because raw loaded-memory presence is sufficient
  for this bounded opcode question.
- **Bounded finding:** no loaded-memory occurrence exists.
- **Interpretation:** this excludes a literal BIOS-interrupt-15 path in the
  helper's loaded image, including a directly encoded `AH=86h` BIOS wait call.
  It does not exclude other interrupts, port I/O, busy loops, a driver, a
  constructed/runtime-generated path, or code held only in the physical-file
  overlay; it establishes no decoder, device, duration, sample-rate, or
  player-visible audio timing behavior.
- **Confidence:** high for the loaded-image opcode absence; unknown for all
  helper timing and playback semantics.
- **Implementation consequence:** do not reproduce a DOS wait loop or derive
  audio timing from this absence. Retain an explicit CPU-independent monotonic
  schedule until a bounded consumer path and controlled playback observation
  establish one.

### EXE-GOG-SOUND-004 - Sound helper has no loaded `.VOC` filename extension

- **Question:** Does the separately shipped sound helper contain a literal
  `.VOC` extension that could expose a bounded filename-based voice or
  sound-effect loader path?
- **Target:** BLD-GOG-EN-1.1 `SOUND_DS.EXE`, reverified at 204,593 bytes with
  XXH3-128 `236c2dc23c071eca421eb5b427caee57`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1. A full analyzer
  pass completed before this focused query.
- **Method:** `ReportBytePattern` searched every loaded program-memory block
  for the four-byte ASCII sequence `2e 56 4f 43` (`.VOC`). The bounded query
  would report every raw hit and any direct reference to it.
- **Bounded finding:** the loaded helper image has no `.VOC` byte-pattern
  occurrence. Therefore this query yields no filename-extension literal,
  direct reference, file-open path, decoder entry, or playback caller.
- **Interpretation:** this excludes only a raw `.VOC` literal in the loaded
  image. A filename or extension may be constructed, supplied by a caller,
  handled by a driver or overlay, or omitted by a format-agnostic path. The
  absence does not prove the helper cannot consume VOC data and assigns no
  decoder, codec, routing, sample-rate, or timing semantics.
- **Confidence:** high for the exact loaded-image literal absence; unknown for
  every sound-file consumer and playback behavior.
- **Implementation consequence:** retain VOC files as opaque local assets and
  preserve the CPU-independent scheduling requirement. Do not add a decoder,
  filename mapping, or playback behavior without a bounded consumer path and
  controlled observation.

### EXE-GOG-SOUND-005 - Sound helper has no loaded `.adv` module suffix

- **Question:** Do the two padded `.adv` module-identifier envelopes in the
  bounded `SOUND.CFG` file have a direct loaded-image suffix route in the
  separately shipped sound helper that could constrain driver selection?
- **Target:** BLD-GOG-EN-1.1 `SOUND_DS.EXE`, reverified at 204,593 bytes with
  XXH3-128 `236c2dc23c071eca421eb5b427caee57`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1. The full analyzer
  pass completed before both focused queries.
- **Method:** `ReportBytePattern` searched every loaded program-memory block
  for both four-byte ASCII case variants `2e 61 64 76` (`.adv`) and
  `2e 41 44 56` (`.ADV`). Either query would report raw hits and direct
  references. Because the MZ loader reports a 0x13951-byte physical overlay,
  a separate PowerShell 5.1.26100.9444 bounded scan reverified the exact file
  fingerprint, read the complete 204,593-byte physical file, and reported only
  count and offset for those same two patterns; no source bytes were retained.
- **Bounded finding:** neither case-variant occurs in either the loaded helper
  image or the complete physical file, including its overlay. The queries
  therefore yield no literal suffix, direct reference, driver filename
  construction, configuration read, module load, or Preferences path.
- **Interpretation:** this excludes only the two exact raw suffix encodings in
  the helper's complete physical file. A complete module name or suffix can be
  supplied by a caller, constructed dynamically, delegated to a driver, or
  unused. The result neither assigns the two `SOUND.CFG` identifiers a role nor
  establishes helper ownership.
- **Confidence:** high for both exact full-file literal absences; unknown for
  configuration ownership, driver selection, device setup, and all audio
  behavior.
- **Implementation consequence:** retain `SOUND.CFG` as opaque DSOP and keep
  Preferences mutations inert. Do not add a driver-selection reader or map
  settings to audio behavior until an independent bounded consumer path and
  controlled observation establish their relationship.

### EXE-GOG-SOUND-006 - Physical sound-helper overlay has no complete VOC header

- **Question:** Does the physical `SOUND_DS.EXE` file, including the MZ overlay
  omitted from Ghidra's loaded image, contain the complete `Creative Voice File`
  signature that could identify a whole-header VOC validation path?
- **Target:** BLD-GOG-EN-1.1 `SOUND_DS.EXE`, 204,593 bytes, XXH3-128
  `236c2dc23c071eca421eb5b427caee57`.
  The documented MZ overlay is included in this physical-file query.
- **Method:** a bounded PowerShell 5.1.26100.9444 scan read the exact
  204,593-byte physical file and compared every possible start offset with the
  explicit 19-byte ASCII `Creative Voice File` pattern. It reported only the
  file length, pattern length, match count, and offsets; no source bytes were
  retained.
- **Bounded finding:** the complete physical file has zero matches. Together
  with `EXE-GOG-SOUND-002`, neither the loaded image nor the physical overlay
  contains the whole VOC signature.
- **Interpretation:** this excludes a decoder/validator that embeds that exact
  complete header anywhere in this executable. It does not exclude partial or
  bytewise validation, a constructed signature, caller-supplied data, a driver,
  another module, or a nonvalidating playback path. It establishes no codec,
  sample-rate, device, routing, or timing behavior.
- **Confidence:** high for the exact complete-file pattern absence; unknown for
  all VOC consumption and audio semantics.
- **Implementation consequence:** do not promote the standard VOC header
  envelope into a runtime decoder or playback clock. Keep voice assets opaque
  and CPU-independent scheduling explicit until a bounded consumer path and a
  controlled playback observation corroborate a supported subset.

### EXE-GOG-SOUND-007 - Sound helper port I/O has no recovered playback owner

- **Question:** Does the loaded sound helper expose direct port-I/O sequences
  that identify a decoder, device setup, or playback clock for the owned VOC
  files?
- **Target:** BLD-GOG-EN-1.1 `SOUND_DS.EXE`, 204,593 bytes, XXH3-128
  `236c2dc23c071eca421eb5b427caee57`;
  Ghidra 12.1.3, JDK 21.0.12.1, 16-bit real-mode MZ loader, after the default
  analyzer pass.
- **Method:** `ReportImmediatePortIo` tested five explicit 16-bit interface
  values (513, 544, 556, 816, and 904) for `MOV DX, immediate` followed within
  sixteen instructions by `IN`/`OUT DX`. A capped `ReportInstructionText out`
  scan then enumerated every decoded output instruction, and bounded contexts
  and direct-reference reports inspected each resulting helper.
- **Bounded finding:** none of the five requested values participates in the
  tested immediate-DX sequence. The output scan has six sites: one writes zero
  to fixed port `0x43`, then reads fixed port `0x40` twice and complements the
  resulting word; a second helper writes `0x36` to `0x43`, sends the low then
  high byte of a caller-supplied word to `0x40`, and has three direct callers;
  a third helper derives two output ports by adding four and five to a runtime
  word, then writes `0x83` and `0x0b`, with one direct caller. The recovered
  callers and argument sources do not name a VOC file, decoder, device,
  configuration field, gameplay event, or time unit.
- **Interpretation:** the helper contains direct port traffic, but the bounded
  code does not establish why it occurs or connect it to voice playback.
  Immediate-port forms and dynamic-base forms are distinct; the five tested
  values cover neither arbitrary runtime bases nor all possible port patterns.
  The fixed-port writes do not establish a sample clock, duration, rate, or
  a CPU-speed-independent schedule.
- **Confidence:** high for the six decoded output sites, their local operation
  order, the selected immediate-DX absence, and the four direct-caller counts;
  unknown for device ownership, decoder, buffering, playback mapping, timing,
  and every user-visible audio behavior.
- **Implementation consequence:** retain opaque VOC assets and a monotonic
  runtime scheduling policy. Do not emulate port writes, infer a hardware
  backend, use the caller word as a duration/rate, or bind a Preferences
  setting until a consumer path and controlled playback observation establish
  those semantics.

### EXE-GOG-SOUND-008 - Main executable has no complete VOC header signature

- **Question:** Does the main game executable, rather than the separately
  shipped sound helper, embed the complete standard `Creative Voice File`
  header signature that could identify a whole-header VOC decoder or validator?
- **Target:** the documented stable BLD-GOG-EN-1.1 `DSUN.EXE` target,
  634,416 bytes, XXH3-128
  `e296af55ba2ecde7e77f555c90f33d0b`.
- **Method:** a bounded read-only PowerShell scan read the exact physical file,
  including any bytes outside Ghidra's loaded image, and compared every valid
  start offset with the explicit 19-byte ASCII pattern `Creative Voice File`.
  It reported only physical length, pattern length, match count, and offsets;
  it retained or printed no executable bytes.
- **Bounded finding:** the 634,416-byte physical file has zero matches for the
  19-byte signature.
- **Interpretation:** together with `EXE-GOG-SOUND-002` and
  `EXE-GOG-SOUND-006`, neither the main executable nor the sound helper
  contains the complete standard VOC header signature. This excludes only a
  consumer that embeds that exact whole-header literal. It does not exclude a
  partial, bytewise, constructed, delegated, or file-name-independent reader,
  and establishes no codec, sample rate, playback route, or duration unit.
- **Confidence:** high for the exact complete-file absence; unknown for all
  VOC consumption, device ownership, audio mapping, and timing behavior.
- **Implementation consequence:** do not add a standard VOC decoder, derive a
  playback schedule from header data, or emulate original device behavior.
  Keep audio opaque and any future runtime clock monotonic and CPU-independent
  until a bounded consumer path and controlled observation corroborate it.

### EXE-GOG-SOUND-009 - Main executable VOC-extension data is unlinked

- **Question:** Do the `.VOC` filename-extension markers in the main executable
  have a direct reference or instruction context that identifies a native
  voice/sound-effect filename loader?
- **Target:** the documented stable BLD-GOG-EN-1.1 `DSUN.EXE` target,
  634,416 bytes, XXH3-128
  `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, JDK 21.0.12.1, 16-bit real-mode MZ loader, after its default
  analyzer pass.
- **Method:** a complete physical-file scan first found five exact four-byte
  ASCII `.VOC` occurrences. `ReportBytePattern` then searched loaded memory
  for `2e 56 4f 43`, reporting each match and direct inbound reference.
  `ReportInstructionContext` separately classified every resulting virtual
  address without reading adjacent payload bytes.
- **Bounded finding:** the five loaded matches are at `5000:8d7e`,
  `5000:8d92`, `5000:8d9f`, `5000:8db5`, and `5000:96aa`, all in `CODE_208`.
  Ghidra reports no direct inbound reference for any address, and none is
  contained by a decoded instruction.
- **Interpretation:** the program carries five raw extension fragments, but
  the focused analysis supplies no direct filename construction, file open,
  resource lookup, decoder, playback caller, or duration conversion. The
  strings may be reached indirectly, copied at runtime, or be unrelated data;
  they do not establish an audio path.
- **Confidence:** high for the five raw matches and the no-reference/
  non-instruction classifications; unknown for filename ownership, audio
  routing, decoding, device behavior, and timing.
- **Implementation consequence:** do not map numbered VOC files to events or
  add a filename-derived decoder/scheduler. Keep any future playback clock
  monotonic and CPU-independent until an independent consumer path and
  controlled observation agree.

### EXE-GOG-SOUND-010 - Main executable has no literal sound-helper filename

- **Question:** Does the complete main executable directly name the shipped
  `SOUND_DS.EXE` helper, providing a bounded launcher or audio-ownership lead?
- **Target:** the documented stable BLD-GOG-EN-1.1 `DSUN.EXE` target,
  634,416 bytes, XXH3-128
  `e296af55ba2ecde7e77f555c90f33d0b`.
- **Method:** a bounded read-only PowerShell scan compared every valid offset
  in the complete physical file, including any bytes outside the loaded MZ
  image, with the exact twelve-byte ASCII `SOUND_DS.EXE` pattern. It reported
  only file length, pattern length, match count, and offsets; no executable
  bytes were printed or retained.
- **Bounded finding:** the 634,416-byte file has zero exact matches.
- **Interpretation:** this excludes only a literal complete helper filename in
  the main executable. The helper might still be launched through a constructed
  name, a shorter identifier, a batch/launcher layer, indirect state, or no
  direct launch path at all. It establishes no audio ownership, configuration,
  device, decoding, or timing contract.
- **Confidence:** high for the exact whole-file literal absence; unknown for
  helper invocation and all audio behavior.
- **Implementation consequence:** do not model `SOUND_DS.EXE` as a main-game
  child process or map it to Preferences/audio events. Preserve opaque assets
  and a CPU-independent runtime policy pending a traceable consumer path and
  controlled observation.

### EXE-GOG-SOUND-011 - No explicit literal DOS EXEC service lead

- **Question:** Does the main executable contain an explicit DOS `INT 21h`
  call with a nearby literal `AH=4Bh` setup for the EXEC service, which could
  provide a direct helper-launch path even without a literal helper filename?
- **Target:** the documented stable BLD-GOG-EN-1.1 `DSUN.EXE` target,
  634,416 bytes, XXH3-128
  `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, JDK 21.0.12.1, 16-bit real-mode MZ loader, after its default
  analyzer pass.
- **Method:** `ReportDosInt21Services` scanned every decoded `INT 21h`
  instruction for a literal `MOV AH, 0x4b` within the preceding twelve
  instructions in the same containing function. The bounded script reports
  matching interrupt and setup addresses only.
- **Bounded finding:** no explicit `INT 21h` instruction matched the requested
  nearby literal setup.
- **Interpretation:** this rejects only the specific decoded EXEC pattern. It
  does not exclude an EXEC call whose service value is loaded indirectly, set
  farther away, supplied through a wrapper, reached through another executable,
  or unavailable in the analyzed path. It establishes no helper invocation or
  audio ownership.
- **Confidence:** high for the bounded decoded-pattern absence; unknown for
  every broader process-launch and audio behavior question.
- **Implementation consequence:** do not implement or emulate a child-process
  audio model. Continue to require a traceable consumer path and controlled
  observation before mapping audio behavior or timing.

### EXE-GOG-UI-011 - Start-window button IDs have no direct handler operands

- **Question:** Do the four serialized start-window controls identify a direct
  executable dispatch path that can establish their native activation or
  transition behavior?
- **Target:** the documented stable BLD-GOG-EN-1.1 `DSUN.EXE` target,
  634,416 bytes, XXH3-128
  `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, JDK 21.0.12.1, 16-bit real-mode MZ loader. A disposable
  project completed Ghidra's default analysis before the focused scan.
- **Method:** `ReportScalarConstants` scanned every decoded instruction operand
  for the four unsigned decimal `BUTN` identities from `WIND` #19500: 19300
  (START GAME), 19301 (CREATE CHARACTERS), 19302 (LOAD SAVED GAME), and 19303
  (EXIT TO DOS). The reusable report caps results at 256.
- **Bounded finding:** no requested scalar occurs as a decoded instruction
  operand. The scan therefore identifies no direct immediate-ID comparison,
  registration, callback, dispatch, or screen-transition path for any of the
  four controls.
- **Interpretation:** this rejects only a simple direct-operand handler model.
  It does not make the controls inactive and does not contradict their bounded
  icon labels or the manual-described start flow. Resource-derived controls,
  runtime callbacks, resident state, computed identifiers, indirect dispatch,
  and another module remain possible.
- **Confidence:** high for the four bounded absent operand results; unknown for
  native event delivery, focus, activation edge, transition order, timing, and
  the supplied-party loader.
- **Implementation consequence:** retain the independently evidenced semantic
  start choices and deterministic routing, but do not use a direct native
  handler claim to infer focus, frame-state, callback, or transition behavior.
  `ShippedPartyUnresolved` remains required until the four source records are
  independently identified.

For each useful finding, add a concise entry here or in the relevant
`docs/RULES-AND-EVIDENCE.md` / `docs/ORIGINAL-FORMATS.md` section with:

- finding ID and the exact question asked;
- executable edition, length, and XXH3-128;
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
