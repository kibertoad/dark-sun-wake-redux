---
id: FND-ITEM-066
title: The modeled placed-table allocation covers the extra insertion row but does not initialize all of its bytes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008941E..0x000894E6
tool: tools/research/exec-census/overlay_listing.py at a6941523; composition with FND-PARTY-047, FND-CONFIG-209/210/211/212 and FND-ITEM-065
environment: null
---

## Observation

FND-ITEM-065 leaves the effect-row constructor's extra-row storage and
contents open. FND-PARTY-047 records the placed-table allocation; the
existing allocator readings give its wrapper, clearing and paragraph
header contracts. This composes those exact widths for the table request,
removing insufficient nominal length as an inference under admitted heap
state, without assuming native header validity or insertion publication.

### Concrete allocation arithmetic

On the allocation branch, 200 +02BE pushes doublewords 1500 and eight
to 444C:0008, and stores its returned far pointer at DS:67B7. A null
pointer takes the failure path. FND-CONFIG-209's product is 12000;
the wrapper adds one and asks 1000:18D0 for 12001 times one. There is no
doubleword overflow for this request.

FND-CONFIG-210's admitted nonzero request adds nineteen and divides by
sixteen using a right shift: floor((12001+19)/16)=751 paragraphs. Under
FND-CONFIG-211's valid ordinary heap/header path, the allocated header
records that extent and the returned pointer is segment:0004. The complete
block spans 12016 bytes from its header base; from the returned pointer
it has 12012 bytes including the eventual marker.

With the matching 751-paragraph header retained, 444C:0008's marker store
is at header-base +12015, or returned-pointer +12011. The eight-byte row
at index 1500 occupies returned-pointer offsets [12000,12008), entirely
before that marker. Thus the wrapper's requested table length 12000 is
not the entire physical extent supplied by this modeled allocation path.
This is conditional on the established header/return relationship,
valid surrounding heap, nonwrapping addresses and actual allocation
lifetime. It does not admit an arbitrary nonnull pointer or restored header.

### Capacity differs from initialization

With clear direction flag and valid spans, 1000:18D0 zeroes exactly its
12001-byte requested product, offsets [0,12001) from the returned pointer,
as FND-CONFIG-210 records. The marker follows that clearing. At the largest
extra row, index 1500, only its first byte lies in that zeroed interval;
the remaining seven bytes have no zero guarantee from this allocation.
The marker at offset 12011 is outside the row and does not initialize it.

The three explicit fills in 200 +02BE after allocation target DS:6577,
DS:67BB and 52A2:0CA8, not the allocated placed-table pointer. They supply
no further zeroing of this padding. FND-COMBAT-035/082's numbered-reader
clear of 12000 bytes also does not initialize the row at index 1500.
Later table transfers and mutations can change what the original allocation
left in any extra row; allocation-time clearing is not a use-time fact.

### Composition with the constructor's boundary

31E0:03CA accepts old count n below 1500 and increments it to N=n+1.
FND-ITEM-065's insertion path requires a readable N-row prefix and may
write through row index N. For every admitted n=0..1499, this footprint
ends no later than offset 12008. The modeled allocation above supplies
that capacity before its marker, including n=1499, N=1500. The request's
nominal 1500 rows alone would not have proved it; the allocator composition
does so only under its heap/header assumptions.

It still does not prove the search's extra input row has a useful key,
that the N-row input is ordered, or that returned position K lies below
persistent count N. Those are distinct from readable/writable capacity.
Nor does it show a flagged row survives to the later selection/removal.
FND-ITEM-065's publication and FND-PARTY-262's zero-count questions remain.

## Interpretation

Q-PARTY-109 no longer needs to treat the extra insertion row as exceeding
this modeled ordinary allocation merely because the table request is
12000 bytes. It retains actual allocation/header/lifetime admission,
extra-row contents and order, counted insertion and row preservation.
Q-CONFIG-008 and Q-SCRIPT-003 retain the shared heap prerequisites.
No native allocation contents, invalid access, lost effect, game defect,
parity change or complete-reading promotion is claimed.

## Alternatives

Treating the request size as the whole block would discard the plus-one,
paragraph rounding and returned offset. Treating rounded padding as zero
would extend the explicit fill beyond its count. Treating a marker or
nonnull return as a valid live header would omit the retained heap
conditions. Sufficient capacity is not a proof of sorted publication.

## How to reproduce

Use installed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run overlay_listing.py for descriptor 200,
8941E..894E6 hex. placement-table-allocation.txt under
GAME_DIR/analysis/item-removal-20261104/ retains the complete entry-based
setup body. Check the two doubleword arguments, pointer store, null gate
and every later fill destination.

Reuse the complete allocator readings of FND-CONFIG-209/210/211/212 for
product plus one, zero-fill length, request-plus-nineteen rounding,
751-paragraph header, offset four and marker placement. Derive the
half-open byte intervals above before comparing FND-ITEM-065's largest
row index. Reuse FND-COMBAT-035/082 for the numbered-reader fill's separate
12000-byte extent. No original code, memory or record fixture is committed
or executed, and no rounded padding contents are inspected or invented.
