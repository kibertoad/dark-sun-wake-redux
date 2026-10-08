# Ghidra setup for clean-room analysis

Ghidra is a recommended research tool for questions that the manual,
walkthrough, controlled play, and bounded data inspection cannot answer exactly.
Use it only against the repository owner's legally owned local executable.
Ghidra projects, binaries, byte dumps, screenshots, and full disassembly or
decompiler output must never be added to Git.

Use the documented executable identity check against its build entry before
interpreting addresses, retaining the stable-path fingerprint policy below.
Another version of the executable is another build, with its own
`spec/builds/` entry, and a finding lists it only when it was checked there
too, with a location in each build. Addresses are written in the
[notation](../vendor/upstream/documentation-standard.md#notation) (lines 381-420) for the
executable's format: the full virtual address at the header's image base for
PE, and `segment:offset` for MZ, COM, and NE, with the load segment the
standard fixes for each.

This workflow follows the established practice in
`C:\sources\rechaos-overlords`: establish and document the executable
fingerprint once for its stable approved path, keep a
disposable local analysis project, answer narrow questions with bounded scripts,
record address-level factual findings in `spec/findings/`, then implement the
behavior independently with synthetic tests.

## Installed toolchain

The current research machine has:

| Tool | Version | Location |
|---|---|---|
| Ghidra | 12.1.3 | `C:\Users\kiber\AppData\Local\Programs\Ghidra\ghidra_12.1.3_PUBLIC` |
| Eclipse Temurin JDK | 25.0.4.1 | `C:\Program Files\Eclipse Adoptium\jdk-25.0.4.101-hotspot` |

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
$wakeJavaHome = 'C:\Program Files\Eclipse Adoptium\jdk-25.0.4.101-hotspot'
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
  -scriptPath ('"' + (& ./tools/Get-GhidraScriptPath.ps1) + '"') `
  -postScript ReportFunctionSummary.java 0x00000000
```

The extra literal quotes keep the semicolon-separated script directories in one
argument through the Windows batch launcher. Without them, the second directory
can be rejected by Ghidra as a separate argument, even under PowerShell 7.

The address above is intentionally a placeholder example, not a finding. Replace
it only with an address selected through an evidence-led query.

## Function inventory

For the Survey inventory of `BLD-GOG-EN-1.1`, run
`ExportFunctionInventory.java` once on the original `DSUN.EXE` import and once
on a separate import of the local-only image made by
`New-FbovMappedImage.ps1`. Give each run a distinct temporary TSV path. Then
run `Join-FunctionInventory.ps1` with those paths, the original executable as
`-SourcePath`, and `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv` as `-OutputPath`.
The join takes resident functions from the original import and overlay-code
functions from the mapped import. Their auto-analysis results differ, so the
mapped import does not replace the original resident inventory. Starts in the
committed file are offsets in the shipped `DSUN.EXE`, written as
`DSUN.EXE+0x...`; sizes are Ghidra function-body byte counts. Ghidra's
discovery does not prove that every original function was found. The inventory
contains no code, bytes, strings or auto-generated names.

Known entries added outside that export use measured body-byte counts from
the published bounded reporter, with its partial dispatch and continuation
assumptions retained in local reports. Body-byte counts are not end addresses:
query regions use independently documented source spans. Never replace a body
count with its span length merely to extend a search domain.

The disc's distinct `CD:DSUN.EXE` also has an inventory, at
`coverage/BLD-GOG-EN-1.1/CD/DSUN.EXE.tsv`. The `CD:` manifest prefix becomes
the `CD/` directory because Windows cannot use a colon in a filename. Extract
the file from track 1 of `game.gog` into a local-only path, verify its 634,704
bytes and XXH3-128 `318cd5ec0559901add3780097162a919`, then run the same
original and mapped imports. Pass `-ManifestPath 'CD:DSUN.EXE'` to
`Join-FunctionInventory.ps1` so the start addresses retain the manifest path.
Ghidra 12.1.3 found 1,283 resident starts in the original import and 858
overlay-code starts in the mapped import, for 2,141 rows.

The `SOUND_DS.EXE.tsv`, `SVIEW.EXE.tsv` and `PATCH.EXE.tsv` files under
`coverage/BLD-GOG-EN-1.1/` come from direct MZ imports of the installed
helpers. `CHARTRAN.EXE.tsv` comes from an import of the local-only unpacked
helper whose XXH3-128 is
`a2804715759141397dca547934213843` (FND-PARTY-010). These use Ghidra's
`segment:offset` function starts with the load image at segment `0x1000`.
`CHARTRAN.EXE` addresses refer to the unpacked image, not offsets in the
shipped compressed file. Each inventory keeps only starts inside its MZ load
image and the function-body byte counts Ghidra reported.

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

Static findings made with Ghidra go into `spec/findings/` as `method: static` findings,
with the Ghidra version in their `tool` field.

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

## Adopted bounded evidence tools

See [EVIDENCE-TOOLS](EVIDENCE-TOOLS.md) for the shared Node operand, incoming-call,
flow, table and inventory commands, and [EVIDENCE-REVIEW](EVIDENCE-REVIEW.md) for
claim-dependent checks. `ExportBoundedFlow.java` records local flow metadata;
`ExportFunctionInventory.java` now publishes its output only after traversal
completes. Reports remain local; synthetic tests run in `tools/Test.ps1`.

Keep `Join-FunctionInventory.ps1` for the established DSUN mapped-image pipeline:
it converts that import's overlay coordinates and filters mapped-view starts.
The shared join instead accepts resident segmented coordinates or already
canonical file offsets, then enforces explicit view ownership. Existing committed
inventories keep their documented paths, including `CD/DSUN.EXE.tsv`; new shared
exports use the collision-resistant `@CD/` encoding. Never replace a historical
inventory or mapping merely because a new tool is available.


## Large memory maps and JVM diagnostics

ReportMemoryBlocks.java accepts `page <zero-based-start> <count>` (count 1..512) or `name <exact-name>`. Page ordering is the current program's block order; repeat queries against the same unchanged analysis. The header reports total blocks, starting index, emitted count and whether the selection is partial; a page header also reports the requested count, so a page ending past the map is visibly clipped. Start equal to total emits an empty page. Whole-map requests above 512 fail rather than silently truncate. Exact names reject missing blocks, and ambiguous names list the matching indices to page to. Reports stay in GAME_DIR; no code or bytes are emitted.

JVM fatal-error/replay logs and heap dumps (`*.hprof`) are ignored and rejected by repository policy (`deniedFileNamePatterns` in `tools/repository-policy.json`) even when force-staged, at any directory depth. Start analysis with `-XX:ErrorFile=<local-only-dir>/hs_err_pid%p.log`, `-XX:ReplayDataFile=<local-only-dir>/replay_pid%p.log` and, when heap dumps are enabled, `-XX:HeapDumpPath=<local-only-dir>` in the analysis JVM options (`JAVA_TOOL_OPTIONS` for that invocation); choose GAME_DIR/analysis or a unique temporary directory, never a tracked output. Logs do not prove task ownership or a live process; inspect PID, command line and task provenance before any cleanup.

## FBOV analysis mapper relocation revision 2

The mapper now emits `MapperContractRevision: 2`. Earlier revisions added
trampoline and code-fixup relocation pairs in segment/offset order while the
MZ writer emitted offset/segment order. An independent synthetic MZ-reader
regression fails before the correction and passes after it, retaining original
relocation sites and resolving both rewritten segment words to their targets.

Regenerate analysis derivatives at new GAME_DIR paths and import fresh projects;
retain old images, snapshots and reports for comparison. Do not carry an old
snapshot's discovery or computed-target ownership into the corrected denominator
without re-export and source-mapping review. Original-content sources are unchanged.
Mapped-image findings need dependency review before they support native behavior;
this tooling fix alone does not supersede every historical observation or establish
a complete reading. The mapper output and analysis derivatives are local-only.

## Segment-register move rendering in Ghidra 12.1.3

Ghidra 12.1.3 renders the register forms of x86 opcode `8E /r` with reversed
operands. The installed processor constructors retain the correct assignment
semantics while their display templates put the general register first. This is
tracked upstream in [Ghidra issue 9739](https://github.com/NationalSecurityAgency/ghidra/issues/9739)
and the earlier [real-mode report 9635](https://github.com/NationalSecurityAgency/ghidra/issues/9635).

Instruction-window and instruction-text reporters use Ghidra's rendered listing.
Do not infer segment producers or absence of segment writes from that text alone.
Check bounded original bytes and instruction semantics through the pinned reader
and engine. Keep original-source and loaded-relocation values distinct. Operand
index consumers also need review; a display error is not evidence that p-code
has the opposite assignment. Historical readings require individual dependency
review, rather than blanket supersession or automatic status promotion.

## Citation endpoint controls

`ReportCitationBoundaries.java` takes 1..32 `start..end` queries, optionally
suffixed `:return`. End is exclusive. It checks only the start instruction
and the instruction containing end-minus-one, independently of function
ownership. Interior endpoints, mapped bytes with no decoded instruction and
unmapped bytes remain distinct. `:return` additionally checks whether the
last included decoded instruction is x86 RET/RETF; it catches an aligned span
that stops before the claimed return. It does not infer that every citation
must end in a return. An undecoded endpoint produces unknown return status.

Run it read-only with analysis disabled against the saved source project.
The output contains only addresses and classifications. A completed diagnostic
does not validate interior coverage, source mapping, reachability, callers,
aliases or runtime state, and does not create complete-reading declarations.
An invalid or unrepresentable range fails before any query result is emitted.

Synthetic integration: import a local 16-byte `citation-boundaries-synthetic.bin`
with a NOP, a six-byte conditional branch to the following one-byte RET, and
eight trailing zero bytes, using BinaryLoader at base `0x1000` and processor
`x86:LE:32:default`, with analysis disabled. Run `TestCitationBoundaries.java`
in that separate disposable project; it admits only the named fixture with
the expected prefix and disassembles only its eight-byte control interval.
Never run the synthetic harness against licensed sources. The reporter itself
does not disassemble or mutate a program.

`tools/ghidra/Test-CitationBoundaries.ps1 -GhidraHome <installation> -JavaHome
<jdk>` automates fixture creation and valid/rejected queries under a fresh
directory in `artifacts/`. Use PowerShell 7. It checks completion markers and
expected classifications because a headless exit code alone can hide a script
failure. The fixture and logs remain local; only the synthetic project is
deleted by headless. This optional installed-Ghidra check needs no GAME_DIR.
