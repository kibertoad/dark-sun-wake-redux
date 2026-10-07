---
id: FMT-EXE-006
title: BAT launch files
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["RAVAGER.BAT", "SOUND.BAT", "CD:*.BAT"]
byte_order: null
size: null
text: true
definition: null
evidence: [FND-EXE-008, FND-EXE-009, FND-EXE-010]
conflicting: []
split_with: []
related: []
---

## Layout

| Key | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|
| line | byte text | command_text | A command, label, comment or blank line terminated by CRLF | supported | FND-EXE-008 |
| EOF | file boundary | end | Immediately after the final CRLF, without a byte-26 marker | supported | FND-EXE-008 |


The seven manifest-listed files are CRLF-delimited command text without NUL
or DOS end-of-file marker bytes. Six contain only ASCII bytes. Disc
`SOUND.BAT` additionally uses bytes 185, 186, 187, 188, 200, 201, 204 and
205 in display commands; their intended code page remains unknown.

Installed `RAVAGER.BAT` names `DSUN` with options `-W0` and `-L`, then exits
the shell. Installed `SOUND.BAT` suppresses command echo, displays a notice,
copies `D:\*.ADV` into the current directory, names `SOUND_DS`, deletes
current-directory `*.ADV`, and exits. Copy and delete output goes to NUL;
there are no conditional result checks or remembered copied-file list.

Disc `AA.BAT`, `AC.BAT`, `CA.BAT` and `CC.BAT` display notices and pause
before naming `C:\DOS\EDIT` with, respectively, `A:\AUTOEXEC.BAT`,
`A:\CONFIG.SYS`, `C:\AUTOEXEC.BAT` and `C:\CONFIG.SYS`.

Disc `SOUND.BAT` selects its ULTRADIR path when that variable is nonempty;
otherwise it tests ARIA. With neither set, it names `SOUND_DS` without
installing a TSR. The ULTRADIR path tests MIDI patch-file presence and
selects `UM200.ini`, `UM206A.ini` or `UM206.ini` for copying to `ssi1.ini`.
The tests, ERRORLEVEL thresholds and ordering are recorded in FND-EXE-008.
The 200 and 206A success paths jump to the undefined `G_SUCCESS` label.
The 206 path reaches the Ultramid load command; its successful fall-through
names `SOUND_DS`, then unloads Ultramid. Error-message paths reach the
shared cleanup that clears ULTRASND and BLASTER and then selects non-TSR
setup. ARIA selects the GM2 bank load command and a threshold-1 branch
whose target token is `INS_GM1:` while the label is `INS_GM1`. Its ordinary
fall-through names `SOUND_DS` and unloads that TSR. The GM1 label loads
the smaller bank then jumps to undefined `RUN_ARIA`. These describe the
shipped command text, not an observed execution or shell error outcome.

The distribution's declared primary task selects a DOSBox configuration
with separate game and setup branches (FND-EXE-010). Its menu admits choices
123 and tests ERRORLEVEL thresholds 3, 2 and 1 in that order, selecting exit,
setup and game labels respectively. After declaring installation and overlay
mounts on C and the disc mount on D, and changing to C, its game label names
bare `ravager`; its setup label names bare `sound`. Those names match the
installed helper stems. The text after them jumps to exit and launcher,
respectively, but resolution and actual continuation remain open. The named
installed helpers themselves both end with EXIT.

## Enumerations and flags

No binary enumeration or byte-order field applies to this text listing.
The meanings of executable command-line options are outside this entry.

## Differences between builds

None known. Installed and disc sound scripts differ within this build.

## Coverage

Complete command-text reads of `RAVAGER.BAT`, `SOUND.BAT`, and the five
`CD:` `.BAT` files of BLD-GOG-EN-1.1 (FND-EXE-008, FND-EXE-009). No complete
reading of callers, external commands or interpreter behavior is claimed.

## Open questions

- Does the shipped game or sound-setup executable launch any batch helpers
  (Q-EXE-007)? Automatic invocation and manual-only use both remain possible.
  FND-EXE-008 describes helper contents; FND-EXE-010 establishes wrapper
  command branches, not executable callers. Direct executable launch
  references traced through their inputs settle it.
- Which disc helpers does the disc installer select (Q-EXE-008)? Selection
  by its installer and standalone manual use are competing readings. The
  file roles in FND-EXE-008 support neither caller claim. Direct installer
  launch references and selection inputs settle it.
- How does the shipped wrapper resolve the two bare helper commands and
  continue after them (Q-EXE-009)? Resolution to installed batch files is
  consistent with the declared mounts, but command-search order, batch
  chaining and EXIT handling may affect that result and the textual return
  path (FND-EXE-010). Reading the shipped interpreter's resolution and
  continuation code under the declared launch inputs settles the static
  behavior; mutable overlay substitutions remain conditional. The bundled
  source predicts replacement of the wrapper by a bare batch command, not
  return to it, because only CALL retains the active batch
  (SRC-DOSBOX-GOG-0742). The competing return reading remains unsupported;
  FND-EXE-013 and FND-EXE-014 now provide compiled dispatch and a conditional
  batch-cleanup distinction, including the CALL flag interval. Command/mount
  inputs, other flag writers and exceptional/EXIT paths must still decide
  the complete continuation. FND-EXE-015 now records supplied-name, COM,
  EXE and BAT ordering before admitted PATH candidates, conditional on
  normal helpers and a valid input. Its availability predicate crosses an
  unread normalization/drive-object boundary, so installed-BAT resolution
  remains conditional. FND-EXE-016 separately identifies the count-80 PATH
  scan that can advance past NUL; supported-input reachability and subsequent
  effects remain unknown. FND-EXE-017 records initial selector production
  and guarded table access, without settling later normalization, initial
  selector writers or drive-object behavior. FND-EXE-018 reads two direct
  selector writers and their differing success/update predicates; caller
  admission, indirect writers and later virtual effects remain unknown.
  FND-EXE-019 identifies the exact drive-command suffix gates and CRT import
  slots; unchanged bare helper tokens take the local filename-lookup branch,
  conditional on their delivery. FND-EXE-020 records the filename helper's
  byte scan and prefix setup, separating consumed and emitted bounds.
  FND-EXE-021 now reads component transformations, return/failure branches
  and retained output mutations. Prefix provenance, caller contracts and
  complete resolution remain open. FND-EXE-022 records two pointer-installation
  paths and one direct prefix transfer; constructors, concrete virtual targets
  and storage/alias bounds remain unread. FND-EXE-023 records a direct record
  append and its conditional growth path; allocation contracts, object
  construction and caller invariants still need evidence. FND-EXE-024
  identifies allocation/release imports and local retry/return boundaries;
  callback state, exceptional effects and caller inputs remain conditional.
  FND-EXE-025 reads the separate failure-object prefix and bitmap fallback,
  without establishing its lifetime or final exceptional outcome.
  FND-EXE-026 links a bounded caller loop to append and initialization;
  earlier selector admission, list/object producers and aliases remain unread.
  FND-EXE-027 traces one earlier list producer and construction-call inputs;
  construction effects, status meanings and dispatch targets remain open.
  FND-EXE-028 resolves the bounded dispatch targets and shared output reread;
  helper effects, aliases and final continuations remain conditional.
  FND-EXE-029 reads the first shared helper as a sentinel-linked search;
  comparison semantics, collection producers and returned-word lifetime remain open.
  FND-EXE-030 reads stored-length and unsigned-byte comparison operations;
  valid storage, producer contracts and downstream effects remain conditional.
  FND-EXE-031 traces one collection producer and direct link-write order;
  field helpers, ownership, initialization and cleanup remain unread.
  FND-EXE-032 reads signed field-publication branches; preceding-word
  producers, auxiliary effects and ownership remain unresolved.
  FND-EXE-033 reads the add-one auxiliary and a distinct previous-value
  exchange-add return; ownership and caller cleanup contracts remain open.
  FND-EXE-034 traces negative-path payload copying and length publication;
  FND-EXE-035 traces capacity rounding and three-word prefix initialization;
  FND-EXE-036 traces the bounded capacity-limit construction/publication path;
  FND-EXE-037 traces its object first-word and payload-field publication order;
  FND-EXE-038 traces temporary input/end production and range-copy branches;
  FND-EXE-039 traces the bounded null-input construction and failure route;
  FND-EXE-040 traces their conditional raw-prefix release boundary;
  FND-EXE-041 traces a mutable final target and its normal-return import boundary;
  FND-EXE-042 traces the bounded encoded shared-record reader;
  FND-EXE-043 traces record selection, initialization and shared-pointer publication;
  FND-EXE-044 bounds the shipped copied tail and local name terminators;
  FND-EXE-045 traces lazy initialization and mode-dependent record publication;
  FND-EXE-046 traces mode admission, flag publication and the bounded wait loop;
  FND-EXE-047 traces imported resource and local-helper mode publication;
  FND-EXE-048 resolves record/wait imports and the zero-result tail return;
  FND-EXE-049 traces saved-link cleanup and its freshly selected mode;
  FND-EXE-050 bounds setup/cleanup return handling in construction callers;
  FND-EXE-051 traces recovered stored-handler prefixes and forwarding branches;
  FND-EXE-052 traces selected-record publication and saved-state indirect transfer;
  FND-EXE-053 traces register-input selection and mutable-local link traversal;
  FND-EXE-054 traces the second selector and its two distinct callback result gates;
  FND-EXE-055 traces nested callback record writers and early-exit status reloads;
  FND-EXE-056 bounds callback selected-record access and full-width field stores;
  FND-EXE-057 traces signature-selected saved-state reads and seven-return preparation;
  FND-EXE-058 traces ordinary signed admission, helper iteration and six-return stores;
  FND-EXE-059 bounds the matching byte reader and terminating full-word outputs;
  FND-EXE-060 traces metadata marker branches, cursor returns and relative targets;
  allocation units, capacity, aliases and lifetime remain conditional.
- Which code page interprets the disc sound display bytes (Q-EXE-005)?
  Several OEM code pages may agree on those byte values; ASCII-only helpers
  are compatible with multiple encodings too. FND-EXE-008 identifies the
  bytes, not an exclusive decoder. Interpreter configuration or font/code
  page selection evidence settles it.
- How does the supported interpreter resolve missing labels and the
  colon-suffixed target (Q-EXE-006)? FND-EXE-009 rules out hidden definitions
  in this file, but not interpreter normalization or error handling. A
  static reading of the actual interpreter's label handling settles its
  behavior; any environment-dependent outcome needs an owner observation.
  The bundled source retains a target's trailing colon and deletes the
  active batch after a failed label search (SRC-DOSBOX-GOG-0742), predicting
  failures for all three target spellings. Normalization or continued-batch
  readings would require contrary shipped-interpreter evidence; source
  correspondence is not established. FND-EXE-011 now directly records a
  compiled target-token path that retains the trailing colon. FND-EXE-012
  records its search callee and conditional normal-path cleanup restoration.
  FND-EXE-013 additionally reads the compiled command-record consumer and
  both words of its call target. These bounded findings support the source
  reading without settling batch-line production, external file operations
  or exceptional paths.
