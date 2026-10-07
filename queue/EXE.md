# EXE

Next ID: Q-EXE-007

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

- Q-EXE-004. FMT-EXE-006: Which batch helpers does the game, setup or
  distribution wrapper actually launch? Settles it: direct launch references
  traced through their selectors and inputs. Tried: complete seven-file
  command-text reading (FND-EXE-008), which describes targets but not callers.
  Split from Q-EXE-002. Blocks: complete caller coverage.

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
  cannot decide how the shell interprets those tokens. Blocks: complete shell
  outcome description.

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
