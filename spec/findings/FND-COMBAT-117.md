---
id: FND-COMBAT-117
title: All fifteen initially null restore slots have known allocation chains but binding alone does not admit their live storage
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070B4A..0x00070BFD
tool: tools/research/exec-census/overlay_listing.py at ac4c81ee; existing allocation and source-table findings named below
environment: null
---

## Observation

FND-COMBAT-116 leaves fifteen initially null source-table slots outside
its fixed-span comparison. Q-COMBAT-065 needs their actual destinations
before excluding overlap with resident effect rows and count. This
reading combines the complete binding helper with existing producer
findings; it does not claim those earlier allocations are newly found.
Addresses and lengths below are hexadecimal; resource numbers are decimal.

Overlay 187 +0D1A, file 70B4A..70BFD, has fifteen own double-word
source loads and destination stores, no child call and no allocation.
For admitted current DS=57E0 it copies these pointers into the source
array at DS:157E. The first six source fields use current DS. The
eight script sources explicitly use ES=4C13, and the final trigger
source explicitly uses ES=57E0. All destination stores use current DS.
The helper copies pointer bits unchanged; it neither resolves a handle
nor checks a buffer's nullness, size, ownership or lifetime.

| SAVE resource | Binding source | Known logical allocation request | Existing producer evidence |
|---|---|---|---|
| 5 | DS:19C9 | 80A | FND-COMBAT-072/083 |
| 6 | DS:19C5 | 462 | FND-COMBAT-072/083 |
| 7 | DS:19C1 | 23F0 | FND-PARTY-236 |
| 8 | DS:67B7 | 2EE0 | FND-PARTY-047 |
| 9 | DS:55B8 | C40 | FND-EXE-639, FND-SAVE-021 |
| 10 | DS:19D1 | 1CC | FND-ACTOR-016 |
| 30 | 4C13:034B | 65 | FND-SCRIPT-026 |
| 31 | 4C13:0347 | 190 | FND-SCRIPT-026, FND-PARTY-241 |
| 32 | 4C13:0343 | 50 | FND-SCRIPT-026 |
| 33 | 4C13:033F | 1A4 | FND-SCRIPT-026 |
| 34 | 4C13:033B | 9 | FND-SCRIPT-026 |
| 35 | 4C13:0337 | 40 | FND-SCRIPT-026 |
| 36 | 4C13:0333 | 28 | FND-SCRIPT-026 |
| 37 | 4C13:032F | 1A4 | FND-SCRIPT-026 |
| 38 | 57E0:40C0 | A28 for supplied count 200 | FND-CONFIG-156 |

Each listed logical request equals the corresponding shipped length
in FND-SAVE-015 and the reread of FND-COMBAT-116. This is request
arithmetic, not a proof of selected restore length or allocated capacity.

### Two allocator routes, one shared lower dependency

The first six known producers call 444C:0008. FND-CONFIG-209/210
read its product-plus-one, header-derived marker and 1000:18D0
forwarding to 1000:15A5. The eight script blocks and trigger block
instead use overlay 169 +0552, whose admitted allocation calls
444C:00FA. FND-CONFIG-156 and FND-SCRIPT-026 read that route and
its failure-byte gate. It also reaches 1000:15A5, through the direct
far-allocation path described in FND-RNG-093. The trigger block is
not another 444C:0008 request merely because its size matches a table.

FND-CONFIG-223/211/212 and FND-RNG-093 retain the lower allocator's
request, root/header/link, bound, arithmetic and runtime conditions.
Under admitted startup mapping, valid heap state, successful allocations,
preserved source fields and fitting nonwrapping transfers, these known
blocks lie above resident data and cannot overlap the earlier resident
effect-row/count spans of FND-COMBAT-116. Separate allocation calls
alone do not establish that joint condition or mutual live disjointness.

### The binder does not establish producer-to-consumer identity

Some known producer entries can bypass allocation based on current
state. FND-CONFIG-156's signed-positive initialization state retains
earlier script/trigger pointers; FND-COMBAT-083 and FND-PARTY-236
record release paths and copied aliases. A pointer copied by +0D1A
can therefore require prior-lifetime evidence rather than a fresh local
allocation. The helper itself supplies none of those admission tests.

The numbered reader allocates its bitmap through +1DD8 before calling
+0D1A, but reaching the binder after a diagnostic is not proof of a
successful bitmap allocation or returning diagnostic contract
(FND-COMBAT-035/111). The reader also performs the placed-pointer
equality/zero-fill branch before its null-pointer resource-skip test.
Consequently a null bound entry must not be treated as skipping every
earlier instruction: equality with a null placed pointer is a distinct
unadmitted case. This is local branch ordering, not an observed null
buffer operation or a claim about admitted native state.

## Interpretation

The finite producer map for all fifteen dynamic slots is now explicit.
Q-COMBAT-065 need not discover those known binders or allocation families
again. Its remaining work is whether each actual field still has that
provenance, valid lifetime and extent when used, including bypass/release,
other pointer writes, earlier resource transfers and stack aliases.
Q-CONFIG-008 and Q-SCRIPT-003 retain shared heap admission; Q-COMBAT-047
retains the creature-table initializer's bypass/lifetime questions.
Q-COMBAT-064 retains selected lengths, and Q-COMBAT-066 child effects.
No item closes, no native allocation is observed and no status changes.

## Alternatives

- Matching allocation requests and shipped lengths do not enforce restore lengths.
- A copied pointer is not proof of its original allocation or current lifetime.
- A known heap route is not proof of valid live heap metadata.
- A null resource destination skips the read, not necessarily the preceding clear.

## How to reproduce

Use DSUN.EXE resolved through tools/game-dir.mjs and locked evidence Python.
overlay_listing.py verifies size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Read descriptor 187, file bounds
70B4A 70BFD, including the far return. Pair its fifteen source fields
with destination index (store displacement minus 157E) divided by four,
then add one for the resource number under selector zero. Compare the
producer findings above, keeping their failure, alias and census limits.
Retain the listing only in GAME_DIR/analysis/hit-incoming-20261104/
restore-destination-bindings.txt. Reuse FND-COMBAT-116's table reports;
no new archive parser or original runtime is needed for this comparison.
No original listing or table payload is committed.
