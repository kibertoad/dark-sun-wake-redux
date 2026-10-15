---
id: FND-COMBAT-042
title: The image cache fill has three offset groups and a separate handle-capacity request
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007B4F8..0x0007B796
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067453..0x00067465
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067861..0x0006786B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 167B:0138..167B:0184
tool: tools/research/exec-census/immediate_search.py, overlay_listing.py and resident_listing.py; installed-build fingerprint and overlay fixup checks
environment: null
---

## Observation

FND-COMBAT-041 reads the first group in overlay 190 +31B8.
The rest of that same body reaches its far return at +3455.
It keeps the same local resource pointer and accumulated double-word
offset across three groups. No whole offset-table clear occurs in
this body. The table base is current DS:61AB, with four-byte entries.

| Group | Local iterations | Source request | Offset entries |
|---|---|---|---|
| First | 0..20 | BMP number from DS:1C98 + 2*i | 0..20, only after zero reader AX. |
| Second | 0..13 | BMP number 20000+i, sign-extended from its word | 21..34, only after zero reader AX. |
| Third | 0..13 | Call 31E0:426E with word 300+i; unless AX is FFFF, request BMP with its sign-extended result | 35..48 on the successful-reader branch; the following boundary 36..49 is written on every local iteration. |

The second group's store at file 7B5D5 uses DS:61FF + 4*i,
which is base plus 4*(21+i). Its successful-reader path uses the
resource's leading double-word size, requests a write with its low
word, records the current offset, advances by the full size and
requests release in the same order as the first group. Reader error
skips that branch. No boundary store follows the second loop itself.

The third group's current-offset store at 7B74F uses DS:6237 + 4*i,
base plus 4*(35+i). Its following-boundary store at 7B777 uses
DS:623B + 4*i, base plus 4*(36+i). Both a mapping result FFFF and
a nonzero resource-reader AX jump to that following-boundary store,
without this iteration's current-offset store or offset advance.
After fourteen iterations, 7B78E again writes entry 49 from the
current accumulated offset. The body then returns without an
explicit aggregate success result.

Thus a first-group or second-group read failure can leave its own
offset entry untouched. The third group's first current entry, 35,
also depends on that iteration reaching the current-offset branch.
For later third-group iterations, the preceding following-boundary
store supplies the next current boundary under stable state. A
skipped third-group item after the first can therefore have equal
adjacent boundaries. These are conditional local store consequences,
not evidence of admitted failed-fill runtime state.

The third group also inspects and can modify the acquired resource
before its write request, including a path through 5660:002F.
Its local size is not unconditionally the original resource header's
size. This finding establishes the loop and offset stores, not a
complete transformation contract, stable pointer extent or byte
identity for that group. Write results remain unchecked before the
offset updates; FND-COMBAT-041 retains transfer and release gaps.

### Handle and quantity requests precede the fill in separate paths

The immediate/displacement scan for 459E recovers the known fill and
retrieval references as controls and a candidate in overlay 180.
At +0273..+0285, that caller pushes double-word 0004B000, then
far output pointer DS:459E, and calls 167B:0138. It removes eight
argument bytes without testing returned AX before continuing.
The capacity argument is 307200 in decimal. This is not an
allocation-size field stored by the caller itself.

The resident wrapper first calls 1425:1720 with that output pointer.
It tests full AX and on nonzero calls 4448:0002 with a message
pointer and result. On its ordinary returning path it next reads the
double-word through the caller's output pointer and calls 1425:17AE
with that value and the requested double-word quantity. It applies
the same error-helper pattern, then returns the second result.

FND-EXE-431 describes the creation helper's two ordered output stores,
and FND-EXE-413 describes the quantity helper's selector/identity tests,
adjustment and publication. FND-PARTY-034 describes 4448:0002's path
through the runtime exit worker. Error-helper invocation is therefore
not evidence that a real failed initialization continues into play.
The wrapper's encoded continuation after a hypothetical return does
not admit the output handle, current DS or preceding effects.

Overlay 180 +0686 has a fixup-qualified call to 571F:00AC, the
trampoline entering the fill at overlay 190 +31B8. Its nearby caller
instructions do not test a fill result. The initialization and fill
sites supply concrete local producers; this reading does not prove
their complete caller order, successful backend state, absence of
intervening writers, or every alias of the handle and offset table.

## Interpretation

This answers Q-COMBAT-010's local offset-group/boundary producer and
handle-capacity-request parts. The retrieval guard's 49 indices align
with 21+14+14 source slots and a final entry 49, but that arithmetic
does not make every boundary initialized after every branch.

Remaining admission includes startup ordering and all writers,
mapping/transformation outcomes, reader and release validity,
backend creation/adjustment/transfers and actual resource selection.
No persisted cache layout, gameplay behavior or native success claim
is introduced.

## Alternatives

- Treating all source failures as empty entries ignores the first two
  groups' skipped stores and the third group's initial-boundary gap.
- Treating all 49 items as unchanged resource blobs ignores the third
  group's transformation and local size paths.
- Treating the caller's unchecked AX as success ignores the resident
  wrapper's error-helper calls and underlying native dependencies.

## How to reproduce

Use the locked evidence environment and supported DSUN.EXE, size
634416 and XXH3 e296af55ba2ecde7e77f555c90f33d0b.
Run immediate_search.py for 459E with the known fill/retrieval controls.
Read overlay_listing.py descriptor 190, 7B4F8..7B796, and descriptor
180, 67453..67465 and 67861..6786B. Read resident_listing.py
167B:0138..167B:0184 and follow the existing helper findings above.
Bounds are hexadecimal and half-open. Compare each offset displacement
with 61AB using four-byte stride, and follow every bypass to the third
group's following-boundary store and final sentinel store. The scan is
a candidate search, not a complete computed/aliased writer census.
No original function, game or emulator was run.
