---
id: FND-ITEM-086
title: Bitmap provider returns hit indices after unchecked reload and publishes miss fields before count admission
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:3568..31E0:3664
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:3666..31E0:36FA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:36FD..31E0:379E
tool: tools/research/exec-census/resident_listing.py at 764de9c8; Python 3.12.10, Capstone 5.0.7; composition with FND-IMAGE-006 and FND-ITEM-084/085
environment: null
---

## Observation

Q-PARTY-116 retains the bitmap provider behind FND-ITEM-084's image
refresh. FND-IMAGE-006 identifies 31E0:3568's BMP/CBMP cache. This reads
its owned local ranges, keeping allocation, archive transfer, reload and
backing-store children separate. Addresses, offsets, masks and sentinel
words below are hexadecimal; other counts are decimal.

### Entry and hit selection

The second argument's low byte selects BMP for zero or CBMP otherwise.
DS:265C zero or resource-number word FFFF returns AX FFFF. Other numbers
enter a scan from index zero through the current unsigned DS:265D count
over 16-byte records at 52A2:0CA8. The count is reread at scan conditions.
There is no own capacity check before these reads.

A match requires record word +0 equal to the resource number and record
byte +0E mask four having the same zero/nonzero sense as the tag selector.
The first such match stops the scan. If its mask two is clear, the routine
calls 31E0:31F7(index, zero), ignores its result and returns the saved index.
Mask two set returns the index directly. No hit path itself verifies a
nonnull data pointer, frame layout or completed reload.

### Miss, size and allocation

At scan exhaustion, equality of the scan index and current count admits
the miss branch. A current count at least 300 returns FFFF. Otherwise the
candidate is the record selected by the current count. The routine clears
its dword +2, then asks 38FF:05B5 for the selected tag and zero-extended
resource-number word, writing size through that field. Nonzero returned
AX returns FFFF; the candidate clear/size output is not rolled back.

There are two code branches selected by DS:14E3, with the same local
admission: signed size dword at most 37600 proceeds; a larger signed size
also proceeds when the sign-extended byte DS:655E AND the resource word
equals that resource word. Otherwise it ORs the resource word's low byte
into DS:655E and returns FFFF. The intervening candidate-pointer comparisons
on these rejection routes have no conditional branch that changes that
result. A signed nonpositive size is not independently rejected here.

The admitted size is passed to 31E0:3C02. Its returned DX:AX is stored
at candidate far field +0A before a null test. Null returns FFFF. A nonnull
result first publishes the resource word at +0, then calls 38FF:04AB with
the address of that pointer field as the output/buffer parameter. Nonzero
returned AX returns FFFF without an own release, field restoration or
count increment. This is local ordering, not an observed allocation leak
or claim that a native request supplies invalid content.

### Backing-store request and count publication

A zero transfer result requests overlay service 56EF:00D9 with doubleword
1000 and ignores its result. It clears candidate dword +6; with a current
nonzero count it replaces that field with the previous record's dword +6
plus its dword +2, at doubleword width. Thus the offset depends on current
count and prior metadata, not solely the candidate selected before children.

It calls 1425:1B0B with current DS:45A2, candidate pointer +0A, offset +6
and only the low word of size +2. The result is ignored. It then saves the
current count as the return index and increments DS:265D at word width.
It ORs masks one and two into candidate byte +0E, additionally ORing mask
four for a nonzero tag selector. The zero selector does not explicitly
clear an inherited mask four. No own post-child capacity/identity recheck
or rollback precedes this publication.

The returned index therefore need not equal the earlier candidate index
unless count, candidate and table identity survive the named children.
Nor does an index prove successful backing-store transfer. Valid disjoint
stack/table/buffer state and child preservation remain explicit requirements.
SI/BP are restored; AX is the result. ES, scratch registers and flags are
not preservation guarantees of this body.

## Interpretation

This removes the provider's local scan/tag/residency, miss/size/allocation,
failure and count-publication gates from Q-PARTY-116. Image refresh can
receive a non-FFFF index without a checked reload/backing-store outcome.
Actual archive selection and size/data identity, allocation and buffer
lifetime, cache flags/count/offset writers and 31F7/3C02/56EF:00D9/
1425:1B0B effects remain open. Local field writes do not admit native
frames or prove that the cache was loaded successfully.

## Alternatives

Treating all matching numbers as hits ignores the tag flag. Treating a
hit as resident ignores its reload route. Treating a miss as transactional
ignores stores before failure and unchecked children before count publication.
Treating zero tag as clearing old flags adds a missing store. No universal
cache invariant, native failure, complete reading or parity promotion follows.

## How to reproduce

Use licensed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and project-local evidence Python -I
with resident_listing.py on the three location ranges above in order.
GAME_DIR/analysis/item-removal-20261104/ holds
effect-bitmap-provider-contract.txt and its -repeat pair, both SHA-256
d428268eb10c417d531d5e4c0edbe672f3b9d43cf3ec189aa45e88195e941d14.
Keep reports local. No original game or isolated function was executed.
