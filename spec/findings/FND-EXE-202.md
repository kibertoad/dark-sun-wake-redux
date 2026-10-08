---
id: FND-EXE-202
title: Gate-neighbor indexed publication has an unsigned index guard and a distinct callee-preservation obligation
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005B85E3..0x005B8622
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005B8622..0x005B8667
tool: Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-201 retains the producer of the gate at `0x0242C910`.
A decoded whole-listing operand-text search for nearby addresses exposed
an indexed store using base `0x0242C970`, without selecting functions.
The initial padded token `0242c9` did not match that indexed operand:
its base was rendered without the leading zero. The unpadded token
`242c9` recovered it. The independent address-taking control from
FND-EXE-075 at `0x005FCD56` likewise appeared only with `2427e50`,
not `02427e50`. Known reads and writes through FND-EXE-025/073's
bitmap at `0x02427E40` appeared in both forms. Rendering-dependent
negative results cannot cover a reference kind whose control failed.

The local path pushes the word from its frame offset -1092 and calls
`0x00601EA0`. After removing sixteen outgoing bytes it copies the
full returned EAX word to ESI. It compares EAX against 127 at 32-bit
width and takes an unsigned-above branch before the table read.
Only values from zero through 127 continue on this direct path;
negative signed representations are rejected as large unsigned words.
The table read is at base plus four times that checked EAX, at 32-bit
effective-address width. A nonzero fetched word bypasses the allocation
and table publication.

A zero fetched word reserves twelve outgoing bytes, pushes 516, calls
`0x005BE710`, then removes twelve bytes before storing the full EAX
return at base plus four times ESI. The direct code does not reset or
recheck ESI between the guard and this store. Preservation of the
saved index across that callee is therefore required; it is not supplied
by the earlier comparison alone. The allocation return is not locally
null-checked before publication.

If ESI retains the checked index, each four-byte table destination lies
wholly in `0x0242C970..0x0242CB70`. That interval is disjoint from the
four-byte gate at `0x0242C910..0x0242C914`. A nearby base literal alone
therefore does not make this guarded store a gate writer. If the callee
changes ESI, the checked EAX bound cannot be substituted for its new
value. No actual callee mutation or gate write is claimed.

The next path prepares a zero-fill call with 516, zero and the published
pointer. It rereads the indexed pointer into EBX, replaces the current
outgoing slot with 3584 and calls `0x005BE710` again. It stores that
return through EBX, then prepares another zero-fill call with 3584,
zero and the freshly read pointer's first word. These indirect writes,
callee effects and destination extents are separate from the bounded
table-store interval. No allocation unit, lifetime or non-alias contract
is inferred from the numeric requests.

## Interpretation

This narrows one gate-neighbor computed-writer candidate under explicit
index preservation. Q-EXE-009 retains the callee's effects, alternate
entries, indirect destinations, computed or encoded bases, undecoded
instructions, external writers and actual gate initialization. The
search includes decoded rendered operands only; it does not establish
all writers or a complete reading. FND-EXE-078 remains the separate
virtual-only storage and declared-relocation evidence.

## Alternatives

Using a signed upper-bound test would admit negative indices; this
path uses unsigned above. Assuming that a checked EAX automatically
bounds the ESI used after a call skips the preservation obligation.
Treating a padded text query as a numeric operand search would miss
the recovered indexed store and immediate address-taking control.

## How to reproduce

Use shipped interpreter XXH3-128 `09861838aa3018346f9f15c9a4f5925c`.
In the saved project, read-only with analysis disabled, run
ReportInstructionText with tokens `0242c9`, `02427e40`, `02427e50`,
then with `242c9`, `2427e40`, `2427e50`; each has a combined cap of
256 matches. Neither reaches the cap. Compare known bitmap read/write
and immediate address-taking controls independently of the candidate.
Keep computed, symbolic and undecoded representations excluded.

Read ReportInstructionWindow at `0x005B85C1`, count 22, and at
`0x005B8600`, count 55; restrict claims to the cited spans. Check
ReportCitationBoundaries for `005B85E3..005B8622` and
`005B8622..005B8667`. Trace the returned index, unsigned comparison,
ESI copy, intervening call, publication and deferred outgoing cleanup.
For preserved indices zero through 127, bound the four-byte destination
using the actual base and scale. Endpoints do not prove caller coverage,
interior completeness or callee effects. Keep rich reports local and
execute no original program.
