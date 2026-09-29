---
id: FND-CONFIG-178
title: The large region helper stages checked appends and its follow-up is a self-copy
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4237:0180
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4237:0DEC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4072:0098
tool: Python 3.14.7 and Capstone 5.0.7 focused call/write/branch inventories and complete small-helper readings with declared MZ relocation mapping
environment: null
---

## Observation

FND-CONFIG-177's 0D3F and 0E7C wrappers call resident
4237:0180 with a private region as both first input
and output. The bounded contiguous 0180 body occupies
file span `0x000376F0..0x00038125`, ending with far
return at `0x00038124`. Entry-based decoding covers
2,613 bytes through 1,077 instructions. This finding
reads its call arguments, writes and result/loop routes;
it does not assign every geometry-selection formula a
complete behavioral interpretation.

After its stack-limit guard through 1000:2E48, 0180
checks the second argument through 4072:021E. Returned
one, the four-zero sentinel in FND-CONFIG-177, copies
first input directly to output through local 0076 and
returns zero. Other results initialize a private 138-byte
region at SS:BP-9A through local 0003. That result is
tested for FFFF even though the ordinary local initializer
returns zero (FND-CONFIG-176).

The following loop uses a word index, re-reading the
first input's unsigned word count at each comparison.
Each record has its supplied segment and wrapped offset
input+0A plus eight times that index. The loop initially
passes the input record, second argument and private
output at SS:BP-8 to 4400:00BA. Zero result appends
the unchanged input record to the private region and
advances the index. Nonzero result compares the input
record with those four output words through 4072:0098.
Returned one skips that record's later append routes
and advances the index.

The complete 4072:0098 body spans
`0x000359B8..0x00035A0D`. After its guard, it compares
the two pointers' words at offsets 0, 4, 2 and 6 in
that order. Any unequal word returns zero; all four
equal returns one. It has no own null check. It is an
exact word-equality test, not a native semantic identity
or validity check.

The remaining 0180 paths compare original and local
words and create candidate eight-byte records. Their
explicit memory stores are confined to SS-frame words
BP-10, -0E, -0C and -0A, with word increment/decrement
arithmetic. Calls to 4072:0078 copy an input record to
local SS:BP-10; calls to 4400:0139 write that local
record from supplied words. Their output contracts are
bounded in FND-CONFIG-176. The four-word result at
BP-8 comes from 4400:00BA. There is no explicit direct
store through a supplied region pointer in this body.

There are 33 near-call sites to local 00A6, all supplying
private SS:BP-9A as the destination. Each removes its
eight argument bytes, compares AX with FFFF and sends
that result to the same error continuation at 01C1.
That continuation is the body's only explicit AX FFFF
producer; it returns without the final output copy.
The initializer's encoded FFFF edge also reaches it.
Every other ordinary finished path reaches an AX clear.
FND-CONFIG-176 supplies the append's actual capacity
rejection and bounds it before a seventeenth record write.

A branch-edge inventory of one iteration, from 01CC
through its common index advance 0B85 or error 01C1,
finds no internal loop or edge outside the decoded body.
Treating each call as returning, syntactic paths contain
at most four 00A6 calls. This is a conservative branch-
graph upper bound, not proof that every selected path
has feasible or native geometry inputs. After the common
advance, the outer count comparison can repeat. There
is no own input-count cap, pointer-null test or progress
condition beyond that re-read count/index relationship.

A finished non-sentinel scan passes the private region
to 4400:015C, then copies it to the supplied output
through 0076 and returns zero. The normalizer and copy
results are ignored. Under valid distinct private/input
storage and ordinary guard-bypass calls, private count
begins zero and all record additions use the count-16
append gate. That bounds the private record writes;
it does not validate input records or establish that
the output count always fits without rejection. Aliasing,
guard effects or changed state invalidate a generalized
write-safety or transactionality conclusion.

FND-CONFIG-177's later local 0DEC call in 0D3F uses
the same SS:BP-8A pointer for both arguments. The complete
0DEC body spans `0x0003835C..0x0003837B`. After its
stack guard it calls local 0076 with its first pointer
as source and second as destination, then returns that
AX. It has no count scan, append, merge, deletion or
compaction operation. At the named valid guard-bypass
call, 0076's forward 138-byte copy uses identical source
and destination and returns zero; this is a self-copy,
not an additional region-transform step. Other 0DEC
callers and guard outcomes remain separate.

All far-call segment operands in 0180 and the small
helpers were verified through declared MZ relocations;
near calls use push-CS/near-call pairs. The call/write
inventory includes the full contiguous body and every
append site, while detailed geometry branch selection,
all input producers, callers, aliases and runtime guard
outcomes remain outside this finding's complete claim.

## Interpretation

The large helper funnels its candidate records through
checked private appends and defers a caller-output copy
until local continuation, or uses a sentinel direct-copy
bypass. This traces the capacity/result ordering without
establishing the complete subtraction geometry or native
count reachability. The following helper's named self-copy
supplies no compaction evidence. A syntactic four-append
upper bound is distinct from a measured feasible case
and from the output's fixed 16-record capacity.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain complete geometry-
branch conditions and word-wrap cases, valid counts and
records, producer invariants, all callers, input/output/
stack aliases, DS/index preservation and guard outcomes.
One reading admits no more than 16 appended candidates;
another reaches append capacity and returns FFFF before
the final copy. Complete geometry and producer evidence
would separate native reachability; the call inventory
alone does not. Invalid input reads or external non-return
are not observations of failed original rendering.

For 0DEC, a reading that assigns compaction at the named
call is ruled out by its direct-copy body and identical
arguments. Other pointer values can copy different regions,
and a null pointer can bypass the shared copy. Actual
buffer contents and successful native continuation remain
unobserved. Resident cases stay in Q-SCRIPT-007 after
supported layouts and the harness exist. No native or
emulated execution is claimed.

## How to reproduce

Decode 4237:0180 from 0180 through 0BB4. Inventory
every call and explicit memory-write destination, following
all 33 append arguments and their FFFF branches to 01C1.
Read the entry sentinel and initializer routes and the
common 0B85 loop advance and final copy. Validate branch
targets against instruction starts; count append calls
along paths within a single iteration, keeping this as
an upper bound rather than a geometry-feasibility proof.
Read 4072:0098 through 00EC and 4237:0DEC through
0E0A. Compare the two identical pointers at 4237:0DD0
in FND-CONFIG-177. Verify all declared MZ segments;
retain unresolved geometry formulas, provenance and guard
conditions rather than deriving them from these inventories.
