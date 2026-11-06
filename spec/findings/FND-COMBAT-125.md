---
id: FND-COMBAT-125
title: Directory movement copies bytes directionally through normalized far cursors without checking capacity
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38DC:0066..38DC:0182
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0586..1000:05C7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:05CA..1000:060B
tool: tools/research/exec-census/resident_listing.py at d5f1be4d; shared pointer arithmetic FND-CONFIG-223/212
environment: null
---

## Observation

FND-COMBAT-121/122/123 identify 38DC:0066 as the directory-shift
child in resource insertion, removal and free-list movement.
Q-COMBAT-064 retains its effects. This reads the complete copy body
and its in-place far-cursor helpers, separately from the caller's live
count, capacity and pointer admission. Offsets below are hexadecimal.

0066 accepts far destination, far source and a double-word count N.
After its guard it copies the two pointers into distinct SS-frame
locals. It computes each initial modeled linear value as
(16*segment + offset) masked to FFFFF and compares them unsigned.
Destination at or below source chooses the forward path; destination
above source chooses the backward path. It does not first test overlap.

### Forward and backward stores

The forward loop checks the old remaining count against zero and
decrements the stored argument each time. For each nonzero old count
it reads one byte through the source local, writes it through the
destination local, then advances both locals by one through 1000:0586.
It repeats until the old count is zero. A zero initial count makes no
payload read or write on this path.

The backward branch first advances both local cursors by the N-minus-one
bit pattern through 0586. Its loop performs the same one-byte read
and write, then subtracts one from each cursor through 1000:05CA.
The same old-count test bounds byte operations; initial count zero
still skips payload reads/writes despite the preceding local cursor
adjustment. The body does not issue an I/O request or allocation.

These are explicit byte loads/stores and arithmetic cursor changes,
not direction-flag-controlled string operations. The copy logic does
not require an inherited clear direction flag. This does not assign a
contract to the earlier stack guard or external asynchronous effects.

### Cursor arithmetic

0586 and 05CA receive the far address of a pointer variable in DX:AX
and a signed double-word displacement in CX:BX. They load that far
pointer, choose addition/subtraction from the displacement sign, account
for low-offset carry or borrow and write normalized offset/segment
words back through the supplied variable address. The resulting offset
is 0..0F; segment operations wrap at word width.

For these positive one-byte steps, 0586 adds one and 05CA subtracts
one in the modeled twenty-bit address space. The initial N-minus-one
adjustment likewise retains the modular arithmetic; a negative bit
pattern is not a separately rejected input. FND-CONFIG-223/212 read
connected pointer arithmetic and its wrap qualifications. The helpers
provide no own allocation, capacity, physical mapping or payload bound.

0066 reloads its source and destination locals for each byte and
uses its stored count rather than preserving CX across the helpers.
There is no own source/destination-null test, independent size check,
same-segment requirement or protection against aliasing the frame.
Its stack locals and argument storage must remain valid and distinct
from copied data for the described iteration to hold.

## Interpretation

With admitted valid contiguous source/destination spans, count, disjoint
frame storage and no modeled address wrap, the directional choice copies
exactly N source bytes even when the ordinary intervals overlap: moving
right walks backward and moving left walks forward. That local contract
resolves the directory writer's own shift-helper dependency.

It does not validate the caller's computed shift length, free-list index,
directory extent, changed record pointer or twenty-bit physical mapping.
Wrapped or frame-aliased cases cannot be treated as ordinary contiguous
movement merely because the helper has a finite byte counter. No actual
overlapping original resource was constructed or executed.

Q-COMBAT-064 can reuse this complete local copy footprint instead of
treating 0066 and its cursor children as unread. Input/record identity,
capacities, guards and global preservation remain open; preparation and
publication effects are FND-COMBAT-124 and FND-COMBAT-121/122/123.
No parser or runtime implementation changes and no evidence status
promotion follow from the helper reading.

## Alternatives

- Selecting a copy direction does not prove capacity or valid pointers.
- Unsigned modeled-address ordering is not admission of physical mapping.
- A manual byte walk does not depend on the direction flag.
- Zero count avoids payload accesses but need not avoid local arithmetic.
- A bounded counter does not prevent address or frame aliasing.

## How to reproduce

Resolve DSUN.EXE through tools/game-dir.mjs and use locked evidence
Python. resident_listing.py verifies size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Read all listed ranges, excluding
the separate three-byte entry at 05C7..05CA. Track the SS local
cursors, unsigned initial ordering, N-minus-one adjustment, old-count
tests and positive one-byte steps. Compare FND-CONFIG-223/212 and
the caller ranges in FND-COMBAT-121/122/123 rather than admitting
their lengths by analogy. Retain reports only under
GAME_DIR/analysis/hit-incoming-20261104/: restore-directory-far-helpers.txt
and restore-directory-copy-cursors.txt. No original listing is committed.
