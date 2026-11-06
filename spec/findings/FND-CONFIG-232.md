---
id: FND-CONFIG-232
title: The shipped FONT request admits an 8300-byte user span and the width reader has a bounded ASCII domain with distinct high-byte accesses
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:599E..1BF3:59C1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:03B7..2C5F:03F1
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x4E9755..0x4EB7C0
tool: resident_listing.py at 6d87ab46; composed FND-TEXT-001 and FND-CONFIG-151/206/207/208/209/211/223 interfaces; exact integer arithmetic
environment: null
---

## Observation

FND-CONFIG-207/208 identify the FONT/100 request and returned-pointer
publication. FND-TEXT-001 supplies the shipped resource's8299-byte
extent, identity character map and complete glyph-offset/record layout.
This composes those facts with the allocation and width-reader interfaces;
it does not establish that every live lookup selects an unchanged copy.
The instruction check uses DSUN.EXE,634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b.

### Allocation arithmetic for the selected shipped length

Under an unchanged reachable directory record for this resource,
FND-CONFIG-151's complete-length selection gives count8299. Its null
destination calls444C:0008 with1 and8299. The product does not wrap;
the added one gives8300, passed through18D0 to15A5. Adding19 gives
8319 and shifting right four gives519 paragraphs, hexadecimal0207.
Neither addition carry nor the entry's high-word rejection is selected.

For the ordinary offset-four return and retained header count519,
the block occupies8304 bytes from header offset0. Its user span has
8300 bytes at offsets4 through8303 inclusive. The18D0 fill requests
all8300 bytes, one chunk, before the outer wrapper writes its marker.
With DF clear and valid admitted storage, that fill ends at offset8304
exclusive and does not wrap the word offset. The marker is at offset8303,
displacement8299 from the user pointer. A complete8299-byte resource
transfer covers offsets4 through8302; it does not overwrite that marker.

These calculations remove size-gate, fill-count and marker-wrap uncertainty
for this selected length and header binding. They do not prove allocation
success, DF provenance, backing, pointer/header stability or actual I/O.
Another selected archive record or changed length requires its own analysis.

### Byte-input domain of the width lookup

FND-CONFIG-206's width loop sign-extends a nonzero character byte c
to a word before calling599E. The unprefixed98 instruction is CBW
in this16-bit body despite the listing tool's mnemonic. 599E doubles
that word, adds the FONT base offset, reads a near word at an additional
0108, adds the same base again, and reads the result word. Neither body
reads char_map as a separate character-to-glyph translation step.

Assume the retained FONT pointer is P:0004, points to the unchanged
8299-byte resource, and object/frame accesses are valid and separate.
For c from1 through127 the first word is glyph_offsets[c]. Every
selected glyph width word is within the resource by FND-TEXT-001's
record coverage. A zero input byte terminates the outer loop before
lookup. Thus the ASCII byte domain has a concrete table/record capacity
argument under these bindings, rather than only a nonnull-pointer test.
This is a width-reading claim, not a pixel-order or drawing claim.

For c from128 through255, signed extension and word doubling instead
make the first word address resource-relative8+2*(c-128). These are
pairs in the identity map, not entries in the glyph-offset table. Put
k=c-128. The selected little-endian word is2k+256*(2k+1), or
256+514k. The final word address in P is (4+256+514k) modulo65536.

For bytes128 through143 the resulting word lies within the resource,
at resource-relative256 through7966 in steps of514. Bounds alone do
not make those positions glyph-record starts or their words glyph widths.
For144 through254 the final word lies beyond the8299-byte resource
and beyond this admitted user span; the first such relative offset is8480.
For255 the selected word is65534, and adding base4 wraps the final
offset to2, the allocated header's predecessor word under this binding.
The reader has no local check that distinguishes these cases.

No actual high-byte input, out-of-range memory access or native defect is
observed here. These are exhaustive arithmetic consequences for the byte
domain under the explicit unchanged-font and returned-base assumptions.
The low-byte decimal output of FND-CONFIG-205 is separate input evidence;
it does not establish every string the width reader can receive.

## Interpretation

Q-CONFIG-013 can reuse the shipped FONT length's allocator arithmetic
and marker/transfer separation. Q-CONFIG-008 can reuse the ASCII lookup
capacity proof and exact high-byte address classes instead of a generic
unchecked-index caveat. Actual archive selection, pointer/table lifetime,
input domain, aliases, DF and heap/backing admission remain unresolved.
The current finding neither establishes rendering semantics nor changes
the font format's evidence status or implementation parity.

## Alternatives

Treating every byte as a zero-extended glyph index misses the sign extension.
Treating all high-byte reads as outside the resource misses128 through143.
Treating an in-bounds word as a glyph width ignores its record identity.
Treating the font transfer as8300 bytes confuses allocation with resource
length; treating the fill as8299 bytes misses the wrapper's added byte.

## How to reproduce

Reuse FND-TEXT-001's complete font layout and the cited connected
request, allocation and publication readings. Calculate N=8299,
R=N+1, H=floor((R+19)/16), capacity16H-4 and marker16H-1.
Compare the fill interval [4,4+R) and transfer interval [4,4+N).

Run resident_listing.py for1BF3:599E..59C1 and2C5F:03B7..03F1,
retaining font-width-byte-domain.txt under
GAME_DIR/analysis/handler-census-20261106/. Check sign extension,
word arithmetic and both effective reads. Enumerate k=0..127 using
word256+514k and final offset(4+word) modulo65536; compare each
two-byte read with the8299-byte resource and8300-byte user span.
No original-game or emulated call is required for these arithmetic results.
Original-derived reports remain local; no source bytes are committed.
