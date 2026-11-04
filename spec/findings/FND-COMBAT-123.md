---
id: FND-COMBAT-123
title: Archive payload positions are selected before consumption and freed ranges merge with at most one neighbour
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 37FC:09C1..37FC:0A9F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 37FC:0A9F..37FC:0CE6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 37FC:0CE6..37FC:0E05
tool: tools/research/exec-census/resident_listing.py at 3e285c47; caller contracts FND-COMBAT-121/122
environment: null
---

## Observation

Q-COMBAT-064 retains the resource writer's payload-position and freed-space
dependencies. FND-COMBAT-121/122 identify the three local helpers below.
Their own field access and branch order are now read together. All offsets
and arithmetic constants below are hexadecimal. Input validation, directory
capacity, helper/guard effects and live record identity remain conditions.

For record R, the helpers derive a free-list pointer F by adding current
double word R+35 to the far address R+31 through 38DC:0005. F starts
with a word count N; row i has a double-word position at F+2+8*i
and double-word length at F+6+8*i. Record +1C is the candidate end,
+20 the current metadata extent and +2D its available directory space.
These field roles follow their own uses, not an admitted on-disk snapshot.

### Position selection does not consume space

0CE6 first checks the record's signature at +10. Mismatch returns zero
without its later output initialization. On the matching path it obtains F,
clears the caller's far list-output slot and word index-output slot, and
scans from index zero while the index is unsigned below the current count.

The first row whose length equals the requested double word returns that
row's position immediately, writing F and its index to the caller outputs.
Otherwise it retains the smallest unsigned row length greater than the
request. A strictly smaller adequate length replaces that candidate;
equal-length ties retain the first candidate. At exhaustion it returns
that candidate's position and outputs, or R+1C when no row is adequate.
The latter path leaves both caller outputs zero.

The own body does not decrement a free length, remove a row, advance
R+1C or mark the archive dirty. Its result is therefore a candidate
position, not a reservation already reflected in the archive. It supplies
no own request-positive or span-capacity check. Its word index and
word-sized eight-times-index addressing also require admitted list extent.

### Returning a freed range

0A9F receives R, position P and length L. It first compares the wrapped
double-word sum P+L with current R+1C. Equality subtracts L from +1C
and goes directly to dirty marking and AX zero, without scanning the
free list or recursively absorbing another range at the new end.
This changes bookkeeping; the body issues no own file-truncate request.

Otherwise it rejects R+2D below eight before any adjacency test.
After deriving F it likewise rejects count FFFF. These own rejections
return AX FFFF before its own archive-field mutation, though guards
and pointer helpers still have separate effects. Directory-space rejection
therefore does not prove that no existing range could have been extended.

With admitted sorted row positions, its unsigned search chooses an
insertion index. If a predecessor ends at P, it adds L to that
predecessor length and immediately finishes. It does not then inspect
the successor, so a freed range connecting both neighbours does not
locally merge all three into one row. If that predecessor test fails
and P+L equals the successor position, it subtracts L from that
position, adds L to its length and finishes.

With neither adjacency, it requests an eight-byte shift of trailing
metadata through 38DC:0066, increases +20 by eight, decreases +2D
by eight, increments the list count and stores P and L at the chosen
row. All successful own paths mark R+2C one and return AX zero.
There is no general positive-length, overlap, ownership or overflow
validator before those arithmetic and store paths. Valid ordered,
nonoverlapping extents and fitting copies remain caller prerequisites.

### Consuming an exactly matched free-list row

09C1 receives R and a word index. It rejects only an empty list
before its row-removal work. If the index equals count minus one,
it skips shifting; otherwise it computes a trailing byte count and
requests a shift from the following row over the chosen row. It
decrements the list count, subtracts eight from +20, adds eight
to +2D, marks +2C one and returns zero.

There is no independent index-below-count test. Its shift lengths,
word-multiplied offsets and copy spans require valid index/metadata
admission; an AX-zero ending alone is not such validation. An empty
list returns FFFF without an own DS:9D99 error assignment.

### The writer consumes after payload I/O

FND-COMBAT-121's writer calls 0CE6 before inserting the directory
entry and attempting payload seek/write. Its later zero-error path
updates R+1C when the candidate equals that end. Otherwise an
exact-size free row calls 09C1; a larger selected row has its
position advanced and length reduced by the requested length.
These are later bookkeeping steps, not effects already made by 0CE6.

At 37FC:0570 the writer calls 09C1, removes its arguments and jumps
to the shared error-state check without testing returned AX. Thus
that particular result is not independently combined with transfer
success. Current index, list identity and shared error state still
need admission. The writer's directory-before-payload ordering and
lack of own rollback remain FND-COMBAT-121; this does not establish
a complete atomic resource update.

## Interpretation

The local candidate selection, free-range policy and later consumption
are separated for Q-COMBAT-064. The selection helper does not itself
reserve or consume space; freeing has early capacity gates and
single-neighbour coalescing; list-row removal needs an admitted index.
These remove the unread own-body allocation/free bookkeeping questions
from FND-COMBAT-121/122.

Actual record/header/list producers, count and arithmetic bounds,
pointer/copy/guard effects, selected directory identity and flush/I/O
outcomes remain. The source's bytes and later lookup result do not
follow merely from a candidate position or an AX-zero helper result.
No native fragmentation, overlapping resource, failed save or new
format behavior was executed or implemented. No question closes and
no rule or parity status changes.

## Alternatives

- Candidate selection is not immediate reservation.
- A capacity gate can precede a possible adjacency merge.
- Extending one neighbour does not imply merging both neighbours.
- A valid last-index special case is not a general index bound.
- Later bookkeeping failure does not undo earlier directory or payload writes.

## How to reproduce

Resolve DSUN.EXE through tools/game-dir.mjs and use locked evidence
Python. resident_listing.py verifies size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Read 37FC:09C1..37FC:0E05,
tracking F separately from the selected row and the output slots.
Compare the writer's 01C9..01F9 and 0518..0591 continuations in
FND-COMBAT-121 and the removal call in FND-COMBAT-122. Reuse
their pointer/copy and error-state limits. Preserve unsigned length,
position and index comparisons and wrapped arithmetic rather than
assuming validated extents. Retain the listing only in
GAME_DIR/analysis/hit-incoming-20261104/restore-archive-space-helpers.txt.
No original listing or free-list payload is committed.
