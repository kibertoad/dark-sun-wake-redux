---
id: FND-CONFIG-211
title: Heap acquisition writes paragraph headers through exact removal, tail splitting and memory-request paths
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:143B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:14C4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1528
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1582
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1831
tool: Python 3.14.7 and Capstone 5.0.7 bounded resident readings with effective segments and word arithmetic
environment: null
---

## Observation

FND-CONFIG-210's admitted paragraph count reaches four lower
helpers. Their complete local bodies occupy offsets 143B..1463,
14C4..1527, 1528..1581 and 1582..15A4, mapped by adding file
0x5200. FND-CONFIG-167 inspected 143B as a release dependency;
this reading follows its allocation caller and the previously unread
allocation helpers to identify concrete header writers.

For an exact-size free block, 143B compares its DS segment with
its word at +6. Equality clears CS:135F without dereferencing another
segment. Inequality uses +6 and +4 as linked segments, writes the
+6 link in the latter and the +4 link in the former, installs the
+4 segment in CS:135F, then restores the selected block's DS.
Neither path changes DX or AX. FND-CONFIG-210's caller then copies
the selected block's +8 word to +2 and returns segment DX, offset
four. Valid linked segments and the +8 producer are still conditions.

For an oversized block, 1582 subtracts requested paragraphs from
word DS:0 in word arithmetic. It adds the remaining extent to DX
and makes that segment current DS. At this derived tail block it
writes requested paragraphs to +0 and the original block segment
to +2. It then adds the requested extent to that new segment,
changes DS to the following block and writes the allocated segment
to that block's +2. It returns offset four in AX and allocated segment
in DX. It does not unlink the remaining free block or rewrite its
+4/+6 links. There is no own underflow, segment-wrap or address check;
the caller's unsigned oversized comparison supplies the local count
condition, while metadata and following-block validity remain open.

First acquisition 14C4 retains the paragraph count and restores DS
from CS:1361 before calling 1831 with double-word zero. It tests the
returned offset's low nibble; if nonzero, it calls 1831 with the
alignment distance 16 minus that nibble. It does not test those two
calls for the FFFF error sentinel before proceeding. It then requests
the paragraph count times 16 as a double word, using shifts that
retain the upper bits of the original count. That final returned AX
is tested against FFFF. Match returns DX:AX zero. Otherwise it stores
returned DX in CS:135B and CS:135D, writes the paragraph count at
that segment's +0 and its own segment at +2, and returns offset four.
It does not initialize +4, +6 or +8 here. Earlier alignment-request
effects are not rolled back on final rejection.

Growth helper 1528 requests the same paragraph count times 16.
An AX FFFF result returns DX:AX zero. An accepted aligned offset
stores old CS:135D at the returned segment's +2, stores that new
segment in CS:135D and writes the paragraph count at +0, returning
offset four. For an accepted unaligned offset it saves returned DX
and the count, calls 1831 with alignment distance 16 minus the low
nibble, then restores that saved segment and count. A failed alignment
request returns zero without those own header/root writes, but does
not roll back the earlier accepted request. An accepted alignment
request increments the saved original segment by one, with word wrap,
and uses that segment for the same header/root writes; it does not
use the alignment call's returned segment as the block identity.

The own near body 1831 spans offsets 1831..18BB, file
0x00006A31..0x00006ABB. It derives a preliminary linear quantity
from current DS:00A2/00A4 and its double-word increment through
helper 0562 and addition. Its high-word upper comparison is signed,
not a general unsigned overflow proof. It derives a candidate far
address through 060B, compares it against DS:009E/00A0 and
DS:00A6/00A8 through FND-CONFIG-167's 07F0, and rejects outside
those bounds with DX:AX FFFFFFFF. Equality at either bound passes.
It retains the previous DS:00A2/00A4 far pointer, calls 177C with
the candidate and returns the retained previous pointer on a nonzero
AX result. Zero AX from 177C instead returns FFFFFFFF. Thus accepted
requests return the preceding pointer, not the newly requested bound.
FND-CONFIG-167 records 177C's runtime-state writes and possible
interrupt-21h path. Complete 0562/060B arithmetic, current bounds,
177C inputs and interrupt outcomes remain dependencies.

## Interpretation

The marker extent read in FND-CONFIG-209 has concrete paragraph-word
writers on the named allocation paths. Their returned local pointer
normally has offset four, placing its preceding header at offset zero.
That relationship follows from these own bodies and their admission
branches; it does not prove a valid live heap, successful growth or
adequate storage for every caller's wrapped request.

The acquisition and growth paths can perform a preceding memory
request before a later rejection. Their returned null result is not a
transactional rollback guarantee. The paragraph headers, predecessor
fields, free links, shared roots and bounds have distinct producers.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain heap initialization and +8/link
producers, current bound/root writers, request arithmetic through
0562/060B, direction-flag provenance, segment-wrap conditions, request
reachability, runtime effects and lifetime. Valid linked metadata and
accepted requests support the recorded header/offset relationship;
invalid links, changed roots or rejected requests need different state
and effect analysis. Complete producer and memory-request readings
would distinguish those cases. No native or emulated result is claimed.

The reading that split allocation moves the remaining free block is
ruled out by its tail construction and unchanged free links. A reading
that growth uses the second alignment return as its block segment is
ruled out by restoring and incrementing the first request's segment.
A null result alone cannot establish that earlier request effects were
undone. Header writes do not establish inaccessible or unknown links.

## How to reproduce

Read each named lower helper from its entry through every return.
Follow FND-CONFIG-210's exact/oversized conditions into 143B and 1582,
keeping effective DS separate at each linked or following-block write.
Track 14C4's three possible requests and only final sentinel test;
track 1528's alignment branch, saved segment, second failure and root
writes. Read 1831's own branches, signed high-word comparison, far
bounds, retained previous pointer and 177C result handling. Compare
FND-CONFIG-167 for the already recorded comparator and runtime effects,
and FND-CONFIG-209 for the marker's paragraph-derived location. Do not
infer heap validity or rollback from local return values.
