---
id: FND-COMBAT-121
title: The resource writer creates a plain one-entry type before payload I/O and rejects an existing indexed type
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 37FC:00E8..37FC:05C5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38DC:0005..38DC:0182
tool: tools/research/exec-census/resident_listing.py at a26f4a97; existing save-writer and archive-growth evidence FND-COMBAT-034 and FND-CONFIG-068
environment: null
---

## Observation

Q-COMBAT-064 retains the directory supplying runtime SAVE resources.
FND-COMBAT-034 and FND-SAVE-021 connect the numbered writer to
37FC:00E8. This reading identifies that entry's own type-construction
and publication order, without claiming the complete allocator, copy,
free-space, flush or external I/O contracts. Offsets below are hexadecimal.

### The current selected record and type gate

After guard/signature checks, 00E8 requires a nonnull active pointer
DS:9D9F and option mask 0002 at record +6. Rejection sets error 0C
or 1E respectively and returns FFFF. It retains the record's file
handle at +4 in SI, then asks 39A9:0045 for the supplied tag in
that active record. It has no own traversal to another archive.

The local directory-space request starts at 0C bytes for a numbered
entry and adds eight when the type is missing. If an existing type's
double word +4 has bit 80000000 set, it sets error 1F and returns
FFFF before its own directory mutation and payload transfer. This
writer therefore does not update that indexed type through this route.

Insufficient current record space at +2D calls 077C with the far
address of global DS:9D9F. A zero result jumps back to type lookup,
so the local type pointer is reacquired after growth rather than simply
reused. FND-CONFIG-068 records growth's possible record replacement.
Actual global/handle identity and growth effects remain conditions.

### New types have a plain count and one numbered entry

After the common gates, 00E8 calls 0CE6 with the active record,
request length and local outputs. A zero returned candidate position
sets error 0F and returns FFFF. This reading does not treat a nonzero
candidate as proof of valid file extent or successful reservation.

For a missing type, 0206..02C9 derives insertion pointers through
38DC:0005 and requests a 14-byte directory shift through 38DC:0066.
It adjusts the record's directory extent/free-space fields and the
directory type count. Its explicit stores at the resulting type pointer
are:

| Relative offset | Own stored value |
|---|---|
| +0 | Requested four-byte tag. |
| +4 | Double-word one, with indexed flag clear. |
| +8 | Requested resource number. |
| +0C | Candidate payload file position retained from 0CE6. |
| +10 | Requested payload length. |

Thus under valid distinct record, directory and frame storage and
admitted pointer/copy helpers, this branch constructs an eight-byte
plain type header followed by one twelve-byte numbered entry. It does
not initialize an indexed range table or GFFI mapping. FND-GFF-003
and FMT-GFF-003/006 distinguish those encodings for the reader.

For an existing nonindexed type, the body locates an insertion position,
rejects an equal resource number with error 1D on its duplicate branch,
shifts the trailing directory and increments double-word count +4.
It then stores number, candidate position and length in the new
twelve-byte slot. That increment has no separate high-bit rejection.
For an admitted initial count at most 7FFFFFFE, adding one keeps
the indexed flag clear; the local body alone is not an invariant over
all possible initial counts or directory aliases.

### Directory publication precedes payload completion

The new-type and existing-type paths join at 04B6 only after those
metadata stores. The body then seeks to the candidate position using
the saved handle and requires that returned position, then writes the
requested payload and requires the exact returned double-word count.
A mismatch sets error 15. At 0508 it marks current active record
byte +2C dirty, including this transfer-failure continuation.

Its later branches update end/free-space bookkeeping and optionally
call 08FB when record flags +24 contain mask 00000008. They supply no
own restoration of the previously written type/count/entry on error.
The common result returns FFFF for nonzero error, zero for the
ordinary zero-error path, or the optional preparation helper's AX.
Flush and bookkeeping helpers can have additional effects; their mere
position after mutation is not proof of durable complete directory data.

### Far helpers and remaining storage conditions

38DC:0005 adds the double-word displacement to the pointer's offset,
folds paragraphs into its segment and retains a low-nibble offset,
with word-sized segment truncation. It supplies no independent extent
check. 0066 compares modeled twenty-bit addresses, selects forward
or backward byte copying and calls runtime pointer-adjustment helpers
while walking. Its guards, address wrap, exact helper effects and
supplied copy spans remain prerequisites; this finding claims the
writer's own field stores and ordering rather than unconditional
correctness of every directory move.
FND-COMBAT-124/125 subsequently supply the local preparation and copy
footprints, retaining those input, guard and runtime prerequisites.

## Interpretation

The known SAVE write route has a concrete plain-type constructor,
an indexed-type rejection and a count-preservation condition. This
removes the unread local type-creation question from Q-COMBAT-064.
FND-COMBAT-112 excludes SAVE tables in the unchanged shipped
archives, but actual copying/writing, directory identity, count bounds
and later preservation are still needed to apply this constructor to
a particular restored save. A filename alone does not establish it.

Nor does successful directory construction establish successful payload
storage: metadata publication precedes final file I/O and the local
failure path does not undo it. This is a conditional mutation-order
result, not an observed corrupt save or native I/O failure. Actual
lookup branches and selected lengths remain Q-COMBAT-064, with
copy admission Q-COMBAT-063 and child effects Q-COMBAT-066.
No format reader, game behavior or evidence status is changed.

## Alternatives

- A new plain type does not prove every later runtime type remains plain.
- Rejection of an indexed type is not conversion of it to a plain type.
- Publishing a length and position does not prove those payload bytes arrived.
- A failure result does not imply unchanged in-memory directory contents.
- Reacquiring the type after growth does not freeze other global state.

## How to reproduce

Use DSUN.EXE resolved through tools/game-dir.mjs and locked evidence
Python. resident_listing.py verifies size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Read 37FC:00E8..37FC:05C5
and 38DC:0005..38DC:0182 from their inventoried entries. Follow
the missing/existing/high-bit type cases, growth relookup, explicit
field widths, count increment and common I/O continuation. Keep
candidate reservation, pointer/copy, free-space and flush admission
separate from own stores. Reuse FND-COMBAT-034 and FND-SAVE-021
for the actual SAVE-tag call route. Retain listings only under
GAME_DIR/analysis/hit-incoming-20261104/: restore-save-directory-writer.txt
and restore-directory-far-helpers.txt. No original listing or payload
is committed.
