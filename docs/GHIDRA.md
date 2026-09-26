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
