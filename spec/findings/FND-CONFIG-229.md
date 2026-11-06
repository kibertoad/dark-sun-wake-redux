---
id: FND-CONFIG-229
title: The game memory-total query combines unchecked block extents and a bound-derived addend after a temporary allocation
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0D30..2C5F:0E1A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1BCB..1000:1C32
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1959..1000:19A9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:24CF..1000:2519
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:066B..1000:0693
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000798A2..0x000798E3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00087D85..0x00087DCC
tool: resident_listing.py, overlay_listing.py and direct_callers.py at f78127af; Ghidra 12.1.3 read-only INST-single-20261103b
environment: null
---

## Observation

This settles the connected query's local return calculation and whether it
uses FND-CONFIG-228's consistency checker. The source is installed DSUN.EXE,
634416 bytes, XXH3-128 e296af55ba2ecde7e77f555c90f33d0b.
It does not establish that its inputs describe valid live allocations.

### Temporary request and physical cursor

2C5F:0D30 first calls444C:0008 with double-word arguments1 and1024.
A nonnull DX:AX result is passed to444C:0092. It does not undo or verify
those services' effects. Reuse FND-CONFIG-209/211/223 and
FND-CONFIG-225/226/227 for the allocator, release and shared-state
interfaces; this query is not a passive observation of the incoming heap.

It initializes the first pointer pair of a local cursor to zero and passes
the cursor's near offset to1000:1BCB repeatedly. That helper reads and
writes through DS, whereas the caller forms its record in SS. The described
local-record behavior therefore requires those accesses to name the same
storage; this call does not pass a segment or establish DS/SS identity.

The cursor's fields are a word offset at0, word segment at2, double-word
extent at4 and word allocation flag at8. A zero cursor segment selects
CS:135B; a zero first root returnsAX1. An existing segment equal to
CS:135D returnsAX5. Otherwise it advances the segment by that header's
word-zero paragraph count at word width. On a produced node it writes
offset4, the selected segment, unsigned paragraph count multiplied by16,
and flag0 for zero header word two or1 otherwise, then returnsAX2.

Neither the cursor nor its query caller invokes19A9. The cursor has no
membership, range, carry or positive-progress check. A zero count at a
nonterminal segment can repeat that segment. Roots are read during calls,
not captured once. Backing, stability and alias admission remain required.

### Total, separate near cursor and bound addend

For each AX2 cursor result with allocation flag zero, the query adds the
double-word extent to its total modulo2^32 and updates an unsigned maximum.
These extents include all16H bytes of a block, including metadata; they
are not the allocated user capacity16H-4 or the diagnostic pattern span
16H-10. Any cursor result other than2 ends this summation without an
error distinction in the query's returned total.

Unless DS:4272 is exactly1, it also iterates1000:24CF over a separate
near cursor initialized to zero. This helper uses DS:390C/390E, the
near-heap roots whose producers FND-EXE-515 describes. Initial zero
selects390C; zero root returns1. A nonzero cursor minus4 equal to390E
returns5; otherwise it advances by the current header word and clears
the low bit. Success writes the new header offset plus4, its word-zero
value masked with FFFE, and its low-bit flag into three cursor words,
then returns2. These writes also use DS, not an explicitly supplied SS.

The outer query counts successful iterations up to wordFFFF. Reaching
that count sets DS:4272 to1; other exits do not clear it. This loop's
sizes and count are not added to the returned total. Its own iteration
limit does not bound the earlier physical cursor loop.

Next1000:1959 reads upper pair DS:00A8:00A6 and current pair
DS:00A4:00A2. Helper066B subtracts their segment-times16-plus-offset
representations and returns the signed difference in DX:AX. Its byte
carry/subtract and sign extension preserve the signed high portion; it
does not truncate both linear values to words. The near call uses a
return-frame adapter and ends with a far return; it is not an ordinary
unadjusted near-return ABI.

1959 treats the resulting pair as unsigned for its comparison: if greater
than16 it subtracts16, otherwise retains it, then clears the low four bits.
Thus an admitted nonnegative difference D gives D rounded down to16 for
D<=16, or (D-16) rounded down to16 otherwise. A negative difference is
not clamped to zero by a signed test. There is no interrupt, allocator or
bound update in1959 or066B themselves. Bound provenance and intervening
updates remain FND-CONFIG-226's conditions.

The query adds this double-word addend to its total modulo2^32 and
updates the maximum. When maximum differs from total it reads a byte
at4E71:0003, but neither comparison changes the selected return value.
The total is returned in DX:AX. There is no separate validity result.
Instruction checks establish these widths and segment identities; the
decompiler infers incomplete argument and return types for these routines.

### Connected game consumers

The direct-call report finds three sites in overlay190 at+1562,+1576
and+159B, and one in overlay199 at+04A9. These are checked against
FND-CONFIG-044's dispatcher paths and FND-EXE-603's established
overlay199+02A0 routine, not treated as entries merely because a byte
search found them. Computed, indirect and changed incoming transfers
are outside this direct-call search.

The debug dispatcher calls the query, immediately calls277B:05D5 and
passes that helper's returned text pointer to the message entry. Its
second query result is explicitly pushed as DX then AX into formatting
and shown as a message. A third query call branches to the dispatcher's
shared continuation. This finding does not assign an input contract to
277B:05D5 or claim all shared-continuation effects.

In overlay199, a missing text buffer triggers the1024-by1 allocation
described in FND-EXE-603. Only a null result calls this query, pushes its
DX:AX result alongside requested1024 into fatal-message formatting,
then calls overlay180+0867. FND-CONFIG-224 supplies the ordinary fatal
exit contract. The query value is diagnostic; this caller does not use it
as an independent capacity check before allocation or as validation of
the failed result.

## Interpretation

The named game caller reaches an unchecked heap cursor through a
diagnostic total query. Its result combines free physical block extents
and a separate bound-derived quantity after a mutating service probe.
This resolves that caller's relationship to the checker: it bypasses it.
It cannot discharge Q-CONFIG-013's heap-membership, actual span,
backing or alias conditions. No native invocation, complete-reading
declaration or whole queue answer is established.

## Alternatives

Reading the query as a read-only heap validator is contradicted by its
temporary request/release and unchecked cursor. Treating the reported
total as the largest successful request ignores the sum and metadata.
Treating its second walk as additional free capacity ignores that those
sizes never enter the total. Treating underflow as zero ignores1959's
unsigned comparison. The actual admitted state remains unresolved.

## How to reproduce

Use resident_listing.py on the five address ranges above, verifying the
near/far frame adaptation at066B, both cursors' DS accesses, status gates,
unsigned comparisons, and the total's double-word operations. Retain
heap-walk-game-consumer.txt, heap-query-connected-helpers.txt and
heap-query-pointer-difference.txt under
GAME_DIR/analysis/handler-census-20261106/. The last listing continues
beyond0693; that continuation is outside this reading.

Reuse heap-walk-game-consumer-callers.txt from direct_callers.py for
2C5F:0D30. Run overlay_listing.py for descriptor190,file79890..798F0
and descriptor199,file87D70..87DE0; qualify their ownership using the
existing findings above. Retain heap-query-debug-consumers.txt and
heap-walk-overlay199-use.txt; surrounding bytes are context, not new
function entries. In the resident read-only project run
ReportDecompileMatches.java at2C5F:0D30,1000:1959,1000:24CF and
1000:066B with their call, local, return and root patterns. Require the
four function markers in heap-query-connected-decompile.log and
heap-query-arithmetic-decompile.log. Check instructions where inferred
C loses widths or arguments. No original-game or emulated call was run;
all original-derived reports remain local.
