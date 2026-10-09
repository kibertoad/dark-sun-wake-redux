---
id: FND-EXE-295
title: An adjacent request wrapper reaches another shared saved-DS writer
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 444C:0166..444C:017F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1702..1000:177C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0000655B..0x00006563
tool: executable-reader 2.5.0, Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

The shipped words corresponding to modeled CS:135B, 135D, 135F and
1361 are each zero. This is an observation of file data, not a proof of
native startup state or absence of writes before an allocation.

An explicit-CS direct MOV destination scan of the resident load image
returned a candidate at shipped 0x00006910 writing DS to CS:1361.
The independent already-read writer at 0x000067B4 was also returned.
This scan supplies instruction-byte leads; it neither verifies every entry
path nor searches indirect, computed, implicit-segment or other-opcode writes.

The adjacent wrapper at 444C:0166 forms BP and pushes the four
incoming SS-relative words at BP+0C, +0A, +08 and +06, in that order.
Its call's segment operand at shipped 0x00039838 resolves to
1000:1702. It removes eight outgoing argument bytes, restores BP and
far-returns. Its own incoming callers are not established here.

The callee loads DX from SS:BP+0C, AX from +0A and BX from +08,
saves SI and DI, then writes incoming DS to CS:1361, DX to CS:1363
and AX to CS:1365. It does not read the word at +06 in this body.
Zero BX forwards DX:AX as a request to 1000:15A5 using two pushed
argument words and a pushed CS before a near call; it removes four
argument bytes afterwards. Thus a nested allocator call can write the
same shared DS slot again, rather than using a separate private save.

For nonzero BX and zero AX OR DX, it calls 1000:149B with low
argument zero and high argument BX, then removes four argument bytes
and returns a zero pair. Nonzero requests use the same add-nineteen,
carry rejection, high-mask rejection and word conversion as the allocator.
It loads ES from BX and compares ES:0000 with the converted AX.
Equality returns DX=BX and AX=0004. A larger existing count calls
1000:169E; a smaller one calls 1000:1622. These two helper effects
are not read in this finding. Arithmetic rejection returns a zero pair.

Every ordinary suffix reloads DS from shared CS:1361, restores DI,
SI and BP and far-returns. The early publications to CS:1363/1365
precede the branch and its calls; this body does not locally undo them
on rejection. The suffix does not establish an unchanged private copy
of the outer incoming DS.

## Interpretation

This adds a concrete shared saved-DS writer and nested-call path to
FND-EXE-272's restoration obligation. The zero shipped words alone do
not admit current list/header state. Q-EXE-010 retains this wrapper's
incoming callers, both unread helpers, additional writers and physical
aliases, admitted segment/header state and extent lifetime. It also retains
the original allocator's separate failure and interrupt obligations. No
complete-reading promotion or writer-absence claim follows.

## Alternatives

Treating CS:1361 as private to the allocator ignores this additional writer.
Treating every wrapper word as consumed here ignores the unread +06 word.
Treating shipped zero data as live initialization ignores earlier writers.
Treating a MOV-only scan as all storage writes ignores its exclusions.

## How to reproduce

At revision 5a1a1f7 require FND-EXE-176's installed DSUN.EXE identity,
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Read little-endian
words at shipped 0x0000655B, 0x0000655D, 0x0000655F and
0x00006561. In resident half-open 0x00005200..0x00057570,
decode one sixteen-bit instruction from each byte starting with an explicit
CS prefix; select MOV with direct memory destination, CS segment, no
base/index and displacement among 135B, 135D, 135F and 1361.
Use Capstone 5.0.7 operand details, with independent control 0x000067B4;
retain the stated exclusions and verify candidates separately.
Decode 0x00039826..0x0003983F at IP 0166, CS 444C, and
0x00006902..0x0000697C at IP 1702, CS 1000. Resolve
site 0x00039838/targetOffset 1702 with the committed operand reporter,
loadSegment 1000. Source bytes and reports remain outside Git. No
original execution or complete writer search is claimed.
