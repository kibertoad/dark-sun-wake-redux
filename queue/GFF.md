# GFF

Next ID: Q-GFF-003

## Static

- Q-GFF-001. FMT-GFF-001: Which resource tags in the shipped GFF files have no
  bounded layout yet, and what reads each of them? Settles it: one format
  entry per remaining tag, from the code that loads it. Blocks: later logic
  slices.
- Q-GFF-002. FMT-GFF-001, FMT-GFF-002, FMT-GFF-003: How does `DSUN.EXE` open a
  GFF file and find a resource by tag and number, and which header and
  directory fields does it read or write? Settles it: the code that reads the
  header, walks the tag tables and the `GFFI` index, and saves a file, showing
  what `unk_14` and `unk_18` hold, whether the gap list and the directory's
  prefix values are read or written, whether the bytes after the directory are
  ever read, and which file it searches for a tag. Blocks: none.

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
