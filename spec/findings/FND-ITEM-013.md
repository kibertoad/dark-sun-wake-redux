---
id: FND-ITEM-013
title: Item-record release links the record at the free-list head and clears its identifier, while allocation pops the same link
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0E80..2D40:0EB4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0EB4..2D40:0EF1
tool: tools/research/exec-census/resident_listing.py at 9959e725; Python 3.12.10, Capstone module 5.0.7 (distribution 5.0.9)
environment: null
---

## Observation

The source is installed DSUN.EXE, checked by the reader against length 634416
and XXH3-128 e296af55ba2ecde7e77f555c90f33d0b, relocated to segment 1000.
The committed function inventory gives the allocation body 52 bytes and the
release body 61 bytes, ending immediately after their far returns.
No original function was executed.

The installed reporter catalog provides bounded semantic/access reports but
no instruction-listing command. This question needs the full local store
order and pointer reloads, so the existing fingerprint-checked bounded
Capstone reader supplies the static reading. This is no new decoder or
unbounded scan, and no negative census is claimed.

### Release

The complete body at 0EB4 takes a word index from its first stack argument.
It repeatedly multiplies that index by 23 at 16-bit width and adds the result
to the item-table pointer through DS:19C1.

It first compares the selected record's word zero with zero. No branch,
call or conditional instruction consumes that comparison before the stores:
a zero identifier does not skip the following local sequence.

With stable table/head storage and the selected record distinct from those
globals, the sequence is:

1. Store the old word DS:5AF3 into record word +4.
2. Store zero into record word zero.
3. Store the supplied index into DS:5AF3.

The final record store reloads only the base offset and retains ES from
the preceding far-pointer load. The body changes no DS and calls no child
routine. Pointer stability and non-aliasing are conditions of the concise
record-level interpretation, not proved properties of arbitrary inputs.
Its own writes are the two record words and the head word; it does not
perform a general record clear.

### Allocation control

The adjacent complete body at 0E80 reads DS:5AF3 as its result index.
An exact value 9999 returns that value with no record read. Otherwise it
selects the 23-byte record, copies its word +4 to DS:5AF3, then stores 9999
in that record word +4, retaining ES when it reloads the base offset.
It returns the original head index in AX and does not initialize the
record's identifier. FND-PARTY-051 separately records a caller's later setup.

The two local bodies therefore use the same head and record link with
opposite push/pop ordering. Their offset arithmetic is 16-bit and neither
body establishes record-table capacity or the admitted index domain.

## Interpretation

FND-PARTY-218's kind-one deletion branch passes entry word +4 to this
release helper. FND-PARTY-214 supplies the distinct deletion-map reading.
These current findings replace any reliance on superseded FND-PARTY-172.

For an admitted, stable, disjoint item record the helper returns that
record to a free list and clears its identifier. The allocator is an
independent positive control for the head/link interpretation. This
settles the previously unread leaf's own stores, rather than proving the
complete effect of the recursive deletion routine.

The continuation after the deletion switch, its other entries and recursive
order, DS/SS map identity, table/head initialization and restoration,
independent callers and alias/input admission remain open in Q-ITEM-004
and the existing graph-cleanup questions. No complete_reading declaration
or spec-status promotion follows from reading these local bodies.

## Alternatives

- Clearing the entire 23-byte record is ruled out by the complete leaf's
  own store sequence. Its list-link and identifier writes are distinct.
- Skipping release whenever the identifier is already zero is ruled out
  locally: the comparison has no conditional successor before the stores.
  This does not establish that a caller admits such a record.
- Allocation does not scan records for a zero identifier in this body;
  it follows the head/link and checks the exact empty-head sentinel.
- A free-list interpretation does not establish initialization, unique
  ownership, absence of aliases, or successful complete graph cleanup.

## How to reproduce

With the locked evidence Python and the verified installed <dsun>, run:

```text
python -I tools/research/exec-census/resident_listing.py <dsun> 2D40:0EB4..2D40:0EF1
python -I tools/research/exec-census/resident_listing.py <dsun> 2D40:0E80..2D40:0EB4
```

The range extents agree with coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv. Retain the
outputs only in GAME_DIR/analysis/item-removal-20261104/. The release report
item-record-release.txt has SHA-256
47ffc70ba09ed20fb6963d892e1889c4752e5f6af3482407a4eff5de36e0e99f;
the allocation control item-record-allocation-control.txt has SHA-256
747af448b9a717350cf2460f7ef7fce0f4b998a56df296fa44cbcbdfcb633448.
No original instructions, bytes or extracted records are committed.
