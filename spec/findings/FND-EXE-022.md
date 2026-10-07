---
id: FND-EXE-022
title: Compiled pointer installation paths differ in prefix transfer and publication order
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A8330..0x004A83EA
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A83F0..0x004A843D
tool: Ghidra 12.1.3 PUBLIC, pinned bounded instruction and reference reporters
environment: null
---

## Observation

FND-EXE-020 leaves the source of the filename helper's object-plus-four
prefix open. The analyzer's references to the pointer array at `0x01BE6D60`
identify direct stores in two routines read here. This is not a complete
writer census: other stores, indirect aliases, construction and virtual
targets remain unread. The objects and their strings are not assigned a
semantic type from the bundled source lead.

The routine at `0x004A8330` starts a slot counter at zero and advances it
through 25 inclusive. Corresponding records start at `0x01BA4AA0`, with
16-byte stride. For each record it reads a base word at offset zero and a
limit word at offset four, subtracts the base from the limit at 32-bit width,
and arithmetically shifts that result right by two. A computed signed count
at most one skips that record. This computation does not independently prove
allocation size, alignment or that either word describes a valid list.

For a count greater than one, it reads the record's 32-bit index word at
offset 12 and the object pointer at base plus four times that index. It
increments the index at 32-bit width and uses signed division by the count
to obtain the remainder. That remainder is stored back as the record index
before any string or virtual call, then selects another object pointer from
the same base. There is no additional local guard validating the old index
or converting a negative remainder to a positive index.

It passes old object plus four as source and newly selected object plus four
as destination to the strcpy thunk identified in FND-EXE-015. On normal
completion it reads the new object's first word as a vtable pointer and calls
the near target at byte offset 88, passing the new object itself as the
stack argument. Only after that call returns does it publish the new object
pointer into the corresponding slot of `0x01BE6D60`. It then calls
`0x00520320` with a data pointer and numeric slot/index/count arguments,
whose effects are not read here, before advancing the slot counter. No
branch tests the virtual call's return value before publication. The direct
body contains no rollback of the earlier index store or prefix copy.

The routine at `0x004A83F0` takes a full 32-bit slot argument. Before examining
its record it writes that argument to shared word `0x01BA4C40`. It derives
the record address by a 32-bit left shift of four, then computes the same
base/limit difference and arithmetic right shift. A zero computed count
returns with the shared word already changed. A nonzero count, including a
negative one at this local test, stores zero to the record's index word,
loads the first object pointer from the base, and publishes it into the
pointer array. It then calls the target at byte offset 88 of that object's
vtable, passing the object, and returns normally. It makes no prefix copy
in its own body and has no local slot-range or object-null guard. This
finding does not establish which argument values its callers admit.

## Interpretation

These paths establish one direct transfer of the object-plus-four string
used by filename prefix initialization, but not its original construction,
maximum length, allocation bounds or meaning as a current directory. The
rotation path copies then calls then publishes; the initialization path
publishes then calls. Their index/shared-word writes precede those boundaries.
A child error, nonlocal exit or mutation cannot be described as transactional
from these local sequences. The virtual target may itself change the prefix
or shared pointers; its concrete object types and implementations still
need reading before a final-state or wrapper-resolution claim.

## Alternatives

Publishing before the virtual call on the rotation path, delaying publication
until after it on the initialization path, rejecting a negative computed
count in the latter's local guard, or preserving the shared word on its zero
count return are ruled out by the complete direct branch sequences. Valid
record producers and bounded caller arguments might exclude negative or
invalid states, but those preconditions are not established by these routines
alone. A routine with a bounded slot loop does not prove the pointer array's
complete allocation or writer set.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Query references to
`0x01BE6D60`, retaining DATA references as well as WRITE references: direct
stores are not all labeled WRITE by the analyzer. Summarize `0x004A8330`
and `0x004A83F0`. Check 75 instructions from the former, then 32 from
`0x004A839E`; check 30 from the latter. Stop at each cited body's end and
exclude following-function instructions. Verify the slot bound and record
stride, 32-bit subtraction and arithmetic shift, signed count guards,
increment/division/remainder width, both index stores and the complete
ordering of source/destination copy, virtual target and publication.
Enumerate skipped and processed rotation records, and zero/nonzero initializer
counts, conditional on readable producer state and normal callees. Follow
which arguments and memory locations remain unknown across the calls;
do not treat the analyzer's caller list or an imported source lead as a
complete producer proof. Keep rich reports local and execute no interpreter
or game.
