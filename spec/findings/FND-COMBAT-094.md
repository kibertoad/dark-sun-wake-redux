---
id: FND-COMBAT-094
title: The window opener gates registration on the allocated output pointer rather than the resource reader status
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068914..0x00068946
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000689BB..0x00068A3F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:04AB..38FF:04D5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:07B1..38FF:09D6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:0438..38FF:04AB
tool: tools/research/exec-census/resident_listing.py and overlay_listing.py at aa7003f2; Capstone 5.0.9
environment: null
---

## Observation

FND-COMBAT-040 identifies the window-acquisition entry at overlay
182 +00C4, file 68914..68946. It initializes SS:BP-4 as a null far
pointer, then passes that slot by far address to resident 38FF:04AB
with tag WIND and its zero-extended word identifier. The segment
operand at overlay +00E7 names descriptor 37, resolving to 38FF.
The wrapper receives the reader's AX status, tests AX with OR but
does not branch on that test. It loads DX and AX from the local slot
and returns that pointer. It supplies no separate length output or
expected minimum window size.

38FF:04AB has a stack-limit guard, then forwards the original tag,
number and output-slot address to 38FF:07B1 with start zero and
count FFFFFFFF. After removing twenty argument bytes it returns the
reader's AX unchanged. FND-CONFIG-151 records the shared reader's
archive and lookup gates. Here the sentinel selects the recorded
complete resource length, not a window-header or child-table capacity.

The reader tests the caller's pointer slot at 0906. A zero value calls
444C:0008 with double-word arguments one and selected count. A null
allocation sets error ten and reaches the FFFF return without assigning
a nonzero output. A nonnull allocation is retained locally, then written
as a double word through the caller's pointer-slot address at 0939.
This output assignment precedes archive preparation, seeking and reading.
FND-CONFIG-209 and FND-CONFIG-210 describe the allocation, marker
and attempted zero-filling contracts and their remaining preconditions;
this reading does not assume arbitrary or fully initialized contents.

The later gates are distinct:

| After output assignment | Rejection condition | Own result and output handling |
| --- | --- | --- |
| 37FC:08FB archive preparation | Returned AX is nonzero | AX FFFF; no own clear or release of the assigned output pointer. |
| 38FF:0A72 positioning | Returned DX:AX differs from the computed offset | Error 21, AX FFFF; no own clear or release of the output pointer. |
| 44DE:01AF transfer | Returned DX:AX differs from selected count | Error 21, AX FFFF; no own clear or release of the output pointer. |
| All three gates admitted | None of the above rejects | Stores count into DS:9D91 and returns AX zero. |

Each rejected branch reaches the common return without a later own
assignment to the output slot. Earlier archive/signature, lookup, length
and range rejection occurs before this allocation/output-store phase.
The initially null caller slot therefore has no own nonzero producer on
those earlier paths. These statements concern the bodies' own stores;
callee writes through an alias, guard effects and actual external outcomes
remain separate admission conditions.

The opener at overlay 182 +016B calls +00C4, saves the returned
DX:AX and tests the two words together at +019B. That pointer test
overwrites the flags from the acquisition helper's AX-status test.
Null goes to its diagnostic path. Nonnull passes the saved pointer and
placement words to local registration wrapper +00F6. There is no own
intervening test of reader AX, selected length, embedded WIND tag or
child-table extent before that call. FND-COMBAT-093 follows the wrapper
into resident registration and its early graph publication.

Consequently, with preserved valid stack/output storage and admitted
callee returns, a nonzero allocated pointer can reach the registration
call after the reader has returned FFFF. This is a conditional code
path, not evidence that such an I/O result occurs in an actual load.
It does not prove that the registration's later bounds and setup gates
pass for the contents left by any particular rejected transfer.

The APFM child acquisition uses the separate 38FF:0438 pointer wrapper
(FND-COMBAT-089, FND-CONFIG-208). That wrapper also has a null local
destination, but after 07B1 it branches on nonzero AX and returns a
null DX:AX in that case. Its AX-zero path returns its local pointer.
Thus the parent window's pointer-only admission cannot be substituted
for the APFM child's explicit reader-status gate. Neither wrapper
locally verifies the returned payload's embedded tag or complete layout.

## Interpretation

A nonnull input to the registration path does not establish a successful
complete WIND read. Its local producer is the output slot assigned before
the shared reader's later failure gates. The branch into registration is
decided by that slot's returned pointer, independently of those gates'
AX result. FND-COMBAT-093's early publication order therefore still needs
source bytes, admitted allocation/transfer outcomes and preservation evidence
before it can establish the live child table's identity.

Q-COMBAT-051 can reuse this connected status/output propagation rather
than treating a nonnull parent pointer as a completed read. Its parent-window
input obligation is separated into Q-COMBAT-053: selected archive/resource,
heap and buffer contents, preservation and actual admitted outcomes.
There is no unread local branch translating reader status into this
registration decision. APFM source admission remains Q-COMBAT-050.

## Alternatives

A complete successful transfer from the known source and a nonnull
output retained after a later reader rejection both satisfy the opener's
pointer test under the named storage conditions. The actual selected bytes,
external results and subsequent record accesses distinguish those cases.
The local reading does not choose a native outcome between them.

An allocator or later callee can fail, terminate or change aliased storage;
those possibilities cannot be replaced with successful stub returns. Nor
does a read rejection prove either an unchanged buffer or a particular
partial payload. FND-CONFIG-152's bounded metadata transfer is not a
validation of every window-sized request. No concrete nonzero callback,
native partial window or complete-reading declaration is claimed.

## How to reproduce

Use overlay_listing.py on the licensed installed DSUN.EXE with descriptor
182 and hexadecimal ranges 68914..68946 and 689BB..68A3F.
Use resident_listing.py for 38FF:04AB..38FF:04D5,
38FF:07B1..38FF:09D6 and 38FF:0438..38FF:04AB. Both tools verify
the build length and XXH3-128. The resident tool applies the MZ
relocations at segment 1000; the overlay tool resolves descriptor 37.

Bind SS:BP-4 through the output far argument to the store at 0939,
then trace each later failure edge without inventing a pointer clear.
Follow the returned AX test in +00C4 through its unconditional pointer
loads and the opener's later OR of AX with DX. Compare the explicit
error branch at 38FF:048F in the APFM pointer wrapper. Retain the
archive, heap, direction, alias, capacity and I/O limitations of
FND-CONFIG-151, FND-CONFIG-209 and FND-CONFIG-210. No original-game
process or emulated function call is used.
