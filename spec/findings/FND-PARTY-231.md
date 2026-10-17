---
id: FND-PARTY-231
title: The inventory resolver offers existing storage while the image request offers a null destination to the full-resource reader
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007B26F..0x0007B2B9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007663D..0x00076668
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:04AB..38FF:04D5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:07B1..38FF:09D6
tool: tools/research/exec-census/overlay_listing.py and resident_listing.py; Python 3.12.10, Capstone wheel 5.0.9 (module version 5.0.7), xxhash 4.0.1
environment: null
---

## Observation

FND-PARTY-225 records descriptor 190 +2F2F's offered sixteen-byte
local region, separate pointer variable, OJFF request and result
read. FND-PARTY-226 records the subsequent inventory BMP request
with a cleared local image pointer. This finding binds those caller
arguments to the resource-reader branches already described in
FND-CONFIG-151 and the bounded transfer in FND-CONFIG-152.

38FF:04AB forwards type, number and the far address of the caller's
pointer variable to local 07B1, supplying start zero and count
FFFFFFFF. The reader replaces that count sentinel with the selected
recorded resource length after its lookup and length/offset gates.
The caller supplies no destination capacity. The selected full
length determines the later transfer request.

At the destination branch, the reader inspects the far pointer held
in the offered variable:

- Nonzero: copy that pointer into its local transfer destination.
  This local branch makes no allocation request and performs no own
  write to the offered pointer variable.
- Zero: request allocation for the selected count through 444C:0008.
  Null allocation fails with error ten. A nonnull result is written
  to the offered pointer variable before the later archive preparation,
  positioning and read operations. A later failure need not undo it.

The OJFF resolver offers the SS far address of its original local
data region in its separate pointer variable. When that offered
pointer is nonnull and its memory remains valid, it takes the first
branch and requests the selected resource's complete bytes there.
The caller subsequently reads the original local word at +0C on
status zero, as FND-PARTY-225 records. The reader does not supply
an independent sixteen-byte destination bound.

FND-ACTOR-001 surveys all 4,479 installed OBJEX.GFF OJFF resources
as sixteen bytes. If the selected unchanged archive record is one
of those resources, the requested count is therefore sixteen and
the intended extent is exactly the caller's offered data region.
This does not prove actual archive selection, the requested number's
presence, header stability, destination aliasing or successful I/O.
FND-CONFIG-157 supplies a concrete startup OBJEX registration
attempt and conditional fallback route; it does not establish those
conditions at this inventory call.

The BMP drawing request instead explicitly clears its pointer
variable before calling the same wrapper. At the reader's destination
branch, a still-zero variable selects allocation. Its allocation
size is the selected full resource count, not the sixteen-byte OJFF
region and not the later 16-by-16 view request. FND-PARTY-226's
caller tests the resulting pointer rather than AX, so an allocation
stored before a later read error can reach its nonnull continuation
under otherwise unchanged local state. No successful image payload
or native rendering is inferred from that continuation.

For selected count sixteen, FND-CONFIG-152's bounded transfer takes
one DOS read request at the normalized far destination. The reader
requires its preparation result zero, its seek result equal to the
requested offset, and the read's returned doubleword equal to the
selected count before returning zero. Short reads or carry-set
errors fail the local check; failure does not restore previously
written destination bytes. The operating system's actual result and
the bytes delivered remain unobserved by this static reading.

## Interpretation

The local full-count and existing-buffer/allocation choices are
mapped for both inventory callers. A selected shipped OJFF has a
compatible source extent, but resource presence and a pointer are
not proof that the original local data was populated. Q-PARTY-088
retains actual resource selection, admitted extents and OS outcomes;
Q-PARTY-089 retains record/selection, aliases and binding preservation.

## Alternatives

- Treating the OJFF request as always allocating omits its offered
  existing pointer and the reader's reuse branch.
- Treating the caller's sixteen-byte region as a reader capacity
  limit adds an argument and comparison absent from this path.
- Treating a nonnull BMP output as proof of read success omits the
  allocation-before-I/O ordering and the caller's ignored AX.
- Treating the installed OJFF size survey as proof of the selected
  runtime archive record conflates source evidence with live state.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and locked evidence Python.
Read descriptor 190 7B26F..7B2B9 and descriptor 189
7663D..76668, hexadecimal and half-open, using the declared parent
entry paths in FND-PARTY-225/226. Read resident 38FF:04AB..04D5
and 38FF:07B1..09D6. The latter includes the three-byte unreached
instruction qualified by FND-CONFIG-151; follow branch edges from
the entry rather than treating every decoded instruction as reached.
Retained reader report:
GAME_DIR/analysis/party-record-heads/drawing-20261018/resource-buffer-path.txt.

Track the pointer-variable address, stored destination, selected count,
allocation result and reader status separately. Compare the conditional
sixteen-byte extent with FND-ACTOR-001 and the interrupt/normalization
contract with FND-CONFIG-152. No original function or game run was
executed, and no OS outcome is established.
