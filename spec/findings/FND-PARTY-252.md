---
id: FND-PARTY-252
title: Item property classification uses word 0x10 as a nonzero test, its name gate replaces the result, and its caller can populate a zero bitmap cache
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00063D1B..0x00063DF0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:28E3..28C9:2A25
tool: tools/research/exec-census/overlay_listing.py, resident_listing.py and direct_callers.py at commit 0c7a5410; Python 3.14.7, Capstone 5.0.7
environment: null
---

## Observation

This reads overlay 178 `+0C6B` and its reached direct caller
`28C9:28E3`. The source is installed DSUN.EXE; no original function
was executed. FND-PARTY-248 supplies the item-type, value, name and
cache field identities. Inputs and storage remain qualified below.

### Local item classifier

The helper accepts a 23-byte record index. An argument FFFF returns
AL zero without a record read. Other indices are multiplied by 23 at
16-bit width and used with the far pointer DS:19C1; this body supplies
no independent capacity check. It reads the item-type word +0x0A and
starts its result byte at zero.

Each of the following predicates assigns the byte 0x08, retaining it
when later predicates are false:

- The item's unsigned value word +0x06 is greater than one.
- The entire word +0x10 is nonzero. This is not a mask of bit 0 or
  any other individual flag.
- Quantity word +0x02 is nonzero, the unsigned item-type word is below
  9999 and bit 1 of the type entry's byte +0x0B is set.
- The unsigned item-type word is below 9999 and its type entry's
  byte +0x00 has any bit of mask 0x13 set.
- The unsigned item-type word is below 9999 and bit 7 of type-entry
  byte +0x0B is set. This bit belongs to the type record, not item
  word +0x10.

Finally it reads the item's name-number word +0x14. An unsigned value
below 9999, followed by a comparison excluding FFFF, assigns 0x80.
This is replacement, not OR: even an earlier 0x08 becomes 0x80.
For stable storage the second name comparison is redundant because
FFFF is already outside the first bound. The function returns this
byte in AL and does not promise a zero high return byte. It has no
child calls or memory stores. Its local byte results are exactly zero,
0x08 and 0x80, with the name condition taking priority.

### Caller and cache write

The direct-call query finds a load-image far-call candidate at file
0x207C6. Its instruction boundary and target are verified by decoding
the complete established body `28C9:28E3..28C9:2A25`: the call is
`28C9:2936`, targeting trampoline `56A2:002A`, which enters `178 +0C6B`.
The paragraph notation printed by the candidate search is not a
function-entry declaration. Indirect calls and aliases remain excluded
from that query's completeness.

The caller starts a local result byte at zero, calls the type accessor
`28C9:2839` for its placed-slot argument, then requires metadata kind
one and an unsigned record index below 9999 before calling the helper
with that record index. It ORs returned AL into the local byte. A type
result at least 9999 skips the subsequent type-entry work.

For a lower type result it addresses the 15-byte entry at DS:19CD.
Bit 3 of its byte +0x0B ORs 0x40 into the local result. When bit 5
of type byte +0x04 is clear, the caller ORs 0x01 into the result,
then tests item cache word +0x0C. If zero, it copies the placed slot's
word +0x19 into that full cache word at `28C9:29BF`. A nonzero cache
is retained; if type byte +0x04 bit 5 is set, the entire cache branch
is skipped. There is no resolver call or nonzero check on the copied
image word inside this branch.

After that branch, bit 0 of type byte +0x0B calls the door-range
predicate `182 +0C00` with the slot's object-number word +0x1B.
A nonzero AL replaces the entire local result byte with 0x10;
a zero AL instead ORs 0x10 into it. These have different effects on
the earlier classifier bits. The predicate's range is 2400 through
2949 (FND-PARTY-250).

The common tail rereads metadata. Kind two with an unsigned record
index below 9999 ORs 0x08 into the local result. The caller then
returns AL. This is a separate kind-two source of the same return
bit; it is not the item word +0x10 test.

## Interpretation

Overlay 178's use of item word +0x10 is now bounded: it contributes
to a local property-classification result when nonzero. The body
does not establish the word as universally magical, usable or a
door flag. Nor does it establish that this predicate affects a
player-visible result on paths where the later name gate replaces
the byte or where the caller's door predicate replaces it again.
The returned classification's consumers and admitted input domain
remain Q-PARTY-108.

The caller adds a direct producer of the bitmap cache beyond the
resolver and transfer paths: it can copy a placed-slot image word
into a cache exactly equal to zero, subject to a type flag. The
remaining producer census and cache identity stay Q-PARTY-087 and
Q-PARTY-089, respectively. This does not establish the copied word's
resource validity or its preservation across other calls.

## Alternatives

- ORing 0x80 into an accumulated 0x08 is ruled out in the helper:
  the final assignment produces 0x80, not 0x88.
- The bit-7 test in this helper does not read item word +0x10.
  It reads type byte +0x0B through DS:19CD.
- A nonzero-word test alone supplies no semantic label for the word.
  Rejected name and later caller paths must be considered by any
  attempt to map its classification to a gameplay action.
- The caller's zero-cache branch does not prove the slot's image
  is nonzero or that a BMP resource exists. Type/slot/table identity,
  capacities, later consumers and all caller routes remain qualified.

## How to reproduce

With the locked evidence Python and installed `<dsun>`, run scripts
under `tools/research/exec-census/`:
`overlay_listing.py <dsun> 178 63D1B 63DF0`,
`direct_callers.py <dsun> 178+0C6B`, and
`resident_listing.py <dsun> 28C9:28E3..28C9:2A25`.
The scripts check the source length and XXH3. Verify the candidate's
instruction boundary inside the inventoried caller, each helper
predicate, name replacement, caller door replacement versus OR,
and cache zero/nonzero and type-flag bypass branches separately.
