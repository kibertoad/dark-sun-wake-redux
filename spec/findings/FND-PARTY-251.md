---
id: FND-PARTY-251
title: Door initialization chooses its presentation from bits 0 and 1 and the shared helper sets or clears GMAP mask 0xC0 over a strip
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069118..0x000692CC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069815..0x000698D6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 25AF:0057..25AF:00BB
tool: tools/research/exec-census/overlay_listing.py and resident_listing.py at commit 3334937c; Python 3.14.7, Capstone 5.0.7
environment: null
---

## Observation

This follows the remaining overlay-182 read of item word +0x10 and
the helper called by both it and FND-PARTY-250's setter. All observations
are static; no original code was executed. FND-EXPLORE-001 identifies
DS:0538 as the loaded region's GMAP pointer. Runtime storage identity
and the resource calls' actual results are still prerequisites.

### Initialization path

Function `182 +08C8` starts an index at zero into the eight-byte entries
at DS:67B7. It processes an entry only while the unsigned index is below
DS:264E and its byte +5 has bit 3 set. The first entry without that bit
ends this loop; this is a prefix, not a filter skipping individual entries.
At termination it calls `182 +0BAC` and returns.

For an entry, the 16-bit negation of its word +6 supplies an object
number. The door classifier accepts signed numbers 2400 through 2949.
If not a door, processing proceeds toward creation only when low bits
0..2 of entry byte +5 are at least six. Creation also requires the
original entry word +6 to be signed negative.

It requests `187 +22B3` with the doubleword one, then calls
`31E0:0EFF` with the negated object number, current entry index, -1,
the low-three-bit value and six. A returned FFFF advances to the next
entry. Another return is retained as the placed slot and written to
the entry's word +6 before the door-specific work.

For a door, a metadata kind of one selects the associated 23-byte
record. It copies the low byte of record word +0x10 to a local byte
and reads slot word +0x16 as a 72-byte presentation-entry index.
If either low bit 0 or 1 is set it selects presentation column one;
otherwise it selects column two. When bit 1 is set it also sets bit 0
in the local byte. This modification is local: the routine has no
corresponding store back to the item flag word.

It reads the selected presentation word, passes only its low byte as
the value argument to `31E0:271E`, with the slot, then calls
`182 +0FC5` with the slot and the local flag byte. The upper byte of
these pushed words is not independently initialized at those pushes;
only the documented low-byte arguments are characterized here.
Actual presentation effects and complete initialization/caller
admission remain unresolved.

### Shared strip helper

Function `182 +0FC5` returns without grid calls unless the supplied
slot's metadata kind is one. It takes unsigned slot coordinate words
at +0x0A and +0x0C, shifts each right four, and uses those as the
starting column and row. It obtains the item record index from the
slot metadata and reads the full item word +0x0E, called n here.

Both local extents start at zero. If unsigned n is below ten, the
column extent becomes n and the row extent stays zero. Otherwise
the row extent becomes n minus ten and the column extent stays zero.
The nested loops include both zero and their respective extent:
n below ten requests n+1 cells across one row, while n at least ten
requests one column of n-9 cells. In particular zero still requests
one cell; ten also requests one cell, using the other orientation.

For each requested cell, argument byte bit 0 selects the operation:
set it to request `25AF:0057`, clear it to request `25AF:0088`.
Both receive the mask byte 0xC0. The coordinates are the starting
column/row plus their offsets, with 16-bit addition.

The resident writers independently refuse a zero GMAP far pointer,
an unsigned column at least 128, or an unsigned row at least 98.
Otherwise they address pointer + 128*row + column. `0057` ORs the
mask into that byte; `0088` ANDs its complement into the byte.
Thus these requests set or clear both bits 6 and 7, retaining every
other bit. Out-of-range requests do not mutate the grid. The helper
does not itself clip or reject the strip before issuing the calls;
each cell is checked by the writer. Far-pointer validity, capacity,
lifetime and the absence of coordinate wrap on admitted inputs
remain separate prerequisites.

## Interpretation

FND-PARTY-250's formerly opaque `+0FC5` request now has a concrete
local grid effect: door state bit 0 drives setting versus clearing
GMAP mask 0xC0 over a strip encoded by item word +0x0E. The initialization
path's bit-1 normalization makes a locked door's local request use
bit 0 as well, without proving that the stored flags are normalized.
Actual wall/passability presentation, player action admission and the
remaining children stay under Q-PARTY-107. This does not name these
item words globally or establish a complete door interaction rule.

## Alternatives

- The initialization path does not persist its local bit-0 normalization.
  No item-word store occurs between the read and the two child requests.
- The strip uses inclusive extents, not a cell count n. Zero and ten
  each issue one request; n+1 or n-9 gives the appropriate orientation.
- A missing entry flag terminates the loop instead of advancing past
  the entry. Whether the admitted input arranges all relevant entries
  in this prefix is not established.
- Creation, object/slot metadata, GMAP identity and presentation indices
  are not validated merely because later calls consume their values.
  The non-door special object-430 path and `+0BAC` are outside this
  door-field reading; no completeness claim covers their behavior.

## How to reproduce

Use the locked evidence Python with the installed `<dsun>` and scripts
under `tools/research/exec-census/`:
`overlay_listing.py <dsun> 182 69118 692CC`,
`overlay_listing.py <dsun> 182 69815 698D6`, and
`resident_listing.py <dsun> 25AF:0057..25AF:00BB`.
The scripts verify the source length and XXH3. Compare the complete
inventory bodies and separately trace both low-bit selections,
creation FFFF, non-kind-one, n below/at ten, null pointer and each
coordinate-bound guard. This is not an emulated call or a runtime
observation of a door.
