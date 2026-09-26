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

### EXE-GOG-SMALLTAG-001 - No direct literal loaders for PLYL and CSEQ

- **Question:** Do the small opaque `PLYL` or `CSEQ` GFF families supply a
  direct static source lead for selectors or gameplay rules?
- **Target:** BLD-GOG-EN-1.1 `DSUN.EXE`, reverified at 634,416 bytes with
  XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`;
  Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1.
- **Method:** `ReportBytePattern` searched all loaded executable blocks for
  the exact four-byte ASCII encodings of `PLYL` and `CSEQ`. The bounded
  resource inventory identifies six and one owned records respectively; no
  payload bytes were retained.
- **Bounded finding:** neither pattern occurs in the loaded image. The search
  did not cover the `FBOV` overlay pack after the load image (FMT-EXE-001),
  where the `GREQ` and `CACT` tags that the same search missed do occur
  (FND-SAVE-003). A later byte search of the whole `DSUN.EXE` file, overlays
  included, finds neither tag either (SCRIPT migration batch).
- **Interpretation:** the result excludes only a literal-tag source path in
  the resident image. It does not rule out overlay code, constructed tags,
  indirect/resource manager lookups, another module, or runtime-propagated
  data, and it assigns no meaning to any payload.
- **Confidence:** high for the two literal absences in the loaded image;
  unknown for every family's loader, format, ownership, and player-visible
  behavior.
- **Implementation consequence:** retain both families as DSOP. Do not add
  readers or infer interaction or combat behavior from this negative result.

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
