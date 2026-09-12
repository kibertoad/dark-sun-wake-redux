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
