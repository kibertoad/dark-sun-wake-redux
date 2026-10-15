---
id: FND-COMBAT-033
title: The effect-owner transfer relocates saved rows through a decrement and increment pair
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00080617..0x0008072B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0452..1000:046E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3143:000D..3143:0061
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 289B:01DC..289B:02EA
tool: tools/research/exec-census/direct_callers.py, overlay_listing.py and resident_listing.py; installed-build fingerprint and relocation/fixup checks
environment: null
---

## Observation

The remaining count decrement/increment candidates at file offsets
80673 and 806B5 belong to exported overlay 193 entry +2327, reached by
573B:00AC. Its far frame reads two owner words: the first supplies the
search threshold and the second supplies the replacement owner. No
count-capacity or owner-domain guard occurs locally in this entry.

The direct-call candidate scan reports one resident far-call pattern
at file offset 257DA, printed with paragraph alias 305D:000A. This is
a locator, not a complete caller-admission claim: raw-pattern scanning
excludes computed transfers and jumps, and does not itself establish
the path or values that reach that call. The local exported body is
the scope of this finding.

### Saved-row relocation

The body initially calls 3143:000D with the first owner and tests its
returned index against DS:2100 using a signed comparison. If index is
not below count, it skips all effect-row mutations. Otherwise each
iteration performs the following operations in order:

1. Copy the selected ten-byte row to a ten-byte local buffer in SS.
2. When index i is below count n, move 10*(n-i) bytes from row i+1
   down to row i using 289B:01DC. Decrement count after that branch.
3. Call 3143:000D with the replacement owner using the reduced count.
4. If the new index j is below that reduced count m, move
   10*(m-j) bytes from row j up to row j+1 using 289B:01DC.
5. Increment count, copy the saved ten bytes into row j, then overwrite
   only its owner word with the replacement owner.
6. Call 3143:000D with the first owner again and repeat if its result
   is below count.

Both fixed-width copies use resident 1000:0452 with CX = 10 and two
far pointers. It loads source into DS:SI and destination into ES:DI,
clears DF, copies five words, restores DS/SI/DI and removes eight
argument bytes on far return. It does not use or write DX, so the first
copy preserves the selected index held there for the following test.
The saved SS buffer and both table spans still require admitted distinct
storage; their identities are not inferred from equal near offsets.

The downward move includes the boundary row at the old count, exactly
as in FND-COMBAT-032. The upward move preserves the retained active
rows, as in FND-COMBAT-031. With valid nonnegative counts at most 192,
these moves are small even spans and use their corresponding forward
and backward copy directions. A completed iteration restores the count
it started with, but count is temporarily one lower and is restored
before the saved row and replacement owner are written. No handler,
key allocator or request-seven timer call occurs in this row loop.
Bytes two through nine, including the effect ID and owner-local key,
come from the saved row unchanged; this producer does not choose a new
key or pass through the application wrapper's code/count gates.

### The search threshold is not an equality test

FND-COMBAT-031 reads 3143:000D as a lower-bound search for admitted
sorted rows and owner 0..47. The transfer body checks only index below
count, not that the selected row's owner equals the first owner.
Thus its local selector is not an owner-equality filter.

For sorted admitted rows with owners 0..47, unchanged storage and a
replacement owner smaller than the first owner, each iteration removes
the first remaining row whose owner is at least that threshold and
prepends it at the replacement owner's lower bound. Count is restored
and that row no longer meets the threshold. The number of such rows
decreases by one, giving a finite conditional progress argument. It
also reverses the selected rows' order within the destination group,
because each later selection is inserted before the earlier ones.
This is a consequence of those explicit assumptions, not an admission
of that input case in the running game.

For a first-owner word at least 48 under an unsigned comparison,
3143:000D returns count. The local row loop is then skipped. For other
inputs, the body itself provides no general owner-equality or progress
guarantee; in particular it does not check that the replacement owner
is below the threshold. All callers and restored table state remain
necessary before asserting a native owner-transfer invariant.

### A separate owner-word rewrite follows

After the row loop, the body scans a separate 17-byte-stride table
using signed word count DS:2A6B. At each index it compares the word at
551D:0357 + 17*index with the first owner. Equal words are replaced
with the second owner; other bytes are untouched locally. This scan
runs even when the effect-row loop was skipped. Its count/storage and
semantic table identity are not established here by proximity to the
effect code. The body then restores SI/DI and returns without an
explicit boolean result assignment.

## Interpretation

This answers the local ownership-transfer pair among the count
mutation candidates in Q-COMBAT-009. It distinguishes a saved-row
relocation from a new effect insertion: the key and all non-owner
fields survive, the count is only temporarily reduced, and membership
and handler state are not rebuilt by this local loop.

It does not finish the writer census. The immediate candidate pass
also contains overlapping-decode candidates; computed/aliased and bulk
writes, table restoration, all callers and independent storage remain
outside this reading. No unreviewed caller is treated as supplying a
valid owner domain, and no native hang, defect or displayed effect
change is claimed.

## Alternatives

- Calling this an equality-only owner transfer adds a comparison absent
  from the row loop. Its separate 17-byte-table pass does have an owner
  equality comparison and must not be substituted for the row selector.
- A completed count-neutral relocation does not mean count is unchanged
  throughout the operation or that all other fields have fresh values.
- Reusing a saved key does not prove owner-local uniqueness after a
  transfer; callers and destination state must establish that separately.
- The row-loop stop for owners at least 48 does not skip the later
  owner-word rewrite in the other table.

## How to reproduce

Use the locked evidence Python and fingerprinted installed DSUN.EXE.
Run direct_callers.py for 193+2327 and overlay_listing.py descriptor
193 with file bounds 80617..8072B. Read resident_listing.py ranges
1000:0452..1000:046E, 3143:000D..3143:0061 and
289B:01DC..289B:02EA. Bounds are hexadecimal and half-open.

Check the fixed-copy callee's source/destination order and stack cleanup,
its preservation of DX, both overlap directions, count stores before
restoration, final owner overwrite, lower-bound re-entry without an
owner-equality test, and the distinct table's equality test and stride.
Retain original-derived listings only in
GAME_DIR/analysis/combat-effects/. No original function was executed.
