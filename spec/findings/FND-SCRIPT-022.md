---
id: FND-SCRIPT-022
title: Script-buffer room search mixes comparison widths and restarts after collisions or eviction
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0698
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0578
tool: Python 3.14.7 and Capstone 5.0.7 entry-based bounded 16-bit instruction checks and declared MZ relocation mapping
environment: null
---

## Observation

The complete near function 172C:0698 occupies file offsets
`0x0000CB58..0x0000CC7B`, ending with near return at
`0x0000CC7A`. Eight declared segment operands resolve to
4C13 and the error-call operand resolves to 5702. The only
callees in its body are 5702:00B1 and local 07BB.

It reads one word size argument, initializes a local found
byte to zero and a local double-word candidate to zero.
The zero-extended size is first compared unsigned against
the double word at 4C13:0313. Size greater than or equal
to that capacity calls 5702:00B1 and returns AX zero if
the error callee returns. This local error path does not
scan or evict a cache slot. Its external effects remain
outside this reading.

For a size below capacity it reaches a common room check.
Candidate plus zero-extended size is compared as a signed
double word against capacity. When that sum is less than
capacity and found is still zero, the function scans slots
zero through 15. A slot whose start word at 0219 is FFFF
is skipped. Otherwise it zero-extends its end word at
01F9 and skips an end below the candidate, unsigned. It
also skips a zero-extended start strictly above candidate
plus size, unsigned. Equality at either boundary is not
skipped: these tests treat the interval endpoints as
colliding, although the fill body stores an end just past
its script and appended stop byte (FND-SCRIPT-019).

A collision changes the candidate to the slot's end plus
one, first incremented in word arithmetic and then
zero-extended. The scan index is reset so that the next
iteration starts at slot zero. The scan has no restart
counter or local stop-byte check. Its apparent 16-slot
limit does not bound the number of restarted iterations.

When the scan reaches index 16, candidate plus size is
again compared signed against capacity. A strictly smaller
sum sets found to one. The common room check then routes
found one to returning the candidate's low word in AX.
The candidate originates at zero or from a zero-extended
word; the sum is therefore at most 131070 under unchanged
local state. No double-word addition wrap is needed to
explain these signed comparisons.

When the sum is not signed-less than capacity and found
is zero, the function compares the zero-extended size
unsigned against capacity again. If size remains smaller,
it calls 07BB, resets the candidate to zero and returns
to the common room check without setting found. If size
is no longer smaller, it returns the candidate's low word.
The latter branch can depend on capacity changing during
the call; there is no own capacity write in this body.
FND-SCRIPT-021 reads 07BB's four invalidating writes and
signed-age selection. This function does not allocate,
move or compact script-buffer bytes itself.

Two conditional repeated-state paths are visible locally.
With candidate zero, size one and an occupied slot whose
start is zero and end is FFFF, that slot's collision
restarts the scan with candidate zero again. With all
starts FFFF, all ages minus one, size one and capacity
80000000, the initial unsigned size check passes but the
signed room check cannot pass. Each eviction selects slot
zero and leaves the same invalid markers and age, then
retries from candidate zero. Under unchanged valid local
memory these paths have no own progress or exit. They are
static state examples, not observations of ordinary game
inputs or native hangs.

The known fill caller increments the resource length's
low word and passes that wrapped word at 0578. It saves
AX as the candidate and checks the stop byte, rather than
an allocator success flag, before writing slot bounds.
The resource reader subsequently transfers the complete
recorded length (FND-SCRIPT-019, FND-CONFIG-151). A zero
returned offset is also a legitimate candidate for an
empty cache; it is not a distinct error encoding.

FND-CONFIG-156's named initializer supplies 10000 as a
word and zero-extends it into capacity. That producer
does not supply the high-bit capacity in the static
example. Whether other writers, cache contents, errors
or intervening state reach these paths remains open.

## Interpretation

The helper searches recorded slot bounds for an offset;
it does not prove a general full-resource capacity or
termination guarantee. Its unsigned size filter, signed
room comparisons, inclusive boundary tests, wrapped
end increment and restarted scans are separate contracts.
Its selected candidate and earlier eviction effects must
be read together with the fill caller's length truncation
and stop test.

## Alternatives

The simple reading that a bounded 16-slot scan always
terminates with sufficient capacity is ruled out as a
universal local claim by the explicit restart edges and
conditional repeated-state examples. This does not show
that those inputs occur in the shipped game.

Q-SCRIPT-003 retains all capacity and cache writers, valid
bounds, age inputs, resource lengths and error handling.
One reading uses ordinary initialized capacity, consistent
bounds and successful resource I/O; another reaches
changed, wrapped or inconsistent state. Their producers
and intervening writes distinguish actual reachability.
Q-CONFIG-008 retains the same dependencies for MAS/99.

Q-SCRIPT-007 retains resident emulator cases for empty and
fragmented caches, boundary equality, zero/equal-capacity
sizes, word-end wrap, signed-capacity edges, repeated scans
and eviction. Cases reaching the overlay error entry need
separate evidence; the harness cannot execute that overlay
or establish operating-system outcomes. No native run or
emulated result is claimed here.

## How to reproduce

Read 0698 from its entry through 07BA. Resolve all nine
segment operands. Compare each unsigned size/bound branch
with the signed common-room and post-scan branches. Follow
the word increment before candidate extension, collision
index reset, found-byte assignments and eviction restart.
Check the two stated input cases against those local edges
without assuming the scan index advances permanently.
Read the fill caller from its verified 04CF entry through
the length increment, 0578 call and following stop test.
Compare the named capacity producer in FND-CONFIG-156 and
replacement helper in FND-SCRIPT-021. Keep full writer
coverage, error effects and native reachability separate.
