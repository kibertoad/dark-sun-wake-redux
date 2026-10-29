---
id: FND-COMBAT-082
title: Saved combatant tables reuse bound pointers and complete resource lengths; nominal table sizes take one DOS read and failures stop the resource loop without rollback
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007089C..0x00070964
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070B4A..0x00070B5D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:01AF..44DE:022C
tool: tools/research/exec-census/overlay_listing.py and resident_listing.py at 1b1491e3; Capstone 5.0.9
environment: null
---

## Observation

The 187 +0D1A binding copies DS:19C9 into DS:158E and DS:19C5 into
DS:1592. Those are the pointer-table entries for SAVE/5 and SAVE/6
(FND-SAVE-015, FND-SAVE-021). Allocation requests and nominal save
lengths agree: 0x80A bytes for 42 combatant records of 49 bytes, and
0x462 for 17 details records of 66 bytes (FND-COMBAT-072).

The numbered reader 187 +0A6C(0) sets its loop count to 61 and walks
entries in order. For each nonnull entry it copies the far pointer into
an SS-frame local and passes the address of that local to 38FF:04AB
with tag SAVE and number index + 1. It supplies no capacity or expected
length argument. A nonzero AX clears its local success byte; the loop
increments its index, then exits instead of processing the next entry.
Earlier successful transfers are not restored by this loop.

Its only pre-read clearing branch compares the entry pointer with
DS:67B7 and fills that placed-entry buffer with 0x2EE0 zero bytes.
There is no corresponding unconditional clearing of SAVE/5 or SAVE/6
in this reader loop. If the entry pointer is null it skips that entry
without setting the failure byte. The read-back local pointer is not
copied into the global table pointer by this loop.

FND-CONFIG-151 reads 38FF:04AB and its underlying resource reader:
they request the selected resource's complete recorded length, reuse a
nonnull destination without a capacity argument, and require the actual
transfer count to equal the selected count. The nominal save-table
lengths are therefore not enforced as bounds by this particular read call.

In 44DE:01AF, counts 0x80A and 0x462 have high word zero and bypass
the 32768-byte chunk loop. Both take the one remaining INT 21h/AH=3Fh
request with CX equal to the count, after destination normalization.
Carry clear returns the actual count; it does not retry a short count.
Carry set stores AX in DS:33BE and returns the double word -1. The
helper restores saved DS, BX and CX before return. Count zero skips
the request. This extends the bounded one-request path described for
smaller counts in FND-CONFIG-152; it does not establish an OS outcome.

## Interpretation

For stable nonnull table pointers and nominal saved-resource lengths,
these two resources request transfers exactly the size of their allocated
tables. A selected shorter positive resource instead requests its own
shorter length, without this loop clearing the unused tail. A failed
transfer stops later resources, and the loop contains no rollback of any
bytes already written. Actual bytes after an I/O failure remain dependent
on the external transfer result and cannot be inferred from failure alone.

## Alternatives

Q-COMBAT-047 retains full pointer-writer/lifetime and alias admission,
actual archive selection and resource lengths on admitted saves, and later
pointer replacement. The raw equality test for the placed-entry clearing
branch must not be treated as proof of nonaliasing with other pointers.
No malformed save was executed, no native read outcome was observed,
and this finding makes no complete-reading or final hit-point claim.
Q-COMBAT-044 retains subsequent load effects.

## How to reproduce

Run overlay_listing.py with the installed licensed DSUN.EXE, descriptor
187, ranges 7089C 70964 and 70B4A 70B5D; run resident_listing.py for
44DE:01AF..44DE:022C. Bind entries and nominal lengths from FND-SAVE-015
and FND-SAVE-021, allocations from FND-COMBAT-072, and the full-resource
wrapper contract from FND-CONFIG-151. Follow the high-word-zero transfer
branch separately for decimal counts 2058 and 1122.