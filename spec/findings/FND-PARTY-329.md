---
id: FND-PARTY-329
title: The shipped VECT load requests bytes after the cast queue and marks its attempt before reading
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00064C78..0x00064CAE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:01AF..44DE:022C
tool: tools/research/exec-census/overlay_listing.py, resident_listing.py and gff_tag_numbers.py at 8d06d44d; locked Python 3.12.10 with Capstone distribution 5.0.9; resource-reader interfaces in FND-CONFIG-151/152 and source extent in FND-RNG-091
environment: null
---

## Observation

Q-PARTY-137 retains writers to the reflected-cast queue payload and its
separate dispatch-kind words. FND-PARTY-328 identifies one nearby block
producer: the VECT loader. This reading connects that producer to the
resource-read interface and transfer request, rather than treating every
write to the segment as an unspecified queue alias.

The complete loader, overlay 179 `+0998..+09CE`, is entered through its
known trampoline `56A7:0039` (FND-EXE-625). If DS byte `0x0914` is nonzero,
it skips its request and returns without assigning a new AX result.
Otherwise it stores one in that byte, then constructs the far pointer
`4DA1:0060` in its SS-local pointer slot. It passes the far address of
that slot, resource number one and tag VECT to `38FF:04AB`. After return
it tests AX but performs no conditional branch, flag rollback or other
resource call before returning. The explicit readiness store is a byte
at 0914, not a word spanning the adjacent queue index at 0915.

FND-CONFIG-151 supplies the complete relevant reader interface: 04AB
requests the whole selected resource, with zero start and all-ones count.
For a nonnull incoming destination it reuses that pointer without allocation
or a destination-capacity check. Its selected resource length controls the
count; the caller does not independently cap the VECT request. Reader
preparation and positioning must return their accepted results before the
native transfer. Failure does not restore bytes already written.

The installed archive tag census finds VECT only in RESOURCE.GFF, with
one numbered resource, one; 25 other installed GFF archives lack that tag.
FND-RNG-091 already measured its length as 1024. This source census does
not prove which archive/record a live reader selects, unchanged directory
metadata or a successful read.

The transfer body's complete `44DE:01AF..022C` reading confirms the
1024-byte case rather than extending the earlier at-most-98 metadata case
by assumption. A zero count high word skips its large-chunk loop. A nonzero
low word is copied to CX and selects AH3F at 44DE:0208, with DS:DX loaded
from the normalized destination and BX the input handle. For count 1024,
this body issues one native request; it does not retry a short carry-clear
result. It accumulates returned AX as DX:AX, or stores the error word and
returns FFFFFFFF on carry set. FND-CONFIG-152's leaf normalization preserves
the linear address and turns `4DA1:0060` into `4DA7:0000` with no wrap.

Under the selected shipped-length and preserved pointer/frame conditions,
the request spans segment offsets `4DA1:0060..0460` (half-open). The six
14-byte queue payload records span `4DA1:0000..0054`, and the six dispatch-
kind words span `4DA1:0054..0060` (FND-PARTY-328). These intervals are
adjacent, disjoint and do not wrap. Normalization does not create an alias
between them. This classifies the destination range of this native request;
it is not a proof that every reached service or interrupt has no other
side effects.

## Interpretation

The known shipped-resource request is not an overlapping queue/kind writer.
To obtain such an overlap from this specific payload request requires a
different selected count, changed destination/stack/segment binding or
behavior outside the evidenced native transfer interface. Other computed,
block or aliased writes remain Q-PARTY-137; they are not excluded by the
installed resource census.

The attempt flag is also distinct from a successful-load flag. On the
ordinary returning failure path, with its stored byte preserved through
the reader, the next call skips the request. That skipped call does not
provide a newly initialized AX status. No actual failed VECT read, stable
live flag, initialized table contents or visible angle result is claimed.

This removes the generic unread-VECT-producer component from the queue
write audit. It neither admits all actual queue states nor establishes
number retention or immutable kinds, and adds no complete reading.

## Alternatives

- Every segment-4DA1 block request might overlap the queue: this particular
  request is disjoint under the stated shipped-count and pointer conditions.
- Setting the attempt byte proves a successful table load or guarantees
  retry after failure: contradicted by its store-before-call ordering and
  absence of a rollback on the ordinary returning path.
- A source length alone proves a bounded live read: it does not establish
  selected archive metadata, pointer/frame preservation or native behavior.
  Changed records and alternate writer mechanisms remain explicit.

## How to reproduce

Use licensed DSUN.EXE size 634416 and XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. With the locked evidence Python run
`tools/research/exec-census/overlay_listing.py <dsun> 179 64C78 64CAE`
and `resident_listing.py <dsun> 44DE:01AF..44DE:022C`. The existing readers
check the executable identity and resolve fixups/relocations. Follow both
flag branches, the byte-width flag store, constructed pointer, reader call
and return; then the transfer's high-word gate, low-word request and both
native-result branches. Use FND-CONFIG-151/152 for the complete selected-
resource and normalization interfaces, retaining their preconditions.

Run `gff_tag_numbers.py <licensed-install> VECT` and require RESOURCE.GFF's
single number-one entry and 25 archives without the tag. Reuse
FND-RNG-091's measured 1024-byte source extent. Compare the resulting
request interval with FND-PARTY-328's queue/kind intervals by linear
address, retaining the no-wrap and preserved-frame conditions. A source
census and instruction reading do not substitute for native observations.
No game was run and no source-derived payload is committed.
