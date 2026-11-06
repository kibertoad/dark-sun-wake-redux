---
id: FND-SCRIPT-050
title: Trigger allocation detaches a free record and exhausted insertion overwrites an occupied position
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:2DF8..172C:3197
tool: resident_listing.py at d6f285da; Python 3.12.10, Capstone distribution 5.0.9; FND-CONFIG-113/175 copy interfaces; Ghidra 12.1.3 bounded matches in repaired resident project, following the older snapshot's missing functions
environment: null
---

## Observation

This reads the connected record-taking, linking and four insertion bodies
to settle their local contract when the free head is FFFF. It corrects
RULE-SCRIPT-008's blanket no-registration claim and missing detachment
assignment. FND-SCRIPT-015 records the same native helpers but does not
include the successful pop's link clear; FND-SCRIPT-038 already identifies
one pair-inserter exhaustion overwrite. No entire finding is superseded.

The source is installed DSUN.EXE, 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Records are thirteen bytes at the
current far pointer DS:40C0, with next at byte eleven (FMT-SCRIPT-004).
The following postconditions assume valid independent records, list links,
head words, source prefixes and returning calls. They do not establish
that a native game session exhausts the pool or admits arbitrary indices.

### Pop and link are separate operations

172C:314F compares free head 4C13:032D with FFFF. Equality returns
AX=FFFF without a local free-head or record write. Otherwise it takes
the current head index, reads that record's next, publishes the next as
the new free head, and writes FFFF into the taken record's link at
318E before returning the taken index in AX. The returned record is
therefore detached from the remaining free chain.

172C:3106 receives a far pointer to an insertion head or predecessor
link. For an empty position it calls 314F and stores AX into that
position, including FFFF on failure. For an occupied position it saves
the old index and calls 314F. FFFF skips the relinking stores and
leaves the old index untouched. A returned new index instead receives
the old index in its next field and is published into the position.
There is no separate failure flag for the inserter to test.

### The inserters test the position after attempting allocation

After its signed one-key traversal and equality test, 2EA1 calls 3106
when an empty or nonmatching position needs a new record. It then
reloads the position and skips its copy only if that word is FFFF.
Thus allocation failure at an occupied nonmatching position still
copies seven source bytes into the old record. Failure at an empty
position copies nothing. An equal-key replacement bypasses allocation
and copies into the existing record regardless of the free head.

The connected cell inserter 2F46 and box inserter 3026 have the same
post-allocation position test and copy nine and eleven bytes,
respectively. Their traversals use the unsigned pair comparisons
already recorded in FND-SCRIPT-015/038. Pair inserter 2DF8 uses its
recorded signed traversal and always calls 3106 at the chosen position,
even for equal keys; its later test and nine-byte copy have the same
failure distinction. Its duplicate-key behavior therefore depends on
whether a new free record is available.

All four copy through 1000:0452. FND-CONFIG-175 supplies that primitive's
forward copy, count and register effects. The prefix copies stop before
next at byte eleven. Under the bindings above, exhausted occupied-position
insertion replaces payload/key bytes while preserving that node's next;
it neither appends a new node nor rolls back the old prefix. The recursive
argument-copy interface 1000:0699 is FND-CONFIG-113. Invalid/aliased
buffers and changed stack or segment bindings are outside these local
postconditions, not assumed absent in Q-SCRIPT-019's admission audit.

For illustration, take a valid one-key chain with keys 20 then 40 and
an empty free list. Starting independently from that chain in each case,
registering key 10 overwrites the first record;
registering key 30 traverses the first and overwrites the second;
registering key 50 reaches the empty tail and adds nothing. Registering
key 20 replaces the existing equal-key prefix without allocation. These
are conditional consequences of the read branches, not executed fixtures.
With a free record available, empty-position insertion returns a detached
node rather than linking the remainder of the free chain into the list.

## Interpretation

Subsequent bounded decompiler matching in the repaired
INST-single-20261103b project finds all six connected function entries.
Its allocation/link contexts agree with the detached-record and failed
occupied-position cases above; each inserter's context shows the linking
call followed by the position's FFFF test. This resolves the older
snapshot's missing-function limitation for these bodies. It does not
establish accurate inferred calling conventions: the linker view infers
extra arguments at one pop call, whereas the instruction reading fixes
the pop's inputs and AX result. Segment bindings, widths and exact copy
counts continue to come from the instructions and evidenced interfaces.

The blanket statement that exhaustion plus a missing key registers nothing
is false for occupied insertion positions. The rule's existing linking
procedure already implied that overwrite, but its edge-case prose denied
it. Its pop procedure also omitted an actual link mutation. Both are
corrected in RULE-SCRIPT-008, which remains supported with no complete-
reading declaration. The registration handlers and later consumers remain
FND-SCRIPT-038/028/029; actual pool/storage admission remains Q-SCRIPT-019.
No queue item is closed merely by correcting this previously stated contract.

## Alternatives

Treating FFFF from 314F as an unconditional early return from every inserter
ignores 3106's retained occupied position and the later reload. Treating
a popped record as still linked to its former free successor omits the
318E store. Assuming any exhausted position is overwritten ignores the
empty-position test. None of these local cases proves a native exhausted pool.

## How to reproduce

With the pinned evidence Python, run resident_listing.py on the licensed
source for 172C:2EA1..172C:3026 and 172C:3106..172C:3197, retaining
trigger-empty-pool-insertion.txt under
GAME_DIR/analysis/handler-census-20261106/. Read the other complete
inserters at 172C:2DF8..172C:2EA1 and 172C:3026..172C:3106 into
trigger-empty-pool-other-inserters.txt. Follow the same head/link pointer
through the allocator, linker and copy gate; retain empty, occupied,
equal-key and free-record branches, not just the failed allocator return.
Reuse FND-CONFIG-113/175 for the two runtime copy interfaces.

A read-only bounded ReportDecompileMatches.java attempt on retained
INST-resident-20261103a, program DSUN.EXE, at 172C:3106 and 172C:314F
reported no containing function for either. Headless returned zero but
the script diagnostics supply no decompiler evidence. The retained
trigger-empty-pool-decompile.log records that limitation; no project was
modified to manufacture a successful result.

For the successful follow-up, select the already repaired project
GAME_DIR/analysis/inventory-reconcile-20261103/projects/INST-single-20261103b,
program DSUN.EXE, read-only with analysis disabled. Run the same bounded
matcher at 172C:3106 with pattern314f and 172C:314F with patterns032d
and0xffff; retain trigger-current-project-decompile.log beside the earlier
report. Run it at 172C:2EA1, 2F46, 3026 and 2DF8 with patterns3106
and the non-FFFF comparison; retain trigger-current-inserters-decompile.log.
Require all six containing-function markers and actual matches, not just
headless exit zero. The starts are already in the declared entries and
committed inventory. No function was added or project modified for this
retry. Bounded instruction/reference reading remains decisive where the
decompiler infers a signature or storage representation. No original game
or emulated call was run, and no original-derived output is committed.
