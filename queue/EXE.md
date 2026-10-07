# EXE

Next ID: Q-EXE-010

## Static

- Q-EXE-001. FMT-EXE-001, FMT-EXE-002, FMT-EXE-003, FMT-EXE-004,
  FMT-EXE-005: How does the overlay manager load an overlay? Which code
  reads the pack header and the segment table, what do the descriptor words
  `unk_02` and `unk_06` and the `flags` values 0, 1 and 4 mean, what does the
  `INT 3Fh` trap do with a trampoline's `target`, what fills the overlay
  header's zero fields at run time, and how are the fixups applied? Settles
  it: the `INT 3Fh` handler that the startup code installs, read through its
  file reads, the header fields it writes and the fixup loop. Tried: the only
  resident routine that calls both literal DOS seek and read wrappers
  (FND-EXE-007), which reads signature-and-length records and takes no pack
  input. Blocks: none.

- Q-EXE-005. FMT-EXE-006: Which code page interprets the disc sound helper's
  display bytes? Settles it: direct interpreter configuration or code-page/font
  selection evidence that distinguishes compatible decoders. Tried: all bytes
  profiled (FND-EXE-008); the high-byte set does not select a unique code page.
  Split from Q-EXE-002. Blocks: original display encoding identification.

- Q-EXE-006. FMT-EXE-006: How does the supported interpreter handle the
  undefined jump labels and colon-suffixed target in the disc sound helper?
  Settles it: static reading of the actual interpreter's label matching and
  error paths, with an owner observation only for environment-dependent
  behavior. Tried: complete label and jump reading (FND-EXE-009); the file
  cannot decide how the shell interprets those tokens. New reading:
  SRC-DOSBOX-GOG-0742 predicts literal trailing-colon matching and deletion
  of the active batch on a failed label search. Next static step: establish
  the relevant source-to-shipped-binary correspondence. FND-EXE-011 and
  FND-EXE-012 now locate compiled token normalization, search and conditional
  normal cleanup. FND-EXE-013 now reads the command-record consumer and its
  two-word call target. Next: trace batch-line production and the parser
  helper outputs into this input path, then external file helpers and
  exceptional cleanup. These partial direct readings do not establish the
  complete shell outcome.
  Blocks: complete shell outcome description.

- Q-EXE-007. FMT-EXE-006: Do the shipped game or sound-setup executables
  launch any batch helpers? Settles it: direct executable launch references
  traced through selectors and arguments, including computed command names.
  Tried: complete helper contents (FND-EXE-008) and distribution-wrapper
  reading (FND-EXE-010); neither locates an executable caller. Split from
  Q-EXE-004. Blocks: complete game/setup caller coverage.

- Q-EXE-008. FMT-EXE-006: Which disc helpers does the disc installer launch?
  Settles it: direct installer launch references and their selection inputs.
  Tried: helper contents and GOG wrapper (FND-EXE-008, FND-EXE-010), which
  do not cover the disc installer. Split from Q-EXE-004. Blocks: complete
  disc-installer caller coverage.

- Q-EXE-009. FMT-EXE-006: How does the shipped interpreter resolve the bare
  ravager/sound commands and continue after them in the declared GOG wrapper?
  Settles it: interpreter command-search, batch-chaining and EXIT code read
  under the declared working directory, mounts and configuration order;
  keep mutable overlay substitutions conditional. Tried: primary task and
  complete wrapper text (FND-EXE-010), which cannot decide shell behavior.
  New reading: SRC-DOSBOX-GOG-0742 predicts COM/EXE/BAT search and replacement
  of the wrapper by a bare batch command, preserving it only for CALL. Next
  static step: verify the relevant compiled routines and mount/file lookup
  inputs; the bundled source alone is not a binary correspondence proof.
  FND-EXE-013 and FND-EXE-014 now record compiled external-command dispatch,
  the conditional batch cleanup gate and CALL flag writes. Next: read the
  lookup helper and extension selection, all flag writers and EXIT paths,
  then the declared mount/overlay inputs. FND-EXE-015 now records compiled
  local/PATH candidate order and its unread drive-predicate boundary;
  FND-EXE-016 records the distinct count-80 PATH scan. Next: trace the
  remaining normalization/index provenance and drive-object targets, PATH input writers
  and long-segment admission, then remaining flag/EXIT and mount inputs.
  FND-EXE-017 now records the initial selector stores, unsigned range and
  nonnull table guards, and failure output mutation. Next: identify the
  initial selector writers and later normalization outputs before attributing
  a complete bound to the caller's consumed index. FND-EXE-018 now reads
  two direct selector writers, their guard-order difference and a bounded
  external-command caller. Next: identify its imported conversion and input
  gates, remaining aliases/initialization and subordinate virtual targets;
  do not equate a truthy setter return with an update. FND-EXE-019 now
  identifies exact drive-command suffix gates and linked CRT imports. Next:
  finish the filename helper's later output/alias and drive-object path,
  then initial selector provenance, PATH admission and remaining EXIT/flag
  paths. External CRT locale behavior stays conditional where relevant.
  FND-EXE-020 now reads the filename byte scan and output-prefix setup.
  Next: read component transformations and their failure/output paths, then
  the object prefix's producers and drive-object virtual targets. A bounded
  local scan is not yet a complete caller-output contract.
  Blocks: resolved wrapper-helper and continuation description.

## Emulated call

None.

## Agent run

None.

## Live session

None.

## Source

None.

## Blocked

None.
