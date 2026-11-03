---
id: FND-SCRIPT-031
title: The conditional resident trigger reset has an index-200 tail distinct from allocation's terminal link
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:027D..277B:028A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:0330..277B:04AC
tool: resident_listing.py at 3b7ace1c with actual MZ relocations; locked evidence Python; FND-CONFIG-156 for the separately recorded allocation phase
environment: null
---

## Observation

FND-SCRIPT-014 records a resident trigger-pool fill and link loop.
FND-CONFIG-156 records an allocation-phase loop with a final terminal-link
overwrite. They are different local blocks. This reading distinguishes
their guards and writes for Q-SCRIPT-019's initialized-storage obligation.

### Allocation and resident reset are separate sites

FND-CONFIG-149/156 connect overlay 180's setup to the trigger-table
initializer. The resident main routine contains its call to 56B2:0020 at
277B:027F. The allocator path described by FND-CONFIG-156 requests
200 times thirteen bytes from its known caller, writes sequential links,
then overwrites record 199's link with FFFF. Its failure and bypass states
remain as recorded there; calling the setup does not prove allocation.

The resident reset begins at the distinct 277B:0330 block. A zero byte
at current DS:13FB jumps to 277B:0534, skipping the reset. A nonzero
byte enters a loop that calls 31E0:05BB with arguments 0, 1, 0 and 0
while the current word DS:264E is unsigned greater than zero. After that
word is read as zero, it calls 5773:0039 and 56BD:00B1, then reaches
the global-buffer and trigger-pool reset instructions on returning paths.
No termination, count progress or child preservation is established here.

### The resident block fills the current table and rebuilds links

At 277B:03E2..03F8, the block passes the current far pointer at
57E0:40C0, a fill word FFFF and length 0x0A28 to 1000:3FA2.
FND-CONFIG-144 supplies the fill wrapper/body contract: its low fill
byte is FF, and the helper clears direction for its byte/word stores.

The following local loop starts its index at zero. For each index i it
reloads the far pointer at 57E0:40C0 and stores word i plus one at
offset thirteen times i plus eleven. The index is incremented and compared
signed with 200. On the unchanged local path it runs for i zero through
199; the largest written offset is 2598 and its two-byte store ends at
2600. The loop's final stored value is therefore 200, not FFFF.

If the fill and all iterations refer to the same valid, independent
2600-byte buffer, this block leaves every non-link byte FF and each
record's link equal to its index plus one, including record 199's link
to 200. This result requires pointer, segment, local-index and storage
preservation; it does not follow merely from recognizing the instructions.
In particular, pointer reloads are not a proof that they keep the same
value or that the buffer cannot alias the pointer or other state.

After this loop the block sets free-list head 4C13:032D to zero and
the trigger-list head words 4C10:0005 through 0017, stepping by two,
to FFFF. It does not overwrite record 199's link with FFFF between
the loop and these head assignments. Later calls and uses remain outside
this local postcondition.

## Interpretation

The allocator's terminal link is not an invariant that can be carried
through the later resident reset without reading that reset. Conversely,
the resident loop's index-200 tail does not describe every successful
allocation or prove that a live walker reaches record 200.

FND-SCRIPT-014's resident-loop observation and FND-CONFIG-156's
allocator-loop observation can both hold for their own local blocks.
Q-SCRIPT-019 must establish which initialization/reset, bypass and restore
paths reach a particular use, together with capacity, pointer lifetime and
preservation. Actual admission of DS:13FB, the cleanup loop, returning
children and subsequent table mutations remain unresolved.

No native overrun, observed trigger-pool exhaustion, unconditional startup
reset or complete reading is established. No original program was run.

## Alternatives

- Treating the allocator's final FFFF as the last write in every run ignores
  the separate conditional resident fill and link loop.
- Treating record 199's resident-reset link as proof of a live access to
  record 200 omits allocation, registration count, list traversal and timing.
- Calling the two observations contradictory without identifying their code
  sites conflates allocation with a later conditional reset.
- Assuming the reset is reached on every startup omits its DS:13FB guard,
  cleanup loop and intervening child-return dependencies.

## How to reproduce

Use the installed DSUN.EXE, size 634416 and XXH3
e296af55ba2ecde7e77f555c90f33d0b, with resident_listing.py at the tool
commit and the locked evidence Python. The committed inventory places
277B:0024..277B:05A1 in one function; the inline jump table starts after
that body. Retain two bounded listings, 277B:0024..277B:0330 and
277B:0330..277B:05A1, to distinguish the earlier setup call, conditional
reset and its external branch targets without decoding the table as code.

At 0330 keep the zero branch to 0534. Follow the count-read loop and
its exact pushes, both post-loop calls, the four preceding global-buffer
fills, and the trigger fill's count/pointer pushes. At 03FB..041C trace
the index initialization, increment, multiplication, pointer reload, word
store and signed upper test. Read through 04AC to distinguish the later
head stores from a nonexistent local final-link overwrite in that block.
Compare FND-CONFIG-156's separately located allocation loop and final
overwrite; reuse FND-CONFIG-144 for the actual fill contract, not an
assumed zero or FF allocator result.

The listings are retained as resident-initializer.txt and
resident-later-phase.txt under
GAME_DIR/analysis/trigger-initialization-audit-20261103/.
Their source is read-only; no original code, memory or table bytes are
committed. This entry records the qualified local transformations only.
