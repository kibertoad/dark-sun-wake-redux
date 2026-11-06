---
id: FND-PARTY-330
title: Fixed spell and saved-state requests have specific queue-relative bounds and SPST has an unchecked slot input
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006F982..0x0006FA21
tool: tools/research/exec-census/overlay_listing.py at ae5827ef; locked Python 3.12.10 with Capstone distribution 5.0.9; retained source/table observations in FND-PARTY-006, FND-SAVE-015/046 and FND-RNG-091
environment: null
---

## Observation

Q-PARTY-137 retains block writers to the cast queue. FND-PARTY-328
places its six payload records at 4DA1:0000..0054 and dispatch kinds at
4DA1:0054..0060. This reading classifies the known fixed spell and
saved-state destinations against those ranges; it is not a new census
claim about all file readers or pointer writers.

The complete overlay 186 reader `+0282..+0321` takes a word slot and a
word resource number. It makes three whole-resource requests in order:

| Tag | Constructed destination |
|---|---|
| SPST | 4D62:((039C + 15 * slot) mod 65536) |
| PSST | 4E71:((09CD + 34 * slot) mod 65536) |
| PSIN | 4E71:((0A55 + slot) mod 65536) |

Each local pointer is nonnull and passed by far address to 38FF:04AB.
The resource number is sign-extended from the incoming word for each
request. The caller has no slot bound or destination-capacity check.
After each request it compares the full AX word with FFFF; equality
returns AL0 without making later requests. Otherwise it continues, and
after the last non-FFFF result returns AL1. It does not undo an earlier
transfer when a later request fails. The relevant whole-resource interface
is FND-CONFIG-151, and the bounded native-transfer interface is
FND-PARTY-329/FND-CONFIG-152. Counts come from selected records rather
than the destination strides.

FND-PARTY-006 records the installed SPST resources as 15 bytes except
numbers 30, 31 and 33, which are nine; the corresponding PSST resources
are 34 bytes. FMT-PARTY-003 records one-byte PSIN. For selected source
records with these lengths, unchanged pointers and slots 0..3, the SPST
requests stay within 4D62:039C..03D8. The queue starts at the physically
equivalent 4D62:03F0; its kind words are 4D62:0444..0450. The request
ranges are therefore disjoint, with 24 bytes between the end of the
largest admitted SPST range and the queue start.

The unchecked input matters. For example, slot five gives SPST offset
03E7; a selected 15-byte transfer would reach 03F6, overlapping the first
six bytes of the queue. Slot eleven gives offset 0441; the same count
would end at 0450, overlapping the kind words at 0444..0450. These are
arithmetic witnesses for the actual pointer-producing instructions, not
observed caller inputs or game runs. For arbitrary word slots the
multiplication wraps; these examples are not a complete slot census.
Caller admission and actual selected source length are required to
exclude this mechanism on a real interval.

For the selected short PSST/PSIN counts, all word offsets in their fixed
segment 4E71 produce physical starting addresses above the queue/kind
range. Pointer normalization and those counts do not wrap the physical
address space or move the transfer backward. This excludes their own
payload requests as queue writers under preserved segment/count inputs;
it does not exclude other effects of reader helpers or interrupts.

The retained saved-state table in FND-SAVE-015 supplies another finite
set of initial destinations and intended lengths. Its fixed entry 20 is
4D62:039C with length 60, ending at 03D8 and missing the queue. Fixed
4C13 entries are at offsets 032D, 0357, 035B, 035F, 0363, 0365, 0367
and 0369, with lengths two or four as listed there. The queue corresponds
to 4C13:18E0, so none of those short ranges reaches it. Every other
nonnull initial fixed entry in that table begins in segment 4C10, 4F49,
4E71, 50B0, 551D or DS57E0. The 4C10 entries have two-byte lengths at
offsets 0005..0021, below the queue; the other segment bases are above
it, and their listed counts have no physical wrap. Thus the initially
listed fixed destinations and lengths miss the queue.

That classification does not cap an actual SAVE load. FND-SAVE-015
shows that the loader requests the entire selected resource through
38FF:04AB, rather than passing the table's intended length as a count.
A changed resource length or pointer-table entry needs its own bound.
Initially null entries, dynamically published pointers, allocator/heap
backing and other variable-pointer readers remain unclassified here.

## Interpretation

The write audit has a concrete nearby mechanism rather than a generic
warning: SPST needs actual slot and selected-length admission, because
its unchecked pointer formula can reach queue data or kinds. Its ordinary
0..3, at-most-15-byte requests do not. The same short PSST/PSIN requests
are above the queue, and the saved-state table's initial fixed ranges
are disjoint under their listed lengths. These request-range conclusions
are conditional interfaces, not observed preservation of live records.

Q-PARTY-137 retains changed SPST slots/lengths, changed SAVE lengths or
pointers, variable pointers and actual queue/index/frame histories. No
whole-item closure, complete reading or parity promotion follows.

## Alternatives

- A stride limits the resource read: false; the caller supplies no count
  or capacity and the selected resource determines the complete length.
- A 0..3 party layout proves every actual caller uses that domain: not
  established by this reader, which has no such check.
- Every saved-state load uses its table's intended length as a read cap:
  not what the retained loader body does. The classification preserves
  changed source/table and variable-pointer obligations.

## How to reproduce

With licensed DSUN.EXE size 634416 and XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`, run the locked evidence Python with
`tools/research/exec-census/overlay_listing.py <dsun> 186 6F982 6FA21`.
The existing reporter checks the executable and resolves far-call fixups.
Read all three pointer constructions, word arithmetic, resource-number
extensions, full-word FFFF checks and both returns. Reuse the recorded
selected-resource and transfer interfaces in FND-CONFIG-151/152 and
FND-PARTY-329; do not substitute unconditional successful reader returns.

Compare the retained shipped lengths in FND-PARTY-006/FMT-PARTY-003 and
fixed table in FND-SAVE-015 with FND-PARTY-328's queue spans by linear
address (segment times 16 plus offset). For the slot witnesses use the
same 16-bit pointer formula and 15-byte count, retaining that no native
case was run. Reuse FND-RNG-091 only as a list of known fixed destinations,
not its broader negative conclusions. No new resource payload, program
or original-game run was created.
