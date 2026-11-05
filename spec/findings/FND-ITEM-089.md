---
id: FND-ITEM-089
title: Bitmap release clears residency before pool updates and consolidation resets the pool bound after unchecked moves
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:3D2A..31E0:3EF2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:38EC..31E0:3A66
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:379E..31E0:38EC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0431..1000:0452
tool: tools/research/exec-census/resident_listing.py at daa73572; Python 3.12.10, Capstone 5.0.7; composition with FND-ITEM-074/085/087/088 and FND-COMBAT-125
environment: null
---

## Observation

Q-PARTY-116 retains release/consolidation after FND-ITEM-088's reclamation.
This reads their local bodies, pool insertion/deletion and resident-pointer
adjustment, with the pointer comparator. Addresses, offsets and masks are
hexadecimal; other counts are decimal. Pool records are eight bytes at
DS:95FB: size dword +0 and far pointer +4; DS:2650 is the inclusive bound
these routines use, not an independently proven native capacity.

### Release and pool cases

31E0:3D2A requires DS:265C and DS:265B nonzero, then tests cache byte
+0E mask two at 52A2:0CA8+16*index. There is no own index/count gate.
If resident, it clears that mask before saving the record's far pointer
+0A and size dword +2. It does not clear those fields or other flag bits.

It scans pool indices zero through current DS:2650 inclusive, selecting
the first whose normalized end pointer is unsigned at least the released
pointer. Pointer additions use FND-ITEM-085's 060E. Comparator 1000:0431
normalizes both input offset/segment pairs at word width, compares segments
unsigned, then offsets when equal, returning comparison flags. It performs
no data read or validation. This is a modular representation comparison,
not proof of disjoint ordered native blocks.

At that selected pool entry, released-end equal to pool-start prepends:
replace the pool pointer with the released pointer and add released size.
Otherwise pool-end equal to released-start appends by adding size. After
append, if another entry lies within the current bound and its start equals
the extended end, add its size too and request deletion of that next entry.
If neither equality holds, request insertion of the released span before
the selected entry. If no pool end qualified, request insertion at the scan's
final index. No overlap rejection, null-pointer test or own release-success
indicator protects these updates. Size arithmetic keeps doubleword width;
indices/near offsets keep word width. SI/BP are restored at return.

### Pool insertion and deletion

31E0:379E and 3807 both require DS:265B and DS:265C nonzero. Insertion
at i requests 289B:01DC to move the pool tail from i to i+1; deletion
requests a move from i+1 to i. Both use a zero-extended word-width byte
count low16(8*(DS:2650-i+1)), including their boundary row. Insertion
then writes the supplied pointer/size at i and increments DS:2650;
deletion decrements that bound. Neither helper checks index/capacity,
child result or arithmetic underflow, and deletion has no own final clear.

FND-ITEM-074 supplies the overlap mover's local contract. Valid source,
destination and extra-row spans remain requirements before the requested
shifts can be treated as successful metadata insertion/deletion. There is
no arbitrary-capacity guarantee from the inclusive bound alone.

### Consolidation and pointer adjustment

31E0:38EC requires both enable bytes nonzero. It scans pool entries from
one through DS:2650 inclusive, stopping early on a zero size. For each,
it forms E as pool-zero pointer plus pool-zero size and P as that entry's
pointer. It computes M=physical20(P)-physical20(E) and
G=physical20(E)-physical20(pool-zero pointer), each as a wrapping dword
subtraction after separately masking each physical address to twenty bits.
It has no own rejection of negative/wrapped distance bit patterns.

It requests 289B:01DC(pool-zero pointer, E, M), then
31E0:384D(near address of local E, G, M). It advances pool-zero's pointer
by M through the existing 0586 cursor-update contract, adds the selected
entry's size to pool-zero size, and zeros that entry's pointer and size.
Child results do not gate those stores. It finally sets DS:2650 to zero,
including after an early zero-size stop. This does not prove every later
record was consumed or all native content moved successfully.

384D requires DS:265B nonzero and scans current cache indices below
DS:265D. For resident entries it compares their buffer pointer with the
pointer reached through its near first argument. Pointers in the normalized
half-open interval [that pointer, that pointer+third dword) are reduced
by the second dword through 1000:05CA. FND-COMBAT-125 supplies that
cursor helper. The near dereference uses DS, so consolidation's stack-local
argument requires the appropriate DS/SS relationship and valid aliases.
Pointer changes do not themselves prove that corresponding bytes moved.

The consolidation and adjustment bodies restore their saved SI/BP and
SI/DI/BP respectively. Neither provides a normalized overall success result.
Pool/cache identity, counts, order, nonwrapping spans, local storage and
child preservation remain necessary for the intended compaction reading.

## Interpretation

This removes release's own residency/pool-match gates and metadata cases,
and consolidation's local move/adjust/zero/bound-reset ordering from
Q-PARTY-116. Together with FND-ITEM-088, requested-size accounting is
distinct from residency clears, usable pool space and preserved content.
Native pool/cache producers and lifetime, alias/span admission, backing-store
and reset-child effects remain open. Local contracts do not establish a
global free-space invariant or complete subsystem reading.

## Alternatives

Treating release as pointer destruction ignores retained cache fields.
Treating the pool bound as capacity ignores extra-row moves. Treating all
consolidation distances as positive assumes order the body never checks.
Treating bound zero as proof of completed moves ignores unconditional stores
and early termination. No native defect or parity promotion is claimed.

## How to reproduce

Use licensed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and project-local evidence Python -I
with resident_listing.py on the four location ranges above in order.
GAME_DIR/analysis/item-removal-20261104/ holds
effect-pool-release-consolidation-contract.txt and its -repeat pair, both
SHA-256 c96501d26199fb08292d67cd53dcd53c9a1117e196668e4d78ef70e25458410a.
Reuse FND-ITEM-074/085 and FND-COMBAT-125 for mover/cursor contracts.
Keep reports local. No original game or isolated function was executed.
