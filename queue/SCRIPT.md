# SCRIPT

Next ID: Q-SCRIPT-003

## Static

- Q-SCRIPT-001. (entries pending migration): What, if anything, reads the 329
  fixed-width records of GPLDATA.GFF GPLI #1, and what do their fields mean?
  Settles it: the code that loads the GPLI resource. Tried: a literal tag
  search and a search for a function holding both the GPL tag and resource
  number 135, neither of which finds a reader. Blocks: slices 2-4.
- Q-SCRIPT-002. (entries pending migration): Which source fills the linked
  13-byte runtime selector records that request GPL scripts, and which game
  features own their predicate shapes? Settles it: the code that writes the
  selector table's pointer and fills its records. Tried: CSEQ #1000, the
  separate 19-byte linked table, and the shared SCMD and RDFF record path,
  none of which is the source. Blocks: slices 3-6.

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
