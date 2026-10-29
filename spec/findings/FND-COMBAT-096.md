---
id: FND-COMBAT-096
title: Failed archive selection preserves the body's active pointer while close-all can retain earlier completed closes on a later failure
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 37ED:0064..37ED:00B8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39A9:0001..39A9:0045
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:02B5..38FF:0429
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 37FC:08FB..37FC:09C1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:003A..44DE:0064
tool: tools/research/exec-census/resident_listing.py at ed4c0ed5; Capstone 5.0.9
environment: null
---

## Observation

FND-CONFIG-037, FND-CONFIG-041 and FND-CONFIG-067 identify archive
selection, closure and the distinction between numeric handles and record
pointers. This reading follows the local failure and mutation order of
those resident bodies. DS-relative fields below mean current DS at each
instruction; stack-limit guards and valid distinct storage remain conditions.

### Handle resolution and selection

39A9:0001 starts from far DS:9DA3 and follows each record's far link
+0C until a null link or a matching double-word handle at +28. It
compares the full input double word, returns the first matching record
as DX:AX and returns zero after null exhaustion. It does not use the
archive count as a bound, validate links or detect cycles. Thus the
local body does not prove termination or unique handle membership.

37ED:0064 initializes a local pointer to zero, calls the archive-signature
helper 39A9:012E and tests its returned AL after zero-extending it to AX.
Signature rejection returns AX FFFF before resolution or selection.
Otherwise it resolves the supplied full double-word handle. Null sets
DS:9D99 to 18 and returns FFFF. Nonnull writes the entire resolved
pointer to DS:9D9F at 00B0 and returns AX zero. Neither rejection has
an own clear or replacement of DS:9D9F. The signature helper's local
contract is FND-CONFIG-208; guard and alias effects are not assumed away.

### Preparation before unlinking

For an ordinary handle, 38FF:02B5 first requires the same nonzero
signature result and a nonnull resolver result. It then calls
37FC:08FB on the resolved record. AX FFFF returns failure before any
own list update. Other values call 44DE:003A with the word at record+4.
An FFFF close result sets DS:9D99 to 21 and returns FFFF before the
list updates. FND-PARTY-042 previously identifies the preparation call;
its complete local own ordering is:

| Record byte +2C / step | Test or request | Continuing effect |
| --- | --- | --- |
| Zero +2C | Skip the file requests. | Reach the common byte-clear and zero return. |
| Nonzero +2C, first positioning | Request offset zero through 38FF:0A72. | Require returned DX:AX zero. |
| Header write | Request 28 bytes from record+10 through 44DE:022C. | Require returned double word 28. |
| Directory positioning | Request the current double word at record+1C. | Require the returned double word to equal the reread field. |
| Directory write | Request the current length at record+20 from record+31. | Require the returned double word to equal the reread length. |
| Final handle operation | Call 44DE:02F8 with the handle word. | Require AX other than FFFF. |

Each failed check above sets DS:9D99 to 21 and returns AX FFFF.
The common success continuation clears record byte +2C and returns
AX zero. There is no own restoration of file bytes already requested
before a later error. Nor does a later close failure undo that successful
byte clear. The position/write/final-operation results remain external
dependencies; no actual file write is established by these requests.

44DE:003A rejects a zero handle locally, stores six in DS:33BE and
returns FFFF without an interrupt. A nonzero word goes in BX for
DOS INT 21h with AH 3E. Carry set stores the returned AX in DS:33BE
and returns FFFF; carry clear normalizes AX to zero. The wrapper
saves and restores BX. This is a static request/result contract, not
an observed operating-system close.

### List and count changes after admitted close

Only after the close result is admitted does 38FF:02B5 change its
own archive graph. If current word DS:9D9D equals one, it clears both
DS:9D9F and DS:9DA3 without further membership tests. Other counts
conditionally replace the head when it equals the target, update the
target's previous/next neighbors through +08/+0C, and change the active
pointer only if it equals the target. That replacement prefers target+0C
when nonnull, otherwise the current list head. Neighbor pointers use
separate word stores; no concurrency or timing effect is inferred.

All admitted paths then decrement the count with word wrap. They
subtract target double words +20 and +2D and constant 31 hexadecimal
from double word DS:9D95, with no own underflow check. They pass the
target to 444C:0092 and, if it returns, return AX zero without testing
its result. FND-CONFIG-165 reads that release wrapper and its header
and guard dependencies. The list/count changes precede that call.

The body has no rollback of its link, active-pointer, count or accounting
stores if a later release call changes continuation. The count-one branch
does not independently prove that the head and active pointers name the
resolved record; a coherent admitted graph remains an input obligation.

### Close-all loop

Input FFFFFFFF takes a separate branch after signature admission.
The loop tests current DS:9D9F for null. Nonnull reads that selected
record's double-word handle +28 and recursively calls the same close
entry with it. Any nonzero recursive AX returns FFFF from close-all.
A zero result rereads DS:9D9F and repeats; null returns AX zero.
It does not use the count as an independent iteration limit.

There is no saved graph snapshot and no restoration of earlier successful
recursive closes after a later failure. Consequently a failing close-all
can follow one or more completed local unlinks, count changes and release
calls. Conversely, a null active pointer makes this loop return zero
without proving that the head/count or other archive fields are empty.
Actual termination requires the graph, handle resolution and returning
callees to maintain the needed progress; those inputs are not proved here.

## Interpretation

Selection failure, ordinary close failure before unlinking and a later
close-all failure are different lifetime facts. The selector's own failure
paths do not replace the active pointer. The ordinary close's early gates
precede its own graph mutations, but preparation may already have cleared
the record's dirty byte or requested file writes. Close-all does not undo
previous completed closes when a subsequent iteration fails.

Q-COMBAT-053 can reuse these local transitions while tracing actual
archive-state producers and admitted calls. A nonzero result from an
archive operation is not a general proof that the provider remains open,
unchanged or searchable. A zero result is likewise not a substitute for
the graph and source admission required by FND-COMBAT-095.

FND-CONFIG-061 identifies the exit-callback route to a close-all caller,
and FND-CONFIG-041/FND-CONFIG-069 retain direct/indirect caller limits.
This reading does not broaden those censuses or place a close-all execution
inside the numbered-load path.

## Alternatives

With a coherent list, distinct records, stable handles and admitted returning
callees, ordinary operations can preserve the expected provider lifetime.
Aliases, changed links or handles, guard effects and external failures can
change the relevant continuation. The local body does not determine which
case a particular window request reaches. No native archive disappearance,
successful file request, callback target or complete reading is claimed.

## How to reproduce

Run resident_listing.py on the licensed installed DSUN.EXE for
37ED:0064..37ED:00B8, 39A9:0001..39A9:0045,
38FF:02B5..38FF:0429, 37FC:08FB..37FC:09C1 and
44DE:003A..44DE:0064. The tool checks the build length and
XXH3-128 and applies MZ relocations at segment 1000.

Follow both selector rejection paths to their returns before 00B0.
For close, distinguish the all-ones branch, the full-handle resolver,
preparation, file close, unlink and count/accounting stores, and release.
For preparation, retain the current-field rereads, all result widths and
the success-only byte clear. Trace one successful recursive close followed
by a rejecting iteration as a conditional path, without executing or
inventing the external results. Keep 38FF:0429's following error accessor
outside the close body. No game process or emulated call is used.
