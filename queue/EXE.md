# EXE

Next ID: Q-EXE-004

## Static

- Q-EXE-003. BLD-GOG-EN-1.1: Reconcile a repeatable complete listing of the
  installed source and its disc with the manifest and Other files. Settles
  it: every path belongs to the manifest or an exact exclusion with a reason,
  and the build records the listing procedure and scope. Tried: the existing
  manifest includes installed game data and disc files, but Other files lists
  some wrapper/installer paths by patterns rather than a complete listing.
  Blocks: the refreshed Survey file-denominator exit.

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

- Q-EXE-002. FMT-EXE-006: What commands and encoding do the installed and disc `.BAT` files
  contain, and which ones does the game or setup invoke? Settles it: bounded readings of the seven
  files and of references that launch them. Blocks: Survey format coverage.

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
