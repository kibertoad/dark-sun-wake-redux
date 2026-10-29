---
id: FND-COMBAT-097
title: The Save/Load row reader closes its locally returned archive handle without branching on the close result
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007D32A..0x0007D3F2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:0066..38FF:02B5
tool: tools/research/exec-census/overlay_listing.py and resident_listing.py at 731f518a; Capstone 5.0.9
environment: null
---

## Observation

FND-SAVE-007 identifies overlay 192 +002A, file 7D32A..7D3F2,
as the row-description reader. It saves SI/DI and loads its word row
argument into SI. Its first archive call passes four arguments to
38FF:0066: the far address of row r's path, double word 1236, word
two and the far output address SS:BP-4. Row addressing uses the
current far table base DS:6278 plus 125*r in word offset arithmetic.
The helper has no own row-range or table-capacity check.

The open result AX is copied to DI and tested for zero. Nonzero skips
the resource read and archive close. On zero, the helper forms a separate
far pointer to row+50 hexadecimal in SS:BP-8 and passes the address
of that local pointer, tag STXT and double-word number one to 38FF:04AB.
Thus the resource-reader destination slot is separate from the archive
handle slot SS:BP-4. The helper stores that computed row-description
address rather than explicitly initializing the destination slot to zero.
When the computed pointer is nonnull, FND-CONFIG-151's shared reader
takes its reuse branch; pointer validity still needs table/input admission.
No destination capacity is supplied to it here.

### Producer of the close argument

38FF:0066's normal success continuation links the newly opened record
at the archive-list head and selects it, as FND-CONFIG-037 records.
It then zero-extends word DS:9D8E into the record's double-word handle
field +28. With a nonnull output address, it writes a zero high word
and the current DS:9D8E low word through that address. It increments
that word counter and the open-count word, then returns AX zero.
FND-CONFIG-067 distinguishes this numeric handle from the far record
pointer. The count and counter increments have word wrap; this body
does not reject a repeated handle after counter wrap.

The named caller supplies SS:BP-4 as that output address. After the
STXT read, it pushes the full double word still at SS:BP-4 to
38FF:02B5. It has no intervening own write of the startup resource
handle, a selected-handle snapshot or the close-all value into that slot.
The close argument's local producer is therefore the archive-open output,
subject to preserved stack storage and the intervening callee effects.
The helper does not initialize this handle slot before open, but it only
reads it on the open-AX-zero continuation that contains the output writes.

### Result handling around close

Immediately after the STXT request, +008E copies returned AX into DI.
It then calls 38FF:02B5 with the local handle, removes four argument
bytes and compares DI with FFFF. It does not test the close's returned
AX, save that AX for a later test or retry closure. The meaning of the
later DI test as the read result requires DI preservation through close
and its callees; complete preservation across their guards, release and
external services is not established by this caller reading.

DI equal to FFFF clears the row path's first byte. It also clears the
description's first byte in Load mode, or copies the empty source string
at DS:2084 there in Save mode, as FND-SAVE-007 records. DI not equal
to FFFF bypasses those own clears. This applies after an open rejection
using the retained open result, or after read/close using DI at that point.
The helper restores its saved DI/SI and returns without an own normalized
success or error value for the row operation.

Under valid distinct stack/table storage and admitted DI-preserving calls,
both a zero read result followed by successful close and a zero read
result followed by close FFFF bypass the row clears. Conversely, a read
FFFF result reaches the close before clearing the row, regardless of
that close's AX. The kept or cleared row marker is therefore not the
caller's own confirmation of archive cleanup success.

FND-COMBAT-096 records the close's actual local gates and mutations.
Its resolver searches from the current head for the first matching full
handle. The open output does not alone freeze that head, the new record
or its links through the STXT call. A proof that the intended temporary
archive is the record later closed still needs those preservation and
membership conditions; numeric-handle provenance is not a complete
list-lifetime proof.

## Interpretation

This is a concrete Save/Load caller of archive close using the output
of its own temporary-archive open. Its resource-read and close results
are not combined into one checked success result. Successful-looking row
state cannot substitute for checking actual archive state or the close
return when establishing later resource-provider admission.

Q-COMBAT-053's archive-state obligation is separated into Q-COMBAT-054.
It can reuse this caller's own argument/result ordering, while retaining
caller admission, row bounds, actual handles/links and callee preservation.
The parent-window buffer and transfer-content questions remain in
Q-COMBAT-053. No close-all execution, loss of RESOURCE.GFF or native
I/O failure is inferred from this local caller.

## Alternatives

A stable new head with matching handle can make the ordinary close
resolve the intended temporary record. Changed links, handle fields,
stack aliases or callee effects can change that input. Likewise, DI
preservation makes the post-close test reflect the earlier read result;
it is not proved merely by the caller storing AX into DI before the call.

The description occupies the row from +50 onward, nominally 45 bytes
within a 125-byte record. The actual STXT length, transfer bytes and
table extent still need their own admission. The lack of a capacity
argument is not evidence that an ordinary read exceeds the row. The
list builder's row-index limit is separately retained by FND-EXE-621;
this finding does not assume every parsed filename supplies a valid index.
No complete-reading declaration or native row/lifetime observation is made.

## How to reproduce

Use overlay_listing.py on the licensed installed DSUN.EXE, descriptor
192, hexadecimal range 7D32A..7D3F2. The three archive calls resolve
through fixup descriptor 37 to resident 38FF. Use resident_listing.py
for 38FF:0066..38FF:02B5, retaining the success-only handle output
and count/counter increments. Both tools check the build length and
XXH3-128; the resident tool applies MZ relocations at segment 1000.

Track the distinct SS:BP-4 handle slot and SS:BP-8 destination slot,
their far addresses passed to callees and the full-width close argument.
Follow DI from the open and read results through the zero/FFFF tests;
do not substitute close AX for DI or assume its preservation. Compare
the existing row semantics in FND-SAVE-007, caller/index context in
FND-EXE-621 and list/close conditions in FND-COMBAT-096. No game
process or emulated function call is used.
