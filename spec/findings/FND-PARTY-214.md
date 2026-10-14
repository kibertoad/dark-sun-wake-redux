---
id: FND-PARTY-214
title: Deletion-map construction rewrites the current object for type-2 steps and placement compaction adjusts slot indices through a bounded near helper
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
tool: Python 3.12 with Capstone 5.0.9, tools/research/exec-census/resident_listing.py and direct_callers.py
environment: null
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 289B:0004..289B:00F0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2452..2D40:248D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:27B7..2D40:286C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1AA0:0009..1AA0:012A
---

## Observation

Resident addresses refer to the installed DSUN.EXE relocated to segment 1000.
FND-PARTY-172 describes the ten-byte object-walk entries passed to deletion;
they are not the serialized CHAR chunks of FMT-PARTY-006.

### Slot-index adjustment

The near helper 289B:007A starts its unsigned loop counter at zero, visits BX
records with stride 37 bytes from DS:SI, and adds AX to each record's word +1
when that word is unsigned greater than or equal to DX and below DI. The
addition and stride arithmetic are 16-bit. It makes no call or interrupt,
changes no segment register, and returns after exactly BX loop iterations;
BX zero returns without touching a record. Pointer wrap and aliasing remain
possible because no storage bound is checked here.

The deletion compactor 289B:0094 calls this helper at 00E6 after copying
the remaining eight-byte placement entries forward (FND-PARTY-213). It sets
DI to the old placement count, loads DS:SI from the supplied related-record
far pointer, sets AX to -1, BX to the supplied record count, and DX to the
removed placement index. On 31E0:074B's route the related records are the
320 slot records at DS:67BB (FND-PARTY-135). Thus stored indices in the
half-open interval from removed index to old count are decremented, including
an index exactly equal to the removed index. This helper returns with that
selected DS; 0094 restores its own incoming DS at 00EB.

The other byte-pattern candidate is the near call at 289B:0063, within
the insertion routine starting at 0004. The instruction stream from 0004
reaches it after the backward copy at 0052. It uses the supplied related-record
far pointer, AX 1, BX the supplied record count, DX the insertion index and
DI the old count. Both candidate calls are instruction starts. This bounded
caller search does not exclude other indirect or differently segmented calls.

### Deletion-map builder

2D40:2452 allocates 160 local bytes and passes the near offset BP-160 as the
fourth argument to 27B7, after the first entry's word +6, its sign-extended
byte +1 (the count), and the list's far pointer. The output segment is not
passed. 27B7 keeps the output offset in DI and uses incoming DS for output.

It visits entries from zero while the signed entry index is less than the
signed count. For an entry whose byte +0 is not 2, the map word at 2 * index
is the current word DS:60EB.

For byte +0 equal to 2, it sign-extends the entry's byte +1 as its parent
index. Before calling anything it writes that parent's byte +2 into the
kind byte of the object numbered by DS:60EB at 4F49:(0C33 + 3 * object),
then writes the parent's word +4 into the object's record-index word at
4F49:(0C34 + 3 * object). It calls 1AA0:0009 with that object and the
current entry's word +6 as the field selector, and stores AX to the map
word. It reloads DS:60EB on each use; its preservation through the callee
is not established here.

At the end, including a nonpositive count, it overwrites map word zero with
its first argument (2863..2868). The word originally produced for entry zero
is therefore not retained. SI and DI are restored; no own DS change occurs.

1AA0:0009 rejects object 9999, negative objects and objects with kind zero,
setting DS:40C4 to -2 and returning zero. Otherwise it forms the record
pointer using the kind-indexed stride at 57E0:6167 and table pointer at
DS:614F, calls 2D40:06C4 to resolve the field, and reads its type from
DS:60ED + field. The type is stored at 4C0E:0005. An unresolved field sets
DS:40C4 to -1 and returns zero. A resolved field yields its far address
for type 6 or a type at least 128; otherwise it reads signed byte, signed
word or dword according to the width table at DS:040C. Types at least 10
then pass the value through 1AA0:039A with type minus 9. Success stores the
field pointer at DS:40BC, clears DS:40C4, and returns the value's low word
in AX and high word in DX. The builder consumes AX only.

## Interpretation

The compactor's previously unread near helper terminates by its explicit
record count and adjusts the related slot indices. It is not an external
service and contributes no unresolved callee-return dependency of its own.

Deletion-map construction can change object-table state before recursive
deletion starts. Treating it as only a passive index lookup would omit two
stores and the field-accessor's status/pointer side effects. Its DS-relative
output accesses do not themselves establish identity with the caller's
SS-relative local allocation.

## Alternatives

- These local bodies do not complete Q-PARTY-078. Actual current-object,
  parent-index and count admission, DS/SS identity, storage bounds and the
  field lookup/conversion callees remain unproved.
- The field-accessor result may be zero from a failed lookup; the builder
  does not test DS:40C4. No claim of successful map construction follows
  solely from its return.
- The type-2 object's parent may alias it or another visited object. The
  reading retains the store order and does not assume disjoint objects.
- No runtime probe or new complete-reading declaration is claimed.

## How to reproduce

Use the hash-locked evidence Python and the installed DSUN.EXE with
resident_listing.py ranges 289B:0004..289B:00F0,
2D40:2452..2D40:248D, 2D40:27B7..2D40:286C and
1AA0:0009..1AA0:012A. The insertion routine starts at 0004, not 0000.
Run direct_callers.py with targets 289B:007A and 2D40:27B7.
Its candidates for 27B7 are near calls at 2D40:2471, 28DD, 2AA8 and
2AE4; only the deletion caller at 2471 is analyzed here. The other
callers and all indirect candidates remain outside this bounded reading.
