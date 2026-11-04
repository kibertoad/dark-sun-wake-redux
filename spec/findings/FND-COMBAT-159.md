---
id: FND-COMBAT-159
title: Sorted insertion and removal preserve an empty temporary index in their adjustment pass while direct slot writes remain conditional
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:00A2..31E0:0121
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 289B:0004..289B:00F0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007129C..0x00071445
tool: tools/research/exec-census/resident_listing.py and overlay_listing.py at daf5d83e; Python 3.12.10; Capstone 5.0.7
environment: null
---

## Observation

Q-COMBAT-045 retains indexed and bulk producers of temporary slot 319's
presentation entry-index word. FND-MAGIC-011 already identifies sorted
placed-entry insertion/removal and the insertion's known direct caller.
This reading isolates their presentation-index adjustment's sentinel behavior, rather than
claiming that an immediate-operand search covers every indirect write.

### Direct slot write and adjustment helper are separate mechanisms

The complete 31E0:00A2 body compares unsigned DS:264E with 1500 and returns
FFFF without insertion when at least that count is reached. Otherwise it
uses its first word argument as a slot, multiplies by 37 at word width and
adds DS offset 67BB. It writes coordinates/height in that record, calls
31E0:444A, then calls 289B:0004 with its local eight-byte entry, current
placed-table pointer, address of DS:264E, presentation table DS:67BB and
presentation-record count 320. After the helper returns, +0118 writes AX
to that selected presentation record's word +1 and returns it.

There is no own slot bound or special exclusion of 319 in 31E0:00A2.
If slot 319 reaches this routine and the count gate admits insertion,
the direct +0118 store selects its entry-index word. This is a conditional
write footprint, not evidence that such a call is reachable.

289B:0004 saves the old count and increments the count through its near
pointer before locating the insertion position. When shifting placed
entries is necessary, it calls its near helper +007A with AX one,
DX the insertion position, DI the old count and BX the caller's presentation
record count. The adjustment walks that many 37-byte records and changes
word +1 only when both unsigned comparisons pass:

```text
insertion_position <= entry_index < old_count
```

The adjustment has no child call. A word FFFF cannot be unsigned below
any word-sized old_count, so this pass never changes an FFFF presentation
entry index. For the supplied base/count, slot 319 is included in the pass,
but its empty sentinel remains empty. This conclusion concerns the
adjustment's own stores. The preceding placed-table shift and following
eight-byte insertion copy have separate source/destination footprints.

The complete removal helper 289B:0094 returns without changing the count
when it is zero. Otherwise it decrements the count first, then skips its
move/adjustment when the unsigned new count is at most the requested removal
index. On its shifting path it calls the same +007A adjustment with AX FFFF
(minus one), DX the removal index and DI the saved old count. Consequently
that pass also excludes FFFF indices. This does not validate an out-of-range
removal request or buffer capacities; the early count decrement has already
occurred even when the requested index prevents a shift.

### The known overlay caller supplies party slots or allocator results

The complete overlay 187 +146C body initializes loop ordinal DI to zero.
At its +1525 call of 31E0:00A2 it passes SI as the slot. Ordinals below
four use SI equal to DI; other ordinals call 2D40:0453 and may retry after
31E0:0D09. The caller rejects allocator result 9999 but has no separate
literal exclusion of 319. Its later loop bound uses the loaded local
count plus four at word width with a signed comparison.

Under FND-PARTY-119's initialized, uncorrupted free-list contract,
2D40:0453 does not hand out slot 319. That contract therefore excludes
319 from this caller's direct insertion slot on admitted ordinary paths.
It does not prove the actual restored free-list state, aliased storage,
all incoming calls or uninterrupted save-time temporary-slot lifetime.

## Interpretation

This removes the sorted insertion/removal adjustment's treatment of an empty
temporary index from Q-COMBAT-045's unread producer obligations. It also
separates that bulk pass from the insertion routine's unchecked direct
slot write and records the known caller's conditional slot admission.

The whole insertion helper is not proved harmless to temporary storage:
its placed-table copies could overlap another span unless backing extents
and nonaliasing are admitted. The binary-search helper's full contract is
not established here. Other pointer-based producers, resource transfers,
restoration, corrupted free lists and save-call timing remain open.
No universal absence of a populated temporary slot follows, and no
behavioral confidence or parity status changes.

## Alternatives

An indexed producer can be missed by searches for the literal temporary
address. Conversely, participation in a 320-record adjustment does not
mean an empty sentinel is changed: the unsigned upper comparison excludes
FFFF. A caller with an unadmitted slot/free-list state is distinct from
the initialized ordinary caller contract.

## How to reproduce

Resolve the licensed DSUN.EXE through tools/game-dir.mjs. With the pinned
project evidence Python, run resident_listing.py on the complete ranges
31E0:00A2..31E0:0121 and 289B:0004..289B:00F0, and overlay_listing.py
descriptor 187 with half-open hexadecimal file bounds 7129C 71445.
The approved readers verify size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and apply declared mappings.
The retained combined listing is
GAME_DIR/analysis/hit-incoming-20261104/temporary-index-insertion.txt.
Compare the two unsigned predicates at +0080/+0085, separate the direct
+0118 store, and trace SI at the overlay +1525 call. Use FND-MAGIC-011
and FND-PARTY-119 for existing insertion/free-list context. No original ran;
original-derived listings remain only in the approved local store.
